using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Demos;

public sealed partial class DrawerControlDemo : UserControl
{
    private const double PanelWidth = 280;
    private const double CommitThreshold = 0.15;
    private const double CommitRamp = 0.30;
    private const double Rubber = 0.33;
    private const int SnapDurationMs = 540;

    private double _openness = 0;            // logical 0..1, clamped
    private double _visualOpenness = 0;      // includes rubber-band overdrag
    private bool _dragging = false;
    private double _startX;
    private double _startOpenness;
    private double _pressX;
    private Storyboard? _running;

    public DrawerControlDemo()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => SyncVisuals();
    }

    private void OnTabPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        _dragging = true;
        var p = e.GetCurrentPoint(this).Position;
        _startX = p.X;
        _pressX = p.X;
        _startOpenness = _openness;
        fe.CapturePointer(e.Pointer);
        _running?.Stop();
        SetState("pulling");
        SetGripActive(true);
    }

    private void OnTabMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_dragging) return;
        var x = e.GetCurrentPoint(this).Position.X;
        var dx = _startX - x;                       // drag-left → positive
        var rawOpen = _startOpenness + dx / PanelWidth;
        var newVisual = ApplyResistance(rawOpen);
        // Skip sub-pixel changes — pointer events fire ~120Hz on desktop and
        // most consecutive samples produce no visible delta.
        if (Math.Abs(newVisual - _visualOpenness) < 0.001) return;
        _visualOpenness = newVisual;
        _openness = Math.Clamp(_visualOpenness, 0, 1);
        SyncVisuals();
    }

    private void OnTabReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        if (sender is FrameworkElement fe)
            fe.ReleasePointerCapture(e.Pointer);
        SetGripActive(false);

        var releaseX = e.GetCurrentPoint(this).Position.X;
        var moved = Math.Abs(releaseX - _pressX);

        double target;
        if (moved < 4)
            // Click without drag → toggle
            target = _openness < 0.5 ? 1 : 0;
        else
            target = _openness >= 0.5 ? 1 : 0;

        SnapTo(target);
    }

    private double ApplyResistance(double rawOpen)
    {
        double r = rawOpen;

        // Resistance only when starting from near-closed.
        if (_startOpenness < CommitThreshold)
        {
            if (rawOpen < CommitThreshold)
            {
                // 35% sensitivity zone
                r = rawOpen * 0.35;
            }
            else if (rawOpen < CommitThreshold + CommitRamp)
            {
                // Ramp from 35%-relative position to full 1:1 by end of ramp
                var startVal = CommitThreshold * 0.35;
                var endVal = CommitThreshold + CommitRamp;
                var t = (rawOpen - CommitThreshold) / CommitRamp;
                r = startVal + t * (endVal - startVal);
            }
            // else: 1:1 mapping (rawOpen unchanged)
        }

        // Rubber-band overdrag
        if (r < 0) r = r * Rubber;
        if (r > 1) r = 1 + (r - 1) * Rubber;
        return r;
    }

    private void SyncVisuals()
    {
        DrawerTransform.TranslateX = (1 - _visualOpenness) * PanelWidth;
        ProgressFillTransform.ScaleX = Math.Clamp(_openness, 0, 1);

        if (!_dragging)
            SetState(_openness >= 0.5 ? "open" : "closed");
        else if (_openness <= 0.02)
            SetState("closed");
        else if (_openness >= 0.98)
            SetState("open");
        else
            SetState(_openness > 0.5 ? "partial" : "pulling");
    }

    private void SetState(string state)
    {
        StateLabel.Text = state;
        // Endpoint labels are brighter; transient states are dimmer.
        StateLabel.Foreground = (state == "open" || state == "closed") ? Theme.Fg2 : Theme.Fg4;
    }

    private void SetGripActive(bool active)
    {
        var dotBrush = active ? Theme.Accent : Theme.Fg4;
        GripDot1.Fill = dotBrush;
        GripDot2.Fill = dotBrush;
        GripDot3.Fill = dotBrush;
        PullTab.Background = active ? Theme.AccentSoft : Theme.DrawerTab;
    }

    public void TriggerToggle()
    {
        var target = _openness < 0.5 ? 1.0 : 0.0;
        _running?.Stop();
        SnapTo(target);
    }

    private void SnapTo(double target)
    {
        var startTx = (1 - _visualOpenness) * PanelWidth;
        var endTx = (1 - target) * PanelWidth;

        // Spring physics — single BackEase EaseOut for a continuous overshoot trajectory
        // instead of the keyframed 3-stitched curves. Reads as one fluid spring.
        var sb = new Storyboard();
        var anim = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(SnapDurationMs),
            From = startTx,
            To = endTx,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 },
        };
        Storyboard.SetTarget(anim, DrawerTransform);
        Storyboard.SetTargetProperty(anim, "TranslateX");
        sb.Children.Add(anim);

        // Progress fill animates synchronously
        var progressAnim = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(SnapDurationMs),
            From = ProgressFillTransform.ScaleX,
            To = target,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(progressAnim, ProgressFillTransform);
        Storyboard.SetTargetProperty(progressAnim, "ScaleX");
        sb.Children.Add(progressAnim);

        sb.Completed += (_, _) =>
        {
            _openness = target;
            _visualOpenness = target;
            SetState(target >= 0.5 ? "open" : "closed");
        };
        sb.Begin();
        _running = sb;
    }
}
