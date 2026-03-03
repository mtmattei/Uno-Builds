using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Worn.Converters;

public sealed class HexToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && hex.Length >= 6)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                var r = System.Convert.ToByte(hex[..2], 16);
                var g = System.Convert.ToByte(hex[2..4], 16);
                var b = System.Convert.ToByte(hex[4..6], 16);
                return new SolidColorBrush(Color.FromArgb(255, r, g, b));
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns one of two brushes based on a bool value.
/// Parameter format: "TrueResourceKey|FalseResourceKey"
/// </summary>
public sealed class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var isTrue = value is true;
        if (parameter is string keys)
        {
            var parts = keys.Split('|');
            if (parts.Length == 2)
            {
                var key = isTrue ? parts[0] : parts[1];
                // Use indexer which resolves ThemeDictionary resources correctly
                try
                {
                    if (Application.Current.Resources[key] is Brush brush)
                        return brush;
                }
                catch { /* key not found */ }
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns Visibility.Visible when the bool is true, Collapsed otherwise.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
