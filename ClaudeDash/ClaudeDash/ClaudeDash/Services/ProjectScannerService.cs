namespace ClaudeDash.Services;

public class ProjectScannerService : IProjectScannerService
{
    private readonly IClaudeConfigService _configService;
    private readonly IWorktreeService _worktreeService;

    public ProjectScannerService(IClaudeConfigService configService, IWorktreeService worktreeService)
    {
        _configService = configService;
        _worktreeService = worktreeService;
    }

    public async Task<List<ProjectInfo>> GetAllProjectsAsync()
    {
        var projects = new Dictionary<string, ProjectInfo>(StringComparer.OrdinalIgnoreCase);

        // 1. Get repos from config service (session-derived)
        try
        {
            var repos = await _configService.GetReposAsync();
            foreach (var repo in repos)
            {
                if (string.IsNullOrEmpty(repo.Path)) continue;
                if (!projects.ContainsKey(repo.Path))
                {
                    projects[repo.Path] = new ProjectInfo
                    {
                        Name = repo.Name,
                        Path = repo.Path,
                        LastActivity = repo.LastActivity,
                        SessionCount = repo.SessionCount,
                        CurrentBranch = repo.LastBranch
                    };
                }
                else
                {
                    var existing = projects[repo.Path];
                    existing = existing with { SessionCount = existing.SessionCount + repo.SessionCount };
                    if (repo.LastActivity > existing.LastActivity)
                    {
                        existing = existing with { LastActivity = repo.LastActivity };
                        if (!string.IsNullOrEmpty(repo.LastBranch))
                            existing = existing with { CurrentBranch = repo.LastBranch };
                    }
                    projects[repo.Path] = existing;
                }
            }
        }
        catch { }

        // 2. Scan ~/.claude/projects/ directories for additional projects
        try
        {
            var claudeDir = _configService.GetClaudeDir();
            var projectsDir = System.IO.Path.Combine(claudeDir, "projects");
            if (Directory.Exists(projectsDir))
            {
                foreach (var dir in Directory.GetDirectories(projectsDir))
                {
                    var dirName = System.IO.Path.GetFileName(dir);
                    var decodedPath = ClaudeConfigService.DecodeProjectPath(dirName);

                    if (!projects.ContainsKey(decodedPath))
                    {
                        var name = System.IO.Path.GetFileName(decodedPath.TrimEnd('\\', '/'));
                        projects[decodedPath] = new ProjectInfo
                        {
                            Name = name,
                            Path = decodedPath,
                            LastActivity = new DirectoryInfo(dir).LastWriteTimeUtc
                        };
                    }

                    // Count session files in this project directory
                    var jsonlCount = Directory.GetFiles(dir, "*.jsonl", SearchOption.TopDirectoryOnly).Length;
                    if (jsonlCount > projects[decodedPath].SessionCount)
                        projects[decodedPath] = projects[decodedPath] with { SessionCount = jsonlCount };
                }
            }
        }
        catch { }

        // 3. Enrich with filesystem checks
        foreach (var key in projects.Keys.ToList())
        {
            var project = projects[key];
            var pathExists = Directory.Exists(project.Path);
            project = project with { PathExists = pathExists };

            if (pathExists)
            {
                var isGitRepo = Directory.Exists(System.IO.Path.Combine(project.Path, ".git"));
                var hasClaudeMd = File.Exists(System.IO.Path.Combine(project.Path, "CLAUDE.md"))
                    || File.Exists(System.IO.Path.Combine(project.Path, ".claude", "CLAUDE.md"));

                project = project with { IsGitRepo = isGitRepo, HasClaudeMd = hasClaudeMd };

                // Get worktree count for git repos
                if (isGitRepo)
                {
                    try
                    {
                        var worktrees = await _worktreeService.ListWorktreesAsync(project.Path);
                        project = project with { WorktreeCount = worktrees.Count };
                    }
                    catch { }
                }
            }

            projects[key] = project;
        }

        return projects.Values
            .OrderByDescending(p => p.LastActivity)
            .ToList();
    }
}
