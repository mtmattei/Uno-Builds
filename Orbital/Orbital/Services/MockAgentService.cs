namespace Orbital.Services;

public class MockAgentService : IAgentService
{
    private static readonly ImmutableList<AgentSession> _sessions = ImmutableList.Create(
        new AgentSession("s1", "Fix NavigationView rendering", "Orbital", "main",
            "Debug why the NavigationView sidebar is not visible on launch",
            SessionStatus.Active, 12, 3,
            ImmutableList.Create(
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(34), "Session started", "Agent initialized with 24 tools", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(35), "Read Shell.xaml", "Analyzed NavigationView region setup", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(36), "Identified root cause", "Region.Attached on outer Grid replaces content", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(37), "Created MainPage.xaml", "Moved NavigationView to separate page", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(38), "Build succeeded", "0 warnings, 0 errors", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(14).AddMinutes(39), "Screenshot verified", "Sidebar and navigation working", ActionStatus.Ok)),
            DateTime.Today.AddHours(14).AddMinutes(34)),
        new AgentSession("s2", "Add custom controls", "Orbital", "main",
            "Build StatusDot, PulsingBars, VersionPill, and other custom controls",
            SessionStatus.Done, 8, 6,
            ImmutableList.Create(
                new AgentAction(DateTime.Today.AddHours(13), "Session started", "Agent initialized", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(13).AddMinutes(15), "Created 6 controls", "StatusDot, PulsingBars, VersionPill, DataStream, ConsoleBlock, TimelineItem", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(13).AddMinutes(20), "Build verified", "All controls compile and render", ActionStatus.Ok)),
            DateTime.Today.AddHours(13)),
        new AgentSession("s3", "Scaffold theme system", "Orbital", "main",
            "Create dark theme with emerald accent and typography system",
            SessionStatus.Done, 6, 4,
            ImmutableList.Create(
                new AgentAction(DateTime.Today.AddHours(11), "Session started", "", ActionStatus.Ok),
                new AgentAction(DateTime.Today.AddHours(11).AddMinutes(10), "Theme complete", "ColorPaletteOverride, TextBlock styles, Surfaces", ActionStatus.Ok)),
            DateTime.Today.AddHours(11)));

    public ValueTask<ImmutableList<AgentSession>> GetSessionsAsync(CancellationToken ct) =>
        ValueTask.FromResult(_sessions);

    public ValueTask<AgentSession?> GetActiveSessionAsync(CancellationToken ct) =>
        ValueTask.FromResult<AgentSession?>(_sessions[0]);

    public ValueTask CreateSessionAsync(CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask ReplayAsync(string sessionId, CancellationToken ct) => ValueTask.CompletedTask;
}
