namespace PhosphorProtocol.Models;

public record ChargeState(
    int BatteryPercent,
    int RangeMiles,
    double ChargeRateKw,
    int ChargeLimitPercent,
    bool IsCharging,
    ImmutableList<Supercharger> NearbySuperchargers);

public record Supercharger(string Name, double DistanceMiles, int Available, int Total);
