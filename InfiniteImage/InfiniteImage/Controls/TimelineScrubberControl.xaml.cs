using InfiniteImage.Models;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace InfiniteImage.Controls;

public sealed partial class TimelineScrubberControl : UserControl
{
    public TimelineScrubberControl()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty CurrentZProperty =
        DependencyProperty.Register(
            nameof(CurrentZ),
            typeof(float),
            typeof(TimelineScrubberControl),
            new PropertyMetadata(0f, OnCurrentZChanged));

    public static readonly DependencyProperty MinZProperty =
        DependencyProperty.Register(
            nameof(MinZ),
            typeof(float),
            typeof(TimelineScrubberControl),
            new PropertyMetadata(0f));

    public static readonly DependencyProperty MaxZProperty =
        DependencyProperty.Register(
            nameof(MaxZ),
            typeof(float),
            typeof(TimelineScrubberControl),
            new PropertyMetadata(1000f));

    public static readonly DependencyProperty EarliestDateProperty =
        DependencyProperty.Register(
            nameof(EarliestDate),
            typeof(DateTimeOffset),
            typeof(TimelineScrubberControl),
            new PropertyMetadata(DateTimeOffset.Now, OnDateChanged));

    public static readonly DependencyProperty LatestDateProperty =
        DependencyProperty.Register(
            nameof(LatestDate),
            typeof(DateTimeOffset),
            typeof(TimelineScrubberControl),
            new PropertyMetadata(DateTimeOffset.Now, OnDateChanged));

    public float CurrentZ
    {
        get => (float)GetValue(CurrentZProperty);
        set => SetValue(CurrentZProperty, value);
    }

    public float MinZ
    {
        get => (float)GetValue(MinZProperty);
        set => SetValue(MinZProperty, value);
    }

    public float MaxZ
    {
        get => (float)GetValue(MaxZProperty);
        set => SetValue(MaxZProperty, value);
    }

    public DateTimeOffset EarliestDate
    {
        get => (DateTimeOffset)GetValue(EarliestDateProperty);
        set => SetValue(EarliestDateProperty, value);
    }

    public DateTimeOffset LatestDate
    {
        get => (DateTimeOffset)GetValue(LatestDateProperty);
        set => SetValue(LatestDateProperty, value);
    }

    public string CurrentDateText { get; private set; } = "";
    public string EarliestDateText { get; private set; } = "";
    public string LatestDateText { get; private set; } = "";
    public double ProgressWidth { get; private set; } = 0;

    private const double TotalBarWidth = 240.0;

    private static void OnCurrentZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineScrubberControl control)
        {
            control.UpdateCurrentDate();
            control.UpdateProgressWidth();
        }
    }

    private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineScrubberControl control)
        {
            control.UpdateDateLabels();
            control.UpdateCurrentDate();
            control.UpdateProgressWidth();
        }
    }

    private void UpdateCurrentDate()
    {
        var date = TimelineConfig.CalculateDateForZ(CurrentZ, EarliestDate);
        CurrentDateText = date.ToString("yyyy");
        Bindings.Update();
    }

    private void UpdateDateLabels()
    {
        EarliestDateText = EarliestDate.ToString("yyyy");
        LatestDateText = LatestDate.ToString("yyyy");
        Bindings.Update();
    }

    private void UpdateProgressWidth()
    {
        if (MaxZ > 0)
        {
            var progress = Math.Clamp(CurrentZ / MaxZ, 0.0, 1.0);
            ProgressWidth = progress * TotalBarWidth;
        }
        else
        {
            ProgressWidth = 0;
        }
        Bindings.Update();
    }

}
