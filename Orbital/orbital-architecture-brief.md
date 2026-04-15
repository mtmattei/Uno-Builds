# Orbital — Uno Platform Architecture & Design Brief

## 1. Product Definition

Orbital is a native desktop application built with Uno Platform that serves as the daily starting point for Uno development. It unifies environment health, project management, agentic AI sessions, Studio/connector configuration, and runtime diagnostics in a single "Terminal × Apple" interface.

**Target:** Desktop-first (Windows via WinAppSDK, macOS/Linux via Skia). WASM as stretch target.

**Rendering:** Skia renderer (default in Uno.Sdk 6.0+) for pixel-identical cross-platform output.

---

## 2. Solution Structure

```
Orbital/
├── Orbital.sln
├── Orbital/                          # Main app project (shared head)
│   ├── Orbital.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── Styles/
│   │   ├── ColorPaletteOverride.xaml   # Custom dark theme colors
│   │   ├── TextBlock.xaml              # Typography overrides
│   │   ├── Surfaces.xaml               # Card/surface styles
│   │   └── Animations.xaml             # Reusable Storyboard resources
│   ├── Views/
│   │   ├── Shell.xaml                  # NavigationView shell
│   │   ├── HomePage.xaml               # Greeting + status dashboard
│   │   ├── ProjectPage.xaml            # Project workspace
│   │   ├── AgentsPage.xaml             # Agent session list + detail
│   │   ├── StudioPage.xaml             # License + connectors
│   │   └── DiagnosticsPage.xaml        # Uno Check + deps + runtime tools
│   ├── Models/
│   │   ├── HomeModel.cs                # MVUX model — feeds for status, clock, greeting
│   │   ├── ProjectModel.cs             # MVUX model — build/run state, artifacts
│   │   ├── AgentsModel.cs              # MVUX model — session list, selected detail
│   │   ├── StudioModel.cs              # MVUX model — license, features, connectors
│   │   └── DiagnosticsModel.cs         # MVUX model — uno-check, deps, platform targets
│   ├── Services/
│   │   ├── IEnvironmentService.cs      # SDK versions, workloads, OS info
│   │   ├── IStudioService.cs           # License, features, account
│   │   ├── IAgentService.cs            # Session CRUD, tool permissions, replay
│   │   ├── IBuildService.cs            # dotnet build/run/package via Process
│   │   ├── IDiagnosticsService.cs      # uno-check runner, dep resolution
│   │   ├── IMcpService.cs              # MCP server health, tool inventory
│   │   └── IClockService.cs            # DispatcherTimer-based tick for live clock
│   ├── Controls/
│   │   ├── StatusDot.xaml              # Pulsing status indicator (ok/warn/error/idle)
│   │   ├── PulsingBars.xaml            # Equalizer-style animated bars
│   │   ├── ConsoleOutput.xaml          # Rich-text console with line numbers + badges
│   │   ├── TimelineView.xaml           # Vertical timeline with status dots
│   │   ├── VersionPill.xaml            # Label + value pill for version strip
│   │   └── DataStream.xaml             # Flickering hex telemetry strip
│   └── Assets/
│       └── Fonts/
│           ├── DMSans-Variable.ttf
│           └── JetBrainsMono-Variable.ttf
├── Orbital.Services/                   # Service implementations (class library)
│   ├── EnvironmentService.cs
│   ├── StudioService.cs
│   ├── AgentService.cs
│   ├── BuildService.cs
│   ├── DiagnosticsService.cs
│   ├── McpService.cs
│   └── ClockService.cs
└── Orbital.Tests/                      # Unit + UI tests
```

### Project Template Command

```bash
dotnet new unoapp -preset=recommended -o Orbital \
  -tfm net9.0 \
  -platforms desktop \
  -presentation mvux \
  -theme material \
  -toolkit true \
  -extensions true \
  -di true \
  -nav regions \
  -log default
```

### Required UnoFeatures (csproj)

```xml
<UnoFeatures>
  Material;
  Toolkit;
  Extensions;
  ExtensionsCore;
  Hosting;
  MVUX;
  Navigation;
  Logging;
  ThemeService;
  Dsp;
  SkiaRenderer;
</UnoFeatures>
```

---

## 3. Navigation Architecture

### Shell (NavigationView with Region Navigation)

The app shell uses a left-pane `NavigationView` with Uno Extensions region-based navigation. Each `NavigationViewItem` maps to a named region that resolves to a page.

```xml
<!-- Shell.xaml -->
<Page x:Class="Orbital.Views.Shell"
      xmlns:uen="using:Uno.Extensions.Navigation.UI"
      xmlns:muxc="using:Microsoft.UI.Xaml.Controls"
      xmlns:utu="using:Uno.Toolkit.UI">

    <muxc:NavigationView x:Name="NavView"
                         uen:Region.Attached="true"
                         PaneDisplayMode="Left"
                         IsPaneOpen="True"
                         OpenPaneLength="220"
                         IsBackButtonVisible="Collapsed"
                         IsSettingsVisible="False">

        <muxc:NavigationView.PaneHeader>
            <!-- Orbital logo + version + MCP status widget -->
        </muxc:NavigationView.PaneHeader>

        <muxc:NavigationView.MenuItems>
            <muxc:NavigationViewItem Content="Home"
                                     uen:Region.Name="Home"
                                     Icon="{ui:FontIcon Glyph='&#xE80F;'}" />
            <muxc:NavigationViewItem Content="Project"
                                     uen:Region.Name="Project"
                                     Icon="{ui:FontIcon Glyph='&#xE8B7;'}" />
            <muxc:NavigationViewItem Content="Agent Sessions"
                                     uen:Region.Name="Agents"
                                     Icon="{ui:FontIcon Glyph='&#xE99A;'}" />
            <muxc:NavigationViewItem Content="Studio"
                                     uen:Region.Name="Studio"
                                     Icon="{ui:FontIcon Glyph='&#xE8D7;'}" />
            <muxc:NavigationViewItem Content="Diagnostics"
                                     uen:Region.Name="Diagnostics"
                                     Icon="{ui:FontIcon Glyph='&#xE9D9;'}" />
        </muxc:NavigationView.MenuItems>

        <muxc:NavigationView.PaneFooter>
            <!-- Live clock + MCP connection status card -->
        </muxc:NavigationView.PaneFooter>

        <!-- Content region -->
        <Grid uen:Region.Attached="true"
              uen:Region.Name="Main" />

    </muxc:NavigationView>
</Page>
```

### Route Registration (App.xaml.cs)

```csharp
private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
{
    views.Register(
        new ViewMap(ViewModel: typeof(ShellModel)),
        new ViewMap<HomePage, HomeModel>(),
        new ViewMap<ProjectPage, ProjectModel>(),
        new ViewMap<AgentsPage, AgentsModel>(),
        new ViewMap<StudioPage, StudioModel>(),
        new ViewMap<DiagnosticsPage, DiagnosticsModel>()
    );

    routes.Register(
        new RouteMap("", View: views.FindByViewModel<ShellModel>(),
            Nested:
            [
                new RouteMap("Main", View: views.FindByViewModel<HomeModel>(),
                    IsDefault: true,
                    Nested:
                    [
                        new RouteMap("Home", View: views.FindByViewModel<HomeModel>()),
                        new RouteMap("Project", View: views.FindByViewModel<ProjectModel>()),
                        new RouteMap("Agents", View: views.FindByViewModel<AgentsModel>()),
                        new RouteMap("Studio", View: views.FindByViewModel<StudioModel>()),
                        new RouteMap("Diagnostics", View: views.FindByViewModel<DiagnosticsModel>()),
                    ])
            ])
    );
}
```

---

## 4. MVUX Models (Reactive Data Layer)

Each page gets a `partial record` model using `IFeed<T>` and `IState<T>` for async data that maps directly to `FeedView` in XAML.

### HomeModel

```csharp
public partial record HomeModel(
    IEnvironmentService Env,
    IStudioService Studio,
    IMcpService Mcp,
    IAgentService Agents,
    IClockService Clock)
{
    // Status cards — each is an async feed with built-in loading/error states
    public IFeed<EnvironmentStatus> EnvStatus => Feed.Async(Env.GetStatusAsync);
    public IFeed<StudioStatus> StudioStatus => Feed.Async(Studio.GetStatusAsync);
    public IFeed<McpStatus> McpStatus => Feed.Async(Mcp.GetConnectionStatusAsync);
    public IFeed<AgentSession?> ActiveSession => Feed.Async(Agents.GetActiveSessionAsync);
    public IFeed<IImmutableList<RecentProject>> RecentProjects => Feed.Async(Env.GetRecentProjectsAsync);

    // Live clock — ticks every second via IClockService observable
    public IState<DateTime> CurrentTime => State.Async(this, Clock.GetTimeStream);

    // Greeting — derived from clock
    public IFeed<string> Greeting => CurrentTime.Select(t =>
        t.Hour < 12 ? "Good morning" : t.Hour < 17 ? "Good afternoon" : "Good evening");

    // User name from Studio account
    public IFeed<string> UserName => Feed.Async(async ct =>
        (await Studio.GetStatusAsync(ct)).AccountName ?? "Developer");

    // Version strip data
    public IFeed<VersionInfo> Versions => Feed.Async(Env.GetVersionInfoAsync);
}
```

### Data Records (immutable)

```csharp
public record EnvironmentStatus(string UnoSdkVersion, string DotNetVersion, int WorkloadCount, string Renderer);
public record StudioStatus(string Version, string Tier, string AccountName, string AccountEmail, DateOnly Expiry, bool IsValid);
public record McpStatus(bool Connected, int ServerCount, int ToolCount, IImmutableList<McpServer> Servers);
public record McpServer(string Name, string Url, bool Healthy, int ToolCount);
public record AgentSession(string Id, string Name, string Repo, string Branch, string Goal,
    SessionStatus Status, int ActionCount, int ArtifactCount, IImmutableList<AgentAction> Actions);
public record AgentAction(DateTime Time, string Title, string Detail, ActionStatus Status);
public record RecentProject(string Name, string Path, string Branch, string LastBuild, HealthStatus Status);
public record VersionInfo(string OrbitalVersion, string UnoSdkVersion, string DotNetVersion, string LlmModel, int McpServerCount);

public enum SessionStatus { Active, Paused, Done }
public enum ActionStatus { Ok, Warn, Error, Idle }
public enum HealthStatus { Ok, Warn, Error, Idle }
```

### ProjectModel

```csharp
public partial record ProjectModel(IBuildService Build, IEnvironmentService Env)
{
    public IFeed<ProjectMeta> Meta => Feed.Async(Env.GetCurrentProjectAsync);
    public IState<string> ActiveTab => State.Value(this, () => "build");
    public IFeed<IImmutableList<ConsoleLine>> BuildOutput => Feed.Async(Build.GetLastBuildOutputAsync);
    public IFeed<IImmutableList<ConsoleLine>> RunOutput => Feed.Async(Build.GetLastRunOutputAsync);
    public IFeed<IImmutableList<Artifact>> Artifacts => Feed.Async(Build.GetArtifactsAsync);

    public async ValueTask BuildProject() => await Build.BuildAsync();
    public async ValueTask RunProject() => await Build.RunAsync();
    public async ValueTask BuildRunVerify() => await Build.BuildRunVerifyAsync();
}
```

### AgentsModel

```csharp
public partial record AgentsModel(IAgentService Agents)
{
    public IListFeed<AgentSession> Sessions => ListFeed.Async(Agents.GetSessionsAsync);
    public IState<AgentSession?> SelectedSession => State.Value<AgentSession?>(this, () => null);

    public async ValueTask CreateSession() => await Agents.CreateSessionAsync();
    public async ValueTask ReplaySession(AgentSession session) => await Agents.ReplayAsync(session.Id);
}
```

---

## 5. Dependency Injection Setup

```csharp
// App.xaml.cs — inside OnLaunched
var appBuilder = this.CreateBuilder(args)
    .Configure(host =>
    {
        host.ConfigureServices((context, services) =>
        {
            // Core services
            services.AddSingleton<IEnvironmentService, EnvironmentService>();
            services.AddSingleton<IStudioService, StudioService>();
            services.AddSingleton<IAgentService, AgentService>();
            services.AddSingleton<IBuildService, BuildService>();
            services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
            services.AddSingleton<IMcpService, McpService>();
            services.AddSingleton<IClockService, ClockService>();
        });
    });
```

---

## 6. Visual Design System

### Color Palette (ColorPaletteOverride.xaml)

The "Terminal × Apple" aesthetic is dark-first with a deep blue-charcoal base and an emerald accent. Override Material Design 3 color slots:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Dark">
            <!-- Surfaces -->
            <Color x:Key="SurfaceColor">#FF0F1117</Color>           <!-- hsl(220, 16%, 6%) -->
            <Color x:Key="SurfaceContainerColor">#FF151820</Color>   <!-- hsl(220, 14%, 8.5%) -->
            <Color x:Key="SurfaceContainerHighColor">#FF1A1E28</Color> <!-- hsl(220, 14%, 11%) -->
            <Color x:Key="SurfaceContainerHighestColor">#FF212633</Color> <!-- hsl(220, 14%, 14%) -->

            <!-- Primary accent: Emerald -->
            <Color x:Key="PrimaryColor">#FF34D399</Color>           <!-- emerald-400 -->
            <Color x:Key="OnPrimaryColor">#FF0F1117</Color>
            <Color x:Key="PrimaryContainerColor">#FF0D3B2A</Color>  <!-- emerald-900/50 -->
            <Color x:Key="OnPrimaryContainerColor">#FF34D399</Color>

            <!-- Secondary -->
            <Color x:Key="SecondaryColor">#FF8B5CF6</Color>         <!-- violet-400 (agent accent) -->
            <Color x:Key="TertiaryColor">#FFFBBF24</Color>          <!-- amber-400 (warning accent) -->

            <!-- Text hierarchy -->
            <Color x:Key="OnSurfaceColor">#FFE5E7EB</Color>         <!-- ~92% white -->
            <Color x:Key="OnSurfaceVariantColor">#FF7B8298</Color>  <!-- ~50% muted -->
            <Color x:Key="OutlineColor">#FF212633</Color>           <!-- border -->
            <Color x:Key="OutlineVariantColor">#FF1A1E28</Color>

            <!-- Error -->
            <Color x:Key="ErrorColor">#FFEF4444</Color>             <!-- red-500 -->
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

### Typography (TextBlock.xaml)

Two-font system: DM Sans for UI, JetBrains Mono for data/console.

```xml
<ResourceDictionary>
    <!-- Load custom fonts -->
    <FontFamily x:Key="OrbitalSansFont">ms-appx:///Assets/Fonts/DMSans-Variable.ttf#DM Sans</FontFamily>
    <FontFamily x:Key="OrbitalMonoFont">ms-appx:///Assets/Fonts/JetBrainsMono-Variable.ttf#JetBrains Mono</FontFamily>

    <!-- Override Material typography resource keys -->
    <Style x:Key="DisplayLarge" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
        <Setter Property="FontSize" Value="28" />
        <Setter Property="FontWeight" Value="Bold" />
        <Setter Property="Foreground" Value="{ThemeResource OnSurfaceBrush}" />
    </Style>

    <!-- Mono style for console, badges, data -->
    <Style x:Key="MonoSmall" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
        <Setter Property="FontSize" Value="11" />
        <Setter Property="FontWeight" Value="Normal" />
        <Setter Property="CharacterSpacing" Value="20" />
        <Setter Property="Foreground" Value="{ThemeResource OnSurfaceVariantBrush}" />
    </Style>

    <Style x:Key="MonoLabel" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
        <Setter Property="FontSize" Value="10" />
        <Setter Property="FontWeight" Value="Medium" />
        <Setter Property="CharacterSpacing" Value="80" />
        <Setter Property="TextTransform" Value="Uppercase" />
        <Setter Property="Foreground" Value="{ThemeResource OnSurfaceVariantBrush}" />
        <Setter Property="Opacity" Value="0.5" />
    </Style>
</ResourceDictionary>
```

### Spacing Scale

Use Uno Toolkit `AutoLayout` with this 4px/8px scale throughout:

| Token   | Value | Usage                            |
|---------|-------|----------------------------------|
| xs      | 4px   | Inline gaps, icon margins        |
| sm      | 8px   | Badge padding, compact gaps      |
| md      | 12px  | Card internal spacing            |
| base    | 16px  | Standard content padding         |
| lg      | 20px  | Card padding (Surface)           |
| xl      | 24px  | Section spacing                  |
| 2xl     | 32px  | Page margins                     |
| 3xl     | 48px  | Hero spacing                     |

---

## 7. Custom Controls

### StatusDot

A 10px circle with color + optional pulsing animation via a looping `Storyboard`.

```xml
<!-- StatusDot.xaml -->
<UserControl x:Class="Orbital.Controls.StatusDot">
    <Grid Width="10" Height="10">
        <Ellipse x:Name="Dot" Width="10" Height="10">
            <Ellipse.RenderTransform>
                <ScaleTransform x:Name="DotScale" CenterX="5" CenterY="5" />
            </Ellipse.RenderTransform>
        </Ellipse>
        <!-- Pulse ring (outer, fades out) -->
        <Ellipse x:Name="PulseRing" Width="10" Height="10"
                 StrokeThickness="1.5" Opacity="0">
            <Ellipse.RenderTransform>
                <ScaleTransform x:Name="RingScale" CenterX="5" CenterY="5" />
            </Ellipse.RenderTransform>
        </Ellipse>
    </Grid>

    <UserControl.Resources>
        <Storyboard x:Name="PulseAnimation" RepeatBehavior="Forever">
            <!-- Dot breathe -->
            <DoubleAnimation Storyboard.TargetName="DotScale"
                             Storyboard.TargetProperty="ScaleX"
                             From="1" To="0.85" Duration="0:0:1"
                             AutoReverse="True">
                <DoubleAnimation.EasingFunction>
                    <SineEase EasingMode="EaseInOut" />
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            <DoubleAnimation Storyboard.TargetName="DotScale"
                             Storyboard.TargetProperty="ScaleY"
                             From="1" To="0.85" Duration="0:0:1"
                             AutoReverse="True">
                <DoubleAnimation.EasingFunction>
                    <SineEase EasingMode="EaseInOut" />
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            <!-- Ring expand + fade -->
            <DoubleAnimation Storyboard.TargetName="RingScale"
                             Storyboard.TargetProperty="ScaleX"
                             From="0.85" To="1.4" Duration="0:0:2.5" />
            <DoubleAnimation Storyboard.TargetName="RingScale"
                             Storyboard.TargetProperty="ScaleY"
                             From="0.85" To="1.4" Duration="0:0:2.5" />
            <DoubleAnimation Storyboard.TargetName="PulseRing"
                             Storyboard.TargetProperty="Opacity"
                             From="0.6" To="0" Duration="0:0:2.5" />
        </Storyboard>
    </UserControl.Resources>
</UserControl>
```

**Dependency properties:** `Status` (enum: Ok, Warn, Error, Idle) — drives Fill color and starts/stops PulseAnimation.

### PulsingBars

Five narrow rectangles with staggered `DoubleAnimation` on `ScaleY` (bar-pulse). Each bar gets a random initial height and a slightly offset `BeginTime`.

### ConsoleOutput

An `ItemsRepeater` inside a `ScrollViewer` displaying `ConsoleLine` records. Each line renders a line-number `TextBlock` (mono, dimmed) + content `TextBlock` with color based on `LineType` enum. A scanline `Rectangle` with `TranslateTransform` animation sweeps vertically. Copy button per line via `CommandExtensions`.

### TimelineView

Vertical `ItemsRepeater` with a template containing: status circle, connector line (`Rectangle`, 1px wide), title, time badge, detail text.

### VersionPill

A horizontal `AutoLayout` with two `TextBlock` children (label in `MonoLabel` style, value in `MonoSmall` style), wrapped in a `Border` with `CornerRadius="6"` and `SurfaceContainerHigh` background.

---

## 8. Animation System

All animations use XAML `Storyboard` resources defined in `Animations.xaml` and triggered in code-behind or via `VisualStateManager`.

| Animation         | Property                  | Duration | Easing        | Repeat          |
|-------------------|---------------------------|----------|---------------|-----------------|
| Pulse Dot         | ScaleX/Y + Opacity        | 2s       | SineEaseInOut | Forever         |
| Ring Pulse        | ScaleX/Y + Opacity        | 2.5s     | Linear        | Forever         |
| Border Breathe    | BorderBrush Opacity       | 3s       | SineEaseInOut | Forever         |
| Bar Pulse         | ScaleY (per bar)          | 1.2-1.9s | SineEaseInOut | Forever         |
| Scanline Sweep    | TranslateTransform.Y      | 4s       | Linear        | Forever         |
| Fade-Up Entrance  | Opacity + TranslateY      | 350ms    | CubicEaseOut  | Once            |
| Orb Float         | TranslateY + ScaleX/Y     | 5s       | SineEaseInOut | Forever         |
| Data Flicker      | Opacity                   | 3s       | Linear steps  | Forever         |
| Gradient Shift    | GradientStop.Offset       | 6s       | Linear        | Forever         |
| Glow              | ShadowContainer opacity   | 3s       | SineEaseInOut | Forever         |

### Entrance Choreography

Cards on each page stagger their fade-up by 60-80ms per item using `BeginTime` offsets on the `Storyboard`. Trigger on `Loaded` event.

```xml
<Storyboard x:Name="CardEntrance">
    <DoubleAnimation Storyboard.TargetName="Card0"
                     Storyboard.TargetProperty="Opacity"
                     From="0" To="1" Duration="0:0:0.35" BeginTime="0:0:0">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetName="Card0Transform"
                     Storyboard.TargetProperty="Y"
                     From="8" To="0" Duration="0:0:0.35" BeginTime="0:0:0">
        <DoubleAnimation.EasingFunction>
            <CubicEase EasingMode="EaseOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <!-- Card1 at BeginTime="0:0:0.07", Card2 at "0:0:0.14", etc. -->
</Storyboard>
```

---

## 9. Service Layer Contracts

### IClockService

```csharp
public interface IClockService
{
    // Returns an async enumerable that yields DateTime every second
    IAsyncEnumerable<DateTime> GetTimeStream(CancellationToken ct);
}
```

Implementation uses `DispatcherTimer` at 1-second intervals, exposed as `IAsyncEnumerable` for MVUX `State.Async` consumption.

### IBuildService

```csharp
public interface IBuildService
{
    Task<IImmutableList<ConsoleLine>> BuildAsync(CancellationToken ct = default);
    Task<IImmutableList<ConsoleLine>> RunAsync(CancellationToken ct = default);
    Task BuildRunVerifyAsync(CancellationToken ct = default);
    Task<IImmutableList<ConsoleLine>> GetLastBuildOutputAsync(CancellationToken ct = default);
    Task<IImmutableList<ConsoleLine>> GetLastRunOutputAsync(CancellationToken ct = default);
    Task<IImmutableList<Artifact>> GetArtifactsAsync(CancellationToken ct = default);
}
```

Implementation wraps `System.Diagnostics.Process` to run `dotnet build`, `dotnet run`, `uno-check`, and capture stdout/stderr line-by-line into structured `ConsoleLine` records.

### IAgentService

```csharp
public interface IAgentService
{
    Task<IImmutableList<AgentSession>> GetSessionsAsync(CancellationToken ct = default);
    Task<AgentSession?> GetActiveSessionAsync(CancellationToken ct = default);
    Task<AgentSession> CreateSessionAsync(CancellationToken ct = default);
    Task ReplayAsync(string sessionId, CancellationToken ct = default);
    Task<IImmutableList<ToolPermission>> GetPermissionsAsync(string sessionId, CancellationToken ct = default);
    Task SetPermissionAsync(string sessionId, string toolName, bool granted, CancellationToken ct = default);
}

public record ToolPermission(string Name, bool Granted);
```

---

## 10. Page-by-Page XAML Mapping

### A. HomePage.xaml

| Prototype Element       | Uno Control                                         |
|-------------------------|-----------------------------------------------------|
| Greeting hero           | `AutoLayout` (horizontal) + `DisplayLarge` TextBlock |
| Avatar orb              | `Ellipse` with `LinearGradientBrush` + float Storyboard |
| Live clock              | `TextBlock` bound to `CurrentTime` feed, `MonoSmall` style |
| Version pill strip      | `AutoLayout` (horizontal) of `VersionPill` UserControls |
| Data stream             | `TextBlock` + `DispatcherTimer` randomizing hex, `DataFlicker` Storyboard |
| Status cards (4-grid)   | `Grid` (4 columns) of `CardContentControl` (FilledCardContentControlStyle) |
| Quick actions            | `AutoLayout` (vertical) of `Button` with Toolkit icon |
| Active session card     | `CardContentControl` with `TimelineView` inside |
| Recent projects list    | `ItemsRepeater` with horizontal `AutoLayout` template |

### B. ProjectPage.xaml

| Prototype Element       | Uno Control                                          |
|-------------------------|------------------------------------------------------|
| Meta strip (5-grid)     | `Grid` (5 columns) of compact `CardContentControl`  |
| Task bar                | `AutoLayout` (horizontal) of `Button` variants       |
| Tabbed console          | Custom `TabBar` (Toolkit) switching `ConsoleOutput` visibility |
| Artifacts list          | `ItemsRepeater` with `CommandExtensions` for Copy    |

### C. AgentsPage.xaml

| Prototype Element       | Uno Control                                          |
|-------------------------|------------------------------------------------------|
| Session list (left)     | `ListView` with `SelectionChanged` → `SelectedSession` state update |
| Session detail (right)  | `FeedView` bound to `SelectedSession`                |
| Tool permissions        | `ItemsRepeater` of `Badge`-styled `Border` elements  |
| Action timeline         | `TimelineView` (custom control)                      |
| Acceptance checks       | `ItemsRepeater` with check/x icon + text             |

### D. StudioPage.xaml

| Prototype Element       | Uno Control                                          |
|-------------------------|------------------------------------------------------|
| License card            | `CardContentControl` (ElevatedCardContentControlStyle) with breathing border |
| Feature grid            | `Grid` (3 columns) of `CardContentControl` with check/x icon |
| Connector list          | `ItemsRepeater` with `StatusDot` + `PulsingBars` + action `Button` |

### E. DiagnosticsPage.xaml

| Prototype Element       | Uno Control                                          |
|-------------------------|------------------------------------------------------|
| Uno Check output        | `ConsoleOutput` (custom) with scanline overlay       |
| Dependencies grid       | `Grid` (2 columns) of dependency `CardContentControl` with Update button |
| Runtime verification    | `Grid` (3 columns) of clickable `CardContentControl` with `PulsingBars` |
| Platform targets strip  | `AutoLayout` (horizontal) of `Border` pills with `StatusDot` |

---

## 11. FeedView Loading States

Every feed-driven section uses `FeedView` with custom templates:

```xml
<mvux:FeedView Source="{Binding EnvStatus}">
    <mvux:FeedView.ValueTemplate>
        <DataTemplate>
            <!-- Rendered status card content -->
        </DataTemplate>
    </mvux:FeedView.ValueTemplate>
    <mvux:FeedView.ProgressTemplate>
        <DataTemplate>
            <!-- Shimmer skeleton: Rectangle with animate-shimmer style -->
            <Border CornerRadius="10" Height="120"
                    Background="{ThemeResource SurfaceContainerBrush}">
                <Rectangle RadiusX="4" RadiusY="4"
                           Fill="{ThemeResource ShimmerBrush}" />
            </Border>
        </DataTemplate>
    </mvux:FeedView.ProgressTemplate>
    <mvux:FeedView.ErrorTemplate>
        <DataTemplate>
            <TextBlock Text="Failed to load"
                       Style="{StaticResource MonoSmall}"
                       Foreground="{ThemeResource ErrorBrush}" />
        </DataTemplate>
    </mvux:FeedView.ErrorTemplate>
</mvux:FeedView>
```

---

## 12. Build & Run

```bash
# Restore + build
dotnet build -f net9.0-desktop Orbital/Orbital.csproj

# Run with Hot Reload
export DOTNET_MODIFIABLE_ASSEMBLIES=debug
dotnet run -f net9.0-desktop --project Orbital/Orbital.csproj

# WASM (stretch)
dotnet run -f net9.0-browserwasm --project Orbital/Orbital.csproj
```

---

## 13. Implementation Sequence

| Phase | Tasks | Acceptance |
|-------|-------|------------|
| 1. Scaffold | `dotnet new unoapp`, add UnoFeatures, add fonts, create ColorPaletteOverride + TextBlock styles | App launches dark-themed with custom fonts |
| 2. Shell | Shell.xaml with NavigationView, register 5 routes, confirm tab switching | All 5 pages navigate correctly |
| 3. Home | HomeModel + HomePage.xaml — greeting, clock, version strip, status cards (mock data) | Greeting shows correct time-of-day, clock ticks, cards render |
| 4. Animations | StatusDot, PulsingBars, entrance stagger, breathing borders, scanline | All prototype animations running natively |
| 5. Console | ConsoleOutput control, mock build/run output | Colored console with line numbers, scanline, copy buttons |
| 6. Project | ProjectModel + ProjectPage — meta strip, task bar, tabbed console, artifacts | Tabs switch, artifacts list renders |
| 7. Agents | AgentsModel + AgentsPage — session list, detail, timeline, permissions, checks | Session selection works, timeline renders |
| 8. Studio | StudioModel + StudioPage — license card, features grid, connectors list | Feature toggles and connector status visible |
| 9. Diagnostics | DiagnosticsModel + DiagnosticsPage — uno-check, deps, runtime tools, platform strip | Uno-check output renders, update buttons visible |
| 10. Live services | Replace mock data with real `System.Diagnostics.Process` calls, real MCP client | Build/run actually executes, uno-check runs live |

---

## 14. Key Uno Platform Rules (from MCP docs)

These constraints are baked into the brief and must be followed during implementation:

- **Never use hardcoded hex colors in XAML.** All colors go through `ColorPaletteOverride.xaml` or existing Material resource keys.
- **Never set font sizes directly.** Use the custom TextBlock styles defined in `TextBlock.xaml`.
- **Use `AutoLayout` for all stacking layouts** (not StackPanel) when Uno Toolkit is present, with `Spacing` and `Padding` on the container.
- **Never set padding/margin on children of AutoLayout.** Use container `Spacing`/`Padding`.
- **Use `CardContentControl`** instead of `Border` with `CornerRadius` + `ThemeShadow` for card surfaces.
- **Use `ThemeShadow`** with `Translation="0,0,Z"` for elevation (Z in 8-32 range).
- **All Bindings use `Mode=TwoWay`** for state updates via MVUX.
- **MVUX commands bind via `{Binding MethodName}`** — never invoke from code-behind.
- **Navigation uses `uen:Region.Name`** on `NavigationViewItem` — never navigate from code-behind.
- **Generate `x:Uid`** for all visible/interactive elements for localization readiness.
- **Set `AutomationProperties.Name`** on buttons, inputs, and templated list items.
- **Prefer Material theme** when both Material and Fluent are present.
- **Use `Responsive` markup extension** for any layout that must adapt to window width.
- **Use `FontIcon`** for icons (Segoe Fluent Icons) — no image assets for standard icons.
