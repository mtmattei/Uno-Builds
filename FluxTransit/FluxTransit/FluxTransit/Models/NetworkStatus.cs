namespace FluxTransit.Models;

/// <summary>
/// Represents the overall network status.
/// </summary>
public partial record NetworkStatus(
    NetworkHealth Health,
    string Summary,
    IReadOnlyList<ServiceAlert>? Alerts = null);

public enum NetworkHealth
{
    Normal,
    MinorDelays,
    MajorDelays,
    ServiceDisruption
}

/// <summary>
/// Represents a service alert or notification.
/// </summary>
public partial record ServiceAlert(
    string Id,
    string Title,
    string Message,
    AlertSeverity Severity,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime = null,
    IReadOnlyList<string>? AffectedRoutes = null);

public enum AlertSeverity
{
    Info,
    Warning,
    Severe
}
