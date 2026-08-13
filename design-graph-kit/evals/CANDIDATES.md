# Eval candidates — the Uno-Builds app pool

The kit has three source-backed evals: `05-orbital-settings` (code-behind),
`06-flux-profile` (MVUX), `07-caffe-main` (MVVM). This ranks the rest of the
repo as candidates for evals 09+, against two criteria:

1. **carries a real design spec** usable as design-first input;
2. **has a visually rich custom design system** — token dictionaries and
   reusable controls — which is what stresses token scoping, consolidation, and
   the copy-don't-coin uno mapping layer hardest.

Measured across every app in the repo: `.xaml` count, dictionaries under
`Styles/`/`Themes/`, `x:Key` declarations in those dictionaries, reusable
controls under `Controls/`, design/spec/brief documents, and screenshots over
30 KB outside `Assets/`.

## Shortlist

| Rank | App | Res keys | Controls | Style dicts | Design docs | Why it earns a slot |
|---|---|---|---|---|---|---|
| 1 | **Composer** | 287 | 11 | 8 | 9 | The densest design system in the repo paired with the deepest doc set (architecture / design / interaction briefs, several in "detailed" and "from-scratch" variants). Best single stress test for token scoping and canonical-component consolidation. |
| 2 | **Meridian** | 118 | 0 | 5 | 6 | Per-screen design briefs *and* interaction specs (`docs/MainPage/DESIGN-BRIEF.md`, `INTERACTION-SPEC.md`, `docs/StockDetail/…`). Zero custom controls, so it isolates token/state modeling from component consolidation — a useful contrast with Composer. |
| 3 | **Text-Grab** | 149 | 16 | 7 | 1 | The most reusable controls of any app here. Consolidation and `instance-of` discipline are the whole test; 55 XAML files make it the largest surface in the pool. |
| 4 | **QuoteCraft** | 119 | 0 | 3 | 5 | Five briefs including separate desktop and general design briefs, plus a state-labelled screenshot walkthrough (`01-entry`, `02-quote-detail`, `03-quote-editor`) — the closest thing in the repo to per-state design references. |
| 5 | **Gridform** | 47 | 13 | 5 | 4 | `DESIGN-SPEC.md` + `INTERACTION-SPEC.md` + product spec, with 13 controls. Specs are unusually concrete about interaction, which suits a states-heavy eval. |
| 6 | **ClaudeDash** | 341 | 11 | 4 | 0 | Highest raw resource-key count in the repo. No design docs, so source-backed only — but it is the strongest test of screen-scoped token extraction versus whole-dictionary enumeration, the failure the v0.2 token rules exist to prevent. |
| 7 | **SantaTracker** | 309 | 13 | 2 | 0 | Second-densest keys with 13 controls concentrated in only 2 dictionaries — an unusual shape that stresses token scoping differently from ClaudeDash. |
| 8 | **FieldOpsPro** | 152 | 17 | 2 | 0 | Most controls after Text-Grab. Source-backed component-expansion test (the PageHeader lesson at scale). |

## On images

Nearly every image in this repo is an **output screenshot** — `after-anim1.png`,
`fixes-final.png`, walkthrough captures — produced from the implemented app,
not a design handed to an implementer. The genuine design *inputs* here are
written briefs, which is the input shape the design-first pilot already used.

So this pool does not, by itself, supply the image-input round (item B). It
does supply something the kit lacked: apps where a written design spec, the
implementation, and screenshots all coexist, which enables a three-way
comparison on one screen — spec → graph, source → graph, image → graph — with
the same gold. Meridian and QuoteCraft are the best candidates for that.

## Architecture coverage

The three existing evals cover code-behind, MVUX, and MVVM. Nothing in the
shortlist adds a genuinely new *architecture*; the differentiator across this
pool is design-system shape, not state pattern. If architecture coverage
becomes the goal, the gaps worth hunting are C# Markup (no XAML at all),
navigation-heavy region shells, and Skia-drawn surfaces where much of the
"design" is inside `RenderOverride` and invisible to a XAML-only reading.
