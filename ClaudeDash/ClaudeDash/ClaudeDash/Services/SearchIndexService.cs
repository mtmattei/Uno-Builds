using ClaudeDash.Models.Search;
using Path = System.IO.Path;

namespace ClaudeDash.Services;

public class SearchIndexService : ISearchIndexService
{
    private readonly IClaudeConfigService _configService;
    private readonly IProjectScannerService _projectScanner;
    private List<SearchableItem> _items = [];
    private readonly object _lock = new();

    public int IndexSize
    {
        get { lock (_lock) return _items.Count; }
    }

    public bool IsReady { get; private set; }
    public event Action? IndexRebuilt;

    public SearchIndexService(IClaudeConfigService configService, IProjectScannerService projectScanner)
    {
        _configService = configService;
        _projectScanner = projectScanner;
    }

    public async Task BuildIndexAsync()
    {
        var items = new List<SearchableItem>();

        // Run all indexing tasks in parallel
        var sessionsTask = IndexSessionsAsync();
        var projectsTask = IndexProjectsAsync();
        var reposTask = IndexReposAsync();
        var skillsTask = IndexSkillsAsync();
        var agentsTask = IndexAgentsAsync();
        var mcpTask = IndexMcpServersAsync();
        var memoryTask = IndexMemoryFilesAsync();
        var hooksTask = IndexHooksAsync();
        var depsTask = IndexDependenciesAsync();

        await Task.WhenAll(sessionsTask, projectsTask, reposTask, skillsTask,
            agentsTask, mcpTask, memoryTask, hooksTask, depsTask);

        items.AddRange(await sessionsTask);
        items.AddRange(await projectsTask);
        items.AddRange(await reposTask);
        items.AddRange(await skillsTask);
        items.AddRange(await agentsTask);
        items.AddRange(await mcpTask);
        items.AddRange(await memoryTask);
        items.AddRange(await hooksTask);
        items.AddRange(await depsTask);

        lock (_lock)
        {
            _items = items;
        }

        IsReady = true;
        IndexRebuilt?.Invoke();
    }

    public List<SearchResult> Search(string query, int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var queryTokens = Tokenize(query);
        if (queryTokens.Count == 0)
            return [];

        List<SearchableItem> snapshot;
        lock (_lock)
        {
            snapshot = _items;
        }

        var scored = new List<(SearchableItem Item, double Score)>();

        foreach (var item in snapshot)
        {
            var score = CalculateScore(item, queryTokens, query.ToLowerInvariant());
            if (score > 0)
                scored.Add((item, score));
        }

        return scored
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .Select(x => new SearchResult
            {
                Type = x.Item.Type,
                Title = x.Item.Title,
                Subtitle = x.Item.Subtitle,
                Icon = x.Item.Icon,
                Score = x.Score,
                LastActivity = x.Item.LastActivity,
                PageKey = x.Item.PageKey,
                ItemId = x.Item.Id,
                SourceObject = x.Item.SourceObject
            })
            .ToList();
    }

    private static double CalculateScore(SearchableItem item, List<string> queryTokens, string fullQuery)
    {
        double score = 0;
        var matchedTokens = 0;

        foreach (var queryToken in queryTokens)
        {
            var tokenMatched = false;

            foreach (var searchToken in item.SearchTokens)
            {
                if (searchToken.Equals(queryToken, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10.0; // Exact match
                    tokenMatched = true;
                    break;
                }
                else if (searchToken.StartsWith(queryToken, StringComparison.OrdinalIgnoreCase))
                {
                    score += 6.0; // Prefix match
                    tokenMatched = true;
                    break;
                }
                else if (searchToken.Contains(queryToken, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3.0; // Substring match
                    tokenMatched = true;
                    break;
                }
                else if (FuzzyMatch(searchToken, queryToken) >= 0.7)
                {
                    score += 1.5; // Fuzzy match
                    tokenMatched = true;
                    break;
                }
            }

            if (tokenMatched)
                matchedTokens++;
        }

        // All tokens must match for a result
        if (matchedTokens < queryTokens.Count)
            return 0;

        // Bonus for title match
        if (item.Title.Contains(fullQuery, StringComparison.OrdinalIgnoreCase))
            score += 15.0;

        // Recency boost (decay over 30 days)
        var daysSince = (DateTime.UtcNow - item.LastActivity).TotalDays;
        if (daysSince < 1) score += 5.0;
        else if (daysSince < 7) score += 3.0;
        else if (daysSince < 30) score += 1.0;

        return score;
    }

    private static double FuzzyMatch(string source, string target)
    {
        if (source.Length == 0 || target.Length == 0)
            return 0;

        // Simple Jaro-like similarity
        var maxDist = Math.Max(source.Length, target.Length) / 2 - 1;
        if (maxDist < 0) maxDist = 0;

        var sourceMatches = new bool[source.Length];
        var targetMatches = new bool[target.Length];
        var matches = 0;

        for (var i = 0; i < source.Length; i++)
        {
            var start = Math.Max(0, i - maxDist);
            var end = Math.Min(i + maxDist + 1, target.Length);

            for (var j = start; j < end; j++)
            {
                if (targetMatches[j] || source[i] != target[j])
                    continue;
                sourceMatches[i] = true;
                targetMatches[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0;

        var transpositions = 0;
        var k = 0;
        for (var i = 0; i < source.Length; i++)
        {
            if (!sourceMatches[i]) continue;
            while (!targetMatches[k]) k++;
            if (source[i] != target[k]) transpositions++;
            k++;
        }

        return ((double)matches / source.Length +
                (double)matches / target.Length +
                (double)(matches - transpositions / 2) / matches) / 3.0;
    }

    private static List<string> Tokenize(string text)
    {
        return text.ToLowerInvariant()
            .Split([' ', '-', '_', '.', '/', '\\', '@'], StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 1)
            .Distinct()
            .ToList();
    }

    private static List<string> BuildTokens(params string?[] values)
    {
        var tokens = new List<string>();
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;
            tokens.AddRange(Tokenize(value));
        }
        return tokens.Distinct().ToList();
    }

    // ---- Indexers for each entity type ----

    private async Task<List<SearchableItem>> IndexSessionsAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(100);
            foreach (var s in sessions)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Session,
                    Id = s.SessionId,
                    Title = string.IsNullOrEmpty(s.FirstUserMessage)
                        ? $"Session {s.ShortId}"
                        : s.FirstUserMessage,
                    Subtitle = $"{s.Model} - {s.GitBranch}",
                    Icon = "\uE916", // Clock
                    LastActivity = s.LastActivity,
                    PageKey = "sessions",
                    SearchTokens = BuildTokens(s.FirstUserMessage, s.SessionId, s.ShortId,
                        s.Model, s.GitBranch, s.ProjectPath, s.RepoName, "session"),
                    SourceObject = s
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexProjectsAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var projects = await _projectScanner.GetAllProjectsAsync();
            foreach (var p in projects)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Project,
                    Id = p.Path,
                    Title = p.Name,
                    Subtitle = $"{p.SessionCount} sessions - {p.CurrentBranch}",
                    Icon = "\uE8B7", // Folder
                    LastActivity = p.LastActivity,
                    PageKey = "projects",
                    SearchTokens = BuildTokens(p.Name, p.Path, p.CurrentBranch, "project"),
                    SourceObject = p
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexReposAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var repos = await _configService.GetReposAsync();
            foreach (var r in repos)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Repo,
                    Id = r.Path,
                    Title = r.Name,
                    Subtitle = $"{r.SessionCount} sessions - {r.LastBranch}",
                    Icon = "\uE943", // Code
                    LastActivity = r.LastActivity,
                    PageKey = "repos",
                    SearchTokens = BuildTokens(r.Name, r.Path, r.LastBranch, "repo", "repository"),
                    SourceObject = r
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexSkillsAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var skills = await _configService.GetSkillsAsync();
            foreach (var s in skills)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Skill,
                    Id = s.Name,
                    Title = s.Name,
                    Subtitle = s.Description,
                    Icon = "\uE945", // Lightning bolt
                    LastActivity = DateTime.UtcNow, // Skills don't have activity dates
                    PageKey = "skills",
                    SearchTokens = BuildTokens(s.Name, s.Description, s.Category, "skill"),
                    SourceObject = s
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexAgentsAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var agents = await _configService.GetAgentsAsync();
            foreach (var a in agents)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Agent,
                    Id = a.SessionId,
                    Title = $"Agent {a.SessionId[..Math.Min(8, a.SessionId.Length)]}",
                    Subtitle = $"Project: {Path.GetFileName(a.ProjectPath.TrimEnd('\\', '/'))}",
                    Icon = "\uE99A", // Robot
                    LastActivity = a.LastActivity,
                    PageKey = "agents",
                    SearchTokens = BuildTokens(a.SessionId, a.ProjectPath, a.ParentSessionId, "agent", "subagent"),
                    SourceObject = a
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexMcpServersAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var servers = await _configService.GetMcpServersAsync();
            foreach (var s in servers)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.McpServer,
                    Id = s.Name,
                    Title = s.Name,
                    Subtitle = $"{s.ServerType} - {s.Source}",
                    Icon = "\uE968", // Server
                    LastActivity = DateTime.UtcNow,
                    PageKey = "mcp-health",
                    SearchTokens = BuildTokens(s.Name, s.ServerType, s.Command, s.Url, "mcp", "server"),
                    SourceObject = s
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexMemoryFilesAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var files = await _configService.GetMemoryFilesAsync();
            foreach (var f in files)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.MemoryFile,
                    Id = f.FilePath,
                    Title = f.FileName,
                    Subtitle = f.ProjectContext,
                    Icon = "\uE8A5", // Document
                    LastActivity = f.LastModified,
                    PageKey = "memory",
                    SearchTokens = BuildTokens(f.FileName, f.ProjectContext, f.Content, "memory", "claude.md"),
                    SourceObject = f
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexHooksAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var hooks = await _configService.GetHooksAsync();
            foreach (var h in hooks)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Hook,
                    Id = $"{h.HookType}_{h.Matcher}",
                    Title = $"{h.HookType} ({h.Matcher})",
                    Subtitle = h.Command,
                    Icon = "\uE8C8", // Settings
                    LastActivity = DateTime.UtcNow,
                    PageKey = "hooks",
                    SearchTokens = BuildTokens(h.HookType, h.Matcher, h.Command, "hook"),
                    SourceObject = h
                });
            }
        }
        catch { }
        return items;
    }

    private async Task<List<SearchableItem>> IndexDependenciesAsync()
    {
        var items = new List<SearchableItem>();
        try
        {
            var deps = await _configService.GetDependenciesAsync();
            foreach (var d in deps)
            {
                items.Add(new SearchableItem
                {
                    Type = SearchResultType.Dependency,
                    Id = $"{d.ProjectName}_{d.PackageName}",
                    Title = d.PackageName,
                    Subtitle = $"{d.Version} - {d.ProjectName}",
                    Icon = "\uE8F1", // Package
                    LastActivity = DateTime.UtcNow,
                    PageKey = "deps",
                    SearchTokens = BuildTokens(d.PackageName, d.Version, d.ProjectName, "dependency", "package", "nuget"),
                    SourceObject = d
                });
            }
        }
        catch { }
        return items;
    }
}
