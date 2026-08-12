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
| 7. Repeat 5× for stability | ✅ | `evals/05-orbital-settings/blind/` — **semantics stable, ids not**; vs-gold macro F1 mean 0.069 |
| 8. Test `SKILL.md` | ✅ | run 2: `skill.graph.json`, macro F1 **0.9742** (same-author; blind runs correct this — see below) |
| 9. Value test (Design → Uno vs Design → Graph → Uno) | ✅ | `experiments/ab-orbital-settings/ab-results.md` — B ≫ A on semantics; **arm C matched B** |
| 9b. Stage 6 round-trip parity | ✅ | node-id recall **1.000**; real parity findings (12 implementation-introduced tokens, state-attachment drift) |
| 10. Productize decision | ✅ | **Do not productize v0.1.** v0.2 shipped and blind-validated (below); one more iteration (v0.3 levers) before product integration. |
| 11. v0.2 blind validation | ✅ | `evals/05-orbital-settings/blind-v2/` — vs-gold node-id F1 ×3.1, drift tail −58%, style-level states eliminated, 0 hallucinations |

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

### Contamination caveat (applies to runs 1–2) — now quantified

Gold and both generated graphs were authored by the same agent lineage. Run 2's
near-perfect score is an **upper bound demonstrating the pipeline works
end-to-end**, not evidence of blind stability. The blind replication then
measured the contamination effect directly: same skill, same source, same
model, fresh contexts with no gold access → macro F1 fell from **0.9742 to a
mean of 0.069**. Same-author evaluation numbers for this kit are essentially
meaningless as stability evidence.

### Blind replication (5×, fresh contexts) — the decisive stability test

Full analysis: `evals/05-orbital-settings/blind/README.md`.

- **Semantic stability is genuinely good:** all five runs assert exactly the
  two source-backed behaviors under `triggers` (zero hallucination — the
  scorer's proxy flagged all five, but every flagged edge is a real behavior
  under a drifted id, a proxy defect); 48 core concepts appear identically in
  all five runs; sizes cluster tightly (85–90 nodes); consolidation and
  unresolved discipline repeat every time.
- **Lexical/granularity stability fails the kit's own criteria:** id spelling
  drifts (72 singleton ids, almost all synonyms of shared concepts), 4/5 runs
  modeled style-level hover/pressed/disabled visuals as screen `state` nodes,
  and token extraction enumerated entire style dictionaries (37–38 tokens vs
  gold's screen-scoped 13). Two of `docs/testing-plan.md`'s explicit failure
  signals are hit. **v0.1 does not meet its own Stage-3/4 naming-stability
  exit criteria under blind conditions.**

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

## Where this lands (after all follow-ups)

The three follow-up experiments together give a clear, three-part answer:

1. **The graph's one-shot codegen advantage is about information, not
   structure.** Arm C (prose notes with the same facts) matched arm B on every
   semantic measure. The Stage-5 win was really "the handoff carried behavior"
   vs "it didn't."
2. **The structure earns its keep in the machinery around generation.**
   Round-trip parity (node-id recall 1.000, actionable drift findings, honest
   new unresolveds) is impossible with prose — that, plus validation and
   deterministic drift scoring, is the graph's real product surface.
3. **v0.1 is not stable enough to productize.** Blind replication shows
   excellent semantic agreement but failing lexical/granularity stability.
   The failure modes are systematic and fixable, and the data names the fixes.

## v0.2 blind validation (same scorer, same gold — deltas caused by the revisions alone)

Full analysis: `evals/05-orbital-settings/blind-v2/README.md`.

| Metric | v0.1 | v0.2 |
|---|---:|---:|
| mean vs-gold macro F1 | 0.114 | **0.256** |
| mean vs-gold node-id F1 | 0.154 | **0.484** (×3.1) |
| mean pairwise node-id F1 | 0.707 | **0.814** |
| drift tail (singleton ids) | 72 | **30** |
| style-level states per run | ~6 | **0** |
| hallucinations (10 blind runs total) | 0 | **0** |

The naming vocabulary converged all five runs onto gold's canonical names
(`component.info-row`, `control.profile.save`, …) and the state-altitude rule
held in every run. Remaining drift is concentrated in three specific,
rule-shaped places — the v0.3 levers: `uses-token` edges wired per-instance
instead of once on the canonical component (66–71 edges vs gold's 11),
canonical-component internals modeled as child nodes one level below gold's
altitude, and per-use token variants (~2× gold's token set, down from ~3×).

## The applied v0.2 work list (now shipped — kept for the record)

1. **SKILL.md Pass 8:** binding id grammar (`<type>.<screen>.<slug>`, flat;
   canonical components unprefixed) + small naming thesaurus. The five blind
   runs disagree exactly where the grammar is silent.
2. **Ontology `state`:** scope to screen/component presentation conditions;
   exclude style-level hover/pressed/disabled from screen graphs (4/5 runs
   added them). Add a rule for which node owns a transient state (the
   round-trip surfaced card-vs-button attachment drift).
3. **token-rules.md:** extract only tokens the modeled surface consumes;
   whole-dictionary enumeration belongs to a separate design-system graph.
4. **Scorer:** endpoint-role matching for the hallucination proxy (all 5
   blind runs were false-flagged); add a normalized-signature dimension
   (type + text + role) that survives id drift.
5. **Gold checklist:** expand every source-backed component reference; record
   the intended granularity altitude in the eval README.
6. Re-run the blind 5× protocol against v0.2 — done; see the validation
   section above.

## Uno MCP assessment (v0.3)

Checked whether the Uno Platform MCP server should back any of the kit's
rules (`docs/uno-mcp-integration.md` has the full analysis). Conclusion:
**no for the Design Graph layer** — its rules are framework-agnostic semantic
conventions by architectural design — but **adopted in two places**: Uno
implementation arms now initialize the MCP's usage rules and ground idioms
via docs search (identical across arms, per `design-implement.md` rule 9),
and MCP-grounded review is the sanctioned static verification when no
compiler is available. The MCP is the right grounding source for the future
Implementation Graph layer, not for this one.

## v0.4 — Uno-first pivot (product decision)

Direction set by the user: the graph is **Uno-specific now**; a
framework-agnostic IR is deferred until the Uno-specific graph proves
product value. This supersedes the agnostic half of the MCP assessment
above — the Uno docs MCP is now in the graph-generation loop for the new
`properties.uno` mapping layer (`references/uno-mapping.md`): exact control
types, x:Names, style keys, and declared resource keys carried per node.
Gold v1.3 is the exemplar (40/47 nodes mapped from verified source). The
blind-v3 runs (agnostic v0.3 rules) are archived unscored in
`evals/05-orbital-settings/blind-v3/`; the next validation round runs under
v0.4 and scores mapping-layer recovery in addition to the semantic layer.

## v0.4 blind validation — the Uno-first format holds

Full analysis: `evals/05-orbital-settings/blind-v4/README.md`.

| Metric | v0.1 | v0.2 | **v0.4** |
|---|---:|---:|---:|
| mean pairwise macro F1 | 0.497 | 0.563 | **0.758** |
| mean pairwise node-id F1 | 0.707 | 0.814 | **0.929** |
| drift tail (singleton ids) | 72 | 30 | **9** |
| hallucinations (20 blind runs) | 0 | 0 | **0** |

Five blind runs now produce near-clone graphs, and the new mapping layer
transports declared Uno identity almost losslessly: verbatim value recall
of resource keys / x:Names / style keys / control types is **0.86–0.90**
(strict triple F1 0.64–0.66; the difference is classification variance,
not loss).

The residual vs-gold gap changed character: the five runs *agree with each
other* (pairwise edge F1 up to 0.95) on a token-wiring altitude richer than
gold's (~53 `uses-token` edges vs 11, including the text-emphasis brushes).
That is now a **consensus-vs-answer-key calibration decision** — adopt the
consensus into gold v1.4, or pin typography tokens to canonical
style-consumer concepts by rule — to be made explicitly, not ruled away.
Small true gaps: icon-glyph capture (0/5), one run dropped one trigger.

## Next steps (v0.3 — evidence from blind-v2)

1. `uses-token` attachment rule: token edges belong on the canonical
   component or screen, never per-instance/per-child.
2. Canonical-component internals: template parts are `properties` of the
   canonical node, not child nodes, unless an instance overrides them.
3. Token variant folding: per-use alpha/hover variants fold into the base
   token or the design-system layer.
4. Re-run blind 5×; if the vs-gold gap closes materially again, graduate to
   a second eval screen (different app/domain) before any product step.
5. Compile/render the A/B/C arms in a toolchain-equipped environment for
   true visual parity.
