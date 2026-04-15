using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class AlertTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AlertType type) return new SolidColorBrush(ColorHelper.FromArgb(255, 67, 67, 72));
        return type switch
        {
            AlertType.Warning => new SolidColorBrush(ColorHelper.FromArgb(255, 251, 191, 36)),  // yellow
            AlertType.Error => new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)),     // red
            AlertType.Info => new SolidColorBrush(ColorHelper.FromArgb(255, 67, 67, 72)),       // dim
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 67, 67, 72))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
