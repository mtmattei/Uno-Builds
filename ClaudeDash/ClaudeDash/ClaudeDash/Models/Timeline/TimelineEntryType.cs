namespace ClaudeDash.Models.Timeline;

public enum TimelineEntryType
{
    UserMessage,
    AssistantText,
    ToolCall,
    ToolResult,
    FileChange,
    ThinkingBlock,
    SystemEvent,
    ProgressEvent,
    FileHistorySnapshot
}
