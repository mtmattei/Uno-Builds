using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SplitFlap.Controls;

public sealed partial class SplitFlapCountdown : UserControl
{
    private DispatcherTimer? _timer;
    private bool _hasCompleted = false;

    public SplitFlapCountdown()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme();
        ApplySize();
        ApplyShowDays();
        ApplyShowLabels();

        UpdateCountdown();
        StartTimer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopTimer();
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetDateProperty =
        DependencyProperty.Register(
            nameof(TargetDate),
            typeof(DateTimeOffset),
            typeof(SplitFlapCountdown),
            new PropertyMetadata(DateTimeOffset.Now.AddDays(1), OnTargetDateChanged));

    public DateTimeOffset TargetDate
    {
        get => (DateTimeOffset)GetValue(TargetDateProperty);
        set => SetValue(TargetDateProperty, value);
    }

    private static void OnTargetDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCountdown countdown)
        {
            countdown._hasCompleted = false;
            countdown.UpdateCountdown();
        }
    }

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(
            nameof(Theme),
            typeof(SplitFlapTheme),
            typeof(SplitFlapCountdown),
            new PropertyMetadata(SplitFlapTheme.Dark, OnThemeChanged));

    public SplitFlapTheme Theme
    {
        get => (SplitFlapTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCountdown countdown)
        {
            countdown.ApplyTheme();
        }
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(SplitFlapSize),
            typeof(SplitFlapCountdown),
            new PropertyMetadata(SplitFlapSize.Medium, OnSizeChanged));

    public SplitFlapSize Size
    {
        get => (SplitFlapSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCountdown countdown)
        {
            countdown.ApplySize();
        }
    }

    public static readonly DependencyProperty ShowDaysProperty =
        DependencyProperty.Register(
            nameof(ShowDays),
            typeof(bool),
            typeof(SplitFlapCountdown),
            new PropertyMetadata(true, OnShowDaysChanged));

    public bool ShowDays
    {
        get => (bool)GetValue(ShowDaysProperty);
        set => SetValue(ShowDaysProperty, value);
    }

    private static void OnShowDaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCountdown countdown)
        {
            countdown.ApplyShowDays();
        }
    }

    public static readonly DependencyProperty ShowLabelsProperty =
        DependencyProperty.Register(
            nameof(ShowLabels),
            typeof(bool),
            typeof(SplitFlapCountdown),
            new PropertyMetadata(true, OnShowLabelsChanged));

    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    private static void OnShowLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapCountdown countdown)
        {
            countdown.ApplyShowLabels();
        }
    }

    #endregion

    #region Events

    public event EventHandler? CountdownComplete;

    #endregion

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void OnTimerTick(object? sender, object e)
    {
        UpdateCountdown();
    }

    private void UpdateCountdown()
    {
        var remaining = TargetDate - DateTimeOffset.Now;

        if (remaining <= TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;

            if (!_hasCompleted)
            {
                _hasCompleted = true;
                CountdownComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        var days = (int)remaining.TotalDays;
        var hours = remaining.Hours;
        var minutes = remaining.Minutes;
        var seconds = remaining.Seconds;

        DaysDisplay.Value = days.ToString("D3");
        HoursDisplay.Value = hours.ToString("D2");
        MinutesDisplay.Value = minutes.ToString("D2");
        SecondsDisplay.Value = seconds.ToString("D2");
    }

    private void ApplyTheme()
    {
        DaysDisplay.Theme = Theme;
        HoursDisplay.Theme = Theme;
        MinutesDisplay.Theme = Theme;
        SecondsDisplay.Theme = Theme;

        // Apply label color based on theme
        var labelColor = GetLabelColor(Theme);
        var brush = new SolidColorBrush(labelColor);

        DaysLabel.Foreground = brush;
        HoursLabel.Foreground = brush;
        MinutesLabel.Foreground = brush;
        SecondsLabel.Foreground = brush;
    }

    private void ApplySize()
    {
        DaysDisplay.Size = Size;
        HoursDisplay.Size = Size;
        MinutesDisplay.Size = Size;
        SecondsDisplay.Size = Size;

        // Adjust label font size based on display size
        var labelFontSize = Size switch
        {
            SplitFlapSize.Small => 10,
            SplitFlapSize.Large => 16,
            _ => 12
        };

        DaysLabel.FontSize = labelFontSize;
        HoursLabel.FontSize = labelFontSize;
        MinutesLabel.FontSize = labelFontSize;
        SecondsLabel.FontSize = labelFontSize;
    }

    private void ApplyShowDays()
    {
        DaysPanel.Visibility = ShowDays ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyShowLabels()
    {
        var visibility = ShowLabels ? Visibility.Visible : Visibility.Collapsed;

        DaysLabel.Visibility = visibility;
        HoursLabel.Visibility = visibility;
        MinutesLabel.Visibility = visibility;
        SecondsLabel.Visibility = visibility;
    }

    private static Color GetLabelColor(SplitFlapTheme theme)
    {
        return theme switch
        {
            SplitFlapTheme.Light => Color.FromArgb(255, 28, 25, 23),    // stone-800
            SplitFlapTheme.Vintage => Color.FromArgb(255, 69, 26, 3),  // amber-950
            _ => Color.FromArgb(255, 254, 243, 199)                      // amber-100
        };
    }
}
