namespace ClaudeDash.Services;

public class ScanSnapshot
{
    public int SessionCount { get; set; }
    public int ProjectCount { get; set; }
    public int McpServerCount { get; set; }
    public int SkillCount { get; set; }
    public int AgentCount { get; set; }
    public int MemoryFileCount { get; set; }
    public int HookCount { get; set; }
    public DateTime LastScanTime { get; set; }
    public TimeSpan ScanDuration { get; set; }
    public List<string> ChangedCategories { get; set; } = [];
}

public interface IBackgroundScannerService
{
    /// <summary>
    /// Start the background polling loop.
    /// </summary>
    void Start(TimeSpan? interval = null);

    /// <summary>
    /// Stop the background polling loop.
    /// </summary>
    void Stop();

    /// <summary>
    /// Force an immediate scan (e.g., on manual refresh).
    /// </summary>
    Task<ScanSnapshot> ScanNowAsync();

    /// <summary>
    /// The most recent scan snapshot.
    /// </summary>
    ScanSnapshot? LatestSnapshot { get; }

    /// <summary>
    /// Whether the scanner is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Raised after each scan completes. The snapshot includes which categories changed.
    /// </summary>
    event Action<ScanSnapshot>? ScanCompleted;
}
