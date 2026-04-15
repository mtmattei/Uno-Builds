namespace Orbital.Helpers;

/// <summary>
/// Centralized color constants for status indicators and accent colors.
/// Used by controls that need Color values (not brushes) in code-behind.
/// </summary>
public static class OrbitalColors
{
    // Status colors
    public static readonly Windows.UI.Color Emerald400 = Windows.UI.ColorHelper.FromArgb(255, 52, 211, 153);
    public static readonly Windows.UI.Color Emerald500 = Windows.UI.ColorHelper.FromArgb(255, 16, 185, 129);
    public static readonly Windows.UI.Color Violet400 = Windows.UI.ColorHelper.FromArgb(255, 167, 139, 250);
    public static readonly Windows.UI.Color Amber400 = Windows.UI.ColorHelper.FromArgb(255, 251, 191, 36);
    public static readonly Windows.UI.Color Red400 = Windows.UI.ColorHelper.FromArgb(255, 248, 113, 113);
    public static readonly Windows.UI.Color Blue400 = Windows.UI.ColorHelper.FromArgb(255, 96, 165, 250);
    public static readonly Windows.UI.Color Zinc500 = Windows.UI.ColorHelper.FromArgb(255, 113, 113, 122);

    // Console line type colors
    public static readonly Windows.UI.Color Success = Emerald400;
    public static readonly Windows.UI.Color Error = Red400;
    public static readonly Windows.UI.Color Warn = Amber400;
    public static readonly Windows.UI.Color Dim = Windows.UI.ColorHelper.FromArgb(255, 86, 92, 107);
    public static readonly Windows.UI.Color Info = Windows.UI.ColorHelper.FromArgb(255, 163, 168, 182);

    // Surface
    public static readonly Windows.UI.Color Surface0 = Windows.UI.ColorHelper.FromArgb(255, 10, 10, 11);

    public static Windows.UI.Color StatusColor(string status) => status switch
    {
        "ok" => Emerald500,
        "warn" => Amber400,
        "error" => Red400,
        _ => Zinc500,
    };

    public static Windows.UI.Color AccentColor(string name) => name switch
    {
        "emerald" => Emerald400,
        "violet" => Violet400,
        "amber" => Amber400,
        "blue" => Blue400,
        "red" => Red400,
        _ => Zinc500,
    };

    public static Microsoft.UI.Xaml.Media.Brush AccentBrush(string name) =>
        (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources[name switch
        {
            "emerald" => "OrbitalEmerald400Brush",
            "blue" => "OrbitalBlue400Brush",
            "violet" => "OrbitalViolet400Brush",
            "amber" => "OrbitalAmber400Brush",
            _ => "OrbitalText50Brush",
        }];

    public static string TimeAgo(DateTime from)
    {
        var elapsed = DateTime.Now - from;
        return elapsed.TotalMinutes < 1 ? "just now"
            : elapsed.TotalMinutes < 60 ? $"{(int)elapsed.TotalMinutes}m ago"
            : elapsed.TotalHours < 24 ? $"{(int)elapsed.TotalHours}hr ago"
            : $"{(int)elapsed.TotalDays}d ago";
    }
}
