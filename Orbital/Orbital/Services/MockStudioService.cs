namespace Orbital.Services;

public class MockStudioService : IStudioService
{
    public ValueTask<StudioStatus> GetStatusAsync(CancellationToken ct) =>
        ValueTask.FromResult(new StudioStatus(
            "6.5.31", "Professional", "Matt", "matt@unoplatform.com",
            new DateOnly(2027, 3, 15), true));

    public ValueTask<ImmutableList<StudioFeature>> GetFeaturesAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new StudioFeature("Hot Reload", "Live XAML and C# updates", true, "Free"),
            new StudioFeature("Hot Design", "Visual editor integration", true, "Professional"),
            new StudioFeature("AI Agent", "Agentic code generation", true, "Professional"),
            new StudioFeature("XAML Inspector", "Runtime visual tree", true, "Free"),
            new StudioFeature("Performance Profiler", "Frame timing and allocation tracking", true, "Professional"),
            new StudioFeature("Cloud Build", "Distributed build pipeline", false, "Enterprise")));

    public ValueTask<ImmutableList<McpConnector>> GetConnectorsAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new McpConnector("Uno Platform", "https://mcp.platform.uno", true, 12, "Connected"),
            new McpConnector("Microsoft Learn", "https://mcp.learn.microsoft.com", true, 8, "Connected"),
            new McpConnector("GitHub Copilot", "https://mcp.github.com", false, 0, "Disconnected")));
}
