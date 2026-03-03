using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Color = Windows.UI.Color;

namespace AdaptiveInputDemo.Controls;

public sealed partial class TagPickerPanel : UserControl
{
    private readonly List<TagItem> _allTags;
    private string _currentFilter = string.Empty;

    public event EventHandler<string>? TagSelected;

    public TagPickerPanel()
    {
        InitializeComponent();

        // Sample tags - in a real app, this would come from a data source
        _allTags = new List<TagItem>
        {
            new("urgent", "#EF4444", 12),
            new("bug", "#F97316", 8),
            new("feature", "#22C55E", 15),
            new("enhancement", "#3B82F6", 6),
            new("documentation", "#8B5CF6", 4),
            new("design", "#EC4899", 9),
            new("backend", "#06B6D4", 11),
            new("frontend", "#F59E0B", 7),
            new("testing", "#10B981", 5),
            new("performance", "#6366F1", 3)
        };

        UpdateTagsDisplay();
    }

    public void UpdateFilter(string filter)
    {
        _currentFilter = filter.ToLowerInvariant();
        UpdateTagsDisplay();
    }

    private void UpdateTagsDisplay()
    {
        var filtered = string.IsNullOrWhiteSpace(_currentFilter)
            ? _allTags
            : _allTags.Where(t => t.Name.Contains(_currentFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        var tagButtons = filtered.Select(BuildTagButton).ToList();
        TagsContainer.ItemsSource = tagButtons;

        // Show create button if no exact match exists
        var hasExactMatch = _allTags.Any(t => t.Name.Equals(_currentFilter, StringComparison.OrdinalIgnoreCase));
        var shouldShowCreate = !string.IsNullOrWhiteSpace(_currentFilter) && !hasExactMatch;

        CreateTagButton.Visibility = shouldShowCreate ? Visibility.Visible : Visibility.Collapsed;
        CreateTagLabel.Text = $"Create \"{_currentFilter}\"";
    }

    private Button BuildTagButton(TagItem tag)
    {
        var color = ParseColor(tag.ColorHex);

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        // Color dot
        content.Children.Add(new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(color),
            VerticalAlignment = VerticalAlignment.Center
        });

        // Tag name
        content.Children.Add(new TextBlock
        {
            Text = tag.Name,
            VerticalAlignment = VerticalAlignment.Center
        });

        // Usage count
        content.Children.Add(new TextBlock
        {
            Text = tag.UsageCount.ToString(),
            Foreground = (Brush)Application.Current.Resources["OnSurfaceVariantBrush"],
            VerticalAlignment = VerticalAlignment.Center,
            Style = (Style)Application.Current.Resources["LabelSmall"]
        });

        var button = new Button
        {
            Content = content,
            Tag = tag.Name,
            Style = (Style)Application.Current.Resources["FilledTonalButtonStyle"],
            Padding = new Thickness(12, 8, 12, 8)
        };

        button.Click += OnTagClick;

        return button;
    }

    private void OnTagClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tagName)
        {
            TagSelected?.Invoke(this, tagName);
        }
    }

    private void OnCreateTagClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentFilter))
        {
            TagSelected?.Invoke(this, _currentFilter);
        }
    }

    private static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            return Color.FromArgb(
                255,
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }
        return Color.FromArgb(255, 128, 128, 128);
    }
}

public class TagItem
{
    public string Name { get; }
    public string ColorHex { get; }
    public int UsageCount { get; }

    public TagItem(string name, string colorHex, int usageCount)
    {
        Name = name;
        ColorHex = colorHex;
        UsageCount = usageCount;
    }
}
