using System;
using Liveline.Models;
using Microsoft.UI.Xaml.Input;

namespace SmartCity.Presentation;

public sealed partial class DashboardPage : Page
{
    // Simulated live building-load telemetry for the Liveline chart. A rolling window of points is
    // re-pushed each tick; Liveline lerps smoothly between frames for continuous motion. View-only
    // sample data — no business logic.
    private readonly DispatcherTimer _liveTimer = new() { Interval = TimeSpan.FromMilliseconds(700) };
    private readonly List<LivelinePoint> _liveWindow = new();
    private readonly Random _liveRng = new(74);
    private double _livePhase;
    private const int LiveWindowSize = 48;

    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LiveChart.Theme = new LivelineTheme { Color = "#2EE0E0", IsDark = true };

        // Seed the window so the chart opens full rather than drawing in from empty.
        var now = DateTimeOffset.Now;
        _liveWindow.Clear();
        for (var i = LiveWindowSize - 1; i >= 0; i--)
            _liveWindow.Add(new LivelinePoint(now.AddSeconds(-i * 0.7), SampleLoad()));
        PushLive();

        _liveTimer.Tick += OnLiveTick;
        _liveTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _liveTimer.Stop();
        _liveTimer.Tick -= OnLiveTick;
    }

    private void OnLiveTick(object? sender, object e)
    {
        _liveWindow.Add(new LivelinePoint(DateTimeOffset.Now, SampleLoad()));
        if (_liveWindow.Count > LiveWindowSize) _liveWindow.RemoveAt(0);
        PushLive();
    }

    private void PushLive()
    {
        var latest = _liveWindow[^1].Value;
        LiveChart.Data = new List<LivelinePoint>(_liveWindow);
        LiveChart.Value = latest;
        LiveValueText.Text = $"{latest:0} kW";
    }

    // Baseline office load with a slow swell plus small jitter — reads as live consumption.
    private double SampleLoad()
    {
        _livePhase += 0.22;
        var swell = Math.Sin(_livePhase) * 26 + Math.Sin(_livePhase * 0.37) * 14;
        var jitter = (_liveRng.NextDouble() - 0.5) * 12;
        return 318 + swell + jitter;
    }

    // ── View-only twin interaction. Forwards to the render control; no business logic here. ──
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => Twin.ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => Twin.ZoomOut();

    private void Toggle2D_Click(object sender, RoutedEventArgs e)
    {
        Twin.Toggle2D();
        Toggle2DBtn.Content = Twin.Is2D ? "3D" : "2D";
    }

    private void TogglePause_Click(object sender, RoutedEventArgs e)
    {
        Twin.TogglePause();
        PauseBtn.Content = Twin.IsPaused ? "▶" : "❚❚";
    }

    // Focus selector switches which building is centered + active in the twin.
    private void FocusSelect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string tag } && int.TryParse(tag, out var index))
            Twin.Focus = index;
    }

    // Ruler click/hover selects a floor via the shared ActiveFloorIndex DP and pins the sweep on it.
    private void RulerFloor_Click(object sender, RoutedEventArgs e) => SelectRulerFloor(sender);
    private void RulerFloor_PointerEntered(object sender, PointerRoutedEventArgs e) => SelectRulerFloor(sender);

    private void SelectRulerFloor(object sender)
    {
        if (sender is FrameworkElement { Tag: string tag } && int.TryParse(tag, out var index))
        {
            Twin.ActiveFloorIndex = index; // two-way DP → model state → tooltip + panel update
            Twin.Pause();
            PauseBtn.Content = "▶";
        }
    }
}
