using EnterpriseDashboard.Models;

namespace EnterpriseDashboard.Services;

public class MockDashboardService : IDashboardService
{
    private static readonly string[] Regions = ["North America", "Europe", "Asia Pacific", "Latin America", "Middle East", "Africa", "Oceania", "South Asia"];
    private static readonly string[] Products = ["Platform License", "Cloud Hosting", "Support Plan", "Analytics Suite", "API Gateway", "Data Pipeline"];

    private static readonly (string Name, double Lat, double Lng)[] RegionCoords =
    [
        ("New York", 40.7128, -74.0060),
        ("London", 51.5074, -0.1278),
        ("Tokyo", 35.6762, 139.6503),
        ("São Paulo", -23.5505, -46.6333),
        ("Dubai", 25.2048, 55.2708),
        ("Sydney", -33.8688, 151.2093),
        ("Singapore", 1.3521, 103.8198),
        ("Mumbai", 19.0760, 72.8777),
        ("Berlin", 52.5200, 13.4050),
        ("Toronto", 43.6532, -79.3832)
    ];

    private readonly Random _rng = new(42);

    public ValueTask<KpiSummary> GetKpiAsync(CancellationToken ct)
    {
        var sales = GenerateSales();
        var totalRevenue = sales.Sum(s => s.Revenue);
        var totalOrders = sales.Count;
        var avgOrderValue = totalRevenue / totalOrders;
        var avgGrowth = sales.Average(s => s.Growth);

        return ValueTask.FromResult(new KpiSummary(
            TotalRevenue: Math.Round(totalRevenue, 2),
            TotalOrders: totalOrders,
            AvgOrderValue: Math.Round(avgOrderValue, 2),
            GrowthPercent: Math.Round(avgGrowth, 1)));
    }

    public ValueTask<IImmutableList<SalesRecord>> GetSalesAsync(CancellationToken ct)
    {
        return ValueTask.FromResult<IImmutableList<SalesRecord>>(
            GenerateSales().ToImmutableList());
    }

    public ValueTask<IImmutableList<RegionMetric>> GetRegionsAsync(CancellationToken ct)
    {
        var metrics = RegionCoords.Select(r => new RegionMetric(
            Name: r.Name,
            Latitude: r.Lat,
            Longitude: r.Lng,
            Value: Math.Round(500_000 + _rng.NextDouble() * 4_500_000, 0)
        )).ToImmutableList();

        return ValueTask.FromResult<IImmutableList<RegionMetric>>(metrics);
    }

    public ValueTask<IImmutableList<ChartDataPoint>> GetRevenueSeriesAsync(CancellationToken ct)
    {
        var points = new List<ChartDataPoint>();
        var baseDate = new DateTime(2025, 1, 1);
        double runningValue = 120_000;

        for (int i = 0; i < 24; i++)
        {
            var date = baseDate.AddMonths(i);
            runningValue += (_rng.NextDouble() - 0.3) * 25_000;
            runningValue = Math.Max(80_000, runningValue);

            points.Add(new ChartDataPoint(date, Math.Round(runningValue, 0), "Revenue"));
        }

        return ValueTask.FromResult<IImmutableList<ChartDataPoint>>(points.ToImmutableList());
    }

    public ValueTask<IImmutableList<ChartDataPoint>> GetRegionRevenueBreakdownAsync(CancellationToken ct)
    {
        var breakdown = Regions.Select(r => new ChartDataPoint(
            Date: DateTime.Today,
            Value: Math.Round(200_000 + _rng.NextDouble() * 800_000, 0),
            Category: r
        )).ToImmutableList();

        return ValueTask.FromResult<IImmutableList<ChartDataPoint>>(breakdown);
    }

    private List<SalesRecord> GenerateSales()
    {
        var records = new List<SalesRecord>();
        var baseDate = new DateTime(2025, 6, 1);

        for (int i = 0; i < 50; i++)
        {
            records.Add(new SalesRecord(
                Region: Regions[_rng.Next(Regions.Length)],
                Product: Products[_rng.Next(Products.Length)],
                Revenue: Math.Round(5_000 + _rng.NextDouble() * 95_000, 2),
                Growth: Math.Round((_rng.NextDouble() - 0.3) * 40, 1),
                Date: baseDate.AddDays(-_rng.Next(180))
            ));
        }

        return records;
    }
}
