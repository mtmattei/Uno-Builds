using AgentNotifier.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AgentNotifier.Converters;

public class StatusToColorConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, SolidColorBrush> _brushCache = new()
    {
        { AgentStatus.Working, new SolidColorBrush(Color.FromArgb(255, 255, 107, 157)) },  // Sugar Pink
        { AgentStatus.Waiting, new SolidColorBrush(Color.FromArgb(255, 102, 102, 112)) },  // Gray (queued)
        { AgentStatus.Finished, new SolidColorBrush(Color.FromArgb(255, 74, 222, 128)) },  // Mint Green
        { AgentStatus.Error, new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)) },      // Red
        { AgentStatus.Idle, new SolidColorBrush(Color.FromArgb(255, 102, 102, 112)) }      // Gray
    };
    private static readonly SolidColorBrush _defaultBrush = new(Color.FromArgb(255, 102, 102, 112));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AgentStatus status)
            return _defaultBrush;
        return _brushCache.TryGetValue(status, out var brush) ? brush : _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StatusToGlowColorConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, SolidColorBrush> _brushCache = new()
    {
        { AgentStatus.Working, new SolidColorBrush(Color.FromArgb(77, 255, 107, 157)) },   // Sugar Pink 30%
        { AgentStatus.Waiting, new SolidColorBrush(Color.FromArgb(77, 102, 102, 112)) },   // Gray 30%
        { AgentStatus.Finished, new SolidColorBrush(Color.FromArgb(77, 74, 222, 128)) },   // Mint 30%
        { AgentStatus.Error, new SolidColorBrush(Color.FromArgb(77, 239, 68, 68)) },       // Red 30%
        { AgentStatus.Idle, new SolidColorBrush(Color.FromArgb(26, 255, 255, 255)) }
    };
    private static readonly SolidColorBrush _defaultBrush = new(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AgentStatus status)
            return _defaultBrush;
        return _brushCache.TryGetValue(status, out var brush) ? brush : _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StatusToDarkColorConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, SolidColorBrush> _brushCache = new()
    {
        { AgentStatus.Working, new SolidColorBrush(Color.FromArgb(255, 204, 85, 128)) },   // Dark pink
        { AgentStatus.Waiting, new SolidColorBrush(Color.FromArgb(255, 68, 68, 73)) },     // Dark gray
        { AgentStatus.Finished, new SolidColorBrush(Color.FromArgb(255, 52, 156, 90)) },   // Dark green
        { AgentStatus.Error, new SolidColorBrush(Color.FromArgb(255, 168, 48, 48)) },      // Dark red
        { AgentStatus.Idle, new SolidColorBrush(Color.FromArgb(255, 37, 37, 41)) }         // Border default
    };
    private static readonly SolidColorBrush _defaultBrush = new(Color.FromArgb(255, 37, 37, 41));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AgentStatus status)
            return _defaultBrush;
        return _brushCache.TryGetValue(status, out var brush) ? brush : _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class WaitingIndicatorConverter : IValueConverter
{
    private static readonly SolidColorBrush _waitingBrush = new(Color.FromArgb(255, 255, 107, 157));
    private static readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isWaiting && isWaiting)
            return _waitingBrush;
        return _transparentBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class AgentCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
            return count.ToString();
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StatusToLabelConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, string> _labels = new()
    {
        { AgentStatus.Working, "PROC" },
        { AgentStatus.Waiting, "WAIT" },
        { AgentStatus.Finished, "DONE" },
        { AgentStatus.Error, "ERR!" },
        { AgentStatus.Idle, "IDLE" }
    };

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AgentStatus status && _labels.TryGetValue(status, out var label))
            return label;
        return "----";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StatusToBgColorConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, SolidColorBrush> _brushCache = new()
    {
        { AgentStatus.Working, new SolidColorBrush(Color.FromArgb(20, 255, 107, 157)) },   // Pink 8%
        { AgentStatus.Waiting, new SolidColorBrush(Color.FromArgb(20, 102, 102, 112)) },   // Gray 8%
        { AgentStatus.Finished, new SolidColorBrush(Color.FromArgb(20, 74, 222, 128)) },   // Green 8%
        { AgentStatus.Error, new SolidColorBrush(Color.FromArgb(20, 239, 68, 68)) },       // Red 8%
        { AgentStatus.Idle, new SolidColorBrush(Color.FromArgb(20, 102, 102, 112)) }
    };
    private static readonly SolidColorBrush _defaultBrush = new(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AgentStatus status)
            return _defaultBrush;
        return _brushCache.TryGetValue(status, out var brush) ? brush : _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class StatusToBorderColorConverter : IValueConverter
{
    private static readonly Dictionary<AgentStatus, SolidColorBrush> _brushCache = new()
    {
        { AgentStatus.Working, new SolidColorBrush(Color.FromArgb(64, 255, 107, 157)) },   // Pink 25%
        { AgentStatus.Waiting, new SolidColorBrush(Color.FromArgb(64, 102, 102, 112)) },   // Gray 25%
        { AgentStatus.Finished, new SolidColorBrush(Color.FromArgb(64, 74, 222, 128)) },   // Green 25%
        { AgentStatus.Error, new SolidColorBrush(Color.FromArgb(64, 239, 68, 68)) },       // Red 25%
        { AgentStatus.Idle, new SolidColorBrush(Color.FromArgb(64, 102, 102, 112)) }
    };
    private static readonly SolidColorBrush _defaultBrush = new(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not AgentStatus status)
            return _defaultBrush;
        return _brushCache.TryGetValue(status, out var brush) ? brush : _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
            return b ? 1.0 : 0.4;
        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToCardBorderConverter : IValueConverter
{
    private static readonly SolidColorBrush _expandedBrush = new(Color.FromArgb(255, 255, 107, 157));  // Accent
    private static readonly SolidColorBrush _defaultBrush = new(Color.FromArgb(255, 37, 37, 41));     // Border default

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isExpanded && isExpanded)
            return _expandedBrush;
        return _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
