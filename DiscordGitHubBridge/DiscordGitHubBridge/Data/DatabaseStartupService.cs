using DiscordGitHubBridge.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DiscordGitHubBridge.Data;

/// <summary>Applies migrations and reports Pending rows left behind by an earlier crash. Runs before the gateway connects.</summary>
public sealed class DatabaseStartupService(
    IDbContextFactory<BridgeDbContext> contextFactory,
    IMappingRepository repository,
    TimeProvider timeProvider,
    ILogger<DatabaseStartupService> logger) : IHostedService
{
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(5);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using (var db = await contextFactory.CreateDbContextAsync(cancellationToken))
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        var stale = await repository.GetStalePendingAsync(timeProvider.GetUtcNow() - StaleAfter, cancellationToken);
        foreach (var row in stale)
        {
            logger.LogWarning(
                "Mapping for Discord thread {DiscordThreadId} in channel {DiscordChannelId} is still Pending since {CreatedAt}. Check GitHub for an issue linking to the thread before re-tagging.",
                row.DiscordThreadId, row.DiscordChannelId, row.CreatedAt);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
