using System;

namespace PrecisionDial.Controls;

/// <summary>
/// Computes runtime geometry, segment counts and feature-visibility flags
/// from the dial's available size. All v3 dial scaling decisions go through this.
/// </summary>
internal sealed class DialSizeProfile
{
    public float Size { get; }

    public DialSizeProfile(float size)
    {
        Size = size;
    }

    // ── Geometry (proportional) ──────────────────────────────────────────────
    public float ArcR => Size * 0.43f;
    public float KnobR => Size * 0.35f;
    public float BezelR => Size * 0.36f;

    // ── Counts ───────────────────────────────────────────────────────────────
    public int AutoSegments => Math.Max(8, (int)MathF.Round(Size / 7f));
    public int AutoDetents => Math.Max(4, (int)MathF.Round(Size / 10f));
    public int KnurlCount => (int)MathF.Round(Size * 0.9f);

    // ── Stroke / font ────────────────────────────────────────────────────────
    public float IndicatorStrokeWidth => MathF.Max(1.0f, Size * 0.007f);
    public float ValueFontSize => MathF.Max(8f, Size * 0.04f);

    // Dashed arc strokes scale with size; floors keep them visible at tiny sizes.
    public float SegmentInactiveStroke => MathF.Max(1.0f, Size * 0.0075f); // ~1.5dp at 200
    public float SegmentActiveStroke => MathF.Max(1.5f, Size * 0.015f);    // ~3dp at 200

    // ── Feature visibility thresholds (from brief section 08) ────────────────
    public bool ShowOrbitingValue => Size >= 56f;
    public bool ShowBrushedMetal => Size >= 60f;
    public bool ShowMenuIcons => Size >= 60f;
    public bool ShowReadout => Size >= 72f;
    public bool ShowMenuLabel => Size >= 96f;
}
