# Manual Run Scorecard — 05-orbital-settings

## Run metadata

- Eval: 05-orbital-settings
- Run number: 1
- Model: Claude (claude-opus-4-8)
- Model/version: Opus 4.8
- Input source: existing app source — `SettingsPage.xaml` + `.xaml.cs` + Orbital style dictionaries (`xaml` + `csharp` + `design-system`)
- Prompt/Skill version: `prompts/design-understanding.md` (v0.1)
- Schema version: 0.1.0
- Generated file: `evals/05-orbital-settings/generated.graph.json`

## Deterministic score (vs `gold.graph.json`)

| Dimension | Precision | Recall | F1 | gold / gen |
|---|---:|---:|---:|---|
| node_id | 0.725 | 0.644 | 0.682 | 45 / 40 |
| node_signature | 0.725 | 0.644 | 0.682 | 45 / 40 |
| edge | 0.628 | 0.571 | 0.598 | 56 / 51 |
| unresolved | 0.000 | 0.000 | 0.000 | 2 / 1 |
| **macro_f1** | | | **0.4907** | |

`severe_hallucination_proxy`: **false** — no `navigates-to`/`triggers` edge absent from gold.

## Scores (0–5, see evaluation-rubric.md)

| Dimension | Score | Notes |
|---|---:|---|
| Structure | 4 | Header + four cards captured cleanly, no raw wrapper tree. Dropped the explicit `region.settings.header` (put title/subtitle directly under the screen) — defensible, minor. |
| Semantics | 5 | Controls classified correctly (textbox, buttons); `primaryAction`, `externalLink`, `sectionHeader`, `helperText` all well-placed. |
| Consolidation | 4 | Consolidated the four cards, the repeated rows, and the ghost buttons. Merged ABOUT info-rows and PATHS fields into one `key-value-row` — reasonable, but loses the grid-vs-stacked distinction the gold keeps as two components. |
| State modeling | 3 | Captured the transient `Saved` state, but missed the source-backed entrance (`FadeUp`) state. |
| Token normalization | 4 | Good color/radius/spacing tokens; neutral color names (`token.color.0a0a0b`) are acceptable per token-rules. Omitted the typography token and the `spacing.12` value. |
| Relationships | 4 | Edges accurate; `Save → triggers → Saved` matches the source. Missed a couple of `uses-token` edges present in gold. |
| Uncertainty discipline | 4 | Recorded a genuine `unresolved`; no invented behavior. But it silently resolved the row/field-consolidation ambiguity (gold flags it) instead of flagging it, and marked the trigger/state `inferred 0.7` rather than reading the code-behind as `declared`. |
| Stability | 4* | *Single run vs gold only. Canonical names were mostly stable; true stability needs the 5-run protocol. |

**Average: 4.0**

## Severe hallucinations

None. No invented command names, bindings, routes, data models, or business rules. The one behavioral edge (`Save → Saved`) is supported by the code-behind.

## What the graph got right

- Correct screen → card → control hierarchy without reproducing XAML wrapper layers.
- Recognized all three repeated structures (cards, key/value rows, ghost buttons) and used `instance-of`.
- Extracted real design tokens and kept naming neutral where semantics were uncertain.
- Left behavior it could not see (`open-docs` destination) as `unresolved` instead of inventing a route.

## What the graph got wrong

- Missed the entrance-animation state (a `csharp`-backed fact).
- Under-credited evidence: treated the `Save` confirmation as `inferred 0.7` when the code-behind makes it `declared`.
- Merged two visually-distinct row components; did not flag the choice.

## Schema/ontology changes suggested by this failure

None yet. Every divergence is representable in v0.1 — these are prompt/consistency issues, not missing concepts. Do not expand the ontology on a single run. The one recurring-if-confirmed candidate to watch: a way to mark a token as coming from a **declared style resource** vs a **derived** value, since source-backed runs will keep hitting that distinction.

## Note on methodology

Both `gold` and `generated` were produced by the same agent in one session, so
this measures the **scorer's sensitivity and the prompt's internal consistency**,
not blinded model stability. The 5-run, separately-generated stability protocol
(START-HERE step 7) is still required before trusting these numbers as a
stability signal.
