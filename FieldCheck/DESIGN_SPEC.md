# FieldCheck v1 — Design Specification

## Design intent

FieldCheck should feel like a precise industrial instrument, not a generic admin template: quiet surfaces, strong information hierarchy, visible structure, and one high-visibility accent.

Keywords: **industrial field tool / editorial utility / restrained CAD instrument / clear, tactile, production-oriented**.

## Reference viewports

- Android reference: **412 × 915 px**, portrait.
- Windows reference: **1440 × 900 px**.
- Capture benchmark screenshots at these logical target sizes where the environment permits. If exact physical/device pixels differ, record logical viewport/window size and scaling factor.

## Color tokens

| Token | Value | Use |
|---|---|---|
| Canvas | `#F3F2ED` | App background |
| Surface | `#FFFFFF` | Elevated/working surfaces |
| Ink | `#171918` | Primary text/icons |
| Muted | `#6A6D69` | Secondary text |
| Rule | `#D8D8D1` | Hairlines/dividers |
| Accent | `#DFFF45` | Primary action/highlight |
| Success | `#176B4D` | Good/success status |
| Attention | `#A85C00` | Attention status |
| Critical | `#B42318` | Critical/error status |
| Soft Success | `#E8F3ED` | Success background |
| Soft Attention | `#F7EDDF` | Attention background |
| Soft Critical | `#F8E8E6` | Critical background |

Accent is not decorative. Reserve it for primary actions, active navigation markers and small emphasis surfaces.

## Typography

Use the native system sans-serif for the platform (for example Segoe UI on Windows and Roboto/system sans on Android). Do not download a custom font solely for this benchmark.

Suggested scale:

- Display metric: 40–48, semibold
- Screen title: 28–32, semibold
- Section title: 18–20, semibold
- Body: 15–16, regular
- Secondary: 13–14, regular
- Micro label: 11–12, medium/semibold, slight tracking where supported

Use tabular numerals for metric counts if readily available.

## Geometry

- Corners: 10–12 px on buttons/inputs/status containers; avoid excessive rounded cards.
- Hairline dividers: 1 px.
- Mobile horizontal margin: 20 px.
- Desktop content margin: 32 px.
- Base spacing unit: 4 px.
- Typical vertical gaps: 8 / 12 / 16 / 24 / 32.
- Touch targets: minimum 44 × 44 logical px.

## Component rules

### Navigation
Android uses bottom navigation with three destinations. Active destination uses Ink text/icon and a small Accent indicator. Keep chrome quiet.

Windows uses a 220–236 px persistent sidebar with product mark at top and destinations below. Active item has a subtle Accent marker/surface rather than a large filled pill.

### Rows rather than card grids
Asset and history collections are structured rows with separators. Do not wrap every list item in a floating card.

### Status
Status is expressed by a compact label/chip plus semantic color. Never rely on color alone; include the text label.

### Buttons
Primary: Accent background + Ink text, 48 px high minimum.
Secondary: transparent/Surface with Ink border or quiet text treatment.
Destructive styling is not needed in v1.

### Inputs
Use labeled fields with visible borders/rules and explicit validation text. Do not rely only on placeholder text as a label.

## Screen composition

### Dashboard
- Quiet top header.
- Large "12 assets" primary metric with three compact condition summaries.
- A thin rule then "Needs attention" rows.
- No card mosaic.

### Assets
Android: search, horizontal filter chips, vertical rows.
Windows: list/search/filter in a ~430 px master pane; selected detail fills remaining area.

### Asset Detail
Treat metadata as an information sheet: label/value pairs, status near title, description and latest inspection separated by rules. Primary action stays easy to reach.

### New Inspection
Single continuous form divided by section labels/rules rather than many cards. Required relationships must be obvious. The conditional issue description inserts into the flow without causing layout breakage.

### Success
Use generous negative space, a concise confirmation mark/message, inspection ID and two clear next actions. Do not add confetti or decorative illustration.

### History
Rows prioritize asset + condition, then timestamp/inspector. Search and filter mirror Assets patterns.

## Adaptive rules

- No horizontal scrolling for primary app content.
- Mobile list/detail is sequential navigation.
- Desktop Assets becomes master/detail at >= 1000 logical px.
- Long text wraps naturally.
- When window width contracts, controls can wrap/reflow before content clips.
- Keyboard focus indicators must be visible on Windows.

## Reference images

See `reference-ui/mobile/` and `reference-ui/windows/`. The contact sheets are overview-only; compare implementation screenshots to the individual PNGs.
