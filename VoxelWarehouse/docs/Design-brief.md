# GRIDFORM — Design Brief
## Visual Language & Interaction Specification

*Extracted from working prototype · Reference for Uno Platform production build*

---

## 1. Design Philosophy

**Aesthetic:** Monochromatic instrument. GRIDFORM looks like a piece of precision measurement equipment — a thermal camera, a sonar display, a surgical navigation screen. The interface recedes so the spatial data comes forward.

**Influences:** Mission control dashboards, oscilloscope UIs, architectural section drawings, topographic survey equipment, aerial LiDAR visualizations.

**Key principles:**
- **Information density over decoration** — every visible element communicates state
- **Luminance as the sole visual variable** — no hue; meaning is encoded through brightness, opacity, and spatial position
- **Glass-panel HUD** — controls float over the workspace as translucent overlays, never solid panels
- **Camera-first** — the live video feed is the ground plane; all UI composites on top of reality

---

## 2. Color System

The entire palette is grayscale with precisely controlled alpha channels. No hex colors, no hues — only white at varying opacities against a near-black base.

### 2.1 Base & Background

| Token | Value | Usage |
|-------|-------|-------|
| `Surface.Base` | `#050507` | App background (near-black with blue-black undertone) |
| `Camera.Filter` | `grayscale(0.9) brightness(0.18) contrast(1.15)` | Live camera feed treatment |
| `Overlay.Scanline` | `repeating-linear-gradient` at 25% opacity, 1.5px/3px | CRT scanline texture |
| `Overlay.Vignette` | `radial-gradient(ellipse at 50% 36%, transparent 25%, rgba(3,3,5,0.8) 100%)` | Edge darkening, draws focus to center |

### 2.2 Foreground Opacity Scale

All foreground elements are white (`255,255,255`) at these opacity tiers:

| Tier | Alpha | Usage |
|------|-------|-------|
| `Fg.Primary` | `0.85` | Active text, selected controls, wordmark |
| `Fg.Secondary` | `0.70` | Metric values, active state data |
| `Fg.Tertiary` | `0.50` | Hand tracking status (active), medium-priority labels |
| `Fg.Muted` | `0.35` | Metric labels, inactive body text |
| `Fg.Ghost` | `0.28` | Inactive button text |
| `Fg.Dim` | `0.20` | Preset buttons, zone labels at rest |
| `Fg.Whisper` | `0.15` | Subtitle text, hand status (inactive) |
| `Fg.Trace` | `0.12` | Cursor info, help text, corner marks |
| `Fg.Faint` | `0.08` | Borders, dividers, edge highlights |
| `Fg.Invisible` | `0.02–0.04` | Frame border, ground grid, zone fill |

### 2.3 Surface/Panel Backgrounds

| Token | Value | Usage |
|-------|-------|-------|
| `Panel.Glass` | `rgba(0,0,0,0.35–0.45)` + `backdrop-filter: blur(10–12px)` | All HUD panels |
| `Panel.Border` | `rgba(255,255,255,0.04)` | Panel edge (barely visible) |
| `Btn.Active` | `rgba(255,255,255,0.10)` | Selected button background |
| `Btn.Rest` | `transparent` | Unselected button |
| `Divider` | `rgba(255,255,255,0.06)` | Vertical separators in toolbars |

### 2.4 Isometric Voxel Colors (Per Asset Type)

Each asset uses a unique near-gray tint for the three isometric faces. Values are base brightness levels (0–255) before AO and fog multipliers:

| Asset | Top Face | Left Face | Right Face | Accent (RGB) | Character |
|-------|----------|-----------|------------|---------------|-----------|
| **Pallet** | 180 | 135 | 95 | `180, 175, 160` | Warm gray — woody |
| **Rack** | 160 | 120 | 85 | `140, 155, 170` | Cool gray — steel |
| **Container** | 155 | 115 | 80 | `170, 160, 150` | Neutral — concrete |
| **Equipment** | 140 | 105 | 72 | `165, 150, 140` | Dark — industrial |
| **Aisle** | 85 | 65 | 48 | `85, 90, 95` | Very dark — floor |

The final rendered color for each face is: `(baseFaceValue × AO × fog + accent × weight) / 2`, creating a subtle chromatic variation within the monochrome constraint.

### 2.5 Zone Colors

Zones use barely-visible tinted fills on isometric ground tiles:

| Zone | Fill | Border |
|------|------|--------|
| **Receiving** | `rgba(200,210,220, 0.06)` | `rgba(180,200,220, 0.15)` |
| **Storage** | `rgba(220,215,200, 0.06)` | `rgba(220,210,180, 0.15)` |
| **Staging** | `rgba(210,200,220, 0.06)` | `rgba(200,180,220, 0.15)` |
| **Shipping** | `rgba(200,220,210, 0.06)` | `rgba(180,220,200, 0.15)` |

These are the only chromatic values in the entire interface — and they're nearly invisible. Zone differentiation relies on spatial position more than color.

---

## 3. Typography

**Single typeface. One family. Four weights.**

| Property | Value |
|----------|-------|
| **Family** | `JetBrains Mono` (fallback: `SF Mono`, `Cascadia Code`, `monospace`) |
| **Weights** | 300 (Light), 400 (Regular), 500 (Medium), 600 (SemiBold) |
| **Transform** | All UI text is `uppercase` |
| **Rendering** | Subpixel antialiasing, no smoothing overrides |

### Type Scale (from prototype)

| Role | Size | Weight | Tracking | Usage |
|------|------|--------|----------|-------|
| **Wordmark** | 11px | 600 | `0.22em` | "GRIDFORM" title |
| **Stat Value** | 8–9px | 400 | `0.08em` | Metric numbers, data readouts |
| **Label** | 8px | 300 | `0.08–0.15em` | Stat labels, status indicators |
| **Button** | 8px | 300 (rest) / 500 (active) | `0.12–0.15em` | All toolbar buttons |
| **Subtitle** | 8px | 300 | `0.12em` | "SPATIAL PLANNER" sub-label |
| **Section Header** | 7px | 500 | `0.20em` | "UTILIZATION", "ASSETS", "ZONES" |
| **Help Text** | 7px | 300 | `0.08em` | Keyboard hints, cursor position |
| **Asset Label** | 6px | bold | — | Single-char stamp on voxel top face ("P", "R", "C", "E") |
| **Layer Label** | 7px | 300 | `0.18em` | "HEIGHT" label under layer control |

**Critical design rule:** No font size in the prototype exceeds 11px. The entire HUD is intentionally miniaturized — precision instrument aesthetic, not reading material.

---

## 4. Layout & Spatial Composition

### 4.1 Screen Zones

```
┌──────────────────────────────────────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓▓▓ TITLE BAR (h:44px) ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │
├───────┬──────────────────────────────────────┬───────┤
│       │                                      │       │
│ PANEL │                                      │ LAYER │
│ 170px │        ISOMETRIC CANVAS              │ CTRL  │
│       │        (full bleed)                  │ 26px  │
│       │                                      │       │
│       │                                      │       │
├───────┤          origin at 50%, 35%          │       │
│ CRSR  │                                      │       │
│ INFO  │                                      │       │
├───────┴──────────────────────────────────────┴───────┤
│              TOOLBAR (bottom-center)                  │
│              ┌─ mode ─┬─ tools ─┬─ assets/zones ─┐   │
│              └────────┴─────────┴────────────────┘   │
└──────────────────────────────────────────────────────┘
```

### 4.2 Key Positions

| Element | Anchor | Offset |
|---------|--------|--------|
| **Title bar** | Top, full width | `top: 0`, height 44px |
| **Metrics panel** | Top-left | `top: 56px, left: 12px`, width 170px |
| **Layer control** | Right, vertically centered | `right: 16px, top: 50%` (transform center) |
| **Toolbar** | Bottom-center | `bottom: 12px`, centered horizontal |
| **Cursor info** | Bottom-left | `bottom: 80px, left: 12px` |
| **Help text** | Below cursor info | (part of cursor info block) |
| **Frame border** | Inset all edges | `inset: 8px, top: 44px` |
| **Corner marks** | Four corners | 4–6px from edges, 12×12px L-shapes |

### 4.3 Isometric Canvas Positioning

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| **Origin X** | `viewport width / 2` | Horizontally centered |
| **Origin Y** | `viewport height × 0.35` | Upper third — leaves room for toolbar below, sky above |
| **Grid size** | 14 × 14 cells | Large enough for warehouse layouts, small enough to fit viewport |
| **Tile half-width** | 22px | Balances detail vs. viewport coverage |
| **Tile half-height** | 11px | Standard 2:1 isometric ratio |
| **Voxel depth** | 18px | Visible side face height |
| **Max layers** | 6 | Typical warehouse stacking height |

---

## 5. Component Specifications

### 5.1 Title Bar

```
Height:             44px
Background:         rgba(0,0,0, 0.35) + blur(12px)
Border bottom:      1px solid rgba(255,255,255, 0.04)
Padding:            0 20px
Layout:             flex, space-between, align-center
Z-index:            10 (above canvas)

Left section:       Wordmark + subtitle, baseline-aligned, gap 10px
Center section:     Hand tracking status dot + label
Right section:      Quick stats (FLOOR, VOL, WT, LY) gap 16px
```

**Hand status dot:**
- Tracking active: `5×5px circle, #fff, box-shadow: 0 0 6px rgba(255,255,255,0.2)`
- Inactive: `5×5px circle, rgba(255,255,255, 0.1), no shadow`
- Transition: `all 0.2s`

### 5.2 Metrics Panel

```
Position:           absolute, top: 56px, left: 12px
Width:              170px
Background:         rgba(0,0,0, 0.35) + blur(10px)
Border:             1px solid rgba(255,255,255, 0.04)
Border radius:      3px
Padding:            14px
Font size:          8px
Line height:        2.2
Letter spacing:     0.08em
```

**Section dividers:**
- Font size 7px, weight 500, tracking 0.20em
- Color: `rgba(255,255,255, 0.25)`
- Bottom border: `1px solid rgba(255,255,255, 0.04)`
- Margin: 12px top, 6px bottom, 6px padding-bottom

**Progress bars:**
- Track: height 3px, radius 1px, `rgba(255,255,255, 0.04)`
- Fill: `rgba(255,255,255, 0.18–0.25)`, width animated (`transition: width 0.3s`)

### 5.3 Toolbar (Bottom)

Two rows, stacked vertically with 4px gap, centered.

**Row 1 — Mode + Tools:**
```
Background:         rgba(0,0,0, 0.40) + blur(12px)
Border radius:      3px
Border:             1px solid rgba(255,255,255, 0.04)
Padding:            3px
Button gap:         2px
```

**Row 2 — Asset/Zone selector:**
```
Background:         rgba(0,0,0, 0.35) + blur(10px)
Same border/radius specs as Row 1
```

**Button states:**
| State | Background | Color | Weight |
|-------|-----------|-------|--------|
| Rest | `transparent` | `rgba(255,255,255, 0.28)` | 300 |
| Active | `rgba(255,255,255, 0.10)` | `rgba(255,255,255, 0.85)` | 500 |
| Preset (rest) | `transparent` | `rgba(255,255,255, 0.18)` | 300 |
| Preset (hover) | `transparent` | `rgba(255,255,255, 0.50)` | 300 |
| Transition | `all 0.15s` | | |

**Button dimensions:**
- Padding: `6px 12px`
- Font: 8px, tracking `0.12em`
- Border: none, radius 2px
- Key hint prefix: 7px, opacity 0.30, right margin 5px

**Dividers between groups:**
- Width 1px, height 16px
- Color: `rgba(255,255,255, 0.06)`
- Margin: `auto 4–6px`

### 5.4 Layer Control (Right Edge)

Vertically centered stack:

```
▲ button:       26×26px, radius 2px
Layer bars:     26×5px each, gap 2px, radius 1px
▼ button:       26×26px, radius 2px
"HEIGHT" label: 7px, 300 weight, 0.18em tracking, below with 3px margin
```

**Up/Down buttons:**
- Border: `1px solid rgba(255,255,255, 0.06)`
- Background: `rgba(0,0,0, 0.3)` + `blur(6px)`
- Text: `rgba(255,255,255, 0.3)`, 10px
- Hover: border → `0.15`, color → `0.6`

**Layer bars:**
| State | Background | Border |
|-------|-----------|--------|
| Active layer | `rgba(255,255,255, 0.55)` | `1px solid rgba(255,255,255, 0.2)` |
| Has voxels | `rgba(255,255,255, 0.12)` | transparent |
| Empty | `rgba(255,255,255, 0.02)` | transparent |
| Transition | `all 0.15s` | |

### 5.5 Corner Marks

Four L-shaped registration marks at viewport corners:
- Size: 12×12px
- Border: 1px, `rgba(255,255,255, 0.08)`
- Only two edges per corner (forming the L)
- Offset: 4px from edge (bottom corners), 48px from top (top corners, below title bar)

### 5.6 Frame Border

- Inset: 8px from all edges, 44px from top
- Border: `1px solid rgba(255,255,255, 0.02)`
- Radius: 1px

---

## 6. Isometric Rendering Specification

### 6.1 Voxel Geometry

Each voxel is drawn as three parallelogram faces (top, left, right):

```
              (sx, sy)          ← apex
              ╱      ╲
    (sx-HW, sy+HH)    (sx+HW, sy+HH)
              ╲      ╱
         (sx, sy+2·HH)         ← bottom of top face
              │
              │  DEPTH (18px)
              │
    (sx-HW, sy+HH+DEPTH)   (sx+HW, sy+HH+DEPTH)
         (sx, sy+2·HH+DEPTH)

    HW = 22px (half-width)
    HH = 11px (half-height)
    DEPTH = 18px (side face height)
```

### 6.2 Lighting Model

Three-face directional lighting with no explicit light source — brightness decreases from top → left → right:

| Face | Base Range | Direction |
|------|-----------|-----------|
| **Top** | 140–200 | Receives "overhead" light |
| **Left** | 105–150 | Receives partial side light |
| **Right** | 48–108 | In shadow |

Final face color: `base × AO_factor × depth_fog × (1 + accent_blend)`

### 6.3 Ambient Occlusion

Per-face check against neighboring voxels. Occlusion accumulates:

**Top face AO:**
- Direct above neighbor: instant 0.35 (darkens top to ~65%)
- Four diagonal-above neighbors: +0.12 each
- Two horizontal neighbors: +0.06 each
- Clamped to `max(0, 1 - total_occlusion)`

**Left face AO:**
- Direct left neighbor: instant 0.30
- Above/below-left: +0.15 each
- Front neighbor: +0.08
- Below: +0.06

**Right face AO:** Mirror of left, checking right/front directions.

### 6.4 Edge Highlights

Applied to exposed (no neighbor) faces only:

| Edge | Width | Alpha | Position |
|------|-------|-------|----------|
| **Top ridgeline** (two strokes: apex→left, apex→right) | 1.1px | `0.20 × fog` | Top face peak edges |
| **Left vertical edge** | 0.7px | `0.08 × fog` | Left face outer edge |
| **Right vertical edge** | 0.7px | `0.05 × fog` | Right face outer edge |
| **Contact shadow** (bottom crease) | 1.0px | `0.14 × fog` (black) | Bottom edge of both side faces |

### 6.5 Depth Fog

Atmospheric fade based on isometric depth (`gx + gz`):

```
fog_factor = 1 - ((gx + gz) / ((GRID-1) × 2)) × 0.40
```

Range: 1.0 (front-left corner) → 0.60 (back-right corner). Applied as a multiplier to all color values and edge alpha.

### 6.6 Ground Shadows

For each voxel at `y > 0`, project a shadow diamond onto the ground plane (layer 0):
- Shape: same isometric diamond as top face
- Color: `rgba(0,0,0, 0.09 × fog)`
- No blur (sharp shadow)

### 6.7 Grid

Isometric grid lines at `rgba(255,255,255, 0.04)`, width 0.5px. Active layer grid (when layer > 0) renders at 45% of that opacity.

### 6.8 Ghost Cursor

Current asset type preview at cursor position:
- Global alpha: `0.20`
- Top face: accent at 50% alpha
- Left face: accent at 30% alpha
- Right face: accent at 20% alpha
- Crown highlight: accent at 30% alpha, 0.8px stroke on top ridgeline

### 6.9 Stacking Preview

When active layer > 0, shows vertical context at cursor column:

**Vertical guide lines:**
- Three dashed lines (left edge, right edge, front edge) from ground to active layer
- Color: `rgba(255,255,255, 0.06)`, width 0.5px, dash pattern: `[2, 4]`

**Empty-layer wireframes:**
- Dashed diamond outline at each empty layer below cursor
- Color: `rgba(255,255,255, 0.18)`, width 0.5px, dash: `[3, 5]`
- Alpha ramps from `0.025` (ground) to `0.065` (near active layer)

### 6.10 Crosshair

Dashed vertical and horizontal lines through cursor position:
- Color: `rgba(255,255,255, 0.05)`, width 0.5px
- Dash: `[6, 8]`
- Full viewport extent

---

## 7. Gesture Tracking Overlay

### 7.1 Hand Reticle (camera mode)

When hand is detected, draw targeting circle at hand centroid:

**Tracking state:**
- Outer circle: radius 15px, `rgba(255,255,255, 0.2)`, width 1px
- Inner crosshair: ±6px lines, width 0.5px

**Pinch state:**
- Outer circle: `rgba(255,255,255, 0.6)`, width 1.5px
- Second ring: radius 20px (only during pinch), `rgba(255,255,255, 0.4)`, width 1px
- Transition: immediate (gesture frames are latency-critical)

### 7.2 Hand Skeleton Overlay (ML mode)

When 21-landmark detection is active, draw bone connections + joint dots on the canvas:

**Bone lines:**
- Color: `rgba(255,255,255, 0.30)`
- Width: 1.5px
- Five finger chains + one palm chain connecting MCP joints

**Joint dots:**
- Radius: 3px
- Color: `rgba(255,255,255, 0.70)`

**Active gesture highlights:**
- Pinch: 6px filled white dots on thumb tip + index tip
- Point: 4px dot on index tip only

---

## 8. Interaction Design

### 8.1 Cursor Behavior

| Input | Response | Feedback |
|-------|----------|----------|
| **Mouse move** | Inverse-isometric projection → grid coordinates → ghost cursor update | Ghost voxel moves, crosshair follows |
| **Left click** | Place current asset at cursor cell on active layer | Voxel appears instantly, metrics update |
| **Right click** | Erase voxel at cursor cell on active layer | Voxel removed instantly |
| **Scroll wheel** | Layer ±1 | Layer bar highlight shifts, grid plane moves |
| **Hand move** (camera) | Centroid mapped to screen → inverse iso → cursor | Same ghost cursor as mouse |
| **Pinch** (camera) | Place at cursor | Same as left click |
| **Fist** (camera) | Erase at cursor | Same as right click |

### 8.2 Keyboard Shortcuts

| Key | Action | Feedback |
|-----|--------|----------|
| `1` | Place tool | Button highlight changes |
| `2` | Erase tool | Button highlight changes |
| `B` | Build mode | Mode button highlight |
| `Z` | Zone mode | Mode button highlight |
| `M` | Toggle metrics panel | Panel appears/disappears |
| `Q` | Cycle asset type | Asset button highlight changes |
| `W` / `↑` | Layer up | Layer bar shift |
| `S` / `↓` | Layer down | Layer bar shift |
| `C` | Toggle camera | Camera feed appears/disappears |

### 8.3 Transitions & Timing

| Element | Transition | Duration |
|---------|-----------|----------|
| All buttons | `all` | 150ms |
| Layer bars | `all` | 150ms |
| Hand status dot | `all` | 200ms |
| Progress bar fills | `width` | 300ms |
| Hover states (buttons, layer controls) | color, border-color | 150ms |

**Explicit design choice:** No easing curves specified. All transitions use linear or browser-default — the instrument aesthetic avoids "bouncy" or "spring" animations.

---

## 9. Responsive Behavior

### 9.1 Canvas Scaling

The isometric grid origin is always relative (`50%, 35%`), so it scales with viewport. Tile dimensions are fixed pixel values — larger viewports show more negative space around the grid, not larger tiles.

### 9.2 Panel Collapse Rules

| Viewport Width | Metrics Panel | Toolbar | Layer Control |
|---------------|---------------|---------|---------------|
| ≥ 900px | Visible (toggleable) | Two rows | Full height |
| 600–899px | Hidden by default | Single row, truncated presets | Compact |
| < 600px | Hidden | Minimal (mode + tool only) | Collapsed to ▲/▼ only |

### 9.3 Touch Adaptation

On touch devices (no hover):
- Button padding increases to `8px 16px` for 44px minimum touch target
- Hand reticle not shown (touch IS the input)
- Long-press = erase (replaces right-click)

---

## 10. Camera Composite Stack (Z-Order)

From back to front:

```
Z0  Camera video feed (mirrored, desaturated, darkened)
Z1  Scanline overlay (25% opacity, 1.5px/3px repeating gradient)
Z2  Vignette overlay (radial gradient, 80% edge opacity)
Z3  SkiaSharp canvas (grid, shadows, voxels, cursor, hand overlay)
Z4  Frame border + corner marks
Z5  Metrics panel
Z6  Layer control
Z7  Cursor info + help text
Z8  Toolbar
Z9  Title bar (z-index: 10)
```

---

## 11. Asset Label System

Single-character stamps rendered on exposed top faces of non-aisle voxels:

| Asset | Label | Font |
|-------|-------|------|
| Pallet | **P** | Bold 6px JetBrains Mono |
| Rack | **R** | Bold 6px JetBrains Mono |
| Container | **C** | Bold 6px JetBrains Mono |
| Equipment | **E** | Bold 6px JetBrains Mono |
| Aisle | *(none)* | — |

Color: `rgba(0,0,0, 0.35 × fog)` — dark text on the lighter top face. Centered on the isometric diamond at `(sx, sy + HH + 1)`.

---

## 12. Design Tokens Summary (for Uno Resource Dictionary)

```
Surface.Base:                #050507
Surface.Glass:               rgba(0,0,0, 0.35) + Acrylic blur 12px
Surface.GlassLight:          rgba(0,0,0, 0.30) + Acrylic blur 6px
Border.Panel:                rgba(255,255,255, 0.04)
Border.Control:              rgba(255,255,255, 0.06)
Border.ControlHover:         rgba(255,255,255, 0.15)
Border.CornerMark:           rgba(255,255,255, 0.08)
Border.Frame:                rgba(255,255,255, 0.02)
Divider.Toolbar:             rgba(255,255,255, 0.06)

Text.Primary:                rgba(255,255,255, 0.85)
Text.Value:                  rgba(255,255,255, 0.70)
Text.Label:                  rgba(255,255,255, 0.35)
Text.Inactive:               rgba(255,255,255, 0.28)
Text.Dim:                    rgba(255,255,255, 0.20)
Text.Hint:                   rgba(255,255,255, 0.15)
Text.Ghost:                  rgba(255,255,255, 0.12)

Button.ActiveBg:             rgba(255,255,255, 0.10)
Button.RestBg:               transparent
Button.ActiveFg:             rgba(255,255,255, 0.85)
Button.RestFg:               rgba(255,255,255, 0.28)
Button.HoverFg:              rgba(255,255,255, 0.50)

Status.Active:               #FFFFFF + glow(0 0 6px rgba(255,255,255,0.2))
Status.Inactive:             rgba(255,255,255, 0.10)

Progress.Track:              rgba(255,255,255, 0.04)
Progress.FloorFill:          rgba(255,255,255, 0.25)
Progress.VolumeFill:         rgba(255,255,255, 0.18)

Canvas.Grid:                 rgba(255,255,255, 0.04)
Canvas.Crosshair:            rgba(255,255,255, 0.05)
Canvas.Shadow:               rgba(0,0,0, 0.09)
Canvas.ContactShadow:        rgba(0,0,0, 0.14)
Canvas.EdgeHighlight.Top:    rgba(255,255,255, 0.20)
Canvas.EdgeHighlight.Left:   rgba(255,255,255, 0.08)
Canvas.EdgeHighlight.Right:  rgba(255,255,255, 0.05)
Canvas.Outline:              rgba(10,10,14, 0.35)
Canvas.StackGuide:           rgba(255,255,255, 0.06)
Canvas.StackWire:            rgba(255,255,255, 0.18)

Hand.Reticle.Track:          rgba(255,255,255, 0.20)
Hand.Reticle.Pinch:          rgba(255,255,255, 0.60)
Hand.Reticle.Ring:           rgba(255,255,255, 0.40)
Hand.Skeleton.Bone:          rgba(255,255,255, 0.30)
Hand.Skeleton.Joint:         rgba(255,255,255, 0.70)

Type.Family:                 JetBrains Mono
Type.Wordmark:               11px / 600 / 0.22em
Type.Value:                  8-9px / 400 / 0.08em
Type.Label:                  8px / 300 / 0.08-0.15em
Type.Button:                 8px / 300|500 / 0.12em
Type.Section:                7px / 500 / 0.20em
Type.Help:                   7px / 300 / 0.08em
Type.VoxelStamp:             6px / bold
```
