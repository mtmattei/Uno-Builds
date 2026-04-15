using System.Text.Json;
using System.Text.RegularExpressions;

namespace Orbital.Services;

public partial class RuntimeMcpService : IMcpService
{
    private McpStatus? _cached;

    public ValueTask<McpStatus> GetConnectionStatusAsync(CancellationToken ct)
    {
        _cached ??= DiscoverMcpServers();
        return ValueTask.FromResult(_cached);
    }

    private McpStatus DiscoverMcpServers()
    {
        var servers = new List<McpServer>();

        // 1. Scan Claude Code session JSONL files for mcp__ tool names to discover active MCP servers
        var sessionServers = DiscoverFromSessionLogs();
        servers.AddRange(sessionServers);

        // 2. Check for .mcp.json in the project directory tree
        var configServers = DiscoverFromMcpConfig();
        foreach (var s in configServers)
        {
            if (!servers.Any(existing => existing.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase)))
                servers.Add(s);
        }

        var totalTools = servers.Sum(s => s.ToolCount);
        var anyConnected = servers.Any(s => s.Healthy);

        return new McpStatus(anyConnected, servers.Count, totalTools, servers.ToImmutableList());
    }

    private static List<McpServer> DiscoverFromSessionLogs()
    {
        var servers = new Dictionary<string, (HashSet<string> Tools, bool Found)>(StringComparer.OrdinalIgnoreCase);
        var projectDir = GetProjectSessionsDir();

        if (projectDir is null || !Directory.Exists(projectDir))
            return [];

        // Scan all session files (root-level only, subagent files are nested)
        var sessionFiles = Directory.GetFiles(projectDir, "*.jsonl")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc);

        foreach (var file in sessionFiles)
        {
            try
            {
                using var reader = new StreamReader(file.FullName);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    // Fast check before regex
                    if (!line.Contains("mcp__"))
                        continue;

                    foreach (Match match in McpToolNameRegex().Matches(line))
                    {
                        var fullName = match.Groups[1].Value; // e.g. "mcp__uno__uno_platform_docs_search"
                        var parts = fullName.Split("__", 3);
                        if (parts.Length >= 3)
                        {
                            var serverKey = parts[1]; // e.g. "uno", "claude_ai_uno", "uno-app"
                            var toolName = parts[2];  // e.g. "uno_platform_docs_search"

                            if (!servers.TryGetValue(serverKey, out var entry))
                            {
                                entry = (new HashSet<string>(), true);
                                servers[serverKey] = entry;
                            }
                            entry.Tools.Add(toolName);
                            servers[serverKey] = entry;
                        }
                    }
                }
            }
            catch
            {
                // Skip unreadable files
            }
        }

        return servers.Select(kv =>
        {
            var displayName = FormatServerName(kv.Key);
            var url = InferServerUrl(kv.Key);
            return new McpServer(displayName, url, true, kv.Value.Tools.Count);
        }).ToList();
    }

    private static List<McpServer> DiscoverFromMcpConfig()
    {
        var servers = new List<McpServer>();

        // Walk up from AppContext.BaseDirectory looking for .mcp.json
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var mcpJsonPath = Path.Combine(dir.FullName, ".mcp.json");
            if (File.Exists(mcpJsonPath))
            {
                try
                {
                    var json = JsonDocument.Parse(File.ReadAllText(mcpJsonPath));
                    if (json.RootElement.TryGetProperty("mcpServers", out var mcpServers))
                    {
                        foreach (var prop in mcpServers.EnumerateObject())
                        {
                            var name = prop.Name;
                            var url = "";
                            if (prop.Value.TryGetProperty("url", out var urlProp))
                                url = urlProp.GetString() ?? "";
                            else if (prop.Value.TryGetProperty("command", out var cmdProp))
                                url = cmdProp.GetString() ?? "local";

                            servers.Add(new McpServer(name, url, true, 0));
                        }
                    }
                }
                catch
                {
                    // Skip malformed config
                }
                break; // Stop at the first .mcp.json found
            }
            dir = dir.Parent;
        }

        return servers;
    }

    private static string? GetProjectSessionsDir()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeDir = Path.Combine(userProfile, ".claude", "projects");

        if (!Directory.Exists(claudeDir))
            return null;

        // Build encoded path from actual project root
        var projectRoot = FindProjectRoot() ?? AppContext.BaseDirectory;
        var encoded = projectRoot
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace(":", "-")
            .Replace("\\", "-")
            .Replace("/", "-")
            .Replace(" ", "-");

        // Exact match first
        foreach (var dir in Directory.GetDirectories(claudeDir))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName.Equals(encoded, StringComparison.OrdinalIgnoreCase) &&
                Directory.GetFiles(dir, "*.jsonl").Length > 0)
                return dir;
        }

        // Partial match fallback
        foreach (var dir in Directory.GetDirectories(claudeDir))
        {
            var dirName = Path.GetFileName(dir);
            if ((encoded.Contains(dirName, StringComparison.OrdinalIgnoreCase) ||
                 dirName.Contains(encoded, StringComparison.OrdinalIgnoreCase)) &&
                Directory.GetFiles(dir, "*.jsonl").Length > 0)
                return dir;
        }

        return null;
    }

    private static string? FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static string FormatServerName(string key)
    {
        return key switch
        {
            "uno" => "Uno Platform",
            "uno-app" or "uno_app" => "Uno App",
            "claude_ai_uno" => "Uno Platform (claude.ai)",
            "claude_ai_Microsoft_Learn" => "Microsoft Learn",
            "claude_ai_HubSpot" => "HubSpot",
            "analytics-mcp" or "analytics_mcp" => "Google Analytics",
            _ => key.Replace("_", " ").Replace("-", " "),
        };
    }

    private static string InferServerUrl(string key)
    {
        return key switch
        {
            "uno" => "mcp.platform.uno",
            "uno-app" or "uno_app" => "localhost (runtime)",
            "claude_ai_uno" => "claude.ai/uno",
            "claude_ai_Microsoft_Learn" => "claude.ai/microsoft-learn",
            "claude_ai_HubSpot" => "claude.ai/hubspot",
            "analytics-mcp" or "analytics_mcp" => "analytics",
            _ => key,
        };
    }

    [GeneratedRegex(@"""name"":""(mcp__[^""]+)""")]
    private static partial Regex McpToolNameRegex();
}
