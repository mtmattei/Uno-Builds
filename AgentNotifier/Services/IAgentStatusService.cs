using AgentNotifier.Models;

namespace AgentNotifier.Services;

public interface IAgentStatusService
{
    event EventHandler<MultiAgentPayload>? AgentsChanged;
    IReadOnlyList<AgentInfo> Agents { get; }
    void Start();
    void Stop();
}
