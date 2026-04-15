namespace ClaudeDash.Models.Timeline;

public class SessionTimeline
{
    public string SessionId { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string? ClaudeVersion { get; set; }
    public string? GitBranch { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;

    // Full ordered timeline
    public List<TimelineEntry> Entries { get; set; } = [];

    // Aggregated stats
    public int UserMessageCount { get; set; }
    public int AssistantMessageCount { get; set; }
    public int ToolCallCount { get; set; }
    public int FileChangeCount { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public HashSet<string> ModelsUsed { get; set; } = [];
    public HashSet<string> FilesTouched { get; set; } = [];
    public HashSet<string> ToolsUsed { get; set; } = [];

    // Subagent sessions
    public List<SubagentRef> Subagents { get; set; } = [];

    // Computed cost estimate (USD)
    public double EstimatedCost { get; set; }
}
