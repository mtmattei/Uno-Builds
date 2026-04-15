using System.Collections.Immutable;

namespace RivieraHome.Services;

public class MockSmartHomeService : ISmartHomeService
{
    private readonly Random _rng = new();
    private int _targetTemp = 72;
    private int _brightness = 75;

    private readonly Dictionary<string, bool> _zones = new()
    {
        ["LIVING"] = true,
        ["BEDROOM"] = true,
        ["KITCHEN"] = false,
        ["GARAGE"] = false
    };

    private readonly Dictionary<string, bool> _lights = new()
    {
        ["LIVING"] = true,
        ["BEDROOM"] = false,
        ["KITCHEN"] = true,
        ["PORCH"] = false,
        ["GARAGE"] = true
    };

    private readonly Dictionary<string, bool> _doors = new()
    {
        ["FRONT"] = true,
        ["BACK"] = true,
        ["GARAGE"] = false,
        ["SIDE"] = true
    };

    private readonly Dictionary<string, bool> _cameras = new()
    {
        ["FRONT"] = true,
        ["BACK"] = true,
        ["GARAGE"] = false
    };

    public Task<ClimateData> GetClimateData(CancellationToken ct = default)
    {
        var temp = _targetTemp + _rng.Next(-2, 3);
        return Task.FromResult(new ClimateData(
            Temp: temp,
            TargetTemp: _targetTemp,
            Humidity: 42 + _rng.Next(-3, 4),
            OutsideTemp: 85 + _rng.Next(-2, 3),
            Zones: _zones.ToImmutableDictionary()));
    }

    public Task<SecurityData> GetSecurityData(CancellationToken ct = default)
    {
        return Task.FromResult(new SecurityData(
            Doors: _doors.ToImmutableDictionary(),
            Cameras: _cameras.ToImmutableDictionary(),
            MotionEvents: 7 + _rng.Next(0, 3),
            Log: ImmutableList.Create(
                $"{DateTime.Now:HH:mm:ss} Front door motion detected",
                $"{DateTime.Now.AddMinutes(-5):HH:mm:ss} Garage camera offline",
                $"{DateTime.Now.AddMinutes(-12):HH:mm:ss} Back door locked",
                $"{DateTime.Now.AddMinutes(-30):HH:mm:ss} System armed",
                $"{DateTime.Now.AddHours(-1):HH:mm:ss} Front camera motion",
                $"{DateTime.Now.AddHours(-2):HH:mm:ss} Side door unlocked")));
    }

    public Task<EnergyData> GetEnergyData(CancellationToken ct = default)
    {
        return Task.FromResult(new EnergyData(
            SolarKw: Math.Round(4.2 + _rng.NextDouble() * 1.5, 1),
            GridKw: Math.Round(1.8 + _rng.NextDouble() * 0.8, 1),
            BatteryPct: Math.Round(78 + _rng.NextDouble() * 5, 0),
            DailyKwh: Math.Round(32.4 + _rng.NextDouble() * 3, 1)));
    }

    public Task<LightingData> GetLightingData(CancellationToken ct = default)
    {
        return Task.FromResult(new LightingData(
            Lights: _lights.ToImmutableDictionary(),
            Brightness: _brightness));
    }

    public Task<DiagnosticsData> GetDiagnosticsData(CancellationToken ct = default)
    {
        return Task.FromResult(new DiagnosticsData(
            CpuTemp: 42 + _rng.Next(-3, 4),
            MemPct: 67 + _rng.Next(-5, 6),
            NetworkUp: true,
            UptimeDays: 14,
            DeviceCount: 23,
            Alerts: ImmutableList.Create(
                "HVAC filter replacement due",
                "Battery firmware update available"),
            Log: ImmutableList.Create(
                $"> {DateTime.Now:HH:mm:ss} System health check OK",
                $"> {DateTime.Now.AddMinutes(-15):HH:mm:ss} Network latency 12ms",
                $"> {DateTime.Now.AddMinutes(-30):HH:mm:ss} Backup completed",
                $"> {DateTime.Now.AddHours(-1):HH:mm:ss} Device scan: 23 online",
                $"> {DateTime.Now.AddHours(-2):HH:mm:ss} Firmware check complete")));
    }

    public Task SetTargetTemp(int temp, CancellationToken ct = default)
    {
        _targetTemp = Math.Clamp(temp, 60, 85);
        return Task.CompletedTask;
    }

    public Task ToggleZone(string zone, bool enabled, CancellationToken ct = default)
    {
        if (_zones.ContainsKey(zone))
            _zones[zone] = enabled;
        return Task.CompletedTask;
    }

    public Task ToggleLight(string room, bool enabled, CancellationToken ct = default)
    {
        if (_lights.ContainsKey(room))
            _lights[room] = enabled;
        return Task.CompletedTask;
    }

    public Task SetBrightness(int level, CancellationToken ct = default)
    {
        _brightness = Math.Clamp(level, 25, 100);
        return Task.CompletedTask;
    }

    public Task ToggleDoor(string door, bool locked, CancellationToken ct = default)
    {
        if (_doors.ContainsKey(door))
            _doors[door] = locked;
        return Task.CompletedTask;
    }

    public Task ToggleCamera(string camera, bool enabled, CancellationToken ct = default)
    {
        if (_cameras.ContainsKey(camera))
            _cameras[camera] = enabled;
        return Task.CompletedTask;
    }
}
