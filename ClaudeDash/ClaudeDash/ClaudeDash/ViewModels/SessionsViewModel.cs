using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record SessionsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<SessionsModel> _logger;

    public SessionsModel(IClaudeConfigService configService, ILogger<SessionsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IListFeed<ClaudeSessionInfo> Sessions => ListFeed.Async(async ct =>
    {
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(30);
            foreach (var s in sessions)
            {
                if (string.IsNullOrEmpty(s.RepoName) && !string.IsNullOrEmpty(s.ProjectPath))
                    s.RepoName = System.IO.Path.GetFileName(s.ProjectPath.TrimEnd('\\', '/'));
                if (string.IsNullOrEmpty(s.Model)) s.Model = "sonnet 4";
                if (string.IsNullOrEmpty(s.FirstUserMessage)) s.FirstUserMessage = $"Session {s.ShortId}";

                var recentThreshold = DateTime.UtcNow.AddMinutes(-15);
                if (s.LastActivity > recentThreshold)
                {
                    s.Status = "running";
                    var diff = DateTime.UtcNow - s.LastActivity;
                    s.Duration = diff.TotalMinutes < 1 ? "<1m" : $"{(int)diff.TotalMinutes}m";
                }
                else
                {
                    s.Status = string.IsNullOrEmpty(s.Status) ? "completed" : s.Status;
                    if (string.IsNullOrEmpty(s.Duration))
                    {
                        var diff = DateTime.UtcNow - s.LastActivity;
                        s.Duration = diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}m ago"
                            : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h ago"
                            : $"{(int)diff.TotalDays}d ago";
                    }
                }
            }
            return sessions.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load sessions");
            return ImmutableList<ClaudeSessionInfo>.Empty;
        }
    });

    public IFeed<SessionStats> Stats => Feed.Async(async ct =>
    {
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(30);
            var recentThreshold = DateTime.UtcNow.AddMinutes(-15);
            var activeCount = sessions.Count(s => s.LastActivity > recentThreshold);
            var todaySessions = sessions.Where(s => s.LastActivity.Date == DateTime.UtcNow.Date).ToList();
            var totalTokensToday = todaySessions.Sum(s => s.TokenCount);
            var avgMsgs = sessions.Count > 0 ? $"{Math.Max(sessions.Average(s => s.MessageCount), 1):F0} msgs" : "0 msgs";
            var costToday = $"${todaySessions.Sum(s => s.CostAmount):F2}";

            return new SessionStats(sessions.Count, activeCount, totalTokensToday, avgMsgs, costToday);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compute session stats");
            return new SessionStats();
        }
    });
}

public record SessionStats(
    int TotalSessions = 0,
    int ActiveCount = 0,
    int TotalTokensToday = 0,
    string AvgSessionLength = "0 msgs",
    string CostToday = "$0.00");
