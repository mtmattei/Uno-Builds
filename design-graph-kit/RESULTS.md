# Design Graph Kit — Test Run Results

Run date: 2026-08-12 · Model: Claude (claude-opus-4-8) · Kit: v0.1

This document records the first execution of the Design Graph workflow against
a **real** Uno Platform screen from this repository, following `START-HERE.md`
and `docs/first-test.md`.

## What was done

| START-HERE step | Status | Evidence |
|---|---|---|
| 1. Read `docs/first-test.md` | ✅ | — |
| 2. `python scripts/run_all.py` (verify kit) | ✅ | 4/4 gold graphs pass |
| 3. Pick one simple real UI design | ✅ | `Orbital/Orbital/Presentation/SettingsPage.xaml` |
| 4. Hand-author the gold graph | ✅ | `evals/05-orbital-settings/gold.graph.json` (45 nodes / 56 edges / 2 unresolved) |
| 5. Generate via `prompts/design-understanding.md` | ✅ | `evals/05-orbital-settings/generated.graph.json` (40 / 51 / 1) |
| 6. Validate + score | ✅ | validate PASS; macro F1 **0.4907** |
| 7. Repeat 5× for stability | ⏳ | see *Limitations* — one pass done, protocol documented |
| 8. Test `SKILL.md` | ⏳ | next, after stability |
| 9. Value test (Design → Uno vs Design → Graph → Uno) | ⏳ | protocol in *Next step* below |

## Why this design was chosen

`SettingsPage` is the kit's recommended first shape (a settings/profile screen)
but with a crucial upgrade: it ships with **full source** — XAML, code-behind,
and style dictionaries. That lets the graph exercise capabilities evals 01–04
can only simulate:

- tokens pulled from **declared** style resources (`OrbitalCardStyle`,
  `OrbitalPrimaryButtonSm`, brush/type dictionaries) instead of guessed pixels;
- **source-backed behavior** — a `triggers` edge (`Save → Saved!`) and an
  entrance state (`FadeUp`), honestly `declared`/`csharp` rather than invented;
- a real consolidation problem (four cards, four info-rows, three path-fields,
  three ghost buttons) to test `instance-of`.

It is the bridge from "understand a picture" to "understand an application,"
the direction `docs/architecture.md` points the graph.

## Results

### Deterministic score (generated vs gold)

| Dimension | F1 | Note |
|---|---:|---|
| node_id | 0.68 | naming mostly aligned; drift on merged rows + neutral color names |
| node_signature | 0.68 | same set; types/roles consistent |
| edge | 0.60 | containment tracks the node drift |
| unresolved | 0.00 | equivalent uncertainty, different wording → exact-tuple miss |
| **macro_f1** | **0.49** | pulled down hard by the 0.00 unresolved row |

`severe_hallucination_proxy: false` — no unsupported behavior edge.

### Human rubric (see `evals/05-orbital-settings/scorecard.md`)

Average **4.0 / 5**, no severe hallucinations → **passing** by the prototype
threshold (avg ≥ 4.0, no severe hallucination, schema + integrity pass).

## Findings

1. **The kit runs end-to-end on a real Uno screen.** Schema validation,
   graph-integrity checks (duplicate ids/edges, missing endpoints, inferred
   items missing a rationale), and scoring all work. The integrity check
   actually caught a real defect during authoring (an `inferred` edge with no
   rationale), which is exactly its job.

2. **Source-backed input changes the honesty profile.** With code-behind
   available, behavior that eval 04 must leave `unresolved` becomes a
   legitimate `triggers` edge. The graph got measurably more useful *without*
   inventing anything — the distinction the ontology cares about.

3. **The deterministic scorer is dominated by exact string identity.** Two
   defensible graphs of the same screen land at macro F1 0.49. Most of the gap
   is not disagreement about the UI — it is:
   - `unresolved` matched by exact `(question, relatedIds)` tuple → 0.00 even
     though both graphs flag real uncertainty;
   - one canonical-name / consolidation choice (merging info-rows and
     path-fields) cascading through every child id and edge.

   This is a property of the score, not a defect in either graph. Treat
   `macro_f1` as a **regression/drift tripwire**, and lean on the human rubric
   for quality — which the kit's own docs already say.

4. **No ontology change is warranted yet.** Every divergence was representable
   in v0.1. Per `docs/testing-plan.md`, the ontology should only grow on a
   *repeated* representational failure. One candidate to watch across future
   runs: distinguishing a token sourced from a **declared style resource** from
   a **derived** value — source-backed runs keep hitting it.

## Limitations (read before trusting the numbers)

- **Not blinded.** `gold` and `generated` were authored by the same agent in
  one session. This measures the scorer's sensitivity and the prompt's internal
  consistency — **not** independent model stability. The 5-run protocol
  (START-HERE step 7) is still required.
- **One screen, one domain.** A dark, mono-typographic developer-tool settings
  page. Broader shapes (lists, forms with validation, navigation) untested.
- **Value not yet demonstrated.** Generating a good graph is table stakes. The
  kit's own thesis (README) is that the graph must *improve downstream
  implementation*. That is the next experiment, below.

## Next step — the value experiment (Stage 5)

This is the test that decides whether the graph is worth productizing. Protocol,
ready to run against the same screen:

- **Baseline A** — give the model a plain design brief of the settings screen
  (no graph) and `mcp__uno` docs access; ask for a fresh Uno `SettingsPage`.
- **Treatment B** — same brief + `generated.graph.json` as the semantic source
  of truth, using `prompts/design-implement.md`.
- **Hold constant:** model, target framework (Uno), design brief, implementation
  instructions, iteration budget. Only the graph's presence changes.
- **Score:** visual parity, component reuse (did B reuse one card/row/button
  style where A copy-pasted?), token consistency, state coverage, unsupported
  behavior, and number of correction prompts. Log a row per arm in
  `evals/experiment-log.csv`.
- **Decision rule:** productize only if B shows a *material* downstream
  advantage (README step 7 / testing-plan Stage 5). Generating valid JSON is
  not the bar.

Run `SKILL.md` against this same eval first (Stage 4) so the A/B uses the
skill-orchestrated graph rather than the hand-run prompt.

## How to reproduce

```bash
cd design-graph-kit
python -m pip install -r requirements.txt
python scripts/run_all.py                 # validates 5 gold graphs + scores the generated one
python scripts/validate_graph.py evals/05-orbital-settings/gold.graph.json
python scripts/score_graph.py \
  evals/05-orbital-settings/gold.graph.json \
  evals/05-orbital-settings/generated.graph.json --json
```
