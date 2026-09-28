# RETICLE LOADER — SPEC

Component B from the **HUD Ring Components** canvas. It is a HUD-style loading ring built as a **keyed retemplate of the stock WinUI `ProgressRing`**, with three states: Scan, Lock and Idle. The deliverable is the control template plus a single-page sample app that demonstrates it.

- **Status:** Built (2026-09-28). The open questions were answered with the defaults listed under Unresolved Questions. See the README for measurements.
- **Date:** 2026-09-28
- **Visual source of truth:** the canvas board "B — Reticle Loader" (HTML prototype). This spec's geometry is generated from the same math (Appendix A).
- **Framework evidence:** `unoplatform/uno` main @ `bd31521` (2026-09-26): `src/Uno.UI/UI/Xaml/Controls/ProgressRing/ProgressRing.{cs,xaml}`, `ProgressRingAutomationPeer.cs`.

## Goal

Show that a stock `ProgressRing` can become a layered, animated HUD instrument with **only a Style and a ControlTemplate**: no subclass, no code-behind in the control, and no Lottie. The piece sits in the same family as the hardware-panel retemplating demo.

## Non-goals

- Determinate progress display, such as a percentage arc. Any determinate state renders as Lock (see Unresolved Questions).
- A light theme. The HUD is dark-only.
- Error/fault visuals inside the control, because `ProgressRing` has no error state.
- A NuGet package or reusable library.

## Pinned versions

| Item | Value | Note |
|---|---|---|
| .NET | `net10.0` | .NET 9 is out of scope |
| Uno.Sdk | **`6.7.30`** (latest stable on NuGet at scaffold time, 2026-09-28) | Known-good reference was `6.6.42` |
| Template | `dotnet new unoapp -preset recommended -presentation mvvm` | Skia renderer everywhere (the default since Uno.Sdk 6.0) |
| Targets | `net10.0-desktop` (primary), `net10.0-browserwasm`, `net10.0-android` | iOS is an open question |
| Lottie package | **Not referenced** | The custom template has no `AnimatedVisualPlayer`. `_player?` is null-safe in Uno's `ProgressRing.cs` |
| Font | IBM Plex Mono, static TTFs (Regular 400, Medium 500) | Variable TTFs render only their default instance on Skia |

---

## Architecture Brief

### App/module structure

```
ReticleLab/
  ReticleLab.sln
  .mcp.json                         ← copied from ~/.claude/templates/uno/.mcp.json
  ReticleLab/
    App.xaml                        ← merges Themes/Hud.xaml, Themes/ReticleRing.xaml
    Themes/
      Hud.xaml                      ← HUD colors, brushes, text styles, HudSegmentStyle (root-level dictionary)
      ReticleRing.xaml              ← ReticleRingStyle, ReticleRingCompactStyle (keyed)
    Presentation/
      MainPage.xaml(.cs)
      MainViewModel.cs
    Assets/Fonts/IBMPlexMono-Regular.ttf, IBMPlexMono-Medium.ttf
  tools/
    reticle-geometry.cs             ← file-based `dotnet run` app; prints Path Data (port of Appendix A math)
```

### Decisions

**D1 — Retemplate `Microsoft.UI.Xaml.Controls.ProgressRing` with a keyed Style**
- **Decision:** the control is `<ProgressRing Style="{StaticResource ReticleRingStyle}"/>`, with no subclass.
- **Reason:** Uno drives exactly three states in `CommonStates`: `Active` (IsActive + IsIndeterminate), `DeterminateActive` (IsActive, not indeterminate) and `Inactive`. These map 1:1 onto Scan, Lock and Idle, and the template parts `LottiePlayer` and `LayoutRoot` are optional (`as` casts, null-safe calls).
- **Tradeoff:**
  - VisualStateManager cannot pause a rotation mid-angle. Lock therefore reads as "align to home" instead of the prototype's freeze.
  - Scan's motion is a `Forever` storyboard, with a measured cost of about ⅓ of a core on Skia desktop while it runs.
  - Reduced motion can't be handled inside the template.
- **Fallback:** a `ReticleRing : ProgressRing` subclass driven by a DispatcherTimer. This needs a Stop Check.

**D2 — Keyed style, not implicit**
- **Decision:** the style is keyed.
- **Reason:** an implicit style would restyle every `ProgressRing` in the app, including Material and pull-to-refresh.
- **Tradeoff:** every instance must set `Style` explicitly.

**D3 — Precomputed Path Data**
- **Decision:** geometry is precomputed from the prototype's math, pasted into the template, and the generator is kept in `tools/`.
- **Reason:** the geometry is deterministic and all arcs are circular. Uno's `Path.Data` throws on elliptical arcs where rx≠ry.
- **Tradeoff:** changing the geometry means regenerating and pasting again.

**D4 — Two size tiers, each a `Viewbox` over a fixed design canvas**
- **Decision:** a Hero tier (600-unit canvas, 240–480 px) and a Compact tier (40-unit canvas, 24–64 px).
- **Reason:** XAML has no non-scaling stroke, and a single scaled template produces invisible strokes at 24 px.
- **Tradeoff:** stroke weight drifts by about ±50% across each tier's range.

**D5 — MVVM (CommunityToolkit.Mvvm) plus `x:Bind` for the sample page**
- **Decision:** use MVVM, not MVUX.
- **Reason:** by project rule, this is a single-page demo whose state is one imperative toggle. There are no feeds or async data, so MVUX adds nothing here.
- **Tradeoff:** the sample doesn't double as an MVUX example.

**D6 — Colors through the existing brushes: `Foreground` = ink, `Background` = track/dim**
- **Decision:** the template uses `TemplateBinding` on the two brushes `ProgressRing` already has.
- **Reason:** theming works with no custom dependency properties.
- **Tradeoff:** only two colors are themable, so the prototype's two greys (#3A3A3A track, #5E5E5E dots) collapse to `Background`.

### State model

The page owns one enum. The control's state is derived from it:

| `ReticleMode` | `IsActive` | `IsIndeterminate` | `Value` | VSM state | Status text |
|---|---|---|---|---|---|
| Scan | true | true | – | `Active` | "Scanning" |
| Lock | true | false | 100 | `DeterminateActive` | "Target locked" |
| Idle | false | (unchanged) | – | `Inactive` | "Standby" |

`MainViewModel` (ObservableObject):
- `[ObservableProperty] ReticleMode Mode` (default Scan)
- The computed values `IsActive`, `IsIndeterminate`, `IsScan`, `IsLock`, `IsIdle`, `StatusText` and `MotionEnabled`, all notified via `[NotifyPropertyChangedFor]`
- `[RelayCommand] SetMode(ReticleMode)`

### Navigation model

A single page. The recommended preset's Shell → MainPage route stays as generated, and no routes are added.

### Services/dependencies

- `CommunityToolkit.Mvvm` (from the preset) is the only package.
- No new services.
- `Windows.UI.ViewManagement.UISettings` supplies `AnimationsEnabled`, which Uno implements **on Android only**. Keep a reference to the instance.

### Data flow

1. RadioButton → `SetModeCommand(mode)`.
2. `Mode` changes, which notifies the derived properties.
3. `x:Bind` OneWay pushes `IsActive`/`IsIndeterminate` into `ProgressRing`, and Uno's `ChangeVisualState` calls `GoToState`.
4. The template's VisualTransitions and storyboards run.
5. In parallel, the page-level `VisualStateManager` (StateTriggers bound to `IsScan`/`IsLock`/`IsIdle`) updates the status dots, and a `TextBlock` shows `StatusText`.

### Platform constraints (from validated runtime notes)

- **Forever storyboards pump frames at display refresh:** measured at about 29% of one core, and the pump continues while the target is collapsed. `Stop()` reclaims it. The VSM leaving `Active` must stop the storyboard; this is verified in R3.
- **`Path.Data` arcs must be circular.** Appendix A complies.
- **`CornerRadius` larger than half the height renders as an oval** on Skia. The pill uses `CornerRadius=22` for a 44 px height.
- **`CharacterSpacing` does nothing on the Skia text stack.** Labels are untracked uppercase, with no per-character workaround.
- **App-specific brushes go in a root-level dictionary** (`Hud.xaml`), not in theme dictionaries.
- **`Opacity` on large containers** re-composites every frame on Android. Idle opacity applies to `LayoutRoot` inside the control only, which is small.

### Testing/validation approach

- No unit-test project. The ViewModel mapping is a six-line switch, and runtime verification covers it.
- Validation runs through the App MCP runtime checks listed in the Interaction Brief, plus a Release-build CPU table.

---

## Design Brief

### Visual direction

The look is a monochrome technical instrument: hairline white strokes on pure black, with no fills except status dots and no glow. Motion comes from layers counter-rotating at unrelated speeds, which creates the "machine at work" read. Lock is the only emphatic moment: everything aligns, the reticle closes, and the ring pulses once.

### Tokens (`Themes/Hud.xaml`)

This file is the only place hex values appear.

| Key | Value | Use |
|---|---|---|
| `HudInkColor` / `HudInkBrush` | #EDEDED | Strokes and primary text (`ProgressRing.Foreground`) |
| `HudDimColor` / `HudDimBrush` | #5E5E5E | Track, dim dots (`ProgressRing.Background`) |
| `HudTrackColor` / `HudTrackBrush` | #3A3A3A | Unchecked segment border |
| `HudGroundColor` / `HudGroundBrush` | #000000 | Page background, text on a checked segment |
| `HudMutedColor` / `HudMutedBrush` | #8F8F8F | Secondary labels (6.5:1 on black) |

### Layout structure (MainPage)

```
Grid (HudGroundBrush, RequestedTheme=Dark)
└─ StackPanel  MaxWidth=720, centered, Padding 48 (≥600px) / 16 (<600px), Spacing 24
   ├─ Header row: "B — RETICLE LOADER" (ink) · "PROGRESSRING TEMPLATE" (muted), space-between
   ├─ Segmented row: 3 RadioButtons (Scan / Lock / Idle), Spacing 8, GroupName="Mode"
   ├─ Hero: ProgressRing Style=ReticleRingStyle, 320×320 (240×240 below 600px)
   ├─ Status row: 3 Ellipses 8×8, Spacing 5 + StatusText, centered, Spacing 14
   └─ Compact row: 64 / 40 / 24 px ProgressRings (ReticleRingCompactStyle), each over a muted size label, Spacing 56, centered
```

### Control template: Hero (`ReticleRingStyle`)

- **Setters:** `Foreground`=HudInkBrush, `Background`=HudDimBrush, `Width`/`Height`=320, `IsHitTestVisible`=False, `IsTabStop`=False.
- **Template:** `Grid x:Name="LayoutRoot"` > `Viewbox` > `Grid Width=600 Height=600`. Every `Path` has `Width=600 Height=600`, no stretch, and absolute coordinates from Appendix A. Rotating paths use `RotateTransform CenterX=300 CenterY=300`.

Stroke units assume a 320 px nominal size (prototype stroke × 1.75): hairline **1.3**, standard **1.75**, heavy **2.6**, lock **4.4**.

| # | Part name | Geometry (Appendix A) | Stroke | Brush | Scan | Lock |
|---|---|---|---|---|---|---|
| L1 | `OuterArcs` | Hero.OuterArcs | hairline | Fg | 360° cw / 28 s | → 0° |
| L2 | `TickScale` | Hero.TickScale | hairline | Fg | 360° ccw / 16 s | → 0° |
| L3 | `MainHalf` + `Knob` | Hero.MainHalf + Ellipse r3.5 at (300,85), Fill=Ground | heavy | Fg | static | static |
| L4 | `InnerRing` | Ellipse r165 at center | standard | Fg | static | thickness → 4.4 |
| L5 | `Pulse` | Ellipse r165, Opacity 0 | standard | Fg | – | one-shot pulse |
| L6 | `GapArc` | Hero.GapArc | standard | Fg | 360° cw / 2.8 s | → 0° |
| L7 | `Band` | Hero.Band | hairline | Fg | 360° cw / 7 s | → 0° |
| L8 | `Radials` | Hero.Radials | hairline | Fg | static | static |
| L9 | `Reticle` | Hero.Reticle, CompositeTransform center 300,300 | standard | Fg | static | rotate 45°, scale 0.5 |
| L10 | `LockDot` | Ellipse r3, Fill=Fg, Opacity 0 | – | Fg | – | fade in |
| L11 | `Callouts`, `Marker` | Hero.Callouts, Hero.Marker | hairline / standard | Fg | static | static |
| L12 | `Dots` | 13 Ellipses (coordinates in Appendix A) | – | Fg / Bg | static | static |

### Control template: Compact (`ReticleRingCompactStyle`)

- **Setters:** `Width`/`Height`=40.
- **Template:** `LayoutRoot` > `Viewbox` > `Grid 40×40`.

| Part | Geometry | Stroke | Brush | Scan | Lock |
|---|---|---|---|---|---|
| `Track` | Ellipse r18 | 1 | Bg | static | static |
| `SweepArc` | CompactArc | 1.25 | Fg | 360° ccw / 7 s | → 0° |
| `GapArc` | CompactGap | 1.25 | Fg | 360° cw / 2.8 s | → 0° |
| `LockDot` | Ellipse r2.5 | – | Fg | – | fade in |

### Typography

- **Family:** IBM Plex Mono, from static TTFs.
  - Fetch through Google Fonts css2 using the Android 2.2 User-Agent, and check the TTF magic bytes `00 01 00 00`.
  - The `#fragment` must match the internal family name exactly. Verify what "IBM Plex Mono" vs "IBM Plex Mono Medium" report.
- **Styles** (BasedOn Material type scale, overriding `FontFamily`/`Foreground` only, with no explicit sizes):
  - `HudLabelStyle`: LabelSmall, uppercase, muted
  - `HudTitleStyle`: LabelMedium, ink
  - `HudStatusStyle`: LabelMedium, ink

### Spacing

The spacing uses an 8 px grid: page padding 48/16, section spacing 24, segment spacing 8, status gap 14, dot gap 5, compact gap 56.

### Component hierarchy

`MainPage` → `HudSegmentStyle` RadioButtons ×3 → `ProgressRing[ReticleRingStyle]` → status row → `ProgressRing[ReticleRingCompactStyle]` ×3.

`HudSegmentStyle` (RadioButton retemplate) is a 44 px pill with `CornerRadius=22` and horizontal padding 20:
- **Unchecked:** transparent fill, `HudTrackBrush` border, muted text.
- **PointerOver:** `HudMutedBrush` border, ink text.
- **Checked:** `HudInkBrush` fill, `HudGroundBrush` text.
- **Focus:** system focus visuals, with `FocusVisualPrimaryBrush`=Ink.

### Theme usage

- Material stays the app theme, and the page forces `RequestedTheme="Dark"`.
- The HUD brushes override locally.
- The ring's colors are themable per instance through `Foreground`/`Background`. The canvas's accents (cyan #7FD4FF, amber #FFB547, violet #B9A7FF) are available as optional extra brushes.

### Responsive/adaptive behavior

- A single `AdaptiveTrigger` at `MinWindowWidth=600`.
  - **Below 600:** padding 16, Hero 240×240.
  - **600 and above:** padding 48, Hero 320×320.
- The compact row fits at 360 px width: 64+40+24+2×56 = 240.

---

## Interaction Brief

### User flows

1. The app launches in **Scan**: the ring rotates, the status reads "Scanning" and the dots are on/dim/off.
2. The user taps **Lock**: the layers align, the reticle closes to a plus, the ring pulses once, the lock dot appears, the status reads "Target locked" and all dots are on.
3. The user taps **Idle**: the ring dims to 35% and stops, the status reads "Standby" and all dots are hollow.
4. Any mode can be reached from any other mode.

### Input behavior

- The segment control is a RadioButton group (Tab to enter, arrow keys to move, Space to select), because the modes are mutually exclusive.
- The ring itself takes no input (`IsHitTestVisible=False`, `IsTabStop=False`).

### Visual transitions (template `VisualStateGroup.Transitions`)

| From → To | Duration | Easing | Animations |
|---|---|---|---|
| Inactive → Active | 300 ms | Cubic out | `LayoutRoot.Opacity` 0.35→1; Scan storyboard starts at 0° |
| Active → DeterminateActive | 550 ms | `KeySpline 0.2,0.8 0.2,1` | L1/L2/L6/L7 angle → 0 (generated transition); Reticle rotate→45, scale→0.5; InnerRing thickness 1.75→4.4 (300 ms, `EnableDependentAnimation`); Pulse opacity 0.9→0 and scale 1→1.28 over 900 ms, ease-out; LockDot opacity→1 over 300 ms, BeginTime 250 ms |
| DeterminateActive → Active | 550 ms | same spline | Reticle → identity; LockDot → 0; InnerRing → 1.75; Scan storyboard restarts |
| * → Inactive | 500 ms | Cubic out | Hero: `LayoutRoot.Opacity` → 0.35. Compact: → 0 |

### States

- **Empty:** Idle is the resting state. Hero stays visible at 35% (prototype behavior); Compact hides (WinUI convention).
- **Loading:** Scan is the loading state.
- **Error:** none inside the control. A host shows errors in its own UI, and the sample has none.
- **Feedback:** the status text plus the three dots. The dot mapping is Scan = on/dim/off, Lock = on/on/on, Idle = off/off/off, driven by page StateTriggers.

### Accessibility considerations

- **Automation peer:** Uno's peer reports `AutomationControlType.ProgressBar` and prefixes the localized "indeterminate" status to the Name while indeterminate. Set `AutomationProperties.Name="Reticle loader"`.
- **Live region:** the status `TextBlock` uses `AutomationProperties.LiveSetting="Polite"`, so mode changes are announced.
- **Motion is never the only signal:** every state has text and dots.
- **Reduced motion:**
  - When `MotionEnabled` is false (from `UISettings.AnimationsEnabled`, Android only), Scan maps to `IsActive=false`. The ring is still and dim, and "Scanning" plus the dots carry the state.
  - Desktop and WASM default to motion on. See the Unresolved Questions for an in-app toggle.
- **Contrast:** ink on black is about 18:1 and muted on black is 6.5:1. The dim grey (#5E5E5E) is decorative and never used for text.
- **Touch targets:** segments are 44 px tall.

### Runtime verification steps

1. `dotnet build`: no errors, and no `Uno0001` warnings from the templates.
2. `uno_app_start` (desktop), then `uno_app_visualtree_snapshot`: `LayoutRoot` and the named layers are present, with no `AnimatedVisualPlayer`.
3. Drive each mode with `uno_app_element_peer_default_action` on the RadioButtons. If the peer fails, fall back to client-coordinate `uno_app_pointer_click`. Take a screenshot per mode and compare against canvas board B at 320 px.
4. **Mid-flight Lock:** on a freshly restarted process, hot-reload the Lock transition to 60 s, capture, then restore it. Screenshot RPC takes about 2 s, so short animations always capture settled.
5. **CPU (Release, 15 s settle, 3 samples per mode):** record Scan, Lock and Idle. Pass: Lock and Idle each under 1% of a core. Record Scan's cost in the README.
6. **Automation:** check the ring's Name reads correctly in each mode, and that the status TextBlock announces on change (Narrator on Windows or TalkBack on Android, spot check).
7. **Size tiers:** screenshot Hero at 240/320/480 and Compact at 24/40/64, and confirm no stroke falls below about 0.5 px or rises above about 3 px.
8. **WASM and Android smoke test:** all three modes. On Android, turn on "Remove animations" and confirm Scan shows a still ring with "Scanning".

---

## Risks — time-boxed spikes (run before step 4 of the plan)

| # | Question | Box | If it fails |
|---|---|---|---|
| R1 | Does the keyed custom template on the MUX `ProgressRing` render on the pinned Uno.Sdk with no Lottie package? (Source says yes.) | 10 min | Check the `Uno.WinUI.Lottie` requirement; add it only if the build demands it |
| R2 | Does a generated `VisualTransition` animate from the live rotating angle to 0, or snap? | 15 min | Accept the snap, which the pulse and reticle motion mask; otherwise Stop Check on the D1 fallback |
| R3 | Does leaving `Active` stop the Forever storyboard, bringing CPU back to baseline? | 15 min | Stop Check: move to the subclass plus 30 fps DispatcherTimer |
| R4 | Does `StrokeThickness` animation (`EnableDependentAnimation=True`) work on Skia? | 10 min | Cross-fade a second, heavier ring via Opacity |
| R5 | Binding order Idle→Lock: does setting `IsActive` before `IsIndeterminate` pass through `Active` for a frame? | 10 min | Reorder the notifications in the ViewModel (IsIndeterminate first), or accept it if invisible |
| R6 | Does `UISettings.AnimationsEnabled` reflect the Android system setting? | 10 min | Rely on the in-app toggle only |
| R7 | Does the RadioButton automation peer's default action work through the App MCP? | 5 min | Use client-coordinate clicks |

If any spike goes past its time box, stop and write a Debug Checkpoint.

### Spike outcomes (Uno.Sdk 6.7.30, `net10.0-desktop` on Linux X11/Xvfb)

| # | Outcome |
|---|---|
| R1 | **Pass.** The keyed template builds and renders with no Lottie package and no `Uno0001` warnings. |
| R2 | **Pass, with a different mechanism.** Uno's VSM ignores `GeneratedDuration`, but `TurnOverAnimationsTo` pauses outgoing animations at their live value. Each state storyboard animates every property with From-less keyframes, and Scan→Lock eases from the live angle (confirmed with a frame burst). |
| R3 | **Pass.** Release CPU is about 2.2 cores in Scan (software GL under Xvfb) and 0.0–0.2% in Lock and Idle. |
| R4 | **Pass.** The `StrokeThickness` keyframe animation (`EnableDependentAnimation`) renders on Skia. |
| R5 | **Pass by construction.** The generated setter runs `OnModeChanged` (which sets `IsIndeterminate`) before `PropertyChanged(IsActive)`. |
| R6 | Not run (no Android here). |
| R7 | Not run (no App MCP here). Driven with X11 XTEST input and Playwright on WASM instead. |

Also found during the build:
- The 6.7 `recommended` preset defaults to `SimpleTheme` and generates no Shell. The app was scaffolded with `-theme material`, and the generated `Main` route is kept.
- `XYFocusKeyboardNavigation` has no effect on the segment row, so arrow-key navigation is open (see the README).

---

## Implementation Plan

Each step ends with a build and a commit.

1. `chore: scaffold ReticleLab`: `dotnet new unoapp -preset recommended -presentation mvvm -o ReticleLab` targeting desktop, WASM and Android. Copy `.mcp.json`, pin Uno.Sdk in `global.json`, build.
2. `spike: progressring template states`: a throwaway page for R1–R4 and R7. Record the outcomes under Decisions, then delete the page.
3. `feat: add reticle geometry generator`: port Appendix A's math to `tools/reticle-geometry.cs` and check that its output matches Appendix A.
4. `feat: add HUD tokens and fonts`: `Hud.xaml`, the static TTFs and the text styles.
5. `feat: add static ReticleRingStyle`: all layers plus the `Inactive` state only. Screenshot parity against board B.
6. `feat: add scan state`: the `Active` storyboard (four rotations). Take the CPU measurement.
7. `feat: add lock state and transitions`: `DeterminateActive`, the pulse, the reticle and the lock dot.
8. `feat: add ReticleRingCompactStyle`.
9. `feat: add sample page`: the ViewModel, `HudSegmentStyle`, status row, compact row and adaptive trigger.
10. `feat: map reduced motion`: `UISettings.AnimationsEnabled` to `MotionEnabled` (plus the toggle if approved).
11. `test: runtime verification pass`: the Interaction Brief steps on desktop, then WASM and Android. Add the CPU table to the README.
12. `docs: add README`: the retemplating pattern, the state mapping and the measured costs.

---

## Unresolved Questions

> Built with these defaults. Each is easy to reverse:
> standalone `ReticleLab` · keep the Idle split (Hero 35% / Compact hidden) · determinate = Lock · decoration stays in the Hero template · no in-app Motion toggle · no iOS · compact 64/40/24 · fallback not needed (R2/R3 passed).

- **Home:** standalone `ReticleLab`, or folded into the hardware-panel retemplating demo as one more stock control?
- **Idle visibility:** Hero stays visible at 35% (prototype) while Compact hides (WinUI convention). Keep that split, or hide both?
- **Determinate = Lock:** is it OK that any determinate `Value` renders as Lock with no progress arc? The alternative is lighting the tick scale by `Value`, which overlaps Component A (Orbit Dial).
- **Decoration inside the control:** should the callouts, marker and dot clusters (L11–L12) stay in the Hero template, or move to the page so the control stays a clean ring?
- **Reduced motion on desktop/WASM:** add an in-app "Motion" toggle, or rely on Android's system setting only?
- **Targets:** include iOS?
- **Compact sizes:** 64/40/24 replaces the prototype's 96/48/24, because 96 px falls outside both stroke tiers. Is that acceptable?
- **Fallback trigger:** if R2 or R3 fails, approve the switch to a `ReticleRing : ProgressRing` subclass with timer-driven rotation? That drops "template only" from the story.

---

## Appendix A — Geometry

**Hero:** 600-unit canvas, center (300,300). Angles run clockwise from 12 o'clock: `P(r,a) = (300 + r·sin a, 300 − r·cos a)`. Every arc is circular, with the large-arc flag set when the sweep is over 180°.

| Part | Definition |
|---|---|
| OuterArcs | arc r240 0°→195°; arc r235 35°→88°; tick r240→248 at 90° |
| TickScale | ticks r222→229 from 200° to 356°, step 3°; every 5th tick to r234 |
| MainHalf | arc r215 180°→360°; knob at (300,85) |
| GapArc | arc r156 150°→420° |
| Band | ticks r168→176 from 95° to 190°, step 2°; every 6th tick to r181 |
| Radials | r182→206 at 105/122/140/158°; r176→190 at 225/315°; r170→185 at 0°; r172→190 at 270°; r232→262 at 270°; r180→214 at 90° |
| Reticle | r22→34 at 45/135/225/315° |

**Hero.OuterArcs**
```
M300,60 A240,240 0 1 1 237.88,531.82 M434.79,107.5 A235,235 0 0 1 534.86,291.8 M540,300 L548,300
```

**Hero.TickScale**
```
M224.07,508.61 L219.97,519.89 M213.26,504.35 L210.52,510.8 M202.68,499.53 L199.61,505.82 M192.37,494.17 L188.98,500.29 M182.36,488.27 L178.65,494.2 M172.67,481.85 L165.78,491.68 M163.32,474.94 L159.01,480.45 M154.35,467.55 L149.76,472.83 M145.79,459.69 L140.92,464.73 M137.64,451.4 L132.52,456.18 M129.94,442.7 L120.75,450.41 M122.7,433.6 L117.11,437.82 M115.95,424.14 L110.15,428.06 M109.71,414.34 L103.71,417.94 M103.99,404.22 L97.81,407.51 M98.8,393.82 L87.92,398.89 M94.17,383.16 L87.67,385.78 M90.09,372.28 L83.48,374.56 M86.6,361.19 L79.87,363.12 M83.69,349.94 L76.87,351.51 M81.37,338.55 L69.55,340.63 M79.65,327.05 L72.71,327.91 M78.54,315.49 L71.56,315.97 M78.03,303.87 L71.03,304 M78.14,292.25 L71.14,292.01 M78.84,280.65 L66.89,279.61 M80.16,269.1 L73.23,268.13 M82.08,257.64 L75.21,256.3 M84.59,246.29 L77.8,244.6 M87.7,235.09 L81.01,233.05 M91.39,224.07 L80.11,219.97 M95.65,213.26 L89.2,210.52 M100.47,202.68 L94.18,199.61 M105.83,192.37 L99.71,188.98 M111.73,182.36 L105.8,178.65 M118.15,172.67 L108.32,165.78 M125.06,163.32 L119.55,159.01 M132.45,154.35 L127.17,149.76 M140.31,145.79 L135.27,140.92 M148.6,137.64 L143.82,132.52 M157.3,129.94 L149.59,120.75 M166.4,122.7 L162.18,117.11 M175.86,115.95 L171.94,110.15 M185.66,109.71 L182.06,103.71 M195.78,103.99 L192.49,97.81 M206.18,98.8 L201.11,87.92 M216.84,94.17 L214.22,87.67 M227.72,90.09 L225.44,83.48 M238.81,86.6 L236.88,79.87 M250.06,83.69 L248.49,76.87 M261.45,81.37 L259.37,69.55 M272.95,79.65 L272.09,72.71 M284.51,78.54 L284.03,71.56
```

**Hero.MainHalf**
```
M300,515 A215,215 0 0 1 300,85
```

**Hero.GapArc**
```
M378,435.1 A156,156 0 1 1 435.1,222
```

**Hero.Band**
```
M467.36,314.64 L480.31,315.78 M466.75,320.47 L474.69,321.45 M465.93,326.28 L473.83,327.53 M464.91,332.06 L472.77,333.58 M463.69,337.79 L471.49,339.59 M462.28,343.48 L470,345.55 M460.66,349.12 L473.09,352.92 M458.85,354.7 L466.41,357.3 M456.84,360.21 L464.31,363.07 M454.64,365.64 L462.01,368.77 M452.26,371 L459.51,374.38 M449.69,376.27 L456.82,379.9 M446.94,381.45 L458.31,387.75 M444,386.53 L450.86,390.65 M440.9,391.5 L447.61,395.86 M437.62,396.36 L444.17,400.95 M434.17,401.1 L440.56,405.92 M430.56,405.73 L436.78,410.76 M426.79,410.22 L436.6,418.75 M422.87,414.58 L428.72,420.03 M418.79,418.79 L424.45,424.45 M414.58,422.87 L420.03,428.72 M410.22,426.79 L415.47,432.83 M405.73,430.56 L410.76,436.78 M401.1,434.17 L408.93,444.55 M396.36,437.62 L400.95,444.17 M391.5,440.9 L395.86,447.61 M386.53,444 L390.65,450.86 M381.45,446.94 L385.33,453.93 M376.27,449.69 L379.9,456.82 M371,452.26 L376.49,464.04 M365.64,454.64 L368.77,462.01 M360.21,456.84 L363.07,464.31 M354.7,458.85 L357.3,466.41 M349.12,460.66 L351.46,468.31 M343.48,462.28 L345.55,470 M337.79,463.69 L340.72,476.36 M332.06,464.91 L333.58,472.77 M326.28,465.93 L327.53,473.83 M320.47,466.75 L321.45,474.69 M314.64,467.36 L315.34,475.33 M308.79,467.77 L309.21,475.76 M302.93,467.97 L303.16,480.97 M297.07,467.97 L296.93,475.97 M291.21,467.77 L290.79,475.76 M285.36,467.36 L284.66,475.33 M279.53,466.75 L278.55,474.69 M273.72,465.93 L272.47,473.83
```

**Hero.Radials**
```
M475.8,347.11 L498.98,353.32 M454.34,396.45 L474.7,409.16 M416.99,439.42 L432.41,457.81 M368.18,468.75 L377.17,491 M175.55,424.45 L165.65,434.35 M175.55,175.55 L165.65,165.65 M300,130 L300,115 M128,300 L110,300 M68,300 L38,300 M480,300 L514,300
```

**Hero.Reticle**
```
M315.56,284.44 L324.04,275.96 M315.56,315.56 L324.04,324.04 M284.44,315.56 L275.96,324.04 M284.44,284.44 L275.96,275.96
```

**Hero.Callouts**
```
M405,150 L450,78 L532,78 M478,470 L543,528 M175,478 L175,500 A240,240 0 0 0 235,535 L230,543 L55,543
```

**Hero.Marker** (closed triangle, standard stroke, round joins; plus a Bg dot r2.5 at (43,88))
```
M42,62 L72,62 L57,87 Z
```

**Hero.Dots** (center, radius, style): on = Fill Fg · dim = Fill Bg · off = Stroke Fg 1.3, no fill
```
TR cluster:  (539,68) r3.5 dim · (549,68) r3.5 on · (539,78) r3 off
BR cluster:  (550,513) r3.5 on · (550,522) r3.5 dim · (550,540) r3 off
BL row:      (58,533) dim · (69,533) on · (79,533) dim · (90,533) on   [all r3.5]
```

**Compact:** 40-unit canvas, center (20,20).

**CompactArc** (r18, 0°→70°)
```
M20,2 A18,18 0 0 1 36.91,13.84
```

**CompactGap** (r12, 150°→420°)
```
M26,30.39 A12,12 0 1 1 30.39,14
```
