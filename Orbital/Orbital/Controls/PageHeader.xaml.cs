using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class PageHeader : UserControl
{
    public static event EventHandler? SearchRequested;

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeader),
            new PropertyMetadata("", OnTitleChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PageHeader),
            new PropertyMetadata("", OnSubtitleChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public PageHeader()
    {
        this.InitializeComponent();

        SearchBorder.Tapped += (_, _) => SearchRequested?.Invoke(this, EventArgs.Empty);
        SearchBorder.PointerEntered += (s, _) =>
        {
            var b = (Border)s;
            b.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_20Brush"];
            b.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"];
        };
        SearchBorder.PointerExited += (s, _) =>
        {
            var b = (Border)s;
            b.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"];
            b.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"];
        };

    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageHeader header)
            header.TitleText.Text = e.NewValue as string ?? "";
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageHeader header)
            header.SubtitleText.Text = e.NewValue as string ?? "";
    }
}
