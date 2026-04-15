using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record EnergyModel(IEnergyService EnergyService)
{
    public IFeed<EnergyState> Energy => Feed.Async(async ct => await EnergyService.GetCurrentState(ct));
}
