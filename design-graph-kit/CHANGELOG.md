# Changelog

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
