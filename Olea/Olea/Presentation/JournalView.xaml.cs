using Microsoft.UI.Xaml.Media;

namespace Olea.Presentation;

public sealed partial class JournalView : UserControl
{
    private readonly TastingService _tastingService;

    public JournalView()
    {
        this.InitializeComponent();
        _tastingService = new TastingService();
        Loaded += (_, _) => RenderJournal();
    }

    public void RenderJournal()
    {
        RootPanel.Children.Clear();
        var entries = _tastingService.GetJournal();

        if (entries.Count == 0)
        {
            RenderEmptyState();
            return;
        }

        foreach (var entry in entries)
        {
            RootPanel.Children.Add(CreateJournalCard(entry));
        }
    }

    private void RenderEmptyState()
    {
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Padding = new Thickness(0, 48, 0, 48)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "\U0001FAD2",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = "No tastings yet",
            FontFamily = new FontFamily("Georgia"),
            FontSize = 28,
            FontWeight = Microsoft.UI.Text.FontWeights.Light,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45)),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = "Record your first olive oil tasting to start building your journal.",
            FontSize = 14,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96)),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        RootPanel.Children.Add(panel);
    }

    private Border CreateJournalCard(TastingEntry entry)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 252, 249)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 221, 208)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Translation = new System.Numerics.Vector3(0, 0, 8),
            Shadow = new ThemeShadow()
        };

        var outerStack = new StackPanel();

        // Color bar gradient
        var colorBar = new Border
        {
            Height = 4,
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Background = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0.5),
                EndPoint = new Windows.Foundation.Point(1, 0.5),
                GradientStops =
                {
                    new GradientStop { Color = Windows.UI.Color.FromArgb(255, 74, 93, 58), Offset = 0 },
                    new GradientStop { Color = Windows.UI.Color.FromArgb(255, 196, 164, 74), Offset = 1 }
                }
            }
        };
        outerStack.Children.Add(colorBar);

        var contentStack = new StackPanel { Padding = new Thickness(20), Spacing = 8 };

        // Date + Stars row
        var dateStarsRow = new Grid();
        dateStarsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        dateStarsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var dateText = new TextBlock
        {
            Text = FormatDate(entry.TastingDate),
            FontSize = 9,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 139, 115, 85)),
            CharacterSpacing = 100,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(dateText, 0);
        dateStarsRow.Children.Add(dateText);

        var starsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        for (int i = 1; i <= 5; i++)
        {
            starsPanel.Children.Add(new FontIcon
            {
                Glyph = "\uE735",
                FontSize = 14,
                Foreground = i <= entry.Rating
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 196, 164, 74))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 221, 208))
            });
        }
        Grid.SetColumn(starsPanel, 1);
        dateStarsRow.Children.Add(starsPanel);
        contentStack.Children.Add(dateStarsRow);

        // Oil name
        contentStack.Children.Add(new TextBlock
        {
            Text = entry.Name,
            FontFamily = new FontFamily("Georgia"),
            FontSize = 17,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45))
        });

        // Origin + Cultivar
        var originText = string.IsNullOrEmpty(entry.Cultivar)
            ? entry.Origin
            : $"{entry.Origin} \u00b7 {entry.Cultivar}";
        contentStack.Children.Add(new TextBlock
        {
            Text = originText,
            FontSize = 11,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96))
        });

        // Flavor tags
        if (entry.Flavors.Count > 0)
        {
            var tagsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 4, 0, 0) };
            foreach (var flavor in entry.Flavors)
            {
                var tagBorder = new Border
                {
                    Background = new SolidColorBrush(ParseColor(flavor.Color)),
                    CornerRadius = new CornerRadius(100),
                    Padding = new Thickness(10, 4, 10, 4)
                };
                tagBorder.Child = new TextBlock
                {
                    Text = flavor.Name.ToUpperInvariant(),
                    FontSize = 7,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                    CharacterSpacing = 50
                };
                tagsPanel.Children.Add(tagBorder);
            }
            contentStack.Children.Add(tagsPanel);
        }

        // Intensity bars - 3-column row
        var intensityGrid = new Grid { Margin = new Thickness(0, 8, 0, 0), ColumnSpacing = 12 };
        intensityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        intensityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        intensityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        AddIntensityColumn(intensityGrid, 0, "FRUITY", entry.Intensities.Fruity, "#6B9B4A");
        AddIntensityColumn(intensityGrid, 1, "BITTER", entry.Intensities.Bitter, "#C49B3A");
        AddIntensityColumn(intensityGrid, 2, "PUNGENT", entry.Intensities.Pungent, "#B85A4A");

        contentStack.Children.Add(intensityGrid);
        outerStack.Children.Add(contentStack);
        card.Child = outerStack;

        return card;
    }

    private void AddIntensityColumn(Grid grid, int col, string label, int value, string colorHex)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 7,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 139, 115, 85)),
            CharacterSpacing = 100,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        var trackGrid = new Grid { Height = 4 };
        trackGrid.Children.Add(new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 232, 216)),
            CornerRadius = new CornerRadius(2)
        });

        var fillWidth = value * 10.0; // percentage as rough pixel width
        trackGrid.Children.Add(new Border
        {
            Background = new SolidColorBrush(ParseColor(colorHex)),
            CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = fillWidth > 0 ? fillWidth : 0
        });

        stack.Children.Add(trackGrid);
        Grid.SetColumn(stack, col);
        grid.Children.Add(stack);
    }

    private static string FormatDate(string dateStr)
    {
        if (DateTime.TryParse(dateStr, out var dt))
        {
            return dt.ToString("MMM d, yyyy").ToUpperInvariant();
        }
        return dateStr.ToUpperInvariant();
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            return Windows.UI.Color.FromArgb(255,
                byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber));
        }
        return Windows.UI.Color.FromArgb(255, 128, 128, 128);
    }
}
