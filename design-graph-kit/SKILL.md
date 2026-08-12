# Skill: Generate Design Graph

## Purpose

Analyze a UI design, runtime UI, design document, or source-backed application and produce a **Design Graph**: a semantic, machine-readable representation of the design's structure, reusable components, controls, states, design tokens, and relationships.

The graph is an intermediate representation. Do not generate implementation code while executing this skill unless the caller separately asks for implementation after the graph is complete.

## Inputs

Use any available combination of:

- screenshot or image;
- Figma/design-document structure;
- existing XAML;
- existing C#;
- runtime UI inspection;
- HTML/DOM;
- design-system definitions;
- design tokens;
- explicit user description.

Input quality varies. Represent partial knowledge honestly.

## Required references

Before generating a graph, follow:

1. `schema/design-graph.schema.json`
2. `references/ontology.md`
3. `references/inference-rules.md`
4. `references/token-rules.md`

## Output

Produce one JSON document conforming to:

`schema/design-graph.schema.json`

Default filename:

`design.graph.json`

Do not wrap the JSON in explanatory prose if the caller asks for a file or machine-readable result.

## Procedure

### Pass 1: Inventory observable facts

Identify only what is directly supported by the source:

- screens or major views;
- visible regions;
- visible controls;
- visible text/content;
- assets;
- geometry if available;
- colors;
- typography;
- spacing;
- borders/radii;
- source-backed component names;
- source-backed states.

Mark these as `observed`, `declared`, or `derived` as appropriate.

Do not infer behavior during this pass.

### Pass 2: Establish structural hierarchy

Build `contains` relationships from coarse to fine:

`screen -> region -> component -> control/content/asset`

Prefer meaningful semantic containers over reproducing every drawing-layer wrapper.

Do not recreate a Figma layer tree verbatim when intermediate frames/groups have no semantic value.

### Pass 3: Detect repetition

Look for recurring structures with comparable:

- child roles;
- visual treatment;
- spacing;
- typography;
- purpose;
- source identity.

When multiple items are clearly instances of one reusable concept:

1. create a canonical `component` node;
2. create instance nodes only when the instances need distinct identity;
3. connect instances with `instance-of`.

Do not collapse merely similar objects when their semantics differ.

### Pass 4: Identify states and variants

When multiple presentations represent the same conceptual UI under different conditions:

- create one canonical screen/component;
- create `state` nodes;
- connect using `has-state`.

Examples:

- loading;
- empty;
- populated;
- error;
- disabled;
- selected.

Use `variant-of` when two components are stable variants of the same component concept rather than transient states.

### Pass 5: Normalize candidate design tokens

Use `references/token-rules.md`.

Create `token` nodes for recurring or explicitly declared values such as:

- color;
- typography;
- spacing;
- radius;
- border;
- elevation;
- size.

Prefer semantic token names when source evidence supports them.

When semantic naming is not supported, use stable neutral names such as:

- `token.color.1`
- `token.spacing.16`
- `token.radius.8`

Connect consumers using `uses-token`.

### Pass 6: Infer semantics conservatively

Infer concepts only when useful and reasonably supported.

Every inference must:

- use `evidence.kind = "inferred"`;
- include a confidence between 0 and 1;
- include a short rationale.

Do not invent:

- command names;
- binding names;
- navigation destinations;
- application state;
- data models;
- business rules;
- hidden interactions.

If behavior is plausible but unsupported, add an `unresolved` item instead of asserting it.

### Pass 7: Add supported relationships

Allowed v0.1 relationships:

- `contains`
- `instance-of`
- `variant-of`
- `uses-token`
- `has-state`
- `navigates-to`
- `triggers`

Only use `navigates-to` or `triggers` when supported by source code, explicit interaction metadata, runtime behavior, or explicit user description.

### Pass 8: Canonicalize IDs

IDs should be:

- stable;
- semantic;
- lowercase where practical;
- source-independent when the concept is source-independent.

Preferred patterns:

- `screen.settings`
- `region.settings.account`
- `component.metric-card`
- `control.settings.save`
- `state.orders.loading`
- `token.spacing.16`

Avoid random IDs unless the source has no useful semantic identity.

### Pass 9: Record uncertainty

Use the top-level `unresolved` array when:

- multiple interpretations remain plausible;
- interaction behavior is unknown;
- repeated elements may or may not be one component;
- token semantic naming is uncertain;
- state relationships are ambiguous.

A correct unresolved item is better than a confident hallucination.

### Pass 10: Validate

Before returning:

1. validate against the JSON Schema;
2. ensure every edge `from` and `to` references an existing node;
3. ensure node IDs are unique;
4. ensure no duplicate edges exist;
5. ensure every inferred item has confidence and rationale;
6. ensure no unsupported behavior was invented;
7. ensure repeated patterns were considered for component consolidation.

## Quality priorities

In order:

1. No unsupported semantic claims.
2. Correct hierarchy.
3. Correct repeated-component recognition.
4. Correct state relationships.
5. Useful token normalization.
6. Stable canonical naming.
7. Completeness.

Prefer a smaller accurate graph over a larger speculative graph.

## Stop condition

The graph is complete when it captures the important semantic structure of the supplied UI and remaining uncertainty is represented explicitly.

Do not attempt to model invisible application behavior from a visual design alone.
