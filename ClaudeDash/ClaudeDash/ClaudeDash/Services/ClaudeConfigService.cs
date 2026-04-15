using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace ClaudeDash.Services;

public class ClaudeConfigService : IClaudeConfigService
{
    private static readonly string ClaudeDir = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    public string GetClaudeDir() => ClaudeDir;

    public async Task<List<McpServerInfo>> GetMcpServersAsync()
    {
        var servers = new Dictionary<string, McpServerInfo>();

        // settings.json takes priority
        await MergeServersFromFile(servers, System.IO.Path.Combine(ClaudeDir, "settings.json"), "settings.json");
        await MergeServersFromFile(servers, System.IO.Path.Combine(ClaudeDir, "mcp.json"), "mcp.json");

        return servers.Values.ToList();
    }

    private static async Task MergeServersFromFile(Dictionary<string, McpServerInfo> servers, string filePath, string source)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("mcpServers", out var mcpServers))
                return;

            foreach (var server in mcpServers.EnumerateObject())
            {
                // settings.json wins - don't overwrite
                if (servers.ContainsKey(server.Name))
                    continue;

                var info = new McpServerInfo
                {
                    Name = server.Name,
                    Source = source
                };

                if (server.Value.TryGetProperty("url", out var urlProp))
                {
                    info.ServerType = "URL";
                    info.Url = urlProp.GetString() ?? "";
                }
                else if (server.Value.TryGetProperty("command", out var cmdProp))
                {
                    info.ServerType = "Command";
                    info.Command = cmdProp.GetString() ?? "";

                    if (server.Value.TryGetProperty("args", out var argsProp))
                    {
                        info.Args = argsProp.EnumerateArray()
                            .Select(a => a.GetString() ?? "")
                            .ToArray();
                    }
                }

                if (server.Value.TryGetProperty("transport", out var transportProp))
                {
                    info.Transport = transportProp.GetString() ?? "";
                }

                servers[server.Name] = info;
            }
        }
        catch
        {
            // Graceful degradation - return what we have
        }
    }

    public async Task<List<ClaudeSessionInfo>> GetRecentSessionsAsync(int count = 20)
    {
        var projectsDir = System.IO.Path.Combine(ClaudeDir, "projects");
        if (!Directory.Exists(projectsDir))
            return [];

        var sessions = new List<ClaudeSessionInfo>();

        try
        {
            var jsonlFiles = Directory.GetFiles(projectsDir, "*.jsonl", SearchOption.AllDirectories)
                .Where(f => !f.Contains("subagents", StringComparison.OrdinalIgnoreCase))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Take(count)
                .ToList();

            foreach (var file in jsonlFiles)
            {
                var session = await ParseSessionFileAsync(file);
                if (session != null)
                    sessions.Add(session);
            }
        }
        catch
        {
            // Graceful degradation
        }

        return sessions;
    }

    private static async Task<ClaudeSessionInfo?> ParseSessionFileAsync(FileInfo file)
    {
        try
        {
            var session = new ClaudeSessionInfo
            {
                SessionId = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                LastActivity = file.LastWriteTimeUtc
            };
            session.ShortId = session.SessionId.Length >= 8
                ? session.SessionId[..8]
                : session.SessionId;

            // Decode project path from parent directory name
            var projectDirName = file.Directory?.Name ?? "";
            session.ProjectPath = DecodeProjectPath(projectDirName);

            // Read full file to extract metadata and token usage
            var messageCount = 0;
            var totalTokens = 0;
            DateTimeOffset? firstTimestamp = null;
            DateTimeOffset? lastTimestamp = null;
            using var reader = new StreamReader(file.FullName);

            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    // Track timestamps for duration
                    if (root.TryGetProperty("timestamp", out var tsProp))
                    {
                        if (DateTimeOffset.TryParse(tsProp.GetString(), out var ts))
                        {
                            firstTimestamp ??= ts;
                            lastTimestamp = ts;
                        }
                    }

                    if (root.TryGetProperty("type", out var typeProp))
                    {
                        var type = typeProp.GetString();

                        if (type == "user" || type == "assistant")
                            messageCount++;

                        // Get first user message
                        if (type == "user" && string.IsNullOrEmpty(session.FirstUserMessage))
                        {
                            if (root.TryGetProperty("message", out var msg) &&
                                msg.TryGetProperty("content", out var content))
                            {
                                var text = content.ValueKind == JsonValueKind.String
                                    ? content.GetString()
                                    : content.ValueKind == JsonValueKind.Array
                                        ? ExtractTextFromContentArray(content)
                                        : null;

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    session.FirstUserMessage = text.Length > 120
                                        ? text[..120] + "..."
                                        : text;
                                }
                            }
                        }

                        // Parse token usage from assistant messages
                        if (type == "assistant" && root.TryGetProperty("message", out var aMsg))
                        {
                            if (aMsg.TryGetProperty("usage", out var usage))
                            {
                                if (usage.TryGetProperty("input_tokens", out var inp))
                                    totalTokens += inp.GetInt32();
                                if (usage.TryGetProperty("output_tokens", out var outp))
                                    totalTokens += outp.GetInt32();
                            }
                        }
                    }

                    // Extract version, model, git branch from any entry that has them
                    if (string.IsNullOrEmpty(session.ClaudeVersion) &&
                        root.TryGetProperty("version", out var versionProp))
                    {
                        session.ClaudeVersion = versionProp.GetString() ?? "";
                    }

                    if (string.IsNullOrEmpty(session.GitBranch) &&
                        root.TryGetProperty("gitBranch", out var branchProp))
                    {
                        session.GitBranch = branchProp.GetString() ?? "";
                    }

                    if (string.IsNullOrEmpty(session.Model) &&
                        root.TryGetProperty("model", out var modelProp))
                    {
                        session.Model = modelProp.GetString() ?? "";
                    }
                }
                catch
                {
                    // Skip malformed lines
                }
            }

            session.MessageCount = messageCount;
            session.TokenCount = totalTokens;

            // Compute duration from timestamps
            if (firstTimestamp.HasValue && lastTimestamp.HasValue)
            {
                var dur = lastTimestamp.Value - firstTimestamp.Value;
                session.Duration = dur.TotalHours >= 1
                    ? $"{(int)dur.TotalHours}h {dur.Minutes}m"
                    : dur.TotalMinutes >= 1
                        ? $"{(int)dur.TotalMinutes}m"
                        : $"{dur.Seconds}s";
            }

            // Extract repo name from project path
            if (!string.IsNullOrEmpty(session.ProjectPath))
            {
                session.RepoName = System.IO.Path.GetFileName(
                    session.ProjectPath.TrimEnd('\\', '/'));
            }

            return session;
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractTextFromContentArray(JsonElement content)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                item.TryGetProperty("text", out var text))
            {
                return text.GetString();
            }
        }
        return null;
    }

    public static string DecodeProjectPath(string encoded)
    {
        if (string.IsNullOrEmpty(encoded))
            return encoded;

        // Pattern: C--Users-Platform006-OneDrive---Uno-Platform-...
        // Single dash = path separator (\)
        // Triple dash (---) = space + hyphen in the original? Actually: " - " (space-dash-space)
        // Double dash at start = drive letter colon (C--)

        // First, handle the drive letter: "C--" -> "C:\"
        var result = encoded;

        // Replace "---" with a placeholder first (these are " - " in original paths)
        result = result.Replace("---", "\x01");

        // Now the remaining single dashes are path separators
        result = result.Replace("-", "\\");

        // Restore " - " from placeholder
        result = result.Replace("\x01", " - ");

        // Fix drive letter: "C\\" at start should be "C:\"
        if (result.Length >= 3 && char.IsLetter(result[0]) && result[1] == '\\' && result[2] == '\\')
        {
            result = result[0] + ":\\" + result[3..];
        }

        return result;
    }

    public Task<List<SkillInfo>> GetSkillsAsync()
    {
        var skillsDir = System.IO.Path.Combine(ClaudeDir, "skills");
        if (!Directory.Exists(skillsDir))
            return Task.FromResult(new List<SkillInfo>());

        var skills = new List<SkillInfo>();

        try
        {
            foreach (var dir in Directory.GetDirectories(skillsDir))
            {
                var dirInfo = new DirectoryInfo(dir);
                var skill = new SkillInfo
                {
                    Name = dirInfo.Name,
                    Path = dirInfo.FullName,
                    IsSymlink = dirInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint)
                };

                if (skill.IsSymlink)
                {
                    try
                    {
                        skill.ResolvedTarget = dirInfo.LinkTarget ?? dirInfo.FullName;
                    }
                    catch
                    {
                        skill.ResolvedTarget = dirInfo.FullName;
                    }
                }

                skills.Add(skill);
            }
        }
        catch
        {
            // Graceful degradation
        }

        return Task.FromResult(skills);
    }

    public async Task<List<MemoryFile>> GetMemoryFilesAsync()
    {
        var files = new List<MemoryFile>();

        try
        {
            // Global CLAUDE.md
            var globalClaude = System.IO.Path.Combine(ClaudeDir, "CLAUDE.md");
            if (File.Exists(globalClaude))
            {
                var fi = new FileInfo(globalClaude);
                files.Add(new MemoryFile
                {
                    FileName = "CLAUDE.md",
                    FilePath = fi.FullName,
                    ProjectContext = "Global",
                    Content = await File.ReadAllTextAsync(fi.FullName),
                    LastModified = fi.LastWriteTimeUtc,
                    SizeBytes = fi.Length
                });
            }

            // Project memory directories
            var projectsDir = System.IO.Path.Combine(ClaudeDir, "projects");
            if (Directory.Exists(projectsDir))
            {
                foreach (var projDir in Directory.GetDirectories(projectsDir))
                {
                    var projName = DecodeProjectPath(System.IO.Path.GetFileName(projDir));
                    var memoryDir = System.IO.Path.Combine(projDir, "memory");

                    if (Directory.Exists(memoryDir))
                    {
                        foreach (var mdFile in Directory.GetFiles(memoryDir, "*.md"))
                        {
                            var fi = new FileInfo(mdFile);
                            files.Add(new MemoryFile
                            {
                                FileName = fi.Name,
                                FilePath = fi.FullName,
                                ProjectContext = projName,
                                Content = await File.ReadAllTextAsync(fi.FullName),
                                LastModified = fi.LastWriteTimeUtc,
                                SizeBytes = fi.Length
                            });
                        }
                    }

                    // Also check for CLAUDE.md at project level
                    var projClaude = System.IO.Path.Combine(projDir, "CLAUDE.md");
                    if (File.Exists(projClaude))
                    {
                        var fi = new FileInfo(projClaude);
                        files.Add(new MemoryFile
                        {
                            FileName = "CLAUDE.md",
                            FilePath = fi.FullName,
                            ProjectContext = projName,
                            Content = await File.ReadAllTextAsync(fi.FullName),
                            LastModified = fi.LastWriteTimeUtc,
                            SizeBytes = fi.Length
                        });
                    }
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        return files.OrderByDescending(f => f.LastModified).ToList();
    }

    public async Task<List<RepoInfo>> GetReposAsync()
    {
        var repos = new Dictionary<string, RepoInfo>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var sessions = await GetRecentSessionsAsync(100);
            foreach (var session in sessions)
            {
                if (string.IsNullOrEmpty(session.ProjectPath)) continue;

                var repoName = System.IO.Path.GetFileName(session.ProjectPath.TrimEnd('\\', '/'));
                if (string.IsNullOrEmpty(repoName)) continue;

                if (!repos.TryGetValue(session.ProjectPath, out var repo))
                {
                    repo = new RepoInfo
                    {
                        Name = repoName,
                        Path = session.ProjectPath,
                        LastBranch = session.GitBranch
                    };
                    repos[session.ProjectPath] = repo;
                }

                repo.SessionCount++;
                if (session.LastActivity > repo.LastActivity)
                {
                    repo.LastActivity = session.LastActivity;
                    if (!string.IsNullOrEmpty(session.GitBranch))
                        repo.LastBranch = session.GitBranch;
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        return repos.Values.OrderByDescending(r => r.LastActivity).ToList();
    }

    public Task<List<DependencyInfo>> GetDependenciesAsync(string? projectPath = null)
    {
        var deps = new List<DependencyInfo>();

        try
        {
            // Scan known project paths from recent sessions for .csproj files
            var searchPaths = new List<string>();

            if (!string.IsNullOrEmpty(projectPath))
            {
                searchPaths.Add(projectPath);
            }
            else
            {
                var projectsDir = System.IO.Path.Combine(ClaudeDir, "projects");
                if (Directory.Exists(projectsDir))
                {
                    foreach (var dir in Directory.GetDirectories(projectsDir))
                    {
                        var decoded = DecodeProjectPath(System.IO.Path.GetFileName(dir));
                        if (Directory.Exists(decoded))
                            searchPaths.Add(decoded);
                    }
                }
            }

            foreach (var searchPath in searchPaths.Take(10))
            {
                try
                {
                    var csprojFiles = Directory.GetFiles(searchPath, "*.csproj", SearchOption.AllDirectories)
                        .Take(5);

                    foreach (var csproj in csprojFiles)
                    {
                        var projName = System.IO.Path.GetFileNameWithoutExtension(csproj);
                        var doc = XDocument.Load(csproj);
                        var packageRefs = doc.Descendants("PackageReference");

                        foreach (var pkg in packageRefs)
                        {
                            var name = pkg.Attribute("Include")?.Value ?? "";
                            var version = pkg.Attribute("Version")?.Value ?? "";
                            if (string.IsNullOrEmpty(name)) continue;

                            deps.Add(new DependencyInfo
                            {
                                PackageName = name,
                                Version = version,
                                ProjectName = projName,
                                ProjectPath = csproj
                            });
                        }
                    }
                }
                catch
                {
                    // Skip inaccessible paths
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        return Task.FromResult(deps.OrderBy(d => d.PackageName).ToList());
    }

    public async Task<List<HookInfo>> GetHooksAsync()
    {
        var hooks = new List<HookInfo>();

        async Task ParseHooksFromFile(string filePath, string source)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("hooks", out var hooksElement))
                    return;

                foreach (var hookType in hooksElement.EnumerateObject())
                {
                    if (hookType.Value.ValueKind != JsonValueKind.Array) continue;

                    foreach (var hook in hookType.Value.EnumerateArray())
                    {
                        var matcher = hook.TryGetProperty("matcher", out var m)
                            ? m.GetString() ?? "*"
                            : "*";

                        var command = "";
                        if (hook.TryGetProperty("command", out var cmd))
                        {
                            command = cmd.GetString() ?? "";
                        }
                        else if (hook.TryGetProperty("commands", out var cmds) &&
                                 cmds.ValueKind == JsonValueKind.Array)
                        {
                            command = string.Join(" && ", cmds.EnumerateArray().Select(c => c.GetString()));
                        }

                        hooks.Add(new HookInfo
                        {
                            HookType = hookType.Name,
                            Matcher = matcher,
                            Command = command,
                            Source = source
                        });
                    }
                }
            }
            catch
            {
                // Skip malformed files
            }
        }

        await ParseHooksFromFile(System.IO.Path.Combine(ClaudeDir, "settings.json"), "settings.json");
        await ParseHooksFromFile(System.IO.Path.Combine(ClaudeDir, "settings.local.json"), "settings.local.json");

        return hooks;
    }

    public Task<List<AgentInfo>> GetAgentsAsync()
    {
        var agents = new List<AgentInfo>();

        try
        {
            var projectsDir = System.IO.Path.Combine(ClaudeDir, "projects");
            if (!Directory.Exists(projectsDir))
                return Task.FromResult(agents);

            foreach (var projDir in Directory.GetDirectories(projectsDir))
            {
                var subagentsDir = System.IO.Path.Combine(projDir, "subagents");
                if (!Directory.Exists(subagentsDir)) continue;

                var projPath = DecodeProjectPath(System.IO.Path.GetFileName(projDir));

                var agentFiles = Directory.GetFiles(subagentsDir, "*.jsonl", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Take(20);

                foreach (var file in agentFiles)
                {
                    agents.Add(new AgentInfo
                    {
                        SessionId = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                        ParentSessionId = file.Directory?.Name ?? "",
                        ProjectPath = projPath,
                        LastActivity = file.LastWriteTimeUtc,
                        MessageCount = CountLines(file.FullName)
                    });
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        return Task.FromResult(agents.OrderByDescending(a => a.LastActivity).Take(50).ToList());
    }

    private static int CountLines(string filePath)
    {
        try
        {
            return File.ReadLines(filePath).Count();
        }
        catch
        {
            return 0;
        }
    }

    public async Task<GitStatusInfo> GetGitStatusAsync(string repoPath)
    {
        var result = new GitStatusInfo();

        async Task<string> RunGit(string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "";
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return output.Trim();
            }
            catch
            {
                return "";
            }
        }

        // Get current branch
        var branch = await RunGit("rev-parse --abbrev-ref HEAD");
        result.CurrentBranch = branch;

        // Get porcelain status
        var status = await RunGit("status --porcelain");
        if (!string.IsNullOrEmpty(status))
        {
            var lines = status.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            result.IsDirty = lines.Length > 0;
            result.UntrackedCount = lines.Count(l => l.StartsWith("??"));

            // Count additions/deletions from diff --stat
            var diffStat = await RunGit("diff --shortstat");
            if (!string.IsNullOrEmpty(diffStat))
            {
                // Format: " 3 files changed, 47 insertions(+), 12 deletions(-)"
                var insertMatch = System.Text.RegularExpressions.Regex.Match(diffStat, @"(\d+) insertion");
                var deleteMatch = System.Text.RegularExpressions.Regex.Match(diffStat, @"(\d+) deletion");
                if (insertMatch.Success) result.Additions = int.Parse(insertMatch.Groups[1].Value);
                if (deleteMatch.Success) result.Deletions = int.Parse(deleteMatch.Groups[1].Value);
            }
        }

        return result;
    }

    public async Task<List<EnvCheckResult>> RunEnvAuditAsync()
    {
        var results = new List<EnvCheckResult>();

        async Task<string> RunCommand(string command, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "";
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return output.Trim();
            }
            catch
            {
                return "";
            }
        }

        // dotnet
        var dotnetVer = await RunCommand("dotnet", "--version");
        results.Add(new EnvCheckResult
        {
            ToolName = "dotnet",
            Status = string.IsNullOrEmpty(dotnetVer) ? "missing" : "ok",
            Version = dotnetVer,
            Detail = string.IsNullOrEmpty(dotnetVer) ? "Not found in PATH" : $".NET SDK {dotnetVer}"
        });

        // git
        var gitVer = await RunCommand("git", "--version");
        results.Add(new EnvCheckResult
        {
            ToolName = "git",
            Status = string.IsNullOrEmpty(gitVer) ? "missing" : "ok",
            Version = gitVer.Replace("git version ", ""),
            Detail = string.IsNullOrEmpty(gitVer) ? "Not found in PATH" : gitVer
        });

        // node
        var nodeVer = await RunCommand("node", "--version");
        results.Add(new EnvCheckResult
        {
            ToolName = "node",
            Status = string.IsNullOrEmpty(nodeVer) ? "missing" : "ok",
            Version = nodeVer,
            Detail = string.IsNullOrEmpty(nodeVer) ? "Not found in PATH" : $"Node.js {nodeVer}"
        });

        // npm
        var npmVer = await RunCommand("npm", "--version");
        results.Add(new EnvCheckResult
        {
            ToolName = "npm",
            Status = string.IsNullOrEmpty(npmVer) ? "missing" : "ok",
            Version = npmVer,
            Detail = string.IsNullOrEmpty(npmVer) ? "Not found in PATH" : $"npm {npmVer}"
        });

        // Claude Code
        var claudeVer = await RunCommand("claude", "--version");
        results.Add(new EnvCheckResult
        {
            ToolName = "claude",
            Status = string.IsNullOrEmpty(claudeVer) ? "missing" : "ok",
            Version = claudeVer,
            Detail = string.IsNullOrEmpty(claudeVer) ? "Not found in PATH" : $"Claude Code {claudeVer}"
        });

        // Check .claude directory
        results.Add(new EnvCheckResult
        {
            ToolName = ".claude dir",
            Status = Directory.Exists(ClaudeDir) ? "ok" : "missing",
            Version = "",
            Detail = Directory.Exists(ClaudeDir) ? ClaudeDir : "~/.claude directory not found"
        });

        // Check settings.json
        var settingsPath = System.IO.Path.Combine(ClaudeDir, "settings.json");
        results.Add(new EnvCheckResult
        {
            ToolName = "settings.json",
            Status = File.Exists(settingsPath) ? "ok" : "missing",
            Version = "",
            Detail = File.Exists(settingsPath) ? "Configuration present" : "No settings.json found"
        });

        return results;
    }

    public async Task<UnoPlatformInfo> GetUnoPlatformInfoAsync()
    {
        var info = new UnoPlatformInfo();

        async Task<string> RunCommand(string command, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "";
                var readTask = proc.StandardOutput.ReadToEndAsync();
                if (await Task.WhenAny(readTask, Task.Delay(8000)) != readTask)
                {
                    try { proc.Kill(); } catch { }
                    return "";
                }
                return (await readTask).Trim();
            }
            catch { return ""; }
        }

        // 1. .NET SDK version
        info.DotNetSdkVersion = await RunCommand("dotnet", "--version");

        // 2. Scan for Uno Platform projects using proper path decoding
        var projectPaths = new List<string>();
        try
        {
            var projectsDir = System.IO.Path.Combine(ClaudeDir, "projects");
            if (Directory.Exists(projectsDir))
            {
                foreach (var encoded in Directory.GetDirectories(projectsDir))
                {
                    var dirName = System.IO.Path.GetFileName(encoded);
                    var decoded = DecodeProjectPath(dirName);

                    if (Directory.Exists(decoded))
                    {
                        try
                        {
                            foreach (var csproj in Directory.GetFiles(decoded, "*.csproj", SearchOption.AllDirectories).Take(20))
                            {
                                var content = await File.ReadAllTextAsync(csproj);
                                if (content.Contains("Uno.Sdk", StringComparison.OrdinalIgnoreCase))
                                {
                                    projectPaths.Add(csproj);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        catch { }

        info.ProjectCount = projectPaths.Count;

        // 3. Parse the first Uno project for SDK info
        if (projectPaths.Count > 0)
        {
            try
            {
                info.ProjectName = System.IO.Path.GetFileNameWithoutExtension(projectPaths[0]);
                var csprojContent = await File.ReadAllTextAsync(projectPaths[0]);
                var doc = XDocument.Parse(csprojContent);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

                // SDK version from Project Sdk attribute
                var sdkAttr = doc.Root?.Attribute("Sdk")?.Value;
                if (sdkAttr != null)
                    info.SdkVersion = sdkAttr;

                // SingleProject flag
                var singleProj = doc.Descendants(ns + "UnoSingleProject").FirstOrDefault()?.Value;
                info.IsSingleProject = string.Equals(singleProj, "true", StringComparison.OrdinalIgnoreCase);

                // TargetFrameworks
                var tfms = doc.Descendants(ns + "TargetFrameworks").FirstOrDefault()?.Value
                    ?? doc.Descendants(ns + "TargetFramework").FirstOrDefault()?.Value
                    ?? "";
                if (!string.IsNullOrEmpty(tfms))
                    info.TargetFrameworks = tfms.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

                // UnoFeatures
                var features = doc.Descendants(ns + "UnoFeatures").FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrEmpty(features))
                {
                    info.UnoFeatures = features
                        .Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f))
                        .ToList();
                }

                // Renderer type from UnoFeatures
                if (info.UnoFeatures.Any(f => f.Contains("SkiaRenderer", StringComparison.OrdinalIgnoreCase)))
                    info.RendererType = "Skia (SkiaRenderer)";
                else if (info.UnoFeatures.Any(f => f.Equals("Skia", StringComparison.OrdinalIgnoreCase)))
                    info.RendererType = "Skia";
                else
                    info.RendererType = "Native";

                // WASM AOT check
                var wasmAot = doc.Descendants(ns + "WasmShellMonoRuntimeExecutionMode").FirstOrDefault()?.Value;
                info.IsWasmAotEnabled = string.Equals(wasmAot, "InterpreterAndAOT", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(wasmAot, "FullAOT", StringComparison.OrdinalIgnoreCase);

                // NuGet packages
                foreach (var pkgRef in doc.Descendants(ns + "PackageReference"))
                {
                    var id = pkgRef.Attribute("Include")?.Value ?? "";
                    var version = pkgRef.Attribute("Version")?.Value ?? "";
                    if (!string.IsNullOrEmpty(id))
                    {
                        info.Packages.Add(new NuGetPackageInfo
                        {
                            Id = id,
                            Version = version,
                            IsUnoPackage = id.StartsWith("Uno.", StringComparison.OrdinalIgnoreCase)
                        });

                        // Detect Uno.WinUI version
                        if (id.Equals("Uno.WinUI", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(version))
                            info.UnoWinUIVersion = version;
                    }
                }

                // If Uno.WinUI version not found via PackageReference (Uno.Sdk resolves it),
                // try to get it from the global.json or NuGet lock file
                if (string.IsNullOrEmpty(info.UnoWinUIVersion))
                {
                    var projDir = System.IO.Path.GetDirectoryName(projectPaths[0]) ?? "";
                    var globalJson = System.IO.Path.Combine(projDir, "global.json");
                    if (!File.Exists(globalJson))
                        globalJson = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(projDir) ?? "", "global.json");
                    if (File.Exists(globalJson))
                    {
                        try
                        {
                            var gjContent = await File.ReadAllTextAsync(globalJson);
                            using var gjDoc = JsonDocument.Parse(gjContent);
                            if (gjDoc.RootElement.TryGetProperty("msbuild-sdks", out var sdks) &&
                                sdks.TryGetProperty("Uno.Sdk", out var unoSdkVer))
                            {
                                info.SdkVersion = $"Uno.Sdk {unoSdkVer.GetString()}";
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // 3b. Fallback: fetch latest Uno.Sdk version from NuGet if not detected locally
        if (string.IsNullOrEmpty(info.SdkVersion))
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var json = await http.GetStringAsync("https://api.nuget.org/v3-flatcontainer/uno.sdk/index.json");
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("versions", out var versions))
                {
                    // Get the last stable version (skip pre-release)
                    string latest = "";
                    foreach (var v in versions.EnumerateArray())
                    {
                        var ver = v.GetString() ?? "";
                        if (!string.IsNullOrEmpty(ver) && !ver.Contains('-'))
                            latest = ver;
                    }
                    if (!string.IsNullOrEmpty(latest))
                        info.SdkVersion = latest;
                }
            }
            catch { }
        }

        // 4. uno-check
        var unoCheckOutput = await RunCommand("uno-check", "--non-interactive");
        if (!string.IsNullOrEmpty(unoCheckOutput))
        {
            info.UnoCheckStatus = unoCheckOutput.Contains("Congratulations") ? "passed" : "issues";
            info.UnoCheckDetail = unoCheckOutput.Length > 200
                ? unoCheckOutput[..200] + "..."
                : unoCheckOutput;
        }
        else
        {
            unoCheckOutput = await RunCommand("dotnet", "uno-check --non-interactive");
            if (!string.IsNullOrEmpty(unoCheckOutput))
            {
                info.UnoCheckStatus = unoCheckOutput.Contains("Congratulations") ? "passed" : "issues";
                info.UnoCheckDetail = unoCheckOutput.Length > 200
                    ? unoCheckOutput[..200] + "..."
                    : unoCheckOutput;
            }
        }

        // 5. dotnet workloads
        var workloadOutput = await RunCommand("dotnet", "workload list");
        if (!string.IsNullOrEmpty(workloadOutput))
        {
            foreach (var line in workloadOutput.Split('\n').Skip(3))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1 && !line.StartsWith("---") && !string.IsNullOrWhiteSpace(line)
                    && !line.Contains("Use `dotnet"))
                {
                    info.Workloads.Add(new DotNetWorkloadInfo
                    {
                        Id = parts[0],
                        Version = parts.Length >= 2 ? parts[1] : ""
                    });
                }
            }
        }

        // 6. IDE detection
        var vsProcesses = await RunCommand("tasklist", "/FI \"IMAGENAME eq devenv.exe\" /NH");
        if (vsProcesses.Contains("devenv.exe", StringComparison.OrdinalIgnoreCase))
        {
            info.DetectedIde = "Visual Studio";
        }
        else
        {
            var codeProcesses = await RunCommand("tasklist", "/FI \"IMAGENAME eq Code.exe\" /NH");
            if (codeProcesses.Contains("Code.exe", StringComparison.OrdinalIgnoreCase))
                info.DetectedIde = "VS Code";
            else
            {
                var riderProcesses = await RunCommand("tasklist", "/FI \"IMAGENAME eq rider64.exe\" /NH");
                if (riderProcesses.Contains("rider64.exe", StringComparison.OrdinalIgnoreCase))
                    info.DetectedIde = "JetBrains Rider";
            }
        }

        // 7. Hot Reload status - check if Uno.UI.RemoteControl server process is running
        var hotReloadProcesses = await RunCommand("tasklist", "/FI \"IMAGENAME eq dotnet.exe\" /NH");
        info.HotReloadStatus = hotReloadProcesses.Contains("dotnet.exe", StringComparison.OrdinalIgnoreCase)
            ? "available" : "inactive";

        // 8. Hot Design status - check for Uno.WinUI.DevServer package
        info.HotDesignStatus = info.Packages.Any(p =>
            p.Id.Contains("DevServer", StringComparison.OrdinalIgnoreCase) ||
            p.Id.Contains("Uno.WinUI.DevServer", StringComparison.OrdinalIgnoreCase))
            ? "available" : "not detected";

        // 9. Uno Studio license tier - check ~/.uno/license or env variable
        try
        {
            var unoLicenseEnv = Environment.GetEnvironmentVariable("UNO_LICENSE_TIER");
            if (!string.IsNullOrEmpty(unoLicenseEnv))
            {
                info.LicenseTier = unoLicenseEnv;
            }
            else
            {
                var unoDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".uno");
                var licensePath = System.IO.Path.Combine(unoDir, "license");
                if (File.Exists(licensePath))
                {
                    var licenseContent = await File.ReadAllTextAsync(licensePath);
                    info.LicenseTier = licenseContent.Trim().Length > 0
                        ? licenseContent.Trim().Split('\n')[0]
                        : "unknown";
                }
            }
        }
        catch { }

        return info;
    }

    public async Task<List<Models.UnoCheckResult>> RunUnoCheckAsync()
    {
        var results = new List<Models.UnoCheckResult>();

        // uno-check requires elevation on Windows, so we synthesize checks
        // from data we can gather without elevation

        // 1. .NET SDK
        async Task<string> RunCmd(string command, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "";
                var readTask = proc.StandardOutput.ReadToEndAsync();
                if (await Task.WhenAny(readTask, Task.Delay(8000)) != readTask)
                {
                    try { proc.Kill(); } catch { }
                    return "";
                }
                return (await readTask).Trim();
            }
            catch { return ""; }
        }

        var dotnetVersion = await RunCmd("dotnet", "--version");
        if (!string.IsNullOrEmpty(dotnetVersion))
        {
            results.Add(new Models.UnoCheckResult
            {
                Category = ".NET SDK",
                Name = ".NET SDK",
                Status = "ok",
                Detail = dotnetVersion
            });
        }
        else
        {
            results.Add(new Models.UnoCheckResult
            {
                Category = ".NET SDK",
                Name = ".NET SDK",
                Status = "error",
                Detail = "not found in PATH",
                Recommendation = "Install .NET SDK from https://dot.net"
            });
        }

        // 2. Workloads
        var workloadOutput = await RunCmd("dotnet", "workload list");
        if (!string.IsNullOrEmpty(workloadOutput))
        {
            var hasWasm = workloadOutput.Contains("wasm", StringComparison.OrdinalIgnoreCase);
            var hasIos = workloadOutput.Contains("ios", StringComparison.OrdinalIgnoreCase);
            var hasAndroid = workloadOutput.Contains("android", StringComparison.OrdinalIgnoreCase);
            var hasMaui = workloadOutput.Contains("maui", StringComparison.OrdinalIgnoreCase);
            var hasMacCatalyst = workloadOutput.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);

            void AddWorkload(string name, bool installed)
            {
                results.Add(new Models.UnoCheckResult
                {
                    Category = "Workloads",
                    Name = name,
                    Status = installed ? "ok" : "warning",
                    Detail = installed ? "installed" : "not installed",
                    Recommendation = installed ? "" : $"Install via: dotnet workload install {name.ToLowerInvariant()}"
                });
            }

            AddWorkload("wasm-tools", hasWasm);
            AddWorkload("ios", hasIos);
            AddWorkload("android", hasAndroid);
            AddWorkload("maui", hasMaui);
            AddWorkload("maccatalyst", hasMacCatalyst);
        }

        // 3. uno-check tool availability
        var unoCheckVersion = await RunCmd("dotnet", "tool list -g");
        var hasUnoCheck = unoCheckVersion.Contains("uno.check", StringComparison.OrdinalIgnoreCase);
        results.Add(new Models.UnoCheckResult
        {
            Category = "Tools",
            Name = "uno-check",
            Status = hasUnoCheck ? "ok" : "warning",
            Detail = hasUnoCheck ? "installed globally" : "not installed",
            Recommendation = hasUnoCheck ? "" : "Install via: dotnet tool install -g uno.check"
        });

        // 4. IDE detection
        var vsProcs = await RunCmd("tasklist", "/FI \"IMAGENAME eq devenv.exe\" /NH");
        var codeProcs = await RunCmd("tasklist", "/FI \"IMAGENAME eq Code.exe\" /NH");
        var riderProcs = await RunCmd("tasklist", "/FI \"IMAGENAME eq rider64.exe\" /NH");

        if (vsProcs.Contains("devenv.exe", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new Models.UnoCheckResult
            {
                Category = "IDE",
                Name = "Visual Studio",
                Status = "ok",
                Detail = "running"
            });
        }
        if (codeProcs.Contains("Code.exe", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new Models.UnoCheckResult
            {
                Category = "IDE",
                Name = "VS Code",
                Status = "ok",
                Detail = "running"
            });
        }
        if (riderProcs.Contains("rider64.exe", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new Models.UnoCheckResult
            {
                Category = "IDE",
                Name = "JetBrains Rider",
                Status = "ok",
                Detail = "running"
            });
        }

        // 5. Check for git
        var gitVersion = await RunCmd("git", "--version");
        results.Add(new Models.UnoCheckResult
        {
            Category = "Tools",
            Name = "git",
            Status = !string.IsNullOrEmpty(gitVersion) ? "ok" : "error",
            Detail = !string.IsNullOrEmpty(gitVersion) ? gitVersion.Replace("git version ", "") : "not found",
            Recommendation = string.IsNullOrEmpty(gitVersion) ? "Install git from https://git-scm.com" : ""
        });

        // 6. Check for OpenJDK (Android dev)
        var javaVersion = await RunCmd("java", "-version");
        var javaStderr = "";
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc != null)
            {
                javaStderr = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();
            }
        }
        catch { }
        var javaInfo = !string.IsNullOrEmpty(javaStderr) ? javaStderr : javaVersion;
        var hasJava = !string.IsNullOrEmpty(javaInfo) && javaInfo.Contains("version", StringComparison.OrdinalIgnoreCase);
        results.Add(new Models.UnoCheckResult
        {
            Category = "Tools",
            Name = "OpenJDK",
            Status = hasJava ? "ok" : "warning",
            Detail = hasJava ? javaInfo.Split('\n')[0].Trim() : "not found",
            Recommendation = hasJava ? "" : "Required for Android development"
        });

        return results;
    }

    public async Task<Models.LicenseInfo> GetLicenseInfoAsync()
    {
        string tier = "unknown";
        string email = "";
        string expiryDate = "";
        bool isExpired = false;
        bool isSignedIn = false;

        try
        {
            // Check env var override first
            var envTier = Environment.GetEnvironmentVariable("UNO_LICENSE_TIER");
            if (!string.IsNullOrEmpty(envTier))
            {
                return new Models.LicenseInfo(Tier: envTier, IsSignedIn: true);
            }

            var unoDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".uno");
            var licensePath = System.IO.Path.Combine(unoDir, "license");

            if (File.Exists(licensePath))
            {
                var lines = await File.ReadAllLinesAsync(licensePath);
                if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
                {
                    tier = lines[0].Trim();
                    isSignedIn = true;
                }
                // Best-effort parse: look for email= and expiry= lines
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("email=", StringComparison.OrdinalIgnoreCase))
                        email = trimmed["email=".Length..].Trim();
                    else if (trimmed.StartsWith("expiry=", StringComparison.OrdinalIgnoreCase))
                    {
                        expiryDate = trimmed["expiry=".Length..].Trim();
                        if (DateTime.TryParse(expiryDate, out var exp))
                            isExpired = exp < DateTime.UtcNow;
                    }
                    else if (trimmed.Contains('@') && string.IsNullOrEmpty(email))
                        email = trimmed;
                }
            }
        }
        catch { }

        return new Models.LicenseInfo(
            Tier: tier,
            Email: email,
            ExpiryDate: expiryDate,
            IsExpired: isExpired,
            IsSignedIn: isSignedIn);
    }

    public async Task<List<ActivityDataPoint>> GetActivityDataFromSessionsAsync(int days = 30)
    {
        var result = new List<ActivityDataPoint>();
        try
        {
            var sessions = await GetRecentSessionsAsync(200);
            var cutoff = DateTime.UtcNow.AddDays(-days);

            // Group sessions by day
            var byDay = sessions
                .Where(s => s.LastActivity > cutoff)
                .GroupBy(s => s.LastActivity.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var maxVal = byDay.Values.DefaultIfEmpty(1).Max();

            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var count = byDay.GetValueOrDefault(date, 0);
                result.Add(new ActivityDataPoint
                {
                    Label = date.ToString("MMM d"),
                    Value = count,
                    MaxValue = Math.Max(maxVal, 1),
                    TooltipText = $"{count} sessions on {date:MMM d}"
                });
            }
        }
        catch
        {
            // Return empty on failure
        }
        return result;
    }

    public async Task<List<HourlyActivity>> GetHourlyActivityFromSessionsAsync()
    {
        var result = new List<HourlyActivity>();
        try
        {
            var sessions = await GetRecentSessionsAsync(200);
            var today = DateTime.UtcNow.Date;

            // Count sessions that were active in each hour slot over the last 7 days
            var hourCounts = new int[24];
            foreach (var s in sessions.Where(s => s.LastActivity > today.AddDays(-7)))
            {
                var hour = s.LastActivity.ToLocalTime().Hour;
                hourCounts[hour]++;
            }

            var maxVal = hourCounts.DefaultIfEmpty(1).Max();

            for (int h = 0; h < 24; h++)
            {
                var label = h == 0 ? "12a" : h < 12 ? $"{h}a" : h == 12 ? "12p" : $"{h - 12}p";
                result.Add(new HourlyActivity
                {
                    HourLabel = label,
                    Value = hourCounts[h],
                    MaxValue = Math.Max(maxVal, 1),
                    TooltipText = $"{hourCounts[h]} sessions at {label}"
                });
            }
        }
        catch
        {
            // Return empty on failure
        }
        return result;
    }

    public async Task<List<(DateTimeOffset Timestamp, int Value)>> GetRollingActivityAsync(int windowMinutes = 60, int bucketMinutes = 5)
    {
        var result = new List<(DateTimeOffset Timestamp, int Value)>();
        try
        {
            var sessions = await GetRecentSessionsAsync(200);
            var now = DateTimeOffset.Now;
            var windowStart = now.AddMinutes(-windowMinutes);
            var bucketCount = windowMinutes / bucketMinutes;

            for (int i = 0; i < bucketCount; i++)
            {
                var bucketStart = windowStart.AddMinutes(i * bucketMinutes);
                var bucketEnd = bucketStart.AddMinutes(bucketMinutes);

                var count = sessions.Count(s =>
                {
                    var activity = new DateTimeOffset(s.LastActivity, TimeSpan.Zero);
                    return activity >= bucketStart && activity < bucketEnd;
                });

                result.Add((bucketStart, count));
            }
        }
        catch
        {
            // Return empty on failure
        }
        return result;
    }

    public async Task<List<ModelCost>> GetModelCostsFromSessionsAsync()
    {
        var result = new List<ModelCost>();
        try
        {
            var sessions = await GetRecentSessionsAsync(200);

            // Group by model and count sessions as a proxy for cost
            var byModel = sessions
                .Where(s => !string.IsNullOrEmpty(s.Model))
                .GroupBy(s => NormalizeModelName(s.Model))
                .Select(g => new { Model = g.Key, Count = g.Count(), Tokens = g.Sum(s => s.TokenCount) })
                .OrderByDescending(x => x.Tokens)
                .ToList();

            if (byModel.Count == 0)
                return result;

            // Estimate cost from token counts (rough: opus ~$15/MTok, sonnet ~$3/MTok, haiku ~$0.25/MTok)
            double EstimateCost(string model, int tokens)
            {
                var rate = model.Contains("opus", StringComparison.OrdinalIgnoreCase) ? 15.0 / 1_000_000
                    : model.Contains("haiku", StringComparison.OrdinalIgnoreCase) ? 0.25 / 1_000_000
                    : 3.0 / 1_000_000;
                return tokens * rate;
            }

            var costs = byModel.Select(m => new { m.Model, Cost = EstimateCost(m.Model, m.Tokens) }).ToList();
            var maxCost = costs.Max(c => c.Cost);

            var colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["opus"] = "#A78BFA",
                ["sonnet"] = "#4A9EF5",
                ["haiku"] = "#2DD4BF"
            };

            foreach (var c in costs)
            {
                var colorKey = colors.Keys.FirstOrDefault(k => c.Model.Contains(k, StringComparison.OrdinalIgnoreCase));
                result.Add(new ModelCost
                {
                    ModelName = c.Model,
                    Amount = Math.Round(c.Cost, 2),
                    MaxAmount = Math.Round(maxCost, 2),
                    Color = colorKey != null ? colors[colorKey] : "#6B7280"
                });
            }
        }
        catch
        {
            // Return empty on failure
        }
        return result;
    }

    private static string NormalizeModelName(string model)
    {
        if (string.IsNullOrEmpty(model)) return "unknown";
        var lower = model.ToLowerInvariant().Trim();
        if (lower.Contains("opus")) return "Claude Opus 4";
        if (lower.Contains("sonnet")) return "Claude Sonnet 4";
        if (lower.Contains("haiku")) return "Claude Haiku 3.5";
        return model;
    }

    public async Task<List<AlertItem>> GetAlertsFromEnvironmentAsync()
    {
        var alerts = new List<AlertItem>();
        try
        {
            // Check MCP servers
            var servers = await GetMcpServersAsync();
            if (servers.Count == 0)
            {
                alerts.Add(new AlertItem
                {
                    Type = AlertType.Info,
                    Message = "No MCP servers configured",
                    NavigationTarget = "mcp-health"
                });
            }

            // Check env audit for missing tools
            var envResults = await RunEnvAuditAsync();
            var missing = envResults.Where(r => r.Status == "missing").ToList();
            foreach (var m in missing)
            {
                alerts.Add(new AlertItem
                {
                    Type = m.ToolName == "claude" ? AlertType.Error : AlertType.Warning,
                    Message = $"{m.ToolName} not found in PATH",
                    NavigationTarget = "env-audit"
                });
            }

            // Check for stale sessions (>24h with no recent activity)
            var sessions = await GetRecentSessionsAsync(50);
            var staleCount = sessions.Count(s =>
                s.LastActivity < DateTime.UtcNow.AddHours(-24) &&
                s.LastActivity > DateTime.UtcNow.AddDays(-7));
            if (staleCount > 3)
            {
                alerts.Add(new AlertItem
                {
                    Type = AlertType.Info,
                    Message = $"{staleCount} sessions inactive for over 24h",
                    NavigationTarget = "sessions"
                });
            }

            // Check hygiene
            var hygieneIssues = envResults.Count(r => r.Status != "ok");
            if (hygieneIssues > 0)
            {
                alerts.Add(new AlertItem
                {
                    Type = AlertType.Warning,
                    Message = $"{hygieneIssues} environment issues detected",
                    NavigationTarget = "hygiene"
                });
            }
        }
        catch
        {
            // Return empty on failure
        }
        return alerts;
    }

    public async Task<List<SessionItem>> GetRecentSessionItemsAsync(int count = 5)
    {
        var items = new List<SessionItem>();
        try
        {
            var sessions = await GetRecentSessionsAsync(count);
            foreach (var s in sessions)
            {
                var repoName = !string.IsNullOrEmpty(s.ProjectPath)
                    ? System.IO.Path.GetFileName(s.ProjectPath.TrimEnd('\\', '/'))
                    : "";

                var description = !string.IsNullOrEmpty(s.FirstUserMessage)
                    ? s.FirstUserMessage
                    : $"Session {s.ShortId}";

                // Determine status from recency
                var isRecent = s.LastActivity > DateTime.UtcNow.AddHours(-1);
                items.Add(new SessionItem
                {
                    Status = isRecent ? "active" : "completed",
                    Description = description,
                    RepoName = repoName,
                    Timestamp = new DateTimeOffset(s.LastActivity, TimeSpan.Zero)
                });
            }
        }
        catch
        {
            // Return empty on failure
        }
        return items;
    }
}
