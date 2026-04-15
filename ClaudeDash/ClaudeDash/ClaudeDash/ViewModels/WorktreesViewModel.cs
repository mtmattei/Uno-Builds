using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record WorktreesModel
{
    private readonly IWorktreeService _worktreeService;
    private readonly IProjectScannerService _scannerService;
    private readonly ILogger<WorktreesModel> _logger;

    public WorktreesModel(
        IWorktreeService worktreeService,
        IProjectScannerService scannerService,
        ILogger<WorktreesModel> logger)
    {
        _worktreeService = worktreeService;
        _scannerService = scannerService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);
    public IState<int> TotalRepos => State.Value(this, () => 0);

    public IListFeed<WorktreeInfo> Worktrees => ListFeed.Async(async ct =>
    {
        try
        {
            var projects = await _scannerService.GetAllProjectsAsync();
            var gitProjects = projects.Where(p => p.IsGitRepo && p.PathExists).ToList();

            var allWorktrees = new List<WorktreeInfo>();
            var reposWithWorktrees = 0;

            foreach (var project in gitProjects)
            {
                var wts = await _worktreeService.ListWorktreesAsync(project.Path);
                if (wts.Count > 0) reposWithWorktrees++;
                allWorktrees.AddRange(wts);
            }

            await TotalRepos.Set(reposWithWorktrees, ct);

            return allWorktrees.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load worktrees");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<WorktreeInfo>.Empty;
        }
    });

    public IFeed<int> TotalWorktrees => Feed.Async(async ct =>
    {
        var wts = await Worktrees;
        return wts?.Count ?? 0;
    });
}
