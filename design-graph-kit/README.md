# Design Graph Kit v0.6

A validated kit for generating and consuming **Design Graphs** — a semantic,
machine-checkable intermediate representation between UI design inputs and
Uno Platform implementation.

> **Status (v0.6):** the v0.1 hypothesis below has been tested. See
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

## What a graph looks like

An abridged excerpt from a real one: `evals/05-orbital-settings/gold.graph.json`
(65 nodes, 96 edges), authored from the Orbital app's `SettingsPage.xaml`,
its code-behind, and the Orbital style dictionaries. Every identifier below —
control types, `x:Name`s, style keys — is copied verbatim from that source.

```jsonc
{
  "schemaVersion": "0.1",
  "graphId": "orbital-settings",
  "name": "Orbital Settings",
  "nodes": [
    {
      "id": "screen.settings",
      "type": "screen",
      "name": "Settings",
      "evidence": { "kind": "observed", "confidence": 1.0,
        "source": { "type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml" } },
      "properties": { "uno": { "type": "Page", "class": "Orbital.Presentation.SettingsPage" } }
    },
    {
      "id": "control.profile.save",
      "type": "control",
      "name": "Save",
      "role": "button",
      "semanticRole": "primaryAction",
      "evidence": { "kind": "declared", "confidence": 1.0,
        "source": { "type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml" },
        "rationale": "x:Name=SaveUsernameButton, OrbitalPrimaryButtonSm (only high-emphasis action)." },
      "properties": { "uno": { "type": "Button", "styleKey": "OrbitalPrimaryButtonSm", "xName": "SaveUsernameButton" } }
    },
    {
      "id": "component.info-row",           // canonical: four label/value rows fold into one concept
      "type": "component",
      "name": "Info row (label/value)",
      "role": "keyValueRow",
      "evidence": { "kind": "derived", "confidence": 1.0,
        "source": { "type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml" },
        "rationale": "Four label/value grids with identical structure inside ABOUT." },
      "properties": { "uno": { "type": "Grid" } }
    },
    {
      "id": "component.info-row.platform",  // instance: carries only what differs
      "type": "component",
      "name": "Platform",
      "properties": { "value": "{Binding PlatformInfo}" },
      "evidence": { "kind": "declared", "confidence": 1.0,
        "source": { "type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml" } }
    },
    {
      "id": "state.profile.saved",          // a presentation condition, owned by the button that changes
      "type": "state",
      "name": "Saved",
      "semanticRole": "confirmation",
      "evidence": { "kind": "declared", "confidence": 1.0,
        "source": { "type": "csharp", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs" },
        "rationale": "Save sets button content to 'Saved!' for 1.5s after SettingsService.SaveUsername." },
      "properties": { "uno": { "mechanism": "code-behind" } }
    },
    {
      "id": "token.radius.12",
      "type": "token",
      "name": "12 radius",
      "category": "radius",
      "value": 12,
      "properties": { "unit": "px", "uno": { "styleKey": "OrbitalCardStyle", "property": "CornerRadius" } },
      "evidence": { "kind": "declared", "confidence": 1.0,
        "source": { "type": "design-system", "label": "Orbital Styles/*.xaml" },
        "rationale": "OrbitalCardStyle CornerRadius." }
    }
    // ... 59 more nodes: regions, cards, fields, dialog, entrance states, tokens
  ],
  "edges": [
    { "from": "screen.settings", "relation": "contains", "to": "region.settings-content", /* ... */ },
    { "from": "component.settings-card.profile", "relation": "contains", "to": "control.profile.save", /* ... */ },
    { "from": "component.info-row.platform", "relation": "instance-of", "to": "component.info-row", /* ... */ },
    { "from": "component.settings-card", "relation": "uses-token", "to": "token.radius.12", /* ... */ },
    { "from": "control.profile.save", "relation": "triggers", "to": "state.profile.saved",
      "evidence": { "kind": "declared", "confidence": 1.0,
        "source": { "type": "csharp", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs" },
        "rationale": "Save handler swaps content to 'Saved!' after persisting the name." } },
    { "from": "control.profile.save", "relation": "has-state", "to": "state.profile.saved", /* ... */ }
    // ... 90 more edges
  ],
  "unresolved": [
    {
      "id": "unresolved.header.search-target",
      "question": "What UI does the header search / command palette open?",
      "relatedIds": ["control.header.search"],
      "possibleValues": ["global command palette", "search overlay", "unknown"],
      "reason": "PageHeader raises a static SearchRequested event; the handler and resulting UI are outside the supplied source."
    }
  ]
}
```

What to notice:

- **Every claim carries evidence.** `observed` / `declared` / `derived` /
  `inferred`, with a source path and, for anything non-obvious, a rationale.
- **The `properties.uno` mapping layer is copied, never coined.** `Button`,
  `OrbitalPrimaryButtonSm`, `SaveUsernameButton`, `OrbitalCardStyle` all exist
  in the Orbital source, character for character. A design-only input would
  carry the same layer marked `inferred` instead.
- **Behavior edges require code.** The `triggers` / `has-state` pair on the
  Save button cites the code-behind line that swaps its content to "Saved!".
  Visual plausibility alone never produces a behavior edge.
- **Repetition folds into components.** Four identical label/value grids
  become one canonical `component.info-row` plus thin instances, connected by
  `instance-of`.
- **Unknowns are recorded, not invented.** The header search raises an event
  whose handler is outside the source set, so the graph says exactly that in
  `unresolved` instead of guessing a destination.

The full gold validates and scores cleanly:

```bash
python scripts/validate_graph.py evals/05-orbital-settings/gold.graph.json
```

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
