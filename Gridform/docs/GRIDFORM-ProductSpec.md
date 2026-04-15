# GRIDFORM — Complete Product Specification
## Industrial Tooling Distribution & Warehouse Spatial Planning Platform
### Prototype Analysis & Uno Platform Build Spec

---

## 1. Executive Summary

GRIDFORM is an enterprise operations platform for industrial tooling distributors — companies that warehouse and distribute CNC tooling, precision instruments, and industrial equipment to manufacturing clients.

The platform combines three operational domains into a single workspace:
1. **Operations Dashboard** — morning briefing with KPIs, pipeline visibility, and activity feed
2. **Warehouse Spatial Planner** — isometric voxel-based floor layout tool for distribution center space planning
3. **Procurement Management** — purchase order lifecycle with AI-assisted risk analysis, approval chains, and SLA tracking

The prototype demonstrates enterprise patterns including multi-step approval workflows, SLA accountability timers, bulk operations, breadcrumb navigation, notification center, command palette, and role-based context.

**Target stack:** Uno Platform (C# + WinUI 3 + XAML), targeting Windows, WebAssembly, and optionally macOS/Linux via Skia rendering.

---

## 2. Master App Inventory

### A. App-Level Structure

| Attribute | Value |
|---|---|
| **App purpose** | Operational command center for industrial tooling distribution — procurement, warehouse layout planning, and logistics oversight |
| **Primary user** | Operations Manager at a regional distribution center (role: "Ops Manager") |
| **Secondary users** | Procurement approvers, warehouse planners, finance reviewers (implied by approval chain) |
| **Main navigation model** | Persistent left sidebar (NavigationView) with icon+label items, grouped into "Operations" and "Reference" sections |
| **Information architecture** | 3 active views (Dashboard, Warehouse, Orders) + 3 placeholder views (Inventory, Vendors, Contracts) |
| **Entry point** | Dashboard view (default on load) |
| **Global shell** | Topbar (48px) + Left Nav (200px) + Main Content (fluid) + Status Bar (28px) |
| **Cross-cutting patterns** | Toast notifications, breadcrumb trail, notification dropdown, command palette overlay, mini warehouse metrics in nav footer |
| **Shared services** | Voxel engine (isometric renderer), metrics computation, PO data store, notification store, activity feed |
| **Global overlays** | Command Palette (⌘K), Notification Dropdown, Toast |

### B. Full Screen/Page/View Inventory

#### B1. Dashboard View
| Attribute | Value |
|---|---|
| **Unique name** | DashboardView |
| **Route** | `/dashboard` (default) |
| **Purpose** | Morning operations briefing — KPIs, order pipeline, activity feed |
| **Parent** | Main content area |
| **How user reaches it** | Click "Dashboard" in nav, or click "Operations" breadcrumb |
| **How user exits** | Click another nav item |
| **Sections** | KPI Strip (5 cards), Order Pipeline panel, Activity panel |
| **Components** | KpiCard ×5, StatusPipelineBar, PipelineOrderCard ×4, ActivityItem ×7, "View all" button |
| **Actions** | Click KPI card (no action defined), click pipeline card → navigate to Orders detail, click "View all" → navigate to Orders list |
| **States** | Default (loaded with data), animated entrance (staggered fadeUp) |
| **Transitions** | fadeUp animation on KPI cards (0.4s, staggered 0.07s per card), fadeIn on pipeline/activity items |
| **Dependencies** | POS data, ACTIVITY data, metrics computation (warehouse), stPipeline computed count |

#### B2. Warehouse Planner View
| Attribute | Value |
|---|---|
| **Unique name** | WarehouseView |
| **Route** | `/warehouse` |
| **Purpose** | Isometric spatial planning for distribution center floor layout |
| **Parent** | Main content area |
| **How user reaches it** | Click "Warehouse" in nav |
| **How user exits** | Click another nav item |
| **Sections** | Toolbar strip, Canvas area, Layer control (right edge), Bottom HUD (left + right) |
| **Components** | ModeButtons (Build/Zone/Erase), AssetPalette (5 asset types), ZonePalette (4 zone types), LayoutPresetButtons (A/B/Clear), IsometricCanvas, LayerScrubber (▲/▼ + level indicators), CursorInfoHUD, KeyboardHintsHUD |
| **Actions** | Switch mode (build/zone), select asset type, select zone type, click canvas to place, right-click to erase, scroll to change layer, load preset layout, clear layout |
| **States** | Build mode + Place tool (default), Build mode + Erase tool, Zone mode + Place tool, Zone mode + Erase tool. Per-layer: empty/populated indicators. Asset/zone selection active states. Ghost cursor preview |
| **Transitions** | Canvas renders at 60fps via requestAnimationFrame. No page-level transitions — instant swap |
| **Dependencies** | Voxel engine (Map-based grid), zone data (Map-based), metrics computation, preset generators |

#### B3. Orders List View
| Attribute | Value |
|---|---|
| **Unique name** | OrdersListView |
| **Route** | `/orders` |
| **Purpose** | Browse, filter, and bulk-manage purchase orders |
| **Parent** | Main content area |
| **How user reaches it** | Click "Orders" in nav, or click "View all →" from dashboard pipeline, or click "Purchase Orders" breadcrumb from detail |
| **How user exits** | Click another nav item, or click a PO row to enter detail |
| **Sections** | Status Pipeline Summary (4 cards), Bulk Action Bar (conditional), Data Table |
| **Components** | PipelineSummaryCard ×4, BulkActionBar (conditional), DataTable with columns: Checkbox, PO#, Vendor, Amount, Status, Risk, AI, SLA |
| **Actions** | Toggle row checkbox, click row → open PO detail, bulk approve (when selected), bulk export (when selected), clear selection |
| **States** | Default (no selection), rows selected (bulk bar visible), alternating row backgrounds, selected row highlight |
| **Transitions** | fadeIn on table rows (staggered 0.03s), fadeUp on bulk action bar appearance |
| **Dependencies** | POS data, selected Set state, stPipeline counts |

#### B4. Order Detail View
| Attribute | Value |
|---|---|
| **Unique name** | OrderDetailView |
| **Route** | `/orders/{poId}` |
| **Purpose** | Full PO review with line items, approval chain, history, and AI analysis |
| **Parent** | Main content area (replaces OrdersListView) |
| **How user reaches it** | Click PO row in orders list, or click pipeline card on dashboard |
| **How user exits** | Click "Purchase Orders" breadcrumb, press Escape |
| **Sections** | Detail Header (PO#, status, badges, SLA, vendor, requester), Tab bar (Overview/Items/Approvals/History), Tab content area, Action buttons (Approve/Reject/Escalate) |
| **Components** | DetailHeader, StatusBadge, AIChip, SLAIndicator, TabBar (4 tabs), OverviewTab (AI analysis card + order details grid + vendor card), ItemsTab (line item table with footer total), ApprovalsTab (step chain visualization), HistoryTab (timestamped audit log) |
| **Actions** | Switch tabs, approve PO, reject PO, escalate PO |
| **States** | Tab: overview (default), items, approvals, history. Action buttons visible only for pending/flagged/review status. SLA color coding: red if hours-only, amber otherwise |
| **Transitions** | fadeIn on view entry |
| **Dependencies** | Single PO object with full data: items[], chain[], history[], aiNote |

#### B5. Inventory View (Placeholder)
| Attribute | Value |
|---|---|
| **Unique name** | InventoryView |
| **Route** | `/inventory` |
| **Purpose** | Placeholder — "Coming soon" |
| **Components** | CenteredEmptyState (icon + title + subtitle) |

#### B6. Vendors View (Placeholder)
| Attribute | Value |
|---|---|
| **Unique name** | VendorsView |
| **Route** | `/vendors` |
| **Purpose** | Placeholder — "Coming soon" |
| **Components** | CenteredEmptyState (icon + title + subtitle) |

### C. Component Inventory

#### C1. TopBar
| Attribute | Value |
|---|---|
| **Appears** | All views — fixed at top |
| **Purpose** | App identity, breadcrumb navigation, global search, notifications, user context |
| **Sub-components** | BrandLogo, Breadcrumb, SearchTrigger, NotificationBell, UserAvatar |
| **States** | Notification dot visible (unread count > 0), notification dropdown open/closed, command palette open/closed |
| **Uno mapping** | Custom `Grid` layout in Shell page. `BreadcrumbBar` control for crumbs. `PersonPicture` for avatar. Custom flyout for notifications |

#### C2. NavRail (Left Sidebar)
| Attribute | Value |
|---|---|
| **Appears** | All views — fixed left |
| **Purpose** | Primary navigation + persistent warehouse metrics |
| **Sub-components** | NavGroupLabel, NavButton (icon + label + optional badge), MiniMetrics (3 progress bars) |
| **States** | Active item highlighted (accent background + left indicator bar), badge count on Orders, disabled items (Inventory/Vendors/Contracts at 60% opacity) |
| **Uno mapping** | `NavigationView` with `PaneDisplayMode="Left"`, custom `MenuItems`. Or `AutoLayout` with `Button` list and `Region.Attached` navigation. Mini-metrics as custom `UserControl` |

#### C3. KpiCard
| Attribute | Value |
|---|---|
| **Appears** | Dashboard view (5 instances) |
| **Purpose** | Single KPI display with label, value, delta badge, subtitle |
| **Props** | label: string, value: string, delta: string (optional), subtitle: string (optional), accentColor: Color |
| **Visual variants** | Delta badge colors: green (↑), red (overdue), amber (neutral). Bottom accent strip (2px, colored) |
| **States** | Default, hover (border color change implied by cursor:pointer) |
| **Animation** | fadeUp entrance with staggered delay |
| **Uno mapping** | `CardContentControl` (Uno Toolkit) or custom `Border` + `AutoLayout`. Material elevation via `ThemeShadow` |

#### C4. StatusBadge
| Attribute | Value |
|---|---|
| **Appears** | Orders list, order detail, pipeline cards |
| **Purpose** | Visual status indicator (pending/approved/review/flagged) |
| **Props** | status: enum (pending, approved, review, flagged) |
| **Variants** | 4 color combos: amber/amber-dim, green/green-dim, blue/blue-dim, red/red-dim. Each with left dot indicator |
| **Uno mapping** | Custom `Border` with `CornerRadius` + `TextBlock`. Or `Chip` (Uno Toolkit) in read-only mode |

#### C5. AIChip
| Attribute | Value |
|---|---|
| **Appears** | Orders list, order detail header |
| **Purpose** | Inline AI risk/insight indicator |
| **Props** | ai: {type: alert/warn/info, text: string} or null |
| **Variants** | 3 types (red/amber/blue backgrounds) + null state (dash) |
| **Uno mapping** | Same pattern as StatusBadge — `Border` + `TextBlock` |

#### C6. SLA Indicator
| Attribute | Value |
|---|---|
| **Appears** | Orders list (SLA column), order detail header |
| **Purpose** | Time-remaining display with urgency coloring |
| **States** | Critical (hours only, no days → red), normal (days+hours → amber), completed (dash) |
| **Uno mapping** | `TextBlock` with conditional `Foreground` binding |

#### C7. BulkActionBar
| Attribute | Value |
|---|---|
| **Appears** | Orders list (conditional — when selection > 0) |
| **Purpose** | Floating contextual toolbar for batch operations |
| **Components** | Selection count label, Bulk Approve button, Export button, Clear button |
| **States** | Visible (selection > 0), hidden (selection = 0) |
| **Animation** | fadeUp on appear |
| **Uno mapping** | `Border` + `AutoLayout` with `Visibility` bound to selection count. Or `CommandBar` custom style |

#### C8. DataTable (Orders)
| Attribute | Value |
|---|---|
| **Appears** | Orders list view |
| **Purpose** | Tabular PO listing with selection, sorting indicators, row interaction |
| **Columns** | Checkbox, PO#, Vendor (name + region + category), Amount, Status, Risk, AI, SLA |
| **States** | Unselected row, selected row (accent background), alternating stripe, hover |
| **Actions** | Checkbox toggle, row click → detail navigation |
| **Uno mapping** | `ListView` with custom `ItemTemplate` using `Grid` for column alignment. Or `ItemsRepeater` with header row. Selection via `CommandExtensions` (Uno Toolkit) |

#### C9. ApprovalChain
| Attribute | Value |
|---|---|
| **Appears** | Order detail, Approvals tab |
| **Purpose** | Visual multi-step approval workflow |
| **Per-step data** | who, role, status (done/current/waiting), timestamp |
| **Visual** | Vertical stepper: circle (✓/number) + connecting line + name/role/status badge |
| **States** | Done (green circle + ✓), current (amber circle + number + "CURRENT" badge), waiting (gray circle + number) |
| **Uno mapping** | `ItemsRepeater` with custom item template. Line connector via `Border` with fixed width. Step circle via `Ellipse` or `Border` with `CornerRadius="12"` |

#### C10. IsometricCanvas
| Attribute | Value |
|---|---|
| **Appears** | Warehouse view (fills main content) |
| **Purpose** | Real-time isometric voxel renderer for warehouse floor planning |
| **Inputs** | Voxel Map, Zone Map, current layer, cursor position, current tool/mode/asset |
| **Rendering** | 60fps canvas: zone tiles → grid lines → ground shadows → sorted voxels (back-to-front) → ghost cursor |
| **Interactions** | Mouse move (cursor tracking), click (place), right-click (erase), scroll (layer change) |
| **Uno mapping** | `SKXamlCanvas` (SkiaSharp) for custom drawing. Touch/pointer events via standard WinUI pointer handlers. Render loop via `CompositionTarget.Rendering` or `DispatcherTimer` |

#### C11. LayerScrubber
| Attribute | Value |
|---|---|
| **Appears** | Warehouse view, right edge |
| **Purpose** | Vertical control for navigating z-layers (0–6) |
| **Components** | Up arrow button, 7 level indicator bars, down arrow button, current level label |
| **States** | Per-level: active (accent), populated (dim white), empty (near-invisible). Active level bar is highlighted |
| **Uno mapping** | `StackPanel` (vertical) with `Button` ×2 + `ItemsRepeater` for level bars |

#### C12. CommandPalette
| Attribute | Value |
|---|---|
| **Appears** | Global overlay (⌘K or click search trigger) |
| **Purpose** | Unified search across POs, vendors, AI actions, navigation |
| **Components** | Search input, results list (categorized: Search/Action/Navigate) |
| **States** | Closed (default), open (modal overlay with backdrop blur) |
| **Dismissal** | Click backdrop, press Escape |
| **Uno mapping** | Custom `ContentDialog` or `Popup` with `TextBox` + `ListView`. Backdrop via semi-transparent `Grid` overlay |

#### C13. NotificationDropdown
| Attribute | Value |
|---|---|
| **Appears** | Anchored to notification bell in topbar |
| **Purpose** | Recent operational alerts grouped by type |
| **Per-notification** | title, body, type (alert/warn/info/ok), timestamp, read/unread |
| **States** | Open/closed. Per-item: read (transparent bg) / unread (accent-tinted bg) |
| **Actions** | Mark all read (header action), click individual notification |
| **Uno mapping** | `Flyout` anchored to bell `Button`. `ListView` inside flyout with custom `ItemTemplate` |

#### C14. Toast
| Attribute | Value |
|---|---|
| **Appears** | Bottom center, global |
| **Purpose** | Transient confirmation/feedback message |
| **States** | Visible (2.4s duration), then auto-dismiss |
| **Animation** | fadeUp entrance |
| **Uno mapping** | `InfoBar` (WinUI) positioned at bottom, or custom `Border` in overlay with `DispatcherTimer` auto-hide |

#### C15. StatusBar (Footer)
| Attribute | Value |
|---|---|
| **Appears** | All views — fixed at bottom |
| **Purpose** | Connection status, sync time, org context, aggregate counts |
| **Content** | Green dot + "Connected", "Last sync: 2m ago", "Org: Precision Tools Midwest", order/unit/tonnage counts |
| **Uno mapping** | `Grid` or `AutoLayout` with `TextBlock` items. Status dot via `Ellipse` |

### D. State Inventory

| State | Where | Description |
|---|---|---|
| **Default/loaded** | All views | Data present, normal rendering |
| **Hover** | KPI cards, nav items, table rows, buttons, notification items, pipeline cards | Cursor:pointer, border/background shift |
| **Active/selected** | Nav items, asset palette, zone palette, mode buttons, table rows (checkbox), detail tabs | Accent background + color, left indicator bar (nav), border highlight |
| **Disabled** | Nav items (Inventory/Vendors/Contracts) | 60% opacity, no click handler |
| **Expanded** | Notification dropdown, command palette | Overlay visible with backdrop |
| **Collapsed** | Notification dropdown, command palette | Hidden (display:none) |
| **Bulk selection active** | Orders list | Selection count > 0, bulk bar visible |
| **No selection** | Orders list | Bulk bar hidden |
| **Detail active** | Orders view | OrderDetailView replaces OrdersListView |
| **Tab states** | Order detail | 4 tabs: overview (default), items, approvals, history. Active tab has bottom accent border |
| **Build mode** | Warehouse | Asset palette visible, zone palette hidden |
| **Zone mode** | Warehouse | Zone palette visible, asset palette hidden |
| **Erase tool** | Warehouse | Red-tinted erase button, right-click behavior matches left-click |
| **Layer N active** | Warehouse | Layer scrubber shows current level, grid overlay appears for layer > 0 |
| **Ghost cursor** | Warehouse (build mode) | Translucent voxel preview at cursor position |
| **Empty warehouse** | Warehouse after clear | Empty grid, metrics show 0% |
| **Preset loaded** | Warehouse after load | Full layout rendered, metrics update |
| **SLA critical** | Orders list + detail | Red text for hours-only SLA values |
| **Flagged PO** | Orders list + detail | Red status badge, AI alert chip |
| **Unread notifications** | Topbar bell | Red dot indicator |
| **Toast visible** | Global | 2.4s auto-dismiss |
| **Animated entrance** | Dashboard KPIs, table rows, pipeline cards | Staggered fadeUp/fadeIn on mount |
| **Empty/placeholder** | Inventory, Vendors views | Centered icon + "Coming soon" message |

### E. Transition Inventory

| Transition | Trigger | Behavior |
|---|---|---|
| **View switch** | Nav click | Instant content swap (conditional render). Breadcrumbs update. Selected state on nav changes |
| **Dashboard → Order detail** | Click pipeline card | setView("orders") + setDetail(po). Breadcrumbs: Operations / Purchase Orders / PO-XXXX |
| **Orders list → detail** | Click table row | setDetail(po). Content replaces list. Breadcrumbs add PO ID |
| **Detail → list** | Click "Purchase Orders" breadcrumb or Escape | setDetail(null). List re-renders |
| **Tab switch** | Click tab button | Conditional render swap. Active tab underline changes |
| **Notification open** | Click bell | Dropdown appears anchored to bell. Click outside or re-click closes |
| **Command palette open** | ⌘K or click search trigger | Modal overlay with backdrop blur. Input auto-focused. Escape closes |
| **Bulk bar appear** | First checkbox checked | fadeUp animation. Bar shows with count |
| **Bulk bar disappear** | Last checkbox unchecked or "Clear" | Bar hides |
| **Toast appear/dismiss** | Action completed | fadeUp entrance, 2.4s auto-dismiss |
| **Warehouse mode switch** | Click Build/Zone or press B/Z | Palette swap (asset ↔ zone). Mode button active state changes |
| **Asset/zone selection** | Click palette item or press Q | Active state changes on palette buttons |
| **Layer change** | Scroll wheel or ▲/▼ buttons or W/S keys | Layer scrubber updates, grid overlay appears/moves, ghost cursor height changes |
| **Voxel place** | Left-click on canvas | Voxel added to Map, canvas re-renders, metrics recompute |
| **Voxel erase** | Right-click or left-click in erase mode | Voxel removed from Map, canvas re-renders, metrics recompute |
| **Preset load** | Click Layout A/B/Clear | Full grid replacement, metrics recompute, toast notification |
| **KPI entrance** | Dashboard mount | Staggered fadeUp (0.07s interval per card, 0.4s duration) |
| **Table row entrance** | Orders list mount | Staggered fadeIn (0.03s interval per row, 0.2s duration) |

---

## 3. Coverage Table

| Page/View | Sections | Components | States | Transitions | Confidence | Possible Gaps |
|---|---|---|---|---|---|---|
| Dashboard | 3 (KPIs, Pipeline, Activity) | 5 types (KpiCard, PipelineBar, PipelineCard, ActivityItem, ViewAllBtn) | 2 (default, animated entrance) | 3 (entrance anim, card click→detail, view all→list) | HIGH | No loading/error/empty states. No refresh mechanism. KPI data is static |
| Warehouse Planner | 4 (Toolbar, Canvas, LayerCtrl, HUD) | 8+ (ModeBtn, AssetPalette, ZonePalette, PresetBtn, Canvas, LayerScrubber, CursorHUD, KeysHUD) | 8+ (build/zone, place/erase, layer 0-6, ghost, empty, populated) | 6+ (mode switch, asset select, layer change, place, erase, preset load) | HIGH | No pan/zoom, no undo/redo, no save/load user layouts, no multi-select, no copy/paste |
| Orders List | 3 (Pipeline summary, Bulk bar, Table) | 6 (PipelineCard, BulkBar, DataTable, Badge, Chip, SLA) | 5 (default, rows selected, bulk bar visible, alternating rows, animated entrance) | 5 (row click→detail, checkbox toggle, bulk approve, bulk export, clear selection) | HIGH | No actual filtering by status (chips are static), no sort behavior, no pagination, no search within orders |
| Order Detail | 5 (Header, Tabs, Overview, Items, Approvals, History) | 10+ (Header, Badge, Chip, SLA, TabBar, AICard, DetailsGrid, VendorCard, ItemTable, ApprovalChain, HistoryLog, ApproveBtn, RejectBtn, EscalateBtn) | 6 (4 tabs, action buttons conditional, SLA color variants) | 5 (tab switch, approve, reject, escalate, back to list) | HIGH | No inline editing, no comments/notes, no attachment support, no related PO linking (PO-7420↔PO-7417 mentioned but not navigable) |
| Inventory (placeholder) | 1 (EmptyState) | 1 (CenteredEmptyState) | 1 (placeholder) | 0 | COMPLETE | N/A — intentionally placeholder |
| Vendors (placeholder) | 1 (EmptyState) | 1 (CenteredEmptyState) | 1 (placeholder) | 0 | COMPLETE | N/A — intentionally placeholder |
| Command Palette | 2 (Input, Results) | 3 (Overlay, SearchInput, ResultItem) | 2 (open, closed) | 2 (open, close) | MEDIUM | No actual search filtering (results are static), no keyboard arrow navigation in results |
| Notification Dropdown | 2 (Header, List) | 3 (Header, MarkAllRead, NotificationItem) | 3 (open, closed, read/unread per item) | 2 (open, close) | MEDIUM | Mark all read has no handler. Click notification has no navigation. No dismiss individual |
| Toast | 1 | 1 | 2 (visible, hidden) | 2 (appear, auto-dismiss) | HIGH | No queue for multiple toasts |

**Second-pass items identified:**
- No loading states for any view
- No error states for data fetch failures
- No offline handling
- No empty state for orders (if zero POs existed)
- No responsive/mobile breakpoints
- No dark/light theme toggle (dark only)
- No settings/preferences view
- No user profile/logout
- Filter chips in orders list are non-functional
- Command palette search does not actually filter results

---

## 4. PRD — Product Requirements Document

### 4.1 Product Overview
GRIDFORM is an enterprise operations platform for industrial tooling distribution centers. It provides a unified workspace for procurement management, warehouse spatial planning, and operational monitoring. The target deployment is cross-platform via Uno Platform (Windows primary, WebAssembly for browser access).

### 4.2 Target Users
**Primary:** Operations Manager at a regional distribution center (Precision Tools Midwest). Opens GRIDFORM every morning to review pending approvals, check warehouse capacity, and monitor procurement pipeline health.

**Secondary:** Procurement Approvers (finance, VP), Warehouse Planners, Receiving Staff.

### 4.3 Problem Statement
Industrial tooling distributors currently operate across fragmented tools: ERP for procurement (SAP/Oracle), spreadsheets for warehouse planning, email for approvals. Mode-switching between these tools creates cognitive overhead and delays. AI-assisted risk detection is absent from most ERP workflows.

### 4.4 Core User Journeys

**J1: Morning Operations Review**
Dashboard → scan KPIs → identify urgent items → click into flagged PO → review AI analysis → approve/reject/escalate → return to dashboard.

**J2: PO Approval Workflow**
Notification (SLA alert) → Orders → PO Detail → Overview tab (AI analysis) → Items tab (verify line items) → Approvals tab (see chain position) → Approve/Reject → Toast confirmation → back to list.

**J3: Warehouse Layout Planning**
Warehouse view → load preset or start fresh → switch between Build/Zone modes → place assets (pallets, racks, containers, equipment, aisles) → paint zones (receiving, storage, staging, shipping) → adjust layers for vertical stacking → monitor utilization metrics → iterate.

**J4: Bulk PO Processing**
Orders list → select multiple POs via checkboxes → bulk action bar appears → "Bulk Approve" → toast confirmation → selection cleared.

### 4.5 Feature Breakdown

**F1: Dashboard**
- FR-1.1: Display 5 KPI cards (Open POs, Pending Approval, Q2 Committed, Warehouse Floor %, Savings YTD)
- FR-1.2: Each KPI shows value, optional delta badge (color-coded), optional subtitle
- FR-1.3: Order Pipeline panel shows status distribution bar + top 4 non-approved POs with click-through to detail
- FR-1.4: Activity panel shows timestamped event log with type-colored indicators

**F2: Warehouse Spatial Planner**
- FR-2.1: 14×14 isometric grid with 6 vertical layers
- FR-2.2: 5 asset types: Pallet (1.2t), Rack (0.4t), Container (2.5t), Equipment (3.0t), Aisle (0t)
- FR-2.3: 4 zone types: Receiving, Storage, Staging, Shipping — painted on ground plane
- FR-2.4: Build mode (place assets) and Zone mode (paint zones) togglable
- FR-2.5: Place tool (left-click) and Erase tool (right-click or toggle)
- FR-2.6: Scroll wheel changes active layer (0–6)
- FR-2.7: Layer scrubber on right edge shows populated vs empty layers
- FR-2.8: Ghost cursor preview in build mode
- FR-2.9: Ambient occlusion rendering for visual depth
- FR-2.10: Depth fog for back-to-front spatial reading
- FR-2.11: Preset layouts: Warehouse A (full), Staging B, Clear
- FR-2.12: Real-time metrics: floor coverage %, volumetric fill %, total tonnage, peak height, asset counts, zone cell counts

**F3: Procurement / Orders**
- FR-3.1: Orders list with columns: Checkbox, PO#, Vendor (name/region/cat), Amount, Status, Risk, AI, SLA
- FR-3.2: Status pipeline summary cards (Pending/Review/Approved/Flagged with counts)
- FR-3.3: Bulk selection via checkboxes with floating action bar (Bulk Approve, Export, Clear)
- FR-3.4: PO detail view with tabs: Overview, Line Items, Approvals, History
- FR-3.5: Overview tab: AI analysis card, order details grid, vendor info card
- FR-3.6: Items tab: SKU, description, qty, unit price, line total, with footer total
- FR-3.7: Approvals tab: Visual step chain (done/current/waiting) with role, timestamp, "CURRENT" badge
- FR-3.8: History tab: Timestamped audit log
- FR-3.9: Action buttons: Approve, Reject, Escalate (visible only for actionable statuses)
- FR-3.10: SLA indicators with urgency coloring (red for critical, amber for normal)
- FR-3.11: AI chips on table rows (alert/warn/info types)

**F4: Global Features**
- FR-4.1: Command palette (⌘K) with categorized results (Search/Action/Navigate)
- FR-4.2: Notification dropdown with read/unread states and type-colored indicators
- FR-4.3: Breadcrumb navigation reflecting current view depth
- FR-4.4: Toast notifications for action confirmations (2.4s auto-dismiss)
- FR-4.5: Status bar showing connection status, sync time, org name, aggregate counts
- FR-4.6: Persistent warehouse mini-metrics in nav sidebar footer
- FR-4.7: Keyboard shortcuts: ⌘K (command palette), Escape (close overlays / back), Q (cycle asset), B/Z/X (modes), W/S/arrows (layers), 1/2/3 (view switch)

### 4.6 Non-Goals (v1)
- Real-time multi-user collaboration
- Actual API integration / backend data persistence
- Mobile-responsive layouts (desktop-first)
- Print/export functionality
- Role-based access control enforcement
- Undo/redo in warehouse planner
- Drag-and-drop reordering
- Camera overlay / AR features (deferred from earlier prototype)

### 4.7 Open Questions
- Q1: Should warehouse layouts persist to a backend, or are they session-only?
- Q2: Should the approval workflow support delegation (reassign to another approver)?
- Q3: What is the actual SLA calculation source — is it from submission timestamp + policy, or manually set?
- Q4: Should related POs (PO-7420 ↔ PO-7417) be navigable from the detail view?
- Q5: Should the Inventory and Vendor views be included in v1 scope, or remain placeholders?

---

## 5. Architecture Brief — Uno Platform

### 5.1 Solution Structure

```
GridForm/
├── GridForm.sln
├── GridForm/                          # Shared head project
│   ├── App.xaml / App.xaml.cs
│   ├── Themes/
│   │   ├── ColorPaletteOverride.xaml  # Warm industrial palette
│   │   └── TextBlock.xaml             # Typography overrides
│   ├── Models/
│   │   ├── PurchaseOrder.cs           # PO record
│   │   ├── Asset.cs                   # Warehouse asset types
│   │   ├── Zone.cs                    # Warehouse zone types
│   │   └── Notification.cs            # Notification record
│   ├── Services/
│   │   ├── IProcurementService.cs     # PO data access
│   │   ├── IWarehouseService.cs       # Voxel grid + zone data
│   │   ├── INotificationService.cs
│   │   └── Implementations/
│   ├── Presentation/
│   │   ├── Shell.xaml / Shell.xaml.cs  # App shell: TopBar + Nav + Content + StatusBar
│   │   ├── DashboardPage.xaml
│   │   ├── DashboardModel.cs          # MVUX model
│   │   ├── WarehousePage.xaml
│   │   ├── WarehouseModel.cs          # MVUX model
│   │   ├── OrdersPage.xaml
│   │   ├── OrdersModel.cs
│   │   ├── OrderDetailPage.xaml
│   │   ├── OrderDetailModel.cs
│   │   └── Placeholder/
│   │       └── ComingSoonPage.xaml
│   ├── Controls/
│   │   ├── KpiCard.xaml               # Reusable KPI display
│   │   ├── StatusBadge.xaml           # Status indicator
│   │   ├── AiChip.xaml                # AI risk chip
│   │   ├── ApprovalChain.xaml         # Stepper visualization
│   │   ├── IsometricCanvas.xaml       # SkiaSharp canvas
│   │   ├── LayerScrubber.xaml
│   │   ├── PipelineBar.xaml
│   │   └── MiniMetrics.xaml
│   └── Converters/
│       ├── StatusToColorConverter.cs
│       ├── RiskToColorConverter.cs
│       └── SlaToColorConverter.cs
├── GridForm.Wasm/
├── GridForm.Windows/
└── GridForm.Skia.Gtk/ (optional)
```

### 5.2 UnoFeatures (csproj)

```xml
<UnoFeatures>
  Material;
  Toolkit;
  Extensions;
  ExtensionsCore;
  Hosting;
  MVUX;
  Navigation;
  ThemeService;
  Skia;
  Logging;
</UnoFeatures>
```

### 5.3 Navigation Model

Use Uno Extensions Navigation with region-based routing:

- **Shell** (`Shell.xaml`): Top-level `Grid` with `Region.Attached="True"`. Contains `NavigationView` (left nav) and content `Grid` with `Region.Attached="True"` + `Region.Navigator="Visibility"`.
- **Routes**: Dashboard, Warehouse, Orders, OrderDetail (nested under Orders), Inventory, Vendors.
- **Breadcrumbs**: Use `BreadcrumbBar` control. Bind `ItemsSource` to a computed breadcrumb list in the shell model, updated on navigation events via `IRouteNotifier`.
- **Deep navigation**: Orders → OrderDetail via `Navigation.Request` with PO data parameter.

```xml
<!-- Shell.xaml excerpt -->
<Grid uen:Region.Attached="True">
  <muxc:NavigationView uen:Region.Attached="True" PaneDisplayMode="Left">
    <muxc:NavigationViewItem Content="Dashboard" uen:Region.Name="Dashboard" />
    <muxc:NavigationViewItem Content="Warehouse" uen:Region.Name="Warehouse" />
    <muxc:NavigationViewItem Content="Orders" uen:Region.Name="Orders" />
  </muxc:NavigationView>
  <Grid uen:Region.Attached="True" uen:Region.Navigator="Visibility">
    <!-- Pages render here -->
  </Grid>
</Grid>
```

### 5.4 State Management (MVUX)

Each view has a corresponding MVUX Model class:

- `DashboardModel`: `IListFeed<PurchaseOrder> PendingOrders`, `IState<WarehouseMetrics> Metrics`
- `WarehouseModel`: `IState<WarehouseState> Warehouse` (contains voxel grid, zones, current tool/mode/layer/asset), commands for Place, Erase, LoadPreset, CycleAsset, etc.
- `OrdersModel`: `IListFeed<PurchaseOrder> Orders`, `IState<SelectionState> Selection`, commands for ToggleSelect, BulkApprove, Export
- `OrderDetailModel`: `IState<PurchaseOrder> Order`, `IState<string> ActiveTab`, commands for Approve, Reject, Escalate

### 5.5 Isometric Canvas — SkiaSharp

The isometric renderer uses `SKXamlCanvas` from SkiaSharp.Views.WinUI:

```xml
<skia:SKXamlCanvas PaintSurface="OnPaintSurface"
                   PointerMoved="OnPointerMoved"
                   PointerPressed="OnPointerPressed"
                   PointerWheelChanged="OnPointerWheelChanged" />
```

The rendering logic (isometric projection, AO calculation, voxel drawing, zone tiles, grid lines) ports directly from the prototype's canvas code into C# SkiaSharp calls (`SKCanvas.DrawPath`, `SKCanvas.DrawText`, etc.). The `PaintSurface` handler redraws on every pointer event via `canvas.InvalidateVisual()`.

### 5.6 Data Flow

```
PurchaseOrder record → IProcurementService (in-memory for v1)
                     → MVUX Feed/State in Model
                     → Data binding to XAML controls

WarehouseGrid (Dictionary<VoxelKey, AssetType>) → IWarehouseService
                                                → MVUX State in WarehouseModel
                                                → SKXamlCanvas re-render on state change
```

### 5.7 Accessibility

- All interactive elements: `AutomationProperties.Name` and `AutomationProperties.LabeledBy`
- Tab order: TopBar → Nav → Main content → Status bar
- Canvas interactions: keyboard equivalents (Q/B/Z/X/W/S/arrows already defined)
- High-contrast mode: Leverage Material theme's built-in high-contrast support
- Focus indicators: Use WinUI default focus rectangles
- Screen reader: Status badge and SLA indicator need `AutomationProperties.LiveSetting="Polite"` for dynamic updates

### 5.8 Technical Risks

| Risk | Impact | Mitigation |
|---|---|---|
| SkiaSharp canvas performance at 14×14×6 grid | Medium — potential jank on WASM | Profile early. Consider dirty-rect rendering (only redraw changed voxels). Throttle pointer events |
| MVUX state size for warehouse grid (Map with ~1000+ entries) | Low-Medium — frequent re-renders | Use `IState<T>` with selective update. Avoid full grid replacement on single voxel change |
| Navigation complexity (Orders ↔ OrderDetail with data passing) | Low | Use Uno Navigation data parameter passing. `Navigation.Request="OrderDetail"` with PO record |
| Dark theme only — Material theme defaults to light | Low | Override `ColorPaletteOverride.xaml` with warm dark palette. Set `RequestedTheme="Dark"` |

---

## 6. Design Brief

### 6.1 Design Intent
**"Precision Industrial"** — warm, matte, purposeful. Inspired by the environment of a well-maintained machine shop: machined metal, brass fixtures, concrete floors. The aesthetic conveys trustworthiness and operational authority without cold tech-sterility.

### 6.2 Color System (Design Tokens)

| Token | Hex | Usage |
|---|---|---|
| `Background` | `#0F0F0D` | App background |
| `Surface` | `#161614` | Panels, nav, topbar |
| `SurfaceRaised` | `#1C1B18` | Cards, elevated surfaces |
| `SurfaceHigh` | `#242320` | Hover states, table stripes |
| `Border` | `#282724` | Default borders |
| `BorderHigh` | `#363431` | Hover/active borders |
| `TextPrimary` | `#EAE7DF` | Headings, primary content |
| `TextSecondary` | `#A8A49A` | Body text, descriptions |
| `TextTertiary` | `#706D64` | Labels, metadata |
| `TextMuted` | `#3E3C38` | Disabled, hints |
| `Accent` | `#5FB89E` | Primary actions, active states, links — sage green |
| `AccentDim` | `rgba(95,184,158,0.10)` | Accent backgrounds |
| `Copper` | `#D4956A` | Secondary accent, warmth |
| `StatusGreen` | `#5FB89E` | Approved, success, up |
| `StatusAmber` | `#D4A64E` | Pending, warning, SLA normal |
| `StatusRed` | `#C75B5B` | Flagged, error, SLA critical, overdue |
| `StatusBlue` | `#6B9FC8` | In review, informational |

### 6.3 Typography

| Role | Font | Weight | Size | Usage |
|---|---|---|---|---|
| Display | Outfit | 700 | 26–28px | KPI values, page-level numbers |
| Heading | Outfit | 700 | 13–17px | Section titles, card titles, PO IDs in detail |
| Body | Outfit | 400–500 | 11.5–12px | Descriptions, table cells, activity messages |
| Label | Outfit | 500–600 | 10–11px | Button labels, filter chips, nav items |
| Caption | JetBrains Mono | 400–600 | 9–10px | Timestamps, SLA values, metadata, column headers |
| Mono Data | JetBrains Mono | 600 | 10–11px | PO numbers, amounts, SKUs, cursor coordinates |

### 6.4 Layout Grid

- **App shell**: CSS Grid — `200px 1fr` columns, `48px 1fr 28px` rows
- **Dashboard KPIs**: Flexbox row, 5 equal-flex cards, 10px gap
- **Dashboard panels**: 2-column grid, 14px gap
- **Orders table**: Full-width, 8 columns with defined proportions
- **Order detail**: Single column, max content width, internal 2-column grid for overview tab
- **Warehouse**: Full-bleed canvas with overlay controls

### 6.5 Spacing Scale
4, 5, 6, 8, 10, 12, 14, 16, 18, 20, 24. All padding/margin/gap values drawn from this scale.

### 6.6 Border Radius
- Cards/panels: 10px
- Buttons/chips: 6px
- Status badges: 4px
- Layer indicators: 2px
- User avatar: 50% (circle)

### 6.7 Elevation
- Cards: flat (border only, no shadow)
- Notification dropdown: `boxShadow: 0 12px 40px rgba(0,0,0,0.4)`
- Command palette: `boxShadow: 0 16px 48px rgba(0,0,0,0.5)`
- Toast: `boxShadow: 0 8px 32px rgba(0,0,0,0.4)`

### 6.8 Iconography
The prototype uses Unicode symbols as icons: ◫ ◆ ◇ ⊞ ⊟ ⊡ 🔔. In Uno implementation, replace with `FontIcon` (Segoe Fluent Icons or Material Symbols) for consistency and accessibility.

### 6.9 Consistency Notes
- The nav uses icon + label format consistently
- All status representations (badge, pipeline, risk) follow the same 4-color system
- Table headers use monospace uppercase with letter-spacing — consistent with the "industrial instrumentation" feel
- Bottom accent strips on KPI cards create visual hierarchy without adding visual noise

---

## 7. Interaction Brief

### 7.1 Navigation Interactions

| Element | Trigger | Response | State Change |
|---|---|---|---|
| Nav item | Click | Main content swaps to target view. Breadcrumbs update. Detail/selection reset | `view` changes. Active indicator moves |
| Breadcrumb segment | Click | Navigate to that level. If "Purchase Orders" clicked from detail, return to list | `detail` set to null, or `view` changes |
| Dashboard pipeline card | Click | Navigate to Orders view with PO detail open | `view="orders"`, `detail=po`, `detailTab="overview"` |
| "View all →" button | Click | Navigate to Orders list | `view="orders"`, `detail=null` |

### 7.2 Table Interactions

| Element | Trigger | Response |
|---|---|---|
| Row checkbox | Click (stopPropagation) | Toggle selection. Bulk bar appears/disappears based on count |
| Row body (non-checkbox) | Click | Open PO detail view. Breadcrumbs update |
| "Bulk Approve" button | Click | Toast: "✓ N orders approved". Selection cleared |
| "Export" button | Click | Toast: "Exported" |
| "Clear" button | Click | Selection cleared, bulk bar hides |

### 7.3 PO Detail Interactions

| Element | Trigger | Response |
|---|---|---|
| Tab button (Overview/Items/Approvals/History) | Click | Tab content swaps. Active tab gets accent underline |
| Approve button | Click | Toast: "✓ PO-XXXX approved". Return to list (`detail=null`) |
| Reject button | Click | Toast: "✕ PO-XXXX rejected". Return to list |
| Escalate button | Click | Toast: "↑ Escalated" (stays on detail) |
| Escape key | Press | Return to Orders list |

### 7.4 Warehouse Interactions

| Element | Trigger | Response |
|---|---|---|
| Build/Zone mode buttons | Click or B/Z keys | Mode switches. Palette swaps (asset↔zone). Active state changes on buttons |
| Erase button | Click or X key | Toggle erase tool on/off. Red styling when active |
| Asset palette item | Click or Q key (cycle) | Selected asset changes. Palette highlight updates. Ghost cursor type changes |
| Zone palette item | Click | Selected zone changes. Palette highlight updates |
| Canvas | Left-click | Place current asset/zone at cursor grid cell + current layer. Metrics recompute |
| Canvas | Right-click | Erase topmost voxel/zone at cursor cell. Metrics recompute |
| Canvas | Mouse move | Cursor position updates. Ghost preview follows. HUD coordinates update |
| Canvas | Scroll wheel | Active layer increments/decrements (clamped 0–6). Layer scrubber updates. Grid overlay moves |
| Layer ▲/▼ buttons | Click | Same as scroll |
| Layer indicator bar | Click | Jump to that layer directly |
| Layout A/B/Clear buttons | Click | Full grid replacement. Toast notification. Metrics recompute |

### 7.5 Overlay Interactions

| Element | Trigger | Dismiss |
|---|---|---|
| Command palette | ⌘K or click search trigger | Escape, click backdrop, select result |
| Notification dropdown | Click bell icon | Re-click bell, click outside (implicit — not implemented in prototype) |
| Toast | Action completion | Auto-dismiss after 2.4s |

### 7.6 Keyboard Shortcuts

| Key | Context | Action |
|---|---|---|
| `⌘K` / `Ctrl+K` | Global | Toggle command palette |
| `Escape` | Global | Close overlays → close detail → no-op |
| `1` | Global | Switch to Dashboard |
| `2` | Global | Switch to Warehouse |
| `3` | Global | Switch to Orders |
| `Q` | Warehouse | Cycle asset type |
| `B` | Warehouse | Switch to Build mode |
| `Z` | Warehouse | Switch to Zone mode |
| `X` | Warehouse | Toggle Erase tool |
| `W` / `↑` | Warehouse | Layer up |
| `S` / `↓` | Warehouse | Layer down |

### 7.7 Animation Timing

| Animation | Duration | Easing | Delay Pattern |
|---|---|---|---|
| KPI card entrance | 0.4s | ease | +0.07s per card |
| Pipeline card entrance | 0.3s | ease | +0.05s per card (start 0.35s) |
| Activity item entrance | 0.3s | ease | +0.05s per item (start 0.4s) |
| Table row entrance | 0.2s | ease | +0.03s per row |
| Bulk action bar | 0.2s | ease | none |
| Command palette | 0.15s | ease | none |
| Toast | 0.25s | ease | auto-dismiss at 2.4s |
| Button hover | 0.15s | ease | none |
| Nav item transition | 0.15s | ease | none |
| Metric bar width | 0.5s | cubic-bezier(0.16,1,0.3,1) | none |

---

## 8. Missing Coverage Check

### Pages/Views Possibly Missed
- **Settings/Preferences page**: Not present in prototype. Likely needed for: notification preferences, SLA thresholds, default view, user profile
- **User profile/logout**: Avatar is visible but has no click handler or dropdown
- **Vendor detail view**: Referenced in command palette but not implemented
- **Contract management**: Listed in nav (disabled) but no design

### Components Possibly Missed
- **Filter chips (functional)**: Present in Orders list but not wired to actual filtering
- **Sort indicators on table headers**: Prototype marks headers as "sortable" concept but has no sort implementation
- **Pagination / infinite scroll**: Table shows all 7 POs; no pagination pattern defined
- **Search within command palette**: Input exists but results don't filter
- **Related PO linking**: AI notes reference PO-7420↔PO-7417 but there's no clickable cross-reference

### States Possibly Missed
- **Loading**: No skeleton/spinner state for any data fetch
- **Error**: No error state for failed operations
- **Empty**: No empty state for Orders if zero POs exist
- **Offline**: No offline indicator or degraded mode
- **First-run / onboarding**: No first-time user experience
- **Permission-gated**: Approve/Reject/Escalate buttons appear for all POs; no role check
- **Responsive breakpoints**: No mobile/tablet adaptation
- **Canvas loading**: No loading state while preset generates

### Transitions Possibly Missed
- **Notification → PO detail**: Clicking a notification should navigate to the related PO
- **AI chip → detail**: Clicking AI chip in table could open detail to overview tab
- **Command palette result → navigation**: Results should navigate to the target entity
- **Related PO navigation**: From PO-7420 detail, link to PO-7417

### Assumptions Made
- Single-user, single-org context (no org switcher)
- All data is local/in-memory (no API)
- Dark theme only (no light mode toggle)
- Desktop-first (no responsive breakpoints)
- Approval actions are final (no confirmation dialog before approve/reject)
- Warehouse state is session-only (not persisted)

---

## 9. Open Questions

1. **Data persistence**: Should warehouse layouts save to backend storage, or are they planning-session tools?
2. **Real-time updates**: Should the activity feed and notification center poll or use WebSocket push?
3. **Multi-user**: Will multiple users view the same warehouse layout simultaneously?
4. **Approval confirmation**: Should Approve/Reject have a confirmation dialog, or is the current one-click pattern intentional?
5. **SLA source of truth**: Are SLA timers computed from policy rules + submission timestamp, or manually assigned?
6. **Vendor detail scope**: Should clicking a vendor name in PO detail navigate to a vendor profile?
7. **Export format**: What format should "Export" produce (CSV, Excel, PDF)?
8. **Undo/redo in warehouse**: Is this a v1 requirement for the spatial planner?
9. **Camera overlay**: The earlier prototype included webcam overlay for AR site survey — is this deferred or cut?
10. **Mobile/tablet**: Is responsive layout a v1 requirement, or desktop-only?
11. **Localization**: Is multi-language support required? The prototype uses English with `x:Uid` pattern recommended for Uno
12. **Accessibility certification**: Is WCAG 2.1 AA compliance a hard requirement?
