# Smart City Platform — Uno Desktop Implementation Brief

> Implementation spec for rebuilding the Predictive AI / Building energy screen as an Uno Platform **desktop** app.
> Source brief: [`nakatomi-platform-ui-breakdown.md`](./nakatomi-platform-ui-breakdown.md) · Reference: `Screenshot 2026-06-03 094001.png` · Date: 2026-06-03

## Scope

Rebuild the single **Predictive AI → Building** screen as a running `net10.0-desktop` app: dark operations dashboard with a live 3D building twin, an actionable AI suggestion card, an energy-consumption bar chart with KPI roll-ups, and a monthly-savings block with a trend line. One screen, real interactions, mock data.

Out of scope for this build: the Overview / Monitoring / Management top-nav destinations, the City/District/Street scale levels (Building only is wired; the others are present but inert), authentication, real telemetry.

## Locked Decisions

| # | Decision | Choice | Reason | Tradeoff |
|---|----------|--------|--------|----------|
| 1 | 3D building twin | **Real 3D via SkiaSharp SKMesh** | Highest fidelity to the glowing digital-twin; SKMesh is the GPU mesh primitive in SkiaSharp v4. | Requires a **preview SkiaSharp pin** + matching Uno.Sdk dev build; projection/lighting/depth-sort are our code, not a 3D engine's. Highest risk item — isolated to its own milestone with a fallback. |
| 2 | Charts | **Custom SkiaSharp drawing** | No new NuGet; full control over the bespoke neon look; reuses the SkiaSharp dependency already pulled for the twin. | We own axis/scale/label math. |
| 3 | Data | **In-memory mock service** behind MVUX feeds | Realistic reactive architecture with zero backend; ideal for a showcase. | Numbers are generated, not live. |
| 4 | Savings chip semantics | **Compute true % reduction** `(current − optimized) / current` | Resolves the doc's flagged ambiguity; chip is always self-consistent. | Values won't match the mock's literal "+25%" (e.g. 332→204 shows ~38%). |

---

## Architecture Brief

### App/module structure
- **Single Project** Uno app, `net10.0-desktop` only (desktop-first; Skia renderer is default since Uno.Sdk 6.0).
- `IHostBuilder` startup (`UseNavigation`, `UseHosting`, DI). One shell, one content page.
- Folders per project convention:
  - `src/Models/` — MVUX `partial record` models + feed/state.
  - `src/Presentation/` — `Shell`, `DashboardPage`, and UserControls (`BuildingTwinView`, `BarChartView`, `TrendLineView`, `AiSuggestionCard`, `KpiCard`).
  - `src/Services/` — `IEnergyService` + `MockEnergyService`, `IBuildingGeometryProvider` + `BoxStackGeometryProvider`.
  - `src/Strings/en/` — all user-facing strings (`Resources.resw`).
  - `src/Styles/` — dark theme overrides, named accent brushes, type-scale.

### Pattern: MVUX (binding)
The screen is **durable async state with reactive data flows** (energy feeds, AI suggestion, savings) — the project default. Confirmed MVUX, not MVVM.

- `EnergyDashboardModel` (`partial record`) is the surface for `DashboardPage`.
- Primitives:
  - `IState<BuildingScale> Scale` — City/District/Street/**Building** segmented selector (two-way).
  - `IListState<FloorReading> Floors` — selection-bindable floor list; drives the ruler + tooltip + which mesh band highlights.
  - `IFeed<HourlyConsumption[]> Hourly` — bar-chart series (current vs after-optimization).
  - `IFeed<ConsumptionRollup> Rollup` — Daily/Weekly/Monthly/Yearly KPI cards.
  - `IFeed<AiSuggestion> Suggestion` — the blue card content.
  - `IFeed<SavingsSummary> Savings` — cost/energy/CO₂ stats + trend points.
- Commands (auto-generated from public methods on the model):
  - `ApplySuggestion()` — commits the optimization; recomputes optimized series/rollup so the bar chart + KPIs animate to their post-apply values.
  - `DismissSuggestion()` — clears the card.
  - `SelectFloor(FloorReading)` — sets selection, drives tooltip + glow.
- **XAML binding: `{Binding}`** for the MVUX data surface (DataContext is the source-generated bindable proxy; `x:Bind` bypasses the feed/state projection and won't see `FeedView.Data`). Two-way to state is `{Binding Scale, Mode=TwoWay}`. This is the one place the repo's "prefer `x:Bind`" convention is overridden — and only for the MVUX-bound page; static control internals still use `x:Bind`.

### Data model (immutable records)
```
record FloorReading(int Floor, string Label, double CurrentKwh, double OptimizedKwh, bool IsFlagged);
record HourlyConsumption(string TimeLabel, double CurrentKwh, double OptimizedKwh);
record ConsumptionRollup(Metric Daily, Metric Weekly, Metric Monthly, Metric Yearly);
record Metric(double Current, double Optimized);   // % reduction computed, not stored
record AiSuggestion(string Title, string Body, double ReductionPct, int TargetFloor);
record SavingsSummary(double CostSaved, double EnergySaved, double Co2Avoided, TrendPoint[] Trend);
record TrendPoint(string DateLabel, double Value);
```
`Metric.ReductionPct => Current <= 0 ? 0 : (Current - Optimized) / Current` — Decision 4, computed everywhere a chip renders.

### Services / dependencies
- `IEnergyService` (DI singleton) → `MockEnergyService`: returns the seeded building profile, hourly series, rollup, suggestion, and savings. Async signatures (`ValueTask<…>`) so feeds are real async even though data is local.
- `IBuildingGeometryProvider` → `BoxStackGeometryProvider`: emits the vertex + index buffers for the tower (N stacked floor slabs as extruded boxes) consumed by the SKMesh control. Keeping geometry behind an interface lets us swap a richer OBJ-loaded mesh later without touching the view.
- Registered via `services.AddSingleton<…>()` in `App.xaml.cs` host builder — no `App.Current` lookups, no new singletons outside DI.

### Navigation
- Region-based (`Uno.Extensions.Navigation`). Shell hosts the top horizontal nav (Overview/Monitoring/**Predictive AI**/Management) as regions; only Predictive AI routes to `DashboardPage`, others to a lightweight "coming soon" stub. Routes registered centrally via `ViewMap`/`RouteMap` in `App.xaml.cs`. No `Frame.Navigate`, no code-behind navigation.

### 3D twin architecture (SKMesh)
- `BuildingTwinView : UserControl` hosts an `SKCanvasElement` (GPU-backed Skia surface for desktop).
- Render pipeline (our code — SKMesh is a primitive, not an engine):
  1. `BoxStackGeometryProvider` builds floor-slab vertices once.
  2. Per frame: apply model→view→projection (orbit camera from pointer drag; default ~30° azimuth matching the mock), CPU depth-sort slabs back-to-front.
  3. Build `SKMesh` with a user SkSL vertex+fragment shader; fragment shader tints each slab by an `isFlagged`/`glowIntensity` attribute — flagged floor (14F) pulses green, the rest read as the blue glass body.
  4. Floor ruler (11F–17F) and tooltip are XAML layered over the canvas, positioned from the projected screen-space of the selected slab.
- Controls overlay (+/−, 2D, layers) are XAML buttons bound to camera commands on the view.
- **Preview SkiaSharp pin (from project rules):** Uno.Sdk dev build matching the SkiaSharp 4.x preview, `net10.0-desktop`, `<UnoDisableLottieSkiaVersionCheck>true</UnoDisableLottieSkiaVersionCheck>`. Known-good combo to start from: `Uno.Sdk 6.6.0-dev.208` + SkiaSharp `4.147.0-preview.2.1` (bump every Skia package together). Verify against the SkiaSharp Uno gallery sample for the matching Uno.Sdk before patching managed code.

### Testing / validation
- `dotnet build` green for `net10.0-desktop`.
- Smoke-run on desktop: twin renders + orbits, Apply animates charts, floor click moves tooltip.
- If a test project is added: unit-test `Metric.ReductionPct`, the mock service seed, and geometry vertex counts. SKMesh rendering validated by screenshot, not unit test.

---

## Design Brief

### Visual direction
Dark, high-contrast operations console. Deep navy-to-charcoal background, a luminous building as the hero, neon-green optimization glow, electric-blue AI card. Calm chrome, bright data. Material Design 3 **dark** as the base; bespoke accents added as named theme resources (never inline hex).

### Layout
Three bands over a full-bleed dark canvas:
- **Top bar** (~56px): brand mark + "Smart City Platform", centered horizontal nav, right cluster (location · weather/time · notifications · avatar).
- **Left content column** (~360px, scrollable): AI Suggestion card → Energy Consumption (bar chart + 2×2 KPI cards) → Monthly Savings (3 stats + trend line).
- **Right viewport** (fills remainder): 3D twin with floor ruler (right edge), floating floor tooltip, scale segmented control (top), and zoom/2D/layers controls (bottom-right).
- **Far-left icon rail** (~48px): vertical app-section icons (decorative/secondary nav).

Layout built with **Tier 1 Uno Toolkit `AutoLayout`** for the stacked columns/cards and **`SafeArea`** for window insets — replaces nested `StackPanel`/`Grid` nesting and keeps spacing consistent. KPI cards use Toolkit **`Card`**. Scale selector uses the Toolkit **segmented control**. (Component-priority tiers stated per control at implementation time.)

### Typography
Material 3 type scale only — no explicit font sizes. KPI hero numbers = `DisplaySmall`/`HeadlineMedium`; units (kWh) = `LabelSmall` muted; section titles ("Energy Consumption", "Monthly Savings") = `TitleMedium`; card body = `BodySmall`.

### Spacing
8px rhythm via AutoLayout spacing tokens; card padding 16px; column gutter 24px. No magic-number margins scattered in XAML.

### Color tokens (named resources in `Styles/`, dark theme)
- `TwinGlowGreenBrush` — flagged-floor optimization glow.
- `TwinBodyBlueBrush` — building glass body.
- `AiCardBlueBrush` / `AiCardBlueGradient` — suggestion card.
- `AccentTealBrush` — bottom hairline / live accents.
- `SurfaceCardBrush`, `SurfaceCanvasBrush` — panel vs background separation.
- Positive-delta text uses a single `SavingsAccentBrush`.
All chart strokes/fills pull from these resources so SkiaSharp drawing stays themeable (resolve brush → `SKColor` at draw time).

### Component hierarchy
`Shell` → `DashboardPage` → `AutoLayout(horizontal)` [ `LeftColumn` (AiSuggestionCard, EnergyPanel{BarChartView, KpiCard×4}, SavingsPanel{StatRow, TrendLineView}) | `TwinViewport` (BuildingTwinView, FloorRuler, FloorTooltip, ScaleSelector, ViewportControls) ].

### Responsive / adaptive
Desktop-first, but use Toolkit `Responsive`/`VisualStateManager` breakpoints so that below ~1100px the left column collapses to an overlay drawer (Toolkit `DrawerControl`) and the twin goes full-bleed. Not a phone layout — graceful narrow-window behavior only.

---

## Interaction Brief

### Core user flow
Pick Building scale → twin shows the tower with floor 14 flagged green → operator reads the AI suggestion → reviews projected bar-chart + KPI savings → hits **Apply** → charts/KPIs animate to optimized values, savings block ticks up.

### Input behavior
- **Scale selector**: Building active; City/District/Street selectable but show an inline "Building-level only in this build" hint.
- **Twin**: pointer-drag orbits, scroll/`+`/`−` zoom, `2D` flattens to top-down, `layers` toggles the floor-glow encoding. Click a slab → selects that floor.
- **Floor ruler**: click a floor label → same as selecting the slab; selected floor highlighted.
- **AI card**: `Apply` (primary) / `Dismiss` (secondary); "Show more" expands the rationale.

### State coverage (every panel)
- **Loading**: `FeedView` shows a skeleton/shimmer for charts + KPIs; twin shows a quiet "initializing twin…" state until geometry + first frame are ready.
- **Empty**: no suggestion → card collapses to a neutral "No optimization suggested" tile; charts with no series show an axis-only empty frame.
- **Error**: service failure → `FeedView` error template with a Retry that re-pulls the feed; twin failure (e.g. SkiaSharp pin mismatch) falls back to the static-render fallback panel (see Unresolved Q's) and logs.
- **Data**: the normal rendered state above.

### Feedback & animation
- **Apply** → bars cross-fade from current to optimized heights (ease-out, ~400ms); KPI numbers count up/down; savings stats increment; flagged floor's green glow intensifies briefly to confirm.
- **Floor select** → tooltip slides in next to the floor; slab glow lifts.
- Use Uno-safe easings only — `BackEase`/`ElasticEase` for overshoot; **do not** author `KeySpline` Y outside [0,1] (Uno rejects WinUI overshoot splines).
- Twin idle: subtle continuous glow pulse on the flagged floor (CPU-driven shader uniform), paused when the window is unfocused.

### Accessibility
- All interactive controls keyboard-reachable; scale selector + Apply/Dismiss have `AutomationProperties.Name`.
- Twin is decorative-but-informative: provide a text-equivalent ("Floor 14 flagged, 332→204 kWh") in the tooltip and an automation name on the viewport.
- Color is never the only signal — flagged floor also labeled in the ruler and tooltip.
- Respect reduced-motion: gate the idle pulse + count-up on a setting.

### Runtime verification steps
1. Launch desktop → twin renders, orbits smoothly, floor 14 glows green.
2. Click floor 13/15 → tooltip repositions, glow moves.
3. Apply → bars + KPIs animate to optimized values; chip shows the computed % reduction.
4. Dismiss → card collapses; charts stay at pre-apply values.
5. Narrow window < 1100px → left column becomes a drawer; twin full-bleed.

---

## Implementation Plan

**M0 — Scaffold & shell**
- `dotnet new unoapp -preset recommended`, trim to `net10.0-desktop`, confirm `dotnet build`.
- Host builder: DI, navigation, Material dark theme. Shell + top nav regions; `DashboardPage` route. Stub the three non-AI nav destinations.
- *Skills to invoke first:* `uno-platform-agent`, `uno-navigation`, `uno-material`, `winui-xaml`.

**M1 — Data + MVUX surface**
- Records, `IEnergyService`/`MockEnergyService` with seeded profile, `EnergyDashboardModel` with feeds/states/commands. No real UI yet — bind a debug dump to confirm feeds resolve.
- *Skills:* `mvux`, `uno-extensions-services`.

**M2 — Left column (charts + cards), no twin**
- `BarChartView` + `TrendLineView` (custom SkiaSharp), `KpiCard`×4, `AiSuggestionCard`, savings stat row. `FeedView` for loading/empty/error. Apply/Dismiss wired with animations. `Metric.ReductionPct` driving every chip.
- *Skills:* `uno-toolkit`, `winui-xaml`, `userinterface-wiki-uno`.

**M3 — 3D building twin (SKMesh)** — isolated, highest risk
- Pin preview SkiaSharp + matching Uno.Sdk dev build; set `UnoDisableLottieSkiaVersionCheck`. Verify a trivial SKMesh draws before building the tower.
- `BoxStackGeometryProvider` → `BuildingTwinView` (orbit/zoom/2D, glow shader, flagged floor). Floor ruler + tooltip overlay + viewport controls bound to the model.
- *Skills:* `winui-xaml`; reference the SkiaSharp Uno gallery sample for the pin.

**M4 — Polish & responsive**
- AutoLayout/SafeArea spacing pass, named accent brushes, reduced-motion gating, narrow-window drawer, accessibility names, idle-pulse focus pausing.
- *Skills:* `userinterface-wiki-uno`, `uno-toolkit`, `uno-material`.

Each milestone ends with `dotnet build` + desktop smoke test + a conventional commit.

## Unresolved Questions

- **SKMesh pin validation**: the `Uno.Sdk 6.6.0-dev.208` + SkiaSharp `4.147.0-preview.2.1` combo is known-good from TypeLab, but must be re-validated against the current SkiaSharp Uno gallery sample before M3 — confirm the dev build still matches the native lib. What's the M3 **fallback** if the pin is unstable: pre-rendered tower image + XAML floor overlays, or a stylized 2.5D Skia tower? (Recommend pre-rendered image fallback — keeps the screen shippable.)
- **Geometry fidelity**: start with a plain stacked-box tower, or model the mock's tapered/terraced silhouette? Box stack for M3, refine later?
- **Mock numbers**: reproduce the exact figures from the screenshot (332 / 1 437 / 10.4k / 154.3k kWh, 4.3k$ / 2.4k kWh / 135 CO₂) as the seed, accepting that the computed chip will read ~38% instead of the mock's 25%?
- **Top-nav stubs**: minimal "coming soon" page, or hide the inactive destinations entirely for this build?
- **Window chrome**: default desktop title bar, or extended/custom title bar to match the borderless look of the reference?
- **Assets**: do we have the brand mark / icon-rail glyphs, or use Material Symbols placeholders?

---

> Next step on approval: run **M0** (scaffold + shell), invoking `uno-platform-agent` / `uno-navigation` / `uno-material` / `winui-xaml` first and citing each pattern, then `dotnet build` before proceeding.
