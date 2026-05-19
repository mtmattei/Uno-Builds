using Windows.UI;

namespace Composer.Models;

/// <summary>
/// Layer 4 — Design System. Eight named color tokens covering the surface
/// hierarchy + four semantic accents, plus the body font choice.
/// Spec: <c>docs/update-context-engine-05-design.md</c> §1.
///
/// Default values reflect the field-service archetype the briefs use as a
/// running example — outdoor-readable mobile UI: low-chroma neutrals with one
/// saturated accent (Action) reserved for the primary CTA. Other archetypes
/// produce different defaults via the AI layer-preview path.
///
/// Token → Material slot mapping (briefs 05 §4):
///   Action  → SecondaryColor (both themes)
///   Warn    → ErrorColor     (both themes)
///   Surface → BackgroundColor (Dark)
///   Panel   → SurfaceColor    (Dark)
/// Info / Success / Tag / Locked are semantic resources without canonical
/// Material slots — kept stable across themes.
/// </summary>
public record DesignTokens(
    Color Surface,
    Color Action,
    Color Info,
    Color Success,
    Color Warn,
    Color Panel,
    Color Tag,
    Color Locked,
    string BodyFont)
{
    public static DesignTokens Default { get; } = new(
        Surface: Color.FromArgb(0xFF, 0x0c, 0x0d, 0x0f),
        Action:  Color.FromArgb(0xFF, 0xd4, 0xa9, 0x59),
        Info:    Color.FromArgb(0xFF, 0x7a, 0xb3, 0xdf),
        Success: Color.FromArgb(0xFF, 0x9b, 0xbf, 0x9d),
        Warn:    Color.FromArgb(0xFF, 0xe8, 0x7f, 0x6d),
        Panel:   Color.FromArgb(0xFF, 0x16, 0x18, 0x1c),
        Tag:     Color.FromArgb(0xFF, 0xb2, 0x88, 0xc4),
        Locked:  Color.FromArgb(0xFF, 0x0c, 0x0d, 0x0f),
        BodyFont: "Fraunces");
}
