using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System.Text.RegularExpressions;

namespace AdaptiveInputDemo.Controls;

public sealed partial class ColorPickerPanel : UserControl
{
    private static readonly string[] PresetColors =
    [
        "#EF4444", "#F97316", "#F59E0B", "#EAB308", "#84CC16",
        "#22C55E", "#10B981", "#14B8A6", "#06B6D4", "#0EA5E9",
        "#3B82F6", "#6366F1", "#8B5CF6", "#A855F7", "#D946EF",
        "#EC4899", "#F43F5E", "#FFFFFF", "#94A3B8", "#1E293B"
    ];

    // Cached brushes for performance
    private static readonly SolidColorBrush TransparentBrush = new(Color.FromArgb(0, 0, 0, 0));
    private static readonly Regex HexValidationRegex = new(@"^#[0-9a-fA-F]{3}([0-9a-fA-F]{3})?$", RegexOptions.Compiled);

    public event EventHandler<string>? ColorSelected;

    public ColorPickerPanel()
    {
        InitializeComponent();
        CreateColorSwatches();
    }

    private void CreateColorSwatches()
    {
        var swatches = new List<FrameworkElement>();

        foreach (var colorHex in PresetColors)
        {
            var color = ParseColor(colorHex);
            var swatch = CreateColorSwatch(colorHex, color);
            swatches.Add(swatch);
        }

        ColorGrid.ItemsSource = swatches;
    }

    private Button CreateColorSwatch(string hex, Color color)
    {
        var isWhite = color.R == 255 && color.G == 255 && color.B == 255;
        var border = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(color),
            BorderBrush = isWhite
                ? (Brush)Application.Current.Resources["OutlineVariantBrush"]
                : TransparentBrush,
            BorderThickness = new Thickness(1)
        };

        var button = new Button
        {
            Content = border,
            Padding = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            Tag = hex,
            Style = (Style)Application.Current.Resources["IconButtonStyle"]
        };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, $"Color {hex}");

        button.Click += OnSwatchClick;

        return button;
    }

    private void OnSwatchClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string hex)
        {
            ColorSelected?.Invoke(this, hex);
        }
    }

    private void OnHexInputChanged(object sender, TextChangedEventArgs e)
    {
        var text = HexInput.Text;

        // Auto-add # if not present
        if (!string.IsNullOrEmpty(text) && !text.StartsWith('#'))
        {
            text = "#" + text;
            HexInput.Text = text;
            HexInput.SelectionStart = text.Length;
        }

        // Update preview if valid hex
        if (IsValidHex(text))
        {
            var color = ParseColor(text);
            PreviewSwatch.Background = new SolidColorBrush(color);
        }
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        var text = HexInput.Text;
        if (IsValidHex(text))
        {
            ColorSelected?.Invoke(this, text.ToUpperInvariant());
        }
    }

    public void UpdateValue(string value)
    {
        if (value.StartsWith('#') && IsValidHex(value))
        {
            HexInput.Text = value;
            var color = ParseColor(value);
            PreviewSwatch.Background = new SolidColorBrush(color);
        }
    }

    private static bool IsValidHex(string hex)
    {
        return HexValidationRegex.IsMatch(hex);
    }

    private static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');

        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        if (hex.Length == 6)
        {
            return Color.FromArgb(
                255,
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }

        return Color.FromArgb(255, 255, 255, 255);
    }
}
