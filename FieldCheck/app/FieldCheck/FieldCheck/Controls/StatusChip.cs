using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FieldCheck.Controls;

/// <summary>Compact status label. Colour is paired with the text label, never used alone.</summary>
public sealed partial class StatusChip : ContentControl
{
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status), typeof(string), typeof(StatusChip), new PropertyMetadata(string.Empty, (d, _) => ((StatusChip)d).Apply()));

    private readonly Border _border;
    private readonly TextBlock _text;

    public StatusChip()
    {
        IsTabStop = false;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        _text = new TextBlock { FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        _border = new Border
        {
            CornerRadius = new CornerRadius(13),
            Padding = new Thickness(14, 0, 14, 0),
            Height = 26,
            Child = _text,
        };
        Content = _border;
        Apply();
    }

    /// <summary>Operational, Good, Attention or Critical.</summary>
    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private void Apply()
    {
        var (fg, bg) = Status switch
        {
            "Operational" or "Good" => ("SuccessBrush", "SoftSuccessBrush"),
            "Attention" => ("AttentionBrush", "SoftAttentionBrush"),
            "Critical" => ("CriticalBrush", "SoftCriticalBrush"),
            _ => ("MutedBrush", "CanvasBrush"),
        };
        _text.Text = Status;
        _text.Foreground = (Brush)Application.Current.Resources[fg];
        _border.Background = (Brush)Application.Current.Resources[bg];
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(this, $"Status: {Status}");
    }
}
