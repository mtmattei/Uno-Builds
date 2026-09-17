namespace DiscordGitHubBridge.Mapping;

public enum ReservationOutcome
{
    /// <summary>A new Pending row was inserted. The caller owns the thread now.</summary>
    Reserved,

    /// <summary>A row already exists for this thread, in any status.</summary>
    AlreadyMapped,
}

public sealed record ReservationResult(ReservationOutcome Outcome, ThreadIssueMapping? Existing);

public interface IMappingRepository
{
    Task<ReservationResult> TryReserveAsync(ulong threadId, ulong channelId, CancellationToken cancellationToken);

    Task CompleteAsync(ulong threadId, long issueNumber, string issueUrl, CancellationToken cancellationToken);

    Task MarkNotifiedAsync(ulong threadId, CancellationToken cancellationToken);

    /// <summary>Deletes the row only while it is still Pending, so a failed GitHub call can be retried later.</summary>
    Task ReleaseAsync(ulong threadId, CancellationToken cancellationToken);

    Task<ThreadIssueMapping?> FindAsync(ulong threadId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ThreadIssueMapping>> GetStalePendingAsync(DateTimeOffset olderThan, CancellationToken cancellationToken);
}
