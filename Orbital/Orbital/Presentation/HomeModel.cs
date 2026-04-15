using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record HomeModel(
    IEnvironmentService Env,
    IStudioService Studio,
    IMcpService Mcp,
    IAgentService Agents,
    IClockService Clock)
{
    public IFeed<EnvironmentStatus> EnvStatus => Feed.Async(Env.GetStatusAsync);
    public IFeed<StudioStatus> StudioInfo => Feed.Async(Studio.GetStatusAsync);
    public IFeed<McpStatus> McpInfo => Feed.Async(Mcp.GetConnectionStatusAsync);
    public IFeed<AgentSession> ActiveSession => Feed.Async(async ct =>
        (await Agents.GetActiveSessionAsync(ct))!);
    public IFeed<VersionInfo> Versions => Feed.Async(Env.GetVersionInfoAsync);

    public IFeed<DateTime> CurrentTime => Feed.AsyncEnumerable(Clock.GetTimeStream);

    public IFeed<string> Greeting => CurrentTime.Select(t =>
        t.Hour < 12 ? "Good morning" : t.Hour < 17 ? "Good afternoon" : "Good evening");

    public IFeed<string> UserName => Feed.Async(async ct =>
    {
        var stored = Helpers.SettingsService.GetStoredUsername();
        if (!string.IsNullOrEmpty(stored)) return stored;
        return (await Studio.GetStatusAsync(ct)).AccountName ?? "Developer";
    });

    public IFeed<string> ClockDisplay => CurrentTime.Select(t => t.ToString("h:mm:ss tt"));

    public IFeed<string> DateDisplay => CurrentTime.Select(t => t.ToString("dddd, MMMM d, yyyy"));

    // Computed display feeds for version pills
    public IFeed<string> VersionDisplay => Versions.Select(v => $"v{v.OrbitalVersion}");
    public IFeed<string> McpDisplay => McpInfo.Select(m => $"{m.ServerCount} servers");
    public IFeed<string> LlmDisplay => Versions.Select(v => v.LlmModel);

    // Computed display feeds for status cards
    public IFeed<string> UnoVersion => EnvStatus.Select(e => e.UnoSdkVersion);
    public IFeed<string> UnoDetail => EnvStatus.Select(e => $"Uno.Sdk \u00B7 {e.Renderer}");
    public IFeed<string> DotNetVersion => EnvStatus.Select(e => e.DotNetVersion);
    public IFeed<string> DotNetDetail => EnvStatus.Select(e => $"net{Environment.Version.ToString(2)} \u00B7 {e.WorkloadCount} workloads");
    public IFeed<string> StudioVersion => StudioInfo.Select(s => s.Tier);
    public IFeed<string> StudioDetail => StudioInfo.Select(s => s.IsValid ? $"{s.AccountName} \u00B7 Active" : "Not detected");
    public IFeed<string> AvatarInitial => UserName.Select(n => string.IsNullOrEmpty(n) ? "?" : n[..1].ToUpperInvariant());
}
