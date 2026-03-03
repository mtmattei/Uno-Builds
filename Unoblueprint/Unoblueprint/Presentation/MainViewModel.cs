namespace Unoblueprint.Presentation;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;
    private DispatcherTimer? _timer;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private int focusMinutes = 25;

    [ObservableProperty]
    private int breakMinutes = 5;

    [ObservableProperty]
    private int timeRemaining = 1500; // 25 minutes in seconds

    [ObservableProperty]
    private string timeDisplay = "25:00";

    [ObservableProperty]
    private bool isRunning = false;

    [ObservableProperty]
    private bool isFocusMode = true;

    [ObservableProperty]
    private double progress = 1.0;

    [ObservableProperty]
    private int completedSessions = 0;

    [ObservableProperty]
    private string moodEmoji = "😊";

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Focus Flow";
        GoToSecond = new AsyncRelayCommand(GoToSecondView);
    }

    public string? Title { get; }

    public ICommand GoToSecond { get; }

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
    }

    [RelayCommand]
    private void StartPause()
    {
        IsRunning = !IsRunning;

        if (IsRunning)
        {
            _timer ??= new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        else
        {
            _timer?.Stop();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        _timer?.Stop();
        IsRunning = false;
        TimeRemaining = IsFocusMode ? FocusMinutes * 60 : BreakMinutes * 60;
        UpdateTimeDisplay();
        Progress = 1.0;
    }

    [RelayCommand]
    private void SwitchMode()
    {
        _timer?.Stop();
        IsRunning = false;
        IsFocusMode = !IsFocusMode;
        TimeRemaining = IsFocusMode ? FocusMinutes * 60 : BreakMinutes * 60;
        UpdateTimeDisplay();
        Progress = 1.0;
    }

    [RelayCommand]
    private void SetMood(string emoji)
    {
        MoodEmoji = emoji;
    }

    private void Timer_Tick(object? sender, object e)
    {
        if (TimeRemaining > 0)
        {
            TimeRemaining--;
            UpdateTimeDisplay();

            int totalTime = IsFocusMode ? FocusMinutes * 60 : BreakMinutes * 60;
            Progress = (double)TimeRemaining / totalTime;
        }
        else
        {
            _timer?.Stop();
            IsRunning = false;

            if (IsFocusMode)
            {
                CompletedSessions++;
            }

            SwitchMode();
        }
    }

    private void UpdateTimeDisplay()
    {
        int minutes = TimeRemaining / 60;
        int seconds = TimeRemaining % 60;
        TimeDisplay = $"{minutes:D2}:{seconds:D2}";
    }

}
