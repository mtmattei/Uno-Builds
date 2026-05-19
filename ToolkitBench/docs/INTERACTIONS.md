# Interaction Brief

Per-animation timing, easings, key-frame values, and XAML strategy. Pairs with [DESIGN.md](./DESIGN.md) (visual specs) and [ARCHITECTURE.md](./ARCHITECTURE.md) (code structure).

---

## Easing catalog

Define all easings as `KeySpline` resources in `Tokens.xaml`. Names match CSS variables in the source HTML.

| Resource key | CSS bezier | Use |
|---|---|---|
| `EaseMech` | `cubic-bezier(0.85, 0, 0.15, 1)` | mechanical, quick decel — button hovers, opacity transitions |
| `EaseInk` | `cubic-bezier(0.65, 0, 0.35, 1)` | ink absorbing — progress line, fade transitions |
| `EaseStrike` | `cubic-bezier(0.2, 0.9, 0.2, 1.4)` | typewriter return with overshoot — chip press, ink-land |
| `EaseWater` | `cubic-bezier(0.45, 0, 0.55, 1)` | liquid find-level — drawer shadow, close-button reveal |
| `EaseFlip` | `cubic-bezier(0.18, 0.74, 0.22, 1.18)` | coin landing — chip flip, drawer snap |
| `EaseUnfold` | `cubic-bezier(0.45, 0.05, 0.1, 1)` | hinge with weight — reserved |
| `EaseMorph` | `cubic-bezier(0.45, 0, 0.15, 1)` | shared icon morph easing — TabBar |
| `EaseBackOut` | `cubic-bezier(0.34, 1.36, 0.64, 1)` | back-out with light overshoot — TabBar scale-pulse |

In XAML, a `KeySpline x="0.85,0" y="0.15,1"` corresponds to `cubic-bezier(0.85, 0, 0.15, 1)`. Use `<KeySplineDoubleKeyFrame>` in storyboards.

---

## 01 — NavigationBar: split-flap title cascade

**Trigger**: forward (•••) or back (‹) button click; gated by a `_busy` flag while a cascade is running.

**Approach**: Time-driven via `DispatcherTimer`, one timer per character.

### Composition

`SplitFlapText` is a custom control wrapping an `ItemsControl`:

- `ItemsSource` is a `Feed<ImmutableArray<char>>` exposing the current target string padded to the longest length seen so far
- `ItemTemplate` produces a `TextBlock` per character with a `CompositeTransform` exposing `ScaleY`
- The control owns a private `Dictionary<int, DispatcherTimer>` mapping char index → its current cycling timer

### Sequence (per character at index `i`)

1. **Start delay**: `i * 50ms`
2. **Cycle phase**: every `55ms`, replace `Text` with a random `A–Z` letter; trigger a `tick` storyboard:
   - `ScaleY: 1.0 → 0.35` over 40ms
   - `ScaleY: 0.35 → 1.0` over 40ms
   - Total 80ms, no easing (use `LinearDoubleKeyFrame`)
   - Color: `Fg4Brush` during cycling
3. **Settle**: after `4 + Random.Next(3)` cycles, set text to target char, run one final tick storyboard, color → `Fg1Brush` over 120ms

### Total duration

`(charCount * 50) + (7 * 55) + 100` ms — used to gate the next nav action.

### Body crossfade (parallel)

The `nav-body` body fades on each navigation:
- `Opacity 1 → 0` over 200ms (no easing) — body content swaps when opacity hits 0
- `Opacity 0 → 1` over 200ms

### Sample stack

Hardcoded 3-page stack with title and body content per page. Page 1 is the resting state on app launch.

### Edge cases

- Rapid clicks gated by `_busy` flag; ignore until cascade completes
- Back disabled at `depth=0`; forward disabled at `depth=stackLength-1`

---

## 02 — TabBar: identity morph (per-icon)

**Trigger**: `TabBar.SelectionChanged`, only when new selection differs.

**Approach**: State-driven, pure `VisualStateManager` per tab item. JS in the HTML reference is just toggling the `.active` class — the morph itself is declarative CSS transitions on SVG attributes. The XAML equivalent uses storyboards on `Path.Data`, `Width`, `Height`, `X`, `Y`, etc.

### Shared timing

All four icon morphs use:
- **Duration**: 520ms
- **Easing**: `EaseMorph` (`cubic-bezier(0.45, 0, 0.15, 1)`)
- Properties animated: `Path.Data` (or `Geometry`), transform `Width`/`Height`, color `Fill`/`Stroke`, `Opacity`, `Rectangle.X/Y/Width/Height/RadiusX/RadiusY`

### Whole-icon scale-pulse (parallel)

When a tab activates, its entire icon group scales:
- `CompositeTransform.ScaleX/Y: 1.0 → 1.06`
- Easing: `EaseBackOut` (back-out with overshoot)
- Duration: 540ms

When deactivating, scale returns to 1.0 with the same easing/duration.

### Per-tab morph specs

#### Home: pentagon outline → pentagon with carved doorway

The icon is a single `Path` with 10 vertices (M + 9 L commands). Six trace the outer pentagon; four trace where the doorway will appear. In rest, the door points sit on the bottom edge. On active, only their Y-coordinates change.

```
Rest:    M 4 20 L 4 10 L 12 4 L 20 10 L 20 20 L 14 20 L 14 20 L 10 20 L 10 20 L 4 20
Active:  M 4 20 L 4 10 L 12 4 L 20 10 L 20 20 L 14 20 L 14 13 L 10 13 L 10 20 L 4 20
                                                            ^^^^^^^^^^^
                                                            (door extends up to y=13)
```

Both states have identical command sequences. Animate via `PathDoubleKeyFrame` on the geometry, or use `MorphIcon` custom control that interpolates two `PathGeometry` instances.

#### Search: lens (rect with rx=6) → horizontal pill (search bar)

The lens is a `<rect>` with `RadiusX="6"` (visually a circle at rest). All transitions on the rect's properties:

| Property | Rest | Active |
|---|---|---|
| `X` | 3 | 2 |
| `Y` | 5 | 7 |
| `Width` | 12 | 19 |
| `Height` | 12 | 8 |
| `RadiusX` | 6 | 4 |
| `RadiusY` | 6 | 4 |
| `Stroke` | currentColor | `AccentBrush` |

Plus the handle (a `Line`):
| Property | Rest | Active |
|---|---|---|
| `Opacity` | 1 | 0 |
| `X1, Y1` | 13.5, 15.5 | 13.5, 15.5 |
| `X2, Y2` | 18, 20 | 13.5, 15.5 (collapses to point) |

Plus a cursor (a `Line`, hidden at rest):
| Property | Rest | Active |
|---|---|---|
| `Opacity` | 0 | 1 (delay 320ms) |
| `Stroke` | — | `AccentBrush` |

Cursor blink animation (after the morph): `Opacity 1 ↔ 0.25` infinite, 1.1s period, `EaseInOut`. Begin at +700ms after activation.

#### Library: 2×2 grid → 4 horizontal stripes

Four `Rect`s with classes `lib-1` through `lib-4`. Each animates its `X`, `Y`, `Width`, `Height` simultaneously.

| Rect | Rest (`x, y, w, h`) | Active |
|---|---|---|
| lib-1 | `(3, 4, 7, 7)` | `(3, 4.5, 18, 3)` |
| lib-2 | `(14, 4, 7, 7)` | `(3, 9.0, 18, 3)` |
| lib-3 | `(3, 14, 7, 7)` | `(3, 13.5, 18, 3)` |
| lib-4 | `(14, 14, 7, 7)` | `(3, 18, 18, 3)` |

All rects' `Stroke` shifts from `currentColor` to `AccentBrush` on active. `RadiusX/Y=1` on active for slight pill-corners.

#### Profile: outline figure → outline figure + status dot

The figure stays unchanged (head circle + body curve). Two extra circles appear:

**Status ring** (acts as mask backdrop, `Bg1Brush` color):
- `cx=16.5, cy=5`
- `r: 0 → 3.2`
- `Fill="Bg1Brush"` `Stroke="Bg1Brush"`

**Status dot** (the visible accent mark):
- `cx=16.5, cy=5`
- `r: 0 → 2.2` with a 120ms delay (so the ring carves the space first)
- `Fill="AccentBrush"` no stroke

### Cancellation

The HTML uses CSS transitions which interrupt cleanly. In XAML, before starting any new storyboard on a tab item, call `Storyboard.Stop()` on any in-flight storyboards on that item, then reset target properties to the appropriate state.

### Canvas label crossfade (parallel)

The `phone-canvas` label updates simultaneously with the morph:
- Old label `Opacity 1 → 0` over 200ms
- Swap text
- New label `Opacity 0 → 1` over 200ms

---

## 03 — ChipGroup: coin flip with cascade

**Trigger**: chip click. Selection logic:
- Click "All" → if everyone is checked, **cascade-uncheck all**; else **cascade-check all**
- Click any non-All → instant single flip; if nothing selected after toggle, cascade-recheck everyone
- "All" chip uses `AccentBrush` on its back face; rest use `Fg1Brush`

**Approach**: Per-chip flip is state-driven (VisualStateManager). Cascade adds per-element start delays before applying the state change.

### Composition

Each chip's template contains a `Grid` (the inner) with two `Border` faces overlapping in the same cell. `PlaneProjection` is applied to the inner.

### Visual states

`Unchecked`:
- `chip-inner.PlaneProjection.RotationY = 0`
- `frontFace.Opacity = 1`
- `backFace.Opacity = 0`

`Checked`:
- `chip-inner.PlaneProjection.RotationY = 180`
- `frontFace.Opacity = 0`
- `backFace.Opacity = 1`

The opacity binding is a workaround for `BackfaceVisibility` not being natively exposed on WinUI's `PlaneProjection`. Use a `BoolToOpacityConverter` keyed on whether `RotationY % 360` is in `[90, 270]`.

### Storyboard (each `VisualState.Storyboard`)

- Target: `chip-inner.PlaneProjection.RotationY`
- Duration: 600ms
- `EaseFlip` (`KeySpline 0.18,0.74 0.22,1.18`) — slight overshoot at landing

### Pressed micro-animation (separate VSM group)

- `Pressed`: `CompositeTransform.TranslateY=1, ScaleX=0.97, ScaleY=0.97`
- Duration: 180ms, `EaseStrike`

### Cascade implementation

When "All" is tapped to toggle every chip, iterate the chips in DOM order and apply the state change with a per-chip delay:

```csharp
const int STAGGER_MS = 70;
foreach (var (chip, i) in chips.Select((c, i) => (c, i)))
{
    var delay = TimeSpan.FromMilliseconds(i * STAGGER_MS);
    DispatcherQueue.TryEnqueue(async () =>
    {
        await Task.Delay(delay);
        VisualStateManager.GoToState(chip, target == "checked" ? "Checked" : "Unchecked", true);
    });
}
```

5 chips × 70ms = 350ms of lead-in plus 600ms flip = ~950ms end-to-end. This is the satisfying part — clicking "All" reads as one wave gesture, not five simultaneous flips.

### Single-chip toggles

Skip the cascade entirely; just call `VisualStateManager.GoToState` immediately. After the toggle, check whether any chip remains selected — if not, trigger the cascade-check on all chips so selection never goes empty.

---

## 04 — DrawerControl: pull-tab drag with resistance + bounce

**Trigger**: pointer-press on the tab → drag left → release. Or click without drag (toggles). Or keyboard arrows.

**Approach**: Gesture-driven during drag (procedural transform writes per pointer event), state-driven on release (storyboard with overshoot keyframes).

### Composition

`DrawerControl` from the Toolkit handles the basic two-pane structure. Override the template to:

1. Add a custom **pull tab** as a child of the drawer panel (so it moves with the panel)
2. Add a **hint progress bar** as a sibling of the drawer frame (below it)
3. Hook the drawer's manipulation events (or use a custom drag controller)

### Drag math

State variables:
- `_openness: double` — logical 0..1 (clamped)
- `_visualOpenness: double` — includes resistance and rubber-band (can exceed [0, 1])
- `_dragging: bool`
- `_startX: double`, `_startOpenness: double`, `_panelWidth: double`

**Resistance curve** (only applied when dragging from closed):

```
COMMIT_THRESHOLD = 0.15
COMMIT_RAMP = 0.30

if startOpenness < COMMIT_THRESHOLD:
    if rawOpen < COMMIT_THRESHOLD:
        return rawOpen * 0.35           // 35% sensitivity zone
    elif rawOpen < COMMIT_THRESHOLD + COMMIT_RAMP:
        return ramped(rawOpen)          // linear ramp from 35% → 100%
return rawOpen                          // 1:1 elsewhere
```

**Rubber-band overdrag**:

```
RUBBER = 0.33
if rawOpen < 0:    return rawOpen * RUBBER
if rawOpen > 1:    return 1 + (rawOpen - 1) * RUBBER
```

**Apply to transform**:
```
panel.CompositeTransform.TranslateX = (1 - visualOpenness) * panelWidth
```
where `panelWidth` is the panel's actual width in pixels.

### Snap on release

When `PointerReleased` fires:
1. Compute `target = openness >= 0.5 ? 1 : 0`
2. Read current `lastTx` (last applied translateX value, in % of panel width)
3. Run a custom `Storyboard` with overshoot keyframes:

**Snap-open (target = 1)**:
| Time | TranslateX% |
|---|---|
| 0% | `lastTx` (current position) |
| 55% | `-7%` (overshoot — panel pulls 7% past its left rest position) |
| 78% | `+2.5%` (counter-rebound) |
| 100% | `0%` (settled) |

Duration: 540ms, easing: `EaseFlip`

**Snap-closed (target = 0)**:
| Time | TranslateX% |
|---|---|
| 0% | `lastTx` |
| 55% | `+107%` (overshoot — panel pushes 7% past its right rest position) |
| 78% | `+97.5%` (counter-rebound) |
| 100% | `+100%` |

### Hint progress bar (parallel)

The accent-fill rectangle's `Width` animates synchronously with the snap:
- During drag: width = `openness * railWidth` (real-time)
- During snap: animate via storyboard with `Width 0 → railWidth` (or reverse) over 540ms with `EaseFlip`

The state label updates: `closed` / `pulling` (during drag) / `partial` (mid-drag) / `open`.

### Click-without-drag

If `|releaseX - pressX| < 4px`, treat as a click:
- If openness < 0.5, snap to 1
- Else snap to 0

### Keyboard

When the tab has focus:
- `←` / `Enter` / `Space` → snap to 1
- `→` / `Esc` → snap to 0

---

## 05 — LoadingView: honest timer

**Trigger**: `Run task` button click. State machine: `Idle → Running → Loaded → (Reset) → Idle`.

**Approach**: Time-driven via `DispatcherTimer` + storyboard for the progress line.

### Composition

`utu:LoadingView` with a custom `Source` implementing `ILoadable`. Three templates:
- `IdleTemplate` — dash + "Awaiting source."
- `LoadingTemplate` — `TimerReadout` bound to `Source.Elapsed`
- `LoadedTemplate` — final headline with `Source.LastElapsed`

### Source state machine

`TimerSourceModel : ILoadable, INotifyPropertyChanged`:

```csharp
public TimeSpan Elapsed         { get; }
public TimeSpan LastElapsed     { get; }
public LoadableState State      { get; }   // Idle | Running | Loaded

public async Task LoadAsync()
{
    State = Running;
    var targetMs = 1200 + Random.Shared.Next(700);   // 1200–1900
    var sw = Stopwatch.StartNew();
    var timer = DispatcherQueue.CreateTimer();
    timer.Interval = TimeSpan.FromMilliseconds(16);
    timer.Tick += (_, _) => { Elapsed = sw.Elapsed; OnPropertyChanged(nameof(Elapsed)); };
    timer.Start();

    await Task.Delay(targetMs);
    timer.Stop();
    LastElapsed = sw.Elapsed;
    State = Loaded;
}
```

### Progress line

Triggered when state enters `Running`:
- Storyboard target: `progressLine.Width` (or `RenderTransform.ScaleX`)
- Duration: matched to `targetMs` (the same random target the source uses)
- From: 0
- To: pane content width
- Easing: `EaseInk`

When state enters `Loaded`, the line freezes at full width. When state resets to `Idle`, the line snaps back to 0 (no animation).

### Reset (Loaded → Idle)

Click `Reset`:
- Cancel timer + storyboard
- Reset `Elapsed = 0`, line `Width = 0`
- State → `Idle`

### State pill

Bind border + text color to `Source.State`:
- `Idle` → `RuleBrush` border, `Fg4Brush` text, `Bg1Brush` bg, "idle"
- `Running` → `AccentSoftBrush` border + bg, `AccentBrush` text, "running"
- `Loaded` → `OkSoftBrush` border + bg, `OkBrush` text, "done"

Transitions: 220ms `EaseMech` on color/border.

---

## Reduced motion

When `UISettings.AnimationsEnabled == false`:

| Demo | Reduced behavior |
|---|---|
| **NavigationBar** | Skip char-cycling; set target text + run a single tick storyboard for confirmation |
| **TabBar** | Skip path morph; jump directly to active state. Cross-fade label opacity instead. |
| **ChipGroup** | Skip cascade and rotation; cross-fade face opacities directly (300ms `EaseMech`) |
| **DrawerControl** | Skip drag; clicking the tab snaps directly to target with no animation |
| **LoadingView** | Skip timer ticking and line animation; show "loaded" state immediately on click |

Implement once at the demo-bindable level — read the static flag in each demo's `Loaded` handler.

---

## Cancellation pattern (used by TabBar and Drawer snap)

When a new animation needs to start before the previous completes:

```csharp
foreach (var anim in element.Resources.OfType<Storyboard>())
    anim.Stop();

// Reset target properties to a known resting state
ResetCompositeTransform(element);

// Start new
newStoryboard.Begin();
```

Without this, `PlaneProjection`, `CompositeTransform`, and animated `Path.Data` properties accumulate residual values when storyboards are restarted mid-flight, producing visual glitches.

---

## Frame budget

Targets at 60Hz (16.67ms / frame):

| Demo | Per-frame work | Headroom |
|---|---|---|
| NavigationBar | 1 timer tick + 1 storyboard step per cycling char | OK up to ~15 chars |
| TabBar | 1 path interpolation + 1 transform + N opacity transitions | OK |
| ChipGroup (single flip) | 1 PlaneProjection + 1 opacity converter | OK |
| ChipGroup (cascade) | 5 simultaneous flips | OK; profile WASM |
| DrawerControl (drag) | 1 transform write per pointer move (~120Hz on desktop) | OK |
| DrawerControl (snap) | 1 keyframe-driven transform + 1 width animation | OK |
| LoadingView | 1 DispatcherTimer (16ms) + 1 width animation | bump timer to 33ms on WASM if needed |

Profile on the slowest target (WASM) before shipping. The DrawerControl drag is the most likely source of jank because every pointer event writes a transform; consider RAF-throttling if you see drift.
