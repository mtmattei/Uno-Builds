# 06-flux-profile

Second **real, source-backed** eval: the FluxTransit `ProfilePage`
(MVUX + Uno Toolkit + glass design system). See `fixture.md`.

## Granularity altitude (binding — same contract as eval 05)

- Screen semantics only; style-level interaction visuals are not states.
- Tokens are screen-consumed declared values (21 in gold), wired to
  consumers at the v1.4 consensus altitude (typography/color per consumer;
  canonical components own shared tokens; instances only override).
- Canonical component internals (`route-item` parts) are `properties`,
  not child nodes.
- IDs follow SKILL.md Pass 8; scope slugs: `header`, `opus`, `routes`,
  `settings`, `footer`.
- Every node with declared Uno identity carries `properties.uno`
  (`references/uno-mapping.md`).
- gold: `state.opus.refreshing` attaches to the OPUS panel (the button ↔
  progress swap happens inside it), triggered by `control.opus.update`.

## Files

- `gold.graph.json` — hand-authored answer key (57 nodes / 74 edges /
  4 unresolved), same-author caveat as eval 05 applies.
- `blind/` — fresh-context blind replication runs.

Validate/score from the kit root:

```bash
python scripts/validate_graph.py evals/06-flux-profile/gold.graph.json
python scripts/score_graph.py evals/06-flux-profile/gold.graph.json evals/06-flux-profile/blind/run1.graph.json
```
