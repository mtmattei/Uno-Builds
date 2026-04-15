using ClaudeDash.Models.Search;
using Microsoft.UI.Xaml.Data;

namespace ClaudeDash.Converters;

public class SearchResultTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SearchResultType type)
            return new SolidColorBrush(ColorHelper.FromArgb(255, 116, 116, 122)); // TextTertiary

        return type switch
        {
            SearchResultType.Session => new SolidColorBrush(ColorHelper.FromArgb(255, 74, 222, 128)),   // StatusGreen
            SearchResultType.Project => new SolidColorBrush(ColorHelper.FromArgb(255, 251, 191, 36)),   // StatusYellow
            SearchResultType.Repo => new SolidColorBrush(ColorHelper.FromArgb(255, 210, 210, 212)),     // TextSecondary
            SearchResultType.Skill => new SolidColorBrush(ColorHelper.FromArgb(255, 251, 146, 60)),     // StatusOrange
            SearchResultType.Agent => new SolidColorBrush(ColorHelper.FromArgb(255, 173, 173, 175)),    // AccentMid
            SearchResultType.McpServer => new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)),  // StatusRed
            SearchResultType.MemoryFile => new SolidColorBrush(ColorHelper.FromArgb(255, 228, 228, 229)), // AccentLight
            SearchResultType.Hook => new SolidColorBrush(ColorHelper.FromArgb(255, 116, 116, 122)),     // TextTertiary
            SearchResultType.Dependency => new SolidColorBrush(ColorHelper.FromArgb(255, 173, 173, 175)), // AccentMid
            SearchResultType.FileChange => new SolidColorBrush(ColorHelper.FromArgb(255, 74, 222, 128)), // StatusGreen
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 116, 116, 122))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public class SearchResultTypeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SearchResultType type)
            return "";

        return type switch
        {
            SearchResultType.Session => "SESSION",
            SearchResultType.Project => "PROJECT",
            SearchResultType.Repo => "REPO",
            SearchResultType.Skill => "SKILL",
            SearchResultType.Agent => "AGENT",
            SearchResultType.McpServer => "MCP",
            SearchResultType.MemoryFile => "MEMORY",
            SearchResultType.Hook => "HOOK",
            SearchResultType.Dependency => "DEP",
            SearchResultType.FileChange => "FILE",
            _ => ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public class SearchResultTypeToBadgeBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SearchResultType type)
            return new SolidColorBrush(ColorHelper.FromArgb(26, 116, 116, 122));

        return type switch
        {
            SearchResultType.Session => new SolidColorBrush(ColorHelper.FromArgb(26, 74, 222, 128)),
            SearchResultType.Project => new SolidColorBrush(ColorHelper.FromArgb(26, 251, 191, 36)),
            SearchResultType.Repo => new SolidColorBrush(ColorHelper.FromArgb(26, 210, 210, 212)),
            SearchResultType.Skill => new SolidColorBrush(ColorHelper.FromArgb(26, 251, 146, 60)),
            SearchResultType.Agent => new SolidColorBrush(ColorHelper.FromArgb(26, 173, 173, 175)),
            SearchResultType.McpServer => new SolidColorBrush(ColorHelper.FromArgb(26, 239, 68, 68)),
            SearchResultType.MemoryFile => new SolidColorBrush(ColorHelper.FromArgb(26, 228, 228, 229)),
            SearchResultType.Hook => new SolidColorBrush(ColorHelper.FromArgb(26, 116, 116, 122)),
            SearchResultType.Dependency => new SolidColorBrush(ColorHelper.FromArgb(26, 173, 173, 175)),
            SearchResultType.FileChange => new SolidColorBrush(ColorHelper.FromArgb(26, 74, 222, 128)),
            _ => new SolidColorBrush(ColorHelper.FromArgb(26, 116, 116, 122))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
