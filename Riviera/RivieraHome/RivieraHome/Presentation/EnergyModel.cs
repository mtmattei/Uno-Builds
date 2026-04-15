using RivieraHome.Services;

namespace RivieraHome.Presentation;

public partial record EnergyModel
{
    private readonly ISmartHomeService _service;

    public EnergyModel(ISmartHomeService service)
    {
        _service = service;
    }

    public IFeed<EnergyData> Energy => Feed.Async(async ct =>
    {
        await Task.Delay(100, ct);
        return await _service.GetEnergyData(ct);
    });
}
