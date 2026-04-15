namespace Orbital.Services;

public interface IEnvironmentService
{
    ValueTask<EnvironmentStatus> GetStatusAsync(CancellationToken ct);
    ValueTask<VersionInfo> GetVersionInfoAsync(CancellationToken ct);
    ValueTask<ProjectMeta> GetCurrentProjectAsync(CancellationToken ct);
    ValueTask<ImmutableList<RecentProject>> GetRecentProjectsAsync(CancellationToken ct);
    ValueTask<ProjectCreateResult> CreateProjectAsync(string name, string outputPath, CancellationToken ct);
}
