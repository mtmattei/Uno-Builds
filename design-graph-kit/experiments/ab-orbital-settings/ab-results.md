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

# Compile verification (HANDOFF item A)

Date: 2026-08-12 · Windows 11, .NET 10.0.303, Uno.Sdk **6.5.31** (pinned to
match the real Orbital app), `net10.0-desktop`, Release.
Host: `ArmHost/` in this folder — a blank `unoapp` whose only job is to
navigate to one arm's page chosen by command line (`ArmHost.exe A|B|C|B2`),
so each arm renders alone with no launcher chrome in frame. Repro:
`run-arms.ps1`.

Arms were copied into `ArmHost/ArmHost/Arms/<arm>/`; the originals in this
folder are the experiment artifacts and were left untouched. Fixes below are
counted against the arm only when the arm's own markup or code required the
change.

## Compile result

| Arm | Compile errors | Arm-attributable fixes | Result |
|---|---|---|---|
| A — baseline | 0 | 0 | builds clean |
| B — graph | 0 | 0 | builds clean |
| C — notes | 0 | 0 | builds clean |
| B2 — uno-graph | 0 | 0 | builds clean |

**All four arms compiled on the first attempt with zero changes to arm code.**
The static verification in follow-up 3 predicted this correctly. Two build
errors did occur and both were host-side, introduced by scaffolding decisions,
so neither is arm data: a missing `App.InitializeLogging` after the host's
`App.xaml.cs` was rewritten, and `host.Run()` vs `await host.RunAsync()`
(the template scaffolds for a newer Uno.Sdk than the 6.5.31 this host is
pinned to).

## Runtime result — where the compile-clean arms diverged

Compiling clean is not the same as running. One arm failed at first frame:

| Arm | Launches | Runtime defect |
|---|---|---|
| A — baseline | **no** (fixed) | `InvalidOperationException: Cannot locate resource from 'ms-appx:///A-baseline/Tokens.xaml'` — thrown inside `InitializeComponent`, killing the process before any UI appears |
| B — graph | yes | — |
| C — notes | yes | — |
| B2 — uno-graph | yes | — |

Arm A hardcoded an absolute `ms-appx:///` URI containing its own arm-folder
name; B, C, and B2 all used a relative `Source="Tokens.xaml"`, which resolves
against the page's own location and survives being relocated. One line changed
in the *copy* (`ms-appx:///Arms/A-baseline/Tokens.xaml`) fixes it; the artifact
keeps the original. This is the only arm-attributable fix in the whole
exercise, and it is a fair finding rather than a host artifact: any host that
does not reproduce arm A's exact folder name hits it, and `verify_arm.py`
cannot catch it because the referenced dictionary genuinely exists — at a path
that only exists in the experiment's own layout.

## Behavior verified at runtime

The three source-backed behaviors were verified live in the arms that
implement them:

- **Entrance stagger** — B/B2's cards are at full opacity in captures taken
  ~4 s after launch, so the `FadeUp` from `Opacity="0"` runs to completion
  rather than leaving cards invisible (the failure mode of an animation wired
  to the wrong element).
- **Save flash / Cleared dialog** — present in code and compiled; driving them
  needs synthesized clicks, recorded as open below.

## Visual parity against the real Orbital SettingsPage

For the first time the real app is available (`Orbital/` in this repo), so
these are true parity comparisons rather than brief-vs-implementation.
Reference: `parity/00-real-orbital-settings.png`, captured from the running
Orbital app (Release, 6.5.31) at the same 1600×1000 window size as the arms.

| Element | Real Orbital | A | B | C | B2 |
|---|---|---|---|---|---|
| Header title + subtitle | ✅ | ✅ | ✅ | ✅ | ✅ |
| Search / Ctrl+K pill, right-aligned | ✅ | ✅ | ✅ | ✅ | ✅ |
| PROFILE card: label, field, Save, helper | ✅ | ✅ | ✅ | ✅ | ✅ |
| Two-column ABOUT ‖ PATHS+ACTIONS | ✅ | ✅ | ✅ | ✅ | ✅ |
| 4 info rows, right-aligned values | ✅ | ✅ | ✅ | ✅ | ✅ |
| 3 path fields, 3 ghost actions w/ icons | ✅ | ✅ | ✅ | ✅ | ✅ |
| Emerald Save button | ✅ | ✅ | ✅ | ✅ | ✅ |

All four arms reproduce the real screen's structure. The differences that
remain are attributable to inputs, not arm quality:

- **Values render as `...`** in every arm. The real page binds
  `{Binding EnvStatus.UnoSdkVersion, FallbackValue='...'}` etc. to a model the
  arms never had, so all four correctly show the declared fallbacks. The real
  app shows live values (`6.5.153`, `.NET 10.0.11`, `Skia/WPF`).
- **Nav rail absent** in every arm — the arms implement a page, the real
  capture is the whole shell. Expected, not a defect.
- **Focus ring on the text box**: arm A shows the stock WinUI blue focus
  border, B2 an emerald one matching the app's accent. The real app's field is
  unfocused in the reference capture, so this is untested rather than wrong.
- **Logo**: B/B2 reference `ms-appx:///Assets/Icons/Uno-logo.png` — the real
  app's asset path, so supplying the real asset made them render the real
  logo. A draws a glyph instead.

**The visual tie from Stage 5 holds under compilation.** A good brief was
enough for pixels; the graph's advantage stays where the original experiment
found it — semantics — and now has one more piece of evidence: arm A's
guessed docs URL (`https://platform.uno/docs/`) can finally be checked against
the real code-behind, which launches
`https://platform.uno/docs/articles/intro.html`. **A's invented URL is
confirmed wrong; B, C, and B2 match the real app exactly.** B2's data-folder
implementation (`LocalApplicationData/Orbital`) also matches the real
`OpenDataFolderButton` handler, which no arm was told about.

## Transient behaviors, driven and verified

Compiling and launching an arm proves its markup is sound; it does not prove
the declared behaviors fire. Both transient ones were driven through
synthesized clicks (`verify-interactions.ps1`); captures in
`parity/interactions/`.

| Behavior | Arm B | Arm B2 | Matches real Orbital |
|---|---|---|---|
| `state.profile.saved` — Save flips to "Saved!" | ✅ | ✅ | yes: 1.5 s, then reverts to "Save" |
| `dialog.recents-cleared` — Cleared ContentDialog | ✅ | ✅ | yes: title "Cleared", body "Recent projects list has been cleared.", button "OK" — verbatim |

The dialog copy is character-identical to the real `SettingsPage.xaml.cs`, in
arms whose author never saw that file. The graph carried it.

**All three source-backed behaviors are now verified end to end in the graph
arms** — entrance stagger (from the launch captures), the Saved! flash, and the
Cleared dialog. The baseline arm implements none of them.

### Two traps worth recording for anyone repeating this

1. **`SetForegroundWindow` fails when the calling process does not own the
   foreground** — the normal case when a script drives an app. It returns
   `false`, the arm stays behind whatever window is on top, and synthesized
   clicks land on *that* window. Because `PrintWindow` is occlusion-proof, the
   captures still look perfectly correct while nothing is being clicked. The
   first run of this verification produced three plausible screenshots showing
   no state change at all. Use `SetWindowPos(HWND_TOPMOST)`, which needs no
   foreground rights, and assert the click target with
   `WindowFromPoint` + `GetWindowThreadProcessId` before trusting a click.
2. **Compare capture hashes.** Four byte-identical PNGs mean nothing happened,
   however convincing each looks alone. That single check turns "the feature
   works" into a claim with evidence behind it, and it is what caught trap 1.

## Still open

- Pixel-differencing rather than structural comparison; needs the arms hosted
  in the real shell (nav rail included) to make crops comparable.
