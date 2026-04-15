namespace Orbital.Services;

public interface IMcpService
{
    ValueTask<McpStatus> GetConnectionStatusAsync(CancellationToken ct);
}
