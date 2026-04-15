namespace PhosphorProtocol.Models;

public record EnergyState(
    double TripMiles,
    double AverageDrawWh,
    double InstantDrawWh,
    int RangeMiles,
    double MotorPowerKw,
    double FuelEconomyMiPerKwh,
    double TripDurationHours,
    double AverageSpeedMph,
    ImmutableList<double> ConsumptionHistory);
