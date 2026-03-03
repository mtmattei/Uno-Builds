# OLEA — Olive Oil Tasting Journal

## Design Brief

UI Specification & Visual Design System · Version 1.0 — February 2026

---

## Table of Contents

1. Design Philosophy & Aesthetic Direction
2. Color System
3. Typography System
4. Spacing & Layout System
5. Border, Shadow & Surface System
6. Animation & Transition System
7. Component Catalog
8. Screen: Navigation Bar
9. Screen: New Tasting (Tab 1)
10. Screen: Flavor Wheel (detailed)
11. Screen: My Journal (Tab 2)
12. Screen: Explore Regions (Tab 3)
13. Responsive Breakpoints
14. Accessibility Notes
15. Seed / Demo Data

---

## 1. Design Philosophy & Aesthetic Direction

### AESTHETIC

Luxury editorial. The app should feel like a premium food magazine — warm, refined, tactile. Think aged parchment, olive groves, Mediterranean sunlight. The tone is sophisticated but approachable, never sterile or corporate.

### KEY PRINCIPLES

- **Warmth over coolness:** Cream and earth tones dominate. No blue, no grey-blue, no cool neutrals.
- **Serif for display, sans-serif for UI:** Decorative serif headings contrast with clean sans-serif body text.
- **Generous whitespace:** Content breathes. Panels float on the cream background with clear separation.
- **Subtle texture:** A faint grain overlay across the entire viewport adds tactile depth (3% opacity noise SVG filter).
- **Organic color accents:** Flavor categories use nature-derived colors (olive greens, amber, terracotta, lavender).

### WHAT TO AVOID

- Generic tech aesthetics: no blue gradients, no neon accents, no dark mode
- Flat or sterile surfaces: every panel should have border + shadow + subtle background difference
- Over-animation: motion is purposeful (entrance reveals, hover feedback) not decorative

---

## 2. Color System

### CORE PALETTE

| Token Name | Hex | Usage |
|---|---|---|
| gold | `#C4A44A` | Primary accent. Star ratings, active indicators, decorative lines |
| gold-light | `#D4BC6A` | Hover borders on tabs |
| gold-muted | `#B89B3E` | Region area subtext |
| olive-dark | `#2D3B2D` | Primary brand color. Logo, headings, active tab fill, save button |
| olive | `#4A5D3A` | Card gradient start, nav-link active |
| olive-light | `#6B7F5A` | Subtle accents |
| cream (bg) | `#FAF6EE` | App background, input backgrounds |
| cream-dark | `#F0E8D8` | Intensity track backgrounds, cultivar tag backgrounds |
| warm-white | `#FDFCF9` | Card/panel surfaces, nav background |
| charcoal (text) | `#1A1A18` | Primary body text |
| brown | `#5C4A2A` | Tab hover text |
| brown-light | `#8B7355` | Form labels, date text, intensity labels |
| text-muted | `#6B6560` | Subtitle text, placeholder text, nav links inactive |
| border | `#E5DDD0` | All borders: cards, inputs, dividers, inactive stars |

### SEMANTIC / INTENSITY COLORS

| Token | Hex | Usage |
|---|---|---|
| fruity-green | `#6B9B4A` | Fruity intensity fill bar |
| bitter-amber | `#C49B3A` | Bitter intensity fill bar |
| pepper-red | `#B85A4A` | Pungent intensity fill bar |

### FLAVOR WHEEL CATEGORY COLORS

| Category | Color | Child Color Range |
|---|---|---|
| Fruity | `#6B9B4A` | `#5E8B3E` – `#8CB56A` (4 children) |
| Floral | `#9B7BB5` | `#8E6BA8` – `#B69AC8` (3 children) |
| Herbal | `#5A8B6A` | `#4A7058` – `#6A9B7A` (4 children) |
| Nutty | `#A67B5B` | `#9A6B4B` – `#B89878` (3 children) |
| Peppery | `#B85A4A` | `#A04A3A` – `#D4756A` (3 children) |
| Bitter | `#C49B3A` | `#A47B2A` – `#D4AB4A` (3 children) |
| Buttery | `#D4A85A` | `#C89B4E` – `#DEAF66` (2 children) |
| Woody | `#7A6B5A` | `#6A5B4A` – `#9A8B78` (3 children) |

### SHADOW TOKENS

| Token | Value | Usage |
|---|---|---|
| shadow | `rgba(44, 40, 30, 0.08)` | Default card/panel elevation |
| shadow-lg | `rgba(44, 40, 30, 0.12)` | Hover state elevated shadow |

---

## 3. Typography System

### FONT STACK

| Role | Font Family | Fallback |
|---|---|---|
| Display / Page Titles | Cormorant Garamond | Georgia, serif |
| Section Headings / Logo | Playfair Display | Georgia, serif |
| Body / UI / Labels | DM Sans | Helvetica Neue, sans-serif |

### TYPE SCALE (ALL SIZES)

| Element | Font | Size | Weight | Spacing | Transform | Color |
|---|---|---|---|---|---|---|
| Page title (h1) | Cormorant Garamond | 48px | 300 (Light) | 1px letter | None | olive-dark |
| Page subtitle | DM Sans | 15px | 300 | 0.5px letter | None | text-muted |
| Panel heading (h2) | Playfair Display | 22px | 400 | Normal | None | olive-dark |
| Panel sub-heading (h3) | Playfair Display | 18px | 400 | Normal | None | olive-dark |
| Logo | Playfair Display | 28px | 700 | 2px letter | UPPERCASE | olive-dark |
| Logo period | Playfair Display | 28px | 700 | 0 | italic, none | gold |
| Nav links | DM Sans | 13px | 500 | 1.5px letter | UPPERCASE | text-muted / olive-dark |
| Tab buttons | DM Sans | 13px | 500 | 0.5px letter | None | text-muted / cream |
| Form labels | DM Sans | 11px | 600 | 1.5px letter | UPPERCASE | brown-light |
| Input text | DM Sans | 14px | 400 | Normal | None | charcoal |
| Placeholder text | DM Sans | 14px | 400 | Normal | None | text-muted |
| Save button | DM Sans | 14px | 600 | 1px letter | UPPERCASE | cream |
| Star rating icons | — (SVG) | 32px | — | — | — | border / gold |
| Wheel category label | DM Sans | 10px | 600 | 0.8px letter | UPPERCASE | white |
| Wheel sub-label | DM Sans | 8.5px | 400 | Normal | None | white @ 85% |
| Wheel center text | Cormorant Garamond | 13-14px | 300/600 | Normal | None | olive-dark |
| Flavor tag | DM Sans | 12px | 500 | Normal | None | white |
| Intensity label | DM Sans | 12px | 500 | 1px letter | UPPERCASE | brown-light |
| Intensity value | DM Sans | 13px | 600 | Normal | None | olive-dark |
| Card date | DM Sans | 11px | 500 | 1px letter | UPPERCASE | brown-light |
| Card name | Playfair Display | 20px | 400 | Normal | None | olive-dark |
| Card origin | DM Sans | 13px | 300 | Normal | None | text-muted |
| Card flavor tag | DM Sans | 10px | 600 | 0.5px letter | UPPERCASE | white |
| Card bar label | DM Sans | 9px | 600 | 1px letter | UPPERCASE | brown-light |
| Region name | Playfair Display | 20px | 400 | Normal | None | olive-dark |
| Region area | DM Sans | 13px | 500 | Normal | None | gold-muted |
| Region desc | DM Sans | 13px | 300 | Normal | 1.7 line-height | text-muted |
| Cultivar tag | DM Sans | 10px | 600 | 0.5px letter | UPPERCASE | olive |
| Toast message | DM Sans | 14px | 500 | Normal | None | cream |
| Empty state title | Cormorant Garamond | 28px | 300 | Normal | None | olive-dark |
| Empty state body | DM Sans | 14px | 300 | Normal | None | text-muted |

---

## 4. Spacing & Layout System

### GLOBAL LAYOUT

- **Max content width:** 1400px, centered horizontally
- **Page padding:** 40px top/bottom, 48px left/right
- **Page header bottom margin:** 56px
- **Tab bar bottom margin:** 48px
- **Tab bar gap between tabs:** 8px

### PANEL / CARD SPACING

| Element | Padding | Border Radius | Gap Between |
|---|---|---|---|
| Form panel | 36px all sides | 16px | 40px (grid gap to wheel panel) |
| Wheel panel | 36px all sides | 16px | — |
| Journal card | 24px content area | 16px | 24px (grid gap) |
| Region card | 28px all sides | 16px | 24px (inside 40px grid gap) |

### FORM ELEMENT SPACING

- **Form group margin-bottom:** 20px
- **Label to input gap:** 8px (label margin-bottom)
- **Input padding:** 12px vertical, 16px horizontal
- **Form row (2-col) gap:** 16px
- **Textarea min-height:** 80px

### GRID LAYOUTS

- **New Tasting:** 2-column grid, 1fr 1fr, 40px gap, align-items: start
- **Journal:** auto-fill grid, minmax(340px, 1fr), 24px gap
- **Explore:** 2-column grid, 1fr 1fr, 40px gap

### NAVIGATION

- **Nav padding:** 24px vertical, 48px horizontal
- **Nav link gap:** 36px between items
- **Active indicator:** 2px gold bar, positioned 4px below text

---

## 5. Border, Shadow & Surface System

### BORDERS

- **Default border:** 1px solid `#E5DDD0` (all cards, panels, inputs, nav bottom)
- **Input border:** 1.5px solid `#E5DDD0`
- **Tab border:** 1.5px solid `#E5DDD0` (inactive), 1.5px solid olive-dark (active)
- **Active nav indicator:** 2px solid `#C4A44A`, radius 1px
- **Card top color bar:** 4px height, linear-gradient(90deg, olive `#4A5D3A` → gold `#C4A44A`)
- **Section divider (intensity):** 1px solid `#E5DDD0`, with 28px padding-top and margin-top

### SHADOWS

- **Default elevation:** `0 4px 24px rgba(44, 40, 30, 0.08)`
- **Hover elevation:** `0 12px 40px rgba(44, 40, 30, 0.12)`
- **Save button hover:** `0 6px 20px rgba(45, 59, 45, 0.25)`
- **Toast shadow:** `0 8px 32px rgba(0, 0, 0, 0.2)`
- **Wheel SVG:** `drop-shadow(0 2px 12px rgba(0, 0, 0, 0.06))`

### SURFACES (Z-ORDER FRONT TO BACK)

1. **Grain overlay:** fixed fullscreen, z-index 9999, pointer-events none. 3% opacity fractal noise SVG.
2. **Toast:** fixed, z-index 200, centered at bottom 32px.
3. **Navigation:** sticky top, z-index 100, backdrop-filter blur(20px), warm-white background.
4. **Panels / Cards:** warm-white (`#FDFCF9`) surfaces with border + shadow.
5. **Page background:** cream (`#FAF6EE`) base layer.

---

## 6. Animation & Transition System

### KEYFRAME ANIMATIONS

| Name | From | To | Duration | Easing |
|---|---|---|---|---|
| fadeUp | opacity:0, translateY(16px) | opacity:1, translateY(0) | 0.5–0.8s | ease-out |
| popIn | opacity:0, scale(0.85) | opacity:1, scale(1) | 0.3s | cubic-bezier(0.34, 1.56, 0.64, 1) |

### STAGGERED ENTRY DELAYS

- **Page header:** 0s (immediate)
- **Tab bar:** 0.1s delay
- **Section content:** 0s on tab switch (fadeUp 0.6s)
- **Journal/Region cards:** 0.06s per card index (card[0]=0s, card[1]=0.06s, card[2]=0.12s, card[3]=0.18s)
- **Flavor tags:** popIn animation, triggered on each add

### HOVER / INTERACTION TRANSITIONS

| Element | Property | Duration | Easing | Effect |
|---|---|---|---|---|
| Nav link | color | 0.3s | ease (default) | text-muted → olive-dark |
| Tab button | all | 0.35s | cubic-bezier(0.4, 0, 0.2, 1) | Border gold-light, text brown |
| Star icon | transform | 0.2s | ease | scale(1.15) |
| Wheel segment | opacity, filter | 0.3s | ease | opacity 0.85, brightness(1.08) |
| Wheel selected | filter | 0.3s | ease | brightness(1.15), saturate(1.2) |
| Intensity fill | width | 0.3s | ease | Animated width change |
| Save button | all | 0.35s | ease | translateY(-1px), lighter bg, shadow |
| Save button:active | transform | instant | — | translateY(0) |
| Journal card | transform, box-shadow | 0.4s | cubic-bezier(0.4, 0, 0.2, 1) | translateY(-4px), shadow-lg |
| Region card | transform, box-shadow | 0.35s | ease | translateY(-3px), shadow |
| Flavor tag remove | opacity | instant | — | 0.7 → 1.0 |
| Input focus | border-color, box-shadow | 0.3s | ease | Gold border, 3px gold glow at 12% opacity |

### TOAST ANIMATION

- **Show:** translateY(80px) → translateY(0), opacity 0 → 1. Duration 0.4s, cubic-bezier(0.34, 1.56, 0.64, 1) (spring overshoot).
- **Auto-dismiss:** After 2500ms, reverse transition.

---

## 7. Component Catalog

Every reusable UI element in the app, described for implementation in any framework.

### 7.1 Tab Button (Pill)

- **Shape:** Full-round corners (border-radius 100px / capsule / stadium shape)
- **Padding:** 12px vertical, 28px horizontal
- **States:** Default (transparent bg, `#E5DDD0` border, muted text) | Hover (gold-light border, brown text) | Active (olive-dark fill, cream text, olive-dark border)

### 7.2 Text Input

- **Shape:** 10px radius
- **Padding:** 12px vertical, 16px horizontal
- **Border:** 1.5px solid border-color
- **States:** Default (cream bg) | Focused (gold border, 3px gold glow ring at 12% opacity) | Placeholder (text-muted color, normal weight)

### 7.3 Star Rating

- **Icon:** SVG star path — viewBox 0 0 24 24, path: `M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z`
- **Size:** 32px × 32px (full rating), 14px × 14px (mini on cards)
- **Gap:** 6px (full), 2px (mini)
- **Colors:** Inactive = border (`#E5DDD0` fill), Active = gold (`#C4A44A` fill)
- **Behavior:** Click star N → stars 1..N become active. Hover scales to 1.15x.

### 7.4 Intensity Slider (custom)

- **Track:** Full width (flex:1), 6px height, cream-dark bg, 3px radius
- **Fill:** Colored bar inside track. Fruity = `#6B9B4A`, Bitter = `#C49B3A`, Pungent = `#B85A4A`
- **Layout:** Row: [Label 80px right-aligned] [Track flex] [Value 28px centered]
- **Interaction:** Click anywhere on track → calculate percentage → map to 0-10 integer → update fill width and value display
- **Animation:** Fill width transitions over 0.3s ease

### 7.5 Flavor Tag (chip)

- **Shape:** Capsule (100px radius), 6px/14px padding
- **Color:** Background = flavor's color from wheel data, text = white
- **Remove button:** × character, 14px, opacity 0.7 → 1.0 on hover
- **Entry animation:** popIn (scale 0.85 → 1.0, spring easing, 0.3s)

### 7.6 Toast Notification

- **Position:** Fixed, bottom 32px, horizontally centered
- **Shape:** Capsule (100px radius), 14px/28px padding
- **Colors:** olive-dark background, cream text
- **Lifecycle:** Slide up from +80px with spring easing → visible 2500ms → slide down and fade

### 7.7 Journal Entry Card

- **Structure (top to bottom):** Color bar (4px gradient) | Content area (24px padding): [Date + Stars row] [Name] [Origin · Cultivar] [Flavor tags row] [3-column intensity bars]
- **Hover:** translateY(-4px), shadow upgrades to shadow-lg, 0.4s cubic-bezier

### 7.8 Region Card

- **Structure:** [Flag emoji 32px] [Region name] [Area subtext] [Description paragraph] [Cultivar tags row]
- **Cultivar tag:** cream-dark background, olive text, capsule shape, 4px/10px padding, 10px UPPERCASE

---

## 8. Screen: Navigation Bar

### LAYOUT

Full-width bar, sticky to top of viewport. Horizontal flex layout with space-between. Left: logo. Right: nav link list.

### LOGO

Text "OLEA" in Playfair Display Bold 28px, olive-dark, UPPERCASE, 2px letter spacing. Followed by a period "." in gold, italic, normal case, 0 letter spacing. No separate icon — the typography IS the brand mark.

### NAV LINKS

Three items: "New Tasting", "Journal", "Explore". Horizontal row, 36px gap. Active item gets olive-dark color + 2px gold underline 4px below baseline. Inactive items are text-muted. Hover transitions color over 0.3s.

### BEHAVIOR

Clicking a nav link switches the active tab (same behavior as the pill tab buttons below the page header). Nav links and tab pills are always in sync.

---

## 9. Screen: New Tasting (Tab 1)

### PAGE HEADER

Title: "Record a Tasting". Subtitle: "Capture every nuance of your olive oil experience". Centered, fadeUp 0.8s on load.

### LAYOUT

Two-column grid (1fr 1fr), 40px gap, align-items: start. Left column = Form Panel. Right column = Wheel Panel. On mobile (<900px), single column stacked.

### FORM PANEL (LEFT COLUMN)

Panel heading: "Oil Details" (Playfair Display 22px). Contains these form fields in order:

1. **Name / Brand** (full width text input). Placeholder: "e.g. Castello di Ama, Laconiko Reserve"
2. **Origin / Region + Cultivar** (2-column row, 16px gap). Placeholders: "e.g. Tuscany, Italy" and "e.g. Frantoio, Koroneiki"
3. **Harvest Date + Tasting Date** (2-column row). Harvest = month picker, Tasting = date picker.
4. **Overall Rating** (5-star SVG rating component, see Component 7.3)
5. **Tasting Notes** (textarea, min-height 80px). Placeholder: "Describe the aroma, mouthfeel, finish..."
6. **Save to Journal button** (full width, olive-dark fill, see Component 7.6 for behavior)

### WHEEL PANEL (RIGHT COLUMN)

Described in detail in the next section (Section 10).

---

## 10. Screen: Flavor Wheel (detailed)

### OVERVIEW

A two-ring interactive radial chart for tagging flavor notes. The wheel is the centerpiece of the tasting experience. It must be drawn programmatically (SVG, Canvas, or SkiaSharp).

### GEOMETRY

- **Viewport:** 400×400 unit coordinate space (scale to container)
- **Center:** (200, 200)
- **Center circle:** Radius 60, warm-white fill, 1px border. Contains text "Flavor" (13px semibold) + "Profile" (12px light), both Cormorant Garamond, olive-dark.
- **Inner ring (categories):** innerRadius=62, outerRadius=125. 8 segments. Gap between segments: 1.2°.
- **Outer ring (subcategories):** innerRadius=127, outerRadius=175. Each category's arc subdivided equally among its children. 0.5° gap between sub-segments.
- **Starting angle:** -90° (12 o'clock position)

### SEGMENT ARC CONSTRUCTION

Each segment is an SVG path (or equivalent) with 4 points: two on the inner radius, two on the outer radius, connected by arcs. The path formula: M(innerStart) L(outerStart) A(outerR arc to outerEnd) L(innerEnd) A(innerR arc back to innerStart) Z.

### DATA MODEL (8 CATEGORIES, 25 SUBCATEGORIES)

| Category | Children |
|---|---|
| Fruity (`#6B9B4A`) | Green Apple (`#7BA85A`), Tomato Leaf (`#5E8B3E`), Banana (`#8CB56A`), Citrus (`#7DAD4E`) |
| Floral (`#9B7BB5`) | Artichoke (`#A88BC2`), Chamomile (`#B69AC8`), Lavender (`#8E6BA8`) |
| Herbal (`#5A8B6A`) | Fresh Grass (`#6A9B7A`), Basil (`#4E7B5E`), Mint (`#5A9B78`), Rosemary (`#4A7058`) |
| Nutty (`#A67B5B`) | Almond (`#B08B6B`), Walnut (`#9A6B4B`), Pine Nut (`#B89878`) |
| Peppery (`#B85A4A`) | Black Pepper (`#A04A3A`), Arugula (`#C86A5A`), Chili (`#D4756A`) |
| Bitter (`#C49B3A`) | Radicchio (`#B48B2A`), Green Olive (`#D4AB4A`), Dark Choc. (`#A47B2A`) |
| Buttery (`#D4A85A`) | Cream (`#DEAF66`), Ripe Fruit (`#C89B4E`) |
| Woody (`#7A6B5A`) | Cedar (`#8A7B6A`), Hay (`#9A8B78`), Tobacco (`#6A5B4A`) |

### LABELS

- **Category labels:** Positioned at midpoint angle of segment arc, at radius (innerR + outerR) / 2 + 4. DM Sans 10px bold UPPERCASE white, centered, non-interactive.
- **Subcategory labels:** Same positioning logic in outer ring. DM Sans 8.5px normal white at 85% opacity.

### INTERACTION

- **Tap/click any segment:** Toggles that flavor in the selectedFlavors list.
- **Visual feedback on select:** Segment gets brightness(1.15) + saturate(1.2) filter.
- **Hover:** opacity 0.85, brightness(1.08).
- **Selected flavors display:** Below the wheel, a wrapping row of flavor tag chips (see Component 7.5). When empty, show italic placeholder "No flavors selected yet".

### INTENSITY PROFILE

Below the selected flavors, separated by a 1px border-top with 28px spacing. Three intensity sliders for Fruity, Bitter, and Pungent (see Component 7.4). Default values: Fruity=5, Bitter=3, Pungent=4.

---

## 11. Screen: My Journal (Tab 2)

### PAGE HEADER

Title: "Your Journal". Subtitle: "{count} tasting(s) recorded" (dynamic count from data).

### LAYOUT

Responsive auto-fill grid: columns minmax(340px, 1fr), 24px gap. Cards fill naturally.

### JOURNAL CARD ANATOMY (SEE COMPONENT 7.7)

From top to bottom inside each card:

1. **Color bar:** 4px tall, full width, linear-gradient(90deg, `#4A5D3A` → `#C4A44A`)
2. **Meta row:** Flex space-between. Left = formatted date (e.g. "Jan 15, 2025"). Right = mini star rating (5 stars, 14px, gold filled / border empty).
3. **Oil name:** Playfair Display 20px
4. **Origin line:** "{origin} · {cultivar}" — DM Sans 13px light, text-muted. Cultivar omitted if empty.
5. **Flavor tags:** Wrapping row of mini flavor tags (10px UPPERCASE, capsule, colored background, white text).
6. **Intensity bars:** 3-column equal flex row. Each column: [Label 9px] [4px track with colored fill]. Width = intensity × 10%. Colors: Fruity=`#6B9B4A`, Bitter=`#C49B3A`, Pungent=`#B85A4A`.

### EMPTY STATE

When no entries exist: centered layout spanning full grid width. Olive emoji (🫒) in an 80px circle (cream-dark bg), title "No tastings yet" (Cormorant Garamond 28px light), body "Record your first olive oil tasting to start building your journal." (DM Sans 14px light muted).

### CARD ORDERING

Most recent entries first (newest at top-left). New saves are prepended to the list.

---

## 12. Screen: Explore Regions (Tab 3)

### PAGE HEADER

Title: "Explore Regions". Subtitle: "Discover the world's great olive oil terroirs".

### LAYOUT

Two-column grid, 1fr 1fr, 40px gap. On mobile (<900px), single column.

### REGION CARD ANATOMY (SEE COMPONENT 7.8)

Each card contains: Flag emoji (32px), region name (Playfair 20px olive-dark), area subtext (13px gold-muted), description paragraph (13px light muted, 1.7 line-height), and a wrapping row of cultivar tags (10px UPPERCASE, cream-dark bg, olive text, capsule shape).

### REGION DATA

| Flag | Name | Area | Cultivars |
|---|---|---|---|
| 🇮🇹 | Tuscany | Central Italy | Frantoio, Moraiolo, Leccino |
| 🇬🇷 | Peloponnese | Southern Greece | Koroneiki, Athinolia, Manaki |
| 🇪🇸 | Andalusia | Southern Spain | Picual, Hojiblanca, Arbequina |
| 🇺🇸 | California | West Coast, USA | Mission, Arbequina, Arbosana |
| 🇹🇳 | Tunisia | North Africa | Chetoui, Chemlali |
| 🇵🇹 | Alentejo | Southern Portugal | Galega, Cobrançosa, Cordovil |

Each card also has a description paragraph (see seed data in Section 15).

---

## 13. Responsive Breakpoints

| Breakpoint | Changes |
|---|---|
| Default (>900px) | All grids 2-column, nav 48px padding, page title 48px, wheel 380px |
| <= 900px | Nav padding → 20px/24px. App padding → 28px/24px. New Tasting grid → 1 column. Explore grid → 1 column. Page title → 36px. Wheel container → 320px. |

The journal grid uses auto-fill with minmax(340px, 1fr), so it automatically reflows to 1 column on narrow screens without a breakpoint.

---

## 14. Accessibility Notes

- All interactive elements should have visible focus indicators (gold border + glow ring as defined for inputs)
- Star rating and flavor wheel segments should be keyboard-navigable (arrow keys) and have aria-labels
- Intensity sliders should support keyboard input (left/right arrow) and expose aria-valuenow, aria-valuemin, aria-valuemax
- Color is never the only indicator of state — selected wheel segments also have a brightness/saturation filter change; stars change fill completely
- Toast notifications should use role="status" and aria-live="polite"
- Minimum contrast ratio: all body text on cream background passes WCAG AA. White text on colored wheel segments/tags passes due to deliberately saturated backgrounds

---

## 15. Seed / Demo Data

The app should ship with 4 pre-populated journal entries for demonstration.

### ENTRY 1

- Name: Castello di Ama | Origin: Tuscany, Italy | Cultivar: Frantoio
- Harvest: 2024-10 | Tasting Date: 2025-01-15 | Rating: 5/5
- Flavors: Green Apple (`#7BA85A`), Black Pepper (`#A04A3A`), Basil (`#4E7B5E`)
- Intensities: Fruity 7, Bitter 5, Pungent 8
- Notes: "Incredible complexity. Opens with bright green apple, transitions to fresh basil, finishes with a long peppery burn."

### ENTRY 2

- Name: Laconiko Reserve | Origin: Laconia, Greece | Cultivar: Koroneiki
- Harvest: 2024-11 | Tasting Date: 2025-01-22 | Rating: 4/5
- Flavors: Citrus (`#7DAD4E`), Almond (`#B08B6B`), Chamomile (`#B69AC8`)
- Intensities: Fruity 8, Bitter 3, Pungent 4
- Notes: "Silky smooth with bright citrus on the nose. Delicate floral finish. Perfect for drizzling over fish."

### ENTRY 3

- Name: Oro del Desierto Coupage | Origin: Almería, Spain | Cultivar: Picual / Arbequina
- Harvest: 2024-10 | Tasting Date: 2025-02-03 | Rating: 4/5
- Flavors: Tomato Leaf (`#5E8B3E`), Arugula (`#C86A5A`), Green Olive (`#D4AB4A`)
- Intensities: Fruity 6, Bitter 6, Pungent 7
- Notes: "Robust and assertive. Tomato leaf aroma dominates, balanced by a pleasant bitter green olive character."

### ENTRY 4

- Name: California Olive Ranch Reserve | Origin: California, USA | Cultivar: Arbequina
- Harvest: 2024-11 | Tasting Date: 2025-02-10 | Rating: 3/5
- Flavors: Banana (`#8CB56A`), Cream (`#DEAF66`)
- Intensities: Fruity 7, Bitter 2, Pungent 2
- Notes: "Mild and approachable. Ripe banana notes with a creamy, buttery mouthfeel. Good everyday oil."

---

*— End of Design Brief —*
