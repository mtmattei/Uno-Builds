# Changelog

## 0.4.1 — 2026-08-12

- Eval 05 gold v1.4 — **consensus calibration**: adopted the blind-v4
  five-run consensus token-wiring altitude (the open decision recorded in
  blind-v4/README.md). Adds 9 declared tokens (six text-emphasis colors,
  emerald accent, page-title/body typography) and 30 `uses-token` edges
  wiring typography/color to their resting-surface consumers (gold: 19
  tokens / 41 token edges). Blind-v4 re-scored: `uno_mapping` F1 rose to
  0.75–0.77 (was 0.64–0.66).
- Scorer 0.3.0: new `uno_mapping` dimension — exact recovery of
  (node type, uno key, uno value) triples from `properties.uno`, measuring
  the copy-don't-coin contract independent of node ids.
- New eval `06-flux-profile`: second real source-backed case (FluxTransit
  ProfilePage — MVUX model, Toolkit controls, glass design system, four
  honesty traps), gold at the calibrated v0.4 altitude.

## 0.4.0 — 2026-08-12 — Uno-first pivot

Product decision: the Design Graph targets Uno Platform now; a
framework-agnostic IR is explicitly deferred (`docs/architecture.md`).

- New `references/uno-mapping.md`: binding `properties.uno` convention —
  exact WinUI/Toolkit control types, `x:Name`s, style keys, and declared
  resource keys carried per node (copy-don't-coin; omission over invention).
  No JSON Schema change (`properties` is open).
- `SKILL.md` 0.4: Target-framework section; generators populate the mapping
  layer and use the Uno Platform docs MCP to resolve control identity and
  Themes/resource idioms for it. Semantic-layer rules from 0.2/0.3 unchanged.
- `docs/uno-mcp-integration.md`: verdict revised — the MCP is now part of
  graph generation (mapping layer), not just implementation arms.
- Eval 05 gold v1.3: Uno mapping retrofitted onto 40/47 nodes from verified
  source facts.
- Blind-v3 (agnostic v0.3 rules) archived unscored; run reports show sizes
  converging to 61–64 nodes and `uses-token` down to 51–60 (was 66–71).
  Next validation round runs under v0.4 and scores mapping-layer recovery.

## 0.3.0 — 2026-08-12

Driven by the blind-v2 replication (naming and state altitude fixed; residual
drift concentrated in token wiring and component internals — see
`evals/05-orbital-settings/blind-v2/README.md`):

- `SKILL.md` Pass 3 / `references/ontology.md`: canonical-component internals
  are `properties` on the canonical node, never child nodes per instance.
- `SKILL.md` Pass 5 / `references/token-rules.md` /
  `references/ontology.md`: `uses-token` attaches once per token per concept,
  on the canonical component or screen; instances inherit, and get their own
  edge only when overriding (blind-v2 showed 6× edge inflation from
  per-instance wiring). Interaction-only variants (hover/pressed shades,
  alpha steps) fold into the base token; resting-surface emphasis levels stay
  distinct.
- `prompts/design-implement.md` rule 9 + `docs/uno-mcp-integration.md`:
  assessed the Uno Platform MCP server — not applicable to Design Graph rules
  (framework-agnostic layer by design), but adopted for Uno implementation
  arms (usage rules + docs grounding, identical across A/B arms) and as the
  best static verification when no compiler is available.

## 0.2.0 — 2026-08-12

Driven by the blind 5× replication of eval 05 (semantics stable, lexical
stability failing the kit's own exit criteria — see
`evals/05-orbital-settings/blind/README.md`):

- `SKILL.md` Pass 8: binding ID grammar (three dot-segments max; canonical
  components `component.<slug>`; source-declared names slugified, never
  re-synonymized) plus a fixed naming vocabulary (card / info-row / field /
  action-button / section-title). Pass 4: state-altitude rule (no style-level
  hover/pressed/disabled states in screen graphs) and smallest-changing-node
  `has-state` attachment. Pass 5: tokens scoped to values the modeled surface
  consumes.
- `references/ontology.md`: `state` scope + attachment rules formalized.
- `references/token-rules.md`: screen-scoped token extraction made binding;
  whole-dictionary enumeration belongs to a separate design-system graph.
- `scripts/score_graph.py` (scorer 0.2.0): new `node_concept` dimension
  (type + name/text word tokens) that survives id-spelling drift and now
  counts toward macro F1; hallucination proxy rewritten from exact-triple
  matching to relation + stemmed-endpoint-token matching — all five blind
  runs' real behaviors clear it, while an injected fake `navigates-to` is
  still caught. Macro F1 values are not comparable across scorer versions.
- Eval 05 gold v1.2: `state.profile.saved` re-attached from the profile card
  to the Save button per the new attachment rule; granularity-altitude
  contract recorded in the eval README.

## 0.1.1 — 2026-08-12

- `score_graph.py`: `unresolved` items now match on sorted `relatedIds` instead
  of the exact `(question, relatedIds)` tuple. Two runs flagging the same
  ambiguity always word the question differently, so exact matching scored
  semantically identical uncertainty at 0.0 (observed in eval 05 run 1).
- New eval `05-orbital-settings`: first real, source-backed case (Orbital
  `SettingsPage` XAML + code-behind + style dictionaries).
- Eval 05 gold v1.1: fixed two answer-key recall defects found during review —
  the page header is a *declared* reusable `PageHeader` control (with a
  search / command-palette affordance gold had missed entirely), and
  `Clear Recent Projects` triggers a declared "Cleared" `ContentDialog`.
  Both corrections are source-driven, not model-driven.

## 0.1.0 — 2026-08-12

- Initial Design Graph ontology.
- JSON Schema with provenance, inference confidence, and unresolved ambiguity.
- Design Graph generation Skill.
- Manual understanding and implementation prompts.
- Four starter evals with gold graphs.
- Validation and deterministic scoring tools.
- Staged testing, A/B implementation, and round-trip parity plan.
