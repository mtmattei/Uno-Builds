namespace Orbital.Services;

public interface IProjectContext
{
    OrbitalProject? ActiveProject { get; }
    void SetActiveProject(OrbitalProject project);
    ValueTask<ImmutableList<OrbitalProject>> GetRecentProjectsAsync(CancellationToken ct);
    ValueTask<OrbitalProject?> OpenProjectAsync(string path, CancellationToken ct);
    void RemoveRecentProject(string solutionPath);
    event Action? ActiveProjectChanged;
}
