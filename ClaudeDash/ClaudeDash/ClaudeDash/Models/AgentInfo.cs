namespace ClaudeDash.Models;

public class AgentInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string ParentSessionId { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public int MessageCount { get; set; }
    public string Model { get; set; } = string.Empty;
}
