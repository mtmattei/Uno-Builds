# Composer Context Engine — Architecture Brief

**Version:** v11 (canonical)
**Audience:** an implementation agent building this app on Uno Platform with MVUX + Region-based Navigation + Material + Skia renderer. Self-contained — no prerequisite reading.
**Stack:** C# 13, .NET 10, Uno Platform 6.5.29+, MVUX, Region Navigation, Material theme, Kiota HTTP, Skia renderer. Single Project layout with platform heads under `Platforms/`.
**Companion briefs:** `DESIGN-BRIEF-detailed.md` (visual), `INTERACTION-BRIEF-detailed.md` (behavioral). All three describe the same system at different abstraction levels.

This brief specifies *what the app is made of and how it's wired*. Every named class, every IFeed, every service interface, every DI registration, every navigation route is defined here. The implementation agent should be able to scaffold the project structure, wire MVUX models, and register navigation routes without referring to any other document.

---

## 1. Top-level structure

### 1.1 Single Project layout

```
ComposerContextEngine.sln
└── ComposerContextEngine/
    ├── ComposerContextEngine.csproj
    ├── Directory.Packages.props
    ├── App.xaml
    ├── App.xaml.cs                    // Hosting + DI + nav routes registration
    ├── GlobalUsings.cs
    │
    ├── Models/                         // Pure data records — no behavior
    │   ├── IntentValues.cs
    │   ├── DesignTokens.cs
    │   ├── ArchitectureBlueprint.cs
    │   ├── ModuleDef.cs
    │   ├── EdgeDef.cs
    │   ├── UXFlow.cs
    │   ├── ScreenDef.cs
    │   ├── InteractionsMatrix.cs
    │   ├── InteractionFlow.cs
    │   ├── StateDef.cs
    │   ├── DataContracts.cs
    │   ├── EntityDef.cs
    │   ├── FieldDef.cs
    │   ├── BuildPlan.cs
    │   ├── PhaseDef.cs
    │   ├── LayerSnapshot.cs
    │   ├── LayerDef.cs
    │   └── DerivedContext.cs
    │
    ├── Presentation/                   // MVUX models (feeds + states + commands)
    │   ├── ShellModel.cs               // Owns activeIndex, lockedIds, layerStates
    │   ├── IntentModel.cs              // Owns IntentValues IState
    │   ├── DesignModel.cs              // Owns DesignTokens IState
    │   ├── ArchitectureModel.cs        // Owns ArchitectureBlueprint IState
    │   ├── UXModel.cs                  // Owns UXFlow IState
    │   ├── InteractionsModel.cs        // Owns InteractionsMatrix IState
    │   ├── DataModel.cs                // Owns DataContracts IState
    │   ├── ImplementationModel.cs      // Owns BuildPlan IState
    │   ├── ScaffoldModel.cs            // Computes scaffold command + bundle
    │   ├── FilesRailModel.cs           // Owns file statuses + active preview
    │   └── ComposerModel.cs            // Owns per-layer prompts + previewAcks
    │
    ├── Services/                       // Cross-cutting concerns
    │   ├── ILayerPreviewService.cs
    │   ├── ClaudeLayerPreviewService.cs       // Production impl
    │   ├── IdentityLayerPreviewService.cs     // Fallback when no API key
    │   ├── IMarkdownGenerator.cs
    │   ├── MarkdownGenerator.cs
    │   ├── IContextDeriver.cs
    │   ├── ContextDeriver.cs
    │   ├── IBundleBuilder.cs
    │   ├── BundleBuilder.cs
    │   ├── IClipboardService.cs                // Abstracts navigator.clipboard / DataPackage
    │   └── ClipboardService.cs
    │
    ├── Views/                          // XAML pages + user controls
    │   ├── Shell.xaml                  // Three-column workspace
    │   ├── Shell.xaml.cs
    │   ├── Pages/
    │   │   ├── IntentPage.xaml
    │   │   ├── UXPage.xaml
    │   │   ├── ArchitecturePage.xaml
    │   │   ├── DesignPage.xaml
    │   │   ├── InteractionsPage.xaml
    │   │   ├── DataPage.xaml
    │   │   ├── ImplementationPage.xaml
    │   │   └── ScaffoldPage.xaml
    │   ├── Controls/
    │   │   ├── CompositionStackRegion.xaml      // Left rail
    │   │   ├── FilesRailRegion.xaml             // Right rail
    │   │   ├── ComposerFooter.xaml              // Bottom of every page
    │   │   ├── ActiveLayerHeader.xaml
    │   │   ├── LockedContextCard.xaml
    │   │   ├── FuturePreviewCard.xaml
    │   │   ├── ProgressIndicator.xaml
    │   │   ├── StackItem.xaml
    │   │   ├── FileRow.xaml
    │   │   ├── MarkdownPreview.xaml              // Templated control
    │   │   ├── Annotation.xaml                   // Marginalia primitive
    │   │   ├── SectionHeader.xaml
    │   │   ├── CodeBlock.xaml
    │   │   └── BlockHandle.xaml
    │   └── Canvases/                  // Layer-specific canvas user controls
    │       ├── IntentCanvas.xaml
    │       ├── UXFlowStripCanvas.xaml
    │       ├── ArchitectureBlueprintCanvas.xaml
    │       ├── DesignTokenGridCanvas.xaml
    │       ├── StateTransitionDiagramCanvas.xaml
    │       ├── DataContractGridCanvas.xaml
    │       ├── ImplementationPhaseGridCanvas.xaml
    │       └── ScaffoldTerminalCanvas.xaml
    │
    ├── Themes/                         // Resource dictionaries
    │   ├── ColorPaletteOverride.xaml   // Live-synthesized from DesignTokens
    │   ├── ThemeColorOverrides.xaml    // Static semantic colors
    │   ├── Brushes.xaml
    │   ├── Typography.xaml
    │   ├── Editorial.xaml              // Eyebrow/Mono/Body styles
    │   ├── Buttons.xaml
    │   └── TextBlockStyles.xaml
    │
    ├── Navigation/
    │   └── RouteMap.cs                 // Registers all 8 page routes
    │
    ├── Assets/
    │   └── (none required — all chrome is XAML-rendered)
    │
    └── Platforms/
        ├── Android/
        ├── iOS/
        ├── MacCatalyst/
        ├── Desktop/
        ├── Wasm/
        └── Windows/
```

### 1.2 csproj configuration

```xml
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
  </PropertyGroup>

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
  </ItemGroup>
</Project>
```

`Uno.Sdk` version pinned to **6.5.29** in `global.json`.

### 1.3 App.xaml.cs hosting bootstrap

```csharp
public partial class App : Application
{
    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
                .UseLogging(configure: (context, logBuilder) => logBuilder
                    .SetMinimumLevel(LogLevel.Information))
                .UseConfiguration(configure: configBuilder => configBuilder
                    .EmbeddedSource<App>()
                    .Section<AppConfig>())
                .UseHttp((context, services) => services
                    .AddKiotaClient<ClaudeApiClient>(context, "Claude"))
                .UseThemeSwitching()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IContextDeriver, ContextDeriver>();
                    services.AddSingleton<IMarkdownGenerator, MarkdownGenerator>();
                    services.AddSingleton<IBundleBuilder, BundleBuilder>();
                    services.AddSingleton<IClipboardService, ClipboardService>();

                    // Preview service registration with feature flag:
                    // identity fallback when no API key configured.
                    services.AddSingleton<ILayerPreviewService>(sp =>
                    {
                        var cfg = sp.GetRequiredService<IConfiguration>();
                        var apiKey = cfg["AppConfig:AnthropicApiKey"];
                        if (string.IsNullOrWhiteSpace(apiKey))
                            return new IdentityLayerPreviewService();
                        return new ClaudeLayerPreviewService(
                            sp.GetRequiredService<ClaudeApiClient>(),
                            sp.GetRequiredService<ILogger<ClaudeLayerPreviewService>>());
                    });

                    services.AddTransient<ShellModel>();
                    services.AddTransient<IntentModel>();
                    services.AddTransient<DesignModel>();
                    services.AddTransient<ArchitectureModel>();
                    services.AddTransient<UXModel>();
                    services.AddTransient<InteractionsModel>();
                    services.AddTransient<DataModel>();
                    services.AddTransient<ImplementationModel>();
                    services.AddTransient<ScaffoldModel>();
                    services.AddTransient<FilesRailModel>();
                    services.AddTransient<ComposerModel>();
                })
                .UseNavigation(RegisterRoutes));

        MainWindow = builder.Window;
        Host = builder.Build();
        MainWindow.Content = new Shell();
        MainWindow.Activate();
    }
}
```

### 1.4 Route registration

`Navigation/RouteMap.cs`:

```csharp
public static class RouteMap
{
    public static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<IntentPage, IntentModel>(),
            new ViewMap<UXPage, UXModel>(),
            new ViewMap<ArchitecturePage, ArchitectureModel>(),
            new ViewMap<DesignPage, DesignModel>(),
            new ViewMap<InteractionsPage, InteractionsModel>(),
            new ViewMap<DataPage, DataModel>(),
            new ViewMap<ImplementationPage, ImplementationModel>(),
            new ViewMap<ScaffoldPage, ScaffoldModel>());

        routes.Register(new RouteMap("", View: views.FindByViewModel<ShellModel>(),
            Nested: new[]
            {
                new RouteMap("Intent",         View: views.FindByViewModel<IntentModel>(),         IsDefault: true),
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

The 8 page routes are siblings under a single Shell route. Each page is nested inside the Shell's `ActivePageRegion`.

---

## 2. The eight pages (layer canvases)

Each "layer" in the composition is a **page** in the navigation graph. The shell hosts exactly one page at a time inside its center column. The left rail (Composition Stack) and right rail (Files Rail) are sibling regions that do not change as the active page changes.

### 2.1 Canonical layer table

| Index | Route name      | Page class                | Model class             | Canvas user control                  | Output file              |
|-------|----------------|---------------------------|--------------------------|--------------------------------------|--------------------------|
| 01    | Intent         | `IntentPage`              | `IntentModel`            | `IntentCanvas`                       | `README.md`              |
| 02    | UX             | `UXPage`                  | `UXModel`                | `UXFlowStripCanvas`                  | `ux-flows.md`            |
| 03    | Architecture   | `ArchitecturePage`        | `ArchitectureModel`      | `ArchitectureBlueprintCanvas`        | `architecture.md`        |
| 04    | Design         | `DesignPage`              | `DesignModel`            | `DesignTokenGridCanvas`              | `design-system.md`       |
| 05    | Interactions   | `InteractionsPage`        | `InteractionsModel`      | `StateTransitionDiagramCanvas`       | `interaction-spec.md`    |
| 06    | Data           | `DataPage`                | `DataModel`              | `DataContractGridCanvas`             | `data-contracts.md`      |
| 07    | Implementation | `ImplementationPage`      | `ImplementationModel`    | `ImplementationPhaseGridCanvas`      | `implementation-plan.md` |
| 08    | Scaffold       | `ScaffoldPage`            | `ScaffoldModel`          | `ScaffoldTerminalCanvas`             | `scaffold.command`       |
| —     | —              | —                         | —                        | (synthesized at Scaffold lock)       | `prompt-context.md`      |

This table is the canonical source of truth for layer order and identity. The `LayerDef` record in `Models/LayerDef.cs` mirrors it as runtime data:

```csharp
public record LayerDef(
    LayerKind Kind,
    string Label,
    string OutputFile,
    string Hint,
    string Route);

public enum LayerKind
{
    Intent = 0,
    UX = 1,
    Architecture = 2,
    DesignSystem = 3,
    Interactions = 4,
    Data = 5,
    Implementation = 6,
    Scaffold = 7,
}

public static class Layers
{
    public static readonly ImmutableArray<LayerDef> All = ImmutableArray.Create(
        new LayerDef(LayerKind.Intent,         "Intent",         "README.md",              "what the app is for",          "Intent"),
        new LayerDef(LayerKind.UX,             "UX",             "ux-flows.md",            "how users move through it",     "UX"),
        new LayerDef(LayerKind.Architecture,   "Architecture",   "architecture.md",        "how it is shaped",              "Architecture"),
        new LayerDef(LayerKind.DesignSystem,   "Design System",  "design-system.md",       "how it feels",                  "Design"),
        new LayerDef(LayerKind.Interactions,   "Interactions",   "interaction-spec.md",    "every state of every flow",     "Interactions"),
        new LayerDef(LayerKind.Data,           "Data",           "data-contracts.md",      "shapes and contracts",          "Data"),
        new LayerDef(LayerKind.Implementation, "Implementation", "implementation-plan.md", "phased build plan",             "Implementation"),
        new LayerDef(LayerKind.Scaffold,       "Scaffold",       "scaffold.command",       "runnable starting point",       "Scaffold"));
}
```

### 2.2 Page structure pattern

Every page follows the same XAML layout pattern. The shell provides the three-column scaffold; each page populates only the **center column content** via its Region binding.

```xml
<!-- Pages/IntentPage.xaml (example — all 8 pages follow this shape) -->
<Page x:Class="ComposerContextEngine.Views.Pages.IntentPage"
      xmlns:utu="using:Uno.Toolkit.UI"
      xmlns:canvases="using:ComposerContextEngine.Views.Canvases"
      xmlns:controls="using:ComposerContextEngine.Views.Controls">

    <ScrollViewer Padding="32,32,48,80">
        <utu:AutoLayout Orientation="Vertical" Spacing="0">

            <!-- 1. Progress indicator (always at top of page content) -->
            <controls:ProgressIndicator
                ActiveIndex="{Binding ActiveIndex}"
                Total="{Binding LayerCount}" />

            <!-- 2. App title row -->
            <controls:AppTitleRow />

            <!-- 3. Locked context cards (rendered for all locked layers
                 below the current index — controlled by Shell context) -->
            <ItemsRepeater ItemsSource="{Binding LockedCards}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:LockedContextCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>

            <!-- 4. Active layer header (recap + index + title + subtitle) -->
            <controls:ActiveLayerHeader />

            <!-- 5. Layer-specific canvas (the only thing that varies per page) -->
            <canvases:IntentCanvas />

            <!-- 6. Composer footer (always at bottom of page content) -->
            <controls:ComposerFooter />

            <!-- 7. Future preview cards (rendered for all upcoming layers) -->
            <ItemsRepeater ItemsSource="{Binding FutureCards}">
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

The only difference between pages is the canvas element (item 5). Items 1, 2, 3, 4, 6, 7 are identical structure across all pages.

---

## 3. Shell — three-column workspace

### 3.1 Shell.xaml structure

```xml
<UserControl x:Class="ComposerContextEngine.Views.Shell"
             xmlns:uen="using:Uno.Extensions.Navigation.UI"
             xmlns:controls="using:ComposerContextEngine.Views.Controls"
             Background="{ThemeResource BackgroundBrush}">

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="{Binding LeftRailWidth, Mode=OneWay}" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="{Binding RightRailWidth, Mode=OneWay}" />
        </Grid.ColumnDefinitions>

        <!-- LEFT: Composition Stack (named region — fixed content) -->
        <controls:CompositionStackRegion
            Grid.Column="0"
            uen:Region.Attached="True"
            uen:Region.Name="LeftRail"
            Opacity="{Binding LeftRailOpacity, Mode=OneWay}"
            Visibility="{Binding LeftRailVisibility, Mode=OneWay}" />

        <!-- CENTER: active page region (one of the 8 layer pages) -->
        <Grid Grid.Column="1"
              uen:Region.Attached="True"
              uen:Region.Name="ActivePage"
              MaxWidth="{Binding CenterMaxWidth, Mode=OneWay}"
              HorizontalAlignment="Center" />

        <!-- RIGHT: Files Rail (named region — fixed content) -->
        <controls:FilesRailRegion
            Grid.Column="2"
            uen:Region.Attached="True"
            uen:Region.Name="RightRail"
            Opacity="{Binding RightRailOpacity, Mode=OneWay}"
            Visibility="{Binding RightRailVisibility, Mode=OneWay}" />
    </Grid>
</UserControl>
```

### 3.2 Shell column-width binding contract

`ShellModel` exposes these computed feeds:

```csharp
public IFeed<GridLength> LeftRailWidth   => RailsVisible.Select(v => v ? new GridLength(260) : new GridLength(0));
public IFeed<GridLength> RightRailWidth  => RailsVisible.Select(v => v ? new GridLength(340) : new GridLength(0));
public IFeed<double> LeftRailOpacity     => RailsVisible.Select(v => v ? 1.0 : 0.0);
public IFeed<double> RightRailOpacity    => RailsVisible.Select(v => v ? 1.0 : 0.0);
public IFeed<Visibility> LeftRailVisibility  => RailsVisible.Select(v => v ? Visibility.Visible : Visibility.Collapsed);
public IFeed<Visibility> RightRailVisibility => RailsVisible.Select(v => v ? Visibility.Visible : Visibility.Collapsed);
public IFeed<double> CenterMaxWidth      => RailsVisible.Select(v => v ? 880.0 : 720.0);

public IFeed<bool> RailsVisible => Feed.Combine(ActiveIndex, LockedIds).Select(t =>
    t.Item2.Count > 0 || t.Item1 > 0);
```

`RailsVisible` is the load-bearing computation. When `activeIndex == 0` AND `lockedIds.Count == 0`, both rails collapse to width 0 and the center column tightens to 720px. This is the **focused first-screen** state — a brand new user lands on Intent with no chrome competing.

### 3.3 Rail reveal animation contract

The transitions between collapsed and expanded rail states animate via Storyboard:

| Property                            | From → To       | Duration | Easing       | Delay  |
|-------------------------------------|-----------------|----------|--------------|--------|
| `LeftRailWidth`                     | 0 → 260         | 480ms    | `RailReveal` | 0      |
| `RightRailWidth`                    | 0 → 340         | 480ms    | `RailReveal` | 0      |
| `LeftRailOpacity` / `RightRailOpacity` | 0 → 1        | 320ms    | `EaseInOut`  | 160ms  |
| `CenterMaxWidth`                    | 720 → 880       | 480ms    | `RailReveal` | 0      |

`RailReveal` easing is defined in `App.xaml` as `CubicEase` with `EasingMode="EaseOut"` and `Power="5"` (approximates the v8 prototype's `cubic-bezier(0.16, 1, 0.3, 1)` JavaScript curve).

The 160ms delay on opacity ensures rails *slide in first*, then their content fades on. Without the delay rails would feel like they appear; with it, they read as opening up.

---

## 4. ShellModel — the orchestrator

`ShellModel` owns the cross-cutting state that every page depends on. It is the only model whose lifetime spans all 8 pages.

### 4.1 IState ownership

```csharp
public partial record ShellModel
{
    private readonly INavigator _navigator;
    private readonly ILayerPreviewService _previewService;
    private readonly IContextDeriver _contextDeriver;
    private readonly IMarkdownGenerator _markdownGenerator;

    // Active layer index — 0..7. Drives which page is rendered in the
    // ActivePage region.
    public IState<int> ActiveIndex => State.Value(this, () => 0);

    // Set of layers that have been locked. ImmutableHashSet keeps
    // reference equality cheap for change detection in derived feeds.
    public IState<ImmutableHashSet<LayerKind>> LockedIds =>
        State.Value(this, () => ImmutableHashSet<LayerKind>.Empty);

    // Per-layer state machine values. Initial = all clean.
    public IState<ImmutableDictionary<LayerKind, LayerState>> LayerStates =>
        State.Value(this, () => Layers.All
            .ToImmutableDictionary(l => l.Kind, _ => LayerState.Clean));

    // One-shot toast — "you can revisit any layer" — fired after first lock.
    public IState<bool> RevisitHintShown => State.Value(this, () => false);
    public IState<bool> RevisitHintDismissed => State.Value(this, () => false);

    // Feature flag — true when ILayerPreviewService is not the identity fallback.
    public IFeed<bool> AiConfigured => Feed.Async(async ct =>
        await _previewService.IsConfiguredAsync(ct));
}
```

### 4.2 LayerState enum

```csharp
public enum LayerState
{
    Clean,        // No edits since locked or initial. Lock-and-continue is the action.
    Dirty,        // User edited canvas or typed in prompt. Generate-preview is the action.
    Previewing,   // AI rendered proposed redraw. Accept-or-discard.
    Locked,       // Terminal. Layer settled, file drafted, advanced past.
}
```

State transitions are enforced by ShellModel methods (not free assignment):

```csharp
public ValueTask MarkDirty(LayerKind kind);            // clean → dirty, capture snapshot
public ValueTask GeneratePreview();                    // dirty → previewing, capture ack
public ValueTask AcceptPreview();                      // previewing → locked + advance
public ValueTask DiscardPreview();                     // previewing → clean, restore snapshot
public ValueTask DiscardEdits();                       // dirty → clean, restore snapshot
public ValueTask LockAndContinue();                    // clean → locked + advance
public ValueTask Revisit(LayerKind kind);              // locked → clean, set activeIndex
public ValueTask Reset();                              // all → initial state
```

### 4.3 Snapshot contract

When a layer transitions clean → dirty, ShellModel captures a snapshot of every layer model's mutable state. On discard, the snapshot is restored:

```csharp
public record LayerSnapshot(
    IntentValues? Intent,
    DesignTokens? Design,
    ArchitectureBlueprint? Architecture,
    UXFlow? UX,
    InteractionsMatrix? Interactions,
    DataContracts? Data,
    BuildPlan? Implementation,
    ImmutableDictionary<LayerKind, string>? OverrideMarkdown);

private readonly Dictionary<LayerKind, LayerSnapshot> _snapshots = new();
```

Only the layer's own data is captured into its snapshot — other layers' state is not touched. The dictionary is private (not an IState) because it doesn't drive UI.

### 4.4 Lock advancement

`LockAndContinue` and `AcceptPreview` both follow the same advancement rule:

```csharp
private async ValueTask AdvanceAfterLock(LayerKind kind)
{
    var locked = await LockedIds;
    var nextLocked = locked.Add(kind);
    await LockedIds.SetAsync(nextLocked);

    var currentIdx = await ActiveIndex;
    if (currentIdx < Layers.All.Length - 1)
    {
        await ActiveIndex.SetAsync(currentIdx + 1);
        // Navigate to the next layer's page via the ActivePage region
        await _navigator.NavigateRouteAsync(this,
            Layers.All[currentIdx + 1].Route,
            qualifier: Qualifiers.Nested);
    }

    // One-shot revisit hint after first lock
    if (locked.Count == 0)
        await RevisitHintShown.SetAsync(true);
}
```

`Revisit(kind)` does the inverse — sets `ActiveIndex` to the kind's index and transitions its `LayerState` from `Locked` back to `Clean` (values preserved):

```csharp
public async ValueTask Revisit(LayerKind kind)
{
    var idx = (int)kind;
    await ActiveIndex.SetAsync(idx);

    var states = await LayerStates;
    if (states[kind] == LayerState.Locked)
    {
        await LayerStates.SetAsync(states.SetItem(kind, LayerState.Clean));
    }

    await _navigator.NavigateRouteAsync(this,
        Layers.All[idx].Route,
        qualifier: Qualifiers.Nested);
}
```

---

## 5. Layer models — data ownership

Each layer has a dedicated MVUX model. The model owns the layer's canvas state and exposes commands the canvas binds to.

### 5.1 IntentModel

```csharp
public record IntentValues(
    string AppType,
    string PrimaryUser,
    string Workflow,
    string Platforms,
    string Notes);

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

    public async ValueTask UpdateField(string fieldName, string value)
    {
        var current = await Values;
        var updated = fieldName switch
        {
            nameof(IntentValues.AppType)     => current with { AppType = value },
            nameof(IntentValues.PrimaryUser) => current with { PrimaryUser = value },
            nameof(IntentValues.Workflow)    => current with { Workflow = value },
            nameof(IntentValues.Platforms)   => current with { Platforms = value },
            nameof(IntentValues.Notes)       => current with { Notes = value },
            _ => current,
        };
        await Values.SetAsync(updated);
        await Shell.MarkDirty(LayerKind.Intent);
    }

    public async ValueTask ClearAll()
    {
        await Values.SetAsync(new IntentValues("", "", "", "", ""));
        await Shell.MarkDirty(LayerKind.Intent);
    }
}
```

### 5.2 DesignModel

```csharp
public record DesignTokens(
    Color Surface,        // #0C0D0F
    Color Action,         // #C89C3F — only saturated hue
    Color Info,           // #7AB3DF
    Color Success,        // #9BBF9D
    Color Warn,           // #E87F6D
    Color Panel,          // #16181C
    Color Tag,            // #B288C4
    Color Locked,         // #0C0D0F
    string BodyFont);     // "Inter" | "Newsreader" | "Fraunces"

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

    // Live-synthesized XAML — updates immediately on any token change
    public IFeed<string> ColorPaletteOverrideXaml => Tokens.Select(BuildXaml);

    public async ValueTask UpdateToken(string tokenName, object value)
    {
        var current = await Tokens;
        var updated = tokenName switch
        {
            nameof(DesignTokens.Surface)  => current with { Surface  = (Color)value },
            nameof(DesignTokens.Action)   => current with { Action   = (Color)value },
            nameof(DesignTokens.Info)     => current with { Info     = (Color)value },
            nameof(DesignTokens.Success)  => current with { Success  = (Color)value },
            nameof(DesignTokens.Warn)     => current with { Warn     = (Color)value },
            nameof(DesignTokens.Panel)    => current with { Panel    = (Color)value },
            nameof(DesignTokens.Tag)      => current with { Tag      = (Color)value },
            nameof(DesignTokens.Locked)   => current with { Locked   = (Color)value },
            nameof(DesignTokens.BodyFont) => current with { BodyFont = (string)value },
            _ => current,
        };
        await Tokens.SetAsync(updated);
        await Shell.MarkDirty(LayerKind.DesignSystem);
    }

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

### 5.3 ArchitectureModel

```csharp
public record ModuleDef(
    string Id,
    string Label,
    Point Position,           // x, y on the 800x340 viewBox
    Color Color,
    string Description,
    int Files);               // Count for hover badge

public record EdgeDef(
    string FromId,
    string ToId,
    string Label);

public record ArchitectureBlueprint(
    ImmutableList<ModuleDef> Modules,
    ImmutableList<EdgeDef> Edges);

public partial record ArchitectureModel(ShellModel Shell, IntentModel Intent, IContextDeriver ContextDeriver)
{
    public IFeed<ArchitectureBlueprint> Blueprint => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildBaseline(ctx);
    });

    public IState<string?> HoveredModuleId => State.Value(this, () => (string?)null);

    public async ValueTask RegenerateBlueprint()
    {
        // Bypasses the dirty-state check — explicit re-roll via AI
        await Shell.GeneratePreview();
    }

    private static ArchitectureBlueprint BuildBaseline(DerivedContext ctx)
    {
        // ... see deriveArchitecture in section 8 ...
    }
}
```

### 5.4 UXModel

```csharp
public record ScreenDef(
    string Name,
    string Note,
    ImmutableList<string> MockBlocks);  // Visual placeholder widths

public record UXFlow(
    string FlowName,
    ImmutableList<ScreenDef> Screens);

public partial record UXModel(ShellModel Shell, IntentModel Intent, IContextDeriver ContextDeriver)
{
    public IFeed<UXFlow> Flow => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildFlow(ctx);
    });

    // Static — no mutable state in v11
    private static UXFlow BuildFlow(DerivedContext ctx) { /* ... */ }
}
```

### 5.5 InteractionsModel

```csharp
public enum StateKind
{
    Default = 0,    // Baseline / first paint
    Loading = 1,    // Async work in progress
    Empty = 2,      // No data yet, not an error
    Error = 3,      // User or system error, recoverable
    Success = 4,    // Confirmation, terminal positive
    Offline = 5,    // Connection unavailable, queued or read-only
}

public record StateDef(
    StateKind Kind,
    string Label,
    Point Position,           // On 800x340 viewBox
    Color Color,
    string Description);

public record TransitionDef(
    string FromId,
    string ToId,
    string Label,
    string Path,              // SVG path string — hand-tuned
    Point LabelPosition);

public record InteractionFlow(
    string Id,                // "create-job" — stable for VSM mapping
    string Label,             // "Create job" or "Create habit" (ctx-derived)
    ImmutableList<StateDef> States);

public record InteractionsMatrix(
    ImmutableList<InteractionFlow> Flows);

public partial record InteractionsModel(ShellModel Shell, IntentModel Intent, IContextDeriver ContextDeriver)
{
    public IFeed<InteractionsMatrix> Matrix => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildMatrix(ctx);
    });

    // Local view state — not part of the matrix data
    public IState<string> ActiveFlowId => State.Value(this, () => "create-job");
    public IState<StateKind> ActiveStateKind => State.Value(this, () => StateKind.Default);
    public IState<string?> HoveredStateId => State.Value(this, () => (string?)null);

    public async ValueTask SetActiveFlow(string flowId)
    {
        await ActiveFlowId.SetAsync(flowId);
        await ActiveStateKind.SetAsync(StateKind.Default);  // reset on flow switch
    }
}
```

### 5.6 DataModel

```csharp
public record FieldDef(string Name, string TypeText);
public record EntityDef(string Name, EntityKind Kind, ImmutableList<FieldDef> Fields);
public enum EntityKind { Record, Class, Struct }
public record DataContracts(ImmutableList<EntityDef> Entities);

public partial record DataModel(ShellModel Shell, IntentModel Intent, IContextDeriver ContextDeriver)
{
    public IFeed<DataContracts> Contracts => Intent.Values.Select(intent =>
    {
        var ctx = ContextDeriver.Derive(intent);
        return BuildContracts(ctx);
    });

    public IFeed<string> PrimaryRecordCSharp => Contracts.Select(BuildRecordCode);
}
```

### 5.7 ImplementationModel

```csharp
public record PhaseDef(
    int Number,
    string Label,                        // "Scaffold", "Shell", etc.
    string Title,                        // "Solution skeleton"
    string Description,
    Color Accent,
    ImmutableList<string> Files,
    string AgentPrompt,
    string After);                       // Dependency phase label or "—"

public record BuildPlan(ImmutableList<PhaseDef> Phases);

public partial record ImplementationModel(ShellModel Shell, IntentModel Intent)
{
    public IFeed<BuildPlan> Plan => Intent.Values.Select(_ => BuildStaticPlan());
    // Static plan in v11 — phases don't yet derive from Intent
}
```

### 5.8 ScaffoldModel

```csharp
public partial record ScaffoldModel(
    ShellModel Shell,
    IntentModel Intent,
    DesignModel Design,
    ComposerModel Composer,
    IMarkdownGenerator MarkdownGenerator,
    IBundleBuilder BundleBuilder,
    IClipboardService Clipboard)
{
    public IFeed<string> ScaffoldCommand => Intent.Values.Select(BuildCommand);

    public IFeed<string> PromptContextMarkdown =>
        Feed.Combine(Intent.Values, Design.Tokens, Composer.Overrides)
            .Select(t => BundleBuilder.BuildPromptContext(t.Item1, t.Item2, t.Item3));

    public async ValueTask DownloadBundle()
    {
        var content = await BundleBuilder.BuildFullBundleAsync(/* ... */);
        var fileName = $"{AppName(await Intent.Values)}-bundle.md";
        await Shell.SaveFileAsync(fileName, content);
        await Shell.LockAndContinue();   // Locks scaffold layer
    }

    public async ValueTask CopyPromptContextToClipboard()
    {
        var content = await PromptContextMarkdown;
        await Clipboard.SetTextAsync(content);
    }

    public async ValueTask CopyScaffoldCommandToClipboard()
    {
        var content = await ScaffoldCommand;
        await Clipboard.SetTextAsync(content);
    }
}
```

### 5.9 ComposerModel

```csharp
public partial record ComposerModel(ShellModel Shell, IMarkdownGenerator MarkdownGenerator)
{
    // Per-layer composer textarea content. Empty string per layer initially.
    public IState<ImmutableDictionary<LayerKind, string>> Prompts =>
        State.Value(this, () => Layers.All
            .ToImmutableDictionary(l => l.Kind, _ => ""));

    // Per-layer user-edited markdown overrides. Absent key = use generator.
    public IState<ImmutableDictionary<LayerKind, string>> Overrides =>
        State.Value(this, () => ImmutableDictionary<LayerKind, string>.Empty);

    // Per-layer acknowledgment lines captured at preview time.
    public IState<ImmutableDictionary<LayerKind, string?>> PreviewAcks =>
        State.Value(this, () => Layers.All
            .ToImmutableDictionary<LayerDef, LayerKind, string?>(l => l.Kind, _ => null));

    public async ValueTask UpdatePrompt(LayerKind kind, string text)
    {
        var prompts = await Prompts;
        await Prompts.SetAsync(prompts.SetItem(kind, text));
        if (!string.IsNullOrWhiteSpace(text))
        {
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
        // Template: "You asked: '<first sentence>' — here's what changes if I apply that."
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

### 5.10 FilesRailModel

```csharp
public enum FileStatus
{
    Planned,    // Layer hasn't been touched yet
    Writing,    // Layer is currently active
    Drafted,    // Layer is locked, file has been written
}

public record FileRowData(
    string FileName,
    FileStatus Status);

public partial record FilesRailModel(
    ShellModel Shell,
    IntentModel Intent,
    DesignModel Design,
    ComposerModel Composer,
    IMarkdownGenerator MarkdownGenerator,
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
            // Synthesized file appears at the end, drafted only when scaffold locked
            var promptContextStatus = locked.Contains(LayerKind.Scaffold)
                ? FileStatus.Drafted : FileStatus.Planned;
            return rows.Add(new FileRowData("prompt-context.md", promptContextStatus));
        });

    private static FileStatus ResolveStatus(LayerDef layer, int activeIdx, ImmutableHashSet<LayerKind> locked)
    {
        if (locked.Contains(layer.Kind)) return FileStatus.Drafted;
        if ((int)layer.Kind == activeIdx) return FileStatus.Writing;
        return FileStatus.Planned;
    }

    public IFeed<string> ActivePreviewContent =>
        Feed.Combine(Shell.ActiveIndex, Intent.Values, Design.Tokens,
                     Composer.Overrides, ViewAllMode).Select(t =>
    {
        var (idx, intent, design, overrides, viewAll) = t;
        var activeLayer = Layers.All[idx];
        if (viewAll && activeLayer.Kind == LayerKind.Scaffold)
            return BundleBuilder.BuildPromptContext(intent, design, overrides);
        if (overrides.TryGetValue(activeLayer.Kind, out var ov))
            return ov;
        return MarkdownGenerator.Generate(activeLayer.Kind, intent, design);
    });

    public IFeed<bool> CanViewAll =>
        Feed.Combine(Shell.ActiveIndex, Shell.LockedIds).Select(t =>
            (LayerKind)t.Item1 == LayerKind.Scaffold && t.Item2.Count >= Layers.All.Length - 1);

    public async ValueTask ToggleEditing() => await EditingMode.SetAsync(!await EditingMode);
    public async ValueTask ToggleViewAll() => await ViewAllMode.SetAsync(!await ViewAllMode);

    public async ValueTask CopyActiveContentToClipboard()
    {
        var content = await ActivePreviewContent;
        await Clipboard.SetTextAsync(content);
    }
}
```

---

## 6. Services

### 6.1 ILayerPreviewService

The AI augmentation contract. Implementations: `ClaudeLayerPreviewService` (production), `IdentityLayerPreviewService` (fallback when no API key).

```csharp
public record LayerPreviewRequest(
    LayerKind Kind,
    object CurrentValues,                   // Type varies by kind
    string UserPrompt,
    ImmutableDictionary<LayerKind, string> LockedContextSummaries);

public record LayerPreviewResult(
    object ProposedValues,                  // Same type as request.CurrentValues
    string Summary);                        // One-line italic-serif headline

public interface ILayerPreviewService
{
    bool IsConfigured { get; }
    Task<bool> IsConfiguredAsync(CancellationToken ct = default);
    Task<LayerPreviewResult> GeneratePreviewAsync(LayerPreviewRequest request, CancellationToken ct = default);
}

public sealed class IdentityLayerPreviewService : ILayerPreviewService
{
    public bool IsConfigured => false;
    public Task<bool> IsConfiguredAsync(CancellationToken ct = default) => Task.FromResult(false);
    public Task<LayerPreviewResult> GeneratePreviewAsync(LayerPreviewRequest req, CancellationToken ct = default)
        => Task.FromResult(new LayerPreviewResult(
            ProposedValues: req.CurrentValues,
            Summary: "Showing your edits as proposed."));
}
```

Implementations must:
- Return `ProposedValues` of the same runtime type as `CurrentValues`
- Never throw on AI failures — return an `IdentityLayerPreviewService` result instead
- Cancel cleanly on the passed `CancellationToken`

### 6.2 IContextDeriver

Pure extraction of domain context from Intent. No I/O.

```csharp
public record DerivedContext(
    string AppName,         // PascalCase, filename-safe ("HabitTracker")
    string EntityNoun,      // Lowercase singular ("habit")
    string EntityTitle,     // Capitalized ("Habit")
    string EntityPlural,    // Naive plural ("habits")
    string UserNoun,        // Last word of primaryUser, lowercase
    bool IsOfflineFirst,    // Mentions offline/local/queue
    bool IsMobileFirst,     // Mentions mobile/phone/tablet
    bool IsFieldService);   // Still showing the example default

public interface IContextDeriver
{
    DerivedContext Derive(IntentValues intent);
}

public sealed class ContextDeriver : IContextDeriver
{
    private static readonly (Regex Match, string Noun)[] Rules = new[]
    {
        (new Regex(@"habit|streak", RegexOptions.IgnoreCase),               "habit"),
        (new Regex(@"recipe|cook|meal", RegexOptions.IgnoreCase),           "recipe"),
        (new Regex(@"workout|exercise|fitness", RegexOptions.IgnoreCase),   "workout"),
        (new Regex(@"trade|portfolio|invest|stock", RegexOptions.IgnoreCase), "trade"),
        (new Regex(@"task|todo|backlog", RegexOptions.IgnoreCase),          "task"),
        (new Regex(@"note|journal|diary", RegexOptions.IgnoreCase),         "note"),
        (new Regex(@"appointment|booking|reserv", RegexOptions.IgnoreCase), "appointment"),
        (new Regex(@"patient|medical|health|clinic", RegexOptions.IgnoreCase), "patient"),
        (new Regex(@"invoice|billing|payment", RegexOptions.IgnoreCase),    "invoice"),
        (new Regex(@"order|purchase|cart", RegexOptions.IgnoreCase),        "order"),
        (new Regex(@"ticket|incident|issue", RegexOptions.IgnoreCase),      "ticket"),
        (new Regex(@"lesson|class|course", RegexOptions.IgnoreCase),        "lesson"),
        (new Regex(@"dispatch|field-service|job|service-call", RegexOptions.IgnoreCase), "job"),
    };

    public DerivedContext Derive(IntentValues intent)
    {
        var blob = string.Join(' ', new[] { intent.AppType, intent.Workflow, intent.PrimaryUser }
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

### 6.3 IMarkdownGenerator

Per-layer markdown synthesis from current canvas state.

```csharp
public interface IMarkdownGenerator
{
    string Generate(LayerKind kind, IntentValues intent, DesignTokens design);
}
```

The generator owns the format strings for all 8 layer files. Implementations interpolate live state for Intent and Design, return static templated content for the other 6 layers. Templates are documented inline in `MarkdownGenerator.cs` and mirrored in `DESIGN-BRIEF-detailed.md` §16 for cross-reference.

### 6.4 IBundleBuilder

```csharp
public interface IBundleBuilder
{
    string BuildPromptContext(
        IntentValues intent,
        DesignTokens design,
        ImmutableDictionary<LayerKind, string> overrides);

    Task<byte[]> BuildFullBundleAsync(
        IntentValues intent,
        DesignTokens design,
        ImmutableDictionary<LayerKind, string> overrides,
        string scaffoldCommand,
        CancellationToken ct = default);
}
```

`BuildPromptContext` concatenates each layer's markdown (override if set, generator output otherwise) with `<!-- {filename} -->` separators. `BuildFullBundleAsync` wraps the prompt context with a bundle header and trailing scaffold command, returning the bytes ready for download.

### 6.5 IClipboardService

```csharp
public interface IClipboardService
{
    Task SetTextAsync(string text, CancellationToken ct = default);
    Task<string?> GetTextAsync(CancellationToken ct = default);
}
```

WinUI implementation uses `DataPackage` + `Clipboard.SetContent`. WASM implementation uses `navigator.clipboard.writeText` via JS interop.

---

## 7. Navigation graph

Navigation between the 8 layer pages is **driven by Shell**, not by page code-behind. The user does not navigate by clicking a "go to next page" link — they `Lock and continue →` (or `Accept and lock →`) which calls `ShellModel.AdvanceAfterLock`, which in turn calls `INavigator.NavigateRouteAsync` to swap the page in the ActivePage region.

### 7.1 Navigation triggers (exhaustive)

| Trigger                                   | Navigates to                          | Side effects                          |
|-------------------------------------------|---------------------------------------|---------------------------------------|
| `Continue →` (first layer only)           | Next layer's route                    | Layer becomes Locked                  |
| `Lock and continue →`                     | Next layer's route                    | Layer becomes Locked                  |
| `Accept and lock →`                       | Next layer's route                    | Layer becomes Locked + previewValues adopted |
| `Revisit ↗` on a LockedContextCard        | That layer's route                    | That layer's state goes Locked → Clean |
| Click a Locked layer in CompositionStack  | That layer's route                    | That layer's state goes Locked → Clean |
| `Reset`                                   | Intent route                          | All state reset to initial            |

There are no other navigation triggers. The user cannot navigate to a future layer; future entries in CompositionStack are non-interactive (cursor: default).

### 7.2 ActivePage region binding

The center column region binds to the active layer via name and qualifier-nested route resolution. The binding is configured in `Shell.xaml.cs` via the navigation extensions:

```csharp
public Shell()
{
    InitializeComponent();
    this.Loaded += async (s, e) =>
    {
        var navigator = this.Navigator();
        await navigator.NavigateRouteAsync(this, route: "Intent",
            qualifier: Qualifiers.Nested);
    };
}
```

After initial Intent navigation, all subsequent navigation comes from ShellModel methods.

---

## 8. Derived feeds — context flow from Intent

The agent's `IContextDeriver` (§6.2) produces a `DerivedContext` from current Intent values. Six of the eight layer models feed off this. The data flow is:

```
IntentModel.Values (IState<IntentValues>)
    │
    ▼
IContextDeriver.Derive(intent) → DerivedContext (pure function)
    │
    ├─► UXModel.Flow            (IFeed<UXFlow>)
    ├─► ArchitectureModel.Blueprint (IFeed<ArchitectureBlueprint>)
    ├─► InteractionsModel.Matrix   (IFeed<InteractionsMatrix>)
    ├─► DataModel.Contracts        (IFeed<DataContracts>)
    ├─► ImplementationModel.Plan   (IFeed<BuildPlan>)
    └─► ScaffoldModel.ScaffoldCommand (IFeed<string>)
```

Every change to Intent values cascades through all dependent layers automatically. Per-layer specialization happens inside each model's `Build*` method.

### 8.1 deriveArchitecture — example specialization

```csharp
private static ArchitectureBlueprint BuildBaseline(DerivedContext ctx)
{
    var modules = BaseModules.Select(m =>
    {
        if (m.Id == "services" && !ctx.IsFieldService)
            return m with { Description = $"{ctx.EntityTitle}, {Capitalize(ctx.UserNoun)}, Schedule" };
        return m;
    });

    var edges = BaseEdges.AsEnumerable();

    if (ctx.IsOfflineFirst)
    {
        // Drop HTTP module and its incoming edge for offline-first apps
        modules = modules.Where(m => m.Id != "http");
        edges = edges.Where(e => e.ToId != "http");
    }

    return new ArchitectureBlueprint(
        modules.ToImmutableList(),
        edges.ToImmutableList());
}
```

Other layer specializations follow the same shape: take the baseline, apply targeted modifications based on `ctx` flags. See the implementations of `BuildFlow` (UX), `BuildMatrix` (Interactions), `BuildContracts` (Data), `BuildCommand` (Scaffold) in their respective models.

---

## 9. Resource dictionary structure

### 9.1 Loading order (App.xaml)

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Material theme baseline (Uno Toolkit Material) -->
            <MaterialTheme xmlns="using:Uno.Material" />

            <!-- Per-token color overrides (live-synthesized from DesignTokens) -->
            <ResourceDictionary Source="Themes/ColorPaletteOverride.xaml" />

            <!-- Static semantic colors (Info / Success / Tag / Locked) -->
            <ResourceDictionary Source="Themes/ThemeColorOverrides.xaml" />

            <!-- Editorial primitives — Eyebrow, Mono, Body styles -->
            <ResourceDictionary Source="Themes/Editorial.xaml" />

            <!-- Brushes — pre-computed from theme colors -->
            <ResourceDictionary Source="Themes/Brushes.xaml" />

            <!-- Typography styles — bound to Inter and JetBrains Mono families -->
            <ResourceDictionary Source="Themes/Typography.xaml" />

            <!-- TextBlock variant styles — Display, Heading, Body, Caption -->
            <ResourceDictionary Source="Themes/TextBlockStyles.xaml" />

            <!-- Buttons — PrimaryButton, GhostButton, ChipButton styles -->
            <ResourceDictionary Source="Themes/Buttons.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 9.2 Resource key conventions

Resources follow Material's slot naming:

| Prefix          | Examples                          | Use                                   |
|-----------------|-----------------------------------|---------------------------------------|
| `Background*`   | `BackgroundBrush`                 | Page surface                          |
| `Surface*`      | `SurfaceBrush`                    | Card / elevated surface               |
| `Primary*`      | `PrimaryBrush` / `OnPrimaryBrush` | Primary text / on-primary text        |
| `Secondary*`    | `SecondaryBrush`                  | Accent (action color)                 |
| `Error*`        | `ErrorBrush`                      | Error / warn                          |
| `Hairline*`     | `HairlineBrush` / `Hairline2Brush` | Dividers, primary and nested         |
| `Ink*`          | `InkBrush` ... `Ink5Brush`        | Text ramp from primary to faintest   |
| `Paper*`        | `PaperBrush` ... `Paper4Brush`    | Surface ramp from white to near-black |
| `Amber*`        | `AmberBrush` / `AmberSoftBrush`   | Active marker, preview state          |
| `Indigo*`       | `IndigoBrush`                     | State (MVUX) module only              |
| `Phase{N}*`     | `Phase1Brush` ... `Phase6Brush`   | Implementation phase tints           |
| `StateColor{Kind}` | `StateColorDefault` etc.       | Interactions state color per kind    |

The `Phase*` and `StateColor*` brushes are scoped to a single canvas each — referenced only by ImplementationPhaseGrid and StateTransitionDiagram respectively.

---

## 10. Lifecycle and process model

### 10.1 Startup sequence

```
App.OnLaunched
  → CreateBuilder → DI registration → Build host
  → MainWindow.Content = new Shell()
  → Shell.Loaded → Navigator.NavigateRouteAsync("Intent")
  → IntentPage created, IntentModel resolved from DI
  → ShellModel resolves with TransientLifetime
```

### 10.2 Per-layer model lifecycle

Each layer model is registered as `Transient` so it can be discarded when its page navigates away. The model's state lives only while the page is active.

This is intentional — **Intent's state is the only state that persists** across layers via direct injection into other models (UXModel takes `IntentModel`, ArchitectureModel takes `IntentModel`, etc.). The downstream layer models do not need their own persisted state because their canvases are derived feeds off Intent + context.

Future versions may persist downstream models when direct manipulation (drag-edit) lands. For v11, transient is correct.

### 10.3 ShellModel lifecycle

`ShellModel` is `Transient` from DI but practically singleton — the Shell holds a single instance for the entire workspace session. When Reset is triggered, the Shell is not recreated; ShellModel.Reset() resets all its IStates and re-navigates to Intent.

---

## 11. Acceptance criteria

After implementing this brief:

### Foundation
- [ ] All 8 page classes exist under `Views/Pages/`
- [ ] All 11 model classes exist under `Presentation/`
- [ ] All 4 service interfaces exist with at least one implementation each
- [ ] `LayerKind` enum has exactly 8 values in canonical order
- [ ] `Layers.All` immutable array matches the canonical layer table in §2.1
- [ ] `RouteMap.RegisterRoutes` registers all 8 routes nested under the Shell route
- [ ] DI registration in `App.xaml.cs` includes all services and all 11 models

### Shell + navigation
- [ ] Shell renders three columns with named navigation regions: `LeftRail`, `ActivePage`, `RightRail`
- [ ] `ShellModel.RailsVisible` returns false when `activeIndex == 0 && lockedIds.Count == 0`
- [ ] Column widths animate over 480ms with ease-out-quint when `RailsVisible` flips
- [ ] Rail content opacity animates over 320ms with 160ms delay
- [ ] Navigation between pages goes through `ShellModel.AdvanceAfterLock` only — never code-behind
- [ ] Initial navigation lands on Intent route via Shell.Loaded handler

### Layer models
- [ ] `IntentModel.Values` is an `IState<IntentValues>` initialized to `IntentModel.Example`
- [ ] `DesignModel.Tokens` is an `IState<DesignTokens>` initialized to `DesignModel.Defaults`
- [ ] `ArchitectureModel.Blueprint`, `UXModel.Flow`, `InteractionsModel.Matrix`, `DataModel.Contracts`, `ImplementationModel.Plan` are all `IFeed<T>` that derive from `IntentModel.Values` via `IContextDeriver`
- [ ] `ComposerModel.Prompts` stores per-layer textarea content in an `IState<ImmutableDictionary<LayerKind, string>>`
- [ ] `ComposerModel.Overrides` stores per-layer markdown overrides
- [ ] `ComposerModel.PreviewAcks` stores per-layer acknowledgment strings
- [ ] `FilesRailModel.FileRows` derives from Shell.ActiveIndex + Shell.LockedIds
- [ ] `FilesRailModel.ActivePreviewContent` derives from active layer + overrides + ViewAllMode

### Services
- [ ] `IContextDeriver.Derive` returns `DerivedContext` with all 8 fields populated correctly
- [ ] `ContextDeriver` matches 13 domain noun patterns in priority order
- [ ] `ILayerPreviewService` has both Identity (no API key) and Claude (configured) implementations
- [ ] `IdentityLayerPreviewService.IsConfigured` returns false
- [ ] `IMarkdownGenerator.Generate` handles all 8 `LayerKind` values
- [ ] `IBundleBuilder.BuildPromptContext` produces concatenated markdown with `<!-- {filename} -->` separators
- [ ] `IClipboardService` works on Windows (DataPackage) and WASM (navigator.clipboard)

### State machine
- [ ] `LayerState` enum has 4 values: Clean, Dirty, Previewing, Locked
- [ ] `MarkDirty(kind)` transitions clean → dirty and captures snapshot
- [ ] `GeneratePreview()` transitions dirty → previewing, calls ILayerPreviewService, captures ack
- [ ] `AcceptPreview()` transitions previewing → locked, adopts previewValues, advances activeIndex
- [ ] `DiscardPreview()` and `DiscardEdits()` restore from snapshot
- [ ] `Revisit(kind)` transitions locked → clean, sets activeIndex, navigates to that layer's route
- [ ] Snapshot capture happens only on the first clean → dirty transition

### Resources
- [ ] `ColorPaletteOverride.xaml` is generated live from DesignTokens via `DesignModel.ColorPaletteOverrideXaml`
- [ ] All hex colors live in resource keys — no hex in component XAML
- [ ] Material is preferred over Cupertino/Fluent (Toolkit precedence rule)
- [ ] TextBlock styles use only typed scale (`DisplaySTextStyle`, `HeadlineSTextStyle`, `BodyTextStyle`, `LabelSmallTextStyle`)

---

## 12. Out of scope (v11)

These are scoped for future versions, explicitly NOT in this brief:

- **Real AI calls** — `ClaudeLayerPreviewService` is specified by interface only; implementation against Anthropic API is a separate workstream
- **Direct manipulation** of canvas elements — no drag, no inline rename, no click-to-edit on diagrams; all edits flow through the composer prompt
- **Persistent storage** — workspace state is in-memory only; refreshing the app loses the composition
- **Cross-tab sync** — multiple browser tabs / windows do not share state
- **Authentication** — no user accounts in v11
- **Real-time collaboration** — single-user workspace only
- **Localization** — all strings are English; `x:Uid` keys are not yet defined
- **Telemetry** — no usage tracking in v11

The companion `INTERACTION-BRIEF-detailed.md` covers every user-facing behavior. The companion `DESIGN-BRIEF-detailed.md` covers every visual specification. This brief covers the structural truth — refer to it for any question about "what is this thing made of."
