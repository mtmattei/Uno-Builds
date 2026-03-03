using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using ColorHelper = Microsoft.UI.ColorHelper;

namespace LiquidMorph;

public sealed partial class MainPage : Page
{
    private int _viewIndex;
    private const int CardCount = 3;

    private static readonly string[] HeroImages =
    [
        "ms-appx:///Assets/Images/ocean.jpg",
        "ms-appx:///Assets/Images/volcano.jpg",
        "ms-appx:///Assets/Images/glacier.jpg"
    ];

    // Card data
    private static readonly CardData[] Cards =
    [
        new("OCEANOGRAPHY",
            "Abyssal Bioluminescence",
            "In the perpetual dark below 4,000 meters, life itself becomes the only source of light.",
            ColorHelper.FromArgb(255, 80, 90, 50),    // olive gradient top
            ColorHelper.FromArgb(255, 40, 35, 25),     // dark brown bottom
            ColorHelper.FromArgb(255, 160, 200, 60)),   // lime badge

        new("VOLCANOLOGY",
            "Submarine Eruptions",
            "Where tectonic plates diverge beneath kilometers of ocean, magma meets water in violent creation.",
            ColorHelper.FromArgb(255, 90, 45, 30),     // warm rust top
            ColorHelper.FromArgb(255, 30, 20, 25),      // dark bottom
            ColorHelper.FromArgb(255, 255, 140, 60)),    // orange badge

        new("GLACIOLOGY",
            "Subglacial Lakes",
            "Hidden beneath Antarctic ice sheets, liquid water persists in complete isolation for millions of years.",
            ColorHelper.FromArgb(255, 35, 60, 85),     // steel blue top
            ColorHelper.FromArgb(255, 18, 22, 35),      // deep navy bottom
            ColorHelper.FromArgb(255, 100, 200, 220)),   // teal badge
    ];

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        MorphPanel.SetContent(BuildCard(0));
    }

    private void OnSwitchClicked(object sender, RoutedEventArgs e)
    {
        _viewIndex = (_viewIndex + 1) % CardCount;
        MorphPanel.TransitionTo(BuildCard(_viewIndex));
    }

    private static UIElement BuildCard(int index)
    {
        var card = Cards[index];

        // Outer border with rounded corners. The morph captures this entire
        // element, so displacement warps the rounded edges.
        var outerBorder = new Border
        {
            CornerRadius = new CornerRadius(20),
            Background = new SolidColorBrush(ColorHelper.FromArgb(255, 23, 23, 32)),
            Translation = new System.Numerics.Vector3(0, 0, 24),
            Shadow = new ThemeShadow()
        };

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.42, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.58, GridUnitType.Star) });

        // --- Hero image area ---
        var heroGrid = new Grid();

        // Base gradient fallback
        var heroGradient = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0.3, 0),
            EndPoint = new Windows.Foundation.Point(0.8, 1)
        };
        heroGradient.GradientStops.Add(new GradientStop { Color = card.GradientTop, Offset = 0.0 });
        heroGradient.GradientStops.Add(new GradientStop { Color = card.GradientBottom, Offset = 1.0 });
        heroGrid.Children.Add(new Border { Background = heroGradient });

        // Actual hero image
        var heroImage = new Image
        {
            Source = new BitmapImage(new System.Uri(HeroImages[index])),
            Stretch = Stretch.UniformToFill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.7
        };
        heroGrid.Children.Add(heroImage);

        // Dark tint overlay for atmospheric mood
        var tintOverlay = new Border
        {
            Background = new SolidColorBrush(ColorHelper.FromArgb(100,
                card.GradientTop.R, card.GradientTop.G, card.GradientTop.B))
        };
        heroGrid.Children.Add(tintOverlay);

        // Bottom fade into card bg
        var fadeGradient = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0.5, 0),
            EndPoint = new Windows.Foundation.Point(0.5, 1)
        };
        fadeGradient.GradientStops.Add(new GradientStop
            { Color = ColorHelper.FromArgb(0, 23, 23, 32), Offset = 0.0 });
        fadeGradient.GradientStops.Add(new GradientStop
            { Color = ColorHelper.FromArgb(0, 23, 23, 32), Offset = 0.5 });
        fadeGradient.GradientStops.Add(new GradientStop
            { Color = ColorHelper.FromArgb(255, 23, 23, 32), Offset = 1.0 });
        heroGrid.Children.Add(new Border { Background = fadeGradient });

        Grid.SetRow(heroGrid, 0);
        root.Children.Add(heroGrid);

        // --- Content area ---
        var content = new StackPanel
        {
            Padding = new Thickness(28, 8, 28, 24),
            Spacing = 16
        };
        Grid.SetRow(content, 1);

        // Category badge
        var badgeBorder = new Border
        {
            Background = new SolidColorBrush(ColorHelper.FromArgb(30, card.AccentColor.R, card.AccentColor.G, card.AccentColor.B)),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 4, 12, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(50, card.AccentColor.R, card.AccentColor.G, card.AccentColor.B)),
            BorderThickness = new Thickness(1)
        };
        var badgeText = new TextBlock
        {
            Text = card.Category,
            FontSize = 11,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(card.AccentColor),
            CharacterSpacing = 80
        };
        badgeBorder.Child = badgeText;
        content.Children.Add(badgeBorder);

        // Title
        var title = new TextBlock
        {
            Text = card.Title,
            FontSize = 28,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(ColorHelper.FromArgb(240, 230, 230, 235)),
            TextWrapping = TextWrapping.Wrap
        };
        content.Children.Add(title);

        // Body
        var body = new TextBlock
        {
            Text = card.Body,
            FontSize = 15,
            Foreground = new SolidColorBrush(ColorHelper.FromArgb(140, 180, 180, 190)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22
        };
        content.Children.Add(body);

        // Spacer
        content.Children.Add(new Border { Height = 8 });

        // Dot indicators
        var dotsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        for (int i = 0; i < CardCount; i++)
        {
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = i == index
                    ? new SolidColorBrush(card.AccentColor)
                    : new SolidColorBrush(ColorHelper.FromArgb(80, 140, 140, 150))
            };
            dotsPanel.Children.Add(dot);
        }
        content.Children.Add(dotsPanel);

        root.Children.Add(content);
        outerBorder.Child = root;
        return outerBorder;
    }

    private record CardData(
        string Category,
        string Title,
        string Body,
        Windows.UI.Color GradientTop,
        Windows.UI.Color GradientBottom,
        Windows.UI.Color AccentColor);
}
