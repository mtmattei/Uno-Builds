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

## Contamination note

The session that set this eval up listed the `Pens/` file names — enough to
know a `BeersViewModel` exists — but did **not** open `BeersPage.xaml`.
Whoever authors the gold should read the source directly; whoever runs the
blind fleet must not.
