using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Controls;

public sealed class SplitFlapText : UserControl
{
    public static readonly DependencyProperty TargetTextProperty =
        DependencyProperty.Register(
            nameof(TargetText), typeof(string), typeof(SplitFlapText),
            new PropertyMetadata(string.Empty, OnTargetTextChanged));

    public string TargetText
    {
        get => (string)GetValue(TargetTextProperty);
        set => SetValue(TargetTextProperty, value);
    }

    private readonly StackPanel _host = new()
    {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };
    private readonly Random _rand = new();
    private readonly Dictionary<int, DispatcherTimer> _cycleTimers = new();
    private readonly Dictionary<int, DispatcherTimer> _delayTimers = new();
    private readonly List<TextBlock> _cells = new();

    private const int CharStartDelayMs = 50;
    private const int CycleIntervalMs = 55;

    public SplitFlapText()
    {
        Content = _host;
        IsTabStop = false;
    }

    private static void OnTargetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SplitFlapText)d).Cycle((string)e.NewValue ?? string.Empty);
    }

    public TimeSpan EstimatedDuration(string target)
    {
        var len = (target ?? string.Empty).Length;
        return TimeSpan.FromMilliseconds(len * CharStartDelayMs + 7 * CycleIntervalMs + 100);
    }

    // Cache lookups once per control — Cycle is called per nav click, but the
    // underlying resources are immutable for the app lifetime.
    private Brush? _muteBrush;
    private Brush? _inkBrush;
    private Style? _titleStyle;

    private void Cycle(string target)
    {
        foreach (var t in _cycleTimers.Values) t.Stop();
        foreach (var t in _delayTimers.Values) t.Stop();
        _cycleTimers.Clear();
        _delayTimers.Clear();

        _muteBrush  ??= (Brush)Application.Current.Resources["Fg4Brush"];
        _inkBrush   ??= (Brush)Application.Current.Resources["Fg1Brush"];
        _titleStyle ??= (Style)Application.Current.Resources["NavTitleStyle"];

        while (_cells.Count < target.Length)
        {
            var tb = new TextBlock
            {
                Style = _titleStyle,
                MinWidth = 10,
                TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new CompositeTransform { ScaleY = 1.0 },
            };
            _host.Children.Add(tb);
            _cells.Add(tb);
        }

        // Collapse unused cells so they don't take MinWidth=10 in the layout —
        // otherwise a shorter title would render left-of-center inside the host.
        for (int i = 0; i < _cells.Count; i++)
        {
            if (i < target.Length)
            {
                _cells[i].Visibility = Visibility.Visible;
            }
            else
            {
                _cells[i].Text = string.Empty;
                _cells[i].Visibility = Visibility.Collapsed;
            }
        }

        for (int i = 0; i < target.Length; i++)
        {
            var idx = i;
            var targetChar = target[i];

            if (idx == 0)
            {
                StartCharCycle(idx, targetChar, _inkBrush, _muteBrush);
            }
            else
            {
                var delay = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(idx * CharStartDelayMs)
                };
                _delayTimers[idx] = delay;
                delay.Tick += (s, _) =>
                {
                    delay.Stop();
                    _delayTimers.Remove(idx);
                    StartCharCycle(idx, targetChar, _inkBrush, _muteBrush);
                };
                delay.Start();
            }
        }
    }

    private void StartCharCycle(int index, char targetChar, Brush settle, Brush cycle)
    {
        var cell = _cells[index];
        cell.Foreground = cycle;

        var cycleCount = 0;
        var maxCycles = 4 + _rand.Next(3);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CycleIntervalMs) };
        _cycleTimers[index] = timer;

        timer.Tick += (s, _) =>
        {
            cycleCount++;
            if (cycleCount >= maxCycles)
            {
                timer.Stop();
                _cycleTimers.Remove(index);
                cell.Text = targetChar.ToString();
                cell.Foreground = settle;
                RunSettleTick(cell);
                return;
            }
            cell.Text = ((char)('A' + _rand.Next(26))).ToString();
        };
        timer.Start();
    }

    // Run a single ScaleY tick when the character settles. Per-cycle ticks were dropped
    // — the rapid text flicker reads as the flap, the settle tick just punctuates it.
    private static void RunSettleTick(TextBlock cell)
    {
        if (cell.RenderTransform is not CompositeTransform transform) return;

        var sb = new Storyboard();
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1.0 });
        anim.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50)), Value = 0.4 });
        anim.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)), Value = 1.0 });
        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, "ScaleY");
        sb.Children.Add(anim);
        sb.Begin();
    }
}
