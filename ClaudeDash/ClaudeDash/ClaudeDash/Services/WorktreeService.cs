using System.Diagnostics;

namespace ClaudeDash.Services;

public class WorktreeService : IWorktreeService
{
    public async Task<List<WorktreeInfo>> ListWorktreesAsync(string repoPath)
    {
        var worktrees = new List<WorktreeInfo>();
        var output = await RunGitAsync(repoPath, "worktree list --porcelain");
        if (string.IsNullOrEmpty(output))
            return worktrees;

        WorktreeInfo? current = null;
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("worktree "))
            {
                current = new WorktreeInfo
                {
                    Path = trimmed["worktree ".Length..],
                    RepoPath = repoPath
                };
                worktrees.Add(current);
            }
            else if (current != null && trimmed.StartsWith("HEAD "))
            {
                var fullHash = trimmed["HEAD ".Length..];
                current.HeadShort = fullHash.Length >= 7 ? fullHash[..7] : fullHash;
            }
            else if (current != null && trimmed.StartsWith("branch "))
            {
                var refPath = trimmed["branch ".Length..];
                // refs/heads/main -> main
                current.Branch = refPath.StartsWith("refs/heads/")
                    ? refPath["refs/heads/".Length..]
                    : refPath;
                current.IsMain = current.Branch is "main" or "master";
            }
            else if (current != null && trimmed == "bare")
            {
                current.IsMain = true;
            }
        }

        return worktrees;
    }

    public async Task<WorktreeInfo> CreateWorktreeAsync(string repoPath, string branchName, string? baseBranch = null)
    {
        // Create worktree in .claude/worktrees/{branchName} relative to repo
        var worktreeDir = System.IO.Path.Combine(repoPath, ".claude", "worktrees", branchName);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(worktreeDir)!);

        var args = baseBranch != null
            ? $"worktree add \"{worktreeDir}\" -b {branchName} {baseBranch}"
            : $"worktree add \"{worktreeDir}\" -b {branchName}";

        var output = await RunGitAsync(repoPath, args);

        // Get HEAD of new worktree
        var head = await RunGitAsync(worktreeDir, "rev-parse --short HEAD");

        return new WorktreeInfo
        {
            Path = worktreeDir,
            Branch = branchName,
            HeadShort = head.Trim(),
            IsMain = false,
            RepoPath = repoPath
        };
    }

    public async Task RemoveWorktreeAsync(string repoPath, string worktreePath)
    {
        await RunGitAsync(repoPath, $"worktree remove \"{worktreePath}\" --force");
    }

    private static async Task<string> RunGitAsync(string workingDirectory, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc == null) return "";
            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();
            return output;
        }
        catch
        {
            return "";
        }
    }
}
