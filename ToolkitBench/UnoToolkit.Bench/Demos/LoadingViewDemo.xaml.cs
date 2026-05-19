using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Demos;

public sealed partial class LoadingViewDemo : UserControl
{
    private enum SourceState { Idle, Running, Loaded }

    private SourceState _state = SourceState.Idle;
    private DispatcherTimer? _tickTimer;
    private Stopwatch? _stopwatch;
    private Storyboard? _progressStoryboard;
    private DispatcherTimer? _completionTimer;

    private readonly Random _rand = new();

    public LoadingViewDemo()
    {
        this.InitializeComponent();
        SetState(SourceState.Idle);
    }

    public void TriggerAction() => OnActionClick(this, new RoutedEventArgs());

    private void OnActionClick(object sender, RoutedEventArgs e)
    {
        switch (_state)
        {
            case SourceState.Idle:    StartRun(); break;
            case SourceState.Running: break; // ignore during run
            case SourceState.Loaded:  Reset(); break;
        }
    }

    private void StartRun()
    {
        var targetMs = 1200 + _rand.Next(700);
        SetState(SourceState.Running);

        _stopwatch = Stopwatch.StartNew();
        // 50ms (20Hz) — still reads smooth for an F3 seconds display, and frees
        // UI thread cycles for concurrent animations (drawer slide, chip cascade,
        // tab morph) that would otherwise compete with this timer's text updates.
        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _tickTimer.Tick += (s, _) =>
        {
            ElapsedReadout.Text = (_stopwatch!.Elapsed.TotalSeconds).ToString("F3");
        };
        _tickTimer.Start();

        // ScaleX 0→1 on both line + halo (glow) for the spec'd soft-glow effect.
        ProgressTransform.ScaleX = 0;
        ProgressGlowTransform.ScaleX = 0;
        _progressStoryboard = new Storyboard();
        AddProgressKey(_progressStoryboard, ProgressTransform, targetMs);
        AddProgressKey(_progressStoryboard, ProgressGlowTransform, targetMs);
        _progressStoryboard.Begin();

        _completionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(targetMs) };
        _completionTimer.Tick += (s, _) =>
        {
            _completionTimer!.Stop();
            FinishRun();
        };
        _completionTimer.Start();
    }

    private void FinishRun()
    {
        _tickTimer?.Stop();
        _tickTimer = null;
        var finalElapsed = _stopwatch?.Elapsed ?? TimeSpan.Zero;
        _stopwatch = null;

        LoadedNum.Text = finalElapsed.TotalSeconds.ToString("F3") + "s";
        SetState(SourceState.Loaded);
    }

    private void Reset()
    {
        _tickTimer?.Stop();
        _tickTimer = null;
        _completionTimer?.Stop();
        _completionTimer = null;
        _progressStoryboard?.Stop();
        _progressStoryboard = null;
        _stopwatch = null;
        ProgressTransform.ScaleX = 0;
        ProgressGlowTransform.ScaleX = 0;
        ElapsedReadout.Text = "0.000";
        SetState(SourceState.Idle);
    }

    private static void AddProgressKey(Storyboard sb, CompositeTransform target, int targetMs)
    {
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        anim.KeyFrames.Add(new SplineDoubleKeyFrame
        {
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(targetMs)),
            Value = 1.0,
            KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.65, 0), ControlPoint2 = new Windows.Foundation.Point(0.35, 1) },
        });
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "ScaleX");
        sb.Children.Add(anim);
    }

    private void SetState(SourceState newState)
    {
        _state = newState;

        IdlePanel.Visibility    = newState == SourceState.Idle    ? Visibility.Visible : Visibility.Collapsed;
        RunningPanel.Visibility = newState == SourceState.Running ? Visibility.Visible : Visibility.Collapsed;
        LoadedPanel.Visibility  = newState == SourceState.Loaded  ? Visibility.Visible : Visibility.Collapsed;

        switch (newState)
        {
            case SourceState.Idle:
                ActionButton.Content = "Run task";
                ActionButton.IsEnabled = true;
                StatePillText.Text = "IDLE";
                StatePillText.Foreground = Theme.Fg4;
                StatePill.BorderBrush = Theme.Rule;
                StatePill.Background = Theme.Bg1;
                ProgressTransform.ScaleX = 0;
                ProgressGlowTransform.ScaleX = 0;
                break;
            case SourceState.Running:
                ActionButton.Content = "Running";
                ActionButton.IsEnabled = false;
                StatePillText.Text = "RUNNING";
                StatePillText.Foreground = Theme.Accent;
                StatePill.BorderBrush = Theme.AccentSoft;
                StatePill.Background = Theme.AccentSoft;
                break;
            case SourceState.Loaded:
                ActionButton.Content = "Reset";
                ActionButton.IsEnabled = true;
                StatePillText.Text = "DONE";
                StatePillText.Foreground = Theme.Ok;
                StatePill.BorderBrush = Theme.OkSoft;
                StatePill.Background = Theme.OkSoft;
                break;
        }
    }
}
