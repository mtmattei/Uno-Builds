using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record ShellModel
{
    private readonly IClaudeConfigService _configService;
    private readonly IBackgroundScannerService _scanner;
    private readonly ILogger<ShellModel> _logger;

    public ShellModel(
        IClaudeConfigService configService,
        IBackgroundScannerService scanner,
        ILogger<ShellModel> logger)
    {
        _configService = configService;
        _scanner = scanner;
        _logger = logger;
    }

    // --- Navigation items ---
    public IListFeed<NavItem> NavItems => ListFeed.Async(async ct =>
        ImmutableList.Create(
            new NavItem("home", "Home", "\uE80F", "CORE", "Dashboard overview of your dev environment"),
            new NavItem("chat", "Assistant", "\uE8BD", "CORE", "Ask the AI about your setup (Ctrl+J)"),
            new NavItem("sessions", "Sessions", "\uE916", "CORE", "Claude Code session history (Ctrl+1)"),
            new NavItem("projects", "Projects", "\uE8B7", "WORKSPACE", "Projects, repos, worktrees, and dependencies"),
            new NavItem("uno-platform", "Uno Platform", "\uE71D", "WORKSPACE", "Uno Platform tools, checks, and ralph loops"),
            new NavItem("ralph-loops", "Ralph Loop", "\uE8F1", "WORKSPACE", "7-stage AI pipeline: idea to shipped app"),
            new NavItem("hygiene", "Hygiene", "\uE90F", "WORKSPACE", "Dev environment issues and fixes (Ctrl+2)"),
            new NavItem("mcp-skills", "MCP & Skills", "\uE95E", "SYSTEM", "MCP servers, skills, and sub-agents"),
            new NavItem("hooks-memory", "Hooks & Memory", "\uE7F4", "SYSTEM", "Hook configurations and memory files"),
            new NavItem("costs", "Costs", "\uE8EC", "SYSTEM", "Token usage and cost breakdown"),
            new NavItem("settings", "Settings", "\uE713", "SYSTEM", "Preferences, system info, and environment")));

    // --- Selected nav item ---
    public IState<NavItem?> SelectedItem => State<NavItem?>.Value(this, () => null);

    // --- Verdict (fed from HomeModel via messaging) ---
    public IState<VerdictState> Verdict => State.Value(this, () => new VerdictState());

    // --- Environment info ---
    public IFeed<EnvInfo> EnvironmentInfo => Feed.Async(async ct =>
    {
        try
        {
            var info = await _configService.GetUnoPlatformInfoAsync();
            var license = await _configService.GetLicenseInfoAsync();
            return new EnvInfo(
                DotNetVersion: !string.IsNullOrEmpty(info.DotNetSdkVersion) ? $".NET {info.DotNetSdkVersion}" : ".NET n/a",
                UnoSdkVersion: !string.IsNullOrEmpty(info.SdkVersion) ? $"Uno {info.SdkVersion}" : "Uno SDK n/a",
                StudioVersion: !string.IsNullOrEmpty(info.DetectedIde) && info.DetectedIde != "n/a" ? info.DetectedIde : "Studio n/a",
                HasDotNet: !string.IsNullOrEmpty(info.DotNetSdkVersion),
                HasUnoSdk: !string.IsNullOrEmpty(info.SdkVersion),
                HasStudio: !string.IsNullOrEmpty(info.DetectedIde) && info.DetectedIde != "n/a",
                License: license);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load environment info");
            return new EnvInfo();
        }
    });

    // --- Connection status ---
    public IFeed<ConnectionStatus> ClaudeConnection => Feed.Async(async ct =>
    {
        try
        {
            var sessions = await _configService.GetRecentSessionsAsync(50);
            var recentCutoff = DateTime.UtcNow.AddHours(-1);
            var activeCount = sessions.Count(s => s.LastActivity > recentCutoff);

            bool cliRunning = false;
            try
            {
                var procs = System.Diagnostics.Process.GetProcessesByName("claude");
                cliRunning = procs.Length > 0;
                foreach (var p in procs) p.Dispose();
            }
            catch { }

            return new ConnectionStatus(
                IsConnected: activeCount > 0 || cliRunning,
                ActiveCount: activeCount,
                StatusText: activeCount > 0 ? $"{activeCount} active" : cliRunning ? "running" : "idle");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check connection status");
            return new ConnectionStatus();
        }
    });

    // --- Scanner status ---
    public IState<ScanStatus> ScannerStatus => State.Value(this, () => new ScanStatus());

    // --- Current page title ---
    public IState<string> PageTitle => State.Value(this, () => "Home");

    // --- Clock ---
    public IState<string> CurrentTime => State.Value(this, () => DateTime.Now.ToString("HH:mm:ss"));

    // --- Commands ---
    public async ValueTask NavigateTo(NavItem item, CancellationToken ct)
    {
        await SelectedItem.Update(_ => item, ct);
    }

    public async ValueTask UpdateVerdict(VerdictState verdict, CancellationToken ct)
    {
        await Verdict.Update(_ => verdict, ct);
    }

    public async ValueTask UpdateScanStatus(ScanStatus status, CancellationToken ct)
    {
        await ScannerStatus.Update(_ => status, ct);
    }

    public async ValueTask UpdateTime(CancellationToken ct)
    {
        await CurrentTime.Update(_ => DateTime.Now.ToString("HH:mm:ss"), ct);
    }

    public async ValueTask UpdatePageTitle(string title, CancellationToken ct)
    {
        await PageTitle.Update(_ => title, ct);
    }
}

// --- Supporting records ---

public record EnvInfo(
    string DotNetVersion = ".NET n/a",
    string UnoSdkVersion = "Uno SDK n/a",
    string StudioVersion = "Studio n/a",
    bool HasDotNet = false,
    bool HasUnoSdk = false,
    bool HasStudio = false,
    LicenseInfo? License = null)
{
    public LicenseInfo License { get; init; } = License ?? new LicenseInfo();
}

public record ConnectionStatus(
    bool IsConnected = false,
    int ActiveCount = 0,
    string StatusText = "idle");

public record ScanStatus(
    bool IsActive = true,
    string TooltipText = "Background scanner active",
    bool HasChanges = false);
