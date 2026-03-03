# Flux Transit - Uno Platform Implementation Brief

## 1. Overview

Flux Transit is a real-time, AI-enhanced dashboard for Montreal public transit (STM) commuters. The app provides network health visualization, crowd analytics, and AI-driven route optimization in a "Cyber-Noir / Deep Aurora" glassmorphism aesthetic with a dark-only theme.

This implementation targets **WebAssembly** as the primary platform (mobile-responsive and desktop), leveraging Uno Platform's Skia renderer for consistent cross-platform rendering. The architecture uses **MVUX** for reactive state management with `IState` and `IFeed`, region-based navigation with **TabBar** for responsive shell, and the **Material** design system customized to achieve the glassmorphism aesthetic.

**Key Uno Platform decisions:**
- MVUX for reactive data flows (AI responses, live tracking simulation)
- TabBar (Toolkit) for responsive navigation shell (bottom on mobile, vertical on desktop)
- FeedView for async data display with loading states
- Custom ResourceDictionary for glassmorphism styling
- Kiota for future API integration (currently mocked)

---

## 2. Target Platforms + Project Template

| Aspect | Decision | MCP Source |
|--------|----------|------------|
| Primary Platform | WebAssembly (Desktop + Mobile responsive) | Project requirement |
| Secondary Platforms | Windows, macOS, Linux (Skia Desktop) | Cross-platform by default |
| Template | Uno.Extensions recommended template | [MCP: dotnet new templates] |
| Renderer | Skia (default, consistent cross-platform) | [MCP: agent rules] |

### UnoFeatures

```xml
<UnoFeatures>
    Material;
    Toolkit;
    MVUX;
    Navigation;
    Localization;
    Configuration;
    Logging;
    Http;
</UnoFeatures>
```

---

## 3. Screen-by-Screen Mapping

| Screen | Purpose | Uno Controls | Navigation | State | Notes |
|--------|---------|--------------|------------|-------|-------|
| **Dashboard** | Main view with AI planner, live routes, crowd chart, network status | Grid, AutoLayout, TextBox, Button, ChipGroup, ItemsRepeater, ProgressBar, FeedView | Default route `/` | `DashboardModel` with IState/IFeed | Primary screen, 2-column on desktop |
| **Profile** | User details, OPUS management, settings | ContentDialog (modal) | Route `!Profile` (dialog) | `ProfileModel` | Modal overlay, not full page |

### Dashboard Layout Structure

```
┌─────────────────────────────────────────────────────────────────┐
│ HEADER: Greeting | Weather | OPUS Balance | Trip Count          │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────┐  ┌─────────────────────────────┐│
│ │ AI SMART PLANNER            │  │ NETWORK STATUS              ││
│ │ - Origin/Destination        │  │ - Global health indicator   ││
│ │ - Preferences input         │  │ - Status: Normal/Delay/Down ││
│ │ - Favorites chips           │  │                             ││
│ │ - "Find Routes" button      │  ├─────────────────────────────┤│
│ ├─────────────────────────────┤  │ ALERT BANNERS               ││
│ │ AI INSIGHT CARD             │  │ - Construction notices      ││
│ │ - Generated recommendation  │  │ - Weather impacts           ││
│ ├─────────────────────────────┤  └─────────────────────────────┘│
│ │ ROUTE CARDS (ItemsRepeater) │                                 │
│ │ - TransitCard per route     │                                 │
│ │ - Progress bars animating   │                                 │
│ ├─────────────────────────────┤                                 │
│ │ CROWD CHART                 │                                 │
│ │ - Area chart visualization  │                                 │
│ └─────────────────────────────┘                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Mobile (< 600px):** Single column stack, all sections vertically arranged.

---

## 4. Design System in Uno Terms

### Theme Strategy

**Dark mode only** - No light theme toggle required. All resources defined for dark theme.

### Color Resources (ResourceDictionary)

```xml
<!-- Flux Transit Color Palette -->
<Color x:Key="FluxBackground">#0f172a</Color>           <!-- slate-900 -->
<Color x:Key="FluxSurface">#1e293b</Color>              <!-- slate-800 -->
<Color x:Key="FluxGlassPanel">#1e293b66</Color>         <!-- slate-800/40 -->
<Color x:Key="FluxGlassIndigo">#312e811a</Color>        <!-- indigo-900/10 -->
<Color x:Key="FluxBorderSubtle">#ffffff0d</Color>       <!-- white/5 -->
<Color x:Key="FluxBorderLight">#ffffff1a</Color>        <!-- white/10 -->

<!-- Semantic Colors -->
<Color x:Key="FluxPrimary">#818cf8</Color>              <!-- indigo-400 -->
<Color x:Key="FluxPrimaryStrong">#4f46e5</Color>        <!-- indigo-600 -->
<Color x:Key="FluxSuccess">#34d399</Color>              <!-- emerald-400 -->
<Color x:Key="FluxWarning">#fbbf24</Color>              <!-- amber-400 -->
<Color x:Key="FluxError">#fb7185</Color>                <!-- rose-400 -->

<!-- Text Colors -->
<Color x:Key="FluxTextPrimary">#ffffff</Color>
<Color x:Key="FluxTextSecondary">#94a3b8</Color>        <!-- slate-400 -->
<Color x:Key="FluxTextMuted">#64748b</Color>            <!-- slate-500 -->

<!-- Brushes -->
<SolidColorBrush x:Key="FluxBackgroundBrush" Color="{StaticResource FluxBackground}" />
<SolidColorBrush x:Key="FluxSurfaceBrush" Color="{StaticResource FluxSurface}" />
<SolidColorBrush x:Key="FluxPrimaryBrush" Color="{StaticResource FluxPrimary}" />
<!-- ... etc -->
```

### Typography (TextBlock Styles)

```xml
<!-- H1: Greeting -->
<Style x:Key="FluxHeadingLarge" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="Inter" />
    <Setter Property="FontSize" Value="36" />
    <Setter Property="FontWeight" Value="Bold" />
    <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
    <Setter Property="CharacterSpacing" Value="-20" />
</Style>

<!-- H2: Section Headers -->
<Style x:Key="FluxHeadingSection" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="Inter" />
    <Setter Property="FontSize" Value="20" />
    <Setter Property="FontWeight" Value="Bold" />
    <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
</Style>

<!-- Body -->
<Style x:Key="FluxBody" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="Inter" />
    <Setter Property="FontSize" Value="14" />
    <Setter Property="Foreground" Value="{StaticResource FluxTextSecondaryBrush}" />
</Style>

<!-- Micro (Labels) -->
<Style x:Key="FluxMicro" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="Inter" />
    <Setter Property="FontSize" Value="10" />
    <Setter Property="FontWeight" Value="Bold" />
    <Setter Property="CharacterSpacing" Value="80" />
    <Setter Property="Foreground" Value="{StaticResource FluxTextMutedBrush}" />
</Style>
```

### Spacing Tokens

| Token | Value | Usage |
|-------|-------|-------|
| `FluxSpacingXS` | 4 | Micro gaps |
| `FluxSpacingS` | 8 | Tight spacing |
| `FluxSpacingM` | 16 | Standard padding |
| `FluxSpacingL` | 24 | Section gaps |
| `FluxSpacingXL` | 32 | Major sections |
| `FluxSpacingXXL` | 40 | Desktop padding |

### Glass Panel Style

```xml
<Style x:Key="FluxGlassPanelStyle" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource FluxGlassPanelBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource FluxBorderSubtleBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="Padding" Value="24" />
</Style>
```

**Note:** No AcrylicBrush - using solid semi-transparent colors for cross-platform consistency. The glassmorphism effect is achieved through layered opacity and subtle borders rather than true backdrop blur.

---

## 5. Component Specifications

### 5.1 Status Pill (Header Stats)

**Control Mapping:** Custom `UserControl` or styled `Border` with content

**Structure:**
```xml
<Border Style="{StaticResource FluxStatusPillStyle}">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <Border CornerRadius="20" Background="{StaticResource FluxPrimaryBrush}" Padding="8">
            <FontIcon Glyph="&#xE8C8;" FontSize="16" />
        </Border>
        <StackPanel>
            <TextBlock Style="{StaticResource FluxMicro}" Text="BALANCE" />
            <TextBlock Style="{StaticResource FluxBody}" FontWeight="Bold" Text="$18.50" />
        </StackPanel>
    </StackPanel>
</Border>
```

**Visual States:** Default, Hover (increase opacity to 60%)
**Accessibility:** AutomationProperties.Name for screen readers

### 5.2 Transit Card

**Control Mapping:** Custom `UserControl` for use in `ItemsRepeater`

**Structure:**
```xml
<UserControl x:Class="FluxTransit.Controls.TransitCard">
    <Border Style="{StaticResource FluxGlassPanelStyle}">
        <Grid ColumnDefinitions="Auto,*,Auto" RowDefinitions="Auto,Auto">
            <!-- Route Icon (colored circle with line letter) -->
            <Border Grid.RowSpan="2" CornerRadius="20" Width="40" Height="40"
                    Background="{Binding Color}">
                <TextBlock Text="{Binding LineCode}" HorizontalAlignment="Center" />
            </Border>

            <!-- Route Name + Status Badge -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                <TextBlock Text="{Binding Name}" Style="{StaticResource FluxBody}" />
                <Border x:Name="StatusBadge" CornerRadius="4" Padding="4,2">
                    <TextBlock Text="{Binding StatusText}" FontSize="10" />
                </Border>
            </StackPanel>

            <!-- ETA -->
            <TextBlock Grid.Column="2" Text="{Binding EtaDisplay}"
                       Style="{StaticResource FluxHeadingSection}" />

            <!-- Progress Bar -->
            <ProgressBar Grid.Row="1" Grid.ColumnSpan="3"
                         Value="{Binding Progress}" Maximum="100"
                         Style="{StaticResource FluxProgressBarStyle}" />
        </Grid>
    </Border>
</UserControl>
```

**Visual States:**
- Default: Glass panel
- Active: `bg-indigo-600/90`, `ring-1 ring-indigo-400`
- Delayed: Status badge shows amber

**Accessibility:**
- `AutomationProperties.Name` = "Orange Line to Montmorency, arriving in 3 minutes"
- Keyboard focusable

### 5.3 AI Smart Planner

**Control Mapping:** Panel with TextBox, ChipGroup (Favorites), Button

**Structure:**
```xml
<Border Style="{StaticResource FluxGlassPanelStyle}">
    <utu:AutoLayout Spacing="16" Orientation="Vertical">
        <!-- Destination Input -->
        <TextBox x:Uid="Dashboard.TextBox.Destination"
                 PlaceholderText="Where to?"
                 Text="{Binding Destination, Mode=TwoWay}" />

        <!-- Preferences Input (optional) -->
        <TextBox x:Uid="Dashboard.TextBox.Preferences"
                 PlaceholderText="Preferences (e.g., avoid stairs)"
                 Text="{Binding Preferences, Mode=TwoWay}" />

        <!-- Favorites Chips -->
        <utu:ChipGroup ItemsSource="{Binding Favorites}"
                       Style="{StaticResource InputChipGroupStyle}"
                       SelectionChanged="OnFavoriteSelected">
            <utu:ChipGroup.ItemTemplate>
                <DataTemplate>
                    <utu:Chip Content="{Binding Name}" />
                </DataTemplate>
            </utu:ChipGroup.ItemTemplate>
        </utu:ChipGroup>

        <!-- Find Routes Button -->
        <Button x:Uid="Dashboard.Button.FindRoutes"
                Content="Find Best Routes"
                Command="{Binding FindRoutesCommand}"
                Style="{StaticResource FluxPrimaryButtonStyle}">
            <utu:ControlExtensions.Icon>
                <FontIcon Glyph="&#xE8F1;" />
            </utu:ControlExtensions.Icon>
        </Button>
    </utu:AutoLayout>
</Border>
```

**Loading State:** Button shows ProgressRing, text changes to "Optimizing Route..."
**Performance:** AI response within 3s or show skeleton [MCP: FeedView loading states]

### 5.4 AI Insight Card

**Control Mapping:** Border with TextBlock, appears after AI response

**Visual:** Indigo-tinted glass panel with insight text

### 5.5 Network Status Widget

**Control Mapping:** Border with status indicator and text

**States:**
- Normal: Emerald dot + "Normal Service"
- Delay: Amber dot + "Delays on [Line]"
- Down: Rose dot + "Service Disruption"

### 5.6 Alert Banner

**Control Mapping:** `InfoBar` control [MCP: InfoBar implemented for WASM, Skia, Mobile]

```xml
<InfoBar x:Uid="Dashboard.InfoBar.Alert"
         IsOpen="{Binding HasAlert}"
         Severity="{Binding AlertSeverity}"
         Title="{Binding AlertTitle}"
         Message="{Binding AlertMessage}" />
```

### 5.7 Crowd Chart

**Control Mapping:** LiveCharts2 `CartesianChart` with `LineSeries` (area fill)

**Package:** `LiveChartsCore.SkiaSharpView.Uno.WinUI`

```xml
<lvc:CartesianChart Series="{Binding CrowdSeries}"
                    XAxes="{Binding XAxes}"
                    YAxes="{Binding YAxes}">
</lvc:CartesianChart>
```

```csharp
// In DashboardModel
public ISeries[] CrowdSeries => new ISeries[]
{
    new LineSeries<CrowdDataPoint>
    {
        Values = CrowdData,
        Fill = new SolidColorPaint(SKColors.Indigo.WithAlpha(100)),
        Stroke = new SolidColorPaint(SKColors.Indigo),
        GeometryFill = null,
        GeometryStroke = null
    }
};
```

**Styling:** Match Flux color palette with indigo fill, slate background

### 5.8 Progress Bar (Vehicle Position)

**Control Mapping:** `ProgressBar` with custom style

```xml
<Style x:Key="FluxProgressBarStyle" TargetType="ProgressBar">
    <Setter Property="Height" Value="6" />
    <Setter Property="Background" Value="#334155" />  <!-- slate-700/50 -->
    <Setter Property="Foreground" Value="{StaticResource FluxPrimaryBrush}" />
    <Setter Property="CornerRadius" Value="3" />
</Style>
```

**Animation:** CSS/Storyboard transition on Value changes for 60fps smoothness

---

## 6. Architecture

### Layers

```
┌─────────────────────────────────────────────────┐
│                 Presentation                     │
│  Pages, Controls, Styles, Resources              │
├─────────────────────────────────────────────────┤
│                    Models                        │
│  DashboardModel, ProfileModel (MVUX records)     │
├─────────────────────────────────────────────────┤
│                   Services                       │
│  ITransitService, IAIService, IStorageService    │
├─────────────────────────────────────────────────┤
│                    Business                      │
│  Route calculation, Data transformation          │
├─────────────────────────────────────────────────┤
│                 DataContracts                    │
│  DTOs, API models, Entities                      │
└─────────────────────────────────────────────────┘
```

### Pattern Choice: MVUX

**Justification:** [MCP: MVUX reactive state, IState, IFeed]
- Real-time data updates (live tracking simulation)
- Async AI responses with loading states
- Built-in FeedView for progress/error/data states
- Simpler than manual INotifyPropertyChanged

### Solution Structure

```
FluxTransit/
├── FluxTransit/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── Presentation/
│   │   ├── Shell.xaml              # Navigation shell with TabBar
│   │   ├── DashboardPage.xaml
│   │   ├── ProfileDialog.xaml      # ContentDialog
│   │   └── Controls/
│   │       ├── TransitCard.xaml
│   │       ├── StatusPill.xaml
│   │       ├── AIPlannerPanel.xaml
│   │       ├── NetworkStatusWidget.xaml
│   │       └── CrowdChart.xaml
│   ├── Models/
│   │   ├── DashboardModel.cs       # MVUX partial record
│   │   └── ProfileModel.cs
│   ├── Services/
│   │   ├── ITransitService.cs
│   │   ├── MockTransitService.cs
│   │   ├── IAIService.cs
│   │   └── MockAIService.cs
│   ├── Business/
│   │   └── RouteCalculator.cs
│   ├── DataContracts/
│   │   ├── TransitRoute.cs
│   │   ├── GeminiRouteResponse.cs
│   │   ├── CrowdDataPoint.cs
│   │   └── UserProfile.cs
│   ├── Strings/
│   │   ├── en/
│   │   │   └── Resources.resw
│   │   └── fr/
│   │       └── Resources.resw
│   ├── Styles/
│   │   ├── Colors.xaml
│   │   ├── Typography.xaml
│   │   ├── Controls.xaml
│   │   └── GlassPanel.xaml
│   └── appsettings.json
└── FluxTransit.Tests/
```

### Dependency Injection

```csharp
// App.xaml.cs or HostBuilder setup
services.AddSingleton<ITransitService, MockTransitService>();
services.AddSingleton<IAIService, MockAIService>();
services.AddTransient<DashboardModel>();
services.AddTransient<ProfileModel>();
```

### Navigation (Region-Based)

```csharp
// Route registration
routes
    .Register(
        new RouteMap("", View: views.FindByViewModel<ShellModel>(),
            Nested: new RouteMap[]
            {
                new RouteMap("Dashboard", View: views.FindByViewModel<DashboardModel>(), IsDefault: true),
                new RouteMap("Profile", View: views.FindByViewModel<ProfileModel>(),
                    Qualifiers: Qualifiers.Dialog)  // Opens as modal
            }
        )
    );
```

### State Management (MVUX)

```csharp
public partial record DashboardModel(
    ITransitService TransitService,
    IAIService AIService,
    INavigator Navigator)
{
    // Editable state for inputs
    public IState<string> Destination => State<string>.Value(this, () => string.Empty);
    public IState<string> Preferences => State<string>.Value(this, () => string.Empty);

    // Feed for live routes (auto-refreshes)
    public IListFeed<TransitRoute> ActiveRoutes => ListFeed
        .Async(async ct => await TransitService.GetActiveRoutesAsync(ct))
        .RefreshOn(TimeSpan.FromSeconds(10));  // Simulate real-time

    // Feed for AI-generated routes (on-demand)
    public IFeed<GeminiRouteResponse> AIRoutes => Feed
        .Async(async ct => await AIService.GenerateRoutesAsync(
            await Destination,
            await Preferences,
            ct));

    // State for network status
    public IFeed<NetworkStatus> NetworkStatus => Feed
        .Async(async ct => await TransitService.GetNetworkStatusAsync(ct));

    // Command - auto-detected
    public async ValueTask FindRoutes(CancellationToken ct)
    {
        // Triggers AIRoutes feed refresh
        await AIRoutes.RefreshAsync(ct);
    }
}
```

---

## 7. Data + Integrations

### Data Entities

```csharp
public record TransitRoute(
    string Id,
    string Name,
    string LineCode,        // "O" for Orange, "G" for Green, etc.
    TransitType Type,       // BUS or METRO
    string ColorHex,
    ImmutableList<string> Stops,
    int Progress,           // 0-100 position between stops
    RouteStatus Status      // Normal, Delayed, Down
);

public enum TransitType { BUS, METRO }
public enum RouteStatus { Normal, Delayed, Down }

public record GeminiRouteResponse(
    ImmutableList<SuggestedRoute> Routes,
    string Insight           // AI-generated recommendation
);

public record SuggestedRoute(
    string Id,
    string Summary,
    TimeSpan Duration,
    ImmutableList<RouteStep> Steps
);

public record CrowdDataPoint(
    string Time,            // "06:00", "07:00", etc.
    int Value               // 0-100 density percentage
);

public record UserProfile(
    string Name,
    decimal OpusBalance,
    int WeeklyTripCount,
    ImmutableList<Favorite> Favorites
);

public record Favorite(string Name, string Destination);
```

### API Contracts (Future - Currently Mocked)

**STM GTFS-Realtime:** Future integration for real transit data
**Gemini API:** `gemini-2.5-flash` model for route suggestions

```csharp
public interface IAIService
{
    Task<GeminiRouteResponse> GenerateRoutesAsync(
        string destination,
        string preferences,
        CancellationToken ct);
}
```

### Caching Strategy

| Data | Cache Location | TTL | Notes |
|------|----------------|-----|-------|
| Active Routes | Memory | 10s | Simulates real-time updates |
| Network Status | Memory | 30s | |
| AI Responses | Memory | Session | Don't re-query same destination |
| User Profile | Browser localStorage | Persistent | OPUS balance, favorites |

### Offline Support

**Minimal** - App is primarily online. Show cached last-known state with "Last updated X minutes ago" indicator.

### Local Storage

**WebAssembly:** Browser localStorage via `Uno.Extensions.Storage` [MCP: Storage extension]
**Desktop:** ApplicationData

---

## 8. Telemetry + Logging

### Logging

```csharp
// Using ILogger via Uno.Extensions.Logging
services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Debug));
```

### Analytics Events (Future)

| Event | Properties |
|-------|------------|
| `route_search` | destination, preferences, result_count |
| `route_selected` | route_id, route_type |
| `network_status_viewed` | status |

---

## 9. Testing Strategy

### Unit Tests

- `DashboardModel` state transitions
- `RouteCalculator` business logic
- Service mocks

### UI Tests

- Navigation flow
- Loading states display correctly
- Responsive breakpoints

### Platform Testing

| Platform | Priority | Notes |
|----------|----------|-------|
| WebAssembly (Chrome) | P0 | Primary target |
| WebAssembly (Safari) | P1 | iOS Safari users |
| Windows | P2 | Desktop validation |

---

## 10. Build Plan

### Slice 1: Foundation (Shell + Theme)
- [ ] Create project with UnoFeatures
- [ ] Set up ResourceDictionary with Flux color palette
- [ ] Create Shell.xaml with responsive TabBar (Bottom/Vertical)
- [ ] Configure navigation routes
- [ ] Create basic DashboardPage skeleton

### Slice 2: Header + Status Pills
- [ ] Create StatusPill control
- [ ] Implement header layout with greeting, weather mock, OPUS balance, trip count
- [ ] Style with glass panel aesthetic

### Slice 3: Transit Cards + Live Tracking (Real GTFS)
- [ ] Create TransitRoute data model
- [ ] Create TransitCard UserControl
- [ ] Implement STM GTFS-Realtime service integration
- [ ] Display routes in ItemsRepeater with FeedView
- [ ] Add progress bar animation from real position data

### Slice 4: AI Smart Planner
- [ ] Create AI planner panel with inputs
- [ ] Implement Favorites ChipGroup
- [ ] Create MockAIService
- [ ] Wire up FindRoutes command
- [ ] Add LoadingView for "Optimizing..." state
- [ ] Display AI Insight card on success

### Slice 5: Network Status + Alerts
- [ ] Create NetworkStatusWidget
- [ ] Implement status feed in DashboardModel
- [ ] Add InfoBar for alert banners

### Slice 6: Crowd Chart (LiveCharts2)
- [ ] Add LiveChartsCore.SkiaSharpView.Uno.WinUI package
- [ ] Implement CrowdDataPoint feed
- [ ] Create CartesianChart with area fill styling
- [ ] Match Flux color palette

### Slice 7: Profile Modal
- [ ] Create ProfileDialog (ContentDialog)
- [ ] Implement OPUS balance display (manual entry)
- [ ] Add favorites management

### Slice 8: Polish + Responsive
- [ ] Fine-tune glassmorphism styling
- [ ] Test responsive breakpoints (mobile < 600px, desktop)
- [ ] Add hover states and micro-interactions
- [ ] Implement 60fps animations

### Slice 9: Localization
- [ ] Add EN resources
- [ ] Add FR resources (station names)
- [ ] Test language switching

### Risks

| Risk | Mitigation |
|------|------------|
| Glassmorphism performance on WASM | Use solid semi-transparent colors, avoid heavy blur |
| Charting library compatibility | Have fallback simple visualization |
| 60fps animations on low-end devices | Use CSS transforms, avoid layout triggers |

### Performance Considerations

- LCP < 1.5s: Minimize initial bundle, lazy load non-critical components
- 60fps animations: Use `ProgressBar` with CSS transitions, avoid JavaScript-driven animation
- AI latency: Show skeleton states immediately, stream response if possible

### Accessibility Considerations

- All interactive elements keyboard focusable
- `AutomationProperties.Name` on Transit Cards with full context
- High contrast text (white on dark meets WCAG AA)
- Touch targets minimum 44x44px

---

## 11. Resolved Questions

| Item | Decision | Rationale |
|------|----------|-----------|
| **Charting Library** | **LiveCharts2** | Open source, cross-platform compatible |
| **Real GTFS Data** | **ASAP** | Build with real STM GTFS-Realtime data from start |
| **Map Integration** | **Abstract only** | Keep dashboard abstract, no map layer |
| **OPUS Balance Storage** | **Browser localStorage** | Via Uno.Extensions.Storage |
| **Glassmorphism Blur** | **Solid semi-transparent fallback** | No AcrylicBrush - use solid colors for cross-platform consistency |

### Sample App Notes

| Item | Decision |
|------|----------|
| **Gemini API Key** | User provides own key via settings/environment variable (sample app, not production) |

---

## 12. MCP References

### Queries Used

1. `uno_platform_docs_search("NavigationView shell navigation")` - Confirmed TabBar for responsive shell
2. `uno_platform_docs_search("TabBar bottom navigation mobile Toolkit")` - Bottom and Vertical TabBar styles
3. `uno_platform_docs_search("MVUX reactive state IState IFeed")` - Confirmed MVUX pattern for reactive UI
4. `uno_platform_docs_search("ProgressBar ProgressRing loading indicator")` - Progress controls available
5. `uno_platform_docs_search("Card glass panel styling")` - Card control from Toolkit
6. `uno_platform_docs_search("Responsive markup extension")` - Responsive breakpoints
7. `uno_platform_docs_search("ContentDialog modal flyout")` - Dialog navigation with `!` qualifier
8. `uno_platform_docs_search("Chip ChipGroup tags pills")` - Chip for favorites
9. `uno_platform_docs_search("ItemsRepeater ListView")` - List display options
10. `uno_platform_docs_search("InfoBar notification banner")` - Alert banners
11. `uno_platform_docs_search("LoadingView skeleton loading")` - Loading states
12. `uno_platform_docs_search("FeedView MVUX loading")` - Async data display
13. `uno_platform_docs_search("localization strings resources")` - EN/FR support
14. `uno_platform_docs_search("HTTP Kiota API")` - Future API integration

### Key Documentation Fetched

1. `uno_platform_docs_fetch("TabBarAndTabBarItem.md")` - Full TabBar API and styling
2. `uno_platform_docs_fetch("LoadingView.md")` - Loading state implementation

### Uno Platform Conventions Applied

- "Uno Platform" naming (never "Uno")
- No `{Binding StringFormat}` - use `<Run>` elements
- ThemeResource/StaticResource for all colors
- Material theme as base (customized)
- x:Uid for all user-facing strings
- MVUX preferred for reactive patterns
- Kiota preferred for HTTP (future)
