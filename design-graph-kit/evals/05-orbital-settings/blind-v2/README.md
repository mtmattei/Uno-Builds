# Blind Replication v2 — validating the v0.2 revisions

Same protocol as `../blind/` (five fresh-context agents, source + kit method
files only, no gold access), but against kit v0.2 (id grammar, naming
vocabulary, state altitude, token scoping). Both generations are scored with
the same scorer (0.2.0) and the same gold (v1.2), so the deltas below are
caused by the SKILL/reference revisions alone.

## v0.1 vs v0.2, same scorer, same gold

| Metric | v0.1 runs | v0.2 runs | Δ |
|---|---:|---:|---|
| mean vs-gold macro F1 | 0.114 | **0.256** | +125% |
| mean vs-gold node-id F1 | 0.154 | **0.484** | ×3.1 |
| mean pairwise macro F1 | 0.497 | **0.563** | +13% |
| mean pairwise node-id F1 | 0.707 | **0.814** | +15% |
| node ids identical in all 5 runs | 48 | **53** | ↑ |
| node ids in exactly 1 run (drift tail) | 72 | **30** | −58% |
| nodes per run | 85–90 | **68–81** | → gold's 47 |
| state nodes per run | ~9–10 (incl. style-level) | **2–4** (screen-level only) | rule held |
| hallucination flags | 5 (all false) | **0** | proxy + runs clean |
| behavioral edges | 2 real per run | 2 real per run | stable throughout |

## What the revisions fixed

- **Naming converged hard.** The vocabulary and grammar did their job: all
  five runs now say `component.info-row`, `component.path-field`,
  `control.profile.save` — gold's names. The drift tail (singleton ids)
  more than halved, and what remains is mostly slug-length variation
  (`component.info-row.dotnet` vs `component.info-row.dotnet-runtime`),
  not concept disagreement.
- **State altitude held in all five runs.** Zero style-level
  hover/pressed/disabled states; every run modeled exactly the real
  screen states (entering/saved, some the cleared-dialog) with the same
  two source-backed `triggers` edges.
- **Hallucination discipline: still perfect.** Ten blind runs total across
  both generations, zero invented behaviors.

## What still drifts (the v0.3 levers)

1. **`uses-token` inflation** — v0.2 runs emit 66–71 `uses-token` edges vs
   gold's 11, wiring tokens per-instance and per-child instead of once on
   the canonical component. Rule candidate: token edges attach to the
   canonical component (or the screen), never to instances.
2. **Canonical component internals** — runs model template children as
   nodes (`content.info-row.label`, `content.info-row.value`), one level
   deeper than gold's altitude. Rule candidate: a canonical component's
   internal parts are properties, not child nodes, unless an instance
   overrides them.
3. **Token set still ~2× gold** (27–32 vs 13) — scoping to "consumed by the
   surface" helped (was 37–38) but runs still tokenize per-use variants
   (`token.color.emerald500-20`, `token.color.surface15`) that gold folds
   into the interaction layer.

These three account for most of the remaining vs-gold gap; pairwise
agreement (what the runs share with *each other*) is already at 0.81
node-id F1.

## Verdict

v0.2 is validated: the revisions were the right fixes, with large, measured
improvements on exactly the axes the v0.1 data indicted. Still short of
productization stability — one more targeted iteration (the three levers
above), then re-run this protocol.

## Reproduce

```bash
python scripts/score_graph.py evals/05-orbital-settings/gold.graph.json \
  evals/05-orbital-settings/blind-v2/run1.graph.json   # …run5
```
