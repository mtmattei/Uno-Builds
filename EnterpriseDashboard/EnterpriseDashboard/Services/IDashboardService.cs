using EnterpriseDashboard.Models;

namespace EnterpriseDashboard.Services;

public interface IDashboardService
{
    ValueTask<KpiSummary> GetKpiAsync(CancellationToken ct);
    ValueTask<IImmutableList<SalesRecord>> GetSalesAsync(CancellationToken ct);
    ValueTask<IImmutableList<RegionMetric>> GetRegionsAsync(CancellationToken ct);
    ValueTask<IImmutableList<ChartDataPoint>> GetRevenueSeriesAsync(CancellationToken ct);
    ValueTask<IImmutableList<ChartDataPoint>> GetRegionRevenueBreakdownAsync(CancellationToken ct);
}
