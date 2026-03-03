namespace HockeyBarn.Converters;

using HockeyBarn.Models;
using Microsoft.UI.Xaml.Data;

public class SkillLevelColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SkillLevel skillLevel)
        {
            return skillLevel switch
            {
                SkillLevel.Beginner => Application.Current.Resources["TertiaryContainerColor"],
                SkillLevel.Intermediate => Application.Current.Resources["PrimaryContainerColor"],
                SkillLevel.Advanced => Application.Current.Resources["SecondaryContainerColor"],
                _ => Application.Current.Resources["SurfaceVariantColor"]
            };
        }
        return Application.Current.Resources["SurfaceVariantColor"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class PaymentStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Paid => Application.Current.Resources["TertiaryContainerColor"],
                PaymentStatus.Unpaid => Application.Current.Resources["ErrorContainerColor"],
                PaymentStatus.PayAtRink => Application.Current.Resources["SecondaryContainerColor"],
                _ => Application.Current.Resources["SurfaceVariantColor"]
            };
        }
        return Application.Current.Resources["SurfaceVariantColor"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NotificationTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Alert => Application.Current.Resources["ErrorContainerColor"],
                NotificationType.GameInvite => Application.Current.Resources["PrimaryContainerColor"],
                NotificationType.Confirmation => Application.Current.Resources["TertiaryContainerColor"],
                NotificationType.Message => Application.Current.Resources["SecondaryContainerColor"],
                _ => Application.Current.Resources["SurfaceVariantColor"]
            };
        }
        return Application.Current.Resources["SurfaceVariantColor"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NotificationTypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Alert => "\uE7BA",
                NotificationType.GameInvite => "\uE8F2",
                NotificationType.Confirmation => "\uE73E",
                NotificationType.Message => "\uE8F2",
                _ => "\uEA8F"
            };
        }
        return "\uEA8F";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("dddd, MMMM dd - h:mm tt");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
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
