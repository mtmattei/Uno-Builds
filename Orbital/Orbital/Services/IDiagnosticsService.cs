namespace Orbital.Services;

public interface IDiagnosticsService
{
    ValueTask<ImmutableList<DiagnosticsCheck>> GetChecksAsync(CancellationToken ct);
    ValueTask<ImmutableList<DependencyInfo>> GetDependenciesAsync(CancellationToken ct);
    ValueTask<ImmutableList<PlatformTarget>> GetPlatformTargetsAsync(CancellationToken ct);
    ValueTask<ImmutableList<RuntimeTool>> GetRuntimeToolsAsync(CancellationToken ct);
}
