using FluxTransit.Models;

namespace FluxTransit.Services;

/// <summary>
/// Service interface for AI-powered route planning.
/// </summary>
public interface IAiRouteService
{
    /// <summary>
    /// Gets AI-generated route suggestions for a trip.
    /// </summary>
    /// <param name="origin">The starting location.</param>
    /// <param name="destination">The destination location.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of route suggestions ranked by AI preference.</returns>
    Task<IReadOnlyList<RouteSuggestion>> GetRouteSuggestionsAsync(
        string origin,
        string destination,
        CancellationToken ct = default);
}
