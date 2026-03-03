using Microsoft.UI.Xaml.Media;

namespace Olea.Presentation;

public sealed partial class ExploreView : UserControl
{
    public ExploreView()
    {
        this.InitializeComponent();
        Loaded += (_, _) => RenderRegions();
    }

    private void RenderRegions()
    {
        RootPanel.Children.Clear();

        foreach (var region in SeedData.Regions)
        {
            RootPanel.Children.Add(CreateRegionCard(region));
        }
    }

    private Border CreateRegionCard(OleaRegion region)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 252, 249)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 221, 208)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(24),
            Translation = new System.Numerics.Vector3(0, 0, 8),
            Shadow = new ThemeShadow()
        };

        var stack = new StackPanel { Spacing = 8 };

        // Flag emoji
        stack.Children.Add(new TextBlock
        {
            Text = region.Flag,
            FontSize = 24
        });

        // Name + Area row
        var nameAreaPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        nameAreaPanel.Children.Add(new TextBlock
        {
            Text = region.Name,
            FontFamily = new FontFamily("Georgia"),
            FontSize = 17,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45)),
            VerticalAlignment = VerticalAlignment.Bottom
        });
        nameAreaPanel.Children.Add(new TextBlock
        {
            Text = region.Area,
            FontSize = 11,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 184, 155, 62)),
            VerticalAlignment = VerticalAlignment.Bottom
        });
        stack.Children.Add(nameAreaPanel);

        // Description
        stack.Children.Add(new TextBlock
        {
            Text = region.Description,
            FontSize = 10,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96)),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 17
        });

        // Cultivar tags
        var tagsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 4, 0, 0) };
        foreach (var cultivar in region.Cultivars)
        {
            var tagBorder = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 232, 216)),
                CornerRadius = new CornerRadius(100),
                Padding = new Thickness(10, 4, 10, 4)
            };
            tagBorder.Child = new TextBlock
            {
                Text = cultivar.ToUpperInvariant(),
                FontSize = 7,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 74, 93, 58)),
                CharacterSpacing = 50
            };
            tagsPanel.Children.Add(tagBorder);
        }
        stack.Children.Add(tagsPanel);

        card.Child = stack;
        return card;
    }
}
