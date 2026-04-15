namespace Orbital.Services;

public class ClockService : IClockService
{
    public async IAsyncEnumerable<DateTime> GetTimeStream(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            yield return DateTime.Now;
            await Task.Delay(1000, ct);
        }
    }
}
