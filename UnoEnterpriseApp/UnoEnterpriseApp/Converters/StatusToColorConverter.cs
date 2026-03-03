using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using UnoEnterpriseApp.Models;

namespace UnoEnterpriseApp.Converters;

public class StatusToBackgroundColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CustomerStatus status)
        {
            var resourceKey = status switch
            {
                CustomerStatus.Active => "SuccessBackgroundBrush",
                CustomerStatus.Inactive => "InactiveBackgroundBrush",
                CustomerStatus.Pending => "WarningBackgroundBrush",
                CustomerStatus.Suspended => "ErrorBackgroundBrush",
                _ => null
            };
            
            if (resourceKey != null && Application.Current.Resources.TryGetValue(resourceKey, out var brush))
            {
                return brush;
            }
        }
        return Application.Current.Resources["InactiveBackgroundBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StatusToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CustomerStatus status)
        {
            var resourceKey = status switch
            {
                CustomerStatus.Active => "SuccessTextBrush",
                CustomerStatus.Inactive => "InactiveTextBrush",
                CustomerStatus.Pending => "WarningTextBrush",
                CustomerStatus.Suspended => "ErrorTextBrush",
                _ => null
            };
            
            if (resourceKey != null && Application.Current.Resources.TryGetValue(resourceKey, out var brush))
            {
                return brush;
            }
        }
        return Application.Current.Resources["TextPrimaryBrush"] as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            var invert = parameter as string == "Invert";
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

public class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum enumValue)
        {
            return System.Convert.ToInt32(enumValue);
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue && parameter is Type enumType)
        {
            return Enum.ToObject(enumType, intValue);
        }
        return null!;
    }
}
