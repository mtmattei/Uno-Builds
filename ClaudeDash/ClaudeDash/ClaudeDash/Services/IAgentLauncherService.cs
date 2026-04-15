namespace ClaudeDash.Services;

public interface IAgentLauncherService
{
    Task<List<RunningAgent>> GetRunningAgentsAsync();
    Task<int> LaunchAgentAsync(string projectPath, string? prompt = null, string? model = null);
    Task StopAgentAsync(int processId);
}
