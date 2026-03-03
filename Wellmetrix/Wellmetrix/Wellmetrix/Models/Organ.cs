namespace Wellmetrix.Models;

public partial record Organ(
    string Id,
    string Name,
    string Icon,
    string AccentColorKey,
    int HealthScore,
    string Status,
    IReadOnlyList<HealthMetric> Metrics,
    IReadOnlyList<Insight> Insights,
    IReadOnlyList<TrendDataPoint> WeeklyTrend
);

public partial record HealthMetric(
    string Id,
    string Label,
    string Value,
    string Unit,
    TrendDirection Trend,
    double ChangePercentage,
    double MinRange,
    double MaxRange,
    IReadOnlyList<double> SparklineData
);

public partial record Insight(
    string Id,
    string Message,
    InsightType Type
);

public record TrendDataPoint(
    string Day,
    double Value,
    double NormalizedHeight
);

public enum TrendDirection
{
    Up,
    Down,
    Stable
}

public enum InsightType
{
    Positive,
    Suggestion,
    Neutral,
    Warning
}

public enum HealthStatus
{
    Optimal,
    Good,
    Fair,
    Attention
}
