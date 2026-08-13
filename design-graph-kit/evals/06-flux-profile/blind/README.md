# Eval 06 Blind Runs — first-contact generalization test

Five fresh-context blind agents against a screen the kit's rules were never
tuned on: FluxTransit ProfilePage (MVUX, Toolkit controls, glass design
system). Same protocol as eval 05; scorer 0.3.0; gold at the calibrated
v0.4 altitude. All five validated.

## Headline — the v0.4 rules generalize

| Metric | eval 05 (blind-v4, home turf) | **eval 06 (first contact)** |
|---|---:|---:|
| mean vs-gold macro F1 | 0.34 | **0.43** |
| mean vs-gold node-id F1 | 0.49 | **0.56** |
| mean vs-gold `uno_mapping` F1 | 0.76 | **0.79** (up to 0.89) |
| nodes per run | 60 ±1 (gold 47) | 48–55 (gold 57) |
| hallucinations | 0 | **0** |

Every rule held on first contact: sizes landed at gold's altitude, no
style-level states, token sets stayed screen-scoped (23 vs gold 21 — the
closest yet), and the mapping layer transported the Flux design system's
declared identity (resource keys, style keys, Toolkit types like
`utu:ChipGroup`) at 0.71–0.89 exact-triple F1.

## The honesty traps all held — and runs dug deeper than gold

This screen was chosen for its four traps (stack-dependent Back target,
unbound Add button, orphaned `SaveSettings` command, literal-valued
chips/toggle). **No run invented behavior for any of them** — all five kept
exactly the one declared trigger (`UpdateBalance → refreshing`), and every
run surfaced 6–9 unresolved items vs gold's 4, catching the traps plus
honest extras (e.g. per-control binding questions). MVUX bindings were read
correctly as declared behavior; XAML literals were not.

## Dominant residual: vocabulary coverage on new pattern types

Pairwise node-id agreement (0.60–0.86, mean ~0.76) sits below eval 05's
0.93, and the drift concentrates on exactly two canonical names the Pass-8
vocabulary doesn't cover:

- the glass section container: runs split between `component.card`
  (following the vocabulary literally) and `component.section-card`;
  **gold itself said `component.glass-panel`, violating the kit's own
  "grouping container → card" rule** — an answer-key defect this eval
  exposed;
- the route row: 4/5 runs converged on `component.route-card` vs gold's
  `component.route-item` (the vocabulary has no list-row entry).

This is precisely the failure mode the vocabulary's "extend through eval
review, not per-run invention" line anticipated. Next-iteration decisions
(explicit, per protocol — not quiet edits): rename gold's canonical to
`component.card` to obey the kit's own rule, and add a `route`/list-row
entry (or a generic `list-item`) to the Pass-8 vocabulary. Those two names
account for most of the id and edge drift.

## Verdict

Generalization: **pass**. First-contact scores beat the home-turf eval on
every vs-gold dimension, honesty discipline held against four designed
traps, and the residual drift is narrowly attributable to two missing
vocabulary entries plus one answer-key naming defect — all rule-shaped,
none behavioral.

## Reproduce

```bash
python scripts/score_graph.py evals/06-flux-profile/gold.graph.json \
  evals/06-flux-profile/blind/run1.graph.json   # …run5
```
