---
name: design-graph
description: Generate a Design Graph (semantic JSON IR of a UI screen — structure, components, states, tokens, Uno mapping) from an Uno/WinUI app's source, a Figma export, or a screenshot; or implement a screen FROM an existing Design Graph. Use when the user asks to "generate a design graph", "map this screen", "extract the design", analyze a screen's design semantics, or build a screen from a *.graph.json. Also validates and scores graphs.
---

# Design Graph

This repo vendors the Design Graph Kit at `design-graph-kit/` — a validated
pipeline (v0.4.x, three-architecture blind-tested) for turning UI designs
and Uno Platform screens into a machine-checkable semantic graph, and for
implementing screens from such graphs.

## Mode 1 — Generate a graph

1. Read and follow, in order:
   - `design-graph-kit/SKILL.md` (the 10-pass procedure and every binding
     rule: ID grammar, naming vocabulary, state altitude, canonical
     internals, token scoping, uses-token attachment, Uno mapping layer)
   - `design-graph-kit/schema/design-graph.schema.json`
   - `design-graph-kit/references/ontology.md`
   - `design-graph-kit/references/inference-rules.md`
   - `design-graph-kit/references/token-rules.md`
   - `design-graph-kit/references/uno-mapping.md`
2. Gather the input: app source (XAML + code-behind/ViewModel/model +
   style dictionaries — expand every custom-control reference), or a
   design-only input (image/Figma — then behavior stays `unresolved` and
   the uno mapping is a proposed realization marked `inferred`).
3. If the Uno Platform docs MCP is available (`mcp__uno__*`), use it to
   resolve control identity and Themes/resource idioms for the
   `properties.uno` layer.
4. Write `<name>.graph.json` and validate until it passes:
   `python3 design-graph-kit/scripts/validate_graph.py <file>`
5. Never invent behavior: bindings/commands/handlers are declared evidence;
   visual plausibility is not. Prefer `unresolved` over low confidence.

## Mode 2 — Implement from a graph

Follow `design-graph-kit/prompts/design-implement.md`. The graph is the
semantic source of truth (hierarchy, canonical components + instance-of,
has-state/triggers, uses-token); honor the `properties.uno` mapping layer —
adopt its resource keys, x:Names, and control types verbatim so the result
stays traceable to the source design system.

## Scoring / regression (when a gold answer key exists)

`python3 design-graph-kit/scripts/score_graph.py <gold> <generated> [--json]`
— six dimensions including id-drift-tolerant `node_concept` and the
`uno_mapping` copy-don't-coin measure. Eval protocol and history:
`design-graph-kit/RESULTS.md`, `design-graph-kit/evals/`.
