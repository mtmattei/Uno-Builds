using System.Collections.Immutable;
using System.Diagnostics;
using ClaudeDash.Models;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record UnoPlatformOverviewModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<UnoPlatformOverviewModel> _logger;

    public UnoPlatformOverviewModel(
        IClaudeConfigService configService,
        ILogger<UnoPlatformOverviewModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);
    public IState<bool> IsCheckRunning => State.Value(this, () => false);
    public IState<string> StudioLicenseStatus => State.Value(this, () => "unknown");
    public IState<string> GeneratedCommand => State.Value(this, () => "");

    // Project configurator fields
    public IState<string> ProjectName => State.Value(this, () => "");
    public IState<int> SelectedTemplateIndex => State.Value(this, () => 0);
    public IState<int> SelectedThemeIndex => State.Value(this, () => 0);
    public IState<int> SelectedMarkupIndex => State.Value(this, () => 0);
    public IState<int> SelectedMvvmIndex => State.Value(this, () => 0);

    // Platform toggles
    public IState<bool> PlatformDesktop => State.Value(this, () => true);
    public IState<bool> PlatformWasm => State.Value(this, () => false);
    public IState<bool> PlatformIos => State.Value(this, () => false);
    public IState<bool> PlatformAndroid => State.Value(this, () => false);
    public IState<bool> PlatformMacCatalyst => State.Value(this, () => false);

    // Uno Features
    public IState<bool> FeatureExtensions => State.Value(this, () => false);
    public IState<bool> FeatureToolkit => State.Value(this, () => false);
    public IState<bool> FeatureNavigation => State.Value(this, () => false);
    public IState<bool> FeatureMvvm => State.Value(this, () => true);
    public IState<bool> FeatureConfiguration => State.Value(this, () => false);
    public IState<bool> FeatureHttp => State.Value(this, () => false);
    public IState<bool> FeatureLocalization => State.Value(this, () => false);
    public IState<bool> FeatureLogging => State.Value(this, () => false);

    public IFeed<UnoPlatformInfo> UnoInfo => Feed.Async(async ct =>
    {
        try
        {
            var info = await _configService.GetUnoPlatformInfoAsync();
            await StudioLicenseStatus.Set(info.LicenseTier, ct);
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Uno Platform info");
            await ErrorMessage.Set(ex.Message, ct);
            return new UnoPlatformInfo();
        }
    });

    public IListState<UnoCheckResult> CheckResults => ListState.Value(this, () => ImmutableList<UnoCheckResult>.Empty);

    public IFeed<int> CheckPassedCount => Feed.Async(async ct =>
    {
        var results = await CheckResults;
        return results?.Count(r => r.Status == "ok") ?? 0;
    });

    public IFeed<int> CheckWarningCount => Feed.Async(async ct =>
    {
        var results = await CheckResults;
        return results?.Count(r => r.Status == "warning") ?? 0;
    });

    public IFeed<int> CheckErrorCount => Feed.Async(async ct =>
    {
        var results = await CheckResults;
        return results?.Count(r => r.Status == "error") ?? 0;
    });

    public async ValueTask RunUnoCheck(CancellationToken ct)
    {
        await IsCheckRunning.Set(true, ct);
        try
        {
            var results = await _configService.RunUnoCheckAsync();
            await CheckResults.UpdateAsync(_ => results.ToImmutableList(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to run uno-check");
            await ErrorMessage.Set(ex.Message, ct);
        }
        finally
        {
            await IsCheckRunning.Set(false, ct);
        }
    }

    public ValueTask OpenDocs(CancellationToken ct)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://platform.uno/docs/",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open Uno Platform docs");
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask OpenSamples(CancellationToken ct)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://platform.uno/docs/articles/samples.html",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open Uno Platform samples");
        }
        return ValueTask.CompletedTask;
    }

    public async ValueTask GenerateCommand(CancellationToken ct)
    {
        var parts = new List<string> { "dotnet new unoapp" };

        var projectName = await ProjectName;
        if (!string.IsNullOrWhiteSpace(projectName))
            parts.Add($"-n {projectName}");

        // Platforms
        var platforms = new List<string>();
        if (await PlatformDesktop) platforms.Add("desktop");
        if (await PlatformWasm) platforms.Add("wasm");
        if (await PlatformIos) platforms.Add("ios");
        if (await PlatformAndroid) platforms.Add("android");
        if (await PlatformMacCatalyst) platforms.Add("maccatalyst");

        if (platforms.Count > 0 && platforms.Count < 5)
            parts.Add($"--platforms \"{string.Join(';', platforms)}\"");

        // Theme
        var themes = new[] { "material", "cupertino", "fluent" };
        var themeIndex = await SelectedThemeIndex;
        if (themeIndex >= 0 && themeIndex < themes.Length)
            parts.Add($"--theme {themes[themeIndex]}");

        // Markup
        var markupIndex = await SelectedMarkupIndex;
        if (markupIndex == 1)
            parts.Add("--markup csharp");

        // MVVM pattern
        var mvvmIndex = await SelectedMvvmIndex;
        if (mvvmIndex == 1)
            parts.Add("--presentation mvux");

        // Features
        var features = new List<string>();
        if (await FeatureExtensions) features.Add("extensions");
        if (await FeatureToolkit) features.Add("toolkit");
        if (await FeatureNavigation) features.Add("navigation");
        if (await FeatureMvvm) features.Add("mvvm");
        if (await FeatureConfiguration) features.Add("configuration");
        if (await FeatureHttp) features.Add("http");
        if (await FeatureLocalization) features.Add("localization");
        if (await FeatureLogging) features.Add("logging");

        if (features.Count > 0)
            parts.Add($"--features \"{string.Join(';', features)}\"");

        await GeneratedCommand.Set(string.Join(" ", parts), ct);
    }
}
