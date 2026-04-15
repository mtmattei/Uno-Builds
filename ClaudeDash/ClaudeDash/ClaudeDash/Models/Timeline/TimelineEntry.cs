namespace ClaudeDash.Models.Timeline;

public class TimelineEntry
{
    public string Id { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public DateTime Timestamp { get; set; }
    public TimelineEntryType Type { get; set; }
    public int Index { get; set; }

    // User message fields
    public string? UserText { get; set; }

    // Assistant text fields
    public string? AssistantText { get; set; }
    public string? Model { get; set; }
    public string? StopReason { get; set; }

    // Tool call fields
    public string? ToolName { get; set; }
    public string? ToolCallId { get; set; }
    public Dictionary<string, JsonElement>? ToolInput { get; set; }

    // Tool result fields
    public string? ToolResultText { get; set; }
    public bool? ToolSuccess { get; set; }

    // File change fields
    public string? FilePath { get; set; }
    public string? OldContent { get; set; }
    public string? NewContent { get; set; }
    public List<DiffHunk>? StructuredPatch { get; set; }

    // Thinking block fields
    public string? ThinkingText { get; set; }

    // System/progress fields
    public string? EventSubtype { get; set; }
    public string? EventDetail { get; set; }

    // Token usage (on assistant messages)
    public TokenUsage? Usage { get; set; }

    // Context
    public string? WorkingDirectory { get; set; }
    public string? GitBranch { get; set; }
}
