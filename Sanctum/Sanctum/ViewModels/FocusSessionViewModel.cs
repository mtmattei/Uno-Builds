using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sanctum.Services;

namespace Sanctum.ViewModels;

public partial class FocusSessionViewModel : ObservableObject
{
    private readonly IAppStateService _appState;
    private DispatcherTimer? _timer;

    public FocusSessionViewModel(IAppStateService appState)
    {
        _appState = appState;
    }

    [ObservableProperty]
    private string _objective = string.Empty;

    [ObservableProperty]
    private bool _isInputMode = true;

    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private int _remainingMinutes = 25;

    [ObservableProperty]
    private int _remainingSeconds;

    [ObservableProperty]
    private string _timerDisplay = "25:00";

    [RelayCommand]
    private void StartSession()
    {
        if (string.IsNullOrWhiteSpace(Objective)) return;

        IsInputMode = false;
        IsSessionActive = true;
        IsPaused = false;
        RemainingMinutes = 25;
        RemainingSeconds = 0;
        UpdateTimerDisplay();

        _appState.SetFocusSessionActive(true);
        StartTimer();
    }

    [RelayCommand]
    private void TogglePause()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
        {
            _timer?.Stop();
        }
        else
        {
            StartTimer();
        }
    }

    [RelayCommand]
    private void AddTime()
    {
        RemainingMinutes += 10;
        UpdateTimerDisplay();
    }

    [RelayCommand]
    private void EndSession()
    {
        _timer?.Stop();
        _timer = null;
        IsSessionActive = false;
        IsInputMode = true;
        Objective = string.Empty;
        _appState.SetFocusSessionActive(false);

        // Add reclaimed attention
        var minutesCompleted = 25 - RemainingMinutes;
        if (minutesCompleted > 0)
        {
            _appState.AddAttentionReclaimed(minutesCompleted);
        }
    }

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, object e)
    {
        if (RemainingSeconds > 0)
        {
            RemainingSeconds--;
        }
        else if (RemainingMinutes > 0)
        {
            RemainingMinutes--;
            RemainingSeconds = 59;
        }
        else
        {
            // Timer complete
            EndSession();
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        TimerDisplay = $"{RemainingMinutes:D2}:{RemainingSeconds:D2}";
    }
}
