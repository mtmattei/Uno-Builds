using EnterpriseDashboard.Models;
using EnterpriseDashboard.Services;

namespace EnterpriseDashboard.Models;

public partial record DashboardModel(IDashboardService DashboardService)
{
    public IFeed<KpiSummary> Kpi => Feed.Async(DashboardService.GetKpiAsync);

    public IListFeed<SalesRecord> Sales => ListFeed.Async(DashboardService.GetSalesAsync);

    public IListFeed<RegionMetric> Regions => ListFeed.Async(DashboardService.GetRegionsAsync);

    public IListFeed<ChartDataPoint> RevenueSeries => ListFeed.Async(DashboardService.GetRevenueSeriesAsync);

    public IListFeed<ChartDataPoint> RegionBreakdown => ListFeed.Async(DashboardService.GetRegionRevenueBreakdownAsync);
}
