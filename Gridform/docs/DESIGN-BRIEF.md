# GRIDFORM — Design Brief
## Visual System & Component Specification
### Industrial Tooling Distribution & Warehouse Spatial Planning

---


## 2.1 Design Intent

**"Precision Industrial"** — the visual language of a well-maintained machine shop: machined metal, brass fixtures, matte concrete. Warm, purposeful, trustworthy. The aesthetic conveys operational authority without cold tech sterility.

This is not a consumer app — it's an ops tool used 8 hours/day. Density is a feature. Whitespace is earned, not assumed. Every pixel should communicate operational state.

---

## 2.2 Color System — Material Token Mapping

GRIDFORM uses a dark theme with warm undertones. All colors are defined in `ColorPaletteOverride.xaml` using Material Design 3 semantic roles mapped to the warm industrial palette.

### Primary Palette

| Semantic Role | Token | Hex | Usage |
|---|---|---|---|
| `PrimaryColor` | Accent | `#5FB89E` | Primary actions, active nav, links, approval |
| `PrimaryContainerColor` | AccentDim | `#1A2E28` | Accent backgrounds, selected states |
| `OnPrimaryColor` | — | `#0F0F0D` | Text on accent buttons |
| `OnPrimaryContainerColor` | — | `#5FB89E` | Text on accent containers |
| `SecondaryColor` | Copper | `#D4956A` | Secondary accent, brand warmth, avatar gradient |
| `SecondaryContainerColor` | CopperDim | `#2B1F16` | Secondary backgrounds |
| `TertiaryColor` | Blue | `#6B9FC8` | Informational, "in review" status |
| `ErrorColor` | Red | `#C75B5B` | Flagged, rejected, SLA critical |

### Surface Hierarchy

| Token | Hex | Usage |
|---|---|---|
| `BackgroundColor` | `#0F0F0D` | App background, main content area |
| `SurfaceColor` | `#161614` | Panels (nav, topbar, statusbar) |
| `SurfaceContainerColor` | `#1C1B18` | Cards, elevated surfaces, table rows (alt) |
| `SurfaceContainerHighColor` | `#242320` | Hover states, selected rows, dropdowns |
| `SurfaceContainerHighestColor` | `#2A2926` | Active hover, pressed states |
| `OutlineColor` | `#282724` | Default borders |
| `OutlineVariantColor` | `#363431` | Hover/focus borders |

### Text Hierarchy

| Token | Hex | Usage |
|---|---|---|
| `OnSurfaceColor` | `#EAE7DF` | Primary text — headings, values, table data |
| `OnSurfaceVariantColor` | `#A8A49A` | Secondary text — descriptions, body |
| `SurfaceTintColor` | `#706D64` | Tertiary text — labels, metadata, timestamps |
| `OnSurfaceDisabledColor` | `#3E3C38` | Disabled text, muted hints, placeholder |

### Status Colors (Semantic)

| Status | Foreground | Background (Dim) | Mapped From |
|---|---|---|---|
| Pending | `#D4A64E` | `rgba(212,166,78,0.10)` | `WarningColor` |
| Approved | `#5FB89E` | `rgba(95,184,158,0.08)` | `PrimaryColor` |
| In Review | `#6B9FC8` | `rgba(107,159,200,0.10)` | `TertiaryColor` |
| Flagged | `#C75B5B` | `rgba(199,91,91,0.08)` | `ErrorColor` |

### ColorPaletteOverride.xaml Structure

```xml
<ResourceDictionary>
    <!-- Primary -->
    <Color x:Key="PrimaryColor">#5FB89E</Color>
    <Color x:Key="PrimaryContainerColor">#1A2E28</Color>
    <Color x:Key="OnPrimaryColor">#0F0F0D</Color>
    <Color x:Key="OnPrimaryContainerColor">#5FB89E</Color>

    <!-- Secondary -->
    <Color x:Key="SecondaryColor">#D4956A</Color>
    <Color x:Key="SecondaryContainerColor">#2B1F16</Color>

    <!-- Tertiary -->
    <Color x:Key="TertiaryColor">#6B9FC8</Color>

    <!-- Error -->
    <Color x:Key="ErrorColor">#C75B5B</Color>

    <!-- Background + Surface -->
    <Color x:Key="BackgroundColor">#0F0F0D</Color>
    <Color x:Key="SurfaceColor">#161614</Color>
    <Color x:Key="SurfaceContainerColor">#1C1B18</Color>
    <Color x:Key="SurfaceContainerHighColor">#242320</Color>
    <Color x:Key="SurfaceContainerHighestColor">#2A2926</Color>

    <!-- On Surface (Text) -->
    <Color x:Key="OnSurfaceColor">#EAE7DF</Color>
    <Color x:Key="OnSurfaceVariantColor">#A8A49A</Color>

    <!-- Outline (Borders) -->
    <Color x:Key="OutlineColor">#282724</Color>
    <Color x:Key="OutlineVariantColor">#363431</Color>

    <!-- Custom semantic brushes -->
    <SolidColorBrush x:Key="StatusPendingBrush" Color="#D4A64E" />
    <SolidColorBrush x:Key="StatusApprovedBrush" Color="#5FB89E" />
    <SolidColorBrush x:Key="StatusReviewBrush" Color="#6B9FC8" />
    <SolidColorBrush x:Key="StatusFlaggedBrush" Color="#C75B5B" />
    <SolidColorBrush x:Key="StatusPendingDimBrush" Color="#1F1C14" />
    <SolidColorBrush x:Key="StatusApprovedDimBrush" Color="#141F1C" />
    <SolidColorBrush x:Key="StatusReviewDimBrush" Color="#14191F" />
    <SolidColorBrush x:Key="StatusFlaggedDimBrush" Color="#1F1416" />
</ResourceDictionary>
```

---

## 2.3 Typography

### Font Stack

| Role | Family | Fallback |
|---|---|---|
| Display + Body | Outfit | Segoe UI Variable, system-ui |
| Mono / Data | JetBrains Mono | Cascadia Code, SF Mono, Consolas |

Fonts must be bundled as app assets in `Assets/Fonts/` and registered in the `AppHead` project.

### Type Scale (Mapped to Material TextBlock Styles)

| Style Key | Family | Weight | Size | Line Height | Usage |
|---|---|---|---|---|---|
| `DisplaySmall` | Outfit | 700 | 26px | 32px | KPI values, hero numbers |
| `HeadlineSmall` | Outfit | 700 | 17px | 24px | Page titles, section headers |
| `TitleMedium` | Outfit | 600 | 14px | 20px | Card titles, PO IDs (detail header) |
| `TitleSmall` | Outfit | 600 | 13px | 20px | Sub-section titles |
| `BodyLarge` | Outfit | 500 | 12px | 20px | Table cells, descriptions, vendor names |
| `BodyMedium` | Outfit | 400 | 11.5px | 16px | Activity messages, AI analysis text |
| `BodySmall` | Outfit | 400 | 11px | 16px | Secondary descriptions, subtitles |
| `LabelLarge` | Outfit | 600 | 11px | 16px | Button labels, nav items, filter chips |
| `LabelMedium` | JetBrains Mono | 600 | 10px | 16px | Amounts, SLAs, PO numbers, SKUs |
| `LabelSmall` | JetBrains Mono | 600 | 9px | 12px | Table headers, section labels, timestamps, badges |

### TextBlockStyles.xaml

```xml
<ResourceDictionary>
    <!-- Override Material type scale with Outfit -->
    <FontFamily x:Key="ContentControlThemeFontFamily">ms-appx:///Assets/Fonts/Outfit-Variable.ttf#Outfit</FontFamily>

    <!-- Data-specific mono style -->
    <Style x:Key="MonoDataStyle" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="ms-appx:///Assets/Fonts/JetBrainsMono-Regular.ttf#JetBrains Mono" />
        <Setter Property="FontSize" Value="10" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="CharacterSpacing" Value="40" />
    </Style>

    <Style x:Key="MonoLabelStyle" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="ms-appx:///Assets/Fonts/JetBrainsMono-Regular.ttf#JetBrains Mono" />
        <Setter Property="FontSize" Value="9" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="CharacterSpacing" Value="80" />
        <Setter Property="Foreground" Value="{StaticResource OnSurfaceDisabledBrush}" />
    </Style>
</ResourceDictionary>
```

---

## 2.4 Layout Grid

### App Shell

```
┌────────────────────────────────────────────────────┐
│                    TopBar (48px)                     │ Grid.Row="0" Grid.ColumnSpan="2"
├──────────┬─────────────────────────────────────────┤
│          │                                         │
│   Nav    │              Main Content               │ Grid.Row="1"
│  (200px) │                (flex)                   │
│          │                                         │
├──────────┴─────────────────────────────────────────┤
│                   StatusBar (28px)                   │ Grid.Row="2" Grid.ColumnSpan="2"
└────────────────────────────────────────────────────┘
```

### Dashboard Layout

```
┌─────────────────────────────────────────────────────┐
│  [ KPI ] [ KPI ] [ KPI ] [ KPI ] [ KPI ]  ← flex row, 10px gap
├────────────────────────┬────────────────────────────┤
│    Order Pipeline      │       Activity Feed        │ ← 2-col grid, 14px gap
│    (status bar)        │       (event list)         │
│    (PO cards ×4)       │                            │
│    [View all →]        │                            │
└────────────────────────┴────────────────────────────┘
```

### Orders List Layout

```
┌─────────────────────────────────────────────────────┐
│  [Pending: 2] [Review: 2] [Approved: 2] [Flagged:1]│ ← 4 pipeline summary cards
├─────────────────────────────────────────────────────┤
│  [ 3 selected — Bulk Approve | Export | Clear ]     │ ← conditional bulk bar
├─────────────────────────────────────────────────────┤
│  ☐ │ PO#    │ Vendor         │ Amount  │ Status ... │ ← sticky header
│  ☑ │ PO-7421│ Kennametal     │ $48,200 │ Pending ...│ ← data rows
│  ☐ │ PO-7420│ Sandvik        │ $67,800 │ Flagged ...│
└─────────────────────────────────────────────────────┘
```

### Order Detail Layout

```
┌─────────────────────────────────────────────────────┐
│  PO-7420  [Flagged] [+$12K ceiling] [SLA: 1d 4h]   │ ← header
│  Sandvik Coromant · Fair Lawn, NJ · Milling Tools   │
│  Requested by Lisa Park (Production) · 5h ago       │
│                              [Approve][Reject][Esc] │ ← action buttons
├─────────────────────────────────────────────────────┤
│  [Overview] [Items] [Approvals] [History]           │ ← tab bar
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌── AI Analysis ──────────────────────────────┐    │ ← accent border card
│  │ This PO + PO-7417 = $122K vs $110K ...      │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ┌─ Order Details ─┐  ┌── Vendor ──────────────┐   │ ← 2-col grid
│  │ Items: ...      │  │ Sandvik Coromant       │   │
│  │ Ship: Jun 20    │  │ Risk: HIGH             │   │
│  │ Terms: Net 45   │  │ On-Time: 94%           │   │
│  └─────────────────┘  └────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### Warehouse Layout

```
┌─────────────────────────────────────────────────────┐
│ [Build][Zone]│[Erase]│[Pallet][Rack][...]│Layout A/B│ ← toolbar
├─────────────────────────────────────────────────┬───┤
│                                                 │ ▲ │
│            Isometric Canvas                     │ █ │ ← layer
│         (SKXamlCanvas, full bleed)              │ ░ │    scrubber
│                                                 │ ░ │
│                                                 │ ▼ │
│  POS 6,8 · H2 · 42 units                       │H2 │
└─────────────────────────────────────────────────┴───┘
```

---

## 2.5 Spacing Scale

All spacing values derived from a 4px base: **4, 6, 8, 10, 12, 14, 16, 18, 20, 24, 32, 48**

| Context | Value |
|---|---|
| Card internal padding | 16–18px |
| Section gap (dashboard panels) | 14px |
| KPI card gap | 10px |
| Table cell padding | 10–12px horizontal, 10px vertical |
| Nav item padding | 8–10px |
| Badge internal padding | 2px vertical, 8px horizontal |
| Button padding | 6px vertical, 12px horizontal |
| Toolbar gap | 5–6px between items |

---

## 2.6 Border Radius Scale

| Context | Radius |
|---|---|
| Cards, panels, KPI tiles | 10px |
| Buttons, filter chips, dropdown | 6px |
| Status badges, AI chips, risk badges | 4px |
| Layer indicator bars | 2px |
| User avatar | 50% (circle) |
| Toast | 10px |
| Command palette | 12px |

---

## 2.7 Elevation

GRIDFORM uses a flat aesthetic with border-defined surfaces. Elevation is reserved for overlays:

| Element | Shadow | Z Translation |
|---|---|---|
| Cards, panels | None (border-only) | 0 |
| Notification dropdown | `ThemeShadow`, Translation `0,0,32` | 32 |
| Command palette | `ThemeShadow`, Translation `0,0,48` | 48 |
| Toast | `ThemeShadow`, Translation `0,0,24` | 24 |

---

## 2.8 Iconography

The prototype uses Unicode symbols. For Uno implementation, use `FontIcon` with **Segoe Fluent Icons** (Windows/WASM) with fallback to **Material Symbols** (via Uno Material).

| Prototype Symbol | Fluent Glyph | Purpose |
|---|---|---|
| ◫ | `E80F` (GridView) | Dashboard |
| ◆ | `E913` (ViewAll) | Warehouse |
| ◇ | `E8A5` (Document) | Orders |
| ⊞ | `E8F1` (Package) | Inventory |
| ⊟ | `E779` (People) | Vendors |
| ⊡ | `E8D4` (Attach) | Contracts |
| 🔔 | `EA8F` (Ringer) | Notifications |
| ✓ | `E73E` (CheckMark) | Approval done |

---

## 2.9 Component Visual Specs

### StatusBadge

```
┌─[●]─[Pending]─┐
└────────────────┘
  4px radius
  Dot: 5×5 circle, status color
  Text: LabelSmall (mono, 10px, 600)
  Padding: 2px 8px
  Background: status dim color
  Foreground: status color
```

### KpiCard

```
┌────────────────────────┐
│ Open POs               │ ← BodySmall, tertiary
│ 8  [↑ 12%]            │ ← DisplaySmall + delta badge
│ vs last quarter         │ ← MonoLabel, muted
│━━━━━━━━━━━━━━━━━━━━━━━━│ ← 2px accent strip at bottom
└────────────────────────┘
  10px radius, SurfaceContainer background
  16px 18px padding
  Border: 1px Outline
```

### ApprovalChain Step

```
  ┌─(✓)─┐  James Chen        Requester
  │  │   │  Jun 2, 9:14 AM
  │  │   │
  │  ②   │  You               Manager Review    [CURRENT]
  │  │   │  Pending
  │  │   │
  │  ③   │  Finance           Budget Check
  │  │   │  Waiting
  │  │   │
  └──④───┘  VP Ops            Final (>$25K)
             Waiting

  Done: Green circle + ✓, green connecting line
  Current: Amber circle + number, amber "CURRENT" badge
  Waiting: Gray circle + number, default line
```

