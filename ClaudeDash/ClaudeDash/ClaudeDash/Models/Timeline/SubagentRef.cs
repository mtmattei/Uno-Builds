namespace ClaudeDash.Models.Timeline;

public class SubagentRef
{
    public string AgentId { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
}
