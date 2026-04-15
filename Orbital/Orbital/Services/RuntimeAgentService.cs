using System.Text.Json;
using System.Text.RegularExpressions;

namespace Orbital.Services;

public partial class RuntimeAgentService : IAgentService
{
    private ImmutableList<AgentSession>? _sessions;

    public async ValueTask<ImmutableList<AgentSession>> GetSessionsAsync(CancellationToken ct)
    {
        _sessions ??= await Task.Run(DiscoverSessions, ct);
        return _sessions;
    }

    public async ValueTask<AgentSession?> GetActiveSessionAsync(CancellationToken ct)
    {
        var sessions = await GetSessionsAsync(ct);
        // The most recent session is considered "active" if it was modified within the last hour
        var latest = sessions.FirstOrDefault();
        if (latest is not null)
        {
            // Check if it's recent enough to be "active"
            var isRecent = (DateTime.Now - latest.StartTime).TotalHours < 2;
            return latest with { Status = isRecent ? SessionStatus.Active : SessionStatus.Done };
        }
        return null;
    }

    public ValueTask CreateSessionAsync(CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask ReplayAsync(string sessionId, CancellationToken ct) => ValueTask.CompletedTask;

    private static ImmutableList<AgentSession> DiscoverSessions()
    {
        var projectDir = GetProjectSessionsDir();
        if (projectDir is null || !Directory.Exists(projectDir))
            return ImmutableList<AgentSession>.Empty;

        var sessionFiles = Directory.GetFiles(projectDir, "*.jsonl")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        var sessions = new List<AgentSession>();

        foreach (var file in sessionFiles)
        {
            var session = ParseSessionFile(file);
            if (session is not null)
                sessions.Add(session);
        }

        return sessions.ToImmutableList();
    }

    private static AgentSession? ParseSessionFile(FileInfo file)
    {
        try
        {
            var sessionId = Path.GetFileNameWithoutExtension(file.Name);
            string? sessionName = null;
            string? firstUserMessage = null;
            var toolUseCount = 0;
            var artifactCount = 0;
            DateTime? startTime = null;
            var actions = new List<AgentAction>();
            var lineCount = 0;

            using var reader = new StreamReader(file.FullName);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                lineCount++;

                // Fast pre-checks to avoid parsing every line as JSON
                if (line.Length < 20)
                    continue;

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("type", out var typeProp))
                        continue;

                    var type = typeProp.GetString();

                    if (type == "user" && root.TryGetProperty("message", out var userMsg))
                    {
                        if (userMsg.TryGetProperty("content", out var contentProp))
                        {
                            var content = contentProp.ValueKind == JsonValueKind.String
                                ? contentProp.GetString()
                                : null;

                            if (content is not null)
                            {
                                if (firstUserMessage is null)
                                {
                                    firstUserMessage = content.Length > 80
                                        ? content[..80]
                                        : content;
                                }
                                // Short messages (< 60 chars) after the first are likely session names
                                else if (sessionName is null && content.Length < 60 && content.Length > 2
                                         && !content.StartsWith("initialize", StringComparison.OrdinalIgnoreCase)
                                         && !content.StartsWith("continue", StringComparison.OrdinalIgnoreCase))
                                {
                                    sessionName = content.Trim().TrimEnd('\\', ' ');
                                }
                            }
                        }

                        // Extract timestamp
                        if (startTime is null && root.TryGetProperty("timestamp", out var tsProp))
                        {
                            if (DateTime.TryParse(tsProp.GetString(), out var ts))
                                startTime = ts.ToLocalTime();
                        }
                    }
                    else if (type == "assistant" && root.TryGetProperty("message", out var assistantMsg))
                    {
                        if (assistantMsg.TryGetProperty("content", out var contentArr) &&
                            contentArr.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in contentArr.EnumerateArray())
                            {
                                if (item.TryGetProperty("type", out var itemType))
                                {
                                    var itemTypeStr = itemType.GetString();
                                    if (itemTypeStr == "tool_use")
                                    {
                                        toolUseCount++;

                                        // Track significant tool actions (limit to avoid huge lists)
                                        if (actions.Count < 20)
                                        {
                                            var toolName = item.TryGetProperty("name", out var nameProp)
                                                ? nameProp.GetString() ?? "unknown"
                                                : "unknown";

                                            var detail = SummarizeToolUse(toolName, item);
                                            if (detail is not null)
                                            {
                                                actions.Add(new AgentAction(
                                                    startTime?.AddSeconds(actions.Count * 30) ?? DateTime.Now,
                                                    FormatToolAction(toolName),
                                                    detail,
                                                    ActionStatus.Ok));
                                            }
                                        }

                                        // Count file writes as artifacts
                                        if (item.TryGetProperty("name", out var n))
                                        {
                                            var nm = n.GetString();
                                            if (nm is "Write" or "Edit" or "NotebookEdit")
                                                artifactCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Skip malformed lines
                }
            }

            if (firstUserMessage is null)
                return null;

            // Determine session name
            sessionName ??= firstUserMessage.Length > 50
                ? firstUserMessage[..50] + "..."
                : firstUserMessage;

            // Clean up session name
            sessionName = sessionName.Replace("\\n", " ").Trim();
            if (sessionName.StartsWith("<"))
                sessionName = "Imported session";

            var isRecent = (DateTime.UtcNow - file.LastWriteTimeUtc).TotalHours < 2;

            return new AgentSession(
                Id: sessionId,
                Name: sessionName,
                Repo: "Orbital",
                Branch: "main",
                Goal: firstUserMessage,
                Status: isRecent ? SessionStatus.Active : SessionStatus.Done,
                ActionCount: toolUseCount,
                ArtifactCount: artifactCount,
                Actions: actions.Take(10).ToImmutableList(),
                StartTime: startTime ?? file.CreationTime);
        }
        catch
        {
            return null;
        }
    }

    private static string? SummarizeToolUse(string toolName, JsonElement item)
    {
        // Only capture interesting actions, skip noise
        if (toolName is "Read" or "Glob" or "Grep" or "ToolSearch")
            return null; // Too noisy

        if (toolName == "Write" && item.TryGetProperty("input", out var writeInput))
        {
            if (writeInput.TryGetProperty("file_path", out var fp))
                return Path.GetFileName(fp.GetString() ?? "");
        }

        if (toolName == "Edit" && item.TryGetProperty("input", out var editInput))
        {
            if (editInput.TryGetProperty("file_path", out var fp))
                return Path.GetFileName(fp.GetString() ?? "");
        }

        if (toolName == "Bash" && item.TryGetProperty("input", out var bashInput))
        {
            if (bashInput.TryGetProperty("description", out var desc))
                return desc.GetString();
            if (bashInput.TryGetProperty("command", out var cmd))
            {
                var cmdStr = cmd.GetString() ?? "";
                return cmdStr.Length > 60 ? cmdStr[..60] + "..." : cmdStr;
            }
        }

        if (toolName.StartsWith("mcp__"))
            return toolName.Split("__").LastOrDefault();

        return toolName;
    }

    private static string FormatToolAction(string toolName)
    {
        return toolName switch
        {
            "Write" => "Created file",
            "Edit" => "Edited file",
            "Bash" => "Ran command",
            "Glob" => "Searched files",
            "Grep" => "Searched content",
            "Read" => "Read file",
            _ when toolName.StartsWith("mcp__uno__") => "Uno Platform docs",
            _ when toolName.StartsWith("mcp__uno-app__") => "Uno App control",
            _ when toolName.StartsWith("mcp__") => "MCP tool",
            _ => toolName,
        };
    }

    private static string? GetProjectSessionsDir()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeDir = Path.Combine(userProfile, ".claude", "projects");

        if (!Directory.Exists(claudeDir))
            return null;

        // Claude Code encodes project paths as directory names:
        //   C:\Users\Foo\MyProject -> C--Users-Foo-MyProject
        // Build the expected encoded name from our actual working directory
        var cwd = FindProjectRoot() ?? AppContext.BaseDirectory;
        var encoded = EncodePath(cwd);

        // First: exact match on the encoded project path
        foreach (var dir in Directory.GetDirectories(claudeDir))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName.Equals(encoded, StringComparison.OrdinalIgnoreCase) &&
                Directory.GetFiles(dir, "*.jsonl").Length > 0)
            {
                return dir;
            }
        }

        // Fallback: partial match — find directories whose encoded name is contained in ours or vice versa
        foreach (var dir in Directory.GetDirectories(claudeDir))
        {
            var dirName = Path.GetFileName(dir);
            if ((encoded.Contains(dirName, StringComparison.OrdinalIgnoreCase) ||
                 dirName.Contains(encoded, StringComparison.OrdinalIgnoreCase)) &&
                Directory.GetFiles(dir, "*.jsonl").Length > 0)
            {
                return dir;
            }
        }

        return null;
    }

    private static string EncodePath(string path)
    {
        // Claude Code encoding: colon → dash, backslash → dash, space → dash
        // e.g. C:\Users\Foo → C--Users-Foo (colon+backslash = two dashes)
        return path
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace(":", "-")
            .Replace("\\", "-")
            .Replace("/", "-")
            .Replace(" ", "-");
    }

    private static string? FindProjectRoot()
    {
        // Walk up from base dir to find the directory containing the .csproj
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
