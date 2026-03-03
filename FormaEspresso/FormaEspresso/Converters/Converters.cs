using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using FormaEspresso.Models;

namespace FormaEspresso.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var boolValue = value is bool b && b;
        var invert = parameter is string s && s == "Invert";
        return (boolValue != invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class IntensityToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intensity && parameter is string barStr && int.TryParse(barStr, out var barIndex))
        {
            return intensity >= barIndex ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class IntensityToBrushConverter : IValueConverter
{
    public Brush ActiveBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 180, 83, 9));
    public Brush InactiveBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 231, 229, 227));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intensity && parameter is string barStr && int.TryParse(barStr, out var barIndex))
        {
            return intensity >= barIndex ? ActiveBrush : InactiveBrush;
        }
        return InactiveBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class SelectedToBrushConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 28, 25, 23));
    public Brush UnselectedBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool isSelected && isSelected ? SelectedBrush : UnselectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class SelectedToForegroundConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    public Brush UnselectedBrush { get; set; } = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 28, 25, 23));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool isSelected && isSelected ? SelectedBrush : UnselectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class QuantitySelectedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int currentQty && parameter is string qtyStr && int.TryParse(qtyStr, out var qty))
        {
            return currentQty == qty;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StageToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AppStage stage && parameter is string stageStr)
        {
            return stage.ToString() == stageStr ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isNull = value is null;
        var invert = parameter is string s && s == "Invert";
        return (isNull != invert) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class EqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.Equals(parameter) ?? false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class ItemEqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is EspressoItem selected && parameter is EspressoItem item)
        {
            return selected.Id == item.Id;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class ProgressToHeightConverter : IValueConverter
{
    public double MaxHeight { get; set; } = 72;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double progress)
        {
            return (progress / 100.0) * MaxHeight;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
