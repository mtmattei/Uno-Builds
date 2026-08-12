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

## Separation from implementation graph

The Design Graph should describe the semantic design.

A future Implementation Graph may map concepts to:
- XAML types;
- source files;
- C# types;
- bindings;
- resources;
- runtime objects.

Keeping these layers separate prevents the design representation from becoming tied to one framework.
