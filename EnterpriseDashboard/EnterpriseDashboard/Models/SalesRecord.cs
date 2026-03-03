namespace EnterpriseDashboard.Models;

public record SalesRecord(
    string Region,
    string Product,
    double Revenue,
    double Growth,
    DateTime Date);

public record RegionMetric(
    string Name,
    double Latitude,
    double Longitude,
    double Value);

public record KpiSummary(
    double TotalRevenue,
    double TotalOrders,
    double AvgOrderValue,
    double GrowthPercent);

public record ChartDataPoint(
    DateTime Date,
    double Value,
    string Category);
