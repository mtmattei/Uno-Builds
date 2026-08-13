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

The kit is undecided about `region` nodes. This gold contains **5**
of them, while the page it models is built from 25 nested layout
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

# The answer key under review: `08-pens-beers`

56 nodes · 78 edges · 2 unresolved items

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
  "graphId": "eval.pens-beers.gold",
  "name": "Pens - Beers (season beer tracker)",
  "sourceSummary": [
    {
      "type": "xaml",
      "label": "Pens/Pens/Presentation/BeersPage.xaml"
    },
    {
      "type": "csharp",
      "label": "Pens/Pens/Presentation/BeersViewModel.cs"
    },
    {
      "type": "csharp",
      "label": "Pens/Pens/Presentation/CaseBlock.cs"
    },
    {
      "type": "xaml",
      "label": "Pens/Pens/Presentation/Shell.xaml"
    },
    {
      "type": "xaml",
      "label": "Pens/Pens/App.xaml"
    },
    {
      "type": "csharp",
      "label": "Pens/Pens/Converters/Converters.cs"
    }
  ],
  "nodes": [
    {
      "id": "screen.beers",
      "type": "screen",
      "name": "Beers",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Page x:Class Pens.Presentation.BeersPage, hosted in Shell's NavigationContent."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Pens.Presentation.BeersPage",
          "xName": "PageRoot",
          "styleKey": "ArenaDarkBrush"
        }
      }
    },
    {
      "id": "region.header",
      "type": "region",
      "name": "Team header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        },
        "rationale": "Shell.xaml row 0: bordered header bar with logo and team identity."
      },
      "properties": {
        "uno": {
          "type": "Border",
          "property": "AutomationProperties.Name"
        }
      }
    },
    {
      "id": "asset.team-logo",
      "type": "asset",
      "name": "Penguins team logo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        },
        "rationale": "Image Source ms-appx:///Assets/images/pens-logo.png in a 56x56 rounded border."
      },
      "properties": {
        "uno": {
          "type": "Image",
          "source": "ms-appx:///Assets/images/pens-logo.png"
        }
      }
    },
    {
      "id": "content.header.team-name",
      "type": "content",
      "name": "PENGUINS",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "PENGUINS",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "fontResourceKey": "BebasNeueFont"
        }
      }
    },
    {
      "id": "content.header.league-name",
      "type": "content",
      "name": "DORVAL YOUNGTIMERS",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "DORVAL YOUNGTIMERS",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "fontResourceKey": "BarlowMedium"
        }
      }
    },
    {
      "id": "region.tab-bar",
      "type": "region",
      "name": "Bottom navigation",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        },
        "rationale": "Shell.xaml row 2: utu:TabBar with five TabBarItems in a 5-column ItemsPanel."
      },
      "properties": {
        "uno": {
          "type": "TabBar",
          "xName": "TabBar"
        }
      }
    },
    {
      "id": "component.tab-item",
      "type": "component",
      "name": "Tab item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        },
        "rationale": "Five utu:TabBarItem instances, each an icon over a caption; identical internals, Tag distinguishes them."
      },
      "role": "navigationTab",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag"
        },
        "internals": [
          "icon",
          "caption"
        ]
      }
    },
    {
      "id": "component.tab-item.schedule",
      "type": "component",
      "name": "Schedule tab",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "Schedule",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag=Schedule",
          "iconGlyph": "Calendar"
        }
      }
    },
    {
      "id": "component.tab-item.chat",
      "type": "component",
      "name": "Chat tab",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "Chat",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag=Chat",
          "iconGlyph": "Message"
        }
      }
    },
    {
      "id": "component.tab-item.beers",
      "type": "component",
      "name": "Beers tab",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "Beers",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag=Beers",
          "iconGlyph": "&#xE799;"
        }
      }
    },
    {
      "id": "component.tab-item.duties",
      "type": "component",
      "name": "Duties tab",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "Duties",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag=Duties",
          "iconGlyph": "Bullets"
        }
      }
    },
    {
      "id": "component.tab-item.roster",
      "type": "component",
      "name": "Roster tab",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      },
      "text": "Roster",
      "properties": {
        "uno": {
          "type": "TabBarItem",
          "property": "Tag=Roster",
          "iconGlyph": "People"
        }
      }
    },
    {
      "id": "screen.schedule",
      "type": "screen",
      "name": "Schedule",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "_pageFactories[\"Schedule\"] constructs SchedulePage with its view model."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Pens.Presentation.SchedulePage"
        },
        "scopeNote": "navigation target only; not modeled in this eval"
      }
    },
    {
      "id": "screen.chat",
      "type": "screen",
      "name": "Chat",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "_pageFactories[\"Chat\"] constructs ChatPage with its view model."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Pens.Presentation.ChatPage"
        },
        "scopeNote": "navigation target only; not modeled in this eval"
      }
    },
    {
      "id": "screen.duties",
      "type": "screen",
      "name": "Duties",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "_pageFactories[\"Duties\"] constructs DutiesPage with its view model."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Pens.Presentation.DutiesPage"
        },
        "scopeNote": "navigation target only; not modeled in this eval"
      }
    },
    {
      "id": "screen.roster",
      "type": "screen",
      "name": "Roster",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "_pageFactories[\"Roster\"] constructs RosterPage with its view model."
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Pens.Presentation.RosterPage"
        },
        "scopeNote": "navigation target only; not modeled in this eval"
      }
    },
    {
      "id": "region.beer-summary",
      "type": "region",
      "name": "Season consumption summary",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Centered StackPanel; AutomationProperties.Name 'Season beer consumption statistics'."
      }
    },
    {
      "id": "content.summary.cases-value",
      "type": "content",
      "name": "Consumed cases value",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "TextBlock bound to ConsumedCases, 80pt display face."
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "member": "ConsumedCases",
          "fontResourceKey": "BebasNeueFont"
        }
      }
    },
    {
      "id": "content.summary.cases-label",
      "type": "content",
      "name": "cases",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "cases",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "property": "x:Uid=BeersPage_Cases"
        }
      }
    },
    {
      "id": "content.summary.season-caption",
      "type": "content",
      "name": "CONSUMED THIS SEASON",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "CONSUMED THIS SEASON",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "property": "x:Uid=BeersPage_ConsumedThisSeason"
        }
      }
    },
    {
      "id": "content.summary.total-beers",
      "type": "content",
      "name": "Total beers line",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Runs: bound TotalBeers, ' beers ', then literal '(30 per case)'."
      },
      "text": "780 beers (30 per case)",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "member": "TotalBeers"
        }
      }
    },
    {
      "id": "component.card",
      "type": "component",
      "name": "Card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Bordered rounded container: BoardsDarkBrush fill, 1px border, used for the tracker and the four stat boxes."
      },
      "role": "container",
      "properties": {
        "uno": {
          "type": "Border",
          "styleKey": "BoardsDarkBrush"
        },
        "internals": [
          "background",
          "cornerRadius",
          "border"
        ]
      }
    },
    {
      "id": "component.card.season-tracker",
      "type": "component",
      "name": "Season tracker card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Border with LargeCornerRadius and SubtleBorderBrush containing the tracker grid."
      },
      "properties": {
        "uno": {
          "type": "Border",
          "resourceKey": "LargeCornerRadius"
        }
      }
    },
    {
      "id": "content.tracker.title",
      "type": "content",
      "name": "SEASON TRACKER",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "SEASON TRACKER",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "property": "x:Uid=BeersPage_SeasonTracker"
        }
      }
    },
    {
      "id": "content.tracker.counter",
      "type": "content",
      "name": "Case counter",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Runs: bound ConsumedCases then literal ' / 52 cases'."
      },
      "text": "26 / 52 cases",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "member": "ConsumedCases"
        }
      }
    },
    {
      "id": "control.tracker.case-grid",
      "type": "control",
      "name": "Case grid",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "ItemsRepeater over CaseBlocks with UniformGridLayout (28x28 min, 8 spacing); utu:CommandExtensions.Command binds ToggleCaseCommand."
      },
      "role": "itemsRepeater",
      "properties": {
        "uno": {
          "type": "ItemsRepeater",
          "member": "CaseBlocks",
          "property": "utu:CommandExtensions.Command=ToggleCaseCommand"
        }
      }
    },
    {
      "id": "component.case-tile",
      "type": "component",
      "name": "Case tile",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "DataTemplate x:DataType local:CaseBlock: a Border whose Background and BorderBrush come from IsConsumed through two converters. 52 instances (TotalCases)."
      },
      "role": "statusTile",
      "properties": {
        "uno": {
          "type": "Border",
          "resourceKey": "SmallCornerRadius",
          "member": "IsConsumed"
        },
        "instanceCount": 52
      }
    },
    {
      "id": "state.case-tile.consumed",
      "type": "state",
      "name": "Consumed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Converters/Converters.cs"
        },
        "rationale": "BoolToConsumedBackgroundConverter -> NeonAmberBrush and BoolToConsumedBorderConverter -> NeonAmberSemiBorderBrush when IsConsumed is true; the resting treatment is BoardsMidBrush / SubtleWhiteBorderBrush."
      },
      "properties": {
        "uno": {
          "member": "IsConsumed",
          "mechanism": "IValueConverter on the item template"
        }
      }
    },
    {
      "id": "region.legend",
      "type": "region",
      "name": "Tracker legend",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Centered horizontal AutoLayout; AutomationProperties.Name 'Legend for beer tracker'."
      }
    },
    {
      "id": "component.legend-swatch",
      "type": "component",
      "name": "Legend swatch",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "18x18 rounded Border plus caption; two instances distinguished only by fill."
      },
      "role": "legendKey",
      "properties": {
        "uno": {
          "type": "Border"
        },
        "internals": [
          "swatch",
          "caption"
        ]
      }
    },
    {
      "id": "component.legend-swatch.remaining",
      "type": "component",
      "name": "Remaining swatch",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "Remaining",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_Remaining"
        }
      }
    },
    {
      "id": "component.legend-swatch.consumed",
      "type": "component",
      "name": "Consumed swatch",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "Consumed",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_Consumed"
        }
      }
    },
    {
      "id": "region.stats-grid",
      "type": "region",
      "name": "Stats grid",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "2x2 Grid with 12px gutter columns/rows holding four stat cards."
      }
    },
    {
      "id": "component.stat-card",
      "type": "component",
      "name": "Stat card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        },
        "rationale": "Border with CardCornerRadius and CardBorderBrush, 16 padding, containing a 32pt display value over an 11pt letterspaced caption. Four identical instances."
      },
      "role": "statistic",
      "properties": {
        "uno": {
          "type": "Border",
          "resourceKey": "CardCornerRadius",
          "styleKey": "CardBorderBrush"
        },
        "internals": [
          "value",
          "caption"
        ]
      }
    },
    {
      "id": "component.stat-card.avg-per-game",
      "type": "component",
      "name": "Avg / Game",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "10 AVG / GAME",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_AvgPerGame"
        }
      }
    },
    {
      "id": "component.stat-card.games-played",
      "type": "component",
      "name": "Games Played",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "12 GAMES PLAYED",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_GamesPlayed"
        }
      }
    },
    {
      "id": "component.stat-card.top-consumer",
      "type": "component",
      "name": "Top Consumer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "B-Rob TOP CONSUMER",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_TopConsumer"
        }
      }
    },
    {
      "id": "component.stat-card.most-in-game",
      "type": "component",
      "name": "Most In A Game",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      },
      "text": "18 MOST IN A GAME",
      "properties": {
        "uno": {
          "type": "Border",
          "property": "x:Uid=BeersPage_MostInGame"
        }
      }
    },
    {
      "id": "token.color.neon-amber",
      "type": "token",
      "name": "Neon amber (accent)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#FFAA00",
      "properties": {
        "uno": {
          "resourceKey": "NeonAmberColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.powder-blue",
      "type": "token",
      "name": "Powder blue",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#B4D7E8",
      "properties": {
        "uno": {
          "resourceKey": "PowderBlueColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.arena-dark",
      "type": "token",
      "name": "Arena dark (page background)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#0A0C10",
      "properties": {
        "uno": {
          "resourceKey": "ArenaDarkColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.boards-dark",
      "type": "token",
      "name": "Boards dark (card surface)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#12161D",
      "properties": {
        "uno": {
          "resourceKey": "BoardsDarkColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.boards-mid",
      "type": "token",
      "name": "Boards mid (resting tile)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#1A1F2A",
      "properties": {
        "uno": {
          "resourceKey": "BoardsMidColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.text-primary",
      "type": "token",
      "name": "Text primary",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#F0F4F8",
      "properties": {
        "uno": {
          "resourceKey": "TextPrimaryColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.text-muted",
      "type": "token",
      "name": "Text muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#6B7A8F",
      "properties": {
        "uno": {
          "resourceKey": "TextMutedColor",
          "resourceType": "Color"
        }
      }
    },
    {
      "id": "token.color.border-subtle",
      "type": "token",
      "name": "Subtle border",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#0DFFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "SubtleBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.border-card",
      "type": "token",
      "name": "Card border",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#08FFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "CardBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.border-powder-blue",
      "type": "token",
      "name": "Powder blue border",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#4DB4D7E8",
      "properties": {
        "uno": {
          "resourceKey": "PowderBlueBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.border-amber-semi",
      "type": "token",
      "name": "Amber semi border (consumed tile)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#80FFAA00",
      "properties": {
        "uno": {
          "resourceKey": "NeonAmberSemiBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.border-white-subtle",
      "type": "token",
      "name": "Subtle white border (resting tile)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#1AFFFFFF",
      "properties": {
        "uno": {
          "resourceKey": "SubtleWhiteBorderBrush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.gradient.neon-amber",
      "type": "token",
      "name": "Neon amber gradient (consumed legend swatch)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "color",
      "value": "#FFAA00 -> #CC8800",
      "properties": {
        "uno": {
          "resourceKey": "NeonAmberGradientBrush",
          "resourceType": "LinearGradientBrush"
        }
      }
    },
    {
      "id": "token.radius.small",
      "type": "token",
      "name": "Small radius",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "radius",
      "value": "4",
      "properties": {
        "uno": {
          "resourceKey": "SmallCornerRadius",
          "resourceType": "CornerRadius"
        }
      }
    },
    {
      "id": "token.radius.card",
      "type": "token",
      "name": "Card radius",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "radius",
      "value": "12",
      "properties": {
        "uno": {
          "resourceKey": "CardCornerRadius",
          "resourceType": "CornerRadius"
        }
      }
    },
    {
      "id": "token.radius.large",
      "type": "token",
      "name": "Large radius",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "radius",
      "value": "16",
      "properties": {
        "uno": {
          "resourceKey": "LargeCornerRadius",
          "resourceType": "CornerRadius"
        }
      }
    },
    {
      "id": "token.font.display",
      "type": "token",
      "name": "Display face (Bebas Neue)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/BebasNeue-Regular.ttf#Bebas Neue",
      "properties": {
        "uno": {
          "resourceKey": "BebasNeueFont",
          "resourceType": "FontFamily"
        }
      }
    },
    {
      "id": "token.font.body-medium",
      "type": "token",
      "name": "Body medium (Barlow Medium)",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/App.xaml"
        },
        "rationale": "Declared in App.xaml application resources."
      },
      "category": "typography",
      "value": "ms-appx:///Assets/Fonts/Barlow-Medium.ttf#Barlow",
      "properties": {
        "uno": {
          "resourceKey": "BarlowMedium",
          "resourceType": "FontFamily"
        }
      }
    }
  ],
  "edges": [
    {
      "from": "component.tab-item.schedule",
      "relation": "instance-of",
      "to": "component.tab-item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "contains",
      "to": "component.tab-item.schedule",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item.chat",
      "relation": "instance-of",
      "to": "component.tab-item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "contains",
      "to": "component.tab-item.chat",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item.beers",
      "relation": "instance-of",
      "to": "component.tab-item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "contains",
      "to": "component.tab-item.beers",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item.duties",
      "relation": "instance-of",
      "to": "component.tab-item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "contains",
      "to": "component.tab-item.duties",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item.roster",
      "relation": "instance-of",
      "to": "component.tab-item",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "contains",
      "to": "component.tab-item.roster",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item.schedule",
      "relation": "navigates-to",
      "to": "screen.schedule",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "OnTabSelectionChanged -> NavigateToTab(tag) -> NavigationContent."
      }
    },
    {
      "from": "component.tab-item.chat",
      "relation": "navigates-to",
      "to": "screen.chat",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "OnTabSelectionChanged -> NavigateToTab(tag) -> NavigationContent."
      }
    },
    {
      "from": "component.tab-item.duties",
      "relation": "navigates-to",
      "to": "screen.duties",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "OnTabSelectionChanged -> NavigateToTab(tag) -> NavigationContent."
      }
    },
    {
      "from": "component.tab-item.roster",
      "relation": "navigates-to",
      "to": "screen.roster",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "OnTabSelectionChanged -> NavigateToTab(tag) -> NavigationContent."
      }
    },
    {
      "from": "component.tab-item.beers",
      "relation": "navigates-to",
      "to": "screen.beers",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/Shell.xaml.cs"
        },
        "rationale": "OnTabSelectionChanged -> NavigateToTab(\"Beers\") -> this screen."
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "instance-of",
      "to": "component.card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.legend-swatch.remaining",
      "relation": "instance-of",
      "to": "component.legend-swatch",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.legend",
      "relation": "contains",
      "to": "component.legend-swatch.remaining",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.legend-swatch.consumed",
      "relation": "instance-of",
      "to": "component.legend-swatch",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.legend",
      "relation": "contains",
      "to": "component.legend-swatch.consumed",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card",
      "relation": "instance-of",
      "to": "component.card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card.avg-per-game",
      "relation": "instance-of",
      "to": "component.stat-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.stats-grid",
      "relation": "contains",
      "to": "component.stat-card.avg-per-game",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card.games-played",
      "relation": "instance-of",
      "to": "component.stat-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.stats-grid",
      "relation": "contains",
      "to": "component.stat-card.games-played",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card.top-consumer",
      "relation": "instance-of",
      "to": "component.stat-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.stats-grid",
      "relation": "contains",
      "to": "component.stat-card.top-consumer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card.most-in-game",
      "relation": "instance-of",
      "to": "component.stat-card",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.stats-grid",
      "relation": "contains",
      "to": "component.stat-card.most-in-game",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "region.header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "region.beer-summary",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "component.card.season-tracker",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "region.legend",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "region.stats-grid",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "screen.beers",
      "relation": "contains",
      "to": "region.tab-bar",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.header",
      "relation": "contains",
      "to": "asset.team-logo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.header",
      "relation": "contains",
      "to": "content.header.team-name",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.header",
      "relation": "contains",
      "to": "content.header.league-name",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.beer-summary",
      "relation": "contains",
      "to": "content.summary.cases-value",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.beer-summary",
      "relation": "contains",
      "to": "content.summary.cases-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.beer-summary",
      "relation": "contains",
      "to": "content.summary.season-caption",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "region.beer-summary",
      "relation": "contains",
      "to": "content.summary.total-beers",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "contains",
      "to": "content.tracker.title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "contains",
      "to": "content.tracker.counter",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "contains",
      "to": "control.tracker.case-grid",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "control.tracker.case-grid",
      "relation": "contains",
      "to": "component.case-tile",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.case-tile",
      "relation": "has-state",
      "to": "state.case-tile.consumed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Converters/Converters.cs"
        },
        "rationale": "Per-tile visual state driven by the CaseBlock.IsConsumed flag."
      }
    },
    {
      "from": "control.tracker.case-grid",
      "relation": "triggers",
      "to": "state.case-tile.consumed",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Presentation/BeersViewModel.cs"
        },
        "rationale": "utu:CommandExtensions.Command on the repeater invokes ToggleCaseAsync(CaseBlock), which sets ConsumedCases from the tapped block's index."
      }
    },
    {
      "from": "screen.beers",
      "relation": "uses-token",
      "to": "token.color.arena-dark",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.color.boards-dark",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card",
      "relation": "uses-token",
      "to": "token.color.border-card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "uses-token",
      "to": "token.radius.large",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.card.season-tracker",
      "relation": "uses-token",
      "to": "token.color.border-subtle",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card",
      "relation": "uses-token",
      "to": "token.radius.card",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.case-tile",
      "relation": "uses-token",
      "to": "token.radius.small",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.case-tile",
      "relation": "uses-token",
      "to": "token.color.boards-mid",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.case-tile",
      "relation": "uses-token",
      "to": "token.color.border-white-subtle",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "state.case-tile.consumed",
      "relation": "uses-token",
      "to": "token.color.neon-amber",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Converters/Converters.cs"
        }
      }
    },
    {
      "from": "state.case-tile.consumed",
      "relation": "uses-token",
      "to": "token.color.border-amber-semi",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Pens/Pens/Converters/Converters.cs"
        }
      }
    },
    {
      "from": "content.summary.cases-value",
      "relation": "uses-token",
      "to": "token.color.neon-amber",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.summary.cases-value",
      "relation": "uses-token",
      "to": "token.font.display",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.summary.cases-label",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.summary.season-caption",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.tracker.title",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.tracker.counter",
      "relation": "uses-token",
      "to": "token.color.powder-blue",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card",
      "relation": "uses-token",
      "to": "token.font.display",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card",
      "relation": "uses-token",
      "to": "token.color.neon-amber",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.stat-card",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.legend-swatch.consumed",
      "relation": "uses-token",
      "to": "token.gradient.neon-amber",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "component.legend-swatch.remaining",
      "relation": "uses-token",
      "to": "token.color.boards-mid",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/BeersPage.xaml"
        }
      }
    },
    {
      "from": "content.header.team-name",
      "relation": "uses-token",
      "to": "token.font.display",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "content.header.team-name",
      "relation": "uses-token",
      "to": "token.color.text-primary",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "content.header.league-name",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "asset.team-logo",
      "relation": "uses-token",
      "to": "token.color.border-powder-blue",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item",
      "relation": "uses-token",
      "to": "token.font.body-medium",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "component.tab-item",
      "relation": "uses-token",
      "to": "token.color.text-muted",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.header",
      "relation": "uses-token",
      "to": "token.color.border-subtle",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    },
    {
      "from": "region.tab-bar",
      "relation": "uses-token",
      "to": "token.color.border-subtle",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Pens/Pens/Presentation/Shell.xaml"
        }
      }
    }
  ],
  "unresolved": [
    {
      "id": "unresolved.stat-values",
      "question": "Are the four stat values placeholders?",
      "relatedIds": [
        "component.stat-card",
        "component.stat-card.avg-per-game",
        "component.stat-card.games-played",
        "component.stat-card.top-consumer",
        "component.stat-card.most-in-game"
      ],
      "reason": "The hero count and tracker counter are bound to the ViewModel, but all four stat values ('10', '12', 'B-Rob', '18') are literal Text in XAML with no binding and no ViewModel member behind them. Whether that is deliberate or unfinished is not decidable from source."
    },
    {
      "id": "unresolved.loading-error-state",
      "question": "Where are IsLoading and HasError rendered?",
      "relatedIds": [
        "screen.beers"
      ],
      "reason": "BeersViewModel declares IsLoading, ErrorMessage and HasError and sets them in LoadBeerCountAsync and the toggle rollback path, but BeersPage.xaml binds none of them. Nothing in the page binds them, so this screen renders no load or failure feedback at all. Recorded because the ViewModel declares presentation state the screen never shows, which is a defect worth surfacing rather than an ambiguity."
    }
  ]
}
```

## Source files the gold cites

Line-numbered so you can cite `file:line`.

### `Pens/Pens/Presentation/BeersPage.xaml`

```xml
    1  <Page x:Class="Pens.Presentation.BeersPage"
    2        x:Name="PageRoot"
    3        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    4        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    5        xmlns:utu="using:Uno.Toolkit.UI"
    6        xmlns:local="using:Pens.Presentation"
    7        Background="{StaticResource ArenaDarkBrush}">
    8  
    9    <ScrollViewer Padding="20,10,20,20" VerticalScrollBarVisibility="Auto">
   10      <utu:AutoLayout Spacing="20">
   11  
   12        <!-- Beer Header -->
   13        <StackPanel HorizontalAlignment="Center" Margin="0,0,0,4"
   14                    AutomationProperties.Name="Season beer consumption statistics">
   15          <!-- Total Row -->
   16          <utu:AutoLayout Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
   17            <TextBlock Text="{Binding ConsumedCases}"
   18                       Foreground="{StaticResource NeonAmberBrush}"
   19                       FontFamily="{StaticResource BebasNeueFont}"
   20                       FontSize="80" />
   21            <TextBlock x:Uid="BeersPage_Cases"
   22                       Text="cases"
   23                       Foreground="{StaticResource TextMutedBrush}"
   24                       FontFamily="{StaticResource BarlowMedium}"
   25                       FontSize="24"
   26                       CharacterSpacing="40"
   27                       VerticalAlignment="Bottom"
   28                       Margin="0,0,0,16" />
   29          </utu:AutoLayout>
   30  
   31          <!-- Subtitle -->
   32          <TextBlock x:Uid="BeersPage_ConsumedThisSeason"
   33                     Text="CONSUMED THIS SEASON"
   34                     Foreground="{StaticResource TextMutedBrush}"
   35                     FontSize="12"
   36                     CharacterSpacing="80"
   37                     HorizontalAlignment="Center"
   38                     Margin="0,8,0,0" />
   39  
   40          <!-- Beer Count -->
   41          <TextBlock HorizontalAlignment="Center" Margin="0,4,0,0" FontSize="14">
   42            <Run Text="{Binding TotalBeers}" Foreground="{StaticResource NeonAmberBrush}" />
   43            <Run Text=" beers " Foreground="{StaticResource NeonAmberBrush}" />
   44            <Run Text="(30 per case)" Foreground="{StaticResource TextMutedBrush}" />
   45          </TextBlock>
   46        </StackPanel>
   47  
   48        <!-- Cases Section -->
   49        <Border Background="{StaticResource BoardsDarkBrush}"
   50                CornerRadius="{StaticResource LargeCornerRadius}"
   51                Padding="20"
   52                BorderBrush="{StaticResource SubtleBorderBrush}"
   53                BorderThickness="1">
   54          <utu:AutoLayout Spacing="16">
   55            <!-- Section Header -->
   56            <Grid>
   57              <TextBlock x:Uid="BeersPage_SeasonTracker"
   58                         Text="SEASON TRACKER"
   59                         Foreground="{StaticResource TextMutedBrush}"
   60                         FontSize="14"
   61                         CharacterSpacing="80"
   62                         FontWeight="SemiBold"
   63                         HorizontalAlignment="Left" />
   64              <TextBlock HorizontalAlignment="Right" FontSize="13">
   65                <Run Text="{Binding ConsumedCases}" Foreground="{StaticResource PowderBlueBrush}" />
   66                <Run Text=" / 52 cases" Foreground="{StaticResource PowderBlueBrush}" />
   67              </TextBlock>
   68            </Grid>
   69  
   70            <!-- Cases Grid -->
   71            <ItemsRepeater ItemsSource="{Binding CaseBlocks}"
   72                           utu:CommandExtensions.Command="{Binding ToggleCaseCommand}">
   73              <ItemsRepeater.Layout>
   74                <UniformGridLayout MinItemWidth="28"
   75                                   MinItemHeight="28"
   76                                   MinColumnSpacing="8"
   77                                   MinRowSpacing="8"
   78                                   ItemsStretch="Fill" />
   79              </ItemsRepeater.Layout>
   80              <ItemsRepeater.ItemTemplate>
   81                <DataTemplate x:DataType="local:CaseBlock">
   82                  <Border CornerRadius="{StaticResource SmallCornerRadius}"
   83                          Background="{Binding IsConsumed, Converter={StaticResource BoolToConsumedBackgroundConverter}}"
   84                          BorderBrush="{Binding IsConsumed, Converter={StaticResource BoolToConsumedBorderConverter}}"
   85                          BorderThickness="1" />
   86                </DataTemplate>
   87              </ItemsRepeater.ItemTemplate>
   88            </ItemsRepeater>
   89          </utu:AutoLayout>
   90        </Border>
   91  
   92        <!-- Legend -->
   93        <utu:AutoLayout Orientation="Horizontal" Spacing="24" HorizontalAlignment="Center"
   94                        AutomationProperties.Name="Legend for beer tracker">
   95          <utu:AutoLayout Orientation="Horizontal" Spacing="8">
   96            <Border Width="18" Height="18"
   97                    CornerRadius="{StaticResource SmallCornerRadius}"
   98                    Background="{StaticResource BoardsMidBrush}"
   99                    BorderBrush="{StaticResource SubtleBorderBrush}"
  100                    BorderThickness="1" />
  101            <TextBlock x:Uid="BeersPage_Remaining"
  102                       Text="Remaining"
  103                       Foreground="{StaticResource TextMutedBrush}"
  104                       FontSize="12"
  105                       VerticalAlignment="Center" />
  106          </utu:AutoLayout>
  107          <utu:AutoLayout Orientation="Horizontal" Spacing="8">
  108            <Border Width="18" Height="18"
  109                    CornerRadius="{StaticResource SmallCornerRadius}"
  110                    Background="{StaticResource NeonAmberGradientBrush}" />
  111            <TextBlock x:Uid="BeersPage_Consumed"
  112                       Text="Consumed"
  113                       Foreground="{StaticResource TextMutedBrush}"
  114                       FontSize="12"
  115                       VerticalAlignment="Center" />
  116          </utu:AutoLayout>
  117        </utu:AutoLayout>
  118  
  119        <!-- Stats Grid -->
  120        <Grid>
  121          <Grid.ColumnDefinitions>
  122            <ColumnDefinition Width="*" />
  123            <ColumnDefinition Width="12" />
  124            <ColumnDefinition Width="*" />
  125          </Grid.ColumnDefinitions>
  126          <Grid.RowDefinitions>
  127            <RowDefinition Height="Auto" />
  128            <RowDefinition Height="12" />
  129            <RowDefinition Height="Auto" />
  130          </Grid.RowDefinitions>
  131  
  132          <!-- Stat Box 1 -->
  133          <Border Grid.Column="0" Grid.Row="0"
  134                  Background="{StaticResource BoardsDarkBrush}"
  135                  CornerRadius="{StaticResource CardCornerRadius}"
  136                  Padding="16"
  137                  BorderBrush="{StaticResource CardBorderBrush}"
  138                  BorderThickness="1"
  139                  AutomationProperties.Name="Average beers per game: 10">
  140            <StackPanel>
  141              <TextBlock Text="10"
  142                         Foreground="{StaticResource NeonAmberBrush}"
  143                         FontFamily="{StaticResource BebasNeueFont}"
  144                         FontSize="32" />
  145              <TextBlock x:Uid="BeersPage_AvgPerGame"
  146                         Text="AVG / GAME"
  147                         Foreground="{StaticResource TextMutedBrush}"
  148                         FontSize="11"
  149                         CharacterSpacing="40" />
  150            </StackPanel>
  151          </Border>
  152  
  153          <!-- Stat Box 2 -->
  154          <Border Grid.Column="2" Grid.Row="0"
  155                  Background="{StaticResource BoardsDarkBrush}"
  156                  CornerRadius="{StaticResource CardCornerRadius}"
  157                  Padding="16"
  158                  BorderBrush="{StaticResource CardBorderBrush}"
  159                  BorderThickness="1"
  160                  AutomationProperties.Name="Games played: 12">
  161            <StackPanel>
  162              <TextBlock Text="12"
  163                         Foreground="{StaticResource NeonAmberBrush}"
  164                         FontFamily="{StaticResource BebasNeueFont}"
  165                         FontSize="32" />
  166              <TextBlock x:Uid="BeersPage_GamesPlayed"
  167                         Text="GAMES PLAYED"
  168                         Foreground="{StaticResource TextMutedBrush}"
  169                         FontSize="11"
  170                         CharacterSpacing="40" />
  171            </StackPanel>
  172          </Border>
  173  
  174          <!-- Stat Box 3 -->
  175          <Border Grid.Column="0" Grid.Row="2"
  176                  Background="{StaticResource BoardsDarkBrush}"
  177                  CornerRadius="{StaticResource CardCornerRadius}"
  178                  Padding="16"
  179                  BorderBrush="{StaticResource CardBorderBrush}"
  180                  BorderThickness="1"
  181                  AutomationProperties.Name="Top beer consumer: B-Rob">
  182            <StackPanel>
  183              <TextBlock Text="B-Rob"
  184                         Foreground="{StaticResource NeonAmberBrush}"
  185                         FontFamily="{StaticResource BebasNeueFont}"
  186                         FontSize="32" />
  187              <TextBlock x:Uid="BeersPage_TopConsumer"
  188                         Text="TOP CONSUMER"
  189                         Foreground="{StaticResource TextMutedBrush}"
  190                         FontSize="11"
  191                         CharacterSpacing="40" />
  192            </StackPanel>
  193          </Border>
  194  
  195          <!-- Stat Box 4 -->
  196          <Border Grid.Column="2" Grid.Row="2"
  197                  Background="{StaticResource BoardsDarkBrush}"
  198                  CornerRadius="{StaticResource CardCornerRadius}"
  199                  Padding="16"
  200                  BorderBrush="{StaticResource CardBorderBrush}"
  201                  BorderThickness="1"
  202                  AutomationProperties.Name="Most beers in a game: 18">
  203            <StackPanel>
  204              <TextBlock Text="18"
  205                         Foreground="{StaticResource NeonAmberBrush}"
  206                         FontFamily="{StaticResource BebasNeueFont}"
  207                         FontSize="32" />
  208              <TextBlock x:Uid="BeersPage_MostInGame"
  209                         Text="MOST IN A GAME"
  210                         Foreground="{StaticResource TextMutedBrush}"
  211                         FontSize="11"
  212                         CharacterSpacing="40" />
  213            </StackPanel>
  214          </Border>
  215        </Grid>
  216  
  217        <!-- Bottom padding for nav -->
  218        <Border Height="24" />
  219      </utu:AutoLayout>
  220    </ScrollViewer>
  221  </Page>
```

### `Pens/Pens/Presentation/Shell.xaml`

```xml
    1  <Page x:Class="Pens.Presentation.Shell"
    2        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4        xmlns:utu="using:Uno.Toolkit.UI"
    5        Background="{StaticResource ArenaDarkBrush}">
    6  
    7    <Grid Background="{StaticResource ArenaDarkBrush}">
    8      <Grid.RowDefinitions>
    9        <RowDefinition Height="Auto" />
   10        <RowDefinition Height="*" />
   11        <RowDefinition Height="Auto" />
   12      </Grid.RowDefinitions>
   13  
   14      <!-- Header -->
   15      <Grid Grid.Row="0"
   16            utu:SafeArea.Insets="Top">
   17        <Border Padding="20,20,20,16"
   18                BorderBrush="{StaticResource SubtleBorderBrush}"
   19                BorderThickness="0,0,0,1"
   20                Background="{StaticResource ArenaDarkBrush}"
   21                AutomationProperties.Name="Team header">
   22          <StackPanel Orientation="Horizontal" Spacing="16">
   23            <!-- Team Logo -->
   24            <Border Width="56" Height="56"
   25                    CornerRadius="12"
   26                    BorderBrush="{StaticResource PowderBlueBorderBrush}"
   27                    BorderThickness="2"
   28                    AutomationProperties.Name="Penguins team logo">
   29              <Image Source="ms-appx:///Assets/images/pens-logo.png"
   30                     Stretch="UniformToFill" />
   31            </Border>
   32  
   33            <!-- Team Info -->
   34            <StackPanel VerticalAlignment="Center">
   35              <TextBlock x:Uid="Shell_TeamName"
   36                         Text="PENGUINS"
   37                         Foreground="{StaticResource TextPrimaryBrush}"
   38                         FontFamily="{StaticResource BebasNeueFont}"
   39                         FontSize="32"
   40                         CharacterSpacing="80" />
   41              <TextBlock x:Uid="Shell_LeagueName"
   42                         Text="DORVAL YOUNGTIMERS"
   43                         Foreground="{StaticResource TextMutedBrush}"
   44                         FontFamily="{StaticResource BarlowMedium}"
   45                         FontSize="11"
   46                         CharacterSpacing="60" />
   47            </StackPanel>
   48          </StackPanel>
   49        </Border>
   50      </Grid>
   51  
   52      <!-- Content Area -->
   53      <ContentControl x:Name="NavigationContent"
   54                      Grid.Row="1"
   55                      HorizontalContentAlignment="Stretch"
   56                      VerticalContentAlignment="Stretch" />
   57  
   58      <!-- Bottom Navigation -->
   59      <Grid Grid.Row="2"
   60            utu:SafeArea.Insets="Bottom">
   61        <Border Padding="8,8,8,20"
   62                Background="{StaticResource ArenaDarkBrush}"
   63                BorderBrush="{StaticResource SubtleBorderBrush}"
   64                BorderThickness="0,1,0,0">
   65          <utu:TabBar x:Name="TabBar"
   66                      SelectedIndex="0">
   67            <utu:TabBar.ItemsPanel>
   68              <ItemsPanelTemplate>
   69                <Grid>
   70                  <Grid.ColumnDefinitions>
   71                    <ColumnDefinition Width="*" />
   72                    <ColumnDefinition Width="*" />
   73                    <ColumnDefinition Width="*" />
   74                    <ColumnDefinition Width="*" />
   75                    <ColumnDefinition Width="*" />
   76                  </Grid.ColumnDefinitions>
   77                </Grid>
   78              </ItemsPanelTemplate>
   79            </utu:TabBar.ItemsPanel>
   80  
   81          <utu:TabBarItem Grid.Column="0" Tag="Schedule"
   82                          AutomationProperties.Name="Schedule tab">
   83            <StackPanel HorizontalAlignment="Center" Spacing="4">
   84              <SymbolIcon Symbol="Calendar"
   85                          Foreground="{StaticResource TextMutedBrush}"
   86                          HorizontalAlignment="Center" />
   87              <TextBlock x:Uid="Shell_Tab_Schedule"
   88                         Text="Schedule"
   89                         FontFamily="{StaticResource BarlowMedium}"
   90                         FontSize="10"
   91                         Foreground="{StaticResource TextMutedBrush}"
   92                         HorizontalAlignment="Center" />
   93            </StackPanel>
   94          </utu:TabBarItem>
   95  
   96          <utu:TabBarItem Grid.Column="1" Tag="Chat"
   97                          AutomationProperties.Name="Chat tab">
   98            <StackPanel HorizontalAlignment="Center" Spacing="4">
   99              <SymbolIcon Symbol="Message"
  100                          Foreground="{StaticResource TextMutedBrush}"
  101                          HorizontalAlignment="Center" />
  102              <TextBlock x:Uid="Shell_Tab_Chat"
  103                         Text="Chat"
  104                         FontFamily="{StaticResource BarlowMedium}"
  105                         FontSize="10"
  106                         Foreground="{StaticResource TextMutedBrush}"
  107                         HorizontalAlignment="Center" />
  108            </StackPanel>
  109          </utu:TabBarItem>
  110  
  111          <utu:TabBarItem Grid.Column="2" Tag="Beers"
  112                          AutomationProperties.Name="Beers tab">
  113            <StackPanel HorizontalAlignment="Center" Spacing="4">
  114              <FontIcon Glyph="&#xE799;"
  115                        FontSize="20"
  116                        Foreground="{StaticResource TextMutedBrush}"
  117                        HorizontalAlignment="Center" />
  118              <TextBlock x:Uid="Shell_Tab_Beers"
  119                         Text="Beers"
  120                         FontFamily="{StaticResource BarlowMedium}"
  121                         FontSize="10"
  122                         Foreground="{StaticResource TextMutedBrush}"
  123                         HorizontalAlignment="Center" />
  124            </StackPanel>
  125          </utu:TabBarItem>
  126  
  127          <utu:TabBarItem Grid.Column="3" Tag="Duties"
  128                          AutomationProperties.Name="Duties tab">
  129            <StackPanel HorizontalAlignment="Center" Spacing="4">
  130              <SymbolIcon Symbol="Bullets"
  131                          Foreground="{StaticResource TextMutedBrush}"
  132                          HorizontalAlignment="Center" />
  133              <TextBlock x:Uid="Shell_Tab_Duties"
  134                         Text="Duties"
  135                         FontFamily="{StaticResource BarlowMedium}"
  136                         FontSize="10"
  137                         Foreground="{StaticResource TextMutedBrush}"
  138                         HorizontalAlignment="Center" />
  139            </StackPanel>
  140          </utu:TabBarItem>
  141  
  142          <utu:TabBarItem Grid.Column="4" Tag="Roster"
  143                          AutomationProperties.Name="Roster tab">
  144            <StackPanel HorizontalAlignment="Center" Spacing="4">
  145              <SymbolIcon Symbol="People"
  146                          Foreground="{StaticResource TextMutedBrush}"
  147                          HorizontalAlignment="Center" />
  148              <TextBlock x:Uid="Shell_Tab_Roster"
  149                         Text="Roster"
  150                         FontFamily="{StaticResource BarlowMedium}"
  151                         FontSize="10"
  152                         Foreground="{StaticResource TextMutedBrush}"
  153                         HorizontalAlignment="Center" />
  154            </StackPanel>
  155          </utu:TabBarItem>
  156        </utu:TabBar>
  157      </Border>
  158    </Grid>
  159    </Grid>
  160  </Page>
```

### `Pens/Pens/Presentation/Shell.xaml.cs`

```csharp
    1  namespace Pens.Presentation;
    2  
    3  public sealed partial class Shell : Page
    4  {
    5      private readonly IServiceProvider _services;
    6      private readonly Dictionary<string, Func<Page>> _pageFactories;
    7  
    8      public Shell(IServiceProvider services)
    9      {
   10          _services = services;
   11          this.InitializeComponent();
   12  
   13          _pageFactories = new Dictionary<string, Func<Page>>
   14          {
   15              ["Schedule"] = () => new SchedulePage { DataContext = _services.GetRequiredService<ScheduleViewModel>() },
   16              ["Chat"] = () => new ChatPage { DataContext = _services.GetRequiredService<ChatViewModel>() },
   17              ["Beers"] = () => new BeersPage { DataContext = _services.GetRequiredService<BeersViewModel>() },
   18              ["Duties"] = () => new DutiesPage { DataContext = _services.GetRequiredService<DutiesViewModel>() },
   19              ["Roster"] = () => new RosterPage { DataContext = _services.GetRequiredService<RosterViewModel>() }
   20          };
   21  
   22          TabBar.SelectionChanged += OnTabSelectionChanged;
   23  
   24          // Load default page
   25          this.Loaded += (s, e) => NavigateToTab("Schedule");
   26      }
   27  
   28      private void OnTabSelectionChanged(object sender, Uno.Toolkit.UI.TabBarSelectionChangedEventArgs e)
   29      {
   30          if (e.NewItem is Uno.Toolkit.UI.TabBarItem tab)
   31          {
   32              var tabName = tab.Tag?.ToString();
   33              if (!string.IsNullOrEmpty(tabName))
   34              {
   35                  NavigateToTab(tabName);
   36              }
   37          }
   38      }
   39  
   40      private void NavigateToTab(string tabName)
   41      {
   42          if (_pageFactories.TryGetValue(tabName, out var factory))
   43          {
   44              try
   45              {
   46                  NavigationContent.Content = factory();
   47              }
   48              catch (Exception ex)
   49              {
   50                  System.Diagnostics.Debug.WriteLine($"Navigation error for {tabName}: {ex}");
   51              }
   52          }
   53      }
   54  }
```

### `Pens/Pens/Converters/Converters.cs`

```csharp
    1  using Pens.Models;
    2  using Microsoft.UI;
    3  using Microsoft.UI.Xaml;
    4  using Microsoft.UI.Xaml.Media;
    5  using Windows.UI;
    6  
    7  namespace Pens.Converters;
    8  
    9  /// <summary>
   10  /// Helper class to retrieve brushes from Application resources.
   11  /// </summary>
   12  internal static class ResourceHelper
   13  {
   14      public static Brush GetBrush(string key, Brush fallback)
   15      {
   16          if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is Brush brush)
   17          {
   18              return brush;
   19          }
   20          return fallback;
   21      }
   22  
   23      public static Color GetColor(string key, Color fallback)
   24      {
   25          if (Application.Current.Resources.TryGetValue(key, out var resource) && resource is Color color)
   26          {
   27              return color;
   28          }
   29          return fallback;
   30      }
   31  }
   32  
   33  public class ToUpperConverter : IValueConverter
   34  {
   35      public object? Convert(object? value, Type targetType, object? parameter, string language)
   36      {
   37          return value?.ToString()?.ToUpperInvariant();
   38      }
   39  
   40      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
   41      {
   42          throw new NotImplementedException();
   43      }
   44  }
   45  
   46  public class StatusToTextConverter : IValueConverter
   47  {
   48      public object? Convert(object? value, Type targetType, object? parameter, string language)
   49      {
   50          return value switch
   51          {
   52              PlayerStatus.In => "IN",
   53              PlayerStatus.Out => "OUT",
   54              PlayerStatus.Pending => "?",
   55              _ => ""
   56          };
   57      }
   58  
   59      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
   60      {
   61          throw new NotImplementedException();
   62      }
   63  }
   64  
   65  public class StatusToBackgroundConverter : IValueConverter
   66  {
   67      private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);
   68  
   69      public object? Convert(object? value, Type targetType, object? parameter, string language)
   70      {
   71          return value switch
   72          {
   73              PlayerStatus.In => ResourceHelper.GetBrush("StatusInBackgroundBrush", TransparentBrush),
   74              PlayerStatus.Out => ResourceHelper.GetBrush("StatusOutBackgroundBrush", TransparentBrush),
   75              PlayerStatus.Pending => ResourceHelper.GetBrush("StatusPendingBackgroundBrush", TransparentBrush),
   76              _ => TransparentBrush
   77          };
   78      }
   79  
   80      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
   81      {
   82          throw new NotImplementedException();
   83      }
   84  }
   85  
   86  public class StatusToForegroundConverter : IValueConverter
   87  {
   88      private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
   89  
   90      public object? Convert(object? value, Type targetType, object? parameter, string language)
   91      {
   92          return value switch
   93          {
   94              PlayerStatus.In => ResourceHelper.GetBrush("SuccessGreenBrush", WhiteBrush),
   95              PlayerStatus.Out => ResourceHelper.GetBrush("HotRedBrush", WhiteBrush),
   96              PlayerStatus.Pending => ResourceHelper.GetBrush("NeonAmberBrush", WhiteBrush),
   97              _ => WhiteBrush
   98          };
   99      }
  100  
  101      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  102      {
  103          throw new NotImplementedException();
  104      }
  105  }
  106  
  107  public class DutyTypeToColorConverter : IValueConverter
  108  {
  109      public object? Convert(object? value, Type targetType, object? parameter, string language)
  110      {
  111          return value switch
  112          {
  113              DutyType.Ice => ResourceHelper.GetColor("IceBlueColor", Colors.White),
  114              DutyType.Beer => ResourceHelper.GetColor("NeonAmberColor", Colors.White),
  115              DutyType.Cooler => ResourceHelper.GetColor("PurpleAccentColor", Colors.White),
  116              DutyType.Food => ResourceHelper.GetColor("SuccessGreenColor", Colors.White),
  117              _ => Colors.White
  118          };
  119      }
  120  
  121      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  122      {
  123          throw new NotImplementedException();
  124      }
  125  }
  126  
  127  public class DutyTypeToIconConverter : IValueConverter
  128  {
  129      public object? Convert(object? value, Type targetType, object? parameter, string language)
  130      {
  131          return value switch
  132          {
  133              DutyType.Ice => "\uE9C4",      // Snowflake icon
  134              DutyType.Beer => "\uE799",      // Coffee cup/drink icon
  135              DutyType.Cooler => "\uE74C",    // Shop/box icon
  136              DutyType.Food => "\uE7E6",      // Emoji/food icon
  137              _ => ""
  138          };
  139      }
  140  
  141      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  142      {
  143          throw new NotImplementedException();
  144      }
  145  }
  146  
  147  public class BoolToConsumedBackgroundConverter : IValueConverter
  148  {
  149      public object? Convert(object? value, Type targetType, object? parameter, string language)
  150      {
  151          var fallback = ResourceHelper.GetBrush("FallbackDarkBrush", new SolidColorBrush(Colors.DarkGray));
  152          if (value is bool isConsumed && isConsumed)
  153          {
  154              return ResourceHelper.GetBrush("NeonAmberBrush", fallback);
  155          }
  156          return ResourceHelper.GetBrush("BoardsMidBrush", fallback);
  157      }
  158  
  159      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  160      {
  161          throw new NotImplementedException();
  162      }
  163  }
  164  
  165  public class BoolToConsumedBorderConverter : IValueConverter
  166  {
  167      public object? Convert(object? value, Type targetType, object? parameter, string language)
  168      {
  169          var fallback = ResourceHelper.GetBrush("FallbackSubtleBorderBrush", new SolidColorBrush(Colors.Gray));
  170          if (value is bool isConsumed && isConsumed)
  171          {
  172              return ResourceHelper.GetBrush("NeonAmberSemiBorderBrush", fallback);
  173          }
  174          return ResourceHelper.GetBrush("SubtleWhiteBorderBrush", fallback);
  175      }
  176  
  177      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  178      {
  179          throw new NotImplementedException();
  180      }
  181  }
  182  
  183  public class BoolToPowderBlueBorderConverter : IValueConverter
  184  {
  185      public object? Convert(object? value, Type targetType, object? parameter, string language)
  186      {
  187          var fallback = ResourceHelper.GetBrush("FallbackSubtleBorderBrush", new SolidColorBrush(Colors.Gray));
  188          if (value is bool isPenguins && isPenguins)
  189          {
  190              return ResourceHelper.GetBrush("PowderBlueSemiBorderBrush", fallback);
  191          }
  192          return ResourceHelper.GetBrush("SubtleWhiteBorderBrush", fallback);
  193      }
  194  
  195      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  196      {
  197          throw new NotImplementedException();
  198      }
  199  }
  200  
  201  public class GameNightEmojiConverter : IValueConverter
  202  {
  203      public object? Convert(object? value, Type targetType, object? parameter, string language)
  204      {
  205          if (value is bool isGameToday && isGameToday)
  206          {
  207              return "\U0001F6A8"; // 🚨 Red rotating light
  208          }
  209          return "\U000026A1"; // ⚡ Lightning bolt
  210      }
  211  
  212      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  213      {
  214          throw new NotImplementedException();
  215      }
  216  }
  217  
  218  public class InverseBoolConverter : IValueConverter
  219  {
  220      public object? Convert(object? value, Type targetType, object? parameter, string language)
  221      {
  222          if (value is bool b)
  223          {
  224              return !b;
  225          }
  226          return true;
  227      }
  228  
  229      public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
  230      {
  231          throw new NotImplementedException();
  232      }
  233  }
```

### `Pens/Pens/App.xaml`

```xml
    1  <Application x:Class="Pens.App"
    2         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4         xmlns:utum="using:Uno.Toolkit.UI.Material"
    5         xmlns:converters="using:Pens.Converters">
    6  
    7    <Application.Resources>
    8      <ResourceDictionary>
    9        <ResourceDictionary.MergedDictionaries>
   10          <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
   11          <utum:MaterialToolkitTheme
   12            ColorOverrideSource="ms-appx:///Styles/ColorPaletteOverride.xaml" />
   13        </ResourceDictionary.MergedDictionaries>
   14  
   15        <!-- Converters -->
   16        <converters:ToUpperConverter x:Key="ToUpperConverter" />
   17        <converters:StatusToTextConverter x:Key="StatusToTextConverter" />
   18        <converters:StatusToBackgroundConverter x:Key="StatusToBackgroundConverter" />
   19        <converters:StatusToForegroundConverter x:Key="StatusToForegroundConverter" />
   20        <converters:DutyTypeToColorConverter x:Key="DutyTypeToColorConverter" />
   21        <converters:DutyTypeToIconConverter x:Key="DutyTypeToIconConverter" />
   22        <converters:BoolToConsumedBackgroundConverter x:Key="BoolToConsumedBackgroundConverter" />
   23        <converters:BoolToConsumedBorderConverter x:Key="BoolToConsumedBorderConverter" />
   24        <converters:BoolToPowderBlueBorderConverter x:Key="BoolToPowderBlueBorderConverter" />
   25        <converters:GameNightEmojiConverter x:Key="GameNightEmojiConverter" />
   26        <converters:InverseBoolConverter x:Key="InverseBoolConverter" />
   27  
   28        <!-- Hockey Theme Colors -->
   29        <Color x:Key="NeonAmberColor">#FFAA00</Color>
   30        <Color x:Key="NeonAmberGlowColor">#99FFAA00</Color>
   31        <Color x:Key="PowderBlueColor">#B4D7E8</Color>
   32        <Color x:Key="PowderBlueDeepColor">#7EB8D4</Color>
   33        <Color x:Key="PowderBlueGlowColor">#66B4D7E8</Color>
   34        <Color x:Key="IceBlueColor">#00D4FF</Color>
   35        <Color x:Key="IceBlueGlowColor">#8000D4FF</Color>
   36        <Color x:Key="HotRedColor">#FF3B3B</Color>
   37        <Color x:Key="ArenaDarkColor">#0A0C10</Color>
   38        <Color x:Key="BoardsDarkColor">#12161D</Color>
   39        <Color x:Key="BoardsMidColor">#1A1F2A</Color>
   40        <Color x:Key="TextPrimaryColor">#F0F4F8</Color>
   41        <Color x:Key="TextMutedColor">#6B7A8F</Color>
   42        <Color x:Key="SuccessGreenColor">#10B981</Color>
   43        <Color x:Key="PurpleAccentColor">#8B5CF6</Color>
   44  
   45        <!-- Brushes -->
   46        <SolidColorBrush x:Key="NeonAmberBrush" Color="{StaticResource NeonAmberColor}" />
   47        <SolidColorBrush x:Key="NeonAmberGlowBrush" Color="{StaticResource NeonAmberGlowColor}" />
   48        <SolidColorBrush x:Key="PowderBlueBrush" Color="{StaticResource PowderBlueColor}" />
   49        <SolidColorBrush x:Key="PowderBlueDeepBrush" Color="{StaticResource PowderBlueDeepColor}" />
   50        <SolidColorBrush x:Key="PowderBlueGlowBrush" Color="{StaticResource PowderBlueGlowColor}" />
   51        <SolidColorBrush x:Key="IceBlueBrush" Color="{StaticResource IceBlueColor}" />
   52        <SolidColorBrush x:Key="IceBlueGlowBrush" Color="{StaticResource IceBlueGlowColor}" />
   53        <SolidColorBrush x:Key="HotRedBrush" Color="{StaticResource HotRedColor}" />
   54        <SolidColorBrush x:Key="ArenaDarkBrush" Color="{StaticResource ArenaDarkColor}" />
   55        <SolidColorBrush x:Key="BoardsDarkBrush" Color="{StaticResource BoardsDarkColor}" />
   56        <SolidColorBrush x:Key="BoardsMidBrush" Color="{StaticResource BoardsMidColor}" />
   57        <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}" />
   58        <SolidColorBrush x:Key="TextMutedBrush" Color="{StaticResource TextMutedColor}" />
   59        <SolidColorBrush x:Key="SuccessGreenBrush" Color="{StaticResource SuccessGreenColor}" />
   60        <SolidColorBrush x:Key="PurpleAccentBrush" Color="{StaticResource PurpleAccentColor}" />
   61  
   62        <!-- Subtle background brushes -->
   63        <SolidColorBrush x:Key="SuccessGreenSubtleBrush" Color="#1A10B981" />
   64        <SolidColorBrush x:Key="HotRedSubtleBrush" Color="#1AFF3B3B" />
   65        <SolidColorBrush x:Key="NeonAmberSubtleBrush" Color="#1AFFAA00" />
   66        <SolidColorBrush x:Key="PowderBlueSubtleBrush" Color="#26B4D7E8" />
   67        <SolidColorBrush x:Key="IceBlueSubtleBrush" Color="#2600D4FF" />
   68        <SolidColorBrush x:Key="PurpleAccentSubtleBrush" Color="#268B5CF6" />
   69  
   70        <!-- Border brushes -->
   71        <SolidColorBrush x:Key="SubtleBorderBrush" Color="#0DFFFFFF" />
   72        <SolidColorBrush x:Key="CardBorderBrush" Color="#08FFFFFF" />
   73        <SolidColorBrush x:Key="PowderBlueBorderBrush" Color="#4DB4D7E8" />
   74        <SolidColorBrush x:Key="SuccessGreenBorderBrush" Color="#4D10B981" />
   75        <SolidColorBrush x:Key="HotRedBorderBrush" Color="#4DFF3B3B" />
   76        <SolidColorBrush x:Key="NeonAmberBorderBrush" Color="#4DFFAA00" />
   77  
   78        <!-- Status background brushes (for converters) -->
   79        <SolidColorBrush x:Key="StatusInBackgroundBrush" Color="#3310B981" />
   80        <SolidColorBrush x:Key="StatusOutBackgroundBrush" Color="#33FF3B3B" />
   81        <SolidColorBrush x:Key="StatusPendingBackgroundBrush" Color="#33FFAA00" />
   82  
   83        <!-- Semi-transparent border brushes (for converters) -->
   84        <SolidColorBrush x:Key="NeonAmberSemiBorderBrush" Color="#80FFAA00" />
   85        <SolidColorBrush x:Key="PowderBlueSemiBorderBrush" Color="#80B4D7E8" />
   86        <SolidColorBrush x:Key="SubtleWhiteBorderBrush" Color="#1AFFFFFF" />
   87  
   88        <!-- Fallback brushes (for converters when resources unavailable) -->
   89        <SolidColorBrush x:Key="FallbackDarkBrush" Color="#1A1F2A" />
   90        <SolidColorBrush x:Key="FallbackSubtleBorderBrush" Color="#1AFFFFFF" />
   91  
   92        <!-- Gradient brushes -->
   93        <LinearGradientBrush x:Key="PowderBlueGradientBrush" StartPoint="0,0" EndPoint="1,1">
   94          <GradientStop Color="{StaticResource PowderBlueColor}" Offset="0" />
   95          <GradientStop Color="{StaticResource PowderBlueDeepColor}" Offset="1" />
   96        </LinearGradientBrush>
   97  
   98        <LinearGradientBrush x:Key="NeonAmberGradientBrush" StartPoint="0,0" EndPoint="1,1">
   99          <GradientStop Color="{StaticResource NeonAmberColor}" Offset="0" />
  100          <GradientStop Color="#CC8800" Offset="1" />
  101        </LinearGradientBrush>
  102  
  103        <LinearGradientBrush x:Key="CardBackgroundGradientBrush" StartPoint="0,0" EndPoint="1,1">
  104          <GradientStop Color="{StaticResource BoardsMidColor}" Offset="0" />
  105          <GradientStop Color="{StaticResource BoardsDarkColor}" Offset="1" />
  106        </LinearGradientBrush>
  107  
  108        <LinearGradientBrush x:Key="FeatureCardTopBarBrush" StartPoint="0,0.5" EndPoint="1,0.5">
  109          <GradientStop Color="{StaticResource PowderBlueColor}" Offset="0" />
  110          <GradientStop Color="{StaticResource NeonAmberColor}" Offset="0.5" />
  111          <GradientStop Color="{StaticResource PowderBlueColor}" Offset="1" />
  112        </LinearGradientBrush>
  113  
  114        <!-- Fonts -->
  115        <FontFamily x:Key="BebasNeueFont">ms-appx:///Assets/Fonts/BebasNeue-Regular.ttf#Bebas Neue</FontFamily>
  116        <FontFamily x:Key="BarlowRegular">ms-appx:///Assets/Fonts/Barlow-Regular.ttf#Barlow</FontFamily>
  117        <FontFamily x:Key="BarlowMedium">ms-appx:///Assets/Fonts/Barlow-Medium.ttf#Barlow</FontFamily>
  118        <FontFamily x:Key="BarlowSemiBold">ms-appx:///Assets/Fonts/Barlow-SemiBold.ttf#Barlow</FontFamily>
  119        <FontFamily x:Key="BarlowBold">ms-appx:///Assets/Fonts/Barlow-Bold.ttf#Barlow</FontFamily>
  120  
  121        <!-- Corner Radius -->
  122        <CornerRadius x:Key="SmallCornerRadius">4</CornerRadius>
  123        <CornerRadius x:Key="MediumCornerRadius">8</CornerRadius>
  124        <CornerRadius x:Key="CardCornerRadius">12</CornerRadius>
  125        <CornerRadius x:Key="LargeCornerRadius">16</CornerRadius>
  126        <CornerRadius x:Key="PillCornerRadius">20</CornerRadius>
  127        <CornerRadius x:Key="FeatureCornerRadius">24</CornerRadius>
  128  
  129        <!-- Thickness -->
  130        <Thickness x:Key="PagePadding">20</Thickness>
  131        <Thickness x:Key="CardPadding">16</Thickness>
  132        <Thickness x:Key="LargeCardPadding">20,24</Thickness>
  133  
  134      </ResourceDictionary>
  135    </Application.Resources>
  136  
  137  </Application>
```

### `Pens/Pens/Presentation/BeersPage.xaml.cs`

```csharp
    1  namespace Pens.Presentation;
    2  
    3  public sealed partial class BeersPage : Page
    4  {
    5      public BeersPage()
    6      {
    7          this.InitializeComponent();
    8      }
    9  }
```

### `Pens/Pens/App.xaml.cs`

```csharp
    1  using Pens.Services;
    2  using Uno.Resizetizer;
    3  
    4  namespace Pens;
    5  
    6  public partial class App : Application
    7  {
    8      public App()
    9      {
   10          this.InitializeComponent();
   11      }
   12  
   13      protected Window? MainWindow { get; private set; }
   14      public IHost? Host { get; private set; }
   15  
   16      protected override void OnLaunched(LaunchActivatedEventArgs args)
   17      {
   18          var appBuilder = this.CreateBuilder(args)
   19              .Configure(host => host
   20                  .UseConfiguration(configure: configBuilder =>
   21                      configBuilder
   22                          .EmbeddedSource<App>()
   23                          .Section<AppConfig>()
   24                  )
   25                  .ConfigureServices((context, services) =>
   26                  {
   27                      // Register services (swap to SupabaseService for production)
   28                      services.AddSingleton<ISupabaseService, MockSupabaseService>();
   29                      services.AddSingleton<IPlayerIdentityService, PlayerIdentityService>();
   30  
   31                      // Register ViewModels
   32                      services.AddTransient<ScheduleViewModel>();
   33                      services.AddTransient<ChatViewModel>();
   34                      services.AddTransient<BeersViewModel>();
   35                      services.AddTransient<DutiesViewModel>();
   36                      services.AddTransient<RosterViewModel>();
   37                  }));
   38  
   39          MainWindow = appBuilder.Window;
   40          Host = appBuilder.Build();
   41  
   42          MainWindow.SetWindowIcon();
   43          ShowAppContent();
   44          MainWindow.Activate();
   45      }
   46  
   47      private void ShowAppContent()
   48      {
   49          var identity = Host!.Services.GetRequiredService<IPlayerIdentityService>();
   50  
   51          if (identity.IsLoggedIn)
   52          {
   53              MainWindow!.Content = new Presentation.Shell(Host.Services);
   54          }
   55          else
   56          {
   57              ShowPlayerPicker();
   58          }
   59      }
   60  
   61      private void ShowPlayerPicker()
   62      {
   63          var supabase = Host!.Services.GetRequiredService<ISupabaseService>();
   64          var identity = Host.Services.GetRequiredService<IPlayerIdentityService>();
   65  
   66          var viewModel = new Presentation.PlayerPickerViewModel(supabase, identity, () =>
   67          {
   68              // After player is selected, show the main shell
   69              MainWindow!.Content = new Presentation.Shell(Host.Services);
   70          });
   71  
   72          MainWindow!.Content = new Presentation.PlayerPickerPage(viewModel);
   73      }
   74  }
   75  
   76  public class AppConfig
   77  {
   78      public SupabaseConfig? Supabase { get; set; }
   79  }
   80  
   81  public class SupabaseConfig
   82  {
   83      public string? Url { get; set; }
   84      public string? AnonKey { get; set; }
   85  }
```
