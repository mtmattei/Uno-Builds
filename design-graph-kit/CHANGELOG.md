# Changelog

## 0.6.0 — 2026-08-12

Rule and scorer round driven by eval 09 (Composer Shell) and the eval-08 image
round, plus the kit's first execution against real toolchains and real sources.

**Scorer 0.4.0 — two defects fixed, both found by running on real data.**
Macro F1 values are not comparable with 0.3.0 results.

- The **hallucination proxy no longer conflates target choice with invention.**
  Requiring both endpoints to match gold flagged all five runs in evals 05, 07
  and 09; every flagged edge was verified by hand as a real code path. The
  cause is multi-effect actions — a layer-row click both swaps the canvas and
  opens the rails, so gold and a run each record a different true target. The
  proxy now flags an edge only when its **source** is one gold never says
  triggers anything; a divergent target on a real source is reported as
  `divergent_behavior_targets` and does not raise the flag. Re-verified against
  an injected fake edge, which is still caught.
- Endpoint identity now unions **id segments with name/text tokens**. Names
  drift harder than ids ("Expand toggle" vs "Locked context card" for the same
  control), which was making real behaviors read as invented.
- The **`unresolved` dimension survives id drift.** 0.1.1 fixed it for question
  *wording*; eval 08 showed ids were still fatal (four of five runs scored
  0.000 while naming the same gaps as gold). It now matches by greedy overlap
  of id tokens rather than set equality: eval-08 runs 0.000 → 0.125–0.400,
  eval-09 runs 0.000 → 0.154–0.667.

**Rules — three gaps closed, each with fleet evidence.**

- `SKILL.md` Pass 8: **precedence when a declared name and the naming
  vocabulary disagree** — the source-declared name wins (`component.layer-row`,
  not `component.layer-item`); the vocabulary governs concepts the source does
  not name. Eval 09's fleet split 3–2 with both sides correctly citing Pass 8,
  which contradicted itself until now.
- `references/ontology.md`: **multi-effect rule** — one `triggers` edge per
  declared effect, never a chosen "main" one.
- `references/ontology.md`: **when to emit a `region`** — a structural grouping
  that owns layout or state, not every layout panel. Ten independent runs
  across evals 08 and 09 emitted regions unprompted while gold 05 has zero;
  gold 05 is **queued for revision through the human review** rather than
  silently recalibrated by the lineage that authored it.

**Tooling.**

- `tools/build_review_packet.py` (new): a node-by-node review sheet per gold
  that classifies every quoted `properties.uno` value as fabricated, compound,
  or miscited. It reported 26 fabrications against a clean gold 09 before
  expression-valued keys were handled — fixed; all five source-backed golds now
  verify with **0 fabricated identifiers**.
- `tools/audit_designfirst.py` and `tools/stability.py`: both carried hardcoded
  sandbox paths and had never run outside the sandbox. Parameterized.
- `tools/Capture-Window.ps1` (new): occlusion-proof capture with a blank-bitmap
  guard, since `PrintWindow` can report success and return a uniform image.
- `experiments/ab-orbital-settings/ArmHost`, `run-arms.ps1`,
  `verify-interactions.ps1` (new): compile, launch and drive the A/B arms.

**Evals.**

- New `08-pens-beers`: image input — recorded honestly as a *screenshot* round,
  since the input is a render of a running app rather than a design export.
  MVVM; 5/5 honesty-perfect.
- New `09-composer-shell`: MVUX, the densest design system in the pool. Token
  scoping and component expansion both held 5/5 under the largest dictionary
  set the kit has faced.
- Gold 08 corrected: `iconGlyph` held prose pairs rather than source values.

**Experiments.** All four A/B arms compile with zero arm-code changes; arm A
still dies at first frame on a relocated `ms-appx:///` URI. Both transient
behaviors verified live in the graph arms, with dialog copy character-identical
to a code-behind their authors never saw. Arm A's guessed docs URL is confirmed
wrong against the real source.

## 0.5.0 — 2026-08-12

Consensus calibration round from eval-07's unanimous blind findings, plus
the design-first pilot:

- `SKILL.md` Pass 8: screen-slug rule (generic page names take the app
  prefix: `screen.caffe-main`); source-declared reusable controls
  (UserControls) are always `component`, with interactivity in `role`.
- `references/ontology.md`: condition-vs-presentation rule (one logical
  condition driving several nodes → one state per affected node, named for
  its local presentation, linked via `properties.uno.member`); trigger
  attachment rule (canonical component when instances trigger identically).
- Eval-07 gold v1.1 recalibrated to the 5/5 consensus on all four points +
  token breadth; blind runs re-score macro 0.370 → **0.521**, node-id F1
  0.55–0.59 → **0.80–0.86**.
- Design-first pilot: `evals/05-orbital-settings/design-first/` — five
  blind runs generating graphs from the visual brief alone (no source),
  measuring structural recall, behavior honesty, and mapping-layer
  fabrication discipline. Results in its README.

## 0.4.2 — 2026-08-12

- `SKILL.md` Pass 8 vocabulary (from eval-06 blind findings): `card` now
  explicitly covers any bordered/rounded grouping container regardless of
  visual treatment; new list-row entry (`<content>-item`, fallback
  `list-item`). Eval-06 gold v1.1 renamed `component.glass-panel` →
  `component.card` to obey the kit's own rule (blind runs that followed the
  vocabulary re-score higher: run3 macro 0.47 → 0.53).
- New eval `07-caffe-main`: third architecture — **MVVM**
  (CommunityToolkit ObservableObject / RelayCommand / x:Bind) on a
  composed-custom-controls dashboard with two interacting screen states.
- Packaged as a repo-level Claude Code skill:
  `.claude/skills/design-graph/SKILL.md` (generate / implement / score
  modes wrapping the kit).

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
