using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GridForm.Converters;

public class SlaToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is DateTimeOffset deadline)
		{
			var remaining = deadline - DateTimeOffset.Now;
			var key = remaining.TotalHours < 24 ? "StatusFlaggedBrush"
				: remaining.TotalHours < 48 ? "StatusPendingBrush"
				: "SurfaceTintBrush";
			return GetBrush(key);
		}
		return GetBrush("SurfaceTintBrush");
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();

	private static object GetBrush(string key)
		=> Application.Current.Resources.TryGetValue(key, out var brush) ? brush : new SolidColorBrush();
}
