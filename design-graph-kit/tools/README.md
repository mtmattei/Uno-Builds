# Research tooling (provenance + analysis)

These scripts produced and analyzed everything in `evals/` and
`experiments/`. Preserved here so every answer key and every reported number
is regenerable.

## Gold-graph builders (answer-key provenance)

- `build_graphs.py` — eval 05 gold (Orbital Settings), all revisions v1.0 →
  v1.4 (page-header fix, saved-state attachment, uno mapping layer,
  consensus token calibration) are in its history via the repo log.
- `build_generated.py` — eval 05 run-1 manual-pass graph (historical).
- `build_gold06.py` — eval 06 gold (FluxTransit Profile), incl. the v1.1
  `card` rename.
- `build_gold07.py` — eval 07 gold (Caffe Main, MVVM), incl. the v1.1
  consensus calibration (screen slug, UserControl=component, per-site
  states, canonical triggers, token breadth).

Regenerate any gold: `python3 tools/build_gold07.py` (writes into `evals/`),
then `python3 scripts/validate_graph.py <gold>`.

## Analysis

- `stability.py <runs-dir> <eval-dir>` — vs-gold scores, pairwise drift
  matrix, and concept-stability table over a blind fleet, e.g.
  `python3 tools/stability.py blind 07-caffe-main`.
- `verify_arm.py <arm-dir>` — static verification of an implementation arm
  (XML well-formedness, resource-key resolution, event-handler resolution);
  the sanctioned check when no .NET toolchain is available.
- `audit_designfirst.py` — design-first honesty audit: zero behavioral
  edges, zero fabricated declared identifiers, structural recall vs the
  source-backed gold.

Paths inside the scripts are absolute for the session container; adjust the
`KIT`/`OUT` constants when running elsewhere.
