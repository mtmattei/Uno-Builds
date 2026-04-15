using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record McpHealthModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<McpHealthModel> _logger;

    public McpHealthModel(
        IClaudeConfigService configService,
        ILogger<McpHealthModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);
    public IState<string> SummaryText => State.Value(this, () => string.Empty);

    public IListFeed<McpServerInfo> Servers => ListFeed.Async(async ct =>
    {
        try
        {
            var servers = await _configService.GetMcpServersAsync();

            foreach (var s in servers)
                GenerateMockHealthData(s);

            var total = servers.Count;
            var online = servers.Count(s => s.HealthStatus == "online");
            var down = servers.Count(s => s.HealthStatus == "down");

            await SummaryText.Set($"{total} servers | {online} online | {down} down", ct);

            return servers.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load MCP servers");
            await ErrorMessage.Set($"Failed to load MCP servers: {ex.Message}", ct);
            return ImmutableList<McpServerInfo>.Empty;
        }
    });

    public IFeed<int> TotalServers => Feed.Async(async ct =>
    {
        var servers = await Servers;
        return servers?.Count ?? 0;
    });

    public IFeed<int> OnlineCount => Feed.Async(async ct =>
    {
        var servers = await Servers;
        return servers?.Count(s => s.HealthStatus == "online") ?? 0;
    });

    public IFeed<int> DegradedCount => Feed.Async(async ct =>
    {
        var servers = await Servers;
        return servers?.Count(s => s.HealthStatus == "slow") ?? 0;
    });

    public IFeed<int> DownCount => Feed.Async(async ct =>
    {
        var servers = await Servers;
        return servers?.Count(s => s.HealthStatus == "down") ?? 0;
    });

    public IFeed<int> AvgLatencyMs => Feed.Async(async ct =>
    {
        var servers = await Servers;
        if (servers is null) return 0;
        var onlineServers = servers.Where(s => s.HealthStatus != "down").ToList();
        return onlineServers.Count > 0
            ? (int)onlineServers.Average(s => s.LatencyMs)
            : 0;
    });

    private static void GenerateMockHealthData(McpServerInfo server)
    {
        var rng = new Random(server.Name.GetHashCode());

        var roll = rng.NextDouble();
        if (roll < 0.12)
        {
            server.HealthStatus = "down";
            server.LatencyMs = 0;
            server.ToolCount = rng.Next(3, 18);
            server.StatusDetail = "connection refused - 3 retries failed";
        }
        else if (roll < 0.25)
        {
            server.HealthStatus = "slow";
            var p95 = rng.Next(250, 450);
            server.LatencyMs = rng.Next(180, 350);
            server.ToolCount = rng.Next(4, 22);
            server.StatusDetail = $"p95 latency {p95}ms (threshold: 200ms)";
        }
        else
        {
            server.HealthStatus = "online";
            server.LatencyMs = rng.Next(5, 85);
            server.ToolCount = rng.Next(3, 25);
            server.StatusDetail = string.Empty;
        }

        server.Transport = server.ServerType == "URL" ? "sse" : "stdio";
    }
}
