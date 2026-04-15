using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface INavigationDataService
{
    ValueTask<NavigationState> GetCurrentState(CancellationToken ct);
}
