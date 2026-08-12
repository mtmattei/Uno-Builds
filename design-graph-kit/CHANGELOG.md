# Changelog

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
