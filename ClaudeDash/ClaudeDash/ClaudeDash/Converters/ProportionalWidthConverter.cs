using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class ProportionalWidthConverter : IValueConverter
{
    public double MaxWidth { get; set; } = 200;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not double amount) return 0d;
        if (parameter is double max && max > 0)
            return (amount / max) * MaxWidth;
        return amount;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
