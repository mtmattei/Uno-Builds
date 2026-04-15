namespace PrecisionDial.Controls;

/// <summary>
/// One entry in a <see cref="PrecisionDial"/> radial menu.
/// </summary>
public sealed class DialMenuItem
{
    /// <summary>Display label for the menu item.</summary>
    public string? Label { get; set; }

    /// <summary>Single-glyph icon (font character or emoji) drawn at the segment midpoint.</summary>
    public string? Icon { get; set; }

    /// <summary>Optional caller-supplied tag carried alongside the item.</summary>
    public object? Tag { get; set; }
}
