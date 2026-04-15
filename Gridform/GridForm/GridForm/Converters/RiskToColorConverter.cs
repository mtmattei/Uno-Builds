using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GridForm.Converters;

public class RiskToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is not RiskLevel risk) return GetBrush("OnSurfaceVariantBrush");
		var key = risk switch
		{
			RiskLevel.Low => "RiskLowBrush",
			RiskLevel.Medium => "RiskMediumBrush",
			RiskLevel.High => "RiskHighBrush",
			_ => "OnSurfaceVariantBrush"
		};
		return GetBrush(key);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();

	private static object GetBrush(string key)
		=> Application.Current.Resources.TryGetValue(key, out var brush) ? brush : new SolidColorBrush();
}
