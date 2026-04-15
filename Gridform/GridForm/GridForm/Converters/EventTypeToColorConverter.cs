using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GridForm.Converters;

public class EventTypeToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is not ActivityEventType eventType) return GetBrush("SurfaceTintBrush");
		var key = eventType switch
		{
			ActivityEventType.Approval => "StringBrush",          // green
			ActivityEventType.Submission => "TertiaryBrush",      // blue
			ActivityEventType.Escalation => "PrimaryBrush",       // amber
			ActivityEventType.System => "SurfaceTintBrush",       // dim
			ActivityEventType.Delivery => "FunctionBrush",        // gold
			_ => "SurfaceTintBrush"
		};
		return GetBrush(key);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();

	private static object GetBrush(string key)
		=> Application.Current.Resources.TryGetValue(key, out var brush) ? brush : new SolidColorBrush();
}
