# Architecture Brief

How the bench is structured as code. Read after [README.md](../README.md), before [INTERACTIONS.md](./INTERACTIONS.md).

---

## Decisions and rationale

### Pattern: MVUX

Each demo is a small visual-state machine — input → feed → output. MVUX's `Feed<T>` maps to this cleanly without `INotifyPropertyChanged` plumbing or RelayCommand boilerplate. The trade-off vs MVVM is more compile-time ceremony for less runtime indirection. Worth it here because the demos benefit from determinism (re-runnable for screenshots, recordings, and tests).

### Rendering: Skia on both targets

Uno 6 uses Skia for both Desktop and WASM. The bench specifies a single set of animations and visual materials with the assumption that they look pixel-identical on both heads. SVG path morphing (used for TabBar icons) maps cleanly to `Path.Data` interpolation in WinUI.

### Animation orchestration: four approaches

| Type | Approach | Used for |
|---|---|---|
| **State-driven** | `VisualStateManager` + `Storyboard` in XAML | TabBar icon morphs, Chip flip, Drawer state pill |
| **Time-driven (timer)** | `DispatcherTimer` driving a feed/property | Split-flap cascade, honest timer |
| **Gesture-driven** | `ManipulationStarted/Delta/Completed` events | Pull-tab drawer drag |
| **Cascade-driven** | Timed `Storyboard.Begin()` calls with per-element delay | Chip group cascade |

No third-party animation library. No `Composition` API direct calls (kept to XAML primitives for portability).

---

## Project layout

```
src/UnoToolkit.Bench/
├── App.xaml                          Merged dictionaries; theme resources
├── App.xaml.cs                       Standard Uno app bootstrap
├── MainPage.xaml                     Masthead + 5-row vertical grid
├── MainPage.xaml.cs                  Code-behind (minimal)
│
├── Demos/                            One UserControl per row
│   ├── NavigationBarDemo.xaml(.cs)
│   ├── TabBarDemo.xaml(.cs)
│   ├── ChipGroupDemo.xaml(.cs)
│   ├── DrawerControlDemo.xaml(.cs)
│   └── LoadingViewDemo.xaml(.cs)
│
├── Models/                           MVUX feeds, one per stateful demo
│   ├── NavStackModel.cs              Page depth + title
│   ├── ChipSelectionModel.cs         Selected ids, cascade trigger
│   ├── DrawerStateModel.cs           Openness 0..1, snap target
│   └── TimerSourceModel.cs           ILoadable for utu:LoadingView
│
├── Controls/                         Custom primitives
│   ├── SplitFlapText.cs              Char-cycling animator
│   ├── MorphIcon.cs                  Path-data interpolating icon control
│   └── PullTabDrawer.cs              Drawer chrome + tab + drag math
│
├── Themes/
│   ├── Tokens.xaml                   Color brushes, gradient endpoints, easings, type styles
│   └── Bench.xaml                    Composite styles (cell, button, pill)
│
└── Assets/
    └── Fonts/                        Fraunces, Instrument Sans, JetBrains Mono
```

---

## Resource dictionary structure

`App.xaml` merges in this exact order:

```xml
<ResourceDictionary.MergedDictionaries>
  <MaterialColors    xmlns="using:Uno.Toolkit.UI.Material" />
  <MaterialFonts     xmlns="using:Uno.Toolkit.UI.Material" />
  <MaterialResources xmlns="using:Uno.Toolkit.UI.Material" />
  <ToolkitResources  xmlns="using:Uno.Toolkit.UI" />
  <ResourceDictionary Source="ms-appx:///Themes/Tokens.xaml" />
  <ResourceDictionary Source="ms-appx:///Themes/Bench.xaml" />
</ResourceDictionary.MergedDictionaries>
```

Order matters. Material defines the theme baseline; Tokens overrides specific brushes (e.g., `MaterialPrimaryBrush` → our `AccentBrush`); Bench defines composite styles that consume tokens.

`Tokens.xaml` exposes:

- Color brushes for surfaces, foregrounds, accent (see DESIGN.md §Palette)
- Gradient endpoint brushes for subtle surface depth
- `KeySpline` resources for each named easing (see INTERACTIONS.md §Easings)
- Type styles tuned for the row layout

`Bench.xaml` exposes:

- `RowStyle`, `SpecPanelStyle`, `StagePanelStyle`
- `MotionTagStyle`, `IdentityTagStyle`
- `MastheadStyle`, `BenchButtonStyle`, `StatePillStyle`

---

## State model per demo

| Demo | State surface | Implementation |
|---|---|---|
| **NavigationBar** | Current page index `0..2` | `NavStackModel.Depth: Feed<int>`; title + body derived |
| **TabBar** | Active tab id `0..3` | Local `Feed<int>` in the demo's bindable; VisualState change drives morph |
| **ChipGroup** | Selected ids set + cascade trigger | `ChipSelectionModel.Selected: Feed<ImmutableHashSet<string>>`; cascade flag drives staggered animation |
| **DrawerControl** | Openness `0..1` (continuous) + snap state | `DrawerStateModel.Openness: Feed<double>`; gesture writes during drag, snap on release |
| **LoadingView** | Idle / Running / Loaded + elapsed | `TimerSourceModel : ILoadable`; exposes `Elapsed: TimeSpan` |

Feeds are surfaced via MVUX bindable proxies. Demo XAML binds against the bindable, not the model directly.

---

## Animation orchestration

### State-driven (XAML-only)

**TabBar** uses `VisualStateManager` per tab item. Each tab has its own `Selected` state with a storyboard that morphs `Path.Data` (or the equivalent `Geometry`), `Width`, `Height`, `X`, `Y`, etc. CSS-style declarative — toggle a state, the storyboard runs.

**ChipGroup**'s base flip is also state-driven. The cascade *adds* per-chip start delays before triggering states; the per-chip flip itself remains a `VisualState`.

### Time-driven (DispatcherTimer)

**SplitFlapText** uses one `DispatcherTimer` per character at 55ms intervals. Per-character start delay = `index × 50ms`. Each character cycles 4–7 random `A–Z` letters, then settles to its target with a final `ScaleY 1 → 0.35 → 1` "tick" storyboard.

**TimerSourceModel** uses a `DispatcherTimer` at 16ms (≈60Hz) to update `Elapsed`. The UI binds to its `TotalSeconds` formatted `F3`.

### Gesture-driven (manipulation events)

**PullTabDrawer** is the most procedural demo. The drawer panel has a `CompositeTransform` whose `TranslateX` is driven directly by pointer events:

1. `PointerPressed` on the tab → capture pointer, store start position + current openness
2. `PointerMoved` → compute new openness from `dx`, apply resistance curve + rubber-band, write to transform
3. `PointerReleased` → snap to nearest end via a custom `Storyboard` that reads the current `TranslateX` and animates to the target with overshoot keyframes

See INTERACTIONS.md for the full math.

### Cascade-driven (timed begins)

**ChipGroup** "All" toggle: when triggered, iterate chips in DOM order, queue their flip-state transitions with `index × 70ms` delay. Each chip's individual flip storyboard runs at full speed; the offset alone produces the wave.

---

## Cross-target performance

Tested behaviors and recommendations:

| Concern | Skia Desktop | WASM | Mitigation |
|---|---|---|---|
| `PlaneProjection` flips | smooth | smooth | none needed |
| `Path.Data` morph (TabBar icons) | smooth | smooth | ensure both states use same command sequence count |
| `DispatcherTimer` at 16ms | smooth | acceptable | bump to 33ms on WASM if profiling shows jank |
| Surface gradients | smooth | smooth | gradients are static, no impact |
| `ImageBrush` paper noise | n/a | n/a | not used (theme is gradient-based, no noise PNG) |
| Pointer manipulation (drawer drag) | smooth | acceptable | 60Hz pointer events; throttle to RAF if needed |
| Custom font loading | instant | ~200–400ms | preload via `embeddedFonts.json` |

---

## Reduced motion

Honor the OS-level "reduce motion" preference:

- Read `UISettings.AnimationsEnabled` on each platform
- Demos check this flag during construction. When `false`:
  - Split-flap → skip cycling, set target text directly
  - TabBar morph → skip path interpolation, jump to active state
  - Chip cascade → skip stagger, all chips flip simultaneously (or simply toggle opacity)
  - Drawer → no animation; `Run task` jumps openness directly to 0 or 1
  - Honest timer → skip ticking and line traverse, jump to loaded state immediately

This is the closest equivalent to the HTML's `@media (prefers-reduced-motion: reduce)`.

---

## Out of scope (architecture)

- Dependency injection beyond MVUX's defaults
- Telemetry / analytics
- Persistence — every demo resets to default state on app launch
- Localization infrastructure
- Hot-reload-specific concerns (works fine with `dotnet watch`, no special handling)
