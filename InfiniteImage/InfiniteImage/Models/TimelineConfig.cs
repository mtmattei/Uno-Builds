namespace InfiniteImage.Models;

public static class TimelineConfig
{
    public const float UnitsPerDay = 10f;

    public static float CalculateZForDate(DateTimeOffset date, DateTimeOffset earliestDate)
    {
        return (float)(date - earliestDate).TotalDays * UnitsPerDay;
    }

    public static DateTimeOffset CalculateDateForZ(float z, DateTimeOffset earliestDate)
    {
        return earliestDate.AddDays(z / UnitsPerDay);
    }
}
