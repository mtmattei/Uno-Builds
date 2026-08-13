# HANDOFF — Design Graph Kit v0.5

Everything in this kit was produced and validated inside a cloud sandbox
(no .NET toolchain, no design files, single-author). Four items remain that
need resources the sandbox lacked. Each has a step-by-step below. Do them in
any order; A and B together constitute the full real-scenario pilot.

Current state: kit v0.5.0 · 3 evals (code-behind / MVUX / MVVM) · 50 blind
runs, 0 hallucinations · A/B/C/B2 implementation arms · round-trip node-id
recall 1.000 · design-first pilot passed · Claude Code skill in
`.claude/skills/design-graph/`. Full record: `RESULTS.md`, `CHANGELOG.md`,
`evals/experiment-log.csv`.

---

## Status update — 2026-08-12, Windows machine with the real sources

The sandbox's two missing resources turned out to be available here: this repo
holds the actual `Orbital/`, `FluxTransit/`, and `Caffe/` apps the evals were
authored from, and the machine has .NET 10 + Uno.Templates. That closed most of
the list.

| Item | State | Evidence |
|---|---|---|
| **A** — compile + visual parity | **done** | All four arms compile with zero arm-code changes; arm A crashes at runtime on a relocated `ms-appx:///` URI; visual parity against the real Orbital SettingsPage recorded. `experiments/ab-orbital-settings/ab-results.md` → "Compile verification"; captures in `experiments/ab-orbital-settings/parity/` |
| **B** — image input | **scaffolded, blocked** | `evals/08-image-input/README.md` has the protocol; `tools/audit_designfirst.py` is parameterized and reproduces the pilot result. Waiting on a real design image — see below |
| **C** — human gold review | **packet built, review open** | `tools/build_review_packet.py` generates `gold-review.md` per eval; automated pre-pass found **0 fabricated identifiers** across all three golds. The independent human pass is still required and is the only thing that discharges this item |
| **D** — installable plugin | **done, unpublished** | Self-contained plugin repo built and committed locally at `../uno-design-graph-plugin`. Not pushed to GitHub — publishing is a human call |

What changed in the findings, beyond ticking boxes:

- **Arm A's invented docs URL is now confirmed wrong** against the real
  code-behind (`https://platform.uno/docs/` vs the real
  `https://platform.uno/docs/articles/intro.html`). The Stage-5 claim was
  inference; it is now a checked fact. B, C, and B2 match exactly, and B2's
  data-folder path matches an implementation detail no arm was told about.
- **Compiling clean and running clean are different bars.** All four arms
  passed static verification *and* compilation; one still died at first frame.
  `verify_arm.py` cannot catch it, because the resource it points at genuinely
  exists — at a path that only exists in the experiment's own layout.
- **The uno mapping layer survives contact with the real source.** Across 3
  golds, every quoted identifier exists in the app it claims to come from.
  The residue is hygiene, not invention: one node cites its design system by
  glob label rather than a path, and two pack prose into a `member` field.
- **Gold 05 models no `region` nodes** while the real page is a prominent
  two-column arrangement (ABOUT ‖ PATHS+ACTIONS) and the cited XAML declares 27
  layout containers. The brief given to the implementation arms *did* describe
  the two-column row, so the arms got it right. Whether the graph should carry
  that arrangement is a genuine altitude question, now check 6 in the review
  packet.

### What item B still needs

A real design image. Scanning every app in this repo found hundreds of
screenshots but essentially all are **output** captures of implemented screens;
the genuine design inputs here are written briefs, the shape the design-first
pilot already tested. Drop a Figma export at
`evals/08-image-input/design.png` and the round is one command away.
See `evals/CANDIDATES.md` for the apps pairing a written design spec with an
implementation and screenshots — Meridian and QuoteCraft allow a three-way
spec → graph vs source → graph vs image → graph comparison on one screen.

---

## A. Compile the four implementation arms + visual parity (needs Uno toolchain)

The arms pass static verification (well-formed XAML, all resource refs
resolve, all handlers exist — `tools/verify_arm.py`) but have never been
compiled. Machine: Windows/macOS/Linux with .NET 9 SDK.

1. Verify the toolchain:
   ```bash
   dotnet tool install -g uno.check && uno-check   # fix anything it flags
   dotnet new install Uno.Templates
   ```
2. Create a desktop-only host app (fastest inner loop):
   ```bash
   dotnet new unoapp -o ArmHost -platforms desktop
   ```
3. For each arm in `design-graph-kit/experiments/ab-orbital-settings/`
   (`A-baseline`, `B-graph`, `C-notes`, `B2-uno-graph`):
   - copy its `SettingsPage.xaml`, `SettingsPage.xaml.cs`, `Tokens.xaml`
     into `ArmHost`;
   - align the namespace (`AbExperiment.A/B/C/B2`) with the host or add it
     to the project;
   - check the `Tokens.xaml` merge: arms reference it via page-local
     MergedDictionaries; fix the `Source` path if the folder layout differs
     (arm A uses an `ms-appx:///A-baseline/...` URI — adjust to the host
     layout);
   - point `MainWindow`/shell navigation at the arm's page.
4. Build and run:
   ```bash
   dotnet build ArmHost -f net9.0-desktop
   dotnet run --project ArmHost -f net9.0-desktop
   ```
5. **Record as data** (this is an experiment, not just a fix-up):
   - number and nature of compile fixes needed per arm — append a
     "Compile verification" section to `experiments/ab-orbital-settings/ab-results.md`;
   - screenshot each arm and compare against the real Orbital
     `SettingsPage` (run the Orbital app, same window size) — visual-parity
     notes per arm, and whether B/B2's entrance animation + "Saved!" flash +
     Cleared dialog behave like the original;
   - one row per arm in `evals/experiment-log.csv`.

## B. Image/Figma-input round (the real design-first scenario)

The design-first pilot used a *written* mock description; this round uses a
real image, which is the production input shape.

1. Export one screen design as PNG (Figma: Frame → Export; keep it a
   single, simple screen for round one) into
   `design-graph-kit/evals/08-<slug>/design.png`. Add any designer
   annotations as `notes.md` beside it.
2. In Claude Code at the repo root (the `design-graph` skill loads
   automatically), in a **fresh session**:
   > Generate a design graph of design-graph-kit/evals/08-<slug>/design.png,
   > save it as design-graph-kit/evals/08-<slug>/blind/run1.graph.json
   The skill enforces the rules; the agent reads the image directly.
3. Repeat in 4 more fresh sessions (`run2..run5`) — fresh sessions are the
   blind protocol; never show a session the other runs.
4. Validate and audit each:
   ```bash
   python3 design-graph-kit/scripts/validate_graph.py <run>
   ```
   Honesty bar (adapt `tools/audit_designfirst.py` paths): zero
   `triggers`/`navigates-to`, zero fabricated resource keys/x:Names, uno
   layer all `inferred`.
5. Author the gold **by hand from the image** (ideally the human reviewer
   from item C), following the altitude contract in any eval README; then
   score with `tools/stability.py blind 08-<slug>`.
6. Close the loop: implement from the best run's graph
   (`prompts/design-implement.md`, Mode 2 of the skill) and compile via
   item A. Design → graph → running app, end to end.

## C. Human pass over one gold (breaks same-author circularity)

Every answer key was authored and calibrated by the same agent lineage
(with blind-consensus correction). One independent human review of one gold
removes the last circularity.

1. Pick **eval 05** (`evals/05-orbital-settings/` — richest documentation).
2. Reviewer reads, in order: `fixture.md` (source list), the actual Orbital
   source files it names, `SKILL.md` (Pass 8 grammar + vocabulary),
   `references/ontology.md`, and the eval README's altitude contract.
3. Review `gold.graph.json` node-by-node against this checklist:
   - every node/edge evidence-backed by the named source (spot-check the
     rationales against the XAML/C#);
   - every source-backed component reference expanded (the PageHeader
     lesson);
   - altitude respected: no style-level states, tokens screen-scoped,
     canonical internals as properties;
   - `properties.uno` values copied exactly from source (keys, x:Names,
     types);
   - `unresolved` items genuinely undecidable from the source.
4. Write findings to `evals/05-orbital-settings/gold-review.md` (reviewer
   name + date + verdict per finding). Do not edit gold silently.
5. Apply accepted fixes via `tools/build_graphs.py` (edit → regenerate →
   `scripts/validate_graph.py`), bump the gold version in `CHANGELOG.md`,
   re-score the fleets (`tools/stability.py blind{,-v2,-v4} 05-orbital-settings`)
   and note deltas in `RESULTS.md`.

## D. Promote the Claude Code skill to an installable plugin

Today the skill is repo-level (`.claude/skills/design-graph/`) — it works
for anyone who opens *this repo* on a branch containing it. A plugin makes
it installable anywhere.

1. Create a new repo (e.g. `uno-design-graph-plugin`) with this layout:
   ```
   .claude-plugin/plugin.json        # {"name": "design-graph", "version": "0.5.0", "description": "..."}
   .claude-plugin/marketplace.json   # lists the plugin, so the repo doubles as a marketplace
   skills/design-graph/SKILL.md      # adapted from .claude/skills/design-graph/SKILL.md
   skills/design-graph/schema/       # copy of design-graph-kit/schema/
   skills/design-graph/references/   # copy of references/ (incl. uno-mapping.md)
   skills/design-graph/prompts/      # copy of prompts/
   skills/design-graph/scripts/      # validate_graph.py + score_graph.py
   ```
2. Make the skill **self-contained**: rewrite its paths from
   `design-graph-kit/...` to paths relative to the skill directory (a
   bundled skill can't assume the kit repo exists).
3. Install and test in any project:
   ```
   /plugin marketplace add <owner>/uno-design-graph-plugin
   /plugin install design-graph
   ```
   (Exact flags per the current docs: https://code.claude.com/docs — Plugins.)
4. Interim shortcut, zero packaging: copy `.claude/skills/design-graph/`
   (plus the kit folders it references) into `~/.claude/skills/` on any
   machine for personal-global use.
5. Keep versions in lockstep: plugin `version` ↔ kit `CHANGELOG.md`; when
   the rules change, re-run one blind fleet before releasing.

---

## Re-running any experiment (reference)

```bash
cd design-graph-kit
python -m pip install -r requirements.txt
python scripts/run_all.py                                  # validate all golds
python tools/stability.py blind 07-caffe-main              # fleet analysis
python tools/verify_arm.py experiments/ab-orbital-settings/B2-uno-graph
python scripts/score_graph.py <gold> <generated> --json    # single comparison
```

Blind-fleet protocol (for new evals): 5 agents in fresh contexts, allowed to
read only the target source + the kit method files (never `evals/`,
`experiments/`, `RESULTS.md`, or any `*.graph.json`), each writing one
validated `runN.graph.json`. Score, then treat 5/5 consensus-vs-gold
divergence as an answer-key defect to calibrate explicitly.
