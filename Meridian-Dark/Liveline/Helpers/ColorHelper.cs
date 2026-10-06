using SkiaSharp;

namespace Liveline.Helpers;

/// <summary>
/// Derives a full chart palette from a single hex color using HSL manipulation.
/// </summary>
public static class ColorHelper
{
    public static SKColor ParseHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return new SKColor(76, 175, 80);

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new SKColor(r, g, b);
        }

        if (hex.Length == 8)
        {
            byte a = Convert.ToByte(hex.Substring(0, 2), 16);
            byte r = Convert.ToByte(hex.Substring(2, 2), 16);
            byte g = Convert.ToByte(hex.Substring(4, 2), 16);
            byte b = Convert.ToByte(hex.Substring(6, 2), 16);
            return new SKColor(r, g, b, a);
        }

        return new SKColor(76, 175, 80);
    }

    public static (float H, float S, float L) ToHsl(SKColor color)
    {
        float r = color.Red / 255f;
        float g = color.Green / 255f;
        float b = color.Blue / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float l = (max + min) / 2f;
        float h = 0f, s = 0f;

        if (max != min)
        {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g)
                h = (b - r) / d + 2f;
            else
                h = (r - g) / d + 4f;

            h /= 6f;
        }

        return (h * 360f, s, l);
    }

    public static SKColor FromHsl(float h, float s, float l, byte alpha = 255)
    {
        h = ((h % 360f) + 360f) % 360f;
        s = Math.Clamp(s, 0f, 1f);
        l = Math.Clamp(l, 0f, 1f);

        float c = (1f - Math.Abs(2f * l - 1f)) * s;
        float x = c * (1f - Math.Abs(h / 60f % 2f - 1f));
        float m = l - c / 2f;

        float r, g, b;

        if (h < 60f) { r = c; g = x; b = 0; }
        else if (h < 120f) { r = x; g = c; b = 0; }
        else if (h < 180f) { r = 0; g = c; b = x; }
        else if (h < 240f) { r = 0; g = x; b = c; }
        else if (h < 300f) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return new SKColor(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255),
            alpha);
    }

    public static SKColor WithAlpha(SKColor color, byte alpha)
        => new(color.Red, color.Green, color.Blue, alpha);

    /// <summary>WCAG relative luminance.</summary>
    private static double RelativeLuminance(SKColor color)
    {
        static double Channel(byte v)
        {
            var c = v / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(color.Red)
             + 0.7152 * Channel(color.Green)
             + 0.0722 * Channel(color.Blue);
    }

    /// <summary>WCAG contrast ratio between two opaque colors.</summary>
    public static double Contrast(SKColor a, SKColor b)
    {
        var la = RelativeLuminance(a);
        var lb = RelativeLuminance(b);
        var (hi, lo) = la >= lb ? (la, lb) : (lb, la);
        return (hi + 0.05) / (lo + 0.05);
    }

    /// <summary>
    /// Derives chart palette colors from a single base color.
    /// </summary>
    public static ChartPalette DerivePalette(string hexColor, bool isDark)
    {
        var baseColor = ParseHex(hexColor);
        var (h, s, l) = ToHsl(baseColor);

        var lineColor = baseColor;
        var fillTop = WithAlpha(baseColor, 80);
        var fillBottom = WithAlpha(baseColor, 0);

        SKColor background, gridLine, textColor, textDim;
        SKColor momentumUp, momentumDown, momentumFlat;
        if (isDark)
        {
            // Warm ink ground so the chart sits flush in a warm-dark card rather
            // than punching a cool near-black hole through it.
            background = new SKColor(0x1E, 0x1B, 0x16);
            gridLine = new SKColor(0xF2, 0xED, 0xE3, 28);
            textColor = new SKColor(0xF2, 0xED, 0xE3, 200);
            textDim = new SKColor(0xF2, 0xED, 0xE3, 105);
            momentumUp = new SKColor(0x5C, 0xA8, 0x7E);
            momentumDown = new SKColor(0xDA, 0x61, 0x54);
            momentumFlat = new SKColor(0x9C, 0x91, 0x7C);
        }
        else
        {
            background = new SKColor(255, 255, 255);
            gridLine = new SKColor(0, 0, 0, 25);
            textColor = new SKColor(0, 0, 0, 200);
            textDim = new SKColor(0, 0, 0, 100);
            momentumUp = ParseHex("#4CAF50");
            momentumDown = ParseHex("#F44336");
            momentumFlat = new SKColor(158, 158, 158);
        }

        // Pick the badge label by measured contrast, not by an L threshold: a
        // mid-lightness accent (L just under the old 0.55 cut) took white text
        // at ~2.9:1, which is unreadable on the badge.
        var badgeBg = baseColor;
        var badgeText = Contrast(baseColor, SKColors.Black) >= Contrast(baseColor, SKColors.White)
            ? new SKColor(0, 0, 0, 230)
            : new SKColor(255, 255, 255, 240);

        return new ChartPalette(
            lineColor, fillTop, fillBottom,
            background, gridLine, textColor, textDim,
            badgeBg, badgeText,
            momentumUp, momentumDown, momentumFlat);
    }
}

public record ChartPalette(
    SKColor LineColor,
    SKColor FillTop,
    SKColor FillBottom,
    SKColor Background,
    SKColor GridLine,
    SKColor TextColor,
    SKColor TextDim,
    SKColor BadgeBg,
    SKColor BadgeText,
    SKColor MomentumUp,
    SKColor MomentumDown,
    SKColor MomentumFlat);
