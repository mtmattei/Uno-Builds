using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record MainModel(IClockService Clock, IMcpService Mcp, IProjectContext ProjectContext)
{
    public IFeed<DateTime> CurrentTime => Feed.AsyncEnumerable(Clock.GetTimeStream);

    public IFeed<string> ClockDisplay => CurrentTime.Select(t => t.ToString("h:mm:ss tt"));

    public IFeed<McpStatus> McpInfo => Feed.Async(Mcp.GetConnectionStatusAsync);

    public IFeed<string> McpDetail => McpInfo.Select(m => $"{m.ServerCount} servers \u00B7 {m.ToolCount} tools");

    public IFeed<string> ActiveProjectName => Feed.Async(async ct =>
    {
        var recents = await ProjectContext.GetRecentProjectsAsync(ct);
        return ProjectContext.ActiveProject?.Name ?? recents.FirstOrDefault()?.Name ?? "Orbital";
    });
}
