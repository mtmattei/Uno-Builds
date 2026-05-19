# Architecture

## Stack ledger

```
Runtime          · .NET 10
Targets          · WebAssembly (primary)
Markup           · XAML
State            · MVUX (Uno.Extensions.Reactive)
Renderer         · Skia for WebAssembly
Theme            · Material (Uno.Toolkit.UI.Material) with monochromatic overrides
Navigation       · None (single-page app — no Region, no Frame)
Dependency Inj.  · Microsoft.Extensions.DependencyInjection via Uno IHostBuilder
HTTP             · Refit (single endpoint for Anthropic Messages API)
Logging          · Serilog with Microsoft.Extensions.Logging integration
Localization     · Uno.Extensions.Localization (default English; structure ready for FR)
File I/O         · Windows.Storage.Pickers (FileSavePicker via WASM download fallback)
Configuration    · UseConfiguration with appsettings.json (API endpoint base URL only)
```

Single-page application; no navigation framework needed. The entire app is one `MainPage` with three structural regions composed via XAML rather than `Frame.Navigate`.

## MVUX model shape

The composer is one top-level model (`ComposerModel`) with a single immutable record `ComposerState` driving every visible piece of state. MVUX's `IState<ComposerState>` exposes a hot stream of state values to the view; bindings observe the stream and re-render selectively.

### State record

```csharp
namespace Composer.Models;

public record ComposerState(
    // ─── Foundation ───────────────────────────────
    string AppName,
    string AppDescription,
    ImmutableArray<string> Platforms,           // values from PlatformOptions
    string Runtime,                              // ".NET 10" or ".NET 9"

    // ─── Chat ─────────────────────────────────────
    ImmutableArray<ChatTurn> Messages,
    string InputValue,
    bool IsThinking,
    string? JustAddedMessageId,
    string? ErrorMessage,

    // ─── Suggestion state ─────────────────────────
    bool AlternativesOpen,
    DesignAssets DesignAssets,
    ImmutableDictionary<string, string> ContextualReasonings,  // layer → sentence

    // ─── Artifacts ────────────────────────────────
    ImmutableDictionary<string, ArtifactState> Artifacts,
    ImmutableHashSet<string> ExpandedIds,
    string? EditingArtifactId
)
{
    public bool FoundationReady =>
        !string.IsNullOrWhiteSpace(AppName) && Platforms.Length > 0;

    public ChatTurn? LatestComposerTurn =>
        Messages.LastOrDefault(m => m.Role == TurnRole.Composer);

    public string? CurrentLayer => LatestComposerTurn?.Layer;
}

public record ChatTurn(
    string Id,
    TurnRole Role,                 // User | Composer
    string Layer,                  // description | wiring | design | …
    string Body,
    ImmutableArray<Callout> Callouts,
    LayerSuggestion? Suggestion,   // null for user turns
    string? AppliedLabel,          // set when locked-in
    DateTimeOffset Timestamp
);

public enum TurnRole { User, Composer }

public record Callout(string LayerLabel, string Note);

public record LayerSuggestion(
    string Label,
    string Reasoning,
    SuggestionAction Action,
    ImmutableArray<SuggestionAction> Alternatives,
    bool RequiresAssets,            // true only for design layer
    string FreeTextHint
);

public record SuggestionAction(
    string Label,
    string NextLayer,
    ImmutableDictionary<string, string> Updates  // artifactId → content
);

public record ArtifactState(string Status, string Content);
public static class ArtifactStatus
{
    public const string Planned = "Planned";
    public const string Drafted = "Drafted";
}

public record DesignAssets(
    string FigmaUrl,
    string PrototypeUrl,
    string ScreenshotUrl
);
```

All collections are `ImmutableArray`, `ImmutableDictionary`, or `ImmutableHashSet`. State transitions return new state records — never mutate.

### Top-level MVUX model

```csharp
public partial record ComposerModel
{
    private readonly IAnthropicClient _api;
    private readonly IBundleExporter _exporter;

    public ComposerModel(IAnthropicClient api, IBundleExporter exporter)
    {
        _api = api;
        _exporter = exporter;
    }

    // ─── Hot state stream ─────────────────────────────────────────
    public IState<ComposerState> State => Composer.State.Value(
        this,
        () => InitialState());

    private static ComposerState InitialState() => new(
        AppName: "",
        AppDescription: "",
        Platforms: ImmutableArray.Create("Web", "Android", "iOS"),
        Runtime: ".NET 10",
        Messages: ImmutableArray<ChatTurn>.Empty,
        InputValue: "",
        IsThinking: false,
        JustAddedMessageId: null,
        ErrorMessage: null,
        AlternativesOpen: false,
        DesignAssets: new("", "", ""),
        ContextualReasonings: ImmutableDictionary<string, string>.Empty,
        Artifacts: BuildInitialArtifacts(),
        ExpandedIds: ImmutableHashSet<string>.Empty,
        EditingArtifactId: null
    );

    // ─── Public commands (auto-detected by MVUX as ICommand) ──────

    public async ValueTask SetAppName(string name)
        => await State.UpdateAsync(s => RefreshFoundation(s with { AppName = name }));

    public async ValueTask TogglePlatform(string platform)
        => await State.UpdateAsync(s => RefreshFoundation(s with
        {
            Platforms = s.Platforms.Contains(platform)
                ? s.Platforms.Remove(platform)
                : s.Platforms.Add(platform)
        }));

    public async ValueTask SetRuntime(string runtime)
        => await State.UpdateAsync(s => RefreshFoundation(s with { Runtime = runtime }));

    public async ValueTask BeginConversation()
        => await State.UpdateAsync(s => s.FoundationReady && s.Messages.IsEmpty
            ? PushPrompt(s, "description")
            : s);

    public async ValueTask ApplySuggestion()
        => await State.UpdateAsync(s =>
        {
            var suggestion = s.LatestComposerTurn?.Suggestion;
            return suggestion is null ? s : ApplyAction(s, suggestion.Action);
        });

    public async ValueTask PickAlternative(SuggestionAction alternative)
        => await State.UpdateAsync(s => ApplyAction(s, alternative));

    public async ValueTask ApplyWithAssets()
        => await State.UpdateAsync(s =>
        {
            // Build the design action dynamically from current asset state
            var current = s.LatestComposerTurn?.Suggestion;
            if (current is null) return s;
            var action = current.Action with
            {
                Updates = current.Action.Updates.SetItem(
                    "design",
                    ArtifactTemplates.Design(s.DesignAssets, s.Runtime))
            };
            return ApplyAction(s, action);
        });

    public async ValueTask SubmitInput()
        => await State.UpdateAsync(async s =>
        {
            var text = s.InputValue.Trim();
            if (text.Length == 0 || s.IsThinking) return s;

            var layer = s.CurrentLayer;
            if (layer is null) return s;

            // Description layer: handle locally without API
            if (layer == "description")
            {
                var withDescription = s with
                {
                    AppDescription = text,
                    InputValue = "",
                    Messages = s.Messages.Add(new ChatTurn(
                        Id: $"u-{Guid.NewGuid():N}",
                        Role: TurnRole.User,
                        Layer: layer,
                        Body: text,
                        Callouts: ImmutableArray<Callout>.Empty,
                        Suggestion: null,
                        AppliedLabel: null,
                        Timestamp: DateTimeOffset.UtcNow))
                };
                _ = FetchContextualReasoningsAsync(withDescription.AppName, text);
                return PushPrompt(RefreshFoundation(withDescription), "wiring");
            }

            // Other layers: API override
            return await SubmitFreeTextOverrideAsync(s, text, layer);
        });

    public async ValueTask EditArtifact(string artifactId)
        => await State.UpdateAsync(s => s with { EditingArtifactId = artifactId });

    public async ValueTask CommitArtifactEdit(string artifactId, string content)
        => await State.UpdateAsync(s => s with
        {
            Artifacts = s.Artifacts.SetItem(artifactId,
                s.Artifacts[artifactId] with { Content = content }),
            EditingArtifactId = null
        });

    public async ValueTask GoBackTo(int messageIndex)
        => await State.UpdateAsync(s => Rollback(s, messageIndex));

    public async ValueTask DownloadBundle()
        => await _exporter.SaveAsync(await State);

    // ─── Pure transitions ────────────────────────────────────────

    private static ComposerState RefreshFoundation(ComposerState s)
        => s with
        {
            Artifacts = s.Artifacts
                .SetItem("readme",   new(ArtifactStatus.Drafted,
                    ArtifactTemplates.Readme(s)))
                .SetItem("claude",   new(ArtifactStatus.Drafted,
                    ArtifactTemplates.Claude(s)))
                .SetItem("scaffold", new(ArtifactStatus.Drafted,
                    ArtifactTemplates.Scaffold(s)))
        };

    private static ComposerState PushPrompt(ComposerState s, string layer)
    {
        var built = LayerPrompts.Build(layer, s);
        if (built is null) return s;

        return s with
        {
            Messages = s.Messages.Add(built),
            JustAddedMessageId = built.Id,
            ExpandedIds = LayerToArtifacts.Get(layer).ToImmutableHashSet(),
            EditingArtifactId = null,
            AlternativesOpen = false
        };
    }

    private static ComposerState ApplyAction(ComposerState s, SuggestionAction action)
    {
        // Mark prior composer turn as locked-in
        var marked = s.Messages.IsEmpty ? s.Messages : MarkLatestComposer(s.Messages, action.Label);

        // Apply artifact updates
        var nextArtifacts = action.Updates.Aggregate(
            s.Artifacts,
            (acc, kv) => acc.SetItem(kv.Key, new(ArtifactStatus.Drafted, kv.Value)));

        // Push next layer prompt
        var built = LayerPrompts.Build(action.NextLayer, s);
        var withNextPrompt = built is null
            ? marked
            : marked.Add(built);

        return s with
        {
            Messages = withNextPrompt,
            JustAddedMessageId = built?.Id,
            Artifacts = nextArtifacts,
            ExpandedIds = LayerToArtifacts.Get(action.NextLayer).ToImmutableHashSet(),
            EditingArtifactId = null,
            AlternativesOpen = false
        };
    }

    private static ImmutableArray<ChatTurn> MarkLatestComposer(
        ImmutableArray<ChatTurn> messages, string label)
    {
        for (int i = messages.Length - 1; i >= 0; i--)
        {
            if (messages[i].Role == TurnRole.Composer)
                return messages.SetItem(i, messages[i] with { AppliedLabel = label });
        }
        return messages;
    }

    private static ComposerState Rollback(ComposerState s, int messageIndex)
    {
        var trimmed = s.Messages.Take(messageIndex).ToImmutableArray();
        var targetLayer = s.Messages[messageIndex].Layer;
        var resetIds = ArtifactResets.From(targetLayer);
        var resetArtifacts = resetIds.Aggregate(
            s.Artifacts,
            (acc, id) => acc.SetItem(id, new(ArtifactStatus.Planned, "")));

        var rolled = s with
        {
            Messages = trimmed,
            Artifacts = resetArtifacts,
            AppDescription = targetLayer == "description" ? "" : s.AppDescription,
            ExpandedIds = ImmutableHashSet<string>.Empty,
            EditingArtifactId = null,
            AlternativesOpen = false
        };

        // Re-push the layer's prompt fresh so it becomes the new latest
        return PushPrompt(rolled, targetLayer);
    }

    // ... (FetchContextualReasoningsAsync, SubmitFreeTextOverrideAsync,
    //      BuildInitialArtifacts elided for brevity — see source)
}
```

The model is a `partial record` because MVUX's source generators add the implicit `IAsyncCommand` properties for each public method. Bindings in XAML target these auto-generated commands by name (e.g., `Command="{Binding ApplySuggestion}"`).

### View → state binding pattern

Views bind to slices of `ComposerState` via MVUX's `Feed.Select` for derived values, e.g.:

```xml
<TextBox Text="{Binding AppName, Mode=TwoWay}" x:Uid="FoundationPanel.TextBox.AppName"/>
<ItemsRepeater ItemsSource="{Binding Messages}">
  <DataTemplate x:DataType="m:ChatTurn">
    <local:MessageBlock Turn="{Binding}"/>
  </DataTemplate>
</ItemsRepeater>
<Button Visibility="{Binding FoundationReady, Converter={StaticResource BoolToVisibility}}"
        x:Uid="FoundationPanel.Button.Begin"
        Command="{Binding BeginConversation}"/>
```

For derived UI states like `IsLatestComposer` (used to determine whether to render a turn as full or compact), compute the derivation in a `DataTemplateSelector` that takes the message index and the messages list.

## Service layer

Three services registered in DI.

### IAnthropicClient

A Refit interface for the single endpoint used. `IServiceCollection.AddRefitClient<IAnthropicClient>()` in `App.xaml.cs`.

```csharp
[Headers("Content-Type: application/json")]
public interface IAnthropicClient
{
    [Post("/v1/messages")]
    Task<MessagesResponse> CreateMessageAsync(
        [Body] MessagesRequest request,
        [Header("x-api-key")] string apiKey,
        [Header("anthropic-version")] string version,
        CancellationToken ct = default);
}
```

The API key is **not** baked into the WASM bundle. Two viable patterns: (a) a thin proxy service deployed alongside the static host that forwards requests with the key server-side; (b) a development-mode local-storage fallback where the developer pastes their own key. For the prototype, start with pattern (b) and document the migration to (a) for production.

### IBundleExporter

```csharp
public interface IBundleExporter
{
    Task SaveAsync(ComposerState state);
}

public class BundleExporter : IBundleExporter
{
    public async Task SaveAsync(ComposerState state)
    {
        var picker = new FileSavePicker
        {
            SuggestedFileName = $"{Slug(state.AppName)}-bundle",
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeChoices.Add("Markdown", new List<string> { ".md" });

        var file = await picker.PickSaveFileAsync();
        if (file is null) return;

        await FileIO.WriteTextAsync(file, BuildBundle(state));
    }

    private static string BuildBundle(ComposerState s) => /* concatenate all artifacts */;
}
```

On WebAssembly, `FileSavePicker` falls back to a download-style picker that triggers the browser's native save flow.

### Localization service

Standard Uno localization via `Uno.Extensions.Localization`. All visible text uses `x:Uid`. The `Strings/en/Resources.resw` contains every string keyed by the pattern `{ControlName}.{PropertyName}.{Variant}`.

## Component → control mapping

The React prototype's components map directly to Uno controls:

| React component | Uno control | Notes |
|---|---|---|
| `App` | `MainPage` (`Page`) | Root layout |
| `MonoLabel` | `TextBlock` with `MonoEyebrow` style | Use `x:Uid` for localized strings |
| `Callout` | `Border` + `TextBlock` | Custom style with left-border treatment |
| `MessageBlock` | `UserControl` | Drives entrance animation via `Loaded` |
| `CompactComposerTurn` | `UserControl` | Selected by `DataTemplateSelector` based on whether turn is latest |
| `SuggestionPanel` | `UserControl` | Contains the tinted card frame + actions + alternatives expander |
| `AssetField` | Custom `UserControl` over `TextBox` | Mono typography, hairline border |
| `ThinkingIndicator` | `UserControl` with three `Ellipse` + `Storyboard` | Storyboard set up in resources, started on Loaded |
| `InputBox` | `UserControl` | TextBox + suggestion chips WrapPanel + Send button |
| `StatusGlyph` | `UserControl` | Ellipse + Path with `VisualStateManager` for Drafted/Planned |
| `Chevron` | Inline `Path` with `RotateTransform` | No `UserControl` needed; small enough to inline |
| `ArtifactCard` | `UserControl` | StatusGlyph + filename row + collapsible body |
| `PlatformIcon` | Inline `Path` from `IconWeb` etc. resources | Geometry resources |
| `PlatformChip` | `ToggleButton` with custom template | Two visual states with chip morph storyboard |

The React prototype's pure functions (`ArtifactTemplates`, `LayerPrompts`, `LayerToArtifacts`, `ArtifactResets`) translate to static C# classes in `Models/Templates.cs`.

## App startup

Standard Uno `App.xaml.cs` with `IHostBuilder`:

```csharp
public partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var host = UnoHost
            .CreateDefaultBuilder()
            .UseSerilog()
            .UseConfiguration(configure: cfg =>
                cfg.EmbeddedSource<App>()
                   .Section<AnthropicConfig>("Anthropic"))
            .UseHttp((ctx, services) =>
                services.AddRefitClient<IAnthropicClient>(ctx.Configuration))
            .UseLocalization()
            .UseToolkitNavigation()  // optional even with single page; loads Material
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IBundleExporter, BundleExporter>();
                services.AddSingleton<ComposerModel>();
            })
            .Build();

        Host = host;

        var window = new Window();
        window.Content = new MainPage();
        window.Activate();
    }
}
```

`MainPage.xaml` sets its `DataContext` to `App.Host.Services.GetRequiredService<ComposerModel>()` in the code-behind constructor. From there, every binding flows from the model.

## Theming

`App.xaml` merges in:

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <MaterialColors xmlns="using:Uno.Toolkit.UI.Material" />
      <MaterialFonts  xmlns="using:Uno.Toolkit.UI.Material" />
      <MaterialResources xmlns="using:Uno.Toolkit.UI.Material" />
      <ResourceDictionary Source="Themes/Colors.xaml"/>
      <ResourceDictionary Source="Themes/Typography.xaml"/>
      <ResourceDictionary Source="Themes/Icons.xaml"/>
      <ResourceDictionary Source="Themes/ChipStyles.xaml"/>
      <ResourceDictionary Source="Themes/ArtifactCardStyle.xaml"/>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

The override sequence matters: Material loads first, then `Colors.xaml` redefines key brushes (`PrimaryBrush`, `BackgroundBrush`, `OnSurfaceBrush`) to point at our monochromatic palette. `Typography.xaml` redefines `BodyTextBlockStyle`, `CaptionTextBlockStyle`, etc. to use Fraunces and Martian Mono.

No light/dark theme switching. The app is locked to light mode by deliberate choice (matches the editorial aesthetic).

## Responsive behavior

The two-column layout is implemented with `ResponsiveView` from Uno Toolkit. The layout has three configurations that switch at 600px and 905px viewport width.

```xml
<utu:ResponsiveView>
  <utu:ResponsiveView.NarrowestTemplate>
    <DataTemplate>
      <!-- < 600px: stacked accordion -->
    </DataTemplate>
  </utu:ResponsiveView.NarrowestTemplate>
  <utu:ResponsiveView.NarrowTemplate>
    <DataTemplate>
      <!-- 600-904px: stacked, foundation pinned -->
    </DataTemplate>
  </utu:ResponsiveView.NarrowTemplate>
  <utu:ResponsiveView.NormalTemplate>
    <DataTemplate>
      <!-- ≥ 905px: two-column grid -->
    </DataTemplate>
  </utu:ResponsiveView.NormalTemplate>
</utu:ResponsiveView>
```

Each template binds to the same `ComposerModel` via `DataContext` inheritance — the model is layout-agnostic, the views are layout-specific.

## Runtime targets

For development: `dotnet run -f net10.0-browserwasm` from `src/Composer/`.

For production: `dotnet publish -f net10.0-browserwasm -c Release -o ./dist` produces a static-file deployable `dist/wwwroot/` ready to host on any static CDN. The Skia renderer adds about 4MB to the initial bundle (uncompressed); brotli compression reduces that by ~70%. AOT compilation of the IL is enabled by default for Release builds and reduces startup time by 40-50% at the cost of larger compiled output.

The `Composer.csproj` `<UnoFeatures>` declares: `Material Skia SkiaRenderer Toolkit MVUX Hosting Configuration HttpRefit Logging LoggingSerilog Localization`. The Skia + SkiaRenderer pair is what enables the cross-browser high-fidelity rendering this design depends on (especially for the chip morph and status glyph animations, which need exact subpixel paths the DOM renderer doesn't provide on all browsers).
