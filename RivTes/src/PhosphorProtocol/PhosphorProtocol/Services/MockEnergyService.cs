using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockEnergyService : IEnergyService
{
    private readonly Random _random = new();

    public ValueTask<EnergyState> GetCurrentState(CancellationToken ct)
    {
        var history = Enumerable.Range(0, 40)
            .Select(i => 180.0 + _random.NextDouble() * 120)
            .ToImmutableList();

        return ValueTask.FromResult(new EnergyState(
            TripMiles: 42.3,
            AverageDrawWh: 246,
            InstantDrawWh: 210 + _random.Next(-30, 30),
            RangeMiles: 186,
            MotorPowerKw: 52.0 + _random.NextDouble() * 10,
            FuelEconomyMiPerKwh: 4.2,
            TripDurationHours: 0.68,
            AverageSpeedMph: 62.2,
            ConsumptionHistory: history));
    }
}
