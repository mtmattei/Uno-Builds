# Fixture: Composer Shell (source-backed)

The fourth source-backed eval, and the densest design system in the pool:
Composer carries 8 theme dictionaries (`ChipStyles.xaml` alone is 51 KB), a
separate `Typography.xaml` and `Tokens.xaml`, and 11 reusable controls. It is
chosen to stress the two rules that the earlier evals only lightly exercised —
**screen-scoped token extraction** (a whole-dictionary dump here would be
enormous and wrong) and **component reference expansion** (the screen is almost
nothing *but* references).

Architecture: **MVUX** (`ShellModel` feeds), Uno Toolkit, `net10.0-desktop` +
`net10.0-browserwasm`.

## Source (the modeled surface)

- `Composer/src/Composer/Composer/Shell.xaml` — the three-column workspace and
  the two rail storyboards
- `Composer/src/Composer/Composer/Shell.xaml.cs` — rail column toggling
- `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml` — left rail
- `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml` — center canvas
- `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml` — header inside the canvas
- `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml` — right rail
- `Composer/src/Composer/Composer/Themes/Tokens.xaml` — color/spacing/radius tokens
- `Composer/src/Composer/Composer/Themes/Typography.xaml` — type scale

## Scope boundary

The eight views under `Views/Layers/` (IntentCard, DesignTokenGrid,
ScaffoldTerminal, …) are **out of scope**. They are content the center canvas
hosts, not part of the shell surface, and pulling them in would turn one eval
into eight. A graph that models them is over-reaching; a graph that notes the
canvas hosts swappable layer content is correct.

`ChipStyles.xaml`, `PlatformChip.xaml`, `RuntimeChip.xaml`, `Icons.xaml` and
`ContextEngineStyles.xaml` are in the app but are consumed by those layer
views. Tokens should be extracted for what **this** surface actually consumes —
that is the discipline being measured.

## What makes this eval hard

1. **The screen is a composition of references.** `Shell.xaml` contains three
   custom controls and almost no leaf content of its own. A graph that stops at
   the three references has missed the screen entirely — the PageHeader lesson,
   but three times over and one level deeper (`ActiveCanvas` itself contains
   `ActiveLayerHeader`).
2. **Token dictionaries are large enough to punish over-extraction.** Emitting
   every `x:Key` in `Themes/` would produce hundreds of token nodes for a
   surface that consumes a fraction of them.
3. **Real declared state.** Two storyboards (`RailsRevealStoryboard`,
   `RailsHideStoryboard`) with exact timings, driven from code-behind by
   toggling `ColumnDefinition.Width` between 0 and 280. The rails are 0px on
   the first screen and snap open on first lock, which is a genuine screen
   state, not a style-level hover.
4. **Comments state design intent that the markup does not.** `Shell.xaml`
   cites briefs and explains *why* the columns snap rather than animate
   ("Grid columns don't smoothly re-measure under DoubleAnimation on Skia
   desktop"). Rationale is evidence about the design, and worth carrying —
   but it is not a licence to invent behavior the code does not implement.

## Results — 5-run blind fleet + gold, 2026-08-12

Model: Claude Opus 5. Gold authored from source in an isolated context with no
access to the runs; all 77 of its declared identifiers verified verbatim
against source. All five runs validate.

| Run | Nodes | Edges | Unres | macro | node-id | concept | edge | uno |
|---|---|---|---|---|---|---|---|---|
| run1 | 92 | 140 | 7 | 0.330 | 0.526 | 0.390 | 0.252 | 0.431 |
| run2 | 92 | 170 | 7 | 0.267 | 0.379 | 0.474 | 0.163 | 0.345 |
| run3 | 85 | 146 | 7 | 0.321 | 0.492 | 0.426 | 0.247 | 0.453 |
| run4 | 94 | 176 | 7 | 0.330 | 0.521 | 0.438 | 0.238 | 0.440 |
| run5 | 80 | 123 | 8 | 0.288 | 0.393 | 0.393 | 0.175 | 0.507 |
| **gold** | **98** | **186** | **5** | — | — | — | — | — |

Mean vs-gold macro **0.307**; mean pairwise macro **0.525** (min 0.462, max
0.632). Runs agree with each other 1.7× more than with the gold — the same
signature every eval in this kit produces, at the largest scale yet attempted.

### The token rule held under the worst case

This eval exists to stress screen-scoped token extraction, because Composer's
`Themes/` is large enough that a whole-dictionary dump would drown the graph.
All five runs extracted **24–33 tokens** (gold: 24) and several published their
exclusion list unprompted — phase and state tints, the `Space*`/`Corner*`/
`Duration*` scales, the 15-style type ramp. In v0.1 the equivalent failure
produced 37–38 tokens against a gold of 13 on a *much* simpler screen. The
v0.2 token rules are doing their job.

**Component expansion likewise held 5/5.** Every run expanded every custom
control reference, including `ActiveLayerHeader` nested inside `ActiveCanvas`,
and all seven canvas slots in declared order. The PageHeader defect that broke
gold 05 did not recur once, three levels deeper.

### The hallucination proxy fired 5/5 and is wrong again

`severe_hallucination_proxy: true` on every run. Manual inspection of all
trigger edges: **every one corresponds to a real code path.** This is the third
eval where the proxy has flagged real behavior under drifted endpoint ids
(eval 05 blind, eval 07, now here). It should be treated as a known-defective
signal until rewritten, and the kit should stop reporting it as-is.

### A real ontology gap this eval exposed

Gold and runs disagree on trigger *targets* while agreeing on trigger *sources*,
because one action legitimately has several effects:

| Action | Gold records | Runs record | Both true? |
|---|---|---|---|
| layer-row click | `state.composer-shell.rails-open` | `region.active-canvas.canvas-slot` | yes — the jump swaps the canvas *and* opens the rails |
| footer primary | `state.layer-row.locked`, `state.file-row.drafted` | `state.composer-shell.rails-revealed` | yes — `RailsVisible = locked.Count>0 \|\| ActiveIndex>0` |
| locked-card toggle | on the toggle control | on the card canonical | yes — and the runs arguably follow the v0.5 canonical-attachment rule more closely than the gold does |

**The ontology does not say which effect of a multi-effect action to record**,
so two correct graphs disagree and the scorer counts it as error. This is the
sharpest rule gap the kit has surfaced since token scoping, and it is worth a
v0.6 decision.

### Naming conflict: two binding rules contradict each other

The repeated rows split 3–2. Runs 1/3/4 and the gold used
`component.layer-row` / `component.file-row`, from the source-declared
`LayerRow` / `FileRow` projection records, citing Pass 8's "slugify the
source-declared name, never re-synonymize". Runs 2/5 used
`component.layer-item` / `component.file-item`, citing Pass 8's naming
vocabulary entry for list rows (`<content>-item`).

Both cite Pass 8. Pass 8 contradicts itself when a source-declared type name
exists *and* the vocabulary has an entry for that shape. It needs a precedence
ruling, not a preference.
