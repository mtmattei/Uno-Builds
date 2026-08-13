# Design Brief: "Orbital" Settings Screen

Implement this screen for a desktop-first dark-theme developer tool. This brief
describes a static design mock (populated state). All measurements in px.

## Overall

- Page background: `#0A0A0B` (near-black).
- Cards: background `#141416`, 1px border `#212123`, corner radius 12, padding 20.
- Two font families: a geometric display/sans face for titles and body, and a
  monospace face for labels, values, and small text.
- Text colors (white-ish, by emphasis): high `#D6D9E1`, medium `#B5B9C5`,
  muted `#7B8191`, faint `#5D6372`.
- Accent (used sparingly): emerald `#10B981`, with dark text on it.

## Header (full width, top)

- Bottom border 1px `#1A1A1C`; padding 32 horizontal, 20 vertical.
- Left: title `Settings` (display face, ~28, high emphasis), 4 below it the
  subtitle `Profile and preferences` (~13, faint).
- Right: a search pill (background `#141416`, 1px border `#212123`, radius 8,
  padding 12×8) containing a small search glyph, the mono text
  `Search or run command...` (~12, faint), and a small `Ctrl+K` keycap badge
  (background `#1A1A1C`, radius 4, mono ~10).

## Content area

Scrollable; padding 32; vertical gap between cards 24.

### Card 1 — PROFILE (full width)

- Section header `PROFILE`: mono, ~11, uppercase, faint `#5D6372`. 16 gap to content.
- Field label `Display Name`: mono ~11, muted `#7B8191`; 8 below it a text input
  (background `#141416`, 1px border `#212123`, mono ~13, placeholder
  `Enter your name`) with, 8 to its right, a compact filled button `Save`
  (emerald `#10B981` background, dark text, radius 8, padding 12×6, ~13 medium).
- 8 below: helper text `This name appears in the homepage greeting.` (mono ~11,
  faint `#5D6372`).

### Two-column row (16 gutter)

**Left column — Card 2 — ABOUT**

- Section header `ABOUT` (same treatment as PROFILE header).
- Identity row: a 48×48 tile (background `#1A1A1C`, radius 12) containing a
  32×32 product logo; 16 to the right, stacked: `Orbital` (~13 semibold, high
  emphasis) and `v0.1.0-alpha` (mono ~11, faint), 4 apart.
- Below, 16 gap, then four rows stacked 8 apart. Each row: label left
  (mono ~11, muted `#7B8191`), value right-aligned (mono ~11, medium
  `#B5B9C5`). Rows: `Uno Platform SDK`, `.NET Runtime`, `Renderer`,
  `Platform` — values in the mock read `...` (still loading).

**Right column — Card 3 — PATHS, then 24 below it Card 4 — ACTIONS**

- PATHS: section header `PATHS`; three field groups stacked 16 apart. Each:
  label (mono ~11, muted) with, 8 below, a wrapping value line (mono ~11,
  medium). Labels: `Project Root`, `Recent Projects Database`,
  `Claude Code Skills` — values in the mock read `...`.
- ACTIONS: section header `ACTIONS`; 12 below, three full-width, left-aligned
  quiet buttons stacked 12 apart (transparent background, radius 8, padding
  12×6, mono/sans ~13, medium-muted text). Each has a small leading icon and a
  label: 🗑 `Clear Recent Projects`, 📁 `Open Data Folder`,
  📖 `Uno Platform Documentation`.

## Notes

- The mock shows only this single populated state.
- Match the design as closely as the framework allows; keep the result
  maintainable.
