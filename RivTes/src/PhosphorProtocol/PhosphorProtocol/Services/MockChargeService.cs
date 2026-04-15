using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockChargeService : IChargeService
{
    public ValueTask<ChargeState> GetCurrentState(CancellationToken ct)
    {
        return ValueTask.FromResult(new ChargeState(
            BatteryPercent: 74,
            RangeMiles: 186,
            ChargeRateKw: 0,
            ChargeLimitPercent: 90,
            IsCharging: false,
            NearbySuperchargers:
            [
                new Supercharger("Montreal SC", 2.1, 6, 12),
                new Supercharger("Laval SC", 8.4, 4, 8),
                new Supercharger("Longueuil SC", 5.7, 8, 10)
            ]));
    }
}
