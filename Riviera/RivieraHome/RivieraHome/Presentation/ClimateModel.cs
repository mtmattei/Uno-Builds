using RivieraHome.Services;

namespace RivieraHome.Presentation;

public partial record ClimateModel
{
    private readonly ISmartHomeService _service;

    public ClimateModel(ISmartHomeService service)
    {
        _service = service;
    }

    public IFeed<ClimateData> Climate => Feed.Async(async ct =>
    {
        await Task.Delay(100, ct);
        return await _service.GetClimateData(ct);
    });

    public IState<int> TargetTemp => State<int>.Value(this, () => 72);

    public async ValueTask IncrementTemp(CancellationToken ct)
    {
        var current = await TargetTemp;
        if (current < 85)
        {
            var next = current + 1;
            await TargetTemp.UpdateAsync(_ => next, ct);
            await _service.SetTargetTemp(next, ct);
        }
    }

    public async ValueTask DecrementTemp(CancellationToken ct)
    {
        var current = await TargetTemp;
        if (current > 60)
        {
            var next = current - 1;
            await TargetTemp.UpdateAsync(_ => next, ct);
            await _service.SetTargetTemp(next, ct);
        }
    }

    public async ValueTask ToggleZone(string zone, CancellationToken ct)
    {
        var data = await Climate;
        if (data?.Zones.TryGetValue(zone, out var current) == true)
        {
            await _service.ToggleZone(zone, !current, ct);
        }
    }
}
