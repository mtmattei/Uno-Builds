# Uno Platform MCP — where it fits this kit

Assessed 2026-08-12 against the live `uno` MCP server
(`uno_platform_agent_rules_init`, `uno_platform_usage_rules_init`,
`uno_platform_docs_search`/`_fetch`).

> **v0.4 revision.** The kit is now deliberately **Uno-first**
> (`docs/architecture.md`): the graph carries a `properties.uno` mapping
> layer (`references/uno-mapping.md`), and the MCP is part of the graph
> generation loop for that layer — resolving control identity, Toolkit
> component names, and Themes/resource idioms. The paragraph below records
> the earlier framework-agnostic reasoning for the deferred agnostic IR; it
> no longer gates graph-side MCP use.

## The kit-internal semantic rules still aren't MCP territory

The rules that drive graph *semantic* quality (ID grammar, naming
vocabulary, state altitude, token scoping, `uses-token` attachment) are
kit-internal conventions — no framework documentation defines them, so the
MCP cannot ground them. What the MCP grounds is the **`uno` mapping layer**:
which real control a concept is, which style/resource key realizes a token,
which Toolkit component matches a pattern. Semantic layer = evidence from
the design source; mapping layer = evidence from the source plus MCP-grounded
Uno knowledge.

## Yes, for implementation (design-implement.md consumers)

The MCP's usage rules overlap almost exactly with what the Stage-5 experiment
scores: never hardcode hex colors, reuse existing styles, put styles in
resource dictionaries, maximize UserControl reuse, prefer the theme's
(Material) resources. Agents implementing a graph against a real Uno app
should initialize `uno_platform_usage_rules_init` and use
`uno_platform_docs_search` for idioms.

Experiment-protocol requirement: docs access must be given to **all arms
equally** (baseline and treatment), or it becomes a confound.

Observed gap it would have closed: all three A/B/C arms defined their color
palettes as page-local hex resources in a standalone `Tokens.xaml`. Correct
for an isolated exercise, but in a real Uno app the usage rules route these
through App.xaml/theme resource overrides (e.g. Uno.Themes lightweight
styling keys). Future in-app arms should be scored on that too.

## Yes, for verification when no compiler is available

This environment has no .NET SDK, so arm XAML cannot be compiled.
MCP docs searches confirmed the Uno-specific idioms the arms used are
supported and documented (code-behind `Storyboard`/`DoubleAnimation` with
string target properties; lightweight styling resource keys), which is the
best static assurance available. The MCP `agent_rules` (build/run/Hot Reload
workflow) become relevant only in a toolchain-equipped environment — use them
when the "compile the arms" follow-up runs.
