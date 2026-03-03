namespace FluxTransit.Models;

/// <summary>
/// Represents a transit route with real-time tracking information.
/// </summary>
public partial record TransitRoute(
    string RouteId,
    string RouteNumber,
    string RouteName,
    string Direction,
    RouteType Type,
    int EtaMinutes,
    double ProgressPercent,
    CrowdLevel CrowdLevel,
    string? VehicleId = null,
    double? Latitude = null,
    double? Longitude = null);

public enum RouteType
{
    Metro,
    Bus,
    Train
}

public enum CrowdLevel
{
    Low,
    Moderate,
    High,
    VeryHigh
}
