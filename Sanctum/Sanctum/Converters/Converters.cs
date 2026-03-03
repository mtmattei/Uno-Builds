using Microsoft.UI.Xaml.Data;
using Sanctum.Models;

namespace Sanctum.Converters;

public class BoolToVisibilityInverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ModeToCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AppMode mode && parameter is string modeString)
        {
            return mode.ToString().Equals(modeString, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SourceStatus status)
        {
            return status switch
            {
                SourceStatus.Allowed => Application.Current.Resources["AllowedBackgroundBrush"],
                SourceStatus.Batched => Application.Current.Resources["BatchedBackgroundBrush"],
                SourceStatus.Muted => Application.Current.Resources["MutedBackgroundBrush"],
                _ => Application.Current.Resources["MutedBackgroundBrush"]
            };
        }
        return Application.Current.Resources["MutedBackgroundBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SourceStatus status)
        {
            return status switch
            {
                SourceStatus.Allowed => Application.Current.Resources["AllowedForegroundBrush"],
                SourceStatus.Batched => Application.Current.Resources["BatchedForegroundBrush"],
                SourceStatus.Muted => Application.Current.Resources["MutedForegroundBrush"],
                _ => Application.Current.Resources["MutedForegroundBrush"]
            };
        }
        return Application.Current.Resources["MutedForegroundBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SourceStatus status)
        {
            return status.ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IntToTimeStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int minutes)
        {
            var hours = minutes / 60;
            var mins = minutes % 60;
            if (hours > 0)
            {
                return $"{hours}h {mins}m";
            }
            return $"{mins}m";
        }
        return "0m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
