using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IAutopilotService
{
    ValueTask<AutopilotState> GetCurrentState(CancellationToken ct);
}
