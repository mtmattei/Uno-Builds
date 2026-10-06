using System.Globalization;
using FreewriteUno.Models;

namespace FreewriteUno.ViewModels;

public static class Formatters
{
    public static string FormatTimer(int totalSeconds)
    {
        var clamped = Math.Max(0, totalSeconds);
        var minutes = clamped / 60;
        var seconds = clamped % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public static string FormatFontSize(double size) => ((int)Math.Round(size)).ToString(CultureInfo.InvariantCulture);

    public static string FormatRelativeDate(DateTimeOffset value)
    {
        var today = DateTimeOffset.Now.Date;
        var date = value.Date;
        var days = (today - date).Days;
        return days switch
        {
            0 => "Today",
            1 => "Yesterday",
            _ => value.ToString("MMM d", CultureInfo.InvariantCulture),
        };
    }

    public static string FormatEntryPreview(Entry? entry)
    {
        if (entry is null) return string.Empty;
        return string.IsNullOrWhiteSpace(entry.Preview) ? "Empty entry" : entry.Preview;
    }

    public static double EntryPreviewOpacity(Entry? entry)
        => entry is null || string.IsNullOrWhiteSpace(entry.Preview) ? 0.55 : 1.0;

    // Editor placeholder: 1 (fully visible) when the entry body (after the leading
    // "\n\n" padding) is still empty, 0 once the user types. TextBox.PlaceholderText
    // doesn't work here because the underlying Text always contains the padding,
    // so we overlay a TextBlock and drive its opacity from this function. The
    // shimmer pulse on the brush controls the visible intensity within that.
    public static double PlaceholderOpacity(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 1.0;
        return string.IsNullOrWhiteSpace(text) ? 1.0 : 0.0;
    }

    public static string ThemeGlyph(Microsoft.UI.Xaml.ElementTheme theme)
        => theme == Microsoft.UI.Xaml.ElementTheme.Dark ? "☀" : "☾";

    public static string SidebarToggleGlyph(bool isOpen) => "≡";
}
