using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record AgentsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<AgentsModel> _logger;

    public AgentsModel(
        IClaudeConfigService configService,
        ILogger<AgentsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<AgentInfo> Agents => ListFeed.Async(async ct =>
    {
        try
        {
            var agents = await _configService.GetAgentsAsync();
            return agents.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load agents");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<AgentInfo>.Empty;
        }
    });

    public IFeed<int> TotalAgents => Feed.Async(async ct =>
    {
        var agents = await Agents;
        return agents?.Count ?? 0;
    });
}
