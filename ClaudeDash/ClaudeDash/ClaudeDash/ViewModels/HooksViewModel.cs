using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record HooksModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<HooksModel> _logger;

    public HooksModel(
        IClaudeConfigService configService,
        ILogger<HooksModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);
    public IState<string> ConfigPreview => State.Value(this, () => string.Empty);

    public IListFeed<string> AllowedDomains => ListFeed.Async(async ct =>
    {
        return ImmutableList.Create(
            "api.anthropic.com",
            "github.com",
            "registry.npmjs.org",
            "pypi.org",
            "files.pythonhosted.org",
            "crates.io",
            "archive.ubuntu.com",
            "security.ubuntu.com",
            "npmjs.com",
            "yarnpkg.com");
    });

    public IListState<HookInfo> Hooks => ListState.Async(this, async ct =>
    {
        try
        {
            var hooks = await _configService.GetHooksAsync();

            if (hooks.Count == 0)
            {
                hooks = GenerateMockHooks();
            }

            LoadConfigPreview(ct);

            return hooks.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load hooks");
            await ErrorMessage.Set(ex.Message, ct);

            // Still load mock data on error so the page is useful
            var mockHooks = GenerateMockHooks();
            LoadConfigPreview(ct);
            return mockHooks.ToImmutableList();
        }
    });

    public IFeed<int> TotalHooks => Feed.Async(async ct =>
    {
        var hooks = await Hooks;
        return hooks?.Count ?? 0;
    });

    public IFeed<int> ActiveCount => Feed.Async(async ct =>
    {
        var hooks = await Hooks;
        return hooks?.Count(h => h.IsActive) ?? 0;
    });

    private void LoadConfigPreview(CancellationToken ct)
    {
        _ = ConfigPreview.Set(@"[models]
default = ""claude-opus-4""
fallback = ""claude-sonnet-4""
auto_select = true
max_tokens = 16384

[budget]
monthly_cap = 50.00
warn_at = 0.80

[permissions]", ct);
    }

    private static List<HookInfo> GenerateMockHooks()
    {
        return new List<HookInfo>
        {
            new HookInfo
            {
                Name = "pre-commit lint",
                Description = "Run ESLint + Prettier before any commit",
                TriggerType = "pre-commit",
                Action = "npx lint-staged",
                IsActive = true,
                HookType = "PreCommit",
                Command = "npx lint-staged",
                Matcher = "*"
            },
            new HookInfo
            {
                Name = "type check",
                Description = "Run tsc --noEmit after file edits in .ts files",
                TriggerType = "post-edit",
                Action = "npx tsc --noEmit",
                IsActive = true,
                HookType = "PostEdit",
                Command = "npx tsc --noEmit",
                Matcher = "*.ts"
            },
            new HookInfo
            {
                Name = "test runner",
                Description = "Run related test suites when source files change",
                TriggerType = "post-edit",
                Action = "vitest related",
                IsActive = true,
                HookType = "PostEdit",
                Command = "vitest related",
                Matcher = "*"
            },
            new HookInfo
            {
                Name = "notify on failure",
                Description = "Send Slack message when a session ends with error",
                TriggerType = "on-error",
                Action = "mcp:slack \u2192 post",
                IsActive = true,
                HookType = "OnError",
                Command = "mcp:slack \u2192 post",
                Matcher = "*"
            }
        };
    }
}
