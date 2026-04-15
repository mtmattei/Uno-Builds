using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record TerminalModel
{
    private readonly IAgentLauncherService _launcherService;
    private readonly ILogger<TerminalModel> _logger;

    public TerminalModel(
        IAgentLauncherService launcherService,
        ILogger<TerminalModel> logger)
    {
        _launcherService = launcherService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListState<RunningAgent> Agents => ListState.Async(this, async ct =>
    {
        try
        {
            var agents = await _launcherService.GetRunningAgentsAsync();
            return agents.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load running agents");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<RunningAgent>.Empty;
        }
    });

    public IFeed<int> RunningCount => Feed.Async(async ct =>
    {
        var agents = await Agents;
        return agents?.Count ?? 0;
    });

    public async ValueTask Launch(string projectPath, CancellationToken ct)
    {
        try
        {
            await _launcherService.LaunchAgentAsync(projectPath);
            // Refresh the agents list
            var agents = await _launcherService.GetRunningAgentsAsync();
            await Agents.UpdateAsync(_ => agents.ToImmutableList(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to launch agent for {ProjectPath}", projectPath);
            await ErrorMessage.Set(ex.Message, ct);
        }
    }

    public async ValueTask Stop(int processId, CancellationToken ct)
    {
        try
        {
            await _launcherService.StopAgentAsync(processId);
            // Refresh the agents list
            var agents = await _launcherService.GetRunningAgentsAsync();
            await Agents.UpdateAsync(_ => agents.ToImmutableList(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stop agent with PID {ProcessId}", processId);
            await ErrorMessage.Set(ex.Message, ct);
        }
    }
}
