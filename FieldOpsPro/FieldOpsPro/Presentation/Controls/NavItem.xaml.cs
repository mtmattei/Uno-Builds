using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class NavItem : UserControl
{
    public NavItem()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(NavItem),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(NavItem),
            new PropertyMetadata("\uE80F", OnPropertyChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(NavItem),
            new PropertyMetadata(false, OnPropertyChanged));

    public static readonly DependencyProperty BadgeCountProperty =
        DependencyProperty.Register(nameof(BadgeCount), typeof(int?), typeof(NavItem),
            new PropertyMetadata(null, OnPropertyChanged));

    public static readonly DependencyProperty RouteProperty =
        DependencyProperty.Register(nameof(Route), typeof(string), typeof(NavItem),
            new PropertyMetadata(""));

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

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public int? BadgeCount
    {
        get => (int?)GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }

    public string Route
    {
        get => (string)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    public event EventHandler<string>? NavigationRequested;

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavItem item)
        {
            item.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (NavLabel == null) return;

        NavLabel.Text = Label;
        NavIcon.Glyph = IconGlyph;

        // Active state
        if (IsActive)
        {
            ActiveIndicator.Visibility = Visibility.Visible;
            ItemBorder.Background = Application.Current.Resources.TryGetValue("BgTertiaryBrush", out var bgBrush) ? bgBrush as Brush : null;
            NavIcon.Opacity = 1.0;
            NavIcon.Foreground = Application.Current.Resources.TryGetValue("AccentPrimaryBrush", out var accentBrush) ? accentBrush as Brush : null;
            NavLabel.Foreground = Application.Current.Resources.TryGetValue("TextPrimaryBrush", out var textBrush) ? textBrush as Brush : null;
        }
        else
        {
            ActiveIndicator.Visibility = Visibility.Collapsed;
            ItemBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            NavIcon.Opacity = 0.7;
            NavIcon.Foreground = Application.Current.Resources.TryGetValue("TextSecondaryBrush", out var secBrush1) ? secBrush1 as Brush : null;
            NavLabel.Foreground = Application.Current.Resources.TryGetValue("TextSecondaryBrush", out var secBrush2) ? secBrush2 as Brush : null;
        }

        // Badge
        if (BadgeCount.HasValue && BadgeCount.Value > 0)
        {
            BadgeBorder.Visibility = Visibility.Visible;
            BadgeText.Text = BadgeCount.Value.ToString();
        }
        else
        {
            BadgeBorder.Visibility = Visibility.Collapsed;
        }
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!IsActive)
        {
            ItemBorder.Background = Application.Current.Resources["BgTertiaryBrush"] as Brush;
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!IsActive)
        {
            ItemBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        NavigationRequested?.Invoke(this, Route);
    }
}
