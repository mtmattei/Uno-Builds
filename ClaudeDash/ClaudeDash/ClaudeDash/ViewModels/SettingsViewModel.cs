using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record SettingsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<SettingsModel> _logger;

    public SettingsModel(IClaudeConfigService configService, ILogger<SettingsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IFeed<string> ClaudeDir => Feed.Async(async ct => _configService.GetClaudeDir());

    public IFeed<string> SettingsContent => Feed.Async(async ct =>
    {
        try
        {
            var dir = _configService.GetClaudeDir();
            var settingsPath = System.IO.Path.Combine(dir, "settings.json");
            if (File.Exists(settingsPath))
                return await File.ReadAllTextAsync(settingsPath, ct);
            return "No settings.json found.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read settings");
            return $"Error: {ex.Message}";
        }
    });

    // Model preferences (two-way state)
    public IState<string> DefaultModel => State.Value(this, () => "claude-opus-4");
    public IState<string> FallbackModel => State.Value(this, () => "claude-sonnet-4");
    public IState<bool> AutoSelectByTask => State.Value(this, () => true);
    public IState<string> MaxTokensPerTurn => State.Value(this, () => "16,384");
    public IState<string> MonthlyBudgetCap => State.Value(this, () => "$50.00");
    public IState<string> BudgetWarningThreshold => State.Value(this, () => "80%");

    // Permissions (two-way state)
    public IState<bool> AutoApproveFileEdits => State.Value(this, () => true);
    public IState<bool> AutoApproveShellCommands => State.Value(this, () => false);
    public IState<bool> NetworkAccess => State.Value(this, () => true);
    public IState<bool> SandboxMode => State.Value(this, () => true);
    public IState<bool> GitPushProtection => State.Value(this, () => true);
    public IState<bool> SecretDetection => State.Value(this, () => true);
}

public record McpServerEntry(
    string Name = "",
    string Description = "",
    string Transport = "",
    string CommandOrUrl = "",
    int ToolCount = 0,
    string Status = "running",
    bool IsHealthy = true);

public record EnvVarEntry(
    string Name = "",
    string Value = "",
    bool IsSecret = false,
    bool IsHighlighted = false);
