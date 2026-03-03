using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SalesHeatmap.Converters;

public class IntensityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double intensity)
        {
            // Resolve theme-aware low/high intensity colors from application resources
            var resources = Application.Current.Resources;
            if (resources.TryGetValue("HeatmapLowIntensityColor", out var lowObj)
                && resources.TryGetValue("HeatmapHighIntensityColor", out var highObj)
                && lowObj is Color low && highObj is Color high)
            {
                var r = (byte)(low.R + (high.R - low.R) * intensity);
                var g = (byte)(low.G + (high.G - low.G) * intensity);
                var b = (byte)(low.B + (high.B - low.B) * intensity);
                return new SolidColorBrush(Color.FromArgb(255, r, g, b));
            }

            // Fallback: original dark-theme behavior
            var lightness = 12 + (intensity * 73);
            var grayValue = (byte)(lightness * 255 / 100);
            return new SolidColorBrush(Color.FromArgb(255, grayValue, grayValue, grayValue));
        }
        return new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class IntensityToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double intensity)
        {
            return 0.7 + (intensity * 0.3);
        }
        return 0.7;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return $"${(int)d:N0}";
        }
        return "$0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            var sign = d >= 0 ? "+" : "";
            return $"{sign}{d:F1}%";
        }
        return "+0.0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class PercentageToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            var key = d >= 0 ? "HeatmapPositiveChangeBrush" : "HeatmapNegativeChangeBrush";
            if (Application.Current.Resources.TryGetValue(key, out var brushObj) && brushObj is Brush brush)
            {
                return brush;
            }

            // Fallback
            return d >= 0
                ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                : new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        }
        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class NumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return $"{(int)d:N0}";
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class PercentageSuffixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var suffix = parameter as string ?? "";
        if (value is double d)
        {
            var sign = d >= 0 ? "+" : "";
            return $"{sign}{d:F1}% {suffix}";
        }
        return $"+0.0% {suffix}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BoolToAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? 180.0 : 0.0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
