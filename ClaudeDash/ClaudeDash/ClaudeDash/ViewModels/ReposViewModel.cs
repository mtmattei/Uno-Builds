using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record ReposModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<ReposModel> _logger;

    public ReposModel(
        IClaudeConfigService configService,
        ILogger<ReposModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<RepoInfo> Repos => ListFeed.Async(async ct =>
    {
        try
        {
            var repos = await _configService.GetReposAsync();

            var enrichments = GetMockEnrichments();
            for (int i = 0; i < repos.Count; i++)
            {
                var repo = repos[i];

                if (!string.IsNullOrEmpty(repo.Path) && Directory.Exists(repo.Path))
                {
                    try
                    {
                        var gitStatus = await _configService.GetGitStatusAsync(repo.Path);
                        repo.IsDirty = gitStatus.IsDirty;
                        repo.Additions = gitStatus.Additions;
                        repo.Deletions = gitStatus.Deletions;
                        if (!string.IsNullOrEmpty(gitStatus.CurrentBranch))
                            repo.LastBranch = gitStatus.CurrentBranch;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get git status for {RepoPath}", repo.Path);
                        ApplyMockEnrichment(repo, enrichments, i);
                    }
                }
                else
                {
                    ApplyMockEnrichment(repo, enrichments, i);
                }

                if (repo.LanguageSegments.Length == 0 && i < enrichments.Count)
                {
                    var e = enrichments[i];
                    if (string.IsNullOrEmpty(repo.TechStack)) repo.TechStack = e.TechStack;
                    repo.LanguageSegments = e.LanguageSegments;
                    repo.LanguageColorHexes = e.LanguageColorHexes;
                    repo.DepCount = e.DepCount;
                }
            }

            // If fewer than 4 repos from service, add mock ones to fill out the dashboard
            if (repos.Count < 6)
            {
                foreach (var mock in GetFallbackRepos())
                {
                    if (repos.Count >= 8) break;
                    if (repos.Any(r => r.Name == mock.Name)) continue;
                    repos.Add(mock);
                }
            }

            return repos.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load repos");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<RepoInfo>.Empty;
        }
    });

    public IListFeed<DepUpdateInfo> DepUpdates => ListFeed.Async(async ct =>
        GetMockDepUpdates().ToImmutableList());

    public IFeed<int> TotalRepos => Feed.Async(async ct =>
    {
        var repos = await Repos;
        return repos?.Count ?? 0;
    });

    public IFeed<int> UncommittedCount => Feed.Async(async ct =>
    {
        var repos = await Repos;
        return repos?.Count(r => r.IsDirty) ?? 0;
    });

    public IFeed<int> ChangedCount => Feed.Async(async ct =>
    {
        var repos = await Repos;
        return repos?.Count(r => r.IsDirty) ?? 0;
    });

    public IFeed<int> DepUpdateCount => Feed.Async(async ct =>
    {
        var deps = await DepUpdates;
        return deps?.Count ?? 0;
    });

    public IFeed<int> LastPushMinutes => Feed.Async(async ct => 14);

    private static void ApplyMockEnrichment(
        RepoInfo repo,
        List<(string TechStack, bool IsDirty, int Additions, int Deletions, int DepCount, double[] LanguageSegments, string[] LanguageColorHexes, string Branch)> enrichments,
        int index)
    {
        if (index >= enrichments.Count) return;
        var e = enrichments[index];
        repo.TechStack = e.TechStack;
        repo.IsDirty = e.IsDirty;
        repo.Additions = e.Additions;
        repo.Deletions = e.Deletions;
        repo.DepCount = e.DepCount;
        repo.LanguageSegments = e.LanguageSegments;
        repo.LanguageColorHexes = e.LanguageColorHexes;
        if (string.IsNullOrEmpty(repo.LastBranch))
            repo.LastBranch = e.Branch;
    }

    private static List<(string TechStack, bool IsDirty, int Additions, int Deletions, int DepCount, double[] LanguageSegments, string[] LanguageColorHexes, string Branch)> GetMockEnrichments()
    {
        return new()
        {
            ("Node.js REST API \u00b7 TypeScript", true, 47, 12, 3, [55, 25, 12, 8], ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"], "feat/auth-refactor"),
            ("React SPA \u00b7 TypeScript", false, 0, 0, 1, [60, 20, 15, 5], ["#89B4FA", "#F9E2AF", "#94E2D5", "#F5C2E7"], "main"),
            ("Python ML Pipeline \u00b7 FastAPI", true, 23, 8, 2, [70, 15, 10, 5], ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"], "feat/model-v2"),
            (".NET Microservice \u00b7 C#", false, 0, 0, 0, [65, 20, 10, 5], ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"], "main"),
            ("Go CLI Tool \u00b7 Cobra", true, 15, 3, 1, [80, 10, 5, 5], ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"], "fix/output-format"),
            ("Rust WebAssembly \u00b7 wasm-pack", false, 0, 0, 1, [75, 15, 5, 5], ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"], "main"),
        };
    }

    private static List<RepoInfo> GetFallbackRepos()
    {
        return new()
        {
            new RepoInfo
            {
                Name = "api-gateway",
                Path = "C:\\Projects\\api-gateway",
                SessionCount = 8,
                LastActivity = DateTime.UtcNow.AddMinutes(-14),
                LastBranch = "feat/auth-refactor",
                TechStack = "Node.js REST API \u00b7 TypeScript",
                IsDirty = true,
                Additions = 47,
                Deletions = 12,
                DepCount = 3,
                LanguageSegments = [55, 25, 12, 8],
                LanguageColorHexes = ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"]
            },
            new RepoInfo
            {
                Name = "dashboard-ui",
                Path = "C:\\Projects\\dashboard-ui",
                SessionCount = 12,
                LastActivity = DateTime.UtcNow.AddHours(-2),
                LastBranch = "main",
                TechStack = "React SPA \u00b7 TypeScript",
                IsDirty = false,
                Additions = 0,
                Deletions = 0,
                DepCount = 1,
                LanguageSegments = [60, 20, 15, 5],
                LanguageColorHexes = ["#89B4FA", "#F9E2AF", "#94E2D5", "#F5C2E7"]
            },
            new RepoInfo
            {
                Name = "ml-pipeline",
                Path = "C:\\Projects\\ml-pipeline",
                SessionCount = 5,
                LastActivity = DateTime.UtcNow.AddHours(-6),
                LastBranch = "feat/model-v2",
                TechStack = "Python ML Pipeline \u00b7 FastAPI",
                IsDirty = true,
                Additions = 23,
                Deletions = 8,
                DepCount = 2,
                LanguageSegments = [70, 15, 10, 5],
                LanguageColorHexes = ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"]
            },
            new RepoInfo
            {
                Name = "auth-service",
                Path = "C:\\Projects\\auth-service",
                SessionCount = 3,
                LastActivity = DateTime.UtcNow.AddDays(-1),
                LastBranch = "main",
                TechStack = ".NET Microservice \u00b7 C#",
                IsDirty = false,
                Additions = 0,
                Deletions = 0,
                DepCount = 0,
                LanguageSegments = [65, 20, 10, 5],
                LanguageColorHexes = ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"]
            },
            new RepoInfo
            {
                Name = "cli-tools",
                Path = "C:\\Projects\\cli-tools",
                SessionCount = 4,
                LastActivity = DateTime.UtcNow.AddHours(-3),
                LastBranch = "fix/output-format",
                TechStack = "Go CLI Tool \u00b7 Cobra",
                IsDirty = true,
                Additions = 15,
                Deletions = 3,
                DepCount = 1,
                LanguageSegments = [80, 10, 5, 5],
                LanguageColorHexes = ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"]
            },
            new RepoInfo
            {
                Name = "wasm-renderer",
                Path = "C:\\Projects\\wasm-renderer",
                SessionCount = 2,
                LastActivity = DateTime.UtcNow.AddDays(-2),
                LastBranch = "main",
                TechStack = "Rust WebAssembly \u00b7 wasm-pack",
                IsDirty = false,
                Additions = 0,
                Deletions = 0,
                DepCount = 1,
                LanguageSegments = [75, 15, 5, 5],
                LanguageColorHexes = ["#89B4FA", "#94E2D5", "#F9E2AF", "#F5C2E7"]
            },
        };
    }

    private static List<DepUpdateInfo> GetMockDepUpdates()
    {
        return new()
        {
            new DepUpdateInfo
            {
                PackageName = "express",
                CurrentVersion = "4.18.2",
                LatestVersion = "5.0.1",
                UpdateType = "major",
                RepoName = "api-gateway"
            },
            new DepUpdateInfo
            {
                PackageName = "react",
                CurrentVersion = "18.2.0",
                LatestVersion = "19.1.0",
                UpdateType = "major",
                RepoName = "dashboard-ui"
            },
            new DepUpdateInfo
            {
                PackageName = "fastapi",
                CurrentVersion = "0.104.1",
                LatestVersion = "0.109.0",
                UpdateType = "minor",
                RepoName = "ml-pipeline"
            },
            new DepUpdateInfo
            {
                PackageName = "typescript",
                CurrentVersion = "5.3.2",
                LatestVersion = "5.3.3",
                UpdateType = "patch",
                RepoName = "api-gateway"
            },
        };
    }
}
