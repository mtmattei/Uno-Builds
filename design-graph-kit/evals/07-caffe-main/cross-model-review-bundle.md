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

# The answer key under review: `07-caffe-main`

48 nodes · 67 edges · 1 unresolved items

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
  "graphId": "eval.caffe-main",
  "name": "Caffe Main",
  "description": "Gold graph for the Caffe MainPage (source-backed MVVM: CommunityToolkit ObservableObject/RelayCommand, x:Bind). Kit v0.5 altitude. Gold v1.1: calibrated to the eval-07 5/5 blind consensus (screen slug, UserControl=component, per-site states, canonical trigger attachment, token breadth).",
  "sourceSummary": [
    {
      "type": "xaml",
      "path": "Caffe/Caffe/MainPage.xaml"
    },
    {
      "type": "csharp",
      "path": "Caffe/Caffe/MainPage.xaml.cs"
    },
    {
      "type": "csharp",
      "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
    },
    {
      "type": "csharp",
      "path": "Caffe/Caffe/Models/EspressoItem.cs"
    },
    {
      "type": "xaml",
      "path": "Caffe/Caffe/Controls/"
    },
    {
      "type": "design-system",
      "path": "Caffe/Caffe/Styles/AppResources.xaml"
    }
  ],
  "nodes": [
    {
      "id": "screen.caffe-main",
      "type": "screen",
      "name": "Caffe main (brew)",
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Caffe.MainPage",
          "viewModel": "Caffe.ViewModels.MainViewModel (CommunityToolkit.Mvvm)"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "id": "component.caffe-header",
      "type": "component",
      "name": "Caffe header",
      "role": "pageHeader",
      "properties": {
        "parts": [
          "accent-bar",
          "logo",
          "tagline"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.CaffeHeader"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "id": "asset.header.accent-bar",
      "type": "asset",
      "name": "Accent bar",
      "role": "decoration",
      "semanticRole": "brandAccent",
      "properties": {
        "colors": [
          "primary",
          "accent-red"
        ]
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        },
        "rationale": "Two-color 56x4 bar above the logo."
      }
    },
    {
      "id": "content.header.logo",
      "type": "content",
      "name": "Logo",
      "text": "Caff\u00e8",
      "semanticRole": "appName",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "LogoTextStyle"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "id": "content.header.tagline",
      "type": "content",
      "name": "Tagline",
      "text": "NOTHING MORE. NOTHING LESS.",
      "semanticRole": "tagline",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "TaglineTextStyle"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "id": "component.caffe-footer",
      "type": "component",
      "name": "Caffe footer",
      "role": "pageFooter",
      "properties": {
        "parts": [
          "accent-bar"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.CaffeFooter"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "id": "region.caffe-main.menu",
      "type": "region",
      "name": "Espresso menu",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "2x2 grid of espresso cards."
      }
    },
    {
      "id": "component.espresso-card",
      "type": "component",
      "name": "Espresso card",
      "role": "card",
      "properties": {
        "parts": [
          "name",
          "volume-badge",
          "description"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.EspressoCard"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        },
        "rationale": "Reusable card bound to an EspressoItem (Name, VolumeML, Description); IsSelected visual."
      }
    },
    {
      "id": "component.espresso-card.espresso",
      "type": "component",
      "name": "Espresso",
      "properties": {
        "name": "Espresso",
        "volumeML": 30,
        "description": "Pure, concentrated, bold",
        "uno": {
          "xName": "EspressoCard"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Bound to ViewModel.EspressoItems; Tapped selects this espresso."
      }
    },
    {
      "id": "component.espresso-card.doppio",
      "type": "component",
      "name": "Doppio",
      "properties": {
        "name": "Doppio",
        "volumeML": 60,
        "description": "Double the intensity",
        "uno": {
          "xName": "DoppioCard"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Bound to ViewModel.EspressoItems; Tapped selects this espresso."
      }
    },
    {
      "id": "component.espresso-card.ristretto",
      "type": "component",
      "name": "Ristretto",
      "properties": {
        "name": "Ristretto",
        "volumeML": 20,
        "description": "Short, sweet, powerful",
        "uno": {
          "xName": "RistrettoCard"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Bound to ViewModel.EspressoItems; Tapped selects this espresso."
      }
    },
    {
      "id": "component.espresso-card.lungo",
      "type": "component",
      "name": "Lungo",
      "properties": {
        "name": "Lungo",
        "volumeML": 50,
        "description": "Long pull, smooth finish",
        "uno": {
          "xName": "LungoCard"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Bound to ViewModel.EspressoItems; Tapped selects this espresso."
      }
    },
    {
      "id": "region.caffe-main.parameters",
      "type": "region",
      "name": "Brew parameters",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Three-column parameter panel."
      }
    },
    {
      "id": "component.temperature-gauge",
      "type": "component",
      "name": "Temperature",
      "role": "gauge",
      "semanticRole": "temperatureInput",
      "properties": {
        "value": "{x:Bind ViewModel.Temperature, Mode=TwoWay}",
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.TemperatureGauge",
          "xName": "TempGauge"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "id": "component.extraction-arc",
      "type": "component",
      "name": "Extraction time",
      "role": "dial",
      "semanticRole": "extractionTimeInput",
      "properties": {
        "value": "{x:Bind ViewModel.ExtractionTime, Mode=TwoWay}",
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.ExtractionArc",
          "xName": "ExtractionArc"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "id": "component.grind-selector",
      "type": "component",
      "name": "Grind level",
      "role": "selector",
      "semanticRole": "grindLevelInput",
      "properties": {
        "value": "{x:Bind ViewModel.GrindLevel, Mode=TwoWay}",
        "options": [
          "Fine",
          "Medium",
          "Coarse"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.GrindSelector",
          "xName": "GrindSelector"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        },
        "rationale": "GrindLevel enum clamped 0-2; labels via ToLabel."
      }
    },
    {
      "id": "component.selection-overview",
      "type": "component",
      "name": "Selection overview",
      "role": "summary",
      "semanticRole": "selectionSummary",
      "properties": {
        "parts": [
          "espresso-name",
          "temperature",
          "grind",
          "extraction-time"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.SelectionOverview",
          "xName": "SelectionOverview"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "Visible only while HasSelection."
      }
    },
    {
      "id": "component.brew-button",
      "type": "component",
      "name": "Brew",
      "role": "button",
      "semanticRole": "primaryAction",
      "properties": {
        "text": "{x:Bind ViewModel.BrewButtonText}",
        "command": "BrewCommand (RelayCommand, CanExecute=HasSelection)",
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.BrewButton",
          "xName": "BrewBtn"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/MainPage.xaml.cs"
        },
        "rationale": "BrewRequested -> BrewCommand.ExecuteAsync when CanExecute."
      }
    },
    {
      "id": "state.espresso-card.selected",
      "type": "state",
      "name": "Card selected",
      "semanticRole": "selected",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "HasSelection / IsSelected"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        },
        "rationale": "Selected card gets IsSelected visual via code-behind sync."
      }
    },
    {
      "id": "state.brew-button.disabled",
      "type": "state",
      "name": "Brew disabled",
      "semanticRole": "disabled",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "HasSelection (RelayCommand CanExecute)"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        },
        "rationale": "BrewCommand CanExecute=HasSelection; button disabled until a card is selected."
      }
    },
    {
      "id": "state.selection-overview.hidden",
      "type": "state",
      "name": "Overview hidden",
      "semanticRole": "hidden",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "HasSelection"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        },
        "rationale": "Overview Visibility bound to HasSelection; hidden until selection."
      }
    },
    {
      "id": "state.caffe-main.brewing",
      "type": "state",
      "name": "Brewing",
      "semanticRole": "busy",
      "properties": {
        "uno": {
          "mechanism": "binding",
          "member": "IsBrewing"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        },
        "rationale": "BrewCommand sets IsBrewing; MainContent hides, BrewingScreen overlay x:Loads; ~2.5s simulated progress."
      }
    },
    {
      "id": "component.brewing-screen",
      "type": "component",
      "name": "Brewing overlay",
      "role": "overlay",
      "semanticRole": "progressOverlay",
      "properties": {
        "parts": [
          "espresso-name",
          "parameters-text",
          "progress"
        ],
        "bindings": [
          "EspressoName",
          "ParametersText",
          "BrewProgress"
        ],
        "uno": {
          "type": "UserControl",
          "class": "Caffe.Controls.BrewingScreen",
          "xName": "BrewingOverlay"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "id": "token.color.background",
      "type": "token",
      "name": "Background",
      "category": "color",
      "value": "#FAFAFA",
      "properties": {
        "uno": {
          "resourceKey": "CaffeBackgroundBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.surface",
      "type": "token",
      "name": "Surface",
      "category": "color",
      "value": "#FFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "CaffeSurfaceBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.primary",
      "type": "token",
      "name": "Primary (espresso green)",
      "category": "color",
      "value": "#1B4332",
      "properties": {
        "uno": {
          "resourceKey": "CaffePrimaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.accent-red",
      "type": "token",
      "name": "Accent red",
      "category": "color",
      "value": "#C1121F",
      "properties": {
        "uno": {
          "resourceKey": "CaffeAccentRedBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.accent-green",
      "type": "token",
      "name": "Accent green",
      "category": "color",
      "value": "#2D6A4F",
      "properties": {
        "uno": {
          "resourceKey": "CaffeAccentGreenBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.text-primary",
      "type": "token",
      "name": "Text primary",
      "category": "color",
      "value": "#1A1A1A",
      "properties": {
        "uno": {
          "resourceKey": "CaffeTextPrimaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.text-secondary",
      "type": "token",
      "name": "Text secondary",
      "category": "color",
      "value": "#888888",
      "properties": {
        "uno": {
          "resourceKey": "CaffeTextSecondaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.text-muted",
      "type": "token",
      "name": "Text muted",
      "category": "color",
      "value": "#999999",
      "properties": {
        "uno": {
          "resourceKey": "CaffeTextMutedBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.border",
      "type": "token",
      "name": "Border",
      "category": "color",
      "value": "#E0E0E0",
      "properties": {
        "uno": {
          "resourceKey": "CaffeBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.color.on-primary",
      "type": "token",
      "name": "On primary",
      "category": "color",
      "value": "#FFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "CaffeOnPrimaryBrush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.logo",
      "type": "token",
      "name": "Logo type",
      "category": "typography",
      "value": {
        "family": "Cormorant Light",
        "size": 48
      },
      "properties": {
        "uno": {
          "styleKey": "LogoTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.tagline",
      "type": "token",
      "name": "Tagline type",
      "category": "typography",
      "value": {
        "family": "DM Sans Medium",
        "size": 11,
        "tracking": 150
      },
      "properties": {
        "uno": {
          "styleKey": "TaglineTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.card-title",
      "type": "token",
      "name": "Card title type",
      "category": "typography",
      "value": {
        "family": "Cormorant",
        "size": 22
      },
      "properties": {
        "uno": {
          "styleKey": "CardTitleTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.card-description",
      "type": "token",
      "name": "Card description type",
      "category": "typography",
      "value": {
        "family": "DM Sans",
        "size": 12
      },
      "properties": {
        "uno": {
          "styleKey": "CardDescriptionTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.volume-badge",
      "type": "token",
      "name": "Volume badge type",
      "category": "typography",
      "value": {
        "family": "DM Sans Medium",
        "size": 11
      },
      "properties": {
        "uno": {
          "styleKey": "VolumeBadgeTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.parameter-value",
      "type": "token",
      "name": "Parameter value type",
      "category": "typography",
      "value": {
        "family": "Cormorant",
        "size": 26
      },
      "properties": {
        "uno": {
          "styleKey": "ParameterValueTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.parameter-label",
      "type": "token",
      "name": "Parameter label type",
      "category": "typography",
      "value": {
        "family": "DM Sans Medium",
        "size": 10,
        "tracking": 100
      },
      "properties": {
        "uno": {
          "styleKey": "ParameterLabelTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.button",
      "type": "token",
      "name": "Button type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "ButtonTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.grind-hint",
      "type": "token",
      "name": "Grind hint type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "GrindHintTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.overview-label",
      "type": "token",
      "name": "Overview label type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "OverviewLabelTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.overview-value",
      "type": "token",
      "name": "Overview value type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "OverviewValueTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.brewing-title",
      "type": "token",
      "name": "Brewing title type",
      "category": "typography",
      "value": {
        "family": "Cormorant"
      },
      "properties": {
        "uno": {
          "styleKey": "BrewingTitleTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.body",
      "type": "token",
      "name": "Body type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "BodyTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.typography.arc-label",
      "type": "token",
      "name": "Arc label type",
      "category": "typography",
      "value": {
        "family": "DM Sans"
      },
      "properties": {
        "uno": {
          "styleKey": "ArcLabelTextStyle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        }
      }
    },
    {
      "id": "token.radius.14",
      "type": "token",
      "name": "14 radius",
      "category": "radius",
      "value": 14,
      "properties": {
        "unit": "px"
      },
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        },
        "rationale": "Recurs across EspressoCard, BrewButton, SelectionOverview."
      }
    }
  ],
  "edges": [
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "component.caffe-header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.caffe-header",
      "relation": "contains",
      "to": "asset.header.accent-bar",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "from": "component.caffe-header",
      "relation": "contains",
      "to": "content.header.logo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "from": "component.caffe-header",
      "relation": "contains",
      "to": "content.header.tagline",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/Controls/"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "region.caffe-main.menu",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "region.caffe-main.parameters",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "component.brew-button",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "component.caffe-footer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.parameters",
      "relation": "contains",
      "to": "component.temperature-gauge",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.parameters",
      "relation": "contains",
      "to": "component.extraction-arc",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.parameters",
      "relation": "contains",
      "to": "component.grind-selector",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "has-state",
      "to": "state.caffe-main.brewing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        }
      }
    },
    {
      "from": "screen.caffe-main",
      "relation": "contains",
      "to": "component.selection-overview",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "has-state",
      "to": "state.espresso-card.selected",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        }
      }
    },
    {
      "from": "component.brew-button",
      "relation": "has-state",
      "to": "state.brew-button.disabled",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        }
      }
    },
    {
      "from": "component.selection-overview",
      "relation": "has-state",
      "to": "state.selection-overview.hidden",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"
        }
      }
    },
    {
      "from": "state.caffe-main.brewing",
      "relation": "contains",
      "to": "component.brewing-screen",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        },
        "rationale": "x:Load bound to IsBrewing."
      }
    },
    {
      "from": "component.brew-button",
      "relation": "triggers",
      "to": "state.caffe-main.brewing",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/MainPage.xaml.cs"
        },
        "rationale": "BrewRequested -> BrewCommand -> IsBrewing=true for the simulated brew."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "triggers",
      "to": "state.espresso-card.selected",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Caffe/Caffe/MainPage.xaml.cs"
        },
        "rationale": "Every card's Tapped handler selects its espresso (canonical attachment; instances identical)."
      }
    },
    {
      "from": "region.caffe-main.menu",
      "relation": "contains",
      "to": "component.espresso-card.espresso",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.espresso-card.espresso",
      "relation": "instance-of",
      "to": "component.espresso-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.menu",
      "relation": "contains",
      "to": "component.espresso-card.doppio",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.espresso-card.doppio",
      "relation": "instance-of",
      "to": "component.espresso-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.menu",
      "relation": "contains",
      "to": "component.espresso-card.ristretto",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.espresso-card.ristretto",
      "relation": "instance-of",
      "to": "component.espresso-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "region.caffe-main.menu",
      "relation": "contains",
      "to": "component.espresso-card.lungo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "component.espresso-card.lungo",
      "relation": "instance-of",
      "to": "component.espresso-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Caffe/Caffe/MainPage.xaml"
        }
      }
    },
    {
      "from": "screen.caffe-main",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "content.header.logo",
      "relation": "uses-token",
      "to": "token.typography.logo",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "content.header.tagline",
      "relation": "uses-token",
      "to": "token.typography.tagline",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "asset.header.accent-bar",
      "relation": "uses-token",
      "to": "token.color.primary",
      "properties": {
        "appliesTo": "leftColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "asset.header.accent-bar",
      "relation": "uses-token",
      "to": "token.color.accent-red",
      "properties": {
        "appliesTo": "rightColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.caffe-footer",
      "relation": "uses-token",
      "to": "token.color.primary",
      "properties": {
        "appliesTo": "accentColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.caffe-footer",
      "relation": "uses-token",
      "to": "token.color.accent-red",
      "properties": {
        "appliesTo": "accentColor2"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.color.border",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.radius.14",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.color.primary",
      "properties": {
        "appliesTo": "selectionAccent"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.typography.card-title",
      "properties": {
        "appliesTo": "nameFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.typography.card-description",
      "properties": {
        "appliesTo": "descriptionFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.typography.volume-badge",
      "properties": {
        "appliesTo": "badgeFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.color.on-primary",
      "properties": {
        "appliesTo": "badgeForeground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.temperature-gauge",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.temperature-gauge",
      "relation": "uses-token",
      "to": "token.typography.parameter-value",
      "properties": {
        "appliesTo": "valueFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.temperature-gauge",
      "relation": "uses-token",
      "to": "token.typography.parameter-label",
      "properties": {
        "appliesTo": "labelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.extraction-arc",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.extraction-arc",
      "relation": "uses-token",
      "to": "token.typography.parameter-value",
      "properties": {
        "appliesTo": "valueFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.extraction-arc",
      "relation": "uses-token",
      "to": "token.typography.parameter-label",
      "properties": {
        "appliesTo": "labelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.extraction-arc",
      "relation": "uses-token",
      "to": "token.color.accent-green",
      "properties": {
        "appliesTo": "arcColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.grind-selector",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.grind-selector",
      "relation": "uses-token",
      "to": "token.typography.parameter-value",
      "properties": {
        "appliesTo": "valueFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.grind-selector",
      "relation": "uses-token",
      "to": "token.typography.parameter-label",
      "properties": {
        "appliesTo": "labelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.selection-overview",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.selection-overview",
      "relation": "uses-token",
      "to": "token.radius.14",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brew-button",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brew-button",
      "relation": "uses-token",
      "to": "token.radius.14",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brewing-screen",
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
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brewing-screen",
      "relation": "uses-token",
      "to": "token.color.text-primary",
      "properties": {
        "appliesTo": "titleColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brewing-screen",
      "relation": "uses-token",
      "to": "token.color.text-secondary",
      "properties": {
        "appliesTo": "parametersColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.espresso-card",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "properties": {
        "appliesTo": "descriptionColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brew-button",
      "relation": "uses-token",
      "to": "token.typography.button",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.grind-selector",
      "relation": "uses-token",
      "to": "token.typography.grind-hint",
      "properties": {
        "appliesTo": "hintFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.selection-overview",
      "relation": "uses-token",
      "to": "token.typography.overview-label",
      "properties": {
        "appliesTo": "labelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.selection-overview",
      "relation": "uses-token",
      "to": "token.typography.overview-value",
      "properties": {
        "appliesTo": "valueFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brewing-screen",
      "relation": "uses-token",
      "to": "token.typography.brewing-title",
      "properties": {
        "appliesTo": "titleFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.brewing-screen",
      "relation": "uses-token",
      "to": "token.typography.body",
      "properties": {
        "appliesTo": "bodyFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    },
    {
      "from": "component.extraction-arc",
      "relation": "uses-token",
      "to": "token.typography.arc-label",
      "properties": {
        "appliesTo": "labelFont"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "path": "Caffe/Caffe/Styles/AppResources.xaml"
        },
        "rationale": "Audited StaticResource consumption in the page/controls."
      }
    }
  ],
  "unresolved": [
    {
      "id": "unresolved.menu.data-source",
      "question": "Is the four-espresso menu fixed, or intended to become data-driven?",
      "relatedIds": [
        "region.caffe-main.menu"
      ],
      "possibleValues": [
        "fixed product menu",
        "placeholder for a data source"
      ],
      "reason": "EspressoItems is a hard-coded ObservableCollection in the ViewModel; no service or persistence is referenced."
    }
  ]
}
```

## Source files the gold cites

Line-numbered so you can cite `file:line`.

### `Caffe/Caffe/MainPage.xaml`

```xml
    1  <Page x:Class="Caffe.MainPage"
    2        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4        xmlns:local="using:Caffe"
    5        xmlns:controls="using:Caffe.Controls"
    6        xmlns:models="using:Caffe.Models"
    7        xmlns:utu="using:Uno.Toolkit.UI"
    8        Background="{StaticResource CaffeBackgroundBrush}">
    9  
   10      <Grid>
   11          <!-- Main Content (hidden during brewing) -->
   12          <Grid x:Name="MainContent"
   13                Visibility="{x:Bind ViewModel.IsBrewing, Mode=OneWay, Converter={StaticResource ReverseBoolToVisibilityConverter}}">
   14  
   15              <ScrollViewer>
   16                  <utu:AutoLayout
   17                      Orientation="Vertical"
   18                      PrimaryAxisAlignment="Start"
   19                      CounterAxisAlignment="Center"
   20                      MaxWidth="{utu:Responsive Narrow=480, Normal=640, Wide=960}"
   21                      Padding="{utu:Responsive Narrow='16,0', Normal='24,0', Wide='40,0'}">
   22  
   23                      <!-- Header -->
   24                      <controls:CaffeHeader />
   25  
   26                      <!-- Responsive Content Grid -->
   27                      <Grid ColumnSpacing="{utu:Responsive Narrow=0, Wide=24}">
   28                          <Grid.ColumnDefinitions>
   29                              <ColumnDefinition Width="*" />
   30                              <ColumnDefinition Width="{utu:Responsive Narrow=0, Wide=*}" />
   31                          </Grid.ColumnDefinitions>
   32                          <Grid.RowDefinitions>
   33                              <RowDefinition Height="Auto" />
   34                              <RowDefinition Height="Auto" />
   35                          </Grid.RowDefinitions>
   36  
   37                          <!-- Espresso Card Grid (always col 0, row 0) -->
   38                          <Grid Grid.Row="0" Grid.Column="0"
   39                                Margin="0,8,0,16">
   40                              <Grid.RowDefinitions>
   41                                  <RowDefinition Height="Auto" />
   42                                  <RowDefinition Height="{utu:Responsive Narrow=8, Wide=12}" />
   43                                  <RowDefinition Height="Auto" />
   44                              </Grid.RowDefinitions>
   45                              <Grid.ColumnDefinitions>
   46                                  <ColumnDefinition Width="*" />
   47                                  <ColumnDefinition Width="{utu:Responsive Narrow=8, Wide=12}" />
   48                                  <ColumnDefinition Width="*" />
   49                              </Grid.ColumnDefinitions>
   50  
   51                              <!-- Espresso -->
   52                              <controls:EspressoCard x:Name="EspressoCard"
   53                                                     Grid.Row="0" Grid.Column="0"
   54                                                     Espresso="{x:Bind ViewModel.EspressoItems[0]}"
   55                                                     Tapped="OnEspressoCardTapped" />
   56  
   57                              <!-- Doppio -->
   58                              <controls:EspressoCard x:Name="DoppioCard"
   59                                                     Grid.Row="0" Grid.Column="2"
   60                                                     Espresso="{x:Bind ViewModel.EspressoItems[1]}"
   61                                                     Tapped="OnDoppioCardTapped" />
   62  
   63                              <!-- Ristretto -->
   64                              <controls:EspressoCard x:Name="RistrettoCard"
   65                                                     Grid.Row="2" Grid.Column="0"
   66                                                     Espresso="{x:Bind ViewModel.EspressoItems[2]}"
   67                                                     Tapped="OnRistrettoCardTapped" />
   68  
   69                              <!-- Lungo -->
   70                              <controls:EspressoCard x:Name="LungoCard"
   71                                                     Grid.Row="2" Grid.Column="2"
   72                                                     Espresso="{x:Bind ViewModel.EspressoItems[3]}"
   73                                                     Tapped="OnLungoCardTapped" />
   74                          </Grid>
   75  
   76                          <!-- Right Panel: Parameters + Overview + Brew -->
   77                          <utu:AutoLayout
   78                              Orientation="Vertical"
   79                              Spacing="0"
   80                              Grid.Column="{utu:Responsive Narrow=0, Wide=1}"
   81                              Grid.Row="{utu:Responsive Narrow=1, Wide=0}"
   82                              Grid.RowSpan="{utu:Responsive Narrow=1, Wide=2}">
   83  
   84                              <!-- Parameters Panel -->
   85                              <Grid ColumnSpacing="{utu:Responsive Narrow=6, Normal=10, Wide=16}"
   86                                    Margin="0,0,0,16">
   87                                  <Grid.ColumnDefinitions>
   88                                      <ColumnDefinition Width="*" />
   89                                      <ColumnDefinition Width="*" />
   90                                      <ColumnDefinition Width="*" />
   91                                  </Grid.ColumnDefinitions>
   92  
   93                                  <controls:TemperatureGauge x:Name="TempGauge"
   94                                                             Grid.Column="0"
   95                                                             VerticalAlignment="Stretch"
   96                                                             Temperature="{x:Bind ViewModel.Temperature, Mode=TwoWay}" />
   97  
   98                                  <controls:ExtractionArc x:Name="ExtractionArc"
   99                                                          Grid.Column="1"
  100                                                          VerticalAlignment="Stretch"
  101                                                          ExtractionTime="{x:Bind ViewModel.ExtractionTime, Mode=TwoWay}" />
  102  
  103                                  <controls:GrindSelector x:Name="GrindSelector"
  104                                                          Grid.Column="2"
  105                                                          VerticalAlignment="Stretch"
  106                                                          GrindLevel="{x:Bind ViewModel.GrindLevel, Mode=TwoWay}" />
  107                              </Grid>
  108  
  109                              <!-- Selection Overview (visible when selected) -->
  110                              <controls:SelectionOverview x:Name="SelectionOverview"
  111                                                          Margin="0,0,0,20"
  112                                                          Visibility="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
  113                                                          EspressoName="{x:Bind ViewModel.SelectedEspresso.Name, Mode=OneWay, FallbackValue='Espresso'}"
  114                                                          Temperature="{x:Bind ViewModel.Temperature, Mode=OneWay}"
  115                                                          GrindAbbreviation="{x:Bind ViewModel.GrindAbbreviation, Mode=OneWay}"
  116                                                          ExtractionTime="{x:Bind ViewModel.ExtractionTime, Mode=OneWay}" />
  117  
  118                              <!-- Brew Button -->
  119                              <controls:BrewButton x:Name="BrewBtn"
  120                                                   Margin="0,0,0,16"
  121                                                   Text="{x:Bind ViewModel.BrewButtonText, Mode=OneWay}"
  122                                                   IsBrewEnabled="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
  123                                                   BrewRequested="OnBrewRequested" />
  124  
  125                          </utu:AutoLayout>
  126                      </Grid>
  127  
  128                      <!-- Footer -->
  129                      <controls:CaffeFooter />
  130  
  131                  </utu:AutoLayout>
  132              </ScrollViewer>
  133          </Grid>
  134  
  135          <!-- Brewing Screen Overlay -->
  136          <controls:BrewingScreen x:Name="BrewingOverlay"
  137                                  x:Load="{x:Bind ViewModel.IsBrewing, Mode=OneWay}"
  138                                  EspressoName="{x:Bind ViewModel.SelectedEspresso.Name, Mode=OneWay, FallbackValue='Espresso'}"
  139                                  ParametersText="{x:Bind ViewModel.BrewingParametersText, Mode=OneWay}"
  140                                  BrewProgress="{x:Bind ViewModel.BrewProgress, Mode=OneWay}" />
  141      </Grid>
  142  
  143  </Page>
```

### `Caffe/Caffe/Controls/`

```csharp
    1  --- Caffe/Caffe/Controls/BrewButton.xaml ---
    2  <UserControl
    3      x:Class="Caffe.Controls.BrewButton"
    4      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    5      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    6  
    7      <Button x:Name="MainButton"
    8              HorizontalAlignment="Stretch"
    9              Height="56"
   10              CornerRadius="14"
   11              Click="OnClick">
   12          <Button.Resources>
   13              <Style TargetType="Button">
   14                  <Setter Property="Background" Value="{StaticResource CaffePrimaryBrush}" />
   15                  <Setter Property="Foreground" Value="White" />
   16                  <Setter Property="BorderThickness" Value="0" />
   17              </Style>
   18          </Button.Resources>
   19          <TextBlock x:Name="ButtonText"
   20                     Text="Select your espresso"
   21                     Style="{StaticResource ButtonTextStyle}" />
   22      </Button>
   23  
   24  </UserControl>
   25  
   26  
   27  --- Caffe/Caffe/Controls/BrewButton.xaml.cs ---
   28  namespace Caffe.Controls;
   29  
   30  public sealed partial class BrewButton : UserControl
   31  {
   32      private readonly SolidColorBrush _enabledBrush;
   33      private readonly SolidColorBrush _disabledBrush;
   34  
   35      public static readonly DependencyProperty TextProperty =
   36          DependencyProperty.Register(nameof(Text), typeof(string), typeof(BrewButton),
   37              new PropertyMetadata("Select your espresso", OnTextChanged));
   38  
   39      public static readonly DependencyProperty IsBrewEnabledProperty =
   40          DependencyProperty.Register(nameof(IsBrewEnabled), typeof(bool), typeof(BrewButton),
   41              new PropertyMetadata(false, OnIsBrewEnabledChanged));
   42  
   43      public string Text
   44      {
   45          get => (string)GetValue(TextProperty);
   46          set => SetValue(TextProperty, value);
   47      }
   48  
   49      public bool IsBrewEnabled
   50      {
   51          get => (bool)GetValue(IsBrewEnabledProperty);
   52          set => SetValue(IsBrewEnabledProperty, value);
   53      }
   54  
   55      public event EventHandler? BrewRequested;
   56  
   57      public BrewButton()
   58      {
   59          this.InitializeComponent();
   60  
   61          _enabledBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"];
   62          _disabledBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryDisabledBrush"];
   63  
   64          UpdateVisual(false);
   65      }
   66  
   67      private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   68      {
   69          if (d is BrewButton button)
   70          {
   71              var text = (string)e.NewValue;
   72              button.ButtonText.Text = text;
   73              Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, text);
   74          }
   75      }
   76  
   77      private static void OnIsBrewEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   78      {
   79          if (d is BrewButton button)
   80              button.UpdateVisual((bool)e.NewValue);
   81      }
   82  
   83      private void UpdateVisual(bool isEnabled)
   84      {
   85          MainButton.Background = isEnabled ? _enabledBrush : _disabledBrush;
   86          MainButton.IsEnabled = isEnabled;
   87      }
   88  
   89      private void OnClick(object sender, RoutedEventArgs e)
   90      {
   91          if (IsBrewEnabled)
   92              BrewRequested?.Invoke(this, EventArgs.Empty);
   93      }
   94  }
   95  
   96  
   97  --- Caffe/Caffe/Controls/BrewingScreen.xaml ---
   98  <UserControl
   99      x:Class="Caffe.Controls.BrewingScreen"
  100      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  101      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  102      xmlns:utu="using:Uno.Toolkit.UI">
  103  
  104      <Grid Background="{StaticResource CaffeBackgroundBrush}">
  105          <utu:AutoLayout
  106              Orientation="Vertical"
  107              PrimaryAxisAlignment="Center"
  108              CounterAxisAlignment="Center"
  109              Spacing="24">
  110  
  111              <!-- Cup with Coffee Fill -->
  112              <Grid Width="120" Height="100">
  113                  <!-- Cup Body -->
  114                  <Border Width="80"
  115                          Height="70"
  116                          CornerRadius="0,0,8,8"
  117                          BorderBrush="{StaticResource CaffeTextPrimaryBrush}"
  118                          BorderThickness="3"
  119                          HorizontalAlignment="Center"
  120                          VerticalAlignment="Bottom"
  121                          Margin="0,0,20,0">
  122  
  123                      <!-- Coffee Fill -->
  124                      <Border x:Name="CoffeeFill"
  125                              VerticalAlignment="Bottom"
  126                              CornerRadius="0,0,5,5"
  127                              Height="0">
  128                          <Border.Background>
  129                              <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
  130                                  <GradientStop Color="{StaticResource CoffeeDarkColor}" Offset="0" />
  131                                  <GradientStop Color="{StaticResource CoffeeLightColor}" Offset="1" />
  132                              </LinearGradientBrush>
  133                          </Border.Background>
  134                      </Border>
  135                  </Border>
  136  
  137                  <!-- Handle -->
  138                  <Border Width="20"
  139                          Height="40"
  140                          CornerRadius="0,10,10,0"
  141                          BorderBrush="{StaticResource CaffeTextPrimaryBrush}"
  142                          BorderThickness="3,3,3,3"
  143                          HorizontalAlignment="Right"
  144                          VerticalAlignment="Center"
  145                          Margin="0,0,0,5" />
  146  
  147                  <!-- Saucer -->
  148                  <Border Height="6"
  149                          Width="100"
  150                          Background="{StaticResource CaffeTextPrimaryBrush}"
  151                          CornerRadius="3"
  152                          VerticalAlignment="Bottom"
  153                          HorizontalAlignment="Center"
  154                          Margin="0,0,10,0" />
  155              </Grid>
  156  
  157              <!-- Brewing Text -->
  158              <TextBlock x:Name="BrewingText"
  159                         Text="Brewing Espresso"
  160                         Style="{StaticResource BrewingTitleTextStyle}"
  161                         HorizontalAlignment="Center" />
  162  
  163              <!-- Parameters -->
  164              <TextBlock x:Name="ParamsText"
  165                         Text="93°C · Fine · 27s"
  166                         Style="{StaticResource BodyTextStyle}"
  167                         Foreground="{StaticResource CaffeTextSecondaryBrush}"
  168                         HorizontalAlignment="Center">
  169                  <TextBlock.RenderTransform>
  170                      <ScaleTransform x:Name="ParamsScale" ScaleX="1" ScaleY="1" />
  171                  </TextBlock.RenderTransform>
  172              </TextBlock>
  173  
  174          </utu:AutoLayout>
  175      </Grid>
  176  
  177  </UserControl>
  178  
  179  
  180  --- Caffe/Caffe/Controls/BrewingScreen.xaml.cs ---
  181  namespace Caffe.Controls;
  182  
  183  public sealed partial class BrewingScreen : UserControl
  184  {
  185      public static readonly DependencyProperty EspressoNameProperty =
  186          DependencyProperty.Register(nameof(EspressoName), typeof(string), typeof(BrewingScreen),
  187              new PropertyMetadata("Espresso", OnEspressoNameChanged));
  188  
  189      public static readonly DependencyProperty ParametersTextProperty =
  190          DependencyProperty.Register(nameof(ParametersText), typeof(string), typeof(BrewingScreen),
  191              new PropertyMetadata("93°C · Fine · 27s", OnParametersTextChanged));
  192  
  193      public static readonly DependencyProperty BrewProgressProperty =
  194          DependencyProperty.Register(nameof(BrewProgress), typeof(double), typeof(BrewingScreen),
  195              new PropertyMetadata(0.0, OnBrewProgressChanged));
  196  
  197      public string EspressoName
  198      {
  199          get => (string)GetValue(EspressoNameProperty);
  200          set => SetValue(EspressoNameProperty, value);
  201      }
  202  
  203      public string ParametersText
  204      {
  205          get => (string)GetValue(ParametersTextProperty);
  206          set => SetValue(ParametersTextProperty, value);
  207      }
  208  
  209      public double BrewProgress
  210      {
  211          get => (double)GetValue(BrewProgressProperty);
  212          set => SetValue(BrewProgressProperty, value);
  213      }
  214  
  215      public BrewingScreen()
  216      {
  217          this.InitializeComponent();
  218      }
  219  
  220      private static void OnEspressoNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  221      {
  222          if (d is BrewingScreen screen)
  223              screen.BrewingText.Text = $"Brewing {e.NewValue}";
  224      }
  225  
  226      private static void OnParametersTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  227      {
  228          if (d is BrewingScreen screen)
  229              screen.ParamsText.Text = (string)e.NewValue;
  230      }
  231  
  232      private static void OnBrewProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  233      {
  234          if (d is BrewingScreen screen)
  235          {
  236              var progress = (double)e.NewValue;
  237              // Fill height: 0 to 45 (65% of 70px cup height)
  238              screen.CoffeeFill.Height = progress * 45;
  239  
  240              // Pulse effect on parameters text
  241              var opacity = 0.5 + 0.5 * Math.Sin(progress * Math.PI * 4);
  242              screen.ParamsText.Opacity = opacity;
  243          }
  244      }
  245  }
  246  
  247  
  248  --- Caffe/Caffe/Controls/CaffeFooter.xaml ---
  249  <UserControl
  250      x:Class="Caffe.Controls.CaffeFooter"
  251      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  252      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  253      xmlns:utu="using:Uno.Toolkit.UI">
  254  
  255      <utu:AutoLayout
  256          Orientation="Vertical"
  257          PrimaryAxisAlignment="Center"
  258          CounterAxisAlignment="Center"
  259          Padding="0,20">
  260  
  261          <!-- Accent Bar (reversed colors from header) -->
  262          <Grid Width="36" Height="3">
  263              <Grid.ColumnDefinitions>
  264                  <ColumnDefinition Width="*" />
  265                  <ColumnDefinition Width="*" />
  266              </Grid.ColumnDefinitions>
  267              <Border Grid.Column="0"
  268                      Background="{StaticResource CaffeAccentRedBrush}"
  269                      CornerRadius="1.5,0,0,1.5" />
  270              <Border Grid.Column="1"
  271                      Background="{StaticResource CaffePrimaryBrush}"
  272                      CornerRadius="0,1.5,1.5,0" />
  273          </Grid>
  274  
  275      </utu:AutoLayout>
  276  
  277  </UserControl>
  278  
  279  
  280  --- Caffe/Caffe/Controls/CaffeFooter.xaml.cs ---
  281  namespace Caffe.Controls;
  282  
  283  public sealed partial class CaffeFooter : UserControl
  284  {
  285      public CaffeFooter()
  286      {
  287          this.InitializeComponent();
  288      }
  289  }
  290  
  291  
  292  --- Caffe/Caffe/Controls/CaffeHeader.xaml ---
  293  <UserControl
  294      x:Class="Caffe.Controls.CaffeHeader"
  295      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  296      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  297      xmlns:utu="using:Uno.Toolkit.UI">
  298  
  299      <utu:AutoLayout
  300          Orientation="Vertical"
  301          PrimaryAxisAlignment="Center"
  302          CounterAxisAlignment="Center"
  303          Padding="{utu:Responsive Narrow='0,24,0,16', Wide='0,40,0,28'}"
  304          Spacing="12">
  305  
  306          <!-- Accent Bar -->
  307          <Grid Width="56" Height="4">
  308              <Grid.ColumnDefinitions>
  309                  <ColumnDefinition Width="*" />
  310                  <ColumnDefinition Width="*" />
  311              </Grid.ColumnDefinitions>
  312              <Border Grid.Column="0"
  313                      Background="{StaticResource CaffePrimaryBrush}"
  314                      CornerRadius="2,0,0,2" />
  315              <Border Grid.Column="1"
  316                      Background="{StaticResource CaffeAccentRedBrush}"
  317                      CornerRadius="0,2,2,0" />
  318          </Grid>
  319  
  320          <!-- Logo -->
  321          <TextBlock Text="Caffè"
  322                     Style="{StaticResource LogoTextStyle}"
  323                     HorizontalAlignment="Center" />
  324  
  325          <!-- Tagline -->
  326          <TextBlock Text="NOTHING MORE. NOTHING LESS."
  327                     Style="{StaticResource TaglineTextStyle}"
  328                     HorizontalAlignment="Center" />
  329  
  330      </utu:AutoLayout>
  331  
  332  </UserControl>
  333  
  334  
  335  --- Caffe/Caffe/Controls/CaffeHeader.xaml.cs ---
  336  namespace Caffe.Controls;
  337  
  338  public sealed partial class CaffeHeader : UserControl
  339  {
  340      public CaffeHeader()
  341      {
  342          this.InitializeComponent();
  343      }
  344  }
  345  
  346  
  347  --- Caffe/Caffe/Controls/EspressoCard.xaml ---
  348  <UserControl
  349      x:Class="Caffe.Controls.EspressoCard"
  350      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  351      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  352      xmlns:utu="using:Uno.Toolkit.UI">
  353  
  354      <Border x:Name="CardBorder"
  355              Background="{StaticResource CaffeSurfaceBrush}"
  356              BorderBrush="{StaticResource CaffeBorderBrush}"
  357              BorderThickness="2"
  358              CornerRadius="14"
  359              Padding="16,20"
  360              Translation="0,0,4">
  361          <Border.Shadow>
  362              <ThemeShadow />
  363          </Border.Shadow>
  364  
  365          <Grid>
  366              <Grid.RowDefinitions>
  367                  <RowDefinition Height="Auto" />
  368                  <RowDefinition Height="8" />
  369                  <RowDefinition Height="Auto" />
  370                  <RowDefinition Height="4" />
  371                  <RowDefinition Height="Auto" />
  372              </Grid.RowDefinitions>
  373  
  374              <!-- Volume Badge + Checkmark Row -->
  375              <Grid Grid.Row="0">
  376                  <Border x:Name="VolumeBadge"
  377                          Background="{StaticResource CaffeAccentRedBrush}"
  378                          CornerRadius="4"
  379                          Padding="8,4"
  380                          HorizontalAlignment="Left">
  381                      <TextBlock x:Name="VolumeText"
  382                                 Style="{StaticResource VolumeBadgeTextStyle}" />
  383                  </Border>
  384  
  385                  <!-- Checkmark (visible when selected) -->
  386                  <Border x:Name="CheckmarkBorder"
  387                          HorizontalAlignment="Right"
  388                          VerticalAlignment="Top"
  389                          Background="{StaticResource CaffePrimaryBrush}"
  390                          CornerRadius="12"
  391                          Width="24"
  392                          Height="24"
  393                          Visibility="Collapsed">
  394                      <FontIcon Glyph="&#xE73E;"
  395                                FontSize="14"
  396                                Foreground="White"
  397                                HorizontalAlignment="Center"
  398                                VerticalAlignment="Center" />
  399                  </Border>
  400              </Grid>
  401  
  402              <!-- Espresso Name -->
  403              <TextBlock x:Name="NameText"
  404                         Grid.Row="2"
  405                         Style="{StaticResource CardTitleTextStyle}" />
  406  
  407              <!-- Description -->
  408              <TextBlock x:Name="DescriptionText"
  409                         Grid.Row="4"
  410                         Style="{StaticResource CardDescriptionTextStyle}" />
  411          </Grid>
  412      </Border>
  413  
  414  </UserControl>
  415  
  416  
  417  --- Caffe/Caffe/Controls/EspressoCard.xaml.cs ---
  418  using Caffe.Models;
  419  
  420  namespace Caffe.Controls;
  421  
  422  public sealed partial class EspressoCard : UserControl
  423  {
  424      public static readonly DependencyProperty EspressoProperty =
  425          DependencyProperty.Register(
  426              nameof(Espresso),
  427              typeof(EspressoItem),
  428              typeof(EspressoCard),
  429              new PropertyMetadata(null, OnEspressoChanged));
  430  
  431      public static readonly DependencyProperty IsSelectedProperty =
  432          DependencyProperty.Register(
  433              nameof(IsSelected),
  434              typeof(bool),
  435              typeof(EspressoCard),
  436              new PropertyMetadata(false, OnIsSelectedChanged));
  437  
  438      public EspressoItem? Espresso
  439      {
  440          get => (EspressoItem?)GetValue(EspressoProperty);
  441          set => SetValue(EspressoProperty, value);
  442      }
  443  
  444      public bool IsSelected
  445      {
  446          get => (bool)GetValue(IsSelectedProperty);
  447          set => SetValue(IsSelectedProperty, value);
  448      }
  449  
  450      public EspressoCard()
  451      {
  452          this.InitializeComponent();
  453      }
  454  
  455      private static void OnEspressoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  456      {
  457          if (d is EspressoCard card && e.NewValue is EspressoItem item)
  458          {
  459              card.VolumeText.Text = item.VolumeDisplay;
  460              card.NameText.Text = item.Name;
  461              card.DescriptionText.Text = item.Description;
  462              Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(card, $"{item.Name} espresso, {item.VolumeDisplay}");
  463          }
  464      }
  465  
  466      private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  467      {
  468          if (d is EspressoCard card)
  469          {
  470              var isSelected = (bool)e.NewValue;
  471              card.CardBorder.BorderBrush = isSelected
  472                  ? (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"]
  473                  : (SolidColorBrush)Application.Current.Resources["CaffeBorderBrush"];
  474  
  475              card.CheckmarkBorder.Visibility = isSelected
  476                  ? Visibility.Visible
  477                  : Visibility.Collapsed;
  478          }
  479      }
  480  }
  481  
  482  
  483  --- Caffe/Caffe/Controls/ExtractionArc.xaml ---
  484  <UserControl
  485      x:Class="Caffe.Controls.ExtractionArc"
  486      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  487      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  488      xmlns:utu="using:Uno.Toolkit.UI">
  489  
  490      <Border Background="{StaticResource CaffeSurfaceBrush}"
  491              CornerRadius="{utu:Responsive Narrow=12, Wide=16}"
  492              Padding="{utu:Responsive Narrow='8,12', Normal='12,16', Wide='20,24'}"
  493              Translation="0,0,4">
  494          <Border.Shadow>
  495              <ThemeShadow />
  496          </Border.Shadow>
  497  
  498          <Grid RowSpacing="{utu:Responsive Narrow=4, Wide=8}">
  499              <Grid.RowDefinitions>
  500                  <RowDefinition Height="Auto" />
  501                  <RowDefinition Height="*" />
  502                  <RowDefinition Height="Auto" />
  503                  <RowDefinition Height="Auto" />
  504                  <RowDefinition Height="Auto" />
  505              </Grid.RowDefinitions>
  506  
  507              <!-- Label -->
  508              <TextBlock Grid.Row="0"
  509                         Text="EXTRACTION"
  510                         Style="{StaticResource ParameterLabelTextStyle}"
  511                         HorizontalAlignment="Center" />
  512  
  513              <!-- Arc Visual -->
  514              <Viewbox Grid.Row="1"
  515                       Stretch="Uniform"
  516                       MaxWidth="{utu:Responsive Narrow=88, Normal=120, Wide=150}"
  517                       MaxHeight="{utu:Responsive Narrow=72, Normal=96, Wide=120}"
  518                       VerticalAlignment="Center"
  519                       Margin="{utu:Responsive Narrow='0,4', Wide='0,8'}">
  520                  <Grid Width="110" Height="88">
  521                      <!-- Background arc -->
  522                      <Path Stroke="{StaticResource CaffeBorderBrush}"
  523                            StrokeThickness="6"
  524                            Margin="5,4,5,0">
  525                          <Path.Data>
  526                              <PathGeometry>
  527                                  <PathFigure StartPoint="15,65" IsClosed="False">
  528                                      <ArcSegment Point="85,65"
  529                                                  Size="40,40"
  530                                                  SweepDirection="Clockwise"
  531                                                  IsLargeArc="True" />
  532                                  </PathFigure>
  533                              </PathGeometry>
  534                          </Path.Data>
  535                      </Path>
  536  
  537                      <!-- Sweet spot highlight (25-30s range) -->
  538                      <Path Stroke="{StaticResource CaffeAccentGreenBrush}"
  539                            StrokeThickness="6"
  540                            Opacity="0.3"
  541                            Margin="5,4,5,0">
  542                          <Path.Data>
  543                              <PathGeometry>
  544                                  <PathFigure StartPoint="38,22" IsClosed="False">
  545                                      <ArcSegment Point="62,22"
  546                                                  Size="40,40"
  547                                                  SweepDirection="Clockwise"
  548                                                  IsLargeArc="False" />
  549                                  </PathFigure>
  550                              </PathGeometry>
  551                          </Path.Data>
  552                      </Path>
  553  
  554                      <!-- Value arc -->
  555                      <Path x:Name="ValueArc"
  556                            Stroke="{StaticResource CaffePrimaryBrush}"
  557                            StrokeThickness="6"
  558                            Margin="5,4,5,0">
  559                          <Path.Data>
  560                              <PathGeometry>
  561                                  <PathFigure x:Name="ArcFigure" StartPoint="15,65" IsClosed="False">
  562                                      <ArcSegment x:Name="ArcSegment"
  563                                                  Point="50,12"
  564                                                  Size="40,40"
  565                                                  SweepDirection="Clockwise"
  566                                                  IsLargeArc="False" />
  567                                  </PathFigure>
  568                              </PathGeometry>
  569                          </Path.Data>
  570                      </Path>
  571  
  572                      <!-- Center text -->
  573                      <StackPanel VerticalAlignment="Center"
  574                                  HorizontalAlignment="Center"
  575                                  Margin="0,14,0,0">
  576                          <TextBlock x:Name="ArcValueText"
  577                                     Text="27"
  578                                     Style="{StaticResource ParameterValueTextStyle}"
  579                                     HorizontalAlignment="Center" />
  580                          <TextBlock Text="SECONDS"
  581                                     Style="{StaticResource ArcLabelTextStyle}"
  582                                     HorizontalAlignment="Center" />
  583                      </StackPanel>
  584                  </Grid>
  585              </Viewbox>
  586  
  587              <!-- Value Display (matches Temperature layout for slider alignment) -->
  588              <TextBlock Grid.Row="2"
  589                         x:Name="ValueText"
  590                         Text="27s"
  591                         Style="{StaticResource ParameterValueTextStyle}"
  592                         HorizontalAlignment="Center" />
  593  
  594              <!-- Slider -->
  595              <Slider Grid.Row="3"
  596                      x:Name="TimeSlider"
  597                      Width="{utu:Responsive Narrow=76, Normal=110, Wide=150}"
  598                      HorizontalAlignment="Center"
  599                      Minimum="20"
  600                      Maximum="35"
  601                      Value="27"
  602                      StepFrequency="1"
  603                      SnapsTo="StepValues"
  604                      AutomationProperties.Name="Extraction time"
  605                      ValueChanged="OnSliderValueChanged" />
  606  
  607              <!-- Range Labels -->
  608              <Grid Grid.Row="4"
  609                    Width="{utu:Responsive Narrow=76, Normal=110, Wide=150}"
  610                    HorizontalAlignment="Center">
  611                  <TextBlock Text="20s"
  612                             Style="{StaticResource ParameterLabelTextStyle}"
  613                             HorizontalAlignment="Left" />
  614                  <TextBlock Text="35s"
  615                             Style="{StaticResource ParameterLabelTextStyle}"
  616                             HorizontalAlignment="Right" />
  617              </Grid>
  618  
  619          </Grid>
  620      </Border>
  621  
  622  </UserControl>
  623  
  624  
  625  --- Caffe/Caffe/Controls/ExtractionArc.xaml.cs ---
  626  namespace Caffe.Controls;
  627  
  628  public sealed partial class ExtractionArc : UserControl
  629  {
  630      private const int MinTime = 20;
  631      private const int MaxTime = 35;
  632      private const int DefaultTime = 27;
  633      private const double ArcCenterX = 50;
  634      private const double ArcCenterY = 45;
  635      private const double ArcRadius = 40;
  636      private const double ArcStartAngle = -120;
  637      private const double ArcSweepDegrees = 240;
  638  
  639      public static readonly DependencyProperty ExtractionTimeProperty =
  640          DependencyProperty.Register(
  641              nameof(ExtractionTime),
  642              typeof(int),
  643              typeof(ExtractionArc),
  644              new PropertyMetadata(DefaultTime, OnExtractionTimeChanged));
  645  
  646      public int ExtractionTime
  647      {
  648          get => (int)GetValue(ExtractionTimeProperty);
  649          set => SetValue(ExtractionTimeProperty, value);
  650      }
  651  
  652      public event EventHandler<int>? ExtractionTimeChanged;
  653  
  654      public ExtractionArc()
  655      {
  656          this.InitializeComponent();
  657          UpdateVisual(DefaultTime);
  658      }
  659  
  660      private static void OnExtractionTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  661      {
  662          if (d is ExtractionArc arc)
  663          {
  664              var time = (int)e.NewValue;
  665              arc.TimeSlider.Value = time;
  666              arc.UpdateVisual(time);
  667          }
  668      }
  669  
  670      private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  671      {
  672          var time = (int)e.NewValue;
  673          ExtractionTime = time;
  674          UpdateVisual(time);
  675          ExtractionTimeChanged?.Invoke(this, time);
  676      }
  677  
  678      private void UpdateVisual(int time)
  679      {
  680          ArcValueText.Text = time.ToString();
  681          ValueText.Text = $"{time}s";
  682  
  683          var percentage = (time - MinTime) / (double)(MaxTime - MinTime);
  684          var angle = ArcStartAngle + (percentage * ArcSweepDegrees);
  685          var radians = angle * Math.PI / 180;
  686  
  687          var endX = ArcCenterX + ArcRadius * Math.Sin(radians);
  688          var endY = ArcCenterY - ArcRadius * Math.Cos(radians);
  689  
  690          ArcSegment.Point = new Windows.Foundation.Point(endX, endY);
  691          ArcSegment.IsLargeArc = percentage > 0.5;
  692      }
  693  }
  694  
  695  
  696  --- Caffe/Caffe/Controls/GrindSelector.xaml ---
  697  <UserControl
  698      x:Class="Caffe.Controls.GrindSelector"
  699      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  700      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  701      xmlns:utu="using:Uno.Toolkit.UI">
  702  
  703      <Border Background="{StaticResource CaffeSurfaceBrush}"
  704              CornerRadius="{utu:Responsive Narrow=12, Wide=16}"
  705              Padding="{utu:Responsive Narrow='8,12', Normal='12,16', Wide='20,24'}"
  706              Translation="0,0,4">
  707          <Border.Shadow>
  708              <ThemeShadow />
  709          </Border.Shadow>
  710  
  711          <Grid RowSpacing="{utu:Responsive Narrow=4, Wide=8}">
  712              <Grid.RowDefinitions>
  713                  <RowDefinition Height="Auto" />
  714                  <RowDefinition Height="*" />
  715                  <RowDefinition Height="Auto" />
  716                  <RowDefinition Height="Auto" />
  717                  <RowDefinition Height="Auto" />
  718              </Grid.RowDefinitions>
  719  
  720              <!-- Label -->
  721              <TextBlock Grid.Row="0"
  722                         Text="GRIND SIZE"
  723                         Style="{StaticResource ParameterLabelTextStyle}"
  724                         HorizontalAlignment="Center" />
  725  
  726              <!-- Particle Display -->
  727              <Grid Grid.Row="1"
  728                    x:Name="ParticleGrid"
  729                    Width="{utu:Responsive Narrow=48, Normal=64, Wide=84}"
  730                    Height="{utu:Responsive Narrow=48, Normal=64, Wide=84}"
  731                    HorizontalAlignment="Center"
  732                    VerticalAlignment="Center"
  733                    Margin="{utu:Responsive Narrow='0,4', Wide='0,8'}" />
  734  
  735              <!-- Grind Label + Hint -->
  736              <StackPanel Grid.Row="2"
  737                          HorizontalAlignment="Center">
  738                  <TextBlock x:Name="GrindLabelText"
  739                             Text="Fine"
  740                             Style="{StaticResource ParameterValueTextStyle}"
  741                             HorizontalAlignment="Center" />
  742                  <TextBlock x:Name="GrindHintText"
  743                             Text="Slower"
  744                             Style="{StaticResource GrindHintTextStyle}"
  745                             HorizontalAlignment="Center" />
  746              </StackPanel>
  747  
  748              <!-- Size Selector Buttons -->
  749              <StackPanel Grid.Row="3"
  750                          Orientation="Horizontal"
  751                          Spacing="{utu:Responsive Narrow=4, Normal=12, Wide=18}"
  752                          HorizontalAlignment="Center"
  753                          Margin="0,4,0,0">
  754  
  755                  <!-- Fine (Small) -->
  756                  <Button x:Name="FineButton"
  757                          Width="{utu:Responsive Narrow=20, Normal=28, Wide=34}"
  758                          Height="{utu:Responsive Narrow=20, Normal=28, Wide=34}"
  759                          Padding="0"
  760                          MinWidth="0"
  761                          MinHeight="0"
  762                          CornerRadius="{utu:Responsive Narrow=10, Normal=14, Wide=17}"
  763                          AutomationProperties.Name="Fine grind"
  764                          Click="OnFineClick">
  765                      <Ellipse Width="{utu:Responsive Narrow=6, Normal=8, Wide=10}"
  766                               Height="{utu:Responsive Narrow=6, Normal=8, Wide=10}"
  767                               Fill="White" />
  768                  </Button>
  769  
  770                  <!-- Medium -->
  771                  <Button x:Name="MediumButton"
  772                          Width="{utu:Responsive Narrow=26, Normal=34, Wide=42}"
  773                          Height="{utu:Responsive Narrow=26, Normal=34, Wide=42}"
  774                          Padding="0"
  775                          MinWidth="0"
  776                          MinHeight="0"
  777                          CornerRadius="{utu:Responsive Narrow=13, Normal=17, Wide=21}"
  778                          AutomationProperties.Name="Medium grind"
  779                          Click="OnMediumClick">
  780                      <Ellipse Width="{utu:Responsive Narrow=8, Normal=12, Wide=15}"
  781                               Height="{utu:Responsive Narrow=8, Normal=12, Wide=15}"
  782                               Fill="White" />
  783                  </Button>
  784  
  785                  <!-- Coarse (Large) -->
  786                  <Button x:Name="CoarseButton"
  787                          Width="{utu:Responsive Narrow=32, Normal=40, Wide=48}"
  788                          Height="{utu:Responsive Narrow=32, Normal=40, Wide=48}"
  789                          Padding="0"
  790                          MinWidth="0"
  791                          MinHeight="0"
  792                          CornerRadius="{utu:Responsive Narrow=16, Normal=20, Wide=24}"
  793                          AutomationProperties.Name="Coarse grind"
  794                          Click="OnCoarseClick">
  795                      <Ellipse Width="{utu:Responsive Narrow=12, Normal=16, Wide=20}"
  796                               Height="{utu:Responsive Narrow=12, Normal=16, Wide=20}"
  797                               Fill="White" />
  798                  </Button>
  799  
  800              </StackPanel>
  801  
  802              <!-- Size Labels -->
  803              <Grid Grid.Row="4"
  804                    Width="{utu:Responsive Narrow=76, Normal=110, Wide=140}"
  805                    HorizontalAlignment="Center"
  806                    Margin="0,2,0,0">
  807                  <TextBlock Text="S"
  808                             Style="{StaticResource ParameterLabelTextStyle}"
  809                             HorizontalAlignment="Left"
  810                             Margin="4,0,0,0" />
  811                  <TextBlock Text="M"
  812                             Style="{StaticResource ParameterLabelTextStyle}"
  813                             HorizontalAlignment="Center" />
  814                  <TextBlock Text="L"
  815                             Style="{StaticResource ParameterLabelTextStyle}"
  816                             HorizontalAlignment="Right"
  817                             Margin="0,0,4,0" />
  818              </Grid>
  819  
  820          </Grid>
  821      </Border>
  822  
  823  </UserControl>
  824  
  825  
  826  --- Caffe/Caffe/Controls/GrindSelector.xaml.cs ---
  827  using Caffe.Models;
  828  
  829  namespace Caffe.Controls;
  830  
  831  public sealed partial class GrindSelector : UserControl
  832  {
  833      private readonly SolidColorBrush _selectedBrush;
  834      private readonly SolidColorBrush _unselectedBrush;
  835      private readonly SolidColorBrush _particleBrush;
  836      private readonly Random _random = new(42); // Fixed seed for consistent layout
  837      private GrindLevel _lastParticleLevel = (GrindLevel)(-1);
  838  
  839      public static readonly DependencyProperty GrindLevelProperty =
  840          DependencyProperty.Register(
  841              nameof(GrindLevel),
  842              typeof(GrindLevel),
  843              typeof(GrindSelector),
  844              new PropertyMetadata(GrindLevel.Fine, OnGrindLevelChanged));
  845  
  846      public GrindLevel GrindLevel
  847      {
  848          get => (GrindLevel)GetValue(GrindLevelProperty);
  849          set => SetValue(GrindLevelProperty, value);
  850      }
  851  
  852      public event EventHandler<GrindLevel>? GrindLevelChanged;
  853  
  854      public GrindSelector()
  855      {
  856          this.InitializeComponent();
  857  
  858          _selectedBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"];
  859          _unselectedBrush = (SolidColorBrush)Application.Current.Resources["CaffeBorderBrush"];
  860          _particleBrush = (SolidColorBrush)Application.Current.Resources["CaffeParticleBrush"];
  861  
  862          UpdateVisual(GrindLevel.Fine);
  863      }
  864  
  865      private static void OnGrindLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  866      {
  867          if (d is GrindSelector selector)
  868          {
  869              selector.UpdateVisual((GrindLevel)e.NewValue);
  870          }
  871      }
  872  
  873      private void OnFineClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Fine);
  874      private void OnMediumClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Medium);
  875      private void OnCoarseClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Coarse);
  876  
  877      private void SetGrindLevel(GrindLevel level)
  878      {
  879          GrindLevel = level;
  880          UpdateVisual(level);
  881          GrindLevelChanged?.Invoke(this, level);
  882      }
  883  
  884      private void UpdateVisual(GrindLevel level)
  885      {
  886          // Update labels
  887          GrindLabelText.Text = level.ToLabel();
  888          GrindHintText.Text = level.ToHint();
  889  
  890          // Update button states
  891          FineButton.Background = level == GrindLevel.Fine ? _selectedBrush : _unselectedBrush;
  892          MediumButton.Background = level == GrindLevel.Medium ? _selectedBrush : _unselectedBrush;
  893          CoarseButton.Background = level == GrindLevel.Coarse ? _selectedBrush : _unselectedBrush;
  894  
  895          // Update particle display
  896          UpdateParticles(level);
  897      }
  898  
  899      private void UpdateParticles(GrindLevel level)
  900      {
  901          if (_lastParticleLevel == level) return;
  902          _lastParticleLevel = level;
  903  
  904          ParticleGrid.Children.Clear();
  905          ParticleGrid.RowDefinitions.Clear();
  906          ParticleGrid.ColumnDefinitions.Clear();
  907  
  908          var (count, size) = level switch
  909          {
  910              GrindLevel.Fine => (12, 6.0),
  911              GrindLevel.Medium => (9, 9.0),
  912              GrindLevel.Coarse => (6, 13.0),
  913              _ => (9, 9.0)
  914          };
  915  
  916          var cols = level switch
  917          {
  918              GrindLevel.Fine => 4,
  919              GrindLevel.Medium => 3,
  920              GrindLevel.Coarse => 3,
  921              _ => 3
  922          };
  923  
  924          var rows = (int)Math.Ceiling((double)count / cols);
  925  
  926          for (int c = 0; c < cols; c++)
  927              ParticleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
  928          for (int r = 0; r < rows; r++)
  929              ParticleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
  930  
  931          for (int i = 0; i < count; i++)
  932          {
  933              var row = i / cols;
  934              var col = i % cols;
  935  
  936              var ellipse = new Ellipse
  937              {
  938                  Width = size,
  939                  Height = size,
  940                  Fill = _particleBrush,
  941                  HorizontalAlignment = HorizontalAlignment.Center,
  942                  VerticalAlignment = VerticalAlignment.Center
  943              };
  944  
  945              Grid.SetRow(ellipse, row);
  946              Grid.SetColumn(ellipse, col);
  947              ParticleGrid.Children.Add(ellipse);
  948          }
  949      }
  950  }
  951  
  952  
  953  --- Caffe/Caffe/Controls/SelectionOverview.xaml ---
  954  <UserControl
  955      x:Class="Caffe.Controls.SelectionOverview"
  956      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  957      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  958      xmlns:utu="using:Uno.Toolkit.UI">
  959  
  960      <Border Background="{StaticResource CaffePrimaryBrush}"
  961              CornerRadius="14"
  962              Padding="20,16">
  963  
  964          <Grid>
  965              <Grid.ColumnDefinitions>
  966                  <ColumnDefinition Width="*" />
  967                  <ColumnDefinition Width="Auto" />
  968              </Grid.ColumnDefinitions>
  969  
  970              <!-- Left: Selection Info -->
  971              <StackPanel Grid.Column="0" Spacing="2">
  972                  <TextBlock Text="YOUR SELECTION"
  973                             Style="{StaticResource OverviewLabelTextStyle}" />
  974                  <TextBlock x:Name="EspressoNameText"
  975                             Text="Espresso"
  976                             Style="{StaticResource OverviewValueTextStyle}" />
  977              </StackPanel>
  978  
  979              <!-- Right: Stats -->
  980              <StackPanel Grid.Column="1"
  981                          Orientation="Horizontal"
  982                          Spacing="0">
  983  
  984                  <!-- Temperature -->
  985                  <StackPanel Margin="0,0,12,0">
  986                      <TextBlock x:Name="TempValueText"
  987                                 Text="93°"
  988                                 Style="{StaticResource OverviewValueTextStyle}"
  989                                 HorizontalAlignment="Center" />
  990                      <TextBlock Text="Temp"
  991                                 Style="{StaticResource OverviewLabelTextStyle}"
  992                                 HorizontalAlignment="Center" />
  993                  </StackPanel>
  994  
  995                  <!-- Divider -->
  996                  <Border Width="1"
  997                          Background="White"
  998                          Opacity="0.3"
  999                          Margin="0,4" />
 1000  
 1001                  <!-- Grind -->
 1002                  <StackPanel Margin="12,0">
 1003                      <TextBlock x:Name="GrindValueText"
 1004                                 Text="F"
 1005                                 Style="{StaticResource OverviewValueTextStyle}"
 1006                                 HorizontalAlignment="Center" />
 1007                      <TextBlock Text="Grind"
 1008                                 Style="{StaticResource OverviewLabelTextStyle}"
 1009                                 HorizontalAlignment="Center" />
 1010                  </StackPanel>
 1011  
 1012                  <!-- Divider -->
 1013                  <Border Width="1"
 1014                          Background="White"
 1015                          Opacity="0.3"
 1016                          Margin="0,4" />
 1017  
 1018                  <!-- Time -->
 1019                  <StackPanel Margin="12,0,0,0">
 1020                      <TextBlock x:Name="TimeValueText"
 1021                                 Text="27s"
 1022                                 Style="{StaticResource OverviewValueTextStyle}"
 1023                                 HorizontalAlignment="Center" />
 1024                      <TextBlock Text="Time"
 1025                                 Style="{StaticResource OverviewLabelTextStyle}"
 1026                                 HorizontalAlignment="Center" />
 1027                  </StackPanel>
 1028  
 1029              </StackPanel>
 1030  
 1031          </Grid>
 1032  
 1033      </Border>
 1034  
 1035  </UserControl>
 1036  
 1037  
 1038  --- Caffe/Caffe/Controls/SelectionOverview.xaml.cs ---
 1039  namespace Caffe.Controls;
 1040  
 1041  public sealed partial class SelectionOverview : UserControl
 1042  {
 1043      public static readonly DependencyProperty EspressoNameProperty =
 1044          DependencyProperty.Register(nameof(EspressoName), typeof(string), typeof(SelectionOverview),
 1045              new PropertyMetadata("Espresso", OnEspressoNameChanged));
 1046  
 1047      public static readonly DependencyProperty TemperatureProperty =
 1048          DependencyProperty.Register(nameof(Temperature), typeof(int), typeof(SelectionOverview),
 1049              new PropertyMetadata(93, OnTemperatureChanged));
 1050  
 1051      public static readonly DependencyProperty GrindAbbreviationProperty =
 1052          DependencyProperty.Register(nameof(GrindAbbreviation), typeof(string), typeof(SelectionOverview),
 1053              new PropertyMetadata("F", OnGrindChanged));
 1054  
 1055      public static readonly DependencyProperty ExtractionTimeProperty =
 1056          DependencyProperty.Register(nameof(ExtractionTime), typeof(int), typeof(SelectionOverview),
 1057              new PropertyMetadata(27, OnTimeChanged));
 1058  
 1059      public string EspressoName
 1060      {
 1061          get => (string)GetValue(EspressoNameProperty);
 1062          set => SetValue(EspressoNameProperty, value);
 1063      }
 1064  
 1065      public int Temperature
 1066      {
 1067          get => (int)GetValue(TemperatureProperty);
 1068          set => SetValue(TemperatureProperty, value);
 1069      }
 1070  
 1071      public string GrindAbbreviation
 1072      {
 1073          get => (string)GetValue(GrindAbbreviationProperty);
 1074          set => SetValue(GrindAbbreviationProperty, value);
 1075      }
 1076  
 1077      public int ExtractionTime
 1078      {
 1079          get => (int)GetValue(ExtractionTimeProperty);
 1080          set => SetValue(ExtractionTimeProperty, value);
 1081      }
 1082  
 1083      public SelectionOverview()
 1084      {
 1085          this.InitializeComponent();
 1086      }
 1087  
 1088      private static void OnEspressoNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 1089      {
 1090          if (d is SelectionOverview overview)
 1091              overview.EspressoNameText.Text = (string)e.NewValue;
 1092      }
 1093  
 1094      private static void OnTemperatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 1095      {
 1096          if (d is SelectionOverview overview)
 1097              overview.TempValueText.Text = $"{(int)e.NewValue}°";
 1098      }
 1099  
 1100      private static void OnGrindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 1101      {
 1102          if (d is SelectionOverview overview)
 1103              overview.GrindValueText.Text = (string)e.NewValue;
 1104      }
 1105  
 1106      private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 1107      {
 1108          if (d is SelectionOverview overview)
 1109              overview.TimeValueText.Text = $"{(int)e.NewValue}s";
 1110      }
 1111  }
 1112  
 1113  
 1114  --- Caffe/Caffe/Controls/TemperatureGauge.xaml ---
 1115  <UserControl
 1116      x:Class="Caffe.Controls.TemperatureGauge"
 1117      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
 1118      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
 1119      xmlns:utu="using:Uno.Toolkit.UI">
 1120  
 1121      <Border Background="{StaticResource CaffeSurfaceBrush}"
 1122              CornerRadius="{utu:Responsive Narrow=12, Wide=16}"
 1123              Padding="{utu:Responsive Narrow='8,12', Normal='12,16', Wide='20,24'}"
 1124              Translation="0,0,4">
 1125          <Border.Shadow>
 1126              <ThemeShadow />
 1127          </Border.Shadow>
 1128  
 1129          <Grid RowSpacing="{utu:Responsive Narrow=4, Wide=8}">
 1130              <Grid.RowDefinitions>
 1131                  <RowDefinition Height="Auto" />
 1132                  <RowDefinition Height="*" />
 1133                  <RowDefinition Height="Auto" />
 1134                  <RowDefinition Height="Auto" />
 1135                  <RowDefinition Height="Auto" />
 1136              </Grid.RowDefinitions>
 1137  
 1138              <!-- Label -->
 1139              <TextBlock Grid.Row="0"
 1140                         Text="TEMPERATURE"
 1141                         Style="{StaticResource ParameterLabelTextStyle}"
 1142                         HorizontalAlignment="Center" />
 1143  
 1144              <!-- Thermometer Visual -->
 1145              <Viewbox Grid.Row="1"
 1146                       Stretch="Uniform"
 1147                       MaxWidth="{utu:Responsive Narrow=28, Normal=36, Wide=44}"
 1148                       MaxHeight="{utu:Responsive Narrow=64, Normal=88, Wide=110}"
 1149                       VerticalAlignment="Center"
 1150                       Margin="{utu:Responsive Narrow='0,4', Wide='0,8'}">
 1151                  <Grid Width="24" Height="70">
 1152                      <!-- Track background -->
 1153                      <Border VerticalAlignment="Stretch"
 1154                              HorizontalAlignment="Center"
 1155                              Width="8"
 1156                              CornerRadius="4"
 1157                              Background="{StaticResource CaffeBorderBrush}"
 1158                              Margin="0,0,0,16" />
 1159  
 1160                      <!-- Fill (from bottom) -->
 1161                      <Border x:Name="TempFill"
 1162                              VerticalAlignment="Bottom"
 1163                              HorizontalAlignment="Center"
 1164                              Width="8"
 1165                              Height="35"
 1166                              CornerRadius="4"
 1167                              Margin="0,0,0,16">
 1168                          <Border.Background>
 1169                              <LinearGradientBrush StartPoint="0,1" EndPoint="0,0">
 1170                                  <GradientStop Color="{StaticResource CaffeTemperatureHighColor}" Offset="0" />
 1171                                  <GradientStop Color="{StaticResource CaffeTemperatureLowColor}" Offset="1" />
 1172                              </LinearGradientBrush>
 1173                          </Border.Background>
 1174                      </Border>
 1175  
 1176                      <!-- Bulb -->
 1177                      <Ellipse Width="18"
 1178                               Height="18"
 1179                               Fill="{StaticResource CaffeAccentRedBrush}"
 1180                               VerticalAlignment="Bottom"
 1181                               HorizontalAlignment="Center" />
 1182                  </Grid>
 1183              </Viewbox>
 1184  
 1185              <!-- Value Display -->
 1186              <TextBlock Grid.Row="2"
 1187                         x:Name="ValueText"
 1188                         Text="93°C"
 1189                         Style="{StaticResource ParameterValueTextStyle}"
 1190                         HorizontalAlignment="Center" />
 1191  
 1192              <!-- Slider -->
 1193              <Slider Grid.Row="3"
 1194                      x:Name="TempSlider"
 1195                      Width="{utu:Responsive Narrow=76, Normal=110, Wide=150}"
 1196                      HorizontalAlignment="Center"
 1197                      Minimum="88"
 1198                      Maximum="96"
 1199                      Value="93"
 1200                      StepFrequency="1"
 1201                      SnapsTo="StepValues"
 1202                      AutomationProperties.Name="Temperature"
 1203                      ValueChanged="OnSliderValueChanged" />
 1204  
 1205              <!-- Range Labels -->
 1206              <Grid Grid.Row="4"
 1207                    Width="{utu:Responsive Narrow=76, Normal=110, Wide=150}"
 1208                    HorizontalAlignment="Center">
 1209                  <TextBlock Text="88°"
 1210                             Style="{StaticResource ParameterLabelTextStyle}"
 1211                             HorizontalAlignment="Left" />
 1212                  <TextBlock Text="96°"
 1213                             Style="{StaticResource ParameterLabelTextStyle}"
 1214                             HorizontalAlignment="Right" />
 1215              </Grid>
 1216  
 1217          </Grid>
 1218      </Border>
 1219  
 1220  </UserControl>
 1221  
 1222  
 1223  --- Caffe/Caffe/Controls/TemperatureGauge.xaml.cs ---
 1224  namespace Caffe.Controls;
 1225  
 1226  public sealed partial class TemperatureGauge : UserControl
 1227  {
 1228      private const int MinTemperature = 88;
 1229      private const int MaxTemperature = 96;
 1230      private const int DefaultTemperature = 93;
 1231      private const double MinFillHeight = 10;
 1232      private const double MaxFillHeight = 50;
 1233  
 1234      public static readonly DependencyProperty TemperatureProperty =
 1235          DependencyProperty.Register(
 1236              nameof(Temperature),
 1237              typeof(int),
 1238              typeof(TemperatureGauge),
 1239              new PropertyMetadata(DefaultTemperature, OnTemperatureChanged));
 1240  
 1241      public int Temperature
 1242      {
 1243          get => (int)GetValue(TemperatureProperty);
 1244          set => SetValue(TemperatureProperty, value);
 1245      }
 1246  
 1247      public event EventHandler<int>? TemperatureChanged;
 1248  
 1249      public TemperatureGauge()
 1250      {
 1251          this.InitializeComponent();
 1252          UpdateVisual(DefaultTemperature);
 1253      }
 1254  
 1255      private static void OnTemperatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
 1256      {
 1257          if (d is TemperatureGauge gauge)
 1258          {
 1259              var temp = (int)e.NewValue;
 1260              gauge.TempSlider.Value = temp;
 1261              gauge.UpdateVisual(temp);
 1262          }
 1263      }
 1264  
 1265      private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
 1266      {
 1267          var temp = (int)e.NewValue;
 1268          Temperature = temp;
 1269          UpdateVisual(temp);
 1270          TemperatureChanged?.Invoke(this, temp);
 1271      }
 1272  
 1273      private void UpdateVisual(int temp)
 1274      {
 1275          ValueText.Text = $"{temp}°C";
 1276  
 1277          var percentage = (temp - MinTemperature) / (double)(MaxTemperature - MinTemperature);
 1278          TempFill.Height = MinFillHeight + (percentage * (MaxFillHeight - MinFillHeight));
 1279      }
 1280  }
```

### `Caffe/Caffe/ViewModels/MainViewModel.cs`

```csharp
    1  using System.Collections.ObjectModel;
    2  using System.Windows.Input;
    3  using Caffe.Models;
    4  using CommunityToolkit.Mvvm.ComponentModel;
    5  using CommunityToolkit.Mvvm.Input;
    6  
    7  namespace Caffe.ViewModels;
    8  
    9  public partial class MainViewModel : ObservableObject
   10  {
   11      public ObservableCollection<EspressoItem> EspressoItems { get; } =
   12      [
   13          new("Espresso", 30, "Pure, concentrated, bold"),
   14          new("Doppio", 60, "Double the intensity"),
   15          new("Ristretto", 20, "Short, sweet, powerful"),
   16          new("Lungo", 50, "Long pull, smooth finish")
   17      ];
   18  
   19      [ObservableProperty]
   20      [NotifyPropertyChangedFor(nameof(HasSelection))]
   21      [NotifyPropertyChangedFor(nameof(BrewButtonText))]
   22      [NotifyPropertyChangedFor(nameof(GrindAbbreviation))]
   23      private EspressoItem? _selectedEspresso;
   24  
   25      [ObservableProperty]
   26      [NotifyPropertyChangedFor(nameof(BrewingParametersText))]
   27      private int _temperature = 93;
   28  
   29      [ObservableProperty]
   30      [NotifyPropertyChangedFor(nameof(GrindLabel))]
   31      [NotifyPropertyChangedFor(nameof(GrindHint))]
   32      [NotifyPropertyChangedFor(nameof(GrindAbbreviation))]
   33      [NotifyPropertyChangedFor(nameof(BrewingParametersText))]
   34      private GrindLevel _grindLevel = GrindLevel.Fine;
   35  
   36      [ObservableProperty]
   37      [NotifyPropertyChangedFor(nameof(BrewingParametersText))]
   38      private int _extractionTime = 27;
   39  
   40      [ObservableProperty]
   41      private bool _isBrewing;
   42  
   43      [ObservableProperty]
   44      private double _brewProgress;
   45  
   46      public bool HasSelection => SelectedEspresso is not null;
   47  
   48      public string BrewButtonText => SelectedEspresso is null
   49          ? "Select your espresso"
   50          : $"Brew {SelectedEspresso.Name}";
   51  
   52      public string GrindLabel => GrindLevel.ToLabel();
   53      public string GrindHint => GrindLevel.ToHint();
   54      public string GrindAbbreviation => GrindLevel.ToAbbreviation();
   55  
   56      public string BrewingParametersText =>
   57          $"{Temperature}°C · {GrindLabel} · {ExtractionTime}s";
   58  
   59      public string TemperatureDisplay => $"{Temperature}°C";
   60      public string ExtractionTimeDisplay => $"{ExtractionTime}";
   61  
   62      partial void OnTemperatureChanged(int value)
   63      {
   64          OnPropertyChanged(nameof(TemperatureDisplay));
   65      }
   66  
   67      partial void OnExtractionTimeChanged(int value)
   68      {
   69          OnPropertyChanged(nameof(ExtractionTimeDisplay));
   70      }
   71  
   72      [RelayCommand(CanExecute = nameof(HasSelection))]
   73      private async Task BrewAsync()
   74      {
   75          if (SelectedEspresso is null) return;
   76  
   77          IsBrewing = true;
   78          BrewProgress = 0;
   79  
   80          var tcs = new TaskCompletionSource();
   81          var startTime = DateTime.Now;
   82          var duration = TimeSpan.FromMilliseconds(2500);
   83  
   84          var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
   85          timer.Tick += (s, e) =>
   86          {
   87              var elapsed = DateTime.Now - startTime;
   88              if (elapsed >= duration)
   89              {
   90                  timer.Stop();
   91                  BrewProgress = 1.0;
   92                  tcs.TrySetResult();
   93              }
   94              else
   95              {
   96                  BrewProgress = elapsed.TotalMilliseconds / duration.TotalMilliseconds;
   97              }
   98          };
   99          timer.Start();
  100  
  101          await tcs.Task;
  102          await Task.Delay(200);
  103  
  104          IsBrewing = false;
  105          BrewProgress = 0;
  106      }
  107  
  108      partial void OnSelectedEspressoChanged(EspressoItem? value)
  109      {
  110          BrewCommand.NotifyCanExecuteChanged();
  111      }
  112  
  113      public void SelectGrind(int level)
  114      {
  115          GrindLevel = (GrindLevel)Math.Clamp(level, 0, 2);
  116      }
  117  }
```

### `Caffe/Caffe/MainPage.xaml.cs`

```csharp
    1  using Caffe.Models;
    2  using Caffe.ViewModels;
    3  
    4  namespace Caffe;
    5  
    6  public sealed partial class MainPage : Page
    7  {
    8      public MainViewModel ViewModel { get; } = new();
    9  
   10      public MainPage()
   11      {
   12          this.InitializeComponent();
   13          this.DataContext = ViewModel;
   14  
   15          ViewModel.PropertyChanged += OnViewModelPropertyChanged;
   16      }
   17  
   18      private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
   19      {
   20          if (e.PropertyName is nameof(ViewModel.SelectedEspresso))
   21          {
   22              UpdateCardSelections();
   23          }
   24      }
   25  
   26      private void OnEspressoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(0);
   27      private void OnDoppioCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(1);
   28      private void OnRistrettoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(2);
   29      private void OnLungoCardTapped(object sender, TappedRoutedEventArgs e) => SelectCard(3);
   30  
   31      private void SelectCard(int index)
   32      {
   33          ViewModel.SelectedEspresso = ViewModel.EspressoItems[index];
   34      }
   35  
   36      private void UpdateCardSelections()
   37      {
   38          var selected = ViewModel.SelectedEspresso;
   39          EspressoCard.IsSelected = selected == ViewModel.EspressoItems[0];
   40          DoppioCard.IsSelected = selected == ViewModel.EspressoItems[1];
   41          RistrettoCard.IsSelected = selected == ViewModel.EspressoItems[2];
   42          LungoCard.IsSelected = selected == ViewModel.EspressoItems[3];
   43      }
   44  
   45      private async void OnBrewRequested(object sender, EventArgs e)
   46      {
   47          if (ViewModel.BrewCommand.CanExecute(null))
   48          {
   49              await ViewModel.BrewCommand.ExecuteAsync(null);
   50          }
   51      }
   52  }
```

### `Caffe/Caffe/Styles/AppResources.xaml`

```xml
    1  <ResourceDictionary
    2      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4      xmlns:converters="using:Caffe.Converters">
    5  
    6    <!-- Converters -->
    7    <converters:ReverseBoolToVisibilityConverter x:Key="ReverseBoolToVisibilityConverter" />
    8  
    9    <!-- Custom Fonts -->
   10    <FontFamily x:Key="CormorantLight">ms-appx:///Assets/Fonts/CormorantGaramond-Light.ttf#Cormorant Garamond</FontFamily>
   11    <FontFamily x:Key="CormorantRegular">ms-appx:///Assets/Fonts/CormorantGaramond-Regular.ttf#Cormorant Garamond</FontFamily>
   12    <FontFamily x:Key="DMSansRegular">ms-appx:///Assets/Fonts/DMSans-Regular.ttf#DM Sans</FontFamily>
   13    <FontFamily x:Key="DMSansMedium">ms-appx:///Assets/Fonts/DMSans-Medium.ttf#DM Sans</FontFamily>
   14  
   15    <!-- Caffè Color Brushes -->
   16    <SolidColorBrush x:Key="CaffeBackgroundBrush" Color="#FAFAFA" />
   17    <SolidColorBrush x:Key="CaffeSurfaceBrush" Color="#FFFFFF" />
   18    <SolidColorBrush x:Key="CaffePrimaryBrush" Color="#1B4332" />
   19    <SolidColorBrush x:Key="CaffePrimaryHoverBrush" Color="#14352A" />
   20    <SolidColorBrush x:Key="CaffeAccentRedBrush" Color="#C1121F" />
   21    <SolidColorBrush x:Key="CaffeAccentGreenBrush" Color="#2D6A4F" />
   22    <SolidColorBrush x:Key="CaffeTextPrimaryBrush" Color="#1A1A1A" />
   23    <SolidColorBrush x:Key="CaffeTextSecondaryBrush" Color="#888888" />
   24    <SolidColorBrush x:Key="CaffeTextMutedBrush" Color="#999999" />
   25    <SolidColorBrush x:Key="CaffeBorderBrush" Color="#E0E0E0" />
   26    <SolidColorBrush x:Key="CaffePrimaryDisabledBrush" Color="#591B4332" />
   27    <SolidColorBrush x:Key="CaffeOnPrimaryBrush" Color="#FFFFFF" />
   28    <SolidColorBrush x:Key="CaffeParticleBrush" Color="#1A1A1A" />
   29  
   30    <!-- Gradient Colors -->
   31    <Color x:Key="CoffeeDarkColor">#3D2314</Color>
   32    <Color x:Key="CoffeeLightColor">#5D3A1A</Color>
   33    <Color x:Key="CaffeTemperatureHighColor">#C1121F</Color>
   34    <Color x:Key="CaffeTemperatureLowColor">#E07070</Color>
   35  
   36    <!-- Typography Styles -->
   37    <!-- Logo: 3rem = 48px, Light -->
   38    <Style x:Key="LogoTextStyle" TargetType="TextBlock">
   39      <Setter Property="FontFamily" Value="{StaticResource CormorantLight}" />
   40      <Setter Property="FontSize" Value="48" />
   41      <Setter Property="Foreground" Value="{StaticResource CaffeTextPrimaryBrush}" />
   42    </Style>
   43  
   44    <!-- Tagline: uppercase, letter-spaced -->
   45    <Style x:Key="TaglineTextStyle" TargetType="TextBlock">
   46      <Setter Property="FontFamily" Value="{StaticResource DMSansMedium}" />
   47      <Setter Property="FontSize" Value="11" />
   48      <Setter Property="CharacterSpacing" Value="150" />
   49      <Setter Property="Foreground" Value="{StaticResource CaffeTextSecondaryBrush}" />
   50    </Style>
   51  
   52    <!-- Card Title: 1.4rem = 22px -->
   53    <Style x:Key="CardTitleTextStyle" TargetType="TextBlock">
   54      <Setter Property="FontFamily" Value="{StaticResource CormorantRegular}" />
   55      <Setter Property="FontSize" Value="22" />
   56      <Setter Property="Foreground" Value="{StaticResource CaffeTextPrimaryBrush}" />
   57    </Style>
   58  
   59    <!-- Card Description -->
   60    <Style x:Key="CardDescriptionTextStyle" TargetType="TextBlock">
   61      <Setter Property="FontFamily" Value="{StaticResource DMSansRegular}" />
   62      <Setter Property="FontSize" Value="12" />
   63      <Setter Property="Foreground" Value="{StaticResource CaffeTextMutedBrush}" />
   64    </Style>
   65  
   66    <!-- Parameter Value: 1.6rem = 26px -->
   67    <Style x:Key="ParameterValueTextStyle" TargetType="TextBlock">
   68      <Setter Property="FontFamily" Value="{StaticResource CormorantRegular}" />
   69      <Setter Property="FontSize" Value="26" />
   70      <Setter Property="Foreground" Value="{StaticResource CaffeTextPrimaryBrush}" />
   71    </Style>
   72  
   73    <!-- Parameter Label -->
   74    <Style x:Key="ParameterLabelTextStyle" TargetType="TextBlock">
   75      <Setter Property="FontFamily" Value="{StaticResource DMSansMedium}" />
   76      <Setter Property="FontSize" Value="10" />
   77      <Setter Property="CharacterSpacing" Value="100" />
   78      <Setter Property="Foreground" Value="{StaticResource CaffeTextSecondaryBrush}" />
   79    </Style>
   80  
   81    <!-- Volume Badge -->
   82    <Style x:Key="VolumeBadgeTextStyle" TargetType="TextBlock">
   83      <Setter Property="FontFamily" Value="{StaticResource DMSansMedium}" />
   84      <Setter Property="FontSize" Value="11" />
   85      <Setter Property="Foreground" Value="{StaticResource CaffeOnPrimaryBrush}" />
   86    </Style>
   87  
   88    <!-- Body Text -->
   89    <Style x:Key="BodyTextStyle" TargetType="TextBlock">
   90      <Setter Property="FontFamily" Value="{StaticResource DMSansRegular}" />
   91      <Setter Property="FontSize" Value="14" />
   92      <Setter Property="Foreground" Value="{StaticResource CaffeTextPrimaryBrush}" />
   93    </Style>
   94  
   95    <!-- Button Text -->
   96    <Style x:Key="ButtonTextStyle" TargetType="TextBlock">
   97      <Setter Property="FontFamily" Value="{StaticResource DMSansMedium}" />
   98      <Setter Property="FontSize" Value="15" />
   99      <Setter Property="Foreground" Value="{StaticResource CaffeOnPrimaryBrush}" />
  100    </Style>
  101  
  102    <!-- Grind Hint (italic) -->
  103    <Style x:Key="GrindHintTextStyle" TargetType="TextBlock">
  104      <Setter Property="FontFamily" Value="{StaticResource DMSansRegular}" />
  105      <Setter Property="FontSize" Value="11" />
  106      <Setter Property="FontStyle" Value="Italic" />
  107      <Setter Property="Foreground" Value="{StaticResource CaffeTextMutedBrush}" />
  108    </Style>
  109  
  110    <!-- Overview Label -->
  111    <Style x:Key="OverviewLabelTextStyle" TargetType="TextBlock">
  112      <Setter Property="FontFamily" Value="{StaticResource DMSansMedium}" />
  113      <Setter Property="FontSize" Value="10" />
  114      <Setter Property="CharacterSpacing" Value="100" />
  115      <Setter Property="Foreground" Value="{StaticResource CaffeOnPrimaryBrush}" />
  116      <Setter Property="Opacity" Value="0.7" />
  117    </Style>
  118  
  119    <!-- Overview Value -->
  120    <Style x:Key="OverviewValueTextStyle" TargetType="TextBlock">
  121      <Setter Property="FontFamily" Value="{StaticResource CormorantRegular}" />
  122      <Setter Property="FontSize" Value="18" />
  123      <Setter Property="Foreground" Value="{StaticResource CaffeOnPrimaryBrush}" />
  124    </Style>
  125  
  126    <!-- Brewing Title (based on CardTitleTextStyle, larger) -->
  127    <Style x:Key="BrewingTitleTextStyle" TargetType="TextBlock" BasedOn="{StaticResource CardTitleTextStyle}">
  128      <Setter Property="FontSize" Value="28" />
  129    </Style>
  130  
  131    <!-- Arc Label (based on ParameterLabelTextStyle, smaller) -->
  132    <Style x:Key="ArcLabelTextStyle" TargetType="TextBlock" BasedOn="{StaticResource ParameterLabelTextStyle}">
  133      <Setter Property="FontSize" Value="8" />
  134    </Style>
  135  
  136  </ResourceDictionary>
```
