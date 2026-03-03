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
}
