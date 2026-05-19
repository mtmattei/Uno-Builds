# Design Brief

Every token, type style, dimension, gradient, and visual element. Values are exact and pulled from `uno-toolkit-bench-rows.html`.

---

## Palette

Pure neutral grayscale — saturation 0%, only lightness varies. The accent is true white (`#ffffff`), used sparingly as the brightest thing on screen. Defined as `SolidColorBrush` resources in `Themes/Tokens.xaml`.

### Surfaces

| Brush | Hex | Use |
|---|---|---|
| `Bg0Brush`        | `#1a1a1a` | canvas (the page itself, demo stage) |
| `Bg1Brush`        | `#232323` | masthead, spec column, footer |
| `Bg2Brush`        | `#2a2a2a` | demo containers (nav-frame, phone-frame, drawer-frame, loader-pane) |
| `Bg3Brush`        | `#333333` | hover elevations, drawer panel base |
| `Bg4Brush`        | `#3d3d3d` | highest elevation, button pressed |

### Surface gradient endpoints

Major surfaces use vertical gradients (180°) to suggest depth — top is slightly brighter, bottom slightly darker. Define these as separate brushes so gradients can be tuned globally.

| Brush | Hex | Used by |
|---|---|---|
| `Bg1GradientTop`  | `#262626` | masthead, spec, footer (top) |
| `Bg1GradientBot`  | `#1f1f1f` | masthead, spec, footer (bottom) |
| `Bg2GradientTop`  | `#2e2e2e` | nav-frame, phone-frame, drawer-frame, loader-pane (top) |
| `Bg2GradientBot`  | `#262626` | nav-frame, phone-frame, drawer-frame, loader-pane (bottom) |
| `Bg3GradientTop`  | `#383838` | nav-bar interior (top), drawer-tab |
| `Bg3GradientBot`  | `#2e2e2e` | nav-bar interior (bottom), drawer-panel |

The lightness shift across each gradient is ~3% — perceptible on tall surfaces, invisible on small ones.

### Hairlines

| Brush | Hex | Use |
|---|---|---|
| `RuleBrush`       | `#363636` | 1px borders, dividers, grid lines |
| `RuleSoftBrush`   | `#2a2a2a` | softer dividers (where contrast would be loud) |

### Foregrounds

| Brush | Hex | Use |
|---|---|---|
| `Fg1Brush`        | `#ededed` | primary text, active tab labels |
| `Fg2Brush`        | `#c2c2c2` | secondary text, drawer list items |
| `Fg3Brush`        | `#909090` | tertiary text, eyebrows |
| `Fg4Brush`        | `#707070` | meta text, inactive labels |
| `Fg5Brush`        | `#525252` | faintest, separators |

### Accent

| Brush | Hex | Use |
|---|---|---|
| `AccentBrush`       | `#ffffff` | the brightest mark — chip "All" pill, active tab morph elements (search cursor, profile status, library shelf strokes), progress line, running state pill text |
| `AccentSoftBrush`   | `#3a3a3a` | accent-tinted backgrounds (running state pill) |
| `AccentDeepBrush`   | `#c8c8c8` | reserved for future use; subtle highlights |

### State colors

| Brush | Hex | Use |
|---|---|---|
| `OkBrush`           | `#c0c0c0` | "loaded" / "done" state confirmation |
| `OkSoftBrush`       | `#353535` | done state pill background |

### Shadows

| Token | Spec |
|---|---|
| `Shadow1` | `0 1px 0 rgba(0,0,0,0.5)` — cards/frames at rest |
| `ShadowLift` | `0 22px 40px -22px rgba(0,0,0,0.8), 0 2px 6px -2px rgba(0,0,0,0.5)` — drawer panel projecting |
| `DrawerPanelShadow` | `-12px 0 24px -8px rgba(0,0,0,0.5)` — left edge of slid-out drawer panel |

---

## Typography

Three variable fonts. Register via `Assets/Fonts/embeddedFonts.json` and reference by family name.

| Family | Source | Variable axes used |
|---|---|---|
| **Fraunces** | Google Fonts | `opsz` (9–144), `wght` (300–600), italic |
| **Instrument Sans** | Google Fonts | `wght` (400–600) |
| **JetBrains Mono** | Google Fonts | `wght` (400–600) |

### Type styles

Defined in `Tokens.xaml` as `Style TargetType="TextBlock"`:

| Style key | Family | Size | Weight | Tracking | Other |
|---|---|---|---|---|---|
| `MastheadEyebrowStyle` | JetBrains Mono | 11 | 500 | 0.18em | uppercase, `Fg3Brush` |
| `MastheadMetaStyle` | JetBrains Mono | 10 | 400 | 0.06em | `Fg4Brush`; `<strong>` = `Fg1Brush` 500; separator `·` = `Fg5Brush` |
| `BreadcrumbStyle` | JetBrains Mono | 10 | 400 | 0.06em | `Fg4Brush`; `.crumb-active` = `Fg1Brush` |
| `SpecNumStyle` | JetBrains Mono | 12 | 600 | 0.06em | `AccentBrush` |
| `SpecNameStyle` | JetBrains Mono | 13 | 600 | 0.01em | `Fg1Brush`; `.ns` = `Fg5Brush` 500 |
| `SpecSummaryStyle` | Fraunces | 18 | 400 | -0.01em | `Fg1Brush`, line-height 1.25, opsz 14; `<em>` = 300 italic, `Fg3Brush` |
| `SpecTagStyle` | JetBrains Mono | 9.5 | 400 | 0.10em | uppercase, `Fg4Brush`, 1px `RuleBrush` border, `Bg2Brush` bg, pill |
| `MotionTagStyle` | JetBrains Mono | 9.5 | 400 | 0.10em | uppercase, `AccentBrush`, `AccentSoftBrush` border + bg, pill |
| `NavTitleStyle` | JetBrains Mono | 13 | 600 | 0.18em | uppercase, `Fg1Brush`; `.cycling` = `Fg4Brush`; `.settled` = `Fg1Brush` |
| `NavBodyStyle` | Fraunces | 14 | 300 | — | line-height 1.55, `Fg3Brush`, opsz 14; `<small>` = JetBrains Mono 10/400, 0.10em uppercase, `Fg4Brush`; `<strong>` = `Fg2Brush` 500 |
| `TabLabelStyle` | JetBrains Mono | 9.5 | 400 | 0.06em | uppercase; resting `Fg4Brush`, active `Fg1Brush` |
| `CanvasLabelStyle` | Fraunces | 14 | 300 italic | — | `Fg4Brush`, opsz 14 |
| `ChipFaceStyle` | Instrument Sans | 13 | 500 | -0.005em | line-height 1 |
| `DrawerEyebrowStyle` | JetBrains Mono | 10 | 400 | 0.18em | uppercase, `Fg4Brush` |
| `DrawerListItemStyle` | Instrument Sans | 13 | 400 | — | `Fg2Brush`; active = `AccentBrush`; meta = JetBrains Mono 10, `Fg5Brush` |
| `DrawerHintStyle` | JetBrains Mono | 10 | 400 | 0.06em | `Fg4Brush`; state = `Fg2Brush` 500 |
| `TimerReadoutStyle` | JetBrains Mono | 30 | 600 | -0.02em | tabular-nums, line-height 1, `Fg1Brush` |
| `TimerUnitStyle` | JetBrains Mono | 14 | 400 | — | `Fg4Brush`, ml 4 |
| `RunningLabelStyle` | JetBrains Mono | 10 | 400 | 0.12em | uppercase, `Fg4Brush`, mt 8 |
| `LoadedHeadlineStyle` | Fraunces | 14 | 500 | -0.005em | `Fg1Brush`; inline `.num` = JetBrains Mono 13/600, ml 4, `AccentBrush`, tabular-nums |
| `LoadedBodyStyle` | Instrument Sans | 12.5 | 400 | — | line-height 1.5, `Fg3Brush` |
| `LoadedMetaStyle` | JetBrains Mono | 10 | 400 | 0.04em | `OkBrush` |
| `BenchButtonStyle` | Instrument Sans | 12 | 600 | 0.02em | `Bg0Brush` text on `Fg1Brush` bg, hover bg → `AccentBrush` |
| `StatePillStyle` | JetBrains Mono | 10 | 400 | 0.08em | uppercase, idle = `Fg4Brush`/`RuleBrush`/`Bg1Brush`; running = `AccentBrush`/`AccentSoftBrush`; done = `OkBrush`/`OkSoftBrush` |
| `FooterStyle` | JetBrains Mono | 10 | 400 | 0.06em | `Fg5Brush`; `<strong>` = `Fg3Brush` 500 |

OpenType: enable `tnum` (tabular nums) on every numeric readout (`TimerReadoutStyle`, `LoadedHeadlineStyle .num`).

---

## Layout

### Page

- Outer container `MaxWidth=1440`, centered
- Background: `Bg0Brush`
- Side borders: 1px `RuleBrush` on left and right (frame the centered content from full-width)
- Faint grain overlay: full-window noise, `Opacity=0.25`, `BlendMode=Overlay`. SVG turbulence at `baseFrequency=0.9, octaves=2`, alpha 12% — adds tooth to dark surfaces. **Optional**, can be skipped on WASM if perf-sensitive.

### Masthead

Slim utility bar — eyebrow on the left, version meta on the right, single inline row.

- 2-column `Grid`: `1fr` + `auto`
- Padding: `18px 36px`
- Background: gradient `Bg1GradientTop → Bg1GradientBot` (180°)
- Bottom: 1px `RuleBrush`
- Alignment: center (vertical)

**Left**: `MastheadEyebrowStyle` "UNO TOOLKIT · BENCH"
**Right**: meta line `v.6.4 · 2026 · 05 controls` with hairline `·` separators

### Strip (breadcrumb bar)

Below the masthead, a thin row of context info.

- 2-column `Grid`: breadcrumb left, meta right
- Padding: `12px 36px`
- Background: `Bg0Brush`
- Bottom: 1px `RuleBrush`

**Left**: "Bench / Components" — `BreadcrumbStyle`
**Right**: "SKIA · WASM · DESKTOP · **05 / 05**"

### Row grid

- Vertical `StackPanel` (or `Grid` with `Auto` rows) — one row per component
- Each row: 1px `RuleBrush` bottom border (no border on last)

### Row

- 2-column `Grid`: `320px` + `1fr`
- `MinHeight=280`

### Spec column (left)

- Padding: `32px 28px 32px 36px`
- Right border: 1px `RuleBrush`
- Background: gradient `Bg1GradientTop → Bg1GradientBot` (180°)
- Layout: vertical `StackPanel`, `Spacing=14`, `VerticalAlignment=Center`

Stack contents (top to bottom):

1. **Header row** — number `SpecNumStyle` + name `SpecNameStyle`, gap 14
2. **Summary** — `SpecSummaryStyle` headline (one line, italic span for the metaphor word)
3. **Tag row** — flex-wrap of pills, gap 6:
   - First tag: motion name in `MotionTagStyle` (white-on-dark accent fill)
   - Remaining: identity tags in `SpecTagStyle` (muted)

### Stage column (right)

- Padding: `32px 36px`
- Background: `Bg0Brush`
- Layout: centered `Grid` with `MaxWidth=720` content
- `Perspective=1500` for the chip flip (set on the column)

**Decorative crosshairs** in opposite corners:
- Top-left: 12×12 `Border` with only `BorderThickness="1,1,0,0"` and `RuleBrush`
- Bottom-right: 12×12 `Border` with only `BorderThickness="0,0,1,1"` and `RuleBrush`

### Footer

- Padding: `24px 36px`
- Background: gradient `Bg1GradientTop → Bg1GradientBot` (180°)
- Top border: 1px `RuleBrush`
- 2-column flex: `uno.toolkit.ui · platform.uno` left, `Build with restraint.` right
- `FooterStyle`

---

## Component visual specs

### 01 — NavigationBar demo

**Frame**:
- `MaxWidth=640`
- Background: gradient `Bg2GradientTop → Bg2GradientBot` (180°)
- 1px `RuleBrush`, `Shadow1`

**Bar**:
- Height 52, padding `0,6`
- Background: gradient `Bg3GradientTop → Bg2GradientBot` (180°) — slightly brighter than the body for "frosted lid" feel
- Bottom: 1px `RuleBrush`

**Buttons** (back, forward):
- 44×44, `CornerRadius=4`
- Icon 18×18, `Foreground=Fg2Brush`, stroke-width 1.6
- Hover: `Bg3Brush` background; pressed: `Bg4Brush`
- Disabled: `Opacity=0.25`

**Title**:
- `NavTitleStyle`, centered, monospace mono cells per character
- Each char wrapped in a `TextBlock` with `MinWidth=0.6em` and `CompositeTransform.ScaleY=1` resting

**Body**:
- Padding `24,24,24,28`, `MinHeight=96`
- Background: gradient `Bg2GradientTop → Bg2GradientBot` (180°)
- Small caption "PAGE *n* OF 3" mb 6
- Body paragraph

### 02 — TabBar demo

**Phone frame**:
- `MaxWidth=260`, aspect 9:11
- Background: gradient `Bg2GradientTop → Bg2GradientBot` (180°)
- 1px `RuleBrush`, `CornerRadius=22`, `Shadow1`

**Canvas (top)**:
- Background: gradient `Bg2GradientTop → Bg1GradientBot` (180°)
- Centered `CanvasLabelStyle` text reflecting active tab

**Bar (bottom)**:
- Height 60, 4-column `UniformGrid`
- Top border: 1px `RuleBrush`
- Background: `Bg1Brush`

**Tab item**:
- `StackPanel Orientation=Vertical`, gap 4, centered
- Icon container: SVG/`Path` 22×22 (`ClipToBounds=False` to allow morph elements to extend if needed)
- `TabLabelStyle` text below
- Color: `Fg4Brush` resting; active = `Fg1Brush`

**Whole-icon scale-pulse**: each tab's `Path` group has `CompositeTransform`. Active state: `ScaleX=1.06, ScaleY=1.06`. Inactive: 1.0. Easing: back-out (`KeySpline` matching `cubic-bezier(0.34, 1.36, 0.64, 1)`), 540ms.

**Per-tab paths and morph targets** — see INTERACTIONS.md §02 for the actual `d` strings and animation details. Summary of visual identity per state:

| Tab | Rest | Active morph |
|---|---|---|
| Home | Outline pentagon house | Same outline + carved rectangular doorway in bottom |
| Search | Circular lens (`rect rx=6`) + diagonal handle line | Lens stretches into horizontal pill (search bar) + handle collapses + cursor blink appears at right end |
| Library | 4 outlined squares in 2×2 grid | All 4 squares morph into 4 horizontal bars stacked = bookshelf view |
| Profile | Outline figure (head circle + shoulders curve) | Same outline + accent-white status dot at upper-right of head, with dark `Bg1Brush` ring as backdrop |

All icon morphs share the same easing (`cubic-bezier(0.45, 0, 0.15, 1)`) and duration (520ms).

### 03 — ChipGroup demo

**Group**:
- `WrapPanel` (or `ItemsControl` with wrap), gap 10, centered, `MaxWidth=380`
- Parent has `Perspective=720`
- Below: readout text, mb 18

**Chip**:
- Height 36, transparent button host
- Inner = `Grid` with two faces overlapping in same cell (face stack). Critical for sizing — see ARCHITECTURE.
- `PlaneProjection` on inner; `RotationY=0` resting, `RotationY=180` checked
- Border-radius 999 on faces

**Front face**:
- Background `Bg2Brush`, 1px `RuleBrush`, `ChipFaceStyle` text in `Fg2Brush`
- Padding `18,0`
- Hover (when not checked): border `Fg4Brush`, text `Fg1Brush`

**Back face**:
- Background `Fg1Brush`, 1px `Fg1Brush`, `ChipFaceStyle` text in `Bg0Brush`
- Pre-rotated `RotationY=180`
- "All" chip override: bg + border = `AccentBrush` (pure white), text = `Bg0Brush`

**Pressed**: `TranslateY=1, Scale=0.97`, transition 180ms `ease-strike`

**Readout**:
- "Selected: " + selected ids comma-separated, `Fg2Brush`
- Min-height 14, centered, mono

### 04 — DrawerControl demo

**Stage** (column, `MaxWidth=360`, gap 14):

**Frame** (the visible app shell):
- Width: 100%, height 200
- Background: gradient `Bg2GradientTop → Bg2GradientBot` (180°)
- 1px `RuleBrush`, `Shadow1`
- `ClipToBounds=True`

**Main content** (underneath the panel, always visible):
- Inset 18,20
- 3 stacked items, `VerticalAlignment` distributed:
  - "App · Workspace" eyebrow `JetBrainsMonoMonospace 10 0.12em uppercase Fg5Brush`
  - Italic body `Fraunces 13 300 italic Fg4Brush max-width 26ch`
  - Meta "3 unread · synced 02:14" `JetBrainsMonoMonospace 10 0.06em Fg5Brush`

**Drawer panel** (the slide-in pane):
- Position: right-anchored, top to bottom of frame, width 78%
- Background: gradient `Bg3Brush → Bg2Brush` (90°, horizontal — simulates light hitting leading edge)
- Left border: 1px `RuleBrush`
- Box-shadow: `DrawerPanelShadow` (left-projecting)
- Initial transform: `TranslateX=100%` (offscreen-right)

**Panel content** (4 list items + label):
- Padding `16,18,22,16`
- Eyebrow "WORKSPACE" `DrawerEyebrowStyle`
- 4 list rows, each a 3-col `Grid` `18px 1fr auto`:
  - Icon (Inbox/Projects/Recent/Settings) 14×14 stroke 1.6 `Fg4Brush` (or `AccentBrush` if active)
  - Label `DrawerListItemStyle`
  - Meta (count or shortcut) `JetBrainsMono 10 Fg5Brush`
  - Hover: bg `Bg4Brush`, color `Fg1Brush`

**Pull tab** (visible handle on left edge of panel):
- Position: `top: 50%, left: -1px, transform: translate(-100%, -50%)`
- Size: 22×56
- Background: gradient `Bg3GradientTop → Bg2GradientBot` (90°)
- Border: 1px `RuleBrush` on top/bottom/left only (joins seamlessly with panel)
- `CornerRadius="4,0,0,4"` (rounded only on the left)
- Cursor: grab (grabbing while dragging)
- Content: 3 vertically-stacked grip dots (3×3, `Fg4Brush`)
- Hover: background → `Bg4Brush`
- Dragging state: bg → `AccentSoftBrush`, dots → `AccentBrush`

**Hint progress bar** (under the frame):
- Horizontal flex: "← drag" left, rail middle, state label right
- Rail: 1px `RuleBrush`
- Fill: 0% width at rest, animates to match openness; bg `AccentBrush`
- State label: "closed" / "pulling" / "partial" / "open" — `Fg4Brush` for transient, `Fg2Brush` 500 for endpoints

### 05 — LoadingView demo

**Container**: `MaxWidth=380`, vertical stack, gap 16

**Pane**:
- Background: gradient `Bg2GradientTop → Bg2GradientBot` (180°)
- 1px `RuleBrush`, `MinHeight=130`
- Padding `18,18,18,22`
- `ClipToBounds=True`

**Progress line** (always present, behind state content):
- Bottom-anchored `Rectangle`, height 2, width 0 resting
- Background `AccentBrush`
- Box-shadow: `0 0 8px rgba(255,255,255,0.4)` (glow)

**Idle state**:
- Vertical centered, `MinHeight=92`
- 18×1 `RuleBrush` rectangle mb 10
- "Awaiting source." italic, `CanvasLabelStyle`

**Running state**:
- Vertical, `MinHeight=92`, justify center
- `TimerReadoutStyle` "0.000" + inline `TimerUnitStyle` "s"
- `RunningLabelStyle` "FETCHING" mt 8

**Loaded state**:
- Vertical, `MinHeight=92`, justify center
- `LoadedHeadlineStyle` "FetchAsync resolved in" + inline `.num` "1.524s" (accent color)
- `LoadedBodyStyle` paragraph
- `LoadedMetaStyle` row: 6×6 `OkBrush` dot + " ok · 12.4 kb · gzip"

**Controls row** (below pane):
- Flex, gap 8
- "Run task" / "Running" / "Reset" `BenchButtonStyle` (text reflects state, `MinWidth=120`)
- State pill `StatePillStyle` reflecting state (idle / running / done)

---

## Tone notes

**Restraint borrowed from Naoto Fukasawa.** The bench uses a single accent (pure white) and never more than three places of accent visible at any moment. Rules are 1px and `RuleBrush` only — never thicker, never colored. Italic is used for asides and metaphor-words in headlines, never for emphasis.

**Two voices alternate:**
- **Monospace** is the bench's "voice of measurement" — used for component names, page numbers, timer readouts, meta data, eyebrows. JetBrains Mono throughout.
- **Serif (Fraunces)** is the bench's "voice of intent" — used for spec headlines, italic captions, body copy, canvas labels. Variable opsz axis tuned for each context.
- **Sans (Instrument Sans)** is functional only — chip text, button labels, drawer list items.

**Gradient direction is consistent**: vertical 180° on stationary surfaces (masthead, spec columns, demo containers), horizontal 90° only on the drawer panel and tab (because they're moving horizontally — the gradient suggests the leading edge catching light).

**Why monochromatic.** A grayscale palette forces the design to work on lightness contrast alone. If the bench reads correctly without color, it'll read correctly when desaturated for a screenshot, when projected on a low-quality display, or when the user has a vision difference. It's also calmer to look at — the eye doesn't have to triage which color means what.
