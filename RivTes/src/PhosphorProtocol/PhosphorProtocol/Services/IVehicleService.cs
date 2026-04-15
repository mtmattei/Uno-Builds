using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IVehicleService
{
    ValueTask<VehicleState> GetCurrentState(CancellationToken ct);
}
