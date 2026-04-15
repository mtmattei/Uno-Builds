namespace PhosphorProtocol.Models;

public record ClimateState(
    int Temperature,
    int FanSpeed,
    int SeatHeatLeft,
    int SeatHeatRight,
    bool ACEnabled,
    bool DefrostEnabled);
