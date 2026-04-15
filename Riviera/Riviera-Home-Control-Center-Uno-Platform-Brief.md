# Riviera Home Control Center — Uno Platform Implementation Brief

**Smart Home Dashboard inspired by the 1986 Buick Riviera Graphic Control Center**

| | |
|---|---|
| **Architecture** | MVUX (Model-View-Update eXtended) |
| **Framework** | Uno Platform 6.x + Uno Toolkit + SkiaSharp |
| **Targets** | Windows, WebAssembly, iOS, Android, macOS, Linux |
| **Date** | March 2026 |

---

## 1. Executive Summary

This document is a comprehensive implementation brief for building the Riviera Home Control Center as a cross-platform Uno Platform application. The app is a smart home dashboard whose visual language is directly inspired by the 1986 Buick Riviera Graphic Control Center (GCC): monochrome green phosphor CRT aesthetics, blocky touch-zone navigation, scan-line overlays, and a dark bezel frame. The prototype was built as a React artifact and this brief translates every visual, interaction, and state into the correct Uno Platform components, patterns, and best practices.

The brief is organized panel-by-panel, covering all five top-level destinations (Climate, Security, Energy, Lighting, Diagnostics), the persistent shell/chrome, every reusable custom control (gauge arcs, horizontal bars, toggle switches, touch zone buttons), all interactive states, animations, responsive breakpoints, accessibility requirements, and edge cases.

---

## 2. Project Setup and UnoFeatures

### 2.1 Solution Template

Create the project using `dotnet new unoapp` with the following UnoFeatures enabled. The app should target the Uno Skia Renderer backend so all UI controls work identically on Windows, WebAssembly, iOS, Android, macOS, and Linux.

| Setting | Value |
|---|---|
| **Template** | `dotnet new unoapp -n RivieraHome` |
| **UnoFeatures** | Material, Toolkit, MVUX, Navigation, Hosting, Logging, Configuration, Skia, Extensions, ThemeService |
| **Additional NuGet** | SkiaSharp.Views.Uno.WinUI (for custom gauge/arc rendering) |
| **Target Frameworks** | net9.0-windows10, net9.0-browserwasm, net9.0-ios, net9.0-android, net9.0-maccatalyst, net9.0-desktop |
| **Min Uno Platform** | 6.0+ |
| **Architecture** | MVUX (Model-View-Update eXtended) with IState/IFeed |

### 2.2 Folder Structure

Organize the project into the following logical folders. Separate Models (immutable records), Presentation (pages and their paired MVUX models), Controls (reusable custom controls), Services (data/device service interfaces and implementations), and Themes (ResourceDictionaries).

| Folder | Contents |
|---|---|
| `Presentation/` | Pages and their MVUX Models (e.g., `ClimatePage.xaml` + `ClimateModel.cs`) |
| `Presentation/Shell/` | `MainPage.xaml` (shell with TabBar), `ShellModel.cs` |
| `Controls/` | `GaugeArc.cs`, `HorizontalBar.cs`, `PhosphorToggle.cs`, `TouchZoneButton`, `CrtOverlay`, `ScanLineOverlay`, `DigitalClock` |
| `Models/` | Immutable record types: `ClimateData`, `SecurityData`, `EnergyData`, `LightingData`, `DiagnosticsData`, `ZoneInfo`, `DoorInfo`, `CameraInfo`, etc. |
| `Services/` | `ISmartHomeService`, `SmartHomeService` (mock/real), `IClockService` |
| `Themes/` | `ColorPaletteOverride.xaml`, `CrtTheme.xaml`, `ControlOverrides/` directory |
| `Assets/` | App icons, embedded fonts, any bitmap assets |

---

## 3. Design System: CRT Phosphor Theme

*Every visual decision flows from the 1986 Buick Riviera GCC. This is a monochrome green-on-black CRT aesthetic.*

### 3.1 Color Palette

All colors must be defined as ThemeResources in `ColorPaletteOverride.xaml` or `CrtTheme.xaml`. Never use hardcoded hex values inline in XAML.

| Resource Key | Value | Usage |
|---|---|---|
| `CrtBackground` | `#050A06` | Primary screen background (near-black with green tint) |
| `CrtBackgroundDeep` | `#030703` | Inner shadow areas, vignette regions |
| `PhosphorPrimary` | `#33FF66` | Primary text, active indicators, gauge fill arcs |
| `PhosphorBright` | `#66FFAA` | Emphasized/bright text, active state highlights |
| `PhosphorDim` | `#1A9940` | Secondary text, inactive labels, dim gauge tracks |
| `PhosphorGlow` | `#33FF6659` (35% opacity) | Text-shadow glow, box-shadow glow effects |
| `PhosphorSubtle` | `#33FF6614` (8% opacity) | Active toggle/zone background tints |
| `BezelPrimary` | `#1A1A1A` | Outer bezel frame background |
| `BezelEdge` | `#2A2A2A` | Bezel highlight edge for depth |
| `BezelLabel` | `#555555` | Bezel branding text color |
| `ScanLineOverlay` | `#00000026` (15% opacity) | Horizontal scan-line stripe color |
| `BorderSubtle` | `#33FF6633` (20% opacity) | Section dividers, panel borders |
| `BorderActive` | `#33FF66` | Active element borders |

### 3.2 Typography

The GCC used a fixed-width dot-matrix style font. The app uses two font families to capture this: a monospace font for data readouts and labels, and a geometric display font for headings and large numerals. Both must be embedded as custom fonts.

| Role | Font | Usage |
|---|---|---|
| Data / Monospace | **Share Tech Mono** (Google Fonts) | All numeric readouts, status text, event logs, labels, clock digits |
| Display / Headings | **Orbitron** (Google Fonts) | Panel titles, large temperature displays, section headers |
| Fallback | Consolas or Courier New | System fallback if custom fonts unavailable |

Define custom TextBlock styles in a ResourceDictionary (`Themes/TextBlock.xaml`) referenced from `App.xaml`. Required styles:

- **CrtDisplayXL** — 72px Orbitron, PhosphorBright
- **CrtDisplayLG** — 48px Orbitron, PhosphorBright
- **CrtDisplayMD** — 28px Orbitron, PhosphorPrimary
- **CrtHeading** — 15px Orbitron, letter-spacing 0.15em, PhosphorBright
- **CrtBody** — 14px Share Tech Mono, PhosphorPrimary
- **CrtLabel** — 10px Share Tech Mono, PhosphorDim
- **CrtLogEntry** — 9px Share Tech Mono, PhosphorDim

All text styles must include a phosphor glow effect via a containing `ShadowContainer` or equivalent.

### 3.3 Glow and CRT Effects

The CRT phosphor glow is central to the visual identity. Implement using a combination of Uno Toolkit `ShadowContainer` for text/element glow, SkiaSharp blur filters for the screen-level vignette, and repeating pattern elements for scan lines.

| Effect | Implementation |
|---|---|
| **Text Glow** | Wrap key text elements in a `ShadowContainer` with a green-tinted `ThemeShadow` (Color=PhosphorGlow, BlurRadius=8, Offset=0). For critical readouts, use a double-shadow: one tight (blur 4) and one wide (blur 20). |
| **Scan Lines** | Create a `ScanLineOverlay` UserControl: a Grid overlaying the entire CRT screen area. Use a SkiaSharp `SKXamlCanvas` to draw horizontal lines every 4px with 15% black opacity. Set `IsHitTestVisible=False`. |
| **Vignette** | Create a `CrtVignetteOverlay` UserControl: a Border with a radial gradient background going from transparent center to 60% black at edges. Use `CornerRadius=18` to match the CRT bezel curve. `IsHitTestVisible=False`. |
| **Flicker** | A Storyboard with `RepeatBehavior=Forever` on the CRT screen container. Animate Opacity between 0.97 and 1.0 with `Duration=0:0:0.1` and `AutoReverse=True`. This is decorative only; provide a setting to disable it for accessibility. |
| **Bezel** | The outer frame is a Border with `Background=BezelPrimary`, `CornerRadius=24`, and a `ThemeShadow` (Translation Z=32). Inner CRT screen Border has `CornerRadius=18` and an inset shadow effect. |

### 3.4 Spacing and Grid System

Follow an 8px base grid. All spacing values must be multiples of 4 or 8. Use `AutoLayout` from Uno Toolkit for vertical and horizontal stacking with consistent Spacing. Never set margins or padding on children; use container Spacing and Padding instead.

| Token | Value |
|---|---|
| Bezel Padding | 14px outer frame padding |
| CRT Screen Padding | 16px vertical, 20px horizontal |
| Section Gap | 20px between grid columns |
| Subsection Gap | 12–16px between stacked elements |
| Element Gap | 6–8px between list items, toggles, etc. |
| Label-to-Value | 4px between a label and its associated readout |
| Touch Zone Gap | 6px between bottom navigation buttons |

---

## 4. Architecture: MVUX Pattern

*The app uses the MVUX pattern (Model-View-Update eXtended) from Uno Extensions. Each page has a partial record Model class that exposes `IFeed` and `IState` properties. The MVUX source generator produces a bindable ViewModel automatically.*

### 4.1 Data Models (Immutable Records)

All data entities are immutable C# records. The smart home service returns these records, and the MVUX model wraps them in `IState<T>` for mutable user-controlled state (e.g., target temperature) or `IFeed<T>` for read-only async data streams (e.g., sensor readings).

| Record | Fields | Notes |
|---|---|---|
| `ClimateData` | `int Temp, int TargetTemp, int Humidity, int OutsideTemp, ImmutableDictionary<string,bool> Zones` | Temp fluctuates from service; TargetTemp is user-controlled `IState` |
| `SecurityData` | `ImmutableDictionary<string,bool> Doors, ImmutableDictionary<string,bool> Cameras, int MotionEvents, ImmutableList<string> Log` | Doors: true=locked. Cameras: true=recording |
| `EnergyData` | `double SolarKw, double GridKw, double BatteryPct, double DailyKwh` | All values from simulated service polling |
| `LightingData` | `ImmutableDictionary<string,bool> Lights, int Brightness` | Brightness is user-controlled `IState` (25/50/75/100) |
| `DiagnosticsData` | `int CpuTemp, int MemPct, bool NetworkUp, int UptimeDays, int DeviceCount, ImmutableList<string> Alerts, ImmutableList<string> Log` | Alerts drive warning display |

### 4.2 MVUX Models

Each panel has a corresponding Model class (`partial record`) that injects an `ISmartHomeService`. Public methods become implicit `IAsyncCommand`s that bind to buttons and toggles via `{Binding MethodName}` on `Command` properties.

**ClimateModel example:**

- `IState<int> TargetTemp` exposes a two-way bindable temperature value
- `IFeed<ClimateData> Climate => Feed.Async(service.GetClimateData)` provides the live feed
- Public methods `IncrementTemp()` and `DecrementTemp()` update the `IState`
- `ToggleZone(string zoneName)` dispatches a zone toggle command
- The generated `ClimateViewModel` is set as the page DataContext via dependency injection and Uno Navigation Extensions

For the Shell level, a `ShellModel` exposes `IState<int> ActivePanelIndex` to track which `TabBar` item is selected. This is bound to the TabBar `SelectedIndex`.

### 4.3 Service Layer

Define an `ISmartHomeService` interface with async methods: `GetClimateData()`, `GetSecurityData()`, `GetEnergyData()`, `GetLightingData()`, `GetDiagnosticsData()`, plus command methods `SetTargetTemp(int)`, `ToggleZone(string, bool)`, `ToggleLight(string, bool)`, `SetBrightness(int)`.

The mock implementation returns simulated fluctuating data with random drift on each poll (matching the React prototype behavior). Register via dependency injection in `App.xaml.cs` Host configuration.

### 4.4 FeedView Integration

Use the MVUX `FeedView` control to wrap each panel page's content. FeedView provides built-in templates for Loading, Error, None, and Value states, handling all async data lifecycle automatically without boilerplate.

| Template | Purpose |
|---|---|
| `ValueTemplate` | The normal panel content, rendered when data is available |
| `ProgressTemplate` | A centered ProgressRing styled with PhosphorPrimary color, shown during initial data load |
| `ErrorTemplate` | CRT-styled error message (e.g., `SIGNAL LOST — RETRY`) with a retry Button bound to `FeedView.Refresh` |
| `NoneTemplate` | CRT-styled `NO DATA` message, unlikely but defensive |

---

## 5. Navigation Shell and Flow

*The app has a flat lateral navigation structure: five top-level panels accessed via a custom bottom TabBar. No hierarchical drill-down. This matches the original GCC touch-zone layout.*

### 5.1 Shell Layout (MainPage)

MainPage is the app shell. It contains:

1. The outer bezel `Border`
2. The inner CRT screen container with scan-line and vignette overlays
3. A header bar with the panel title and digital clock
4. A content region where panel pages swap via navigation
5. The bottom `TabBar` for panel switching

The layout structure is a nested Grid. The outermost element is the dark page background. Inside that, a Border with `BezelPrimary` background and rounded corners provides the bezel. Inside the bezel, another Border with `CrtBackground` provides the screen. The screen Grid has three rows: Header (Auto), Content (Star), TabBar (Auto).

### 5.2 TabBar Configuration

Use the Uno Toolkit `TabBar` control with a fully custom style (not `BottomTabBarStyle` or Material styles, since this UI is bespoke). The TabBar sits at the bottom of the CRT screen area, separated from content by a 1px PhosphorDim divider.

Each `TabBarItem` corresponds to one panel: CLIMATE, SECURITY, ENERGY, LIGHTING, DIAGNOSTICS. Items use text-only content (no icons) styled with Orbitron 11px, letter-spacing 0.15em. Use `Region.Attached` navigation so that selecting a `TabBarItem` triggers navigation to the corresponding page within the content Grid region.

| Property | Specification |
|---|---|
| **Style** | Custom `CrtTabBarStyle` and `CrtTabBarItemStyle` defined in `Themes/TabBar.xaml` |
| **Selection Indicator** | A 2px-height Border at the bottom of the active item, colored PhosphorPrimary, with a PhosphorGlow shadow. Use `SelectionIndicatorContent` with `SelectionIndicatorTransitionMode=Slide`. |
| **Active Item** | Background: PhosphorSubtle. Border: 1.5px PhosphorPrimary. Text: PhosphorBright. |
| **Inactive Item** | Background: Transparent. Border: 1.5px PhosphorDim. Text: PhosphorDim. |
| **Pressed Item** | Background: PhosphorSubtle at 16% opacity. Provides tactile feedback. |
| **Focus Item** | A 2px focus rectangle in PhosphorBright around the item (keyboard navigation). |

### 5.3 Navigation Configuration

Use Uno Navigation Extensions with XAML-based region navigation. Attach `Region.Attached=True` on the content Grid. Each `TabBarItem` sets `uen:Navigation.Request` to the route name (e.g., `Climate`, `Security`). Register route-to-view and route-to-model mappings in the Navigation configuration during host setup. Each panel page is a separate Page (`ClimatePage`, `SecurityPage`, `EnergyPage`, `LightingPage`, `DiagnosticsPage`).

Navigation transitions: use a fade-in Opacity animation (0→1 over 150ms) for panel content when switching tabs. This is more appropriate than slide transitions for a CRT aesthetic (CRTs don't slide, they refresh).

---

## 6. Custom Controls Library

*Seven reusable custom controls form the component library. Each is detailed below with its properties, visual states, rendering approach, and accessibility requirements.*

### 6.1 GaugeArc (SkiaSharp Custom Control)

A radial arc gauge that displays a value within a range. Used for humidity, outside temperature, solar output, CPU temperature, and memory usage. Renders via SkiaSharp for pixel-perfect arc drawing.

**Implementation:** Inherit from `SKXamlCanvas`. In the `PaintSurface` handler, draw two arcs: a full background arc (PhosphorDim, 30% opacity, 3px stroke) and a foreground value arc (PhosphorPrimary, 3px stroke) with a drop-shadow glow filter. Center text (the numeric value) is drawn using `SKPaint` with the Orbitron font. A small unit label sits below the value. Below the canvas, a TextBlock shows the gauge label.

**Dependency Properties:**

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | Current value to display |
| `Maximum` | `double` | `100` | Maximum value for the gauge range |
| `Minimum` | `double` | `0` | Minimum value (arc starts here) |
| `Label` | `string` | `""` | Text label below the gauge |
| `Unit` | `string` | `""` | Unit string shown below the numeric value (e.g., `% RH`, `kW`) |
| `GaugeSize` | `double` | `100` | Width/height of the gauge canvas in pixels |
| `StartAngle` | `double` | `-210` | Arc start angle in degrees |
| `EndAngle` | `double` | `30` | Arc end angle in degrees |
| `StrokeWidth` | `double` | `3` | Width of the arc stroke |

**Visual States:**

| State | Behavior |
|---|---|
| **Default** | Foreground arc at PhosphorPrimary, background arc at PhosphorDim 30%, value text at PhosphorBright |
| **Loading** | Show a thin pulsing arc animation (indeterminate) while data is pending. Value text shows `--`. |
| **Error** | Arc is not drawn. Value text shows `ERR` in PhosphorDim. Label shows error hint. |
| **Warning** | When value exceeds 80% of maximum, foreground arc pulses gently (opacity 0.8→1.0, 1s cycle). |
| **Disabled** | All elements at 30% opacity. No glow effects. |

**Animations:** On value change, animate the arc sweep from the previous value to the new value over 800ms with an ease-out curve. Use SkiaSharp animation by calling `Invalidate()` on a timer tick and interpolating the drawn angle. The numeric value should count up/down smoothly in sync.

**Accessibility:** Set `AutomationProperties.Name` to `{Label}: {Value} {Unit}` (e.g., `HUMIDITY: 45 % RH`). Set `AutomationProperties.LiveSetting=Polite` so screen readers announce value changes. The control itself is not focusable (display-only). Ensure the SkiaSharp canvas has a fallback TextBlock for high-contrast mode.

### 6.2 HorizontalBar

A percentage-based horizontal progress bar with a label and value readout. Used for memory usage, thermal load, solar capacity, and grid load indicators.

**Implementation:** A XAML-only custom control (UserControl or Templated Control). Structure: an `AutoLayout` (Vertical, Spacing=4) containing a top row (label left-aligned, value right-aligned in a Grid), and a bar row (a Border with 6px height, PhosphorSubtle background and PhosphorDim 1px border, containing an inner Border whose Width is bound to a percentage of the parent width).

**Dependency Properties:**

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | Current value |
| `Maximum` | `double` | `100` | Maximum value (used for percentage calculation) |
| `Label` | `string` | `""` | Left-aligned label text |
| `ShowValueText` | `bool` | `true` | Whether to display the numeric value/percentage |

**Visual States:**

| State | Behavior |
|---|---|
| **Default** | Bar fill with gradient from PhosphorDim to PhosphorPrimary. Glow shadow on fill edge. |
| **Loading** | Bar fill at 0%. Label shows text; value shows `--`. |
| **Error** | Bar fill at 0%, border turns PhosphorDim. |
| **Warning** | When fill exceeds 80%, the bar fill color intensifies to PhosphorBright. |
| **Disabled** | All elements at 30% opacity. |

**Animations:** Animate the bar fill Width on value change using a `DoubleAnimation` over 800ms with ease-out easing. Use a Storyboard attached to the control or implicit animation via Composition APIs.

**Accessibility:** Set `AutomationProperties.Name` to `{Label}: {computed percentage}%`. Use `ProgressBar` as the underlying control type if possible to inherit screen reader semantics, then re-template it completely with the CRT styling.

### 6.3 PhosphorToggle (Zone/Light Toggle)

A custom toggle control used for zone heating controls and room lighting switches. Visually styled as a rectangular CRT button with an indicator square.

**Implementation:** A Templated Control inheriting from `ToggleButton`. The `ControlTemplate` contains a Border (the button frame) with an inner Grid: a 10×10 indicator square (left) and a TextBlock label (right). The indicator square is filled when toggled on.

**Dependency Properties:**

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | `""` | Display text for the toggle |
| *(inherits `IsChecked`)* | `bool?` | `false` | On/off state from ToggleButton base |

**Visual States (CommonStates group):**

| State | Behavior |
|---|---|
| **Normal (Off)** | Border: 1px BorderSubtle. Background: Transparent. Indicator: 10×10 Border, 1.5px PhosphorDim stroke, no fill. Label: PhosphorDim. |
| **Checked (On)** | Border: 1px PhosphorPrimary. Background: PhosphorSubtle. Indicator: fill PhosphorPrimary with PhosphorGlow box-shadow. Label: PhosphorBright. |
| **PointerOver (Off)** | Border: 1px PhosphorDim at 60% opacity. Background: PhosphorSubtle at 4% opacity. Cursor: Hand. |
| **PointerOver (Checked)** | Border: 1px PhosphorBright. Background: PhosphorSubtle at 12% opacity. |
| **Pressed (Off)** | Border: 1px PhosphorPrimary. Background: PhosphorSubtle at 8%. Brief flash. |
| **Pressed (Checked)** | Border: 1px PhosphorBright. Background: PhosphorSubtle at 16%. Brief flash. |
| **Focused** | 2px focus rectangle in PhosphorBright around entire control. Does not alter internal styling. |
| **Disabled** | All elements at 30% opacity. No pointer cursor. No glow. |

**Animations:** On toggle, the indicator square fill animates in via an Opacity `DoubleAnimation` from 0→1 over 150ms. The glow shadow fades in over 200ms. No bouncy or spring physics — sharp CRT-appropriate transitions.

**Accessibility:** Inherits `ToggleButton` automation (CheckBox pattern). Set `AutomationProperties.Name` to Label value. Screen readers will announce `{Label}, toggle button, checked/unchecked`. Minimum touch target: 44×44px (`MinHeight=44`, `MinWidth` on the outer Border).

### 6.4 TouchZoneButton (Bottom Navigation Item)

The custom `TabBarItem` used in the bottom navigation. This is the most direct homage to the original GCC touch zones: rectangular, blocky, text-only, with a subtle active indicator.

**Implementation:** Custom Style for `TabBarItem` (`CrtTabBarItemStyle`). The template is a Border containing a centered TextBlock. An active-state indicator bar (2px height, PhosphorPrimary) sits at the absolute bottom, visible only when selected.

**Visual States:**

| State | Behavior |
|---|---|
| **Normal (Unselected)** | Border: 1.5px PhosphorDim, BorderRadius=2. Background: Transparent. Text: PhosphorDim, Orbitron 11px, letter-spacing 0.15em. No indicator bar. |
| **Selected** | Border: 1.5px PhosphorPrimary. Background: PhosphorSubtle (12% opacity). Text: PhosphorBright. Indicator bar visible with glow. |
| **PointerOver (Unselected)** | Border: 1.5px PhosphorDim at 80%. Background: PhosphorSubtle at 4%. |
| **PointerOver (Selected)** | No change from Selected (already highlighted). |
| **Pressed** | Background: PhosphorSubtle at 16%. Immediate visual feedback, no delay. |
| **Focused** | 2px PhosphorBright focus ring outside the border. For keyboard navigation. |
| **Disabled** | 30% opacity on all elements. |

### 6.5 DigitalClock

A real-time HH:MM:SS clock display in the header bar. The colon blinks every second.

**Implementation:** A UserControl containing a horizontal `AutoLayout` with TextBlocks for hours, colon, minutes, and seconds. The colon TextBlock has an Opacity Storyboard (`RepeatBehavior=Forever`) that toggles between PhosphorBright and PhosphorDim each second. Time updates via a `DispatcherTimer` ticking every 1000ms.

**Visual States:**

| State | Behavior |
|---|---|
| **Default** | Hours and minutes in Orbitron 28px PhosphorBright. Seconds in Share Tech Mono 16px PhosphorDim. Colon blinks. |
| **Disabled/Error** | All text PhosphorDim, no blink. Shows `--:--`. |

**Accessibility:** Set `AutomationProperties.Name` on the container to a formatted time string (e.g., `2:45 PM`). Set `AutomationProperties.LiveSetting=Off` (clock changes are too frequent for narration).

### 6.6 ScanLineOverlay

A decorative overlay that simulates CRT horizontal scan lines across the entire screen area.

**Implementation:** A UserControl wrapping an `SKXamlCanvas`. In `PaintSurface`, draw horizontal lines every 4 pixels using a semi-transparent black `SKPaint` (alpha=38, about 15%). The canvas fills the entire CRT screen region. Set `IsHitTestVisible=False`. Set `Canvas.ZIndex=100`.

**Performance:** Since the pattern is static, draw it once to an `SKBitmap` and cache it. Only re-render on `SizeChanged`. This keeps GPU load negligible.

**Accessibility:** Purely decorative. Set `AutomationProperties.AccessibilityView=Raw` to hide from the accessibility tree entirely. Provide a user setting to disable it (reduces visual noise for users with visual sensitivities).

### 6.7 CrtVignetteOverlay

A decorative overlay simulating the curved-glass edge darkening of a CRT monitor.

**Implementation:** An `SKXamlCanvas` that draws a radial gradient: fully transparent center, fading to 60% black at edges. `CornerRadius=18` to match the CRT border. `IsHitTestVisible=False`, `Canvas.ZIndex=101` (above scan lines). Cache the bitmap and re-draw only on `SizeChanged`.

**Accessibility:** Decorative only. `AutomationProperties.AccessibilityView=Raw`. Provide a setting to disable for low-vision users, as it reduces edge-area contrast.

---

## 7. Panel Pages: Detailed Breakdown

*Each of the five panels is a separate Page with its own MVUX Model. Below is an exhaustive breakdown of every element, layout, binding, state, and interaction.*

### 7.1 ClimatePage

The primary home monitoring panel. Displays interior temperature, target adjustment controls, humidity and outside temperature gauges, and zone heating toggles.

**Layout Structure:** A two-column Grid (`1* | 1*`, with 20px ColumnSpacing). Left column: centered vertical stack for temperature display and controls. Right column: top row for gauge arcs, bottom section for zone toggles separated by a 1px PhosphorDim divider.

#### Left Column — Temperature Display

Vertically centered `AutoLayout` (Spacing=8). Elements from top to bottom:

1. **Label** `INTERIOR TEMPERATURE` in `CrtLabel` style
2. **Large temperature readout** — horizontal layout with the temperature value in Orbitron 72px PhosphorBright and the unit `°F` in Orbitron 20px PhosphorDim offset 12px from top (superscript alignment)
3. **Target Temperature Control panel** — a Border with 1px PhosphorDim border containing a horizontal layout with a down-arrow Button, a centered `TARGET` label + value display (Orbitron 24px PhosphorBright), and an up-arrow Button

#### Target Temperature Buttons

| Property | Specification |
|---|---|
| **Control Type** | Standard `Button` with custom CRT style |
| **Size** | 32×32px, BorderRadius=2, 1px PhosphorDim border |
| **Content** | FontIcon with down-chevron (▼) or up-chevron (▲) in PhosphorPrimary |
| **Command** | Bound to `DecrementTemp` / `IncrementTemp` methods on `ClimateModel` via `{Binding DecrementTemp}` on `Command` property |
| **Normal** | Background: Transparent, Border: PhosphorDim |
| **PointerOver** | Background: PhosphorSubtle, Border: PhosphorPrimary |
| **Pressed** | Background: PhosphorSubtle 16%, Border: PhosphorBright |
| **Disabled** | 30% opacity. Disabled when temp at min (60) or max (85) |
| **Focus** | 2px PhosphorBright focus ring |

#### Right Column — Gauges

Two `GaugeArc` controls side by side in a horizontal `AutoLayout` (Spacing=8, HorizontalAlignment=Center):

- **Humidity:** `Value={Binding Climate.Data.Humidity}`, Maximum=100, Label=`HUMIDITY`, Unit=`% RH`, GaugeSize=105
- **Outside:** `Value={Binding Climate.Data.OutsideTemp}`, Maximum=120, Label=`OUTSIDE`, Unit=`°F`, GaugeSize=105

#### Right Column — Zone Control

Below the gauges, separated by a 1px Border divider (PhosphorDim, 15% opacity), Padding-top=12. A `ZONE CONTROL` label in `CrtLabel` style. Below it, a 2-column Grid (equal columns, Spacing=6) with four `PhosphorToggle` controls: LIVING, BEDROOM, KITCHEN, GARAGE. Each toggle `IsChecked` is two-way bound to the corresponding zone boolean in `ClimateData`. Toggle command triggers `ToggleZone(zoneName)`.

### 7.2 SecurityPage

Displays door lock status, camera feed indicators, motion event count, and an event log.

**Layout:** Two-column Grid (`1* | 1*`, 20px gap).

#### Left Column

1. **`DOOR STATUS`** `CrtLabel`
2. An `ItemsRepeater` bound to the Doors dictionary, with each item rendered as a horizontal Border (1px border) containing the door name (left, CrtBody) and status text (right). Locked doors show `■ LOCKED` in PhosphorBright with a subtle border (15% opacity). Unlocked doors show `□ UNLOCKED` in PhosphorDim with a full PhosphorPrimary border and PhosphorSubtle 6% background to draw attention.
3. **`CAMERAS ACTIVE`** `CrtLabel`
4. Three camera cards in a horizontal `AutoLayout` (Spacing=8, equal flex). Each card is a Border (1px, centered text) showing camera name in `CrtLabel`, and a status line: `● REC` (PhosphorBright, pulsing dot) when active, `○ OFF` (PhosphorDim) when inactive.

#### Right Column

1. **`MOTION EVENTS TODAY`** `CrtLabel`
2. The count in Orbitron 48px PhosphorBright
3. Divider
4. **`EVENT LOG`** `CrtLabel`
5. An `ItemsRepeater` bound to the Log list. Each entry is a TextBlock in Share Tech Mono 10px. First two entries at PhosphorPrimary, remaining at PhosphorDim (use an index-based opacity converter or a ranked display model).

#### States

| State | Behavior |
|---|---|
| **Default** | All data displayed as described |
| **Loading** | FeedView `ProgressTemplate` shows CRT-styled loading indicator |
| **Error** | FeedView `ErrorTemplate` shows `SECURITY FEED OFFLINE` with retry |
| **Empty/None** | FeedView `NoneTemplate` shows `NO SECURITY DATA` |
| **Alert: Door Unlocked** | Unlocked doors have a more prominent border and background. Consider a gentle pulse animation on the border. |

#### Camera Pulsing Dot

The `● REC` indicator for active cameras uses an Opacity Storyboard that cycles between 0.4 and 1.0 over 1.5 seconds with `RepeatBehavior=Forever`. This mimics a recording indicator LED.

### 7.3 EnergyPage

Solar generation, battery storage visualization, and power draw metrics.

**Layout:** Two-column Grid (`1* | 1*`, 20px gap).

#### Left Column (centered vertically)

1. **`SOLAR GENERATION`** `CrtLabel`
2. A large `GaugeArc`: Value=`{Binding Energy.Data.SolarKw}`, Maximum=8, Label=`CURRENT OUTPUT`, Unit=`kW`, GaugeSize=150
3. Below the gauge, two stat readouts side by side in a horizontal `AutoLayout` (Spacing=24). Left stat: `GRID DRAW` CrtLabel above value in Orbitron 20px PhosphorBright, then `kW` CrtLabel. Right stat: `TODAY` CrtLabel, value in Orbitron 20px PhosphorPrimary, `kWh` CrtLabel.

#### Right Column (centered vertically)

1. **`BATTERY STORAGE`** `CrtLabel`
2. **Custom Battery Visual:** a 80×140 Border (2px PhosphorPrimary, CornerRadius=4) with overflow hidden. Inside, a filled Border anchored to the bottom whose Height is bound to `BatteryPct%` of 140. Fill uses a `LinearGradientBrush` from PhosphorDim (bottom) to PhosphorPrimary (top), at 50% opacity. Centered over the battery, a TextBlock shows the percentage in Orbitron 22px PhosphorBright. A small terminal nub (30×8 Border, 2px PhosphorPrimary, no bottom border, CornerRadius=4,4,0,0) is positioned above the battery using negative margin or a parent Grid arrangement.
3. Below the battery, two `HorizontalBar` controls: `SOLAR CAPACITY` (value: SolarKw/8 × 100) and `GRID LOAD` (value: GridKw/5 × 100).

#### Battery Fill Animation

Animate the fill Height using a `DoubleAnimation` over 1000ms with ease-out easing on data change. The percentage text should update in sync.

### 7.4 LightingPage

Room-by-room light controls and master brightness settings.

**Layout:** Two-column Grid (`1* | 1*`, 20px gap).

#### Left Column

1. **`ROOM CONTROL`** `CrtLabel`
2. Five `PhosphorToggle` controls in a vertical `AutoLayout` (Spacing=10): LIVING, BEDROOM, KITCHEN, PORCH, GARAGE. Each toggle `IsChecked` is two-way bound to the corresponding light boolean. Command triggers `ToggleLight(roomName)`.

#### Right Column (centered vertically)

1. **`MASTER BRIGHTNESS`** `CrtLabel`
2. The brightness value in Orbitron 56px PhosphorBright, with a `%` suffix in Orbitron 12px PhosphorDim
3. Four preset buttons in a horizontal `AutoLayout` (Spacing=8, centered): **25**, **50**, **75**, **100**. Each is a Button (48×36, CornerRadius=2). The active preset has PhosphorSubtle 12% background and PhosphorPrimary border; others have Transparent background and PhosphorDim border. Text in Share Tech Mono 12px. Command bound to `SetBrightness(value)`.
4. Divider
5. **`ACTIVE LIGHTS`** `CrtLabel`
6. A fraction display: `{count} / {total}` in Orbitron 32px PhosphorBright

#### Brightness Button States

| State | Behavior |
|---|---|
| **Normal (Not Active)** | Background: Transparent. Border: 1px PhosphorDim. Text: PhosphorDim. |
| **Active (Current Value)** | Background: PhosphorSubtle 12%. Border: 1px PhosphorPrimary. Text: PhosphorBright. |
| **PointerOver** | Background: PhosphorSubtle 6%. Border: PhosphorPrimary at 60%. |
| **Pressed** | Background: PhosphorSubtle 16%. Border: PhosphorBright. |
| **Focused** | 2px PhosphorBright focus ring. |
| **Disabled** | 30% opacity (unlikely scenario but defensive). |

### 7.5 DiagnosticsPage

System health: hub CPU temperature, memory, network, uptime, device count, alerts, and system log.

**Layout:** Two-column Grid (`1* | 1*`, 20px gap).

#### Left Column

1. **`SYSTEM STATUS`** `CrtLabel`
2. Two `GaugeArc` controls side by side: HUB TEMP (max=90, unit=`°C`) and MEMORY (max=100, unit=`%`), each GaugeSize=100
3. Two `HorizontalBar` controls: `MEMORY USAGE` and `THERMAL`
4. A network status row: a Border (1px subtle) containing `NETWORK` label (left) and status text `● CONNECTED` in PhosphorBright or `○ OFFLINE` in PhosphorDim (right). Connected state has a green glow on the dot.

#### Right Column

1. Two stat readouts side by side: `UPTIME` with value in Orbitron 24px + `d` suffix, `DEVICES` with count in Orbitron 24px
2. Divider
3. **`⚠ ALERTS`** `CrtLabel`
4. An `ItemsRepeater` bound to the Alerts list. Each alert is a Border (1px PhosphorPrimary, CornerRadius=2, PhosphorSubtle 4% background) containing a TextBlock in Share Tech Mono 10px PhosphorPrimary.
5. Divider
6. **`SYSTEM LOG`** `CrtLabel`
7. A vertical list of log entries in Share Tech Mono 9px PhosphorDim, each prefixed with `> `.

#### Alert States

If no alerts exist, show an `ALL SYSTEMS NOMINAL` message in PhosphorDim. Critical alerts (containing keywords like `CRITICAL` or `FAIL`) should have a PhosphorBright border and a subtle pulse animation.

---

## 8. Responsive and Adaptive Behavior

*The app targets a wide range of screen sizes. Use the Uno Toolkit `Responsive` markup extension for property adjustments across five breakpoints: Narrowest, Narrow, Normal, Wide, Widest.*

### 8.1 Breakpoint Strategy

| Breakpoint | Width Range | Layout | Notes |
|---|---|---|---|
| **Narrowest** | < 480px | Single column, panels stack vertically | TabBar text shrinks or switches to icons. Panel two-column grids collapse to single column. |
| **Narrow** | 480–599px | Single column with some side-by-side | Gauges remain side-by-side but smaller (GaugeSize=80). Toggle grids remain 2-column. |
| **Normal** | 600–904px | Default two-column panel layout | This is the baseline design as shown in the prototype. |
| **Wide** | 905–1280px | Two-column with wider spacing | Gauges can increase to GaugeSize=120. Bezel padding increases. |
| **Widest** | > 1280px | Two-column with maximum comfort | MaxWidth constraint (840px) on the bezel container. Content centered in viewport. |

### 8.2 Responsive Properties

Use the `{utu:Responsive}` markup extension on specific properties that need to adapt:

| Property | Responsive Values |
|---|---|
| **CRT Screen Padding** | `Padding="{utu:Responsive Narrowest=8, Narrow=12, Normal='16,20', Wide='20,24', Widest='24,32'}"` |
| **Panel Column Layout** | On Narrowest/Narrow: switch Grid ColumnDefinitions to a single column (stack vertically). Use `VisualStateManager` with `AdaptiveTrigger` for structural changes. |
| **GaugeArc.GaugeSize** | `GaugeSize="{utu:Responsive Narrowest=70, Narrow=80, Normal=100, Wide=120, Widest=130}"` |
| **Temperature Font Size** | The 72px temperature display should scale: Narrowest=36, Narrow=48, Normal=72 |
| **TabBar Orientation** | On Wide/Widest: consider switching to a `VerticalTabBarStyle` on the left side (sidebar navigation). Use `Responsive` Visibility to swap between two `TabBar` instances. |
| **Bezel MaxWidth** | 840px on Normal+, full-width on Narrowest/Narrow |

### 8.3 Adaptive TabBar

On Narrowest/Narrow breakpoints, the bottom TabBar labels may be too long. Strategy:

1. **Shorten labels** (e.g., `CLIM`, `SEC`, `NRG`, `LIT`, `DIAG`) using a Responsive value on `TabBarItem.Content`, or
2. **Switch to icon-only** TabBarItems using FontIcons representing each category

On Wide/Widest, consider a vertical sidebar TabBar (`VerticalTabBarStyle`) positioned to the left of the CRT screen, keeping the bottom area clean.

---

## 9. Animations and Transitions

*Animations should feel immediate and technical, matching the CRT aesthetic. Avoid bouncy, elastic, or playful easing. Prefer linear or ease-out curves with short durations.*

| Animation | Duration | Easing | Implementation |
|---|---|---|---|
| Panel tab switch | 150ms | Ease-out | Opacity fade 0→1 on incoming panel content. Use a `ContentTransition` or manual Storyboard. |
| Gauge arc sweep | 800ms | Ease-out | SkiaSharp timer-driven interpolation. `Invalidate()` every 16ms (60fps) until target angle reached. |
| HorizontalBar fill | 800ms | Ease-out | `DoubleAnimation` on fill element Width or `ScaleTransform.ScaleX`. |
| Toggle state change | 150ms | Linear | Opacity animation on indicator fill (0→1 or 1→0). Glow shadow fade 200ms. |
| Colon blink | 1000ms cycle | Discrete | Opacity toggles between 1.0 and 0.3 every second. `RepeatBehavior=Forever`. |
| CRT flicker | 100ms | Linear | Opacity oscillates 0.97→1.0. `RepeatBehavior=Forever`. `AutoReverse=True`. |
| Camera REC pulse | 1500ms | Sine-like | Opacity cycles 0.4→1.0. `RepeatBehavior=Forever`. `AutoReverse=True`. |
| Battery fill height | 1000ms | Ease-out | `DoubleAnimation` on fill Border Height. |
| Value count-up | 800ms | Ease-out | Timer-driven text update synced with gauge/bar animation. |
| Alert pulse | 2000ms | Sine | Border opacity cycles 0.6→1.0 for critical alerts. `RepeatBehavior=Forever`. |

**Accessibility note:** All decorative animations (flicker, scan lines, pulsing dots) must be disableable via a Settings toggle. Check `SystemInformation` or a user preference for reduced-motion. When reduced motion is enabled, all decorative Storyboards should be stopped and all transitions should be instant (0ms duration).

---

## 10. Accessibility Requirements

*The CRT aesthetic creates inherent accessibility challenges (low contrast green-on-black, decorative overlays). The implementation must address these proactively.*

### 10.1 Contrast and High Contrast Mode

The default CRT theme meets WCAG AA contrast for PhosphorPrimary (`#33FF66`) on CrtBackground (`#050A06`) at approximately 12:1 ratio for large text. However, PhosphorDim (`#1A9940`) on CrtBackground is approximately 4.8:1, which barely passes AA for large text. For small text labels (below 18px), increase PhosphorDim to `#22BB55` (approximately 6.5:1) or ensure all dim text is 14px+ bold.

When Windows High Contrast mode is active, detect via `AccessibilitySettings.HighContrast` and switch the entire CRT theme to system high-contrast colors: `SystemColorWindowTextBrush` for foreground, `SystemColorWindowBrush` for background. Disable all glow, scan-line, and vignette overlays in high contrast.

### 10.2 Keyboard Navigation

All interactive controls must be keyboard-accessible. Tab order flows:

1. TabBar items left to right
2. Panel content in reading order (left column top to bottom, then right column top to bottom)

Focus indicators: a 2px solid PhosphorBright border around the focused element, clearly visible against the dark background. Use `TabIndex` where default order is ambiguous. All buttons, toggles, and preset selectors must respond to Enter/Space for activation.

### 10.3 Screen Reader Support

Set `AutomationProperties.Name` on every interactive and informational element. Use `AutomationProperties.LabeledBy` to associate labels with their values where they are separate elements. Set `AutomationProperties.LiveSetting=Polite` on value readouts that update periodically (temperature, gauges, battery) so screen readers announce changes without interrupting. Set `LiveSetting=Assertive` on critical alerts. All decorative elements (scan lines, vignette, bezel labels) should have `AutomationProperties.AccessibilityView=Raw`.

### 10.4 x:Uid Localization

Assign `x:Uid` to all visible text elements for localization. Pattern: `{PageName}.{Section}.{Element}`. Examples:

- `ClimatePage.Label.InteriorTemp`
- `ClimatePage.Button.IncrementTemp`
- `SecurityPage.Label.DoorStatus`
- `DiagnosticsPage.Alert.HvacFilter`

### 10.5 Minimum Touch Targets

All interactive elements must have a minimum 44×44px touch target. The TabBar items, toggle controls, temperature adjustment buttons, and brightness preset buttons must all meet this requirement. Use `MinHeight` and `MinWidth` properties, and add transparent padding if the visual element is smaller than 44px.

---

## 11. Implementation Notes and Edge Cases

### 11.1 Data Refresh Strategy

The `ISmartHomeService` should be polled every 3 seconds (matching the React prototype interval). Use `IFeed` with `Feed.Async` and a `CancellationToken`-aware service that refreshes on a timer. MVUX handles feed refresh lifecycle automatically. If the service throws, the `FeedView` transitions to `ErrorTemplate`. A manual refresh button is not needed per-panel (the `FeedView.Refresh` command is available if required).

### 11.2 Performance Considerations

- **SkiaSharp gauges:** Invalidate only on data change, not on a continuous render loop. Cache the background arc bitmap and only redraw the foreground arc and text.
- **Scan-line and vignette overlays:** Draw once to a cached bitmap, re-render only on `SizeChanged`.
- **Limit DispatcherTimer usage:** One timer for the clock, one for data polling (shared). Avoid per-gauge timers.
- **Flicker animation:** Use a single Storyboard on the CRT container, not per-element animations.

### 11.3 Font Loading

Share Tech Mono and Orbitron must be embedded as `.ttf` files in the `Assets/Fonts` folder. Register them in `App.xaml` as custom font resources. On WebAssembly, font loading is async; use a FeedView-like loading state on the shell until fonts are ready, or accept a brief FOUT (flash of unstyled text) which is acceptable for a CRT aesthetic (it looks like a boot sequence).

### 11.4 WebAssembly Specific

SkiaSharp rendering on WASM may have slight performance differences. Profile the gauge arc animations on WASM and reduce to 30fps if needed (`Invalidate()` every 33ms instead of 16ms). The scan-line overlay bitmap should be generated at the actual device pixel ratio to avoid blurriness on high-DPI screens.

### 11.5 Platform-Specific Styling

- **iOS/Android:** The outer bezel should extend to a SafeArea, using the Uno Toolkit `SafeArea` control to avoid notches and system bars. The dark background color should extend behind system bars (set StatusBar to dark theme).
- **macOS/Windows desktop:** The bezel border acts as a window-content frame; no additional window chrome considerations unless using custom title bars.

### 11.6 Empty and Error States per Panel

Each panel must handle:

1. **Initial loading** — `ProgressTemplate` in `FeedView`
2. **Service error** — `ErrorTemplate` with CRT-styled `SIGNAL LOST` message and retry button
3. **No data returned** — `NoneTemplate` with `NO DATA` message
4. **Partial data** — e.g., some zones return data but one fails. Show available data with an inline error indicator for the failed zone. The Model should catch per-zone errors and expose a per-zone error state.

### 11.7 State Persistence

User-controlled state (target temperature, brightness level, last active panel) should persist across app sessions. Use Uno Extensions Configuration with `IWritableOptions` to store these values locally. On app launch, restore the last active TabBar selection and user-set values.

---

## 12. Summary: Component Mapping Reference

Quick reference mapping every UI element to its Uno Platform implementation.

| UI Element | Uno Control / Pattern | Custom? | Key Notes |
|---|---|---|---|
| Outer Bezel | `Border` + `ThemeShadow` | Style only | CornerRadius=24, BezelPrimary background, Translation Z=32 |
| CRT Screen | `Border` + `Grid` (3 rows) | Style only | CornerRadius=18, CrtBackground, inset shadow effect |
| Scan Lines | `SKXamlCanvas` (UserControl) | Yes | Cached bitmap, IsHitTestVisible=False, ZIndex=100 |
| Vignette | `SKXamlCanvas` (UserControl) | Yes | Radial gradient, cached, ZIndex=101 |
| Flicker | `Storyboard` (Opacity) | Config | On CRT Border, RepeatBehavior=Forever, 0.97–1.0 |
| Header Bar | `Grid` (2 cols) | Style only | Panel title left, date + clock right, bottom divider |
| Digital Clock | `UserControl` + `DispatcherTimer` | Yes | Orbitron font, blinking colon Storyboard |
| Tab Navigation | `TabBar` (Uno Toolkit) | Restyled | Custom CrtTabBarStyle, region navigation |
| Tab Items | `TabBarItem` | Restyled | Custom CrtTabBarItemStyle, text-only |
| Panel Pages | `Page` + MVUX Model | Yes (5 pages) | Each wrapped in FeedView for async states |
| Gauge Arcs | `SKXamlCanvas` (Custom Control) | Yes | DependencyProperty-driven, animated sweep |
| Horizontal Bars | UserControl or re-templated `ProgressBar` | Yes | Animated fill width, CRT styling |
| Toggle Switches | `TemplatedControl : ToggleButton` | Yes | Indicator square + label, 8 visual states |
| Temp Buttons | `Button` (restyled) | Style only | 32×32, chevron icons, command binding |
| Brightness Presets | `Button` (restyled) | Style only | 48×36, active/inactive state |
| Battery Visual | `Border` + inner fill Border | Yes (UserControl) | Height-animated fill, gradient, terminal nub |
| Door Status Items | `ItemsRepeater` + `DataTemplate` | Template | Locked/unlocked visual differentiation |
| Camera Cards | `ItemsRepeater` + `DataTemplate` | Template | Pulsing REC dot animation |
| Alert Items | `ItemsRepeater` + `DataTemplate` | Template | PhosphorPrimary border, pulse on critical |
| Log Entries | `ItemsRepeater` + `DataTemplate` | Template | Monospace, dimming by recency |
| Network Status | `Border` + conditional TextBlock | Template | Dot indicator, connected/offline states |

---

*End of Implementation Brief*
