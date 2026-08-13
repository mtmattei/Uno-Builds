# A/B Value Experiment — Orbital Settings (Stage 5)

The kit's decisive test (`docs/testing-plan.md` Stage 5): does inserting a
Design Graph between design and implementation *materially improve* the
implementation? Generating a valid graph is table stakes; this is the value
question.

## Protocol

Two isolated single-pass implementation agents, same model, same framework
target (Uno Platform / WinUI 3 XAML), same iteration budget (one shot, no
builds), same visual design brief:

| Arm | Inputs |
|---|---|
| **A — baseline** | `brief.md` only |
| **B — treatment** | `brief.md` + `evals/05-orbital-settings/skill.graph.json` + `prompts/design-implement.md` |

Both arms were forbidden from reading anything else in the repository
(especially the original `Orbital` source the brief was derived from, and the
rest of the kit), so neither could peek at the reference implementation.

`brief.md` is deliberately **visual-only** — it describes the static populated
mock a designer would hand off, with annotated colors/spacing/typography but
no interaction or state notes. The graph additionally carries the semantics
that survive source analysis but die in a static handoff: canonical components
with `instance-of`, `uses-token` relationships, two source-backed states
(entrance, transient "Saved!"), two `triggers` edges (Save → Saved!,
Clear → "Cleared" dialog), and three `unresolved` items. That asymmetry is not
a confound — it *is* the treatment. The graph's claimed value is precisely
that it preserves machine-readable semantics alongside the pixels.

## Outputs

- `A-baseline/` — arm A's implementation
- `B-graph/` — arm B's implementation
- `ab-results.md` — static comparison and verdict

## Scoring dimensions (from `docs/testing-plan.md` Stage 5)

- token/design-system consistency (hardcoded values vs defined-once resources)
- component reuse (styles/templates for repeated structures vs copy-paste)
- state coverage (entrance, saved-confirmation, cleared-dialog)
- semantic structure and hierarchy
- unsupported/invented behavior (hallucination)
- correction prompts required (0 for both by design — single pass)

## Known limitations

- Single run per arm; no statistical power. Treat as a directional pilot.
- Implementations were not compiled or rendered — visual parity is assessed
  structurally, not by pixel diff.
- Both arms use the same model family as the graph author.
