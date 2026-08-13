# Design Graph Kit v0.5

A validated kit for generating and consuming **Design Graphs** — a semantic,
machine-checkable intermediate representation between UI design inputs and
Uno Platform implementation.

> **Status (v0.5):** the v0.1 hypothesis below has been tested. See
> `RESULTS.md` for the full experiment log: 3 source-backed evals across 3
> architectures (code-behind / MVUX / MVVM), 50 blind generation runs with
> **zero hallucinated behaviors**, A/B implementation experiments, a perfect
> round-trip parity result, and a design-first pilot. The kit is Uno-first
> (`docs/architecture.md`), carries a `properties.uno` mapping layer
> (`references/uno-mapping.md`), and is invocable as a Claude Code skill
> (`.claude/skills/design-graph/`). Remaining before production claims:
> compile-verification, an image-input round, and a human-reviewed gold
> (RESULTS.md → Handoff).

The goal is not to prove that an LLM can emit JSON. The goal is to test whether an explicit graph of UI structure, semantics, states, tokens, and relationships improves downstream implementation quality, consistency, and visual parity.

## What is included

- `SKILL.md` — reusable agent skill for generating Design Graphs.
- `schema/design-graph.schema.json` — strict JSON Schema for v0.1 graphs.
- `references/ontology.md` — node and edge definitions.
- `references/inference-rules.md` — what may be observed, derived, inferred, or left unresolved.
- `references/token-rules.md` — token extraction and normalization rules.
- `prompts/design-understanding.md` — manual prompt to test the workflow before relying on the Skill.
- `prompts/design-implement.md` — downstream implementation prompt for A/B testing.
- `docs/testing-plan.md` — staged testing plan from first eval to round-trip parity.
- `docs/architecture.md` — where the graph fits in the product workflow.
- `docs/evaluation-rubric.md` — scoring dimensions and failure criteria.
- `evals/` — four starter eval cases and hand-authored gold graphs.
- `scripts/validate_graph.py` — schema and graph-integrity validation.
- `scripts/score_graph.py` — basic deterministic comparison against a gold graph.
- `requirements.txt` — Python dependency for schema validation.

## Recommended workflow

Do **not** begin by integrating this deeply into a product.

1. Run the manual prompt against a small set of designs.
2. Validate the output.
3. Compare it against the gold graph.
4. Revise the ontology/schema when the same failure repeats.
5. Once the workflow stabilizes, use `SKILL.md`.
6. Run an A/B test:
   - A: Design -> implementation
   - B: Design -> Design Graph -> implementation
7. If B is materially better, add graph persistence and round-trip testing.

See `docs/testing-plan.md` for the complete sequence.

## Quick start

Python 3.10+:

```bash
python -m pip install -r requirements.txt
python scripts/validate_graph.py evals/01-settings/gold.graph.json
python scripts/score_graph.py evals/01-settings/gold.graph.json evals/01-settings/gold.graph.json
```

The second command should return a perfect score because the same graph is compared to itself.

## Generating your first graph

Use a simple settings/profile screen as the first real input.

Give your model:

1. The screenshot/design.
2. `schema/design-graph.schema.json`.
3. `references/ontology.md`.
4. `references/inference-rules.md`.
5. `references/token-rules.md`.
6. The instructions in `prompts/design-understanding.md`.

Save the result as:

```text
evals/01-settings/generated.graph.json
```

Then:

```bash
python scripts/validate_graph.py evals/01-settings/generated.graph.json
python scripts/score_graph.py evals/01-settings/gold.graph.json evals/01-settings/generated.graph.json
```

## Design principles

The graph should:

- represent meaning, not drawing commands;
- preserve source evidence and confidence;
- explicitly separate observed facts from inferred meaning;
- represent partial knowledge without inventing missing behavior;
- normalize repeated visual rules into candidate tokens;
- identify repeated structures as component instances;
- remain stable enough that repeated runs produce comparable IDs and concepts;
- be implementation-agnostic at the Design Graph layer.

## v0.1 scope

Node types:

- `screen`
- `region`
- `component`
- `control`
- `content`
- `asset`
- `token`
- `state`

Relationship types:

- `contains`
- `instance-of`
- `variant-of`
- `uses-token`
- `has-state`
- `navigates-to`
- `triggers`

This is intentionally small. Add concepts only when an eval demonstrates that the graph cannot express something important without them.
