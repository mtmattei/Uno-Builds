namespace ClaudeDash.Helpers;

public static class PageRegistry
{
    private static readonly Dictionary<string, Type> _pages = new()
    {
        // ─── Primary nav (10 pages) ───
        ["home"] = typeof(Views.HomePage),
        ["chat"] = typeof(Views.ChatPage),
        ["sessions"] = typeof(Views.SessionsPage),
        ["projects"] = typeof(Views.ProjectsPage),
        ["uno-platform"] = typeof(Views.UnoPlatformOverviewPage),
        ["hygiene"] = typeof(Views.RemediationPage),
        ["mcp-skills"] = typeof(Views.McpSkillsPage),
        ["hooks-memory"] = typeof(Views.HooksMemoryPage),
        ["costs"] = typeof(Views.CostsPage),
        ["settings"] = typeof(Views.SettingsPage),

        // ─── Detail pages (not in sidebar) ───
        ["session-replay"] = typeof(Views.SessionReplayPage),
        ["ralph-loops"] = typeof(Views.RalphLoopsPage),

        // ─── Backward-compat aliases (old keys -> consolidated pages) ───
        ["mcp-hooks"] = typeof(Views.McpSkillsPage),
        ["mcp-health"] = typeof(Views.McpSkillsPage),
        ["skills"] = typeof(Views.McpSkillsPage),
        ["agents"] = typeof(Views.McpSkillsPage),
        ["hooks"] = typeof(Views.HooksMemoryPage),
        ["memory"] = typeof(Views.HooksMemoryPage),
        ["memory-skills"] = typeof(Views.HooksMemoryPage),
        ["repos"] = typeof(Views.ProjectsPage),
        ["deps"] = typeof(Views.ProjectsPage),
        ["worktrees"] = typeof(Views.ProjectsPage),
        ["terminal"] = typeof(Views.ProjectsPage),
        ["env-audit"] = typeof(Views.RemediationPage),
        ["devices"] = typeof(Views.SettingsPage),
        ["uno-overview"] = typeof(Views.UnoPlatformOverviewPage),
        // ralph-loops is a detail page, see above
    };

    public static Type? GetPageType(string key) =>
        _pages.TryGetValue(key, out var type) ? type : null;
}
