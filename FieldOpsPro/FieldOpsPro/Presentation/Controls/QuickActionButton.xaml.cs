using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class QuickActionButton : UserControl
{
    public QuickActionButton()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(QuickActionButton),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(QuickActionButton),
            new PropertyMetadata("\uE73E", OnPropertyChanged));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(nameof(AccentColor), typeof(QuickActionAccent), typeof(QuickActionButton),
            new PropertyMetadata(QuickActionAccent.Primary, OnPropertyChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public QuickActionAccent AccentColor
    {
        get => (QuickActionAccent)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public event EventHandler? ActionClicked;

    // Cached gradient brushes to avoid allocation on every pointer event
    private static readonly LinearGradientBrush HoverGradient = new()
    {
        StartPoint = new Windows.Foundation.Point(0, 0),
        EndPoint = new Windows.Foundation.Point(0, 1),
        GradientStops = new GradientStopCollection
        {
            new GradientStop { Color = ParseColor("#252525"), Offset = 0 },
            new GradientStop { Color = ParseColor("#1A1A1A"), Offset = 1 }
        }
    };

    private static readonly LinearGradientBrush DefaultGradient = new()
    {
        StartPoint = new Windows.Foundation.Point(0, 0),
        EndPoint = new Windows.Foundation.Point(0, 1),
        GradientStops = new GradientStopCollection
        {
            new GradientStop { Color = ParseColor("#1A1A1A"), Offset = 0 },
            new GradientStop { Color = ParseColor("#0D0D0D"), Offset = 1 }
        }
    };

    private static readonly LinearGradientBrush PressedGradient = new()
    {
        StartPoint = new Windows.Foundation.Point(0, 0),
        EndPoint = new Windows.Foundation.Point(0, 1),
        GradientStops = new GradientStopCollection
        {
            new GradientStop { Color = ParseColor("#303030"), Offset = 0 },
            new GradientStop { Color = ParseColor("#252525"), Offset = 1 }
        }
    };

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuickActionButton button)
        {
            button.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (ActionLabel == null) return;

        ActionLabel.Text = Label;
        ActionIcon.Glyph = IconGlyph;

        var accentColor = GetAccentColor(AccentColor);
        var accentBrush = new SolidColorBrush(accentColor);

        // Icon uses monochromatic accent color (background is set in XAML with gradient)
        ActionIcon.Foreground = accentBrush;
    }

    private static Windows.UI.Color GetAccentColor(QuickActionAccent accent)
    {
        // Monochromatic grey palette to match desktop style
        return accent switch
        {
            QuickActionAccent.Primary => ParseColor("#FFFFFF"),    // White
            QuickActionAccent.Secondary => ParseColor("#C0C0C0"),  // Light Grey
            QuickActionAccent.Tertiary => ParseColor("#909090"),   // Medium Grey
            QuickActionAccent.Success => ParseColor("#E0E0E0"),    // Bright Grey
            _ => ParseColor("#FFFFFF")
        };
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Scale up animation
        AnimateScale(1.03);

        // Brighter gradient on hover
        ActionBorder.Background = HoverGradient;

        // Brighten icon
        var accentColor = GetAccentColor(AccentColor);
        ActionIcon.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(
            255,
            (byte)Math.Min(255, accentColor.R + 30),
            (byte)Math.Min(255, accentColor.G + 30),
            (byte)Math.Min(255, accentColor.B + 30)));
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Scale back to normal
        AnimateScale(1.0);

        // Reset gradient
        ActionBorder.Background = DefaultGradient;

        // Reset icon color
        UpdateAppearance();
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Scale down for press feedback
        AnimateScale(0.96);

        // Pressed state - even lighter
        ActionBorder.Background = PressedGradient;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        // Scale back to hover state
        AnimateScale(1.03);

        // Back to hover gradient
        ActionBorder.Background = HoverGradient;

        ActionClicked?.Invoke(this, EventArgs.Empty);
    }

    private void AnimateScale(double targetScale)
    {
        var scaleXAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleXAnimation, BorderTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");

        var scaleYAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnimation, BorderTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);
        storyboard.Begin();
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }
}

public enum QuickActionAccent
{
    Primary,
    Secondary,
    Tertiary,
    Success
}
