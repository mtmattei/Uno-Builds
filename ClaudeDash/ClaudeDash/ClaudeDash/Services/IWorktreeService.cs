namespace ClaudeDash.Services;

public interface IWorktreeService
{
    Task<List<WorktreeInfo>> ListWorktreesAsync(string repoPath);
    Task<WorktreeInfo> CreateWorktreeAsync(string repoPath, string branchName, string? baseBranch = null);
    Task RemoveWorktreeAsync(string repoPath, string worktreePath);
}
