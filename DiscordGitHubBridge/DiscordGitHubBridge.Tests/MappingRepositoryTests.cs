using DiscordGitHubBridge.Mapping;

namespace DiscordGitHubBridge.Tests;

public sealed class MappingRepositoryTests : IDisposable
{
    private const ulong Thread = 1550000000000000001UL;
    private static readonly DateTimeOffset T0 = new(2026, 9, 17, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteTestDatabase _db = new();
    private readonly FakeTimeProvider _clock = new(T0);
    private readonly MappingRepository _repo;

    public MappingRepositoryTests() => _repo = new MappingRepository(_db, _clock);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Reserve_inserts_pending_row()
    {
        var result = await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);

        Assert.Equal(ReservationOutcome.Reserved, result.Outcome);
        var row = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.NotNull(row);
        Assert.Equal(MappingStatus.Pending, row.Status);
        Assert.Equal(TestData.Forum, row.DiscordChannelId);
        Assert.Null(row.GitHubIssueNumber);
        Assert.Equal(T0, row.CreatedAt);
    }

    [Fact]
    public async Task Second_reserve_reports_existing_mapping()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);
        await _repo.CompleteAsync(Thread, 42, "https://github.com/o/r/issues/42", CancellationToken.None);

        var result = await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);

        Assert.Equal(ReservationOutcome.AlreadyMapped, result.Outcome);
        Assert.NotNull(result.Existing);
        Assert.Equal(42, result.Existing.GitHubIssueNumber);
    }

    [Fact]
    public async Task Pending_row_also_blocks_reservation()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);

        var result = await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);

        Assert.Equal(ReservationOutcome.AlreadyMapped, result.Outcome);
        Assert.Equal(MappingStatus.Pending, result.Existing!.Status);
    }

    [Fact]
    public async Task Complete_then_notified_advances_status()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);
        _clock.Now = T0.AddSeconds(5);

        await _repo.CompleteAsync(Thread, 7, "https://github.com/o/r/issues/7", CancellationToken.None);
        var created = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.Equal(MappingStatus.Created, created!.Status);
        Assert.Equal(7, created.GitHubIssueNumber);
        Assert.Equal("https://github.com/o/r/issues/7", created.GitHubIssueUrl);
        Assert.Equal(T0.AddSeconds(5), created.UpdatedAt);

        await _repo.MarkNotifiedAsync(Thread, CancellationToken.None);
        var notified = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.Equal(MappingStatus.Notified, notified!.Status);
    }

    [Fact]
    public async Task Release_deletes_only_pending_rows()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);
        await _repo.ReleaseAsync(Thread, CancellationToken.None);
        Assert.Null(await _repo.FindAsync(Thread, CancellationToken.None));

        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);
        await _repo.CompleteAsync(Thread, 1, "https://github.com/o/r/issues/1", CancellationToken.None);
        await _repo.ReleaseAsync(Thread, CancellationToken.None);
        Assert.NotNull(await _repo.FindAsync(Thread, CancellationToken.None));
    }

    [Fact]
    public async Task Stale_pending_query_returns_only_old_pending_rows()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);
        await _repo.TryReserveAsync(Thread + 1, TestData.Forum, CancellationToken.None);
        await _repo.CompleteAsync(Thread + 1, 2, "https://github.com/o/r/issues/2", CancellationToken.None);
        _clock.Now = T0.AddMinutes(10);
        await _repo.TryReserveAsync(Thread + 2, TestData.Forum, CancellationToken.None);

        var stale = await _repo.GetStalePendingAsync(T0.AddMinutes(5), CancellationToken.None);

        Assert.Equal([Thread], stale.Select(m => m.DiscordThreadId));
    }

    [Fact]
    public async Task Mappings_survive_a_new_context()
    {
        await _repo.TryReserveAsync(Thread, TestData.Forum, CancellationToken.None);

        var other = new MappingRepository(_db, _clock);
        Assert.NotNull(await other.FindAsync(Thread, CancellationToken.None));
    }
}
