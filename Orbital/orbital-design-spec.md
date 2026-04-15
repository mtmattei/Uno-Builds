# Orbital — Visual Design Specification

> Companion document to `orbital-architecture-brief.md`. This spec contains every pixel-level measurement, color value, opacity, gradient, interaction state, and spatial relationship extracted from the working prototype. An implementer should be able to reproduce the UI from this document alone without referencing the React source.

---

## 1. Design Language Summary

**Aesthetic:** Terminal × Apple — the precision of a CLI dashboard married to the restraint and polish of Apple's design language. Dark, deep, and quiet — the interface recedes so the data stands forward.

**Key principles:**
- Monochromatic charcoal base with a single emerald accent (no purple, no blue gradients)
- Mono typography for ALL data — version numbers, timestamps, labels, console output, badges
- Sans typography only for titles, body text, and button labels
- Surfaces are barely visible against the background — the hierarchy comes from text weight and opacity, not container contrast
- Animation is always purposeful: pulsing = alive/connected, shimmer = loading, fade-up = entrance, breathing = selected/active

---

## 2. Color System (Exact Values)

### Surface Palette (4 tiers of dark)

| Token | HSL | Hex | Usage |
|-------|-----|-----|-------|
| Surface-0 (Background) | hsl(220, 16%, 6%) | #0F1117 | App background, page background |
| Surface-0.5 (Sidebar) | hsl(220, 16%, 5.5%) | #0E1015 | Sidebar background |
| Surface-1 (Card) | hsl(220, 14%, 8.5%) | #141821 | Card/surface background, all `Surface` components |
| Surface-1.5 (Console) | hsl(220, 16%, 5%) | #0D0F14 | Console body, artifact rows, inset containers |
| Surface-2 (Hover/Active) | hsl(220, 14%, 11%) | #1A1E28 | Hover states on project rows, icon boxes |
| Surface-3 (Muted) | hsl(220, 14%, 14%) | #212633 | Active tab bg, dividers, border color |
| Surface-3.5 (Strong border) | hsl(220, 14%, 16%) | #262B38 | Hover borders, gradient divider peak |
| Surface-4 (Scrollbar) | hsl(220, 14%, 20%) | #2E3445 | Scrollbar thumb |

### Text Palette (7 levels of opacity via lightness)

| Token | HSL | Hex | Opacity equiv. | Usage |
|-------|-----|-----|-----------------|-------|
| Text-100 | hsl(220, 10%, 92%) | #E8EAF0 | 100% | Hero greeting, primary values (28px, 18px) |
| Text-90 | hsl(220, 10%, 90%) | #E3E5EC | ~95% | Page titles (20px) |
| Text-88 | hsl(220, 10%, 88%) | #DEE0E8 | ~90% | Card values, session titles, nav labels |
| Text-85 | hsl(220, 10%, 85%) | #D6D9E1 | ~85% | Timeline titles, sub-values |
| Text-82 | hsl(220, 10%, 82%) | #CED1DA | ~80% | Session card names, project names |
| Text-80 | hsl(220, 10%, 80%) | #C9CCD6 | ~78% | Button secondary text |
| Text-75 | hsl(220, 10%, 75%) | #BCC0CB | ~70% | Feature names, connector names, artifact filenames |
| Text-72 | hsl(220, 10%, 72%) | #B5B9C5 | ~65% | Dependency package names |
| Text-65 | hsl(220, 10%, 65%) | #A3A8B6 | ~55% | Console info lines, acceptance check text |
| Text-55 | hsl(220, 10%, 55%) | #8A90A0 | ~45% | Ghost button text |
| Text-50 | hsl(220, 10%, 50%) | #7B8191 | ~40% | MCP status label, icon color |
| Text-45 | hsl(220, 10%, 45%) | #6E7485 | ~35% | Idle nav items, system nominal label |
| Text-42 | hsl(220, 10%, 42%) | #676D7E | ~32% | Subtitles, session goal text, date line |
| Text-40 | hsl(220, 10%, 40%) | #626878 | ~30% | Page subtitle, session metadata |
| Text-38 | hsl(220, 10%, 38%) | #5D6372 | ~28% | Section headers (MONO UPPERCASE) |
| Text-35 | hsl(220, 10%, 35%) | #565C6B | ~25% | Mono metadata, path text, timestamps, console dim |
| Text-32 | hsl(220, 10%, 32%) | #505664 | ~22% | Section sub-labels, permission labels |
| Text-30 | hsl(220, 10%, 30%) | #4A505D | ~20% | Clock digits, console line numbers, project time |
| Text-25 | hsl(220, 10%, 25%) | #3F4452 | ~15% | Chevron icons, console line numbers |

### Accent Colors

| Name | Hex | Usage |
|------|-----|-------|
| Emerald-400 | #34D399 | Primary accent: buttons, active nav, status ok, badges, check icons |
| Emerald-500 | #10B981 | Primary button bg, logo gradient start |
| Emerald-600 | #059669 | Logo gradient end |
| Teal-600 | #0D9488 | Greeting orb gradient end |
| Violet-400 | #A78BFA | Agent/session accent: session icon, pulsing bars, runtime tools |
| Violet-500/20 | #8B5CF6 at 20% | Agent icon box background |
| Violet-600/10 | #7C3AED at 10% | Agent icon box gradient end |
| Amber-400 | #FBBF24 | Warning status, uno-check warning badge, Hot Reload indicator |
| Red-400 | #F87171 | Error status, denied permissions |
| Red-500 | #EF4444 | Error badge, console error lines |
| Blue-400 | #60A5FA | .NET SDK pulsing bars, Screenshot tool bars |
| Zinc-500 | #71717A | Idle status dot |

### Accent Opacity Variants (for backgrounds/borders)

| Pattern | Example |
|---------|---------|
| `emerald-500/15` | Badge bg, feature check box bg |
| `emerald-500/20` | Badge border, nav active badge bg |
| `emerald-500/10` | Active nav item bg |
| `emerald-500/5` | Active platform target bg |
| `emerald-500/25` | Greeting orb shadow |
| `emerald-400/20` | Timeline ok dot inner |
| `amber-500/15` | Warning badge bg |
| `amber-500/20` | Warning badge border |
| `red-500/15` | Error badge bg, denied permission bg |
| `red-400/20` | Timeline error dot inner |
| `violet-500/20` | Agent session icon bg |
| `violet-500/30` | Selected session card border |
| `zinc-500/10` | Muted badge bg, disabled feature icon bg |
| `zinc-500/15` | Muted badge border |

---

## 3. Typography System

### Font Stack

| Role | Family | Weights used | Fallback |
|------|--------|-------------|----------|
| UI (sans) | DM Sans (variable) | 300, 400, 500, 600, 700 | system-ui, sans-serif |
| Data (mono) | JetBrains Mono (variable) | 300, 400, 500, 600 | SF Mono, monospace |

### Type Scale (every size used in the prototype)

| Size | Weight | Family | Tracking | Usage |
|------|--------|--------|----------|-------|
| 32px | Light (300) | Mono | Tight (-0.025em) | Hero clock display |
| 28px | Bold (700) | Sans | Tight (-0.025em) | Greeting headline ("Good morning, Matt") |
| 20px | SemiBold (600) | Sans | Tight (-0.025em) | Page titles ("Diagnostics") |
| 18px | SemiBold (600) | Sans | Tight (-0.025em) | Card primary values ("6.0.176", "Online") |
| 16px | SemiBold (600) | Sans | Normal | Session detail title |
| 15px | SemiBold (600) | Sans | Normal | Project meta values, studio license title |
| 14px | Medium (500) | Sans | Normal | Session name, active session title, date line |
| 14px | Regular (400) | Sans | Normal | Subtitle text |
| 13px | Medium (500) | Sans | Normal | Nav labels, project names, button labels, connector names, timeline titles |
| 13px | Medium (500) | Mono | Normal | Check text, feature names, platform labels |
| 12px | Regular (400) | Sans | Normal | Session goal, acceptance check text |
| 12px | Medium (500) | Sans | Normal | Tab labels, console title |
| 12px | Regular (400) | Mono | Normal | Console body text, artifact filename, dependency name |
| 11px | Medium (500) | Mono | Wide (0.08em) | Section headers — ALWAYS uppercase ("QUICK ACTIONS") |
| 11px | Regular (400) | Mono | Normal | Badge text, MCP status label, metadata lines, path text |
| 10px | Medium (500) | Mono | Wider (0.12em) | Section sub-labels — ALWAYS uppercase |
| 10px | Regular (400) | Mono | Normal | Version pill values, feature descriptions, artifact metadata, timestamps |
| 10px | Regular (400) | Mono | Normal | Sidebar version label, MCP tool count |
| 9px | Regular (400) | Mono | Wide (0.08em) | Version pill labels — ALWAYS uppercase |

### Text Formatting Rules

- **Section headers** are ALWAYS: mono, 11px, medium weight, uppercase, wide tracking (0.08em), Text-38 color
- **Section sub-labels** are ALWAYS: mono, 10px, medium weight, uppercase, wider tracking (0.12em), Text-32 color
- **Mono metadata** (paths, timestamps, tool counts): mono, 11px, regular, normal tracking, Text-35 color
- **String concatenation** in metadata uses ` · ` as separator (space-middot-space)
- **Never use bold (700)** except for the greeting headline and the sidebar logo letter
- **Tabular numbers** (tabular-nums) on clock displays and any numeric data

---

## 4. Spacing & Layout System

### Global Spacing Scale

| Token | Value | CSS class | Usage |
|-------|-------|-----------|-------|
| 0.5 | 2px | gap-0.5 | Nav item vertical spacing |
| 1 | 4px | gap-1, py-1 | Tab button gaps, divider padding |
| 1.5 | 6px | gap-1.5 | Version pill inner gap, status label gap |
| 2 | 8px | gap-2, space-y-2 | Button gaps, session card spacing, connector spacing, badge wrap gap |
| 3 | 12px | gap-3 | Timeline horizontal gap, dependency item gap, version strip gap, section header mb |
| 4 | 16px | gap-4 | Card grid gap, project row gap, session icon gap, section header mb |
| 5 | 20px | p-5 | Card internal padding (ALL surfaces use p-5 = 20px) |
| 6 | 24px | gap-6, space-y-6 | Section vertical spacing, agent grid gap |
| 8 | 32px | p-8 | Page content padding (ALL pages use p-8 = 32px) |

### Key Spatial Rules

- **Page padding:** 32px all sides (`p-8`)
- **Card padding:** 20px all sides (`p-5`) — every `Surface` component
- **Card gap in grids:** 16px (`gap-4`) — all card grids
- **Section vertical spacing:** 24px (`space-y-6`) — between sections on every page
- **Header bar:** px-32, py-20 (px-8 py-5), bottom border
- **Sidebar width:** 220px fixed
- **Console max-height:** 280px with scroll
- **Console line-height:** 1.7 (for mono text)

### Grid Proportions

| Screen | Layout | Details |
|--------|--------|---------|
| Home status cards | 4 equal columns | `grid-cols-4 gap-4` |
| Home actions + session | 1fr + 2fr (1/3 + 2/3) | `grid-cols-3`, actions=`col-span-1`, session=`col-span-2` |
| Project meta strip | 5 equal columns | `grid-cols-5 gap-4` |
| Agents layout | 4/12 + 8/12 (sidebar + detail) | `grid-cols-12 gap-6`, list=`col-span-4`, detail=`col-span-8` |
| Studio features | 3 equal columns | `grid-cols-3 gap-3` |
| Diagnostics deps | 2 equal columns | `grid-cols-2 gap-3` |
| Diagnostics tools | 3 equal columns | `grid-cols-3 gap-3` |

---

## 5. Component Visual Specs

### 5.1 Surface (Card Container)

Every card in the app uses this exact specification:

| Property | Value |
|----------|-------|
| Background | Surface-1: hsl(220, 14%, 8.5%) / #141821 |
| Border | 1px solid Surface-3: hsl(220, 14%, 13%) / #1E2230 |
| Border radius | 12px (rounded-xl) |
| Padding | 20px all sides |
| Hover (if clickable) | Border → hsl(220, 14%, 18%), Bg → hsl(220, 14%, 9.5%) |
| Transition | all 200ms ease |
| Entrance animation | fade-up: 0→1 opacity, 8px→0 translateY, 350ms ease-out, staggered by `delay` |
| Breathing variant | border-color oscillates emerald-500 12%→35% opacity, 3s sine infinite |

### 5.2 StatusDot

| Property | Value |
|----------|-------|
| Size (sm) | 8×8px (w-2 h-2) |
| Size (md) | 10×10px (w-2.5 h-2.5) |
| Shape | Circle (rounded-full) |
| Colors | ok: emerald-400, warn: amber-400, error: red-400, idle: zinc-500 |
| Pulse (ok only) | Scale 1→0.8, opacity 1→0.6, box-shadow 0→8px 2px currentColor, 2s sine infinite |
| Ring pulse (ok only) | Pseudo-element, inset: -3px, 1.5px border currentColor, scale 0.85→1.15, opacity 0.6→0, 2.5s ease-out infinite |

### 5.3 Badge

| Property | Value |
|----------|-------|
| Padding | 8px horizontal, 2px vertical (px-2 py-0.5) |
| Font | Mono, 11px, medium weight |
| Border | 1px solid |
| Border radius | 6px (rounded-md) |
| Variants | success: emerald-500 15%bg/20%border/400 text, warn: amber-500 15%bg/20%border/400 text, error: red-500 15%bg/20%border/400 text, muted: zinc-500 10%bg/15%border/400 text |

### 5.4 Button (Btn)

Three variants, two sizes:

**Size sm:** px-12, py-6 (px-3 py-1.5), 13px text
**Size md:** px-16, py-8 (px-4 py-2), 13px text

| Variant | Background | Text | Border | Shadow | Hover |
|---------|-----------|------|--------|--------|-------|
| Primary | emerald-500 | Surface-0 (#0F1117) | none | 0 0 24px emerald-500/20 | emerald-400 |
| Secondary | Surface-2 hsl(220,14%,12%) | Text-80 | 1px Surface-3.5 hsl(220,14%,16%) | none | bg → hsl(220,14%,16%) |
| Ghost | transparent | Text-55 | none | none | Text-80, bg → Surface-2 |

**All buttons:** rounded-lg (8px), font-medium, gap-8 (for icon), transition 150ms, active scale(0.97)

**Icon placement:** 14×14px icon left of label, 8px gap

### 5.5 PulsingBars

| Property | Value |
|----------|-------|
| Container | flex row, align-end, 3px gap, 16px height |
| Bar count | 3–5 (varies by context) |
| Bar width | 3px |
| Bar shape | rounded-full (pill) |
| Colors | emerald-400, amber-400, violet-400, blue-400 (per context) |
| Opacity | 0.6 base |
| Animation | scaleX 0.3→1, opacity 0.3→1, 1.2–1.9s sine infinite (each bar offset by 0.15s + random initial height 40–100%) |

**Placement rules:**
- Home status cards: 4 bars, color matches card theme (emerald/blue/violet/amber)
- Active session header: 3 bars, violet
- Console title bar: 4 bars, emerald
- Sidebar MCP status: 3 bars, emerald
- Connectors (connected): 3 bars, emerald
- Diagnostics tools: 3 bars, color per tool

### 5.6 ConsoleBlock

| Property | Value |
|----------|-------|
| Outer bg | Surface-1.5: hsl(220, 16%, 5%) |
| Outer border | 1px solid hsl(220, 14%, 11%) |
| Outer radius | 8px (rounded-lg) |
| Title bar height | ~36px (py-2 px-4) |
| Title bar border-bottom | 1px solid hsl(220, 14%, 11%) |
| Traffic dots | 3 circles, 10×10px (w-2.5 h-2.5), colors: red-500/70, amber-500/70, emerald-500/70, 8px gap |
| Title text | Mono, 11px, Text-35, 8px left of dots |
| PulsingBars | right-aligned in title bar, 4 bars |
| Body padding | 16px all sides |
| Body font | Mono, 12px, line-height 1.7 |
| Body max-height | 280px, overflow-y auto |
| Line numbers | Mono, Text-25 (hsl 220,10%,25%), 24px wide, right-aligned, no-select |
| Line gap | 12px between number and content |
| Scanline overlay | Pseudo-element, full-size, 25% height gradient (transparent→emerald 3% opacity→transparent), translateY -100%→400%, 4s linear infinite |

**Line type colors:**
- info: Text-65
- success: emerald-400
- error: red-400
- warn: amber-400
- dim: Text-35

### 5.7 TimelineItem

| Property | Value |
|----------|-------|
| Layout | Horizontal flex, 12px gap |
| Dot | 12×12px circle, 2px border, inner fill at 20% opacity |
| Dot colors | ok: emerald-400 border, emerald-400/20 fill. warn/error/idle: same pattern with respective colors |
| Connector line | 1px wide, Surface-3 (hsl 220,14%,14%), min-height 32px, connects dots vertically |
| Title | Sans, 13px, medium, Text-85 |
| Time badge | Mono, 11px, Text-35, 12px gap right of title |
| Detail | Sans, 12px, regular, Text-45, 4px margin-top |
| Item bottom padding | 20px (pb-5) |
| Last item | No connector line |

### 5.8 VersionPill

| Property | Value |
|----------|-------|
| Layout | Horizontal flex, 6px gap (gap-1.5) |
| Padding | 10px horizontal, 4px vertical (px-2.5 py-1) |
| Background | Surface-1: hsl(220, 14%, 8%) |
| Border | 1px solid hsl(220, 14%, 12%) |
| Radius | 6px (rounded-md) |
| Label | Mono, 9px, uppercase, wide tracking, Text-32 |
| Value | Mono, 10px, regular, Text-55 |

### 5.9 DataStream

| Property | Value |
|----------|-------|
| Width | 160px |
| Font | Mono, 9px |
| Color | emerald-500 at 30% opacity |
| Content | 24 random hex pairs separated by spaces, regenerated every 2 seconds |
| Animation | data-flicker: opacity varies between 0.3–1.0 in irregular steps, 3s linear infinite |
| Overflow | hidden, no-wrap |

### 5.10 NavItem (Sidebar)

| Property | Value |
|----------|-------|
| Padding | 12px horizontal, 10px vertical (px-3 py-2.5) |
| Radius | 8px (rounded-lg) |
| Font | Sans, 13px, medium |
| Icon size | 18×18px |
| Icon-label gap | 12px |
| Idle state | Text-45, no background |
| Hover state | Text-75, bg Surface-2.5 hsl(220,14%,10%) |
| Active state | Text emerald-400, bg emerald-500/10 |
| Active indicator | Left edge, 3px wide, 20px tall, emerald-400, rounded-right, vertically centered |
| Badge (if present) | Mono, 10px, emerald-500/15 bg, emerald-400 text, px-6 py-2 (px-1.5 py-0.5), right-aligned |

---

## 6. Screen-by-Screen Layout Specs

### 6.1 Home Page

**Greeting Hero Region** (top, no header bar — this screen uses a custom hero instead)

```
├─ px-32 pt-32 pb-8
├─ Row (justify-between, align-start)
│   ├─ Left: fade-up
│   │   ├─ Row (gap-16, align-center, mb-12)
│   │   │   ├─ Avatar Orb
│   │   │   │   ├─ 48×48px, rounded-2xl (16px radius)
│   │   │   │   ├─ Gradient: emerald-400 → emerald-500 → teal-600 (top-left to bottom-right)
│   │   │   │   ├─ Shadow: 0 0 24px emerald-500/25
│   │   │   │   ├─ Animation: orb-float (translateY 0→-6→3→0, scale 1→1.02→0.98→1, 5s sine)
│   │   │   │   ├─ Animation: gradient-shift (bg-position 0%→100%→0%, 6s ease, bg-size 200%)
│   │   │   │   ├─ Letter inside: "M", bold, lg (18px), Surface-0 color
│   │   │   │   └─ Online dot: absolute bottom-right, 14×14px circle, emerald-400, 2px border Surface-0, pulse-dot animation
│   │   │   └─ Text stack
│   │   │       ├─ Greeting: "Good morning, Matt" — Sans, 28px, bold, tight tracking, Text-92
│   │   │       └─ Date: "Wednesday, March 4, 2026" — Sans, 14px, regular, Text-42, mt-2
│   │   │
│   ├─ Right: fade-up (80ms delay)
│   │   ├─ Clock: "2:34 PM" — Mono, 32px, light (300), Text-30, tabular-nums, tight tracking, leading-none
│   │   └─ System status pill (mt-4)
│   │       ├─ Row (gap-6)
│   │       ├─ 6×6px emerald-400 dot with pulse-dot
│   │       └─ "All systems nominal" — Mono, 10px, Text-45
│   │       └─ Pill container: px-10 py-4, rounded-md, Surface-1 bg, Surface-3 border
│
├─ Version strip (mt-16, fade-up 140ms delay)
│   ├─ Row (gap-12)
│   ├─ 5 × VersionPill: [Orbital v0.1.0-alpha] [Uno SDK 6.0.176] [.NET 9.0.200] [LLM Claude Sonnet 4] [MCP 3 servers]
│   └─ Right-aligned: DataStream (160px wide)
│
├─ Gradient divider (px-32 py-4)
│   └─ 1px height, gradient: transparent → Surface-3.5 → transparent (left to right)
```

**Status Cards Row** (below divider)
```
├─ 4-column grid, gap-16
├─ Each card is a Surface with breathing border
│   ├─ Top row (justify-between, mb-12)
│   │   ├─ Icon box: 36×36px, rounded-lg, Surface-2 bg, icon 16×16 Text-50
│   │   └─ Right cluster: PulsingBars (4 bars, themed color) + StatusDot (md)
│   ├─ Label: Mono, 11px, uppercase, tracking-wider, Text-38, mb-4
│   ├─ Value: Sans, 18px, semibold, tight tracking, Text-88
│   └─ Sub: Mono, 11px, Text-35, mt-4
├─ Entrance stagger: 200ms + (index × 70ms)
```

**Actions + Session Row**
```
├─ 3-column grid, gap-16
├─ Quick Actions (col-span-1, Surface, entrance 500ms)
│   ├─ Section header: "QUICK ACTIONS"
│   ├─ 4 × Button (full-width, justify-start, size sm)
│   │   ├─ "Run Uno Check" — secondary
│   │   ├─ "New Project" — secondary
│   │   ├─ "Build + Run" — primary
│   │   └─ "Open Docs" — ghost
│   └─ Button spacing: 8px vertical gap
│
├─ Active Session (col-span-2, Surface, entrance 560ms)
│   ├─ Has scanline-overlay (the CRT sweep)
│   ├─ Header row (justify-between, mb-16)
│   │   ├─ Left: Section header "ACTIVE SESSION" + PulsingBars (3, violet)
│   │   └─ Right: Badge "Running" (success variant)
│   ├─ Session info row (gap-16, mb-16)
│   │   ├─ Icon: 40×40px, rounded-lg, gradient violet-500/20 → violet-600/10, border violet-500/20
│   │   │   └─ Bot icon 18px, violet-400
│   │   └─ Text stack
│   │       ├─ Name: Sans, 14px, medium, Text-85
│   │       └─ Meta: Mono, 12px, Text-40 ("orbital · main · 12 actions · 3 artifacts")
│   └─ 3 × TimelineItem (ok status, last one has isLast)
```

**Recent Projects**
```
├─ Surface (entrance 650ms)
├─ Header row: section header "RECENT PROJECTS" + ghost button "Browse All" with search icon
├─ List (4px vertical gap)
│   ├─ Each row: full-width button
│   │   ├─ Padding: 16px horizontal, 12px vertical
│   │   ├─ Radius: 8px
│   │   ├─ Hover: bg Surface-2 hsl(220,14%,11%)
│   │   ├─ Layout: Row (gap-16)
│   │   │   ├─ StatusDot
│   │   │   ├─ Text stack (flex-1, left-aligned)
│   │   │   │   ├─ Name: Sans, 13px, medium, Text-80, hover→Text-92
│   │   │   │   └─ Path: Mono, 11px, Text-35
│   │   │   ├─ Branch badge (muted variant)
│   │   │   ├─ Time: Mono, 11px, Text-30
│   │   │   └─ Chevron: 14px, Text-25, hover→Text-45
```

### 6.2 Project Page

**Uses generic Header bar** — title "Orbital", subtitle "~/dev/orbital · net9.0-desktop, net9.0-browserwasm · main"

**Meta Strip**
```
├─ 5-column grid, gap-16
├─ Each card: compact Surface (same spec, stagger 50ms each)
│   ├─ Top row (justify-between, mb-4)
│   │   ├─ Label: Mono, 10px, uppercase, wider tracking, Text-32
│   │   └─ PulsingBars (only on "Hot Reload" card, 3 bars, amber)
│   ├─ Value: Sans, 15px, semibold, Text-85
│   └─ Sub: Mono, 11px, Text-35, mt-2
```

**Task Bar**
```
├─ Row (gap-8)
├─ 5 × Button (size sm)
│   ├─ Build — secondary
│   ├─ Run — PRIMARY (only primary button in the bar)
│   ├─ Package — secondary
│   ├─ Hot Reload — secondary
│   └─ UI Smoke Test — secondary
├─ Right-aligned: ghost button "Build → Run → Verify" with zap icon
```

**Tabbed Console**
```
├─ Surface (entrance 200ms)
├─ Tab row (gap-4, mb-16)
│   ├─ Each tab: px-12 py-6, rounded-md, 12px medium
│   ├─ Active: Surface-3 bg, Text-85
│   └─ Inactive: Text-40, hover→Text-60
├─ Content: ConsoleBlock or artifacts list
│
├─ Artifacts list items:
│   ├─ Row (gap-16), px-16 py-12, rounded-lg, Surface-1.5 bg, Surface-2 border
│   ├─ Icon: image icon, 16px, Text-40
│   ├─ Text stack (flex-1)
│   │   ├─ Filename: Mono, 12px, Text-75
│   │   └─ Meta: Mono, 10px, Text-35 ("Screenshot · 142 KB")
│   ├─ Time: Mono, 10px, Text-30
│   └─ Copy button: ghost, size sm, copy icon
```

### 6.3 Agents Page

**Split Layout: 4/12 list + 8/12 detail, gap-24**

**Session List (left panel)**
```
├─ Header: Section header "SESSIONS" + primary button "New" (size sm, zap icon)
├─ List (gap-8)
├─ Each card: full-width button
│   ├─ Padding: 16px
│   ├─ Radius: 12px (rounded-xl)
│   ├─ Default: Surface-1 bg, Surface-3 border
│   ├─ Selected: Surface-2.5 bg, emerald-500/30 border, border-breathe animation
│   ├─ Hover (unselected): border → Surface-3.5
│   ├─ Status row (gap-8, mb-8): StatusDot + name (13px medium Text-82) + PulsingBars (if active)
│   └─ Meta: Mono, 11px, Text-35 ("orbital · 12 actions · 34m")
```

**Session Detail (right panel)**
```
├─ Surface: Session info
│   ├─ Header (justify-between, mb-16)
│   │   ├─ Title: Sans, 16px, semibold, Text-88
│   │   ├─ Goal: Sans, 12px, Text-42, mt-4
│   │   └─ Right: status Badge + secondary "Replay" button with refresh icon
│   │
│   ├─ Tool Permissions (mb-20)
│   │   ├─ Section sub-label "TOOL PERMISSIONS"
│   │   ├─ Flex wrap (gap-8)
│   │   ├─ Granted: Badge success variant (green) — "file:read", "file:write", "build:run", "screenshot:capture", "mcp:uno-docs"
│   │   └─ Denied: Badge error variant (red) — "deploy:prod", "git:push"
│   │
│   ├─ Action Timeline
│   │   ├─ Section sub-label "ACTION TIMELINE"
│   │   └─ N × TimelineItem
│
├─ Surface: Acceptance Checks
│   ├─ Section sub-label "ACCEPTANCE CHECKS"
│   ├─ List (gap-8)
│   ├─ Each check row: px-12 py-8, rounded-lg, Surface-1.5 bg
│   │   ├─ Check icon box: 20×20px, rounded (4px), emerald-500/15 bg, check icon 12px emerald-400
│   │   └─ Text: Sans, 12px, Text-65
```

### 6.4 Studio Page

**License Card** (Surface with breathing border)
```
├─ Row (justify-between)
├─ Left: Row (gap-16)
│   ├─ Shield icon box: 48×48px, rounded-xl, gradient emerald-500/20 → emerald-600/10, border emerald-500/20, animate-gradient
│   │   └─ Shield icon 22px, emerald-400
│   └─ Text stack
│       ├─ Title: Sans, 15px, semibold, Text-88
│       └─ Account: Mono, 12px, Text-42
├─ Right: Row (gap-8)
│   ├─ Badge "Active" (success)
│   ├─ PulsingBars (3, emerald)
│   └─ Secondary "Refresh" button with refresh icon
```

**Features Grid** (3 columns, gap-12)
```
├─ Each feature: Row (gap-12), px-16 py-12, rounded-lg, border
│   ├─ Enabled: Surface-1 bg, Surface-3 border
│   ├─ Disabled: Surface-0.5 bg, Surface-2 border, opacity 50%
│   ├─ Icon box: 32×32px, rounded-md
│   │   ├─ Enabled: emerald-500/15 bg, check icon 14px emerald-400
│   │   └─ Disabled: zinc-500/10 bg, x icon 14px zinc-500
│   └─ Text stack
│       ├─ Name: Sans, 12px, medium, Text-75
│       └─ Desc: Sans, 10px, Text-35
```

**Connectors List**
```
├─ List (gap-8)
├─ Each connector: Row (gap-16), px-16 py-12, rounded-lg, Surface-1.5 bg, Surface-2 border
│   ├─ StatusDot (ok or idle)
│   ├─ Text stack (flex-1)
│   │   ├─ Name: Sans, 13px, medium, Text-75
│   │   └─ Detail: Mono, 11px, Text-35
│   ├─ PulsingBars (if connected, 3 bars, emerald)
│   └─ Button: ghost "Configure" (if connected) or secondary "Connect" (if disconnected)
```

### 6.5 Diagnostics Page

**Uno Check section** — Surface with ConsoleBlock inside, header has PulsingBars (4, amber) + Badge "1 warning" (warn) + secondary "Re-run" button

**Dependencies grid** — 2 columns, gap-12, each dep row same as connector pattern but with "Update" secondary button if outdated

**Runtime Verification** — 3 columns, gap-12
```
├─ Each tool: button, column flex, gap-12, p-16, rounded-xl
│   ├─ Default: Surface-1 bg, Surface-3 border
│   ├─ Hover: border → Surface-3.5, bg → Surface-2.5
│   ├─ Top row (justify-between, full-width)
│   │   ├─ Icon box: 36×36px, rounded-lg, Surface-2 bg, icon 16px emerald-400
│   │   └─ PulsingBars (3, themed color)
│   └─ Text stack
│       ├─ Name: Sans, 13px, medium, Text-78
│       └─ Desc: Sans, 11px, Text-38
```

**Platform Targets strip** — horizontal row, gap-8
```
├─ Each pill: Row (gap-8), px-16 py-10, rounded-lg, border
│   ├─ Active: emerald-500/5 bg, emerald-500/15 border, text emerald-400
│   └─ Inactive: Surface-1 bg, Surface-3 border, text Text-40
│   ├─ StatusDot (ok or idle)
│   └─ Name: Sans, 12px, medium
```

---

## 7. Interaction States

### Hover

| Element | Change |
|---------|--------|
| Surface (clickable) | Border lightens to Surface-3.5, bg shifts to Surface-1.5 (+1% lightness) |
| Project row | Background → Surface-2, name text → Text-92, chevron → Text-45 |
| Nav item (inactive) | Text → Text-75, bg → Surface-2.5 |
| Button primary | emerald-500 → emerald-400 (lighter) |
| Button secondary | bg → Surface-3.5 |
| Button ghost | Text → Text-80, bg → Surface-2 |
| Tab (inactive) | Text → Text-60 |
| Runtime tool card | Border → Surface-3.5, bg → Surface-2.5 |

### Active / Pressed

| Element | Change |
|---------|--------|
| All buttons | scale(0.97), 150ms transition |

### Selected

| Element | Change |
|---------|--------|
| Nav item | bg emerald-500/10, text emerald-400, left indicator bar appears |
| Session card | bg Surface-2.5, border emerald-500/30, border-breathe animation starts |
| Tab | bg Surface-3, text Text-85 |

### Focus

Not explicitly designed in the prototype — implement with a 2px emerald-400 focus ring (`outline: 2px solid emerald-400, outline-offset: 2px`) for accessibility.

---

## 8. Animation Choreography

### Page Entrance Sequence

When a page renders, elements fade-up in this order with staggered delays:

**Home page:**
1. 0ms — Greeting hero (left)
2. 80ms — Clock + status pill (right)
3. 140ms — Version strip
4. 200ms — Status card 1
5. 270ms — Status card 2
6. 340ms — Status card 3
7. 410ms — Status card 4
8. 500ms — Quick actions panel
9. 560ms — Active session panel
10. 650ms — Recent projects

**Other pages:** Header appears instantly. Content stagger starts at 0ms, each section adds 100ms.

### Persistent Animations (run continuously)

| Animation | Where | Notes |
|-----------|-------|-------|
| pulse-dot | Every StatusDot with status="ok" | 2s cycle |
| ring-pulse | Every StatusDot with status="ok" | 2.5s cycle |
| border-breathe | Breathing Surface cards, selected session | 3s cycle |
| bar-pulse | Every PulsingBars instance | 1.2–1.9s per bar, staggered |
| orb-float | Greeting avatar only | 5s cycle |
| gradient-shift | Greeting avatar, studio license icon | 6s cycle |
| glow | Sidebar logo only | 3s cycle |
| data-flicker | DataStream hex text | 3s cycle |
| scanline | Console blocks, active session card | 4s cycle |
| blink-caret | Not yet used but available | 1s step |

---

## 9. Sidebar Anatomy

```
┌─────────────────────────┐
│ px-20 py-20              │ ← Logo region
│ ┌──┐                     │
│ │O │ Orbital              │ 32×32 logo, 8px radius, emerald gradient, glow animation
│ └──┘ v0.1.0-alpha         │ Mono 10px, Text-35
│                           │
│ px-12                     │ ← Nav region
│ NAVIGATE                  │ Mono 10px uppercase, Text-30, px-12 mb-8
│ ┃ Home              [1]  │ Active: emerald indicator bar, emerald-400 text
│   Project            3   │ Badge with count
│   Agent Sessions     1   │
│   Studio                 │
│   Diagnostics            │
│                           │
│                           │ ← Flex spacer
│                           │
│ px-12                     │ ← Footer
│ 🕐 2:34:56 PM            │ Clock icon 12px + Mono 11px Text-40, tabular-nums
│ ┌─────────────────────┐  │ MCP card: px-12 py-12, Surface-1 bg, Surface-3 border
│ │ ● MCP Connected |||  │ │ StatusDot md + Mono 11px Text-50 + PulsingBars 3
│ │ 3 servers · 24 tools │ │ Mono 10px Text-30
│ └─────────────────────┘  │
└─────────────────────────┘
Width: 220px fixed
Background: Surface-0.5 hsl(220, 16%, 5.5%)
Right border: 1px solid hsl(220, 14%, 11%)
```

---

## 10. Header Bar Anatomy (used on Project, Agents, Studio, Diagnostics)

```
┌──────────────────────────────────────────────────────────────────────┐
│ px-32 py-20                                                         │
│                                                                      │
│ Page Title                              ┌─────────────────────────┐ │
│ Subtitle text                           │ 🔍 Search or run...  ⌘K │ │
│                                         └─────────────────────────┘ │
│ ─────────────────────────────────────────────────────────────────── │ ← 1px border-b
│                                                                      │
│ Title: Sans 20px semibold tight Text-90                              │
│ Subtitle: Sans 13px regular Text-40, mt-2                            │
│                                                                      │
│ Search bar: Surface-1 bg, Surface-3 border, rounded-lg (8px)        │
│   Icon: search 14px Text-35                                          │
│   Placeholder: Mono 12px Text-30                                     │
│   Kbd: Mono 10px, Surface-2 bg, Text-35, px-6 py-2, rounded (4px)   │
│   Total padding: px-12 py-8                                          │
└──────────────────────────────────────────────────────────────────────┘
Border bottom: 1px solid hsl(220, 14%, 10%)
```

---

## 11. Responsive Considerations

The prototype is desktop-optimized at ~1280px+ content width. For Uno implementation:

| Window width | Behavior |
|-------------|----------|
| ≥1280px (Wide/Widest) | Full layout as specified. Sidebar always open. |
| 900–1279px (Normal) | Agents layout collapses to stacked (list above detail). Status cards go 2×2. Project meta goes 3+2. |
| <900px (Narrow) | Sidebar collapses to icons-only (48px wide). All grids go single-column. Console fills width. |

Use the Uno Toolkit `Responsive` markup extension for these breakpoints:
```xml
Spacing="{utu:Responsive Normal=16, Wide=16, Narrow=8}"
```

---

## 12. Implementer Checklist

Before marking a screen "done," verify:

- [ ] All text uses the correct font family (Sans vs Mono) — check every element
- [ ] All text sizes match the spec (no 16px where 13px is specified)
- [ ] All colors reference theme resources, not hardcoded hex
- [ ] Section headers are mono, 11px, uppercase, wide tracking, Text-38
- [ ] Cards use 20px padding, 12px radius, Surface-1 bg, Surface-3 border
- [ ] Page content has 32px padding
- [ ] Grid gaps are 16px (cards) or 12px (compact items)
- [ ] StatusDots pulse when status is "ok"
- [ ] PulsingBars are present on active/connected elements with correct color
- [ ] Entrance animations fire with correct stagger delays
- [ ] Breathing borders are on active/selected surfaces
- [ ] Console has traffic-light dots, line numbers, scanline overlay
- [ ] Hover states lighten borders and backgrounds
- [ ] Buttons have active scale(0.97)
- [ ] No visible hard edges — all containers have border-radius
- [ ] Scrollbar is 6px wide, Surface-4 color, 3px radius
