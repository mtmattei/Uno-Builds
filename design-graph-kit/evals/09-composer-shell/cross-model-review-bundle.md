# Task: independently review an AI-authored answer key

You are reviewing a **gold graph** - a hand-authored answer key that other AI
runs are scored against. It was authored by a different AI model lineage, and
so was every automated checker applied to it. Your value here is precisely that
you are outside that lineage: you are looking for what it systematically
missed, not confirming what it got right.

## What has already been machine-verified (do not re-do this)

Every identifier the gold quotes from source - resource keys, `x:Name`s, style
keys, class names - has been checked to exist verbatim in the source files
below. The result was **zero fabrications**. Precision is not the question.

## What no checker in that lineage can catch, and what you should hunt

1. **Recall defects - things that should be in the graph and are not.**
   This is the priority. Automated checks validate what *is* present; nothing
   can flag an omission without independently modeling the screen. A previous
   version of this answer key omitted an entire reusable component and its
   search affordance, and every precision check passed it.
2. **Altitude errors** - modeling at the wrong level: layout wrappers promoted
   to meaningful nodes, or meaningful structure flattened away.
3. **Evidence that does not support its claim** - a node whose rationale does
   not follow from the cited file, or an `unresolved` item that is actually
   decidable from the source provided.
4. **Wrong-but-plausible semantics** - a node typed or named in a way that
   reads fine but misrepresents what the source does.

## Required output

For each finding:

- **Severity**: critical (answer key is wrong) / major (defensible but likely
  wrong) / minor (hygiene)
- **Location**: the node or edge id, and the source `file:line` that proves it
- **Claim**: what the gold asserts
- **Reality**: what the source actually shows
- **Fix**: the specific change you would make

Rules for your report:

- Cite `file:line` for every finding. A finding without source evidence is
  noise.
- Do not list what is correct. No summary of strengths. Findings only.
- If you find no defect in a category, say so in one line and move on.
- Flag your own uncertainty explicitly rather than hedging language.

## The open question you must rule on

The kit is undecided about `region` nodes. This gold contains **zero** of them,
while the page it models is built from many nested layout containers and has a
visible multi-column arrangement. Ten independent runs of other screens emitted
6-10 region nodes each, unprompted.

The proposed rule is: *emit a `region` when the source declares a structural
grouping that owns layout or state and is not itself a reusable component; do
not emit one for every layout panel. The test is whether removing the grouping
would change the screen's meaning or only its arrangement.*

**Rule on it.** Should this gold contain region nodes? If yes, which ones, and
what is your reasoning? If no, explain why the arrangement carries no meaning
worth modeling. Answer directly - a hedge here is worth nothing.


---

# The answer key under review: `09-composer-shell`

98 nodes · 186 edges · 5 unresolved items

## What this screen is (the eval's own description)

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


## Rules the gold is supposed to follow — `references/ontology.md`

```markdown
# Design Graph Ontology v0.1

The v0.1 ontology is deliberately small. Expand it only when repeated eval failures demonstrate a missing concept.

## Node types

### `screen`

A top-level user-visible view, page, window, route surface, or major navigable UI.

Examples:
- Settings
- Orders
- Dashboard

Do not create separate screens for loading/error/empty presentations of the same conceptual screen when they are states.

### `region`

A meaningful structural area inside a screen/component.

Examples:
- navigation rail;
- account section;
- toolbar;
- content region;
- footer.

When to emit one (v0.6, from the eval-08 and eval-09 fleets): model a `region`
when the source declares a structural grouping that **owns layout or state**
and is not itself a reusable component — a two-column split, a fixed header
band, a scrolling content area, a bottom navigation bar.

Do **not** emit a region for every layout panel. Most `Grid`/`StackPanel`
elements exist to position things and carry no meaning of their own; a graph
with one region per panel is a XAML transcript, which is precisely what this
IR is not.

The distinguishing question: *if this grouping disappeared, would the screen's
meaning change, or only its arrangement?* A two-column split that pairs ABOUT
with PATHS is meaning. A `Grid` wrapping one label and one value is
arrangement, and belongs in the parent's properties.

Evidence for this rule: all five eval-08 runs and all five eval-09 runs emitted
regions unprompted (6–10 each), while gold 05 has **zero** for a page whose
XAML declares 27 layout containers and whose visible structure is a prominent
two-column split. Ten independent runs reaching for regions where an answer key
has none is a strong signal the answer key is the outlier.

**Gold 05 is therefore queued for revision, not silently revised.** The change
is source-driven (the two-column arrangement is really there), but it alters an
answer key that every eval-05 fleet is scored against, so it belongs to the
independent human review (check 6 in `gold-review.md`) rather than to the agent
lineage that authored the key in the first place.

Do not represent meaningless design-layer groups.

### `component`

A reusable or conceptually reusable UI composition.

Examples:
- metric card;
- profile header;
- product row;
- primary button component.

A component may be source-declared or inferred from repeated structure.

Internals rule (v0.3): a canonical component's internal parts (label slot,
value slot, icon) are recorded once as `properties` on the canonical node
(e.g. `"parts": ["label", "value"]`), never as child nodes repeated under
each instance. Instance nodes carry per-instance data (name, bound value) as
`properties`. A child node under an instance is justified only when that
instance overrides the canonical structure.

### `control`

An interactive UI element.

Examples:
- button;
- textbox;
- checkbox;
- slider;
- tab;
- menu item.

`role` should describe the control class when known.

`semanticRole` can describe purpose when supported, e.g. `primaryAction`, `destructiveAction`, `searchInput`.

### `content`

Non-interactive textual or data content.

Examples:
- title;
- body copy;
- label;
- value;
- helper text.

### `asset`

A visual/media asset.

Examples:
- icon;
- illustration;
- logo;
- avatar image.

### `token`

A reusable design value or rule.

Common `category` values:
- `color`
- `spacing`
- `typography`
- `radius`
- `border`
- `elevation`
- `size`

### `state`

A transient or condition-driven presentation of a screen/component.

Examples:
- loading;
- empty;
- populated;
- error;
- disabled;
- selected;
- entering (an entrance animation the source defines);
- a transient confirmation (e.g. a button label flipping to "Saved!").

Scope rule (v0.2): `state` covers **presentation conditions of the modeled
screen or its components**. Style-level interaction visuals — PointerOver,
Pressed, Focused, a control template's Disabled visual — are design-system
internals and must not appear as `state` nodes in a screen graph. (Blind
evals showed generators reliably over-producing these.)

Attachment rule (v0.2): connect `has-state` from the **smallest node whose
presentation changes**. A button that flips its own label owns that state; a
section that swaps content owns its state; only whole-screen conditions
attach to the screen. The `triggers` edge, when supported, points from the
initiating control to the state or component it produces.

Condition-vs-presentation rule (v0.5, from eval-07 5/5 consensus): when one
logical condition (e.g. a ViewModel's `HasSelection`) drives presentation
changes at **multiple** nodes, create a state per affected node, each named
for its **local presentation** (`state.espresso-card.selected`,
`state.brew-button.disabled`, `state.selection-overview.hidden`); reserve a
single screen-level state for conditions that swap whole-screen presentation
(`state.caffe-main.brewing`). Record the shared driving condition in each
state's `properties.uno.member` so the linkage stays machine-readable.

Trigger attachment rule (v0.5): when every instance of a canonical component
triggers identically, attach the `triggers` edge **once, from the
canonical**; per-instance trigger edges only when instances genuinely
differ. (Mirrors the `uses-token` once-per-concept rule.)

Multi-effect rule (v0.6, from eval-09): when one action has **several declared
effects**, emit **one `triggers` edge per effect**. Do not pick a "main" one.

A layer-row click in Composer both swaps the hosted canvas *and* opens the
rails; the footer's primary button sets the footer previewing *and* locks the
layer *and* marks the file drafted. All of those are in the source. When a
graph records only one, a second correct graph that recorded a different one
reads as disagreement — which is exactly what eval 09's fleet produced, five
runs and the gold each choosing different true targets while the scorer counted
honest modeling as error.

Edge count is not the thing to economize. Emit every declared effect; leave one
out only when the code does not establish it, in which case it belongs in
`unresolved` rather than in a guessed edge.

Corollary for scoring: a behavioral edge whose *source* matches gold but whose
*target* differs is a modeling difference, not invention. `score_graph.py`
0.4.0 reports these as `divergent_behavior_targets` and no longer raises the
hallucination flag for them.

## Relationship types

### `contains`

Structural ownership or semantic containment.

Example:

`screen.settings -> contains -> region.account`

### `instance-of`

An instance belongs to a canonical reusable component.

Example:

`component.metric-card.revenue -> instance-of -> component.metric-card`

### `variant-of`

A stable variant belongs to a canonical component family.

Example:

`component.button.destructive -> variant-of -> component.button`

Use this for design-system variants, not temporary runtime states.

### `uses-token`

A UI concept consumes a design token.

Example:

`component.metric-card -> uses-token -> token.radius.12`

Attachment rule (v0.3): the edge belongs on the **canonical** component (or
the screen for page-level values), once per token per concept. Instances
inherit their canonical's tokens; give an instance its own `uses-token` edge
only when it overrides the canonical value.

### `has-state`

A screen/component has a defined presentation state.

Example:

`screen.orders -> has-state -> state.orders.empty`

### `navigates-to`

A supported interaction navigates to another known screen.

Only assert when source evidence or explicit behavior exists.

### `triggers`

A supported control interaction triggers a known action/concept.

Do not invent command names from button labels.

## Evidence kinds

### `observed`

Directly visible or inspectable.

Examples:
- button label visible in screenshot;
- 16 px gap measured from design structure.

### `declared`

Explicitly named by a source.

Examples:
- Figma component named `PrimaryButton`;
- XAML resource named `PrimaryBrush`.

### `derived`

Computed from source facts without semantic speculation.

Examples:
- repeated exact color value;
- geometric parent-child relationship;
- normalized equivalent color representation.

### `inferred`

Semantic interpretation produced by reasoning.

Examples:
- three visually identical cards likely share one component;
- a visually prominent button appears to be the primary action.

Every inference requires confidence and rationale.

## Partial knowledge

A graph is allowed to be incomplete.

Use `unresolved` for material ambiguity rather than creating fictional behavior.

```

## Rules the gold is supposed to follow — `references/inference-rules.md`

```markdown
# Inference Rules

## Core rule

**Never promote plausibility into fact.**

The Design Graph should be useful to agents while preserving the boundary between source-backed information and inferred interpretation.

## Allowed inference categories

### Semantic role

Allowed when visual hierarchy and context strongly support it.

Example:

A single high-emphasis button at the bottom of an edit form may be inferred as `primaryAction`.

Do not infer the implementation command name.

### Reusable component

Allowed when multiple structures share:
- comparable child structure;
- comparable visual treatment;
- comparable purpose.

Prefer confidence below 1.0 unless the source explicitly declares the component.

### State relationship

Allowed when multiple views clearly depict the same conceptual screen/component in different conditions.

Examples:
- loading spinner vs populated list;
- empty illustration vs populated list.

### Candidate token

Allowed when exact/near-exact values recur enough to suggest a design-system rule.

Do not assign branded semantic names without evidence.

## Prohibited visual-only inference

From screenshots/images alone, do not assert:

- XAML type names;
- C# class names;
- data-binding paths;
- command names;
- API calls;
- route names;
- navigation destinations not visible or described;
- validation logic;
- persistence behavior;
- authorization rules;
- business rules.

Use `unresolved` when one of these is relevant.

## Confidence guidance

Use confidence consistently:

- `1.00` — direct observed/declared fact.
- `0.90–0.99` — extremely strong inference with little plausible alternative.
- `0.75–0.89` — likely inference.
- `0.55–0.74` — plausible but ambiguous; consider `unresolved`.
- `<0.55` — do not assert as graph fact; use `unresolved`.

Observed or declared items generally use `1.0`.

## Consolidation rule

Do not create one canonical component merely because items:
- have the same color;
- have the same rectangle shape;
- occupy similar positions.

Consolidation should reflect shared semantic structure, not visual coincidence.

## State vs variant

Use `state` when the same conceptual thing changes due to transient condition.

Use `variant-of` when separate reusable forms intentionally coexist.

Examples:

`Button / Primary` and `Button / Secondary` -> variants.

`Button / Enabled` and `Button / Disabled` -> state-like presentation; use a state when the distinction is explicitly modeled in the eval.

## Unknown behavior

Example visual:

`[ Edit ]`

Acceptable:

- control role: button;
- text: Edit;
- possible intent in `unresolved`.

Not acceptable:

- `navigates-to -> screen.profile-editor` without evidence.

```

## Rules the gold is supposed to follow — `references/token-rules.md`

```markdown
# Token Extraction and Normalization Rules

## Goal

Capture recurring design rules without overfitting every raw value into a token.

## Candidate categories

- color
- spacing
- typography
- radius
- border
- elevation
- size

## When to create a token

Create a token when at least one is true:

1. The source explicitly declares a token/style/resource.
2. The exact value recurs across semantically related UI.
3. A small cluster of near-equivalent values clearly represents one design rule.
4. The value is important to downstream design consistency.

Do not tokenise every one-off measurement.

## Scope (v0.2 — binding)

A screen graph's tokens are limited to values the modeled surface **actually
consumes**, directly or through a style it uses. Criterion 1 above is
necessary but not sufficient: a declared resource that the modeled screen
never references does not belong in the screen's graph. Do not enumerate
whole style dictionaries or palettes — a complete design-system inventory is
a separate design-system graph. (Blind evals showed generators emitting
~3× the consumed token set by walking the full dictionary.)

## Variant folding (v0.3 — binding)

Interaction-only variants of a base value — hover/pressed shades, alpha
steps of an accent brush, focus tints — are style internals. Tokenize the
**base** value only; the variants live in the design-system layer. Distinct
*emphasis levels the resting surface visibly uses* (label color vs value
color) remain separate tokens.

## Attachment (v0.3 — binding)

`uses-token` attaches to the canonical component, or to the screen for
page-level values — once per token per concept. Never wire token edges
per-instance or per internal part; instances inherit the canonical's tokens.
Attach to an instance only when it **overrides** the canonical value (then
the edge documents exactly that override).

## Naming

Prefer declared semantic names:

`token.color.brand-primary`

If semantics are not known, prefer neutral canonical names:

- `token.color.ff6750a4`
- `token.spacing.16`
- `token.radius.8`
- `token.font-size.14`

Never invent names such as `BrandPrimary` merely because a color looks prominent.

## Value normalization

Normalize equivalent representations before detecting repetition.

Examples:

- `#6750A4`
- `#6750a4`
- `rgb(103, 80, 164)`

may be derived as the same color value.

Do not merge perceptually similar but measurably distinct values unless there is strong evidence they represent accidental drift.

## Spacing

Prefer observed/declared spacing values.

When geometry is estimated from a screenshot, lower confidence accordingly.

Do not infer a full spacing scale from one or two gaps.

## Typography

A typography token may include:
- family;
- size;
- weight;
- line height;
- letter spacing.

Keep incomplete properties if the source does not expose all values.

## Connecting tokens

Use `uses-token`.

Example:

`component.metric-card -> uses-token -> token.radius.12`

For v0.1, `uses-token` may include an edge property identifying what the token controls:

```json
{
  "properties": {
    "appliesTo": "cornerRadius"
  }
}
```

```

## The gold graph

```json
{
  "schemaVersion": "0.1.0",
  "graphId": "eval.composer-shell.gold",
  "name": "Composer - Shell (three-column context-engine workspace)",
  "description": "Gold graph for the Composer Shell: a three-column MVUX workspace (composition stack, active layer canvas, files rail) whose custom controls are expanded two levels deep. Tokens are scoped to the resources this surface consumes; the eight per-layer canvases hosted by ContentControl x:Name=CanvasSlot are out of scope.",
  "sourceSummary": [
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Shell.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Shell.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
    },
    {
      "type": "design-system",
      "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
    },
    {
      "type": "design-system",
      "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
    },
    {
      "type": "design-system",
      "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
    },
    {
      "type": "design-system",
      "path": "Composer/src/Composer/Composer/Themes/ContextEngineStyles.xaml"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Models/Presentation/ShellModel.cs"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
    },
    {
      "type": "csharp",
      "path": "Composer/src/Composer/Composer/Models/LayerDef.cs"
    }
  ],
  "nodes": [
    {
      "id": "screen.composer-shell",
      "type": "screen",
      "name": "Composer shell (three-column workspace)",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        },
        "rationale": "Page x:Class Composer.Shell. DataContext is assigned in OnLoaded to the MVUX-generated ComposerViewModel over ComposerModel, which delegates its shell feeds to ShellModel."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Composer.Shell",
          "property": "RequestedTheme=Light",
          "viewModel": "ComposerViewModel (MVUX bindable over Composer.Models.ComposerModel)"
        },
        "architecture": "MVUX (Uno.Extensions.Reactive) + Uno Toolkit"
      }
    },
    {
      "id": "region.composer-shell.workspace",
      "type": "region",
      "name": "Workspace grid",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        },
        "rationale": "Grid x:Name=WorkspaceRoot with three columns; the two rail columns are declared Width=0 and snapped to 280px from code-behind."
      },
      "properties": {
        "uno": {
          "type": "Grid",
          "xName": "WorkspaceRoot"
        },
        "columns": [
          "LeftRailColumn (0 -> 280)",
          "* (canvas)",
          "RightRailColumn (0 -> 280)"
        ],
        "outerHost": "Grid with utu:SafeArea.Insets=VisibleBounds"
      }
    },
    {
      "id": "region.composer-shell.left-rail",
      "type": "region",
      "name": "Left rail container",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        },
        "rationale": "Border x:Name=LeftRailContainer in column 0, Opacity=0 with a TranslateTransform x:Name=LeftRailTransform X=-40; animated by the rail storyboards."
      },
      "properties": {
        "uno": {
          "type": "Border",
          "xName": "LeftRailContainer",
          "transform": "TranslateTransform x:Name=LeftRailTransform X=-40"
        },
        "restingOpacity": 0
      }
    },
    {
      "id": "region.composer-shell.right-rail",
      "type": "region",
      "name": "Right rail container",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        },
        "rationale": "Border x:Name=RightRailContainer in column 2, Opacity=0 with a TranslateTransform x:Name=RightRailTransform X=40."
      },
      "properties": {
        "uno": {
          "type": "Border",
          "xName": "RightRailContainer",
          "transform": "TranslateTransform x:Name=RightRailTransform X=40"
        },
        "restingOpacity": 0
      }
    },
    {
      "id": "component.composition-stack",
      "type": "component",
      "name": "Composition stack (left rail)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.CompositionStack: Paper2 panel with a right hairline edge, 20,28 padding, vertical AutoLayout Spacing=14."
      },
      "role": "navigationRail",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.CompositionStack"
        },
        "parts": [
          "eyebrow",
          "caption",
          "divider",
          "layer-rows"
        ],
        "widthWhenOpen": 280
      }
    },
    {
      "id": "content.stack.eyebrow",
      "type": "content",
      "name": "COMPOSITION STACK",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      },
      "text": "COMPOSITION STACK",
      "semanticRole": "sectionHeader",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "MonoEyebrow"
        }
      }
    },
    {
      "id": "content.stack.caption",
      "type": "content",
      "name": "Stack caption",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      },
      "text": "A conversation that crystallizes into a build system.",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "fontResourceKey": "SerifLightItalicFontFamily"
        }
      }
    },
    {
      "id": "control.stack.layer-rows",
      "type": "control",
      "name": "Layer row list",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        },
        "rationale": "ItemsRepeater x:Name=LayerRows with a vertical StackLayout (Spacing=2). Render() assigns ItemsSource to an ImmutableArray<LayerRow> projected from Layers.All on every ActiveIndex / LayerStates change."
      },
      "role": "itemsRepeater",
      "properties": {
        "uno": {
          "type": "ItemsRepeater",
          "xName": "LayerRows",
          "property": "ItemsSource=\"{Binding Layers}\" (overwritten in code-behind)",
          "member": "LayerRow.For(def, isActive, isLocked, dimmed)"
        }
      }
    },
    {
      "id": "component.layer-row",
      "type": "component",
      "name": "Layer row",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        },
        "rationale": "ItemTemplate row: a full-width Button (StackRowButtonStyle) over a 3-column Grid \u2014 2px state border, index + uppercase label + italic hint, trailing glyph. Eight rows, one per LayerDef in Layers.All."
      },
      "role": "navigationRow",
      "properties": {
        "uno": {
          "type": "Button",
          "styleKey": "StackRowButtonStyle",
          "property": "Tag={Binding}, Click=OnLayerRowClick",
          "member": "Composer.Views.Controls.LayerRow"
        },
        "parts": [
          "state-border",
          "index-label",
          "label",
          "hint",
          "glyph"
        ],
        "instanceCount": 8,
        "layers": [
          "Intent",
          "UX",
          "Architecture",
          "Design System",
          "Interactions",
          "Data",
          "Implementation",
          "Scaffold"
        ],
        "layerLabelSource": "LayerDef.Label uppercased; IndexLabel is (Index + 1) formatted D2",
        "bindings": [
          "LeftBorderBrush",
          "Opacity",
          "IndexLabel",
          "LabelUpper",
          "Hint",
          "Glyph"
        ]
      }
    },
    {
      "id": "component.files-rail",
      "type": "component",
      "name": "Files rail (right rail)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.FilesRail: Paper2 panel with a left hairline edge, 20,28 padding, vertical AutoLayout Spacing=14."
      },
      "role": "statusRail",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.FilesRail"
        },
        "parts": [
          "eyebrow",
          "caption",
          "divider",
          "file-rows",
          "divider",
          "locked-summary"
        ],
        "widthWhenOpen": 280
      }
    },
    {
      "id": "content.files.eyebrow",
      "type": "content",
      "name": "FILES RAIL",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      },
      "text": "FILES RAIL",
      "semanticRole": "sectionHeader",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "MonoEyebrow"
        }
      }
    },
    {
      "id": "content.files.caption",
      "type": "content",
      "name": "Files caption",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      },
      "text": "Each layer emits files as it locks.",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "fontResourceKey": "SerifLightItalicFontFamily"
        }
      }
    },
    {
      "id": "control.files.file-rows",
      "type": "control",
      "name": "File row list",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        },
        "rationale": "ItemsRepeater x:Name=FileRows with a vertical StackLayout (Spacing=2); no ItemsSource in XAML. Render() builds the rows from Layers.All plus the two synthesized companion files and assigns them on every FileStatuses change."
      },
      "role": "itemsRepeater",
      "properties": {
        "uno": {
          "type": "ItemsRepeater",
          "xName": "FileRows",
          "member": "FileRow.For(fileName, status)"
        }
      }
    },
    {
      "id": "component.file-row",
      "type": "component",
      "name": "File row",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        },
        "rationale": "ItemTemplate row: 8px status Ellipse, mono file name, mono status badge; Opacity bound per status."
      },
      "role": "statusRow",
      "properties": {
        "uno": {
          "type": "Grid",
          "member": "Composer.Views.Controls.FileRow"
        },
        "parts": [
          "status-dot",
          "file-name",
          "status-badge"
        ],
        "instanceCount": 10,
        "files": [
          "README.md",
          "ux-flows.md",
          "architecture.md",
          "design-system.md",
          "interaction-spec.md",
          "data-contracts.md",
          "implementation-plan.md",
          "scaffold.command",
          "README.md",
          "prompt-context.md"
        ],
        "bindings": [
          "DotFill",
          "DotStroke",
          "FileName",
          "StatusBadge",
          "StatusBadgeBrush",
          "Opacity"
        ]
      }
    },
    {
      "id": "content.files.locked-summary",
      "type": "content",
      "name": "Locked counter",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        },
        "rationale": "TextBlock x:Name=LockedSummary; XAML seeds \"0 OF 8 LOCKED\" and Render() rewrites it as \"{drafted} OF {Layers.All.Length} LOCKED\"."
      },
      "text": "0 OF 8 LOCKED",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "LockedSummary",
          "member": "FileStatuses (count of FileStatus.Drafted)"
        }
      }
    },
    {
      "id": "component.active-canvas",
      "type": "component",
      "name": "Active canvas (center column)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.ActiveCanvas, Grid.Column=1, x:Name=CenterCanvas in Shell.xaml. Hosts the canonical page template: progress, title row, header, locked stack, canvas slot, footer, future stack."
      },
      "role": "contentColumn",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.ActiveCanvas",
          "xName": "CenterCanvas"
        }
      }
    },
    {
      "id": "region.canvas.column",
      "type": "region",
      "name": "Canvas column",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        },
        "rationale": "ScrollViewer x:Name=CanvasScrollHost (Padding 32,28,32,40, vertical scroll only) wrapping utu:AutoLayout x:Name=CanvasColumn, Spacing=20, MinWidth=640 MaxWidth=720, centered."
      },
      "properties": {
        "uno": {
          "type": "utu:AutoLayout",
          "xName": "CanvasColumn",
          "host": "ScrollViewer x:Name=CanvasScrollHost"
        },
        "spacing": 20,
        "minWidth": 640,
        "maxWidth": 720
      }
    },
    {
      "id": "component.progress-indicator",
      "type": "component",
      "name": "Progress indicator",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.ProgressIndicator, x:Name=ProgressRegion; three DPs bound OneWay to ProgressFraction / ProgressLabel / ProgressCounter."
      },
      "role": "progress",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.ProgressIndicator",
          "xName": "ProgressRegion",
          "member": "Fraction={Binding ProgressFraction}, Label={Binding ProgressLabel}, Counter={Binding ProgressCounter}"
        },
        "parts": [
          "track",
          "label",
          "counter"
        ],
        "motion": "Fill width animated 480ms QuinticEase EaseOut, re-measured on track SizeChanged"
      }
    },
    {
      "id": "asset.progress.track",
      "type": "asset",
      "name": "Progress hairline",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        },
        "rationale": "Grid x:Name=TrackRoot: 1px hairline Border stretched full width with a 1px amber Border x:Name=ProgressFill left-aligned at fraction * ActualWidth."
      },
      "role": "progressTrack",
      "properties": {
        "uno": {
          "type": "Grid",
          "xName": "TrackRoot",
          "fill": "Border x:Name=ProgressFill"
        },
        "height": 1
      }
    },
    {
      "id": "content.progress.label",
      "type": "content",
      "name": "Progress label",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml.cs"
        },
        "rationale": "TextBlock x:Name=LabelText, set from the Label DP (ShellModel.ProgressLabel: the active layer's uppercase name)."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "LabelText",
          "member": "ProgressLabel"
        }
      }
    },
    {
      "id": "content.progress.counter",
      "type": "content",
      "name": "Progress counter",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml.cs"
        },
        "rationale": "TextBlock x:Name=CounterText, set from the Counter DP (ShellModel.ProgressCounter: \"01 / 08\" style counter)."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "CounterText",
          "member": "ProgressCounter"
        }
      }
    },
    {
      "id": "component.app-title-row",
      "type": "component",
      "name": "App title row",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.AppTitleRow, x:Name=TitleRow, Visibility=Collapsed in ActiveCanvas.xaml; ProjectName and ShowReset stay bound."
      },
      "role": "pageHeader",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.AppTitleRow",
          "xName": "TitleRow",
          "member": "ProjectName={Binding ProjectName}, ShowReset={Binding HasLockedLayers}"
        },
        "visibility": "Collapsed",
        "parts": [
          "project-name",
          "reset-link"
        ]
      }
    },
    {
      "id": "content.title-row.project-name",
      "type": "content",
      "name": "Project name",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml.cs"
        },
        "rationale": "TextBlock x:Name=ProjectNameText, 18px Medium sans; set from the ProjectName DP (ComposerModel.ProjectName = Intent.AppType) with an \"Untitled\" fallback."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "ProjectNameText",
          "fontResourceKey": "SansFontFamily",
          "member": "ProjectName"
        },
        "fallbackText": "Untitled"
      }
    },
    {
      "id": "control.title-row.reset",
      "type": "control",
      "name": "Reset",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml.cs"
        },
        "rationale": "Button x:Name=ResetButton, LinkButtonStyle, Content=\"Reset\", AutomationProperties.Name=\"Reset composition\"; visible only when ShowReset (HasLockedLayers) is true. OnResetClick invokes the Reset command on the page DataContext."
      },
      "role": "button",
      "semanticRole": "destructiveAction",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "ResetButton",
          "styleKey": "LinkButtonStyle",
          "member": "MvuxCommandInvoker.Invoke(dc, \"Reset\")"
        },
        "visibility": "Collapsed until HasLockedLayers"
      }
    },
    {
      "id": "component.active-layer-header",
      "type": "component",
      "name": "Active layer header",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.ActiveLayerHeader, x:Name=HeaderRegion; six DPs bound OneWay to ShellModel's ActiveLayer* feeds. Vertical AutoLayout Spacing=6."
      },
      "role": "sectionHeader",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.ActiveLayerHeader",
          "xName": "HeaderRegion",
          "member": "LayerIndex, LayerLabel, LayerState, Recap, Title, Subtitle"
        },
        "parts": [
          "recap-row",
          "state-badge"
        ],
        "collapsedParts": [
          "LayerLabelText",
          "TitleText",
          "SubtitleText"
        ]
      }
    },
    {
      "id": "content.header.recap",
      "type": "content",
      "name": "Layer recap",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        },
        "rationale": "utu:AutoLayout x:Name=RecapRow: a mono arrow glyph plus TextBlock x:Name=RecapText (italic, MaxWidth=560, LineHeight=20) set from the Recap DP."
      },
      "semanticRole": "recap",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "RecapText",
          "member": "ActiveLayerRecap"
        },
        "prefixGlyph": "\u21b3",
        "maxWidth": 560,
        "lineHeight": 20
      }
    },
    {
      "id": "content.header.state-badge",
      "type": "content",
      "name": "Layer state badge",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        },
        "rationale": "TextBlock x:Name=StateBadgeText, mono eyebrow size, CharacterSpacing=160, amber; text is \"EDITED - PREVIEW PENDING\" (Dirty) or \"PREVIEW - REVIEW AND ACCEPT\" (Previewing), collapsed otherwise."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "StateBadgeText",
          "member": "ActiveLayerLayerState"
        }
      }
    },
    {
      "id": "region.canvas.locked-stack",
      "type": "region",
      "name": "Locked context stack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        },
        "rationale": "utu:AutoLayout x:Name=LockedStack, Spacing=10, above the canvas slot. SyncLockedStack() fills it with one LockedContextCard per layer whose LayerState is Locked, in Layers.All order."
      },
      "properties": {
        "uno": {
          "type": "utu:AutoLayout",
          "xName": "LockedStack",
          "member": "LayerStates / DefaultExpandedKinds"
        },
        "spacing": 10
      }
    },
    {
      "id": "component.locked-context-card",
      "type": "component",
      "name": "Locked context card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.LockedContextCard: Paper2 Border, 1px hairline, CornerRadius=4, 20,18 padding. Header row (expand toggle, check + label + LOCKED, Revisit), italic summary, hairline divider, four-fact grid."
      },
      "role": "card",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.LockedContextCard",
          "member": "LayerKind, Summary, Facts, IsExpanded"
        },
        "parts": [
          "expand-toggle",
          "header",
          "revisit",
          "summary",
          "divider",
          "facts-grid"
        ],
        "factsGrid": "2 columns (120px label / * value) x 4 rows",
        "instantiatedBy": "ActiveCanvas.SyncLockedStack",
        "defaultExpanded": "the two most recently locked layers (ShellModel.DefaultExpandedKinds)"
      }
    },
    {
      "id": "content.locked-card.header",
      "type": "content",
      "name": "Locked card header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        },
        "rationale": "TextBlock of three Runs: a check mark, Run x:Name=HeaderLabelRun (the uppercase layer label), and a literal \"  \u00b7  LOCKED\"."
      },
      "text": "\u2713 {LAYER}  \u00b7  LOCKED",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "HeaderLabelRun"
        }
      }
    },
    {
      "id": "content.locked-card.summary",
      "type": "content",
      "name": "Locked card summary",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "TextBlock x:Name=SummaryText, italic, set from the Summary DP; ActiveCanvas.LockedSummaryFor derives the sentence per layer kind (falls back to LayerDef.Hint)."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "SummaryText",
          "fontResourceKey": "SerifLightItalicFontFamily",
          "member": "Summary"
        }
      }
    },
    {
      "id": "component.info-row",
      "type": "component",
      "name": "Fact row",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "Up to four label/value pairs added to Grid x:Name=FactsGrid in Render(): uppercase mono 10px label in column 0, mono 12px value in column 1."
      },
      "role": "fact",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "member": "IList<KeyValuePair<string,string>> Facts"
        },
        "parts": [
          "label",
          "value"
        ],
        "instanceCount": 4
      }
    },
    {
      "id": "control.canvas.slot",
      "type": "control",
      "name": "Layer canvas slot",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "ContentControl x:Name=CanvasSlot, MinHeight=480, stretched. SyncSlot() reads the active LayerKind and assigns the matching per-layer UserControl via CreateCanvas, passing the shell DataContext down. Region-based navigation was reverted to direct hosting."
      },
      "role": "contentHost",
      "semanticRole": "swappableContent",
      "properties": {
        "uno": {
          "type": "ContentControl",
          "xName": "CanvasSlot",
          "member": "ActiveIndex -> Layers.All[i].Kind -> CreateCanvas(kind)"
        },
        "minHeight": 480,
        "hosts": [
          "IntentCard",
          "UXFlowStrip",
          "ArchitectureBlueprint",
          "DesignTokenGrid",
          "InteractionsStateTimeline",
          "DataContractGrid",
          "ImplementationPhaseGrid",
          "ScaffoldTerminal"
        ],
        "scopeNote": "The hosted per-layer canvases are out of scope for this graph."
      }
    },
    {
      "id": "component.composer-footer",
      "type": "component",
      "name": "Composer footer",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.ComposerFooter, x:Name=FooterRegion: Paper2 Border, 1px hairline, CornerRadius=6, 20,18 padding, vertical AutoLayout Spacing=14. Two DPs (State, Prompt) pushed from ActiveCanvas.SyncFooter."
      },
      "role": "card",
      "semanticRole": "promptComposer",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.ComposerFooter",
          "xName": "FooterRegion",
          "member": "State (LayerState), Prompt (string)"
        },
        "parts": [
          "eyebrow",
          "lead-question",
          "ack-line",
          "prompt-input",
          "suggestions-row",
          "action-row"
        ]
      }
    },
    {
      "id": "content.footer.eyebrow",
      "type": "content",
      "name": "Composer status eyebrow",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "TextBlock x:Name=EyebrowText, MonoEyebrow style; ApplyState writes \"COMPOSER \u00b7 {ComposerStatus.ForLayerState(State)}\" - REFINING / LISTENING / PROPOSING."
      },
      "text": "COMPOSER \u00b7 REFINING",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "EyebrowText",
          "styleKey": "MonoEyebrow",
          "member": "ComposerStatus.ForLayerState"
        }
      }
    },
    {
      "id": "content.footer.lead-question",
      "type": "content",
      "name": "Lead question",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "TextBlock x:Name=LeadQuestionText, sans body, LineHeight=22; SyncLeadQuestion reads ActiveLeadQuestion off the model and collapses the row when empty."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "LeadQuestionText",
          "member": "ActiveLeadQuestion"
        }
      }
    },
    {
      "id": "content.footer.ack",
      "type": "content",
      "name": "Preview acknowledgment",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "Border x:Name=AckLine (2px amber left rule, 12,2 padding) wrapping TextBlock x:Name=AckText; ApplyAck writes \"You asked: ... - here's what changes if I apply that.\" from PreviewAcks[kind]. Visible only in the Previewing state and only when the ack is non-empty."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "AckText",
          "member": "PreviewAcks"
        },
        "visibility": "Collapsed unless Previewing"
      }
    },
    {
      "id": "control.footer.prompt-input",
      "type": "control",
      "name": "Prompt textarea",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "TextBox x:Name=PromptInput, ChatInputTextBoxStyle, PlaceholderText=\"Refine, or accept what's drawn\u2026\", AcceptsReturn, MinHeight=68, AutomationProperties.Name=\"Composer prompt\". TextChanged invokes SetActivePrompt; Ctrl+Enter runs the primary action; Esc discards a preview."
      },
      "role": "textbox",
      "semanticRole": "promptInput",
      "properties": {
        "uno": {
          "type": "TextBox",
          "xName": "PromptInput",
          "styleKey": "ChatInputTextBoxStyle",
          "member": "SetActivePrompt / DiscardPreview"
        },
        "placeholderText": "Refine, or accept what's drawn\u2026",
        "minHeight": 68,
        "keyboard": [
          "Ctrl+Enter -> primary action",
          "Esc (Previewing) -> DiscardPreview"
        ]
      }
    },
    {
      "id": "region.footer.suggestions",
      "type": "region",
      "name": "Suggestion row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "Three-column Grid below the textarea: TRY label, utu:AutoLayout x:Name=ChipsRow (Spacing=8), keyboard hint."
      },
      "properties": {
        "uno": {
          "type": "utu:AutoLayout",
          "xName": "ChipsRow"
        }
      }
    },
    {
      "id": "content.footer.try-label",
      "type": "content",
      "name": "TRY",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      },
      "text": "TRY",
      "semanticRole": "label",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "fontResourceKey": "MonoFontFamily"
        }
      }
    },
    {
      "id": "component.suggestion-chip",
      "type": "component",
      "name": "Suggestion chip",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "RenderChips() builds one Button per string in ComposerModel.SuggestionChips[activeKind] - transparent fill, hairline border, CornerRadius=4, 10,5 padding, mono 11px Medium. Click sets the textarea text, moves the caret to the end, focuses, and invokes SetActivePrompt."
      },
      "role": "button",
      "semanticRole": "promptSuggestion",
      "properties": {
        "uno": {
          "type": "Button",
          "member": "ComposerModel.SuggestionChips"
        },
        "instanceCount": 3,
        "instanceNote": "three chips per layer, swapped when the active layer changes",
        "exampleValues": [
          "Mobile-first",
          "Offline-first",
          "No backend yet"
        ]
      }
    },
    {
      "id": "content.footer.kbd-hint",
      "type": "content",
      "name": "Submit hint",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      },
      "text": "\u2318  \u21b5  TO SUBMIT",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "KbdHint"
        }
      }
    },
    {
      "id": "region.footer.actions",
      "type": "region",
      "name": "Action row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "Four-column Grid: primary button, italic hint, spacer, right-aligned discard links (AutoLayout Spacing=14)."
      }
    },
    {
      "id": "control.footer.primary",
      "type": "control",
      "name": "Primary action",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "Button x:Name=PrimaryButton, InkButtonStyle by default, AutomationProperties.Name=\"Lock and continue\". ApplyState rewrites Content and Style per LayerState; TriggerPrimary maps the resolved state to LockAndContinue / GeneratePreview / AcceptAndLock."
      },
      "role": "button",
      "semanticRole": "primaryAction",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "PrimaryButton",
          "styleKey": "InkButtonStyle",
          "member": "LockAndContinue | GeneratePreview | AcceptAndLock"
        },
        "labels": {
          "Clean": "Lock and continue \u2192 (Continue \u2192 on Intent)",
          "Dirty": "Generate preview \u2192",
          "Previewing": "Accept and lock \u2192"
        }
      }
    },
    {
      "id": "content.footer.primary-hint",
      "type": "content",
      "name": "Primary action hint",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "TextBlock x:Name=PrimaryHintText, italic; ApplyState writes \"accepting the recommendation\" (Clean) / \"with your edits\" (Dirty) / \"the AI's pass\" (Previewing). Mirrors ShellModel.ActivePrimaryHint."
      },
      "text": "accepting the recommendation",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "PrimaryHintText"
        }
      }
    },
    {
      "id": "control.footer.discard-edits",
      "type": "control",
      "name": "Discard edits",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "Button x:Name=DiscardEditsButton, LinkButtonStyle, Content=\"Discard edits\", collapsed by default; visible only in the Dirty state. Invokes DiscardEdits and clears the textarea."
      },
      "role": "button",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "DiscardEditsButton",
          "styleKey": "LinkButtonStyle",
          "member": "DiscardEdits"
        }
      }
    },
    {
      "id": "control.footer.discard-preview",
      "type": "control",
      "name": "Discard preview",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        },
        "rationale": "Button x:Name=DiscardPreviewButton, LinkButtonStyle, Content=\"\u2190 Discard preview\", collapsed by default; visible only in the Previewing state. Invokes DiscardPreview."
      },
      "role": "button",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "DiscardPreviewButton",
          "styleKey": "LinkButtonStyle",
          "member": "DiscardPreview"
        }
      }
    },
    {
      "id": "region.canvas.future-stack",
      "type": "region",
      "name": "Future preview stack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        },
        "rationale": "utu:AutoLayout x:Name=FutureStack, Spacing=8, below the footer. SyncFutureStack() adds one FuturePreviewCard per layer after the active one at opacity max(0.05, 0.40 - distance*0.08)."
      },
      "properties": {
        "uno": {
          "type": "utu:AutoLayout",
          "xName": "FutureStack",
          "member": "ActiveIndex / RailsVisible"
        },
        "spacing": 8,
        "opacityRule": "max(0.05, 0.40 - distance * 0.08)"
      }
    },
    {
      "id": "component.future-preview-card",
      "type": "component",
      "name": "Future preview card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        },
        "rationale": "UserControl Composer.Views.Controls.FuturePreviewCard, IsHitTestVisible=False: dashed hairline outline over 20,14 padding with a mono header and italic hint."
      },
      "role": "card",
      "semanticRole": "upcomingLayer",
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Composer.Views.Controls.FuturePreviewCard",
          "member": "LayerLabel, Hint"
        },
        "parts": [
          "outline",
          "header",
          "hint"
        ],
        "instantiatedBy": "ActiveCanvas.SyncFutureStack",
        "hitTestVisible": false
      }
    },
    {
      "id": "asset.future-card.outline",
      "type": "asset",
      "name": "Dashed card outline",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        },
        "rationale": "Rectangle with hairline Stroke, StrokeThickness=1, StrokeDashArray=3,3, RadiusX/Y=4, Fill=Transparent - laid under the content because Border.BorderBrush has no dash support."
      },
      "role": "decoration",
      "properties": {
        "uno": {
          "type": "Rectangle",
          "property": "StrokeDashArray=3,3"
        }
      }
    },
    {
      "id": "content.future-card.header",
      "type": "content",
      "name": "Future card header",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml.cs"
        },
        "rationale": "TextBlock x:Name=HeaderText; Render() writes \"{LayerLabel}  \u00b7  UPCOMING\"."
      },
      "text": "{LAYER}  \u00b7  UPCOMING",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "HeaderText"
        }
      }
    },
    {
      "id": "content.future-card.hint",
      "type": "content",
      "name": "Future card hint",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml.cs"
        },
        "rationale": "TextBlock x:Name=HintText, italic; set from the Hint DP (LayerDef.Hint)."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "xName": "HintText",
          "member": "LayerDef.Hint"
        }
      }
    },
    {
      "id": "state.composer-shell.rails-hidden",
      "type": "state",
      "name": "Rails hidden (focused first)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Shell.xaml.cs"
        },
        "rationale": "Initial condition: ShellModel.RailsVisible is false while no layer is locked and ActiveIndex is 0. Both rail columns are Width=0, both containers Opacity=0, translated -40 / +40."
      },
      "semanticRole": "focusMode",
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "ShellModel.RailsVisible",
          "storyboardKey": "RailsHideStoryboard"
        },
        "columnWidth": 0,
        "motion": "X to -40 / +40 over 320ms QuinticEase EaseOut; Opacity to 0 over 240ms",
        "reducedMotion": "MotionPreferences.AnimationsEnabled false -> snap without storyboard"
      }
    },
    {
      "id": "state.composer-shell.rails-open",
      "type": "state",
      "name": "Rails open",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Shell.xaml.cs"
        },
        "rationale": "SyncRailsAnimation snaps both ColumnDefinition.Width to 280px (Grid columns don't re-measure smoothly under DoubleAnimation on Skia desktop) and runs RailsRevealStoryboard."
      },
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "ShellModel.RailsVisible",
          "storyboardKey": "RailsRevealStoryboard"
        },
        "columnWidth": 280,
        "motion": "X to 0 over 480ms QuinticEase EaseOut; Opacity to 1 over 320ms after a 160ms delay",
        "trigger": "first lock, or any advance past layer 0"
      }
    },
    {
      "id": "state.active-canvas.focused-first",
      "type": "state",
      "name": "Canvas focused-first dimensions",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncFocusedFirst: when RailsVisible is false the canvas column is MinWidth 640 / MaxWidth 720 and the scroll host's top padding is 64."
      },
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "RailsVisible / IsFocusedFirst"
        },
        "minWidth": 640,
        "maxWidth": 720,
        "padding": "32,64,32,40"
      }
    },
    {
      "id": "state.active-canvas.rails-open",
      "type": "state",
      "name": "Canvas rails-open dimensions",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncFocusedFirst: when RailsVisible is true the column animates to MinWidth 820 / MaxWidth 880 over 480ms QuinticEase EaseOut and the top padding snaps to 28 (padding doesn't animate cleanly on Skia)."
      },
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "RailsVisible / IsFocusedFirst"
        },
        "minWidth": 820,
        "maxWidth": 880,
        "padding": "32,28,32,40",
        "motion": "480ms QuinticEase EaseOut on MinWidth and MaxWidth (pooled Storyboard)"
      }
    },
    {
      "id": "state.future-stack.empty",
      "type": "state",
      "name": "Future stack empty",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncFutureStack clears the stack and returns early while RailsVisible is false, so no future cards render on the first screen."
      },
      "semanticRole": "empty",
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "RailsVisible"
        }
      }
    },
    {
      "id": "state.future-card.entering",
      "type": "state",
      "name": "Future card entrance",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "Each card is added at Opacity 0 and animated to its target opacity over 480ms QuinticEase EaseOut, matching the rail-reveal rhythm; with animations disabled it is added at the target opacity."
      },
      "semanticRole": "entering",
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "MotionPreferences.AnimationsEnabled"
        },
        "motion": "Opacity 0 -> max(0.05, 0.40 - distance*0.08) over 480ms QuinticEase EaseOut"
      }
    },
    {
      "id": "state.composer-footer.clean",
      "type": "state",
      "name": "Clean (REFINING)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "LayerState.Clean: eyebrow REFINING, primary \"Lock and continue \u2192\" (\"Continue \u2192\" on the Intent layer) with InkButtonStyle, both discard links and the ack line collapsed. LayerState.Locked renders the same arm."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ComposerFooter.State"
        },
        "appliesTo": [
          "LayerState.Clean",
          "LayerState.Locked"
        ],
        "eyebrow": "COMPOSER \u00b7 REFINING",
        "primaryHint": "accepting the recommendation"
      }
    },
    {
      "id": "state.composer-footer.dirty",
      "type": "state",
      "name": "Dirty (LISTENING)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "LayerState.Dirty: eyebrow LISTENING, primary \"Generate preview \u2192\" restyled to AmberButtonStyle, Discard edits visible, ack line collapsed."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ComposerFooter.State",
          "styleKey": "AmberButtonStyle"
        },
        "eyebrow": "COMPOSER \u00b7 LISTENING",
        "primaryHint": "with your edits"
      }
    },
    {
      "id": "state.composer-footer.previewing",
      "type": "state",
      "name": "Previewing (PROPOSING)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "LayerState.Previewing: eyebrow PROPOSING, primary \"Accept and lock \u2192\" with InkButtonStyle, Discard preview visible, and the amber-ruled acknowledgment line shown when PreviewAcks holds a quote for the layer."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ComposerFooter.State"
        },
        "eyebrow": "COMPOSER \u00b7 PROPOSING",
        "primaryHint": "the AI's pass"
      }
    },
    {
      "id": "state.composer-footer.hidden",
      "type": "state",
      "name": "Footer hidden on Scaffold",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncFooter collapses the whole footer when the active layer kind is Scaffold - that canvas owns its own terminal Download-bundle action, so the standard Continue affordance would lead nowhere."
      },
      "semanticRole": "hidden",
      "properties": {
        "uno": {
          "mechanism": "code-behind",
          "member": "Layers.All[ActiveIndex].Kind == LayerKind.Scaffold"
        }
      }
    },
    {
      "id": "state.layer-header.edited",
      "type": "state",
      "name": "Edited badge",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        },
        "rationale": "LayerState.Dirty renders the amber badge \"EDITED - PREVIEW PENDING\"."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ActiveLayerHeader.LayerState"
        },
        "text": "EDITED \u2014 PREVIEW PENDING"
      }
    },
    {
      "id": "state.layer-header.preview",
      "type": "state",
      "name": "Preview badge",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        },
        "rationale": "LayerState.Previewing renders the amber badge \"PREVIEW - REVIEW AND ACCEPT\"."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ActiveLayerHeader.LayerState"
        },
        "text": "PREVIEW \u2014 REVIEW AND ACCEPT"
      }
    },
    {
      "id": "state.layer-header.no-recap",
      "type": "state",
      "name": "Recap row collapsed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        },
        "rationale": "The recap row collapses when the Recap DP is null or whitespace - the cold-launch Intent layer has no prior decision to recap."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "ActiveLayerHeader.Recap"
        }
      }
    },
    {
      "id": "state.layer-row.active",
      "type": "state",
      "name": "Row active",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        },
        "rationale": "LayerRow.For(isActive): amber 2px left border, full opacity."
      },
      "semanticRole": "selected",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "ActiveIndex"
        },
        "opacity": 1.0
      }
    },
    {
      "id": "state.layer-row.locked",
      "type": "state",
      "name": "Row locked",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        },
        "rationale": "LayerRow.For(isLocked): Ink2 2px left border and a check-mark glyph in the trailing column."
      },
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "LayerStates[kind] == LayerState.Locked"
        },
        "glyph": "\u2713"
      }
    },
    {
      "id": "state.layer-row.upcoming",
      "type": "state",
      "name": "Row upcoming",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        },
        "rationale": "LayerRow.For(dimmed): rows after the active index that are neither active nor locked get a transparent left border and 0.42 opacity."
      },
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "index > ActiveIndex"
        },
        "opacity": 0.42
      }
    },
    {
      "id": "state.file-row.planned",
      "type": "state",
      "name": "File planned",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        },
        "rationale": "FileRow.For(FileStatus.Planned): hollow Ink4-stroked dot, PLANNED badge in Ink4, 0.42 opacity."
      },
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "FileStatuses[kind]"
        },
        "badge": "PLANNED",
        "opacity": 0.42
      }
    },
    {
      "id": "state.file-row.writing",
      "type": "state",
      "name": "File writing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        },
        "rationale": "FileRow.For(FileStatus.Writing): indigo dot and WRITING badge for the currently active layer, full opacity."
      },
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "FileStatuses[kind]"
        },
        "badge": "WRITING",
        "opacity": 1.0
      }
    },
    {
      "id": "state.file-row.drafted",
      "type": "state",
      "name": "File drafted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        },
        "rationale": "FileRow.For(FileStatus.Drafted): amber dot and DRAFTED badge once the layer is locked, full opacity."
      },
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "FileStatuses[kind]"
        },
        "badge": "DRAFTED",
        "opacity": 1.0
      }
    },
    {
      "id": "state.locked-card.collapsed",
      "type": "state",
      "name": "Card collapsed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "ApplyExpansion hides the summary, divider and facts grid and flips the toggle glyph to +. Cards default to expanded only for the two most recently locked layers."
      },
      "properties": {
        "uno": {
          "mechanism": "dependency-property",
          "member": "LockedContextCard.IsExpanded"
        },
        "toggleGlyph": {
          "expanded": "\u2212",
          "collapsed": "+"
        }
      }
    },
    {
      "id": "token.color.glass-backdrop",
      "type": "token",
      "name": "Glass backdrop (page background)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#FFE3ECF7 -> #FFF1F4F9 -> #FFFAF8F1",
      "properties": {
        "uno": {
          "resourceKey": "GlassBackdropBrush",
          "resourceType": "LinearGradientBrush"
        }
      }
    },
    {
      "id": "token.color.paper",
      "type": "token",
      "name": "Paper (on-ink foreground)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#FFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "PaperBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.paper2",
      "type": "token",
      "name": "Paper 2 (panel surface)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#FAFAFA",
      "properties": {
        "uno": {
          "resourceKey": "Paper2Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.ink",
      "type": "token",
      "name": "Ink (primary text)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#1A1A1A",
      "properties": {
        "uno": {
          "resourceKey": "InkBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.ink2",
      "type": "token",
      "name": "Ink 2 (secondary text)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#3A3A3A",
      "properties": {
        "uno": {
          "resourceKey": "Ink2Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.ink3",
      "type": "token",
      "name": "Ink 3 (tertiary text)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#737373",
      "properties": {
        "uno": {
          "resourceKey": "Ink3Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.ink4",
      "type": "token",
      "name": "Ink 4 (lowest-emphasis text)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#A3A3A3",
      "properties": {
        "uno": {
          "resourceKey": "Ink4Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.hairline",
      "type": "token",
      "name": "Hairline (borders and rules)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#ECECEC",
      "properties": {
        "uno": {
          "resourceKey": "HairlineBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.amber",
      "type": "token",
      "name": "Amber (active / locked accent)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#C89C3F",
      "properties": {
        "uno": {
          "resourceKey": "AmberBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.indigo",
      "type": "token",
      "name": "Indigo (in-progress accent)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "#3D3DFF",
      "properties": {
        "uno": {
          "resourceKey": "IndigoBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.transparent",
      "type": "token",
      "name": "Transparent",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Tokens.xaml"
        },
        "rationale": "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."
      },
      "category": "color",
      "value": "Transparent",
      "properties": {
        "uno": {
          "resourceKey": "TransparentBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.typography.mono",
      "type": "token",
      "name": "Mono face (JetBrains Mono)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/JetBrains_Mono/JetBrainsMono-VariableFont.ttf#JetBrains Mono, Cascadia Mono, Consolas, monospace",
      "properties": {
        "uno": {
          "resourceKey": "MonoFontFamily",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.typography.sans",
      "type": "token",
      "name": "Sans face (Inter)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif",
      "properties": {
        "uno": {
          "resourceKey": "SansFontFamily",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.typography.serif",
      "type": "token",
      "name": "Prompt input face",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif",
      "properties": {
        "uno": {
          "resourceKey": "SerifFontFamily",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.typography.serif-italic",
      "type": "token",
      "name": "Italic voice face",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif",
      "properties": {
        "uno": {
          "resourceKey": "SerifItalicFontFamily",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.typography.serif-light-italic",
      "type": "token",
      "name": "Light italic voice face",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif",
      "properties": {
        "uno": {
          "resourceKey": "SerifLightItalicFontFamily",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.typography.mono-eyebrow",
      "type": "token",
      "name": "Mono eyebrow style",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": {
        "family": "MonoFontFamily",
        "size": 11,
        "weight": "Medium",
        "tracking": 180,
        "lineHeight": 13,
        "foreground": "Ink3Brush",
        "numerals": "Tabular"
      },
      "properties": {
        "uno": {
          "styleKey": "MonoEyebrow",
          "resourceType": "Style(TextBlock)"
        }
      }
    },
    {
      "id": "token.font-size.eyebrow-micro",
      "type": "token",
      "name": "Eyebrow micro size",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": 10,
      "properties": {
        "uno": {
          "resourceKey": "TypeEyebrowMicroSize",
          "resourceType": "x:Double"
        }
      }
    },
    {
      "id": "token.font-size.eyebrow",
      "type": "token",
      "name": "Eyebrow size",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": 11,
      "properties": {
        "uno": {
          "resourceKey": "TypeEyebrowSize",
          "resourceType": "x:Double"
        }
      }
    },
    {
      "id": "token.font-size.label",
      "type": "token",
      "name": "Label size",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": 12,
      "properties": {
        "uno": {
          "resourceKey": "TypeLabelSize",
          "resourceType": "x:Double"
        }
      }
    },
    {
      "id": "token.font-size.body-small",
      "type": "token",
      "name": "Body small size",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": 14,
      "properties": {
        "uno": {
          "resourceKey": "TypeBodySmallSize",
          "resourceType": "x:Double"
        }
      }
    },
    {
      "id": "token.font-size.body",
      "type": "token",
      "name": "Body size",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/Typography.xaml"
        },
        "rationale": "Declared in Themes/Typography.xaml and consumed by this surface."
      },
      "category": "typography",
      "value": 16,
      "properties": {
        "uno": {
          "resourceKey": "TypeBodySize",
          "resourceType": "x:Double"
        }
      }
    },
    {
      "id": "token.spacing.14",
      "type": "token",
      "name": "Panel rhythm (14)",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        },
        "rationale": "utu:AutoLayout Spacing=14 recurs on both rails and the composer footer - the panel-level vertical rhythm. The Spacing14/Space14 resources exist in Tokens.xaml but this surface writes the literal."
      },
      "category": "spacing",
      "value": 14,
      "properties": {
        "unit": "px"
      }
    },
    {
      "id": "token.radius.4",
      "type": "token",
      "name": "Small radius (4)",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        },
        "rationale": "CornerRadius=4 on the locked context card, RadiusX/Y=4 on the future preview card outline, and CornerRadius=4 on the code-built suggestion chips. Written as literals, not via the declared Corner3/Corner6/CornerPill resources."
      },
      "category": "radius",
      "value": 4,
      "properties": {
        "unit": "px"
      }
    },
    {
      "id": "control.locked-card.expand-toggle",
      "type": "control",
      "name": "Expand toggle",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        },
        "rationale": "Button x:Name=ExpandToggle, LinkButtonStyle, Content is the minus/plus glyph; OnExpandToggleClick flips IsExpanded locally."
      },
      "role": "button",
      "semanticRole": "disclosure",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "ExpandToggle",
          "styleKey": "LinkButtonStyle",
          "member": "LockedContextCard.IsExpanded"
        }
      }
    },
    {
      "id": "control.locked-card.revisit",
      "type": "control",
      "name": "Revisit",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "Button x:Name=RevisitButton, LinkButtonStyle, Content=\"Revisit\"; OnRevisitClick invokes Revisit(LayerKind), which jumps ActiveIndex to that layer and resets its state to Clean (non-destructive)."
      },
      "role": "button",
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "RevisitButton",
          "styleKey": "LinkButtonStyle",
          "member": "MvuxCommandInvoker.Invoke(dc, \"Revisit\", LayerKind)"
        }
      }
    }
  ],
  "edges": [
    {
      "from": "screen.composer-shell",
      "relation": "contains",
      "to": "region.composer-shell.workspace",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "region.composer-shell.workspace",
      "relation": "contains",
      "to": "region.composer-shell.left-rail",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "region.composer-shell.workspace",
      "relation": "contains",
      "to": "component.active-canvas",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "region.composer-shell.workspace",
      "relation": "contains",
      "to": "region.composer-shell.right-rail",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "region.composer-shell.left-rail",
      "relation": "contains",
      "to": "component.composition-stack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "region.composer-shell.right-rail",
      "relation": "contains",
      "to": "component.files-rail",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "contains",
      "to": "content.stack.eyebrow",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "contains",
      "to": "content.stack.caption",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "contains",
      "to": "control.stack.layer-rows",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "control.stack.layer-rows",
      "relation": "contains",
      "to": "component.layer-row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "contains",
      "to": "content.files.eyebrow",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "contains",
      "to": "content.files.caption",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "contains",
      "to": "control.files.file-rows",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "control.files.file-rows",
      "relation": "contains",
      "to": "component.file-row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "contains",
      "to": "content.files.locked-summary",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.active-canvas",
      "relation": "contains",
      "to": "region.canvas.column",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "component.progress-indicator",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "component.app-title-row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "component.active-layer-header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "region.canvas.locked-stack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "control.canvas.slot",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "component.composer-footer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "region.canvas.column",
      "relation": "contains",
      "to": "region.canvas.future-stack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml"
        }
      }
    },
    {
      "from": "component.progress-indicator",
      "relation": "contains",
      "to": "asset.progress.track",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "component.progress-indicator",
      "relation": "contains",
      "to": "content.progress.label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "component.progress-indicator",
      "relation": "contains",
      "to": "content.progress.counter",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "component.app-title-row",
      "relation": "contains",
      "to": "content.title-row.project-name",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml"
        }
      }
    },
    {
      "from": "component.app-title-row",
      "relation": "contains",
      "to": "control.title-row.reset",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml"
        }
      }
    },
    {
      "from": "component.active-layer-header",
      "relation": "contains",
      "to": "content.header.recap",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "component.active-layer-header",
      "relation": "contains",
      "to": "content.header.state-badge",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "region.canvas.locked-stack",
      "relation": "contains",
      "to": "component.locked-context-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncLockedStack instantiates one card per locked layer."
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "contains",
      "to": "control.locked-card.expand-toggle",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "contains",
      "to": "content.locked-card.header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "contains",
      "to": "control.locked-card.revisit",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "contains",
      "to": "content.locked-card.summary",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "contains",
      "to": "component.info-row",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "Render() adds up to four label/value pairs into FactsGrid."
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "content.footer.eyebrow",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "content.footer.lead-question",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "content.footer.ack",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "control.footer.prompt-input",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "region.footer.suggestions",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "contains",
      "to": "region.footer.actions",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.suggestions",
      "relation": "contains",
      "to": "content.footer.try-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.suggestions",
      "relation": "contains",
      "to": "component.suggestion-chip",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "RenderChips fills ChipsRow with one Button per suggestion."
      }
    },
    {
      "from": "region.footer.suggestions",
      "relation": "contains",
      "to": "content.footer.kbd-hint",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.actions",
      "relation": "contains",
      "to": "control.footer.primary",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.actions",
      "relation": "contains",
      "to": "content.footer.primary-hint",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.actions",
      "relation": "contains",
      "to": "control.footer.discard-edits",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.footer.actions",
      "relation": "contains",
      "to": "control.footer.discard-preview",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "region.canvas.future-stack",
      "relation": "contains",
      "to": "component.future-preview-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        },
        "rationale": "SyncFutureStack instantiates one card per upcoming layer."
      }
    },
    {
      "from": "component.future-preview-card",
      "relation": "contains",
      "to": "asset.future-card.outline",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "component.future-preview-card",
      "relation": "contains",
      "to": "content.future-card.header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "component.future-preview-card",
      "relation": "contains",
      "to": "content.future-card.hint",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "screen.composer-shell",
      "relation": "has-state",
      "to": "state.composer-shell.rails-hidden",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Shell.xaml.cs"
        },
        "rationale": "Initial presentation - the rails start at zero width."
      }
    },
    {
      "from": "screen.composer-shell",
      "relation": "has-state",
      "to": "state.composer-shell.rails-open",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Shell.xaml.cs"
        }
      }
    },
    {
      "from": "component.active-canvas",
      "relation": "has-state",
      "to": "state.active-canvas.focused-first",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        }
      }
    },
    {
      "from": "component.active-canvas",
      "relation": "has-state",
      "to": "state.active-canvas.rails-open",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        }
      }
    },
    {
      "from": "region.canvas.future-stack",
      "relation": "has-state",
      "to": "state.future-stack.empty",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        }
      }
    },
    {
      "from": "component.future-preview-card",
      "relation": "has-state",
      "to": "state.future-card.entering",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "has-state",
      "to": "state.composer-footer.clean",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "has-state",
      "to": "state.composer-footer.dirty",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "has-state",
      "to": "state.composer-footer.previewing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "has-state",
      "to": "state.composer-footer.hidden",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs"
        }
      }
    },
    {
      "from": "component.active-layer-header",
      "relation": "has-state",
      "to": "state.layer-header.edited",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        }
      }
    },
    {
      "from": "component.active-layer-header",
      "relation": "has-state",
      "to": "state.layer-header.preview",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        }
      }
    },
    {
      "from": "component.active-layer-header",
      "relation": "has-state",
      "to": "state.layer-header.no-recap",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "has-state",
      "to": "state.layer-row.active",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "has-state",
      "to": "state.layer-row.locked",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "has-state",
      "to": "state.layer-row.upcoming",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "has-state",
      "to": "state.file-row.planned",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "has-state",
      "to": "state.file-row.writing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "has-state",
      "to": "state.file-row.drafted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "has-state",
      "to": "state.locked-card.collapsed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "triggers",
      "to": "state.composer-shell.rails-open",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        },
        "rationale": "OnLayerRowClick invokes Jump(row.Index) on the page DataContext; ShellModel.RailsVisible is lockedLayers.Count > 0 || ActiveIndex > 0, so jumping to any layer past the first opens the rails. Attached once at the canonical - every row behaves identically."
      }
    },
    {
      "from": "control.title-row.reset",
      "relation": "triggers",
      "to": "state.composer-shell.rails-hidden",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "Reset() sets every layer back to Clean and ActiveIndex to 0, which makes RailsVisible false and runs RailsHideStoryboard."
      }
    },
    {
      "from": "control.footer.prompt-input",
      "relation": "triggers",
      "to": "state.composer-footer.dirty",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "TextChanged invokes SetActivePrompt, which marks the active layer Dirty once the text is non-empty."
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "triggers",
      "to": "state.composer-footer.dirty",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        },
        "rationale": "Chip click writes the chip text into the textarea and invokes SetActivePrompt -> MarkDirty. Attached once at the canonical - all chips behave identically."
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "triggers",
      "to": "state.composer-footer.previewing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "In the Dirty state TriggerPrimary invokes GeneratePreview, which stages the proposal and sets the layer to Previewing."
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "triggers",
      "to": "state.layer-row.locked",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "LockAndContinue / AcceptAndLock set the layer to LayerState.Locked and advance ActiveIndex; the composition stack re-renders that row with the check glyph and the Ink2 rule."
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "triggers",
      "to": "state.file-row.drafted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/Presentation/ShellModel.cs"
        },
        "rationale": "The same lock flips FileStatuses for the layer to Drafted (ShellModel derives it from LockedLayers), which the files rail renders as an amber dot and DRAFTED badge."
      }
    },
    {
      "from": "control.footer.discard-edits",
      "relation": "triggers",
      "to": "state.composer-footer.clean",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "DiscardEdits restores the pre-edit snapshot, clears the prompt and sets the layer Clean."
      }
    },
    {
      "from": "control.footer.discard-preview",
      "relation": "triggers",
      "to": "state.composer-footer.clean",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Models/ComposerModel.cs"
        },
        "rationale": "DiscardPreview restores the snapshot, clears the prompt and sets the layer Clean."
      }
    },
    {
      "from": "control.locked-card.expand-toggle",
      "relation": "triggers",
      "to": "state.locked-card.collapsed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        },
        "rationale": "OnExpandToggleClick inverts IsExpanded, which drives ApplyExpansion."
      }
    },
    {
      "from": "screen.composer-shell",
      "relation": "uses-token",
      "to": "token.color.glass-backdrop",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Shell.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "uses-token",
      "to": "token.color.paper2",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.composition-stack",
      "relation": "uses-token",
      "to": "token.spacing.14",
      "properties": {
        "appliesTo": "spacing"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "uses-token",
      "to": "token.color.paper2",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.files-rail",
      "relation": "uses-token",
      "to": "token.spacing.14",
      "properties": {
        "appliesTo": "spacing"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.stack.eyebrow",
      "relation": "uses-token",
      "to": "token.typography.mono-eyebrow",
      "properties": {
        "appliesTo": "textStyle"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "content.files.eyebrow",
      "relation": "uses-token",
      "to": "token.typography.mono-eyebrow",
      "properties": {
        "appliesTo": "textStyle"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.stack.caption",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "content.stack.caption",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "content.stack.caption",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "content.files.caption",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.files.caption",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.files.caption",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.files.locked-summary",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.files.locked-summary",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "content.files.locked-summary",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "indexAndLabelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "hintFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "indexAndLabelSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.font-size.label",
      "properties": {
        "appliesTo": "hintAndGlyphSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "labelForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "component.layer-row",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "indexAndHintForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml"
        }
      }
    },
    {
      "from": "state.layer-row.active",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "leftBorderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        }
      }
    },
    {
      "from": "state.layer-row.locked",
      "relation": "uses-token",
      "to": "token.color.ink2",
      "properties": {
        "appliesTo": "leftBorderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "uses-token",
      "to": "token.font-size.label",
      "properties": {
        "appliesTo": "fileNameSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow-micro",
      "properties": {
        "appliesTo": "badgeSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "component.file-row",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "fileNameForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml"
        }
      }
    },
    {
      "from": "state.file-row.planned",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "dotStrokeAndBadge"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "state.file-row.writing",
      "relation": "uses-token",
      "to": "token.color.indigo",
      "properties": {
        "appliesTo": "dotAndBadge"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "state.file-row.drafted",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "dotAndBadge"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs"
        }
      }
    },
    {
      "from": "asset.progress.track",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "trackBackground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "asset.progress.track",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "fillBackground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "content.progress.label",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "content.progress.label",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "content.progress.label",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "content.progress.counter",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml"
        }
      }
    },
    {
      "from": "content.title-row.project-name",
      "relation": "uses-token",
      "to": "token.typography.sans",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml"
        }
      }
    },
    {
      "from": "content.title-row.project-name",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml"
        }
      }
    },
    {
      "from": "content.header.recap",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.recap",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.recap",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.recap",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "arrowGlyphForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.state-badge",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.state-badge",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "content.header.state-badge",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "uses-token",
      "to": "token.color.paper2",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.composer-footer",
      "relation": "uses-token",
      "to": "token.spacing.14",
      "properties": {
        "appliesTo": "spacing"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.eyebrow",
      "relation": "uses-token",
      "to": "token.typography.mono-eyebrow",
      "properties": {
        "appliesTo": "textStyle"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.lead-question",
      "relation": "uses-token",
      "to": "token.typography.sans",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.lead-question",
      "relation": "uses-token",
      "to": "token.font-size.body",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.lead-question",
      "relation": "uses-token",
      "to": "token.color.ink2",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.ack",
      "relation": "uses-token",
      "to": "token.typography.serif-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.ack",
      "relation": "uses-token",
      "to": "token.color.ink2",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.ack",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "leftRuleBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "control.footer.prompt-input",
      "relation": "uses-token",
      "to": "token.typography.serif",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.prompt-input",
      "relation": "uses-token",
      "to": "token.font-size.body",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.prompt-input",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.prompt-input",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "placeholderForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "content.footer.try-label",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.try-label",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.try-label",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "uses-token",
      "to": "token.color.transparent",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "uses-token",
      "to": "token.color.ink2",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "component.suggestion-chip",
      "relation": "uses-token",
      "to": "token.radius.4",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs"
        }
      }
    },
    {
      "from": "content.footer.kbd-hint",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow-micro",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.kbd-hint",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "uses-token",
      "to": "token.color.paper",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "control.footer.primary",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow-micro",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ChipStyles.xaml"
        }
      }
    },
    {
      "from": "state.composer-footer.dirty",
      "relation": "uses-token",
      "to": "token.color.amber",
      "properties": {
        "appliesTo": "primaryButtonBackground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Themes/ContextEngineStyles.xaml"
        }
      }
    },
    {
      "from": "content.footer.primary-hint",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.primary-hint",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "content.footer.primary-hint",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "uses-token",
      "to": "token.color.paper2",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.locked-context-card",
      "relation": "uses-token",
      "to": "token.radius.4",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.header",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.header",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.header",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.header",
      "relation": "uses-token",
      "to": "token.color.ink2",
      "properties": {
        "appliesTo": "checkGlyphForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.header",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "lockedSuffixForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.summary",
      "relation": "uses-token",
      "to": "token.typography.serif-light-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.summary",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "content.locked-card.summary",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        }
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "labelForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        }
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.color.ink",
      "properties": {
        "appliesTo": "valueForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs"
        }
      }
    },
    {
      "from": "control.locked-card.expand-toggle",
      "relation": "uses-token",
      "to": "token.color.ink4",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml"
        }
      }
    },
    {
      "from": "asset.future-card.outline",
      "relation": "uses-token",
      "to": "token.color.hairline",
      "properties": {
        "appliesTo": "stroke"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "asset.future-card.outline",
      "relation": "uses-token",
      "to": "token.radius.4",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.header",
      "relation": "uses-token",
      "to": "token.typography.mono",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.header",
      "relation": "uses-token",
      "to": "token.font-size.eyebrow",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.header",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.hint",
      "relation": "uses-token",
      "to": "token.typography.serif-italic",
      "properties": {
        "appliesTo": "fontFamily"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.hint",
      "relation": "uses-token",
      "to": "token.font-size.body-small",
      "properties": {
        "appliesTo": "fontSize"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    },
    {
      "from": "content.future-card.hint",
      "relation": "uses-token",
      "to": "token.color.ink3",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml"
        }
      }
    }
  ],
  "unresolved": [
    {
      "id": "unresolved.stack.items-source",
      "question": "Is CompositionStack's ItemsSource=\"{Binding Layers}\" live, or vestigial?",
      "relatedIds": [
        "control.stack.layer-rows",
        "component.layer-row"
      ],
      "reason": "The XAML binds ItemsSource to a member named Layers, but neither ComposerModel nor ShellModel exposes one (Composer.Models.Layers is a static class), and CompositionStack.Render() assigns the LayerRow projection to the repeater on every state change. The rows shown are the code-behind projection; whether the binding is leftover or an intended future shape is not decidable from source. FilesRail declares no ItemsSource at all, which suggests the binding is leftover."
    },
    {
      "id": "unresolved.rail-panel-shared",
      "question": "Are CompositionStack and FilesRail two controls or one panel component with two variants?",
      "relatedIds": [
        "component.composition-stack",
        "component.files-rail"
      ],
      "possibleValues": [
        "two independent UserControls",
        "one canonical rail panel with mirrored edge"
      ],
      "reason": "Both are Paper2 borders with a single hairline edge (mirrored left/right), 20,28 padding, a spacing-14 vertical AutoLayout, a MonoEyebrow title, an italic caption, a 1px divider and an ItemsRepeater. The structure is identical, but they are separately declared UserControls with different content, so the gold does not fold them into one canonical."
    },
    {
      "id": "unresolved.canvas-slot-navigation",
      "question": "Will the center column become a navigation region, and does that change the graph's shape?",
      "relatedIds": [
        "control.canvas.slot",
        "component.active-canvas",
        "screen.composer-shell"
      ],
      "reason": "Shell.xaml and ActiveCanvas.xaml.cs both state that the center column is intended to be a uen:Region.Attached navigation surface and that the per-layer Pages plus RouteMap remain registered as scaffolding; the M3b attempt was reverted because pages did not mount inside the nested region. The graph models the implemented direct hosting (SyncSlot assigns a UserControl to CanvasSlot.Content). If navigation lands, the eight layer canvases become separate screens rather than swapped content."
    },
    {
      "id": "unresolved.hidden-canvas-slots",
      "question": "Are the collapsed title row and header title/subtitle removed for good?",
      "relatedIds": [
        "component.app-title-row",
        "component.active-layer-header",
        "content.title-row.project-name",
        "control.title-row.reset"
      ],
      "reason": "AppTitleRow is Visibility=Collapsed in ActiveCanvas.xaml with a comment saying the title moved inside the glass container and the Reset affordance 'will resurface elsewhere'; ActiveLayerHeader keeps LayerLabelText, TitleText and SubtitleText collapsed 'so existing code-behind references compile'. All of them are still bound and still updated at runtime, so nothing in the source decides whether they are deprecated or temporarily parked. Their text is deliberately not modeled as visible content."
    },
    {
      "id": "unresolved.files-rail-row-count",
      "question": "Is the duplicated README.md row in the files rail intentional?",
      "relatedIds": [
        "component.file-row",
        "control.files.file-rows",
        "content.files.locked-summary"
      ],
      "reason": "FilesRail.Render() emits Layers.All (whose Intent layer's File is README.md) and then appends Layers.ReadmeFileName (README.md again) plus prompt-context.md, producing ten rows with README.md twice; the class comment describes nine files. The locked counter meanwhile counts only Drafted per-layer files against Layers.All.Length, so it reads '0 OF 8' while ten rows render."
    }
  ],
  "metadata": {
    "goldVersion": "1.0",
    "altitude": "kit v0.4 screen semantics; states are screen/component presentation conditions only",
    "scope": "Shell + its ten referenced controls; Views/Layers/* excluded (hosted by control.canvas.slot)",
    "tokenRule": "declared resources this surface consumes (21) + 2 derived recurring literals; interaction-only shades folded out"
  }
}
```

## Source files the gold cites

Line-numbered so you can cite `file:line`.

### `Composer/src/Composer/Composer/Shell.xaml`

```xml
    1  ﻿<Page x:Class="Composer.Shell"
    2        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4        xmlns:local="using:Composer"
    5        xmlns:views="using:Composer.Views"
    6        xmlns:controls="using:Composer.Views.Controls"
    7        xmlns:utu="using:Uno.Toolkit.UI"
    8        Background="{ThemeResource GlassBackdropBrush}"
    9        RequestedTheme="Light">
   10  
   11      <Page.Resources>
   12          <!-- Brief 03 §2 — rail reveal/hide. Column widths snap (Grid columns
   13               don't smoothly re-measure under DoubleAnimation on Skia desktop)
   14               but the rail content slides in from -40/+40 px and fades up
   15               over 320ms with a 160ms delay so the rails read as "opening". -->
   16          <Storyboard x:Key="RailsRevealStoryboard">
   17              <DoubleAnimation Storyboard.TargetName="LeftRailTransform"
   18                               Storyboard.TargetProperty="X"
   19                               To="0"
   20                               Duration="0:0:0.48">
   21                  <DoubleAnimation.EasingFunction>
   22                      <QuinticEase EasingMode="EaseOut" />
   23                  </DoubleAnimation.EasingFunction>
   24              </DoubleAnimation>
   25              <DoubleAnimation Storyboard.TargetName="LeftRailContainer"
   26                               Storyboard.TargetProperty="Opacity"
   27                               To="1"
   28                               BeginTime="0:0:0.16"
   29                               Duration="0:0:0.32" />
   30              <DoubleAnimation Storyboard.TargetName="RightRailTransform"
   31                               Storyboard.TargetProperty="X"
   32                               To="0"
   33                               Duration="0:0:0.48">
   34                  <DoubleAnimation.EasingFunction>
   35                      <QuinticEase EasingMode="EaseOut" />
   36                  </DoubleAnimation.EasingFunction>
   37              </DoubleAnimation>
   38              <DoubleAnimation Storyboard.TargetName="RightRailContainer"
   39                               Storyboard.TargetProperty="Opacity"
   40                               To="1"
   41                               BeginTime="0:0:0.16"
   42                               Duration="0:0:0.32" />
   43          </Storyboard>
   44  
   45          <Storyboard x:Key="RailsHideStoryboard">
   46              <DoubleAnimation Storyboard.TargetName="LeftRailTransform"
   47                               Storyboard.TargetProperty="X"
   48                               To="-40"
   49                               Duration="0:0:0.32">
   50                  <DoubleAnimation.EasingFunction>
   51                      <QuinticEase EasingMode="EaseOut" />
   52                  </DoubleAnimation.EasingFunction>
   53              </DoubleAnimation>
   54              <DoubleAnimation Storyboard.TargetName="LeftRailContainer"
   55                               Storyboard.TargetProperty="Opacity"
   56                               To="0"
   57                               Duration="0:0:0.24" />
   58              <DoubleAnimation Storyboard.TargetName="RightRailTransform"
   59                               Storyboard.TargetProperty="X"
   60                               To="40"
   61                               Duration="0:0:0.32">
   62                  <DoubleAnimation.EasingFunction>
   63                      <QuinticEase EasingMode="EaseOut" />
   64                  </DoubleAnimation.EasingFunction>
   65              </DoubleAnimation>
   66              <DoubleAnimation Storyboard.TargetName="RightRailContainer"
   67                               Storyboard.TargetProperty="Opacity"
   68                               To="0"
   69                               Duration="0:0:0.24" />
   70          </Storyboard>
   71      </Page.Resources>
   72  
   73      <Grid HorizontalAlignment="Stretch"
   74            VerticalAlignment="Stretch"
   75            utu:SafeArea.Insets="VisibleBounds">
   76  
   77          <!-- Workspace shell. Rails are 0px wide on the very first screen
   78               (Stack canvas, ActiveIndex=0, no locks); on first lock the
   79               columns snap to 280px and the rail content slides+fades in.
   80               Code-behind toggles ColumnDefinition.Width to keep first-screen
   81               focus mode honest. Per ENGINEERING-BRIEF-page-and-flow-breakdown
   82               §2.1 the center column will become a uen:Region.Attached
   83               navigation surface; this Shell is the pre-decomposition
   84               single-page host that still embeds ActiveCanvas inline. -->
   85          <Grid x:Name="WorkspaceRoot">
   86              <Grid.ColumnDefinitions>
   87                  <ColumnDefinition x:Name="LeftRailColumn"  Width="0" />
   88                  <ColumnDefinition Width="*" />
   89                  <ColumnDefinition x:Name="RightRailColumn" Width="0" />
   90              </Grid.ColumnDefinitions>
   91  
   92              <Border Grid.Column="0"
   93                      x:Name="LeftRailContainer"
   94                      Opacity="0">
   95                  <Border.RenderTransform>
   96                      <TranslateTransform x:Name="LeftRailTransform" X="-40" />
   97                  </Border.RenderTransform>
   98                  <controls:CompositionStack />
   99              </Border>
  100  
  101              <!-- M6: ActiveCanvas no longer takes a Header DP. The inner
  102                   ActiveLayerHeader binds its six discrete DPs directly to
  103                   ShellModel's ActiveLayer* feeds. -->
  104              <controls:ActiveCanvas Grid.Column="1"
  105                                     x:Name="CenterCanvas" />
  106  
  107              <Border Grid.Column="2"
  108                      x:Name="RightRailContainer"
  109                      Opacity="0">
  110                  <Border.RenderTransform>
  111                      <TranslateTransform x:Name="RightRailTransform" X="40" />
  112                  </Border.RenderTransform>
  113                  <controls:FilesRail />
  114              </Border>
  115          </Grid>
  116      </Grid>
  117  </Page>
```

### `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml`

```xml
    1  ﻿<UserControl x:Class="Composer.Views.Controls.CompositionStack"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI"
    5               xmlns:models="using:Composer.Models">
    6      <Border Background="{ThemeResource Paper2Brush}"
    7              BorderBrush="{ThemeResource HairlineBrush}"
    8              BorderThickness="0,0,1,0"
    9              Padding="20,28">
   10          <utu:AutoLayout Orientation="Vertical" Spacing="14">
   11              <TextBlock Style="{StaticResource MonoEyebrow}"
   12                         Text="COMPOSITION STACK" />
   13              <TextBlock FontFamily="{StaticResource SerifLightItalicFontFamily}"
   14                         FontSize="{StaticResource TypeBodySmallSize}"
   15                         Foreground="{ThemeResource Ink3Brush}"
   16                         TextWrapping="Wrap"
   17                         Text="A conversation that crystallizes into a build system." />
   18  
   19              <Border Height="1"
   20                      Background="{ThemeResource HairlineBrush}"
   21                      Margin="0,4,0,4" />
   22  
   23              <ItemsRepeater x:Name="LayerRows"
   24                             ItemsSource="{Binding Layers}">
   25                  <ItemsRepeater.Layout>
   26                      <StackLayout Orientation="Vertical" Spacing="2" />
   27                  </ItemsRepeater.Layout>
   28                  <ItemsRepeater.ItemTemplate>
   29                      <DataTemplate>
   30                          <Button Style="{StaticResource StackRowButtonStyle}"
   31                                  HorizontalAlignment="Stretch"
   32                                  HorizontalContentAlignment="Stretch"
   33                                  Tag="{Binding}"
   34                                  Click="OnLayerRowClick">
   35                              <Grid>
   36                                  <Grid.ColumnDefinitions>
   37                                      <ColumnDefinition Width="Auto" />
   38                                      <ColumnDefinition Width="*" />
   39                                      <ColumnDefinition Width="Auto" />
   40                                  </Grid.ColumnDefinitions>
   41  
   42                                  <Border Grid.Column="0"
   43                                          Width="2"
   44                                          Background="{Binding LeftBorderBrush}"
   45                                          VerticalAlignment="Stretch"
   46                                          Margin="0,2,10,2" />
   47  
   48                                  <utu:AutoLayout Grid.Column="1"
   49                                                  Orientation="Vertical"
   50                                                  Spacing="2"
   51                                                  Padding="0,8,0,8"
   52                                                  Opacity="{Binding Opacity}">
   53                                      <utu:AutoLayout Orientation="Horizontal" Spacing="8">
   54                                          <TextBlock FontFamily="{StaticResource MonoFontFamily}"
   55                                                     FontSize="{StaticResource TypeEyebrowSize}"
   56                                                     CharacterSpacing="160"
   57                                                     Foreground="{ThemeResource Ink3Brush}"
   58                                                     Text="{Binding IndexLabel}" />
   59                                          <TextBlock FontFamily="{StaticResource MonoFontFamily}"
   60                                                     FontSize="{StaticResource TypeEyebrowSize}"
   61                                                     CharacterSpacing="160"
   62                                                     FontWeight="SemiBold"
   63                                                     Foreground="{ThemeResource InkBrush}"
   64                                                     Text="{Binding LabelUpper}" />
   65                                      </utu:AutoLayout>
   66                                      <TextBlock FontFamily="{StaticResource SerifLightItalicFontFamily}"
   67                                                 FontSize="{StaticResource TypeLabelSize}"
   68                                                 Foreground="{ThemeResource Ink3Brush}"
   69                                                 TextWrapping="Wrap"
   70                                                 Text="{Binding Hint}" />
   71                                  </utu:AutoLayout>
   72  
   73                                  <TextBlock Grid.Column="2"
   74                                             FontFamily="{StaticResource MonoFontFamily}"
   75                                             FontSize="{StaticResource TypeLabelSize}"
   76                                             Foreground="{ThemeResource InkBrush}"
   77                                             VerticalAlignment="Center"
   78                                             Margin="8,0,0,0"
   79                                             Text="{Binding Glyph}" />
   80                              </Grid>
   81                          </Button>
   82                      </DataTemplate>
   83                  </ItemsRepeater.ItemTemplate>
   84              </ItemsRepeater>
   85          </utu:AutoLayout>
   86      </Border>
   87  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs`

```csharp
    1  using System.Collections.Generic;
    2  using System.Collections.Immutable;
    3  using System.ComponentModel;
    4  using System.Reflection;
    5  using Composer.Models;
    6  using Composer.Views;
    7  using Microsoft.UI.Xaml;
    8  using Microsoft.UI.Xaml.Controls;
    9  using Microsoft.UI.Xaml.Media;
   10  
   11  namespace Composer.Views.Controls;
   12  
   13  /// <summary>
   14  /// Left rail (280px). Lists the eight layers with their per-layer state
   15  /// reflected in left-border color and opacity. Active layer highlighted
   16  /// amber; locked layers shown ink2 with ✓ glyph; future layers dimmed.
   17  /// </summary>
   18  public sealed partial class CompositionStack : UserControl
   19  {
   20      private INotifyPropertyChanged? _vm;
   21  
   22      public CompositionStack()
   23      {
   24          this.InitializeComponent();
   25          this.DataContextChanged += (_, args) => Attach(args.NewValue);
   26          this.Unloaded            += (_, _)    => Detach();
   27      }
   28  
   29      private void Attach(object? dc)
   30      {
   31          Detach();
   32          if (dc is INotifyPropertyChanged inpc)
   33          {
   34              _vm = inpc;
   35              _vm.PropertyChanged += OnVmPropertyChanged;
   36          }
   37          Render();
   38      }
   39  
   40      private void Detach()
   41      {
   42          if (_vm is null) return;
   43          _vm.PropertyChanged -= OnVmPropertyChanged;
   44          _vm = null;
   45      }
   46  
   47      private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
   48      {
   49          if (e.PropertyName is "ActiveIndex" or "LayerStates")
   50              DispatcherQueue?.TryEnqueue(Render);
   51      }
   52  
   53      private void Render()
   54      {
   55          var dc = DataContext;
   56          var (activeIndex, states) = ReadLayerSnapshot(dc);
   57          var rows = ImmutableArray.CreateBuilder<LayerRow>(Composer.Models.Layers.All.Length);
   58          for (var i = 0; i < Composer.Models.Layers.All.Length; i++)
   59          {
   60              var def = Composer.Models.Layers.All[i];
   61              var s = states.TryGetValue(def.Kind, out var v) ? v : LayerState.Clean;
   62              var isActive = i == activeIndex;
   63              var isLocked = s == LayerState.Locked;
   64              var dimmed   = !isActive && !isLocked && i > activeIndex;
   65              rows.Add(LayerRow.For(def, isActive, isLocked, dimmed));
   66          }
   67          if (LayerRows is { } repeater)
   68              repeater.ItemsSource = rows.MoveToImmutable();
   69      }
   70  
   71      private static (int index, IDictionary<LayerKind, LayerState> states) ReadLayerSnapshot(object? dc)
   72      {
   73          if (dc is null) return (0, new Dictionary<LayerKind, LayerState>());
   74  
   75          var t = dc.GetType();
   76          var idx = (t.GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;
   77  
   78          var statesObj = t.GetProperty("LayerStates")?.GetValue(dc);
   79          if (statesObj is IDictionary<LayerKind, LayerState> dict)
   80              return (idx, dict);
   81          if (statesObj is IReadOnlyDictionary<LayerKind, LayerState> rdict)
   82          {
   83              var copy = new Dictionary<LayerKind, LayerState>();
   84              foreach (var kv in rdict) copy[kv.Key] = kv.Value;
   85              return (idx, copy);
   86          }
   87          return (idx, new Dictionary<LayerKind, LayerState>());
   88      }
   89  
   90      private void OnLayerRowClick(object sender, RoutedEventArgs e)
   91      {
   92          if (sender is FrameworkElement fe && fe.Tag is LayerRow row)
   93          {
   94              var page = FindParent<Page>(this);
   95              MvuxCommandInvoker.Invoke(page?.DataContext, "Jump", row.Index);
   96          }
   97      }
   98  
   99      private static T? FindParent<T>(DependencyObject child) where T : class
  100      {
  101          var parent = VisualTreeHelper.GetParent(child);
  102          while (parent is not null && parent is not T)
  103              parent = VisualTreeHelper.GetParent(parent);
  104          return parent as T;
  105      }
  106  }
  107  
  108  /// <summary>Per-row projection bound by the rail. Brief 02 will derive a richer
  109  /// summary from per-layer state (e.g., Intent's app type); brief 01 uses the
  110  /// static <see cref="LayerDef.Hint"/> as the summary.</summary>
  111  public sealed record LayerRow(
  112      int Index,
  113      string IndexLabel,
  114      string LabelUpper,
  115      string Hint,
  116      string Glyph,
  117      Brush LeftBorderBrush,
  118      double Opacity)
  119  {
  120      public static LayerRow For(LayerDef def, bool isActive, bool isLocked, bool dimmed)
  121      {
  122          var glyph = isLocked ? "✓" : string.Empty;
  123          var border = isActive
  124              ? AppBrushes.Amber
  125              : isLocked
  126                  ? AppBrushes.Ink2
  127                  : AppBrushes.Transparent;
  128          var opacity = dimmed ? 0.42 : 1.0;
  129          return new LayerRow(
  130              Index:           def.Index,
  131              IndexLabel:      $"{(def.Index + 1):D2}",
  132              LabelUpper:      def.Label.ToUpperInvariant(),
  133              Hint:            def.Hint,
  134              Glyph:           glyph,
  135              LeftBorderBrush: border,
  136              Opacity:         opacity);
  137      }
  138  }
```

### `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml`

```xml
    1  ﻿<UserControl x:Class="Composer.Views.Controls.FilesRail"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5      <Border Background="{ThemeResource Paper2Brush}"
    6              BorderBrush="{ThemeResource HairlineBrush}"
    7              BorderThickness="1,0,0,0"
    8              Padding="20,28">
    9          <utu:AutoLayout Orientation="Vertical" Spacing="14">
   10              <TextBlock Style="{StaticResource MonoEyebrow}"
   11                         Text="FILES RAIL" />
   12              <TextBlock FontFamily="{StaticResource SerifLightItalicFontFamily}"
   13                         FontSize="{StaticResource TypeBodySmallSize}"
   14                         Foreground="{ThemeResource Ink3Brush}"
   15                         TextWrapping="Wrap"
   16                         Text="Each layer emits files as it locks." />
   17  
   18              <Border Height="1"
   19                      Background="{ThemeResource HairlineBrush}"
   20                      Margin="0,4,0,4" />
   21  
   22              <ItemsRepeater x:Name="FileRows">
   23                  <ItemsRepeater.Layout>
   24                      <StackLayout Orientation="Vertical" Spacing="2" />
   25                  </ItemsRepeater.Layout>
   26                  <ItemsRepeater.ItemTemplate>
   27                      <DataTemplate>
   28                          <Grid Padding="0,8,0,8" Opacity="{Binding Opacity}">
   29                              <Grid.ColumnDefinitions>
   30                                  <ColumnDefinition Width="Auto" />
   31                                  <ColumnDefinition Width="*" />
   32                                  <ColumnDefinition Width="Auto" />
   33                              </Grid.ColumnDefinitions>
   34                              <Ellipse Grid.Column="0"
   35                                       Width="8" Height="8"
   36                                       Margin="0,0,10,0"
   37                                       VerticalAlignment="Center"
   38                                       Fill="{Binding DotFill}"
   39                                       Stroke="{Binding DotStroke}"
   40                                       StrokeThickness="1" />
   41                              <TextBlock Grid.Column="1"
   42                                         FontFamily="{StaticResource MonoFontFamily}"
   43                                         FontSize="{StaticResource TypeLabelSize}"
   44                                         Foreground="{ThemeResource InkBrush}"
   45                                         VerticalAlignment="Center"
   46                                         Text="{Binding FileName}" />
   47                              <TextBlock Grid.Column="2"
   48                                         FontFamily="{StaticResource MonoFontFamily}"
   49                                         FontSize="{StaticResource TypeEyebrowMicroSize}"
   50                                         CharacterSpacing="160"
   51                                         Foreground="{Binding StatusBadgeBrush}"
   52                                         VerticalAlignment="Center"
   53                                         Margin="8,0,0,0"
   54                                         Text="{Binding StatusBadge}" />
   55                          </Grid>
   56                      </DataTemplate>
   57                  </ItemsRepeater.ItemTemplate>
   58              </ItemsRepeater>
   59  
   60              <Border Height="1"
   61                      Background="{ThemeResource HairlineBrush}"
   62                      Margin="0,8,0,8" />
   63              <TextBlock x:Name="LockedSummary"
   64                         FontFamily="{StaticResource MonoFontFamily}"
   65                         FontSize="{StaticResource TypeEyebrowSize}"
   66                         CharacterSpacing="160"
   67                         Foreground="{ThemeResource Ink3Brush}"
   68                         Text="0 OF 8 LOCKED" />
   69          </utu:AutoLayout>
   70      </Border>
   71  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs`

```csharp
    1  using System.Collections.Generic;
    2  using System.Collections.Immutable;
    3  using System.ComponentModel;
    4  using Composer.Models;
    5  using Composer.Views;
    6  using Microsoft.UI.Xaml.Controls;
    7  using Microsoft.UI.Xaml.Media;
    8  
    9  namespace Composer.Views.Controls;
   10  
   11  /// <summary>
   12  /// Right rail (280px). Lists the nine files (eight per-layer + synthesized
   13  /// prompt-context.md) with status dot and badge. Brief 03 layers on the
   14  /// pulse animation for Writing.
   15  /// </summary>
   16  public sealed partial class FilesRail : UserControl
   17  {
   18      private INotifyPropertyChanged? _vm;
   19  
   20      public FilesRail()
   21      {
   22          this.InitializeComponent();
   23          this.DataContextChanged += (_, args) => Attach(args.NewValue);
   24          this.Unloaded            += (_, _)    => Detach();
   25      }
   26  
   27      private void Attach(object? dc)
   28      {
   29          Detach();
   30          if (dc is INotifyPropertyChanged inpc)
   31          {
   32              _vm = inpc;
   33              _vm.PropertyChanged += OnVmPropertyChanged;
   34          }
   35          Render();
   36      }
   37  
   38      private void Detach()
   39      {
   40          if (_vm is null) return;
   41          _vm.PropertyChanged -= OnVmPropertyChanged;
   42          _vm = null;
   43      }
   44  
   45      private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
   46      {
   47          if (e.PropertyName == "FileStatuses")
   48              DispatcherQueue?.TryEnqueue(Render);
   49      }
   50  
   51      private void Render()
   52      {
   53          var statuses = ReadFileStatuses(DataContext);
   54          var total = Composer.Models.Layers.All.Length;
   55          var rows = ImmutableArray.CreateBuilder<FileRow>(total + 2);
   56          var lockedCount = 0;
   57          for (var i = 0; i < total; i++)
   58          {
   59              var def = Composer.Models.Layers.All[i];
   60              var status = statuses.TryGetValue(def.Kind, out var s) ? s : FileStatus.Planned;
   61              if (status == FileStatus.Drafted) lockedCount++;
   62              rows.Add(FileRow.For(def.File, status));
   63          }
   64          // Synthesized companion files — emit with the Scaffold layer.
   65          rows.Add(FileRow.For(Composer.Models.Layers.ReadmeFileName,        FileStatus.Planned));
   66          rows.Add(FileRow.For(Composer.Models.Layers.PromptContextFileName, FileStatus.Planned));
   67          FileRows.ItemsSource = rows.MoveToImmutable();
   68          LockedSummary.Text = $"{lockedCount} OF {total} LOCKED";
   69      }
   70  
   71      private static IDictionary<LayerKind, FileStatus> ReadFileStatuses(object? dc)
   72      {
   73          if (dc is null) return new Dictionary<LayerKind, FileStatus>();
   74          var prop = dc.GetType().GetProperty("FileStatuses")?.GetValue(dc);
   75          if (prop is IDictionary<LayerKind, FileStatus> dict) return dict;
   76          if (prop is IReadOnlyDictionary<LayerKind, FileStatus> rdict)
   77          {
   78              var copy = new Dictionary<LayerKind, FileStatus>();
   79              foreach (var kv in rdict) copy[kv.Key] = kv.Value;
   80              return copy;
   81          }
   82          return new Dictionary<LayerKind, FileStatus>();
   83      }
   84  }
   85  
   86  public sealed record FileRow(
   87      string FileName,
   88      string StatusBadge,
   89      Brush StatusBadgeBrush,
   90      Brush DotFill,
   91      Brush DotStroke,
   92      double Opacity)
   93  {
   94      public static FileRow For(string fileName, FileStatus status) => status switch
   95      {
   96          FileStatus.Drafted => new FileRow(fileName,
   97              StatusBadge:      "DRAFTED",
   98              StatusBadgeBrush: AppBrushes.Amber,
   99              DotFill:          AppBrushes.Amber,
  100              DotStroke:        AppBrushes.Amber,
  101              Opacity:          1.0),
  102          FileStatus.Writing => new FileRow(fileName,
  103              StatusBadge:      "WRITING",
  104              StatusBadgeBrush: AppBrushes.Indigo,
  105              DotFill:          AppBrushes.Indigo,
  106              DotStroke:        AppBrushes.Indigo,
  107              Opacity:          1.0),
  108          _ => new FileRow(fileName,
  109              StatusBadge:      "PLANNED",
  110              StatusBadgeBrush: AppBrushes.Ink4,
  111              DotFill:          AppBrushes.Transparent,
  112              DotStroke:        AppBrushes.Ink4,
  113              Opacity:          0.42),
  114      };
  115  }
```

### `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml`

```xml
    1  ﻿<UserControl x:Class="Composer.Views.Controls.ActiveCanvas"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:controls="using:Composer.Views.Controls"
    5               xmlns:utu="using:Uno.Toolkit.UI">
    6      <ScrollViewer x:Name="CanvasScrollHost"
    7                    HorizontalScrollMode="Disabled"
    8                    VerticalScrollMode="Auto"
    9                    VerticalScrollBarVisibility="Hidden"
   10                    Padding="32,28,32,40">
   11          <utu:AutoLayout x:Name="CanvasColumn"
   12                          Orientation="Vertical"
   13                          Spacing="20"
   14                          MinWidth="640"
   15                          MaxWidth="720"
   16                          HorizontalAlignment="Center">
   17  
   18              <!-- Slot 1 per brief §7.1 / Brief 03 §4 — extracted control. -->
   19              <controls:ProgressIndicator x:Name="ProgressRegion"
   20                                          Fraction="{Binding ProgressFraction, Mode=OneWay}"
   21                                          Label="{Binding ProgressLabel, Mode=OneWay}"
   22                                          Counter="{Binding ProgressCounter, Mode=OneWay}" />
   23  
   24              <!-- AppTitleRow hidden per user 2026-05-12 — the project name
   25                   isn't needed as a duplicate header above each canvas now
   26                   that the title moves inside the glass container. Reset
   27                   affordance will resurface elsewhere when needed. -->
   28              <controls:AppTitleRow x:Name="TitleRow"
   29                                    Visibility="Collapsed"
   30                                    ProjectName="{Binding ProjectName, Mode=OneWay}"
   31                                    ShowReset="{Binding HasLockedLayers, Mode=OneWay}" />
   32  
   33              <!-- Six discrete DPs per brief §12.1 — ShellModel projects each
   34                   from ActiveLayer / LayerStates / Recaps so this control stays
   35                   dumb. Replaces the old single-Header LayerHeaderModel binding. -->
   36              <controls:ActiveLayerHeader x:Name="HeaderRegion"
   37                                          LayerIndex="{Binding ActiveLayerIndex, Mode=OneWay}"
   38                                          LayerLabel="{Binding ActiveLayerLabel, Mode=OneWay}"
   39                                          LayerState="{Binding ActiveLayerLayerState, Mode=OneWay}"
   40                                          Recap="{Binding ActiveLayerRecap, Mode=OneWay}"
   41                                          Title="{Binding ActiveLayerTitle, Mode=OneWay}"
   42                                          Subtitle="{Binding ActiveLayerSubtitle, Mode=OneWay}" />
   43  
   44              <!-- Locked-context-card stack (above active canvas, brief 01 §4) -->
   45              <utu:AutoLayout x:Name="LockedStack"
   46                              Orientation="Vertical"
   47                              Spacing="10" />
   48  
   49              <!-- Active canvas slot — code-behind swaps between layer canvases
   50                   via CreateCanvas. M3b's Region.Attached host wasn't placing
   51                   pages inside it (likely because the region is nested several
   52                   levels below Shell); reverted to direct hosting so canvases
   53                   render. Pages + RouteMap stay as scaffolding for a future
   54                   navigation pass that correctly resolves the region. -->
   55              <ContentControl x:Name="CanvasSlot"
   56                              HorizontalContentAlignment="Stretch"
   57                              VerticalContentAlignment="Stretch"
   58                              MinHeight="480" />
   59  
   60              <!-- Composer footer sits directly under the current canvas so the
   61                   user's primary action (refine + lock) is anchored to the
   62                   surface they're looking at — the faded upcoming cards then
   63                   hint at what comes next. -->
   64              <controls:ComposerFooter x:Name="FooterRegion" />
   65  
   66              <!-- Brief 03 §3 — future-preview card stack rendered below the
   67                   composer footer at progressively decreasing opacity. -->
   68              <utu:AutoLayout x:Name="FutureStack"
   69                              Orientation="Vertical"
   70                              Spacing="8" />
   71  
   72          </utu:AutoLayout>
   73      </ScrollViewer>
   74  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml`

```xml
    1  <UserControl x:Class="Composer.Views.Controls.ProgressIndicator"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5      <!-- Per docs/ARCHITECTURE-BRIEF-from-scratch.md §7.1 slot 1 and Brief 03
    6           §4: 1px hairline track + amber fill at `(activeIndex+1)/total` *
    7           control-width + 10px gap to the eyebrow/counter row below.
    8           Animation: 480ms QuinticEase on width changes via SizeChanged-driven
    9           re-measure. -->
   10      <utu:AutoLayout Orientation="Vertical" Spacing="10">
   11          <Grid x:Name="TrackRoot" SizeChanged="OnTrackSizeChanged">
   12              <Border Height="1"
   13                      Background="{ThemeResource HairlineBrush}"
   14                      HorizontalAlignment="Stretch" />
   15              <Border x:Name="ProgressFill"
   16                      Height="1"
   17                      Background="{ThemeResource AmberBrush}"
   18                      HorizontalAlignment="Left"
   19                      Width="0" />
   20          </Grid>
   21          <Grid>
   22              <Grid.ColumnDefinitions>
   23                  <ColumnDefinition Width="*" />
   24                  <ColumnDefinition Width="Auto" />
   25              </Grid.ColumnDefinitions>
   26              <TextBlock Grid.Column="0"
   27                         x:Name="LabelText"
   28                         FontFamily="{StaticResource MonoFontFamily}"
   29                         FontSize="{StaticResource TypeEyebrowSize}"
   30                         CharacterSpacing="160"
   31                         Foreground="{ThemeResource Ink3Brush}" />
   32              <TextBlock Grid.Column="1"
   33                         x:Name="CounterText"
   34                         FontFamily="{StaticResource MonoFontFamily}"
   35                         FontSize="{StaticResource TypeEyebrowSize}"
   36                         CharacterSpacing="160"
   37                         Foreground="{ThemeResource Ink4Brush}" />
   38          </Grid>
   39      </utu:AutoLayout>
   40  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml.cs`

```csharp
    1  using System;
    2  using System.Reflection;
    3  using Microsoft.UI.Xaml;
    4  using Microsoft.UI.Xaml.Controls;
    5  using Microsoft.UI.Xaml.Media.Animation;
    6  
    7  namespace Composer.Views.Controls;
    8  
    9  /// <summary>
   10  /// Hairline + amber-fill progress indicator with mono eyebrow label and
   11  /// counter beneath. Three DPs typed object so MVUX bindable wrappers around
   12  /// <c>IFeed&lt;T&gt;</c> can be unwrapped via reflection. Per
   13  /// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §7.1 / Brief 03 §4.
   14  ///
   15  /// Width animation: 480ms QuinticEase on Fraction changes. Re-measures
   16  /// against the track's <c>ActualWidth</c> via <see cref="OnTrackSizeChanged"/>
   17  /// so the fill stays accurate when the rails open / window resizes.
   18  /// </summary>
   19  public sealed partial class ProgressIndicator : UserControl
   20  {
   21      public static readonly DependencyProperty FractionProperty =
   22          DependencyProperty.Register(
   23              nameof(Fraction), typeof(object), typeof(ProgressIndicator),
   24              new PropertyMetadata(null, OnFractionChanged));
   25  
   26      public static readonly DependencyProperty LabelProperty =
   27          DependencyProperty.Register(
   28              nameof(Label), typeof(object), typeof(ProgressIndicator),
   29              new PropertyMetadata(null, OnLabelChanged));
   30  
   31      public static readonly DependencyProperty CounterProperty =
   32          DependencyProperty.Register(
   33              nameof(Counter), typeof(object), typeof(ProgressIndicator),
   34              new PropertyMetadata(null, OnCounterChanged));
   35  
   36      public object? Fraction { get => GetValue(FractionProperty); set => SetValue(FractionProperty, value); }
   37      public object? Label    { get => GetValue(LabelProperty);    set => SetValue(LabelProperty, value); }
   38      public object? Counter  { get => GetValue(CounterProperty);  set => SetValue(CounterProperty, value); }
   39  
   40      public ProgressIndicator() => this.InitializeComponent();
   41  
   42      private static void OnFractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   43      {
   44          if (d is ProgressIndicator p) p.ApplyFraction();
   45      }
   46  
   47      private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   48      {
   49          if (d is ProgressIndicator p)
   50              p.LabelText.Text = AsString(e.NewValue) ?? string.Empty;
   51      }
   52  
   53      private static void OnCounterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   54      {
   55          if (d is ProgressIndicator p)
   56              p.CounterText.Text = AsString(e.NewValue) ?? string.Empty;
   57      }
   58  
   59      private void OnTrackSizeChanged(object sender, SizeChangedEventArgs e) => ApplyFraction();
   60  
   61      private void ApplyFraction()
   62      {
   63          var f = AsDouble(Fraction);
   64          if (double.IsNaN(f) || f < 0) f = 0;
   65          if (f > 1) f = 1;
   66  
   67          var available = TrackRoot.ActualWidth;
   68          if (available <= 0) return; // wait for SizeChanged
   69  
   70          var target = available * f;
   71  
   72          if (Composer.Views.MotionPreferences.AnimationsEnabled)
   73          {
   74              var sb = new Storyboard();
   75              var anim = new DoubleAnimation
   76              {
   77                  To = target,
   78                  Duration = new Duration(TimeSpan.FromMilliseconds(480)),
   79                  EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
   80              };
   81              Storyboard.SetTarget(anim, ProgressFill);
   82              Storyboard.SetTargetProperty(anim, "Width");
   83              sb.Children.Add(anim);
   84              sb.Begin();
   85          }
   86          else
   87          {
   88              ProgressFill.Width = target;
   89          }
   90      }
   91  
   92      private static string? AsString(object? value) => value switch
   93      {
   94          string s => s,
   95          null     => null,
   96          _        => Composer.Views.MvuxValueReader.Unwrap<string>(value),
   97      };
   98  
   99      /// <summary>Unwrap a double from an MVUX bindable feed wrapper. Mirrors
  100      /// the bool helper on <see cref="AppTitleRow"/> — <see cref="MvuxValueReader.Unwrap{T}"/>
  101      /// is class-constrained, so primitives need reflection.</summary>
  102      private static double AsDouble(object? value)
  103      {
  104          if (value is double d) return d;
  105          if (value is int i)    return i;
  106          if (value is null)     return double.NaN;
  107          const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
  108          var t = value.GetType();
  109          foreach (var name in new[] { "Value", "Current", "Model", "_value" })
  110          {
  111              if (t.GetProperty(name, Flags)?.GetValue(value) is double pv) return pv;
  112              if (t.GetField(name, Flags)?.GetValue(value)    is double fv) return fv;
  113              if (t.GetProperty(name, Flags)?.GetValue(value) is int pi)    return pi;
  114          }
  115          foreach (var prop in t.GetProperties(Flags))
  116          {
  117              if (prop.PropertyType == typeof(double) && prop.GetValue(value) is double pv) return pv;
  118          }
  119          return double.NaN;
  120      }
  121  }
```

### `Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml.cs`

```csharp
    1  using System.Reflection;
    2  using Microsoft.UI.Xaml;
    3  using Microsoft.UI.Xaml.Controls;
    4  using Microsoft.UI.Xaml.Media;
    5  
    6  namespace Composer.Views.Controls;
    7  
    8  /// <summary>
    9  /// Project name row above the canvas. Per
   10  /// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §7.1 — slot 2 in the
   11  /// canonical page template, between progress and locked cards.
   12  ///
   13  /// Two DPs:
   14  ///   <see cref="ProjectName"/> — live string from Intent.AppType.
   15  ///   <see cref="ShowReset"/>   — bool gating the Reset link's visibility
   16  ///                                (true once HasLockedLayers flips true).
   17  ///
   18  /// Both typed object so MVUX bindable wrappers around the feeds can be
   19  /// unwrapped via MvuxValueReader.
   20  /// </summary>
   21  public sealed partial class AppTitleRow : UserControl
   22  {
   23      public static readonly DependencyProperty ProjectNameProperty =
   24          DependencyProperty.Register(
   25              nameof(ProjectName), typeof(object), typeof(AppTitleRow),
   26              new PropertyMetadata(null, OnProjectNameChanged));
   27  
   28      public object? ProjectName
   29      {
   30          get => GetValue(ProjectNameProperty);
   31          set => SetValue(ProjectNameProperty, value);
   32      }
   33  
   34      public static readonly DependencyProperty ShowResetProperty =
   35          DependencyProperty.Register(
   36              nameof(ShowReset), typeof(object), typeof(AppTitleRow),
   37              new PropertyMetadata(null, OnShowResetChanged));
   38  
   39      public object? ShowReset
   40      {
   41          get => GetValue(ShowResetProperty);
   42          set => SetValue(ShowResetProperty, value);
   43      }
   44  
   45      public AppTitleRow() => this.InitializeComponent();
   46  
   47      private static void OnProjectNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   48      {
   49          if (d is not AppTitleRow row) return;
   50          var name = e.NewValue switch
   51          {
   52              string s => s,
   53              null     => null,
   54              _        => Composer.Views.MvuxValueReader.Unwrap<string>(e.NewValue),
   55          };
   56          row.ProjectNameText.Text = string.IsNullOrWhiteSpace(name) ? "Untitled" : name!;
   57      }
   58  
   59      private static void OnShowResetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   60      {
   61          if (d is not AppTitleRow row) return;
   62          var show = AsBool(e.NewValue);
   63          row.ResetButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
   64      }
   65  
   66      /// <summary>Unwrap a bool from an MVUX bindable feed wrapper. The
   67      /// generic <see cref="Composer.Views.MvuxValueReader"/> is class-typed
   68      /// so it can't dispatch on <c>bool</c> directly; reflect for the
   69      /// canonical accessors here.</summary>
   70      private static bool AsBool(object? value)
   71      {
   72          if (value is bool b) return b;
   73          if (value is null) return false;
   74          const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
   75          var t = value.GetType();
   76          foreach (var name in new[] { "Value", "Current", "Model", "_value" })
   77          {
   78              if (t.GetProperty(name, Flags)?.GetValue(value) is bool pv) return pv;
   79              if (t.GetField(name, Flags)?.GetValue(value) is bool fv) return fv;
   80          }
   81          foreach (var prop in t.GetProperties(Flags))
   82              if (prop.PropertyType == typeof(bool) && prop.GetValue(value) is bool pv)
   83                  return pv;
   84          return false;
   85      }
   86  
   87      private void OnResetClick(object sender, RoutedEventArgs e)
   88      {
   89          // Walk up to the host Shell/Page and invoke Reset on the bound
   90          // ComposerModel. Same reflection pattern as IntentCard / Stack.
   91          var host = FindParent<Page>(this);
   92          Composer.Views.MvuxCommandInvoker.Invoke(host?.DataContext, "Reset");
   93      }
   94  
   95      private static T? FindParent<T>(DependencyObject child) where T : class
   96      {
   97          var parent = VisualTreeHelper.GetParent(child);
   98          while (parent is not null && parent is not T)
   99              parent = VisualTreeHelper.GetParent(parent);
  100          return parent as T;
  101      }
  102  }
```

### `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml`

```xml
    1  <UserControl x:Class="Composer.Views.Controls.ActiveLayerHeader"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5      <utu:AutoLayout Orientation="Vertical" Spacing="6">
    6  
    7          <!-- Recap line — italic ↳, max-width 560, only rendered for
    8               non-cold-launch layers (Stack returns null and the row collapses).
    9               Per docs/ARCHITECTURE-BRIEF-from-scratch.md §9 every non-Stack
   10               layer carries a one-line recap of what was just decided. -->
   11          <utu:AutoLayout x:Name="RecapRow"
   12                          Orientation="Horizontal"
   13                          Spacing="6"
   14                          Visibility="Collapsed">
   15              <TextBlock Text="↳"
   16                         FontFamily="{StaticResource MonoFontFamily}"
   17                         FontSize="{StaticResource TypeBodySmallSize}"
   18                         Foreground="{ThemeResource Ink4Brush}"
   19                         VerticalAlignment="Top" />
   20              <TextBlock x:Name="RecapText"
   21                         FontFamily="{StaticResource SerifLightItalicFontFamily}"
   22                         FontSize="{StaticResource TypeBodySmallSize}"
   23                         Foreground="{ThemeResource Ink3Brush}"
   24                         TextWrapping="Wrap"
   25                         MaxWidth="560"
   26                         LineHeight="20" />
   27          </utu:AutoLayout>
   28  
   29          <!-- Eyebrow row removed — the ProgressIndicator at the top of the
   30               canvas already shows the layer label + counter, so the dot
   31               "● 01 · INTENT" row was redundant. State badge stays as a
   32               standalone line so EDITED / PREVIEW status still surfaces. -->
   33          <TextBlock x:Name="StateBadgeText"
   34                     FontFamily="{StaticResource MonoFontFamily}"
   35                     FontSize="{StaticResource TypeEyebrowSize}"
   36                     CharacterSpacing="160"
   37                     Foreground="{ThemeResource AmberBrush}"
   38                     Visibility="Collapsed" />
   39          <!-- LayerLabelText kept off-screen so existing code-behind references
   40               compile; the row above renders the badge directly. -->
   41          <TextBlock x:Name="LayerLabelText" Visibility="Collapsed" />
   42          <!-- Title + subtitle hidden per user 2026-05-12 — they now render
   43               INSIDE the glass container on Intent (other canvases will get
   44               the same treatment when their glass wraps land). Kept off-tree
   45               so existing code-behind assignments compile. -->
   46          <TextBlock x:Name="TitleText"    Visibility="Collapsed" />
   47          <TextBlock x:Name="SubtitleText" Visibility="Collapsed" />
   48      </utu:AutoLayout>
   49  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs`

```csharp
    1  using Composer.Models;
    2  using Microsoft.UI.Xaml;
    3  using Microsoft.UI.Xaml.Controls;
    4  
    5  namespace Composer.Views.Controls;
    6  
    7  /// <summary>
    8  /// Eyebrow + italic-Fraunces title + serif subtitle + per-state badge, with
    9  /// an optional ↳ recap row above. Six discrete DPs per
   10  /// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §12.1.
   11  ///
   12  /// LayerIndex / LayerLabel / LayerState / Recap / Title / Subtitle replace
   13  /// the previous single-Header DP that consumed a LayerHeaderModel record.
   14  /// Each is typed object so MVUX bindable wrappers can be unwrapped via
   15  /// MvuxValueReader the same way ActiveCanvas handles its header value.
   16  /// </summary>
   17  public sealed partial class ActiveLayerHeader : UserControl
   18  {
   19      public ActiveLayerHeader() => this.InitializeComponent();
   20  
   21      // ─── LayerIndex ─────────────────────────────────────────────────────
   22      public static readonly DependencyProperty LayerIndexProperty =
   23          DependencyProperty.Register(nameof(LayerIndex), typeof(object), typeof(ActiveLayerHeader),
   24              new PropertyMetadata(null, OnAnyChanged));
   25      public object? LayerIndex
   26      {
   27          get => GetValue(LayerIndexProperty);
   28          set => SetValue(LayerIndexProperty, value);
   29      }
   30  
   31      // ─── LayerLabel ─────────────────────────────────────────────────────
   32      public static readonly DependencyProperty LayerLabelProperty =
   33          DependencyProperty.Register(nameof(LayerLabel), typeof(object), typeof(ActiveLayerHeader),
   34              new PropertyMetadata(null, OnAnyChanged));
   35      public object? LayerLabel
   36      {
   37          get => GetValue(LayerLabelProperty);
   38          set => SetValue(LayerLabelProperty, value);
   39      }
   40  
   41      // ─── LayerState ─────────────────────────────────────────────────────
   42      public static readonly DependencyProperty LayerStateProperty =
   43          DependencyProperty.Register(nameof(LayerState), typeof(object), typeof(ActiveLayerHeader),
   44              new PropertyMetadata(null, OnAnyChanged));
   45      public object? LayerState
   46      {
   47          get => GetValue(LayerStateProperty);
   48          set => SetValue(LayerStateProperty, value);
   49      }
   50  
   51      // ─── Recap ──────────────────────────────────────────────────────────
   52      public static readonly DependencyProperty RecapProperty =
   53          DependencyProperty.Register(nameof(Recap), typeof(object), typeof(ActiveLayerHeader),
   54              new PropertyMetadata(null, OnAnyChanged));
   55      public object? Recap
   56      {
   57          get => GetValue(RecapProperty);
   58          set => SetValue(RecapProperty, value);
   59      }
   60  
   61      // ─── Title ──────────────────────────────────────────────────────────
   62      public static readonly DependencyProperty TitleProperty =
   63          DependencyProperty.Register(nameof(Title), typeof(object), typeof(ActiveLayerHeader),
   64              new PropertyMetadata(null, OnAnyChanged));
   65      public object? Title
   66      {
   67          get => GetValue(TitleProperty);
   68          set => SetValue(TitleProperty, value);
   69      }
   70  
   71      // ─── Subtitle ───────────────────────────────────────────────────────
   72      public static readonly DependencyProperty SubtitleProperty =
   73          DependencyProperty.Register(nameof(Subtitle), typeof(object), typeof(ActiveLayerHeader),
   74              new PropertyMetadata(null, OnAnyChanged));
   75      public object? Subtitle
   76      {
   77          get => GetValue(SubtitleProperty);
   78          set => SetValue(SubtitleProperty, value);
   79      }
   80  
   81      private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   82      {
   83          if (d is ActiveLayerHeader h) h.Apply();
   84      }
   85  
   86      private void Apply()
   87      {
   88          // LayerLabel — string (already projected from "01 · STACK" format).
   89          LayerLabelText.Text = AsString(LayerLabel) ?? string.Empty;
   90  
   91          // Title + Subtitle — strings.
   92          TitleText.Text    = AsString(Title)    ?? string.Empty;
   93          SubtitleText.Text = AsString(Subtitle) ?? string.Empty;
   94  
   95          // Recap — nullable string. Row collapses when null/empty (Stack layer).
   96          var recap = AsString(Recap);
   97          if (string.IsNullOrWhiteSpace(recap))
   98          {
   99              RecapRow.Visibility = Visibility.Collapsed;
  100          }
  101          else
  102          {
  103              RecapText.Text       = recap;
  104              RecapRow.Visibility  = Visibility.Visible;
  105          }
  106  
  107          // LayerState — enum value; map to the per-state badge text. Badge
  108          // visibility flips on/off so the eyebrow row collapses when the
  109          // layer is Clean / Locked.
  110          var state = AsLayerState(LayerState);
  111          var badge = state switch
  112          {
  113              Composer.Models.LayerState.Dirty       => "EDITED — PREVIEW PENDING",
  114              Composer.Models.LayerState.Previewing  => "PREVIEW — REVIEW AND ACCEPT",
  115              _                                       => string.Empty,
  116          };
  117          StateBadgeText.Text = badge;
  118          StateBadgeText.Visibility = string.IsNullOrEmpty(badge)
  119              ? Microsoft.UI.Xaml.Visibility.Collapsed
  120              : Microsoft.UI.Xaml.Visibility.Visible;
  121      }
  122  
  123      private static string? AsString(object? value)
  124      {
  125          if (value is null) return null;
  126          if (value is string s) return s;
  127          return Composer.Views.MvuxValueReader.Unwrap<string>(value);
  128      }
  129  
  130      private static Composer.Models.LayerState AsLayerState(object? value)
  131      {
  132          if (value is Composer.Models.LayerState direct) return direct;
  133          if (value is null) return Composer.Models.LayerState.Clean;
  134          // MVUX wrapper may box the enum; reflect on it.
  135          var t = value.GetType();
  136          foreach (var name in new[] { "Value", "Current", "Model", "_value" })
  137          {
  138              var prop = t.GetProperty(name);
  139              if (prop?.GetValue(value) is Composer.Models.LayerState s) return s;
  140              var field = t.GetField(name, System.Reflection.BindingFlags.Instance
  141                                         | System.Reflection.BindingFlags.Public
  142                                         | System.Reflection.BindingFlags.NonPublic);
  143              if (field?.GetValue(value) is Composer.Models.LayerState f) return f;
  144          }
  145          return Composer.Models.LayerState.Clean;
  146      }
  147  }
```

### `Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml`

```xml
    1  ﻿<UserControl x:Class="Composer.Views.Controls.LockedContextCard"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5      <Border Background="{ThemeResource Paper2Brush}"
    6              BorderBrush="{ThemeResource HairlineBrush}"
    7              BorderThickness="1"
    8              CornerRadius="4"
    9              Padding="20,18">
   10          <Border.Resources />
   11          <Grid>
   12              <Grid.RowDefinitions>
   13                  <RowDefinition Height="Auto" />
   14                  <RowDefinition Height="Auto" />
   15                  <RowDefinition Height="Auto" />
   16                  <RowDefinition Height="Auto" />
   17              </Grid.RowDefinitions>
   18  
   19              <!-- Header row: [−/+] ✓ LABEL · LOCKED            Revisit -->
   20              <Grid Grid.Row="0">
   21                  <Grid.ColumnDefinitions>
   22                      <ColumnDefinition Width="Auto" />
   23                      <ColumnDefinition Width="*" />
   24                      <ColumnDefinition Width="Auto" />
   25                  </Grid.ColumnDefinitions>
   26                  <Button Grid.Column="0"
   27                          x:Name="ExpandToggle"
   28                          Style="{StaticResource LinkButtonStyle}"
   29                          FontFamily="{StaticResource MonoFontFamily}"
   30                          FontSize="{StaticResource TypeBodySmallSize}"
   31                          Padding="4,2,8,2"
   32                          Foreground="{ThemeResource Ink4Brush}"
   33                          Content="−"
   34                          Click="OnExpandToggleClick" />
   35                  <TextBlock Grid.Column="1"
   36                             FontFamily="{StaticResource MonoFontFamily}"
   37                             FontSize="{StaticResource TypeEyebrowSize}"
   38                             CharacterSpacing="160"
   39                             Foreground="{ThemeResource InkBrush}"
   40                             VerticalAlignment="Center">
   41                      <Run Text="✓ " Foreground="{ThemeResource Ink2Brush}" />
   42                      <Run x:Name="HeaderLabelRun" />
   43                      <Run Foreground="{ThemeResource Ink3Brush}" Text="  ·  LOCKED" />
   44                  </TextBlock>
   45                  <Button Grid.Column="2"
   46                          x:Name="RevisitButton"
   47                          Style="{StaticResource LinkButtonStyle}"
   48                          Content="Revisit"
   49                          Click="OnRevisitClick" />
   50              </Grid>
   51  
   52              <!-- Italic-serif summary -->
   53              <TextBlock Grid.Row="1"
   54                         x:Name="SummaryText"
   55                         FontFamily="{StaticResource SerifLightItalicFontFamily}"
   56                         FontSize="{StaticResource TypeBodySmallSize}"
   57                         Foreground="{ThemeResource InkBrush}"
   58                         Margin="0,8,0,0"
   59                         TextWrapping="Wrap" />
   60  
   61              <Border Grid.Row="2"
   62                      x:Name="DividerLine"
   63                      Height="1"
   64                      Background="{ThemeResource HairlineBrush}"
   65                      Margin="0,12,0,12" />
   66  
   67              <!-- 4-fact detail grid (2 columns: label, value × 4 rows) -->
   68              <Grid Grid.Row="3" x:Name="FactsGrid">
   69                  <Grid.RowDefinitions>
   70                      <RowDefinition Height="Auto" />
   71                      <RowDefinition Height="Auto" />
   72                      <RowDefinition Height="Auto" />
   73                      <RowDefinition Height="Auto" />
   74                  </Grid.RowDefinitions>
   75                  <Grid.ColumnDefinitions>
   76                      <ColumnDefinition Width="120" />
   77                      <ColumnDefinition Width="*" />
   78                  </Grid.ColumnDefinitions>
   79              </Grid>
   80          </Grid>
   81      </Border>
   82  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs`

```csharp
    1  using System.Collections.Generic;
    2  using Composer.Models;
    3  using Composer.Views;
    4  using Microsoft.UI.Xaml;
    5  using Microsoft.UI.Xaml.Controls;
    6  using Microsoft.UI.Xaml.Media;
    7  
    8  namespace Composer.Views.Controls;
    9  
   10  /// <summary>
   11  /// One locked layer's compressed summary card. Stacks above the active
   12  /// canvas — by layer 8 there are seven of these visible. Header + italic
   13  /// summary sentence + four-fact detail grid + Revisit link.
   14  /// </summary>
   15  public sealed partial class LockedContextCard : UserControl
   16  {
   17      public static readonly DependencyProperty LayerKindProperty =
   18          DependencyProperty.Register(
   19              nameof(LayerKind), typeof(LayerKind), typeof(LockedContextCard),
   20              new PropertyMetadata(Composer.Models.LayerKind.Architecture, OnLayerKindChanged));
   21  
   22      public static readonly DependencyProperty SummaryProperty =
   23          DependencyProperty.Register(
   24              nameof(Summary), typeof(string), typeof(LockedContextCard),
   25              new PropertyMetadata("", OnSummaryChanged));
   26  
   27      public LayerKind LayerKind
   28      {
   29          get => (LayerKind)GetValue(LayerKindProperty);
   30          set => SetValue(LayerKindProperty, value);
   31      }
   32  
   33      public string Summary
   34      {
   35          get => (string)GetValue(SummaryProperty);
   36          set => SetValue(SummaryProperty, value);
   37      }
   38  
   39      public static readonly DependencyProperty IsExpandedProperty =
   40          DependencyProperty.Register(
   41              nameof(IsExpanded), typeof(bool), typeof(LockedContextCard),
   42              new PropertyMetadata(true, OnIsExpandedChanged));
   43  
   44      /// <summary>Whether the summary + facts grid are visible. Two-way:
   45      /// the parent stack can bind to DefaultExpandedKinds to auto-collapse
   46      /// older locked layers; the chevron in the header toggles it locally
   47      /// as a manual override.</summary>
   48      public bool IsExpanded
   49      {
   50          get => (bool)GetValue(IsExpandedProperty);
   51          set => SetValue(IsExpandedProperty, value);
   52      }
   53  
   54      public IList<KeyValuePair<string, string>>? Facts { get; set; }
   55  
   56      public LockedContextCard()
   57      {
   58          this.InitializeComponent();
   59          this.Loaded += (_, _) => { Render(); ApplyExpansion(); };
   60      }
   61  
   62      private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   63      {
   64          if (d is LockedContextCard c) c.ApplyExpansion();
   65      }
   66  
   67      private void ApplyExpansion()
   68      {
   69          var v = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
   70          SummaryText.Visibility = v;
   71          DividerLine.Visibility = v;
   72          FactsGrid.Visibility   = v;
   73          ExpandToggle.Content   = IsExpanded ? "−" : "+";
   74      }
   75  
   76      private void OnExpandToggleClick(object sender, RoutedEventArgs e)
   77      {
   78          IsExpanded = !IsExpanded;
   79      }
   80  
   81      private static void OnLayerKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   82      {
   83          if (d is LockedContextCard c) c.Render();
   84      }
   85  
   86      private static void OnSummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   87      {
   88          if (d is LockedContextCard c) c.SummaryText.Text = (string?)e.NewValue ?? "";
   89      }
   90  
   91      private void Render()
   92      {
   93          var def = Composer.Models.Layers.Get(LayerKind);
   94          HeaderLabelRun.Text = def.Label.ToUpperInvariant();
   95          SummaryText.Text = Summary ?? string.Empty;
   96  
   97          FactsGrid.Children.Clear();
   98          if (Facts is null) return;
   99  
  100          var row = 0;
  101          foreach (var kv in Facts)
  102          {
  103              if (row > 3) break;
  104  
  105              var label = new TextBlock
  106              {
  107                  Text = kv.Key.ToUpperInvariant(),
  108                  FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
  109                  FontSize = 10,
  110                  CharacterSpacing = 160,
  111                  Foreground = (Brush)Application.Current.Resources["Ink3Brush"],
  112                  Margin = new Thickness(0, 4, 8, 4),
  113              };
  114              Grid.SetRow(label, row);
  115              Grid.SetColumn(label, 0);
  116              FactsGrid.Children.Add(label);
  117  
  118              var value = new TextBlock
  119              {
  120                  Text = kv.Value,
  121                  FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
  122                  FontSize = 12,
  123                  Foreground = (Brush)Application.Current.Resources["InkBrush"],
  124                  TextWrapping = TextWrapping.Wrap,
  125                  Margin = new Thickness(0, 4, 0, 4),
  126              };
  127              Grid.SetRow(value, row);
  128              Grid.SetColumn(value, 1);
  129              FactsGrid.Children.Add(value);
  130  
  131              row++;
  132          }
  133      }
  134  
  135      private void OnRevisitClick(object sender, RoutedEventArgs e)
  136      {
  137          var page = FindParent<Page>(this);
  138          MvuxCommandInvoker.Invoke(page?.DataContext, "Revisit", LayerKind);
  139      }
  140  
  141      private static T? FindParent<T>(DependencyObject child) where T : class
  142      {
  143          var parent = VisualTreeHelper.GetParent(child);
  144          while (parent is not null && parent is not T)
  145              parent = VisualTreeHelper.GetParent(parent);
  146          return parent as T;
  147      }
  148  }
```

### `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs`

```csharp
    1  using System;
    2  using System.Collections.Generic;
    3  using System.ComponentModel;
    4  using Composer.Models;
    5  using Composer.Views.Layers;
    6  using Microsoft.UI.Xaml;
    7  using Microsoft.UI.Xaml.Controls;
    8  using Microsoft.UI.Xaml.Media.Animation;
    9  
   10  namespace Composer.Views.Controls;
   11  
   12  /// <summary>
   13  /// Center column. Picks the right per-layer canvas (brief 02), stacks
   14  /// locked-context cards above (brief 01 §4), and routes the composer footer
   15  /// state to the layer's current LayerState. Brief 03 layers on the progress
   16  /// hairline at the top and the future-preview card stack below the canvas
   17  /// slot at progressively decreasing opacity.
   18  /// </summary>
   19  public sealed partial class ActiveCanvas : UserControl
   20  {
   21      // M6: ActiveCanvas no longer forwards a Header DP — the inner
   22      // ActiveLayerHeader binds its six DPs directly to ShellModel's
   23      // ActiveLayer* feeds (in ActiveCanvas.xaml). Shell.xaml passes nothing
   24      // header-related to ActiveCanvas.
   25  
   26      private INotifyPropertyChanged? _vm;
   27  
   28      public ActiveCanvas()
   29      {
   30          this.InitializeComponent();
   31          this.DataContextChanged += (_, args) => Attach(args.NewValue);
   32          this.Unloaded            += (_, _)    => Detach();
   33          // Progress is now self-managing inside ProgressIndicator.
   34      }
   35  
   36      private void Attach(object? dc)
   37      {
   38          Detach();
   39          if (dc is INotifyPropertyChanged inpc)
   40          {
   41              _vm = inpc;
   42              _vm.PropertyChanged += OnVmPropertyChanged;
   43          }
   44          SyncSlot();
   45          SyncFooter();
   46          SyncLockedStack();
   47          SyncFutureStack();
   48          SyncFocusedFirst();
   49      }
   50  
   51      private void Detach()
   52      {
   53          if (_vm is null) return;
   54          _vm.PropertyChanged -= OnVmPropertyChanged;
   55          _vm = null;
   56      }
   57  
   58      private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
   59      {
   60          if (e.PropertyName is "ActiveIndex" or "ActiveLayer")
   61          {
   62              DispatcherQueue?.TryEnqueue(SyncSlot);
   63              DispatcherQueue?.TryEnqueue(SyncFutureStack);
   64          }
   65          if (e.PropertyName is "ActiveLayerHeader" or "ActiveLayerLayerState" or "LayerStates")
   66              DispatcherQueue?.TryEnqueue(SyncFooter);
   67          if (e.PropertyName is "ActiveLayer" or "LayerStates" or "Architecture" or "UX" or "Design" or "Data" or "Implementation")
   68              DispatcherQueue?.TryEnqueue(SyncLockedStack);
   69          if (e.PropertyName is "RailsVisible" or "IsFocusedFirst" or "LayerStates")
   70          {
   71              DispatcherQueue?.TryEnqueue(SyncFutureStack);
   72              DispatcherQueue?.TryEnqueue(SyncFocusedFirst);
   73          }
   74      }
   75  
   76      /// <summary>Read RailsVisible / IsFocusedFirst off the bound model and
   77      /// animate the canvas chrome between focused-first dimensions
   78      /// (max-width 720, padding-top 64) and rails-open dimensions
   79      /// (max-width 880, padding-top 32). Per design brief §2.1 / §2.2.</summary>
   80      private bool _focusedFirstApplied = true;
   81  
   82      private void SyncFocusedFirst()
   83      {
   84          var dc = DataContext;
   85          if (dc is null) return;
   86          // Reads go through MvuxValueReader.ReadRaw — caches the first-level
   87          // GetProperty lookup so repeated property-change handlers don't pay
   88          // reflection cost. The bool? cast unwraps an unboxed value; if the
   89          // feed is wrapped, fall back to IsFocusedFirst.
   90          var rv = Composer.Views.MvuxValueReader.ReadRaw(dc, "RailsVisible") as bool?;
   91          if (rv is null)
   92          {
   93              var ff = Composer.Views.MvuxValueReader.ReadRaw(dc, "IsFocusedFirst") as bool?;
   94              if (ff.HasValue) rv = !ff.Value;
   95          }
   96          var railsVisible = rv ?? false;
   97          var focused = !railsVisible;
   98          if (focused == _focusedFirstApplied) return;
   99          _focusedFirstApplied = focused;
  100  
  101          var targetMin    = focused ? 640d : 820d;
  102          var targetMax    = focused ? 720d : 880d;
  103          var targetTopPad = focused ? 64d  : 28d;
  104  
  105          if (!Composer.Views.MotionPreferences.AnimationsEnabled)
  106          {
  107              CanvasColumn.MinWidth = targetMin;
  108              CanvasColumn.MaxWidth = targetMax;
  109              CanvasScrollHost.Padding = new Thickness(32, targetTopPad, 32, 40);
  110              return;
  111          }
  112  
  113          // 480ms ease-out-quint per brief — matches the rail reveal rhythm
  114          // so canvas + rails animate together. The Storyboard + animations
  115          // are pooled per control so we don't allocate fresh objects every
  116          // toggle (audit 2026-05-12 §7).
  117          if (_focusedFirstStoryboard is null)
  118          {
  119              _focusedFirstStoryboard = new Storyboard();
  120              _focusedFirstMinAnim = new DoubleAnimation
  121              {
  122                  Duration = TimeSpan.FromMilliseconds(480),
  123                  EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
  124              };
  125              Storyboard.SetTarget(_focusedFirstMinAnim, CanvasColumn);
  126              Storyboard.SetTargetProperty(_focusedFirstMinAnim, "MinWidth");
  127              _focusedFirstStoryboard.Children.Add(_focusedFirstMinAnim);
  128  
  129              _focusedFirstMaxAnim = new DoubleAnimation
  130              {
  131                  Duration = TimeSpan.FromMilliseconds(480),
  132                  EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
  133              };
  134              Storyboard.SetTarget(_focusedFirstMaxAnim, CanvasColumn);
  135              Storyboard.SetTargetProperty(_focusedFirstMaxAnim, "MaxWidth");
  136              _focusedFirstStoryboard.Children.Add(_focusedFirstMaxAnim);
  137          }
  138  
  139          _focusedFirstStoryboard.Stop();
  140          _focusedFirstMinAnim!.To = targetMin;
  141          _focusedFirstMaxAnim!.To = targetMax;
  142          _focusedFirstStoryboard.Begin();
  143  
  144          // Padding doesn't animate cleanly via DoubleAnimation on Skia; snap it.
  145          CanvasScrollHost.Padding = new Thickness(32, targetTopPad, 32, 40);
  146      }
  147  
  148      // Pooled storyboard + animations for SyncFocusedFirst — built lazily on
  149      // first run, then reused with new To values per audit 2026-05-12 §7.
  150      private Storyboard? _focusedFirstStoryboard;
  151      private DoubleAnimation? _focusedFirstMinAnim;
  152      private DoubleAnimation? _focusedFirstMaxAnim;
  153  
  154      // OnHeaderChanged removed in M6 — the header now binds its own DPs directly
  155      // from XAML. SyncFooter / SyncProgress are still triggered from
  156      // OnVmPropertyChanged when the relevant feeds change.
  157  
  158      // NavigateToActiveLayer removed — region-based navigation reverted to
  159      // inline canvas hosting via SyncSlot. Pages + RouteMap stay registered;
  160      // a future pass will wire navigation properly.
  161  
  162  
  163      private void SyncFooter()
  164      {
  165          var state = ReadActiveLayerState();
  166          FooterRegion.State = state;
  167  
  168          var dc = DataContext;
  169          if (dc is null) return;
  170          var prompt = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActivePrompt") as string) ?? string.Empty;
  171          FooterRegion.Prompt = prompt;
  172  
  173          // Scaffold is terminal — its own canvas owns the Download bundle
  174          // action. Hiding the standard composer footer here so the user
  175          // can't get stuck clicking "Continue →" with nowhere to go.
  176          var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
  177          var clamped = System.Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
  178          var activeKind = Composer.Models.Layers.All[clamped].Kind;
  179          FooterRegion.Visibility = activeKind == LayerKind.Scaffold
  180              ? Visibility.Collapsed
  181              : Visibility.Visible;
  182      }
  183  
  184      /// <summary>Read the active layer's state from the bound dictionary on
  185      /// ComposerViewModel. Walks <c>LayerStates</c> at the active index;
  186      /// falls back to <c>Clean</c> when anything along the path is missing.
  187      /// Reflection lookups go through MvuxValueReader.ReadRaw for caching.</summary>
  188      private LayerState ReadActiveLayerState()
  189      {
  190          var dc = DataContext;
  191          if (dc is null) return LayerState.Clean;
  192          var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
  193          var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
  194          var kind = Composer.Models.Layers.All[clamped].Kind;
  195          var statesRaw = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
  196          var states = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState>>(statesRaw);
  197          if (states is null && statesRaw is System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState> direct)
  198              states = direct;
  199          return states is not null && states.TryGetValue(kind, out var s) ? s : LayerState.Clean;
  200      }
  201  
  202      // SyncProgress removed in M4-step-C — the extracted ProgressIndicator
  203      // control manages its own animation off the Fraction DP. Label and
  204      // Counter bind directly to ComposerModel.ProgressLabel/ProgressCounter.
  205  
  206      private void SyncFutureStack()
  207      {
  208          FutureStack.Children.Clear();
  209          var dc = DataContext;
  210          if (dc is null) return;
  211  
  212          var railsVisible = (Composer.Views.MvuxValueReader.ReadRaw(dc, "RailsVisible") as bool?) ?? false;
  213          if (!railsVisible) return; // Brief §2.4: future cards only render when rails are visible.
  214  
  215          var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
  216          var all = Composer.Models.Layers.All;
  217          var animations = Composer.Views.MotionPreferences.AnimationsEnabled;
  218  
  219          for (var i = idx + 1; i < all.Length; i++)
  220          {
  221              var distance = i - idx;
  222              // Brief §2.4 — opacity = max(0.05, 0.40 - distance * 0.08).
  223              //   Distance 1 → 0.32  Distance 4 → 0.08
  224              //   Distance 2 → 0.24  Distance 5+ → 0.05
  225              //   Distance 3 → 0.16
  226              var targetOpacity = Math.Max(0.05, 0.40 - (distance * 0.08));
  227              var card = new FuturePreviewCard
  228              {
  229                  LayerLabel = all[i].Label.ToUpperInvariant(),
  230                  Hint       = all[i].Hint,
  231                  Opacity    = animations ? 0 : targetOpacity,
  232              };
  233              FutureStack.Children.Add(card);
  234  
  235              if (animations)
  236              {
  237                  // 480ms QuinticEase EaseOut — matches the rail-reveal rhythm
  238                  // so the cards fade up alongside the workspace opening.
  239                  var sb = new Storyboard();
  240                  var anim = new DoubleAnimation
  241                  {
  242                      To = targetOpacity,
  243                      Duration = TimeSpan.FromMilliseconds(480),
  244                      EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
  245                  };
  246                  Storyboard.SetTarget(anim, card);
  247                  Storyboard.SetTargetProperty(anim, "Opacity");
  248                  sb.Children.Add(anim);
  249                  sb.Begin();
  250              }
  251          }
  252      }
  253  
  254      private void SyncLockedStack()
  255      {
  256          LockedStack.Children.Clear();
  257          var dc = DataContext;
  258          if (dc is null) return;
  259  
  260          if (Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates") is not IDictionary<LayerKind, LayerState> states)
  261              return;
  262  
  263          // Read DefaultExpandedKinds — the two most recently locked layers
  264          // stay expanded; older ones collapse by default. Per v11 brief §10.
  265          var defaultExpanded = ReadDefaultExpandedKinds(dc);
  266  
  267          foreach (var def in Composer.Models.Layers.All)
  268          {
  269              if (!states.TryGetValue(def.Kind, out var s) || s != LayerState.Locked) continue;
  270              var (summary, facts) = LockedSummaryFor(def.Kind, dc);
  271              var card = new LockedContextCard
  272              {
  273                  LayerKind  = def.Kind,
  274                  Summary    = summary,
  275                  Facts      = facts,
  276                  IsExpanded = defaultExpanded?.Contains(def.Kind) ?? true,
  277              };
  278              LockedStack.Children.Add(card);
  279          }
  280      }
  281  
  282      private static System.Collections.Generic.HashSet<LayerKind>? ReadDefaultExpandedKinds(object dc)
  283      {
  284          var raw = Composer.Views.MvuxValueReader.ReadRaw(dc, "DefaultExpandedKinds");
  285          if (raw is null) return null;
  286          if (raw is System.Collections.Immutable.IImmutableSet<LayerKind> set)
  287              return new System.Collections.Generic.HashSet<LayerKind>(set);
  288          var unwrapped = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableSet<LayerKind>>(raw);
  289          return unwrapped is null ? null : new System.Collections.Generic.HashSet<LayerKind>(unwrapped);
  290      }
  291  
  292      /// <summary>Read the active layer's <see cref="LayerKind"/> off the
  293      /// bound model and place the matching canvas into CanvasSlot. Direct
  294      /// hosting — the Region-based navigation didn't successfully mount
  295      /// pages, so canvases get instantiated here per Brief 02. The Pages
  296      /// + RouteMap stay as future-friendly scaffolding.</summary>
  297      private void SyncSlot()
  298      {
  299          var dc = DataContext;
  300          if (dc is null) return;
  301          var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
  302          var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
  303          var kind = Composer.Models.Layers.All[clamped].Kind;
  304          var canvas = CreateCanvas(kind);
  305          if (canvas is FrameworkElement fe)
  306              fe.DataContext = dc;
  307          CanvasSlot.Content = canvas;
  308      }
  309  
  310      private static UserControl CreateCanvas(LayerKind kind) => kind switch
  311      {
  312          LayerKind.Intent          => new IntentCard(),
  313          LayerKind.UX              => new UXFlowStrip(),
  314          LayerKind.Architecture    => new Composer.Views.Layers.ArchitectureBlueprint(),
  315          LayerKind.DesignSystem    => new DesignTokenGrid(),
  316          LayerKind.Interactions    => new InteractionsStateTimeline(),
  317          LayerKind.Data            => new DataContractGrid(),
  318          LayerKind.Implementation  => new ImplementationPhaseGrid(),
  319          LayerKind.Scaffold        => new ScaffoldTerminal(),
  320          _ => new IntentCard(),
  321      };
  322  
  323      private static (string Summary, IList<KeyValuePair<string, string>> Facts) LockedSummaryFor(LayerKind kind, object dc)
  324      {
  325          var t = dc.GetType();
  326          switch (kind)
  327          {
  328              case LayerKind.Intent:
  329                  if (t.GetProperty("Intent")?.GetValue(dc) is Composer.Models.IntentValues iv)
  330                  {
  331                      var summary = $"\"{iv.AppType}, for {iv.PrimaryUser.ToLowerInvariant()}. {iv.Workflow}.\"";
  332                      return (summary,
  333                          new List<KeyValuePair<string, string>>
  334                          {
  335                              new("App type",     iv.AppType),
  336                              new("Primary user", iv.PrimaryUser),
  337                              new("Workflow",     iv.Workflow),
  338                              new("Platforms",    iv.Platforms),
  339                          });
  340                  }
  341                  break;
  342              case LayerKind.Architecture:
  343                  if (t.GetProperty("Architecture")?.GetValue(dc) is Composer.Models.ArchitectureBlueprint bp)
  344                  {
  345                      var pattern = bp.Modules.Any(m => m.Id == "mvux") ? "MVUX"
  346                                  : bp.Modules.Any(m => m.Id == "mvvm") ? "MVVM"
  347                                  : "Custom";
  348                      var hasStorage = bp.Modules.Any(m => m.Id == "storage");
  349                      var persistence = hasStorage ? "Offline-first" : "Server-only";
  350                      return ($"\"{LayerMarkdownTemplates.DeriveArchitectureSummary(bp)}\"",
  351                          new List<KeyValuePair<string, string>>
  352                          {
  353                              new("Modules",     bp.Modules.Length.ToString()),
  354                              new("Connections", bp.Edges.Length.ToString()),
  355                              new("Pattern",     pattern),
  356                              new("Persistence", persistence),
  357                          });
  358                  }
  359                  break;
  360              case LayerKind.UX:
  361                  if (t.GetProperty("UX")?.GetValue(dc) is UXFlow flow)
  362                  {
  363                      return ($"\"{flow.Screens.Length}-screen {flow.Name.ToLowerInvariant()} flow.\"",
  364                          new List<KeyValuePair<string, string>>
  365                          {
  366                              new("Screens",       flow.Screens.Length.ToString()),
  367                              new("Primary flow",  flow.Name),
  368                              new("Empty states",  "4"),
  369                              new("Error states",  "2"),
  370                          });
  371                  }
  372                  break;
  373              case LayerKind.DesignSystem:
  374                  if (t.GetProperty("Design")?.GetValue(dc) is DesignTokens d)
  375                  {
  376                      var action = $"#{d.Action.R:X2}{d.Action.G:X2}{d.Action.B:X2}";
  377                      return ($"\"{d.BodyFont} body, action {action}, Material rhythm.\"",
  378                          new List<KeyValuePair<string, string>>
  379                          {
  380                              new("Body",       d.BodyFont),
  381                              new("Action",     action),
  382                              new("Type scale", "4 levels"),
  383                              new("Spacing",    "4px grid"),
  384                          });
  385                  }
  386                  break;
  387              case LayerKind.Interactions:
  388                  if (UnwrapInteractionsFlows(t.GetProperty("Interactions")?.GetValue(dc)) is { } flows)
  389                  {
  390                      var allStates    = flows.SelectMany(f => f.States).ToList();
  391                      var hasOffline   = allStates.Any(s => s.Kind == StateKind.Offline
  392                                                            && !string.IsNullOrWhiteSpace(s.Description)
  393                                                            && !s.Description.Contains("not applicable", System.StringComparison.OrdinalIgnoreCase));
  394                      var hasPerm      = allStates.Any(s =>
  395                          s.Description.Contains("permission", System.StringComparison.OrdinalIgnoreCase)
  396                          || s.Description.Contains("camera", System.StringComparison.OrdinalIgnoreCase)
  397                          || s.Description.Contains("location", System.StringComparison.OrdinalIgnoreCase)
  398                          || s.Description.Contains("notification", System.StringComparison.OrdinalIgnoreCase));
  399                      var offlineDesc  = hasOffline ? "offline-first throughout" : "online-only";
  400                      return ($"\"Six states across {flows.Length} flows; {offlineDesc}.\"",
  401                          new List<KeyValuePair<string, string>>
  402                          {
  403                              new("Flows",             flows.Length.ToString()),
  404                              new("States/flow",       "6"),
  405                              new("Offline",           hasOffline ? "Yes" : "No"),
  406                              new("Permission states", hasPerm    ? "Yes" : "No"),
  407                          });
  408                  }
  409                  break;
  410              case LayerKind.Data:
  411                  if (t.GetProperty("Data")?.GetValue(dc) is DataContracts dc2)
  412                  {
  413                      var fieldCount = 0;
  414                      foreach (var ent in dc2.Entities) fieldCount += ent.Fields.Length;
  415                      return ($"\"{dc2.Entities.Length} entities with explicit nullability.\"",
  416                          new List<KeyValuePair<string, string>>
  417                          {
  418                              new("Entities",    dc2.Entities.Length.ToString()),
  419                              new("Fields",      fieldCount.ToString()),
  420                              new("Records",     dc2.Entities.Length.ToString()),
  421                              new("Collections", "—"),
  422                          });
  423                  }
  424                  break;
  425              case LayerKind.Implementation:
  426                  if (t.GetProperty("Implementation")?.GetValue(dc) is BuildPlan plan)
  427                  {
  428                      return ($"\"{plan.Phases.Length} phases, scaffold → polish, with explicit dependencies.\"",
  429                          new List<KeyValuePair<string, string>>
  430                          {
  431                              new("Phases",       plan.Phases.Length.ToString()),
  432                              new("Acceptance",   "Per phase"),
  433                              new("Verification", "Per phase"),
  434                              new("Order",        "Linear"),
  435                          });
  436                  }
  437                  break;
  438          }
  439          return (Composer.Models.Layers.Get(kind).Hint, new List<KeyValuePair<string, string>>());
  440      }
  441  
  442      /// <summary>Unwrap the Interactions state value into its flows array,
  443      /// regardless of whether the binding gave us the raw record, the matrix
  444      /// wrapper, an MVUX bindable proxy, or the legacy direct array shape.</summary>
  445      private static System.Collections.Immutable.ImmutableArray<InteractionFlow>? UnwrapInteractionsFlows(object? value)
  446      {
  447          if (value is null) return null;
  448  
  449          if (value is InteractionsMatrix mx && !mx.Flows.IsDefaultOrEmpty)
  450              return mx.Flows;
  451  
  452          var unwrappedMatrix = Composer.Views.MvuxValueReader.Unwrap<InteractionsMatrix>(value);
  453          if (unwrappedMatrix is { } um && !um.Flows.IsDefaultOrEmpty)
  454              return um.Flows;
  455  
  456          if (value is System.Collections.Immutable.ImmutableArray<InteractionFlow> arr && !arr.IsDefaultOrEmpty)
  457              return arr;
  458  
  459          return null;
  460      }
  461  }
```

### `Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml`

```xml
    1  <UserControl x:Class="Composer.Views.Controls.ComposerFooter"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5      <Border Background="{ThemeResource Paper2Brush}"
    6              BorderBrush="{ThemeResource HairlineBrush}"
    7              BorderThickness="1"
    8              CornerRadius="6"
    9              Padding="20,18">
   10          <utu:AutoLayout Orientation="Vertical" Spacing="14">
   11  
   12              <!-- Eyebrow — COMPOSER · {REFINING/LISTENING/PROPOSING} -->
   13              <TextBlock x:Name="EyebrowText"
   14                         Style="{StaticResource MonoEyebrow}"
   15                         Text="COMPOSER · REFINING" />
   16  
   17              <!-- Lead question — per-layer prompt from ShellModel.ActiveLeadQuestion.
   18                   Sits between the eyebrow and the textarea; this is the line the
   19                   prototype uses to frame what the user can refine. -->
   20              <TextBlock x:Name="LeadQuestionText"
   21                         FontFamily="{StaticResource SansFontFamily}"
   22                         FontSize="{StaticResource TypeBodySize}"
   23                         Foreground="{ThemeResource Ink2Brush}"
   24                         TextWrapping="Wrap"
   25                         LineHeight="22" />
   26  
   27              <!-- Acknowledgment line — visible only in the Previewing state.
   28                   Italic Inter behind a 1px amber left rule. Brief §6. -->
   29              <Border x:Name="AckLine"
   30                      BorderBrush="{ThemeResource AmberBrush}"
   31                      BorderThickness="2,0,0,0"
   32                      Padding="12,2,0,2"
   33                      Visibility="Collapsed">
   34                  <TextBlock x:Name="AckText"
   35                             FontFamily="{StaticResource SerifItalicFontFamily}"
   36                             FontStyle="Italic"
   37                             FontSize="13"
   38                             Foreground="{ThemeResource Ink2Brush}"
   39                             TextWrapping="Wrap" />
   40              </Border>
   41  
   42              <!-- Prompt textarea -->
   43              <TextBox x:Name="PromptInput"
   44                       Style="{StaticResource ChatInputTextBoxStyle}"
   45                       PlaceholderText="Refine, or accept what's drawn…"
   46                       AcceptsReturn="True"
   47                       TextWrapping="Wrap"
   48                       MinHeight="68"
   49                       AutomationProperties.Name="Composer prompt"
   50                       KeyDown="OnPromptKeyDown"
   51                       TextChanged="OnPromptTextChanged" />
   52  
   53              <!-- TRY label + suggestion chips on the left; kbd hint on the right.
   54                   Per prototype: chips appear BELOW the textarea, not above. -->
   55              <Grid>
   56                  <Grid.ColumnDefinitions>
   57                      <ColumnDefinition Width="Auto" />
   58                      <ColumnDefinition Width="*" />
   59                      <ColumnDefinition Width="Auto" />
   60                  </Grid.ColumnDefinitions>
   61                  <TextBlock Grid.Column="0"
   62                             FontFamily="{StaticResource MonoFontFamily}"
   63                             FontSize="{StaticResource TypeEyebrowSize}"
   64                             FontWeight="Medium"
   65                             CharacterSpacing="160"
   66                             Foreground="{ThemeResource Ink3Brush}"
   67                             VerticalAlignment="Center"
   68                             Margin="0,0,12,0"
   69                             Text="TRY" />
   70                  <utu:AutoLayout Grid.Column="1"
   71                                  x:Name="ChipsRow"
   72                                  Orientation="Horizontal"
   73                                  Spacing="8"
   74                                  VerticalAlignment="Center" />
   75                  <TextBlock Grid.Column="2"
   76                             x:Name="KbdHint"
   77                             FontFamily="{StaticResource MonoFontFamily}"
   78                             FontSize="{StaticResource TypeEyebrowMicroSize}"
   79                             CharacterSpacing="180"
   80                             Foreground="{ThemeResource Ink4Brush}"
   81                             VerticalAlignment="Center"
   82                             Text="⌘  ↵  TO SUBMIT" />
   83              </Grid>
   84  
   85              <!-- Action row — primary on the left, italic hint to the right,
   86                   discard links flow on the right edge. Per prototype the
   87                   italic hint reframes the primary action ("accepting the
   88                   recommendation" in Clean state). -->
   89              <Grid>
   90                  <Grid.ColumnDefinitions>
   91                      <ColumnDefinition Width="Auto" />
   92                      <ColumnDefinition Width="Auto" />
   93                      <ColumnDefinition Width="*" />
   94                      <ColumnDefinition Width="Auto" />
   95                  </Grid.ColumnDefinitions>
   96                  <Button Grid.Column="0"
   97                          x:Name="PrimaryButton"
   98                          Style="{StaticResource InkButtonStyle}"
   99                          Content="Lock and continue →"
  100                          VerticalAlignment="Center"
  101                          AutomationProperties.Name="Lock and continue"
  102                          Click="OnPrimaryClick" />
  103                  <TextBlock Grid.Column="1"
  104                             x:Name="PrimaryHintText"
  105                             FontFamily="{StaticResource SerifLightItalicFontFamily}"
  106                             FontSize="{StaticResource TypeBodySmallSize}"
  107                             Foreground="{ThemeResource Ink3Brush}"
  108                             VerticalAlignment="Center"
  109                             Margin="14,0,0,0"
  110                             Text="accepting the recommendation" />
  111                  <utu:AutoLayout Grid.Column="3"
  112                                  Orientation="Horizontal"
  113                                  Spacing="14"
  114                                  HorizontalAlignment="Right">
  115                      <Button x:Name="DiscardEditsButton"
  116                              Style="{StaticResource LinkButtonStyle}"
  117                              Content="Discard edits"
  118                              Padding="6,10"
  119                              VerticalAlignment="Center"
  120                              Visibility="Collapsed"
  121                              AutomationProperties.Name="Discard edits"
  122                              Click="OnDiscardEditsClick" />
  123                      <Button x:Name="DiscardPreviewButton"
  124                              Style="{StaticResource LinkButtonStyle}"
  125                              Content="← Discard preview"
  126                              Padding="6,10"
  127                              VerticalAlignment="Center"
  128                              Visibility="Collapsed"
  129                              AutomationProperties.Name="Discard preview"
  130                              Click="OnDiscardPreviewClick" />
  131                  </utu:AutoLayout>
  132              </Grid>
  133          </utu:AutoLayout>
  134      </Border>
  135  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs`

```csharp
    1  using System;
    2  using Composer.Models;
    3  using Composer.Views;
    4  using Microsoft.UI.Xaml;
    5  using Microsoft.UI.Xaml.Controls;
    6  using Microsoft.UI.Xaml.Input;
    7  using Microsoft.UI.Xaml.Media;
    8  using Windows.System;
    9  
   10  namespace Composer.Views.Controls;
   11  
   12  /// <summary>
   13  /// Bottom-of-canvas composer per v11 interaction brief §5–§6.
   14  /// Three states drive eyebrow word, primary action, and acknowledgment line:
   15  ///   Clean      → REFINING   · Lock and continue → (Continue → on Intent)
   16  ///   Dirty      → LISTENING  · Generate preview →
   17  ///   Previewing → PROPOSING  · Accept and lock →  (+ Discard preview)
   18  ///
   19  /// Suggestion chips row is always visible. Click sets the prompt + marks
   20  /// dirty + focuses the textarea. Cmd/Ctrl+Enter routes to the primary
   21  /// action; Esc in Previewing discards the preview.
   22  /// </summary>
   23  public sealed partial class ComposerFooter : UserControl
   24  {
   25      public static readonly DependencyProperty StateProperty =
   26          DependencyProperty.Register(
   27              nameof(State), typeof(LayerState), typeof(ComposerFooter),
   28              new PropertyMetadata(LayerState.Clean, OnStateChanged));
   29  
   30      public static readonly DependencyProperty PromptProperty =
   31          DependencyProperty.Register(
   32              nameof(Prompt), typeof(string), typeof(ComposerFooter),
   33              new PropertyMetadata(string.Empty, OnPromptChanged));
   34  
   35      public LayerState State
   36      {
   37          get => (LayerState)GetValue(StateProperty);
   38          set => SetValue(StateProperty, value);
   39      }
   40  
   41      public string Prompt
   42      {
   43          get => (string)GetValue(PromptProperty);
   44          set => SetValue(PromptProperty, value);
   45      }
   46  
   47      private bool _suppressPromptCallback;
   48  
   49      public ComposerFooter()
   50      {
   51          this.InitializeComponent();
   52          this.Loaded             += (_, _) => { RenderChips(); ApplyState(); SyncLeadQuestion(); };
   53          this.DataContextChanged += (_, _) => { RenderChips(); ApplyState(); SyncLeadQuestion(); };
   54      }
   55  
   56      private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   57      {
   58          if (d is ComposerFooter f)
   59          {
   60              f.RenderChips();
   61              f.ApplyState();
   62              f.SyncLeadQuestion();
   63          }
   64      }
   65  
   66      private static void OnPromptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   67      {
   68          if (d is ComposerFooter f && !f._suppressPromptCallback)
   69          {
   70              f._suppressPromptCallback = true;
   71              f.PromptInput.Text = (string?)e.NewValue ?? string.Empty;
   72              f._suppressPromptCallback = false;
   73          }
   74      }
   75  
   76      private LayerKind ActiveLayerKind()
   77      {
   78          var dc = DataContext;
   79          if (dc is null) return LayerKind.Intent;
   80          var idx = (dc.GetType().GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;
   81          var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
   82          return Composer.Models.Layers.All[clamped].Kind;
   83      }
   84  
   85      private void ApplyState()
   86      {
   87          var kind = ActiveLayerKind();
   88          // Eyebrow word from ComposerStatus per brief.
   89          EyebrowText.Text = $"COMPOSER · {ComposerStatus.ForLayerState(State)}";
   90  
   91          // Italic hint next to the primary button — reframes the click per
   92          // the current state. Matches ShellModel.ActivePrimaryHint copy.
   93          PrimaryHintText.Text = State switch
   94          {
   95              LayerState.Clean      => "accepting the recommendation",
   96              LayerState.Dirty      => "with your edits",
   97              LayerState.Previewing => "the AI's pass",
   98              _                     => string.Empty,
   99          };
  100  
  101          // First-layer label is "Continue →" — there's nothing to lock when
  102          // the user is just confirming the recommendation. Intent is layer 0
  103          // (matches the prototype's cold-launch surface and softened first-
  104          // layer label per composer-context-engine.jsx line 36).
  105          var firstLayerCleanLabel = kind == LayerKind.Intent
  106              ? "Continue →"
  107              : "Lock and continue →";
  108  
  109          switch (State)
  110          {
  111              case LayerState.Clean:
  112                  PrimaryButton.Content = firstLayerCleanLabel;
  113                  PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
  114                  DiscardEditsButton.Visibility   = Visibility.Collapsed;
  115                  DiscardPreviewButton.Visibility = Visibility.Collapsed;
  116                  AckLine.Visibility = Visibility.Collapsed;
  117                  break;
  118              case LayerState.Dirty:
  119                  PrimaryButton.Content = "Generate preview →";
  120                  PrimaryButton.Style = (Style)Application.Current.Resources["AmberButtonStyle"];
  121                  DiscardEditsButton.Visibility   = Visibility.Visible;
  122                  DiscardPreviewButton.Visibility = Visibility.Collapsed;
  123                  AckLine.Visibility = Visibility.Collapsed;
  124                  break;
  125              case LayerState.Previewing:
  126                  PrimaryButton.Content = "Accept and lock →";
  127                  PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
  128                  DiscardEditsButton.Visibility   = Visibility.Collapsed;
  129                  DiscardPreviewButton.Visibility = Visibility.Visible;
  130                  ApplyAck(kind);
  131                  break;
  132              case LayerState.Locked:
  133                  PrimaryButton.Content = firstLayerCleanLabel;
  134                  PrimaryButton.Style = (Style)Application.Current.Resources["InkButtonStyle"];
  135                  DiscardEditsButton.Visibility   = Visibility.Collapsed;
  136                  DiscardPreviewButton.Visibility = Visibility.Collapsed;
  137                  AckLine.Visibility = Visibility.Collapsed;
  138                  break;
  139          }
  140      }
  141  
  142      /// <summary>Read PreviewAcks[kind] off the model and surface the
  143      /// quote above the primary action. Hides if the ack is empty.</summary>
  144      private void ApplyAck(LayerKind kind)
  145      {
  146          var dc = DataContext;
  147          if (dc is null) { AckLine.Visibility = Visibility.Collapsed; return; }
  148          var acksProp = dc.GetType().GetProperty("PreviewAcks")?.GetValue(dc);
  149          var dict = Composer.Views.MvuxValueReader.Unwrap<
  150              System.Collections.Immutable.IImmutableDictionary<LayerKind, string>>(acksProp);
  151          if (dict is null && acksProp is System.Collections.Immutable.IImmutableDictionary<LayerKind, string> direct)
  152              dict = direct;
  153          if (dict is null || !dict.TryGetValue(kind, out var quote) || string.IsNullOrWhiteSpace(quote))
  154          {
  155              AckLine.Visibility = Visibility.Collapsed;
  156              return;
  157          }
  158          AckText.Text = $"You asked: “{quote}” — here's what changes if I apply that.";
  159          AckLine.Visibility = Visibility.Visible;
  160      }
  161  
  162      /// <summary>Populate the chip row from ComposerModel.SuggestionChips for
  163      /// the active layer. Click sets the prompt + marks dirty + focuses.</summary>
  164      private void RenderChips()
  165      {
  166          ChipsRow.Children.Clear();
  167          var kind = ActiveLayerKind();
  168          if (!ComposerModel.SuggestionChips.TryGetValue(kind, out var chips)) return;
  169  
  170          var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
  171          foreach (var chip in chips)
  172          {
  173              var btn = new Button
  174              {
  175                  Content = chip,
  176                  FontFamily = monoFont,
  177                  FontSize = 11,
  178                  FontWeight = Microsoft.UI.Text.FontWeights.Medium,
  179                  CharacterSpacing = 40,
  180                  Background = (Brush)Application.Current.Resources["TransparentBrush"],
  181                  BorderBrush = (Brush)Application.Current.Resources["HairlineBrush"],
  182                  BorderThickness = new Thickness(1),
  183                  CornerRadius = new CornerRadius(4),
  184                  Padding = new Thickness(10, 5, 10, 5),
  185                  Foreground = (Brush)Application.Current.Resources["Ink2Brush"],
  186                  Tag = chip,
  187              };
  188              btn.Click += OnChipClick;
  189              ChipsRow.Children.Add(btn);
  190          }
  191      }
  192  
  193      private void OnChipClick(object sender, RoutedEventArgs e)
  194      {
  195          if (sender is not Button { Tag: string chip }) return;
  196          // Set the prompt textbox + write through to the model. Caret moves
  197          // to the end so the user can extend the chip text immediately.
  198          _suppressPromptCallback = true;
  199          PromptInput.Text = chip;
  200          PromptInput.SelectionStart = chip.Length;
  201          _suppressPromptCallback = false;
  202          PromptInput.Focus(FocusState.Programmatic);
  203  
  204          var page = FindParent<Page>(this);
  205          MvuxCommandInvoker.Invoke(page?.DataContext, "SetActivePrompt", chip);
  206      }
  207  
  208      private void OnPromptKeyDown(object sender, KeyRoutedEventArgs e)
  209      {
  210          // Esc in Previewing → DiscardPreview (per v11 interaction brief).
  211          if (e.Key == VirtualKey.Escape && State == LayerState.Previewing)
  212          {
  213              e.Handled = true;
  214              var page = FindParent<Page>(this);
  215              MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardPreview");
  216              return;
  217          }
  218  
  219          // Cmd/Ctrl+Enter → primary action.
  220          if (e.Key != VirtualKey.Enter) return;
  221          var ctrl = Microsoft.UI.Input.InputKeyboardSource
  222              .GetKeyStateForCurrentThread(VirtualKey.Control);
  223          if ((ctrl & Windows.UI.Core.CoreVirtualKeyStates.Down) != Windows.UI.Core.CoreVirtualKeyStates.Down)
  224              return;
  225          e.Handled = true;
  226          TriggerPrimary();
  227      }
  228  
  229      private void OnPromptTextChanged(object sender, TextChangedEventArgs e)
  230      {
  231          if (_suppressPromptCallback) return;
  232          var page = FindParent<Page>(this);
  233          MvuxCommandInvoker.Invoke(page?.DataContext, "SetActivePrompt", PromptInput.Text);
  234      }
  235  
  236      private void OnPrimaryClick(object sender, RoutedEventArgs e) => TriggerPrimary();
  237  
  238      private void TriggerPrimary()
  239      {
  240          var page = FindParent<Page>(this);
  241          if (page?.DataContext is null) return;
  242  
  243          var resolved = ResolveCurrentState(page.DataContext);
  244          if (resolved == LayerState.Clean && !string.IsNullOrWhiteSpace(PromptInput.Text))
  245              resolved = LayerState.Dirty;
  246  
  247          var commandName = resolved switch
  248          {
  249              LayerState.Clean       => "LockAndContinue",
  250              LayerState.Dirty       => "GeneratePreview",
  251              LayerState.Previewing  => "AcceptAndLock",
  252              LayerState.Locked      => "LockAndContinue",
  253              _                      => "LockAndContinue",
  254          };
  255          MvuxCommandInvoker.Invoke(page.DataContext, commandName);
  256          if (resolved == LayerState.Clean || resolved == LayerState.Previewing)
  257          {
  258              _suppressPromptCallback = true;
  259              PromptInput.Text = string.Empty;
  260              _suppressPromptCallback = false;
  261          }
  262      }
  263  
  264      private static LayerState ResolveCurrentState(object dc)
  265      {
  266          var t = dc.GetType();
  267          var idx = (t.GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;
  268          var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
  269          var activeKind = Composer.Models.Layers.All[clamped].Kind;
  270  
  271          var statesObj = t.GetProperty("LayerStates")?.GetValue(dc);
  272          if (statesObj is System.Collections.Generic.IDictionary<LayerKind, LayerState> states
  273              && states.TryGetValue(activeKind, out var s))
  274              return s;
  275          return LayerState.Clean;
  276      }
  277  
  278      private void OnDiscardEditsClick(object sender, RoutedEventArgs e)
  279      {
  280          var page = FindParent<Page>(this);
  281          MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardEdits");
  282          _suppressPromptCallback = true;
  283          PromptInput.Text = string.Empty;
  284          _suppressPromptCallback = false;
  285      }
  286  
  287      private void OnDiscardPreviewClick(object sender, RoutedEventArgs e)
  288      {
  289          var page = FindParent<Page>(this);
  290          MvuxCommandInvoker.Invoke(page?.DataContext, "DiscardPreview");
  291      }
  292  
  293      /// <summary>Read <c>ActiveLeadQuestion</c> off the bound model and apply
  294      /// to the lead-question TextBlock. Same reflection pattern as ApplyAck.
  295      /// Hidden if the layer doesn't define a lead question (e.g., Scaffold
  296      /// where the footer doesn't render anyway).</summary>
  297      private void SyncLeadQuestion()
  298      {
  299          var dc = DataContext;
  300          if (dc is null) return;
  301          var raw = dc.GetType().GetProperty("ActiveLeadQuestion")?.GetValue(dc);
  302          var text = raw switch
  303          {
  304              string s => s,
  305              null     => null,
  306              _        => Composer.Views.MvuxValueReader.Unwrap<string>(raw),
  307          };
  308          if (string.IsNullOrWhiteSpace(text))
  309          {
  310              LeadQuestionText.Visibility = Visibility.Collapsed;
  311              LeadQuestionText.Text = string.Empty;
  312          }
  313          else
  314          {
  315              LeadQuestionText.Text = text!;
  316              LeadQuestionText.Visibility = Visibility.Visible;
  317          }
  318      }
  319  
  320      private static T? FindParent<T>(DependencyObject child) where T : class
  321      {
  322          var parent = VisualTreeHelper.GetParent(child);
  323          while (parent is not null && parent is not T)
  324              parent = VisualTreeHelper.GetParent(parent);
  325          return parent as T;
  326      }
  327  }
```

### `Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml`

```xml
    1  ﻿<UserControl x:Class="Composer.Views.Controls.FuturePreviewCard"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI"
    5               IsHitTestVisible="False">
    6  
    7      <!--
    8        Future preview card — design brief §2.4. Flat, no background fill,
    9        dashed hairline outline. Renders the upcoming layer's label + hint
   10        so the user sees the cumulative composition stack below the active
   11        canvas at progressively decreasing opacity.
   12      -->
   13      <Grid Padding="20,14">
   14          <!-- Dashed outline. WinUI/Uno's Border.BorderBrush doesn't support
   15               stroke-dash, so we lay a Rectangle with StrokeDashArray below
   16               the content. -->
   17          <Rectangle Stroke="{ThemeResource HairlineBrush}"
   18                     StrokeThickness="1"
   19                     StrokeDashArray="3,3"
   20                     RadiusX="4"
   21                     RadiusY="4"
   22                     Fill="Transparent" />
   23          <utu:AutoLayout Orientation="Vertical" Spacing="3">
   24              <TextBlock x:Name="HeaderText"
   25                         FontFamily="{StaticResource MonoFontFamily}"
   26                         FontSize="{StaticResource TypeEyebrowSize}"
   27                         CharacterSpacing="160"
   28                         Foreground="{ThemeResource Ink3Brush}" />
   29              <TextBlock x:Name="HintText"
   30                         FontFamily="{StaticResource SerifItalicFontFamily}"
   31                         FontStyle="Italic"
   32                         FontSize="{StaticResource TypeBodySmallSize}"
   33                         Foreground="{ThemeResource Ink3Brush}"
   34                         TextWrapping="Wrap" />
   35          </utu:AutoLayout>
   36      </Grid>
   37  </UserControl>
```

### `Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml.cs`

```csharp
    1  using Microsoft.UI.Xaml;
    2  using Microsoft.UI.Xaml.Controls;
    3  
    4  namespace Composer.Views.Controls;
    5  
    6  /// <summary>
    7  /// Brief 03 §3 — flat preview card for an upcoming layer. Stacks below the
    8  /// active canvas with progressive opacity. Pointer events disabled — these
    9  /// are read-only previews.
   10  /// </summary>
   11  public sealed partial class FuturePreviewCard : UserControl
   12  {
   13      public static readonly DependencyProperty LayerLabelProperty =
   14          DependencyProperty.Register(
   15              nameof(LayerLabel), typeof(string), typeof(FuturePreviewCard),
   16              new PropertyMetadata(string.Empty, OnLabelOrHintChanged));
   17  
   18      public static readonly DependencyProperty HintProperty =
   19          DependencyProperty.Register(
   20              nameof(Hint), typeof(string), typeof(FuturePreviewCard),
   21              new PropertyMetadata(string.Empty, OnLabelOrHintChanged));
   22  
   23      public string LayerLabel
   24      {
   25          get => (string)GetValue(LayerLabelProperty);
   26          set => SetValue(LayerLabelProperty, value);
   27      }
   28  
   29      public string Hint
   30      {
   31          get => (string)GetValue(HintProperty);
   32          set => SetValue(HintProperty, value);
   33      }
   34  
   35      public FuturePreviewCard() => this.InitializeComponent();
   36  
   37      private static void OnLabelOrHintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   38      {
   39          if (d is FuturePreviewCard c) c.Render();
   40      }
   41  
   42      private void Render()
   43      {
   44          HeaderText.Text = $"{LayerLabel}  ·  UPCOMING";
   45          HintText.Text   = Hint;
   46      }
   47  }
```

### `Composer/src/Composer/Composer/Shell.xaml.cs`

```csharp
    1  using System.ComponentModel;
    2  using Composer.Models;
    3  using Composer.Services;
    4  using Composer.Views;
    5  using Microsoft.Extensions.DependencyInjection;
    6  using Microsoft.Extensions.Options;
    7  using Microsoft.UI.Xaml;
    8  using Microsoft.UI.Xaml.Controls;
    9  using Microsoft.UI.Xaml.Media;
   10  using Microsoft.UI.Xaml.Media.Animation;
   11  
   12  namespace Composer;
   13  
   14  /// <summary>
   15  /// Shell host for the context-engine composer. Three-column workspace: left
   16  /// rail (composition stack), center (active layer canvas), right rail (live
   17  /// files). The center column is the surface that will become a
   18  /// <c>uen:Region.Attached</c> navigation region in the next pass (per
   19  /// <c>docs/ENGINEERING-BRIEF-page-and-flow-breakdown.md</c> §2.2). Today it
   20  /// hosts <see cref="Composer.Views.Controls.ActiveCanvas"/>, which dispatches
   21  /// to per-layer canvas UserControls based on the active layer index.
   22  /// </summary>
   23  public sealed partial class Shell : Page
   24  {
   25      private INotifyPropertyChanged? _vm;
   26      private bool _railsVisibleApplied;
   27  
   28      public Shell()
   29      {
   30          this.InitializeComponent();
   31          this.Loaded   += OnLoaded;
   32          this.Unloaded += OnUnloaded;
   33      }
   34  
   35      private void OnLoaded(object sender, RoutedEventArgs e)
   36      {
   37          var services = App.Host?.Services;
   38          if (services is null) return;
   39  
   40          DataContext = new ComposerViewModel(
   41              services.GetRequiredService<Composer.Models.Presentation.ShellModel>(),
   42              services.GetRequiredService<Composer.Models.Presentation.IntentModel>(),
   43              services.GetRequiredService<Composer.Models.Presentation.UXModel>(),
   44              services.GetRequiredService<Composer.Models.Presentation.ArchitectureModel>(),
   45              services.GetRequiredService<Composer.Models.Presentation.DesignModel>(),
   46              services.GetRequiredService<Composer.Models.Presentation.InteractionsModel>(),
   47              services.GetRequiredService<Composer.Models.Presentation.DataModel>(),
   48              services.GetRequiredService<Composer.Models.Presentation.ImplementationModel>(),
   49              services.GetRequiredService<IBundleExporter>(),
   50              services.GetRequiredService<IUnoSdkVersionService>(),
   51              services.GetRequiredService<ILayerPreviewService>(),
   52              services.GetRequiredService<IContextDeriver>(),
   53              services.GetRequiredService<IOptions<AnthropicConfig>>());
   54  
   55          if (DataContext is INotifyPropertyChanged inpc)
   56          {
   57              _vm = inpc;
   58              _vm.PropertyChanged += OnVmPropertyChanged;
   59          }
   60  
   61          // Initial sync — the model starts with RailsVisible=false (Stack
   62          // canvas, no locks, ActiveIndex=0). This is a no-op for the storyboard
   63          // but locks in the rails-applied flag so the first true→ transition
   64          // animates correctly.
   65          SyncRailsAnimation();
   66      }
   67  
   68      private void OnUnloaded(object sender, RoutedEventArgs e)
   69      {
   70          if (_vm is null) return;
   71          _vm.PropertyChanged -= OnVmPropertyChanged;
   72          _vm = null;
   73      }
   74  
   75      private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
   76      {
   77          if (e.PropertyName == "RailsVisible")
   78              DispatcherQueue?.TryEnqueue(SyncRailsAnimation);
   79      }
   80  
   81      private void SyncRailsAnimation()
   82      {
   83          var dc = DataContext;
   84          if (dc is null) return;
   85          var visible = (dc.GetType().GetProperty("RailsVisible")?.GetValue(dc) as bool?) ?? false;
   86  
   87          // Skip if no transition.
   88          if (visible == _railsVisibleApplied) return;
   89          _railsVisibleApplied = visible;
   90  
   91          // Snap the column widths — Grid columns don't smoothly re-measure
   92          // under DoubleAnimation on Skia desktop, so instead we animate the
   93          // rail content (translate + opacity) inside fixed columns.
   94          var width = visible ? new GridLength(280, GridUnitType.Pixel)
   95                              : new GridLength(0,   GridUnitType.Pixel);
   96          LeftRailColumn.Width  = width;
   97          RightRailColumn.Width = width;
   98  
   99          // Reduced motion: snap content as well.
  100          if (!Composer.Views.MotionPreferences.AnimationsEnabled)
  101          {
  102              LeftRailContainer.Opacity  = visible ? 1 : 0;
  103              RightRailContainer.Opacity = visible ? 1 : 0;
  104              ((TranslateTransform)LeftRailContainer.RenderTransform).X  = visible ?  0 : -40;
  105              ((TranslateTransform)RightRailContainer.RenderTransform).X = visible ?  0 :  40;
  106              return;
  107          }
  108  
  109          var key = visible ? "RailsRevealStoryboard" : "RailsHideStoryboard";
  110          if (Resources[key] is Storyboard sb) sb.Begin();
  111      }
  112  }
```

### `Composer/src/Composer/Composer/Themes/Tokens.xaml`

```xml
    1  <ResourceDictionary
    2      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    4  
    5      <!--
    6          Theme-aware tokens.
    7          ===================
    8          Per Uno Platform / WinUI conventions, every theme-aware Color and Brush
    9          lives inside a ThemeDictionary so the system can switch palettes when
   10          the OS theme flips. Consumers reach these via {ThemeResource X}, never
   11          {StaticResource X}, so the runtime can re-resolve on theme change.
   12  
   13          Theme-INVARIANT tokens (spacing, radii, type sizes, easings, durations)
   14          live at the root of this dictionary — they don't change with theme.
   15          -->
   16  
   17      <ResourceDictionary.ThemeDictionaries>
   18  
   19          <!-- ========================================================== -->
   20          <!-- LIGHT THEME (Default)                                       -->
   21          <!-- The editorial monochromatic palette Composer ships with.   -->
   22          <!-- Paper-on-ink, no Material-style state overlays — state     -->
   23          <!-- shifts step along the lightness ramp via InkHover/Pressed. -->
   24          <!-- ========================================================== -->
   25          <ResourceDictionary x:Key="Default">
   26  
   27              <Color x:Key="PaperColor">#FFFFFF</Color>
   28              <Color x:Key="Paper2Color">#FAFAFA</Color>
   29              <Color x:Key="Paper3Color">#F4F4F5</Color>
   30              <!-- Paper4 is the dark-code surface (always dark — even on light
   31                   theme code blocks need an editor-window look). Brief §4.2. -->
   32              <Color x:Key="Paper4Color">#0A0A0A</Color>
   33  
   34              <!-- Editorial ink ramp per design brief §1.3 — desaturated, more
   35                   paper-print than Material baseline. Ink lifts to #1A1A1A so
   36                   prose has weight without reading as black-on-white shock. -->
   37              <Color x:Key="InkColor">#1A1A1A</Color>
   38              <Color x:Key="InkHoverColor">#2A2A2A</Color>
   39              <Color x:Key="InkPressedColor">#0E0E0E</Color>
   40  
   41              <Color x:Key="Ink2Color">#3A3A3A</Color>
   42              <Color x:Key="Ink3Color">#737373</Color>
   43              <Color x:Key="Ink4Color">#A3A3A3</Color>
   44              <!-- Ink5 — very low-emphasis chrome (BlockHandle drag affordance). -->
   45              <Color x:Key="Ink5Color">#D4D4D4</Color>
   46  
   47              <Color x:Key="HairlineColor">#ECECEC</Color>
   48              <Color x:Key="Hairline2Color">#F0F0F0</Color>
   49              <Color x:Key="HairlineDkColor">#1F1F1F</Color>
   50  
   51              <Color x:Key="ErrorBgColor">#FEE2E2</Color>
   52              <Color x:Key="ErrorBorderColor">#DC2626</Color>
   53              <Color x:Key="ErrorTextColor">#7F1D1D</Color>
   54  
   55              <Color x:Key="CheckGreenColor">#16A34A</Color>
   56              <Color x:Key="UnoBrandColor">#3D3DFF</Color>
   57              <!-- Amber — editorial gold per brief, not Material orange.
   58                   Active marker + dirty/preview badge — ONLY usage. -->
   59              <Color x:Key="AmberColor">#C89C3F</Color>
   60              <Color x:Key="AmberSoftColor">#FDF8EF</Color>
   61              <Color x:Key="IndigoColor">#3D3DFF</Color>
   62  
   63              <SolidColorBrush x:Key="PaperBrush"  Color="{StaticResource PaperColor}" />
   64              <SolidColorBrush x:Key="Paper2Brush" Color="{StaticResource Paper2Color}" />
   65              <SolidColorBrush x:Key="Paper3Brush" Color="{StaticResource Paper3Color}" />
   66              <SolidColorBrush x:Key="Paper4Brush" Color="{StaticResource Paper4Color}" />
   67  
   68              <!-- Glass surface treatment — soft pale-blue / cream gradient
   69                   behind a translucent white panel. The backdrop gives the
   70                   panel something to filter through; the panel reads as glass
   71                   because it sits at ~78% white over the gradient. Subtle
   72                   white-on-white border softens the panel edge. -->
   73              <LinearGradientBrush x:Key="GlassBackdropBrush" StartPoint="0,0" EndPoint="1,1">
   74                  <GradientStop Color="#FFE3ECF7" Offset="0.0" />
   75                  <GradientStop Color="#FFF1F4F9" Offset="0.5" />
   76                  <GradientStop Color="#FFFAF8F1" Offset="1.0" />
   77              </LinearGradientBrush>
   78              <SolidColorBrush x:Key="GlassPanelBrush"         Color="#C8FFFFFF" />
   79              <SolidColorBrush x:Key="GlassPanelBorderBrush"   Color="#33FFFFFF" />
   80              <SolidColorBrush x:Key="GlassPanelHairlineBrush" Color="#1A000000" />
   81  
   82              <SolidColorBrush x:Key="InkBrush"        Color="{StaticResource InkColor}" />
   83              <SolidColorBrush x:Key="InkHoverBrush"   Color="{StaticResource InkHoverColor}" />
   84              <SolidColorBrush x:Key="InkPressedBrush" Color="{StaticResource InkPressedColor}" />
   85              <SolidColorBrush x:Key="Ink2Brush"       Color="{StaticResource Ink2Color}" />
   86              <SolidColorBrush x:Key="Ink3Brush"       Color="{StaticResource Ink3Color}" />
   87              <SolidColorBrush x:Key="Ink4Brush"       Color="{StaticResource Ink4Color}" />
   88              <SolidColorBrush x:Key="Ink5Brush"       Color="{StaticResource Ink5Color}" />
   89  
   90              <SolidColorBrush x:Key="HairlineBrush"   Color="{StaticResource HairlineColor}" />
   91              <SolidColorBrush x:Key="Hairline2Brush"  Color="{StaticResource Hairline2Color}" />
   92              <SolidColorBrush x:Key="HairlineDkBrush" Color="{StaticResource HairlineDkColor}" />
   93  
   94              <SolidColorBrush x:Key="ErrorBgBrush"     Color="{StaticResource ErrorBgColor}" />
   95              <SolidColorBrush x:Key="ErrorBorderBrush" Color="{StaticResource ErrorBorderColor}" />
   96              <SolidColorBrush x:Key="ErrorTextBrush"   Color="{StaticResource ErrorTextColor}" />
   97  
   98              <SolidColorBrush x:Key="CheckGreenBrush" Color="{StaticResource CheckGreenColor}" />
   99              <SolidColorBrush x:Key="UnoBrandBrush"   Color="{StaticResource UnoBrandColor}" />
  100              <SolidColorBrush x:Key="AmberBrush"      Color="{StaticResource AmberColor}" />
  101              <SolidColorBrush x:Key="AmberSoftBrush"  Color="{StaticResource AmberSoftColor}" />
  102              <SolidColorBrush x:Key="IndigoBrush"     Color="{StaticResource IndigoColor}" />
  103  
  104          </ResourceDictionary>
  105  
  106          <!-- ========================================================== -->
  107          <!-- DARK THEME                                                  -->
  108          <!-- Paper/Ink invert. Soft-black surfaces (not #000000 — the   -->
  109          <!-- design.md §1.4 rule about OLED comfort applies here too).  -->
  110          <!-- ========================================================== -->
  111          <ResourceDictionary x:Key="Dark">
  112  
  113              <Color x:Key="PaperColor">#141418</Color>
  114              <Color x:Key="Paper2Color">#1B1B1F</Color>
  115              <Color x:Key="Paper3Color">#26262B</Color>
  116              <!-- Paper4 stays dark — it's the code-block surface in both
  117                   themes (editor-window look is theme-invariant). -->
  118              <Color x:Key="Paper4Color">#0A0A0A</Color>
  119  
  120              <Color x:Key="InkColor">#FAFAFA</Color>
  121              <Color x:Key="InkHoverColor">#E4E2E6</Color>
  122              <Color x:Key="InkPressedColor">#FFFFFF</Color>
  123  
  124              <!-- On dark surfaces the secondary tones fade toward Ink, not
  125                   darken — same role (lower contrast than primary) but the
  126                   direction is inverted. -->
  127              <Color x:Key="Ink2Color">#C7C5D0</Color>
  128              <Color x:Key="Ink3Color">#918F9A</Color>
  129              <Color x:Key="Ink4Color">#777680</Color>
  130              <Color x:Key="Ink5Color">#5C5C66</Color>
  131  
  132              <Color x:Key="HairlineColor">#393940</Color>
  133              <Color x:Key="Hairline2Color">#2E2E33</Color>
  134              <Color x:Key="HairlineDkColor">#1F1F1F</Color>
  135  
  136              <Color x:Key="ErrorBgColor">#3F1414</Color>
  137              <Color x:Key="ErrorBorderColor">#F87171</Color>
  138              <Color x:Key="ErrorTextColor">#FCA5A5</Color>
  139  
  140              <!-- Brand colors stay constant across themes — they're identity,
  141                   not surface chrome. Amber gets a small bump in saturation so
  142                   it remains visible against the dark Paper. -->
  143              <Color x:Key="CheckGreenColor">#34D399</Color>
  144              <Color x:Key="UnoBrandColor">#A8B0F4</Color>
  145              <Color x:Key="AmberColor">#F59E0B</Color>
  146              <!-- AmberSoft on dark theme — darker amber-tint so the proposed
  147                   backdrop reads against the dark surface without overwhelming. -->
  148              <Color x:Key="AmberSoftColor">#3F2D14</Color>
  149              <Color x:Key="IndigoColor">#A8B0F4</Color>
  150  
  151              <SolidColorBrush x:Key="PaperBrush"  Color="{StaticResource PaperColor}" />
  152              <SolidColorBrush x:Key="Paper2Brush" Color="{StaticResource Paper2Color}" />
  153              <SolidColorBrush x:Key="Paper3Brush" Color="{StaticResource Paper3Color}" />
  154              <SolidColorBrush x:Key="Paper4Brush" Color="{StaticResource Paper4Color}" />
  155  
  156              <SolidColorBrush x:Key="InkBrush"        Color="{StaticResource InkColor}" />
  157              <SolidColorBrush x:Key="InkHoverBrush"   Color="{StaticResource InkHoverColor}" />
  158              <SolidColorBrush x:Key="InkPressedBrush" Color="{StaticResource InkPressedColor}" />
  159              <SolidColorBrush x:Key="Ink2Brush"       Color="{StaticResource Ink2Color}" />
  160              <SolidColorBrush x:Key="Ink3Brush"       Color="{StaticResource Ink3Color}" />
  161              <SolidColorBrush x:Key="Ink4Brush"       Color="{StaticResource Ink4Color}" />
  162              <SolidColorBrush x:Key="Ink5Brush"       Color="{StaticResource Ink5Color}" />
  163  
  164              <SolidColorBrush x:Key="HairlineBrush"   Color="{StaticResource HairlineColor}" />
  165              <SolidColorBrush x:Key="Hairline2Brush"  Color="{StaticResource Hairline2Color}" />
  166              <SolidColorBrush x:Key="HairlineDkBrush" Color="{StaticResource HairlineDkColor}" />
  167  
  168              <SolidColorBrush x:Key="ErrorBgBrush"     Color="{StaticResource ErrorBgColor}" />
  169              <SolidColorBrush x:Key="ErrorBorderBrush" Color="{StaticResource ErrorBorderColor}" />
  170              <SolidColorBrush x:Key="ErrorTextBrush"   Color="{StaticResource ErrorTextColor}" />
  171  
  172              <SolidColorBrush x:Key="CheckGreenBrush" Color="{StaticResource CheckGreenColor}" />
  173              <SolidColorBrush x:Key="UnoBrandBrush"   Color="{StaticResource UnoBrandColor}" />
  174              <SolidColorBrush x:Key="AmberBrush"      Color="{StaticResource AmberColor}" />
  175              <SolidColorBrush x:Key="AmberSoftBrush"  Color="{StaticResource AmberSoftColor}" />
  176              <SolidColorBrush x:Key="IndigoBrush"     Color="{StaticResource IndigoColor}" />
  177  
  178          </ResourceDictionary>
  179  
  180      </ResourceDictionary.ThemeDictionaries>
  181  
  182      <!-- ============================================================== -->
  183      <!-- Theme-INVARIANT tokens                                         -->
  184      <!-- Stay at the root — they don't switch with theme.               -->
  185      <!-- ============================================================== -->
  186  
  187      <SolidColorBrush x:Key="TransparentBrush" Color="Transparent" />
  188  
  189      <!-- ========================================================== -->
  190      <!-- Phase tints (Implementation canvas only)                    -->
  191      <!-- Brand-stable across themes per design brief §1.3.           -->
  192      <!-- ========================================================== -->
  193      <SolidColorBrush x:Key="PhaseRedBrush"    Color="#B04534" />  <!-- SCAFFOLD -->
  194      <SolidColorBrush x:Key="PhaseBlueBrush"   Color="#3D6F9A" />  <!-- SHELL -->
  195      <SolidColorBrush x:Key="PhasePurpleBrush" Color="#7D4FA0" />  <!-- DOMAIN -->
  196      <SolidColorBrush x:Key="PhasePinkBrush"   Color="#B4567D" />  <!-- SCREENS -->
  197      <SolidColorBrush x:Key="PhaseGreenBrush"  Color="#6F8068" />  <!-- STATES -->
  198      <SolidColorBrush x:Key="PhaseAmberBrush"  Color="#C89C3F" />  <!-- POLISH -->
  199  
  200      <!-- Phase1-6 numeric aliases per the v11 brief. Both naming schemes
  201           are valid; the legacy color-name keys are preserved so existing
  202           consumers keep compiling. -->
  203      <SolidColorBrush x:Key="Phase1Brush" Color="#B04534" />
  204      <SolidColorBrush x:Key="Phase2Brush" Color="#3D6F9A" />
  205      <SolidColorBrush x:Key="Phase3Brush" Color="#7D4FA0" />
  206      <SolidColorBrush x:Key="Phase4Brush" Color="#B4567D" />
  207      <SolidColorBrush x:Key="Phase5Brush" Color="#6F8068" />
  208      <SolidColorBrush x:Key="Phase6Brush" Color="#C89C3F" />
  209  
  210      <!-- ========================================================== -->
  211      <!-- State tints (Interactions canvas only)                      -->
  212      <!-- Per the canonical 6-state contract from design brief §1.3.  -->
  213      <!-- StateColor* are the brief's keys; the legacy *Brush keys     -->
  214      <!-- stay as aliases.                                            -->
  215      <!-- ========================================================== -->
  216      <SolidColorBrush x:Key="StateLoadingBrush" Color="#3D3DFF" />  <!-- system-working indigo -->
  217      <SolidColorBrush x:Key="StateSuccessBrush" Color="#7A9B6E" />  <!-- sage -->
  218      <SolidColorBrush x:Key="StateErrorBrush"   Color="#B04534" />  <!-- coral -->
  219      <SolidColorBrush x:Key="StateOfflineBrush" Color="#C89C3F" />  <!-- amber -->
  220  
  221      <SolidColorBrush x:Key="StateColorDefault" Color="#1A1A1A" />
  222      <SolidColorBrush x:Key="StateColorLoading" Color="#3D3DFF" />
  223      <SolidColorBrush x:Key="StateColorEmpty"   Color="#737373" />
  224      <SolidColorBrush x:Key="StateColorError"   Color="#B04534" />
  225      <SolidColorBrush x:Key="StateColorSuccess" Color="#7A9B6E" />
  226      <SolidColorBrush x:Key="StateColorOffline" Color="#C89C3F" />
  227  
  228      <!-- Spacing scale (Thickness for Margin/Padding) -->
  229      <Thickness x:Key="Space2">2</Thickness>
  230      <Thickness x:Key="Space4">4</Thickness>
  231      <Thickness x:Key="Space6">6</Thickness>
  232      <Thickness x:Key="Space8">8</Thickness>
  233      <Thickness x:Key="Space10">10</Thickness>
  234      <Thickness x:Key="Space12">12</Thickness>
  235      <Thickness x:Key="Space14">14</Thickness>
  236      <Thickness x:Key="Space16">16</Thickness>
  237      <Thickness x:Key="Space20">20</Thickness>
  238      <Thickness x:Key="Space24">24</Thickness>
  239      <Thickness x:Key="Space32">32</Thickness>
  240      <Thickness x:Key="Space40">40</Thickness>
  241      <Thickness x:Key="Space48">48</Thickness>
  242  
  243      <!-- Numeric spacing constants for AutoLayout.Spacing (double) -->
  244      <x:Double x:Key="Spacing4">4</x:Double>
  245      <x:Double x:Key="Spacing6">6</x:Double>
  246      <x:Double x:Key="Spacing8">8</x:Double>
  247      <x:Double x:Key="Spacing10">10</x:Double>
  248      <x:Double x:Key="Spacing12">12</x:Double>
  249      <x:Double x:Key="Spacing14">14</x:Double>
  250      <x:Double x:Key="Spacing16">16</x:Double>
  251      <x:Double x:Key="Spacing20">20</x:Double>
  252      <x:Double x:Key="Spacing24">24</x:Double>
  253      <x:Double x:Key="Spacing32">32</x:Double>
  254  
  255      <!-- Corner radii — chips/buttons reduced another 25% (from 10 → 8). -->
  256      <CornerRadius x:Key="Corner3">3</CornerRadius>
  257      <CornerRadius x:Key="Corner6">6</CornerRadius>
  258      <CornerRadius x:Key="CornerPill">8</CornerRadius>
  259  
  260      <!-- ========================================================== -->
  261      <!-- Motion — durations + easings                                -->
  262      <!-- Per design.md §7.3 / §7.4. Use Duration.* on every          -->
  263      <!-- animation so the whole motion system retunes from one place.-->
  264      <!-- ========================================================== -->
  265      <Duration x:Key="Duration.Quick">0:0:0.15</Duration>
  266      <Duration x:Key="Duration.Standard">0:0:0.25</Duration>
  267      <Duration x:Key="Duration.Expressive">0:0:0.4</Duration>
  268      <Duration x:Key="Duration.Slow">0:0:0.6</Duration>
  269  
  270      <!-- Curve-name keys (kept as aliases). -->
  271      <CubicEase       x:Key="EaseOutCubic"   EasingMode="EaseOut" />
  272      <ExponentialEase x:Key="EaseOutExpo"    EasingMode="EaseOut" Exponent="6" />
  273      <QuinticEase     x:Key="EaseInOutQuint" EasingMode="EaseInOut" />
  274  
  275      <!-- Semantic-intent keys (per design.md §7.4). The cubic-bezier
  276           control points called out in the design system aren't directly
  277           supported by Storyboard easings, so each key uses the closest
  278           built-in approximation. -->
  279      <ExponentialEase x:Key="EaseStandard"   EasingMode="EaseOut" Exponent="6" />
  280      <CubicEase       x:Key="EaseDecelerate" EasingMode="EaseOut" />
  281      <CubicEase       x:Key="EaseAccelerate" EasingMode="EaseIn" />
  282      <BackEase        x:Key="EaseEmphasized" EasingMode="EaseOut" Amplitude="0.3" />
  283  
  284  </ResourceDictionary>
```

### `Composer/src/Composer/Composer/Themes/Typography.xaml`

```xml
    1  ﻿<ResourceDictionary
    2      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    4  
    5      <!--
    6        Typeface stack — Inter + JetBrains Mono per docs/DESIGN-BRIEF.md §4.1.
    7          Inter            — display, body, every prose surface. Open-source,
    8                              commonly installed; falls back to Bahnschrift /
    9                              Segoe UI Variable on systems without it.
   10          Fraunces Italic  — bundled italic serif used for Annotation marginalia
   11                              and locked-card summary prose ("voice"). Loaded as
   12                              ms-appx:/// so it renders identically across hosts.
   13          JetBrains Mono   — eyebrows, labels, code, metadata, numeric data,
   14                              file rows.
   15  
   16        The existing Serif* keys are kept as aliases so older consumers that
   17        reference them (e.g. CompositionStack, FilesRail captions) keep
   18        compiling. SerifLightItalicFontFamily now resolves to the bundled
   19        Fraunces Italic variable face — italic-serif voice has a real face
   20        again instead of synthesizing italics on a sans family.
   21      -->
   22      <FontFamily x:Key="SansFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif</FontFamily>
   23      <FontFamily x:Key="SansLightFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift Light, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif</FontFamily>
   24      <FontFamily x:Key="SansMediumFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift SemiBold, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif</FontFamily>
   25  
   26      <FontFamily x:Key="SerifFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif</FontFamily>
   27      <FontFamily x:Key="SerifLightFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift Light, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif</FontFamily>
   28      <!-- Serif voice removed per v11 brief — italic body is Inter italic now.
   29           The bundled Fraunces TTFs stay in Assets/Fonts/Fraunces/ unused but
   30           harmless; the "Serif" keys remain as backwards-compat aliases that
   31           resolve to Inter Italic so no consumer breaks. -->
   32      <FontFamily x:Key="SerifItalicFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif</FontFamily>
   33      <FontFamily x:Key="SerifLightItalicFontFamily">ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif</FontFamily>
   34  
   35      <!-- Single variable font drives every JetBrains Mono weight (100-800).
   36           FontWeight on the consuming TextBlock selects the rendered axis. -->
   37      <FontFamily x:Key="MonoFontFamily">ms-appx:///Assets/Fonts/JetBrains_Mono/JetBrainsMono-VariableFont.ttf#JetBrains Mono, Cascadia Mono, Consolas, monospace</FontFamily>
   38      <FontFamily x:Key="MonoLightFontFamily">ms-appx:///Assets/Fonts/JetBrains_Mono/JetBrainsMono-VariableFont.ttf#JetBrains Mono, Cascadia Mono, Consolas, monospace</FontFamily>
   39  
   40      <!-- ========================================================== -->
   41      <!-- Modular type scale (ratio ~1.25)                            -->
   42      <!-- Anchor: 11px (eyebrow). Every FontSize across the app should-->
   43      <!-- pull from this scale. Single source — bump in one place,    -->
   44      <!-- the whole interface re-tunes consistently.                  -->
   45      <!-- ========================================================== -->
   46      <x:Double x:Key="TypeEyebrowMicroSize">10</x:Double>
   47      <x:Double x:Key="TypeEyebrowSize">11</x:Double>
   48      <x:Double x:Key="TypeLabelSize">12</x:Double>
   49      <x:Double x:Key="TypeBodySmallSize">14</x:Double>
   50      <x:Double x:Key="TypeBodySize">16</x:Double>
   51      <x:Double x:Key="TypeSubheadingSize">19</x:Double>
   52      <x:Double x:Key="TypeTitleSize">24</x:Double>
   53      <x:Double x:Key="TypeDisplaySize">32</x:Double>
   54  
   55      <!-- Three-weight strategy. Light for display + body, Regular for
   56           small body, Medium for mono labels and emphasized labels. The
   57           skill's rule: "don't use more than 3-4 weights". -->
   58  
   59      <!-- ========================================================== -->
   60      <!-- Mono styles (chrome — eyebrows, labels, files, code)         -->
   61      <!-- ========================================================== -->
   62  
   63      <Style x:Key="MonoEyebrow" TargetType="TextBlock">
   64          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
   65          <Setter Property="FontSize" Value="{StaticResource TypeEyebrowSize}" />
   66          <Setter Property="FontWeight" Value="Medium" />
   67          <Setter Property="CharacterSpacing" Value="180" />
   68          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
   69          <Setter Property="LineHeight" Value="13" />
   70          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
   71      </Style>
   72  
   73      <Style x:Key="LiveBuildEyebrow" TargetType="TextBlock" BasedOn="{StaticResource MonoEyebrow}">
   74          <Setter Property="FontSize" Value="{StaticResource TypeBodySmallSize}" />
   75          <Setter Property="LineHeight" Value="15" />
   76      </Style>
   77  
   78      <Style x:Key="MonoLabel" TargetType="TextBlock">
   79          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
   80          <Setter Property="FontSize" Value="{StaticResource TypeLabelSize}" />
   81          <Setter Property="FontWeight" Value="Medium" />
   82          <Setter Property="CharacterSpacing" Value="140" />
   83          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
   84          <Setter Property="LineHeight" Value="14" />
   85          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
   86      </Style>
   87  
   88      <Style x:Key="MonoCaption" TargetType="TextBlock">
   89          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
   90          <Setter Property="FontSize" Value="{StaticResource TypeBodySmallSize}" />
   91          <Setter Property="FontWeight" Value="Normal" />
   92          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
   93          <Setter Property="LineHeight" Value="19" />
   94          <Setter Property="TextWrapping" Value="Wrap" />
   95          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
   96          <Setter Property="Typography.SlashedZero" Value="True" />
   97      </Style>
   98  
   99      <Style x:Key="MonoMicro" TargetType="TextBlock">
  100          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
  101          <Setter Property="FontSize" Value="{StaticResource TypeEyebrowMicroSize}" />
  102          <Setter Property="FontWeight" Value="Normal" />
  103          <Setter Property="CharacterSpacing" Value="180" />
  104          <Setter Property="Foreground" Value="{ThemeResource Ink4Brush}" />
  105          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
  106      </Style>
  107  
  108      <!-- ========================================================== -->
  109      <!-- Sans content styles (the new semantic scale)                -->
  110      <!-- ========================================================== -->
  111  
  112      <!-- Display — top of page, hero "What are we building?", scaffold
  113           "The bundle is ready." Reserved for the single highest-priority
  114           line on a surface.
  115           Weight reads SemiBold per the design brief mockups (heavy enough
  116           to anchor the page; the prior Light read as too whispery against
  117           the fact-table chrome below it). -->
  118      <Style x:Key="DisplayTitle" TargetType="TextBlock">
  119          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  120          <Setter Property="FontSize" Value="{StaticResource TypeDisplaySize}" />
  121          <Setter Property="FontWeight" Value="SemiBold" />
  122          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  123          <Setter Property="LineHeight" Value="38" />
  124      </Style>
  125  
  126      <!-- Title — section sub-titles, summary card app name. -->
  127      <Style x:Key="SectionTitle" TargetType="TextBlock">
  128          <Setter Property="FontFamily" Value="{StaticResource SansLightFontFamily}" />
  129          <Setter Property="FontSize" Value="{StaticResource TypeTitleSize}" />
  130          <Setter Property="FontWeight" Value="Light" />
  131          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  132          <Setter Property="LineHeight" Value="30" />
  133      </Style>
  134  
  135      <!-- Subheading — locked-card summary, ux flow screen names. -->
  136      <Style x:Key="Subheading" TargetType="TextBlock">
  137          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  138          <Setter Property="FontSize" Value="{StaticResource TypeSubheadingSize}" />
  139          <Setter Property="FontWeight" Value="Normal" />
  140          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  141          <Setter Property="LineHeight" Value="26" />
  142          <Setter Property="TextWrapping" Value="Wrap" />
  143      </Style>
  144  
  145      <!-- Body — every prose surface (subtitles, descriptions).
  146           Oldstyle figures blend into descender lines so digits in
  147           prose don't read as data. -->
  148      <Style x:Key="Body" TargetType="TextBlock">
  149          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  150          <Setter Property="FontSize" Value="{StaticResource TypeBodySize}" />
  151          <Setter Property="FontWeight" Value="Normal" />
  152          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  153          <Setter Property="LineHeight" Value="24" />
  154          <Setter Property="TextWrapping" Value="Wrap" />
  155          <Setter Property="Typography.NumeralStyle" Value="OldStyle" />
  156      </Style>
  157  
  158      <!-- Body small — captions, supplementary copy where Body is too loud. -->
  159      <Style x:Key="BodySmall" TargetType="TextBlock">
  160          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  161          <Setter Property="FontSize" Value="{StaticResource TypeBodySmallSize}" />
  162          <Setter Property="FontWeight" Value="Normal" />
  163          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
  164          <Setter Property="LineHeight" Value="20" />
  165          <Setter Property="TextWrapping" Value="Wrap" />
  166          <Setter Property="Typography.NumeralStyle" Value="OldStyle" />
  167      </Style>
  168  
  169      <!-- ========================================================== -->
  170      <!-- Legacy serif styles — kept so existing consumers compile.   -->
  171      <!-- New code should use the semantic styles above.              -->
  172      <!-- ========================================================== -->
  173  
  174      <Style x:Key="SerifBody" TargetType="TextBlock" BasedOn="{StaticResource Body}">
  175          <Setter Property="FontFamily" Value="{StaticResource SansLightFontFamily}" />
  176          <Setter Property="FontSize" Value="18" />
  177          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  178          <Setter Property="LineHeight" Value="27" />
  179      </Style>
  180  
  181      <Style x:Key="SerifSuggestion" TargetType="TextBlock" BasedOn="{StaticResource Subheading}">
  182          <Setter Property="FontWeight" Value="Medium" />
  183      </Style>
  184  
  185      <Style x:Key="SerifPlaceholder" TargetType="TextBlock" BasedOn="{StaticResource BodySmall}">
  186          <Setter Property="Foreground" Value="{ThemeResource Ink4Brush}" />
  187          <Setter Property="LineHeight" Value="21" />
  188      </Style>
  189  
  190      <Style x:Key="SerifTitle" TargetType="TextBlock" BasedOn="{StaticResource SectionTitle}">
  191          <Setter Property="FontWeight" Value="Medium" />
  192      </Style>
  193  
  194      <!-- ========================================================== -->
  195      <!-- v11 brief — fifteen-style type scale.                        -->
  196      <!-- Display / Heading / Body / Eyebrow / Mono tiers, all using   -->
  197      <!-- Inter (sans) or JetBrains Mono. Older keys above remain as   -->
  198      <!-- backwards-compat aliases.                                    -->
  199      <!-- ========================================================== -->
  200  
  201      <!-- Display — top-of-page hero text. Reserved for the single        -->
  202      <!-- highest-priority line on a surface.                              -->
  203      <Style x:Key="DisplayLargeTextStyle" TargetType="TextBlock">
  204          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  205          <Setter Property="FontSize" Value="32" />
  206          <Setter Property="FontWeight" Value="SemiBold" />
  207          <Setter Property="CharacterSpacing" Value="-15" />
  208          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  209          <Setter Property="LineHeight" Value="38" />
  210      </Style>
  211      <Style x:Key="DisplayMediumTextStyle" TargetType="TextBlock">
  212          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  213          <Setter Property="FontSize" Value="26" />
  214          <Setter Property="FontWeight" Value="Medium" />
  215          <Setter Property="CharacterSpacing" Value="-15" />
  216          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  217          <Setter Property="LineHeight" Value="32" />
  218      </Style>
  219  
  220      <!-- Heading — section sub-titles, summary card app name. -->
  221      <Style x:Key="HeadingLargeTextStyle" TargetType="TextBlock">
  222          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  223          <Setter Property="FontSize" Value="22" />
  224          <Setter Property="FontWeight" Value="Medium" />
  225          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  226          <Setter Property="LineHeight" Value="28" />
  227      </Style>
  228      <Style x:Key="HeadingMediumTextStyle" TargetType="TextBlock">
  229          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  230          <Setter Property="FontSize" Value="19" />
  231          <Setter Property="FontWeight" Value="Medium" />
  232          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  233          <Setter Property="LineHeight" Value="26" />
  234      </Style>
  235      <Style x:Key="HeadingSmallTextStyle" TargetType="TextBlock">
  236          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  237          <Setter Property="FontSize" Value="16" />
  238          <Setter Property="FontWeight" Value="Medium" />
  239          <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
  240          <Setter Property="LineHeight" Value="22" />
  241      </Style>
  242  
  243      <!-- Body — paragraph prose. -->
  244      <Style x:Key="BodyLargeTextStyle" TargetType="TextBlock">
  245          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  246          <Setter Property="FontSize" Value="14" />
  247          <Setter Property="FontWeight" Value="Normal" />
  248          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  249          <Setter Property="LineHeight" Value="22" />
  250          <Setter Property="TextWrapping" Value="Wrap" />
  251      </Style>
  252      <Style x:Key="BodyMediumTextStyle" TargetType="TextBlock">
  253          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  254          <Setter Property="FontSize" Value="13" />
  255          <Setter Property="FontWeight" Value="Normal" />
  256          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  257          <Setter Property="LineHeight" Value="20" />
  258          <Setter Property="TextWrapping" Value="Wrap" />
  259      </Style>
  260      <Style x:Key="BodySmallTextStyle" TargetType="TextBlock">
  261          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  262          <Setter Property="FontSize" Value="12" />
  263          <Setter Property="FontWeight" Value="Normal" />
  264          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
  265          <Setter Property="LineHeight" Value="18" />
  266          <Setter Property="TextWrapping" Value="Wrap" />
  267      </Style>
  268      <!-- Body italic — editorial marginalia voice. Inter italic now
  269           (serif voice retired per v11 brief). -->
  270      <Style x:Key="BodyItalicTextStyle" TargetType="TextBlock">
  271          <Setter Property="FontFamily" Value="{StaticResource SerifItalicFontFamily}" />
  272          <Setter Property="FontStyle" Value="Italic" />
  273          <Setter Property="FontSize" Value="13" />
  274          <Setter Property="FontWeight" Value="Normal" />
  275          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
  276          <Setter Property="LineHeight" Value="20" />
  277          <Setter Property="TextWrapping" Value="Wrap" />
  278      </Style>
  279  
  280      <!-- Eyebrow — caps metadata labels. Always uppercase, 0.04em tracking
  281           (~40 CharacterSpacing units). -->
  282      <Style x:Key="EyebrowLargeTextStyle" TargetType="TextBlock">
  283          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  284          <Setter Property="FontSize" Value="11" />
  285          <Setter Property="FontWeight" Value="Medium" />
  286          <Setter Property="CharacterSpacing" Value="40" />
  287          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
  288      </Style>
  289      <Style x:Key="EyebrowSmallTextStyle" TargetType="TextBlock">
  290          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  291          <Setter Property="FontSize" Value="10" />
  292          <Setter Property="FontWeight" Value="Medium" />
  293          <Setter Property="CharacterSpacing" Value="40" />
  294          <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
  295      </Style>
  296      <Style x:Key="EyebrowTinyTextStyle" TargetType="TextBlock">
  297          <Setter Property="FontFamily" Value="{StaticResource SansFontFamily}" />
  298          <Setter Property="FontSize" Value="9" />
  299          <Setter Property="FontWeight" Value="Medium" />
  300          <Setter Property="CharacterSpacing" Value="40" />
  301          <Setter Property="Foreground" Value="{ThemeResource Ink4Brush}" />
  302      </Style>
  303  
  304      <!-- Mono — code / hex / file paths / kbd. Always JetBrains Mono.
  305           -0.01em tracking (~-10 CharacterSpacing) keeps tabular density. -->
  306      <Style x:Key="MonoLargeTextStyle" TargetType="TextBlock">
  307          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
  308          <Setter Property="FontSize" Value="13" />
  309          <Setter Property="FontWeight" Value="Normal" />
  310          <Setter Property="CharacterSpacing" Value="-10" />
  311          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  312          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
  313      </Style>
  314      <Style x:Key="MonoMediumTextStyle" TargetType="TextBlock">
  315          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
  316          <Setter Property="FontSize" Value="12" />
  317          <Setter Property="FontWeight" Value="Normal" />
  318          <Setter Property="CharacterSpacing" Value="-10" />
  319          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  320          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
  321      </Style>
  322      <Style x:Key="MonoSmallTextStyle" TargetType="TextBlock">
  323          <Setter Property="FontFamily" Value="{StaticResource MonoFontFamily}" />
  324          <Setter Property="FontSize" Value="11" />
  325          <Setter Property="FontWeight" Value="Normal" />
  326          <Setter Property="CharacterSpacing" Value="-10" />
  327          <Setter Property="Foreground" Value="{ThemeResource Ink2Brush}" />
  328          <Setter Property="Typography.NumeralAlignment" Value="Tabular" />
  329      </Style>
  330  
  331  </ResourceDictionary>
```
