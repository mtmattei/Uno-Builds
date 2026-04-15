using RivieraHome.Services;

namespace RivieraHome.Presentation;

public partial record DiagnosticsModel
{
    private readonly ISmartHomeService _service;

    public DiagnosticsModel(ISmartHomeService service)
    {
        _service = service;
    }

    public IFeed<DiagnosticsData> Diagnostics => Feed.Async(async ct =>
    {
        await Task.Delay(100, ct);
        return await _service.GetDiagnosticsData(ct);
    });
}
