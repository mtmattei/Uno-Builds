namespace Orbital.Services;

public class MockMcpService : IMcpService
{
    public ValueTask<McpStatus> GetConnectionStatusAsync(CancellationToken ct) =>
        ValueTask.FromResult(new McpStatus(
            true, 3, 24,
            ImmutableList.Create(
                new McpServer("Uno Platform", "https://mcp.platform.uno", true, 12),
                new McpServer("Microsoft Learn", "https://mcp.learn.microsoft.com", true, 8),
                new McpServer("Uno App", "localhost:5000", true, 4))));
}
