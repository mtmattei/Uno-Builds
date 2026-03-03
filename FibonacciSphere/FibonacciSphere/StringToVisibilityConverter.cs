using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FibonacciSphere;

/// <summary>
/// Converts a string to Visibility. Returns Visible if the string is not null or empty,
/// otherwise returns Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
