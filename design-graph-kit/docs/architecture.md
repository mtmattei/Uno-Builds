# Architecture

## Placement in the workflow

The Design Graph is an intermediate semantic layer:

```text
Design sources
  - screenshot
  - Figma/design document
  - existing XAML/C#
  - runtime UI
  - design system/tokens
        |
        v
Design understanding
        |
        v
+-----------------------+
|     DESIGN GRAPH      |
| semantic UI model     |
+-----------------------+
        |
        +--> code generation
        +--> Hot Design / visual editing
        +--> previews/states
        +--> agent queries
        +--> design-system validation
        +--> semantic parity testing
```

## Architectural principle

Features should consume the shared graph where practical instead of each feature independently re-interpreting the design.

## Persistence model

Prototype first with a portable file:

`design.graph.json`

Do not begin with a graph database. A JSON artifact is:
- inspectable;
- diffable;
- easy to validate;
- easy to pass between agents;
- easy to version.

A database or indexed runtime representation can be introduced later if graph queries or scale require it.

## Long-term update model

The desirable future model is persistent and incrementally updated:

```text
Design -> Graph -> Implementation
            ^            |
            |            v
            +------ edits/runtime
```

Visual edits, source edits, and agent edits should eventually update or reconcile the graph.

## Framework stance (v0.4): Uno-first

The Design Graph v0.x deliberately targets **Uno Platform** (WinUI XAML,
Uno Toolkit, Uno Themes). Its consumers — code generation, Hot Design,
Studio tooling, agent edits — are Uno tools, so framework agnosticism at
this stage is premature abstraction that costs precision (lost resource
keys, synonym control roles, unmappable tokens).

Concretely:

- semantic node fields (`type`, `role`, `semanticRole`, text, hierarchy)
  stay implementation-light and human-readable;
- every node may additionally carry an **Uno mapping layer** under
  `properties.uno` — real control types, `x:Name`s, style and resource
  keys, ms-appx URIs — per `references/uno-mapping.md`;
- when the source is an Uno app, the mapping layer is evidence-backed and
  losslessly recoverable (round-trip exact); when the source is a
  design-only input, the mapping layer is the generator's proposed
  realization and is marked `inferred`;
- the Uno Platform docs MCP is part of the generation loop for this layer
  (`docs/uno-mcp-integration.md`).

There is no separate "Implementation Graph": the `uno` mapping layer *is*
that graph, folded into the design graph where it stays diffable and
round-trippable.

A framework-agnostic IR remains a plausible future extraction — the
semantic fields are already the agnostic core — but it is explicitly
deferred until the Uno-specific graph has proven product value.
