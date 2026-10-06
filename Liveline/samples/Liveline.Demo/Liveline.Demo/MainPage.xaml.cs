using Liveline.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Liveline.Demo;

public sealed partial class MainPage : Page
{
    private const int MaxPoints = 60;

    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _loadingTimer;
    private readonly Queue<LivelinePoint> _data = new(MaxPoints);
    private readonly Random _rng = new();
    private double _currentValue = 100.0;
    private string _currentColor = "#4CAF50";

    public MainPage()
    {
        this.InitializeComponent();

        Chart.Theme = new LivelineTheme { Color = _currentColor, IsDark = true };
        Chart.Momentum = true;
        Chart.IsLoading = true;

        _loadingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _loadingTimer.Tick += OnLoadingComplete;
        _loadingTimer.Start();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += OnTimerTick;
    }

    private void OnLoadingComplete(object? sender, object e)
    {
        _loadingTimer.Stop();

        var now = DateTimeOffset.Now;
        for (int i = MaxPoints; i > 0; i--)
        {
            _currentValue += (_rng.NextDouble() - 0.5) * 4;
            _data.Enqueue(new LivelinePoint(now.AddMilliseconds(-i * 100), _currentValue));
        }

        Chart.IsLoading = false;
        PushData();

        _timer.Start();
    }

    private void OnTimerTick(object? sender, object e)
    {
        // Volatile random walk: occasional large spikes and dips
        double step = (_rng.NextDouble() - 0.5) * 8;
        if (_rng.NextDouble() < 0.08)
            step *= 6;
        _currentValue += step;

        _data.Enqueue(new LivelinePoint(DateTimeOffset.Now, _currentValue));
        while (_data.Count > MaxPoints)
            _data.Dequeue();

        PushData();
    }

    private void PushData()
    {
        Chart.Data = _data.ToArray();
        Chart.Value = _currentValue;
    }

    private void OnToggle(object sender, RoutedEventArgs e)
    {
        Chart.ShowGrid = GridSwitch.IsOn;
        Chart.ShowBadge = BadgeSwitch.IsOn;
        Chart.ShowFill = FillSwitch.IsOn;
        Chart.Momentum = DotSwitch.IsOn;
        Chart.IsPaused = PauseSwitch.IsOn;
        Chart.Theme = new LivelineTheme { Color = _currentColor, IsDark = DarkSwitch.IsOn };
    }

    private void OnColorPreset(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string color)
        {
            _currentColor = color;
            Chart.Theme = new LivelineTheme { Color = _currentColor, IsDark = DarkSwitch.IsOn };
        }
    }
}
