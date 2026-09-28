using Windows.UI.ViewManagement;

namespace ReticleLab.Presentation;

public enum ReticleMode
{
    Scan,
    Lock,
    Idle,
}

public partial class MainViewModel : ObservableObject
{
    // Uno implements AnimationsEnabled on Android only; desktop and WASM report motion on.
    // Kept as a field per the spec so the instance outlives construction.
    private readonly UISettings _uiSettings = new();

    public MainViewModel()
    {
        MotionEnabled = _uiSettings.AnimationsEnabled;
    }

    public bool MotionEnabled { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActive), nameof(IsScan), nameof(IsLock), nameof(IsIdle), nameof(StatusText))]
    public partial ReticleMode Mode { get; set; } = ReticleMode.Scan;

    // Idle leaves this unchanged. It is raised from OnModeChanged, before IsActive, so
    // Idle → Lock goes Inactive → DeterminateActive without passing through Active.
    [ObservableProperty]
    public partial bool IsIndeterminate { get; private set; } = true;

    // With motion off, Scan shows a still, dimmed ring; the status text and dots carry the state.
    public bool IsActive => Mode switch
    {
        ReticleMode.Scan => MotionEnabled,
        ReticleMode.Lock => true,
        _ => false,
    };

    public bool IsScan => Mode == ReticleMode.Scan;

    public bool IsLock => Mode == ReticleMode.Lock;

    public bool IsIdle => Mode == ReticleMode.Idle;

    public string StatusText => Mode switch
    {
        ReticleMode.Scan => "Scanning",
        ReticleMode.Lock => "Target locked",
        _ => "Standby",
    };

    partial void OnModeChanged(ReticleMode value)
    {
        if (value != ReticleMode.Idle)
        {
            IsIndeterminate = value == ReticleMode.Scan;
        }
    }

    [RelayCommand]
    private void SetMode(ReticleMode mode) => Mode = mode;
}
