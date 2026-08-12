# Eval 07 Blind Runs — MVVM architecture validation

Five fresh-context blind agents against the Caffe MainPage (CommunityToolkit
MVVM: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand(CanExecute)]`,
`x:Bind`, code-behind Tapped handlers, 9 composed custom UserControls).
All five validated. Scorer 0.3.0, gold v1.0.

## Headline — best cross-run stability of any fleet to date

| Metric | eval 05 (code-behind) | eval 06 (MVUX) | **eval 07 (MVVM)** |
|---|---:|---:|---:|
| mean pairwise macro F1 | 0.56 | 0.60 | **0.73** |
| pairwise node-id F1 range | 0.76–0.90 | 0.60–0.86 | **0.88–0.99** |
| nodes per run | 60 ±1 | 48–55 | **52–56** |
| hallucinations | 0 | 0 | **0** |

Runs 2–5 produced essentially the same graph: identical state sets,
identical behavioral edges, near-identical ids. MVVM's explicit bindings
and commands make evidence extraction almost deterministic under the
v0.4.2 rules. Both real behaviors (card-tap → selection, Brew → brewing)
were found by all five runs; nothing was invented.

## vs gold: 0.37 — but the gap is the answer key's, not the runs'

All five runs diverged from gold **unanimously and identically** in four
places. Five-of-five blind consensus against a same-author gold is, by this
kit's own precedent (v1.4 calibration), evidence against the gold:

1. **Screen slug:** all runs said `caffe-main`; gold said `main`
   (slugified `MainPage`). The Pass-8 grammar doesn't say whether a generic
   page name takes an app-name prefix. The scope slug cascades through
   every id, so this one choice explains most of the id-level gap.
2. **State decomposition:** gold models one screen-level
   `state.main.selected` (the ViewModel's `HasSelection`); all runs
   decomposed it into per-component presentations
   (`espresso-card.selected`, `brew-button.disabled`,
   `selection-overview.hidden`) — a *stricter* application of the
   smallest-changing-node attachment rule. The ontology has no rule for
   **one logical condition manifesting at multiple sites** (condition vs
   presentation). New, well-defined gap.
3. **Trigger attachment:** gold wires `triggers` per instance (4 espresso
   cards); all runs wired it once on the canonical component — they
   generalized the uses-token once-per-concept attachment principle to
   behavioral edges. Defensible; the ontology is silent.
4. **Token breadth (calibration #2):** runs extracted 31–34 tokens vs
   gold's 18 — they included every style the screen's controls consume
   (grind hints, overview/brewing text styles), which is the literal
   reading of the scope rule; gold trimmed aggressively. Same shape as the
   eval-05 v1.4 calibration, now reproduced on a third screen.

## Decisions queued for the next iteration (explicit, per protocol)

- Pass-8 grammar: define the screen slug for generic page names
  (recommend: app-prefix when the page name is generic — matches unanimous
  consensus and avoids `screen.main` collisions in multi-app corpora).
- Ontology: a **condition vs presentation** rule — model the logical
  condition once (screen/ViewModel level) and optionally its per-site
  presentations, or adopt the runs' decomposed form; pick one.
- Ontology: behavioral-edge attachment — triggers from the canonical
  component when every instance triggers identically; per-instance only
  when instances differ.
- Gold v1.1 calibration to the consensus on all four points, then re-score.

## Verdict

**MVVM: validated.** Third architecture, zero hallucinations, the
strongest replication agreement measured in this project. The vs-gold
number is low for a now-familiar reason with a now-familiar remedy: the
blind consensus has out-ruled the answer key again, and the rules it
exposes as ambiguous are queued as explicit decisions.
