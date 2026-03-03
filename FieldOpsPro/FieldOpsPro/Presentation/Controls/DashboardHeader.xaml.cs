using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class DashboardHeader : UserControl
{
    private DispatcherTimer? _clockTimer;
    private DispatcherTimer? _pulseTimer;
    private DispatcherTimer? _alertPulseTimer;
    private bool _isConnected = true;
    private bool _pulseDirection = true; // true = fading out, false = fading in
    private bool _alertPulseDirection = true;
    private double _currentPulseOpacity = 1.0;
    private double _currentAlertOpacity = 0.2;

    // Cached brushes to avoid repeated allocation
    private static readonly SolidColorBrush AlertBorderBrush = new(Windows.UI.Color.FromArgb(255, 255, 205, 205));
    private static readonly SolidColorBrush DefaultBorderBrush = new(Windows.UI.Color.FromArgb(255, 42, 42, 42));
    private static readonly SolidColorBrush WhiteBrush = new(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    private static readonly SolidColorBrush GreyBrush = new(Windows.UI.Color.FromArgb(255, 128, 128, 128));

    public DashboardHeader()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private Storyboard? _hoverInStoryboard;
    private Storyboard? _hoverOutStoryboard;
    private Storyboard? _popInStoryboard;
    private DispatcherTimer? _alertShowTimer;

    #region Dependency Properties

    public static readonly DependencyProperty TitleAreaProperty =
        DependencyProperty.Register(nameof(TitleArea), typeof(object), typeof(DashboardHeader),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AlertTitleProperty =
        DependencyProperty.Register(nameof(AlertTitle), typeof(string), typeof(DashboardHeader),
            new PropertyMetadata("URGENT: Power Outage Reported", OnAlertPropertyChanged));

    public static readonly DependencyProperty AlertMessageProperty =
        DependencyProperty.Register(nameof(AlertMessage), typeof(string), typeof(DashboardHeader),
            new PropertyMetadata("3 work orders affected in Downtown District", OnAlertPropertyChanged));

    public static readonly DependencyProperty ShowAlertProperty =
        DependencyProperty.Register(nameof(ShowAlert), typeof(bool), typeof(DashboardHeader),
            new PropertyMetadata(true, OnShowAlertChanged));

    public static readonly DependencyProperty TemperatureProperty =
        DependencyProperty.Register(nameof(Temperature), typeof(int), typeof(DashboardHeader),
            new PropertyMetadata(72, OnWeatherPropertyChanged));

    public static readonly DependencyProperty WeatherConditionProperty =
        DependencyProperty.Register(nameof(WeatherCondition), typeof(string), typeof(DashboardHeader),
            new PropertyMetadata("Partly Cloudy", OnWeatherPropertyChanged));

    public static readonly DependencyProperty IsConnectedProperty =
        DependencyProperty.Register(nameof(IsConnected), typeof(bool), typeof(DashboardHeader),
            new PropertyMetadata(true, OnConnectionStatusChanged));

    public object TitleArea
    {
        get => GetValue(TitleAreaProperty);
        set => SetValue(TitleAreaProperty, value);
    }

    public string AlertTitle
    {
        get => (string)GetValue(AlertTitleProperty);
        set => SetValue(AlertTitleProperty, value);
    }

    public string AlertMessage
    {
        get => (string)GetValue(AlertMessageProperty);
        set => SetValue(AlertMessageProperty, value);
    }

    public bool ShowAlert
    {
        get => (bool)GetValue(ShowAlertProperty);
        set => SetValue(ShowAlertProperty, value);
    }

    public int Temperature
    {
        get => (int)GetValue(TemperatureProperty);
        set => SetValue(TemperatureProperty, value);
    }

    public string WeatherCondition
    {
        get => (string)GetValue(WeatherConditionProperty);
        set => SetValue(WeatherConditionProperty, value);
    }

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start the clock timer
        StartClockTimer();

        // Start animations
        StartAnimations();

        // Start delayed alert show (demo): pop-in after a few seconds
        StartAlertShowDelay();

        // Update initial values
        UpdateTimeDisplay();
        UpdateWeatherDisplay();
        UpdateAlertDisplay();
        UpdateConnectionStatus();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Stop and cleanup the timer
        StopClockTimer();
        StopAnimations();
    }

    private void OnDismissAlertClick(object sender, RoutedEventArgs e)
    {
        ShowAlert = false;
    }

    private void OnAlertBannerPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_hoverOutStoryboard != null)
        {
            _hoverOutStoryboard.Stop();
        }

        if (_hoverInStoryboard == null)
            CreateHoverStoryboards();

        _hoverInStoryboard?.Begin();
    }

    private void OnAlertBannerPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_hoverInStoryboard != null)
        {
            _hoverInStoryboard.Stop();
        }

        if (_hoverOutStoryboard == null)
            CreateHoverStoryboards();

        _hoverOutStoryboard?.Begin();
    }

    private void CreateHoverStoryboards()
    {
        if (AlertBanner == null) return;

        // Hover in: scale to 1.02 with easing for smooth transition
        var scaleXIn = new DoubleAnimation
        {
            To = 1.02,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
            EnableDependentAnimation = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var scaleYIn = new DoubleAnimation
        {
            To = 1.02,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
            EnableDependentAnimation = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        _hoverInStoryboard = new Storyboard();
        Storyboard.SetTarget(scaleXIn, AlertBanner);
        Storyboard.SetTargetProperty(scaleXIn, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYIn, AlertBanner);
        Storyboard.SetTargetProperty(scaleYIn, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        _hoverInStoryboard.Children.Add(scaleXIn);
        _hoverInStoryboard.Children.Add(scaleYIn);

        // Hover out: scale back to 1.0
        var scaleXOut = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(160)),
            EnableDependentAnimation = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        var scaleYOut = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(160)),
            EnableDependentAnimation = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        _hoverOutStoryboard = new Storyboard();
        Storyboard.SetTarget(scaleXOut, AlertBanner);
        Storyboard.SetTargetProperty(scaleXOut, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleYOut, AlertBanner);
        Storyboard.SetTargetProperty(scaleYOut, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        _hoverOutStoryboard.Children.Add(scaleXOut);
        _hoverOutStoryboard.Children.Add(scaleYOut);
    }

    #endregion

    #region Timer Management

    private void StartClockTimer()
    {
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += OnClockTick;
        _clockTimer.Start();
    }

    private void StopClockTimer()
    {
        if (_clockTimer != null)
        {
            _clockTimer.Stop();
            _clockTimer.Tick -= OnClockTick;
            _clockTimer = null;
        }
    }

    private void OnClockTick(object? sender, object e)
    {
        UpdateTimeDisplay();
    }

    #endregion

    #region Animation Management

    private void StartAnimations()
    {
        // Start pulse animation for live status indicator using timer
        StartPulseAnimation();

        // Start alert pulse if alert is visible
        if (ShowAlert)
        {
            StartAlertPulseAnimation();
        }
    }

    private void StopAnimations()
    {
        StopPulseAnimation();
        StopAlertPulseAnimation();
    }

    private void StartPulseAnimation()
    {
        if (_pulseTimer != null) return;

        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for smooth animation
        };
        _pulseTimer.Tick += OnPulseTick;
        _pulseTimer.Start();
    }

    private void StopPulseAnimation()
    {
        if (_pulseTimer != null)
        {
            _pulseTimer.Stop();
            _pulseTimer.Tick -= OnPulseTick;
            _pulseTimer = null;
        }
    }

    private void OnPulseTick(object? sender, object e)
    {
        if (LiveStatusDot == null) return;

        // Animate opacity between 0.3 and 1.0 over ~1 second (20 steps each way)
        const double step = 0.035;
        const double minOpacity = 0.3;
        const double maxOpacity = 1.0;

        if (_pulseDirection)
        {
            _currentPulseOpacity -= step;
            if (_currentPulseOpacity <= minOpacity)
            {
                _currentPulseOpacity = minOpacity;
                _pulseDirection = false;
            }
        }
        else
        {
            _currentPulseOpacity += step;
            if (_currentPulseOpacity >= maxOpacity)
            {
                _currentPulseOpacity = maxOpacity;
                _pulseDirection = true;
            }
        }

        LiveStatusDot.Opacity = _currentPulseOpacity;
    }

    private void StartAlertPulseAnimation()
    {
        if (_alertPulseTimer != null) return;

        _alertPulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _alertPulseTimer.Tick += OnAlertPulseTick;
        _alertPulseTimer.Start();
    }

    private void StopAlertPulseAnimation()
    {
        if (_alertPulseTimer != null)
        {
            _alertPulseTimer.Stop();
            _alertPulseTimer.Tick -= OnAlertPulseTick;
            _alertPulseTimer = null;
        }
    }

    // Border pulse removed: border is static pastel red when alert is visible.

    private void OnAlertPulseTick(object? sender, object e)
    {
        if (AlertIconGlow == null) return;

        // Animate opacity between 0.2 and 0.6 over ~0.8 seconds
        const double step = 0.025;
        const double minOpacity = 0.2;
        const double maxOpacity = 0.6;

        if (_alertPulseDirection)
        {
            _currentAlertOpacity += step;
            if (_currentAlertOpacity >= maxOpacity)
            {
                _currentAlertOpacity = maxOpacity;
                _alertPulseDirection = false;
            }
        }
        else
        {
            _currentAlertOpacity -= step;
            if (_currentAlertOpacity <= minOpacity)
            {
                _currentAlertOpacity = minOpacity;
                _alertPulseDirection = true;
            }
        }

        AlertIconGlow.Opacity = _currentAlertOpacity;
    }

    #endregion

    #region Display Updates

    private void UpdateTimeDisplay()
    {
        if (TimeText == null || TimezoneText == null) return;

        var now = DateTime.Now;
        TimeText.Text = now.ToString("HH:mm:ss");

        // Get timezone abbreviation
        var timezone = TimeZoneInfo.Local;
        var isDaylightSaving = timezone.IsDaylightSavingTime(now);
        var abbreviation = GetTimezoneAbbreviation(timezone, isDaylightSaving);
        TimezoneText.Text = abbreviation;
    }

    private static string GetTimezoneAbbreviation(TimeZoneInfo timezone, bool isDaylightSaving)
    {
        // Common timezone abbreviations
        var standardName = timezone.StandardName;
        var daylightName = timezone.DaylightName;
        var name = isDaylightSaving ? daylightName : standardName;

        // Try to extract abbreviation from timezone name
        if (name.Contains("Eastern")) return isDaylightSaving ? "EDT" : "EST";
        if (name.Contains("Central")) return isDaylightSaving ? "CDT" : "CST";
        if (name.Contains("Mountain")) return isDaylightSaving ? "MDT" : "MST";
        if (name.Contains("Pacific")) return isDaylightSaving ? "PDT" : "PST";
        if (name.Contains("UTC") || name.Contains("Coordinated")) return "UTC";
        if (name.Contains("GMT") || name.Contains("Greenwich")) return "GMT";

        // Fallback: create abbreviation from first letters
        var words = name.Split(' ');
        if (words.Length >= 2)
        {
            return string.Concat(words.Take(3).Select(w => w.Length > 0 ? w[0].ToString() : ""));
        }

        return timezone.BaseUtcOffset.Hours >= 0
            ? $"UTC+{timezone.BaseUtcOffset.Hours}"
            : $"UTC{timezone.BaseUtcOffset.Hours}";
    }

    private void UpdateWeatherDisplay()
    {
        if (TemperatureText == null || WeatherConditionText == null || WeatherIcon == null) return;

        TemperatureText.Text = Temperature.ToString();
        WeatherConditionText.Text = WeatherCondition;

        // Update weather icon based on condition
        WeatherIcon.Glyph = GetWeatherGlyph(WeatherCondition);
    }

    private static string GetWeatherGlyph(string condition)
    {
        var lowerCondition = condition.ToLowerInvariant();

        if (lowerCondition.Contains("sun") || lowerCondition.Contains("clear"))
            return "\uE706"; // Sunny
        if (lowerCondition.Contains("cloud") && lowerCondition.Contains("part"))
            return "\uE9BD"; // Partly cloudy
        if (lowerCondition.Contains("cloud") || lowerCondition.Contains("overcast"))
            return "\uE753"; // Cloudy
        if (lowerCondition.Contains("rain") || lowerCondition.Contains("shower"))
            return "\uE9C4"; // Rain
        if (lowerCondition.Contains("storm") || lowerCondition.Contains("thunder"))
            return "\uE9C6"; // Thunderstorm
        if (lowerCondition.Contains("snow"))
            return "\uE9C8"; // Snow
        if (lowerCondition.Contains("fog") || lowerCondition.Contains("mist"))
            return "\uE9CA"; // Fog

        return "\uE9BD"; // Default: partly cloudy
    }

    private void UpdateAlertDisplay()
    {
        if (AlertBanner == null || AlertTitleText == null || AlertMessageText == null) return;
        // Update text regardless
        AlertTitleText.Text = AlertTitle;
        AlertMessageText.Text = AlertMessage;

        if (ShowAlert)
        {
            // For demo: hide initially and show with pop animation after delay
            AlertBanner.Visibility = Visibility.Collapsed;
            // Apply pastel red border immediately so it appears with pop
            AlertBanner.BorderBrush = AlertBorderBrush;
            AlertBanner.BorderThickness = new Thickness(1.5);

            StartAlertShowDelay();
        }
        else
        {
            // Cancel any pending show and hide immediately
            StopAlertShowDelay();

            // Reset to default border when dismissed
            AlertBanner.BorderBrush = DefaultBorderBrush;
            AlertBanner.BorderThickness = new Thickness(1);

            AlertBanner.Visibility = Visibility.Collapsed;
            StopAlertPulseAnimation();
        }
    }

    private void StartAlertShowDelay()
    {
        if (AlertBanner == null) return;

        // If already visible, ensure pulse animation is running
        if (AlertBanner.Visibility == Visibility.Visible)
        {
            StartAlertPulseAnimation();
            return;
        }

        // Cancel previous timer
        StopAlertShowDelay();

        // Prepare initial state for pop-in
        AlertBanner.Opacity = 0;
        if (AlertBanner.RenderTransform is Microsoft.UI.Xaml.Media.ScaleTransform st)
        {
            st.ScaleX = 0.92;
            st.ScaleY = 0.92;
        }

        // Fixed 15 second delay for recording/demo
        _alertShowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(15)
        };
        _alertShowTimer.Tick += OnAlertShowTimerTick;
        _alertShowTimer.Start();
    }

    private void StopAlertShowDelay()
    {
        if (_alertShowTimer != null)
        {
            _alertShowTimer.Stop();
            _alertShowTimer.Tick -= OnAlertShowTimerTick;
            _alertShowTimer = null;
        }
    }

    private void OnAlertShowTimerTick(object? sender, object? e)
    {
        StopAlertShowDelay();
        if (AlertBanner == null) return;

        AlertBanner.Visibility = Visibility.Visible;

        if (_popInStoryboard == null)
            CreatePopInStoryboard();

        _popInStoryboard?.Begin();

        // Start pulsing glow after it appears
        StartAlertPulseAnimation();
    }

    private void CreatePopInStoryboard()
    {
        if (AlertBanner == null) return;

        var scaleX = new DoubleAnimation
        {
            From = 0.92,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(220)),
            EnableDependentAnimation = true
        };
        var scaleY = new DoubleAnimation
        {
            From = 0.92,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(220)),
            EnableDependentAnimation = true
        };
        var fade = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(220)),
            EnableDependentAnimation = true
        };

        _popInStoryboard = new Storyboard();
        Storyboard.SetTarget(scaleX, AlertBanner);
        Storyboard.SetTargetProperty(scaleX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTarget(scaleY, AlertBanner);
        Storyboard.SetTargetProperty(scaleY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        Storyboard.SetTarget(fade, AlertBanner);
        Storyboard.SetTargetProperty(fade, "Opacity");

        _popInStoryboard.Children.Add(scaleX);
        _popInStoryboard.Children.Add(scaleY);
        _popInStoryboard.Children.Add(fade);
    }

    private void UpdateConnectionStatus()
    {
        if (LiveStatusDot == null || LiveStatusText == null) return;

        _isConnected = IsConnected;

        if (_isConnected)
        {
            LiveStatusDot.Fill = WhiteBrush;
            LiveStatusText.Text = "LIVE";

            // Start pulse animation
            StartPulseAnimation();
        }
        else
        {
            LiveStatusDot.Fill = GreyBrush;
            LiveStatusText.Text = "OFFLINE";

            // Stop pulse animation
            StopPulseAnimation();
            LiveStatusDot.Opacity = 1.0; // Reset opacity when offline
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnAlertPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DashboardHeader header)
        {
            header.UpdateAlertDisplay();
        }
    }

    private static void OnShowAlertChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DashboardHeader header)
        {
            header.UpdateAlertDisplay();
        }
    }

    private static void OnWeatherPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DashboardHeader header)
        {
            header.UpdateWeatherDisplay();
        }
    }

    private static void OnConnectionStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DashboardHeader header)
        {
            header.UpdateConnectionStatus();
        }
    }

    #endregion
}
