using Microsoft.UI.Xaml.Data;
using Caffe.Models;

namespace Caffe.Converters;

public class TemperatureToFillHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int temp)
        {
            // Map 88-96 to 0-50 (track height)
            var normalized = (temp - 88) / 8.0;
            return normalized * 50;
        }
        return 25.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class ExtractionTimeToArcConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int time)
        {
            // Map 20-35 seconds to 0-240 degrees
            var normalized = (time - 20) / 15.0;
            return normalized * 240;
        }
        return 120.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? 1.0 : 0.35;
        }
        return 0.35;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class GrindLevelToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is GrindLevel level && parameter is string param && int.TryParse(param, out var target))
        {
            return (int)level == target;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class EspressoSelectionToBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is EspressoItem selected && parameter is EspressoItem item)
        {
            return selected == item ? 2.0 : 0.0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BrewProgressToHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double progress)
        {
            // Max fill is 65% of cup height (approx 52px of 80px)
            return progress * 52;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
