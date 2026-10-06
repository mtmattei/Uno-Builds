using System.Runtime.CompilerServices;

namespace SalesHeatmap.Presentation;

public partial record MainModel
{
    private static readonly TimeSpan SnapshotInterval = TimeSpan.FromSeconds(2);

    public IFeed<SalesSnapshot> Snapshot => Feed.AsyncEnumerable(StreamSnapshots);

    private static async IAsyncEnumerable<SalesSnapshot> StreamSnapshots(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var random = new Random();
        var snapshot = SalesSnapshot.Initial;
        yield return snapshot;

        using var timer = new PeriodicTimer(SnapshotInterval);
        while (await timer.WaitForNextTickAsync(ct))
        {
            snapshot = snapshot.NextRandom(random);
            yield return snapshot;
        }
    }
}
