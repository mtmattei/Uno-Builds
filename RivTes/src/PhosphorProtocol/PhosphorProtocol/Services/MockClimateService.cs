using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockClimateService : IClimateService
{
    public ValueTask<ClimateState> GetCurrentState(CancellationToken ct)
    {
        return ValueTask.FromResult(new ClimateState(
            Temperature: 72,
            FanSpeed: 3,
            SeatHeatLeft: 1,
            SeatHeatRight: 0,
            ACEnabled: true,
            DefrostEnabled: false));
    }
}
