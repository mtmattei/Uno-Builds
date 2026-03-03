using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace AdTokensIDE.Converters;

public class BoolToMessageBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isUser)
        {
            return isUser
                ? Application.Current.Resources["PrimaryBrush"] as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Purple)
                : Application.Current.Resources["SurfaceVariantBrush"] as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }
        return Application.Current.Resources["SurfaceVariantBrush"] as Brush ?? new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
