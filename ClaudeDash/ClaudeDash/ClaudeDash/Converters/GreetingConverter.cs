using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class GreetingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
