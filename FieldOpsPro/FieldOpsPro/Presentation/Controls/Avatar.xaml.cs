using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using FieldOpsPro.Models.Enums;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class Avatar : UserControl
{
    public Avatar()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAvatarAppearance();
        UpdateStatusIndicator();
    }

    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(nameof(Initials), typeof(string), typeof(Avatar),
            new PropertyMetadata("", OnAppearancePropertyChanged));

    public static readonly DependencyProperty AvatarColorProperty =
        DependencyProperty.Register(nameof(AvatarColor), typeof(AvatarColor), typeof(Avatar),
            new PropertyMetadata(AvatarColor.Blue, OnAppearancePropertyChanged));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(AgentStatus?), typeof(Avatar),
            new PropertyMetadata(null, OnStatusPropertyChanged));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(Avatar),
            new PropertyMetadata(48.0, OnSizePropertyChanged));

    public static readonly DependencyProperty ShowStatusProperty =
        DependencyProperty.Register(nameof(ShowStatus), typeof(Visibility), typeof(Avatar),
            new PropertyMetadata(Visibility.Collapsed));

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public AvatarColor AvatarColor
    {
        get => (AvatarColor)GetValue(AvatarColorProperty);
        set => SetValue(AvatarColorProperty, value);
    }

    public AgentStatus? Status
    {
        get => (AgentStatus?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Visibility ShowStatus
    {
        get => (Visibility)GetValue(ShowStatusProperty);
        set => SetValue(ShowStatusProperty, value);
    }

    public new CornerRadius CornerRadius => new CornerRadius(Size <= 40 ? 10 : Size <= 48 ? 12 : 16);

    private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Avatar avatar)
        {
            avatar.UpdateAvatarAppearance();
        }
    }

    private static void OnStatusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Avatar avatar)
        {
            avatar.UpdateStatusIndicator();
        }
    }

    private static void OnSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Avatar avatar)
        {
            avatar.UpdateFontSize();
        }
    }

    private void UpdateAvatarAppearance()
    {
        if (AvatarBorder == null) return;

        AvatarBorder.Background = GetGradientBrush(AvatarColor);
        UpdateFontSize();
    }

    private void UpdateFontSize()
    {
        if (InitialsText == null) return;

        InitialsText.FontSize = Size switch
        {
            <= 36 => 12,
            <= 44 => 14,
            <= 48 => 16,
            _ => 18
        };
    }

    private void UpdateStatusIndicator()
    {
        if (StatusIndicator == null) return;

        if (Status.HasValue)
        {
            ShowStatus = Visibility.Visible;
            StatusIndicator.Fill = GetStatusBrush(Status.Value);
            StatusIndicator.Stroke = Application.Current.Resources.TryGetValue("BgTertiaryBrush", out var bgBrush)
                ? bgBrush as Brush
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));
        }
        else
        {
            ShowStatus = Visibility.Collapsed;
        }
    }

    private static LinearGradientBrush GetGradientBrush(AvatarColor color)
    {
        // Monochromatic black gradients with subtle depth variations
        return color switch
        {
            AvatarColor.Orange => CreateGradient("#2A2A2A", "#1A1A1A"),
            AvatarColor.Cyan => CreateGradient("#303030", "#202020"),
            AvatarColor.Purple => CreateGradient("#282828", "#181818"),
            AvatarColor.Pink => CreateGradient("#353535", "#252525"),
            AvatarColor.Blue => CreateGradient("#2D2D2D", "#1D1D1D"),
            AvatarColor.Green => CreateGradient("#323232", "#222222"),
            _ => CreateGradient("#2A2A2A", "#1A1A1A")
        };
    }

    private static LinearGradientBrush CreateGradient(string startColor, string endColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = ParseColor(startColor), Offset = 0 },
                new GradientStop { Color = ParseColor(endColor), Offset = 1 }
            }
        };
    }

    private static SolidColorBrush GetStatusBrush(AgentStatus status)
    {
        // Monochromatic status indicators
        var color = status switch
        {
            AgentStatus.OnSite or AgentStatus.Available => "#FFFFFF",  // White - active
            AgentStatus.OnRoute => "#C0C0C0",  // Light grey - in transit
            AgentStatus.Break => "#808080",    // Medium grey - paused
            AgentStatus.Offline => "#404040",  // Dark grey - offline
            _ => "#404040"
        };

        return new SolidColorBrush(ParseColor(color));
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }
}
