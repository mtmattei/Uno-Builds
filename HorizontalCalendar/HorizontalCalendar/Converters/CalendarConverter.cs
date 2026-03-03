using System;
using System.Collections.Concurrent;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

namespace HorizontalCalendar.Converters;

/// <summary>
/// Universal converter for all calendar-related conversions.
/// Uses the ConverterParameter to determine conversion type.
/// </summary>
public class CalendarConverter : IValueConverter
{
    // Cached brushes to avoid repeated allocations
    private static readonly Brush SelectedBrush = new SolidColorBrush(Colors.DeepSkyBlue);
    private static readonly Brush SelectedFillBrush = new SolidColorBrush(ColorHelper.FromArgb(51, 0, 191, 255)); // 20% opacity DeepSkyBlue
    private static readonly Brush TodayBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 34, 197, 94)); // #22C55E
    private static readonly Brush TodayBackgroundBrush = new SolidColorBrush(ColorHelper.FromArgb(80, 34, 197, 94)); // Pastel green background for today
    private static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    private static readonly Brush WhiteBrush = new SolidColorBrush(Colors.White);
    private static readonly Brush HighlightedWhiteBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 248, 250, 252)); // Slightly highlighted white
    private static readonly Brush LightGrayBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 200, 200, 200));
    private static readonly Brush DefaultEventBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 59, 130, 246)); // #3B82F6

    // Cache for dynamically created color brushes
    private static readonly ConcurrentDictionary<string, Brush> ColorBrushCache = new();

    /// <summary>
    /// Safely parses a hex color string and returns a cached brush.
    /// </summary>
    public static Brush GetBrushFromHex(string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            return DefaultEventBrush;

        return ColorBrushCache.GetOrAdd(hexColor, hex =>
        {
            try
            {
                var color = ParseHexColor(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return DefaultEventBrush;
            }
        });
    }

    /// <summary>
    /// Parses a hex color string to a Windows.UI.Color.
    /// Supports formats: #RGB, #RRGGBB, #AARRGGBB
    /// </summary>
    private static Windows.UI.Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');

        return hex.Length switch
        {
            3 => Windows.UI.Color.FromArgb(255,
                System.Convert.ToByte(new string(hex[0], 2), 16),
                System.Convert.ToByte(new string(hex[1], 2), 16),
                System.Convert.ToByte(new string(hex[2], 2), 16)),
            6 => Windows.UI.Color.FromArgb(255,
                System.Convert.ToByte(hex.Substring(0, 2), 16),
                System.Convert.ToByte(hex.Substring(2, 2), 16),
                System.Convert.ToByte(hex.Substring(4, 2), 16)),
            8 => Windows.UI.Color.FromArgb(
                System.Convert.ToByte(hex.Substring(0, 2), 16),
                System.Convert.ToByte(hex.Substring(2, 2), 16),
                System.Convert.ToByte(hex.Substring(4, 2), 16),
                System.Convert.ToByte(hex.Substring(6, 2), 16)),
            _ => throw new ArgumentException($"Invalid hex color format: {hex}")
        };
    }

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var paramStr = parameter as string ?? string.Empty;
        
        // Handle date formatting (e.g., parameter="ddd" for day of week)
        if (value is DateTime date && !string.IsNullOrWhiteSpace(paramStr) && paramStr.Length <= 4)
        {
            var formatted = date.ToString(paramStr);
            // Return uppercase for day of week abbreviations
            return paramStr == "ddd" ? formatted.ToUpperInvariant() : formatted;
        }
        
        // Handle badge visibility for multiple events
        if (value is int badgeCount && paramStr == "MultipleBadges")
        {
            return badgeCount >= 2;
        }
        
        if (value is int badgeCount2 && paramStr == "ThreeBadges")
        {
            return badgeCount2 >= 3;
        }
        
        // Handle FontWeight conversion
        if (value is string weightStr && targetType == typeof(Windows.UI.Text.FontWeight))
        {
            return weightStr switch
            {
                "Bold" => new Windows.UI.Text.FontWeight { Weight = 700 },
                "SemiBold" => new Windows.UI.Text.FontWeight { Weight = 600 },
                _ => new Windows.UI.Text.FontWeight { Weight = 400 }
            };
        }
        
        // Handle brush conversions based on state string
        if (value is string state && targetType == typeof(Brush))
        {
            // Check if it's a hex color (starts with #)
            if (state.StartsWith("#"))
            {
                return GetBrushFromHex(state);
            }

            return state switch
            {
                "Selected" => SelectedBrush,
                "SelectedFill" => SelectedFillBrush,
                "Today" => TodayBrush,
                "TodayBackground" => TodayBackgroundBrush,
                "SelectedText" => WhiteBrush,
                "DefaultText" => HighlightedWhiteBrush, // Use highlighted white for better visibility
                _ => TransparentBrush
            };
        }

        // Handle time formatting for events
        if (value is DateTime time && paramStr == "EventTime")
        {
            return time.ToString("h:mm tt");
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
