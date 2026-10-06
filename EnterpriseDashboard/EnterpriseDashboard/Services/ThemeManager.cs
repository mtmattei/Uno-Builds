using Microsoft.UI.Xaml.Media;
using SkiaSharp;

namespace EnterpriseDashboard.Services;

public enum DashboardTheme
{
    Monochrome,
    Terminal
}

public class ThemeColors
{
    public required SKColor LineStroke { get; init; }
    public required SKColor LineFillTop { get; init; }
    public required SKColor LineFillBottom { get; init; }
    public required SKColor[] PiePalette { get; init; }
    public required SKColor Surface { get; init; }
    public required SKColor GridLine { get; init; }
    public required SKColor Label { get; init; }
    public required SKColor GeometryFill { get; init; }

    // Map pin colors
    public required Mapsui.Styles.Color PinFill { get; init; }
    public required Mapsui.Styles.Color PinOutline { get; init; }
    public required Mapsui.Styles.Color RouteLine { get; init; }

    // XAML-level accent for badges and glow borders
    public required string AccentHex { get; init; }
    public required string AccentBgHex { get; init; }
    public required string AlternateRowHex { get; init; }
    public required string GlowBorderHex { get; init; }
}

public static class ThemeManager
{
    public static DashboardTheme Current { get; set; } = DashboardTheme.Monochrome;

    public static ThemeColors GetColors() => Current switch
    {
        DashboardTheme.Terminal => TerminalColors,
        _ => MonoColors
    };

    private static readonly ThemeColors MonoColors = new()
    {
        LineStroke = new SKColor(0xFF, 0xFF, 0xFF),
        LineFillTop = new SKColor(0xFF, 0xFF, 0xFF, 0x60),
        LineFillBottom = new SKColor(0xFF, 0xFF, 0xFF, 0x00),
        PiePalette =
        [
            new(0xFF, 0xFF, 0xFF),
            new(0xCC, 0xCC, 0xCC),
            new(0x99, 0x99, 0x99),
            new(0x66, 0x66, 0x66),
            new(0x4D, 0x4D, 0x4D),
            new(0xB0, 0xB0, 0xB0),
            new(0x85, 0x85, 0x85),
            new(0x38, 0x38, 0x38),
        ],
        Surface = new SKColor(0x0A, 0x0A, 0x0A),
        GridLine = new SKColor(0x1A, 0x1A, 0x1A),
        Label = new SKColor(0x9E, 0x9E, 0x9E),
        GeometryFill = new SKColor(0x0A, 0x0A, 0x0A),
        PinFill = new Mapsui.Styles.Color(224, 224, 224, 200),
        PinOutline = new Mapsui.Styles.Color(10, 10, 10),
        RouteLine = new Mapsui.Styles.Color(224, 224, 224, 200),
        AccentHex = "#FF26A69A",
        AccentBgHex = "#1A26A69A",
        AlternateRowHex = "#FF111111",
        GlowBorderHex = "#FF222222",
    };

    private static readonly ThemeColors TerminalColors = new()
    {
        LineStroke = new SKColor(0x00, 0xFF, 0x66),          // Neon green
        LineFillTop = new SKColor(0x00, 0xFF, 0x66, 0x50),
        LineFillBottom = new SKColor(0x00, 0xFF, 0x66, 0x00),
        PiePalette =
        [
            new(0x00, 0xFF, 0x66),  // Neon green
            new(0x00, 0xE5, 0xFF),  // Electric cyan
            new(0x00, 0xCC, 0x88),  // Emerald
            new(0x00, 0x99, 0xFF),  // Bright blue
            new(0x88, 0xFF, 0x00),  // Lime
            new(0x00, 0xFF, 0xCC),  // Aqua
            new(0x66, 0xFF, 0x66),  // Light green
            new(0x00, 0x88, 0xCC),  // Deep cyan
        ],
        Surface = new SKColor(0x04, 0x08, 0x04),
        GridLine = new SKColor(0x00, 0x33, 0x11),
        Label = new SKColor(0x00, 0xCC, 0x55),
        GeometryFill = new SKColor(0x04, 0x08, 0x04),
        PinFill = new Mapsui.Styles.Color(0, 255, 102, 180),
        PinOutline = new Mapsui.Styles.Color(0, 60, 20),
        RouteLine = new Mapsui.Styles.Color(0, 229, 255, 200),
        AccentHex = "#FF00FF66",
        AccentBgHex = "#1A00FF66",
        AlternateRowHex = "#FF061206",
        GlowBorderHex = "#FF003311",
    };

    public static Windows.UI.Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        byte a = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
        byte r = byte.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex[6..8], System.Globalization.NumberStyles.HexNumber);
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }

    public static void SwapColorPalette(bool terminal, FrameworkElement? rootElement)
    {
        var palettePath = terminal
            ? "ms-appx:///Styles/TerminalPaletteOverride.xaml"
            : "ms-appx:///Styles/ColorPaletteOverride.xaml";

        var mergedDicts = Application.Current.Resources.MergedDictionaries;
        for (int i = mergedDicts.Count - 1; i >= 0; i--)
        {
            if (mergedDicts[i] is ResourceDictionary rd
                && rd.Source?.OriginalString.Contains("PaletteOverride") == true)
            {
                mergedDicts.RemoveAt(i);
            }
        }
        mergedDicts.Add(new ResourceDictionary { Source = new Uri(palettePath) });

        if (rootElement is not null)
        {
            rootElement.RequestedTheme = ElementTheme.Light;
            rootElement.RequestedTheme = ElementTheme.Dark;
        }
    }

    public static void ApplyAccentBrushes()
    {
        var colors = GetColors();
        var resources = Application.Current.Resources;

        SetBrushColor(resources, "VendorBadgeBrush", colors.AccentHex);
        SetBrushColor(resources, "VendorBadgeBgBrush", colors.AccentBgHex);
        SetBrushColor(resources, "TableAlternateRowBrush", colors.AlternateRowHex);
    }

    public static Windows.UI.Color GetGlowBorderColor() => ParseColor(GetColors().GlowBorderHex);

    private static void SetBrushColor(ResourceDictionary resources, string key, string hex)
    {
        if (resources.TryGetValue(key, out var resource) && resource is SolidColorBrush brush)
        {
            brush.Color = ParseColor(hex);
        }
    }
}
