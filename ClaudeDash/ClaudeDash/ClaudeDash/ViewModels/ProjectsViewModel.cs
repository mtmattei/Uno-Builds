using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record ProjectsModel
{
    private readonly IProjectScannerService _scannerService;
    private readonly ILogger<ProjectsModel> _logger;

    public ProjectsModel(
        IProjectScannerService scannerService,
        ILogger<ProjectsModel> logger)
    {
        _scannerService = scannerService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<ProjectInfo> Projects => ListFeed.Async(async ct =>
    {
        try
        {
            var projects = await _scannerService.GetAllProjectsAsync();
            return projects.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load projects");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<ProjectInfo>.Empty;
        }
    });

    public IFeed<int> TotalProjects => Feed.Async(async ct =>
    {
        var projects = await Projects;
        return projects?.Count ?? 0;
    });

    public IFeed<int> GitRepos => Feed.Async(async ct =>
    {
        var projects = await Projects;
        return projects?.Count(p => p.IsGitRepo) ?? 0;
    });

    public IFeed<int> WithClaudeMd => Feed.Async(async ct =>
    {
        var projects = await Projects;
        return projects?.Count(p => p.HasClaudeMd) ?? 0;
    });

    public IFeed<int> ActiveWorktrees => Feed.Async(async ct =>
    {
        var projects = await Projects;
        return projects?.Sum(p => p.WorktreeCount) ?? 0;
    });
}
