# Design-First Pilot — graph from a visual spec alone

The last untested frontier: five fresh-context blind agents generated a
Design Graph from `experiments/ab-orbital-settings/brief.md` **only** — the
written visual mock of the Orbital settings screen — with no access to any
application source. This is the product scenario's input shape
(Figma/screenshot → graph), run against a screen whose *source-backed* gold
exists for comparison. Kit v0.5, scorer 0.3.0. All five validated.

## Honesty: 5/5 perfect — the headline result

| Check | Result |
|---|---|
| Behavioral edges (`triggers`/`navigates-to`) from a static mock | **0 in all five runs** |
| Fabricated declared identifiers (resource keys, x:Names, classes) | **0 in all five runs** — every `properties.uno` entry is a proposed control type marked `inferred` |
| Interaction intent | parked in 6–9 `unresolved` items per run |
| Hallucination proxy | false in all runs |

The evidence rules held exactly where design-first input makes them hardest:
nothing that only source code could know was asserted. Notably, all five
runs *unanimously* derived the same two legitimate states
(`state.about.loading`, `state.paths.loading`) from the mock's visible `…`
placeholder values — observable evidence, identically interpreted.

## Structure: stable and correctly consolidated

- Every run produced **identical structural skeletons**: 26 `contains` +
  14 `instance-of` — the 14 instance-of edges exactly match the
  source-backed gold's consolidation (4 cards, 4 info-rows, 3 path-fields,
  3 action-buttons).
- Cross-run stability: mean pairwise macro **0.66** (node-id up to 0.95) —
  comparable to source-backed fleets, from a purely visual input.

## vs the source-backed gold: 0.30–0.34 node-id recall — the honest gap

This number is **diagnostic, not a failure**: the source-backed gold
contains what a static mock cannot know — the entrance/saved states, the
Cleared dialog, the search event target, binding paths, and tokens *named
from declared resource keys* (`token.color.surface1`) where design-first
runs correctly used neutral value-based names (`token-rules.md` prescribes
exactly this difference per input type). The delta between a design-first
graph and a source-backed graph of the same screen is precisely the
information a design handoff loses — which the kit's A/B experiments showed
is the graph's most valuable cargo when it *is* available.

## Implication for the product flow

Design-first generation is **safe** (nothing invented, uncertainty explicit)
and **structurally reliable** (consolidation and hierarchy match source-
backed analysis). The workflow composes: design-first graph at handoff →
implementation → source-backed re-extraction enriches the same graph with
declared identity and behavior — each layer marked with its evidence kind.

Remaining before production claims: repeat with a real **image/Figma**
input (this pilot used a written mock description — faithful, but text),
and compile a generated implementation in a toolchain-equipped environment.
