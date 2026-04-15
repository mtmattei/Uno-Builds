namespace Orbital.Services;

public class MockSkillsService : ISkillsService
{
    private readonly List<SkillInfo> _skills =
    [
        // Core skills
        new("sk-01", "uno-platform-agent", "Comprehensive Uno Platform dev patterns for Single Project, MVVM/MVUX, navigation, styling, and platform-specific code", "core", true, 342, 0.94, "userSettings:uno-platform-agent"),
        new("sk-02", "winui-xaml", "WinUI 3 and XAML best practices for layout, binding, async, collections, rendering, and accessibility", "core", true, 287, 0.91, "userSettings:winui-xaml"),
        new("sk-03", "mvux-overview", "MVUX architecture patterns — Model-View-Update-eXtended for reactive Uno Platform apps", "core", true, 198, 0.89, "userSettings:mvux-overview"),
        new("sk-04", "uno-navigation-regions", "Region-based navigation with Region.Attached, Region.Name, and Region.Navigator", "core", true, 176, 0.92, "userSettings:uno-navigation-regions"),
        new("sk-05", "uno-csharp-markup", "C# Markup for code-first UI with fluent API, strongly-typed binding, and resources", "core", false, 64, 0.85, "userSettings:uno-csharp-markup"),
        new("sk-06", "uno-toolkit-autolayout", "AutoLayout control for Figma-like spacing, alignment, and padding", "core", true, 153, 0.93, "userSettings:uno-toolkit-autolayout"),

        // Material / Styling
        new("sk-07", "uno-material-lightweight-styling", "Customize control appearance using lightweight styling resource overrides", "styling", true, 221, 0.88, "userSettings:uno-material-lightweight-styling"),
        new("sk-08", "uno-material-colors-brushes", "Material Design 3 color system and brush resources", "styling", true, 189, 0.90, "userSettings:uno-material-colors-brushes"),
        new("sk-09", "uno-material-typography", "Material Design typography styles — Display, Headline, Title, Body, Label", "styling", false, 45, 0.82, "userSettings:uno-material-typography"),
        new("sk-10", "uno-material-button-styles", "Filled, Elevated, Outlined, and Text button styles", "styling", true, 134, 0.91, "userSettings:uno-material-button-styles"),

        // Navigation
        new("sk-11", "uno-navigation-code", "Programmatic navigation with INavigator extension methods", "navigation", true, 156, 0.87, "userSettings:uno-navigation-code"),
        new("sk-12", "uno-navigation-routes", "Define and register routes with ViewMap, DataViewMap, and RouteMap", "navigation", true, 112, 0.93, "userSettings:uno-navigation-routes"),
        new("sk-13", "uno-navigation-navigationview", "NavigationView region-based sidebar navigation", "navigation", true, 98, 0.90, "userSettings:uno-navigation-navigationview"),
        new("sk-14", "uno-navigation-tabbar", "TabBar region-based bottom/tab navigation", "navigation", false, 23, 0.78, "userSettings:uno-navigation-tabbar"),

        // MVUX
        new("sk-15", "mvux-feed-basics", "IFeed<T> for async data loading and reactive data sources", "mvux", true, 245, 0.95, "userSettings:mvux-feed-basics"),
        new("sk-16", "mvux-state-basics", "IState<T> for mutable reactive data and two-way binding", "mvux", true, 201, 0.92, "userSettings:mvux-state-basics"),
        new("sk-17", "mvux-listfeed", "IListFeed<T> for reactive collections", "mvux", true, 167, 0.90, "userSettings:mvux-listfeed"),
        new("sk-18", "mvux-commands", "Commands in MVUX for user interactions and button actions", "mvux", false, 78, 0.86, "userSettings:mvux-commands"),
        new("sk-19", "mvux-feedview", "FeedView control for loading, error, and empty state rendering", "mvux", true, 143, 0.91, "userSettings:mvux-feedview"),

        // Toolkit
        new("sk-20", "uno-toolkit-card", "Card and CardContentControl for elevated, filled, or outlined containers", "toolkit", false, 56, 0.84, "userSettings:uno-toolkit-card"),
        new("sk-21", "uno-toolkit-chip", "Chip and ChipGroup for selection, filtering, or actions", "toolkit", false, 31, 0.80, "userSettings:uno-toolkit-chip"),
        new("sk-22", "uno-toolkit-safearea", "SafeArea for notches, status bars, and on-screen keyboards", "toolkit", true, 89, 0.88, "userSettings:uno-toolkit-safearea"),
        new("sk-23", "uno-toolkit-responsive", "ResponsiveExtension for screen-size-based UI adaptation", "toolkit", true, 112, 0.90, "userSettings:uno-toolkit-responsive"),

        // Testing / Other
        new("sk-24", "uno-app-ui-testing", "Automated UI testing with Uno App MCP server tools", "testing", true, 67, 0.86, "userSettings:uno-app-ui-testing"),
        new("sk-25", "uno-wasm-pwa", "WebAssembly and PWA development — bootstrapper, manifests, hosting", "other", false, 18, 0.75, "userSettings:uno-wasm-pwa"),
    ];

    public ValueTask<ImmutableList<SkillInfo>> GetSkillsAsync(CancellationToken ct) =>
        ValueTask.FromResult(_skills.ToImmutableList());

    public ValueTask ToggleSkillAsync(string skillId, bool active, CancellationToken ct)
    {
        var index = _skills.FindIndex(s => s.Id == skillId);
        if (index >= 0)
            _skills[index] = _skills[index] with { IsActive = active };
        return ValueTask.CompletedTask;
    }
}
