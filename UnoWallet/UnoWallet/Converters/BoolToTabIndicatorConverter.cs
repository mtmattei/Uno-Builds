using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace UnoWallet.Converters;

public class BoolToTabIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected && isSelected)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26)); // Dark/Black
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); // Transparent
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToTabIndicatorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected && !isSelected)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26)); // Dark/Black
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); // Transparent
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected && isSelected)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26)); // OnSurface
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 114, 128)); // OnSurfaceVariant
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected && !isSelected)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26)); // OnSurface
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 114, 128)); // OnSurfaceVariant
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
