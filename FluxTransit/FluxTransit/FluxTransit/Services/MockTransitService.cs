using FluxTransit.Models;

namespace FluxTransit.Services;

/// <summary>
/// Mock implementation of transit service for development and demo purposes.
/// Simulates STM transit data.
/// </summary>
public class MockTransitService : ITransitService
{
    private readonly Random _random = new();

    public async Task<IReadOnlyList<TransitRoute>> GetLiveRoutesAsync(CancellationToken ct = default)
    {
        // Simulate network delay
        await Task.Delay(500, ct);

        return new List<TransitRoute>
        {
            new TransitRoute(
                RouteId: "green-line",
                RouteNumber: "M1",
                RouteName: "Green Line",
                Direction: "Angrignon → Honoré-Beaugrand",
                Type: RouteType.Metro,
                EtaMinutes: _random.Next(1, 6),
                ProgressPercent: _random.Next(40, 90),
                CrowdLevel: CrowdLevel.Moderate,
                VehicleId: "GRN-" + _random.Next(100, 999)
            ),
            new TransitRoute(
                RouteId: "24-sherbrooke",
                RouteNumber: "24",
                RouteName: "24-Sherbrooke",
                Direction: "Westbound → Atwater",
                Type: RouteType.Bus,
                EtaMinutes: _random.Next(5, 12),
                ProgressPercent: _random.Next(20, 60),
                CrowdLevel: CrowdLevel.Low,
                VehicleId: "BUS-" + _random.Next(1000, 9999)
            ),
            new TransitRoute(
                RouteId: "orange-line",
                RouteNumber: "M2",
                RouteName: "Orange Line",
                Direction: "Côte-Vertu → Montmorency",
                Type: RouteType.Metro,
                EtaMinutes: _random.Next(2, 8),
                ProgressPercent: _random.Next(30, 80),
                CrowdLevel: CrowdLevel.High,
                VehicleId: "ORG-" + _random.Next(100, 999)
            ),
            new TransitRoute(
                RouteId: "55-st-laurent",
                RouteNumber: "55",
                RouteName: "55-St-Laurent",
                Direction: "Northbound → Henri-Bourassa",
                Type: RouteType.Bus,
                EtaMinutes: _random.Next(3, 15),
                ProgressPercent: _random.Next(10, 50),
                CrowdLevel: CrowdLevel.Moderate,
                VehicleId: "BUS-" + _random.Next(1000, 9999)
            )
        };
    }

    public async Task<NetworkStatus> GetNetworkStatusAsync(CancellationToken ct = default)
    {
        await Task.Delay(200, ct);

        return new NetworkStatus(
            Health: NetworkHealth.MinorDelays,
            Summary: "All metro lines operating normally. Minor delays on 24-Sherbrooke due to construction.",
            Alerts: new List<ServiceAlert>
            {
                new ServiceAlert(
                    Id: "alert-1",
                    Title: "Construction Notice",
                    Message: "Expect 5-10 minute delays on 24-Sherbrooke between Guy and Atwater due to road work.",
                    Severity: AlertSeverity.Info,
                    StartTime: DateTimeOffset.Now.AddHours(-2),
                    EndTime: DateTimeOffset.Now.AddDays(3),
                    AffectedRoutes: new[] { "24" }
                )
            }
        );
    }

    public async Task<IReadOnlyList<ServiceAlert>> GetAlertsAsync(CancellationToken ct = default)
    {
        var status = await GetNetworkStatusAsync(ct);
        return status.Alerts ?? Array.Empty<ServiceAlert>();
    }
}
