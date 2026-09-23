using System.Globalization;
using FieldCheck.Models;

namespace FieldCheck.ViewModels;

/// <summary>Display formatting shared by view models. English-only app; invariant month names are English.</summary>
public static class Formats
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Date(DateOnly date) => date.ToString("MMM d, yyyy", Culture);

    public static string Date(DateTime date) => date.ToString("MMM d, yyyy", Culture);

    /// <summary>"Today, 2:41 PM", "Yesterday, 8:15 AM", "Sep 18, 9:24 AM" or "Sep 18, 2025, 9:24 AM".</summary>
    public static string Timestamp(DateTime value, DateTime now)
    {
        var time = value.ToString("h:mm tt", Culture);
        if (value.Date == now.Date)
        {
            return $"Today, {time}";
        }

        if (value.Date == now.Date.AddDays(-1))
        {
            return $"Yesterday, {time}";
        }

        return value.Year == now.Year
            ? $"{value.ToString("MMM d", Culture)}, {time}"
            : $"{value.ToString("MMM d, yyyy", Culture)}, {time}";
    }

    public static string Greeting(DateTime now) => now.Hour switch
    {
        < 12 => "Good morning",
        < 18 => "Good afternoon",
        _ => "Good evening",
    };

    public static string FacilityLine(DateTime now) => $"Facility A · {now.ToString("dddd, MMM d", Culture)}";

    public static string Count(int count, string singular, string plural) =>
        $"{count} {(count == 1 ? singular : plural)}";

    public static string ConditionTitle(InspectionCondition condition) => $"{condition} condition";
}
