using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockVehicleService : IVehicleService
{
    private readonly Random _random = new();

    public ValueTask<VehicleState> GetCurrentState(CancellationToken ct)
    {
        return ValueTask.FromResult(new VehicleState(
            Speed: 62 + _random.Next(-3, 4),
            Gear: "D",
            AutopilotActive: true,
            BatteryPercent: 74,
            RangeMiles: 186,
            SpeedLimit: 65,
            OutsideTemp: 72.0,
            CurrentTime: DateTime.Now));
    }
}
