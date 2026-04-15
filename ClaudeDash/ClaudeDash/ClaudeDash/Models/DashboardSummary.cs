namespace ClaudeDash.Models;

public record DashboardSummary(
    int ActiveSessions = 0,
    int TrackedRepos = 0,
    int McpServers = 0,
    int HygieneIssues = 0);
