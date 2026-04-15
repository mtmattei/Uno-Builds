namespace Orbital.Services;

public interface IAgentService
{
    ValueTask<ImmutableList<AgentSession>> GetSessionsAsync(CancellationToken ct);
    ValueTask<AgentSession?> GetActiveSessionAsync(CancellationToken ct);
    ValueTask CreateSessionAsync(CancellationToken ct = default);
    ValueTask ReplayAsync(string sessionId, CancellationToken ct = default);
}
