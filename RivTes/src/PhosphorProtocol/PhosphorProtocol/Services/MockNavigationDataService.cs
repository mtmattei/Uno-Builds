using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockNavigationDataService : INavigationDataService
{
    public ValueTask<NavigationState> GetCurrentState(CancellationToken ct)
    {
        return ValueTask.FromResult(new NavigationState(
            NextTurnDirection: "RIGHT",
            NextTurnDistance: "0.3",
            NextTurnRoad: "Rue Sherbrooke",
            DestinationName: "Montreal SC",
            ETA: "11:42 PM",
            CarLatitude: 45.5017,
            CarLongitude: -73.5673));
    }
}
