# Testing and Build Plan

This is the recommended sequence from idea to usable Skill.

## Stage 1 — Freeze the smallest useful model

Start with v0.1 only.

Node types:
- screen
- region
- component
- control
- content
- asset
- token
- state

Relationships:
- contains
- instance-of
- variant-of
- uses-token
- has-state
- navigates-to
- triggers

Do not expand the ontology because a concept might someday be useful. Expand only after an eval demonstrates a real representational gap.

### Exit criteria

You can manually represent:
- a simple form/settings screen;
- repeated cards;
- multiple UI states;
- uncertainty.

## Stage 2 — Build the eval set

Use at least four test designs.

### Eval 01: Settings

Tests:
- hierarchy;
- control recognition;
- text/content;
- simple tokens;
- semantic role.

### Eval 02: Dashboard

Tests:
- repeated structures;
- canonical reusable component;
- component instances;
- repeated tokens.

### Eval 03: States

Tests:
- one screen across loading, empty, populated, and error;
- correct `has-state` relationships;
- avoidance of four unrelated screen nodes.

### Eval 04: Ambiguous

Tests:
- unsupported behavior is not invented;
- uncertainty is represented in `unresolved`.

### Exit criteria

Each eval has:
- a design/input fixture;
- a hand-authored gold graph;
- written expectations.

## Stage 3 — Run the manual prompt

Before using `SKILL.md`, use `prompts/design-understanding.md`.

For each eval:
1. Provide the design.
2. Provide the schema/references.
3. Generate `generated.graph.json`.
4. Validate.
5. Score.
6. Manually inspect semantic errors.

Run each design multiple times to measure stability.

Suggested stability test:
- 5 runs per design;
- compare node IDs/types and edge triples;
- record naming drift and semantic drift.

### Exit criteria

Repeated outputs:
- validate;
- rarely hallucinate behavior;
- identify repeated components consistently;
- model states consistently;
- use stable enough canonical naming.

## Stage 4 — Package as a Skill

Once the manual workflow stabilizes, use `SKILL.md`.

The Skill should orchestrate the same process rather than introduce new semantics.

Regression-test the Skill against the same eval set.

### Exit criteria

Skill output performs at least as well as the manual prompt.

## Stage 5 — A/B implementation experiment

This is the critical value test.

### Baseline A

```text
Design -> implementation
```

### Treatment B

```text
Design -> Design Graph -> implementation
```

Keep constant:
- model;
- target framework;
- source design;
- implementation instructions;
- available framework skill;
- time/iteration budget where possible.

Score:
- visual parity;
- component reuse;
- design-system consistency;
- semantic structure;
- code quality;
- unsupported behavior;
- number of correction prompts required.

### Exit criteria

Do not productize the graph simply because graph generation works.

Continue only if B produces a meaningful downstream advantage, such as:
- fewer corrections;
- better component reuse;
- better state coverage;
- improved consistency;
- more reliable agent edits.

## Stage 6 — Round-trip semantic parity

If Stage 5 succeeds:

```text
Design
  -> Graph A
  -> implementation
  -> runtime UI/source analysis
  -> Graph B
```

Compare Graph A and Graph B.

Examples of useful parity results:
- expected component missing;
- state not implemented;
- token relationship changed;
- hierarchy changed materially;
- semantic role changed.

This provides semantic parity in addition to pixel comparison.

## Stage 7 — Product integration

Only after the experiments demonstrate value, consider:

- graph persistence in Studio;
- incremental graph updates;
- graph query APIs;
- agent-facing graph tools;
- source mapping;
- design-system impact analysis;
- semantic parity checks;
- graph visualization.

## What to log during experiments

For every run, record:
- model/version;
- input source type;
- prompt/skill version;
- schema version;
- validation result;
- deterministic score;
- manual rubric score;
- number of hallucinations;
- number of unresolved items;
- correction prompts required;
- implementation outcome.

## Failure signals

Pause and revise the design if:
- graph IDs change wildly between identical runs;
- the graph mostly duplicates a layer tree;
- behavior hallucination is common;
- component consolidation is unreliable;
- token extraction creates hundreds of meaningless one-off tokens;
- downstream implementation is not better than direct design-to-code.

## Success signal

The graph is valuable when it gives downstream agents durable semantic leverage they did not reliably obtain from the original visual/source input alone.
