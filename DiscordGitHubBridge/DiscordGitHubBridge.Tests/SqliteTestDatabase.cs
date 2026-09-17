using DiscordGitHubBridge.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiscordGitHubBridge.Tests;

/// <summary>An in-memory SQLite database that lives as long as the open connection. Migrations are applied so the migration itself is tested.</summary>
internal sealed class SqliteTestDatabase : IDbContextFactory<BridgeDbContext>, IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<BridgeDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<BridgeDbContext>().UseSqlite(_connection).Options;
        using var db = CreateDbContext();
        db.Database.Migrate();
    }

    public BridgeDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
