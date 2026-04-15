namespace ClaudeDash.Services;

public interface IClaudeConfigService
{
    Task<List<McpServerInfo>> GetMcpServersAsync();
    Task<List<ClaudeSessionInfo>> GetRecentSessionsAsync(int count = 20);
    Task<List<SkillInfo>> GetSkillsAsync();
    Task<List<MemoryFile>> GetMemoryFilesAsync();
    Task<List<RepoInfo>> GetReposAsync();
    Task<List<DependencyInfo>> GetDependenciesAsync(string? projectPath = null);
    Task<List<HookInfo>> GetHooksAsync();
    Task<List<AgentInfo>> GetAgentsAsync();
    Task<List<EnvCheckResult>> RunEnvAuditAsync();
    Task<UnoPlatformInfo> GetUnoPlatformInfoAsync();
    Task<GitStatusInfo> GetGitStatusAsync(string repoPath);
    Task<List<ActivityDataPoint>> GetActivityDataFromSessionsAsync(int days = 30);
    Task<List<HourlyActivity>> GetHourlyActivityFromSessionsAsync();
    Task<List<(DateTimeOffset Timestamp, int Value)>> GetRollingActivityAsync(int windowMinutes = 60, int bucketMinutes = 5);
    Task<List<ModelCost>> GetModelCostsFromSessionsAsync();
    Task<List<AlertItem>> GetAlertsFromEnvironmentAsync();
    Task<List<SessionItem>> GetRecentSessionItemsAsync(int count = 5);
    Task<List<Models.UnoCheckResult>> RunUnoCheckAsync();
    Task<Models.LicenseInfo> GetLicenseInfoAsync();
    string GetClaudeDir();
}
