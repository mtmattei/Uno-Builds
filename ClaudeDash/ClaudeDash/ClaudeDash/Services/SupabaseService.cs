using ClaudeDash.Helpers;
using Supabase;

namespace ClaudeDash.Services;

/// <summary>
/// Supabase-backed data service. Falls back to mock data when Supabase URL/Key are not configured.
///
/// Expected Supabase schema:
///
///   -- dashboard_summary: single-row aggregate table
///   CREATE TABLE dashboard_summary (
///     id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     active_sessions  int NOT NULL DEFAULT 0,
///     tracked_repos    int NOT NULL DEFAULT 0,
///     mcp_servers      int NOT NULL DEFAULT 0,
///     hygiene_issues   int NOT NULL DEFAULT 0,
///     updated_at       timestamptz NOT NULL DEFAULT now()
///   );
///
///   -- alerts
///   CREATE TABLE alerts (
///     id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     type             text NOT NULL CHECK (type IN ('Warning','Error','Info')),
///     message          text NOT NULL,
///     navigation_target text NOT NULL DEFAULT '',
///     created_at       timestamptz NOT NULL DEFAULT now()
///   );
///
///   -- activity_data: one row per day
///   CREATE TABLE activity_data (
///     id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     label       text NOT NULL,
///     value       double precision NOT NULL DEFAULT 0,
///     max_value   double precision NOT NULL DEFAULT 100,
///     tooltip     text NOT NULL DEFAULT '',
///     recorded_at date NOT NULL DEFAULT CURRENT_DATE
///   );
///
///   -- hourly_activity: one row per hour slot
///   CREATE TABLE hourly_activity (
///     id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     hour_label  text NOT NULL,
///     value       double precision NOT NULL DEFAULT 0,
///     max_value   double precision NOT NULL DEFAULT 100,
///     tooltip     text NOT NULL DEFAULT '',
///     recorded_at timestamptz NOT NULL DEFAULT now()
///   );
///
///   -- model_costs
///   CREATE TABLE model_costs (
///     id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     model_name text NOT NULL,
///     amount     double precision NOT NULL DEFAULT 0,
///     max_amount double precision NOT NULL DEFAULT 0,
///     color      text NOT NULL DEFAULT '#6B7280',
///     updated_at timestamptz NOT NULL DEFAULT now()
///   );
///
///   -- recent_sessions
///   CREATE TABLE recent_sessions (
///     id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
///     status      text NOT NULL DEFAULT 'completed',
///     description text NOT NULL DEFAULT '',
///     repo_name   text NOT NULL DEFAULT '',
///     timestamp   timestamptz NOT NULL DEFAULT now()
///   );
/// </summary>
public class SupabaseService : ISupabaseService
{
    private readonly ILogger<SupabaseService> _logger;
    private readonly IOptions<SupabaseConfig> _config;
    private Supabase.Client? _client;
    private bool _useMock;

    public event Action? OnDataChanged;

    public SupabaseService(ILogger<SupabaseService> logger, IOptions<SupabaseConfig> config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        var cfg = _config.Value;
        if (string.IsNullOrWhiteSpace(cfg.Url) || string.IsNullOrWhiteSpace(cfg.AnonKey))
        {
            _logger.LogInformation("Supabase URL/Key not configured - running in mock mode");
            _useMock = true;
            return;
        }

        try
        {
            _client = new Supabase.Client(cfg.Url, cfg.AnonKey);
            await _client.InitializeAsync();
            _useMock = false;
            _logger.LogInformation("Supabase client initialized: {Url}", cfg.Url);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize Supabase client - falling back to mock mode");
            _useMock = true;
            _client = null;
        }
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        if (_useMock || _client is null)
            return MockDashboardSummary();

        try
        {
            var response = await _client.From<SupabaseDashboardSummary>()
                .Order("updated_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            var row = response.Models.FirstOrDefault();
            if (row is null) return MockDashboardSummary();

            return new DashboardSummary
            {
                ActiveSessions = row.ActiveSessions,
                TrackedRepos = row.TrackedRepos,
                McpServers = row.McpServers,
                HygieneIssues = row.HygieneIssues
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for dashboard_summary");
            return MockDashboardSummary();
        }
    }

    public async Task<List<AlertItem>> GetAlertsAsync()
    {
        if (_useMock || _client is null)
            return MockAlerts();

        try
        {
            var response = await _client.From<SupabaseAlert>()
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(10)
                .Get();

            return response.Models.Select(a => new AlertItem
            {
                Type = Enum.TryParse<AlertType>(a.Type, true, out var t) ? t : AlertType.Info,
                Message = a.Message,
                NavigationTarget = a.NavigationTarget
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for alerts");
            return MockAlerts();
        }
    }

    public async Task<List<ActivityDataPoint>> GetActivityDataAsync(int days = 30)
    {
        if (_useMock || _client is null)
            return MockActivityData(days);

        try
        {
            var since = DateTimeOffset.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
            var response = await _client.From<SupabaseActivityData>()
                .Filter("recorded_at", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, since)
                .Order("recorded_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models.Select(a => new ActivityDataPoint
            {
                Label = a.Label,
                Value = a.Value,
                MaxValue = a.MaxValue,
                TooltipText = a.Tooltip
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for activity_data");
            return MockActivityData(days);
        }
    }

    public async Task<List<HourlyActivity>> GetHourlyActivityAsync()
    {
        if (_useMock || _client is null)
            return MockHourlyActivity();

        try
        {
            var response = await _client.From<SupabaseHourlyActivity>()
                .Order("recorded_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(24)
                .Get();

            return response.Models.Select(h => new HourlyActivity
            {
                HourLabel = h.HourLabel,
                Value = h.Value,
                MaxValue = h.MaxValue,
                TooltipText = h.Tooltip
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for hourly_activity");
            return MockHourlyActivity();
        }
    }

    public async Task<List<ModelCost>> GetModelCostsAsync()
    {
        if (_useMock || _client is null)
            return MockModelCosts();

        try
        {
            var response = await _client.From<SupabaseModelCost>()
                .Order("amount", Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models.Select(m => new ModelCost
            {
                ModelName = m.ModelName,
                Amount = m.Amount,
                MaxAmount = m.MaxAmount,
                Color = m.Color
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for model_costs");
            return MockModelCosts();
        }
    }

    public async Task<List<SessionItem>> GetRecentSessionsAsync(int count = 5)
    {
        if (_useMock || _client is null)
            return MockRecentSessions();

        try
        {
            var response = await _client.From<SupabaseRecentSession>()
                .Order("timestamp", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(count)
                .Get();

            return response.Models.Select(s => new SessionItem
            {
                Status = s.Status,
                Description = s.Description,
                RepoName = s.RepoName,
                Timestamp = s.Timestamp
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase query failed for recent_sessions");
            return MockRecentSessions();
        }
    }

    // ---- Mock data fallbacks (unchanged from original) ----

    private static DashboardSummary MockDashboardSummary() => new()
    {
        ActiveSessions = 3,
        TrackedRepos = 12,
        McpServers = 7,
        HygieneIssues = 4
    };

    private static List<AlertItem> MockAlerts() => new()
    {
        new() { Type = AlertType.Warning, Message = "2 repos have uncommitted changes older than 24h", NavigationTarget = "repos" },
        new() { Type = AlertType.Error, Message = "MCP server 'memory' is unresponsive", NavigationTarget = "mcp-health" },
        new() { Type = AlertType.Info, Message = "4 dependency updates available across 3 repos", NavigationTarget = "deps" },
    };

    private static List<ActivityDataPoint> MockActivityData(int days)
    {
        var rng = new Random(42);
        return Enumerable.Range(0, days).Select(i => new ActivityDataPoint
        {
            Label = DateTimeOffset.Now.AddDays(-days + i + 1).ToString("MMM d"),
            Value = rng.Next(5, 95),
            MaxValue = 100,
            TooltipText = $"{rng.Next(5, 95)} actions"
        }).ToList();
    }

    private static List<HourlyActivity> MockHourlyActivity()
    {
        var rng = new Random(7);
        return Enumerable.Range(0, 24).Select(h => new HourlyActivity
        {
            HourLabel = h == 0 ? "12a" : h < 12 ? $"{h}a" : h == 12 ? "12p" : $"{h - 12}p",
            Value = h >= 9 && h <= 22 ? rng.Next(20, 100) : rng.Next(0, 15),
            MaxValue = 100,
            TooltipText = $"{(h >= 9 && h <= 22 ? rng.Next(20, 100) : rng.Next(0, 15))} actions"
        }).ToList();
    }

    private static List<ModelCost> MockModelCosts() => new()
    {
        new() { ModelName = "Claude Opus 4", Amount = 14.32, MaxAmount = 14.32, Color = "#A78BFA" },
        new() { ModelName = "Claude Sonnet 4", Amount = 8.71, MaxAmount = 14.32, Color = "#4A9EF5" },
        new() { ModelName = "Claude Haiku 3.5", Amount = 2.15, MaxAmount = 14.32, Color = "#2DD4BF" },
        new() { ModelName = "Other", Amount = 0.84, MaxAmount = 14.32, Color = "#6B7280" },
    };

    private static List<SessionItem> MockRecentSessions() => new()
    {
        new() { Status = "active", Description = "Refactoring auth middleware", RepoName = "api-server", Timestamp = DateTimeOffset.Now.AddMinutes(-12) },
        new() { Status = "completed", Description = "Fixed pagination bug in dashboard", RepoName = "web-app", Timestamp = DateTimeOffset.Now.AddHours(-2) },
        new() { Status = "completed", Description = "Added unit tests for user service", RepoName = "api-server", Timestamp = DateTimeOffset.Now.AddHours(-5) },
        new() { Status = "error", Description = "Deploy script failed on staging", RepoName = "infra", Timestamp = DateTimeOffset.Now.AddHours(-8) },
        new() { Status = "completed", Description = "Updated README with new API docs", RepoName = "docs", Timestamp = DateTimeOffset.Now.AddDays(-1) },
    };
}

// ---- Supabase table models (Postgrest DTOs) ----
// These map 1:1 to the Supabase tables defined in the schema above.

[Supabase.Postgrest.Attributes.Table("dashboard_summary")]
public class SupabaseDashboardSummary : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("active_sessions")]
    public int ActiveSessions { get; set; }

    [Supabase.Postgrest.Attributes.Column("tracked_repos")]
    public int TrackedRepos { get; set; }

    [Supabase.Postgrest.Attributes.Column("mcp_servers")]
    public int McpServers { get; set; }

    [Supabase.Postgrest.Attributes.Column("hygiene_issues")]
    public int HygieneIssues { get; set; }
}

[Supabase.Postgrest.Attributes.Table("alerts")]
public class SupabaseAlert : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("type")]
    public string Type { get; set; } = "Info";

    [Supabase.Postgrest.Attributes.Column("message")]
    public string Message { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("navigation_target")]
    public string NavigationTarget { get; set; } = string.Empty;
}

[Supabase.Postgrest.Attributes.Table("activity_data")]
public class SupabaseActivityData : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("label")]
    public string Label { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("value")]
    public double Value { get; set; }

    [Supabase.Postgrest.Attributes.Column("max_value")]
    public double MaxValue { get; set; } = 100;

    [Supabase.Postgrest.Attributes.Column("tooltip")]
    public string Tooltip { get; set; } = string.Empty;
}

[Supabase.Postgrest.Attributes.Table("hourly_activity")]
public class SupabaseHourlyActivity : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("hour_label")]
    public string HourLabel { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("value")]
    public double Value { get; set; }

    [Supabase.Postgrest.Attributes.Column("max_value")]
    public double MaxValue { get; set; } = 100;

    [Supabase.Postgrest.Attributes.Column("tooltip")]
    public string Tooltip { get; set; } = string.Empty;
}

[Supabase.Postgrest.Attributes.Table("model_costs")]
public class SupabaseModelCost : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("model_name")]
    public string ModelName { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("amount")]
    public double Amount { get; set; }

    [Supabase.Postgrest.Attributes.Column("max_amount")]
    public double MaxAmount { get; set; }

    [Supabase.Postgrest.Attributes.Column("color")]
    public string Color { get; set; } = "#6B7280";
}

[Supabase.Postgrest.Attributes.Table("recent_sessions")]
public class SupabaseRecentSession : Supabase.Postgrest.Models.BaseModel
{
    [Supabase.Postgrest.Attributes.PrimaryKey("id")]
    [Supabase.Postgrest.Attributes.Column("id")]
    public string Id { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("status")]
    public string Status { get; set; } = "completed";

    [Supabase.Postgrest.Attributes.Column("description")]
    public string Description { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("repo_name")]
    public string RepoName { get; set; } = string.Empty;

    [Supabase.Postgrest.Attributes.Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
