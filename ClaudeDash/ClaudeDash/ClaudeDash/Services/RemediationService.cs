using System.Diagnostics;
using Path = System.IO.Path;

namespace ClaudeDash.Services;

public class RemediationService : IRemediationService
{
    private readonly IClaudeConfigService _configService;
    private readonly IWorktreeService _worktreeService;
    private readonly IProjectScannerService _projectScanner;
    private readonly ILogger<RemediationService> _logger;

    private readonly HashSet<string> _dismissed = [];

    private static readonly string ClaudeDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    public RemediationService(
        IClaudeConfigService configService,
        IWorktreeService worktreeService,
        IProjectScannerService projectScanner,
        ILogger<RemediationService> logger)
    {
        _configService = configService;
        _worktreeService = worktreeService;
        _projectScanner = projectScanner;
        _logger = logger;
    }

    public async Task<RemediationScanResult> ScanAsync()
    {
        var sw = Stopwatch.StartNew();
        var items = new List<RemediationItem>();

        // Run all checks in parallel
        var tasks = new[]
        {
            CheckOrphanedWorktreesAsync(items),
            CheckLargeSessionFilesAsync(items),
            CheckStaleSessionsAsync(items),
            CheckMissingConfigAsync(items),
            CheckMcpHealthAsync(items),
            CheckGitHygieneAsync(items),
            CheckDiskUsageAsync(items)
        };

        await Task.WhenAll(tasks);

        sw.Stop();

        // Remove dismissed items
        items.RemoveAll(i => _dismissed.Contains(i.Id));

        // Sort: errors first, then warnings, then info
        items.Sort((a, b) =>
        {
            var severityOrder = b.Severity.CompareTo(a.Severity);
            if (severityOrder != 0) return severityOrder;
            return string.Compare(a.Category.ToString(), b.Category.ToString(), StringComparison.Ordinal);
        });

        return new RemediationScanResult
        {
            Items = items,
            ScanDuration = sw.Elapsed
        };
    }

    public async Task<FixResult> ApplyFixAsync(RemediationItem item)
    {
        try
        {
            return item.Category switch
            {
                RemediationCategory.Worktrees => await FixWorktreeAsync(item),
                RemediationCategory.Sessions => await FixSessionAsync(item),
                RemediationCategory.Config => await FixConfigAsync(item),
                RemediationCategory.Git => await FixGitAsync(item),
                RemediationCategory.DiskUsage => await FixDiskAsync(item),
                _ => FixResult.Fail($"No fix available for category {item.Category}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply fix for {Title}", item.Title);
            return FixResult.Fail($"Fix failed: {ex.Message}");
        }
    }

    public Task<FixResult> DismissAsync(RemediationItem item)
    {
        _dismissed.Add(item.Id);
        return Task.FromResult(FixResult.Ok("Dismissed"));
    }

    // ─── Check: Orphaned worktrees ───

    private async Task CheckOrphanedWorktreesAsync(List<RemediationItem> items)
    {
        try
        {
            var projects = await _projectScanner.GetAllProjectsAsync();
            foreach (var project in projects)
            {
                if (string.IsNullOrEmpty(project.Path)) continue;

                try
                {
                    var worktrees = await _worktreeService.ListWorktreesAsync(project.Path);
                    foreach (var wt in worktrees.Where(w => !w.IsMain))
                    {
                        // Check if the worktree directory still exists and has recent activity
                        if (!Directory.Exists(wt.Path))
                        {
                            items.Add(new RemediationItem
                            {
                                Category = RemediationCategory.Worktrees,
                                Severity = RemediationSeverity.Warning,
                                Title = $"Orphaned worktree: {Path.GetFileName(wt.Path)}",
                                Description = $"Worktree directory missing at {wt.Path} (branch: {wt.Branch})",
                                FixLabel = "Prune",
                                IsFixable = true,
                                TargetPath = $"{wt.RepoPath}|{wt.Path}",
                                Impact = "Cleans stale git reference"
                            });
                            continue;
                        }

                        // Check for stale worktrees (no commits in 30+ days)
                        var lastModified = GetDirectoryLastWrite(wt.Path);
                        if (lastModified < DateTime.UtcNow.AddDays(-30))
                        {
                            items.Add(new RemediationItem
                            {
                                Category = RemediationCategory.Worktrees,
                                Severity = RemediationSeverity.Info,
                                Title = $"Stale worktree: {Path.GetFileName(wt.Path)}",
                                Description = $"No activity for {(DateTime.UtcNow - lastModified).Days} days (branch: {wt.Branch})",
                                FixLabel = "Remove",
                                IsFixable = true,
                                TargetPath = $"{wt.RepoPath}|{wt.Path}",
                                Impact = "Frees disk space"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Skipping worktree check for {Path}", project.Path);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check worktrees");
        }
    }

    // ─── Check: Large session files ───

    private Task CheckLargeSessionFilesAsync(List<RemediationItem> items)
    {
        try
        {
            var projectsDir = Path.Combine(ClaudeDir, "projects");
            if (!Directory.Exists(projectsDir)) return Task.CompletedTask;

            var jsonlFiles = Directory.GetFiles(projectsDir, "*.jsonl", SearchOption.AllDirectories);
            long totalSize = 0;
            var largeFiles = new List<(string path, long size)>();

            foreach (var file in jsonlFiles)
            {
                try
                {
                    var info = new FileInfo(file);
                    totalSize += info.Length;

                    // Flag files over 50MB
                    if (info.Length > 50 * 1024 * 1024)
                    {
                        largeFiles.Add((file, info.Length));
                    }
                }
                catch { }
            }

            foreach (var (path, size) in largeFiles)
            {
                var sizeMb = size / (1024.0 * 1024.0);
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.Sessions,
                    Severity = RemediationSeverity.Warning,
                    Title = $"Large session file ({sizeMb:F0} MB)",
                    Description = $"{Path.GetFileName(path)}",
                    FixLabel = "Archive",
                    IsFixable = false, // Don't auto-delete session files
                    TargetPath = path,
                    Impact = $"~{sizeMb:F0} MB"
                });
            }

            // Total session storage over 500MB
            var totalMb = totalSize / (1024.0 * 1024.0);
            if (totalMb > 500)
            {
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.DiskUsage,
                    Severity = RemediationSeverity.Warning,
                    Title = $"Session storage: {totalMb:F0} MB",
                    Description = $"{jsonlFiles.Length} session files consuming {totalMb:F0} MB total",
                    IsFixable = false,
                    Impact = $"{totalMb:F0} MB"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check session file sizes");
        }

        return Task.CompletedTask;
    }

    // ─── Check: Stale sessions (old .jsonl with no recent activity) ───

    private async Task CheckStaleSessionsAsync(List<RemediationItem> items)
    {
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(200);
            var staleCount = sessions.Count(s =>
                s.Status == "completed" && s.LastActivity < DateTime.UtcNow.AddDays(-90));

            if (staleCount > 20)
            {
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.Sessions,
                    Severity = RemediationSeverity.Info,
                    Title = $"{staleCount} sessions older than 90 days",
                    Description = "Old completed sessions that could be archived",
                    IsFixable = false,
                    Impact = "Reduces clutter"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check stale sessions");
        }
    }

    // ─── Check: Missing or broken config ───

    private Task CheckMissingConfigAsync(List<RemediationItem> items)
    {
        try
        {
            // Check for settings.json
            var settingsPath = Path.Combine(ClaudeDir, "settings.json");
            if (!File.Exists(settingsPath))
            {
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.Config,
                    Severity = RemediationSeverity.Warning,
                    Title = "Missing settings.json",
                    Description = "No Claude Code settings file found at ~/.claude/settings.json",
                    FixLabel = "Create",
                    IsFixable = true,
                    TargetPath = settingsPath,
                    Impact = "Restores default config"
                });
            }
            else
            {
                // Validate JSON
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    JsonDocument.Parse(json);
                }
                catch
                {
                    items.Add(new RemediationItem
                    {
                        Category = RemediationCategory.Config,
                        Severity = RemediationSeverity.Error,
                        Title = "Malformed settings.json",
                        Description = "settings.json contains invalid JSON",
                        FixLabel = "Open",
                        IsFixable = false,
                        TargetPath = settingsPath,
                        Impact = "May break Claude Code"
                    });
                }
            }

            // Check for CLAUDE.md
            var claudeMdPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".claude", "CLAUDE.md");
            if (!File.Exists(claudeMdPath))
            {
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.Config,
                    Severity = RemediationSeverity.Info,
                    Title = "No global CLAUDE.md",
                    Description = "A global CLAUDE.md provides default instructions for all projects",
                    FixLabel = "Create",
                    IsFixable = true,
                    TargetPath = claudeMdPath,
                    Impact = "Better AI behavior"
                });
            }

            // Check MCP config
            var mcpPath = Path.Combine(ClaudeDir, "mcp.json");
            if (File.Exists(mcpPath))
            {
                try
                {
                    var json = File.ReadAllText(mcpPath);
                    JsonDocument.Parse(json);
                }
                catch
                {
                    items.Add(new RemediationItem
                    {
                        Category = RemediationCategory.Config,
                        Severity = RemediationSeverity.Error,
                        Title = "Malformed mcp.json",
                        Description = "MCP configuration contains invalid JSON",
                        FixLabel = "Open",
                        IsFixable = false,
                        TargetPath = mcpPath,
                        Impact = "MCP servers won't load"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check config files");
        }

        return Task.CompletedTask;
    }

    // ─── Check: MCP server health ───

    private async Task CheckMcpHealthAsync(List<RemediationItem> items)
    {
        try
        {
            var servers = await _configService.GetMcpServersAsync();

            foreach (var server in servers)
            {
                // Check if the server command exists
                if (!string.IsNullOrEmpty(server.Command))
                {
                    var cmd = server.Command.Split(' ')[0];
                    if (cmd is "npx" or "node" or "python" or "python3" or "uvx")
                    {
                        // These are runtime commands, check if the runtime is available
                        if (!IsCommandAvailable(cmd))
                        {
                            items.Add(new RemediationItem
                            {
                                Category = RemediationCategory.McpServers,
                                Severity = RemediationSeverity.Error,
                                Title = $"MCP runtime missing: {cmd}",
                                Description = $"Server '{server.Name}' requires '{cmd}' which is not on PATH",
                                IsFixable = false,
                                Impact = $"'{server.Name}' won't start"
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check MCP health");
        }
    }

    // ─── Check: Git hygiene ───

    private async Task CheckGitHygieneAsync(List<RemediationItem> items)
    {
        try
        {
            var projects = await _projectScanner.GetAllProjectsAsync();

            foreach (var project in projects)
            {
                if (string.IsNullOrEmpty(project.Path)) continue;

                try
                {
                    var gitDir = Path.Combine(project.Path, ".git");
                    if (!Directory.Exists(gitDir) && !File.Exists(gitDir)) continue;

                    // Check for large .git directory
                    var gitSize = GetDirectorySize(gitDir);
                    var gitSizeMb = gitSize / (1024.0 * 1024.0);

                    if (gitSizeMb > 500)
                    {
                        items.Add(new RemediationItem
                        {
                            Category = RemediationCategory.Git,
                            Severity = RemediationSeverity.Warning,
                            Title = $"Large .git: {Path.GetFileName(project.Path)} ({gitSizeMb:F0} MB)",
                            Description = "Run 'git gc' to compact the repository",
                            FixLabel = "git gc",
                            IsFixable = true,
                            TargetPath = project.Path,
                            Impact = $"May recover {gitSizeMb * 0.3:F0} MB"
                        });
                    }

                    // Check for untracked CLAUDE.md files (project-level)
                    var projectClaudeMd = Path.Combine(project.Path, "CLAUDE.md");
                    if (!File.Exists(projectClaudeMd))
                    {
                        items.Add(new RemediationItem
                        {
                            Category = RemediationCategory.Config,
                            Severity = RemediationSeverity.Info,
                            Title = $"No CLAUDE.md in {Path.GetFileName(project.Path)}",
                            Description = "A project-level CLAUDE.md improves Claude's understanding of this project",
                            FixLabel = "Create",
                            IsFixable = true,
                            TargetPath = projectClaudeMd,
                            Impact = "Better project-specific AI behavior"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Git check failed for {Path}", project.Path);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check git hygiene");
        }
    }

    // ─── Check: Disk usage ───

    private Task CheckDiskUsageAsync(List<RemediationItem> items)
    {
        try
        {
            if (!Directory.Exists(ClaudeDir)) return Task.CompletedTask;

            var totalSize = GetDirectorySize(ClaudeDir);
            var totalMb = totalSize / (1024.0 * 1024.0);

            if (totalMb > 1000)
            {
                items.Add(new RemediationItem
                {
                    Category = RemediationCategory.DiskUsage,
                    Severity = RemediationSeverity.Warning,
                    Title = $"~/.claude is {totalMb:F0} MB",
                    Description = "The Claude configuration directory is using significant disk space",
                    IsFixable = false,
                    Impact = $"{totalMb:F0} MB"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check disk usage");
        }

        return Task.CompletedTask;
    }

    // ─── Fix implementations ───

    private async Task<FixResult> FixWorktreeAsync(RemediationItem item)
    {
        if (string.IsNullOrEmpty(item.TargetPath)) return FixResult.Fail("No target path");

        var parts = item.TargetPath.Split('|');
        if (parts.Length != 2) return FixResult.Fail("Invalid target format");

        var repoPath = parts[0];
        var worktreePath = parts[1];

        await _worktreeService.RemoveWorktreeAsync(repoPath, worktreePath);
        return FixResult.Ok($"Removed worktree at {Path.GetFileName(worktreePath)}");
    }

    private Task<FixResult> FixSessionAsync(RemediationItem item)
    {
        // Session fixes are manual for safety
        return Task.FromResult(FixResult.Fail("Session cleanup must be done manually"));
    }

    private Task<FixResult> FixConfigAsync(RemediationItem item)
    {
        if (string.IsNullOrEmpty(item.TargetPath)) return Task.FromResult(FixResult.Fail("No target path"));

        var fileName = Path.GetFileName(item.TargetPath);

        if (fileName == "settings.json" && !File.Exists(item.TargetPath))
        {
            var dir = Path.GetDirectoryName(item.TargetPath);
            if (dir != null) Directory.CreateDirectory(dir);
            File.WriteAllText(item.TargetPath, "{\n  \"permissions\": {},\n  \"env\": {}\n}\n");
            return Task.FromResult(FixResult.Ok("Created default settings.json"));
        }

        if (fileName == "CLAUDE.md" && !File.Exists(item.TargetPath))
        {
            var dir = Path.GetDirectoryName(item.TargetPath);
            if (dir != null) Directory.CreateDirectory(dir);
            File.WriteAllText(item.TargetPath, "# Project Instructions\n\n- Add your custom instructions here\n");
            return Task.FromResult(FixResult.Ok("Created CLAUDE.md template"));
        }

        return Task.FromResult(FixResult.Fail("Cannot auto-fix this config issue"));
    }

    private Task<FixResult> FixGitAsync(RemediationItem item)
    {
        if (string.IsNullOrEmpty(item.TargetPath)) return Task.FromResult(FixResult.Fail("No target path"));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "gc --aggressive --prune=now",
                WorkingDirectory = item.TargetPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit(60_000); // 60 second timeout

            return Task.FromResult(process?.ExitCode == 0
                ? FixResult.Ok("git gc completed successfully")
                : FixResult.Fail("git gc failed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(FixResult.Fail($"git gc failed: {ex.Message}"));
        }
    }

    private Task<FixResult> FixDiskAsync(RemediationItem item)
    {
        return Task.FromResult(FixResult.Fail("Disk cleanup must be done manually"));
    }

    // ─── Helpers ───

    private static DateTime GetDirectoryLastWrite(string path)
    {
        try
        {
            return Directory.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    private static long GetDirectorySize(string path)
    {
        try
        {
            if (File.Exists(path)) // Could be a .git file (worktree pointer)
                return new FileInfo(path).Length;

            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0; } });
        }
        catch
        {
            return 0;
        }
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(5000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
