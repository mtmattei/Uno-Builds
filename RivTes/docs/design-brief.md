# Tesla × Riviera GCC — Design Brief
### Visual Design System & Component Specification
**Project Codename:** Phosphor Protocol  
**Design Language:** 1986 Buick Riviera GCC × Tesla Model 3  
**Rendering:** Teal CRT phosphor on void black  
**Primary Accent:** `#6FFCF6`  
**Orientation:** Landscape-only

---

## 1. Design Philosophy

This interface exists in an alternate timeline where Buick's 1986 CRT engineers never stopped refining their craft and were hired to design Tesla's dashboard. The result is a luxury phosphor instrument panel — not a hacker terminal, not a retro novelty. It is warm, spacious, and precise.

Every surface glows from within. Information is rendered in teal phosphor on absolute black, viewed through curved glass, framed by chrome and dark plastic, embedded in a dashboard of leather and walnut. The scan lines are faint. The vignette is gentle. The chrome catches light. The text blooms softly.

The emotional target: *sitting in a Riviera at 11PM, I-94 westbound, the CRT casting a warm glow on walnut trim.*

---

## 2. Color System — Phosphor Luminance Hierarchy

All UI is expressed through a single-hue luminance ramp. Color is not a palette — it is a brightness dial on a phosphor tube.

### 2.1 Teal Phosphor Ramp

| Token | Hex | Usage |
|-------|-----|-------|
| `Void` | `#010404` | Deepest black, outside CRT |
| `CRT` | `#020707` | CRT glass surface background |
| `Off` | `#051212` | Inactive segments, empty bars |
| `Ghost` | `#0A2222` | Borders at rest, faint grid lines |
| `Dim` | `#143838` | Secondary labels, inactive states |
| `Glow` | `#247070` | Tertiary data, map roads, muted values |
| `Bright` | `#3AABA6` | Active indicators, filled gauges, route lines |
| `Peak` | `#6FFCF6` | Primary data, hero numerals, active states |
| `Hot` | `#9EFFFA` | Momentary highlights, press feedback |
| `Bloom` | `#C8FFFC` | Glow halos, never used for text |

### 2.2 Physical Material Colors

| Token | Hex | Usage |
|-------|-----|-------|
| `Leather` | `#0B0908` | Dashboard surround base |
| `LeatherMid` | `#100D0B` | Dashboard gradient midpoint |
| `LeatherHi` | `#161210` | Dashboard highlight |
| `Walnut` | `#1A1410` | Trim strip base |
| `WalnutHi` | `#261C14` | Trim strip highlight / grain |
| `Chrome` | `#343028` | Bezel base |
| `ChromeMid` | `#484038` | Bezel midtone |
| `ChromeLit` | `#5C5448` | Bezel highlight |
| `ChromeHot` | `#706860` | Bezel peak specular |

### 2.3 Functional Accents

| Token | Hex | Usage |
|-------|-----|-------|
| `Amber` | `#D4A832` | Seat heater heat waves, low-battery segments |
| `AmberDim` | `#6A5418` | Amber at rest |
| `Red` | `#EE4040` | Taillights on the driving visualization |
| `RedDim` | `#551818` | Detected vehicle taillights |

### 2.4 Phosphor Glow Function

Every element that is "active" or "primary" gets a box shadow / text shadow following this formula:

```
glow(color, spread) = "0 0 {spread}px {color}, 0 0 {spread × 2.5}px {color}40"
```

Default spread: 4px for text, 6px for hero numerals, 3px for small indicators. The outer halo at 25% opacity simulates phosphor bloom on CRT glass.

### 2.5 Material Color Palette Override

Map the phosphor ramp into Material Design slots in `ColorPaletteOverride.xaml`:

| Material Slot | Maps To |
|---------------|---------|
| `PrimaryColor` | `Peak` (#6FFCF6) |
| `PrimaryContainerColor` | `Off` (#051212) |
| `OnPrimaryColor` | `CRT` (#020707) |
| `SecondaryColor` | `Bright` (#3AABA6) |
| `SecondaryContainerColor` | `Ghost` (#0A2222) |
| `SurfaceColor` | `CRT` (#020707) |
| `OnSurfaceColor` | `Peak` (#6FFCF6) |
| `OutlineColor` | `Ghost` (#0A2222) |
| `BackgroundColor` | `Void` (#010404) |
| `ErrorColor` | `Red` (#EE4040) |
| `TertiaryColor` | `Amber` (#D4A832) |

---

## 3. Typography

Two typefaces. Both monospaced or geometric — no serifs, no humanist forms. This is an instrument panel, not a magazine.

### 3.1 Share Tech Mono (Interface)

- **Role:** All labels, status text, button text, annotations, system info
- **Weight:** Regular (400) only
- **Sizes mapped to Material type scale:**

| Material Style | Size | Letter Spacing | Use |
|----------------|------|----------------|-----|
| `LabelSmall` | 7px | 0.14em | Ghost annotations, section headers |
| `LabelMedium` | 9px | 0.10em | Button labels, status items |
| `LabelLarge` | 10px | 0.08em | Tab labels, card titles |
| `BodySmall` | 8px | 0.06em | Track artist, sub-labels |
| `BodyMedium` | 10px | 0.06em | Data values, vehicle info |
| `BodyLarge` | 13px | 0.04em | Now-playing track title |

### 3.2 Orbitron (Numerals)

- **Role:** Speed, temperature, frequency, percentages, time — any hero numeric
- **Weights:** Regular (400) for secondary, Medium (500) for primary, Bold (700) for hero
- **Sizes:**

| Context | Size | Weight | Glow |
|---------|------|--------|------|
| Hero speed | 50–54px | 500 | 8px spread |
| Temperature set point | 18–20px | 500 | 3–4px spread |
| Radio frequency | 16px | 400 | 2–3px spread |
| Battery percentage (inline) | 11–12px | 400 | 2px spread |
| Battery percentage (charge view) | 56px | 400 | 8px spread |
| Clock | 12px | 400 | 2px spread |
| Speed limit | 15–16px | 400 | none |

---

## 4. Component Specifications

### 4.1 CRTFrame

The physical enclosure around the entire dashboard. It is always rendered; nothing exists outside it.

**Structure (outside → inside):**

1. **Leather surround** — full viewport, `linear-gradient(180deg, #080706 → LeatherMid → #080706)`, subtle SVG noise texture at 1.8% opacity
2. **Walnut trim** — 7px strips at top and bottom edges, `Walnut → WalnutHi` gradient, with faint grain lines (repeating-linear-gradient at 0.15 opacity, 40–50px intervals at slight angle)
3. **Chrome outer bezel** — 4px inset from edge, border-radius 22px, multi-stop gradient: `ChromeHot → ChromeMid → Chrome → #1A1815 → ChromeMid → ChromeHot` at 165°. Box shadow: `0 2px 20px rgba(0,0,0,0.6)`, plus inner highlight `inset 0 1px 0 ChromeHot at 27% opacity`
4. **Chrome inner bevel** — 1px inset, border-radius 19px, `Chrome 88% → #161410 → Chrome 44%` at 160°
5. **Dark plastic bezel** — 0px inset (the "frame"), border-radius 18px, `#1E1C18 → #121010 → #0A0908` at 155°. Inner shadow: `inset 0 1px 0 rgba(255,255,255,0.025), inset 0 -2px 10px rgba(0,0,0,0.7)`
6. **CRT glass** — 9px inset, border-radius 11px, `CRT` background, `overflow: hidden`, inner shadow: `inset 0 0 40px rgba(0,0,0,0.4)`

**CRT Effects (layered inside the glass, above content):**

| Layer | Z-Index | Content |
|-------|---------|---------|
| Scan lines | 10 | `repeating-linear-gradient(0deg, transparent 2px, rgba(0,0,0,0.08) 2px → 4px)`, animated upward scroll at 0.2s |
| Phosphor dot matrix | 11 | Tiled 3×3px SVG circles (0.8px radius, #6FFCF6), 0.8% opacity |
| Curvature vignette | 12 | `radial-gradient(ellipse, transparent 60%, rgba(0,0,0,0.12) 78%, rgba(0,0,0,0.38) 100%)` |
| Glass reflection | 13 | `linear-gradient(158deg, rgba(255,255,255,0.018) → transparent → rgba(255,255,255,0.006))` |

**Ambient glow** — below the CRT, on the dashboard surface: `radial-gradient(ellipse 80% 50%, Glow at 3% → transparent)`, pulsing opacity 0.5 → 0.8 over 6s ease-in-out.

**Embossed label** — below the CRT: "GRAPHIC CONTROL CENTER — RIVIERA EDITION" in Helvetica Neue, 7px, 0.45em letter-spacing, color `#24201A`.

### 4.2 GCCButton (Touch Zone)

The primary interactive element. Inspired by the real GCC's chunky capacitive touch zones.

| Property | Rest | Hover | Active/Selected | Pressed |
|----------|------|-------|-----------------|---------|
| Background | Transparent | Transparent | `Peak` at 4% | `Ghost → CRT` radial gradient |
| Border | 1.5px `Ghost` | 1.5px `Glow` | 1.5px `Bright` | 1.5px `Glow` |
| Border Radius | 7px | 7px | 7px | 7px |
| Text Color | `Dim` | `Bright` | `Peak` | `Bright` |
| Text Shadow | None | None | `glow(Peak, 2)` | None |
| Box Shadow | None | None | `0 0 12px Peak at 5%` + `inset 0 0 12px Peak at 2.5%` | None |
| Transition | — | 0.3s cubic-bezier(0.16,1,0.3,1) | 0.35s same | 0.1s |

**Sizes:**
- Standard: padding `4px 10px`, font `LabelMedium` (9px)
- Tab: padding `6px 4px`, font `LabelMedium`, flex: 1, icon + label horizontal
- Large: padding `9px 4px`, font `LabelLarge`

### 4.3 ArcGauge

SkiaSharp-rendered arc meter. Used for RPM, TEMP, VOLTS in gauges view.

- Arc sweep: 120° (from 210° to 90°)
- Background arc: 4px stroke, `Off` color
- Active arc: 5px stroke, configurable color, with 3px drop shadow
- Center value: Orbitron, 22% of control size, colored to match arc
- Label below: Share Tech Mono, 8px, `Dim`
- Tick marks: none (kept minimal for elegance)
- Animated on data change: arc sweeps to new value over 0.8s ease

### 4.4 BarGauge

Segmented horizontal bar for fuel and oil pressure.

- 12 segments, `gap: 3px` between
- Filled: configurable color with `0 0 4px color at 27%` glow
- Empty: `Off`
- Transition on change: `background 0.3s ease`
- Label (left): Share Tech Mono 9px, `Dim`, 0.12em spacing
- Value (right): Share Tech Mono 10px, configurable color

### 4.5 BatteryIndicator

Horizontal segmented bar used in the status bar and charge view.

| Context | Segments | Segment Size | Cap |
|---------|----------|-------------|-----|
| Status bar | 20 | 5×12px, radius 1px | 3×7px, radius 0 2px 2px 0 |
| Charge view | 28 | flex×28px, radius 3px | 6×14px |

Color grading per segment index:
- `< 20% of total` → `Amber`
- `20–40%` → `Glow`
- `> 40%` → `Bright`

Charging animation: the first unfilled segment blinks between `Dim` and `Off` on a 20-tick cycle (first 10 ticks = `Dim`, last 10 = `Off`).

### 4.6 SeatHeaterButton

Compound button with inline SVG seat icon.

- Seat shape: single path stroke (`M3,16 L3,8 Q3,3 8,3 Q13,3 13,8 L13,16`) + baseline (`3,16 → 13,16`)
- Stroke: `Peak` when active (heat > 0), `Dim` when off
- Heat waves: 1–3 wavy paths (`Q` curves) in `Amber`, 0.8px stroke
  - Level 1: center wave only
  - Level 2: center + right wave
  - Level 3: all three waves
- Level number displayed in `Amber`, 7px, adjacent to icon
- Border: 1.5px, `Bright` when active, `Ghost` when off
- Background: `Peak at 3%` when active, transparent when off
- Box shadow: `0 0 10px Peak at 3%` when active

### 4.7 FanSpeedIndicator

5 graduated bars, each 3px wide, heights: 4.5, 7, 9.5, 12, 15px.

- Filled (level ≤ current): `Bright`, transition `0.25s`
- Empty: `Off`
- Contained in a `GCCButton` shell with ghost border

### 4.8 ScanLineOverlay

Full-screen overlay rendered above all content:

```
repeating-linear-gradient(
    0deg,
    transparent 0px,
    transparent 2px,
    rgba(0,0,0,0.08) 2px,
    rgba(0,0,0,0.08) 4px
)
```

Animated: `background-position` scrolls from `0 0` to `0 4px` over 0.2s, linear, infinite. This creates a subtle upward scan-line drift that mimics CRT refresh.

---

## 5. View-by-View Visual Specs

### 5.1 Boot Page

- Background: leather gradient, no walnut trim visible
- CRT frame: simplified (chrome bezel + glass only), max-width 860px
- Content: left-aligned monospaced text
- Line appearance: `fadeUp` — 0px → 0px Y translate, 0 → 1 opacity, 0.25s, `cubic-bezier(0.16,1,0.3,1)`
- Active line: `Peak` with 5px text glow
- Previous lines: `Bright`, no glow
- Cursor: `█` character, `Peak`, blinking via Storyboard (opacity 1→0, 0.6s, `DiscreteObjectKeyFrame`)
- Final message: "ENTERING GCC ..." — peak, 6px glow, 0.4s fadeUp, then 600ms hold before navigation
- CRT effects: scan lines + vignette (no phosphor dots or reflection — keep it stark)

### 5.2 Status Bar

- Height: 38px
- Background: `linear-gradient(180deg, Off at 40%, transparent)` — fades into the CRT
- Border-bottom: 1px `Ghost` at 33% opacity
- Layout: flex, space-between, vertically centered, 16px horizontal padding

**Left cluster:**
- LTE signal: 4 bars (2.5px wide, heights 4/7/10/13px, border-radius 1px), first 3 `Bright`, last `Glow`
- "LTE" label: 8px, `Dim`
- Clock: Orbitron 12px, `Glow`, 2px text glow

**Center cluster:**
- Outside temp: Share Tech Mono 9px, `Dim`
- Gear selector: P R N D in Orbitron, inactive at 10px `Ghost`, active at 15px `Peak` with 5px glow, transition 0.4s
- Autopilot button: `GCCButton` variant, small (3px 10px padding, 8px text)

**Right cluster:**
- Battery bar: `BatteryIndicator` (20 segments)
- Percentage: Orbitron 11px, `Peak`, 2px glow
- Range: Share Tech Mono 7px, `Dim`

### 5.3 Left Panel — Driving Visualization

- Width: 27% of CRT glass, min 190px
- Border-right: 1px `Ghost` at 33% opacity
- Content: `DrivingVisualization` SKXamlCanvas, filling the entire panel

**Overlays (positioned absolute within the panel):**

| Overlay | Position | Content |
|---------|----------|---------|
| Speed | top: 12px, left: 16px | Orbitron 50px weight-500 `Peak` + "MPH" label Share Tech Mono 9px `Dim` |
| Speed limit | top: 12px, right: 14px | Border 1.5px `Dim` at 40%, radius 7px, "LIMIT" 6px `Ghost`, value Orbitron 15px `Glow` |

### 5.4 Tab Bar (Right Panel)

- Height: ~38px (6px top padding, 5px bottom)
- Background: `linear-gradient(180deg, Off at 27%, transparent)`
- Border-bottom: 1px `Ghost` at 33% opacity
- Layout: flex row, 5px gap, 10px horizontal padding

Each tab is a `GCCButton` variant:
- flex: 1 (equal width)
- Icon (12px) + label (8px, 0.08em spacing) horizontally centered, 5px gap
- Active state: `Peak at 5%` background, `Bright` border, `Peak` text + icon, dual glow (outer `0 0 16px Peak at 5%` + inner `inset 0 0 12px Peak at 2.5%`)
- Inactive: transparent bg, `Ghost` border, `Dim`/`Ghost` text, no shadow
- Transition: 0.35s `cubic-bezier(0.16, 1, 0.3, 1)` on all properties

### 5.5 NavView

- Full-bleed `VectorNavMap` canvas
- Turn instruction card: absolute top-left (14px, 16px), `CRT at 93%` background, 1.5px `Bright` border, radius 10px, padding 8px 14px, `backdrop-filter: blur(8px)`, shadow `0 0 20px Bright at 7%`
  - Arrow: Share Tech Mono 24px, `Peak`, 4px glow
  - Distance: Orbitron 16px, `Peak`, 3px glow
  - Road name: Share Tech Mono 9px, `Dim`
- Destination label: absolute top-right, `Ghost` 7px header, `Glow` 10px name with 2px glow

### 5.6 MediaView

- Padding: 14px 18px
- Gap between sections: 12px
1. Source buttons: 4× `GCCButton`, 5px gap, active gets 5% teal fill + `0 0 12px Peak at 5%` shadow
2. Now-playing card: `Off` background, radius 10px, 1px `Ghost` border, `inset 0 0 30px Void` shadow, 14px 16px padding
   - Album art: 52×52px, radius 8px, `Dim` border, `Ghost → Off` radial gradient center, ♫ icon at 18px `Glow` 70% opacity
   - Title: 13px `Peak`, 2px glow, ellipsis overflow
   - Artist: 8px `Dim`, 0.04em spacing
   - Progress bar: 2px height, `Off` background, `Glow → Bright` gradient fill, `0 0 6px Glow at 27%` shadow
   - Timestamps: 7px `Dim`
   - Transport: ◂◂ / ▸▸ at 13px `Glow`, play/pause in 34×34px circle border (1.5px `Bright`, radius 20px, `0 0 12px Glow at 9%` shadow)
3. Spectrum: 28 bars, 2.5px gap, max-width 12px each, border-radius `2px 2px 0 0`, height driven by data (6% min), transition `height 0.07s linear, background 0.2s`
4. Presets: 3×2 grid, 8px gap, each a `GCCButton` with Orbitron 15px frequency + 7px station name

### 5.7 EnergyView

- Padding: 14px 18px
- Gap: 12px
1. Stats: 4× cards in a row, `Off` background, 8px radius, 1px `Ghost` border, `inset 0 0 20px Void` shadow
   - Label: 7px `Ghost`, 0.12em spacing
   - Value: Orbitron 18px, `Peak`, 2px glow
   - Unit: 7px `Dim`
2. Graph: section label 7px `Dim` 0.14em, `EnergyGraph` SkiaSharp canvas filling remaining height
3. Power flow: horizontal layout, battery value ← animated chain → motor value
   - Chain: 4 bars (14×2px, radius 1px, `Bright`), opacity alternates between 0.8 and 0.15 on a 12-tick staggered cycle, 0.25s transition
   - Chevron: ▸ at 10px `Bright`

### 5.8 ChargeView

- Centered vertically, padding 20px 24px
- Gap: 18px between elements
1. Battery: 80% width max 340px, 6px padding container, 2px `Dim` border, radius 10px, `Off` background, `inset 0 0 20px Void`
   - 28 segments, flex, 2px gap, 28px height, 3px radius
   - End cap: 6×14px, radius 0 4px 4px 0, `Dim`
2. Percentage: Orbitron 56px, `Peak`, 8px glow
3. Stats: 3-column (RANGE, RATE, LIMIT), 28px gap
   - Label: 7px `Ghost` 0.14em
   - Value: Orbitron 14px, `Bright` (or `Dim` if "—")
4. Chargers: max-width 340px
   - Section label: 7px `Dim` 0.14em
   - List items: flex space-between, 8px vertical padding, 1px bottom border `Ghost at 13%`
     - Name: 9px `Bright`, distance: 7px `Dim`
     - Availability: 10px `Peak` 2px glow

### 5.9 ControlsView

- Padding: 14px 18px, gap 10px (reduced for density)
- Scrollable (VerticalScrollMode)
1. Section label: 7px `Dim` 0.18em
2. Toggles: 2-column grid, 8px gap
   - `Off` background, 1px border (`Bright` active, `Ghost` rest), radius 8px, 10px 14px padding
   - Label: 10px, `Peak` active / `Dim` rest
   - Toggle pill: 36×18px, radius 9px, `Bright` active / `Ghost` rest, knob 14×14px, `Peak` active / `Dim` rest
   - Knob shadow: `glow(Peak, 3)` active
   - Knob transition: 0.3s `cubic-bezier(0.34, 1.56, 0.64, 1)` — slight overshoot bounce
   - Active container shadow: `0 0 14px Peak at 3%`
3. Headlights: 4× `GCCButton`, flex:1, 9px padding, 7px radius
4. Openings: 2-column, `GCCButton` large variant, 14px padding, centered icon + TRUNK/FRUNK label + OPEN/CLOSED state in `Amber`/`Ghost`
5. Vehicle info: `Off` background, 8px radius, 1px `Ghost` border, `inset 0 0 20px Void`, 10px 14px padding, Share Tech Mono 10px, `Glow`, line-height 2.2, keys in `Dim`

### 5.10 Climate Bar

- Height: 40px
- Grid column span: full width
- Border-top: 1px `Ghost` at 33%
- Background: `linear-gradient(0deg, Off at 40%, transparent)`
- Layout: flex, centered, 8px gap

Elements left-to-right:
1. Left `SeatHeaterButton`
2. `FanSpeedIndicator` in `GCCButton` shell
3. Temp down `GCCButton` (◂, 13px `Glow`)
4. Temperature: Orbitron 18px, `Peak`, 4px glow, 48px min-width centered, transition 0.25s
5. Temp up `GCCButton` (▸)
6. Divider: 1×20px, `Ghost at 40%`
7. A/C `GCCButton` (active state togglable)
8. DEFR `GCCButton` (active state togglable)
9. Divider
10. Right `SeatHeaterButton`

---

## 6. Animations & Transitions

### 6.1 Global

| Animation | Property | Duration | Easing | Trigger |
|-----------|----------|----------|--------|---------|
| Dashboard entry | Opacity + TranslateY | 0.7s, 0.3s delay | cubic-bezier(0.16, 1, 0.3, 1) | Page loaded |
| Scan line scroll | BackgroundPosition Y | 0.2s | Linear, infinite | Always running |
| Ambient glow pulse | Opacity (0.5 → 0.8) | 6s | Ease-in-out, infinite | Always running |

### 6.2 Boot Sequence

| Animation | Property | Duration | Easing |
|-----------|----------|----------|--------|
| Line appear | Opacity (0→1) + TranslateY (5→0) | 0.25s | cubic-bezier(0.16, 1, 0.3, 1) |
| Cursor blink | Opacity (1→0) | 0.6s | Step-end (DiscreteObjectKeyFrame) |
| Final message | Opacity + TranslateY | 0.4s | cubic-bezier(0.16, 1, 0.3, 1) |

### 6.3 View Switching (Tab Change)

| Animation | Property | Duration | Easing |
|-----------|----------|----------|--------|
| View swap | Opacity (0→1) + Scale (0.98→1) + TranslateY (6→0) | 0.4s | cubic-bezier(0.16, 1, 0.3, 1) |

### 6.4 Interactive Elements

| Element | Animation | Duration | Easing |
|---------|-----------|----------|--------|
| GCCButton all properties | border-color, background, color, box-shadow | 0.3s | cubic-bezier(0.16, 1, 0.3, 1) |
| GCCButton (active tab) | all + inset glow | 0.35s | Same |
| Toggle knob position | Transform | 0.3s | cubic-bezier(0.34, 1.56, 0.64, 1) — bounce overshoot |
| Gear selector size/color | FontSize + Color + TextShadow | 0.4s | cubic-bezier(0.16, 1, 0.3, 1) |
| Temperature value | Color + TextShadow | 0.25s | Same |
| Fan speed bars | Background | 0.25s | ease |
| Bar gauge segments | Background | 0.3s | ease |
| Battery segments | Background | 0.4s | cubic-bezier(0.16, 1, 0.3, 1) |

### 6.5 SkiaSharp Canvas Animations

| Canvas | Animation | Rate | Technique |
|--------|-----------|------|-----------|
| Driving viz — lane scroll | Dash offset += speed/20 | 33ms (30fps) | DispatcherTimer |
| Driving viz — headlight pulse | 0.6 + sin(tick×0.06) × 0.4 | 33ms | sin() on tick |
| Driving viz — vehicle bob | sin(tick×0.018 + lane×3) × 5 | 33ms | Per-vehicle offset |
| Driving viz — autopilot rail glow | 0.3 + sin(tick×0.04) × 0.12 | 33ms | Global alpha |
| Nav map — car position pulse | radius: 5 + sin(t×0.08) × 1.8 | 50ms | Circle radius |
| Spectrum bars | current + (target - current) × 0.25 | 70ms | Lerp interpolation |
| Energy graph cursor | opacity: 0.3 + sin(t×0.1) × 0.2 | 100ms | Line opacity |
| Power flow chain | stagger: (t + i×3) % 12 < 6 | 100ms | Per-bar opacity |
| Charge blink | segment visible: t % 20 < 10 | 60ms | Modulo toggle |

---

## 7. Responsive Considerations

The CRT frame scales proportionally. At narrow widths, the chrome bezel thins, border-radius decreases, and the left panel takes a larger percentage to keep the driving viz legible.

| Breakpoint | Left Panel | Tabs | Speed Font | Chrome Bezel |
|------------|-----------|------|-----------|-------------|
| < 700px | 35% | Icon only (no label) | Orbitron 36px | 2px, radius 14px |
| 700–1100px | 27% | Icon + label | Orbitron 50px | 3px, radius 20px |
| > 1100px | 27%, max 300px | Icon + label | Orbitron 54px | 4px, radius 22px |

Use `Responsive` markup extension for adaptive values in XAML.

---

## 8. Accessibility Notes

- All interactive elements have minimum 44×44px touch targets (GCCButton padding ensures this)
- `AutomationProperties.Name` set on all buttons, toggles, and gauges
- Gear selector items have `AutomationProperties.Name="Gear {letter}"`
- Tab items have `AutomationProperties.Name="{view name} view"`
- Contrast ratio of `Peak` on `CRT` exceeds 15:1 — well above WCAG AAA
- Reduced-motion preference: disable scan line animation, ambient glow pulse, and SkiaSharp canvas animations; show static frames instead

---

## 9. Key Design Principles

1. **Black is the material.** The void is not empty — it is the CRT glass. Information glows from within it.
2. **One hue, infinite depth.** The entire luminance ramp from `Void` to `Bloom` creates more visual hierarchy than a full color palette ever could.
3. **Glow is meaning.** Brightness = importance. If something glows, it matters. If it doesn't, it's structure.
4. **Physical presence.** The chrome, leather, walnut, and glass reflection are not decoration — they establish that this is a real object in a real car. Remove them and the interface loses its soul.
5. **Space is luxury.** The original GCC had four big buttons on a 3" screen. Resist the urge to fill every pixel. Let the phosphor breathe.
6. **The easing is the emotion.** `cubic-bezier(0.16, 1, 0.3, 1)` — fast in, gentle settle. Every transition should feel like pressing a button on warm glass.
