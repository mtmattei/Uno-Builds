using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;

namespace AgentNotifier.Controls;

public sealed partial class SegmentedProgress : UserControl
{
    private const char FilledChar = '\u2588'; // █
    private const char EmptyChar = '\u2591';  // ░
    private const int BounceBlockSize = 3;
    private const int MinBarWidth = 10;
    private const double CharWidthRatio = 0.6; // monospace char width ≈ 0.6 × font size

    private DispatcherQueueTimer? _animTimer;
    private double _displayValue;
    private double _targetValue;
    private int _bouncePos;
    private int _bounceDir = 1;
    private int _barWidth = 40;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(SegmentedProgress),
            new PropertyMetadata(0, OnValueChanged));

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(SegmentedProgress),
            new PropertyMetadata(false, OnIsIndeterminateChanged));

    public static readonly DependencyProperty ActiveColorProperty =
        DependencyProperty.Register(nameof(ActiveColor), typeof(Brush), typeof(SegmentedProgress),
            new PropertyMetadata(null, OnActiveColorChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public Brush ActiveColor
    {
        get => (Brush)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public SegmentedProgress()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RecalculateBarWidth(e.NewSize.Width);
    }

    private void RecalculateBarWidth(double availableWidth)
    {
        if (availableWidth <= 0) return;

        var fontSize = ProgressTextBlock.FontSize;
        var charWidth = fontSize * CharWidthRatio;
        var totalChars = (int)(availableWidth / charWidth);

        // Overhead: "[" + "]" = 2 chars, plus " 100%" = 5 chars for determinate
        var overhead = IsIndeterminate ? 2 : 7;
        var newWidth = Math.Max(MinBarWidth, totalChars - overhead);

        if (newWidth != _barWidth)
        {
            _barWidth = newWidth;
            // Reset bounce position if it exceeds new width
            if (_bouncePos > _barWidth - BounceBlockSize)
                _bouncePos = Math.Max(0, _barWidth - BounceBlockSize);
            UpdateProgressDisplay();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animTimer?.Stop();
        _animTimer = DispatcherQueue.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(60);
        _animTimer.Tick += OnAnimTick;

        if (ActiveColor != null)
            FilledRun.Foreground = ActiveColor;

        if (IsIndeterminate)
        {
            _bouncePos = 0;
            _bounceDir = 1;
            _animTimer.Start();
        }
        else if (Math.Abs(_displayValue - _targetValue) > 0.5)
        {
            _animTimer.Start();
        }

        RecalculateBarWidth(ActualWidth);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animTimer?.Stop();
    }

    private void OnAnimTick(DispatcherQueueTimer sender, object args)
    {
        if (IsIndeterminate)
        {
            _bouncePos += _bounceDir;
            if (_bouncePos >= _barWidth - BounceBlockSize)
            {
                _bouncePos = _barWidth - BounceBlockSize;
                _bounceDir = -1;
            }
            else if (_bouncePos <= 0)
            {
                _bouncePos = 0;
                _bounceDir = 1;
            }
        }
        else
        {
            var diff = _targetValue - _displayValue;
            if (Math.Abs(diff) < 0.5)
            {
                _displayValue = _targetValue;
                _animTimer?.Stop();
            }
            else
            {
                _displayValue += diff * 0.12;
            }
        }

        UpdateProgressDisplay();
    }

    private void UpdateProgressDisplay()
    {
        if (IsIndeterminate)
        {
            var preCount = _bouncePos;
            var filledCount = Math.Min(BounceBlockSize, _barWidth - _bouncePos);
            var postCount = Math.Max(0, _barWidth - _bouncePos - BounceBlockSize);

            PreRun.Text = preCount > 0 ? new string(EmptyChar, preCount) : "";
            FilledRun.Text = new string(FilledChar, filledCount);
            PostRun.Text = postCount > 0 ? new string(EmptyChar, postCount) : "";
            PercentRun.Text = "";
        }
        else
        {
            var filledCount = (int)(_displayValue / 100.0 * _barWidth);
            filledCount = Math.Clamp(filledCount, 0, _barWidth);

            PreRun.Text = "";
            FilledRun.Text = filledCount > 0 ? new string(FilledChar, filledCount) : "";
            PostRun.Text = new string(EmptyChar, _barWidth - filledCount);
            PercentRun.Text = $" {(int)_displayValue,3}%";
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedProgress p)
        {
            p._targetValue = (int)e.NewValue;
            if (p._animTimer != null && !p.IsIndeterminate)
                p._animTimer.Start();
        }
    }

    private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedProgress p)
        {
            if ((bool)e.NewValue)
            {
                p._bouncePos = 0;
                p._bounceDir = 1;
                p._animTimer?.Start();
            }
            else
            {
                p._animTimer?.Stop();
                p.UpdateProgressDisplay();
            }
        }
    }

    private static void OnActiveColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedProgress p && e.NewValue is Brush brush)
            p.FilledRun.Foreground = brush;
    }
}
