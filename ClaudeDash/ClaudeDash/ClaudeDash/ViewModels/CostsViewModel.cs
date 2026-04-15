using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record CostsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<CostsModel> _logger;

    public CostsModel(IClaudeConfigService configService, ILogger<CostsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IFeed<CostSummary> Summary => Feed.Async(async ct =>
    {
        try
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var sessions = await _configService.GetRecentSessionsAsync(500);

            var thisMonth = sessions.Where(s => s.LastActivity >= monthStart).ToList();

            static double EstimateCost(ClaudeSessionInfo s)
            {
                var model = (s.Model ?? "").ToLowerInvariant();
                double rate = model.Contains("opus") ? 15.0 / 1_000_000
                    : model.Contains("haiku") ? 0.25 / 1_000_000
                    : 3.0 / 1_000_000;
                return s.TokenCount * rate;
            }

            var totalCost = thisMonth.Sum(EstimateCost);
            var totalTokens = thisMonth.Sum(s => s.TokenCount);

            var prevMonthStart = monthStart.AddMonths(-1);
            var lastMonthCost = sessions.Where(s => s.LastActivity >= prevMonthStart && s.LastActivity < monthStart).Sum(EstimateCost);

            string vsLastMonth;
            if (lastMonthCost > 0)
            {
                var pct = ((totalCost - lastMonthCost) / lastMonthCost) * 100;
                vsLastMonth = pct >= 0 ? $"+{pct:F0}%" : $"{pct:F0}%";
            }
            else
            {
                vsLastMonth = totalCost > 0 ? "new" : "--";
            }

            return new CostSummary(
                MonthLabel: now.ToString("MMMM yyyy").ToLowerInvariant(),
                MonthTotal: Math.Round(totalCost, 2),
                DailyAvg: now.Day > 0 ? Math.Round(totalCost / now.Day, 2) : 0,
                TotalTokens: totalTokens,
                TokensUsed: totalTokens >= 1_000_000 ? $"{totalTokens / 1_000_000.0:F2}M"
                    : totalTokens >= 1_000 ? $"{totalTokens / 1_000.0:F1}K" : totalTokens.ToString(),
                VsLastMonth: vsLastMonth);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load cost summary");
            return new CostSummary();
        }
    });

    public IListFeed<ModelCost> CostByModel => ListFeed.Async(async ct =>
    {
        var costs = await _configService.GetModelCostsFromSessionsAsync();
        return costs.ToImmutableList();
    });
}

public record CostSummary(
    string MonthLabel = "",
    double MonthTotal = 0,
    double DailyAvg = 0,
    int TotalTokens = 0,
    string TokensUsed = "0",
    string VsLastMonth = "--");
