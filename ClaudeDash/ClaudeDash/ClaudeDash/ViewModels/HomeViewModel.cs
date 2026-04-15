using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record HomeModel
{
    private readonly IClaudeConfigService _configService;
    private readonly IAgentLauncherService _agentLauncherService;
    private readonly IBackgroundScannerService _backgroundScanner;
    private readonly IProjectScannerService _projectScanner;
    private readonly IChatService _chatService;
    private readonly ILogger<HomeModel> _logger;

    public HomeModel(
        IClaudeConfigService configService,
        IAgentLauncherService agentLauncherService,
        IBackgroundScannerService backgroundScanner,
        IProjectScannerService projectScanner,
        IChatService chatService,
        ILogger<HomeModel> logger)
    {
        _configService = configService;
        _agentLauncherService = agentLauncherService;
        _backgroundScanner = backgroundScanner;
        _projectScanner = projectScanner;
        _chatService = chatService;
        _logger = logger;
    }

    // --- Greeting ---
    public IFeed<string> Greeting => Feed.Async(async ct =>
    {
        var name = await LoadUserNameAsync();
        var hour = DateTime.Now.Hour;
        var timeGreeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        return string.IsNullOrEmpty(name) ? timeGreeting : $"{timeGreeting}, {name}";
    });

    // --- Dashboard metrics ---
    public IFeed<DashboardMetrics> Metrics => Feed.Async(async ct =>
    {
        try
        {
            var repos = await _configService.GetReposAsync();
            var servers = await _configService.GetMcpServersAsync();
            var sessions = await _configService.GetRecentSessionsAsync(50);
            var envResults = await _configService.RunEnvAuditAsync();
            var skills = await _configService.GetSkillsAsync();
            var hooks = await _configService.GetHooksAsync();
            var memFiles = await _configService.GetMemoryFilesAsync();
            var recentThreshold = DateTime.UtcNow.AddHours(-1);

            return new DashboardMetrics(
                ActiveSessions: sessions.Count(s => s.LastActivity > recentThreshold),
                TotalRecentSessionCount: sessions.Count,
                TrackedRepos: repos.Count,
                DirtyRepoCount: repos.Count(r => r.IsDirty),
                McpServers: servers.Count,
                McpFromSettings: servers.Count(s => s.Source == "settings.json"),
                McpFromMcpJson: servers.Count(s => s.Source == "mcp.json"),
                HygieneIssues: envResults.Count(r => !r.Status.Equals("ok", StringComparison.OrdinalIgnoreCase)),
                HygieneTotalCount: envResults.Count,
                HygieneMissingCount: envResults.Count(r => r.Status.Equals("missing", StringComparison.OrdinalIgnoreCase)),
                HygieneWarnCount: envResults.Count(r => r.Status.Equals("warning", StringComparison.OrdinalIgnoreCase)),
                SkillCount: skills.Count,
                HookCount: hooks.Count,
                MemoryFileCount: memFiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dashboard metrics");
            return new DashboardMetrics();
        }
    });

    // --- Toolchain + Verdict ---
    public IFeed<ToolchainData> Toolchain => Feed.Async(async ct =>
    {
        try
        {
            var info = await _configService.GetUnoPlatformInfoAsync();
            var checks = await _configService.RunUnoCheckAsync();
            var license = await _configService.GetLicenseInfoAsync();

            var items = BuildToolchainItems(info, checks);
            var verdict = ComputeVerdict(items);

            return new ToolchainData(
                Items: items,
                Verdict: verdict,
                UnoSdkVersion: info.SdkVersion ?? "n/a",
                DotNetSdkVersion: info.DotNetSdkVersion ?? "n/a",
                UnoWinUIVersion: info.UnoWinUIVersion ?? "",
                DetectedIde: info.DetectedIde ?? "n/a",
                LicenseInfo: license);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load toolchain data");
            return new ToolchainData(
                Verdict: new VerdictState(VerdictLevel.ReadyWithWarnings, "Unable to scan toolchain",
                    ImmutableList.Create("scan error")));
        }
    });

    // --- Projects ---
    public IListFeed<ProjectInfo> RecentProjects => ListFeed.Async(async ct =>
    {
        try
        {
            var all = await _projectScanner.GetAllProjectsAsync();
            return all.OrderByDescending(p => p.LastActivity).Take(6).ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load projects");
            return ImmutableList<ProjectInfo>.Empty;
        }
    });

    // --- Selected project ---
    public IState<ProjectInfo?> SelectedProject => State<ProjectInfo?>.Value(this, () => null);

    // --- Alerts ---
    public IListFeed<AlertItem> Alerts => ListFeed.Async(async ct =>
    {
        var alerts = await _configService.GetAlertsFromEnvironmentAsync();
        return alerts.ToImmutableList();
    });

    // --- Recent sessions ---
    public IListFeed<SessionItem> RecentSessions => ListFeed.Async(async ct =>
    {
        var sessions = await _configService.GetRecentSessionItemsAsync();
        return sessions.ToImmutableList();
    });

    // --- Workflow stages ---
    public IFeed<ImmutableList<WorkflowStage>> WorkflowStages => Feed.Async(async ct =>
    {
        var metrics = await Metrics;
        var toolchain = await Toolchain;
        return BuildWorkflowStages(metrics, toolchain);
    });

    // --- Agent status ---
    public IFeed<AgentStatus> ActiveAgent => Feed.Async(async ct =>
    {
        try
        {
            var agents = await _agentLauncherService.GetRunningAgentsAsync();
            if (agents.Count > 0)
            {
                var first = agents[0];
                var dirParts = first.WorkingDirectory.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                return new AgentStatus(
                    IsRunning: true,
                    StatusLabel: "RUNNING",
                    Task: !string.IsNullOrEmpty(first.CommandLine) ? first.CommandLine : $"Agent active in {first.WorkingDirectory}",
                    File1: dirParts.Length > 0 ? dirParts[^1] : "",
                    File2: $"PID {first.ProcessId}",
                    Progress: 50);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Agent detection unavailable");
        }
        return new AgentStatus();
    });

    // --- Terminal chat messages ---
    public IListState<ChatMessage> TerminalMessages => ListState.Value(this, () =>
        ImmutableList.Create(new ChatMessage(Role: ChatRole.Assistant, Text: "Orbital v1.0 - Claude Code bridge active.")));

    // --- Commands ---
    public async ValueTask SelectProject(ProjectInfo? project, CancellationToken ct)
    {
        await SelectedProject.Update(_ => project, ct);
    }

    public async ValueTask SendTerminalMessage(string input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        var userMsg = new ChatMessage(Role: ChatRole.User, Text: input);
        await TerminalMessages.AddAsync(userMsg, ct);

        // Real chat via IChatService (falls back to simulated if no API key)
        if (_chatService.IsConfigured)
        {
            try
            {
                var allMsgs = (await TerminalMessages) ?? ImmutableList<ChatMessage>.Empty;
                var sb = new System.Text.StringBuilder();
                await foreach (var chunk in _chatService.StreamResponseAsync(allMsgs.ToList(), ct))
                {
                    sb.Append(chunk);
                }
                var botMsg = new ChatMessage(Role: ChatRole.Assistant, Text: sb.ToString());
                await TerminalMessages.AddAsync(botMsg, ct);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chat stream failed, falling back to simulated response");
            }
        }

        // Fallback: simulated response
        var response = SimulateResponse(input);
        var fallbackMsg = new ChatMessage(Role: ChatRole.Assistant, Text: response);
        await TerminalMessages.AddAsync(fallbackMsg, ct);
    }

    // --- Helpers ---

    private static async Task<string> LoadUserNameAsync()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "config --global user.name")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is null) return Environment.UserName;
            var result = await proc.StandardOutput.ReadToEndAsync();
            using var cts = new CancellationTokenSource(2000);
            try { await proc.WaitForExitAsync(cts.Token); } catch (OperationCanceledException) { }
            return !string.IsNullOrWhiteSpace(result) ? result.Trim().Split(' ')[0] : Environment.UserName;
        }
        catch { return Environment.UserName; }
    }

    private static ImmutableList<ToolchainItem> BuildToolchainItems(UnoPlatformInfo info, List<Models.UnoCheckResult> checks)
    {
        var builder = ImmutableList.CreateBuilder<ToolchainItem>();

        builder.Add(new ToolchainItem(
            Category: ".NET", Name: ".NET SDK", Version: info.DotNetSdkVersion,
            Status: !string.IsNullOrEmpty(info.DotNetSdkVersion) ? ToolchainStatus.Pass : ToolchainStatus.Fail,
            Detail: !string.IsNullOrEmpty(info.DotNetSdkVersion) ? "installed" : "not found",
            FixCommand: string.IsNullOrEmpty(info.DotNetSdkVersion) ? "winget install Microsoft.DotNet.SDK.10" : "",
            FixLabel: "install"));

        builder.Add(new ToolchainItem(
            Category: "Uno Platform", Name: "Uno.Sdk", Version: info.SdkVersion,
            Status: !string.IsNullOrEmpty(info.SdkVersion) ? ToolchainStatus.Pass : ToolchainStatus.Unknown,
            Detail: !string.IsNullOrEmpty(info.SdkVersion) ? "detected" : "no project found"));

        if (!string.IsNullOrEmpty(info.UnoWinUIVersion))
        {
            builder.Add(new ToolchainItem(
                Category: "Uno Platform", Name: "Uno.WinUI", Version: info.UnoWinUIVersion,
                Status: ToolchainStatus.Pass, Detail: "installed"));
        }

        builder.Add(new ToolchainItem(
            Category: "Uno Platform", Name: "Hot Reload",
            Status: info.HotReloadStatus == "available" ? ToolchainStatus.Pass : ToolchainStatus.Unknown,
            Detail: info.HotReloadStatus));

        builder.Add(new ToolchainItem(
            Category: "Uno Platform", Name: "Hot Design",
            Status: info.HotDesignStatus == "available" ? ToolchainStatus.Pass : ToolchainStatus.Unknown,
            Detail: info.HotDesignStatus));

        foreach (var check in checks)
        {
            if (check.Name == ".NET SDK") continue;
            var status = check.Status.ToLowerInvariant() switch
            {
                "ok" => ToolchainStatus.Pass,
                "warning" => ToolchainStatus.Warn,
                "error" => ToolchainStatus.Fail,
                _ => ToolchainStatus.Unknown
            };
            builder.Add(new ToolchainItem(
                Category: check.Category, Name: check.Name, Status: status, Detail: check.Detail,
                FixCommand: check.Recommendation.StartsWith("Install via: ", StringComparison.OrdinalIgnoreCase)
                    ? check.Recommendation["Install via: ".Length..] : "",
                FixLabel: status is ToolchainStatus.Fail or ToolchainStatus.Warn ? "install" : ""));
        }

        return builder.ToImmutable();
    }

    private static VerdictState ComputeVerdict(ImmutableList<ToolchainItem> items)
    {
        var failCount = items.Count(t => t.Status == ToolchainStatus.Fail);
        var warnCount = items.Count(t => t.Status == ToolchainStatus.Warn);

        if (failCount > 0)
        {
            var reasons = items.Where(t => t.Status == ToolchainStatus.Fail).Take(3)
                .Select(t => $"{t.Name} missing").ToImmutableList();
            return new VerdictState(VerdictLevel.Blocked,
                $"{failCount} blocking issue{(failCount > 1 ? "s" : "")} detected", reasons);
        }
        if (warnCount > 0)
        {
            var reasons = items.Where(t => t.Status == ToolchainStatus.Warn).Take(3)
                .Select(t => $"{t.Name} not installed").ToImmutableList();
            return new VerdictState(VerdictLevel.ReadyWithWarnings, "Ready with warnings", reasons);
        }
        return new VerdictState(VerdictLevel.Ready, "Environment ready");
    }

    private static ImmutableList<WorkflowStage> BuildWorkflowStages(DashboardMetrics m, ToolchainData t)
    {
        var hasSessions = m.TotalRecentSessionCount > 0;
        var hasRepos = m.TrackedRepos > 0;
        var hasToolchainIssues = t.Items.Any(i => i.Status == ToolchainStatus.Fail);

        return ImmutableList.Create(
            new WorkflowStage("INTENT", "\uE73E", StageStatus.Completed),
            new WorkflowStage("SPEC / PRD", "\uE8A7", hasSessions ? StageStatus.Completed : StageStatus.Active),
            new WorkflowStage("DESIGN", "\uE790",
                hasToolchainIssues ? StageStatus.Blocked : hasRepos ? StageStatus.Completed : hasSessions ? StageStatus.Active : StageStatus.Pending),
            new WorkflowStage("SCAFFOLD", "\uE943",
                hasRepos && !hasToolchainIssues ? StageStatus.Completed : hasRepos ? StageStatus.Active : StageStatus.Pending),
            new WorkflowStage("IMPLEMENT", "\uE713",
                hasRepos && hasSessions ? StageStatus.Completed : StageStatus.Pending),
            new WorkflowStage("VERIFY", "\uE930",
                m.HygieneTotalCount > 0 && m.HygieneIssues == 0 ? StageStatus.Completed
                : m.HygieneTotalCount > 0 ? StageStatus.Active : StageStatus.Pending),
            new WorkflowStage("SHIP", "\uE724", StageStatus.Pending));
    }

    private static string SimulateResponse(string input)
    {
        var lower = input.ToLowerInvariant();
        if (lower.Contains("scaffold") || lower.Contains("new") || lower.Contains("create"))
            return "Creating project scaffold...\n\n\u2713 dotnet new unoapp -n MyApp\n\u2713 Added MVUX architecture\n\u2713 Configured Hot Design\n\nProject ready.";
        if (lower.Contains("test") || lower.Contains("check"))
            return "\u2713 Running uno-check...\n\u2713 .NET SDK detected\n\u2713 All targets verified\n\nEnvironment healthy.";
        if (lower.Contains("help"))
            return "Commands: scaffold, check, build, deploy, status";
        return $"Processing: {input}\n\n\u2713 Command routed via MCP bridge\n\nReady.";
    }
}

// --- Supporting records ---

public record DashboardMetrics(
    int ActiveSessions = 0,
    int TotalRecentSessionCount = 0,
    int TrackedRepos = 0,
    int DirtyRepoCount = 0,
    int McpServers = 0,
    int McpFromSettings = 0,
    int McpFromMcpJson = 0,
    int HygieneIssues = 0,
    int HygieneTotalCount = 0,
    int HygieneMissingCount = 0,
    int HygieneWarnCount = 0,
    int SkillCount = 0,
    int HookCount = 0,
    int MemoryFileCount = 0);

public record ToolchainData(
    ImmutableList<ToolchainItem>? Items = null,
    VerdictState? Verdict = null,
    string UnoSdkVersion = "n/a",
    string DotNetSdkVersion = "n/a",
    string UnoWinUIVersion = "",
    string DetectedIde = "n/a",
    LicenseInfo? LicenseInfo = null)
{
    public ImmutableList<ToolchainItem> Items { get; init; } = Items ?? ImmutableList<ToolchainItem>.Empty;
    public VerdictState Verdict { get; init; } = Verdict ?? new VerdictState();
    public LicenseInfo LicenseInfo { get; init; } = LicenseInfo ?? new LicenseInfo();
}

public record AgentStatus(
    bool IsRunning = false,
    string StatusLabel = "IDLE",
    string Task = "No active agent sessions",
    string File1 = "",
    string File2 = "",
    int Progress = 0);
