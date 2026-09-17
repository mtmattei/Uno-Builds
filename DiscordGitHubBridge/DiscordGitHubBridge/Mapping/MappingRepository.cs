using DiscordGitHubBridge.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiscordGitHubBridge.Mapping;

public sealed class MappingRepository(IDbContextFactory<BridgeDbContext> contextFactory, TimeProvider timeProvider) : IMappingRepository
{
    private const int SqliteConstraintErrorCode = 19;

    public async Task<ReservationResult> TryReserveAsync(ulong threadId, ulong channelId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.ThreadIssueMappings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.DiscordThreadId == threadId, cancellationToken);
        if (existing is not null)
        {
            return new ReservationResult(ReservationOutcome.AlreadyMapped, existing);
        }

        var now = timeProvider.GetUtcNow();
        db.ThreadIssueMappings.Add(new ThreadIssueMapping
        {
            DiscordThreadId = threadId,
            DiscordChannelId = channelId,
            Status = MappingStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new ReservationResult(ReservationOutcome.Reserved, null);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintErrorCode })
        {
            // Lost the race against a concurrent event for the same thread.
            db.ChangeTracker.Clear();
            var winner = await db.ThreadIssueMappings.AsNoTracking()
                .FirstOrDefaultAsync(m => m.DiscordThreadId == threadId, cancellationToken);
            return new ReservationResult(ReservationOutcome.AlreadyMapped, winner);
        }
    }

    public async Task CompleteAsync(ulong threadId, long issueNumber, string issueUrl, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ThreadIssueMappings.FirstAsync(m => m.DiscordThreadId == threadId, cancellationToken);
        row.GitHubIssueNumber = issueNumber;
        row.GitHubIssueUrl = issueUrl;
        row.Status = MappingStatus.Created;
        row.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkNotifiedAsync(ulong threadId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ThreadIssueMappings.FirstAsync(m => m.DiscordThreadId == threadId, cancellationToken);
        row.Status = MappingStatus.Notified;
        row.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseAsync(ulong threadId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.ThreadIssueMappings
            .Where(m => m.DiscordThreadId == threadId && m.Status == MappingStatus.Pending)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<ThreadIssueMapping?> FindAsync(ulong threadId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ThreadIssueMappings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.DiscordThreadId == threadId, cancellationToken);
    }

    public async Task<IReadOnlyList<ThreadIssueMapping>> GetStalePendingAsync(DateTimeOffset olderThan, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ThreadIssueMappings.AsNoTracking()
            .Where(m => m.Status == MappingStatus.Pending && m.CreatedAt < olderThan)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
