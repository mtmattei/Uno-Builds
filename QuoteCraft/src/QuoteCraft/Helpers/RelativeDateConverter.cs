using Microsoft.UI.Xaml.Data;

namespace QuoteCraft.Helpers;

public class RelativeDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        DateTimeOffset date;
        if (value is DateTimeOffset dto)
            date = dto;
        else if (value is string dateStr && DateTimeOffset.TryParse(dateStr, out date))
        { }
        else
            return string.Empty;

        var now = DateTimeOffset.UtcNow;
        var diff = now - date;

        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24 && date.Date == now.Date) return "Today";
        if (diff.TotalHours < 48 && date.Date == now.Date.AddDays(-1)) return "Yesterday";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
        if (diff.TotalDays < 365) return $"{(int)(diff.TotalDays / 30)} months ago";

        return date.ToString("MMM d, yyyy");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
