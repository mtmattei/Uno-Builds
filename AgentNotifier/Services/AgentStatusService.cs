using System.Text.Json;
using AgentNotifier.Models;
using Microsoft.Extensions.Logging;

namespace AgentNotifier.Services;

public class AgentStatusService : IAgentStatusService, IDisposable
{
    private readonly ILogger<AgentStatusService> _logger;
    private FileSystemWatcher? _watcher;
    private readonly string _statusFilePath;
    private bool _disposed;
    private List<AgentInfo> _agents = new();

    public event EventHandler<MultiAgentPayload>? AgentsChanged;

    public IReadOnlyList<AgentInfo> Agents => _agents.AsReadOnly();

    public AgentStatusService(ILogger<AgentStatusService> logger)
    {
        _logger = logger;
        _statusFilePath = GetStatusFilePath();
    }

    private static string GetStatusFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "AgentNotifier");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "agents.json");
    }

    public void Start()
    {
        try
        {
            var directory = Path.GetDirectoryName(_statusFilePath)!;
            var fileName = Path.GetFileName(_statusFilePath);

            if (!File.Exists(_statusFilePath))
            {
                WriteDefaultStatus();
            }

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnStatusFileChanged;
            _logger.LogInformation("Started watching status file: {Path}", _statusFilePath);

            ReadStatusFile();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start status file watcher");
        }
    }

    private void WriteDefaultStatus()
    {
        var defaultPayload = new Dictionary<string, object>
        {
            ["agents"] = new List<object>(),
            ["total_tokens"] = 0,
            ["total_cost"] = 0.0,
            ["total_elapsed_ms"] = 0,
            ["timestamp"] = DateTime.UtcNow.ToString("o")
        };

        var json = JsonSerializer.Serialize(defaultPayload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_statusFilePath, json);
    }

    private void OnStatusFileChanged(object sender, FileSystemEventArgs e)
    {
        Task.Delay(100).ContinueWith(_ => ReadStatusFile());
    }

    private void ReadStatusFile()
    {
        try
        {
            if (!File.Exists(_statusFilePath)) return;

            var json = File.ReadAllText(_statusFilePath);
            if (string.IsNullOrWhiteSpace(json)) return;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var agents = new List<AgentInfo>();
            int totalTokens = 0;
            double totalCost = 0;
            long totalElapsedMs = 0;

            if (root.TryGetProperty("agents", out var agentsArray) && agentsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var agentElement in agentsArray.EnumerateArray())
                {
                    var agent = ParseAgentInfo(agentElement);
                    agents.Add(agent);
                }
            }

            if (root.TryGetProperty("total_tokens", out var tokensProp))
                totalTokens = tokensProp.GetInt32();
            else
                totalTokens = agents.Sum(a => a.TokensUsed);

            if (root.TryGetProperty("total_cost", out var costProp))
                totalCost = costProp.GetDouble();
            else
                totalCost = agents.Sum(a => a.Cost);

            if (root.TryGetProperty("total_elapsed_ms", out var elapsedProp))
                totalElapsedMs = elapsedProp.GetInt64();

            _agents = agents;

            var payload = new MultiAgentPayload(
                agents,
                totalTokens,
                totalCost,
                TimeSpan.FromMilliseconds(totalElapsedMs),
                DateTime.UtcNow
            );
            AgentsChanged?.Invoke(this, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read status file");
        }
    }

    private static AgentInfo ParseAgentInfo(JsonElement element)
    {
        var id = element.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";
        var tabTitle = element.TryGetProperty("tab_title", out var tabTitleProp) ? tabTitleProp.GetString() ?? "" : "";
        var rawName = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "Agent" : "Agent";
        var name = !string.IsNullOrEmpty(tabTitle) ? tabTitle : rawName;
        var model = element.TryGetProperty("model", out var modelProp) ? modelProp.GetString() ?? "unknown" : "unknown";

        var statusStr = element.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "idle" : "idle";
        var status = Enum.TryParse<AgentStatus>(statusStr, true, out var s) ? s : AgentStatus.Idle;

        var label = element.TryGetProperty("label", out var labelProp)
            ? labelProp.GetString() ?? status.ToString().ToUpper()
            : status.ToString().ToUpper();

        var message = element.TryGetProperty("message", out var msgProp)
            ? msgProp.GetString() ?? ""
            : "";

        var currentTask = element.TryGetProperty("current_task", out var taskProp)
            ? taskProp.GetString() ?? ""
            : "";

        int? progress = element.TryGetProperty("progress", out var progProp) && progProp.ValueKind == JsonValueKind.Number
            ? progProp.GetInt32()
            : null;

        var tokensUsed = element.TryGetProperty("tokens_used", out var tokensProp) && tokensProp.ValueKind == JsonValueKind.Number
            ? tokensProp.GetInt32()
            : 0;

        var tokenLimit = element.TryGetProperty("token_limit", out var limitProp) && limitProp.ValueKind == JsonValueKind.Number
            ? limitProp.GetInt32()
            : 200000;

        var cost = element.TryGetProperty("cost", out var costProp) && costProp.ValueKind == JsonValueKind.Number
            ? costProp.GetDouble()
            : 0;

        var rate = element.TryGetProperty("rate", out var rateProp) && rateProp.ValueKind == JsonValueKind.Number
            ? rateProp.GetDouble()
            : 0;

        var queuePosition = element.TryGetProperty("queue_position", out var queueProp) && queueProp.ValueKind == JsonValueKind.Number
            ? queueProp.GetInt32()
            : 0;

        var isWaitingForInput = element.TryGetProperty("is_waiting_for_input", out var waitProp) && waitProp.GetBoolean();

        SessionData? session = null;
        if (element.TryGetProperty("session", out var sessionProp))
        {
            var sessionId = sessionProp.TryGetProperty("id", out var sesIdProp) ? sesIdProp.GetString() ?? "----" : "----";
            var task = sessionProp.TryGetProperty("task", out var taskNameProp) ? taskNameProp.GetString() ?? "NONE" : "NONE";
            var elapsedMs = sessionProp.TryGetProperty("elapsed_ms", out var elapsedProp) ? elapsedProp.GetInt64() : 0;
            session = new SessionData(sessionId, task, elapsedMs, null);
        }

        return new AgentInfo(
            id, name, model, status, label, message, "", currentTask,
            progress, tokensUsed, tokenLimit, cost, rate, queuePosition,
            session, isWaitingForInput, false, DateTime.UtcNow
        );
    }

    public void Stop()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
