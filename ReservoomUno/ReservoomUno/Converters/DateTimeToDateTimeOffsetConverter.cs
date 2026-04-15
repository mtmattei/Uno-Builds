using Microsoft.UI.Xaml.Data;

namespace ReservoomUno.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return new DateTimeOffset(dt);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
        {
            return dto.DateTime;
        }
        return value;
    }
}
