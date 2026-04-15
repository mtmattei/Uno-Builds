# GRIDFORM Enterprise — Implementation Spec
## Industrial Tooling Distribution & Procurement Ops

---

## 1. Domain

Precision tooling distribution center. Vendors: Kennametal (carbide inserts), Sandvik (milling tools), Mitutoyo (instruments), Haas (machine parts). A procurement manager at an industrial distributor would recognize this data.

---

## 2. Application Shell

### 2.1 Layout (3-column)

```
┌─────────────── TOP BAR (44px) ──────────────────────────┐
│ GRIDFORM · SPATIAL OPS · [PROD] │ KPI pills │ Presets  │
├────────┬─────────────────────────────────┬──────────────┤
│        │                                 │              │
│  LEFT  │         CENTER VIEW             │   RIGHT      │
│  RAIL  │   (Dashboard / Warehouse /      │   AI CONTEXT │
│  220px │    Orders)                      │   PANEL      │
│        │                                 │   240px      │
│        │                                 │              │
└────────┴─────────────────────────────────┴──────────────┘
```

### 2.2 Left Rail Navigation

Persistent sidebar with icon + label items:
- **Dashboard** (default) — morning briefing
- **Warehouse** — voxel spatial planner
- **Orders** — procurement table

Active state: accent-colored left border + tinted background.
Bottom section: mini warehouse metrics (Floor %, Vol %, Tons) always visible.

### 2.3 Top Bar

- Left: Wordmark + "SPATIAL OPS" + [PROD] environment badge
- Center: View-specific actions (presets when in Warehouse, Export/New PO in Orders)
- Right: KPI pills (Floor %, Vol %, Tons, POs count) with colored dots

### 2.4 Right Panel — AI Context

Adapts to current view:
- **Dashboard**: Operations summary + Needs Attention cards
- **Warehouse**: Spatial analysis from live metrics
- **Orders**: PO-specific brief when a row is selected, otherwise procurement summary

Always shows: Activity timeline with timestamped color-coded entries.

---

## 3. Views

### 3.1 Dashboard

Five KPI cards with staggered entrance animation:
| Card | Value | Color | Source |
|------|-------|-------|--------|
| Open POs | count | Amber | PO data |
| Pending Approval | count | Red | PO status=pending |
| Floor Utilization | % | Accent | Warehouse metrics |
| Monthly Spend | $K | Blue | PO amounts |
| At-Risk Orders | count | Red | PO risk=high |

Activity timeline: color-coded event types (approval=green, alert=red, update=blue, warning=amber).

"Needs Attention" cards: top 3 POs that need action, clickable → navigates to Orders view with that PO selected.

### 3.2 Warehouse

Existing voxel spatial planner — integrated into the shell layout. Toolbar moves from floating HUD to the left panel's mode/asset/zone sections. Layer scrubber stays on the right edge of the center view. The isometric canvas fills the center area.

### 3.3 Orders

Procurement table with columns: PO#, Vendor, Amount, Status, Risk, AI, Submitted.

- Status badges: colored pills (pending=amber, approved=green, review=blue, flagged=red)
- Risk badges: HIGH/MED/LOW with color
- AI chips: inline alert/warn/info indicators
- Click row → right panel reshapes with order detail + vendor info + Approve/Reject/Escalate buttons

---

## 4. Data Models

### 4.1 PurchaseOrder

```csharp
public record PurchaseOrder(
    string Id,           // "PO-2421"
    string VendorName,   // "Kennametal Inc."
    string VendorRegion, // "Latrobe, PA"
    decimal Amount,
    string BudgetCode,   // "Q2-TOOL-04"
    POStatus Status,
    RiskLevel Risk,
    string? AiAlertType, // "alert" | "warn" | "info" | null
    string? AiAlertText, // "New vendor" | "+$12K ceiling" | null
    string SubmittedAgo, // "2h ago"
    string Approver,
    string Detail,       // line items summary
    string? AiBrief);    // AI-generated recommendation

public enum POStatus { Pending, Review, Approved, Flagged }
public enum RiskLevel { Low, Med, High }
```

### 4.2 Activity

```csharp
public record ActivityEntry(string TimeAgo, string Message, string Type);
// Type: "approval" | "alert" | "update" | "warning"
```

### 4.3 Seed Data

7-8 POs with realistic industrial tooling data:
- Kennametal: carbide inserts, turning tools
- Sandvik: milling cutters, CoroMill assemblies
- Mitutoyo: digital calipers, CMM probes, gauge blocks
- Haas: spindle bearings, coolant pumps, servo motors
- MSC Industrial: safety equipment, cutting fluids
- Renishaw: tool setters, probes

---

## 5. Color System

Based on the React reference, extending the existing monochrome palette:

| Token | Value | Usage |
|-------|-------|-------|
| `Bg` | `#0C0D10` | App background |
| `Panel` | `#111318` | Sidebar, right panel |
| `Raised` | `#181B22` | Hover states, selected rows |
| `Border` | `#1F232D` | Default borders |
| `Accent` | `#00D4AA` | Primary action, approved status |
| `Amber` | `#E8A230` | Pending, warnings |
| `Red` | `#E84855` | Flagged, high risk, reject |
| `Blue` | `#4A9EF5` | Review, info |
| `Violet` | `#9B7BF4` | Secondary accent |
| `T1` | `#E8E6E1` | Primary text |
| `T2` | `#9398A3` | Secondary text |
| `T3` | `#5D6370` | Tertiary text |
| `T4` | `#3A3F4B` | Muted labels |

---

## 6. Implementation Plan

### Phase 1: Shell + Navigation
- New Shell.xaml with 3-column Grid layout
- Left rail with NavigationView or custom sidebar
- Top bar with KPI pills
- Right AI context panel
- View switching (Dashboard/Warehouse/Orders)

### Phase 2: Data Layer
- PurchaseOrder, ActivityEntry models
- Seed data service with realistic industrial tooling POs
- Metrics service aggregating warehouse + PO data

### Phase 3: Dashboard View
- KPI cards with entrance animations
- Activity timeline
- Needs Attention cards

### Phase 4: Orders View
- Procurement table (ListView with custom template)
- Status/Risk/AI badges
- Detail panel integration with right panel

### Phase 5: Warehouse Integration
- Move existing voxel planner into center view
- Relocate toolbar controls to left panel
- Connect warehouse metrics to KPI pills

### Phase 6: Polish
- Staggered card animations
- Smooth view transitions
- Keyboard shortcuts (1/2/3 for views)
