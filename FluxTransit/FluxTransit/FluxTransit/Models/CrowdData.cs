namespace FluxTransit.Models;

/// <summary>
/// Represents crowd level data for a specific time period.
/// </summary>
public partial record CrowdDataPoint(
    string TimeLabel,
    double CrowdValue);
