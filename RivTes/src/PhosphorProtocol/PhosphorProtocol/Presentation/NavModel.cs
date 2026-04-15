using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record NavModel(INavigationDataService NavigationDataService)
{
    public IFeed<NavigationState> Navigation => Feed.Async(async ct => await NavigationDataService.GetCurrentState(ct));
}
