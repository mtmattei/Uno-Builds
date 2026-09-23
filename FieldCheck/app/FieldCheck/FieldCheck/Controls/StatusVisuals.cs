using Microsoft.UI.Xaml.Media;

namespace FieldCheck.Controls;

/// <summary>x:Bind helpers mapping status/condition names to semantic brushes.</summary>
public static class StatusVisuals
{
    public static Brush Foreground(string status) => (Brush)Application.Current.Resources[status switch
    {
        "Operational" or "Good" => "SuccessBrush",
        "Attention" => "AttentionBrush",
        "Critical" => "CriticalBrush",
        _ => "InkBrush",
    }];

    /// <summary>Navigation item text/icon brush: Ink when active, Muted otherwise.</summary>
    public static Brush Nav(bool active) => (Brush)Application.Current.Resources[active ? "InkBrush" : "MutedBrush"];

    public static Visibility Show(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility Hide(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility ShowText(string value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
}
