namespace Orbital.Services;

public interface IBuildService
{
    ValueTask<ImmutableList<Controls.ConsoleLine>> GetLastBuildOutputAsync(CancellationToken ct);
    ValueTask<ImmutableList<Controls.ConsoleLine>> GetLastRunOutputAsync(CancellationToken ct);
    ValueTask<ImmutableList<Artifact>> GetArtifactsAsync(CancellationToken ct);
    ValueTask BuildAsync(CancellationToken ct = default);
    ValueTask RunAsync(CancellationToken ct = default);
    ValueTask BuildRunVerifyAsync(CancellationToken ct = default);
    ValueTask PackageAsync(CancellationToken ct = default);
    ValueTask<ImmutableList<Controls.ConsoleLine>> RunSmokeTestAsync(CancellationToken ct = default);
}
