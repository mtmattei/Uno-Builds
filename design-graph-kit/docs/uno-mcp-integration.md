# Uno Platform MCP — where it fits this kit

Assessed 2026-08-12 against the live `uno` MCP server
(`uno_platform_agent_rules_init`, `uno_platform_usage_rules_init`,
`uno_platform_docs_search`/`_fetch`).

## Not for the Design Graph rules

The rules that drive graph quality (ID grammar, naming vocabulary, state
altitude, token scoping, `uses-token` attachment) are kit-internal semantic
conventions — no framework documentation defines them, so the MCP cannot
ground them. More fundamentally, `docs/architecture.md` requires the Design
Graph layer to stay implementation-agnostic; anchoring its ontology to Uno
control taxonomy or Uno Themes resources would couple the design IR to one
framework. Framework knowledge belongs to the future **Implementation Graph**
(concept → XAML type / style / resource mapping) — when that layer is built,
the MCP is the right grounding source for it.

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
