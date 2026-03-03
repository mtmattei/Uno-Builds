using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class ShiftTrackerCard : UserControl
{
    private DispatcherTimer? _timer;

    public ShiftTrackerCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty ShiftStartProperty =
        DependencyProperty.Register(nameof(ShiftStart), typeof(TimeSpan), typeof(ShiftTrackerCard),
            new PropertyMetadata(new TimeSpan(8, 0, 0), OnShiftChanged));

    public static readonly DependencyProperty ShiftEndProperty =
        DependencyProperty.Register(nameof(ShiftEnd), typeof(TimeSpan), typeof(ShiftTrackerCard),
            new PropertyMetadata(new TimeSpan(17, 0, 0), OnShiftChanged));

    public TimeSpan ShiftStart
    {
        get => (TimeSpan)GetValue(ShiftStartProperty);
        set => SetValue(ShiftStartProperty, value);
    }

    public TimeSpan ShiftEnd
    {
        get => (TimeSpan)GetValue(ShiftEndProperty);
        set => SetValue(ShiftEndProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateDisplay();
        StartTimer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopTimer();
    }

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timer.Tick += (s, e) => UpdateDisplay();
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void UpdateDisplay()
    {
        var now = DateTime.Now.TimeOfDay;

        // Update current time
        if (CurrentTimeText != null)
        {
            CurrentTimeText.Text = DateTime.Now.ToString("h:mm tt");
        }

        // Calculate progress
        var totalShift = ShiftEnd - ShiftStart;
        var elapsed = now - ShiftStart;

        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        if (elapsed > totalShift) elapsed = totalShift;

        var progress = totalShift.TotalMinutes > 0
            ? elapsed.TotalMinutes / totalShift.TotalMinutes
            : 0;

        // Update progress bar
        if (ProgressFill != null && ProgressFill.Parent is Grid parent)
        {
            var maxWidth = parent.ActualWidth > 0 ? parent.ActualWidth : 300;
            ProgressFill.Width = maxWidth * progress;
        }

        // Update remaining time
        var remaining = ShiftEnd - now;
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

        if (TimeRemainingText != null)
        {
            if (remaining.TotalHours >= 1)
            {
                TimeRemainingText.Text = $"{(int)remaining.TotalHours}h {remaining.Minutes}m remaining";
            }
            else
            {
                TimeRemainingText.Text = $"{remaining.Minutes}m remaining";
            }
        }
    }

    private static void OnShiftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShiftTrackerCard card)
        {
            card.UpdateDisplay();
        }
    }
}
