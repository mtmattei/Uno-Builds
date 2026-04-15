# Design Specification: GRIDFORM — Dashboard, Orders List, Warehouse Planner

| Field | Value |
|-------|-------|
| **Document Type** | Design Specification |
| **Source Files** | `Screenshot 2026-04-01 221533.png` (Dashboard), `Screenshot 2026-04-01 221603.png` (Orders List), `Screenshot 2026-04-01 221638.png` (Warehouse Planner) |
| **Date** | 2026-04-01 |
| **Status** | Draft — review open questions before implementation |
| **Viewport/Canvas** | ~1280x900 (Desktop) |
| **Scale Factor** | 1x (logical pixels) |
| **Platform Target** | Desktop (WinUI 3 / Uno Platform — Windows + WASM primary) |

> This document is a pixel-accurate design specification intended for direct implementation.
> All measurements are in logical pixels (px) unless otherwise noted. Values prefixed with `~` are estimates.
> Cross-referenced against `DESIGN-BRIEF.md`, `ARCHITECTURE.md`, and `INTERACTION-SPEC.md` for consistency validation.

---

## 1. High-Level Intent

### Product Goal
Provide procurement managers at industrial tooling distributors with a unified operational dashboard that surfaces purchase order status, warehouse capacity, budget commitment, and AI-driven anomaly detection — reducing manual tracking and missed SLA deadlines.

### User Goal
Rapidly assess operational health (open POs, budget burn, warehouse utilization), triage flagged orders, and manage warehouse spatial planning — all from a single dark-themed ops console designed for 8-hour daily use.

### Screen Purpose
Three primary views:
1. **Dashboard** — At-a-glance KPI summary, order pipeline, and activity feed
2. **Orders List** — Tabular PO management with bulk actions, pipeline summary cards, and drill-through to detail
3. **Warehouse Planner** — Isometric spatial editor for warehouse layout with build/zone/erase tools

### Context
These are the three core operational views of the GRIDFORM application. Dashboard is the default landing page. Orders and Warehouse are accessed via left navigation. The app targets desktop-first (min 1024px) with a "Precision Industrial" visual language — warm dark theme, machined-metal aesthetic, density-as-feature.

---

## 2. Scope

### Screens Present
| Screen | Description | States Visible |
|--------|-------------|----------------|
| Dashboard | KPI cards, order pipeline, activity feed | Populated state with live data |
| Orders List | Pipeline summary cards, data table with status badges | Populated state with 7+ rows |
| Warehouse Planner | Isometric canvas with toolbar and layer controls | Populated state with rack/pallet layout |

### States Covered
- [x] Success/populated state (all three screens)
- [ ] Empty state
- [ ] Loading state
- [ ] Error state
- [ ] Partial/degraded state

### States Missing or Assumed
- **Loading**: `FeedView.ProgressTemplate` with `LoadingView` (Uno Toolkit) — specified in ARCHITECTURE.md but not shown
- **Empty**: `FeedView.NoneTemplate` with empty state icon + message — specified but not shown
- **Error**: `FeedView.ErrorTemplate` with retry — specified but not shown
- **Offline/Disconnected**: Status bar shows "Connected" — disconnected state not shown
- **Command Palette overlay**: Specified in INTERACTION-SPEC but not captured
- **Notification flyout**: Bell icon visible but flyout not shown
- **Order Detail view**: Referenced in specs but no screenshot provided
- **Toast notifications**: Specified but not visible in screenshots
- **Bulk action bar**: Specified for Orders when rows selected, not visible (no rows selected)

### Out of Scope
- Mobile/tablet responsive layouts (v1 desktop-only per INTERACTION-SPEC 3.8)
- Order Detail page (no screenshot)
- Settings/preferences
- Authentication/login

---

## 3. Information Architecture

### Page Hierarchy

```
App Shell
├── TopBar (48px)
│   ├── Logo + App Name ("GRIDFORM")
│   ├── Breadcrumb ("Dashboard" / "Operations / Purchase Orders" / "Operations / Warehouse Planner")
│   ├── Search Trigger
│   ├── Command Palette Trigger (Ctrl+K)
│   ├── Notification Bell (with unread dot)
│   └── User Avatar + Name + Role
├── Left Navigation (200px)
│   ├── OPERATIONS (section header)
│   │   ├── Dashboard (default, active)
│   │   ├── Warehouse
│   │   └── Orders (with count badge)
│   ├── REFERENCE (section header)
│   │   ├── Inventory (placeholder)
│   │   ├── Vendors (placeholder)
│   │   └── Contracts (placeholder)
│   └── WAREHOUSE (mini-metrics footer)
│       ├── Floor: 51.8% (progress bar)
│       ├── Volume: 19.5% (progress bar)
│       └── Load: 118.5t (progress bar)
├── Main Content Area (flex)
│   ├── [Dashboard View]
│   │   ├── KPI Row (5 cards)
│   │   ├── Order Pipeline Panel (left ~55%)
│   │   │   ├── Stacked Status Bar
│   │   │   ├── PO Summary Cards (x4)
│   │   │   └── "View all" Link
│   │   └── Activity Feed Panel (right ~45%)
│   │       └── Timestamped Event List (x7)
│   ├── [Orders View]
│   │   ├── Pipeline Summary Cards (x4)
│   │   └── Data Table (checkbox, PO#, vendor, amount, status, SLA, AI, actions)
│   └── [Warehouse View]
│       ├── Toolbar (mode, asset palette, presets)
│       ├── Isometric Canvas (full bleed)
│       └── Layer Scrubber (right edge)
└── StatusBar (28px)
    ├── Connection Status (green dot + "Connected")
    ├── Last Sync Timestamp
    ├── Organization Name
    └── Summary Counts (orders, units, tonnage)
```

### Grouping Logic
- **OPERATIONS** groups active workflow pages (Dashboard, Warehouse, Orders)
- **REFERENCE** groups lookup/master-data pages (Inventory, Vendors, Contracts) — currently placeholder
- KPIs grouped as a horizontal row for scan-and-compare
- Order Pipeline + Activity Feed are side-by-side for correlation (orders left, events right)
- Warehouse mini-metrics persist in nav footer for constant visibility regardless of active view

### Navigation Model
- **Left sidebar** (`NavigationView` with `PaneDisplayMode="Left"`) — persistent, 200px
- **Region-based routing** (Uno Extensions Navigation with `Region.Attached`)
- **Breadcrumb** in top bar shows current location
- **Drill-through** from Dashboard pipeline cards → Orders detail
- **Keyboard shortcuts** 1/2/3 for view switching

---

## 4. User Flows

### Entry Points
- App launch → Dashboard (default route)
- Direct navigation via sidebar
- Keyboard shortcut (1=Dashboard, 2=Warehouse, 3=Orders)
- Command palette (Ctrl+K)

### Primary Flow (Happy Path)
1. User opens app → sees Dashboard with KPI summary
2. Notices flagged PO in pipeline → clicks row
3. Navigates to Order Detail → reviews AI analysis
4. Approves or escalates → toast confirmation → returns to list
5. Switches to Warehouse → adjusts pallet placement
6. Returns to Dashboard to confirm metrics updated

### Alternate Paths
- **Bulk approval**: Orders list → select multiple rows → bulk approve bar → confirm
- **Warehouse preset**: Warehouse → click Layout A/B → full preset loads
- **Notification triage**: Bell icon → dropdown → review alerts

### Error/Edge Paths
- **Network disconnect**: Status bar dot turns red, "Disconnected" state
- **Approval failure**: Toast with error, PO stays in current state
- **Empty warehouse**: All metrics show 0%, AI brief suggests loading a preset

### Exit Points
- Dashboard pipeline card → Orders detail
- "View all" → Orders list
- Orders row → Order detail
- Any nav item → switch view
- Ctrl+K → Command palette overlay

---

## 5. Component Inventory

### Components
| Component | Variants | Count | Notes |
|-----------|----------|-------|-------|
| TopBar | Single | 1 | Spans full width, contains search/notifications/user |
| NavigationView | Single | 1 | Left sidebar with sections + footer metrics |
| NavItem | Active, Inactive, Disabled | 6 | OPERATIONS (3 active) + REFERENCE (3 placeholder) |
| NavBadge | Count | 1 | Orders badge showing pending+flagged count |
| KpiCard | Standard (5 variants by content) | 5 | Open POs, Pending Approval, Q2 Committed, Warehouse, Savings |
| DeltaBadge | Positive (green ↑), Negative (red), Neutral (amber) | 3 | Inside KPI cards |
| StatusBadge | Pending, Flagged, Review, Approved | 4 | Dot + label pill |
| PipelineBar | Single | 1 | Stacked horizontal segments |
| PipelineOrderCard | Single | 4 | PO row in dashboard pipeline |
| ActivityItem | Multiple event types | 7 | Timestamped event with colored dot |
| MiniMetrics | 3 bars (Floor, Volume, Load) | 1 | Nav footer widget |
| SearchBox | Single | 1 | TopBar search trigger |
| AvatarCircle | Initials | 1 | User avatar "MP" |
| StatusBar | Single | 1 | Connection, sync, org info |
| DataTable | Single | 1 | Orders list with headers + rows |
| PipelineSummaryCard | 4 statuses | 4 | Orders view top summary |
| Toolbar | Warehouse | 1 | Mode/asset/preset controls |
| ToolbarButton | Default, Active, Destructive (Erase) | ~10 | Toolbar items |
| IsometricCanvas | Single | 1 | SkiaSharp warehouse renderer |
| LayerScrubber | Single | 1 | Vertical layer control |
| Breadcrumb | Single | 1 | TopBar location indicator |

### Component Details

#### TopBar
- **Type**: Custom (Shell region)
- **Sizing**: Full width x 48px height
- **Visual Properties**:
  - Background: `#161614` (SurfaceColor)
  - Border-bottom: 1px `#282724` (OutlineColor)
  - Padding: 0 16px
  - Logo circle: 28px diameter, `#5FB89E` (PrimaryColor) background
  - "GRIDFORM" text: ~13px / 700 weight / `#EAE7DF` / ALL CAPS / letter-spacing ~1.5px
  - Breadcrumb text: ~13px / 400 / `#A8A49A` (OnSurfaceVariant)
  - Search input: ~200px wide, 32px height, `#1C1B18` background, 1px `#282724` border, 6px radius, placeholder `#706D64`
  - Bell icon: ~18px, `#A8A49A`, with 7x7 red unread dot (`#C75B5B`) when active
  - Avatar: 32px circle, `#D4956A` (SecondaryColor) gradient/solid, "MP" initials ~11px 600 weight white
  - Username: ~12px 500 `#EAE7DF`, role subtitle: ~10px 400 `#706D64`

#### KpiCard
- **Type**: Custom control
- **Variants**: 5 (contextual content differs)
- **States**: Default, Hover (border brightens per INTERACTION-SPEC)
- **Sizing**: ~200px width (flex, fills available space in 5-across row), ~95px height
- **Visual Properties**:
  - Background: `#1C1B18` (SurfaceContainerColor)
  - Border: 1px `#282724` (OutlineColor)
  - Border Radius: 10px all corners
  - Shadow: None (flat aesthetic per DESIGN-BRIEF 2.7)
  - Padding: 16px 18px
  - Bottom accent strip: 2px height, full width, color varies by card context:
    - Open POs: `#5FB89E` (Primary)
    - Pending Approval: `#D4956A` (Secondary/Copper)
    - Q2 Committed: `#D4A64E` (Warning/Pending)
    - Warehouse: `#5FB89E` (Primary)
    - Savings YTD: `#5FB89E` (Primary)
  - Label: 11px / 400 / `#706D64` (SurfaceTintColor) — "Open POs", etc.
  - Value: 26px / 700 / `#EAE7DF` (OnSurfaceColor) — "8", "$2.4M", etc.
  - Subtitle: 9px / 600 mono / `#3E3C38` (OnSurfaceDisabledColor) — "vs last quarter", etc.
  - Gap: label → value ~4px, value → subtitle ~6px

#### DeltaBadge
- **Type**: Inline badge within KpiCard
- **Variants**: Positive (↑ green), Negative (↓ red), Neutral (amber count)
- **Visual Properties**:
  - Positive: Background `rgba(95,184,158,0.12)`, text `#5FB89E`, "↑ 12%"
  - Negative/Alert: Background `rgba(199,91,91,0.12)`, text `#C75B5B`, "2 overdue"
  - Neutral: Background `rgba(212,166,78,0.12)`, text `#D4A64E`, "87%"
  - Font: 9px / 600 / mono (JetBrains Mono)
  - Padding: 2px 8px
  - Border Radius: 4px
  - Positioned inline-right of the value number

#### StatusBadge
- **Type**: Custom control (`StatusBadge.xaml`)
- **Variants**: Pending, Flagged, Review, Approved
- **Visual Properties**:
  - Pending: dot `#D4A64E`, text `#D4A64E`, bg `rgba(212,166,78,0.10)` → dim `#1F1C14`
  - Flagged: dot `#C75B5B`, text `#C75B5B`, bg `rgba(199,91,91,0.08)` → dim `#1F1416`
  - Review: dot `#6B9FC8`, text `#6B9FC8`, bg `rgba(107,159,200,0.10)` → dim `#14191F`
  - Approved: dot `#5FB89E`, text `#5FB89E`, bg `rgba(95,184,158,0.08)` → dim `#141F1C`
  - Dot: 5x5px circle
  - Text: 9px / 600 / mono (LabelSmall)
  - Padding: 2px 8px
  - Border Radius: 4px
  - Gap dot→text: 6px

#### PipelineBar
- **Type**: Custom control (`PipelineBar.xaml`)
- **Visual Properties**:
  - Height: ~8-10px
  - Border Radius: 4px (entire bar) — inner segments square except first/last
  - Segments flex-proportional to count per status
  - Segment colors match status: Approved `#5FB89E`, Review `#6B9FC8`, Pending `#D4A64E`, Flagged `#C75B5B`
  - Gap between segments: ~2px (or 0px with subtle opacity difference at edges)
  - Margin below: ~16px to first PO row

#### PipelineOrderCard (Dashboard)
- **Type**: Row within Order Pipeline panel
- **States**: Default, Hover (border brightens)
- **Visual Properties**:
  - Background: `#1C1B18` (SurfaceContainerColor)
  - Border: 1px `#282724` (OutlineColor)
  - Border Radius: ~6px
  - Height: ~40-44px
  - Padding: 10px 12px
  - PO Number: 10px / 600 / mono / `#706D64` — "PO-7421"
  - Vendor Name: 12px / 500 / Outfit / `#EAE7DF` — "Kennametal Inc."
  - Amount: 10px / 600 / mono / `#A8A49A` — "$48.2K"
  - SLA Time: 10px / 600 / mono / `#706D64` — "2d 6h"
  - Status Badge: right-aligned (see StatusBadge spec above)
  - Gap between rows: ~8px
  - Cursor: pointer

#### ActivityItem
- **Type**: Row within Activity Feed
- **Visual Properties**:
  - Height: ~36-40px
  - Timestamp: 9px / 600 / mono / `#3E3C38` — "1h", "23h", etc., ~40px min-width, right-aligned
  - Dot: 8px circle, color based on event type:
    - Shipment/system: `#706D64` (grey/muted)
    - Auto-approved/shipped: `#6B9FC8` (blue/info)
    - AI flagged: `#C75B5B` (red/error)
    - Rebalanced: `#5FB89E` (green/success)
    - Budget alert: `#D4A64E` (amber/warning)
    - Onboarded: `#706D64` (grey/neutral)
  - Message: 11.5px / 400 / Outfit / `#A8A49A` (OnSurfaceVariant)
  - Gap timestamp→dot: ~12px
  - Gap dot→message: ~10px
  - No separator lines between items (breathing room via gap)
  - Separator implied by vertical spacing ~8-10px between items

#### MiniMetrics (Nav Footer)
- **Type**: Custom control (`MiniMetrics.xaml`)
- **Visual Properties**:
  - Section label "WAREHOUSE": 9px / 600 / mono / `#3E3C38` / ALL CAPS / letter-spacing ~80
  - Each metric row:
    - Label: 9px / 600 / mono / `#706D64` — "Floor", "Volume", "Load"
    - Value: 9px / 600 / mono / `#A8A49A` — "51.8%", "19.5%", "118.5t"
    - Progress bar: ~100px wide, 3px height, border-radius 2px
      - Floor bar: `#5FB89E` (Primary/teal)
      - Volume bar: `#6B9FC8` (Tertiary/blue)
      - Load bar: `#D4956A` (Secondary/copper)
      - Track: `#282724` (OutlineColor)
  - Gap between metric rows: ~6px
  - Padding from sidebar edge: ~16px

#### NavItem
- **Type**: `NavigationViewItem` with custom lightweight styling
- **States**: Active, Inactive, Disabled (placeholder)
- **Visual Properties**:
  - Active:
    - Background: `#1A2E28` (PrimaryContainerColor) — pill shape
    - Text: 11px / 600 / Outfit / `#5FB89E` (PrimaryColor)
    - Icon: `#5FB89E`
    - Border Radius: 6px
  - Inactive:
    - Background: transparent
    - Text: 11px / 600 / Outfit / `#A8A49A` (OnSurfaceVariant)
    - Icon: `#A8A49A`
  - Disabled/Placeholder (REFERENCE items):
    - Background: transparent
    - Text: 11px / 400 / Outfit / `#3E3C38` (OnSurfaceDisabledColor)
    - Icon: `#3E3C38`
  - Height: ~36px
  - Padding: 8px 12px
  - Icon size: ~16px
  - Gap icon→text: ~10px
  - Full-width hit target (200px minus nav padding)

#### NavBadge (Orders Count)
- **Type**: Inline badge on NavItem
- **Visual Properties**:
  - Background: `#5FB89E` (PrimaryColor)
  - Text: 9px / 700 / mono / `#0F0F0D` (OnPrimaryColor)
  - Size: ~18px width x ~16px height
  - Border Radius: 8px (pill)
  - Padding: 2px 6px
  - Position: right-aligned within nav item

#### StatusBar
- **Type**: Custom control
- **Visual Properties**:
  - Background: `#161614` (SurfaceColor)
  - Border-top: 1px `#282724` (OutlineColor)
  - Height: 28px
  - Padding: 0 16px
  - Connection dot: 6px circle, `#5FB89E` (connected) / `#C75B5B` (disconnected)
  - Text: 9px / 400 / mono / `#3E3C38` (OnSurfaceDisabledColor)
  - Left: dot + "Connected" + separator + "Last sync: 2s ago" + separator + "Org: Precision Tools Midwest"
  - Right: "7 orders" + separator + "229 warehouse units" + separator + "118.5t"
  - Dot animation: `pulse` 2.5s ease-in-out infinite (opacity 1 → 0.3 → 1)

#### DataTable (Orders View)
- **Type**: `ItemsRepeater` with custom row template
- **Visual Properties**:
  - Header row:
    - Background: `#161614` (SurfaceColor)
    - Text: 9px / 600 / mono / `#3E3C38` / ALL CAPS / letter-spacing ~80
    - Height: ~32px
    - Padding: 10px 12px horizontal
    - Sticky on scroll
  - Data rows:
    - Even: transparent background
    - Odd: `#1C1B18` (SurfaceContainerColor) background
    - Height: ~44px
    - Padding: 10px 12px horizontal
    - Text hierarchy:
      - PO Number: 10px / 600 / mono / `#706D64`
      - Vendor: 12px / 500 / Outfit / `#EAE7DF`
      - Amount: 10px / 600 / mono / `#A8A49A` (right-aligned)
      - Status: StatusBadge component
      - SLA: 10px / 600 / mono, color varies by urgency (`#C75B5B` critical, `#D4A64E` warning, `#706D64` normal)
  - Row states:
    - Default: as above
    - Hover: `#242320` (SurfaceContainerHigh) background
    - Selected: `#1A2E28` (PrimaryContainerColor / AccentDim) background
  - Cursor: pointer on row body, default on checkbox

#### Toolbar (Warehouse)
- **Type**: Custom control bar
- **Visual Properties**:
  - Background: `#1C1B18` (SurfaceContainerColor)
  - Border: 1px `#282724` (OutlineColor)
  - Border Radius: 10px (matches card radius)
  - Height: ~44px
  - Padding: 6px 8px
  - Button groups separated by 1px divider `#282724`
  - Mode buttons (Build/Zone):
    - Default: `#242320` bg, `#A8A49A` text, 6px radius
    - Active: `#1A2E28` bg, `#5FB89E` text, 1px `#5FB89E` border
  - Erase button:
    - Default: same as mode default
    - Active: `rgba(199,91,91,0.12)` bg, `#C75B5B` text, 1px `#C75B5B` border
  - Asset palette buttons:
    - Default: `#242320` bg, asset-specific icon
    - Active: colored border matching asset accent
  - Preset buttons (Layout A/B, Clear):
    - Standard secondary button styling
  - Button size: ~32px height, variable width
  - Gap between buttons within group: 5-6px

#### IsometricCanvas
- **Type**: `SKXamlCanvas` / `SKCanvasElement` custom control
- **Visual Properties**:
  - Background: `#0F0F0D` (BackgroundColor) — full bleed
  - Grid lines: ~10-15% white opacity isometric grid
  - Voxels: 3D isometric blocks with ambient occlusion + edge highlights
    - Top face: base color + 18% white edge stroke
    - Left face: base color at ~75% brightness
    - Right face: base color at ~85% brightness
    - Labels: 6px bold mono, 30% black, center of top face
  - Ghost cursor: current asset color at 18% opacity
  - Zone tiles: colored isometric diamonds on ground plane
  - Depth fog: back-of-grid darkens to ~65% brightness
  - Ground shadows: dark diamonds under elevated voxels

#### LayerScrubber
- **Type**: Custom control
- **Visual Properties**:
  - Width: ~24-28px
  - Position: right edge of canvas
  - ▲ / ▼ buttons: 24x24px, `#A8A49A` icon, transparent bg
  - Layer indicators: 3px wide bars, ~16px height each
    - Active layer: `#5FB89E` (PrimaryColor)
    - Populated layer: `#3E3C38` (dim white)
    - Empty layer: ~`#1C1B18` (barely visible)
  - Gap between layer bars: 2px

---

## 6. Layout + Spacing

### Grid System
- **Columns**: Not a strict 12-column grid; uses flex-based layout within shell
- **Gutter**: N/A (flex gaps instead)
- **Margins**: ~16px page margins (left nav provides left margin, content has internal padding)
- **Max width**: Unconstrained (fills viewport)

### Shell Grid
| Region | Width / Height | Grid Position |
|--------|---------------|---------------|
| TopBar | full width x 48px | Row 0, ColumnSpan 2 |
| Left Nav | 200px x flex | Row 1, Column 0 |
| Main Content | flex x flex | Row 1, Column 1 |
| StatusBar | full width x 28px | Row 2, ColumnSpan 2 |

### Spacing Scale
Base unit: **4px**. Scale: 4, 6, 8, 10, 12, 14, 16, 18, 20, 24, 32, 48

| Token | Value | Base Multiple | Usage |
|-------|-------|---------------|-------|
| `spacing-xxs` | 4px | 1x | Badge internal padding (vertical) |
| `spacing-xs` | 6px | 1.5x | Button padding (vertical), toolbar gap |
| `spacing-sm` | 8px | 2x | Badge padding (horizontal), nav item padding, row gaps |
| `spacing-md` | 10px | 2.5x | KPI card gap, table cell padding |
| `spacing-base` | 12px | 3x | Button padding (horizontal), content horizontal padding |
| `spacing-lg` | 14px | 3.5x | Section gap (dashboard panels) |
| `spacing-xl` | 16px | 4x | Card internal padding, sidebar padding |
| `spacing-2xl` | 18px | 4.5x | Card internal padding (horizontal) |
| `spacing-3xl` | 24px | 6x | Section vertical padding |
| `spacing-4xl` | 32px | 8x | Major section gaps |

### Baseline Unit Compliance
The design uses a 4px base system but includes non-standard values (6px, 10px, 14px, 18px) which are 1.5x, 2.5x, 3.5x, and 4.5x multiples. This is a common pragmatic extension of the 4-point system. All values are still derived from the 2px sub-grid, maintaining visual consistency.

**Verdict**: Mostly compliant. The 14px section gap is unconventional but intentional — the density-as-feature philosophy calls for tighter gaps than a pure 16px system would produce.

### Key Measurements
| Element | Property | Value | Baseline Compliant? |
|---------|----------|-------|---------------------|
| TopBar | height | 48px | Yes (12x) |
| StatusBar | height | 28px | Yes (7x) |
| Left Nav | width | 200px | Yes (50x) |
| KPI card | padding | 16px 18px | 16 yes, 18 = 4.5x |
| KPI card gap | gap | 10px | 2.5x — acceptable |
| Section gap | gap | 14px | 3.5x — unconventional |
| Card border-radius | radius | 10px | 2.5x — not standard 4pt |
| Button border-radius | radius | 6px | 1.5x — not standard 4pt |
| Badge border-radius | radius | 4px | Yes (1x) |
| Table row height | height | ~44px | Yes (11x) |
| Nav item height | height | ~36px | Yes (9x) |
| Button height | height | ~32px | Yes (8x) |

### Spacing Relationships

| Relationship | Spacing A | Spacing B | Ratio | Consistent? |
|-------------|-----------|-----------|-------|-------------|
| Section gap vs KPI card gap | 14px | 10px | 1.4:1 | Yes — tight but intentional density |
| Card padding vs element gap (row) | 16px | 8px | 2:1 | Yes |
| Page margin (nav→content) vs section gap | ~16px | 14px | ~1.14:1 | Nearly equal — very dense |
| KPI label→value vs value→subtitle | ~4px | ~6px | 1:1.5 | Yes — subtitle needs more breathing room |
| Nav section header→first item | ~8px | — | — | Appropriate |
| Pipeline rows gap | 8px | — | — | Consistent across all 4 rows |
| Activity item vertical gap | ~8-10px | — | — | Consistent |

### Layout Proportions

| Region A | Region B | Width Ratio | Closest Standard Ratio |
|----------|----------|-------------|----------------------|
| Left Nav | Main Content | 200px : ~1080px | ~1:5.4 (close to 1:5) |
| Order Pipeline panel | Activity panel | ~55% : ~45% | ~1.2:1 (close to golden ratio 1:1.618 inverted) |
| KPI card width | KPI card height | ~200px : ~95px | ~2.1:1 (close to 2:1) |

**Content-to-Whitespace Ratio**: ~75-80% content, 20-25% whitespace. This is intentionally high — the design brief states "density is a feature" and "whitespace is earned, not assumed." This exceeds the typical 60-70% guideline but is appropriate for an 8hr/day ops tool.

**Above-the-fold Content**: ~100% of KPIs and ~70% of pipeline/activity visible without scrolling at 900px viewport height. Primary actions are all above the fold.

**Aspect Ratios Detected**:
- KPI cards: ~2.1:1 (landscape rectangle)
- Pipeline order rows: ~12:1 (wide strip)
- Dashboard panels: ~1.2:1 (Order Pipeline) and ~1.5:1 (Activity Feed)
- Avatar circle: 1:1

### ASCII Layout Map

#### Dashboard View
```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ (G) GRIDFORM   Dashboard                     [Search_______] [⌘K] (🔔) MP Matt P.     │ ← TopBar 48px
│                                                                          Op Manager    │
├──────────┬──────────────────────────────────────────────────────────────────────────────┤
│OPERATIONS│                                                                              │
│          │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐          │
│[■ Dashb.]│  │ Open POs │ │ Pending  │ │ Q2       │ │Warehouse │ │ Savings  │          │
│◆ Warehou.│  │ 8 [↑12%] │ │ Approval │ │Committed │ │ 51.0%    │ │ YTD      │          │
│◇ Orders 3│  │ vs last  │ │ 3[2 over]│ │$2.4M[87%]│ │ 22K·118t │ │$184K[↑23]│          │
│          │  │ quarter  │ │ avg 2.4d │ │ of q bud │ │          │ │ neg+cons │          │
│REFERENCE │  │━━━teal━━━│ │━copper━━━│ │━━amber━━━│ │━━teal━━━━│ │━━teal━━━━│          │
│  Inventor│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘          │
│  Vendors │             ← 10px gap →                                                    │
│  Contract│                                        ↕ 14px                               │
│          │  ┌────────────────────────────────┐  ┌─────────────────────────────────┐     │
│          │  │ Order Pipeline  HeadlineSmall  │  │ Activity        HeadlineSmall   │     │
│          │  │                                │  │                                 │     │
│          │  │ ███████████████████████████████ │  │ 1h  ● Shipment #4481 received  │     │
│          │  │ ← stacked status bar 8px tall→ │  │       — Sandvik, 4 pallets Bay2 │     │
│          │  │                    ↕ 16px       │  │                                 │     │
│          │  │ ┌────────────────────────────┐  │  │ 23h ● PO-7418 auto-approved    │     │
│          │  │ │PO-7421 Kennametal  $48.2K  │  │  │       — Haas, within policy     │     │
│          │  │ │         Inc.    2d6h[Pend.]│  │  │                                 │     │
│          │  │ └────────────────────────────┘  │  │ 1h  ● AI flagged PO-7420       │     │
│          │  │         ↕ 8px                   │  │       — ceiling exceeded +$12K   │     │
│          │  │ ┌────────────────────────────┐  │  │                                 │     │
│          │  │ │PO-7420 Sandvik   $67.8K    │  │  │ 2h  ● Zone B3 rebalanced       │     │
│          │  │ │         Coromant 1d4h[Flag]│  │  │       — 3 pallets → staging     │     │
│          │  │ └────────────────────────────┘  │  │                                 │     │
│          │  │ ┌────────────────────────────┐  │  │ 4h  ● Walter Tools onboarded    │     │
│          │  │ │PO-7419 Mitutoyo  $89.5K    │  │  │       — initial risk: B         │     │
│          │  │ │         America  3d2h[Revw]│  │  │                                 │     │
│          │  │ └────────────────────────────┘  │  │ 6h  ● Q2 budget alert           │     │
│          │  │ ┌────────────────────────────┐  │  │       — 87% committed           │     │
│          │  │ │PO-7417 Sandvik   $54.2K    │  │  │                                 │     │
│          │  │ │         Coromant  12h[Pend.]│  │  │ 8h  ● PO-7415 shipped          │     │
│          │  │ └────────────────────────────┘  │  │       — Haas, #HAS-44182        │     │
│          │  │                                │  │                                 │     │
│          │  │       [View all →]             │  │                                 │     │
│          │  └────────────────────────────────┘  └─────────────────────────────────┘     │
│          │                     ← 14px gap →                                             │
│WAREHOUSE │                                                                              │
│Floor 51.8│                                                                              │
│Vol  19.5 │                                                                              │
│Load 118.5│                                                                              │
├──────────┴──────────────────────────────────────────────────────────────────────────────┤
│ ● Connected  Last sync: 2s ago  Org: Precision Tools Midwest    7 orders · 229 · 118.5t│ ← StatusBar 28px
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Orders List View
```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ (G) GRIDFORM   Operations / Purchase Orders              [Search___] [⌘K] (🔔) MP     │ ← TopBar 48px
├──────────┬──────────────────────────────────────────────────────────────────────────────┤
│OPERATIONS│                                                                              │
│          │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                        │
│  Dashbrd │  │Pending   │ │In Review │ │Approved  │ │Flagged   │  ← 4 pipeline summary  │
│  Warehou.│  │ 2        │ │ 2        │ │ 2        │ │ 1        │     cards              │
│[■ Orders]│  └──────────┘ └──────────┘ └──────────┘ └──────────┘                        │
│          │                                        ↕ ~14px                               │
│REFERENCE │  ┌───────────────────────────────────────────────────────────────────────┐   │
│  Inventor│  │ ☐  PO#     VENDOR           AMOUNT    STATUS    SLA    AI   ACTIONS  │   │
│  Vendors │  ├───────────────────────────────────────────────────────────────────────┤   │
│  Contract│  │ ☐  PO-7421 Kennametal Inc.   $48,200  ●Pending  2d 6h  ●    ...     │   │
│          │  │ ☐  PO-7420 Sandvik Coromant  $67,800  ●Flagged  1d 4h  AI   ...     │   │
│          │  │ ☐  PO-7419 Mitutoyo America  $89,500  ●Review   3d 2h       ...     │   │
│          │  │ ☐  PO-7421 Haas Automation   $18,300  ●Approved 4d     ●    ...     │   │
│          │  │ ☐  PO-7418 Sandvik Coromant  $54,200  ●Approved 4d          ...     │   │
│          │  │ ☐  PO-7416 Walter Tools      $32,800  ●Pending  4d     ●    ...     │   │
│          │  │ ☐  PO-7415 Haas Automation   $36,400  ●Review   5d          ...     │   │
│          │  └───────────────────────────────────────────────────────────────────────┘   │
│          │                                                                              │
│WAREHOUSE │                                                                              │
│Floor 51.8│                                                                              │
│Vol  19.5 │                                                                              │
│Load 118.5│                                                                              │
├──────────┴──────────────────────────────────────────────────────────────────────────────┤
│ ● Connected  Last sync: 2s ago  Org: Precision Tools Midwest                           │ ← StatusBar 28px
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Warehouse Planner View
```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ (G) GRIDFORM   Operations / Warehouse Planner            [Search___] [⌘K] (🔔) MP     │ ← TopBar 48px
├──────────┬──────────────────────────────────────────────────────────────────────────────┤
│OPERATIONS│                                                                              │
│          │ ┌────────────────────────────────────────────────────────────────────────┐   │
│  Dashbrd │ │[Build][Zone]│[Erase]│[Pallet][Rack][Cont][Equip][Aisle]│[LayoutA][B][C]│  │ ← Toolbar ~44px
│[■ Wareh.]│ └────────────────────────────────────────────────────────────────────────┘   │
│  Orders  │                                                                        ┌──┐ │
│          │                                                                        │▲ │ │
│REFERENCE │                          ╱──────────╲                                  │██│ │
│  Inventor│                         ╱ ┌────────┐ ╲                                 │░░│ │
│  Vendors │                        ╱  │ RACK 1 │  ╲                                │░░│ │
│  Contract│                       ╱   └────────┘   ╲                               │░░│ │
│          │                      ╱ ┌────────┐        ╲                              │▼ │ │
│          │                     ╱  │ RACK 2 │         ╲                             └──┘ │
│          │                    ╱   └────────┘          ╲                                 │
│          │                   ╱ ┌────────┐              ╲           ← Layer Scrubber     │
│          │                  ╱  │ RACK 3 │               ╲              ~28px wide       │
│          │                 ╱   └────────┘                ╲                              │
│          │                ╱  [P][P]  [P][P]  [P][P] [P][P]╲                             │
│          │               ╱   pallets along edges            ╲                           │
│          │              ╲────────────────────────────────────╱                           │
│          │               ← Isometric Canvas (full bleed) →                              │
│          │                                                                              │
│          │   POS 6,8 · H2 · 42 units                              ← HUD overlay        │
│WAREHOUSE │                                                                              │
│Floor 51.8│                                                                              │
│Vol  19.5 │                                                                              │
│Load 118.5│                                                                              │
├──────────┴──────────────────────────────────────────────────────────────────────────────┤
│ ● Connected  Last sync: 2s ago  Org: Precision Tools Midwest                           │ ← StatusBar 28px
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### Responsiveness
v1 is desktop-only (min 1024px). Per INTERACTION-SPEC 3.8, future breakpoints:
- >=1280px: Full sidebar (200px)
- 900-1279px: Collapsed sidebar (icon-only, 56px)
- 600-899px: Bottom TabBar, single column
- <600px: Bottom TabBar, canvas hidden

---

## 7. Typography System

### Font Families
| Role | Family | Type | Fallback |
|------|--------|------|----------|
| Display + Body | Outfit | Sans-serif (Variable) | Segoe UI Variable, system-ui |
| Mono / Data | JetBrains Mono | Monospace | Cascadia Code, SF Mono, Consolas |

### Type Scale
| Style Name | Family | Size | Weight | Line Height | Letter Spacing | Usage | Modular Scale? |
|------------|--------|------|--------|-------------|----------------|-------|----------------|
| DisplaySmall | Outfit | 26px | 700 | 32px (1.23) | 0 | KPI values, hero numbers | Yes (anchor) |
| HeadlineSmall | Outfit | 17px | 700 | 24px (1.41) | 0 | Page titles, section headers | ~Yes (26/1.53) |
| TitleMedium | Outfit | 14px | 600 | 20px (1.43) | 0 | Card titles, PO detail header | ~Yes (17/1.21) |
| TitleSmall | Outfit | 13px | 600 | 20px (1.54) | 0 | Sub-section titles | ~Yes |
| BodyLarge | Outfit | 12px | 500 | 20px (1.67) | 0 | Table cells, vendor names | No — high line-height |
| BodyMedium | Outfit | 11.5px | 400 | 16px (1.39) | 0 | Activity messages, AI text | No — fractional |
| BodySmall | Outfit | 11px | 400 | 16px (1.45) | 0 | Descriptions, subtitles | No |
| LabelLarge | Outfit | 11px | 600 | 16px (1.45) | 0 | Buttons, nav items, chips | No |
| LabelMedium | JetBrains Mono | 10px | 600 | 16px (1.6) | +40 | Amounts, PO numbers, SKUs | No |
| LabelSmall | JetBrains Mono | 9px | 600 | 12px (1.33) | +80 | Table headers, timestamps, badges | No |

### Typography Rules Audit
- [x] Sans-serif used for body/labels/small text — Outfit (sans-serif) for all UI text
- [x] Serif only for large headers/hero — no serif fonts used at all (appropriate for ops tool)
- [x] Bold used sparingly — only DisplaySmall (KPI values) and HeadlineSmall (section titles) are 700 weight
- [x] No italic on buttons/labels — no italics observed anywhere
- [x] ALL CAPS only on titles/labels/buttons — used for nav section headers (OPERATIONS, REFERENCE, WAREHOUSE) and table headers
- [ ] Letter spacing reduced for large text (>64px) — N/A, no text exceeds 64px
- [x] Line height: headers ~1.15-1.25 — DisplaySmall at 1.23 passes. HeadlineSmall at 1.41 is slightly high for a header
- [ ] Font sizes follow modular scale — **Partial**. The scale is functional but not a strict mathematical progression (golden ratio would give 9, 11, 14, 17, 22, 28... vs actual 9, 10, 11, 11.5, 12, 13, 14, 17, 26). The density-driven design compresses the scale at the small end
- [x] Weight increases for small text, decreases for large — LabelSmall is 600 weight at 9px, DisplaySmall is 700 at 26px. Small text IS heavier relative to size
- [x] Clear hierarchy — size + weight + color + font-family (mono vs sans) all contribute to hierarchy

**Issues**:
- **HeadlineSmall line-height at 1.41** is above the recommended 1.15-1.25 for headers. Consider reducing to ~1.29 (22px for 17px text) — **Minor**
- **11.5px BodyMedium** is a fractional size. Consider rounding to 12px or 11px for cleaner rendering — **Minor**
- **BodyLarge line-height at 1.67** is quite generous for 12px text in a dense UI. The 20px line-height is reused across multiple styles for alignment, which is a legitimate baseline-grid strategy — **Acceptable**

---

## 8. Color + Theming

### Color Palette
| Token Name | Hex | Weight | Usage |
|------------|-----|--------|-------|
| PrimaryColor | `#5FB89E` | 500 | Primary actions, active nav, links, approval status |
| PrimaryContainerColor | `#1A2E28` | 900 (dim) | Accent backgrounds, selected nav/row states |
| OnPrimaryColor | `#0F0F0D` | — | Text on accent buttons |
| SecondaryColor | `#D4956A` | 500 | Copper accent, avatar gradient, secondary warmth |
| SecondaryContainerColor | `#2B1F16` | 900 (dim) | Secondary backgrounds |
| TertiaryColor | `#6B9FC8` | 500 | Informational, "In Review" status, info events |
| ErrorColor | `#C75B5B` | 500 | Flagged, rejected, SLA critical, AI alerts |
| WarningColor (StatusPending) | `#D4A64E` | 500 | Pending status, budget alerts |
| BackgroundColor | `#0F0F0D` | 950 | App background, main content area |
| SurfaceColor | `#161614` | 900 | Panels — nav, topbar, statusbar |
| SurfaceContainerColor | `#1C1B18` | 850 | Cards, elevated surfaces, alt table rows |
| SurfaceContainerHighColor | `#242320` | 800 | Hover states, selected rows, dropdowns |
| SurfaceContainerHighestColor | `#2A2926` | 750 | Active hover, pressed states |
| OutlineColor | `#282724` | 700 | Default borders |
| OutlineVariantColor | `#363431` | 650 | Hover/focus borders |
| OnSurfaceColor | `#EAE7DF` | 100 | Primary text — headings, values |
| OnSurfaceVariantColor | `#A8A49A` | 300 | Secondary text — descriptions, body |
| SurfaceTintColor | `#706D64` | 500 | Tertiary text — labels, metadata |
| OnSurfaceDisabledColor | `#3E3C38` | 700 | Disabled text, muted hints |

### Opacity Map
| Element | Opacity | Purpose | Matches Rule? |
|---------|---------|---------|---------------|
| Primary text (OnSurface) | 100% | Full visibility | Yes (87-100%) |
| Secondary text (OnSurfaceVariant) | ~67% relative luminance | De-emphasis | Yes (54-60% — slightly high but acceptable) |
| Tertiary text (SurfaceTint) | ~45% relative luminance | Labels, metadata | Close (38% rule — slightly above) |
| Disabled text (OnSurfaceDisabled) | ~25% relative luminance | Muted hints | Yes (38% — using color rather than opacity) |
| Status badge background (Pending) | 10% | Tinted pill bg | Yes — subtle fill |
| Status badge background (Approved) | 8% | Tinted pill bg | Yes — subtle fill |
| Status badge background (Flagged) | 8% | Tinted pill bg | Yes — subtle fill |
| Status badge background (Review) | 10% | Tinted pill bg | Yes — subtle fill |
| Delta badge backgrounds | ~12% | KPI delta tint | Yes — hover range (4-8% would be for hover, 12% for pressed/emphasis) |
| Connection dot pulse | 100% → 30% → 100% | Breathing indicator | Yes — animated opacity |
| Canvas ghost cursor | 18% | Placement preview | Yes — between hover and pressed range |
| Canvas depth fog | ~65% brightness | Spatial depth | Custom (appropriate for 3D context) |
| Canvas edge highlights | 18% white | Edge separation | Custom (appropriate) |
| Isometric grid lines | ~10-15% | Subtle grid overlay | Yes — matches divider range |

**Note**: This design uses explicit color tokens (varying hex values) rather than opacity-on-white/black to achieve hierarchy. This is the correct approach for dark themes where opacity-based text can appear washed out. The DESIGN-BRIEF defines distinct hex values for each text tier rather than applying opacity to a single text color.

### Gradients
No explicit gradients are visible in the three screenshots. The design uses a flat aesthetic consistent with the DESIGN-BRIEF's elevation spec (2.7): "GRIDFORM uses a flat aesthetic with border-defined surfaces."

| Element | Type | Direction/Angle | Color Stops | Notes |
|---------|------|-----------------|-------------|-------|
| Avatar (MP) | Possible radial/linear | — | `#D4956A` → variant | Subtle — may be solid copper |
| Depth fog (canvas) | Positional | Back→front | 65% brightness → 100% | Not a CSS gradient — computed per-voxel |

**Gradient Consistency**: N/A — no decorative gradients. This is intentional and appropriate for the industrial aesthetic.

### Color Harmony Analysis
The palette uses a **split-complementary / triadic** approach centered on warm neutrals:
- **Primary (teal/green)**: `#5FB89E` — ~160deg hue
- **Secondary (copper/orange)**: `#D4956A` — ~25deg hue
- **Tertiary (blue)**: `#6B9FC8` — ~210deg hue
- **Error (red)**: `#C75B5B` — ~0deg hue
- **Warning (amber)**: `#D4A64E` — ~38deg hue

The teal-copper pairing is roughly complementary (~135deg apart). The blue tertiary sits between them. All chromatic colors are desaturated (muted), staying well within the "saturation safety zone" — none are fully saturated. This creates a cohesive, professional industrial palette.

**Verdict**: Strong harmony. The warm neutral base with muted chromatic accents achieves the "machined metal, brass fixtures" aesthetic described in the design intent.

### Color Psychology Check
| Color | Hex | Intended Meaning | Psychology Match? |
|-------|-----|-----------------|-------------------|
| Teal/Green `#5FB89E` | Primary/Approved | Trust, success, go-ahead | Yes — green = success/positive |
| Copper `#D4956A` | Secondary/Warmth | Friendly, inviting, industrial | Yes — orange = friendly/inviting |
| Blue `#6B9FC8` | Tertiary/Info/Review | Trust, information | Yes — blue = trust/info |
| Red `#C75B5B` | Error/Flagged | Danger, importance | Yes — red = danger/importance |
| Amber `#D4A64E` | Warning/Pending | Caution, attention | Yes — yellow = warning/highlight |

All semantic color assignments match color psychology rules. No mismatches.

### Semantic Mapping
| Semantic Role | Color | Correct Usage? |
|---------------|-------|----------------|
| Error/Danger/Flagged | `#C75B5B` (Red) | Yes — used for flagged POs, SLA critical, AI alerts |
| Success/Approved | `#5FB89E` (Teal/Green) | Yes — used for approved status, positive deltas, active nav |
| Warning/Pending | `#D4A64E` (Amber) | Yes — used for pending status, budget warnings |
| Info/In Review | `#6B9FC8` (Blue) | Yes — used for review status, informational activity events |
| Disabled/Muted | `#3E3C38` (Dark Grey) | Yes — used for placeholder nav items, disabled text |

### Color Weight Scale
The design defines implicit weight through its surface hierarchy (950 → 750 for backgrounds, 100 → 700 for text). However, it does not use a formal 100-800 weight scale per individual hue. Instead:
- Each chromatic color has a **foreground** (full) and **dim background** variant
- The surface scale acts as the neutral weight ramp

This is a pragmatic approach for a dark-theme-only application but would need expansion if light mode is added.

### Dark/Light Mode
Only dark mode is shown and specified. The DESIGN-BRIEF does not mention light mode. Per the design intent ("Precision Industrial"), dark mode is the primary and likely only theme.

**Assumption**: No light mode planned for v1. If added later, the color weight scale would need to be inverted: backgrounds → light (100-300 weights), text → dark (600-800 weights), and all status dim backgrounds recalculated.

### Contrast Audit
| Element | FG | BG | Ratio (est.) | WCAG AA (4.5:1) | Pass/Fail |
|---------|----|----|-------------|-----------------|-----------|
| Primary text on Background | `#EAE7DF` | `#0F0F0D` | ~15.8:1 | 4.5:1 | Pass |
| Primary text on SurfaceContainer | `#EAE7DF` | `#1C1B18` | ~13.2:1 | 4.5:1 | Pass |
| Secondary text on Background | `#A8A49A` | `#0F0F0D` | ~7.5:1 | 4.5:1 | Pass |
| Secondary text on SurfaceContainer | `#A8A49A` | `#1C1B18` | ~6.3:1 | 4.5:1 | Pass |
| Tertiary text on Background | `#706D64` | `#0F0F0D` | ~3.8:1 | 4.5:1 | **Fail** |
| Tertiary text on SurfaceContainer | `#706D64` | `#1C1B18` | ~3.2:1 | 4.5:1 | **Fail** |
| Disabled text on Background | `#3E3C38` | `#0F0F0D` | ~1.9:1 | 4.5:1 | **Fail** (intentional — disabled) |
| Primary accent on Background | `#5FB89E` | `#0F0F0D` | ~8.7:1 | 4.5:1 | Pass |
| Error on SurfaceContainer | `#C75B5B` | `#1C1B18` | ~4.5:1 | 4.5:1 | Borderline Pass |
| Warning on SurfaceContainer | `#D4A64E` | `#1C1B18` | ~5.8:1 | 4.5:1 | Pass |
| Tertiary (blue) on SurfaceContainer | `#6B9FC8` | `#1C1B18` | ~5.5:1 | 4.5:1 | Pass |
| StatusBar text on Surface | `#3E3C38` | `#161614` | ~1.6:1 | 4.5:1 | **Fail** (intentional — ambient) |

**Critical Finding**: `SurfaceTintColor` (`#706D64`) used for labels, metadata, and timestamps fails WCAG AA contrast on both `BackgroundColor` and `SurfaceContainerColor`. This affects:
- KPI card labels ("Open POs", "Pending Approval")
- PO numbers in pipeline cards ("PO-7421")
- SLA timestamps
- Activity timestamps

**Recommendation**: Increase `SurfaceTintColor` luminance from `#706D64` to approximately `#8A877D` to achieve 4.5:1 on `#1C1B18`. This maintains the muted aesthetic while meeting accessibility minimums. Alternatively, accept the violation as intentional de-emphasis (common in dense ops UIs) and document the WCAG exception.

---

## 9. Interaction Design

### State Map
| Element | Default | Hover | Pressed | Focused | Disabled | Valid | Invalid |
|---------|---------|-------|---------|---------|----------|-------|---------|
| KPI Card | SurfaceContainer bg, Outline border | OutlineVariant border brightens | SurfaceContainerHighest bg | Outline + focus ring | N/A | N/A | N/A |
| Pipeline Row | SurfaceContainer bg, Outline border | OutlineVariant border | SurfaceContainerHighest bg | Focus ring | N/A | N/A | N/A |
| Nav Item | Transparent bg | SurfaceContainerHigh bg | SurfaceContainerHighest bg | Focus ring | OnSurfaceDisabled text | N/A | N/A |
| Table Row | Even: transparent / Odd: SurfaceContainer | SurfaceContainerHigh bg | SurfaceContainerHighest bg | Focus ring | N/A | N/A | N/A |
| Button (Primary) | PrimaryColor bg | Lightened ~10% | Darkened ~10% | PrimaryColor + focus ring | 38% opacity | N/A | N/A |
| Button (Secondary) | SurfaceContainerHigh bg | SurfaceContainerHighest bg | Darker | Focus ring | 38% opacity | N/A | N/A |
| Toolbar Button | SurfaceContainerHigh bg | SurfaceContainerHighest bg | Pressed shade | Focus ring | N/A | N/A | N/A |
| Toolbar Button (Active) | PrimaryContainer bg + Primary border | Lightened | — | Focus ring | N/A | N/A | N/A |
| Checkbox | Outline border, transparent | Outline brightens | — | Focus ring | OnSurfaceDisabled | N/A | N/A |
| Checkbox (Checked) | PrimaryColor fill + checkmark | Lightened | — | Focus ring | OnSurfaceDisabled + muted fill | N/A | N/A |

### Detailed Interaction State Specifications

#### KPI Card — State Detail
| Property | Default | Hover | Pressed |
|----------|---------|-------|---------|
| Background | `#1C1B18` | `#1C1B18` | `#2A2926` |
| Border | 1px `#282724` | 1px `#363431` | 1px `#363431` |
| Shadow | none | none | none |
| Cursor | pointer | pointer | pointer |
| Transition | — | 150ms ease | 50ms ease |

#### Pipeline Order Row — State Detail
| Property | Default | Hover | Pressed |
|----------|---------|-------|---------|
| Background | `#1C1B18` | `#242320` | `#2A2926` |
| Border | 1px `#282724` | 1px `#363431` | 1px `#363431` |
| Cursor | pointer | pointer | pointer |
| Transition | — | 150ms ease | 50ms ease |

#### Nav Item — State Detail
| Property | Inactive | Hover | Active |
|----------|----------|-------|--------|
| Background | transparent | `#242320` | `#1A2E28` |
| Text Color | `#A8A49A` | `#EAE7DF` | `#5FB89E` |
| Icon Color | `#A8A49A` | `#EAE7DF` | `#5FB89E` |
| Border Radius | 6px | 6px | 6px |
| Cursor | pointer | pointer | default |
| Transition | — | 150ms ease | — |

#### Table Row (Orders) — State Detail
| Property | Default (even) | Default (odd) | Hover | Selected |
|----------|---------------|---------------|-------|----------|
| Background | transparent | `#1C1B18` | `#242320` | `#1A2E28` |
| Text Color | per column spec | per column spec | per column spec | per column spec |
| Border-bottom | 1px `#282724` at ~12% | same | same | 1px `#5FB89E` at ~20% |
| Cursor | pointer | pointer | pointer | pointer |
| Transition | — | — | 100ms ease | instant |

#### Toolbar Button — State Detail
| Property | Default | Hover | Active (mode selected) | Active + Hover |
|----------|---------|-------|----------------------|----------------|
| Background | `#242320` | `#2A2926` | `#1A2E28` | `#1A2E28` lightened |
| Border | 1px `#282724` | 1px `#363431` | 1px `#5FB89E` | 1px `#5FB89E` |
| Text/Icon Color | `#A8A49A` | `#EAE7DF` | `#5FB89E` | `#5FB89E` |
| Cursor | pointer | pointer | default | pointer |

#### Erase Button (Toolbar) — State Detail
| Property | Default | Hover | Active (erase on) |
|----------|---------|-------|-------------------|
| Background | `#242320` | `#2A2926` | `rgba(199,91,91,0.12)` |
| Border | 1px `#282724` | 1px `#363431` | 1px `#C75B5B` |
| Text/Icon Color | `#A8A49A` | `#EAE7DF` | `#C75B5B` |

### Interaction Rules Audit
- [ ] Input hover: only 5-15% shade/tint change — **No inputs visible in screenshots** (Search box is a trigger, not a full input). The Orders view may have search/filter inputs not shown
- [x] Inputs visually distinct from buttons — Search trigger is clearly styled as an input (recessed, muted border)
- [ ] Submit button shows loading spinner — **Not visible** (Toast pattern used instead per INTERACTION-SPEC)
- [ ] Toggle takes immediate effect — **No toggles visible** in current screenshots
- [ ] Focus states use box-shadow — **Not determinable** from static screenshots
- [ ] Valid = green, Invalid = red + helper text — **Not applicable** to current views (no form inputs)
- [x] Disabled states use muted colors — REFERENCE nav items use `#3E3C38`
- [x] Hover backgrounds use subtle fill — `#242320` on `#1C1B18` is ~5% brightness increase
- [x] Pressed backgrounds use slightly more fill — `#2A2926` on `#1C1B18` is ~10% brightness increase
- [ ] All interactive elements have visible focus indicators — **Not determinable** from screenshots

### Transitions + Animation
Per INTERACTION-SPEC 3.9:

| Trigger | Element | Animation | Duration | Easing | Delay |
|---------|---------|-----------|----------|--------|-------|
| Page load | KPI cards (x5) | fadeUp (translateY 10px→0, opacity 0→1) | 0.4s | ease | +0.07s stagger |
| Page load | Pipeline panel | fadeUp | 0.4s | ease | 0.30s |
| Page load | Activity panel | fadeUp | 0.4s | ease | 0.35s |
| Page load | Pipeline cards (x4) | fadeIn (opacity 0→1) | 0.3s | ease | +0.05s stagger from 0.35s |
| Page load | Activity items (x7) | fadeIn | 0.3s | ease | +0.05s stagger from 0.4s |
| Page load | Table rows | fadeIn | 0.2s | ease | +0.03s stagger |
| Hover | Any interactive | Background/border transition | 0.15s | ease | 0ms |
| Action complete | Toast | fadeUp (translateY 8px→0, opacity 0→1) | 0.25s | ease | 0ms |
| Toast dismiss | Toast | Auto-dismiss | 2.4s | — | — |
| Cmd palette open | Modal | fadeUp + scale(0.98→1) | 0.15s | ease | 0ms |
| Status bar dot | Connection indicator | pulse (opacity 1→0.3→1) | 2.5s | ease-in-out | infinite |
| Nav metric bars | Mini progress bars | width 0→N% | 0.5s | cubic-bezier(0.16,1,0.3,1) | 0ms |
| Selection toggle | Bulk action bar | fadeUp | 0.2s | ease | 0ms |

### Gestures
Desktop-only v1. No touch gestures specified.

| Gesture | Element | Action | Notes |
|---------|---------|--------|-------|
| Scroll wheel | Warehouse canvas | Change active layer (up=+1, down=-1) | `preventDefault` to block page scroll |
| Right-click | Warehouse canvas | Erase action | `ContextMenu` suppressed |
| Click + drag | Warehouse canvas | Not specified in v1 | Future: paint/drag placement |

### Micro-interactions
| Element | Trigger | Visual Change | Duration | Notes |
|---------|---------|---------------|----------|-------|
| Status bar dot | Continuous | Green pulse (opacity cycling) | 2.5s loop | Shows connection is live |
| AI analysis dot | Continuous | Green pulse | 2.5s loop | Shows AI is active (Order Detail view) |
| Nav badge count | Data change | Count updates instantly | — | Bound to computed PO count |
| Progress bars (nav) | Data update | Width animates to new value | 0.5s | Smooth transition |
| KPI delta badges | Data change | Values update, color reflects direction | — | No animation on value change in v1 |

---

## 10. Content + Copy

### All Visible Text

#### Dashboard View
| Location | Text | Type | Capitalization | Notes |
|----------|------|------|----------------|-------|
| TopBar logo | "GRIDFORM" | Brand | ALL CAPS | Monospace-style, always visible |
| TopBar breadcrumb | "Dashboard" | Breadcrumb | Title Case | Single-level on Dashboard |
| Search placeholder | "Search..." | Placeholder | Sentence | 50% opacity |
| User name | "Matt P." | Label | Title Case | Truncated first name + initial |
| User role | "Op Manager" | Sublabel | Title Case | Shortened "Operations Manager" |
| Nav section | "OPERATIONS" | Section Header | ALL CAPS | Mono, letter-spaced |
| Nav item | "Dashboard" | Nav Label | Title Case | Active |
| Nav item | "Warehouse" | Nav Label | Title Case | — |
| Nav item | "Orders" | Nav Label | Title Case | With badge "3" |
| Nav section | "REFERENCE" | Section Header | ALL CAPS | Mono, letter-spaced |
| Nav item | "Inventory" | Nav Label | Title Case | Disabled/placeholder |
| Nav item | "Vendors" | Nav Label | Title Case | Disabled/placeholder |
| Nav item | "Contracts" | Nav Label | Title Case | Disabled/placeholder |
| Nav section | "WAREHOUSE" | Section Header | ALL CAPS | Mini-metrics header |
| Metric label | "Floor" | Label | Title Case | — |
| Metric value | "51.8%" | Data | — | Mono |
| Metric label | "Volume" | Label | Title Case | — |
| Metric value | "19.5%" | Data | — | Mono |
| Metric label | "Load" | Label | Title Case | — |
| Metric value | "118.5t" | Data | — | Mono, tonnage unit |
| KPI label | "Open POs" | Label | Title Case | SurfaceTint color |
| KPI value | "8" | Hero Number | — | DisplaySmall |
| KPI delta | "↑ 12%" | Badge | — | Green positive |
| KPI subtitle | "vs last quarter" | Subtitle | lowercase | Mono, muted |
| KPI label | "Pending Approval" | Label | Title Case | — |
| KPI value | "3" | Hero Number | — | — |
| KPI delta | "2 overdue" | Badge | lowercase | Red alert |
| KPI subtitle | "avg wait: 2.4d" | Subtitle | lowercase | Mono |
| KPI label | "Q2 Committed" | Label | Title Case | — |
| KPI value | "$2.4M" | Hero Number | — | Currency |
| KPI delta | "87%" | Badge | — | Amber neutral |
| KPI subtitle | "of quarterly budget" | Subtitle | lowercase | — |
| KPI label | "Warehouse" | Label | Title Case | — |
| KPI value | "51.0%" | Hero Number | — | — |
| KPI subtitle | "22k units · 118.5t" | Subtitle | lowercase | Mono, dual metric |
| KPI label | "Savings YTD" | Label | Title Case | — |
| KPI value | "$184K" | Hero Number | — | Currency |
| KPI delta | "↑ 23%" | Badge | — | Green positive |
| KPI subtitle | "negotiation + consolidation" | Subtitle | lowercase | — |
| Section title | "Order Pipeline" | Heading | Title Case | HeadlineSmall |
| PO number | "PO-7421" | Code | — | Mono |
| Vendor name | "Kennametal Inc." | Body | Title Case | — |
| Amount | "$48.2K" | Data | — | Mono |
| SLA | "2d 6h" | Data | lowercase | Mono |
| Status | "Pending" | Badge | Title Case | — |
| PO number | "PO-7420" | Code | — | — |
| Vendor name | "Sandvik Coromant" | Body | Title Case | — |
| Amount | "$67.8K" | Data | — | — |
| SLA | "1d 4h" | Data | lowercase | — |
| Status | "Flagged" | Badge | Title Case | — |
| PO number | "PO-7419" | Code | — | — |
| Vendor name | "Mitutoyo America" | Body | Title Case | — |
| Amount | "$89.5K" | Data | — | — |
| SLA | "3d 2h" | Data | lowercase | — |
| Status | "Review" | Badge | Title Case | — |
| PO number | "PO-7417" | Code | — | — |
| Vendor name | "Sandvik Coromant" | Body | Title Case | Duplicate vendor |
| Amount | "$54.2K" | Data | — | — |
| SLA | "12h" | Data | lowercase | — |
| Status | "Pending" | Badge | Title Case | — |
| Link | "View all →" | CTA | Sentence | Accent color, right arrow |
| Section title | "Activity" | Heading | Title Case | HeadlineSmall |
| Activity timestamp | "1h" | Timestamp | lowercase | Mono |
| Activity message | "Shipment #4481 received — Sandvik, 4 pallets at Bay 2" | Body | Sentence | — |
| Activity message | "PO-7418 auto-approved — Haas, within policy" | Body | Sentence | — |
| Activity message | "AI flagged PO-7420 — vendor ceiling exceeded +$12K" | Body | Sentence | — |
| Activity message | "Zone B3 rebalanced — 3 pallets → staging" | Body | Sentence | — |
| Activity message | "Walter Tools onboarded — initial risk: B" | Body | Sentence | — |
| Activity message | "Q2 budget alert — 87% committed" | Body | Sentence | — |
| Activity message | "PO-7415 shipped — Haas consumables, tracking #HAS-44182" | Body | Sentence | — |
| Status bar | "Connected" | Status | Title Case | — |
| Status bar | "Last sync: 2s ago" | Status | Sentence | — |
| Status bar | "Org: Precision Tools Midwest" | Status | Title Case | — |
| Status bar | "7 orders · 229 warehouse units · 118.5t" | Summary | lowercase | — |

### Tone
**Operational and terse**. Copy uses short phrases with em-dash separators in activity messages. Technical language (PO numbers, SLA durations, vendor names, risk grades) is presented without explanation — assumes domain expertise. No marketing language, no casual tone. This is appropriate for an 8hr/day ops tool used by procurement professionals.

### Copy Quality Audit
- [x] Button labels are descriptive actions — "View all →" is descriptive. Approve/Reject/Escalate (from INTERACTION-SPEC) are action-specific
- [x] Labels use positive framing — "Open POs" (neutral), "Pending Approval" (neutral/descriptive)
- [x] Placeholder text at reduced opacity — Search placeholder uses `#706D64` (~45% relative luminance)
- [x] ALL CAPS not used for full sentences — only section headers and table headers
- [ ] Error messages near the relevant element — **Not visible** in screenshots (error states not shown)

### Placeholder/Dynamic Content
| Text | Type | Data Source |
|------|------|-------------|
| KPI values (8, 3, $2.4M, 51.0%, $184K) | Dynamic | `DashboardModel` feeds |
| KPI deltas (↑12%, 2 overdue, 87%, ↑23%) | Dynamic | Computed from feed data |
| PO numbers, vendors, amounts, SLA times | Dynamic | `IProcurementService` |
| Activity messages | Dynamic | `IActivityService` |
| Status bar counts | Dynamic | Computed aggregates |
| Nav badge count (3) | Dynamic | Pending + flagged PO count |
| Mini-metrics percentages | Dynamic | `IWarehouseService.GetMetrics` |
| User name "Matt P." | Semi-static | User context / auth |
| Org name "Precision Tools Midwest" | Semi-static | Configuration |

### Truncation Rules
- **Vendor names**: Should truncate with ellipsis if exceeding column width. Tooltip on hover showing full name
- **Activity messages**: Single line, truncate with ellipsis if exceeding panel width
- **KPI values**: Numbers should never truncate — container must accommodate (consider "$2.4M" vs "$12.4M")
- **PO numbers**: Fixed format (PO-XXXX), no truncation needed
- **User name**: Truncated format already ("Matt P." not "Matt Patterson")

---

## 11. Data + Logic

### Data Fields
| Field | Type | Source | Editable | Validation | Input Size |
|-------|------|--------|----------|------------|------------|
| PO Number | string (PO-XXXX) | System-generated | No | Format: PO-\d{4} | — |
| Vendor Name | string | Master data | No (on PO) | Required | — |
| Amount | currency | Line item sum | No (computed) | >=0 | — |
| Status | enum | Workflow state | Via actions (approve/reject) | Valid transitions | — |
| SLA Time | duration | Computed (now - created) | No | — | — |
| AI Flag | boolean + message | AI analysis | No | — | — |
| Warehouse Floor % | percentage | Computed from voxel grid | No | 0-100 | — |
| Warehouse Volume % | percentage | Computed from voxel grid | No | 0-100 | — |
| Warehouse Load | tonnage | Computed from voxel grid | No | >=0 | — |

### Sorting + Filtering
- **Dashboard pipeline**: Sorted by SLA urgency (most urgent first) — implicit from display order
- **Orders table**: Sortable columns (click header). Default sort: SLA ascending (most urgent first)
- **Activity feed**: Reverse chronological (newest first)

### Computed/Derived Values
- **KPI deltas**: Computed by comparing current period vs previous period
- **Nav badge count**: `pendingCount + flaggedCount`
- **SLA time display**: `now - po.createdAt`, formatted as "Xd Xh"
- **Warehouse metrics**: Computed from voxel grid density
- **Pipeline bar proportions**: `statusCount / totalOrders` per segment
- **"2 overdue"**: Count of POs where SLA exceeds policy threshold

### Pagination / Infinite Scroll
- **Dashboard pipeline**: Shows top 4, "View all →" navigates to full list
- **Orders table**: Pagination expected (per rules: "use pagination, NOT infinite scroll for tables")
- **Activity feed**: Scrollable list, likely with a reasonable limit (20-50 items)

---

## 12. Accessibility

### Keyboard Navigation
Per INTERACTION-SPEC 3.1:
- `Ctrl+K` / `Cmd+K`: Toggle command palette
- `Escape`: Layered dismiss (palette → notification → detail → no-op)
- `1` / `2` / `3`: Navigate to Dashboard / Warehouse / Orders (when no TextBox focused)
- `Q`: Cycle asset type (Warehouse)
- `B` / `Z` / `X`: Build / Zone / Erase mode (Warehouse)
- `W` / `S` / `↑` / `↓`: Layer up/down (Warehouse)

### Focus Order
TopBar → Left Nav → Main Content → StatusBar
Within main: toolbar → content → action bar
Tab order follows visual layout (left-to-right, top-to-bottom)

### Screen Reader
- `AutomationProperties.Name` on all interactive elements
- `x:Uid` for localization support
- `AutomationProperties.LiveSetting="Polite"` on toast, notification count, SLA badge
- Canvas: `AutomationProperties.Name="Warehouse floor plan. Use Q to cycle asset, B for build mode, Z for zone mode, arrow keys for layer"`

### Contrast + Sizing
See Section 8 Contrast Audit. Key failures:
- `SurfaceTintColor` (#706D64) fails WCAG AA on both background colors
- `OnSurfaceDisabledColor` (#3E3C38) fails but is intentional for disabled state
- Status bar text fails but is intentional for ambient/non-critical info

### Touch Targets
| Element | Current Size | Min Required | Pass/Fail |
|---------|-------------|-------------|-----------|
| Nav items | ~36px height, 200px width | 40px desktop | Fail (height) |
| Table rows | ~44px height, full width | 40px desktop | Pass |
| KPI cards | ~95px height, ~200px width | 40px desktop | Pass |
| Toolbar buttons | ~32px height | 40px desktop | **Fail** |
| Avatar | 32px circle | 40px desktop | **Fail** |
| Bell icon | ~18px icon, ~32px target | 40px desktop | **Fail** |
| Search box | 32px height | 40px desktop | **Fail** |
| Nav badge | ~18x16px | — | N/A (not independently clickable) |
| Layer scrubber buttons | ~24x24px | 40px desktop | **Fail** |
| Pipeline order rows | ~44px height | 40px desktop | Pass |
| Status bar | 28px height | — | N/A (read-only) |

**Critical Finding**: Multiple interactive elements fall below the 40px desktop minimum touch target. The most critical are toolbar buttons (32px) and the notification bell (~32px). While these are usable with a mouse, they should have expanded hit areas via padding/margin.

**Recommendation**: Add `MinHeight="40"` to all interactive elements or use invisible hit-target expanders. The ARCHITECTURE.md already specifies `MinHeight="44"` for all buttons — ensure this is implemented even when visual size is smaller.

---

## 13. Implementation Notes

### Recommended Patterns
- **Shell**: Single `Grid` with `Region.Attached` for view switching (per ARCHITECTURE.md)
- **KPI Row**: `ItemsRepeater` with `UniformGridLayout` for equal-width cards
- **Pipeline/Activity**: 2-column `Grid` with proportional star sizing (~1.2* : 1*)
- **Orders Table**: `ItemsRepeater` with `StackLayout`, custom row template with `Grid` columns
- **Status Badges**: Custom `ContentControl` with `TemplateSelector` based on status enum
- **Progress Bars**: Custom `ProgressBar` with lightweight styling overrides
- **Animations**: `Storyboard` + `DoubleAnimation` on `CompositeTransform.TranslateY` and `Opacity` with `BeginTime` for stagger
- **Warehouse Canvas**: `SKXamlCanvas` (Windows) / `SKCanvasElement` (Skia targets)

### Design Tokens to Extract
```
// ─── Colors (Surface Hierarchy) ───
--color-background: #0F0F0D
--color-surface: #161614
--color-surface-container: #1C1B18
--color-surface-container-high: #242320
--color-surface-container-highest: #2A2926
--color-outline: #282724
--color-outline-variant: #363431

// ─── Colors (Chromatic) ───
--color-primary: #5FB89E
--color-primary-container: #1A2E28
--color-on-primary: #0F0F0D
--color-secondary: #D4956A
--color-secondary-container: #2B1F16
--color-tertiary: #6B9FC8
--color-error: #C75B5B
--color-warning: #D4A64E

// ─── Colors (Text Hierarchy) ───
--color-on-surface: #EAE7DF
--color-on-surface-variant: #A8A49A
--color-surface-tint: #706D64
--color-on-surface-disabled: #3E3C38

// ─── Status Semantic ───
--color-status-pending: #D4A64E
--color-status-pending-dim: #1F1C14
--color-status-approved: #5FB89E
--color-status-approved-dim: #141F1C
--color-status-review: #6B9FC8
--color-status-review-dim: #14191F
--color-status-flagged: #C75B5B
--color-status-flagged-dim: #1F1416

// ─── Spacing (4px base) ───
--spacing-unit: 4px
--spacing-xxs: 4px    // 1 unit
--spacing-xs: 6px     // 1.5 units
--spacing-sm: 8px     // 2 units
--spacing-md: 10px    // 2.5 units
--spacing-base: 12px  // 3 units
--spacing-lg: 14px    // 3.5 units
--spacing-xl: 16px    // 4 units
--spacing-2xl: 18px   // 4.5 units
--spacing-3xl: 24px   // 6 units
--spacing-4xl: 32px   // 8 units

// ─── Typography ───
--font-family-display: "Outfit", "Segoe UI Variable", system-ui
--font-family-mono: "JetBrains Mono", "Cascadia Code", "SF Mono", Consolas
--font-size-display-sm: 26px
--font-size-headline-sm: 17px
--font-size-title-md: 14px
--font-size-title-sm: 13px
--font-size-body-lg: 12px
--font-size-body-md: 11.5px
--font-size-body-sm: 11px
--font-size-label-lg: 11px
--font-size-label-md: 10px
--font-size-label-sm: 9px

// ─── Opacity ───
--opacity-primary-text: 1.0
--opacity-secondary-text: 0.67   // relative luminance ratio
--opacity-tertiary-text: 0.45
--opacity-disabled: 0.25
--opacity-status-bg-pending: 0.10
--opacity-status-bg-approved: 0.08
--opacity-status-bg-flagged: 0.08
--opacity-status-bg-review: 0.10
--opacity-delta-bg: 0.12
--opacity-ghost-cursor: 0.18
--opacity-divider: 0.12
--opacity-hover-fill: 0.05
--opacity-pressed-fill: 0.10

// ─── Shadows ───
--shadow-level-0: none               // Cards, panels (flat aesthetic)
--shadow-level-1: ThemeShadow Z=24   // Toast
--shadow-level-2: ThemeShadow Z=32   // Notification dropdown
--shadow-level-3: ThemeShadow Z=48   // Command palette

// ─── Radii ───
--radius-xs: 2px    // Layer indicator bars
--radius-sm: 4px    // Badges
--radius-md: 6px    // Buttons, nav items
--radius-lg: 10px   // Cards, panels
--radius-xl: 12px   // Command palette
--radius-full: 50%  // Avatar

// ─── Component Sizes ───
--topbar-height: 48px
--statusbar-height: 28px
--nav-width: 200px
--button-height: 32px
--button-padding-v: 6px
--button-padding-h: 12px
--kpi-card-height: ~95px
--kpi-card-padding: 16px 18px
--table-row-height: 44px
--table-header-height: 32px
--nav-item-height: 36px
--badge-padding: 2px 8px
--toolbar-height: 44px
--toolbar-button-height: 32px
--progress-bar-height: 3px
--pipeline-bar-height: 8px
--status-dot-size: 5px
--activity-dot-size: 8px
--avatar-size: 32px
--connection-dot-size: 6px
--unread-dot-size: 7px

// ─── Animation ───
--duration-hover: 150ms
--duration-press: 50ms
--duration-fade-up: 400ms
--duration-fade-in: 200ms-300ms
--duration-toast: 250ms
--duration-cmd-palette: 150ms
--duration-pulse: 2500ms
--duration-metric-bar: 500ms
--easing-default: ease
--easing-metric-bar: cubic-bezier(0.16, 1, 0.3, 1)
--stagger-kpi: 70ms
--stagger-pipeline-card: 50ms
--stagger-activity: 50ms
--stagger-table-row: 30ms
```

### Edge Cases
- **Long vendor names**: "Sandvik Coromant Manufacturing Division" → truncate with ellipsis at container width
- **Large currency values**: "$12.4M" vs "$2.4M" — KPI card width must accommodate 6+ characters
- **Zero orders**: Empty pipeline, empty table → empty state template
- **All approved**: Pipeline bar fully green, no pending/flagged → hide urgency indicators
- **SLA at 0**: "0h" display or "Due now" label
- **100+ activity items**: Virtualized list with scroll, possibly paginated
- **Offline**: Status bar dot red, "Disconnected", stale data warning
- **Long PO list (100+ rows)**: `ItemsRepeater` virtualization handles this
- **Multiple concurrent toasts**: Single toast only — new replaces current

### Dev Gotchas
- **Font loading**: Outfit (variable) and JetBrains Mono must be bundled in `Assets/Fonts/`. Variable font support varies by platform
- **NavigationView styling**: Warm dark palette will conflict with Material defaults. Must fully override lightweight styling resources — test early (noted as risk in ARCHITECTURE.md)
- **SkiaSharp canvas on WASM**: Profile with 500+ voxels. Use `SKCanvasElement` on Skia targets for hardware acceleration
- **Scroll wheel on canvas**: Must `preventDefault` to avoid page scroll on WASM
- **Right-click on canvas**: Must suppress browser context menu on WASM
- **z-index**: Command palette and notification flyout must render above all content including canvas
- **Status bar safe area**: Not a concern on desktop but relevant if mobile support is added
- **Fractional font sizes**: 11.5px `BodyMedium` may render with sub-pixel artifacts on some platforms. Consider rounding
- **Letter-spacing in WinUI**: `CharacterSpacing` property uses 1/1000 em units, so `+40` in XAML = +0.04em, `+80` = +0.08em
- **Pipeline bar flex segments**: `ItemsRepeater` or manual `Grid` with proportional star columns. Segments with very small counts may collapse to <1px — set `MinWidth="2"`

---

## 14. Design Quality Audit

### Scorecard
| Category | Rating | Key Findings |
|----------|--------|-------------|
| Color Harmony | Good | Split-complementary palette with desaturated tones. Warm industrial aesthetic is cohesive |
| Color Contrast (WCAG) | Fair | Primary and secondary text pass. Tertiary text (`#706D64`) fails AA on all backgrounds |
| Color Weight System | Fair | Implicit weight through hex values rather than formal 100-800 scale. Functional but not systematic |
| Typography Scale | Good | 10 defined styles with clear hierarchy. Not strictly modular ratio but pragmatically compressed for density |
| Typography Hierarchy | Good | Size + weight + font-family (sans vs mono) + color all contribute. Clear distinction between headings, body, data, metadata |
| Spacing Consistency | Good | 4px-based system consistently applied. Some 1.5x values (6, 10, 14, 18) are non-standard but intentional |
| Component Standards | Good | StatusBadges follow dot+text pattern. KPI cards have proper hierarchy. Pipeline bar is proportional |
| Visual Hierarchy | Good | F-pattern supported — KPIs at top for scanning, pipeline left (primary), activity right (secondary) |
| Button Hierarchy | Good | Primary actions (Approve) are accent-colored. Secondary (toolbar defaults) are muted. Destructive (Erase) is red-tinted |
| Icon Consistency | Fair | Using Unicode symbols in prototype — plan calls for Segoe Fluent Icons with Material fallback. Must verify consistency in implementation |
| Negative Space | Fair | Intentionally dense — "whitespace is earned." Content-to-space ratio ~75-80% is above typical 60-70% but appropriate for ops tool |
| Alignment | Good | Elements align within their containers. KPI cards are uniform width. Table columns align |
| Opacity Usage | Good | Status backgrounds at 8-10%, delta badges at 12%, ghost cursor at 18% — all follow rules |
| Gradient Quality | Good | No decorative gradients — flat aesthetic is intentional and consistent |
| Spacing Relationships | Good | Card padding (16px) : row gap (8px) = 2:1 ratio maintained. Section gaps consistent at 14px |
| Layout Proportions | Good | Nav:Content ~1:5. Pipeline:Activity ~1.2:1. Dense but balanced |
| Interaction State Coverage | Fair | States specified in INTERACTION-SPEC but not visible in screenshots. Cannot verify implementation |
| Accessibility | Fair | Keyboard shortcuts well-defined. Touch targets undersized. Contrast failures on tertiary text |

### Violations Found
| # | Rule Violated | Element | Severity | Recommendation |
|---|---------------|---------|----------|----------------|
| 1 | Contrast minimum (4.5:1) | Tertiary text `#706D64` on `#0F0F0D` (~3.8:1) and `#1C1B18` (~3.2:1) | **Major** | Increase to `#8A877D` (~4.7:1 on SurfaceContainer) or accept as documented WCAG exception |
| 2 | Touch targets (40px desktop) | Toolbar buttons at ~32px height | Major | Add `MinHeight="40"` or expand hit area with transparent padding |
| 3 | Touch targets (40px desktop) | Notification bell at ~32px target | Major | Expand clickable area to 40x40px minimum |
| 4 | Touch targets (40px desktop) | Search trigger at 32px height | Minor | Increase to 40px height |
| 5 | Touch targets (40px desktop) | Layer scrubber buttons at ~24px | Major | Expand to 40x40px hit area |
| 6 | Touch targets (40px desktop) | Nav items at ~36px height | Minor | Increase to 40px (`MinHeight`) |
| 7 | Typography: line-height headers | HeadlineSmall at 1.41 line-height | Minor | Reduce to ~1.24-1.29 (21-22px for 17px) to match header rules |
| 8 | Typography: fractional size | BodyMedium at 11.5px | Minor | Round to 12px or 11px for cleaner rendering |
| 9 | Spacing: border-radius inconsistency | Cards use 10px, buttons 6px, badges 4px — not on strict 4px grid | Minor | Acceptable — radius scale is independent of spacing scale. Document as intentional |
| 10 | Content-to-whitespace ratio | ~75-80% content vs recommended 60-70% | Minor | Intentional ("density is a feature") — document as accepted exception |
| 11 | Button padding ratio | Toolbar buttons use 6px/12px (1:2 correct) but height is 32px vs recommended min 40px | Minor | Already covered by touch target violation |
| 12 | Icon sizing rule | Icons should match accompanying text size. Nav icons at ~16px with ~11px text | Minor | 16px icons with 11px text creates a 1.45:1 ratio. Rule suggests 1:1. Consider 14px icons or accept as visual balance choice |
| 13 | States not shown | Loading, empty, error states not in screenshots | Minor | Already specified in INTERACTION-SPEC but need visual mockups to verify implementation |

### What's Working Well
1. **Color palette cohesion**: The warm industrial dark theme is distinctive, professional, and internally consistent. The teal/copper/blue triad avoids the "generic dark dashboard" trap
2. **Data hierarchy**: Mono font for numbers/codes + sans-serif for labels creates instant visual parsing of data vs. metadata
3. **Status system**: Four-status model (Pending/Approved/Review/Flagged) with consistent dot+text+dim-background treatment across all views
4. **Information density**: KPI row → pipeline → activity gives progressive depth. User gets summary → detail → timeline in a single viewport
5. **Semantic color usage**: Every color has a clear meaning. No decorative color. Red = danger, green = success, amber = pending, blue = info — no mismatches
6. **Flat elevation model**: Border-defined surfaces with no shadows on cards/panels. Shadows reserved for overlays (toast, command palette, notifications). Clean and intentional
7. **Navigation persistence**: Mini-metrics in nav footer provide constant warehouse awareness regardless of active view
8. **Typography pairing**: Outfit (geometric sans) + JetBrains Mono (coding mono) is a distinctive combination that reinforces the "precision industrial" identity
9. **Status bar design**: Ambient information (connection, sync, org) at minimum visual weight. Doesn't compete with primary content
10. **Pipeline bar**: Stacked proportional visualization gives instant order-status distribution without requiring a separate chart
11. **Activity feed design**: Colored dots + timestamps + em-dash structured messages are scannable and information-dense
12. **Consistent border treatment**: 1px `#282724` borders on all cards/panels, brightening to `#363431` on hover — simple, predictable interaction model

---

## 15. Open Questions

| # | Question | Area | Priority | Default Assumption |
|---|----------|------|----------|--------------------|
| 1 | Should `SurfaceTintColor` be lightened to pass WCAG AA? | Accessibility | **High** | Accept current value with documented exception — ops users prioritize density over strict contrast |
| 2 | Are touch targets intentionally undersized for mouse-first desktop? | Accessibility | **High** | Yes — but add invisible hit-area expanders at 40px minimum |
| 3 | What happens when a KPI value is unusually large (e.g., "$12.4M" or "128")? | Layout | Medium | Card width flexes, text truncates at container edge |
| 4 | Is the "View all" link intended to be the only entry to Orders from Dashboard? | Navigation | Medium | No — nav item and keyboard shortcut "3" also work |
| 5 | Should activity items be interactive (clickable to navigate to source)? | Interaction | Medium | Not in v1 (INTERACTION-SPEC says "read-only"), but should be planned for v2 |
| 6 | What determines the bottom accent strip color on KPI cards? | Design | Medium | Mapped to semantic meaning: green = healthy, copper = attention needed, amber = warning threshold |
| 7 | Is the command palette icon the grid-like symbol visible in the topbar? | Component | Low | Yes — appears as a small grid/squares icon, triggers Ctrl+K |
| 8 | Are the REFERENCE nav items (Inventory, Vendors, Contracts) clickable to a "Coming Soon" page? | Navigation | Low | Yes — routes map to `ComingSoonPage` per ARCHITECTURE.md |
| 9 | Should the pipeline bar segments have gaps or be seamless? | Design | Low | ~2px gaps between segments for visual separation |
| 10 | Is light mode planned? | Theming | Low | No — "Precision Industrial" dark theme is the only planned mode |
| 11 | How should the Warehouse canvas degrade on low-end hardware or small WASM allocations? | Performance | Medium | Reduce grid size, disable fog/AO, fall back to 2D top-down view |
| 12 | Should the Orders table support column resizing or reordering? | Interaction | Low | No in v1 — fixed column layout |
| 13 | Is there a notification badge threshold (e.g., show "9+" instead of "12")? | Component | Low | Cap display at "9+" for single-digit badge width |
| 14 | What is the SLA policy threshold that triggers "overdue" / red SLA text? | Business Logic | Medium | Not defined in screenshots — needs product owner input |
| 15 | Should the mini-metrics (Floor/Volume/Load) animate on first render or data change? | Animation | Low | Yes — `metricBar` animation (0.5s, cubic-bezier) per INTERACTION-SPEC |

---

## Appendix A: Pixel-Level Measurement Reference

### Element Dimensions
| Element | Width | Height | Aspect Ratio |
|---------|-------|--------|-------------|
| Full viewport | ~1280px | ~900px | ~1.42:1 |
| TopBar | 1280px (full) | 48px | — |
| StatusBar | 1280px (full) | 28px | — |
| Left Nav | 200px | ~824px (viewport - topbar - statusbar) | — |
| Main Content Area | ~1080px | ~824px | ~1.31:1 |
| KPI Card (each) | ~200px (flex, ~1/5 of content minus gaps) | ~95px | ~2.1:1 |
| Order Pipeline Panel | ~580px (~55% content) | ~500px | ~1.16:1 |
| Activity Panel | ~480px (~45% content) | ~500px | ~0.96:1 |
| Pipeline Order Row | ~540px (panel width - padding) | ~40px | ~13.5:1 |
| Pipeline Status Bar | ~540px | ~8px | ~67.5:1 |
| Activity Item | ~440px | ~36px | ~12.2:1 |
| Table Row (Orders) | ~1040px (content - padding) | ~44px | ~23.6:1 |
| Table Header | ~1040px | ~32px | ~32.5:1 |
| Pipeline Summary Card (Orders) | ~250px (flex, 1/4) | ~60px | ~4.2:1 |
| Toolbar (Warehouse) | ~1040px | ~44px | ~23.6:1 |
| Toolbar Button | ~32-60px (varies) | ~32px | variable |
| Logo Circle | 28px | 28px | 1:1 |
| Avatar Circle | 32px | 32px | 1:1 |
| Search Box | ~200px | 32px | ~6.25:1 |
| Nav Item | 200px (full nav width) | ~36px | ~5.6:1 |
| NavBadge | ~18px | ~16px | ~1.1:1 |
| StatusBadge | ~70-80px (varies by text) | ~18px | ~4:1 |
| DeltaBadge | ~50-60px (varies) | ~18px | ~3:1 |
| Mini-Metric Progress Bar | ~100px | 3px | ~33:1 |
| Connection Dot (StatusBar) | 6px | 6px | 1:1 |
| Unread Dot (Bell) | 7px | 7px | 1:1 |
| Status Dot (Badge) | 5px | 5px | 1:1 |
| Activity Dot | 8px | 8px | 1:1 |
| Layer Scrubber | ~28px | ~400px (canvas height) | ~0.07:1 |
| Layer Scrubber Button (▲/▼) | ~24px | ~24px | 1:1 |
| Layer Indicator Bar | ~3px | ~16px | ~0.19:1 |
| Isometric Canvas | ~1040px | ~740px (main content minus toolbar) | ~1.4:1 |

### Padding & Margin Map
| Element | Padding (T/R/B/L) | Margin (T/R/B/L) | Border Width | Border Radius |
|---------|-------------------|-------------------|-------------|---------------|
| TopBar | 0/16/0/16 | 0/0/0/0 | 0/0/1/0 (bottom) | 0 |
| StatusBar | 0/16/0/16 | 0/0/0/0 | 1/0/0/0 (top) | 0 |
| Left Nav | 8/0/8/0 | 0/0/0/0 | 0/1/0/0 (right) | 0 |
| Main Content | 16/16/16/16 | 0/0/0/0 | 0 | 0 |
| KPI Card | 16/18/16/18 | 0/0/0/0 | 1px | 10px |
| Pipeline Panel | 16/16/16/16 | 0/0/0/0 | 1px | 10px |
| Activity Panel | 16/16/16/16 | 0/0/0/0 | 1px | 10px |
| Pipeline Order Row | 10/12/10/12 | 0/0/0/0 | 1px | 6px |
| StatusBadge | 2/8/2/8 | 0/0/0/0 | 0 | 4px |
| DeltaBadge | 2/8/2/8 | 0/0/0/0 | 0 | 4px |
| NavItem | 8/12/8/12 | 0/0/2/0 | 0 | 6px |
| NavBadge | 2/6/2/6 | 0/0/0/0 | 0 | 8px (pill) |
| Search Box | 6/12/6/12 | 0/0/0/0 | 1px | 6px |
| Table Header | 10/12/10/12 | 0/0/0/0 | 0/0/1/0 (bottom) | 0 |
| Table Row | 10/12/10/12 | 0/0/0/0 | 0/0/1/0 (bottom) | 0 |
| Toolbar | 6/8/6/8 | 0/0/0/0 | 1px | 10px |
| Toolbar Button | 4/10/4/10 | 0/0/0/0 | 1px | 6px |
| Avatar | 0/0/0/0 | 0/0/0/8 | 0 | 50% |

### Gap & Spacing Map
| Between Elements | Gap (px) | Direction | Base Unit Multiple |
|-----------------|----------|-----------|-------------------|
| TopBar → Main Content | 0 (flush) | Vertical | 0 |
| KPI card → KPI card | 10px | Horizontal | 2.5x |
| KPI row → Panels row | ~14px | Vertical | 3.5x |
| Pipeline panel → Activity panel | 14px | Horizontal | 3.5x |
| Panel title → Pipeline bar | ~12px | Vertical | 3x |
| Pipeline bar → First PO row | ~16px | Vertical | 4x |
| PO row → PO row | 8px | Vertical | 2x |
| Last PO row → "View all" | ~12px | Vertical | 3x |
| Activity dot → Activity message | ~10px | Horizontal | 2.5x |
| Timestamp → Activity dot | ~12px | Horizontal | 3x |
| Activity item → Activity item | ~8px | Vertical | 2x |
| KPI label → KPI value | ~4px | Vertical | 1x |
| KPI value → KPI subtitle | ~6px | Vertical | 1.5x |
| DeltaBadge → KPI value | ~8px | Horizontal | 2x |
| Nav section header → First item | ~8px | Vertical | 2x |
| Nav item → Nav item | ~2px | Vertical | 0.5x |
| Nav OPERATIONS → REFERENCE | ~16px | Vertical | 4x |
| Nav icon → Nav text | ~10px | Horizontal | 2.5x |
| Logo circle → "GRIDFORM" text | ~8px | Horizontal | 2x |
| "GRIDFORM" → Breadcrumb | ~16px | Horizontal | 4x |
| Search → CmdPalette icon | ~8px | Horizontal | 2x |
| CmdPalette → Bell | ~8px | Horizontal | 2x |
| Bell → Avatar | ~12px | Horizontal | 3x |
| Avatar → Username | ~8px | Horizontal | 2x |
| Table header → First row | 0 (flush) | Vertical | 0 |
| Table row → Table row | 0 (flush, border separates) | Vertical | 0 |
| Pipeline summary cards (Orders) | ~10px | Horizontal | 2.5x |
| Summary cards → Table | ~14px | Vertical | 3.5x |
| Toolbar → Canvas | 0 (flush) | Vertical | 0 |
| Mini-metric row → row | ~6px | Vertical | 1.5x |
| Mini-metric label → progress bar | ~4px | Horizontal | 1x |
| Mini-metric progress bar → value | ~4px | Horizontal | 1x |

### Shadow Specifications
| Element | X | Y | Blur | Spread | Color | Opacity | Notes |
|---------|---|---|------|--------|-------|---------|-------|
| Cards/Panels | — | — | — | — | — | — | No shadow (flat aesthetic) |
| Toast | ThemeShadow | — | — | — | — | — | Z=24 translation |
| Notification Flyout | ThemeShadow | — | — | — | — | — | Z=32 translation |
| Command Palette | ThemeShadow | — | — | — | — | — | Z=48 translation |

**Note**: GRIDFORM uses WinUI `ThemeShadow` with `Translation` Z values rather than explicit x/y/blur/spread shadow definitions. This is platform-appropriate for WinUI 3 / Uno Platform. The framework renders depth-appropriate shadows automatically based on Z translation.

---

*End of Design Specification. This document should be read alongside `DESIGN-BRIEF.md` (visual system), `ARCHITECTURE.md` (technical implementation), and `INTERACTION-SPEC.md` (behavior + animation) for complete implementation coverage.*
