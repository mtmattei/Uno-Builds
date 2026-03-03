using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MsnMessenger.Models;

namespace MsnMessenger.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PresenceStatus status)
        {
            // Neo-Y2K Design Spec Colors
            return status switch
            {
                PresenceStatus.Online => new SolidColorBrush(ColorHelper.FromArgb(255, 0, 200, 150)),   // #00c896 Teal
                PresenceStatus.Away => new SolidColorBrush(ColorHelper.FromArgb(255, 247, 183, 49)),    // #f7b731 Gold
                PresenceStatus.Busy => new SolidColorBrush(ColorHelper.FromArgb(255, 235, 59, 90)),     // #eb3b5a Red
                PresenceStatus.Offline => new SolidColorBrush(ColorHelper.FromArgb(255, 74, 74, 74)),   // #4a4a4a Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PresenceStatus status)
        {
            return status switch
            {
                PresenceStatus.Online => "Online",
                PresenceStatus.Away => "Away",
                PresenceStatus.Busy => "Busy",
                PresenceStatus.Offline => "Offline",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var boolValue = value is bool b && b;
        var invert = parameter?.ToString() == "Invert";
        return (boolValue != invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i)
            return i > 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class MessageAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSentByMe)
            return isSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        return HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class StatusToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PresenceStatus status)
            return status == PresenceStatus.Offline ? 0.5 : 1.0;
        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isInverted = parameter?.ToString() == "Invert";
        var isNotNull = value != null;
        return (isNotNull != isInverted) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
