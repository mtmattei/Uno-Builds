namespace Orbital.Services;

public interface IClockService
{
    IAsyncEnumerable<DateTime> GetTimeStream(CancellationToken ct);
}
