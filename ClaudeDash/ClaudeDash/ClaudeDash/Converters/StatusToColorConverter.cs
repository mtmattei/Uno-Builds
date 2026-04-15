using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var status = value as string ?? "";
        return status switch
        {
            "active" => new SolidColorBrush(ColorHelper.FromArgb(255, 74, 222, 128)),    // green
            "completed" => new SolidColorBrush(ColorHelper.FromArgb(255, 67, 67, 72)),   // dim
            "error" => new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)),      // red
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 67, 67, 72))              // dim
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
