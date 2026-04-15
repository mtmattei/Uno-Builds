using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IClimateService
{
    ValueTask<ClimateState> GetCurrentState(CancellationToken ct);
}
