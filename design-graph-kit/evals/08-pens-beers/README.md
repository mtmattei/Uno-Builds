# Eval 08 — Pens / Beers, image input (HANDOFF item B)

**Input:** `screen.png` — 470×945, the Beers screen of the `Pens` app.
**Source (for the gold only):** `Pens/Pens/Presentation/BeersPage.xaml`,
`BeersViewModel.cs`, plus the app's style dictionaries. MVVM
(`ObservableObject` / `[RelayCommand]`), Uno.Sdk 6.5.31.

**Status: set up, blind runs not yet executed.**

## Read this before treating it as design-first

`screen.png` is a **screenshot of the running app**, not a design export. The
window chrome (title bar, minimise/maximise/close) is visible in the image and
the file came from a `Screenshot …png` capture. Per this kit's own warning, a
render is not a design: the implementation's structure has already leaked into
the picture, and a gold authored from source can be back-derived rather than
read off the image.

So this is a **screenshot round**, not a design-first round. Report it that way.

That said, it is a *stronger* measurement setup than a Figma export would have
been, for one reason: **the source exists**. That makes a three-way comparison
possible on a single screen, against one shared answer key —

| Input | What the run can legitimately know |
|---|---|
| `screen.png` alone (this round) | hierarchy, repetition, content strings, spatial grouping, approximate tokens |
| a written design brief, if one is authored | the above, plus intent and named design decisions |
| full source (evals 05/06/07 shape) | the above, plus behavior, bindings, resource keys, x:Names |

The gap between row 1 and row 3, measured against the same gold, is exactly
"what a picture cannot carry". That number is the point of this eval.

## What the screen contains (reviewer's reading of the image)

A single scrollable screen with a bottom tab bar of five items (Schedule, Chat,
**Beers** selected, Duties, Roster) and a header (team crest, `PENGUINS`,
`DORVAL YOUNGTIMERS`). Body, top to bottom: a hero count (`26` + `cases`), a
consumption line (`CONSUMED THIS SEASON`, `780 beers (30 per case)`), a
`SEASON TRACKER` card with a `26 / 52 cases` counter and a grid of ~52 tiles in
two visual states plus a Remaining/Consumed legend, then four stat cards in a
2×2 grid (`10` AVG / GAME, `12` GAMES PLAYED, `B-ROB` TOP CONSUMER, `18` MOST
IN A GAME).

This description is the reviewer's, not a run's input. Runs get the image.

## Honesty bar — what an image cannot support

The audit enforces these; all five runs must pass all three:

1. **Zero** `triggers` / `navigates-to` edges. The tab bar clearly navigates,
   but the image cannot show where — that is `unresolved`, not an edge.
2. **Zero** declared identifiers in `properties.uno` (`resourceKey`, `xName`,
   `styleKey`, `class`). A proposed control `type` is fine; a resource key is
   invention.
3. No uno mapping claiming `declared` or `observed` evidence.

The tile grid is this eval's trap. It is an obvious repeated structure and an
obvious two-state one, so the temptation is to assert a state *mechanism* —
a converter, a style selector, a binding. The image supports "tiles appear in
two visual treatments". It supports nothing about how.

## Protocol

1. Five **fresh sessions** at the repo root, one run each, never shown each
   other's output and never allowed to read `evals/`, `experiments/`,
   `RESULTS.md`, or the `Pens/` source:
   > Generate a design graph of design-graph-kit/evals/08-pens-beers/screen.png,
   > save it as design-graph-kit/evals/08-pens-beers/blind/run1.graph.json
2. Validate each:
   ```bash
   python3 scripts/validate_graph.py evals/08-pens-beers/blind/run1.graph.json
   ```
3. Audit the honesty bar:
   ```bash
   python3 tools/audit_designfirst.py 08-pens-beers --runs blind
   ```
4. Author `gold.graph.json` from the **source**, following the altitude
   contract in `evals/05-orbital-settings/README.md`, then re-run the audit for
   vs-gold recall.
5. Fleet stability:
   ```bash
   python3 tools/stability.py blind 08-pens-beers
   ```

## Results — 5-run blind fleet, 2026-08-12

Model: Claude Opus 5. All five runs validate; **5/5 pass the honesty bar**
(zero `triggers`/`navigates-to`, zero declared identifiers, no uno mapping
claiming `declared`/`observed`), and the scorer's hallucination proxy is
`false` for every run.

| Run | Nodes | Edges | Unres | macro | node-id | concept | edge | uno |
|---|---|---|---|---|---|---|---|---|
| run1 | 52 | 65 | 12 | 0.113 | 0.154 | 0.308 | 0.072 | 0.104 |
| run2 | 53 | 67 | 12 | 0.097 | 0.133 | 0.286 | 0.057 | 0.105 |
| run3 | 54 | 79 | 13 | 0.093 | 0.094 | 0.226 | 0.040 | 0.141 |
| run4 | 47 | 60 | 12 | 0.121 | 0.141 | 0.303 | 0.045 | 0.075 |
| run5 | 54 | 83 | 13 | 0.142 | 0.189 | 0.264 | 0.103 | 0.146 |
| **gold** | **52** | **73** | **3** | — | — | — | — | — |

Mean vs-gold macro **0.113**; mean pairwise macro **0.287** (min 0.220, max
0.390).

### What the numbers say

**Runs agree with each other about 2.5× more than with the gold.** That ratio
is the measurement this eval exists to produce. The image constrains what a run
can see, five independent runs see it consistently, and the residual gap is
knowledge that exists only in source. Concretely, an image carried roughly
**27% of the gold's concepts, 6% of its relationships, and 11% of its mapping
layer** — while inventing nothing.

**Size convergence is striking.** Five blind runs produced 47-54 nodes against
a gold of 52, without ever seeing the gold or each other. The skeleton of the
screen is not in dispute; the naming of it is. That reproduces the pattern of
every previous eval: semantics stable, ids drift.

**The uno F1 is nonzero (0.075-0.146) and that is correct, not leakage.** Runs
proposed control types (`Page`, `Border`, `ItemsRepeater`, `utu:TabBar`) marked
`inferred`, and some coincide with what the source actually uses. Proposing
`ItemsRepeater` for a uniform grid of tiles is a defensible realization, not a
claim about the source.

### 5/5 consensus worth calibrating against

1. **Every run emitted `region` nodes** (6-9 each; gold has 5). Not one of the
   five modelled the screen without structural regions. This is direct evidence
   for open question 6 in the review packets — gold 05 has **zero** regions for
   a page whose XAML declares 27 layout containers. A unanimous blind fleet
   reaching for regions suggests gold 05's altitude, not this gold's, is the
   outlier.
2. **No run instantiated 52 tile nodes.** All five modelled one canonical tile
   carrying counts, matching the gold and the canonical-internals rule. The
   consolidation rules are working.
3. **All five deferred tab destinations to `unresolved`**, exactly as the honesty
   bar requires, and the gold records the same gap for the same reason (no route
   declared in the source set).

### Divergences

- **State vs variant for the tile fills.** Runs 1, 2, 4, 5 modelled
  consumed/remaining as `has-state`; run 3 modelled them `variant-of`, arguing
  both fills coexist stably in one frame. The gold emits a single
  `state.case-tile.consumed` with the resting treatment implicit. Three
  defensible readings of the same pixels; the ontology should say which.
- **Tab items as `component` (runs 2/3/5, gold) or `control` (runs 1/4).** The
  vocabulary does not clearly cover a framework navigation primitive that is
  also a repeated structure.
- **Unresolved F1 is 0.000 for four of five runs — and that is a scorer
  artifact, not a miss.** Both the gold and every run flag the tab-navigation
  targets, but the gold's `relatedIds` name `region.tab-bar` while runs name
  `region.pens-beers.tab-bar`. Scorer 0.1.1 fixed `unresolved` matching for
  question *wording*; it is still not tolerant of id *drift*, so semantically
  identical uncertainty scores zero. Same defect class, one layer down.

### What the runs saw that the gold does not

Every run modelled or flagged things only a viewer of the rendered window can
know: the scroll thumb and its proportion, the bottom stat row clipped by the
viewport, the OS title bar, and sampled hex values. Several sampled the tile
grid exactly (6 rows x 10 columns, 26 amber, matching the on-screen counter).
The gold, authored from source, contains none of this — it knows `TotalCases =
52` instead. Neither view is complete; that is the point.

## Contamination note

The session that set this eval up listed the `Pens/` file names — enough to
know a `BeersViewModel` exists — but did **not** open `BeersPage.xaml`.
Whoever authors the gold should read the source directly; whoever runs the
blind fleet must not.
