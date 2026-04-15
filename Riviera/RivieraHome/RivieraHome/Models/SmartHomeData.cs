using System.Collections.Immutable;

namespace RivieraHome.Models;

public record ClimateData(
    int Temp,
    int TargetTemp,
    int Humidity,
    int OutsideTemp,
    ImmutableDictionary<string, bool> Zones);

public record SecurityData(
    ImmutableDictionary<string, bool> Doors,
    ImmutableDictionary<string, bool> Cameras,
    int MotionEvents,
    ImmutableList<string> Log);

public record EnergyData(
    double SolarKw,
    double GridKw,
    double BatteryPct,
    double DailyKwh);

public record LightingData(
    ImmutableDictionary<string, bool> Lights,
    int Brightness);

public record DiagnosticsData(
    int CpuTemp,
    int MemPct,
    bool NetworkUp,
    int UptimeDays,
    int DeviceCount,
    ImmutableList<string> Alerts,
    ImmutableList<string> Log);
