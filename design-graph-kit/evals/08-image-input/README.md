# Eval 08 — image input (HANDOFF item B)

**Status: scaffolded, waiting on the design image.**

Every round so far fed the generator either full source (evals 05/06/07) or a
*written* visual brief (the design-first pilot). This round uses a real image,
which is the production input shape: a designer hands over a Figma frame, not a
prose description of one.

## What to drop here

```
evals/08-image-input/
  design.png     <- the design, exported at 1x or 2x, one screen
  notes.md       <- optional designer annotations, verbatim
  blind/         <- run1..run5.graph.json (created by the runs)
  gold.graph.json <- authored by hand FROM THE IMAGE, after the runs
```

Keep round one to a single, simple screen. A settings, profile, or detail
screen is the shape the kit is calibrated on.

## Why the image must be a real design, not a render of our own app

A screenshot of an already-implemented screen is a tempting shortcut and it
measures the wrong thing: the implementation's structure leaks into the
picture, and the gold can be back-derived from source rather than read off the
image. If the only available image is a render, say so explicitly in this
README when reporting results — the run is still useful, but it is a
*screenshot* round, not a *design-first* round.

## Protocol

1. Export one screen as `design.png`. Add designer annotations as `notes.md`.
2. In Claude Code at the repo root, in a **fresh session** (the `design-graph`
   skill loads automatically):
   > Generate a design graph of design-graph-kit/evals/08-image-input/design.png,
   > save it as design-graph-kit/evals/08-image-input/blind/run1.graph.json
3. Repeat in 4 more fresh sessions (`run2`..`run5`). Fresh sessions **are** the
   blind protocol — never show one session another's output, and never let a
   session read `evals/`, `experiments/`, or `RESULTS.md`.
4. Validate every run:
   ```bash
   python3 scripts/validate_graph.py evals/08-image-input/blind/run1.graph.json
   ```
5. Audit the honesty bar — the whole point of this round:
   ```bash
   python3 tools/audit_designfirst.py 08-image-input --runs blind
   ```
   All three checks must pass on all five runs: zero `triggers`/`navigates-to`
   edges, zero declared identifiers (`resourceKey`, `xName`, `styleKey`,
   `class`) in `properties.uno`, and no uno mapping claiming `declared` or
   `observed` evidence. An image cannot show behavior or name a resource key,
   so any of those is invention.
6. Author `gold.graph.json` **by hand from the image**, ideally by the same
   human doing the item C review. Follow the altitude contract in any eval
   README.
7. Measure stability across the fleet:
   ```bash
   python3 tools/stability.py blind 08-image-input
   ```

## What this round is expected to show

The design-first pilot (`evals/05-orbital-settings/design-first/`) is the
closest prior: five runs from a written brief, 5/5 honesty-perfect, structural
skeletons identical, and a vs-gold gap that is exactly "the information a
static mock cannot know". This round tests whether an **image** holds that line
as well as prose did, or whether pixels invite more guessing than sentences do.

Expected to be recovered: hierarchy, repeated structures and their
consolidation, content strings, spatial grouping, approximate tokens.
Expected to be absent, honestly: behavior, bindings, resource keys, x:Names,
anything about the code behind the picture.

## Closing the loop

Implement from the best run's graph (`prompts/design-implement.md`, Mode 2 of
the skill) and compile it in `experiments/ab-orbital-settings/ArmHost` — the
host built for item A takes any page. Design → graph → running app, end to end.
