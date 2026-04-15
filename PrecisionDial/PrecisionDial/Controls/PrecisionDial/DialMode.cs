namespace PrecisionDial.Controls;

/// <summary>
/// Switches PrecisionDial between continuous value mode and discrete radial menu mode.
/// </summary>
public enum DialMode
{
    /// <summary>Continuous value dial — dashed arc, orbiting value, line indicator.</summary>
    Value,

    /// <summary>Radial menu selector — segments map 1:1 to MenuItems, cone-of-light indicator.</summary>
    Menu,
}
