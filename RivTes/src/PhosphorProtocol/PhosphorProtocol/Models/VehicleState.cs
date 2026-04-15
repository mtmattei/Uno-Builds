namespace PhosphorProtocol.Models;

public record VehicleState(
    int Speed,
    string Gear,
    bool AutopilotActive,
    int BatteryPercent,
    int RangeMiles,
    int SpeedLimit,
    double OutsideTemp,
    DateTime CurrentTime);
