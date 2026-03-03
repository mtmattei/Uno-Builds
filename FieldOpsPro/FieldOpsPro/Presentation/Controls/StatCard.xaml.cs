using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class StatCard : UserControl
{
    private static readonly Random _random = new();
    private static readonly SolidColorBrush HoverBorderBrush = new(ParseColor("#505050"));

    public StatCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
        SetupChart();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatCard),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty ChangePercentProperty =
        DependencyProperty.Register(nameof(ChangePercent), typeof(double?), typeof(StatCard),
            new PropertyMetadata(null, OnPropertyChanged));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(StatCard),
            new PropertyMetadata("\uE8A5", OnPropertyChanged));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(nameof(AccentColor), typeof(StatAccentColor), typeof(StatCard),
            new PropertyMetadata(StatAccentColor.Primary, OnPropertyChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(StatCard),
            new PropertyMetadata(null, OnPropertyChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double? ChangePercent
    {
        get => (double?)GetValue(ChangePercentProperty);
        set => SetValue(ChangePercentProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public StatAccentColor AccentColor
    {
        get => (StatAccentColor)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card)
        {
            card.UpdateAppearance();
            card.SetupChart();
        }
    }

    private void UpdateAppearance()
    {
        if (LabelText == null) return;

        LabelText.Text = Label;
        ValueText.Text = Value;
        StatIcon.Glyph = IconGlyph;

        var accentColor = GetAccentColor(AccentColor);
        var accentBrush = new SolidColorBrush(accentColor);

        // Icon container background (15% opacity)
        IconContainer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(
            (byte)(255 * 0.15), accentColor.R, accentColor.G, accentColor.B));
        StatIcon.Foreground = accentBrush;

        // Change indicator
        if (ChangePercent.HasValue && ChangePercent.Value != 0)
        {
            ChangePanel.Visibility = Visibility.Visible;
            TrendIcon.Visibility = Visibility.Visible;
            var isPositive = ChangePercent.Value > 0;

            // For response time (Tertiary), lower is better
            var isGood = AccentColor == StatAccentColor.Tertiary
                ? !isPositive
                : isPositive;

            var changeColor = isGood ? ParseColor("#E0E0E0") : ParseColor("#707070");
            var changeBrush = new SolidColorBrush(changeColor);

            TrendIcon.Glyph = isPositive ? "\uE74A" : "\uE74B";
            TrendIcon.Foreground = changeBrush;

            ChangeText.Text = $"{(isPositive ? "+" : "")}{ChangePercent.Value:F1}%";
            ChangeText.Foreground = changeBrush;
        }
        else if (!string.IsNullOrEmpty(Subtitle))
        {
            ChangePanel.Visibility = Visibility.Visible;
            TrendIcon.Visibility = Visibility.Collapsed;
            ChangeText.Text = Subtitle;
            ChangeText.Foreground = Application.Current.Resources.TryGetValue("TextMutedBrush", out var mutedBrush)
                ? mutedBrush as Brush
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128));
        }
        else
        {
            ChangePanel.Visibility = Visibility.Collapsed;
        }
    }

    private void SetupChart()
    {
        if (MiniChart == null) return;

        var accentColor = GetAccentColor(AccentColor);
        var skColor = new SKColor(accentColor.R, accentColor.G, accentColor.B);
        var skColorFaded = skColor.WithAlpha(60);

        // Generate trend data
        var data = GenerateTrendData();

        var lineSeries = new LineSeries<double>
        {
            Values = data,
            Fill = new LinearGradientPaint(
                new[] { skColorFaded, SKColors.Transparent },
                new SKPoint(0.5f, 0),
                new SKPoint(0.5f, 1)),
            Stroke = new SolidColorPaint(skColor, 2),
            GeometryFill = null,
            GeometryStroke = null,
            LineSmoothness = 0.65
        };

        MiniChart.Series = new ISeries[] { lineSeries };

        // Hide axes for minimal look
        MiniChart.XAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = false,
                ShowSeparatorLines = false
            }
        };

        MiniChart.YAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = false,
                ShowSeparatorLines = false
            }
        };

        // Remove legend and other UI elements
        MiniChart.DrawMarginFrame = null;
    }

    private double[] GenerateTrendData()
    {
        // Generate realistic trend data based on the stat type
        var baseValue = AccentColor switch
        {
            StatAccentColor.Primary => 45.0,    // Work orders
            StatAccentColor.Secondary => 12.0,  // Agents
            StatAccentColor.Tertiary => 25.0,   // Response time
            StatAccentColor.Success => 92.0,    // Completion rate
            _ => 50.0
        };

        var trend = ChangePercent.HasValue && ChangePercent.Value > 0 ? 0.02 : -0.01;
        var data = new double[12];

        for (int i = 0; i < 12; i++)
        {
            var variation = (_random.NextDouble() - 0.5) * 0.1;
            var trendFactor = 1 + (trend * i) + variation;
            data[i] = baseValue * trendFactor;
        }

        return data;
    }

    private static Windows.UI.Color GetAccentColor(StatAccentColor color)
    {
        // Monochromatic grey palette
        return color switch
        {
            StatAccentColor.Primary => ParseColor("#FFFFFF"),    // White
            StatAccentColor.Secondary => ParseColor("#C0C0C0"),  // Light Grey
            StatAccentColor.Tertiary => ParseColor("#909090"),   // Medium Grey
            StatAccentColor.Success => ParseColor("#E0E0E0"),    // Bright Grey
            StatAccentColor.Warning => ParseColor("#B0B0B0"),    // Mid Grey
            StatAccentColor.Danger => ParseColor("#808080"),     // Dark Grey
            _ => ParseColor("#FFFFFF")
        };
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Animate lift effect
        var liftAnimation = new DoubleAnimation
        {
            To = -4,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(liftAnimation, CardTransform);
        Storyboard.SetTargetProperty(liftAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(liftAnimation);
        storyboard.Begin();

        // Glow border effect
        CardBorder.BorderBrush = HoverBorderBrush;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Animate back down
        var dropAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(dropAnimation, CardTransform);
        Storyboard.SetTargetProperty(dropAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(dropAnimation);
        storyboard.Begin();

        // Reset border
        CardBorder.BorderBrush = Application.Current.Resources.TryGetValue("BorderColorBrush", out var borderBrush)
            ? borderBrush as SolidColorBrush
            : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}

public enum StatAccentColor
{
    Primary,
    Secondary,
    Tertiary,
    Success,
    Warning,
    Danger
}
