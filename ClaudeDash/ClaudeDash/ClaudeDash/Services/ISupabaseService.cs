namespace ClaudeDash.Services;

public interface ISupabaseService
{
    Task InitializeAsync();
    Task<DashboardSummary> GetDashboardSummaryAsync();
    Task<List<AlertItem>> GetAlertsAsync();
    Task<List<ActivityDataPoint>> GetActivityDataAsync(int days = 30);
    Task<List<HourlyActivity>> GetHourlyActivityAsync();
    Task<List<ModelCost>> GetModelCostsAsync();
    Task<List<SessionItem>> GetRecentSessionsAsync(int count = 5);
    event Action? OnDataChanged;
}
