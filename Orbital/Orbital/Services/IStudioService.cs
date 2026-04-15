namespace Orbital.Services;

public interface IStudioService
{
    ValueTask<StudioStatus> GetStatusAsync(CancellationToken ct);
    ValueTask<ImmutableList<StudioFeature>> GetFeaturesAsync(CancellationToken ct);
    ValueTask<ImmutableList<McpConnector>> GetConnectorsAsync(CancellationToken ct);
}
