using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IChargeService
{
    ValueTask<ChargeState> GetCurrentState(CancellationToken ct);
}
