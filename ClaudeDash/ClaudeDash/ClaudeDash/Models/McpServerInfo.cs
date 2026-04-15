namespace ClaudeDash.Models;

public class McpServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string ServerType { get; set; } = string.Empty; // "URL" or "Command"
    public string Source { get; set; } = string.Empty; // "settings.json" or "mcp.json"
    public string Url { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string[] Args { get; set; } = [];
    public string Transport { get; set; } = string.Empty;

    // Health data (mock-generated from server name hash)
    public string HealthStatus { get; set; } = "online"; // "online", "down", "slow"
    public int LatencyMs { get; set; }
    public int ToolCount { get; set; }
    public string StatusDetail { get; set; } = string.Empty;
}
