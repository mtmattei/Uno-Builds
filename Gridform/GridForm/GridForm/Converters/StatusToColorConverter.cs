using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GridForm.Converters;

public class StatusToColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is not OrderStatus status) return GetBrush("OnSurfaceVariantBrush");

		var useBackground = parameter is string p && p == "Background";
		var key = status switch
		{
			OrderStatus.Pending => useBackground ? "StatusPendingDimBrush" : "StatusPendingBrush",
			OrderStatus.Approved => useBackground ? "StatusApprovedDimBrush" : "StatusApprovedBrush",
			OrderStatus.InReview => useBackground ? "StatusReviewDimBrush" : "StatusReviewBrush",
			OrderStatus.Flagged => useBackground ? "StatusFlaggedDimBrush" : "StatusFlaggedBrush",
			_ => "OnSurfaceVariantBrush"
		};
		return GetBrush(key);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();

	private static object GetBrush(string key)
		=> Application.Current.Resources.TryGetValue(key, out var brush) ? brush : new SolidColorBrush();
}
