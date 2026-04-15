using RivieraHome.Services;

namespace RivieraHome.Presentation;

public partial record LightingModel
{
    private readonly ISmartHomeService _service;

    public LightingModel(ISmartHomeService service)
    {
        _service = service;
    }

    public IFeed<LightingData> Lighting => Feed.Async(async ct =>
    {
        await Task.Delay(100, ct);
        return await _service.GetLightingData(ct);
    });

    public IState<int> Brightness => State<int>.Value(this, () => 75);

    public async ValueTask SetBrightness(int level, CancellationToken ct)
    {
        await Brightness.UpdateAsync(_ => level, ct);
        await _service.SetBrightness(level, ct);
    }

    public async ValueTask ToggleLight(string room, CancellationToken ct)
    {
        var data = await Lighting;
        if (data?.Lights.TryGetValue(room, out var current) == true)
        {
            await _service.ToggleLight(room, !current, ct);
        }
    }
}
