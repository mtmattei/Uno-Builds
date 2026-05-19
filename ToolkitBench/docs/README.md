# Uno Toolkit Bench

A reference Uno Platform app: five `Uno.Toolkit.UI` components in a row-based spec sheet, each with a signature animation chosen for its distinct mechanical/material character. A carbon copy of [`uno-toolkit-bench-rows.html`](./uno-toolkit-bench-rows.html) — but built with real `utu:` controls, native XAML composition, and MVUX state.

> Inspired by [webuibench.dev](https://webuibench.dev). Restraint borrowed from Naoto Fukasawa.

---

## Goal

Demonstrate that `Uno.Toolkit.UI`'s most-used primitives — `NavigationBar`, `TabBar`, `ChipGroup`, `DrawerControl`, `LoadingView` — can support distinctive interactions that borrow from physical/mechanical references rather than the standard web-animation vocabulary.

The five gestures, in order:

| # | Component | Signature animation |
|---|---|---|
| 01 | `utu:NavigationBar` | Split-flap title cascade (Solari board) |
| 02 | `utu:TabBar`        | Identity morph — each icon transforms into a more specific variant when active |
| 03 | `utu:ChipGroup`     | Coin flip on selection; "All" tap cascades through every chip |
| 04 | `utu:DrawerControl` | Pull-tab drag with resistance + rubber-band + bounce snap |
| 05 | `utu:LoadingView`   | Honest real-time timer (no spinner; the duration *is* the result) |

## Tech stack

- **Uno Platform 6.x** — single codebase, multi-targeted
- **Skia rendering** on both heads (visual parity)
- **MVUX** with `Feed<T>` for state
- **Uno.Toolkit.UI** + **Uno.Toolkit.WinUI.Material** for components

## Targets

| Head | TFM | Notes |
|---|---|---|
| Skia Desktop | `net9.0-desktop` | Primary dev target, 60fps stable |
| WebAssembly  | `net9.0-browserwasm` | Skia in browser, ~1–2s cold start |

Mobile heads (`-android`, `-ios`) are not in scope but trivial to add.

## Quick start

```bash
dotnet new install Uno.Templates
dotnet new unoapp -preset blank \
  -id com.unoplatform.bench \
  --presentation mvux \
  -platforms desktop wasm \
  -o UnoToolkit.Bench

cd UnoToolkit.Bench
dotnet add src/UnoToolkit.Bench package Uno.Toolkit.WinUI
dotnet add src/UnoToolkit.Bench package Uno.Toolkit.WinUI.Material

# Skia desktop
dotnet run --project src/UnoToolkit.Bench -f net9.0-desktop

# WASM
dotnet run --project src/UnoToolkit.Bench -f net9.0-browserwasm
```

Template flag names shift between Uno versions — verify with `dotnet new unoapp -h` if the above fails.

## Layout overview

The page is a slim masthead bar (eyebrow + version meta), then five vertically-stacked rows. Each row is a 2-column grid:

- **Left column (320px)**: spec sheet — number, `utu:` component name, summary headline, motion tags
- **Right column (1fr)**: live demo, vertically centered, with hairline corner crosshairs suggesting a measurement frame

All five demos run simultaneously. Drawer is closed at rest; LoadingView is idle; NavigationBar is on page 1; TabBar is on Home; ChipGroup has "All" selected. Viewers see all five resting states at once and choose which to interact with.

## File structure

```
UnoToolkit.Bench/
├── README.md                ← you are here
├── docs/
│   ├── ARCHITECTURE.md      ← code structure, MVUX, project layout
│   ├── DESIGN.md            ← palette, typography, layout, visual specs
│   └── INTERACTIONS.md      ← per-animation timing & XAML strategy
└── src/UnoToolkit.Bench/
    ├── App.xaml
    ├── MainPage.xaml
    ├── Demos/               ← one UserControl per row
    ├── Models/              ← MVUX feeds
    ├── Controls/            ← SplitFlapText, MorphIcon, PullTabDrawer
    └── Themes/              ← Tokens.xaml, Bench.xaml
```

## Read order

For someone joining the rebuild:

1. **[ARCHITECTURE.md](./docs/ARCHITECTURE.md)** — project layout, MVUX rationale, animation orchestration strategy
2. **[DESIGN.md](./docs/DESIGN.md)** — every token, type style, dimension, gradient endpoint, visual element
3. **[INTERACTIONS.md](./docs/INTERACTIONS.md)** — exact storyboard timings, easings, key-frame values per component

Each is standalone-readable. Designers can stop after DESIGN; engineers should read all three.

## Out of scope

- Light theme — design is monochromatic dark by intent; a light variant would need its own gradient curve and accent strategy
- Localization of demo content
- Real navigation graph in the NavigationBar demo (3-page sample stack is hardcoded)
- Mobile heads
