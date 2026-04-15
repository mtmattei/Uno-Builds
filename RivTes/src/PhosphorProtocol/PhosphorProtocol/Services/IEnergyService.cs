using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IEnergyService
{
    ValueTask<EnergyState> GetCurrentState(CancellationToken ct);
}
