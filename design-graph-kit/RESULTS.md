# Design Graph Kit — Test Run Results

Run date: 2026-08-12 · Models: Claude Opus 4.8 (run 1), Claude Fable 5 (review, run 2, A/B) · Kit: v0.1 → v0.1.1

This document records the first execution of the Design Graph workflow against
a **real** Uno Platform screen from this repository, following `START-HERE.md`
and `docs/first-test.md`, plus the holistic review that followed and the
Stage-4/Stage-5 experiments.

## Progress against START-HERE

| START-HERE step | Status | Evidence |
|---|---|---|
| 1. Read `docs/first-test.md` | ✅ | — |
| 2. `python scripts/run_all.py` (verify kit) | ✅ | 4/4 bundled gold graphs pass |
| 3. Pick one simple real UI design | ✅ | `Orbital/Orbital/Presentation/SettingsPage.xaml` |
| 4. Hand-author the gold graph | ✅ | `evals/05-orbital-settings/gold.graph.json` (v1.1: 47 nodes / 58 edges / 3 unresolved) |
| 5. Generate via `prompts/design-understanding.md` | ✅ | run 1: `generated.graph.json`, macro F1 **0.6051** |
| 6. Validate + score | ✅ | all graphs validate; see log |
| 7. Repeat 5× for stability | ⏳ | not yet meaningful — see *Contamination*, below |
| 8. Test `SKILL.md` | ✅ | run 2: `skill.graph.json`, macro F1 **0.9742**, human avg 4.6 |
| 9. Value test (Design → Uno vs Design → Graph → Uno) | ✅ | `experiments/ab-orbital-settings/ab-results.md` — **B materially better on semantics** |
| 10. Productize decision | ⏳ | pilot says continue to Stage 6; blind replication + arm C required first |

## Why this design was chosen

`SettingsPage` is the kit's recommended first shape (a settings/profile
screen) with a crucial upgrade: full source — XAML, code-behind, a reusable
`PageHeader` control, and style dictionaries. That lets the graph exercise
what evals 01–04 only simulate: tokens from **declared** style resources,
**source-backed behavior** (`triggers` edges, entrance/confirmation states),
and a real consolidation problem (4 cards, 4 info-rows, 3 path-fields,
3 ghost buttons). It is the bridge from "understand a picture" to "understand
an application" — the direction `docs/architecture.md` points.

## Holistic review → adjustments (v0.1.1)

A review pass over run 1 produced three corrections, all committed:

1. **Answer-key recall defects (gold v1.1).** Gold had been authored from
   `SettingsPage.xaml` without expanding the `controls:PageHeader` reference.
   The header is a *declared reusable component* (not a plain region), and it
   renders a search / command-palette affordance (`Search or run command...`,
   Ctrl+K) that both gold and run 1 missed. Gold also omitted the
   code-behind-declared "Cleared" `ContentDialog` triggered by *Clear Recent
   Projects*. Both fixes are **source-driven** (the eval's own inputs prove
   them), which is the legitimate kind of answer-key correction —
   `first-test.md` forbids bending gold toward model output, not fixing it
   against the source. Lesson recorded: *hand-authored gold graphs need a
   completeness pass that expands every source-backed component reference.*
2. **Scorer fix (v0.1.1).** `unresolved` items were matched by exact
   `(question, relatedIds)` tuple, so two runs flagging the same ambiguity in
   different words scored 0.0 — a repeated failure across runs.
   `score_graph.py` now matches on sorted `relatedIds`. Run 1 was re-scored:
   macro F1 0.4907 → **0.6051**.
3. **Methodology honesty.** Run 1's "generated" graph was authored in the same
   session as gold with deliberately chosen divergences — useful for exercising
   the scorer, but it measures scorer sensitivity, not model behavior. This is
   now stated plainly here and in the scorecard.

## Results

### Run 1 — manual prompt (`design-understanding.md`)

Macro F1 **0.6051** vs gold v1.1 (nodes 0.67 / edges 0.59 / unresolved 0.50).
Human rubric 4.0/5, zero hallucinated behavior. Missed: the header-as-component,
the search affordance, the Cleared dialog, the entrance state.

### Run 2 — Stage 4, `SKILL.md`

Macro F1 **0.9742** (nodes 0.97 / edges 0.96 / unresolved 1.00). Human rubric
4.6/5, zero hallucinated behavior, `severe_hallucination_proxy: false`.
The skill's pass structure (inventory → expand source references → consolidate
→ states → tokens) recovered everything run 1 missed. Its only precision
misses are three *extra declared tokens* (emerald-500 accent, two text-emphasis
colors) plus their `uses-token` edges — defensible under `token-rules.md`,
flagged as a coverage-calibration question rather than an error.

**Stage-4 exit criterion met:** skill (0.974) ≥ manual prompt (0.605).

### Contamination caveat (applies to both runs)

Gold and both generated graphs were authored by the same agent lineage. Run 2's
near-perfect score is an **upper bound demonstrating the pipeline works
end-to-end**, not evidence of blind stability. The 5-run stability protocol
(START-HERE step 7) only becomes meaningful with genuinely independent
generation — fresh sessions with no access to gold. That is the first thing to
do next.

### Stage 5 — A/B value experiment

Protocol and outputs in `experiments/ab-orbital-settings/` (see its README for
the full design). Two isolated one-shot agents implemented the screen from the
same visual-only brief; arm B additionally received `skill.graph.json` +
`prompts/design-implement.md`. Full measurements: `ab-results.md`.

**Outcome: tie on visuals, decisive graph win on semantics.** Both arms
produced clean, fully tokenized, well-consolidated XAML (0 hardcoded hex in
either page; same style-reuse counts) — a good brief was sufficient for
pixels. But arm B implemented **3/3** source-backed behaviors (entrance
stagger matching the real app's timings, the 1.5 s "Saved!" flash, the
"Cleared" dialog with verbatim-identical copy) with **0** invented behaviors,
while arm A implemented **0/3** and **invented one** — it wired the docs
button to a guessed (wrong) URL. B's x:Names and 41 graph-id comments also
make Stage-6 round-trip parity tractable. The graph's value is semantic
transport across the handoff, not prettier XAML.

## Findings

1. **The kit runs end-to-end on a real Uno screen.** Validation, integrity
   checks, and scoring all work; the integrity check caught a real authoring
   defect (inferred edge missing a rationale) during the very first run.
2. **Source-backed input changes the honesty profile.** With code-behind
   available, behavior that eval 04 must leave `unresolved` becomes legitimate
   `triggers`/`has-state` structure — more useful *without* inventing anything.
3. **Answer keys rot the same way code does.** The gold graph itself had two
   recall defects until the review expanded every source reference. Gold
   authoring needs its own checklist, not just model evaluation.
4. **Exact-string scoring needs one more normalization step** (fixed for
   `unresolved` in v0.1.1). Canonical-name drift on one consolidation choice
   still cascades through child ids and edges by design — acceptable for a
   drift tripwire, but don't read macro F1 as quality. The human rubric is the
   quality instrument.
5. **No ontology expansion is warranted yet.** Every divergence so far was
   representable in v0.1. Watch item: distinguishing tokens sourced from
   *declared style resources* vs *derived* values, and where declared-resource
   token extraction should stop.

## How to reproduce

```bash
cd design-graph-kit
python -m pip install -r requirements.txt
python scripts/run_all.py    # validates all gold graphs + scores generated.graph.json
python scripts/validate_graph.py evals/05-orbital-settings/skill.graph.json
python scripts/score_graph.py \
  evals/05-orbital-settings/gold.graph.json \
  evals/05-orbital-settings/skill.graph.json --json
```

## Next steps

1. **Blind replication** — regenerate the graph in fresh sessions (no gold
   access), 5×, and measure stability for real (START-HERE step 7).
2. **Arm C** (brief + free-prose behavior notes) to separate "graph as
   structured IR" from "more information" — the pilot's main open confound.
3. **Stage 6 round-trip parity** — re-extract a graph from arm B's
   implementation and diff it against `skill.graph.json`; B's graph-id
   traceability was built for exactly this.
4. Compile/render both arms before trusting the visual-parity tie.
