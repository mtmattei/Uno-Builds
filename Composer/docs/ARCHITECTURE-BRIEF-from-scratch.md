# Composer Context Engine — Architecture Brief

**Status:** canonical, self-contained
**Version:** 1.0 (from-scratch build)
**Audience:** an implementation agent constructing the app from nothing on Uno Platform
**Stack:** C# 13, .NET 10, Uno Platform 6.5.29+, MVUX, Region Navigation, Material theme, Skia renderer, Kiota HTTP, Single Project layout
**Companion briefs:** `DESIGN-BRIEF-from-scratch.md` (visual truth), `INTERACTION-BRIEF-from-scratch.md` (behavioral truth)

This brief specifies the structural truth of the Composer Context Engine: every class, every interface, every IFeed/IState signature, every DI registration, every route, every file in the project tree. An implementing agent with this brief alone should be able to scaffold the entire project skeleton, wire the MVUX model graph, register navigation routes, and produce a compiling shell — before any visual styling or behavioral logic is added.

The brief is non-negotiable on canonical Uno patterns: MVUX records with `IState<T>` / `IFeed<T>` / `IListFeed<T>` (never `INotifyPropertyChanged`); Region-based Navigation with `uen:Region.Attached` and named regions (never `Frame.Navigate` from code-behind); implicit `IAsyncCommand` bindings discovered from public method names (never `ICommand` properties); Material theme with `x:Uid` localization keys; Single Project layout with `Uno.Sdk`.

---

## Table of contents

1. Solution overview
2. Stack and conventions
3. Solution structure
4. Hosting bootstrap
5. The layer model
6. State machine
7. Shell architecture
8. Navigation graph
9. Layer models
10. Services
11. Derived feeds and context flow
12. Page registration and structure
13. Resource dictionary structure
14. Lifecycle and process model
15. Data flow
16. Error handling
17. Performance budgets
18. Testing strategy
19. Acceptance criteria
20. Out of scope

---

## 1. Solution overview

### 1.1 What this application is

The Composer Context Engine is a workspace application where a user composes a software project across nine named context layers in sequence. Each layer captures a specific dimension of the project (stack defaults, intent, UX, architecture, design, interactions, data, implementation phasing, scaffold output). As each layer locks, the engine writes a structured markdown brief reflecting that layer's decisions. The final output is nine briefs concatenated into a single `prompt-context.md` document an AI coding agent (Claude Code, Cursor, GitHub Copilot, etc.) can execute against to produce a real Uno Platform application.

### 1.2 What it produces

A composition session produces:

- **Nine layer briefs** (`stack-preferences.md`, `README.md`, `ux-flows.md`, `architecture.md`, `design-system.md`, `interaction-spec.md`, `data-contracts.md`, `implementation-plan.md`, `scaffold.command`) — each between 100 and 400 lines of structured markdown
- **One concatenated bundle** (`prompt-context.md`) — all nine briefs joined with `<!-- {filename} -->` separators, totaling 1500–2500 lines
- **One scaffold command** — a `dotnet new unoapp` invocation with flags derived from the locked Stack layer

### 1.3 What it does not produce

The Composer Context Engine does not produce executable code. It produces specifications. A downstream agent consumes those specifications and produces code. This boundary is load-bearing — the engine's value is the precision of the specifications, not the convenience of automatic compilation.

### 1.4 Target deployment

The engine is itself a cross-platform Uno application. It runs on iOS, Android, macOS, Linux, Windows (WinAppSDK), and WebAssembly. Skia is the rendering backend on every platform.

---

## 2. Stack and conventions

### 2.1 Required stack

| Concern              | Choice                                             | Rationale                                                       |
|----------------------|----------------------------------------------------|-----------------------------------------------------------------|
| Language             | C# 13                                              | Latest stable, primary constructors and required properties     |
| Runtime              | .NET 10                                            | Latest stable                                                   |
| Cross-platform SDK   | `Uno.Sdk` 6.5.29 or later                          | Pinned in `global.json`                                         |
| Project layout       | Single Project                                     | One `.csproj`, platform code under `Platforms/{Platform}/`      |
| Renderer             | Skia                                               | Consistent rendering across all targets                         |
| Markup               | XAML                                               | Required for `utu:VisualStateManager.States` binding contract   |
| Reactive pattern     | MVUX                                               | Records with `IState<T>` / `IFeed<T>` / `IListFeed<T>`           |
| Navigation           | Region-based (Uno.Extensions.Navigation)           | Named regions, RouteMap registration, nested routes             |
| Theme                | Material (Uno Toolkit Material)                    | Canonical Uno theme; `Cupertino`/`Fluent` not used               |
| HTTP                 | Kiota (typed clients from OpenAPI)                 | Used only for AI augmentation calls; v1.0 ships with no HTTP    |
| Logging              | `Microsoft.Extensions.Logging` via `UseLogging`    | Standard hosting pattern                                        |
| Configuration        | `Microsoft.Extensions.Configuration`               | App settings, feature flags                                     |
| DI                   | `Microsoft.Extensions.DependencyInjection`         | Standard hosting pattern, configured via `ConfigureServices`    |

### 2.2 Non-negotiable conventions

These rules apply throughout. An agent violating any of them produces structurally incorrect code.

**Navigation:**
- Pages do not contain navigation code in their `.xaml.cs` files
- All navigation requests originate from MVUX model methods
- All navigation uses `INavigator.NavigateRouteAsync(this, route, qualifier: Qualifiers.Nested)`
- Routes are nested under the Shell route (never flat siblings of Shell)
- Initial navigation is set via `IsDefault: true` on the first nested route

**Bindings:**
- All bindings use `Mode=OneWay` for read paths, `Mode=TwoWay` for editable surfaces
- Commands bind to implicit `IAsyncCommand` properties discovered by name from MVUX models
- Public async methods on models become invokable commands automatically (no `ICommand` declaration needed)
- Never use `StringFormat=...` in bindings
- Never use `{x:Static}` or `{x:Reference}` (WPF-only)

**Visual:**
- Never hardcode hex colors in component XAML — always reference `ThemeResource` keys
- Never set fixed `Width` or `Height` in percentages — use star sizing or absolute values from the 4-point scale
- Never mix conflicting inline visual properties on top of a Style
- Always reference TextBlock styles from `Themes/TextBlock.xaml` — never set explicit `FontSize` or `FontWeight` on TextBlocks
- Always add `x:Uid` to visible/interactive elements following the pattern `{Page}.{Role}.{Identifier}` (e.g., `IntentPage.Label.AppType`)
- Always set `AutomationProperties.Name` (or `LabeledBy`) on inputs, buttons, and templated list items
- Use `AutoLayout` (Uno.Toolkit) for vertical flows; do not set per-child margins; use the container's `Spacing` instead

**State:**
- Layer canvas state lives in MVUX models, not page code-behind
- `IState<T>` is the unit of mutable state ownership
- `IFeed<T>` is the unit of derived/computed read-only state
- `IListFeed<T>` is the unit of collection-shaped derived state
- Snapshots for discard/restore are captured per-layer in a dedicated `LayerSnapshot` record

**Files:**
- One model class per layer (`{Layer}Model.cs`)
- One page class per layer (`{Layer}Page.xaml` + `.xaml.cs`)
- One canvas user control per layer (`{Layer}Canvas.xaml` + `.xaml.cs`)
- One control user control per reusable element (`LockedContextCard.xaml`, `ComposerFooter.xaml`, etc.)
- Resource dictionaries split by concern under `Themes/` (one for buttons, one for typography, etc.)

### 2.3 Implementation rules carried from Uno documentation

These come from the Uno Platform agent rules and must be observed at all times:

- Prefer Material theme resources when both Material and Fluent are present
- Use `ControlExtensions.Icon` (Toolkit) for icon-only buttons; never use `AppBarButton` outside a `CommandBar`
- Apply `ThemeShadow` to focal elements (CTAs, primary cards) with `Translation` Z values in the 8–32 range; never apply outer-container shadows on cards that already encode elevation
- For lists fitting on screen, use `ItemsRepeater`; for long lists, use `ListView`; for selection or per-item action, use `CommandExtensions` from Uno.Toolkit, not click handlers on item template roots
- Bind `bool` directly to `Visibility` (implicit conversion is supported)
- Use multiple `<Run>` elements to concatenate strings in TextBlocks — never `StringFormat`
- Compose row/column definitions with star sizing; avoid percentages
- Localization: every visible/interactive element gets an `x:Uid`; resource keys follow `{Page}.{Role}.{Identifier}`
- Accessibility: every focusable element has meaningful `AutomationProperties.Name`

---

## 3. Solution structure

### 3.1 Top-level layout

```
ComposerContextEngine/                          (repository root)
├── ComposerContextEngine.sln                   solution file
├── global.json                                 pins .NET 10 + Uno.Sdk
├── Directory.Packages.props                    central package version management
├── Directory.Build.props                       shared MSBuild properties
├── README.md                                   repo docs (not part of app output)
├── .gitignore
│
└── ComposerContextEngine/                      single project
    ├── ComposerContextEngine.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── GlobalUsings.cs
    │
    ├── Models/                                 pure data records, no behavior
    │   ├── LayerKind.cs
    │   ├── LayerDef.cs
    │   ├── LayerState.cs
    │   ├── LayerSnapshot.cs
    │   ├── StackPreferences.cs
    │   ├── IntentValues.cs
    │   ├── DesignTokens.cs
    │   ├── UXFlow.cs
    │   ├── ScreenDef.cs
    │   ├── ArchitectureBlueprint.cs
    │   ├── ModuleDef.cs
    │   ├── EdgeDef.cs
    │   ├── InteractionsMatrix.cs
    │   ├── InteractionFlow.cs
    │   ├── StateDef.cs
    │   ├── TransitionDef.cs
    │   ├── DataContracts.cs
    │   ├── EntityDef.cs
    │   ├── FieldDef.cs
    │   ├── BuildPlan.cs
    │   ├── PhaseDef.cs
    │   ├── DerivedContext.cs
    │   ├── LayerBrief.cs
    │   ├── SectionSpec.cs
    │   ├── CodeBlockSpec.cs
    │   ├── AnnotationSpec.cs
    │   ├── CrossReference.cs
    │   ├── FileRowData.cs
    │   ├── FileStatus.cs
    │   ├── LockedCardData.cs
    │   ├── FuturePreviewCardData.cs
    │   └── ComposerStatus.cs
    │
    ├── Presentation/                           MVUX models (records)
    │   ├── ShellModel.cs                       cross-cutting orchestrator
    │   ├── StackPreferencesModel.cs            layer 0
    │   ├── IntentModel.cs                      layer 1
    │   ├── UXModel.cs                          layer 2
    │   ├── ArchitectureModel.cs                layer 3
    │   ├── DesignModel.cs                      layer 4
    │   ├── InteractionsModel.cs                layer 5
    │   ├── DataModel.cs                        layer 6
    │   ├── ImplementationModel.cs              layer 7
    │   ├── ScaffoldModel.cs                    layer 8
    │   ├── ComposerModel.cs                    composer state
    │   ├── FilesRailModel.cs                   right-rail state
    │   └── CompositionStackModel.cs            left-rail state
    │
    ├── Services/                               cross-cutting services
    │   ├── ILayerPreviewService.cs
    │   ├── IdentityLayerPreviewService.cs      no-AI fallback
    │   ├── ClaudeLayerPreviewService.cs        AI augmentation
    │   ├── IContextDeriver.cs
    │   ├── ContextDeriver.cs
    │   ├── ILayerBriefGenerator.cs
    │   ├── LayerBriefGenerator.cs
    │   ├── IMarkdownRenderer.cs
    │   ├── MarkdownRenderer.cs
    │   ├── IMarkdownGenerator.cs               compatibility wrapper
    │   ├── StructuredMarkdownGenerator.cs
    │   ├── IBundleBuilder.cs
    │   ├── BundleBuilder.cs
    │   ├── IClipboardService.cs
    │   ├── ClipboardService.cs
    │   └── IFileDownloadService.cs
    │
    ├── Views/                                  XAML user controls and pages
    │   ├── Shell.xaml
    │   ├── Shell.xaml.cs
    │   │
    │   ├── Pages/                              one page per layer
    │   │   ├── StackPreferencesPage.xaml(.cs)
    │   │   ├── IntentPage.xaml(.cs)
    │   │   ├── UXPage.xaml(.cs)
    │   │   ├── ArchitecturePage.xaml(.cs)
    │   │   ├── DesignPage.xaml(.cs)
    │   │   ├── InteractionsPage.xaml(.cs)
    │   │   ├── DataPage.xaml(.cs)
    │   │   ├── ImplementationPage.xaml(.cs)
    │   │   └── ScaffoldPage.xaml(.cs)
    │   │
    │   ├── Canvases/                           one canvas per layer
    │   │   ├── StackPreferencesCanvas.xaml(.cs)
    │   │   ├── IntentCanvas.xaml(.cs)
    │   │   ├── UXFlowStripCanvas.xaml(.cs)
    │   │   ├── ArchitectureBlueprintCanvas.xaml(.cs)
    │   │   ├── DesignTokenGridCanvas.xaml(.cs)
    │   │   ├── StateTransitionDiagramCanvas.xaml(.cs)
    │   │   ├── DataContractGridCanvas.xaml(.cs)
    │   │   ├── ImplementationPhaseGridCanvas.xaml(.cs)
    │   │   └── ScaffoldTerminalCanvas.xaml(.cs)
    │   │
    │   └── Controls/                           reusable user controls
    │       ├── CompositionStackRegion.xaml(.cs)
    │       ├── FilesRailRegion.xaml(.cs)
    │       ├── ComposerFooter.xaml(.cs)
    │       ├── ActiveLayerHeader.xaml(.cs)
    │       ├── ProgressIndicator.xaml(.cs)
    │       ├── AppTitleRow.xaml(.cs)
    │       ├── StackItem.xaml(.cs)
    │       ├── LockedContextCard.xaml(.cs)
    │       ├── FuturePreviewCard.xaml(.cs)
    │       ├── FileRow.xaml(.cs)
    │       ├── LiveFilePanel.xaml(.cs)
    │       ├── MarkdownPreview.xaml(.cs)
    │       ├── Eyebrow.xaml(.cs)
    │       ├── MonoText.xaml(.cs)
    │       ├── SectionHeader.xaml(.cs)
    │       ├── Annotation.xaml(.cs)
    │       ├── CodeBlock.xaml(.cs)
    │       └── BlockHandle.xaml(.cs)
    │
    ├── Navigation/
    │   ├── RouteMap.cs                         all 9 routes registered
    │   └── NavigationExtensions.cs             optional helper methods
    │
    ├── Themes/                                 resource dictionaries
    │   ├── ColorPaletteOverride.xaml           live-synthesized from DesignTokens
    │   ├── ThemeColorOverrides.xaml            static semantic colors
    │   ├── Brushes.xaml                        named brush keys
    │   ├── Typography.xaml                     font family resources
    │   ├── TextBlock.xaml                      TextBlock variant styles
    │   ├── Buttons.xaml                        PrimaryButton/GhostButton/ChipButton
    │   ├── Editorial.xaml                      Eyebrow/Mono/Body styles
    │   └── Animations.xaml                     reusable Storyboard templates
    │
    ├── Strings/                                localization resources
    │   ├── en/Resources.resw
    │   └── fr/Resources.resw                   (optional second locale)
    │
    ├── Assets/                                 minimal — fonts only
    │   └── Fonts/
    │       ├── Inter-Regular.ttf
    │       ├── Inter-Medium.ttf
    │       ├── Inter-SemiBold.ttf
    │       ├── JetBrainsMono-Regular.ttf
    │       ├── JetBrainsMono-Medium.ttf
    │       └── JetBrainsMono-SemiBold.ttf
    │
    └── Platforms/                              platform-specific code
        ├── Android/
        │   └── MainActivity.cs
        ├── iOS/
        │   └── AppDelegate.cs
        ├── MacCatalyst/
        │   └── AppDelegate.cs
        ├── Desktop/
        │   └── Program.cs
        ├── Wasm/
        │   └── (entry point handled by Uno.Sdk)
        └── Windows/
            ├── Package.appxmanifest
            └── (MSBuild-injected entry)
```

### 3.2 csproj configuration

```xml
<!-- ComposerContextEngine.csproj -->
<Project Sdk="Uno.Sdk">

  <PropertyGroup>
    <TargetFrameworks>
      net10.0-android;
      net10.0-ios;
      net10.0-maccatalyst;
      net10.0-windows10.0.19041;
      net10.0-desktop;
      net10.0-browserwasm
    </TargetFrameworks>
    <SingleProject>true</SingleProject>
    <OutputType>Exe</OutputType>
    <UnoSingleProject>true</UnoSingleProject>
    <RootNamespace>ComposerContextEngine</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>

    <!-- Single application identity values -->
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>
    <ApplicationTitle>Composer Context Engine</ApplicationTitle>
    <ApplicationId>com.unoplatform.composercontextengine</ApplicationId>
    <ApplicationIdGuid>00000000-0000-0000-0000-000000000000</ApplicationIdGuid>
  </PropertyGroup>

  <!-- Feature opt-ins. Each maps to a set of Uno NuGet packages + initialization. -->
  <ItemGroup>
    <UnoFeature Include="Material" />
    <UnoFeature Include="Toolkit" />
    <UnoFeature Include="MVUX" />
    <UnoFeature Include="Navigation" />
    <UnoFeature Include="Hosting" />
    <UnoFeature Include="Configuration" />
    <UnoFeature Include="Http" />
    <UnoFeature Include="HttpKiota" />
    <UnoFeature Include="Logging" />
    <UnoFeature Include="Serialization" />
    <UnoFeature Include="ThemeService" />
    <UnoFeature Include="Skia" />
    <UnoFeature Include="SkiaRenderer" />
    <UnoFeature Include="Localization" />
  </ItemGroup>

  <!-- Font assets are embedded so they're available on every platform -->
  <ItemGroup>
    <Content Include="Assets\Fonts\**\*.ttf" />
  </ItemGroup>

</Project>
```

### 3.3 Directory.Packages.props

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <!-- Uno SDK manages most package versions through UnoFeatures.
       Pin third-party packages here when needed. -->
  <ItemGroup>
    <!-- (Reserved for future AI client / serialization packages) -->
  </ItemGroup>
</Project>
```

### 3.4 global.json

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "feature"
  },
  "msbuild-sdks": {
    "Uno.Sdk": "6.5.29"
  }
}
```

---

## 4. Hosting bootstrap

The application uses Uno Hosting to configure DI, logging, configuration, and navigation. All registrations happen in a single `App.OnLaunched` method.

### 4.1 App.xaml

```xml
<Application x:Class="ComposerContextEngine.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mat="using:Uno.Material"
             RequestedTheme="Light">

    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material baseline -->
                <mat:MaterialTheme />

                <!-- Engine-specific overrides, loaded in order -->
                <ResourceDictionary Source="Themes/ColorPaletteOverride.xaml" />
                <ResourceDictionary Source="Themes/ThemeColorOverrides.xaml" />
                <ResourceDictionary Source="Themes/Brushes.xaml" />
                <ResourceDictionary Source="Themes/Typography.xaml" />
                <ResourceDictionary Source="Themes/TextBlock.xaml" />
                <ResourceDictionary Source="Themes/Buttons.xaml" />
                <ResourceDictionary Source="Themes/Editorial.xaml" />
                <ResourceDictionary Source="Themes/Animations.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 4.2 App.xaml.cs

```csharp
namespace ComposerContextEngine;

public partial class App : Application
{
    public Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
                .UseLogging(
                    configure: (context, logBuilder) =>
                    {
                        logBuilder
                            .SetMinimumLevel(context.HostingEnvironment.IsDevelopment()
                                ? LogLevel.Information
                                : LogLevel.Warning)
                            .CoreLogLevel(LogLevel.Warning);
                    },
                    enableUnoLogging: true)
                .UseConfiguration(configBuilder => configBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>())
                .UseLocalization()
                .UseSerialization()
                .UseHttp((context, services) =>
                {
                    // Only used by ClaudeLayerPreviewService when API key configured
                    services.AddSingleton<HttpClient>();
                })
                .UseThemeSwitching()
                .ConfigureServices((context, services) =>
                {
                    RegisterServices(services, context);
                    RegisterModels(services);
                })
                .UseNavigation(RegisterRoutes));

        MainWindow = builder.Window;
        Host = builder.Build();

        MainWindow.Content = new Shell();
        MainWindow.Activate();
    }

    private static void RegisterServices(IServiceCollection services, HostBuilderContext context)
    {
        // Singletons — no per-call state
        services.AddSingleton<IContextDeriver, ContextDeriver>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRenderer>();
        services.AddSingleton<ILayerBriefGenerator, LayerBriefGenerator>();
        services.AddSingleton<IMarkdownGenerator, StructuredMarkdownGenerator>();
        services.AddSingleton<IBundleBuilder, BundleBuilder>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFileDownloadService, FileDownloadService>();

        // Preview service — identity fallback if no API key
        services.AddSingleton<ILayerPreviewService>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var apiKey = cfg["AppConfig:AnthropicApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return new IdentityLayerPreviewService();

            return new ClaudeLayerPreviewService(
                sp.GetRequiredService<HttpClient>(),
                apiKey,
                sp.GetRequiredService<IContextDeriver>(),
                sp.GetRequiredService<ILogger<ClaudeLayerPreviewService>>());
        });
    }

    private static void RegisterModels(IServiceCollection services)
    {
        // Shell is effectively a singleton within a session — register Transient,
        // but the Shell.xaml holds the single instance for lifetime of the window.
        services.AddTransient<ShellModel>();
        services.AddTransient<CompositionStackModel>();
        services.AddTransient<FilesRailModel>();
        services.AddTransient<ComposerModel>();

        // Layer models — Transient. Resolved fresh when their page navigates in.
        services.AddTransient<StackPreferencesModel>();
        services.AddTransient<IntentModel>();
        services.AddTransient<UXModel>();
        services.AddTransient<ArchitectureModel>();
        services.AddTransient<DesignModel>();
        services.AddTransient<InteractionsModel>();
        services.AddTransient<DataModel>();
        services.AddTransient<ImplementationModel>();
        services.AddTransient<ScaffoldModel>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
        => RouteMap.Register(views, routes);
}

public record AppConfig
{
    public string? AnthropicApiKey { get; init; }
    public bool EnableTelemetry { get; init; } = false;
}
```

### 4.3 Service lifetime rationale

| Service                    | Lifetime  | Reason                                                                             |
|----------------------------|-----------|------------------------------------------------------------------------------------|
| `IContextDeriver`          | Singleton | Pure function over Intent values, no state                                         |
| `IMarkdownRenderer`        | Singleton | Pure function over LayerBrief                                                      |
| `ILayerBriefGenerator`     | Singleton | Stateless; produces LayerBrief objects deterministically                           |
| `IMarkdownGenerator`       | Singleton | Compatibility wrapper, no state                                                    |
| `IBundleBuilder`           | Singleton | Stateless concatenation logic                                                      |
| `IClipboardService`        | Singleton | Wraps platform clipboard APIs; no per-request state                                |
| `IFileDownloadService`     | Singleton | Wraps platform download (Blob/SavePicker); no state                                |
| `ILayerPreviewService`     | Singleton | Identity impl has no state; Claude impl pools HTTP connections                     |
| `ShellModel`               | Transient | One per window. Practically single-instance via Shell's DataContext                |
| Layer models               | Transient | One per page navigation. State preserved by Shell while the page is active         |

### 4.4 Configuration sources

`AppConfig` lives in an embedded `appsettings.json`:

```json
{
  "AppConfig": {
    "AnthropicApiKey": null,
    "EnableTelemetry": false
  }
}
```

When `AnthropicApiKey` is null or empty, `ILayerPreviewService` resolves to `IdentityLayerPreviewService` (no AI augmentation; preview state returns identity copy of dirty values). When the key is set, `ClaudeLayerPreviewService` is used.

For local development, the key may be provided via:
- `appsettings.Development.json` (gitignored)
- Environment variable `ANTHROPIC_API_KEY`
- `dotnet user-secrets` (recommended for development)

### 4.5 Hosting environment detection

The `HostBuilderContext.HostingEnvironment` exposes `IsDevelopment()` / `IsProduction()`. Development logging is verbose (`Information` level); production is `Warning` and above.

---

## 5. The layer model

The composition consists of exactly nine layers in fixed order. The order is canonical and must not be modified.

### 5.1 LayerKind enum

```csharp
namespace ComposerContextEngine.Models;

public enum LayerKind
{
    Stack          = 0,
    Intent         = 1,
    UX             = 2,
    Architecture   = 3,
    DesignSystem   = 4,
    Interactions   = 5,
    Data           = 6,
    Implementation = 7,
    Scaffold       = 8,
}
```

The underlying integer value matches the layer's position in the composition stack. This is load-bearing — many computations rely on `(int)LayerKind` ordering (e.g., "every layer with index < activeIndex is candidate for locked context cards").

### 5.2 LayerDef record

```csharp
public record LayerDef(
    LayerKind Kind,
    string Label,         // "Stack", "Intent", "UX", etc.
    string OutputFile,    // "stack-preferences.md", "README.md", etc.
    string Hint,          // "what we're building on" — used in CompositionStack future rows
    string RouteName);    // navigation route name (matches RouteMap registration)
```

### 5.3 Layers.All canonical array

```csharp
public static class Layers
{
    public static readonly ImmutableArray<LayerDef> All = ImmutableArray.Create(
        new LayerDef(LayerKind.Stack,          "Stack",          "stack-preferences.md",   "what we're building on",       "Stack"),
        new LayerDef(LayerKind.Intent,         "Intent",         "README.md",              "what the app is for",          "Intent"),
        new LayerDef(LayerKind.UX,             "UX",             "ux-flows.md",            "how users move through it",    "UX"),
        new LayerDef(LayerKind.Architecture,   "Architecture",   "architecture.md",        "how it is shaped",             "Architecture"),
        new LayerDef(LayerKind.DesignSystem,   "Design System",  "design-system.md",       "how it feels",                 "Design"),
        new LayerDef(LayerKind.Interactions,   "Interactions",   "interaction-spec.md",    "every state of every flow",    "Interactions"),
        new LayerDef(LayerKind.Data,           "Data",           "data-contracts.md",      "shapes and contracts",         "Data"),
        new LayerDef(LayerKind.Implementation, "Implementation", "implementation-plan.md", "phased build plan",            "Implementation"),
        new LayerDef(LayerKind.Scaffold,       "Scaffold",       "scaffold.command",       "runnable starting point",      "Scaffold"));

    public static LayerDef Get(LayerKind kind) => All[(int)kind];
    public static LayerDef Get(int index) => All[index];
    public static int Count => All.Length;
}
```

`Layers.All` is the single source of truth. Every other reference to layer metadata in the codebase reads from this array.

### 5.4 Synthesized output file

After the Scaffold layer locks, a tenth synthetic file is produced by `IBundleBuilder.BuildPromptContext`:

- Filename: `prompt-context.md`
- Content: all nine layer output files concatenated with `<!-- {filename} -->` separators
- Lifecycle: only exists after Scaffold locks; not editable by user

This file is not a layer; it's a derived artifact.

---

## 6. State machine

### 6.1 LayerState enum

```csharp
namespace ComposerContextEngine.Models;

public enum LayerState
{
    Clean,        // No edits since last locked or initial. Lock-and-continue is the action.
    Dirty,        // User edited canvas or typed in composer. Generate-preview is the action.
    Previewing,   // AI rendered proposed values. Accept-and-lock or discard is the action.
    Locked,       // Terminal. Layer settled, file drafted, activeIndex advanced past.
}
```

Each layer has its own state. The states are isolated — Intent can be Locked while UX is Dirty.

### 6.2 Transition matrix

| From         | To           | Trigger                                                                                                                                          |
|--------------|--------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| Clean        | Dirty        | First non-whitespace edit to any canvas value for this layer; OR first non-whitespace character typed in composer textarea for this layer        |
| Clean        | Locked       | User clicks `Lock and continue →` (or `Continue →` on the first layer); OR presses `Cmd/Ctrl+Enter` while composer prompt is empty               |
| Dirty        | Previewing   | User clicks `Generate preview →` (only when `aiConfigured == true`); OR presses `Cmd/Ctrl+Enter` while composer prompt is non-empty              |
| Dirty        | Clean        | User clicks `Discard edits` (restores from snapshot, clears prompt)                                                                              |
| Previewing   | Locked       | User clicks `Accept and lock →`                                                                                                                  |
| Previewing   | Clean        | User clicks `← Discard preview`; OR presses `Esc` while composer textarea has focus                                                              |
| Previewing   | Dirty        | (Same layer) user edits canvas or composer prompt while in Previewing state — the dirty edit invalidates the preview, layer falls back to Dirty  |
| Locked       | Clean        | User clicks `Revisit ↗` on this layer's locked context card; OR clicks this layer's row in CompositionStack                                       |
| Any          | Clean        | `ShellModel.Reset()` is invoked — resets every layer's state to Clean and clears all transient state                                              |

### 6.3 LayerSnapshot record

```csharp
namespace ComposerContextEngine.Models;

public record LayerSnapshot(
    StackPreferences? StackPrefs,
    IntentValues? Intent,
    DesignTokens? Design,
    UXFlow? UX,
    ArchitectureBlueprint? Architecture,
    InteractionsMatrix? Interactions,
    DataContracts? Data,
    BuildPlan? Implementation,
    ImmutableDictionary<LayerKind, string>? OverrideMarkdown,
    ImmutableDictionary<LayerKind, string>? Prompts);
```

A snapshot is captured on the first Clean → Dirty transition per layer (not on subsequent edits during the same Dirty session). Only the fields relevant to the layer being modified are populated; others are null.

On `Discard edits` (Dirty → Clean) and `Discard preview` (Previewing → Clean), the snapshot is restored — every populated field is written back to its owning model's IState. The snapshot is then removed from the ShellModel's snapshots dictionary.

### 6.4 Preview values store

```csharp
// In ShellModel:
private readonly Dictionary<LayerKind, object> _previewValues = new();
private readonly Dictionary<LayerKind, string?> _previewAcks = new();
```

When `GeneratePreview()` runs, the proposed values returned by `ILayerPreviewService.GeneratePreviewAsync` are stored in `_previewValues[layerKind]` (the value's runtime type matches the layer — `IntentValues` for Intent, `DesignTokens` for Design, etc.). The user's prompt is templated into an acknowledgment string and stored in `_previewAcks[layerKind]`.

When the canvas renders in Previewing state, it reads from `_previewValues` rather than the canonical state IState. The original IState is unchanged until `AcceptPreview()` adopts the proposed values.

### 6.5 State machine commands on ShellModel

```csharp
// In ShellModel:
public async ValueTask MarkDirty(LayerKind kind);              // Clean → Dirty (with snapshot)
public async ValueTask GeneratePreview();                      // Dirty → Previewing
public async ValueTask AcceptPreview();                        // Previewing → Locked
public async ValueTask DiscardPreview();                       // Previewing → Clean (restore)
public async ValueTask DiscardEdits();                         // Dirty → Clean (restore)
public async ValueTask LockAndContinue();                      // Clean → Locked + advance
public async ValueTask Revisit(LayerKind kind);                // Locked → Clean (preserve values)
public async ValueTask Reset();                                // Any → Clean (full wipe)
```

These methods are the only entry points for state changes. Pages and canvases do not modify `LayerStates` directly — they always go through these methods.

---

## 7. Shell architecture

### 7.1 Visual layout

The Shell is a three-column workspace with a single navigation region in the center:

```
┌── Shell ──────────────────────────────────────────────────────────────────┐
│                                                                             │
│   ┌─ LeftRail ────┬─ ActivePage region ────┬─ RightRail ────────┐          │
│   │   260px       │   flex, max 880px      │   340px            │          │
│   │   sticky      │                        │   sticky           │          │
│   │               │   {one of 9 pages}     │                    │          │
│   │  Composition  │                        │   Files Rail       │          │
│   │  Stack        │   - Progress           │   - Live file      │          │
│   │               │   - AppTitleRow        │   - File list      │          │
│   │  - StackItem  │   - LockedCards × N    │   - Lock count     │          │
│   │  - StackItem  │   - ActiveHeader       │                    │          │
│   │  - ...        │   - Canvas             │                    │          │
│   │               │   - ComposerFooter     │                    │          │
│   │               │   - FutureCards × M    │                    │          │
│   │               │                        │                    │          │
│   └───────────────┴────────────────────────┴────────────────────┘          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Three navigation regions

Only the center column is a navigation region. The rails are user controls that bind directly to MVUX models.

```xml
<!-- Views/Shell.xaml -->
<UserControl x:Class="ComposerContextEngine.Views.Shell"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:uen="using:Uno.Extensions.Navigation.UI"
             xmlns:controls="using:ComposerContextEngine.Views.Controls"
             Background="{ThemeResource BackgroundBrush}">

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="{Binding LeftRailWidth, Mode=OneWay}" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="{Binding RightRailWidth, Mode=OneWay}" />
        </Grid.ColumnDefinitions>

        <!-- LEFT: Composition Stack — sticky, no navigation -->
        <controls:CompositionStackRegion
            x:Name="LeftRail"
            Grid.Column="0"
            Opacity="{Binding LeftRailOpacity, Mode=OneWay}"
            Visibility="{Binding LeftRailVisibility, Mode=OneWay}"
            DataContext="{Binding CompositionStack}" />

        <!-- CENTER: navigation region hosting one page at a time -->
        <Grid Grid.Column="1"
              uen:Region.Attached="True"
              uen:Region.Name="ActivePage"
              MaxWidth="{Binding CenterMaxWidth, Mode=OneWay}"
              HorizontalAlignment="Center" />

        <!-- RIGHT: Files Rail — sticky, no navigation -->
        <controls:FilesRailRegion
            x:Name="RightRail"
            Grid.Column="2"
            Opacity="{Binding RightRailOpacity, Mode=OneWay}"
            Visibility="{Binding RightRailVisibility, Mode=OneWay}"
            DataContext="{Binding FilesRail}" />
    </Grid>
</UserControl>
```

### 7.3 Shell.xaml.cs

```csharp
namespace ComposerContextEngine.Views;

public sealed partial class Shell : UserControl
{
    public Shell()
    {
        InitializeComponent();
        DataContext = (App.Current as App)!.Host!.Services.GetRequiredService<ShellModel>();
    }
}
```

The Shell resolves its `ShellModel` from DI exactly once. The model is held by the DataContext for the lifetime of the window. Navigation between pages does not recreate the Shell or its model.

### 7.4 ShellModel responsibilities

ShellModel owns cross-cutting state and is the only model whose IState members persist across the entire session:

- `ActiveIndex` — current layer index (0–8)
- `LockedIds` — immutable set of locked layer kinds
- `LayerStates` — per-layer state machine values
- Snapshots dictionary (private; not IState — internal restore-state cache)
- `_previewValues` and `_previewAcks` dictionaries (private)
- One-shot UI hints (e.g., revisit hint after first lock)
- AI configuration feed
- Derived: `RailsVisible`, `LeftRailWidth`, `RightRailWidth`, `CenterMaxWidth`, `LeftRailOpacity`, etc.
- Derived: `LockedCards` list (data for the Slot 3 ItemsRepeater on every page)
- Derived: `FutureCards` list (data for the Slot 7 ItemsRepeater)
- Derived: child model references (`CompositionStack`, `FilesRail`, `Composer`, plus all 9 layer models)
- Commands: `MarkDirty`, `GeneratePreview`, `AcceptPreview`, `DiscardPreview`, `DiscardEdits`, `LockAndContinue`, `Revisit`, `Reset`

### 7.5 Rail visibility computation

```csharp
// In ShellModel:
public IFeed<bool> RailsVisible =>
    Feed.Combine(ActiveIndex, LockedIds).Select(t => t.Item1 > 0 || t.Item2.Count > 0);

public IFeed<GridLength> LeftRailWidth =>
    RailsVisible.Select(v => v ? new GridLength(260) : new GridLength(0));

public IFeed<GridLength> RightRailWidth =>
    RailsVisible.Select(v => v ? new GridLength(340) : new GridLength(0));

public IFeed<double> LeftRailOpacity =>
    RailsVisible.Select(v => v ? 1.0 : 0.0);

public IFeed<double> RightRailOpacity =>
    RailsVisible.Select(v => v ? 1.0 : 0.0);

public IFeed<Visibility> LeftRailVisibility =>
    RailsVisible.Select(v => v ? Visibility.Visible : Visibility.Collapsed);

public IFeed<Visibility> RightRailVisibility =>
    RailsVisible.Select(v => v ? Visibility.Visible : Visibility.Collapsed);

public IFeed<double> CenterMaxWidth =>
    RailsVisible.Select(v => v ? 880.0 : 720.0);
```

When the user has not yet locked any layer (`ActiveIndex == 0 && LockedIds.Count == 0`), `RailsVisible` evaluates false. Rails collapse to zero width. Center column tightens to 720px max-width. This is the **focused first-screen** state.

After the user locks the Stack layer (the first lock possible), `LockedIds.Count > 0` becomes true. `RailsVisible` flips. Animation Storyboard plays (see Section 17.3 of the Design Brief for the animation specs).

---

## 8. Navigation graph

### 8.1 Route registration

```csharp
namespace ComposerContextEngine.Navigation;

public static class RouteMap
{
    public static void Register(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<Pages.StackPreferencesPage, StackPreferencesModel>(),
            new ViewMap<Pages.IntentPage,           IntentModel>(),
            new ViewMap<Pages.UXPage,               UXModel>(),
            new ViewMap<Pages.ArchitecturePage,     ArchitectureModel>(),
            new ViewMap<Pages.DesignPage,           DesignModel>(),
            new ViewMap<Pages.InteractionsPage,     InteractionsModel>(),
            new ViewMap<Pages.DataPage,             DataModel>(),
            new ViewMap<Pages.ImplementationPage,   ImplementationModel>(),
            new ViewMap<Pages.ScaffoldPage,         ScaffoldModel>());

        routes.Register(new RouteMap("",
            View: views.FindByViewModel<ShellModel>(),
            Nested: new[]
            {
                new RouteMap("Stack",          View: views.FindByViewModel<StackPreferencesModel>(), IsDefault: true),
                new RouteMap("Intent",         View: views.FindByViewModel<IntentModel>()),
                new RouteMap("UX",             View: views.FindByViewModel<UXModel>()),
                new RouteMap("Architecture",   View: views.FindByViewModel<ArchitectureModel>()),
                new RouteMap("Design",         View: views.FindByViewModel<DesignModel>()),
                new RouteMap("Interactions",   View: views.FindByViewModel<InteractionsModel>()),
                new RouteMap("Data",           View: views.FindByViewModel<DataModel>()),
                new RouteMap("Implementation", View: views.FindByViewModel<ImplementationModel>()),
                new RouteMap("Scaffold",       View: views.FindByViewModel<ScaffoldModel>()),
            }));
    }
}
```

The 9 page routes are nested under the single Shell route. The Stack route is marked `IsDefault: true`, so initial navigation lands there automatically.

### 8.2 Initial navigation

The Shell does not explicitly navigate on load. Because the `Stack` route is `IsDefault: true` under the Shell route, Uno's Region Navigation system places `StackPreferencesPage` into the `ActivePage` region when the Shell first renders.

If explicit initial navigation is needed (for testing, deep linking, or skipping the default), `ShellModel` can issue:

```csharp
await _navigator.NavigateRouteAsync(this, "Stack", qualifier: Qualifiers.Nested);
```

### 8.3 Navigation triggers (exhaustive)

| Trigger                                                    | Method on ShellModel              | ActiveIndex change | Side effects                                                                            |
|------------------------------------------------------------|-----------------------------------|---------------------|------------------------------------------------------------------------------------------|
| Click `Continue →` (first layer, before any lock)          | `LockAndContinue()`               | 0 → 1               | LayerStates[Stack] → Locked, LockedIds.Add(Stack), RailsVisible flips false → true      |
| Click `Lock and continue →`                                | `LockAndContinue()`               | N → N+1             | LayerStates[N] → Locked, LockedIds.Add(N), navigate to Layers.Get(N+1).RouteName        |
| Click `Accept and lock →` (in Previewing state)            | `AcceptPreview()`                 | N → N+1             | _previewValues[N] adopted into canonical IState, then same as LockAndContinue          |
| `Cmd/Ctrl+Enter` with empty composer prompt (Clean state)  | `LockAndContinue()`               | N → N+1             | Same as Lock and continue                                                                |
| `Cmd/Ctrl+Enter` with non-empty composer prompt (Dirty)    | `GeneratePreview()`               | no change           | LayerStates[N] → Previewing, _previewAcks[N] templated from prompt                      |
| Click `Revisit ↗` on a locked card                         | `Revisit(LayerKind)`              | N → M (M < N)       | LayerStates[M] → Clean (values preserved), downstream layers unchanged                  |
| Click a locked row in CompositionStack                     | `Revisit(LayerKind)`              | N → M               | Same as Revisit ↗                                                                        |
| Click the active row in CompositionStack                   | no-op                             | —                   | —                                                                                        |
| Click a future row in CompositionStack                     | no-op (not hit-testable)          | —                   | —                                                                                        |
| Click `Reset` in AppTitleRow → confirm                     | `Reset()`                         | N → 0               | All transient state wiped, navigate to Stack route, RailsVisible flips true → false      |
| Click `Reset` → cancel                                     | no-op                             | —                   | —                                                                                        |
| Click `Download bundle ↓` on Scaffold                      | `ScaffoldModel.DownloadBundle()`  | 8 (no change)       | Bundle saved as `{AppName}-bundle.md`, LayerStates[Scaffold] → Locked                   |
| Click `Copy prompt-context.md` on Scaffold                 | `ScaffoldModel.CopyPromptContext()` | no change         | Clipboard updated, no navigation                                                         |

**There is no Back button.** Backward navigation happens only via `Revisit` (a targeted jump). The browser/OS back button is intercepted at the Shell level and routes to `Revisit` if a previous layer exists, otherwise does nothing.

### 8.4 LockAndContinue and AcceptPreview advance logic

```csharp
// In ShellModel:
public async ValueTask LockAndContinue()
{
    var idx = await ActiveIndex;
    var kind = Layers.Get(idx).Kind;
    var states = await LayerStates;
    var locked = await LockedIds;

    await LayerStates.SetAsync(states.SetItem(kind, LayerState.Locked));
    await LockedIds.SetAsync(locked.Add(kind));

    // Clear composer prompt for this layer (the lock takes the action; prompt is no longer pending)
    var prompts = await Composer.Prompts;
    await Composer.Prompts.SetAsync(prompts.SetItem(kind, ""));

    // One-shot revisit hint after the very first lock
    if (locked.Count == 0)
        await RevisitHintShown.SetAsync(true);

    await AdvanceToNext(idx);
}

public async ValueTask AcceptPreview()
{
    var idx = await ActiveIndex;
    var kind = Layers.Get(idx).Kind;

    // Adopt the proposed values from _previewValues[kind] into the layer model's canonical IState
    if (_previewValues.TryGetValue(kind, out var proposed))
    {
        await AdoptProposedValues(kind, proposed);
        _previewValues.Remove(kind);
    }
    _previewAcks.Remove(kind);

    // Same path as LockAndContinue from here
    await LockAndContinue();
}

private async ValueTask AdvanceToNext(int currentIdx)
{
    if (currentIdx >= Layers.Count - 1) return;  // already at Scaffold
    await ActiveIndex.SetAsync(currentIdx + 1);
    await _navigator.NavigateRouteAsync(this,
        Layers.Get(currentIdx + 1).RouteName,
        qualifier: Qualifiers.Nested);
}

private async ValueTask AdoptProposedValues(LayerKind kind, object proposed)
{
    switch (kind, proposed)
    {
        case (LayerKind.Stack, StackPreferences sp):       await StackPreferences.Values.SetAsync(sp); break;
        case (LayerKind.Intent, IntentValues iv):          await Intent.Values.SetAsync(iv); break;
        case (LayerKind.DesignSystem, DesignTokens dt):    await Design.Tokens.SetAsync(dt); break;
        // ... other layers as direct manipulation is added
        default: break;
    }
}
```

### 8.5 Revisit logic

```csharp
public async ValueTask Revisit(LayerKind kind)
{
    var targetIdx = (int)kind;
    var states = await LayerStates;

    // Transition target layer Locked → Clean (values preserved)
    if (states[kind] == LayerState.Locked)
    {
        await LayerStates.SetAsync(states.SetItem(kind, LayerState.Clean));
    }

    await ActiveIndex.SetAsync(targetIdx);
    await _navigator.NavigateRouteAsync(this,
        Layers.Get(targetIdx).RouteName,
        qualifier: Qualifiers.Nested);
}
```

Downstream layers are not modified by Revisit. If the user was on layer 5 and revisits layer 2, layers 3 and 4 remain Locked. When the user re-advances through them (by locking layer 2 again, then locking layer 3, etc.), the system steps through each one individually — there is no batch "re-lock everything downstream."

### 8.6 Reset logic

```csharp
public async ValueTask Reset()
{
    // Clear every IState in every layer model
    await StackPreferences.Values.SetAsync(StackPreferencesModel.Defaults);
    await Intent.Values.SetAsync(IntentModel.Example);
    await Design.Tokens.SetAsync(DesignModel.Defaults);
    // ... other layer models

    // Clear ShellModel state
    await ActiveIndex.SetAsync(0);
    await LockedIds.SetAsync(ImmutableHashSet<LayerKind>.Empty);
    await LayerStates.SetAsync(BuildInitialLayerStates());
    await RevisitHintShown.SetAsync(false);

    // Clear Composer state
    await Composer.Prompts.SetAsync(BuildEmptyPromptsMap());
    await Composer.Overrides.SetAsync(ImmutableDictionary<LayerKind, string>.Empty);

    // Clear preview caches
    _previewValues.Clear();
    _previewAcks.Clear();
    _snapshots.Clear();

    // Navigate back to Stack
    await _navigator.NavigateRouteAsync(this, "Stack", qualifier: Qualifiers.Nested);
}

private static ImmutableDictionary<LayerKind, LayerState> BuildInitialLayerStates() =>
    Layers.All.ToImmutableDictionary(l => l.Kind, _ => LayerState.Clean);

private static ImmutableDictionary<LayerKind, string> BuildEmptyPromptsMap() =>
    Layers.All.ToImmutableDictionary(l => l.Kind, _ => "");
```

---

## 9. Layer models

Each layer has a dedicated MVUX model. Models are records with public IState/IFeed/IListFeed members and public async methods (which MVUX exposes as implicit `IAsyncCommand` for binding).

### 9.1 StackPreferencesModel

```csharp
namespace ComposerContextEngine.Presentation;

public partial record StackPreferencesModel(ShellModel Shell)
{
    public static readonly StackPreferences Defaults = new(
        Pattern:   StackPattern.Mvux,
        Markup:    MarkupKind.Xaml,
        Renderer:  RendererKind.Skia,
        Http:      HttpClientKind.Kiota,
        Nav:       NavigationKind.Region,
        Theme:     ThemeKind.Material,
        Platforms: ImmutableHashSet.Create(
            PlatformTarget.Wasm,
            PlatformTarget.iOS,
            PlatformTarget.Android,
            PlatformTarget.Windows));

    public IState<StackPreferences> Values => State.Value(this, () => Defaults);

    // Per-page display-binding feeds
    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(0));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Stack"));
    public IFeed<LayerState> LayerState =>
        Shell.LayerStates.Select(s => s[LayerKind.Stack]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(null));
    public IFeed<string> Title =>
        Feed.Async(_ => ValueTask.FromResult("What are we building on?"));
    public IFeed<string> Subtitle =>
        Feed.Async(_ => ValueTask.FromResult(
            "Pattern, markup, renderer, navigation, theme, platforms. Every downstream brief references this."));
    public IFeed<string> LeadQuestion =>
        Feed.Async(_ => ValueTask.FromResult(
            "These defaults match canonical Uno conventions. Anything you want to change before locking?"));
    public IFeed<ImmutableList<string>> Suggestions =>
        Feed.Async(_ => ValueTask.FromResult(
            ImmutableList.Create("MVVM instead", "Add macOS + Linux", "Custom theme")));
    public IState<string> PromptValue =>
        Shell.Composer.Prompts.Select(p => p[LayerKind.Stack]);
    public IFeed<string?> PreviewAck =>
        Shell.Composer.PreviewAcks.Select(p => p.TryGetValue(LayerKind.Stack, out var v) ? v : null);

    // Commands — public methods discovered as implicit IAsyncCommand
    public async ValueTask UpdatePattern(StackPattern value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Pattern = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask UpdateMarkup(MarkupKind value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Markup = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask UpdateRenderer(RendererKind value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Renderer = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask UpdateHttp(HttpClientKind value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Http = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask UpdateNav(NavigationKind value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Nav = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask UpdateTheme(ThemeKind value)
    {
        var current = await Values;
        await Values.SetAsync(current with { Theme = value });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask TogglePlatform(PlatformTarget value)
    {
        var current = await Values;
        var next = current.Platforms.Contains(value)
            ? current.Platforms.Remove(value)
            : current.Platforms.Add(value);
        await Values.SetAsync(current with { Platforms = next });
        await Shell.MarkDirty(LayerKind.Stack);
    }

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();
}
```

### 9.2 IntentModel

```csharp
public partial record IntentModel(ShellModel Shell)
{
    public static readonly IntentValues Example = new(
        AppType:     "Field-service scheduling",
        PrimaryUser: "Mobile-first technicians",
        Workflow:    "Receive jobs, schedule, dispatch",
        Platforms:   "Web, iOS, Android",
        Notes:       "");

    public IState<IntentValues> Values => State.Value(this, () => Example);

    public IFeed<bool> ShowingExample => Values.Select(v =>
        v.AppType == Example.AppType
        || v.PrimaryUser == Example.PrimaryUser
        || v.Workflow == Example.Workflow
        || v.Platforms == Example.Platforms);

    public IFeed<LayerState> LayerState =>
        Shell.LayerStates.Select(s => s[LayerKind.Intent]);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(1));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Intent"));
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "Stack chosen — now let's name what we're building on it."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("What are we building?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "Fill what you know. I'll infer the rest as we go."));
    public IFeed<string> LeadQuestion => Feed.Async(_ => ValueTask.FromResult(
        "If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?"));
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Mobile-first", "Offline-first", "No backend yet")));
    public IState<string> PromptValue =>
        Shell.Composer.Prompts.Select(p => p[LayerKind.Intent]);
    public IFeed<string?> PreviewAck =>
        Shell.Composer.PreviewAcks.Select(p => p.TryGetValue(LayerKind.Intent, out var v) ? v : null);

    public async ValueTask UpdateAppType(string value) => await UpdateField(v => v with { AppType = value });
    public async ValueTask UpdatePrimaryUser(string value) => await UpdateField(v => v with { PrimaryUser = value });
    public async ValueTask UpdateWorkflow(string value) => await UpdateField(v => v with { Workflow = value });
    public async ValueTask UpdatePlatforms(string value) => await UpdateField(v => v with { Platforms = value });
    public async ValueTask UpdateNotes(string value) => await UpdateField(v => v with { Notes = value });

    public async ValueTask ClearAll()
    {
        await Values.SetAsync(new IntentValues("", "", "", "", ""));
        await Shell.MarkDirty(LayerKind.Intent);
    }

    private async ValueTask UpdateField(Func<IntentValues, IntentValues> mutate)
    {
        var current = await Values;
        await Values.SetAsync(mutate(current));
        await Shell.MarkDirty(LayerKind.Intent);
    }

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();
}
```

### 9.3 UXModel

```csharp
public partial record UXModel(
    ShellModel Shell,
    IntentModel Intent,
    IContextDeriver ContextDeriver)
{
    public IFeed<UXFlow> Flow => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildFlow(ctx);
    });

    public IFeed<bool> IsDemoContent => Intent.Values.Select(intent =>
        ContextDeriver.Derive(intent).IsFieldService);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(2));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("UX"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.UX]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "We've named what we're building — now let's trace how someone uses it."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("How do users move through it?"));
    public IFeed<string> Subtitle => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return $"Five screens for the primary {ctx.EntityNoun} flow. Architecture in §03 will pick the navigation primitive.";
    });
    public IFeed<string> LeadQuestion => Feed.Async(_ => ValueTask.FromResult(
        "Drag-to-reorder schedule, or list-with-time-pickers? Stay on screen after dispatch, or return to dashboard?"));
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Drag-to-reorder", "Return to dashboard", "Modal confirmation")));
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.UX]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.UX, out var v) ? v : null);

    private static UXFlow BuildFlow(DerivedContext ctx)
    {
        var isJob = ctx.EntityNoun == "job";
        var screens = isJob
            ? new[]
            {
                new ScreenDef("Dashboard", "Today's jobs", ImmutableList.Create(100, 70, 85)),
                new ScreenDef("Job detail", "Status, location, parts", ImmutableList.Create(70, 100, 70)),
                new ScreenDef("Schedule", "Drag-to-reorder", ImmutableList.Create(100, 70, 100)),
                new ScreenDef("Dispatch", "Confirm + assign", ImmutableList.Create(40, 70, 100)),
                new ScreenDef("Confirmation", "Synced or queued", ImmutableList.Create(70, 100, 40)),
            }
            : new[]
            {
                new ScreenDef("Dashboard", $"Today's {ctx.EntityPlural}", ImmutableList.Create(100, 70, 85)),
                new ScreenDef($"{ctx.EntityTitle} detail", "Status, history, notes", ImmutableList.Create(70, 100, 70)),
                new ScreenDef("Schedule", "Drag-to-reorder", ImmutableList.Create(100, 70, 100)),
                new ScreenDef("Action", $"Confirm {ctx.EntityNoun}", ImmutableList.Create(40, 70, 100)),
                new ScreenDef("Confirmation", "Synced or queued", ImmutableList.Create(70, 100, 40)),
            };
        return new UXFlow("Dispatch", screens.ToImmutableList());
    }

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();
}
```

### 9.4 ArchitectureModel

```csharp
public partial record ArchitectureModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs,
    IContextDeriver ContextDeriver)
{
    public IFeed<ArchitectureBlueprint> Blueprint =>
        Feed.Combine(Intent.Values, StackPrefs.Values).Select(t =>
        {
            var (intent, prefs) = t;
            var ctx = ContextDeriver.Derive(intent);
            return BuildBaseline(ctx, prefs);
        });

    public IState<string?> HoveredModuleId => State.Value(this, () => (string?)null);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(3));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Architecture"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.Architecture]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "With the user's path mapped — let's figure out the shape underneath."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("How is this app shaped?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "A blueprint of how modules connect, and the solution tree they imply."));
    public IFeed<string> LeadQuestion => Feed.Combine(Intent.Values).Select(t =>
    {
        var ctx = ContextDeriver.Derive(t);
        return $"Two open questions before locking — does {ctx.EntityNoun} logic live in a service, or stay inside State (MVUX)? And do {ctx.UserNoun} authenticate, or is access role-less?";
    });
    public IFeed<ImmutableList<string>> Suggestions => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        var roleNoun = ctx.UserNoun.TrimEnd('s');
        return ImmutableList.Create(
            "Region-based navigation",
            $"Single {roleNoun} role",
            "Offline-first");
    });
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.Architecture]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.Architecture, out var v) ? v : null);

    public async ValueTask RegenerateBlueprint() => await Shell.GeneratePreview();
    public async ValueTask SetHoveredModule(string? moduleId) => await HoveredModuleId.SetAsync(moduleId);
    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();

    private static ArchitectureBlueprint BuildBaseline(DerivedContext ctx, StackPreferences prefs)
    {
        var modules = BaseModules.Select(m =>
        {
            if (m.Id == "services" && !ctx.IsFieldService)
                return m with { Description = $"{ctx.EntityTitle}, {Capitalize(ctx.UserNoun)}, Schedule" };
            if (m.Id == "mvux" && prefs.Pattern != StackPattern.Mvux && prefs.Pattern != StackPattern.MvuxMessaging)
                return m with { Label = "ViewModels", Description = "INPC-backed properties + ICommand" };
            return m;
        }).ToList();

        var edges = BaseEdges.ToList();

        // Remove HTTP module + its edge when offline-first
        if (ctx.IsOfflineFirst || prefs.Http == HttpClientKind.None)
        {
            modules.RemoveAll(m => m.Id == "http");
            edges.RemoveAll(e => e.ToId == "http");
        }

        return new ArchitectureBlueprint(modules.ToImmutableList(), edges.ToImmutableList());
    }

    private static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    private static readonly ImmutableList<ModuleDef> BaseModules = ImmutableList.Create(
        new ModuleDef("pages",    "Pages",        new(110, 90),  "InkBrush",    "Route surfaces — Calendar, Jobs, Technicians", 4),
        new ModuleDef("nav",      "Navigation",   new(110, 230), "InkBrush",    "Region-based routes",                          2),
        new ModuleDef("mvux",     "State (MVUX)", new(380, 90),  "IndigoBrush", "Feeds, States, Selection",                     6),
        new ModuleDef("services", "Services",     new(380, 230), "Ink2Brush",   "Job, Technician, Schedule",                    5),
        new ModuleDef("http",     "HTTP (Kiota)", new(650, 90),  "Ink2Brush",   "Typed clients, generated",                     3),
        new ModuleDef("storage",  "Storage",      new(650, 230), "Ink2Brush",   "Local cache, offline-first",                   2));

    private static readonly ImmutableList<EdgeDef> BaseEdges = ImmutableList.Create(
        new EdgeDef("pages",    "mvux",     "binds"),
        new EdgeDef("pages",    "nav",      "requests"),
        new EdgeDef("mvux",     "services", "consumes"),
        new EdgeDef("services", "http",     "calls"),
        new EdgeDef("services", "storage",  "persists"));
}
```

### 9.5 DesignModel

```csharp
public partial record DesignModel(ShellModel Shell)
{
    public static readonly DesignTokens Defaults = new(
        Surface:  Color.FromArgb(0xFF, 0x0C, 0x0D, 0x0F),
        Action:   Color.FromArgb(0xFF, 0xC8, 0x9C, 0x3F),
        Info:     Color.FromArgb(0xFF, 0x7A, 0xB3, 0xDF),
        Success:  Color.FromArgb(0xFF, 0x9B, 0xBF, 0x9D),
        Warn:     Color.FromArgb(0xFF, 0xE8, 0x7F, 0x6D),
        Panel:    Color.FromArgb(0xFF, 0x16, 0x18, 0x1C),
        Tag:      Color.FromArgb(0xFF, 0xB2, 0x88, 0xC4),
        Locked:   Color.FromArgb(0xFF, 0x0C, 0x0D, 0x0F),
        BodyFont: "Inter");

    public IState<DesignTokens> Tokens => State.Value(this, () => Defaults);

    // Live-synthesized — updates immediately on any token edit (no preview required)
    public IFeed<string> ColorPaletteOverrideXaml => Tokens.Select(BuildXaml);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(4));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Design System"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.DesignSystem]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "Modules in place — let's give the surface a feel."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("How should it feel?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "Tokens, type scale on real sample copy, and the ColorPaletteOverride.xaml the agent will write."));
    public IFeed<string> LeadQuestion => Feed.Async(_ => ValueTask.FromResult(
        "The palette is low-chroma for outdoor visibility. Want a brand override on Action, or stay with amber?"));
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Stay with amber", "Use a brand color", "Show alternatives")));
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.DesignSystem]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.DesignSystem, out var v) ? v : null);

    public async ValueTask UpdateToken(DesignTokenName tokenName, object value)
    {
        var current = await Tokens;
        var updated = tokenName switch
        {
            DesignTokenName.Surface  => current with { Surface  = (Color)value },
            DesignTokenName.Action   => current with { Action   = (Color)value },
            DesignTokenName.Info     => current with { Info     = (Color)value },
            DesignTokenName.Success  => current with { Success  = (Color)value },
            DesignTokenName.Warn     => current with { Warn     = (Color)value },
            DesignTokenName.Panel    => current with { Panel    = (Color)value },
            DesignTokenName.Tag      => current with { Tag      = (Color)value },
            DesignTokenName.Locked   => current with { Locked   = (Color)value },
            DesignTokenName.BodyFont => current with { BodyFont = (string)value },
            _ => current,
        };
        await Tokens.SetAsync(updated);
        await Shell.MarkDirty(LayerKind.DesignSystem);
    }

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();

    private static string BuildXaml(DesignTokens t)
    {
        static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        return $$"""
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <ResourceDictionary.ThemeDictionaries>

                <ResourceDictionary x:Key="Light">
                  <Color x:Key="PrimaryColor">#1A1A1A</Color>
                  <Color x:Key="OnPrimaryColor">#FAFAFA</Color>
                  <Color x:Key="SecondaryColor">{{Hex(t.Action)}}</Color>
                  <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
                  <Color x:Key="SurfaceColor">#FAFAFA</Color>
                  <Color x:Key="OnSurfaceColor">#1A1A1A</Color>
                  <Color x:Key="BackgroundColor">#FFFFFF</Color>
                  <Color x:Key="ErrorColor">{{Hex(t.Warn)}}</Color>
                </ResourceDictionary>

                <ResourceDictionary x:Key="Dark">
                  <Color x:Key="PrimaryColor">#FAFAFA</Color>
                  <Color x:Key="OnPrimaryColor">{{Hex(t.Surface)}}</Color>
                  <Color x:Key="SecondaryColor">{{Hex(t.Action)}}</Color>
                  <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
                  <Color x:Key="SurfaceColor">{{Hex(t.Panel)}}</Color>
                  <Color x:Key="OnSurfaceColor">#FAFAFA</Color>
                  <Color x:Key="BackgroundColor">{{Hex(t.Surface)}}</Color>
                  <Color x:Key="ErrorColor">{{Hex(t.Warn)}}</Color>
                </ResourceDictionary>

              </ResourceDictionary.ThemeDictionaries>
            </ResourceDictionary>
            """;
    }
}
```

### 9.6 InteractionsModel

```csharp
public partial record InteractionsModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs,
    IContextDeriver ContextDeriver)
{
    public IFeed<InteractionsMatrix> Matrix => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildMatrix(ctx);
    });

    public IState<string> ActiveFlowId => State.Value(this, () => "create-job");
    public IState<StateKind> ActiveStateKind => State.Value(this, () => StateKind.Default);
    public IState<string?> HoveredStateId => State.Value(this, () => (string?)null);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(5));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Interactions"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.Interactions]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "The design's settled — now every state of every flow."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("What about every state, every flow?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "For each flow, six states. Click around to see what each one means."));
    public IFeed<string> LeadQuestion => Feed.Async(_ => ValueTask.FromResult(
        "Offline state — queue silently, or always show a sync-pending banner?"));
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Queue silently", "Always show banner", "Banner only when queue has items")));
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.Interactions]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.Interactions, out var v) ? v : null);

    public async ValueTask SetActiveFlow(string flowId)
    {
        await ActiveFlowId.SetAsync(flowId);
        await ActiveStateKind.SetAsync(StateKind.Default);  // reset on flow switch
    }

    public async ValueTask SetActiveStateKind(StateKind kind) => await ActiveStateKind.SetAsync(kind);
    public async ValueTask SetHoveredState(string? stateId) => await HoveredStateId.SetAsync(stateId);
    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();

    private static InteractionsMatrix BuildMatrix(DerivedContext ctx)
    {
        var primaryFlowLabel = ctx.EntityNoun == "job" ? "Create job" : $"Create {ctx.EntityNoun}";
        var states = ImmutableList.Create(
            new StateDef(StateKind.Default, "Default", new(130, 100), "StateColorDefault", "Empty calendar; primary CTA visible."),
            new StateDef(StateKind.Loading, "Loading", new(400, 100), "StateColorLoading", "Skeleton rows while data resolves."),
            new StateDef(StateKind.Success, "Success", new(670, 100), "StateColorSuccess", "Confirmation banner; transition to terminal screen."),
            new StateDef(StateKind.Offline, "Offline", new(130, 240), "StateColorOffline", "Sync pending; queued for later."),
            new StateDef(StateKind.Empty,   "Empty",   new(400, 240), "StateColorEmpty",   "No data yet; empty illustration + CTA."),
            new StateDef(StateKind.Error,   "Error",   new(670, 240), "StateColorError",   "Coral message; retry button."));
        var flows = ImmutableList.Create(
            new InteractionFlow("create-job", primaryFlowLabel, states),
            new InteractionFlow("sign-in",    "Sign in",        states),
            new InteractionFlow("sync",       "Sync data",      states));
        return new InteractionsMatrix(flows);
    }
}
```

### 9.7 DataModel

```csharp
public partial record DataModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs,
    IContextDeriver ContextDeriver)
{
    public IFeed<DataContracts> Contracts =>
        Feed.Combine(Intent.Values, StackPrefs.Values).Select(t =>
        {
            var (intent, prefs) = t;
            var ctx = ContextDeriver.Derive(intent);
            return BuildContracts(ctx, prefs);
        });

    public IFeed<string> PrimaryRecordCSharp => Contracts.Select(BuildRecordCode);

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(6));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Data"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.Data]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "Interactions captured — let's nail down the shapes."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("What shapes does the data take?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "Three entities with explicit fields and nullability."));
    public IFeed<string> LeadQuestion => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return $"Audit trail on {ctx.EntityPlural}, or latest status only? GeoPoint as record struct, or two doubles?";
    });
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Latest status only", "Audit trail", "GeoPoint as record")));
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.Data]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.Data, out var v) ? v : null);

    private static DataContracts BuildContracts(DerivedContext ctx, StackPreferences prefs) { /* ... */ throw new NotImplementedException(); }
    private static string BuildRecordCode(DataContracts contracts) { /* ... */ throw new NotImplementedException(); }

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();
}
```

### 9.8 ImplementationModel

```csharp
public partial record ImplementationModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs)
{
    public IFeed<BuildPlan> Plan => Feed.Async(_ => ValueTask.FromResult(BuildStaticPlan()));

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(7));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Implementation"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.Implementation]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "Shapes locked in — let's plan how it gets built."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("How does it get built?"));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "Six phases as a tabular plan — files, dependencies, agent prompts."));
    public IFeed<string> LeadQuestion => Feed.Async(_ => ValueTask.FromResult(
        "Strictly linear, or can Domain and Screens parallelize?"));
    public IFeed<ImmutableList<string>> Suggestions => Feed.Async(_ => ValueTask.FromResult(
        ImmutableList.Create("Strictly linear", "Parallelize", "Add a tooling phase")));
    public IState<string> PromptValue => Shell.Composer.Prompts.Select(p => p[LayerKind.Implementation]);
    public IFeed<string?> PreviewAck => Shell.Composer.PreviewAcks.Select(p =>
        p.TryGetValue(LayerKind.Implementation, out var v) ? v : null);

    private static BuildPlan BuildStaticPlan() => /* ... 6 phases ... */ throw new NotImplementedException();

    public async ValueTask LockAndContinue() => await Shell.LockAndContinue();
    public async ValueTask GeneratePreview() => await Shell.GeneratePreview();
    public async ValueTask AcceptPreview() => await Shell.AcceptPreview();
    public async ValueTask DiscardPreview() => await Shell.DiscardPreview();
    public async ValueTask DiscardEdits() => await Shell.DiscardEdits();
}
```

### 9.9 ScaffoldModel

```csharp
public partial record ScaffoldModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs,
    ComposerModel Composer,
    IBundleBuilder BundleBuilder,
    IClipboardService Clipboard,
    IFileDownloadService FileDownload,
    IContextDeriver ContextDeriver)
{
    public IFeed<string> ScaffoldCommand =>
        Feed.Combine(Intent.Values, StackPrefs.Values).Select(t =>
            BuildCommand(t.Item1, t.Item2));

    public IFeed<string> PromptContextMarkdown =>
        Feed.Combine(Intent.Values, StackPrefs.Values, Composer.Overrides).Select(t =>
            BundleBuilder.BuildPromptContext(t.Item1, t.Item2, t.Item3));

    public IFeed<int> LayerIndex => Feed.Async(_ => ValueTask.FromResult(8));
    public IFeed<string> LayerLabel => Feed.Async(_ => ValueTask.FromResult("Scaffold"));
    public IFeed<LayerState> LayerState => Shell.LayerStates.Select(s => s[LayerKind.Scaffold]);
    public IFeed<string?> Recap => Feed.Async(_ => ValueTask.FromResult<string?>(
        "Eight layers locked. Here's what ships."));
    public IFeed<string> Title => Feed.Async(_ => ValueTask.FromResult("The bundle is ready."));
    public IFeed<string> Subtitle => Feed.Async(_ => ValueTask.FromResult(
        "Every layer locked. Copy the scaffold, or download the full bundle."));

    public async ValueTask CopyScaffoldCommand()
    {
        var cmd = await ScaffoldCommand;
        await Clipboard.SetTextAsync(cmd);
    }

    public async ValueTask CopyPromptContext()
    {
        var content = await PromptContextMarkdown;
        await Clipboard.SetTextAsync(content);
    }

    public async ValueTask DownloadBundle()
    {
        var intent = await Intent.Values;
        var prefs = await StackPrefs.Values;
        var overrides = await Composer.Overrides;
        var scaffoldCmd = await ScaffoldCommand;
        var ctx = ContextDeriver.Derive(intent);
        var bytes = await BundleBuilder.BuildFullBundleAsync(intent, prefs, overrides, scaffoldCmd, CancellationToken.None);
        await FileDownload.SaveAsync($"{ctx.AppName}-bundle.md", bytes);
        await Shell.LockAndContinue();
    }

    private static string BuildCommand(IntentValues intent, StackPreferences prefs)
    {
        var appName = new string((intent.AppType ?? "App").Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(appName)) appName = "App";
        var platforms = string.Join(",", prefs.Platforms.Select(p => p.ToString().ToLowerInvariant()));
        var presentation = prefs.Pattern switch
        {
            StackPattern.Mvux or StackPattern.MvuxMessaging => "mvux",
            StackPattern.Mvvm => "mvvm",
            _ => "mvux",
        };
        var theme = prefs.Theme.ToString().ToLowerInvariant();
        var markup = prefs.Markup == MarkupKind.Xaml ? "xaml" : "csharp";
        var features = "config,http,logging,nav," + presentation;
        return $$"""
            dotnet new unoapp \
              -n {{appName}} \
              --tfm net10.0 \
              --platforms {{platforms}} \
              --markup {{markup}} --presentation {{presentation}} --theme {{theme}} \
              --features {{features}}
            """;
    }
}
```

### 9.10 ComposerModel

```csharp
public partial record ComposerModel(ShellModel Shell)
{
    public IState<ImmutableDictionary<LayerKind, string>> Prompts =>
        State.Value(this, () => Layers.All.ToImmutableDictionary(l => l.Kind, _ => ""));

    public IState<ImmutableDictionary<LayerKind, string>> Overrides =>
        State.Value(this, () => ImmutableDictionary<LayerKind, string>.Empty);

    public IState<ImmutableDictionary<LayerKind, string?>> PreviewAcks =>
        State.Value(this, () => Layers.All.ToImmutableDictionary<LayerDef, LayerKind, string?>(
            l => l.Kind, _ => null));

    public async ValueTask UpdatePrompt(LayerKind kind, string text)
    {
        var prompts = await Prompts;
        await Prompts.SetAsync(prompts.SetItem(kind, text));
        if (!string.IsNullOrWhiteSpace(text))
        {
            var states = await Shell.LayerStates;
            if (states[kind] == LayerState.Clean)
                await Shell.MarkDirty(kind);
        }
    }

    public async ValueTask SetOverride(LayerKind kind, string? content)
    {
        var overrides = await Overrides;
        if (content == null)
            await Overrides.SetAsync(overrides.Remove(kind));
        else
            await Overrides.SetAsync(overrides.SetItem(kind, content));
        await Shell.MarkDirty(kind);
    }

    public async ValueTask CapturePreviewAck(LayerKind kind, string? userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            var acks = await PreviewAcks;
            await PreviewAcks.SetAsync(acks.SetItem(kind, null));
            return;
        }
        var first = ExtractFirstSentence(userPrompt);
        var concise = first.Length > 80 ? first[..80].TrimEnd() + "…" : first;
        var ack = $"You asked: \"{concise}\" — here's what changes if I apply that.";
        var acksMap = await PreviewAcks;
        await PreviewAcks.SetAsync(acksMap.SetItem(kind, ack));
    }

    private static string ExtractFirstSentence(string prompt)
    {
        var trimmed = prompt.Trim();
        var match = Regex.Match(trimmed, @"^[^.!?]+[.!?]?");
        return match.Success ? match.Value.Trim() : trimmed;
    }
}
```

### 9.11 FilesRailModel

```csharp
public partial record FilesRailModel(
    ShellModel Shell,
    IntentModel Intent,
    StackPreferencesModel StackPrefs,
    DesignModel Design,
    ComposerModel Composer,
    ILayerBriefGenerator BriefGenerator,
    IMarkdownRenderer Renderer,
    IBundleBuilder BundleBuilder,
    IClipboardService Clipboard)
{
    public IState<bool> EditingMode => State.Value(this, () => false);
    public IState<bool> ViewAllMode => State.Value(this, () => false);

    public IFeed<ImmutableList<FileRowData>> FileRows =>
        Feed.Combine(Shell.ActiveIndex, Shell.LockedIds).Select(t =>
        {
            var (idx, locked) = t;
            var rows = Layers.All
                .Select(l => new FileRowData(l.OutputFile, ResolveStatus(l, idx, locked)))
                .ToImmutableList();
            var promptContextStatus = locked.Contains(LayerKind.Scaffold)
                ? FileStatus.Drafted : FileStatus.Planned;
            return rows.Add(new FileRowData("prompt-context.md", promptContextStatus));
        });

    public IFeed<string> ActivePreviewContent =>
        Feed.Combine(Shell.ActiveIndex, Intent.Values, StackPrefs.Values, Design.Tokens,
                     Composer.Overrides, ViewAllMode).Select(t =>
    {
        var (idx, intent, prefs, design, overrides, viewAll) = t;
        var activeLayer = Layers.Get(idx);

        if (viewAll && activeLayer.Kind == LayerKind.Scaffold)
            return BundleBuilder.BuildPromptContext(intent, prefs, overrides);

        if (overrides.TryGetValue(activeLayer.Kind, out var ov))
            return ov;

        var state = new CompositionState(intent, prefs, design);
        var brief = BriefGenerator.Generate(activeLayer.Kind, state);
        return Renderer.Render(brief);
    });

    public IFeed<bool> CanViewAll =>
        Feed.Combine(Shell.ActiveIndex, Shell.LockedIds).Select(t =>
            (LayerKind)t.Item1 == LayerKind.Scaffold && t.Item2.Count >= Layers.Count - 1);

    public IFeed<string> ActivePreviewFilename =>
        Feed.Combine(Shell.ActiveIndex, ViewAllMode).Select(t =>
            t.Item2 && (LayerKind)t.Item1 == LayerKind.Scaffold
                ? "prompt-context.md"
                : Layers.Get(t.Item1).OutputFile);

    public IFeed<string> ActivePreviewSource =>
        Feed.Combine(Shell.ActiveIndex, ViewAllMode, Composer.Overrides).Select(t =>
        {
            var (idx, viewAll, overrides) = t;
            if (viewAll && (LayerKind)idx == LayerKind.Scaffold)
                return $"ALL {Layers.Count} LAYERS · CONCATENATED";
            return overrides.ContainsKey((LayerKind)idx)
                ? "YOUR EDITS"
                : "GENERATED FROM CANVAS";
        });

    public IFeed<string> LockedCountText =>
        Shell.LockedIds.Select(l => $"{l.Count} of {Layers.Count} locked");

    public IFeed<string> StatusContext =>
        Feed.Combine(Shell.ActiveIndex, Shell.LockedIds).Select(t =>
        {
            var (idx, locked) = t;
            if (locked.Count == Layers.Count)
                return "All layers locked. Bundle ready.";
            var layer = Layers.Get(idx);
            return $"{layer.Label} will write {layer.OutputFile} when locked.";
        });

    public async ValueTask ToggleEditing() => await EditingMode.SetAsync(!await EditingMode);
    public async ValueTask ToggleViewAll() => await ViewAllMode.SetAsync(!await ViewAllMode);

    public async ValueTask CopyActiveContent()
    {
        var content = await ActivePreviewContent;
        await Clipboard.SetTextAsync(content);
    }

    public async ValueTask UpdateOverride(string content)
    {
        var idx = await Shell.ActiveIndex;
        var kind = Layers.Get(idx).Kind;
        await Composer.SetOverride(kind, content);
    }

    public async ValueTask ResetOverride()
    {
        var idx = await Shell.ActiveIndex;
        var kind = Layers.Get(idx).Kind;
        await Composer.SetOverride(kind, null);
    }

    private static FileStatus ResolveStatus(LayerDef layer, int activeIdx, ImmutableHashSet<LayerKind> locked)
    {
        if (locked.Contains(layer.Kind)) return FileStatus.Drafted;
        if ((int)layer.Kind == activeIdx) return FileStatus.Writing;
        return FileStatus.Planned;
    }
}
```

### 9.12 CompositionStackModel

```csharp
public partial record CompositionStackModel(ShellModel Shell)
{
    public IFeed<ImmutableList<StackItemData>> Items =>
        Feed.Combine(Shell.ActiveIndex, Shell.LockedIds, Shell.Intent.Values, Shell.Design.Tokens).Select(t =>
        {
            var (idx, locked, intent, design) = t;
            return Layers.All.Select((layer, i) =>
            {
                var state = locked.Contains(layer.Kind) ? StackItemState.Locked
                          : i == idx                    ? StackItemState.Active
                                                        : StackItemState.Future;
                var summary = state switch
                {
                    StackItemState.Active => layer.Hint,
                    StackItemState.Locked => BuildLockedSummary(layer.Kind, intent, design),
                    _ => layer.Hint,
                };
                return new StackItemData(i, layer, state, summary);
            }).ToImmutableList();
        });

    public async ValueTask OnItemClicked(int index)
    {
        var locked = await Shell.LockedIds;
        var layer = Layers.Get(index);
        if (locked.Contains(layer.Kind))
        {
            await Shell.Revisit(layer.Kind);
        }
        // Active and Future rows are no-ops
    }

    private static string BuildLockedSummary(LayerKind kind, IntentValues intent, DesignTokens design)
    {
        return kind switch
        {
            LayerKind.Stack => "MVUX, Material theme",
            LayerKind.Intent => $"\"{intent.AppType}\"",
            LayerKind.DesignSystem => $"{design.BodyFont}, action #{design.Action.R:X2}{design.Action.G:X2}{design.Action.B:X2}",
            _ => Layers.Get(kind).Hint,
        };
    }
}

public record StackItemData(int Index, LayerDef Layer, StackItemState State, string Summary);
public enum StackItemState { Active, Locked, Future }
```

---

## 10. Services

### 10.1 ILayerPreviewService

The AI augmentation contract. Implementations may augment proposed values based on the user's prompt and the cumulative composition context.

```csharp
namespace ComposerContextEngine.Services;

public record LayerPreviewRequest(
    LayerKind Kind,
    object CurrentValues,
    string? UserPrompt,
    StackPreferences StackPrefs,
    IntentValues Intent,
    DesignTokens Design,
    ImmutableDictionary<LayerKind, string> LockedContextSummaries);

public record LayerPreviewResult(
    object ProposedValues,
    string Summary);

public interface ILayerPreviewService
{
    bool IsConfigured { get; }
    Task<bool> IsConfiguredAsync(CancellationToken ct = default);
    Task<LayerPreviewResult> GeneratePreviewAsync(LayerPreviewRequest request, CancellationToken ct = default);
}
```

#### 10.1.1 IdentityLayerPreviewService (fallback)

```csharp
public sealed class IdentityLayerPreviewService : ILayerPreviewService
{
    public bool IsConfigured => false;
    public Task<bool> IsConfiguredAsync(CancellationToken ct = default) => Task.FromResult(false);

    public Task<LayerPreviewResult> GeneratePreviewAsync(LayerPreviewRequest request, CancellationToken ct = default)
        => Task.FromResult(new LayerPreviewResult(
            ProposedValues: request.CurrentValues,
            Summary: "Showing your edits as proposed."));
}
```

This implementation is used when no AI API key is configured. It returns the user's current values unchanged, allowing the rest of the state machine (Preview state, acknowledgment line, Accept-and-lock flow) to function without an AI provider.

#### 10.1.2 ClaudeLayerPreviewService

```csharp
public sealed class ClaudeLayerPreviewService : ILayerPreviewService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly IContextDeriver _contextDeriver;
    private readonly ILogger<ClaudeLayerPreviewService> _logger;

    public ClaudeLayerPreviewService(
        HttpClient http, string apiKey,
        IContextDeriver contextDeriver,
        ILogger<ClaudeLayerPreviewService> logger)
    {
        _http = http;
        _apiKey = apiKey;
        _contextDeriver = contextDeriver;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);
    public Task<bool> IsConfiguredAsync(CancellationToken ct = default) => Task.FromResult(IsConfigured);

    public async Task<LayerPreviewResult> GeneratePreviewAsync(LayerPreviewRequest request, CancellationToken ct = default)
    {
        try
        {
            // Build per-layer prompt, call Anthropic API, parse structured response.
            // Implementation deferred — see ENGINEERING-BRIEF-03 for the AI pipeline spec.
            return new LayerPreviewResult(request.CurrentValues, "Preview generated.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI preview failed for layer {Layer}. Falling through to identity.", request.Kind);
            return new LayerPreviewResult(request.CurrentValues, "AI unavailable. Showing your edits unchanged.");
        }
    }
}
```

Failure mode is silent: if the API call throws, the service returns the identity result with a notice in the Summary field. The user sees the preview state activate; the visual diff is empty (no values changed), but the workflow proceeds.

### 10.2 IContextDeriver

Pure function. Extracts domain context from Intent values for use across downstream layer models.

```csharp
public record DerivedContext(
    string AppName,
    string EntityNoun,
    string EntityTitle,
    string EntityPlural,
    string UserNoun,
    bool IsOfflineFirst,
    bool IsMobileFirst,
    bool IsFieldService);

public interface IContextDeriver
{
    DerivedContext Derive(IntentValues intent);
}

public sealed class ContextDeriver : IContextDeriver
{
    private static readonly (Regex Match, string Noun)[] Rules = new[]
    {
        (new Regex(@"habit|streak",                            RegexOptions.IgnoreCase), "habit"),
        (new Regex(@"recipe|cook|meal",                        RegexOptions.IgnoreCase), "recipe"),
        (new Regex(@"workout|exercise|fitness",                RegexOptions.IgnoreCase), "workout"),
        (new Regex(@"trade|portfolio|invest|stock",            RegexOptions.IgnoreCase), "trade"),
        (new Regex(@"task|todo|backlog",                       RegexOptions.IgnoreCase), "task"),
        (new Regex(@"note|journal|diary",                      RegexOptions.IgnoreCase), "note"),
        (new Regex(@"appointment|booking|reserv",              RegexOptions.IgnoreCase), "appointment"),
        (new Regex(@"patient|medical|health|clinic",           RegexOptions.IgnoreCase), "patient"),
        (new Regex(@"invoice|billing|payment",                 RegexOptions.IgnoreCase), "invoice"),
        (new Regex(@"order|purchase|cart",                     RegexOptions.IgnoreCase), "order"),
        (new Regex(@"ticket|incident|issue",                   RegexOptions.IgnoreCase), "ticket"),
        (new Regex(@"lesson|class|course",                     RegexOptions.IgnoreCase), "lesson"),
        (new Regex(@"dispatch|field-service|job|service-call", RegexOptions.IgnoreCase), "job"),
    };

    public DerivedContext Derive(IntentValues intent)
    {
        var blob = string.Join(' ',
            new[] { intent.AppType, intent.Workflow, intent.PrimaryUser }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        var matched = Rules.FirstOrDefault(r => r.Match.IsMatch(blob));
        var noun = matched.Noun ?? "item";
        var title = char.ToUpper(noun[0]) + noun[1..];
        var plural = noun.EndsWith('s') ? noun : noun + "s";

        var cleaned = Regex.Replace(intent.AppType ?? "App", "[^a-zA-Z0-9 ]", "");
        var appName = string.Concat(cleaned
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0]) + w[1..]));
        if (string.IsNullOrWhiteSpace(appName)) appName = "App";

        var userParts = (intent.PrimaryUser ?? "users").ToLowerInvariant().Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var userNoun = userParts.Length > 0 ? userParts[^1] : "users";

        return new DerivedContext(
            AppName: appName,
            EntityNoun: noun,
            EntityTitle: title,
            EntityPlural: plural,
            UserNoun: userNoun,
            IsOfflineFirst: Regex.IsMatch(blob, @"offline|local|queue|sync\s+later|no\s+backend", RegexOptions.IgnoreCase),
            IsMobileFirst:  Regex.IsMatch(blob, @"mobile|phone|tablet|on-the-go", RegexOptions.IgnoreCase),
            IsFieldService: intent.AppType == IntentModel.Example.AppType);
    }
}
```

### 10.3 ILayerBriefGenerator + IMarkdownRenderer

Structured brief generation. See `ENGINEERING-BRIEF-02-structured-layer-brief-generators.md` for the full implementation pattern.

```csharp
public record CompositionState(IntentValues Intent, StackPreferences StackPrefs, DesignTokens Design);

public interface ILayerBriefGenerator
{
    LayerBrief Generate(LayerKind kind, CompositionState state);
}

public interface IMarkdownRenderer
{
    string Render(LayerBrief brief);
}

public sealed class LayerBriefGenerator : ILayerBriefGenerator
{
    private readonly IContextDeriver _contextDeriver;

    public LayerBriefGenerator(IContextDeriver contextDeriver) => _contextDeriver = contextDeriver;

    public LayerBrief Generate(LayerKind kind, CompositionState state) => kind switch
    {
        LayerKind.Stack          => GenerateStackBrief(state),
        LayerKind.Intent         => GenerateIntentBrief(state),
        LayerKind.UX             => GenerateUXBrief(state),
        LayerKind.Architecture   => GenerateArchitectureBrief(state),
        LayerKind.DesignSystem   => GenerateDesignBrief(state),
        LayerKind.Interactions   => GenerateInteractionsBrief(state),
        LayerKind.Data           => GenerateDataBrief(state),
        LayerKind.Implementation => GenerateImplementationBrief(state),
        LayerKind.Scaffold       => GenerateScaffoldBrief(state),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    // Per-layer Generate* methods. Each returns a LayerBrief with structured sections,
    // code blocks, annotations, cross-references, acceptance criteria. See companion
    // Brief 02 for full implementations.
    private LayerBrief GenerateStackBrief(CompositionState state) { /* ... */ throw new NotImplementedException(); }
    private LayerBrief GenerateIntentBrief(CompositionState state) { /* ... */ throw new NotImplementedException(); }
    // ... seven more
}

public sealed class MarkdownRenderer : IMarkdownRenderer
{
    public string Render(LayerBrief brief)
    {
        var lines = new List<string>();
        lines.Add($"# {brief.Title}");
        lines.Add("");
        lines.Add($"*Generated for {brief.Audience}. Version {brief.Version}.*");
        lines.Add("");

        foreach (var section in brief.Sections)
        {
            lines.Add($"## {section.Number}. {section.Heading}");
            lines.Add("");
            if (!string.IsNullOrEmpty(section.Body))
            {
                lines.Add(section.Body);
                lines.Add("");
            }
            // ... code blocks, annotations, cross-references rendered per Brief 02 spec
        }

        if (brief.AcceptanceCriteria.Any())
        {
            lines.Add("## Acceptance criteria");
            lines.Add("");
            foreach (var crit in brief.AcceptanceCriteria) lines.Add($"- [ ] {crit}");
            lines.Add("");
        }

        if (brief.OutOfScope?.Any() == true)
        {
            lines.Add("## Out of scope");
            lines.Add("");
            foreach (var item in brief.OutOfScope) lines.Add($"- {item}");
            lines.Add("");
        }

        return string.Join('\n', lines).Trim();
    }
}
```

### 10.4 IMarkdownGenerator (compatibility wrapper)

```csharp
public interface IMarkdownGenerator
{
    string Generate(LayerKind kind, IntentValues intent, StackPreferences prefs, DesignTokens design);
}

public sealed class StructuredMarkdownGenerator : IMarkdownGenerator
{
    private readonly ILayerBriefGenerator _briefGen;
    private readonly IMarkdownRenderer _renderer;

    public StructuredMarkdownGenerator(ILayerBriefGenerator briefGen, IMarkdownRenderer renderer)
    {
        _briefGen = briefGen;
        _renderer = renderer;
    }

    public string Generate(LayerKind kind, IntentValues intent, StackPreferences prefs, DesignTokens design)
        => _renderer.Render(_briefGen.Generate(kind, new CompositionState(intent, prefs, design)));
}
```

### 10.5 IBundleBuilder

```csharp
public interface IBundleBuilder
{
    string BuildPromptContext(
        IntentValues intent,
        StackPreferences prefs,
        ImmutableDictionary<LayerKind, string> overrides);

    Task<byte[]> BuildFullBundleAsync(
        IntentValues intent,
        StackPreferences prefs,
        ImmutableDictionary<LayerKind, string> overrides,
        string scaffoldCommand,
        CancellationToken ct = default);
}

public sealed class BundleBuilder : IBundleBuilder
{
    private readonly IMarkdownGenerator _markdownGen;

    public BundleBuilder(IMarkdownGenerator markdownGen) => _markdownGen = markdownGen;

    public string BuildPromptContext(IntentValues intent, StackPreferences prefs, ImmutableDictionary<LayerKind, string> overrides)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var layer in Layers.All)
        {
            if (!first) sb.AppendLine("\n---\n");
            first = false;
            sb.AppendLine($"<!-- {layer.OutputFile} -->");
            sb.AppendLine();
            var content = overrides.TryGetValue(layer.Kind, out var ov)
                ? ov
                : _markdownGen.Generate(layer.Kind, intent, prefs, DesignModel.Defaults);  // design tokens injected at higher level
            sb.AppendLine(content);
        }
        return sb.ToString();
    }

    public Task<byte[]> BuildFullBundleAsync(
        IntentValues intent, StackPreferences prefs,
        ImmutableDictionary<LayerKind, string> overrides,
        string scaffoldCommand, CancellationToken ct)
    {
        var content = $"""
            # {intent.AppType} — Composition bundle

            Generated by Composer Context Engine.

            {BuildPromptContext(intent, prefs, overrides)}

            ---

            ## Scaffold command

            ```bash
            {scaffoldCommand}
            ```
            """;
        return Task.FromResult(Encoding.UTF8.GetBytes(content));
    }
}
```

### 10.6 IClipboardService

```csharp
public interface IClipboardService
{
    Task SetTextAsync(string text, CancellationToken ct = default);
    Task<string?> GetTextAsync(CancellationToken ct = default);
}

public sealed class ClipboardService : IClipboardService
{
    public Task SetTextAsync(string text, CancellationToken ct = default)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
        return Task.CompletedTask;
    }

    public async Task<string?> GetTextAsync(CancellationToken ct = default)
    {
        var content = Clipboard.GetContent();
        if (content.Contains(StandardDataFormats.Text))
            return await content.GetTextAsync();
        return null;
    }
}
```

### 10.7 IFileDownloadService

```csharp
public interface IFileDownloadService
{
    Task SaveAsync(string fileName, byte[] content, CancellationToken ct = default);
}

public sealed class FileDownloadService : IFileDownloadService
{
    public async Task SaveAsync(string fileName, byte[] content, CancellationToken ct = default)
    {
        // Platform-specific implementation. On Windows/macOS/Linux uses FileSavePicker.
        // On WASM uses Blob + URL.createObjectURL + anchor click.
        // On mobile triggers OS share sheet.
        var picker = new FileSavePicker
        {
            SuggestedFileName = fileName,
        };
        picker.FileTypeChoices.Add("Markdown", new[] { ".md" });
        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            await FileIO.WriteBytesAsync(file, content);
        }
    }
}
```

---

## 11. Derived feeds and context flow

### 11.1 Data flow diagram

```
                          ┌──────────────────────┐
                          │ StackPreferencesModel │
                          │  IState<StackPrefs>  │
                          └──────────────────────┘
                                     │
                                     ▼
                          ┌──────────────────────┐
                          │     IntentModel       │
                          │  IState<IntentValues> │
                          └──────────────────────┘
                                     │
                                     ▼
                          ┌──────────────────────┐
                          │    IContextDeriver    │
                          │  pure: IntentValues   │
                          │      → DerivedContext │
                          └──────────────────────┘
                                     │
                ┌────────────────────┼────────────────────┐
                ▼                    ▼                    ▼
       ┌────────────────┐  ┌────────────────┐  ┌────────────────┐
       │    UXModel     │  │ ArchitectureModel│ │  DataModel     │
       │  IFeed<UXFlow> │  │  IFeed<Bp>      │ │  IFeed<DC>     │
       └────────────────┘  └────────────────┘  └────────────────┘
                ▼                    ▼                    ▼
       ┌────────────────┐  ┌────────────────┐  ┌────────────────┐
       │ InteractionsM  │  │ImplementationM │ │  ScaffoldModel │
       │  IFeed<Matrix> │  │  IFeed<Plan>   │ │  IFeed<string> │
       └────────────────┘  └────────────────┘  └────────────────┘

       DesignModel is independent:
       ┌────────────────┐
       │   DesignModel  │
       │  IState<Tokens>│
       │  IFeed<XAML>   │  (live-synthesized)
       └────────────────┘
```

### 11.2 Feed composition rules

- **Combine with `Feed.Combine`** when a derived feed depends on multiple upstream IStates (e.g., Architecture depends on Intent + StackPrefs)
- **Use `.Select(...)` for pure transformations** — never side effects, no I/O
- **Re-derivation is automatic** — any change to an upstream IState triggers re-evaluation of all dependent IFeed values
- **Reference equality matters** — use `with` expressions on records to update; never mutate

### 11.3 Why this matters

When a user edits the Stack layer's Pattern from MVUX to MVVM and re-locks, every downstream IFeed re-evaluates:

- `ArchitectureModel.Blueprint` recomputes — the State (MVUX) module label changes to "ViewModels"
- `InteractionsModel` doesn't recompute (it doesn't depend on StackPrefs)
- `ScaffoldModel.ScaffoldCommand` recomputes — the `--presentation` flag changes from `mvux` to `mvvm`
- `FilesRailModel.ActivePreviewContent` recomputes — the live preview reflects the new Pattern

No explicit invalidation needed. The MVUX feed system handles propagation.

---

## 12. Page registration and structure

### 12.1 The canonical page template

Every page in the application follows the same vertical sequence of UserControls. Only Slot 5 (the canvas) and the bindings in Slots 4 and 6 vary per page.

```xml
<!-- Pages/{Layer}Page.xaml — canonical template -->
<Page x:Class="ComposerContextEngine.Views.Pages.{Layer}Page"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:utu="using:Uno.Toolkit.UI"
      xmlns:controls="using:ComposerContextEngine.Views.Controls"
      xmlns:canvases="using:ComposerContextEngine.Views.Canvases"
      x:Uid="{Layer}Page">

    <ScrollViewer Padding="32,32,48,80">
        <utu:AutoLayout Orientation="Vertical" Spacing="0">

            <!-- Slot 1: Shell-bound progress indicator -->
            <controls:ProgressIndicator
                DataContext="{Binding Shell, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Shell}}" />

            <!-- Slot 2: Shell-bound app title row -->
            <controls:AppTitleRow
                DataContext="{Binding Shell, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Shell}}" />

            <!-- Slot 3: Shell-bound locked context cards -->
            <ItemsRepeater
                DataContext="{Binding Shell, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Shell}}"
                ItemsSource="{Binding LockedCards, Mode=OneWay}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:LockedContextCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>

            <!-- Slot 4: Per-page active layer header -->
            <controls:ActiveLayerHeader
                LayerIndex="{Binding LayerIndex, Mode=OneWay}"
                LayerLabel="{Binding LayerLabel, Mode=OneWay}"
                LayerState="{Binding LayerState, Mode=OneWay}"
                Recap="{Binding Recap, Mode=OneWay}"
                Title="{Binding Title, Mode=OneWay}"
                Subtitle="{Binding Subtitle, Mode=OneWay}" />

            <!-- Slot 5: Per-page canvas (varies per page) -->
            <canvases:{Layer}Canvas />

            <!-- Slot 6: Per-page composer footer -->
            <controls:ComposerFooter
                LayerState="{Binding LayerState, Mode=OneWay}"
                LeadQuestion="{Binding LeadQuestion, Mode=OneWay}"
                Suggestions="{Binding Suggestions, Mode=OneWay}"
                PromptValue="{Binding PromptValue, Mode=TwoWay}"
                PreviewAck="{Binding PreviewAck, Mode=OneWay}" />

            <!-- Slot 7: Shell-bound future preview cards -->
            <ItemsRepeater
                DataContext="{Binding Shell, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Shell}}"
                ItemsSource="{Binding FutureCards, Mode=OneWay}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:FuturePreviewCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>

        </utu:AutoLayout>
    </ScrollViewer>
</Page>
```

### 12.2 Page code-behind (canonical)

```csharp
// Pages/{Layer}Page.xaml.cs
namespace ComposerContextEngine.Views.Pages;

public sealed partial class {Layer}Page : Page
{
    public {Layer}Page()
    {
        InitializeComponent();
    }
}
```

**Empty by design.** No event handlers, no navigation calls, no field bindings. All logic lives in the layer's MVUX model.

### 12.3 ScaffoldPage exception

ScaffoldPage omits Slot 6 (ComposerFooter) entirely. The Scaffold layer has no prompt, no chips, no preview state. The page's full markup:

```xml
<Page x:Class="ComposerContextEngine.Views.Pages.ScaffoldPage"
      ...>
    <ScrollViewer Padding="32,32,48,80">
        <utu:AutoLayout Orientation="Vertical" Spacing="0">
            <controls:ProgressIndicator ... />
            <controls:AppTitleRow ... />
            <ItemsRepeater ItemsSource="{Binding LockedCards, ...}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate><controls:LockedContextCard /></DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>
            <controls:ActiveLayerHeader ... />
            <canvases:ScaffoldTerminalCanvas />
            <!-- No ComposerFooter -->
            <!-- No FuturePreviewCards (Scaffold is the last layer) -->
        </utu:AutoLayout>
    </ScrollViewer>
</Page>
```

---

## 13. Resource dictionary structure

### 13.1 Loading order

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 1. Material baseline -->
            <mat:MaterialTheme />

            <!-- 2. Live-synthesized color overrides from DesignTokens -->
            <ResourceDictionary Source="Themes/ColorPaletteOverride.xaml" />

            <!-- 3. Static semantic colors (Info, Success, Tag, Locked, phase tints, state colors) -->
            <ResourceDictionary Source="Themes/ThemeColorOverrides.xaml" />

            <!-- 4. Named brush keys derived from theme colors -->
            <ResourceDictionary Source="Themes/Brushes.xaml" />

            <!-- 5. Font family resources -->
            <ResourceDictionary Source="Themes/Typography.xaml" />

            <!-- 6. TextBlock styles (DisplayLarge, HeadingLarge, BodyLarge, EyebrowSmall, etc.) -->
            <ResourceDictionary Source="Themes/TextBlock.xaml" />

            <!-- 7. Button styles -->
            <ResourceDictionary Source="Themes/Buttons.xaml" />

            <!-- 8. Editorial primitive styles -->
            <ResourceDictionary Source="Themes/Editorial.xaml" />

            <!-- 9. Reusable Storyboard templates -->
            <ResourceDictionary Source="Themes/Animations.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 13.2 Material slot mapping

DesignModel writes the following Material slots in `ColorPaletteOverride.xaml`. All other resources use Material defaults.

| DesignTokens field | Material Light slot | Material Dark slot                |
|--------------------|---------------------|------------------------------------|
| `Action`           | `SecondaryColor`    | `SecondaryColor` (same)            |
| `Warn`             | `ErrorColor`        | `ErrorColor` (same)                |
| `Surface`          | (Light default)     | `BackgroundColor`                  |
| `Panel`            | (Light default)     | `SurfaceColor`                     |

`Info`, `Success`, `Tag`, `Locked` are written as additional non-Material resources in `ThemeColorOverrides.xaml`.

### 13.3 Live synthesis flow

```
DesignModel.Tokens IState changed
        │
        ▼
DesignModel.ColorPaletteOverrideXaml IFeed re-evaluates
        │
        ▼
(Future: hot-reload the ColorPaletteOverride.xaml resource dictionary)
```

In v1.0, the synthesized XAML is displayed in the Design canvas's CodeBlock for the user/agent to copy. Hot-reloading the actual resource dictionary at runtime is out of scope for this version.

---

## 14. Lifecycle and process model

### 14.1 Startup sequence

```
1. OS launches process
2. Platform-specific entry (Program.cs on Desktop, MainActivity on Android, etc.)
3. App.OnLaunched fires
4. HostBuilder configured:
   - Logging providers registered
   - Configuration sources loaded
   - DI container built
   - All service singletons created
5. Shell window created, Shell UserControl instantiated
6. Shell resolves ShellModel from DI
7. ShellModel's IStates initialize (ActiveIndex=0, LockedIds=empty, LayerStates=all Clean)
8. Navigation system processes Stack route (IsDefault: true)
9. StackPreferencesModel resolved from DI
10. StackPreferencesPage created and placed in ActivePage region
11. Initial render — focused first screen (rails collapsed, center column tight)
```

### 14.2 Per-page model lifetimes

When the user advances from layer N to layer N+1:

```
1. Navigator.NavigateRouteAsync("{NextLayer}", Qualifiers.Nested) called
2. Current page unloaded from ActivePage region
3. Current page's MVUX model is eligible for garbage collection
   (its IStates were transient; no references held by Shell beyond the resolution scope)
4. New page resolved
5. New page's MVUX model resolved from DI
6. New page's IStates initialize with their default values
7. New page placed in ActivePage region
```

**Critical:** the previous page's canvas state (Intent values, Design tokens, etc.) is preserved only because ShellModel holds direct references to those layer models via DI scoping. When the user revisits layer M from layer N, Uno's navigation system resolves a *new* `MModel` instance — but ShellModel's reference graph already contains the previously-instantiated model. The DI container's scoping ensures this consistency.

In practice, for v1.0, layer models are registered as Transient and resolved fresh on each navigation. State preservation across navigation is achieved by ShellModel holding references to the canonical IStates (not the model instances themselves). When ShellModel.Intent.Values is accessed after navigation, it resolves the *same* IState backing store because the State is created with `State.Value(this, () => ...)` keyed off the model identity within the DI scope.

### 14.3 Window close / app suspension

V1.0 has no persistence. When the app closes, all composition state is lost. This is by design — the engine is a workspace for crafting a brief, not an authoring tool with documents.

Future versions may persist:
- The current composition to a versioned local storage entry
- The user's stack preferences as application-level settings (default-loaded across sessions)
- Recent composition history (last 10 sessions)

### 14.4 Reset behavior

`ShellModel.Reset()` does not unload the Shell or recreate any models. It resets every IState to its initial value and clears every transient cache. The Shell continues running; navigation goes back to Stack.

---

## 15. Data flow

### 15.1 User edit → canvas → state → derived feed

```
User clicks a Pattern segment in StackPreferencesCanvas
        │
        ▼
Segment Button raises Click event
        │
        ▼
Button.Command resolves to StackPreferencesModel.UpdatePattern (implicit IAsyncCommand)
        │
        ▼
UpdatePattern: reads current StackPrefs IState, applies `with { Pattern = newValue }`,
              writes new value to StackPrefs IState
        │
        ▼
StackPrefs IState change propagates
        │
        ├──► StackPreferencesModel.ShowingExample re-evaluates (no, was always true here)
        ├──► ArchitectureModel.Blueprint re-evaluates (modules changed: State (MVUX) → ViewModels)
        ├──► ScaffoldModel.ScaffoldCommand re-evaluates (--presentation mvvm)
        ├──► FilesRailModel.ActivePreviewContent re-evaluates
        └──► UpdatePattern then calls ShellModel.MarkDirty(LayerKind.Stack)
                │
                ▼
                ShellModel.LayerStates IState changes (Stack: Clean → Dirty)
                Snapshot captured
                ActiveLayerHeader badge appears
                ComposerFooter changes (primary button becomes "Generate preview →" amber)
```

### 15.2 Generate Preview flow

```
User presses Cmd+Enter with non-empty prompt
        │
        ▼
ComposerFooter handles KeyDown → checks state, calls layer model's GeneratePreview command
        │
        ▼
StackPreferencesModel.GeneratePreview → Shell.GeneratePreview()
        │
        ▼
ShellModel.GeneratePreview:
  - Reads current canvas values for current layer
  - Reads user prompt from Composer.Prompts[currentKind]
  - Builds LayerPreviewRequest
  - Calls ILayerPreviewService.GeneratePreviewAsync(request, ct)
        │
        ▼
ClaudeLayerPreviewService (or IdentityLayerPreviewService) returns LayerPreviewResult
        │
        ▼
Result stored:
  - _previewValues[currentKind] = result.ProposedValues
  - Composer.CapturePreviewAck(currentKind, userPrompt)  →  _previewAcks[currentKind] = templated string
  - LayerStates IState changes (current: Dirty → Previewing)
        │
        ▼
Canvas re-renders with preview values (reads from _previewValues for diff visualization)
ComposerFooter changes to Previewing layout
Acknowledgment line renders below preview-context text
```

### 15.3 Override fall-through in FilesRail

```
FilesRailModel.ActivePreviewContent depends on:
  - Shell.ActiveIndex
  - Intent.Values
  - StackPrefs.Values
  - Design.Tokens
  - Composer.Overrides
  - ViewAllMode

When any of these change, the feed re-evaluates:

  if (viewAll && active == Scaffold):
      return BundleBuilder.BuildPromptContext(...)
  elif (overrides has currentKind):
      return overrides[currentKind]                          // user-edited override wins
  else:
      brief = BriefGenerator.Generate(currentKind, state)    // generator output
      return Renderer.Render(brief)                          // structured markdown
```

---

## 16. Error handling

### 16.1 AI service failures

`ILayerPreviewService.GeneratePreviewAsync` may throw on:
- Network errors (offline, DNS failure)
- API errors (401 unauthorized, 429 rate-limited, 500 server error)
- Timeout (default 60s)
- Cancellation (user navigated away, app suspended)

ShellModel wraps the call in try/catch. On exception:
1. Logs the error at Warning level (with correlation ID, layer kind, prompt fingerprint)
2. Calls `IdentityLayerPreviewService.GeneratePreviewAsync` to get the fallback result
3. Continues the flow with the identity result

**Failures are silent at the UI level in v1.0.** The user sees the preview state activate with empty diff (no proposed changes visible). The acknowledgment line still renders if the user provided a prompt. The flow continues normally.

Future versions may surface failures via a non-blocking notification banner in the composer footer.

### 16.2 Clipboard failures

`Clipboard.SetContent` may fail on:
- Web platforms in non-secure contexts
- Linux without a clipboard daemon
- Permission denied on some platforms

The button's UI confirmation (`✓ Copied` for 1400ms) fires regardless of success. This is a known v1.0 compromise — accurate failure reporting requires a UI overhaul of the button confirmation pattern.

### 16.3 File download failures

`IFileDownloadService.SaveAsync` may fail on:
- Wasm: user cancels the save dialog
- All platforms: insufficient permissions, full disk

The ScaffoldModel.DownloadBundle catches exceptions silently and falls back to copying the bundle content to clipboard. The button label transitions to `✓ Copied (download blocked)` for 1800ms.

### 16.4 Composer edge cases

| Condition                                                | System response                                                                |
|----------------------------------------------------------|--------------------------------------------------------------------------------|
| User types only whitespace                               | `MarkDirty` does NOT fire. Layer remains Clean.                                |
| User clears entire composer textarea (Dirty → empty)     | Layer remains Dirty (state changes are explicit via Discard, not via emptying) |
| User presses Cmd/Ctrl+Enter with empty prompt + clean    | `LockAndContinue` fires                                                        |
| User presses Cmd/Ctrl+Enter with empty prompt + dirty    | `LockAndContinue` fires (preview path requires a prompt)                       |
| User presses Esc with no preview                         | Textarea blurs, no state change                                                |
| User presses Esc while previewing                        | `DiscardPreview` fires                                                         |

### 16.5 Navigation race conditions

If the user clicks `Lock and continue →` while a preview is generating:
1. The pending preview's continuation is canceled via the CancellationToken
2. The lock proceeds with current canonical values (not the not-yet-arrived preview values)
3. The layer state moves directly Dirty → Locked, skipping Previewing

If the user clicks `Reset` while a preview is generating:
1. All cancellation tokens fire
2. Reset proceeds normally
3. The in-flight preview result, if it arrives after reset, is discarded

---

## 17. Performance budgets

### 17.1 Latency targets

| Operation                                          | Target | Rationale                                            |
|----------------------------------------------------|--------|------------------------------------------------------|
| App launch to first visible Stack page             | <2s    | User expects modern app responsiveness              |
| Layer-to-layer navigation                          | <100ms | Page swap should feel instantaneous                  |
| Canvas state edit → visible canvas update          | <16ms  | Single frame budget (60fps)                          |
| Composer prompt typed character → textarea update  | <16ms  | Single frame budget                                  |
| Lock advancement → next page rendered              | <300ms | Includes one navigation + initial canvas render      |
| Rail reveal animation (first lock)                 | 480ms  | Animation duration (intentional)                     |
| ColorPaletteOverride.xaml live re-synthesis        | <50ms  | Cheap string formatting                              |
| MarkdownPreview render of synthesized brief        | <100ms | ~1500 lines parsed and rendered                      |
| Bundle download (concat 9 briefs)                  | <500ms | Concatenation + Blob/SavePicker overhead             |
| Generate Preview (with identity service)           | <50ms  | Just an in-process function call                     |
| Generate Preview (with Claude service)             | <30s   | Single-call architecture; multi-stage is future work |

### 17.2 Memory shape

| Layer                          | Approximate footprint per session   |
|--------------------------------|--------------------------------------|
| ShellModel + all layer models  | ~5–10 MB (records + IStates)         |
| Composer prompts + overrides   | ~2 MB (text only)                    |
| Snapshots dictionary           | ~1–3 MB (point-in-time copies)       |
| Preview values cache           | ~1 MB                                |
| FilesRailModel preview content | ~500 KB (current layer rendered)     |
| UI tree (Skia)                 | ~30–50 MB (typical Uno app baseline) |
| **Total**                      | **~40–70 MB**                        |

### 17.3 Concurrency

The application is fundamentally single-threaded at the UI level (Dispatcher-bound). Background work is limited to:
- `ILayerPreviewService` API calls (HTTP, async)
- `IFileDownloadService` save operations (async)
- Markdown rendering (CPU-bound but fast; runs on UI thread)

All MVUX IState transitions happen on the UI dispatcher.

---

## 18. Testing strategy

### 18.1 Unit tests per model

Each layer model has a dedicated test fixture covering:
- Default initial state
- Each `Update*` method modifies the correct field
- Each `Update*` method triggers `ShellModel.MarkDirty`
- Derived `IFeed` values match expected computations for sample inputs
- Commands flow through `ShellModel` correctly

Example (StackPreferencesModelTests):

```csharp
[Test]
public async Task UpdatePattern_ChangesPattern_MarksDirty()
{
    var shell = new TestShellModel();
    var model = new StackPreferencesModel(shell);

    await model.UpdatePattern(StackPattern.Mvvm);

    var values = await model.Values;
    Assert.That(values.Pattern, Is.EqualTo(StackPattern.Mvvm));
    Assert.That(shell.LastMarkDirtyCall, Is.EqualTo(LayerKind.Stack));
}
```

### 18.2 Service unit tests

- `ContextDeriverTests` — given various IntentValues, verify the returned DerivedContext fields
- `LayerBriefGeneratorTests` — verify section count, code block presence, acceptance criteria per layer
- `MarkdownRendererTests` — verify rendered output matches expected markdown structure
- `BundleBuilderTests` — verify concatenation order, separators, override fall-through

### 18.3 UI tests per page

Each page has at minimum:
- Renders without exception
- All bindings resolve (no `BindingExpression` warnings in test output)
- Primary canvas user control is present
- ComposerFooter is present (except ScaffoldPage)
- Clicking the primary button triggers the expected ShellModel method

### 18.4 Smoke tests (manual or scripted)

The full composition flow:
1. Launch app → land on Stack with example defaults
2. Modify Pattern to MVVM → mark dirty
3. Click `Generate preview →` → preview state activates (with identity service, empty diff)
4. Click `Accept and lock →` → advance to Intent
5. Edit Intent → advance to UX
6. Continue through all 8 layers
7. Reach Scaffold → click `Copy prompt-context.md` → verify clipboard contains 9 layer briefs concatenated
8. Click `Download bundle ↓` → verify file save dialog appears, save succeeds
9. Click `Reset` → confirm → verify returns to Stack with defaults restored

Stack-substitution smoke tests:
- Switch Pattern to MVVM → Architecture brief reflects ViewModels not State (MVUX)
- Switch Markup to C# Markup → Interactions brief references programmatic VSM API
- Add Linux to Platforms → Scaffold command's `--platforms` includes `linux`
- Switch Theme to Cupertino → Design brief's Material slot section becomes Cupertino slot section

### 18.5 Performance tests

- Time the app launch to first visible page
- Time the rail reveal animation (verify it's ~480ms, not significantly more)
- Memory snapshot after 100 layer navigations (verify no leaks)

---

## 19. Acceptance criteria

A v1.0-conformant structural implementation:

### Foundation
- [ ] `Uno.Sdk` 6.5.29 or later pinned in `global.json`
- [ ] All 14 `UnoFeature` entries from §3.2 present in csproj
- [ ] Single Project layout with `Platforms/` subdirectories
- [ ] Embedded fonts under `Assets/Fonts/`
- [ ] `Themes/` directory contains all 9 resource dictionaries

### Layer model
- [ ] `LayerKind` enum has exactly 9 values in canonical order
- [ ] `Layers.All` immutable array matches the canonical layer table
- [ ] `LayerState` enum has 4 values: Clean, Dirty, Previewing, Locked
- [ ] `Layers.Count == 9`

### Models
- [ ] One MVUX model class per layer (9 total)
- [ ] `ShellModel`, `ComposerModel`, `FilesRailModel`, `CompositionStackModel` all exist
- [ ] All models declared as `partial record`
- [ ] All mutable state exposed via `IState<T>`
- [ ] All derived state exposed via `IFeed<T>` or `IListFeed<T>`
- [ ] All commands implemented as public async methods (no explicit `ICommand` declarations)
- [ ] No model has `INotifyPropertyChanged` implementation

### Services
- [ ] `ILayerPreviewService` interface with `IdentityLayerPreviewService` and `ClaudeLayerPreviewService` implementations
- [ ] `IContextDeriver` with `ContextDeriver` implementation containing all 13 domain noun rules
- [ ] `ILayerBriefGenerator` with `LayerBriefGenerator` implementation handling all 9 LayerKinds
- [ ] `IMarkdownRenderer` with `MarkdownRenderer` implementation
- [ ] `IMarkdownGenerator` with `StructuredMarkdownGenerator` wrapper
- [ ] `IBundleBuilder`, `IClipboardService`, `IFileDownloadService` all implemented
- [ ] DI registration in `App.OnLaunched` covers all services and models

### Shell
- [ ] `Shell.xaml` defines three columns with the center column having `uen:Region.Attached="True"` and `uen:Region.Name="ActivePage"`
- [ ] Left and right rails are UserControls in the Shell, NOT inside any Page
- [ ] Column widths bind to ShellModel computed feeds
- [ ] `Shell.xaml.cs` resolves `ShellModel` from DI in constructor

### Navigation
- [ ] 9 page routes registered nested under the Shell route
- [ ] Stack route has `IsDefault: true`
- [ ] No page calls `INavigator` directly from `.xaml.cs`
- [ ] All navigation flows through `ShellModel` methods
- [ ] Navigation uses `Qualifiers.Nested`

### Pages
- [ ] 9 page classes exist under `Views/Pages/`
- [ ] Every page (except Scaffold) follows the 6-slot template from §12.1
- [ ] ScaffoldPage omits Slot 6 (ComposerFooter)
- [ ] Every page's `.xaml.cs` is empty except for `InitializeComponent()`
- [ ] Every page has `x:Uid` on visible/interactive elements

### Canvases
- [ ] 9 canvas user controls exist under `Views/Canvases/`
- [ ] Each canvas binds two-way to its layer model's IStates

### Resource dictionaries
- [ ] Loading order matches §13.1
- [ ] `ColorPaletteOverride.xaml` is live-synthesized from `DesignModel.ColorPaletteOverrideXaml`
- [ ] All component XAML references theme resources by key; no hardcoded hex

### State machine
- [ ] `MarkDirty(kind)` transitions Clean → Dirty and captures snapshot (only on first transition)
- [ ] `GeneratePreview()` calls `ILayerPreviewService` and stores result in `_previewValues`
- [ ] `AcceptPreview()` adopts proposed values into canonical IState
- [ ] `DiscardPreview()` and `DiscardEdits()` restore from snapshot
- [ ] `LockAndContinue()` transitions Clean → Locked and advances ActiveIndex
- [ ] `Revisit(kind)` transitions Locked → Clean preserving values
- [ ] `Reset()` wipes all transient state and navigates to Stack

### Build verification
- [ ] `dotnet build` succeeds with zero warnings
- [ ] Application launches on Windows, macOS, Linux, iOS, Android, and WebAssembly
- [ ] Hot Reload works on every platform with `DOTNET_MODIFIABLE_ASSEMBLIES=debug`

---

## 20. Out of scope

These structural concerns are explicitly NOT addressed in v1.0:

- **Direct manipulation in canvases beyond Stack/Intent/Design** — UX, Architecture, Interactions, Data, Implementation canvases are read-only in v1.0 (composer-prompt edits only)
- **Persistent state across sessions** — closing the app loses the composition
- **Multi-tab/multi-window session sharing** — workspace is single-window only
- **Real-time multi-user collaboration**
- **Authentication and user accounts**
- **Telemetry and analytics** (the `EnableTelemetry` flag is reserved but not implemented)
- **Localization to a second language** (English resw is the only one used in v1.0)
- **Multi-stage AI augmentation pipeline** (outline → content → verify → review → consistency; see ENGINEERING-BRIEF-03)
- **Dark theme** (the system is light-only; the Material Dark resource dictionary entries exist but are not toggled)
- **Right-to-left text support**
- **Accessibility audit beyond `AutomationProperties.Name` on focusable elements**
- **Custom keybinding configuration** (Cmd/Ctrl+Enter and Esc are hardcoded)
- **Cross-brief consistency pass** at Scaffold lock (see ENGINEERING-BRIEF-05)
- **Section-level overrides** in FilesRail (whole-file override is the only granularity)
- **AI-augmented brief regeneration** for layers other than via the preview cycle

The companion `DESIGN-BRIEF-from-scratch.md` covers all visual specifications. The companion `INTERACTION-BRIEF-from-scratch.md` covers all user-facing behaviors. This brief is the structural truth — refer to it for any question about "what is the system made of."

---

## End of Architecture brief

Total scope: 9-layer composition, 1 Shell + 9 Pages + 9 Canvases + 16 reusable Controls, 10 MVUX models, 7 service interfaces with 9 implementations, full DI graph, complete navigation registration, exhaustive state machine.

Implementation order suggested:
1. Stand up the empty Uno project with csproj + global.json + App.xaml.cs hosting bootstrap
2. Create the Models/ directory with all 30+ record/enum definitions
3. Create the Presentation/ directory with all 12 MVUX models (commands can be `throw new NotImplementedException()` initially)
4. Create the Services/ directory with all 7 interfaces and Identity implementations
5. Register everything in DI
6. Create the Shell.xaml with the three-column scaffold
7. Create the 9 Page classes following the canonical template
8. Create the 9 Canvas user controls (empty initially)
9. Create the reusable Controls (ComposerFooter, ActiveLayerHeader, etc.)
10. Wire the routes in RouteMap.cs
11. Verify the app launches and navigates between empty pages
12. Implement visual specifications from the Design Brief
13. Implement behavioral specifications from the Interaction Brief
14. Implement layer-specific generators and ContextDeriver
15. Implement the Architecture and Interactions canvas hover-to-explore visualizations

Architecture is complete. Say "next" for the Design Brief.
