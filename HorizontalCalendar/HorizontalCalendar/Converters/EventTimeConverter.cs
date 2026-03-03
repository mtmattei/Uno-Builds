using System;
using Microsoft.UI.Xaml.Data;

namespace HorizontalCalendar.Converters;

/// <summary>
/// Converts event time information to a formatted display string.
/// </summary>
public class EventTimeConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DateTime dateTime)
            return string.Empty;

        return dateTime.ToString("h:mm tt");
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
