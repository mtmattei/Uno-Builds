namespace RivieraHome.Services;

public interface ISmartHomeService
{
    Task<ClimateData> GetClimateData(CancellationToken ct = default);
    Task<SecurityData> GetSecurityData(CancellationToken ct = default);
    Task<EnergyData> GetEnergyData(CancellationToken ct = default);
    Task<LightingData> GetLightingData(CancellationToken ct = default);
    Task<DiagnosticsData> GetDiagnosticsData(CancellationToken ct = default);

    Task SetTargetTemp(int temp, CancellationToken ct = default);
    Task ToggleZone(string zone, bool enabled, CancellationToken ct = default);
    Task ToggleLight(string room, bool enabled, CancellationToken ct = default);
    Task SetBrightness(int level, CancellationToken ct = default);
    Task ToggleDoor(string door, bool locked, CancellationToken ct = default);
    Task ToggleCamera(string camera, bool enabled, CancellationToken ct = default);
}
