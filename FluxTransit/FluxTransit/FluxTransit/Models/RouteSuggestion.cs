namespace FluxTransit.Models;

/// <summary>
/// Represents an AI-generated route suggestion.
/// </summary>
public partial record RouteSuggestion(
    string Id,
    string Title,
    string Description,
    int TotalMinutes,
    int Transfers,
    IReadOnlyList<RouteLeg> Legs,
    string AiReasoning);

/// <summary>
/// Represents a leg of a route.
/// </summary>
public partial record RouteLeg(
    string RouteNumber,
    string RouteName,
    RouteType Type,
    string DepartureStop,
    string ArrivalStop,
    int DurationMinutes,
    string DepartureTime);
