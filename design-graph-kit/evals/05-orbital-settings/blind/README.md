# Blind Replication — 5× SKILL.md, fresh contexts

START-HERE step 7, done properly: five agents in fresh contexts, each allowed
to read only the Orbital source files and the kit's method files (SKILL.md,
schema, references). No access to gold, prior graphs, scorecards, or results.
Model: Claude Fable 5 for all runs. All five validated (four on first attempt).

## Headline numbers

| | mean | range |
|---|---|---|
| macro F1 vs gold | **0.069** | 0.059 – 0.099 |
| pairwise macro F1 between runs | **0.447** | 0.342 – 0.585 |
| pairwise node-id F1 between runs | ~0.71 | 0.61 – 0.83 |
| nodes per run | 88 | 85 – 90 (gold: 47) |
| edges per run | 132 | 123 – 138 (gold: 58) |

For calibration: the same-session SKILL run scored **0.9742** against the same
gold. Same skill, same source, same model — the only change is removing
same-author contamination. That 0.97 → 0.07 gap is the measured size of the
contamination effect, and it retroactively justifies every caveat attached to
runs 1–2.

## What is actually stable (semantics)

- **Behavior: perfectly stable, zero hallucination.** Every run asserts
  exactly two `triggers` edges — Save → transient saved-state,
  Clear Recent Projects → cleared-dialog — both source-backed. No run invented
  navigation, commands, or bindings. (The scorer's
  `severe_hallucination_proxy` flagged all 5 runs, but inspection shows every
  flagged edge is a real behavior under a drifted id — a proxy defect, see
  below.)
- **Core concepts: stable.** 48 node ids appear *identically* in all five
  runs (screen, all controls, section cards, states for save/entrance,
  key tokens). Consolidation decisions repeat: every run created a canonical
  card component and repeated-row components with `instance-of`.
- **Size: stable.** 85–90 nodes every time — the runs agree with each other
  about granularity, just not with gold.
- **Uncertainty: stable.** 5–6 unresolved items per run, consistently
  covering the search target and data-binding unknowns.

## What is not stable (lexical + granularity vs gold)

- **Id spelling drifts.** The same concept appears as
  `component.settings.info-row` / `component.key-value-row` /
  `component.settings.about.row-uno-sdk` across runs — 72 node ids appear in
  exactly one run, almost all synonyms or granularity variants of shared
  concepts, not disagreements about the UI.
- **Granularity is ~2× gold, in two systematic ways:**
  1. **Design-system interaction states:** 4/5 runs modeled the button
     styles' PointerOver/Pressed/Disabled visual states as first-class
     `state` nodes (~6 per run). Gold treats these as style internals, not
     screen semantics. SKILL.md v0.1 doesn't say which altitude is intended.
  2. **Token enumeration:** runs emitted 37–38 tokens (every brush in the
     style dictionaries), vs gold's screen-scoped 13. token-rules.md doesn't
     bound extraction to values the modeled screen actually uses.

## Verdict against the kit's own criteria

`docs/testing-plan.md` lists failure signals; two are hit:
- "graph IDs change wildly between identical runs" — **hit** (vs gold; partially between runs)
- "token extraction creates hundreds of meaningless one-off tokens" — **trending** (37–38/run, mostly legitimate but unscoped)

And the exit criteria for Stage 3/4 ("stable enough canonical naming") are
**not met** under blind conditions. Semantic stability is genuinely good;
lexical stability is not. Do not proceed to productization on v0.1.

## Concrete v0.2 revisions this data justifies

1. **SKILL.md Pass 8 (canonical IDs):** replace examples-only guidance with a
   binding id grammar — `<type>.<screen>.<slug>` (flat, two dots), canonical
   component ids without screen prefix (`component.info-row`), and a small
   thesaurus rule (label/value pair → `key-value-row`). All five runs would
   have converged under this grammar; their disagreement is exactly where the
   grammar is silent.
2. **Ontology `state`:** scope to *screen/component presentation conditions*
   (loading/empty/saved/entering); explicitly exclude style-level interaction
   visuals (hover/pressed/disabled) from screen graphs.
3. **token-rules.md:** extract only tokens consumed by the modeled surface;
   design-system-wide enumeration belongs to a separate design-system graph.
4. **Scorer:** (a) replace the hallucination proxy's exact-triple match with
   endpoint-role matching so renamed-but-real behavior isn't flagged;
   (b) add a normalized-signature dimension (type + text/name + role) that
   survives id drift — pairwise node overlap at that level is the true
   semantic-stability number and it is much higher than the id-level 0.71.
5. **Gold authoring:** record the intended granularity altitude in the eval
   README so future gold graphs and generators aim at the same layer.

## Reproduce

```bash
python scripts/validate_graph.py evals/05-orbital-settings/blind/run1.graph.json   # …run5
python scripts/score_graph.py evals/05-orbital-settings/gold.graph.json \
  evals/05-orbital-settings/blind/run1.graph.json
```
