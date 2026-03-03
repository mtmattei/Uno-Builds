using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using FieldOpsPro.Models.Enums;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class StatusBadge : UserControl
{
    public StatusBadge()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBadge),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty BadgeTypeProperty =
        DependencyProperty.Register(nameof(BadgeType), typeof(BadgeType), typeof(StatusBadge),
            new PropertyMetadata(BadgeType.Default, OnPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeType BadgeType
    {
        get => (BadgeType)GetValue(BadgeTypeProperty);
        set => SetValue(BadgeTypeProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            badge.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (BadgeBorder == null || BadgeText == null) return;

        BadgeText.Text = Text.ToUpperInvariant();

        var (background, foreground) = GetColors(BadgeType);
        BadgeBorder.Background = background;
        BadgeText.Foreground = foreground;
    }

    private static (SolidColorBrush background, SolidColorBrush foreground) GetColors(BadgeType type)
    {
        return type switch
        {
            BadgeType.Danger or BadgeType.Urgent => (
                new SolidColorBrush(ParseColor("#EF4444", 0.15)),
                new SolidColorBrush(ParseColor("#EF4444"))
            ),
            BadgeType.Warning or BadgeType.Pending => (
                new SolidColorBrush(ParseColor("#F59E0B", 0.15)),
                new SolidColorBrush(ParseColor("#F59E0B"))
            ),
            BadgeType.Success or BadgeType.Completed => (
                new SolidColorBrush(ParseColor("#22C55E", 0.15)),
                new SolidColorBrush(ParseColor("#22C55E"))
            ),
            BadgeType.Info or BadgeType.InProgress => (
                new SolidColorBrush(ParseColor("#4ECDC4", 0.15)),
                new SolidColorBrush(ParseColor("#4ECDC4"))
            ),
            BadgeType.Primary => (
                new SolidColorBrush(ParseColor("#FF6B35", 0.15)),
                new SolidColorBrush(ParseColor("#FF6B35"))
            ),
            _ => (
                new SolidColorBrush(ParseColor("#6B7280", 0.15)),
                new SolidColorBrush(ParseColor("#6B7280"))
            )
        };
    }

    private static Windows.UI.Color ParseColor(string hex, double opacity = 1.0)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex, opacity);
    }
}

public enum BadgeType
{
    Default,
    Primary,
    Success,
    Warning,
    Danger,
    Info,
    Urgent,
    Pending,
    Completed,
    InProgress
}
