using FluxTransit.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace FluxTransit.Converters;

/// <summary>
/// Converts RouteType enum to a SolidColorBrush.
/// </summary>
public class RouteTypeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is RouteType routeType)
        {
            return routeType switch
            {
                RouteType.Metro => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 129, 140, 248)), // #818cf8 - Primary indigo
                RouteType.Bus => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 251, 191, 36)),   // #fbbf24 - Warning amber
                RouteType.Train => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 211, 153)), // #34d399 - Success emerald
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 129, 140, 248))
            };
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 129, 140, 248));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts CrowdLevel enum to display text.
/// </summary>
public class CrowdLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CrowdLevel level)
        {
            return level switch
            {
                CrowdLevel.Low => "Low crowding",
                CrowdLevel.Moderate => "Moderate crowding",
                CrowdLevel.High => "High crowding",
                CrowdLevel.VeryHigh => "Very high crowding",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts NetworkHealth enum to display text.
/// </summary>
public class NetworkHealthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is NetworkHealth health)
        {
            return health switch
            {
                NetworkHealth.Normal => "NORMAL",
                NetworkHealth.MinorDelays => "MINOR DELAYS",
                NetworkHealth.MajorDelays => "MAJOR DELAYS",
                NetworkHealth.ServiceDisruption => "DISRUPTION",
                _ => "UNKNOWN"
            };
        }
        return "UNKNOWN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts bool to Visibility. Supports Inverse parameter.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isVisible = value is bool b && b;
        var inverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);

        if (inverse)
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts count (int) to Visibility. Visible if count > 0.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var count = value is int i ? i : 0;
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts NetworkHealth enum to a color brush.
/// </summary>
public class NetworkHealthColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is NetworkHealth health)
        {
            return health switch
            {
                NetworkHealth.Normal => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 211, 153)),     // #34d399 - Success
                NetworkHealth.MinorDelays => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 251, 191, 36)), // #fbbf24 - Warning
                NetworkHealth.MajorDelays => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 251, 113, 133)), // #fb7185 - Error
                NetworkHealth.ServiceDisruption => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 239, 68, 68)), // #ef4444 - Critical
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 148, 163, 184)) // #94a3b8 - Muted
            };
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 148, 163, 184));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts AlertSeverity enum to a color brush.
/// </summary>
public class AlertSeverityColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AlertSeverity severity)
        {
            return severity switch
            {
                AlertSeverity.Info => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 129, 140, 248)),      // #818cf8 - Primary
                AlertSeverity.Warning => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 251, 191, 36)),   // #fbbf24 - Warning
                AlertSeverity.Severe => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 251, 113, 133)),   // #fb7185 - Error
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 148, 163, 184))
            };
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 148, 163, 184));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
