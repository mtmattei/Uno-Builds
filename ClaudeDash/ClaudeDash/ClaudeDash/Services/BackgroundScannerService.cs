using Path = System.IO.Path;

namespace ClaudeDash.Services;

public class BackgroundScannerService : IBackgroundScannerService
{
    private readonly IClaudeConfigService _configService;
    private readonly IProjectScannerService _projectScanner;
    private readonly ISearchIndexService _searchIndex;
    private readonly ILogger<BackgroundScannerService> _logger;

    private CancellationTokenSource? _cts;
    private Task? _pollingTask;

    // Track counts from the previous scan to detect changes
    private int _prevSessionCount;
    private int _prevProjectCount;
    private int _prevMcpCount;
    private int _prevSkillCount;
    private int _prevAgentCount;
    private int _prevMemoryCount;
    private int _prevHookCount;

    // Track file modification times to detect changes without re-parsing
    private DateTime _lastSettingsModified;
    private DateTime _lastMcpJsonModified;
    private Dictionary<string, DateTime> _sessionFileTimestamps = new();

    private static readonly string ClaudeDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    public ScanSnapshot? LatestSnapshot { get; private set; }
    public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;
    public event Action<ScanSnapshot>? ScanCompleted;

    public BackgroundScannerService(
        IClaudeConfigService configService,
        IProjectScannerService projectScanner,
        ISearchIndexService searchIndex,
        ILogger<BackgroundScannerService> logger)
    {
        _configService = configService;
        _projectScanner = projectScanner;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    public void Start(TimeSpan? interval = null)
    {
        if (IsRunning) return;

        var pollInterval = interval ?? TimeSpan.FromSeconds(30);
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _pollingTask = Task.Run(async () =>
        {
            // Initial scan immediately
            await RunScanAsync(token);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(pollInterval, token);
                    await RunScanAsync(token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Background scan failed, will retry next interval");
                }
            }
        }, token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    public async Task<ScanSnapshot> ScanNowAsync()
    {
        return await RunScanAsync(CancellationToken.None);
    }

    private async Task<ScanSnapshot> RunScanAsync(CancellationToken token)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var changed = new List<string>();

        // Quick filesystem checks first to detect if anything changed
        var configChanged = HasConfigChanged();

        // Run all scans in parallel
        var sessionsTask = _configService.GetRecentSessionsAsync(100);
        var projectsTask = _projectScanner.GetAllProjectsAsync();
        var mcpTask = _configService.GetMcpServersAsync();
        var skillsTask = _configService.GetSkillsAsync();
        var agentsTask = _configService.GetAgentsAsync();
        var memoryTask = _configService.GetMemoryFilesAsync();
        var hooksTask = _configService.GetHooksAsync();

        await Task.WhenAll(sessionsTask, projectsTask, mcpTask, skillsTask,
            agentsTask, memoryTask, hooksTask);

        token.ThrowIfCancellationRequested();

        var sessions = await sessionsTask;
        var projects = await projectsTask;
        var mcpServers = await mcpTask;
        var skills = await skillsTask;
        var agents = await agentsTask;
        var memory = await memoryTask;
        var hooks = await hooksTask;

        // Detect changes
        if (sessions.Count != _prevSessionCount || HasNewSessionFiles())
            changed.Add("sessions");
        if (projects.Count != _prevProjectCount)
            changed.Add("projects");
        if (mcpServers.Count != _prevMcpCount || configChanged)
            changed.Add("mcp");
        if (skills.Count != _prevSkillCount)
            changed.Add("skills");
        if (agents.Count != _prevAgentCount)
            changed.Add("agents");
        if (memory.Count != _prevMemoryCount)
            changed.Add("memory");
        if (hooks.Count != _prevHookCount || configChanged)
            changed.Add("hooks");

        // Update previous counts
        _prevSessionCount = sessions.Count;
        _prevProjectCount = projects.Count;
        _prevMcpCount = mcpServers.Count;
        _prevSkillCount = skills.Count;
        _prevAgentCount = agents.Count;
        _prevMemoryCount = memory.Count;
        _prevHookCount = hooks.Count;

        // Rebuild search index if anything changed
        if (changed.Count > 0 || !_searchIndex.IsReady)
        {
            await _searchIndex.BuildIndexAsync();
        }

        sw.Stop();

        var snapshot = new ScanSnapshot
        {
            SessionCount = sessions.Count,
            ProjectCount = projects.Count,
            McpServerCount = mcpServers.Count,
            SkillCount = skills.Count,
            AgentCount = agents.Count,
            MemoryFileCount = memory.Count,
            HookCount = hooks.Count,
            LastScanTime = DateTime.UtcNow,
            ScanDuration = sw.Elapsed,
            ChangedCategories = changed
        };

        LatestSnapshot = snapshot;

        if (changed.Count > 0)
        {
            _logger.LogInformation("Background scan completed in {Duration}ms. Changes: {Changes}",
                sw.ElapsedMilliseconds, string.Join(", ", changed));
        }

        ScanCompleted?.Invoke(snapshot);
        return snapshot;
    }

    private bool HasConfigChanged()
    {
        var changed = false;

        var settingsPath = Path.Combine(ClaudeDir, "settings.json");
        if (File.Exists(settingsPath))
        {
            var mod = File.GetLastWriteTimeUtc(settingsPath);
            if (mod != _lastSettingsModified)
            {
                _lastSettingsModified = mod;
                changed = true;
            }
        }

        var mcpPath = Path.Combine(ClaudeDir, "mcp.json");
        if (File.Exists(mcpPath))
        {
            var mod = File.GetLastWriteTimeUtc(mcpPath);
            if (mod != _lastMcpJsonModified)
            {
                _lastMcpJsonModified = mod;
                changed = true;
            }
        }

        return changed;
    }

    private bool HasNewSessionFiles()
    {
        var projectsDir = Path.Combine(ClaudeDir, "projects");
        if (!Directory.Exists(projectsDir))
            return false;

        try
        {
            var currentFiles = new Dictionary<string, DateTime>();
            var jsonlFiles = Directory.GetFiles(projectsDir, "*.jsonl", SearchOption.AllDirectories)
                .Where(f => !f.Contains("subagents", StringComparison.OrdinalIgnoreCase));

            foreach (var file in jsonlFiles)
            {
                currentFiles[file] = File.GetLastWriteTimeUtc(file);
            }

            // Check for new or modified files
            var hasChanges = false;
            foreach (var kvp in currentFiles)
            {
                if (!_sessionFileTimestamps.TryGetValue(kvp.Key, out var prevTime) || prevTime != kvp.Value)
                {
                    hasChanges = true;
                    break;
                }
            }

            // Also check if files were removed
            if (!hasChanges && currentFiles.Count != _sessionFileTimestamps.Count)
                hasChanges = true;

            _sessionFileTimestamps = currentFiles;
            return hasChanges;
        }
        catch
        {
            return false;
        }
    }
}
