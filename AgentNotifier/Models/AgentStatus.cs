namespace AgentNotifier.Models;

public enum AgentStatus
{
    Idle,
    Working,
    Waiting,
    Finished,
    Error
}

public partial record AgentInfo(
    string Id,
    string Name,
    string Model,
    AgentStatus Status,
    string Label,
    string Message,
    string TaskTag,
    string CurrentTask,
    int? Progress,
    int TokensUsed,
    int TokenLimit,
    double Cost,
    double Rate,
    int QueuePosition,
    SessionData? Session,
    bool IsWaitingForInput,
    bool IsExpanded,
    DateTime LastUpdated
);

public record StatusPayload(
    AgentStatus Status,
    string Label,
    string Message,
    int? Progress,
    SessionData? Session,
    Dictionary<string, object>? Metadata,
    bool IsWaitingForInput = false
);

public record MultiAgentPayload(
    List<AgentInfo> Agents,
    int TotalTokens,
    double TotalCost,
    TimeSpan TotalElapsed,
    DateTime Timestamp
);

public partial record SessionData(
    string Id,
    string Task,
    long ElapsedMs,
    DateTime? StartedAt
);

public record StatusMessage(
    string Type,
    DateTime Timestamp,
    StatusPayload Payload
);
