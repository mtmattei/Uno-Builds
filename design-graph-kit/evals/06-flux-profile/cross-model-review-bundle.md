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

The kit is undecided about `region` nodes. This gold contains **2**
of them, while the page it models is built from 22 nested layout
containers. Ten independent runs of other screens emitted 6-10 region nodes
each, unprompted.

The proposed rule is: *emit a `region` when the source declares a structural
grouping that owns layout or state and is not itself a reusable component; do
not emit one for every layout panel. The test is whether removing the grouping
would change the screen's meaning or only its arrangement.*

**Rule on it.** Should this gold contain region nodes? If yes, which ones, and
what is your reasoning? If no, explain why the arrangement carries no meaning
worth modeling. Answer directly - a hedge here is worth nothing.


---

# The answer key under review: `06-flux-profile`

57 nodes · 74 edges · 4 unresolved items

## What this screen is (the eval's own description)

# Fixture: FluxTransit Profile (source-backed)

Second real eval — a different app, design system, and architecture than
eval 05, to test that the kit's results are not Orbital-specific.

## Source

- `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml` — layout
- `.../ProfilePage.xaml.cs` — trivial (InitializeComponent only)
- `.../Presentation/ProfileModel.cs` — **MVUX** model: `IState<>`s
  (OpusBalance, GeminiApiKey, SelectedLanguage, IsRefreshing) and commands
  (GoBack, UpdateBalance, SaveSettings)
- `.../Styles/FluxStyles.xaml` — glass-morphism design system (colors,
  spacing scale, corner radii incl. pill, type ramp)

## What makes this different from eval 05

- **MVUX instead of code-behind**: behavior arrives as declared bindings and
  model commands, not Click handlers — evidence discipline must follow
  bindings.
- **A binding-driven state**: `IsRefreshing` swaps the Update button for a
  ProgressRing + "Updating..." row (declared loading state + trigger).
- **Toolkit controls**: `utu:ChipGroup`/`Chip` (language), `ToggleSwitch`,
  `ProgressRing`.
- **Honesty traps**: Back navigates to a *stack-dependent* target;
  `Add New Route` has **no** command bound; `SaveSettings` is declared in the
  model but **nothing invokes it**; the chips/toggle are XAML **literals**,
  not bound to the model's `SelectedLanguage`. All four belong in
  `unresolved`, not as invented behavior.
- **Different token flavor**: alpha-channel glass colors (`#1e293b66`),
  pill radius (9999), a declared spacing scale (XS/S/M/L), tracked type ramp.

## What this eval tests

- generalization of the v0.4 rules (id grammar, altitude, token scoping,
  uno mapping) to a second design language;
- consolidation: 3 glass panels → one canonical; 2 route rows → canonical +
  instances;
- binding-declared state/trigger modeling;
- unresolved discipline on four distinct traps.


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
  "graphId": "eval.flux-profile",
  "name": "FluxTransit Profile",
  "description": "Gold graph for the FluxTransit ProfilePage (source-backed: XAML + MVUX model + FluxStyles). Kit v0.4 altitude. Gold v1.1: canonical renamed glass-panel -> card per the kit's own Pass-8 vocabulary (defect found by blind runs).",
  "sourceSummary": [
    {
      "type": "xaml",
      "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
    },
    {
      "type": "csharp",
      "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml.cs"
    },
    {
      "type": "csharp",
      "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs"
    },
    {
      "type": "design-system",
      "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
    }
  ],
  "nodes": [
    {
      "id": "screen.profile",
      "type": "screen",
      "name": "Profile",
      "properties": {
        "uno": {
          "type": "Page",
          "class": "FluxTransit.Presentation.ProfilePage"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "region.profile.header",
      "type": "region",
      "name": "Header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Back button + heading grid at top."
      }
    },
    {
      "id": "control.header.back",
      "type": "control",
      "name": "Back",
      "role": "button",
      "semanticRole": "backNavigation",
      "properties": {
        "command": "{Binding GoBack}",
        "uno": {
          "type": "Button",
          "styleKey": "FluxIconButtonStyle",
          "iconGlyph": "E72B"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Icon button bound to the GoBack command."
      }
    },
    {
      "id": "content.header.title",
      "type": "content",
      "name": "Title",
      "text": "Profile",
      "semanticRole": "pageTitle",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxHeadingLarge"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.header.subtitle",
      "type": "content",
      "name": "Subtitle",
      "text": "Manage your transit settings",
      "semanticRole": "pageSubtitle",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.card",
      "type": "component",
      "name": "Section card (glass)",
      "role": "card",
      "properties": {
        "uno": {
          "type": "Border",
          "styleKey": "FluxGlassPanelStyle"
        }
      },
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Three sections share FluxGlassPanelStyle (glass bg, subtle border, radius 24, padding 24)."
      }
    },
    {
      "id": "component.card.opus",
      "type": "component",
      "name": "OPUS card section",
      "semanticRole": "opusSection",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.card.routes",
      "type": "component",
      "name": "Saved routes section",
      "semanticRole": "routesSection",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.card.settings",
      "type": "component",
      "name": "Settings section",
      "semanticRole": "settingsSection",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.opus.section-title",
      "type": "content",
      "name": "Opus header",
      "text": "OPUS CARD",
      "semanticRole": "sectionHeader",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxMicro"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.opus-card",
      "type": "component",
      "name": "OPUS card visual",
      "role": "card",
      "properties": {
        "parts": [
          "brand",
          "cardNumber"
        ],
        "brand": "OPUS",
        "value": "{Binding CardNumber}",
        "uno": {
          "type": "Border"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "120x80 indigo card visual; brand text + bound masked number."
      }
    },
    {
      "id": "content.opus.balance-label",
      "type": "content",
      "name": "Balance label",
      "text": "Current Balance",
      "semanticRole": "fieldLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.opus.balance-value",
      "type": "content",
      "name": "Balance value",
      "semanticRole": "metricValue",
      "properties": {
        "value": "{Binding OpusBalance}",
        "prefix": "$",
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxHeadingLarge"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Bound to OpusBalance with a literal $ run."
      }
    },
    {
      "id": "content.opus.refresh-hint",
      "type": "content",
      "name": "Refresh hint",
      "text": "Tap to refresh",
      "semanticRole": "helperText",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "control.opus.update",
      "type": "control",
      "name": "Update Balance",
      "role": "button",
      "text": "Update Balance",
      "semanticRole": "primaryAction",
      "properties": {
        "command": "{Binding UpdateBalance}",
        "uno": {
          "type": "Button",
          "styleKey": "FluxPrimaryButtonStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Bound to UpdateBalance; hidden while IsRefreshing."
      }
    },
    {
      "id": "state.opus.refreshing",
      "type": "state",
      "name": "Refreshing",
      "semanticRole": "loading",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "IsRefreshing"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs"
        },
        "rationale": "IState<bool> IsRefreshing toggles the button/progress swap during UpdateBalance."
      }
    },
    {
      "id": "control.opus.progress",
      "type": "control",
      "name": "Refresh progress",
      "role": "progressRing",
      "semanticRole": "busyIndicator",
      "properties": {
        "uno": {
          "type": "ProgressRing"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.opus.updating-label",
      "type": "content",
      "name": "Updating label",
      "text": "Updating...",
      "semanticRole": "statusText",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.routes.section-title",
      "type": "content",
      "name": "Routes header",
      "text": "SAVED ROUTES",
      "semanticRole": "sectionHeader",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxMicro"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.route-item",
      "type": "component",
      "name": "Saved route row",
      "role": "listItem",
      "properties": {
        "parts": [
          "icon",
          "name",
          "stations",
          "chevron"
        ],
        "uno": {
          "type": "Border",
          "styleKey": "FluxTransitCardStyle"
        }
      },
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Two identical route rows: transit icon, name over stations, chevron."
      }
    },
    {
      "id": "component.route-item.home-work",
      "type": "component",
      "name": "Home \u2192 Work",
      "properties": {
        "name": "Home \u2192 Work",
        "stations": "Berri-UQAM \u2192 McGill"
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "component.route-item.downtown-loop",
      "type": "component",
      "name": "Downtown Loop",
      "properties": {
        "name": "Downtown Loop",
        "stations": "Place-des-Arts \u2192 Mont-Royal"
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "control.routes.add",
      "type": "control",
      "name": "Add New Route",
      "role": "button",
      "text": "Add New Route",
      "semanticRole": "addAction",
      "properties": {
        "uno": {
          "type": "Button",
          "iconGlyph": "E710"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Outlined full-width button; no command or click handler bound."
      }
    },
    {
      "id": "content.settings.section-title",
      "type": "content",
      "name": "Settings header",
      "text": "SETTINGS",
      "semanticRole": "sectionHeader",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxMicro"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.settings.api-label",
      "type": "content",
      "name": "API key label",
      "text": "Gemini API Key (for AI features)",
      "semanticRole": "fieldLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "control.settings.api-key",
      "type": "control",
      "name": "Gemini API key",
      "role": "textbox",
      "semanticRole": "apiKeyInput",
      "properties": {
        "placeholder": "Enter your Gemini API key",
        "value": "{Binding GeminiApiKey, Mode=TwoWay}",
        "uno": {
          "type": "TextBox"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "TwoWay-bound to GeminiApiKey."
      }
    },
    {
      "id": "content.settings.api-helper",
      "type": "content",
      "name": "API key helper",
      "text": "Get your API key from ai.google.dev",
      "semanticRole": "helperText",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.settings.language-label",
      "type": "content",
      "name": "Language label",
      "text": "Language",
      "semanticRole": "fieldLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "control.settings.language",
      "type": "control",
      "name": "Language",
      "role": "chipGroup",
      "semanticRole": "languageSelector",
      "properties": {
        "options": [
          "EN",
          "FR"
        ],
        "selected": "EN",
        "uno": {
          "type": "utu:ChipGroup",
          "styleKey": "FilterChipGroupStyle"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Single-select chip group; EN literal-checked in XAML."
      }
    },
    {
      "id": "content.settings.alerts-label",
      "type": "content",
      "name": "Alerts label",
      "text": "Service Alerts",
      "semanticRole": "fieldLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.settings.alerts-helper",
      "type": "content",
      "name": "Alerts helper",
      "text": "Receive notifications about delays",
      "semanticRole": "helperText",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "control.settings.alerts",
      "type": "control",
      "name": "Service alerts",
      "role": "toggle",
      "semanticRole": "notificationsToggle",
      "properties": {
        "value": true,
        "uno": {
          "type": "ToggleSwitch"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "IsOn=True literal; not bound to any state."
      }
    },
    {
      "id": "region.profile.footer",
      "type": "region",
      "name": "App info footer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.footer.version",
      "type": "content",
      "name": "App version",
      "text": "Flux Transit v1.0",
      "semanticRole": "versionLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "content.footer.credit",
      "type": "content",
      "name": "Credit",
      "text": "Built with Uno Platform",
      "semanticRole": "caption",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "id": "token.color.background",
      "type": "token",
      "name": "Background",
      "category": "color",
      "value": "#0f172a",
      "properties": {
        "uno": {
          "resourceKey": "FluxBackgroundBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Page background."
      }
    },
    {
      "id": "token.color.surface",
      "type": "token",
      "name": "Surface",
      "category": "color",
      "value": "#1e293b",
      "properties": {
        "uno": {
          "resourceKey": "FluxSurfaceBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "TextBox background."
      }
    },
    {
      "id": "token.color.glass-panel",
      "type": "token",
      "name": "Glass panel",
      "category": "color",
      "value": "#1e293b66",
      "properties": {
        "uno": {
          "resourceKey": "FluxGlassPanelBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Panel background."
      }
    },
    {
      "id": "token.color.border-subtle",
      "type": "token",
      "name": "Border subtle",
      "category": "color",
      "value": "#ffffff0d",
      "properties": {
        "uno": {
          "resourceKey": "FluxBorderSubtleBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Panel/card border."
      }
    },
    {
      "id": "token.color.border-light",
      "type": "token",
      "name": "Border light",
      "category": "color",
      "value": "#ffffff1a",
      "properties": {
        "uno": {
          "resourceKey": "FluxBorderLightBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Outlined button/TextBox border."
      }
    },
    {
      "id": "token.color.primary",
      "type": "token",
      "name": "Primary (indigo 400)",
      "category": "color",
      "value": "#818cf8",
      "properties": {
        "uno": {
          "resourceKey": "FluxPrimaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Primary button bg; route icons."
      }
    },
    {
      "id": "token.color.primary-strong",
      "type": "token",
      "name": "Primary strong (indigo 600)",
      "category": "color",
      "value": "#4f46e5",
      "properties": {
        "uno": {
          "resourceKey": "FluxPrimaryStrongBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "OPUS card visual bg."
      }
    },
    {
      "id": "token.color.success",
      "type": "token",
      "name": "Success (emerald)",
      "category": "color",
      "value": "#34d399",
      "properties": {
        "uno": {
          "resourceKey": "FluxSuccessBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Balance value."
      }
    },
    {
      "id": "token.color.text-primary",
      "type": "token",
      "name": "Text primary",
      "category": "color",
      "value": "#ffffff",
      "properties": {
        "uno": {
          "resourceKey": "FluxTextPrimaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "High-emphasis text."
      }
    },
    {
      "id": "token.color.text-secondary",
      "type": "token",
      "name": "Text secondary",
      "category": "color",
      "value": "#94a3b8",
      "properties": {
        "uno": {
          "resourceKey": "FluxTextSecondaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Body text (via FluxBody)."
      }
    },
    {
      "id": "token.color.text-muted",
      "type": "token",
      "name": "Text muted",
      "category": "color",
      "value": "#64748b",
      "properties": {
        "uno": {
          "resourceKey": "FluxTextMutedBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Micro headers; chevrons."
      }
    },
    {
      "id": "token.radius.24",
      "type": "token",
      "name": "24 radius",
      "category": "radius",
      "value": 24,
      "properties": {
        "uno": {
          "resourceKey": "FluxCornerRadiusLarge",
          "resourceType": "CornerRadius"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Glass panels."
      }
    },
    {
      "id": "token.radius.16",
      "type": "token",
      "name": "16 radius",
      "category": "radius",
      "value": 16,
      "properties": {
        "uno": {
          "resourceKey": "FluxCornerRadiusMedium",
          "resourceType": "CornerRadius"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Route cards."
      }
    },
    {
      "id": "token.radius.full",
      "type": "token",
      "name": "Full (pill) radius",
      "category": "radius",
      "value": 9999,
      "properties": {
        "uno": {
          "resourceKey": "FluxCornerRadiusFull",
          "resourceType": "CornerRadius"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Primary button."
      }
    },
    {
      "id": "token.spacing.24",
      "type": "token",
      "name": "24 spacing",
      "category": "spacing",
      "value": 24,
      "properties": {
        "uno": {
          "resourceKey": "FluxSpacingL"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Page section gap."
      }
    },
    {
      "id": "token.spacing.16",
      "type": "token",
      "name": "16 spacing",
      "category": "spacing",
      "value": 16,
      "properties": {
        "uno": {
          "resourceKey": "FluxSpacingM"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Panel inner gap."
      }
    },
    {
      "id": "token.spacing.8",
      "type": "token",
      "name": "8 spacing",
      "category": "spacing",
      "value": 8,
      "properties": {
        "uno": {
          "resourceKey": "FluxSpacingS"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Label/field gaps."
      }
    },
    {
      "id": "token.spacing.4",
      "type": "token",
      "name": "4 spacing",
      "category": "spacing",
      "value": 4,
      "properties": {
        "uno": {
          "resourceKey": "FluxSpacingXS"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Route name/stations gap."
      }
    },
    {
      "id": "token.typography.heading-large",
      "type": "token",
      "name": "Heading large",
      "category": "typography",
      "value": {
        "size": 36,
        "weight": "Bold",
        "tracking": -20
      },
      "properties": {
        "uno": {
          "styleKey": "FluxHeadingLarge"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Page title; balance value."
      }
    },
    {
      "id": "token.typography.body",
      "type": "token",
      "name": "Body",
      "category": "typography",
      "value": {
        "size": 14
      },
      "properties": {
        "uno": {
          "styleKey": "FluxBody"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Default body text."
      }
    },
    {
      "id": "token.typography.body-bold",
      "type": "token",
      "name": "Body bold",
      "category": "typography",
      "value": {
        "size": 14,
        "weight": "Bold"
      },
      "properties": {
        "uno": {
          "styleKey": "FluxBodyBold"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Route names."
      }
    },
    {
      "id": "token.typography.micro",
      "type": "token",
      "name": "Micro header",
      "category": "typography",
      "value": {
        "size": 10,
        "weight": "Bold",
        "tracking": 80
      },
      "properties": {
        "uno": {
          "styleKey": "FluxMicro"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Uppercase section headers."
      }
    }
  ],
  "edges": [
    {
      "from": "screen.profile",
      "relation": "contains",
      "to": "region.profile.header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "region.profile.header",
      "relation": "contains",
      "to": "control.header.back",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "region.profile.header",
      "relation": "contains",
      "to": "content.header.title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "region.profile.header",
      "relation": "contains",
      "to": "content.header.subtitle",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "screen.profile",
      "relation": "contains",
      "to": "component.card.opus",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "screen.profile",
      "relation": "contains",
      "to": "component.card.routes",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "screen.profile",
      "relation": "contains",
      "to": "component.card.settings",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "screen.profile",
      "relation": "contains",
      "to": "region.profile.footer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "region.profile.footer",
      "relation": "contains",
      "to": "content.footer.version",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "region.profile.footer",
      "relation": "contains",
      "to": "content.footer.credit",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "instance-of",
      "to": "component.card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Uses FluxGlassPanelStyle."
      }
    },
    {
      "from": "component.card.routes",
      "relation": "instance-of",
      "to": "component.card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Uses FluxGlassPanelStyle."
      }
    },
    {
      "from": "component.card.settings",
      "relation": "instance-of",
      "to": "component.card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Uses FluxGlassPanelStyle."
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "content.opus.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "component.opus-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "content.opus.balance-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "content.opus.balance-value",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "content.opus.refresh-hint",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "contains",
      "to": "control.opus.update",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.routes",
      "relation": "contains",
      "to": "content.routes.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.routes",
      "relation": "contains",
      "to": "component.route-item.home-work",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.routes",
      "relation": "contains",
      "to": "component.route-item.downtown-loop",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.routes",
      "relation": "contains",
      "to": "control.routes.add",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.route-item.home-work",
      "relation": "instance-of",
      "to": "component.route-item",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Identical route-row structure."
      }
    },
    {
      "from": "component.route-item.downtown-loop",
      "relation": "instance-of",
      "to": "component.route-item",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        },
        "rationale": "Identical route-row structure."
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.api-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "control.settings.api-key",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.api-helper",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.language-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "control.settings.language",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.alerts-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "content.settings.alerts-helper",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.settings",
      "relation": "contains",
      "to": "control.settings.alerts",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "component.card.opus",
      "relation": "has-state",
      "to": "state.opus.refreshing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs"
        },
        "rationale": "IsRefreshing swaps the update button for the progress row inside this section."
      }
    },
    {
      "from": "control.opus.update",
      "relation": "triggers",
      "to": "state.opus.refreshing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs"
        },
        "rationale": "UpdateBalance sets IsRefreshing true for the duration of the refresh."
      }
    },
    {
      "from": "state.opus.refreshing",
      "relation": "contains",
      "to": "control.opus.progress",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "state.opus.refreshing",
      "relation": "contains",
      "to": "content.opus.updating-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"
        }
      }
    },
    {
      "from": "screen.profile",
      "relation": "uses-token",
      "to": "token.color.background",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "screen.profile",
      "relation": "uses-token",
      "to": "token.spacing.24",
      "properties": {
        "appliesTo": "sectionGap"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.color.glass-panel",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.color.border-subtle",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.radius.24",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.spacing.16",
      "properties": {
        "appliesTo": "innerGap"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.header.title",
      "relation": "uses-token",
      "to": "token.typography.heading-large",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.header.subtitle",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.opus.section-title",
      "relation": "uses-token",
      "to": "token.typography.micro",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.routes.section-title",
      "relation": "uses-token",
      "to": "token.typography.micro",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.section-title",
      "relation": "uses-token",
      "to": "token.typography.micro",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.opus-card",
      "relation": "uses-token",
      "to": "token.color.primary-strong",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.opus.balance-label",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.opus.balance-value",
      "relation": "uses-token",
      "to": "token.typography.heading-large",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.opus.balance-value",
      "relation": "uses-token",
      "to": "token.color.success",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.opus.refresh-hint",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.opus.update",
      "relation": "uses-token",
      "to": "token.color.primary",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.opus.update",
      "relation": "uses-token",
      "to": "token.radius.full",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.radius.16",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.color.border-subtle",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.color.primary",
      "properties": {
        "appliesTo": "iconColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "properties": {
        "appliesTo": "chevronColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.typography.body-bold",
      "properties": {
        "appliesTo": "nameFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "stationsFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "component.route-item",
      "relation": "uses-token",
      "to": "token.spacing.4",
      "properties": {
        "appliesTo": "textGap"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.routes.add",
      "relation": "uses-token",
      "to": "token.color.border-light",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.routes.add",
      "relation": "uses-token",
      "to": "token.color.text-primary",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.api-label",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.settings.api-key",
      "relation": "uses-token",
      "to": "token.color.surface",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "control.settings.api-key",
      "relation": "uses-token",
      "to": "token.color.border-light",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.api-helper",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.language-label",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.alerts-label",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.settings.alerts-helper",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.footer.version",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    },
    {
      "from": "content.footer.credit",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"
        },
        "rationale": "Declared style/brush consumption."
      }
    }
  ],
  "unresolved": [
    {
      "id": "unresolved.header.back-target",
      "question": "Where does Back navigate?",
      "relatedIds": [
        "control.header.back"
      ],
      "possibleValues": [
        "previous screen on the navigation stack"
      ],
      "reason": "GoBack delegates to INavigator.GoBack; the destination depends on the runtime stack, not a declared screen."
    },
    {
      "id": "unresolved.routes.add-behavior",
      "question": "What does Add New Route do?",
      "relatedIds": [
        "control.routes.add"
      ],
      "possibleValues": [
        "open a route editor",
        "unknown"
      ],
      "reason": "The button has no Command or Click handler in the source."
    },
    {
      "id": "unresolved.settings.save-orphan",
      "question": "What invokes the declared SaveSettings command?",
      "relatedIds": [
        "screen.profile"
      ],
      "possibleValues": [
        "auto-save on change",
        "missing UI",
        "dead code"
      ],
      "reason": "ProfileModel.SaveSettings exists but nothing in the page binds it."
    },
    {
      "id": "unresolved.settings.unbound-toggles",
      "question": "Are the language chips and alerts toggle meant to bind to model state?",
      "relatedIds": [
        "control.settings.language",
        "control.settings.alerts"
      ],
      "possibleValues": [
        "bind to SelectedLanguage / a notifications state",
        "static mock"
      ],
      "reason": "Chip IsChecked and ToggleSwitch IsOn are XAML literals; SelectedLanguage exists in the model but is unused by the page."
    }
  ]
}
```

## Source files the gold cites

Line-numbered so you can cite `file:line`.

### `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml`

```xml
    1  <Page x:Class="FluxTransit.Presentation.ProfilePage"
    2        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4        xmlns:local="using:FluxTransit.Presentation"
    5        xmlns:uen="using:Uno.Extensions.Navigation.UI"
    6        xmlns:utu="using:Uno.Toolkit.UI"
    7        NavigationCacheMode="Required"
    8        Background="{StaticResource FluxBackgroundBrush}">
    9  
   10    <ScrollViewer>
   11      <StackPanel utu:SafeArea.Insets="VisibleBounds"
   12                  Padding="16"
   13                  Spacing="24"
   14                  MaxWidth="600"
   15                  HorizontalAlignment="Center">
   16  
   17        <!-- Header with Back Button -->
   18        <Grid>
   19          <Grid.ColumnDefinitions>
   20            <ColumnDefinition Width="Auto" />
   21            <ColumnDefinition Width="*" />
   22          </Grid.ColumnDefinitions>
   23  
   24          <Button Style="{StaticResource FluxIconButtonStyle}"
   25                  Command="{Binding GoBack}"
   26                  VerticalAlignment="Top">
   27            <FontIcon Glyph="&#xE72B;" FontSize="20" />
   28          </Button>
   29  
   30          <StackPanel Grid.Column="1" Spacing="8" Margin="12,0,0,0">
   31            <TextBlock Text="Profile"
   32                       Style="{StaticResource FluxHeadingLarge}" />
   33            <TextBlock Text="Manage your transit settings"
   34                       Style="{StaticResource FluxBody}" />
   35          </StackPanel>
   36        </Grid>
   37  
   38        <!-- OPUS Card Section -->
   39        <Border Style="{StaticResource FluxGlassPanelStyle}">
   40          <StackPanel Spacing="16">
   41            <TextBlock Text="OPUS CARD"
   42                       Style="{StaticResource FluxMicro}" />
   43  
   44            <StackPanel Orientation="Horizontal" Spacing="16">
   45              <!-- Card Visual -->
   46              <Border CornerRadius="12"
   47                      Background="{StaticResource FluxPrimaryStrongBrush}"
   48                      Padding="16"
   49                      Width="120"
   50                      Height="80">
   51                <StackPanel VerticalAlignment="Bottom">
   52                  <TextBlock Text="OPUS"
   53                             FontWeight="Bold"
   54                             FontSize="16"
   55                             Foreground="White" />
   56                  <TextBlock Text="{Binding CardNumber}"
   57                             FontSize="12"
   58                             Foreground="White"
   59                             Opacity="0.7" />
   60                </StackPanel>
   61              </Border>
   62  
   63              <!-- Balance Info -->
   64              <StackPanel Spacing="8" VerticalAlignment="Center">
   65                <TextBlock Text="Current Balance"
   66                           Style="{StaticResource FluxBody}" />
   67                <TextBlock Style="{StaticResource FluxHeadingLarge}"
   68                           Foreground="{StaticResource FluxSuccessBrush}">
   69                  <Run Text="$" /><Run Text="{Binding OpusBalance}" />
   70                </TextBlock>
   71                <TextBlock Text="Tap to refresh"
   72                           Style="{StaticResource FluxBody}"
   73                           Opacity="0.6" />
   74              </StackPanel>
   75            </StackPanel>
   76  
   77            <Grid>
   78              <Button Content="Update Balance"
   79                      Command="{Binding UpdateBalance}"
   80                      Style="{StaticResource FluxPrimaryButtonStyle}"
   81                      HorizontalAlignment="Stretch"
   82                      Visibility="{Binding IsRefreshing, Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=Inverse}" />
   83              <StackPanel Orientation="Horizontal"
   84                          HorizontalAlignment="Center"
   85                          Spacing="12"
   86                          Visibility="{Binding IsRefreshing}">
   87                <ProgressRing IsActive="True" Width="20" Height="20" />
   88                <TextBlock Text="Updating..."
   89                           Style="{StaticResource FluxBody}"
   90                           VerticalAlignment="Center" />
   91              </StackPanel>
   92            </Grid>
   93          </StackPanel>
   94        </Border>
   95  
   96        <!-- Saved Routes Section -->
   97        <Border Style="{StaticResource FluxGlassPanelStyle}">
   98          <StackPanel Spacing="16">
   99            <TextBlock Text="SAVED ROUTES"
  100                       Style="{StaticResource FluxMicro}" />
  101  
  102            <!-- Route Item -->
  103            <Border Style="{StaticResource FluxTransitCardStyle}"
  104                    Background="Transparent"
  105                    Padding="12">
  106              <Grid>
  107                <Grid.ColumnDefinitions>
  108                  <ColumnDefinition Width="Auto" />
  109                  <ColumnDefinition Width="*" />
  110                  <ColumnDefinition Width="Auto" />
  111                </Grid.ColumnDefinitions>
  112                <FontIcon Glyph="&#xE80F;"
  113                          FontSize="20"
  114                          Foreground="{StaticResource FluxPrimaryBrush}"
  115                          VerticalAlignment="Center" />
  116                <StackPanel Grid.Column="1" Spacing="4" Margin="12,0">
  117                  <TextBlock Text="Home → Work"
  118                             Style="{StaticResource FluxBodyBold}" />
  119                  <TextBlock Text="Berri-UQAM → McGill"
  120                             Style="{StaticResource FluxBody}" />
  121                </StackPanel>
  122                <FontIcon Grid.Column="2"
  123                          Glyph="&#xE76C;"
  124                          FontSize="16"
  125                          Foreground="{StaticResource FluxTextMutedBrush}"
  126                          VerticalAlignment="Center" />
  127              </Grid>
  128            </Border>
  129  
  130            <!-- Route Item 2 -->
  131            <Border Style="{StaticResource FluxTransitCardStyle}"
  132                    Background="Transparent"
  133                    Padding="12">
  134              <Grid>
  135                <Grid.ColumnDefinitions>
  136                  <ColumnDefinition Width="Auto" />
  137                  <ColumnDefinition Width="*" />
  138                  <ColumnDefinition Width="Auto" />
  139                </Grid.ColumnDefinitions>
  140                <FontIcon Glyph="&#xE80F;"
  141                          FontSize="20"
  142                          Foreground="{StaticResource FluxPrimaryBrush}"
  143                          VerticalAlignment="Center" />
  144                <StackPanel Grid.Column="1" Spacing="4" Margin="12,0">
  145                  <TextBlock Text="Downtown Loop"
  146                             Style="{StaticResource FluxBodyBold}" />
  147                  <TextBlock Text="Place-des-Arts → Mont-Royal"
  148                             Style="{StaticResource FluxBody}" />
  149                </StackPanel>
  150                <FontIcon Grid.Column="2"
  151                          Glyph="&#xE76C;"
  152                          FontSize="16"
  153                          Foreground="{StaticResource FluxTextMutedBrush}"
  154                          VerticalAlignment="Center" />
  155              </Grid>
  156            </Border>
  157  
  158            <Button HorizontalAlignment="Stretch"
  159                    Background="Transparent"
  160                    BorderBrush="{StaticResource FluxBorderLightBrush}"
  161                    Foreground="{StaticResource FluxTextPrimaryBrush}">
  162              <StackPanel Orientation="Horizontal" Spacing="8">
  163                <FontIcon Glyph="&#xE710;" FontSize="16" />
  164                <TextBlock Text="Add New Route" VerticalAlignment="Center" />
  165              </StackPanel>
  166            </Button>
  167          </StackPanel>
  168        </Border>
  169  
  170        <!-- Settings Section -->
  171        <Border Style="{StaticResource FluxGlassPanelStyle}">
  172          <StackPanel Spacing="16">
  173            <TextBlock Text="SETTINGS"
  174                       Style="{StaticResource FluxMicro}" />
  175  
  176            <!-- Gemini API Key -->
  177            <StackPanel Spacing="8">
  178              <TextBlock Text="Gemini API Key (for AI features)"
  179                         Style="{StaticResource FluxBody}" />
  180              <TextBox PlaceholderText="Enter your Gemini API key"
  181                       Text="{Binding GeminiApiKey, Mode=TwoWay}"
  182                       Background="{StaticResource FluxSurfaceBrush}"
  183                       BorderBrush="{StaticResource FluxBorderLightBrush}" />
  184              <TextBlock Text="Get your API key from ai.google.dev"
  185                         Style="{StaticResource FluxBody}"
  186                         Opacity="0.6"
  187                         FontSize="12" />
  188            </StackPanel>
  189  
  190            <!-- Language Toggle -->
  191            <Grid>
  192              <Grid.ColumnDefinitions>
  193                <ColumnDefinition Width="*" />
  194                <ColumnDefinition Width="Auto" />
  195              </Grid.ColumnDefinitions>
  196              <TextBlock Text="Language"
  197                         Style="{StaticResource FluxBody}"
  198                         VerticalAlignment="Center" />
  199              <utu:ChipGroup Grid.Column="1"
  200                             SelectionMode="Single"
  201                             Style="{StaticResource FilterChipGroupStyle}">
  202                <utu:Chip Content="EN" IsChecked="True" />
  203                <utu:Chip Content="FR" />
  204              </utu:ChipGroup>
  205            </Grid>
  206  
  207            <!-- Notifications Toggle -->
  208            <Grid>
  209              <Grid.ColumnDefinitions>
  210                <ColumnDefinition Width="*" />
  211                <ColumnDefinition Width="Auto" />
  212              </Grid.ColumnDefinitions>
  213              <StackPanel Spacing="4">
  214                <TextBlock Text="Service Alerts"
  215                           Style="{StaticResource FluxBody}"
  216                           VerticalAlignment="Center" />
  217                <TextBlock Text="Receive notifications about delays"
  218                           Style="{StaticResource FluxBody}"
  219                           Opacity="0.6"
  220                           FontSize="12" />
  221              </StackPanel>
  222              <ToggleSwitch Grid.Column="1"
  223                            IsOn="True"
  224                            OnContent=""
  225                            OffContent="" />
  226            </Grid>
  227          </StackPanel>
  228        </Border>
  229  
  230        <!-- App Info -->
  231        <StackPanel Spacing="8" HorizontalAlignment="Center" Opacity="0.5">
  232          <TextBlock Text="Flux Transit v1.0"
  233                     Style="{StaticResource FluxBody}"
  234                     HorizontalAlignment="Center" />
  235          <TextBlock Text="Built with Uno Platform"
  236                     Style="{StaticResource FluxBody}"
  237                     HorizontalAlignment="Center" />
  238        </StackPanel>
  239  
  240      </StackPanel>
  241    </ScrollViewer>
  242  </Page>
```

### `FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml`

```xml
    1  <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    2                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    3                      xmlns:converters="using:FluxTransit.Converters">
    4  
    5      <!-- Value Converters -->
    6      <converters:RouteTypeColorConverter x:Key="RouteTypeColorConverter" />
    7      <converters:CrowdLevelConverter x:Key="CrowdLevelConverter" />
    8      <converters:NetworkHealthConverter x:Key="NetworkHealthConverter" />
    9      <converters:NetworkHealthColorConverter x:Key="NetworkHealthColorConverter" />
   10      <converters:AlertSeverityColorConverter x:Key="AlertSeverityColorConverter" />
   11      <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
   12      <converters:CountToVisibilityConverter x:Key="CountToVisibilityConverter" />
   13  
   14      <!-- Flux Transit Custom Colors and Brushes -->
   15  
   16      <!-- Core Flux Colors -->
   17      <Color x:Key="FluxBackground">#0f172a</Color>
   18      <Color x:Key="FluxSurface">#1e293b</Color>
   19      <Color x:Key="FluxGlassPanel">#1e293b66</Color>
   20      <Color x:Key="FluxGlassIndigo">#312e811a</Color>
   21      <Color x:Key="FluxBorderSubtle">#ffffff0d</Color>
   22      <Color x:Key="FluxBorderLight">#ffffff1a</Color>
   23  
   24      <!-- Semantic Colors -->
   25      <Color x:Key="FluxPrimary">#818cf8</Color>
   26      <Color x:Key="FluxPrimaryStrong">#4f46e5</Color>
   27      <Color x:Key="FluxSuccess">#34d399</Color>
   28      <Color x:Key="FluxWarning">#fbbf24</Color>
   29      <Color x:Key="FluxError">#fb7185</Color>
   30  
   31      <!-- Text Colors -->
   32      <Color x:Key="FluxTextPrimary">#ffffff</Color>
   33      <Color x:Key="FluxTextSecondary">#94a3b8</Color>
   34      <Color x:Key="FluxTextMuted">#64748b</Color>
   35  
   36      <!-- Core Brushes -->
   37      <SolidColorBrush x:Key="FluxBackgroundBrush" Color="{StaticResource FluxBackground}" />
   38      <SolidColorBrush x:Key="FluxSurfaceBrush" Color="{StaticResource FluxSurface}" />
   39      <SolidColorBrush x:Key="FluxGlassPanelBrush" Color="{StaticResource FluxGlassPanel}" />
   40      <SolidColorBrush x:Key="FluxGlassIndigoBrush" Color="{StaticResource FluxGlassIndigo}" />
   41      <SolidColorBrush x:Key="FluxBorderSubtleBrush" Color="{StaticResource FluxBorderSubtle}" />
   42      <SolidColorBrush x:Key="FluxBorderLightBrush" Color="{StaticResource FluxBorderLight}" />
   43  
   44      <!-- Semantic Brushes -->
   45      <SolidColorBrush x:Key="FluxPrimaryBrush" Color="{StaticResource FluxPrimary}" />
   46      <SolidColorBrush x:Key="FluxPrimaryStrongBrush" Color="{StaticResource FluxPrimaryStrong}" />
   47      <SolidColorBrush x:Key="FluxSuccessBrush" Color="{StaticResource FluxSuccess}" />
   48      <SolidColorBrush x:Key="FluxWarningBrush" Color="{StaticResource FluxWarning}" />
   49      <SolidColorBrush x:Key="FluxErrorBrush" Color="{StaticResource FluxError}" />
   50  
   51      <!-- Text Brushes -->
   52      <SolidColorBrush x:Key="FluxTextPrimaryBrush" Color="{StaticResource FluxTextPrimary}" />
   53      <SolidColorBrush x:Key="FluxTextSecondaryBrush" Color="{StaticResource FluxTextSecondary}" />
   54      <SolidColorBrush x:Key="FluxTextMutedBrush" Color="{StaticResource FluxTextMuted}" />
   55  
   56      <!-- Spacing Tokens -->
   57      <x:Double x:Key="FluxSpacingXS">4</x:Double>
   58      <x:Double x:Key="FluxSpacingS">8</x:Double>
   59      <x:Double x:Key="FluxSpacingM">16</x:Double>
   60      <x:Double x:Key="FluxSpacingL">24</x:Double>
   61      <x:Double x:Key="FluxSpacingXL">32</x:Double>
   62      <x:Double x:Key="FluxSpacingXXL">40</x:Double>
   63  
   64      <!-- Corner Radius -->
   65      <CornerRadius x:Key="FluxCornerRadiusSmall">8</CornerRadius>
   66      <CornerRadius x:Key="FluxCornerRadiusMedium">16</CornerRadius>
   67      <CornerRadius x:Key="FluxCornerRadiusLarge">24</CornerRadius>
   68      <CornerRadius x:Key="FluxCornerRadiusFull">9999</CornerRadius>
   69  
   70      <!-- Typography Styles -->
   71  
   72      <!-- H1: Greeting -->
   73      <Style x:Key="FluxHeadingLarge" TargetType="TextBlock">
   74          <Setter Property="FontSize" Value="36" />
   75          <Setter Property="FontWeight" Value="Bold" />
   76          <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
   77          <Setter Property="CharacterSpacing" Value="-20" />
   78      </Style>
   79  
   80      <!-- H2: Section Headers -->
   81      <Style x:Key="FluxHeadingSection" TargetType="TextBlock">
   82          <Setter Property="FontSize" Value="20" />
   83          <Setter Property="FontWeight" Value="Bold" />
   84          <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
   85      </Style>
   86  
   87      <!-- H3: Card Headers -->
   88      <Style x:Key="FluxHeadingCard" TargetType="TextBlock">
   89          <Setter Property="FontSize" Value="16" />
   90          <Setter Property="FontWeight" Value="SemiBold" />
   91          <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
   92      </Style>
   93  
   94      <!-- Body -->
   95      <Style x:Key="FluxBody" TargetType="TextBlock">
   96          <Setter Property="FontSize" Value="14" />
   97          <Setter Property="Foreground" Value="{StaticResource FluxTextSecondaryBrush}" />
   98      </Style>
   99  
  100      <!-- Body Bold -->
  101      <Style x:Key="FluxBodyBold" TargetType="TextBlock">
  102          <Setter Property="FontSize" Value="14" />
  103          <Setter Property="FontWeight" Value="Bold" />
  104          <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
  105      </Style>
  106  
  107      <!-- Micro (Labels) -->
  108      <Style x:Key="FluxMicro" TargetType="TextBlock">
  109          <Setter Property="FontSize" Value="10" />
  110          <Setter Property="FontWeight" Value="Bold" />
  111          <Setter Property="CharacterSpacing" Value="80" />
  112          <Setter Property="Foreground" Value="{StaticResource FluxTextMutedBrush}" />
  113      </Style>
  114  
  115      <!-- Glass Panel Style -->
  116      <Style x:Key="FluxGlassPanelStyle" TargetType="Border">
  117          <Setter Property="Background" Value="{StaticResource FluxGlassPanelBrush}" />
  118          <Setter Property="BorderBrush" Value="{StaticResource FluxBorderSubtleBrush}" />
  119          <Setter Property="BorderThickness" Value="1" />
  120          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusLarge}" />
  121          <Setter Property="Padding" Value="24" />
  122      </Style>
  123  
  124      <!-- Status Pill Style -->
  125      <Style x:Key="FluxStatusPillStyle" TargetType="Border">
  126          <Setter Property="Background" Value="{StaticResource FluxGlassPanelBrush}" />
  127          <Setter Property="BorderBrush" Value="{StaticResource FluxBorderSubtleBrush}" />
  128          <Setter Property="BorderThickness" Value="1" />
  129          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusFull}" />
  130          <Setter Property="Padding" Value="12,8" />
  131      </Style>
  132  
  133      <!-- Transit Card Style -->
  134      <Style x:Key="FluxTransitCardStyle" TargetType="Border">
  135          <Setter Property="Background" Value="{StaticResource FluxGlassPanelBrush}" />
  136          <Setter Property="BorderBrush" Value="{StaticResource FluxBorderSubtleBrush}" />
  137          <Setter Property="BorderThickness" Value="1" />
  138          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusMedium}" />
  139          <Setter Property="Padding" Value="16" />
  140      </Style>
  141  
  142      <!-- Progress Bar Style for Vehicle Position -->
  143      <Style x:Key="FluxProgressBarStyle" TargetType="ProgressBar">
  144          <Setter Property="Height" Value="6" />
  145          <Setter Property="Background" Value="#334155" />
  146          <Setter Property="Foreground" Value="{StaticResource FluxPrimaryBrush}" />
  147          <Setter Property="CornerRadius" Value="3" />
  148      </Style>
  149  
  150      <!-- Primary Button Style -->
  151      <Style x:Key="FluxPrimaryButtonStyle" TargetType="Button">
  152          <Setter Property="Background" Value="{StaticResource FluxPrimaryBrush}" />
  153          <Setter Property="Foreground" Value="{StaticResource FluxBackgroundBrush}" />
  154          <Setter Property="FontWeight" Value="SemiBold" />
  155          <Setter Property="Padding" Value="24,12" />
  156          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusFull}" />
  157      </Style>
  158  
  159      <!-- Icon Button Style -->
  160      <Style x:Key="FluxIconButtonStyle" TargetType="Button">
  161          <Setter Property="Background" Value="{StaticResource FluxGlassPanelBrush}" />
  162          <Setter Property="Foreground" Value="{StaticResource FluxTextPrimaryBrush}" />
  163          <Setter Property="BorderBrush" Value="{StaticResource FluxBorderSubtleBrush}" />
  164          <Setter Property="BorderThickness" Value="1" />
  165          <Setter Property="Padding" Value="12" />
  166          <Setter Property="MinWidth" Value="44" />
  167          <Setter Property="MinHeight" Value="44" />
  168          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusFull}" />
  169      </Style>
  170  
  171      <!-- Secondary Button Style -->
  172      <Style x:Key="FluxSecondaryButtonStyle" TargetType="Button">
  173          <Setter Property="Background" Value="Transparent" />
  174          <Setter Property="Foreground" Value="{StaticResource FluxPrimaryBrush}" />
  175          <Setter Property="BorderBrush" Value="{StaticResource FluxPrimaryBrush}" />
  176          <Setter Property="BorderThickness" Value="1.5" />
  177          <Setter Property="FontWeight" Value="SemiBold" />
  178          <Setter Property="Padding" Value="24,12" />
  179          <Setter Property="CornerRadius" Value="{StaticResource FluxCornerRadiusFull}" />
  180      </Style>
  181  
  182      <!-- Alert Badge Style -->
  183      <Style x:Key="FluxAlertBadgeStyle" TargetType="Border">
  184          <Setter Property="Background" Value="{StaticResource FluxErrorBrush}" />
  185          <Setter Property="CornerRadius" Value="8" />
  186          <Setter Property="Padding" Value="8,4" />
  187          <Setter Property="HorizontalAlignment" Value="Left" />
  188      </Style>
  189  
  190      <!-- Success Badge Style -->
  191      <Style x:Key="FluxSuccessBadgeStyle" TargetType="Border">
  192          <Setter Property="Background" Value="{StaticResource FluxSuccessBrush}" />
  193          <Setter Property="CornerRadius" Value="8" />
  194          <Setter Property="Padding" Value="8,4" />
  195          <Setter Property="HorizontalAlignment" Value="Left" />
  196      </Style>
  197  
  198      <!-- Subtle Divider Style -->
  199      <Style x:Key="FluxDividerStyle" TargetType="Border">
  200          <Setter Property="Height" Value="1" />
  201          <Setter Property="Background" Value="{StaticResource FluxBorderSubtleBrush}" />
  202          <Setter Property="HorizontalAlignment" Value="Stretch" />
  203      </Style>
  204  
  205  </ResourceDictionary>
```

### `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs`

```csharp
    1  namespace FluxTransit.Presentation;
    2  
    3  public partial record ProfileModel
    4  {
    5      private readonly INavigator _navigator;
    6      private readonly IStringLocalizer _localizer;
    7  
    8      public ProfileModel(INavigator navigator, IStringLocalizer localizer)
    9      {
   10          _navigator = navigator;
   11          _localizer = localizer;
   12      }
   13  
   14      // User info
   15      public string UserName => "Commuter";
   16      public string CardNumber => "**** 4521";
   17  
   18      // OPUS Balance state
   19      public IState<decimal> OpusBalance => State<decimal>.Value(this, () => 18.50m);
   20  
   21      // Gemini API Key state
   22      public IState<string> GeminiApiKey => State<string>.Value(this, () => string.Empty);
   23  
   24      // Language selection (EN or FR)
   25      public IState<string> SelectedLanguage => State<string>.Value(this, () => "EN");
   26  
   27      // Is refreshing balance
   28      public IState<bool> IsRefreshing => State<bool>.Value(this, () => false);
   29  
   30      // Navigate back command
   31      public async Task GoBack()
   32      {
   33          await _navigator.GoBack(this);
   34      }
   35  
   36      // Update balance command - simulates a refresh
   37      public async Task UpdateBalance(CancellationToken ct)
   38      {
   39          await IsRefreshing.Set(true, ct);
   40          try
   41          {
   42              // Simulate API call
   43              await Task.Delay(1000, ct);
   44              // Update with a slightly random balance
   45              var random = new Random();
   46              var newBalance = 15.00m + (decimal)random.NextDouble() * 10;
   47              await OpusBalance.Set(Math.Round(newBalance, 2), ct);
   48          }
   49          finally
   50          {
   51              await IsRefreshing.Set(false, ct);
   52          }
   53      }
   54  
   55      // Save settings command
   56      public async Task SaveSettings(CancellationToken ct)
   57      {
   58          var apiKey = await GeminiApiKey;
   59          var language = await SelectedLanguage;
   60  
   61          // In a real app, this would save to localStorage or preferences
   62          // For now, just log that settings were saved
   63          await Task.Delay(100, ct);
   64      }
   65  }
```

### `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml.cs`

```csharp
    1  namespace FluxTransit.Presentation;
    2  
    3  public sealed partial class ProfilePage : Page
    4  {
    5      public ProfilePage()
    6      {
    7          this.InitializeComponent();
    8      }
    9  }
```
