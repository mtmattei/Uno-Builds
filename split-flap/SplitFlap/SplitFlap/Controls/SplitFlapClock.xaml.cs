using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SplitFlap.Controls;

public sealed partial class SplitFlapClock : UserControl
{
    private DispatcherTimer? _timer;
    private DateTime _lastUpdate = DateTime.MinValue;

    public SplitFlapClock()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme();
        ApplySize();
        ApplyShowSeconds();

        // Initialize with current time
        UpdateTime();

        // Start timer aligned to next second boundary
        StartAlignedTimer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopTimer();
    }

    #region Dependency Properties

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(
            nameof(Theme),
            typeof(SplitFlapTheme),
            typeof(SplitFlapClock),
            new PropertyMetadata(SplitFlapTheme.Dark, OnThemeChanged));

    public SplitFlapTheme Theme
    {
        get => (SplitFlapTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapClock clock)
        {
            clock.ApplyTheme();
        }
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(SplitFlapSize),
            typeof(SplitFlapClock),
            new PropertyMetadata(SplitFlapSize.Medium, OnSizeChanged));

    public SplitFlapSize Size
    {
        get => (SplitFlapSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapClock clock)
        {
            clock.ApplySize();
        }
    }

    public static readonly DependencyProperty ShowSecondsProperty =
        DependencyProperty.Register(
            nameof(ShowSeconds),
            typeof(bool),
            typeof(SplitFlapClock),
            new PropertyMetadata(true, OnShowSecondsChanged));

    public bool ShowSeconds
    {
        get => (bool)GetValue(ShowSecondsProperty);
        set => SetValue(ShowSecondsProperty, value);
    }

    private static void OnShowSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapClock clock)
        {
            clock.ApplyShowSeconds();
        }
    }

    public static readonly DependencyProperty Use24HourProperty =
        DependencyProperty.Register(
            nameof(Use24Hour),
            typeof(bool),
            typeof(SplitFlapClock),
            new PropertyMetadata(true, OnUse24HourChanged));

    public bool Use24Hour
    {
        get => (bool)GetValue(Use24HourProperty);
        set => SetValue(Use24HourProperty, value);
    }

    private static void OnUse24HourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitFlapClock clock)
        {
            clock.UpdateTime();
        }
    }

    #endregion

    private void StartAlignedTimer()
    {
        // Calculate milliseconds until next second boundary
        var now = DateTime.Now;
        var msUntilNextSecond = 1000 - now.Millisecond;

        // Use a one-shot timer to align to second boundary
        var alignmentTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(msUntilNextSecond)
        };

        alignmentTimer.Tick += (s, e) =>
        {
            alignmentTimer.Stop();
            UpdateTime();

            // Now start the regular 1-second interval timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        };

        alignmentTimer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private void OnTimerTick(object? sender, object e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        var now = DateTime.Now;

        // Only update if second has changed
        if (_lastUpdate.Second == now.Second && _lastUpdate.Minute == now.Minute && _lastUpdate.Hour == now.Hour)
        {
            return;
        }

        _lastUpdate = now;

        var hour = Use24Hour ? now.Hour : (now.Hour % 12 == 0 ? 12 : now.Hour % 12);

        HoursDisplay.Value = hour.ToString("D2");
        MinutesDisplay.Value = now.Minute.ToString("D2");
        SecondsDisplay.Value = now.Second.ToString("D2");
    }

    private void ApplyTheme()
    {
        HoursDisplay.Theme = Theme;
        MinutesDisplay.Theme = Theme;
        SecondsDisplay.Theme = Theme;

        // Apply colon color based on theme
        var colonColor = GetColonColor(Theme);
        ColonSeparator1.Foreground = new SolidColorBrush(colonColor);
        ColonSeparator2.Foreground = new SolidColorBrush(colonColor);
    }

    private void ApplySize()
    {
        HoursDisplay.Size = Size;
        MinutesDisplay.Size = Size;
        SecondsDisplay.Size = Size;

        // Adjust colon font size based on display size
        var colonFontSize = Size switch
        {
            SplitFlapSize.Small => 24,
            SplitFlapSize.Large => 56,
            _ => 40
        };

        ColonSeparator1.FontSize = colonFontSize;
        ColonSeparator2.FontSize = colonFontSize;
    }

    private void ApplyShowSeconds()
    {
        var visibility = ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
        ColonSeparator2.Visibility = visibility;
        SecondsDisplay.Visibility = visibility;
    }

    private static Color GetColonColor(SplitFlapTheme theme)
    {
        return theme switch
        {
            SplitFlapTheme.Light => Color.FromArgb(255, 28, 25, 23),    // stone-800
            SplitFlapTheme.Vintage => Color.FromArgb(255, 69, 26, 3),  // amber-950
            _ => Color.FromArgb(255, 254, 243, 199)                      // amber-100
        };
    }
}
