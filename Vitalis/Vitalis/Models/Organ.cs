namespace Vitalis.Models;

public enum TrendDirection { Up, Down, Stable }
public enum HealthStatus { Optimal, Warning, Critical }

public record Metric(
    string Label,
    string Value,
    string Unit,
    TrendDirection Trend,
    HealthStatus Status
);

public record HistoryPoint(string Time, double Value);

public partial record Organ(
    string Id,
    string Name,
    string Description,
    string Color,
    string Icon,
    IImmutableList<Metric> Metrics,
    IImmutableList<HistoryPoint> History
);

public record AIInsight(
    string Summary,
    IImmutableList<string> Recommendations
);
