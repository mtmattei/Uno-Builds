# 05-orbital-settings

First **real, source-backed** eval case (evals 01–04 are synthetic).

Input: the existing `Orbital` Uno Platform `SettingsPage` (XAML + code-behind +
style dictionaries). See `fixture.md` for the exact source files and the facts
they support.

- `gold.graph.json` — hand-authored answer key, written from the source before
  generating anything.
- `generated.graph.json` — an independent generation pass following
  `prompts/design-understanding.md`, working primarily from the rendered
  structure (as a model reading the design would).
- `scorecard.md` — filled `docs/manual-scorecard.md` for the generated run.

Validate and score from the repository root (`design-graph-kit/`):

```bash
python scripts/validate_graph.py evals/05-orbital-settings/gold.graph.json
python scripts/validate_graph.py evals/05-orbital-settings/generated.graph.json
python scripts/score_graph.py evals/05-orbital-settings/gold.graph.json evals/05-orbital-settings/generated.graph.json
```

## Granularity altitude (v0.2 — read before authoring or generating)

This eval models **screen semantics**, not the design system:

- nodes represent what this screen shows and does — not the style
  dictionaries' internals;
- style-level interaction visuals (hover/pressed/disabled) are NOT `state`
  nodes here;
- tokens are limited to values this screen consumes (19 in gold v1.4), not
  the full palette. Gold v1.4 adopted the blind-v4 five-run **consensus
  wiring**: resting-surface text-emphasis/accent/typography tokens wired to
  their consumers (41 `uses-token` edges) — an explicit answer-key
  calibration, recorded in the CHANGELOG;
- ids follow the v0.2 grammar in SKILL.md Pass 8 (three segments max;
  canonical components two);
- `has-state` attaches to the smallest node whose presentation changes
  (gold v1.2: the Save button owns `state.profile.saved`);
- gold v1.3 carries the **Uno mapping layer** (`properties.uno`, see
  `references/uno-mapping.md`) on every node where the source declares a
  type/x:Name/style/resource key — mapping-layer recovery is scored exactly.

Blind v0.1 runs drifted on exactly these axes; they are now binding rules.

## Why source-backed matters

Because we have the real source, behavior that a screenshot would force into
`unresolved` (eval 04) is here `declared`/`observed`, so the graph can carry a
`triggers` edge (`Save` → transient `Saved!` state) and an entrance state
honestly. This case is the bridge between "extract structure from a picture"
and "extract structure from an application," which is the direction the
architecture doc points the graph toward.
