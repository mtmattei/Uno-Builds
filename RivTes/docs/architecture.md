# Tesla × Riviera GCC — Architecture Brief
### Uno Platform Cross-Platform Implementation
**Project Codename:** Phosphor Protocol  
**Stack:** C# / WinUI 3 / XAML / Uno Platform (Skia Renderer)  
**Targets:** Windows, iOS, Android, macOS, Linux, WebAssembly  
**Architecture Pattern:** MVUX (Model-View-Update eXtended)  
**Navigation:** Uno Navigation Extensions (Region-based)

---

## 1. Concept

A Tesla Model 3 dashboard interface rendered through the aesthetic of the 1986 Buick Riviera Graphic Control Center — a CRT touchscreen embedded in leather and walnut. The app is landscape-only and simulates a physical CRT display with phosphor glow, scan lines, and a chrome bezel surround. All UI is monochromatic teal (#6FFCF6) on near-black, with amber (#D4A832) as a functional accent for heat indicators.

This is not a retro terminal. It is a luxury instrument panel — spacious, warm, and precise.

---

## 2. Solution Structure

```
PhosphorProtocol/
├── PhosphorProtocol.sln
├── PhosphorProtocol/
│   ├── App.xaml                          # Global resources, theme, merged dictionaries
│   ├── App.xaml.cs                       # Host builder, DI, services
│   ├── Themes/
│   │   ├── ColorPaletteOverride.xaml     # Teal phosphor palette mapped to Material slots
│   │   ├── PhosphorBrushes.xaml          # Custom brush resources (glow, ghost, dim, etc.)
│   │   ├── Typography.xaml               # TextBlock style overrides (Share Tech Mono, Orbitron)
│   │   ├── ButtonStyles.xaml             # GCC touch-zone button styles
│   │   └── ControlOverrides.xaml         # Slider, ToggleSwitch, ProgressBar restyling
│   ├── Controls/
│   │   ├── CRTFrame.xaml/.cs             # The physical CRT bezel + glass + scan lines
│   │   ├── GCCButton.xaml/.cs            # Chunky rounded-rect phosphor touch zone
│   │   ├── PhosphorText.xaml/.cs         # Styled text with optional glow
│   │   ├── ArcGauge.xaml/.cs             # SkiaSharp arc gauge (speed, RPM, temp, volts)
│   │   ├── BarGauge.xaml/.cs             # Segmented horizontal bar (fuel, oil pressure)
│   │   ├── BatteryIndicator.xaml/.cs     # Segmented battery bar with color grading
│   │   ├── SeatHeaterButton.xaml/.cs     # SVG seat icon with amber heat wave indicators
│   │   ├── FanSpeedIndicator.xaml/.cs    # Graduated bar segments for fan level
│   │   └── ScanLineOverlay.xaml/.cs      # Full-screen CRT effect layer
│   ├── SkiaControls/
│   │   ├── DrivingVisualization.cs       # SKXamlCanvas — top-down car, lanes, vehicles
│   │   ├── VectorNavMap.cs               # SKXamlCanvas — city grid, route, destination
│   │   ├── SpectrumVisualizer.cs         # SKXamlCanvas — audio spectrum bars
│   │   └── EnergyGraph.cs               # SKXamlCanvas — consumption chart with area fill
│   ├── Views/
│   │   ├── BootPage.xaml/.cs             # POST-style boot sequence
│   │   ├── DashboardShell.xaml/.cs       # Main layout: status bar + left/right + climate
│   │   ├── NavView.xaml/.cs              # Navigation map with turn instructions
│   │   ├── MediaView.xaml/.cs            # Source selector, now-playing, spectrum, presets
│   │   ├── EnergyView.xaml/.cs           # Stats, consumption graph, power flow
│   │   ├── ChargeView.xaml/.cs           # Battery visual, charge stats, nearby chargers
│   │   └── ControlsView.xaml/.cs         # Toggles, headlights, openings, vehicle info
│   ├── Models/
│   │   ├── VehicleState.cs               # Immutable record: speed, gear, battery, temp, etc.
│   │   ├── ClimateState.cs               # Immutable record: temp, fan, seat heat, A/C, defrost
│   │   ├── MediaState.cs                 # Immutable record: source, track, progress, presets
│   │   ├── NavigationState.cs            # Immutable record: route, turn, ETA, position
│   │   ├── EnergyState.cs               # Immutable record: consumption data, range, draw
│   │   └── ChargeState.cs               # Immutable record: battery %, rate, nearby chargers
│   ├── Presentation/
│   │   ├── DashboardModel.cs             # MVUX model — root state, gear, autopilot, time
│   │   ├── ClimateModel.cs               # MVUX model — climate controls
│   │   ├── MediaModel.cs                 # MVUX model — media playback
│   │   ├── NavModel.cs                   # MVUX model — navigation
│   │   ├── EnergyModel.cs               # MVUX model — energy monitoring
│   │   ├── ChargeModel.cs               # MVUX model — charge status
│   │   └── ControlsModel.cs             # MVUX model — vehicle controls
│   ├── Services/
│   │   ├── IVehicleService.cs            # Interface: speed, gear, battery, sensor data
│   │   ├── IClimateService.cs            # Interface: HVAC state, set temp/fan
│   │   ├── IMediaService.cs              # Interface: playback, seek, presets
│   │   ├── INavigationService.cs         # Interface: route, turn-by-turn, ETA
│   │   ├── MockVehicleService.cs         # Demo implementation with simulated data
│   │   └── MockClimateService.cs         # Demo implementation
│   └── Assets/
│       ├── Fonts/
│       │   ├── ShareTechMono-Regular.ttf
│       │   └── Orbitron-Regular.ttf / Orbitron-Bold.ttf
│       └── Lottie/
│           ├── boot-cursor-blink.json
│           └── scan-line-sweep.json
```

---

## 3. Navigation Architecture

### Boot → Dashboard Shell (Frame Navigation)

The app launches into `BootPage`, which plays a timed POST sequence, then navigates to `DashboardShell` via frame navigation. No back-stack — boot is a one-way transition.

### Dashboard Shell (Region Navigation)

`DashboardShell` is a persistent layout with four zones:

| Zone | Content | Navigation Type |
|------|---------|-----------------|
| **Status Bar** | Time, gear, autopilot, battery | Always visible, bound to `DashboardModel` |
| **Left Panel** | `DrivingVisualization` + speed overlay | Always visible, no navigation |
| **Right Panel** | Switchable views (Nav/Media/Energy/Charge/Controls) | Region-based with `TabBar` |
| **Climate Bar** | Temperature, fan, seat heat, A/C, defrost | Always visible, bound to `ClimateModel` |

The right panel uses Uno's **Region Navigation** with a `TabBar` at the top:

```xml
<Grid uen:Region.Attached="True">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />  <!-- TabBar -->
        <RowDefinition Height="*" />     <!-- Content -->
    </Grid.RowDefinitions>

    <utu:TabBar uen:Region.Attached="True">
        <utu:TabBarItem uen:Region.Name="Nav" Content="NAV" />
        <utu:TabBarItem uen:Region.Name="Media" Content="MEDIA" />
        <utu:TabBarItem uen:Region.Name="Energy" Content="ENERGY" />
        <utu:TabBarItem uen:Region.Name="Charge" Content="CHARGE" />
        <utu:TabBarItem uen:Region.Name="Controls" Content="CTRL" />
    </utu:TabBar>

    <Grid Grid.Row="1" uen:Region.Attached="True" uen:Region.Navigator="Visibility">
        <views:NavView uen:Region.Name="Nav" />
        <views:MediaView uen:Region.Name="Media" />
        <views:EnergyView uen:Region.Name="Energy" />
        <views:ChargeView uen:Region.Name="Charge" />
        <views:ControlsView uen:Region.Name="Controls" />
    </Grid>
</Grid>
```

---

## 4. MVUX State Architecture

Each domain has an immutable record and a reactive MVUX model:

```csharp
// Immutable state record
public record VehicleState(
    int Speed,
    string Gear,
    bool AutopilotActive,
    int BatteryPercent,
    int RangeMiles,
    int SpeedLimit,
    DateTime CurrentTime);

// MVUX Model — generates bindable proxy automatically
public partial record DashboardModel(
    IVehicleService VehicleService)
{
    public IState<VehicleState> Vehicle => State.Async(this,
        async ct => await VehicleService.GetCurrentState(ct));

    public IState<string> ActiveGear => State.Value(this, () => "D");
    public IState<bool> Autopilot => State.Value(this, () => true);

    public async ValueTask ToggleAutopilot(CancellationToken ct)
    {
        await Autopilot.Update(current => !current, ct);
    }
}
```

```csharp
public partial record ClimateModel(IClimateService ClimateService)
{
    public IState<int> Temperature => State.Value(this, () => 72);
    public IState<int> FanSpeed => State.Value(this, () => 3);
    public IState<int> SeatHeatLeft => State.Value(this, () => 1);
    public IState<int> SeatHeatRight => State.Value(this, () => 0);
    public IState<bool> ACEnabled => State.Value(this, () => true);
    public IState<bool> DefrostEnabled => State.Value(this, () => false);

    public async ValueTask IncrementTemp(CancellationToken ct)
        => await Temperature.Update(t => Math.Min(85, t + 1), ct);

    public async ValueTask DecrementTemp(CancellationToken ct)
        => await Temperature.Update(t => Math.Max(60, t - 1), ct);

    public async ValueTask CycleFan(CancellationToken ct)
        => await FanSpeed.Update(f => f < 5 ? f + 1 : 0, ct);

    public async ValueTask CycleSeatHeatLeft(CancellationToken ct)
        => await SeatHeatLeft.Update(h => (h + 1) % 4, ct);
}
```

---

## 5. Custom Rendering (SkiaSharp)

Four complex visual components use `SKXamlCanvas` for pixel-level control:

### 5.1 DrivingVisualization

A continuously animated top-down driving scene rendered at ~30fps via `SKXamlCanvas.PaintSurface`. Elements:

| Element | Rendering Technique |
|---------|-------------------|
| Road surface | `SKPaint` filled rounded rect |
| Lane dividers (scrolling) | Dashed `SKPaint` with animated `PathEffect.DashOffset` |
| Edge lines | Solid `SKPaint` strokes |
| Autopilot guide rails | Layered strokes: wide glow (8px, 0.15 alpha) + sharp core (2.5px) |
| Detected vehicles (3) | `RoundRect` stroke + windshield path + taillight fill |
| Distance bracket | Dashed vertical lines + label with background pill |
| Your car | Double-stroke body (4px glow + 1.8px sharp), windshield/rear paths, headlight rects with `MaskFilter.Blur`, beam gradient, taillights with red glow, wheels, mirrors |
| Traffic light | Rounded rect frame + green circle with blur shadow |

Animation is driven by a `DispatcherTimer` at 33ms interval, incrementing a tick counter. Lane scroll offset = `tick * (speed / 20) % 42`. Vehicle positions oscillate with `Math.Sin(tick * 0.018 + lane * 3)`.

### 5.2 VectorNavMap

SVG-like city grid rendered on a `SKXamlCanvas`:

- Grid lines at regular intervals (minor: ghost, major: dim)
- Named road labels rendered with `SKPaint` text
- Building blocks as filled rounded rects
- Active route as a thick polyline with gradient shader (`SKShader.CreateLinearGradient`)
- Turn markers as circles with text
- Car position as a pulsing dot (animated radius via sine wave)
- Direction cone as a filled triangle
- ETA overlay as a rounded rect with text

### 5.3 SpectrumVisualizer

28 vertical bars driven by a feed of audio levels. Each bar's height interpolates toward a target value (`current + (target - current) * 0.25`). Color thresholds: `> 0.5 → peak`, `> 0.3 → bright`, else `glow`. Updated at 70ms intervals.

### 5.4 EnergyGraph

Line chart with area fill. X-axis: 40 data points over 30 minutes. Y-axis: 0–400 Wh/mi. Rendered as:
- Grid lines (dashed, ghost)
- Area fill path (`SKPaint` with `peak` at 4% opacity)
- Line path (1.8px bright with drop shadow)
- Animated cursor line at right edge (pulsing opacity)

---

## 6. View Specifications

### 6.1 BootPage

A full-screen dark CRT frame displaying a sequential text log:

| Line | Content | Delay |
|------|---------|-------|
| 1 | DELCO ELECTRONICS — KOKOMO, IN | 260–380ms |
| 2 | GCC v3.0 — TEAL PHOSPHOR DISPLAY | 260–380ms |
| 3 | TESLA VEHICLE BUS ... LINKED | 260–380ms |
| 4 | AUTOPILOT .......... READY | 260–380ms |
| 5 | SYSTEMS NOMINAL | 260–380ms |
| — | ENTERING GCC ... | 600ms hold, then navigate |

Each line fades in with a 5px upward slide (`TranslateTransform` + `DoubleAnimation`). The final line is followed by a blinking cursor (`█`, toggling opacity at 600ms via `Storyboard`). The CRT frame includes scan lines and vignette.

### 6.2 DashboardShell

The persistent root layout. Grid structure:

```
┌─────────────────────────────────────────────────────┐
│ STATUS BAR (full width)                              │
│ LTE | time | 72°F | P R N [D] | AUTOPILOT | ████ 74% │
├──────────────┬──────────────────────────────────────┤
│              │ TAB BAR: [NAV] [MEDIA] [ENERGY]...    │
│  DRIVING     ├──────────────────────────────────────┤
│  VISUALIZATION│                                      │
│  + SPEED     │  ACTIVE VIEW CONTENT                  │
│  + LIMIT     │  (Region-switched)                    │
│              │                                      │
├──────────────┴──────────────────────────────────────┤
│ CLIMATE BAR (full width)                             │
│ [seat] [fan] ◂ 72° ▸ | A/C | DEFR | [seat]         │
└─────────────────────────────────────────────────────┘
```

Grid definitions:
- Columns: `minmax(190px, 27%) | *`
- Rows: `38px | * | 40px`

### 6.3 NavView

Overlay-based layout over the `VectorNavMap` SkiaSharp canvas:
- Turn instruction card (top-left): arrow icon + distance + road name, frosted background
- Destination label (top-right): label + supercharger name
- The map fills the remaining space

### 6.4 MediaView

Vertical stack with padding:
1. **Source selector** — horizontal row of `GCCButton` instances: FM, STREAM, BT, USB
2. **Now Playing card** — `CardContentControl` with album art placeholder, track title (peak, glowing), artist (dim), progress bar (gradient fill), transport controls (prev/play-pause/next)
3. **Spectrum visualizer** — `SpectrumVisualizer` SkiaSharp control, 28 bars, 44px height
4. **FM Presets grid** — 3×2 grid of `GCCButton` instances with frequency + station name

### 6.5 EnergyView

Vertical stack:
1. **Stats row** — 4× `CardContentControl` (TRIP, AVG DRAW, INSTANT, RANGE) with label/value/unit
2. **Consumption graph** — `EnergyGraph` SkiaSharp control
3. **Power flow** — horizontal layout: "BATTERY 74%" → animated chevron chain → "MOTOR 52 kW"

Power flow animation: 4 small bars that sequence their opacity using staggered `DoubleAnimation` keyframes, creating a left-to-right energy flow illusion.

### 6.6 ChargeView

Centered vertical layout:
1. **Battery bar** — 28 segments in a horizontal row, color-graded (amber → glow → bright), with an end cap
2. **Big percentage** — Orbitron 56px, peak with 8px glow
3. **Stats row** — RANGE, RATE, LIMIT
4. **Nearby Superchargers** — `ListView` with station name, distance, and availability count

### 6.7 ControlsView

Scrollable vertical stack:
1. **Toggles** — 2×1 grid: DOOR LOCKS, FOLD MIRRORS (custom `ToggleSwitch` style with phosphor pill)
2. **Headlights** — 4× `GCCButton` row: OFF, PARK, AUTO, ON (single-select)
3. **Openings** — 2×1 grid: TRUNK, FRUNK (toggle buttons with OPEN/CLOSED state)
4. **Vehicle info** — `CardContentControl` with key-value pairs: MODEL, VIN, FW, ODO

---

## 7. Services & Dependency Injection

```csharp
// In App.xaml.cs
private static void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<IVehicleService, MockVehicleService>();
    services.AddSingleton<IClimateService, MockClimateService>();
    services.AddSingleton<IMediaService, MockMediaService>();
    services.AddSingleton<INavigationService, MockNavigationService>();
}
```

`MockVehicleService` generates simulated data using `PeriodicTimer` at 500ms intervals, pushing updates through `IState`. In production, these interfaces would bind to a CAN bus adapter or Tesla API.

---

## 8. Performance Considerations

- **SkiaSharp canvases** render on a background thread via `SKXamlCanvas`, avoiding UI thread blocking
- **DrivingVisualization** uses `DispatcherTimer` at 33ms (30fps), not `CompositionTarget.Rendering`
- **Spectrum bars** update at 70ms — fast enough for visual smoothness, slow enough to avoid GC pressure
- **Region navigation** uses `Visibility` switching (not frame navigation) for instant tab changes with no page creation overhead
- **Immutable records** in MVUX prevent unnecessary re-renders — only changed properties trigger binding updates
- **Font loading** — custom fonts (Share Tech Mono, Orbitron) are bundled as assets, not loaded from network

---

## 9. Responsive Behavior

The dashboard is designed landscape-first. The `Responsive` markup extension adapts the layout:

| Width | Left Panel | Right Panel | Climate Bar |
|-------|-----------|-------------|-------------|
| < 700px (phone landscape) | 35%, speed text smaller (36px) | Tabs stack to icons only | Single-row compact |
| 700–1100px (tablet) | 27%, speed at 50px | Full tab labels | Full layout |
| > 1100px (desktop) | 27% capped at 300px | MaxWidth 800px centered | Full layout |

The `CRTFrame` control scales its border-radius, bezel thickness, and chrome gradient based on available width.

---

## 10. UnoFeatures Required

```xml
<UnoFeatures>
    Material;
    Toolkit;
    Extensions;
    ExtensionsCore;
    Navigation;
    MVUX;
    Hosting;
    Skia;
    SkiaRenderer;
    Logging;
    ThemeService;
</UnoFeatures>
```

Additional NuGet packages:
- `SkiaSharp.Views.Uno.WinUI` — for `SKXamlCanvas`
- `CommunityToolkit.WinUI.Lottie` — for boot cursor and ambient animations

---

## 11. Build & Target Configuration

| Target | TFM | Notes |
|--------|-----|-------|
| Windows | net10.0-windows10.0.22621 | Native WinUI 3 |
| iOS | net10.0-ios | Landscape lock via Info.plist |
| Android | net10.0-android | Landscape lock via AndroidManifest |
| macOS | net10.0-macos | Skia renderer |
| Linux | net10.0-desktop | Skia + GTK |
| WebAssembly | net10.0-browserwasm | Skia renderer, landscape CSS hint |
