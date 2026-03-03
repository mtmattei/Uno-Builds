using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AgentNotifier.Converters;

public class AudioStateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool enabled)
            return enabled ? "ON" : "OFF";
        return "OFF";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class AudioColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool enabled)
        {
            return enabled
                ? new SolidColorBrush(Color.FromArgb(255, 0, 255, 102))  // Green
                : new SolidColorBrush(Color.FromArgb(255, 255, 68, 102)); // Red
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
