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

---

# Follow-up 1 — Arm C: brief + free-prose notes

Arm C received `brief.md` plus `notes.md` — the graph's semantic delta
transcribed into unstructured prose (same states/timings, dialog copy, docs
URI, naming and reuse conventions, same "don't wire" unknowns). Same
isolation, same one-shot budget.

**Result: C matched B on every semantic measure.**

| Dimension | B (graph) | C (prose notes) |
|---|---|---|
| Hardcoded hex in page | 0 | 0 |
| Repeated-structure consolidation | full | full (same style-reuse counts: 8/7/4/4/3) |
| State coverage | 3/3 | 3/3 (stagger 0/100/200/300 ms, 350 ms ease-out; "Saved!" 1.5 s; "Cleared" dialog verbatim) |
| Invented behavior | 0 | 0 (search + data-folder left unwired, documented) |
| Real developer names | yes (via graph) | yes (via notes) |
| Static verification (well-formed, resources, handlers) | pass | pass |

**Interpretation — this reframes the Stage-5 verdict.** The A-vs-B gap was
never about the graph's *structure*; it was about whether the handoff carried
behavioral semantics at all. Any faithful carrier (graph or careful prose)
closes the gap for a single screen implemented by a strong model in one shot.

What prose cannot do — and where the graph's structure actually earns its
keep — is everything *around* the one-shot generation: schema validation,
integrity checking, deterministic drift scoring, querying, and round-trip
diffing (below). Prose notes cannot be validated, diffed, or re-extracted;
`notes.md` was itself hand-derived *from* the graph. The honest claim for the
graph is therefore: **structured semantic transport with machine-checkable
provenance** — not "better codegen than good notes."

---

# Follow-up 2 — Stage 6: round-trip semantic parity

An isolated agent re-extracted a Design Graph from arm B's implementation
(the three B-graph files as sole source; `roundtrip.graph.json`), which was
then diffed against the original `skill.graph.json`.

**Headline: node-id recall 1.000.** Every one of the original graph's 50
concepts survived design → graph → implementation → graph with its identity
intact — the graph-id comments and stable x:Names in B's code made the
implementation *semantically traceable*. All four behavioral relationships
survived; the extractor also independently re-discovered the same
`unresolved` questions (search target, row/field consolidation) plus honest
new ones (data bindings, persistence).

Real parity findings the diff surfaced (the purpose of Stage 6):

1. **Implementation-introduced tokens (12):** interaction shades
   (emerald-400/600, surface2/4), keycap radius 4, padding values, a display
   typography token — the implementation legitimately needed values the
   design graph never modeled. In a live system these would flow back as
   candidate design-system additions.
2. **State attachment drift:** the original attaches `state.profile.saved`
   to the profile card; the round-trip attaches it to the Save button
   (`control.profile.save -has-state->`), which is arguably *more* precise.
   Ontology gap: no rule for which node owns a transient state.
3. **Scorer brittleness (again):** deterministic macro F1 was only 0.63
   despite perfect concept recall, because `node_signature` compares
   role/semanticRole strings verbatim and the extractor chose near-synonyms.
   Signature matching needs normalization before round-trip diffs can be
   automated.

**Verdict:** the round-trip mechanism works and produces actionable parity
findings rather than noise. It is the strongest evidence so far for the
graph-as-structured-IR position — none of it is possible with prose notes.

---

# Follow-up 4 — Arm B2: implementation from a blind v0.4 graph

After the Uno-first pivot, the treatment was re-run as **B2**: same brief,
same isolation and one-shot budget as arm B, but the graph is now a
**blind-generated v0.4 graph** (`blind-v4/run4.graph.json` — a pipeline
artifact, deliberately *not* the answer key) carrying the `properties.uno`
mapping layer. No MCP tools, matching B's tooling exactly, so the only
delta vs B is the graph version.

**Result — the mapping layer closes the traceability gap to 100%:**

| Measure (independently audited vs the real Orbital source) | B (pre-uno graph) | **B2 (v0.4 graph)** |
|---|---:|---:|
| Real Orbital resource/style keys adopted verbatim | 13/18 | **18/18** |
| Real x:Names adopted verbatim | 8/10 | **10/10** |
| Declared states/triggers implemented | 3/3 | **3/3** (+ SearchRequested event raised, target honored as unresolved) |
| Docs URI / dialog copy exact | yes | yes |
| Invented behavior | 0 | 0 |
| Static verification | pass | pass |

B2's page defines its ResourceDictionary with the *original app's* key names
(`OrbitalSurface1Brush`, `OrbitalCardStyle`, `OrbitalPrimaryButtonSm`, …)
and element names, none of which appear in the brief — they traveled
design → blind graph → implementation intact. That makes the output
drop-in reconcilable with the source design system and makes Stage-6
round-trip diffs exact by construction. Notable detail: where the brief
and the graph disagreed (title size), B2 followed the graph's declared
design-system value and documented the conflict — the graph functioning
as the source of truth, as `design-implement.md` intends.

# Follow-up 3 — Static verification of all arms

No .NET SDK exists in this environment, so the arms were not compiled.
Maximum available static verification was applied to all three arms
(XML well-formedness of every XAML file; every `StaticResource`/
`ThemeResource` reference resolves to a key defined in the arm's own files;
every event handler named in XAML exists in the code-behind):
**A, B, and C all pass all checks.** Compilation and pixel-level parity
remain open until run in an environment with the Uno toolchain.
