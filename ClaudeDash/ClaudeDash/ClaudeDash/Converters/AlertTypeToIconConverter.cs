using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class AlertTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AlertType type) return "\uE946"; // info
        return type switch
        {
            AlertType.Warning => "\uE7BA",  // Warning icon
            AlertType.Error => "\uEA39",    // Error icon
            AlertType.Info => "\uE946",     // Info icon
            _ => "\uE946"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
