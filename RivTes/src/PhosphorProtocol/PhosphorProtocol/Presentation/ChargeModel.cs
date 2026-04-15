using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record ChargeModel(IChargeService ChargeService)
{
    public IFeed<ChargeState> Charge => Feed.Async(async ct => await ChargeService.GetCurrentState(ct));
}
