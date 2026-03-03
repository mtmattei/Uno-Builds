using FluxTransit.Models;

namespace FluxTransit.Services;

/// <summary>
/// Service interface for transit data operations.
/// </summary>
public interface ITransitService
{
    /// <summary>
    /// Gets live transit routes near the user or for saved favorites.
    /// </summary>
    Task<IReadOnlyList<TransitRoute>> GetLiveRoutesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the current network status.
    /// </summary>
    Task<NetworkStatus> GetNetworkStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets service alerts.
    /// </summary>
    Task<IReadOnlyList<ServiceAlert>> GetAlertsAsync(CancellationToken ct = default);
}
