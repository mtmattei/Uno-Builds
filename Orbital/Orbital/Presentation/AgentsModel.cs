using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record AgentsModel(IAgentService Agents)
{
    public IListFeed<AgentSession> Sessions => ListFeed.Async(Agents.GetSessionsAsync);
}
