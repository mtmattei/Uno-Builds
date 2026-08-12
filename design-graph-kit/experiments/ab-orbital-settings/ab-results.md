# A/B Results — Orbital Settings (Stage 5)

Date: 2026-08-12 · Model (both arms): Claude Fable 5 · One shot per arm, no builds.
Protocol: see `README.md`. Arm A: `brief.md` only. Arm B: `brief.md` +
`skill.graph.json` + `prompts/design-implement.md`. Neither arm could read the
real Orbital app the brief was derived from.

## Measurements

### Where the arms tied (both strong)

| Dimension | A — baseline | B — graph | Verdict |
|---|---|---|---|
| Hardcoded hex in page XAML | 0 (all in Tokens.xaml) | 0 (all in Tokens.xaml) | tie |
| Resource keys / StaticResource refs | 57 / 55 | 44 / 66 | tie (A more granular keys, B denser reuse) |
| Repeated-structure consolidation | card, label, value, section-header, ghost-button styles — all shared | same set, same reuse counts (4/8/7/4/3) | tie |
| Layout fidelity to brief | full (header, 4 cards, 2-column) | full | tie |
| Size | 588 lines | 709 lines | — (B larger because behavior exists) |

A skilled one-shot model needs no graph to produce clean, tokenized,
well-factored *visuals* from a good brief. The graph's value did not show up
in pixels or code hygiene.

### Where they diverged

| Dimension | A — baseline | B — graph |
|---|---|---|
| **State coverage** (3 source-backed behaviors) | **0 / 3** — Save, Clear, Open-folder are TODO stubs | **3 / 3** — staggered entrance fade-up, 1.5 s "Saved!" flash, "Cleared" dialog |
| **Invented behavior** | **1** — wired the docs button to a guessed URL (`https://platform.uno/docs/`), unsupported by its input and not the app's real target | **0** — docs URI taken from the graph (declared, confidence 1.0); search pill and data-folder deliberately left unwired because the graph marks them unresolved/undeclared, each documented |
| **Fidelity to the real app** (which neither arm saw) | behaviors absent; guessed URL wrong | entrance stagger 0/100/200/300 ms and cubic ease **match the real `AnimationHelper.FadeUp` timings**; dialog title/body/button text **verbatim identical** to the real code-behind; docs URI exact |
| **Semantic traceability** | reasonable but disconnected names (`DisplayNameBox`, `RecentDbValue`) | x:Names match the real app's declared names (`ProfileSection`, `UsernameBox`, `SaveButton`…) via the graph; 41 graph-id comments (`token.*`, `state.*`, `component.*`) link code back to graph nodes |
| **Uncertainty handling** | n/a (brief carries no uncertainty) | honored all three `unresolved` items; documented its one judgment call (wiring open-docs, whose *behavior* is declared even though its *modeling* is unresolved) |

The interesting failure is arm A's docs button. A was told nothing about
behavior, correctly stubbed three buttons — and then invented the fourth,
because "documentation" *feels* safe to wire. That is exactly the
plausibility-promoted-to-fact failure `references/inference-rules.md` exists
to prevent, reproduced in the wild on the first run. The graph arm, bound by
`design-implement.md` rule 2 ("do not invent behavior that is unresolved"),
made the opposite call on the two genuinely unknown actions and got the known
one exactly right.

## Verdict

**B shows a material downstream advantage — on semantics, not visuals.**

- Visual/code quality: no advantage. A good brief was sufficient.
- Behavior: the graph transported source-backed semantics (states, triggers,
  a URI, real component names) across a handoff that otherwise loses them —
  B reproduced the real app's behavior nearly exactly without ever seeing it,
  while A produced stubs plus one hallucinated action.
- Round-trip readiness: B's graph-id traceability makes Stage 6 (semantic
  parity between Graph A and a graph re-extracted from the implementation)
  actually tractable.

Per the kit's decision rule ("continue only if B produces a meaningful
downstream advantage — fewer corrections, better state coverage, more reliable
agent edits"): **continue to Stage 6**, with the caveats below resolved first.

## Caveats — read before generalizing

1. **n = 1 per arm.** Directional pilot, not statistics.
2. **The information asymmetry is the treatment, but it isn't isolated.** B's
   win could partly come from receiving *any* extra semantic notes, not from
   the graph *structure*. A future arm C (brief + free-prose behavior notes)
   would separate "graph as structured IR" from "more information."
3. **Nothing was compiled or rendered.** Visual parity was assessed
   structurally; both arms may contain XAML that needs touch-ups to build.
4. **Same model family everywhere** (graph author, arm A, arm B).
5. The graph came from *source analysis* of a working app. For a
   design-first workflow (Figma → graph), states/triggers would carry
   inferred/declared-by-designer evidence instead — the transport mechanism is
   the same, but the input honesty profile differs.

## Score summary (Stage-5 dimensions)

| Dimension | Winner |
|---|---|
| Visual parity (structural) | tie |
| Component reuse | tie |
| Design-system/token consistency | tie |
| Semantic structure | **B** |
| State coverage | **B** (3/3 vs 0/3) |
| Unsupported behavior | **B** (0 vs 1) |
| Correction prompts | tie (0 by design; A would need corrections to reach behavioral parity) |
