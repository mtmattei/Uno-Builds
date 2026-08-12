# 07-caffe-main

Third **real, source-backed** eval — completing architecture coverage:

| Eval | App | Architecture |
|---|---|---|
| 05 | Orbital Settings | code-behind (Click handlers) |
| 06 | FluxTransit Profile | MVUX (`IState`/`IFeed`, model commands) |
| **07** | **Caffe Main** | **MVVM** (CommunityToolkit `ObservableObject`, `[ObservableProperty]`, `[RelayCommand(CanExecute)]`, `x:Bind`) |

The screen: an espresso-machine dashboard — 2×2 menu of `EspressoCard`
UserControls bound to a hard-coded `ObservableCollection`, three custom
parameter gauges with TwoWay `x:Bind`, a `SelectionOverview` gated by
`HasSelection`, a `BrewButton` driving `BrewCommand`, and a full-screen
`BrewingScreen` overlay `x:Load`-ed by `IsBrewing`.

## What this eval tests (beyond 05/06)

- **MVVM evidence discipline**: `[ObservableProperty]` + computed properties
  + `RelayCommand.CanExecute` are declared behavior; the selection flow runs
  through code-behind Tapped handlers into the ViewModel.
- **Composed-control altitude**: the page is built almost entirely from
  9 custom UserControls — canonical components/controls with `uno.class`,
  internals as `properties`, not expanded trees.
- **Two interacting screen states**: `selected` (overview + CanExecute +
  card highlights) and `brewing` (whole-content swap to an overlay), each
  with declared triggers (card Tapped → selected; Brew → brewing).
- A light-theme design system (previous evals were dark).

## Granularity altitude (binding — same contract as evals 05/06)

- IDs per SKILL.md Pass 8 (vocabulary v0.4.2); scope slugs `header`,
  `menu`, `parameters`, `main`, `footer`.
- Custom controls: one node each (`component` if compositional, `control`
  if it is an interactive input), `uno.class` carrying the declared type;
  parts as `properties`.
- Tokens: screen-consumed declared resources (18 in gold), consensus wiring
  (33 `uses-token` edges); interaction/disabled shades and gauge-internal
  ramp colors are folded per the variant rule.
- States attach to the screen (both are screen-wide presentations); the
  state-gated components hang off their state via `contains` (eval-03
  pattern).

## Files

- `gold.graph.json` — hand-authored answer key (39 nodes / 61 edges /
  1 unresolved; same-author caveat applies).
- `blind/` — fresh-context blind replication runs.
