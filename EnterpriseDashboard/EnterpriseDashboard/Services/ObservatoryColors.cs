using SkiaSharp;

namespace EnterpriseDashboard.Services;

public static class ObservatoryColors
{
    // Monochrome palette constants
    public static readonly SKColor Background = new(0x00, 0x00, 0x00);
    public static readonly SKColor CardSurface = new(0x0A, 0x0A, 0x0A);
    public static readonly SKColor GridLine = new(0x1A, 0x1A, 0x1A);
    public static readonly SKColor Subtle = new(0x33, 0x33, 0x33);
    public static readonly SKColor Mid = new(0x66, 0x66, 0x66);
    public static readonly SKColor Bright = new(0x99, 0x99, 0x99);
    public static readonly SKColor Text = new(0x9E, 0x9E, 0x9E);
    public static readonly SKColor Emphasis = new(0xCC, 0xCC, 0xCC);
    public static readonly SKColor HighContrast = new(0xFF, 0xFF, 0xFF);

    // Terminal neon variants
    public static readonly SKColor TermBackground = new(0x04, 0x08, 0x04);
    public static readonly SKColor TermCardSurface = new(0x06, 0x12, 0x06);
    public static readonly SKColor TermGridLine = new(0x00, 0x33, 0x11);
    public static readonly SKColor TermSubtle = new(0x00, 0x44, 0x22);
    public static readonly SKColor TermMid = new(0x00, 0x77, 0x44);
    public static readonly SKColor TermBright = new(0x00, 0xAA, 0x55);
    public static readonly SKColor TermText = new(0x00, 0xCC, 0x55);
    public static readonly SKColor TermEmphasis = new(0x00, 0xFF, 0x66);
    public static readonly SKColor TermHighContrast = new(0x00, 0xFF, 0x88);

    // Grayscale ramp for data mapping (11 stops from black to white)
    public static readonly SKColor[] GrayRamp =
    [
        new(0x00, 0x00, 0x00),
        new(0x1A, 0x1A, 0x1A),
        new(0x33, 0x33, 0x33),
        new(0x4D, 0x4D, 0x4D),
        new(0x66, 0x66, 0x66),
        new(0x80, 0x80, 0x80),
        new(0x99, 0x99, 0x99),
        new(0xB3, 0xB3, 0xB3),
        new(0xCC, 0xCC, 0xCC),
        new(0xE6, 0xE6, 0xE6),
        new(0xFF, 0xFF, 0xFF),
    ];

    // Terminal ramp for data mapping
    public static readonly SKColor[] TermRamp =
    [
        new(0x00, 0x11, 0x00),
        new(0x00, 0x22, 0x11),
        new(0x00, 0x44, 0x22),
        new(0x00, 0x55, 0x33),
        new(0x00, 0x77, 0x44),
        new(0x00, 0x99, 0x55),
        new(0x00, 0xAA, 0x55),
        new(0x00, 0xCC, 0x66),
        new(0x00, 0xDD, 0x77),
        new(0x00, 0xEE, 0x88),
        new(0x00, 0xFF, 0x88),
    ];

    public static SKColor GetBackground(bool terminal) => terminal ? TermBackground : Background;
    public static SKColor GetCardSurface(bool terminal) => terminal ? TermCardSurface : CardSurface;
    public static SKColor GetGridLine(bool terminal) => terminal ? TermGridLine : GridLine;
    public static SKColor GetSubtle(bool terminal) => terminal ? TermSubtle : Subtle;
    public static SKColor GetMid(bool terminal) => terminal ? TermMid : Mid;
    public static SKColor GetBright(bool terminal) => terminal ? TermBright : Bright;
    public static SKColor GetText(bool terminal) => terminal ? TermText : Text;
    public static SKColor GetEmphasis(bool terminal) => terminal ? TermEmphasis : Emphasis;
    public static SKColor GetHighContrast(bool terminal) => terminal ? TermHighContrast : HighContrast;
    public static SKColor[] GetRamp(bool terminal) => terminal ? TermRamp : GrayRamp;

    /// <summary>
    /// Interpolate a data value (0..1) to a grayscale/terminal color.
    /// </summary>
    public static SKColor MapValue(double normalized, bool terminal = false)
    {
        var ramp = terminal ? TermRamp : GrayRamp;
        normalized = Math.Clamp(normalized, 0.0, 1.0);
        double idx = normalized * (ramp.Length - 1);
        int lo = (int)Math.Floor(idx);
        int hi = Math.Min(lo + 1, ramp.Length - 1);
        float t = (float)(idx - lo);

        byte r = (byte)(ramp[lo].Red + (ramp[hi].Red - ramp[lo].Red) * t);
        byte g = (byte)(ramp[lo].Green + (ramp[hi].Green - ramp[lo].Green) * t);
        byte b = (byte)(ramp[lo].Blue + (ramp[hi].Blue - ramp[lo].Blue) * t);
        return new SKColor(r, g, b);
    }
}
