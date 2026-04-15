using RivieraHome.Services;

namespace RivieraHome.Presentation;

public partial record SecurityModel
{
    private readonly ISmartHomeService _service;

    public SecurityModel(ISmartHomeService service)
    {
        _service = service;
    }

    public IFeed<SecurityData> Security => Feed.Async(async ct =>
    {
        await Task.Delay(100, ct);
        return await _service.GetSecurityData(ct);
    });

    public async ValueTask ToggleDoor(string door, CancellationToken ct)
    {
        var data = await Security;
        if (data?.Doors.TryGetValue(door, out var current) == true)
        {
            await _service.ToggleDoor(door, !current, ct);
        }
    }

    public async ValueTask ToggleCamera(string camera, CancellationToken ct)
    {
        var data = await Security;
        if (data?.Cameras.TryGetValue(camera, out var current) == true)
        {
            await _service.ToggleCamera(camera, !current, ct);
        }
    }
}
