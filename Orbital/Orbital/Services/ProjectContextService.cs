using System.Diagnostics;
using System.Text.Json;

namespace Orbital.Services;

public class ProjectContextService : IProjectContext
{
    private static readonly string _recentsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Orbital", "recent-projects.json");

    private readonly SemaphoreSlim _lock = new(1, 1);
    private OrbitalProject? _activeProject;
    private List<OrbitalProject>? _recents;

    public OrbitalProject? ActiveProject => _activeProject;
    public event Action? ActiveProjectChanged;

    public void SetActiveProject(OrbitalProject project)
    {
        _activeProject = project;
        _ = TouchRecentAsync(project);
        ActiveProjectChanged?.Invoke();
    }

    public async ValueTask<ImmutableList<OrbitalProject>> GetRecentProjectsAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_recents is null)
            {
                _recents = await LoadRecentsAsync();
                if (_recents.Count == 0)
                {
                    var self = BuildOrbitalSelfProject();
                    if (self is not null)
                    {
                        _recents.Add(self);
                        await SaveRecentsAsync();
                    }
                }
            }
            return _recents.ToImmutableList();
        }
        finally { _lock.Release(); }
    }

    public async ValueTask<OrbitalProject?> OpenProjectAsync(string path, CancellationToken ct)
    {
        // Accept a .csproj, .sln, or directory
        var fullPath = Path.GetFullPath(path);
        string solutionPath;
        string rootDir;

        if (File.Exists(fullPath))
        {
            solutionPath = fullPath;
            rootDir = Path.GetDirectoryName(fullPath)!;
        }
        else if (Directory.Exists(fullPath))
        {
            // Look for .sln or .csproj in the directory
            var sln = Directory.GetFiles(fullPath, "*.sln").FirstOrDefault();
            var csproj = Directory.GetFiles(fullPath, "*.csproj").FirstOrDefault();
            solutionPath = sln ?? csproj ?? fullPath;
            rootDir = fullPath;
        }
        else
        {
            return null;
        }

        var name = Path.GetFileNameWithoutExtension(solutionPath);
        var branch = await GetGitBranchAsync(rootDir, ct);
        var status = IsUnoProject(rootDir) ? HealthStatus.Ok : HealthStatus.Warn;

        var project = new OrbitalProject(
            name, solutionPath, rootDir, branch, DateTime.Now, status);

        _recents ??= await LoadRecentsAsync();
        await TouchRecentAsync(project);

        _activeProject = project;
        ActiveProjectChanged?.Invoke();
        return project;
    }

    public void RemoveRecentProject(string solutionPath)
    {
        if (_recents is null) return;
        _recents.RemoveAll(p => p.SolutionPath.Equals(solutionPath, StringComparison.OrdinalIgnoreCase));
        _ = SaveRecentsAsync();
    }

    // --- Helpers ---

    private async Task TouchRecentAsync(OrbitalProject project)
    {
        _recents ??= await LoadRecentsAsync();
        _recents.RemoveAll(p => p.SolutionPath.Equals(project.SolutionPath, StringComparison.OrdinalIgnoreCase));
        _recents.Insert(0, project with { LastOpened = DateTime.Now });
        // Keep at most 10 recents
        if (_recents.Count > 10)
            _recents.RemoveRange(10, _recents.Count - 10);
        await SaveRecentsAsync();
    }

    private static OrbitalProject? BuildOrbitalSelfProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var csproj = dir.GetFiles("*.csproj").FirstOrDefault();
            if (csproj is not null)
            {
                return new OrbitalProject(
                    Path.GetFileNameWithoutExtension(csproj.Name),
                    csproj.FullName,
                    dir.FullName,
                    "main",
                    DateTime.Now,
                    HealthStatus.Ok);
            }
            dir = dir.Parent;
        }
        // Fallback: use base directory even without csproj
        return new OrbitalProject(
            "Orbital",
            AppContext.BaseDirectory,
            AppContext.BaseDirectory,
            "main",
            DateTime.Now,
            HealthStatus.Ok);
    }

    private static bool IsUnoProject(string rootDir)
    {
        // Check global.json for Uno.Sdk
        var dir = new DirectoryInfo(rootDir);
        while (dir is not null)
        {
            var globalJson = Path.Combine(dir.FullName, "global.json");
            if (File.Exists(globalJson))
            {
                try
                {
                    var text = File.ReadAllText(globalJson);
                    if (text.Contains("Uno.Sdk", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch { }
            }
            dir = dir.Parent;
        }

        // Check any .csproj for Uno.Sdk or Uno.WinUI reference
        try
        {
            foreach (var csproj in Directory.GetFiles(rootDir, "*.csproj", SearchOption.TopDirectoryOnly))
            {
                var text = File.ReadAllText(csproj);
                if (text.Contains("Uno.Sdk", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Uno.WinUI", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch { }

        return false;
    }

    private static async Task<string?> GetGitBranchAsync(string directory, CancellationToken ct)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            var branch = output.Trim();
            return string.IsNullOrEmpty(branch) ? null : branch;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<OrbitalProject>> LoadRecentsAsync()
    {
        try
        {
            if (File.Exists(_recentsPath))
            {
                var json = await File.ReadAllTextAsync(_recentsPath);
                return JsonSerializer.Deserialize<List<OrbitalProject>>(json) ?? [];
            }
        }
        catch { }
        return [];
    }

    private async Task SaveRecentsAsync()
    {
        try
        {
            var dir = Path.GetDirectoryName(_recentsPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(_recents, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_recentsPath, json);
        }
        catch { }
    }
}
