// TimerService is deprecated.
// The BackgroundScannerService now serves as the single polling source.
// Clock updates are handled by a minimal DispatcherTimer in ShellPage.
// This file is kept as a stub for backward compatibility during migration.

namespace ClaudeDash.Services;

[Obsolete("Use BackgroundScannerService for polling. Clock uses DispatcherTimer in ShellPage.")]
public class TimerService : IDisposable
{
    public event Action? OnTick;
    public void Start() { }
    public void Stop() { }
    public void Dispose() => GC.SuppressFinalize(this);
}
