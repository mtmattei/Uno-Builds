using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FreewriteUno.InlineAi.Presentation.Converters;

/// <summary>true → Visible, false → Collapsed. Pass ConverterParameter="Invert" to flip.</summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var flag = value is true;
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility.Visible;
}

/// <summary>true → dimmed opacity (0.45), false → full opacity. For settled (applied/discarded) result cards.</summary>
public sealed class BooleanToDimConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? 0.45 : 1.0;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

/// <summary>Non-empty string → Visible, otherwise Collapsed. Used for the transient toast (empty = no toast).</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hasValue = value is string s ? !string.IsNullOrEmpty(s) : value is not null;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
