# GRIDFORM — Interaction Brief
## Complete Interaction & Animation Specification
### Industrial Tooling Distribution & Warehouse Spatial Planning

---


## 3.1 Global Interactions

### Command Palette (⌘K)

| Attribute | Specification |
|---|---|
| **Trigger** | `Ctrl+K` / `⌘K` keyboard accelerator, OR click search trigger in topbar |
| **Appearance** | Modal overlay with `0.5` opacity backdrop + `blur(6px)`. Content dialog centered at `16vh` from top |
| **Focus** | Auto-focus search `TextBox` on open. `Escape` closes |
| **Results** | Categorized list (Search / Action / Navigate). Static in v1 — no live filtering |
| **Selection** | Click result → fire toast + execute action. Mouse hover → accent background on result item |
| **Dismissal** | Click backdrop, press Escape, or select a result |
| **Animation** | `fadeUp` entrance: `translateY(-10px)` → `translateY(0)`, `opacity 0→1`, `0.15s ease` |
| **Uno control** | `ContentDialog` with custom template, `FullSizeDesired="False"`. Or `Popup` with manual backdrop `Grid` |

### Notification Dropdown

| Attribute | Specification |
|---|---|
| **Trigger** | Click notification bell `Button` in topbar |
| **Appearance** | `Flyout` anchored to bell button, right-aligned. 340px width, max 320px scroll height |
| **Unread indicator** | Red dot (7×7) on bell when any notification has `read: false`. Dot has 2px border matching surface color |
| **Per-item states** | Unread: `AccentDim` background. Read: transparent background |
| **Actions** | "Mark all read" link in header (no handler in v1). Click individual item (no navigation in v1) |
| **Dismissal** | Click outside flyout, or re-click bell |
| **Uno control** | `Flyout` with `FlyoutPresenterStyle` override for custom sizing + dark background |

### Toast Notifications

| Attribute | Specification |
|---|---|
| **Trigger** | Any action completion (approve, reject, escalate, preset load, bulk approve, export) |
| **Position** | Bottom center, 40px from bottom edge |
| **Duration** | 2.4 seconds auto-dismiss |
| **Appearance** | `OnSurface` background, `Background` text. 10px radius. `ThemeShadow` elevation |
| **Animation** | `fadeUp`: `translateY(8px)` → `translateY(0)`, `0.25s ease` |
| **Queue** | Single toast only. New toast replaces current (timer resets) |
| **Uno control** | Custom `Border` in overlay `Grid` with `DispatcherTimer`. Or `InfoBar` positioned absolutely |

### Keyboard Shortcuts

| Shortcut | Scope | Action | Implementation |
|---|---|---|---|
| `Ctrl+K` / `⌘K` | Global | Toggle command palette | `KeyboardAccelerator` on Shell |
| `Escape` | Global (layered) | Close: command palette → notification → detail view → no-op | Priority chain in Shell code-behind |
| `1` | Global | Navigate to Dashboard | `KeyboardAccelerator` (only when no `TextBox` focused) |
| `2` | Global | Navigate to Warehouse | Same |
| `3` | Global | Navigate to Orders | Same |
| `Q` | Warehouse view | Cycle asset type: Pallet → Rack → Container → Equipment → Aisle → Pallet | Registered on `WarehousePage` |
| `B` | Warehouse view | Switch to Build mode | Same |
| `Z` | Warehouse view | Switch to Zone mode | Same |
| `X` | Warehouse view | Toggle Erase tool on/off | Same |
| `W` / `↑` | Warehouse view | Layer up (+1, clamped to MAX_Y) | Same |
| `S` / `↓` | Warehouse view | Layer down (-1, clamped to 0) | Same |

---

## 3.2 Navigation Interactions

| Element | Trigger | Response | State Change | Animation |
|---|---|---|---|---|
| Nav item (Dashboard) | Click | Content swaps to DashboardPage. Breadcrumb: "Dashboard" | `view=dashboard`, nav active state moves, detail cleared | Instant (Visibility region swap) |
| Nav item (Warehouse) | Click | Content swaps to WarehousePage. Breadcrumb: "Operations / Warehouse Planner" | `view=warehouse`, nav active state moves | Instant |
| Nav item (Orders) | Click | Content swaps to OrdersPage (list). Breadcrumb: "Operations / Purchase Orders" | `view=orders`, detail cleared, selection cleared | Instant |
| Nav badge (Orders) | — | Shows count of pending+flagged POs (amber background) | Auto-computed from PO data | N/A |
| Breadcrumb "Operations" | Click | Navigate to Dashboard | `view=dashboard` | Instant |
| Breadcrumb "Purchase Orders" | Click | Return to Orders list (clear detail) | `detail=null` | Instant |
| Dashboard pipeline card | Click | Navigate to Orders + open PO detail | `view=orders`, `detail=po`, `detailTab=overview` | Instant |
| "View all →" button | Click | Navigate to Orders list | `view=orders`, `detail=null` | Instant |

---

## 3.3 Dashboard Interactions

| Element | Trigger | Response |
|---|---|---|
| KPI card | Hover | `cursor: pointer`, subtle border brightening |
| KPI card | Click | No action in v1 (future: drill into detail) |
| Pipeline order card | Hover | Border brightens |
| Pipeline order card | Click | Navigate to Orders detail view for that PO |
| Pipeline status bar | — | Read-only visualization (flex proportional to count per status) |
| Activity item | — | Read-only (no click action) |

### Entrance Animations

| Element | Animation | Duration | Easing | Stagger |
|---|---|---|---|---|
| KPI cards (×5) | `fadeUp` (translateY 10px → 0, opacity 0 → 1) | 0.4s | ease | +0.07s per card (0, 0.07, 0.14, 0.21, 0.28) |
| Pipeline panel | `fadeUp` | 0.4s | ease | 0.30s delay |
| Activity panel | `fadeUp` | 0.4s | ease | 0.35s delay |
| Pipeline cards (×4) | `fadeIn` (opacity 0 → 1) | 0.3s | ease | +0.05s per card (start 0.35s) |
| Activity items (×7) | `fadeIn` | 0.3s | ease | +0.05s per item (start 0.4s) |

In Uno: Use `Storyboard` + `DoubleAnimation` on `TranslateTransform.Y` and `Opacity` with `BeginTime` for stagger. Or use `EntranceThemeTransition` / `RepositionThemeTransition` on `ItemsRepeater`.

---

## 3.4 Orders List Interactions

| Element | Trigger | Response | State |
|---|---|---|---|
| Pipeline summary card (×4) | Hover | Border brightens | No functional filter in v1 |
| Row checkbox | Click (`stopPropagation`) | Toggle PO in selection set. Bulk bar shows/hides based on count | `selected` set updated |
| Row body (non-checkbox cells) | Click | Navigate to OrderDetailPage with PO data | `detail=po`, `detailTab=overview`. Breadcrumb adds PO ID |
| Bulk "Approve" button | Click | Toast: "✓ N orders approved". Selection cleared | `selected` emptied |
| Bulk "Export" button | Click | Toast: "Exported" | No state change |
| Bulk "Clear" button | Click | Selection cleared, bulk bar hides | `selected` emptied |

### Bulk Action Bar Behavior

- **Appears** when `selected.size > 0` with `fadeUp` animation (0.2s ease)
- **Disappears** instantly when selection returns to 0
- **Position** between pipeline summary and data table
- **Uno control** `Border` + `AutoLayout` with `Visibility` bound to `CountToVisibilityConverter` on selection count

### Table Row States

| State | Visual |
|---|---|
| Default (even row) | Transparent background |
| Default (odd row) | `SurfaceContainer` background |
| Hover | `SurfaceContainerHigh` background (via `PointerOver` VisualState) |
| Selected (checkbox checked) | `AccentDim` background |
| SLA critical | SLA column text in `ErrorColor` (red) with `FontWeight=Medium` |

### Entrance Animation

Table rows use staggered `fadeIn`: opacity 0 → 1, `0.2s ease`, `+0.03s` per row.

---

## 3.5 Order Detail Interactions

### Header Actions

| Button | Trigger | Response | Visible When |
|---|---|---|---|
| Approve | Click | Toast: "✓ PO-XXXX approved". Navigate back to orders list (`detail=null`) | `status` = pending, flagged, or review |
| Reject | Click | Toast: "✕ PO-XXXX rejected". Navigate back to list | Same |
| Escalate | Click | Toast: "↑ Escalated". Stay on detail (no navigation) | Same |
| — | `Escape` key | Navigate back to orders list | Always |

### Tab Bar

| Tab | Trigger | Response |
|---|---|---|
| Overview | Click | Show: AI analysis card + order details grid + vendor card |
| Items | Click | Show: Line item table (SKU, desc, qty, unit, total) with footer sum |
| Approvals | Click | Show: Vertical step chain visualization |
| History | Click | Show: Timestamped audit log |

Active tab: accent underline (2px bottom border). Inactive: transparent. Text: accent (active) vs tertiary (inactive). Transition: instant content swap, no animation.

### Tab Content Specifics

**Overview Tab:**
- AI analysis card: accent-dim background, 1px accent border, pulsing green dot (animation: `pulse` 2.5s ease infinite). If `aiNote` is null, card is hidden
- Order details grid: 2 columns. Left: line items summary, ship date, terms, amount. Right: vendor name, region, risk, on-time %, quality grade, YTD orders, contract status

**Items Tab:**
- Full-width table: SKU (mono, accent), Description, Qty (right-aligned, mono), Unit Price (right-aligned, mono), Total (right-aligned, mono, bold)
- Footer row: "Total" label + bold sum amount
- Alternating row backgrounds

**Approvals Tab:**
- Max width 500px
- Vertical stepper with connecting line segments between steps
- Done step: green circle with ✓ checkmark, green connecting line below
- Current step: amber circle with step number, "CURRENT" badge (amber background, mono 9px), amber text
- Waiting step: gray circle with step number, "Waiting" text in muted color
- Connecting line: 2px wide, colored (green) for done connections, default border color for pending

**History Tab:**
- Simple timestamped list. Each entry: timestamp (mono, muted, 48px min-width) + message text
- Separated by 1px border

---

## 3.6 Warehouse Planner Interactions

### Toolbar

| Element | Trigger | Response |
|---|---|---|
| Build button | Click or `B` key | Mode → build. Asset palette visible, zone palette hidden. Button gets accent active style |
| Zone button | Click or `Z` key | Mode → zone. Zone palette visible, asset palette hidden. Button gets accent active style |
| Erase button | Click or `X` key | Toggle erase tool. When active: red tint, red border. When inactive: default |
| Asset palette item | Click or `Q` (cycle) | Selected asset changes. Active item: colored border matching asset accent, colored background. Ghost cursor on canvas updates |
| Zone palette item | Click | Selected zone changes. Active item: colored border matching zone accent |
| Layout A button | Click | Full warehouse preset loads. Voxels + zones replaced. Toast: "Layout loaded". Metrics recompute |
| Layout B button | Click | Staging preset loads. Same behavior |
| Clear button | Click | Grid cleared. Toast: "Cleared". Metrics reset to 0 |

### Canvas Interactions

| Input | Context | Action |
|---|---|---|
| Mouse move | Always | Cursor position updates (grid cell tracking via `fromIso()` projection). Ghost preview follows cursor. HUD coordinates update. Canvas invalidates |
| Left-click | Build mode + Place tool | Voxel placed at `(cursor.gx, currentLayer, cursor.gz)` with current asset type. Metrics recompute. Canvas re-renders |
| Left-click | Build mode + Erase tool | Topmost voxel at cursor cell erased. Metrics recompute |
| Left-click | Zone mode + Place tool | Zone painted at `(cursor.gx, cursor.gz)` with current zone type. Metrics recompute |
| Left-click | Zone mode + Erase tool | Zone cleared at cursor cell |
| Right-click | Any mode | Erase behavior (same as left-click with erase tool). `ContextMenu` suppressed via `e.preventDefault()` |
| Scroll wheel | Always | Active layer increments (scroll up) or decrements (scroll down). Clamped 0–6. Layer scrubber updates. Grid overlay moves to new layer height |

### Layer Scrubber

| Element | Trigger | Response |
|---|---|---|
| ▲ button | Click | Layer + 1 (clamped to MAX_Y) |
| ▼ button | Click | Layer - 1 (clamped to 0) |
| Level indicator bar | Click | Jump directly to that layer |
| — | — | Visual: active layer = accent color. Populated layers = dim white. Empty layers = near-invisible |

### Canvas Visual Feedback

| Feedback | Description |
|---|---|
| Ghost cursor | Translucent voxel at cursor position (build mode only). Uses current asset's accent color at 18% opacity |
| Zone tiles | Tinted isometric diamonds on ground plane. Color matches zone type. Rendered below voxels |
| Ground shadows | Dark diamond under elevated voxels (y > 0). Opacity scales with depth fog |
| Depth fog | Back-of-grid voxels darken to 65% brightness. Creates spatial depth reading |
| Ambient occlusion | Each voxel face darkened based on neighbor adjacency. Creates "stacking shadow" effect |
| Edge highlights | Top-face edges get bright stroke (18% white) for visual separation |
| Asset labels | Top-face center label: P, R, C, E, or · for each asset type (6px bold mono, 30% black) |
| Active layer grid | When layer > 0, additional grid overlay drawn at current layer height (25% opacity) |

---

## 3.7 Overlay Dismiss Rules

| Overlay | Open Trigger | Close Triggers | Focus Trap |
|---|---|---|---|
| Command palette | `Ctrl+K`, click search | `Escape`, click backdrop, select result | Yes — input auto-focused |
| Notification dropdown | Click bell | Click outside, re-click bell | No — standard flyout behavior |
| Toast | Action completion | Auto-dismiss (2.4s timer) | No — non-interactive |

---

## 3.8 Responsive Behavior (Future)

v1 is desktop-only (minimum 1024px width). For future responsive adaptation:

| Breakpoint | Nav | Content | Warehouse |
|---|---|---|---|
| ≥1280px | Full sidebar (200px) | All features | Full canvas |
| 900–1279px | Collapsed sidebar (icon-only, 56px) | Adapted | Full canvas |
| 600–899px | Bottom `TabBar` (Uno Toolkit) | Single column | Canvas with reduced grid (10×10) |
| <600px | Bottom `TabBar` | Full mobile | Canvas hidden — metrics-only view |

Use `VisualStateManager` with `AdaptiveTrigger` on Shell to switch between `NavigationView` (wide) and `TabBar` (narrow) following the Uno responsive shell pattern.

---

## 3.9 Animation Timing Reference

| Animation Name | Property | From | To | Duration | Easing | Used By |
|---|---|---|---|---|---|---|
| `fadeUp` | `TranslateY` + `Opacity` | `10px`, `0` | `0`, `1` | 0.4s | ease | KPI cards, panels, bulk bar |
| `fadeIn` | `Opacity` | `0` | `1` | 0.2–0.3s | ease | Table rows, pipeline cards, activity items |
| `pulse` | `Opacity` | `1` → `0.3` → `1` | — | 2.5s | ease-in-out, infinite | AI dot indicator, status bar dot |
| `buttonHover` | `Background`, `BorderBrush` | default | hover | 0.15s | ease | All buttons, nav items, chips |
| `metricBar` | `Width` | `0%` | `N%` | 0.5s | `cubic-bezier(0.16,1,0.3,1)` | Nav mini-metrics, progress bars |
| `cmdDrop` | `TranslateY` + `Opacity` + `Scale` | `-10px`, `0`, `0.98` | `0`, `1`, `1` | 0.15s | ease | Command palette |
| `toastIn` | `TranslateY` + `Opacity` | `8px`, `0` | `0`, `1` | 0.25s | ease | Toast |

In Uno: implement via `Storyboard` with `DoubleAnimation` targeting `CompositeTransform.TranslateY` and `UIElement.Opacity`. Use `BeginTime` for stagger offsets. For repeating animations (`pulse`), set `RepeatBehavior="Forever"` and `AutoReverse="True"`.

---

## 3.10 Error and Edge Case Interactions

| Scenario | Behavior |
|---|---|
| **Data loading** | `FeedView.ProgressTemplate` shows `LoadingView` (Uno Toolkit) centered in content area |
| **Data error** | `FeedView.ErrorTemplate` shows error icon + "Failed to load" message + "Retry" button bound to `FeedView.Refresh` |
| **Empty orders** | `FeedView.NoneTemplate` shows empty state: document icon + "No purchase orders" |
| **Empty warehouse** | Metrics show 0% for all values. AI brief: "Empty warehouse. Load a preset or start placing assets" |
| **Approve/reject failure** | Toast with error message. PO stays on current state. User can retry |
| **Overlapping click (checkbox + row)** | Checkbox uses `stopPropagation`. Click on checkbox toggles selection only. Click on other cells navigates to detail only |
| **Right-click on canvas (browser)** | `ContextMenu` suppressed. Erase action fires instead |
| **Scroll on canvas (browser)** | `preventDefault` on wheel event to avoid page scroll. Layer change only |

---

*End of briefs. These three documents — Architecture, Design, and Interaction — provide the complete specification needed for a designer, PM, and Uno Platform developer to build GRIDFORM without re-examining the prototype.*
