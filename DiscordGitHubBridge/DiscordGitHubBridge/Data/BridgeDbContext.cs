using DiscordGitHubBridge.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DiscordGitHubBridge.Data;

public sealed class BridgeDbContext(DbContextOptions<BridgeDbContext> options) : DbContext(options)
{
    public DbSet<ThreadIssueMapping> ThreadIssueMappings => Set<ThreadIssueMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var mapping = modelBuilder.Entity<ThreadIssueMapping>();
        mapping.ToTable("ThreadIssueMappings");
        mapping.HasKey(m => m.DiscordThreadId);

        // Discord snowflakes are ulong; SQLite stores 64-bit signed integers. Snowflakes never exceed long.MaxValue.
        mapping.Property(m => m.DiscordThreadId).ValueGeneratedNever().HasConversion<long>();
        mapping.Property(m => m.DiscordChannelId).HasConversion<long>();
        mapping.Property(m => m.GitHubIssueUrl).HasMaxLength(512);
        mapping.Property(m => m.Status).HasConversion<int>();

        // SQLite cannot compare DateTimeOffset stored as text, so keep UTC ticks as INTEGER for the stale-row query.
        mapping.Property(m => m.CreatedAt).HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));
        mapping.Property(m => m.UpdatedAt).HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero));
        mapping.HasIndex(m => m.Status);
    }
}
