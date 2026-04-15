using System.Collections.Immutable;
using System.Text;
using ClaudeDash.Helpers;
using ClaudeDash.Models;

namespace ClaudeDash.ViewModels;

public partial record RalphLoopsModel
{
    private readonly ILogger<RalphLoopsModel> _logger;

    public RalphLoopsModel(ILogger<RalphLoopsModel> logger)
    {
        _logger = logger;
    }

    // 7-stage pipeline: 0=idea, 1=prd, 2=design, 3=scaffold, 4=build, 5=test, 6=ship
    public IState<int> CurrentStage => State.Value(this, () => 0);
    public IState<bool> IsProcessing => State.Value(this, () => false);

    // Stage 0: Idea
    public IState<string> IdeaInput => State.Value(this, () => "");

    public IListFeed<string> SuggestedTags => ListFeed.Async(async ct =>
        ImmutableList.Create(
            "Uno Platform", "WinUI 3", "MVUX", "Material",
            "dashboard", "crud", "auth", "offline", "realtime",
            "mobile", "desktop", "wasm", "responsive"));

    public IListState<string> SelectedTags => ListState.Value(this, () => ImmutableList<string>.Empty);

    // Stage 1: PRD
    public IState<string> PrdInput => State.Value(this, () => "");
    public IState<string> ProjectName => State.Value(this, () => "");

    // Stage 2: Design
    public IState<string> DesignOutput => State.Value(this, () => "");

    // Stage 3: Scaffold
    public IState<int> SelectedThemeIndex => State.Value(this, () => 0);
    public IState<int> SelectedMarkupIndex => State.Value(this, () => 0);
    public IState<int> SelectedMvvmIndex => State.Value(this, () => 0);
    public IState<bool> PlatformDesktop => State.Value(this, () => true);
    public IState<bool> PlatformWasm => State.Value(this, () => false);
    public IState<bool> PlatformIos => State.Value(this, () => false);
    public IState<bool> PlatformAndroid => State.Value(this, () => false);
    public IState<bool> PlatformMacCatalyst => State.Value(this, () => false);

    // Stage 4: Build/Validation
    public IState<bool> IsValidating => State.Value(this, () => false);
    public IState<string> ValidationStatus => State.Value(this, () => "");
    public IListState<string> ValidationResults => ListState.Value(this, () => ImmutableList<string>.Empty);

    // Stage 5: Test
    public IState<string> TestOutput => State.Value(this, () => "");

    // Stage 6: Ship/Output
    public IState<string> GeneratedTruthFile => State.Value(this, () => "");
    public IState<string> GeneratedPrompt => State.Value(this, () => "");

    public async ValueTask ToggleTag(string tag, CancellationToken ct)
    {
        var selected = await SelectedTags ?? ImmutableList<string>.Empty;
        if (selected.Contains(tag))
            await SelectedTags.RemoveAllAsync(t => t == tag, ct);
        else
            await SelectedTags.AddAsync(tag, ct);
    }

    public async ValueTask GeneratePrdFromIdea(CancellationToken ct)
    {
        var idea = await IdeaInput;
        if (string.IsNullOrWhiteSpace(idea)) return;

        await IsProcessing.Set(true, ct);
        try
        {
            await Task.Delay(400, ct); // Simulate processing

            var projectName = await ProjectName;
            var selectedTags = await SelectedTags ?? ImmutableList<string>.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"# {(string.IsNullOrWhiteSpace(projectName) ? "UnoApp" : projectName)}");
            sb.AppendLine();
            sb.AppendLine("## Overview");
            sb.AppendLine(idea);
            sb.AppendLine();

            if (selectedTags.Count > 0)
            {
                sb.AppendLine("## Tags");
                sb.AppendLine(string.Join(", ", selectedTags));
                sb.AppendLine();
            }

            sb.AppendLine("## Requirements");
            sb.AppendLine("- Cross-platform Uno Platform application");
            sb.AppendLine("- Single Project architecture with .NET 10");

            if (selectedTags.Contains("auth"))
                sb.AppendLine("- User authentication (MSAL/OIDC)");
            if (selectedTags.Contains("offline"))
                sb.AppendLine("- Offline-first with local SQLite storage");
            if (selectedTags.Contains("realtime"))
                sb.AppendLine("- Real-time data updates");
            if (selectedTags.Contains("material"))
                sb.AppendLine("- Material Design theme");
            if (selectedTags.Contains("responsive"))
                sb.AppendLine("- Responsive layout for all screen sizes");

            await PrdInput.Set(sb.ToString(), ct);
            await CurrentStage.Set(1, ct);
        }
        finally
        {
            await IsProcessing.Set(false, ct);
        }
    }

    public async ValueTask NextStage(CancellationToken ct)
    {
        var stage = await CurrentStage;
        if (stage < 6)
            await CurrentStage.Set(stage + 1, ct);
    }

    public async ValueTask PreviousStage(CancellationToken ct)
    {
        var stage = await CurrentStage;
        if (stage > 0)
            await CurrentStage.Set(stage - 1, ct);
    }

    public async ValueTask GoToStage(int stage, CancellationToken ct)
    {
        if (stage >= 0 && stage <= 6)
            await CurrentStage.Set(stage, ct);
    }

    private async Task<RalphLoopConfig> BuildConfig()
    {
        var platforms = new List<string>();
        if (await PlatformDesktop) platforms.Add("Desktop");
        if (await PlatformWasm) platforms.Add("WebAssembly");
        if (await PlatformIos) platforms.Add("iOS");
        if (await PlatformAndroid) platforms.Add("Android");
        if (await PlatformMacCatalyst) platforms.Add("Mac Catalyst");

        var themes = new[] { "Material", "Cupertino", "Fluent" };
        var markups = new[] { "XAML", "C# Markup" };
        var patterns = new[] { "MVVM", "MVUX" };

        var themeIdx = await SelectedThemeIndex;
        var markupIdx = await SelectedMarkupIndex;
        var mvvmIdx = await SelectedMvvmIndex;
        var selectedTags = await SelectedTags ?? ImmutableList<string>.Empty;

        return new RalphLoopConfig
        {
            ProjectName = await ProjectName ?? "",
            PrdContent = await PrdInput ?? "",
            IdeaInput = await IdeaInput ?? "",
            Tags = selectedTags.ToList(),
            TargetPlatforms = platforms,
            Theme = themeIdx >= 0 && themeIdx < themes.Length
                ? themes[themeIdx] : "Material",
            MarkupStyle = markupIdx >= 0 && markupIdx < markups.Length
                ? markups[markupIdx] : "XAML",
            MvvmPattern = mvvmIdx >= 0 && mvvmIdx < patterns.Length
                ? patterns[mvvmIdx] : "MVVM"
        };
    }

    public async ValueTask ValidatePrd(CancellationToken ct)
    {
        var prdInput = await PrdInput;
        if (string.IsNullOrWhiteSpace(prdInput))
        {
            await ValidationStatus.Set("Please enter a PRD first.", ct);
            return;
        }

        await IsValidating.Set(true, ct);
        await ValidationStatus.Set("Validating against Uno Platform best practices...", ct);
        await ValidationResults.UpdateAsync(_ => ImmutableList<string>.Empty, ct);

        try
        {
            await Task.Delay(100, ct);

            var config = await BuildConfig();
            var results = new List<string>();

            if (string.IsNullOrWhiteSpace(config.ProjectName))
                results.Add("[warn] No project name specified - consider adding one");

            if (config.TargetPlatforms.Count == 0)
                results.Add("[error] No target platforms selected");
            else
                results.Add($"[ok] Targeting {config.TargetPlatforms.Count} platform(s): {string.Join(", ", config.TargetPlatforms)}");

            results.Add($"[ok] Theme: {config.Theme}");
            results.Add($"[ok] Markup: {config.MarkupStyle}");
            results.Add($"[ok] Pattern: {config.MvvmPattern}");

            var prdLower = prdInput.ToLowerInvariant();

            if (prdLower.Contains("database") || prdLower.Contains("sql"))
                results.Add("[info] Database mentioned - consider SQLite for local storage or Supabase for cloud");
            if (prdLower.Contains("authentication") || prdLower.Contains("login") || prdLower.Contains("sign in"))
                results.Add("[ok] Authentication detected - Uno Platform supports MSAL and OIDC extensions");
            if (prdLower.Contains("map") || prdLower.Contains("location") || prdLower.Contains("gps"))
                results.Add("[ok] Maps/location detected - UnoFeatures includes Maps support");
            if (prdLower.Contains("video") || prdLower.Contains("media player"))
                results.Add("[ok] Media detected - UnoFeatures includes MediaPlayerElement");
            if (prdLower.Contains("camera"))
                results.Add("[warn] Camera access requires platform-specific implementation");
            if (prdLower.Contains("bluetooth") || prdLower.Contains("nfc"))
                results.Add("[warn] Bluetooth/NFC not directly supported by Uno Platform - needs native interop");
            if (prdLower.Contains("chart") || prdLower.Contains("graph") || prdLower.Contains("visualization"))
                results.Add("[info] Charts/visualization detected - consider Liveline or third-party charting library");
            if (prdLower.Contains("push notification"))
                results.Add("[info] Push notifications need platform-specific setup per target");
            if (prdLower.Contains("offline") || prdLower.Contains("local-first"))
                results.Add("[ok] Offline/local-first detected - Uno Platform supports local storage and SQLite");
            if (prdLower.Contains("dark mode") || prdLower.Contains("theme switch"))
                results.Add("[ok] Theme switching supported via Uno Platform ThemeService");
            if (prdLower.Contains("accessibility") || prdLower.Contains("a11y"))
                results.Add("[ok] Accessibility supported via AutomationProperties on all platforms");
            if (config.TargetPlatforms.Contains("WebAssembly") && prdLower.Contains("file system"))
                results.Add("[warn] File system access is limited on WebAssembly - use browser APIs or Storage extension");

            results.Add("[ok] Uno Platform Single Project architecture recommended");
            results.Add("[ok] .NET 10 is the current recommended target");

            await ValidationResults.UpdateAsync(_ => results.ToImmutableList(), ct);
            await ValidationStatus.Set($"Validation complete: {results.Count} items", ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PRD validation failed");
            await ValidationStatus.Set($"Validation failed: {ex.Message}", ct);
        }
        finally
        {
            await IsValidating.Set(false, ct);
        }
    }

    public async ValueTask GenerateTruthFile(CancellationToken ct)
    {
        var config = await BuildConfig();
        var validationResults = await ValidationResults ?? ImmutableList<string>.Empty;
        var prdInput = await PrdInput ?? "";

        var sb = new StringBuilder();

        sb.AppendLine("# TRUTHFILE.uno.md");
        sb.AppendLine();
        sb.AppendLine("## Project Configuration");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(config.ProjectName))
            sb.AppendLine($"- **Project Name**: {config.ProjectName}");
        sb.AppendLine($"- **Framework**: .NET 10 + Uno Platform (Single Project)");
        sb.AppendLine($"- **Target Platforms**: {string.Join(", ", config.TargetPlatforms)}");
        sb.AppendLine($"- **Theme**: {config.Theme}");
        sb.AppendLine($"- **Markup**: {config.MarkupStyle}");
        sb.AppendLine($"- **Pattern**: {config.MvvmPattern}");
        sb.AppendLine();

        sb.AppendLine("## Validated Capabilities");
        sb.AppendLine();
        foreach (var result in validationResults.Where(r => r.StartsWith("[ok]")))
            sb.AppendLine($"- {result.Replace("[ok] ", "")}");
        sb.AppendLine();

        sb.AppendLine("## Warnings and Limitations");
        sb.AppendLine();
        foreach (var result in validationResults.Where(r => r.StartsWith("[warn]") || r.StartsWith("[error]")))
            sb.AppendLine($"- {result.Replace("[warn] ", "").Replace("[error] ", "")}");
        if (!validationResults.Any(r => r.StartsWith("[warn]") || r.StartsWith("[error]")))
            sb.AppendLine("- None detected");
        sb.AppendLine();

        sb.AppendLine("## Notes and Recommendations");
        sb.AppendLine();
        foreach (var result in validationResults.Where(r => r.StartsWith("[info]")))
            sb.AppendLine($"- {result.Replace("[info] ", "")}");
        if (!validationResults.Any(r => r.StartsWith("[info]")))
            sb.AppendLine("- No additional notes");
        sb.AppendLine();

        sb.AppendLine("## PRD Summary");
        sb.AppendLine();
        var prdPreview = prdInput.Length > 500 ? prdInput[..500] + "..." : prdInput;
        sb.AppendLine(prdPreview);

        await GeneratedTruthFile.Set(sb.ToString(), ct);
    }

    public async ValueTask GeneratePrompt(CancellationToken ct)
    {
        var truthFile = await GeneratedTruthFile;
        if (string.IsNullOrWhiteSpace(truthFile))
            await GenerateTruthFile(ct);

        truthFile = await GeneratedTruthFile ?? "";
        var prdInput = await PrdInput ?? "";
        var config = await BuildConfig();

        var sb = new StringBuilder();

        sb.AppendLine("You are an expert Uno Platform developer. Build the following project step by step,");
        sb.AppendLine("validating each component against the TRUTHFILE below.");
        sb.AppendLine();
        sb.AppendLine("## TRUTHFILE");
        sb.AppendLine();
        sb.AppendLine(truthFile);
        sb.AppendLine();
        sb.AppendLine("## Product Requirements Document (PRD)");
        sb.AppendLine();
        sb.AppendLine(prdInput);
        sb.AppendLine();
        sb.AppendLine("## Instructions");
        sb.AppendLine();
        sb.AppendLine("1. Create the project using `dotnet new unoapp` with the configuration above");
        sb.AppendLine("2. Implement each feature from the PRD one at a time");
        sb.AppendLine("3. After each feature, validate it works against the TRUTHFILE constraints");
        sb.AppendLine("4. If a capability is listed as a warning/limitation, find an alternative approach");
        sb.AppendLine("5. Use Uno Platform best practices: Single Project, proper MVVM/MVUX bindings, responsive layouts");
        sb.AppendLine($"6. Use {config.Theme} theme with {config.MarkupStyle} markup and {config.MvvmPattern} pattern");
        sb.AppendLine("7. Ensure cross-platform compatibility across all target platforms");
        sb.AppendLine("8. Add proper error handling and loading states");

        await GeneratedPrompt.Set(sb.ToString(), ct);
    }

    public async ValueTask CopyTruthFile(CancellationToken ct)
    {
        var truthFile = await GeneratedTruthFile;
        if (!string.IsNullOrEmpty(truthFile))
            UiHelpers.CopyToClipboard(truthFile);
    }

    public async ValueTask CopyPrompt(CancellationToken ct)
    {
        var prompt = await GeneratedPrompt;
        if (!string.IsNullOrEmpty(prompt))
            UiHelpers.CopyToClipboard(prompt);
    }
}
