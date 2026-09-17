namespace DiscordGitHubBridge.Mapping;

public enum MappingStatus
{
    /// <summary>Reserved before the GitHub call. No issue exists yet, or the issue exists but its number was not recorded.</summary>
    Pending = 0,

    /// <summary>The GitHub issue exists and is recorded.</summary>
    Created = 1,

    /// <summary>The Discord thread has been told about the issue.</summary>
    Notified = 2,
}

public sealed class ThreadIssueMapping
{
    public ulong DiscordThreadId { get; set; }

    public ulong DiscordChannelId { get; set; }

    public long? GitHubIssueNumber { get; set; }

    public string? GitHubIssueUrl { get; set; }

    public MappingStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
