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
of them, while the page it models is built from 28 nested layout
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

# The answer key under review: `05-orbital-settings`

65 nodes · 96 edges · 3 unresolved items

## What this screen is (the eval's own description)

# Fixture: Orbital Settings (source-backed)

This is the first **real** eval in the kit. Unlike evals 01–04 (synthetic
fixture descriptions), the input here is an existing Uno Platform screen with
full source: XAML, code-behind, and style dictionaries. That makes most facts
`observed`/`declared`/`derived` rather than `inferred`, and it lets the graph
legitimately use `triggers` and a source-backed transient state — things a
screenshot alone could never support.

## Source

- `Orbital/Orbital/Presentation/SettingsPage.xaml` — layout & controls
- `Orbital/Orbital/Presentation/SettingsPage.xaml.cs` — behavior (button wiring, entrance animation, transient "Saved!" state, "Cleared" dialog)
- `Orbital/Orbital/Controls/PageHeader.xaml` + `.xaml.cs` — the reusable page header (title/subtitle **and a search / command-palette affordance** with a Ctrl+K hint; raises a `SearchRequested` event whose handler is outside this source set)
- `Orbital/Orbital/Styles/Surfaces.xaml` — `OrbitalCardStyle` (radius 12, padding 20, border 1, Surface1 bg, Surface3 border)
- `Orbital/Orbital/Styles/Buttons.xaml` — `OrbitalPrimaryButtonSm` / `OrbitalGhostButtonSm` (radius 8, font 13, padding 12,6)
- `Orbital/Orbital/Styles/TextBlock.xaml` — mono/display type styles (`OrbitalSectionHeader`, `OrbitalMonoSmall`, `OrbitalBody`)
- `Orbital/Orbital/Styles/OrbitalBrushes.xaml` — surface/text color tokens

## Visible / declared structure

- **Page header** (reusable `PageHeader` control) — Title `Settings`, Subtitle `Profile and preferences`, and a search / command-palette entry (`Search or run command...`, `Ctrl+K` badge) on the right.
- **PROFILE** card — `Display Name` label, a name textbox (placeholder `Enter your name`), a prominent `Save` button, helper text `This name appears in the homepage greeting.`
- **ABOUT** card — Uno logo asset, app name `Orbital`, a version line, then four repeated label/value rows: `Uno Platform SDK`, `.NET Runtime`, `Renderer`, `Platform`.
- **PATHS** card — three repeated label + wrapping-value fields: `Project Root`, `Recent Projects Database`, `Claude Code Skills`.
- **ACTIONS** card — three repeated ghost buttons (icon + text): `Clear Recent Projects`, `Open Data Folder`, `Uno Platform Documentation`.

All four section containers share `OrbitalCardStyle` (a reusable card).

## Source-backed behavior (from code-behind)

- Every section fades/translates in on load (`AnimationHelper.FadeUp`) → an **entrance** presentation state.
- `Save` persists the name via `SettingsService.SaveUsername(...)` and shows a transient **`Saved!`** confirmation for 1.5s → a `triggers` edge into a source-backed transient state.
- `Open Data Folder`, `Clear Recent Projects`, `Uno Platform Documentation` run utility side-effects (open folder, clear list + dialog, launch external URL).

## What this eval tests

- hierarchy on a real, non-trivial screen (header + 4 cards);
- **consolidation**: four cards → one `settings-card` component; repeated rows/fields/buttons → canonical components + `instance-of`;
- **token extraction from declared styles** (color / spacing / radius / typography) rather than guessed pixels;
- **state modeling** for an entrance state and a transient "saved" state;
- **source-backed behavior** via `triggers` (contrast with eval 04, where behavior must stay `unresolved`);
- **uncertainty discipline**: genuine modeling ambiguities recorded in `unresolved` (are info-rows and path-fields the same key/value component? is an external-URL launch a `navigates-to`?).


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
  "graphId": "eval.orbital-settings",
  "name": "Orbital Settings",
  "description": "Gold graph for the Orbital SettingsPage (source-backed: XAML + code-behind + styles). v1.4: consensus-calibrated token wiring (blind-v4) + properties.uno mapping layer.",
  "sourceSummary": [
    {
      "type": "xaml",
      "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
    },
    {
      "type": "csharp",
      "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
    },
    {
      "type": "xaml",
      "path": "Orbital/Orbital/Controls/PageHeader.xaml"
    },
    {
      "type": "csharp",
      "path": "Orbital/Orbital/Controls/PageHeader.xaml.cs"
    },
    {
      "type": "design-system",
      "label": "Orbital Styles/*.xaml"
    }
  ],
  "nodes": [
    {
      "id": "screen.settings",
      "type": "screen",
      "name": "Settings",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "Page",
          "class": "Orbital.Presentation.SettingsPage"
        }
      }
    },
    {
      "id": "region.header-band",
      "type": "region",
      "name": "Header band",
      "semanticRole": "pageHeaderBand",
      "properties": {
        "uno": {
          "type": "Grid",
          "property": "RowDefinition Height=Auto"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Root grid row 0: fixed, non-scrolling header row holding the PageHeader control."
      }
    },
    {
      "id": "region.settings-content",
      "type": "region",
      "name": "Settings content",
      "semanticRole": "scrollingContent",
      "properties": {
        "uno": {
          "type": "ScrollViewer"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Root grid row 1: ScrollViewer owning scrolling for the settings cards."
      }
    },
    {
      "id": "region.settings-columns",
      "type": "region",
      "name": "Two-column composition",
      "semanticRole": "columnLayout",
      "properties": {
        "uno": {
          "type": "Grid",
          "property": "ColumnDefinitions * / 16 / *"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Declared two-column grid with a 16px gutter, below the profile card."
      }
    },
    {
      "id": "region.about-column",
      "type": "region",
      "name": "About column",
      "semanticRole": "leftColumn",
      "properties": {
        "uno": {
          "type": "AutoLayout",
          "property": "Grid.Column=0"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Left column, grouping ABOUT."
      }
    },
    {
      "id": "region.configuration-column",
      "type": "region",
      "name": "Configuration column",
      "semanticRole": "rightColumn",
      "properties": {
        "uno": {
          "type": "AutoLayout",
          "property": "Grid.Column=2"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Right column, grouping PATHS then ACTIONS."
      }
    },
    {
      "id": "component.page-header",
      "type": "component",
      "name": "Page header",
      "role": "pageHeader",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "controls:PageHeader is a reusable UserControl (Orbital.Controls) instantiated with Title/Subtitle."
      },
      "properties": {
        "uno": {
          "type": "UserControl",
          "class": "Orbital.Controls.PageHeader"
        }
      }
    },
    {
      "id": "control.header.search",
      "type": "control",
      "name": "Search / command palette",
      "role": "button",
      "semanticRole": "searchTrigger",
      "properties": {
        "placeholder": "Search or run command...",
        "shortcutHint": "Ctrl+K",
        "uno": {
          "type": "Border",
          "xName": "SearchBorder"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Controls/PageHeader.xaml"
        },
        "rationale": "SearchBorder: tappable search affordance with Ctrl+K badge."
      }
    },
    {
      "id": "content.settings.title",
      "type": "content",
      "name": "Title",
      "text": "Settings",
      "semanticRole": "pageTitle",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalPageTitle"
        }
      }
    },
    {
      "id": "content.settings.subtitle",
      "type": "content",
      "name": "Subtitle",
      "text": "Profile and preferences",
      "semanticRole": "pageSubtitle",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalBody"
        }
      }
    },
    {
      "id": "component.settings-card",
      "type": "component",
      "name": "Settings section card",
      "role": "card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "All four section containers use OrbitalCardStyle (radius 12, padding 20, Surface1 bg, Surface3 border)."
      },
      "properties": {
        "uno": {
          "type": "Border",
          "styleKey": "OrbitalCardStyle"
        }
      }
    },
    {
      "id": "component.settings-card.profile",
      "type": "component",
      "name": "Profile section",
      "semanticRole": "profileSection",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=ProfileSection."
      },
      "properties": {
        "uno": {
          "xName": "ProfileSection"
        }
      }
    },
    {
      "id": "component.settings-card.about",
      "type": "component",
      "name": "About section",
      "semanticRole": "aboutSection",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=AboutSection."
      },
      "properties": {
        "uno": {
          "xName": "AboutSection"
        }
      }
    },
    {
      "id": "component.settings-card.paths",
      "type": "component",
      "name": "Paths section",
      "semanticRole": "pathsSection",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=PathsSection."
      },
      "properties": {
        "uno": {
          "xName": "PathsSection"
        }
      }
    },
    {
      "id": "component.settings-card.actions",
      "type": "component",
      "name": "Actions section",
      "semanticRole": "actionsSection",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=ActionsSection."
      },
      "properties": {
        "uno": {
          "xName": "ActionsSection"
        }
      }
    },
    {
      "id": "content.profile.section-title",
      "type": "content",
      "name": "Profile header",
      "text": "PROFILE",
      "semanticRole": "sectionHeader",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalSectionHeader"
        }
      }
    },
    {
      "id": "content.profile.name-label",
      "type": "content",
      "name": "Display Name label",
      "text": "Display Name",
      "semanticRole": "fieldLabel",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalMonoSmall"
        }
      }
    },
    {
      "id": "control.profile.username",
      "type": "control",
      "name": "Display Name",
      "role": "textbox",
      "semanticRole": "nameInput",
      "properties": {
        "placeholder": "Enter your name",
        "uno": {
          "type": "TextBox",
          "xName": "UsernameBox"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=UsernameBox."
      }
    },
    {
      "id": "control.profile.save",
      "type": "control",
      "name": "Save",
      "role": "button",
      "text": "Save",
      "semanticRole": "primaryAction",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=SaveUsernameButton, OrbitalPrimaryButtonSm (only high-emphasis action)."
      },
      "properties": {
        "uno": {
          "type": "Button",
          "styleKey": "OrbitalPrimaryButtonSm",
          "xName": "SaveUsernameButton"
        }
      }
    },
    {
      "id": "content.profile.name-helper",
      "type": "content",
      "name": "Name helper",
      "text": "This name appears in the homepage greeting.",
      "semanticRole": "helperText",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalMonoSmall"
        }
      }
    },
    {
      "id": "content.about.section-title",
      "type": "content",
      "name": "About header",
      "text": "ABOUT",
      "semanticRole": "sectionHeader",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalSectionHeader"
        }
      }
    },
    {
      "id": "asset.about.logo",
      "type": "asset",
      "name": "Orbital logo",
      "role": "image",
      "properties": {
        "source": "ms-appx:///Assets/Icons/Uno-logo.png",
        "uno": {
          "type": "Image",
          "source": "ms-appx:///Assets/Icons/Uno-logo.png"
        }
      },
      "semanticRole": "appLogo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "content.about.app-name",
      "type": "content",
      "name": "App name",
      "text": "Orbital",
      "semanticRole": "appName",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalBody"
        }
      }
    },
    {
      "id": "content.about.version",
      "type": "content",
      "name": "Version",
      "semanticRole": "versionLabel",
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalMonoSmall"
        },
        "fallbackValue": "v0.1.0-alpha"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Bound VersionDisplay, FallbackValue v0.1.0-alpha."
      }
    },
    {
      "id": "component.info-row",
      "type": "component",
      "name": "Info row (label/value)",
      "role": "keyValueRow",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Four label/value grids with identical structure inside ABOUT."
      },
      "properties": {
        "uno": {
          "type": "Grid"
        }
      }
    },
    {
      "id": "component.info-row.uno-sdk",
      "type": "component",
      "name": "Uno Platform SDK",
      "properties": {
        "value": "{Binding EnvStatus.UnoSdkVersion}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "component.info-row.dotnet",
      "type": "component",
      "name": ".NET Runtime",
      "properties": {
        "value": "{Binding DotNetDisplay}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "component.info-row.renderer",
      "type": "component",
      "name": "Renderer",
      "properties": {
        "value": "{Binding EnvStatus.Renderer}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "component.info-row.platform",
      "type": "component",
      "name": "Platform",
      "properties": {
        "value": "{Binding PlatformInfo}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "content.paths.section-title",
      "type": "content",
      "name": "Paths header",
      "text": "PATHS",
      "semanticRole": "sectionHeader",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalSectionHeader"
        }
      }
    },
    {
      "id": "component.path-field",
      "type": "component",
      "name": "Path field (label/value)",
      "role": "keyValueField",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Three label + wrapping-value stacks with identical structure inside PATHS."
      },
      "properties": {
        "uno": {
          "type": "StackPanel"
        }
      }
    },
    {
      "id": "component.path-field.project-root",
      "type": "component",
      "name": "Project Root",
      "properties": {
        "value": "{Binding ProjectRoot}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "component.path-field.recents-db",
      "type": "component",
      "name": "Recent Projects Database",
      "properties": {
        "value": "{Binding RecentsPath}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "component.path-field.skills",
      "type": "component",
      "name": "Claude Code Skills",
      "properties": {
        "value": "{Binding SkillsPath}"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "id": "content.actions.section-title",
      "type": "content",
      "name": "Actions header",
      "text": "ACTIONS",
      "semanticRole": "sectionHeader",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      },
      "properties": {
        "uno": {
          "type": "TextBlock",
          "styleKey": "OrbitalSectionHeader"
        }
      }
    },
    {
      "id": "component.action-button",
      "type": "component",
      "name": "Ghost action button (icon+text)",
      "role": "button",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Three OrbitalGhostButtonSm buttons, each an icon glyph + label, stretched."
      },
      "properties": {
        "uno": {
          "type": "Button",
          "styleKey": "OrbitalGhostButtonSm"
        }
      }
    },
    {
      "id": "control.actions.clear-recents",
      "type": "control",
      "name": "Clear Recent Projects",
      "role": "button",
      "text": "Clear Recent Projects",
      "semanticRole": "utilityAction",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=ClearRecentsButton."
      },
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "ClearRecentsButton",
          "iconGlyph": "E74D"
        }
      }
    },
    {
      "id": "control.actions.open-data-folder",
      "type": "control",
      "name": "Open Data Folder",
      "role": "button",
      "text": "Open Data Folder",
      "semanticRole": "utilityAction",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "x:Name=OpenDataFolderButton."
      },
      "properties": {
        "uno": {
          "type": "Button",
          "xName": "OpenDataFolderButton",
          "iconGlyph": "E838"
        }
      }
    },
    {
      "id": "control.actions.open-docs",
      "type": "control",
      "name": "Uno Platform Documentation",
      "role": "button",
      "text": "Uno Platform Documentation",
      "semanticRole": "externalLink",
      "properties": {
        "uri": "https://platform.uno/docs/articles/intro.html",
        "uno": {
          "type": "Button",
          "xName": "OpenDocsButton",
          "iconGlyph": "E8A5"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "OpenDocsButton launches the docs URI (LaunchUriAsync)."
      }
    },
    {
      "id": "component.dialog.recents-cleared",
      "type": "component",
      "name": "Cleared dialog",
      "role": "dialog",
      "semanticRole": "confirmationDialog",
      "properties": {
        "title": "Cleared",
        "body": "Recent projects list has been cleared.",
        "closeText": "OK",
        "uno": {
          "type": "ContentDialog"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "ContentDialog shown after clearing recent projects."
      }
    },
    {
      "id": "state.profile.entering",
      "type": "state",
      "name": "Entering",
      "semanticRole": "entering",
      "properties": {
        "delayMs": 0,
        "uno": {
          "mechanism": "AnimationHelper.FadeUp",
          "member": "OnLoaded"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "AnimationHelper.FadeUp(ProfileSection, 0) on Loaded."
      }
    },
    {
      "id": "state.about.entering",
      "type": "state",
      "name": "Entering",
      "semanticRole": "entering",
      "properties": {
        "delayMs": 100,
        "uno": {
          "mechanism": "AnimationHelper.FadeUp",
          "member": "OnLoaded"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "AnimationHelper.FadeUp(AboutSection, 100) on Loaded."
      }
    },
    {
      "id": "state.paths.entering",
      "type": "state",
      "name": "Entering",
      "semanticRole": "entering",
      "properties": {
        "delayMs": 200,
        "uno": {
          "mechanism": "AnimationHelper.FadeUp",
          "member": "OnLoaded"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "AnimationHelper.FadeUp(PathsSection, 200) on Loaded."
      }
    },
    {
      "id": "state.actions.entering",
      "type": "state",
      "name": "Entering",
      "semanticRole": "entering",
      "properties": {
        "delayMs": 300,
        "uno": {
          "mechanism": "AnimationHelper.FadeUp",
          "member": "OnLoaded"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "AnimationHelper.FadeUp(ActionsSection, 300) on Loaded."
      }
    },
    {
      "id": "state.profile.saved",
      "type": "state",
      "name": "Saved",
      "semanticRole": "confirmation",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "Save sets button content to 'Saved!' for 1.5s after SettingsService.SaveUsername."
      },
      "properties": {
        "uno": {
          "mechanism": "code-behind"
        }
      }
    },
    {
      "id": "token.color.surface0",
      "type": "token",
      "name": "Surface 0 (page bg)",
      "category": "color",
      "value": "#FF0A0A0B",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalSurface0Brush; page Background."
      },
      "properties": {
        "uno": {
          "resourceKey": "OrbitalSurface0Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.surface1",
      "type": "token",
      "name": "Surface 1 (card bg)",
      "category": "color",
      "value": "#FF141416",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalSurface1Brush; OrbitalCardStyle Background."
      },
      "properties": {
        "uno": {
          "resourceKey": "OrbitalSurface1Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.color.surface3",
      "type": "token",
      "name": "Surface 3 (card border)",
      "category": "color",
      "value": "#FF212123",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalSurface3Brush; OrbitalCardStyle BorderBrush."
      },
      "properties": {
        "uno": {
          "resourceKey": "OrbitalSurface3Brush",
          "resourceType": "SolidColorBrush"
        }
      }
    },
    {
      "id": "token.radius.12",
      "type": "token",
      "name": "12 radius",
      "category": "radius",
      "value": 12,
      "properties": {
        "unit": "px",
        "uno": {
          "styleKey": "OrbitalCardStyle",
          "property": "CornerRadius"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalCardStyle CornerRadius."
      }
    },
    {
      "id": "token.radius.8",
      "type": "token",
      "name": "8 radius",
      "category": "radius",
      "value": 8,
      "properties": {
        "unit": "px",
        "uno": {
          "property": "CornerRadius"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Button/TextBox CornerRadius."
      }
    },
    {
      "id": "token.spacing.24",
      "type": "token",
      "name": "24 spacing",
      "category": "spacing",
      "value": 24,
      "properties": {
        "unit": "px",
        "uno": {
          "property": "Spacing"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Root AutoLayout Spacing=24 (section gap)."
      }
    },
    {
      "id": "token.spacing.16",
      "type": "token",
      "name": "16 spacing",
      "category": "spacing",
      "value": 16,
      "properties": {
        "unit": "px",
        "uno": {
          "property": "Spacing"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Card inner AutoLayout Spacing=16."
      }
    },
    {
      "id": "token.spacing.12",
      "type": "token",
      "name": "12 spacing",
      "category": "spacing",
      "value": 12,
      "properties": {
        "unit": "px",
        "uno": {
          "property": "Spacing"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Actions AutoLayout Spacing=12."
      }
    },
    {
      "id": "token.spacing.8",
      "type": "token",
      "name": "8 spacing",
      "category": "spacing",
      "value": 8,
      "properties": {
        "unit": "px",
        "uno": {
          "property": "Spacing"
        }
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        },
        "rationale": "Label/field StackPanel Spacing=8."
      }
    },
    {
      "id": "token.typography.mono-small",
      "type": "token",
      "name": "Mono small",
      "category": "typography",
      "value": {
        "family": "JetBrains Mono",
        "size": 11
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalMonoSmall / OrbitalSectionHeader (mono, 11)."
      },
      "properties": {
        "uno": {
          "styleKey": "OrbitalMonoSmall",
          "fontResourceKey": "OrbitalMonoFont"
        }
      }
    },
    {
      "id": "token.color.text-30",
      "type": "token",
      "name": "Text 30% emphasis",
      "category": "color",
      "value": "#FF4A505D",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText30Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.text-38",
      "type": "token",
      "name": "Text 38% emphasis",
      "category": "color",
      "value": "#FF5D6372",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText38Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.text-40",
      "type": "token",
      "name": "Text 40% emphasis",
      "category": "color",
      "value": "#FF626878",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText40Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.text-50",
      "type": "token",
      "name": "Text 50% emphasis",
      "category": "color",
      "value": "#FF7B8191",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText50Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.text-72",
      "type": "token",
      "name": "Text 72% emphasis",
      "category": "color",
      "value": "#FFB5B9C5",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText72Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.text-85",
      "type": "token",
      "name": "Text 85% emphasis",
      "category": "color",
      "value": "#FFD6D9E1",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalText85Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.color.emerald-500",
      "type": "token",
      "name": "Emerald 500 (primary accent)",
      "category": "color",
      "value": "#FF10B981",
      "properties": {
        "uno": {
          "resourceKey": "OrbitalEmerald500Brush",
          "resourceType": "SolidColorBrush"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Declared brush resource consumed by the resting screen."
      }
    },
    {
      "id": "token.typography.page-title",
      "type": "token",
      "name": "Page title type",
      "category": "typography",
      "value": {
        "family": "Space Grotesk",
        "size": 20,
        "weight": "SemiBold"
      },
      "properties": {
        "uno": {
          "styleKey": "OrbitalPageTitle"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalPageTitle style (display face, 20, SemiBold)."
      }
    },
    {
      "id": "token.typography.section-header",
      "type": "token",
      "name": "Section header type",
      "category": "typography",
      "value": {
        "family": "JetBrains Mono",
        "size": 11,
        "weight": "Medium",
        "letterSpacing": 80
      },
      "properties": {
        "uno": {
          "styleKey": "OrbitalSectionHeader"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalSectionHeader style (mono 11, Medium, tracking 80)."
      }
    },
    {
      "id": "token.typography.body",
      "type": "token",
      "name": "Body type",
      "category": "typography",
      "value": {
        "family": "Space Grotesk",
        "size": 13
      },
      "properties": {
        "uno": {
          "styleKey": "OrbitalBody"
        }
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "OrbitalBody style (display face, 13)."
      }
    }
  ],
  "edges": [
    {
      "from": "screen.settings",
      "relation": "contains",
      "to": "region.header-band",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "screen.settings",
      "relation": "contains",
      "to": "region.settings-content",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "region.header-band",
      "relation": "contains",
      "to": "component.page-header",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "region.settings-content",
      "relation": "contains",
      "to": "region.settings-columns",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "region.settings-columns",
      "relation": "contains",
      "to": "region.about-column",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "region.settings-columns",
      "relation": "contains",
      "to": "region.configuration-column",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.page-header",
      "relation": "contains",
      "to": "content.settings.title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.page-header",
      "relation": "contains",
      "to": "content.settings.subtitle",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.page-header",
      "relation": "contains",
      "to": "control.header.search",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Controls/PageHeader.xaml"
        }
      }
    },
    {
      "from": "region.settings-content",
      "relation": "contains",
      "to": "component.settings-card.profile",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "instance-of",
      "to": "component.settings-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Shares OrbitalCardStyle."
      }
    },
    {
      "from": "region.about-column",
      "relation": "contains",
      "to": "component.settings-card.about",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "instance-of",
      "to": "component.settings-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Shares OrbitalCardStyle."
      }
    },
    {
      "from": "region.configuration-column",
      "relation": "contains",
      "to": "component.settings-card.paths",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "instance-of",
      "to": "component.settings-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Shares OrbitalCardStyle."
      }
    },
    {
      "from": "region.configuration-column",
      "relation": "contains",
      "to": "component.settings-card.actions",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "instance-of",
      "to": "component.settings-card",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Shares OrbitalCardStyle."
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "contains",
      "to": "content.profile.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "contains",
      "to": "content.profile.name-label",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "contains",
      "to": "control.profile.username",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "contains",
      "to": "control.profile.save",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "contains",
      "to": "content.profile.name-helper",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "content.about.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "asset.about.logo",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "content.about.app-name",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "content.about.version",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "component.info-row.uno-sdk",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "component.info-row.dotnet",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "component.info-row.renderer",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "contains",
      "to": "component.info-row.platform",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.info-row.uno-sdk",
      "relation": "instance-of",
      "to": "component.info-row",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.info-row.dotnet",
      "relation": "instance-of",
      "to": "component.info-row",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.info-row.renderer",
      "relation": "instance-of",
      "to": "component.info-row",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.info-row.platform",
      "relation": "instance-of",
      "to": "component.info-row",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "contains",
      "to": "content.paths.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "contains",
      "to": "component.path-field.project-root",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "contains",
      "to": "component.path-field.recents-db",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "contains",
      "to": "component.path-field.skills",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.path-field.project-root",
      "relation": "instance-of",
      "to": "component.path-field",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.path-field.recents-db",
      "relation": "instance-of",
      "to": "component.path-field",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.path-field.skills",
      "relation": "instance-of",
      "to": "component.path-field",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "contains",
      "to": "content.actions.section-title",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "contains",
      "to": "control.actions.clear-recents",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "contains",
      "to": "control.actions.open-data-folder",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "contains",
      "to": "control.actions.open-docs",
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "control.actions.clear-recents",
      "relation": "instance-of",
      "to": "component.action-button",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "control.actions.open-data-folder",
      "relation": "instance-of",
      "to": "component.action-button",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "control.actions.open-docs",
      "relation": "instance-of",
      "to": "component.action-button",
      "evidence": {
        "kind": "derived",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.profile",
      "relation": "has-state",
      "to": "state.profile.entering",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        }
      }
    },
    {
      "from": "component.settings-card.about",
      "relation": "has-state",
      "to": "state.about.entering",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        }
      }
    },
    {
      "from": "component.settings-card.paths",
      "relation": "has-state",
      "to": "state.paths.entering",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "has-state",
      "to": "state.actions.entering",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        }
      }
    },
    {
      "from": "control.profile.save",
      "relation": "has-state",
      "to": "state.profile.saved",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "v0.2 attachment rule: the Save button is the smallest node whose presentation changes."
      }
    },
    {
      "from": "control.profile.save",
      "relation": "triggers",
      "to": "state.profile.saved",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "Save handler swaps content to 'Saved!' after persisting the name."
      }
    },
    {
      "from": "control.actions.clear-recents",
      "relation": "triggers",
      "to": "component.dialog.recents-cleared",
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "csharp",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"
        },
        "rationale": "ClearRecentsButton handler clears recents then shows the Cleared dialog."
      }
    },
    {
      "from": "screen.settings",
      "relation": "uses-token",
      "to": "token.color.surface0",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "screen.settings",
      "relation": "uses-token",
      "to": "token.spacing.24",
      "properties": {
        "appliesTo": "sectionGap"
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card",
      "relation": "uses-token",
      "to": "token.color.surface1",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.settings-card",
      "relation": "uses-token",
      "to": "token.color.surface3",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.settings-card",
      "relation": "uses-token",
      "to": "token.radius.12",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.settings-card",
      "relation": "uses-token",
      "to": "token.spacing.16",
      "properties": {
        "appliesTo": "innerGap"
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "component.settings-card.actions",
      "relation": "uses-token",
      "to": "token.spacing.12",
      "properties": {
        "appliesTo": "innerGap"
      },
      "evidence": {
        "kind": "observed",
        "confidence": 1.0,
        "source": {
          "type": "xaml",
          "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"
        }
      }
    },
    {
      "from": "control.profile.save",
      "relation": "uses-token",
      "to": "token.radius.8",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.action-button",
      "relation": "uses-token",
      "to": "token.radius.8",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.typography.mono-small",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "component.path-field",
      "relation": "uses-token",
      "to": "token.typography.mono-small",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        }
      }
    },
    {
      "from": "content.settings.title",
      "relation": "uses-token",
      "to": "token.typography.page-title",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.settings.subtitle",
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
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.settings.subtitle",
      "relation": "uses-token",
      "to": "token.color.text-40",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.section-title",
      "relation": "uses-token",
      "to": "token.typography.section-header",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.section-title",
      "relation": "uses-token",
      "to": "token.color.text-38",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.section-title",
      "relation": "uses-token",
      "to": "token.typography.section-header",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.section-title",
      "relation": "uses-token",
      "to": "token.color.text-38",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.paths.section-title",
      "relation": "uses-token",
      "to": "token.typography.section-header",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.paths.section-title",
      "relation": "uses-token",
      "to": "token.color.text-38",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.actions.section-title",
      "relation": "uses-token",
      "to": "token.typography.section-header",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.actions.section-title",
      "relation": "uses-token",
      "to": "token.color.text-38",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.name-label",
      "relation": "uses-token",
      "to": "token.typography.mono-small",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.name-label",
      "relation": "uses-token",
      "to": "token.color.text-50",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.color.text-50",
      "properties": {
        "appliesTo": "labelColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "component.info-row",
      "relation": "uses-token",
      "to": "token.color.text-72",
      "properties": {
        "appliesTo": "valueColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "component.path-field",
      "relation": "uses-token",
      "to": "token.color.text-50",
      "properties": {
        "appliesTo": "labelColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "component.path-field",
      "relation": "uses-token",
      "to": "token.color.text-72",
      "properties": {
        "appliesTo": "valueColor"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.name-helper",
      "relation": "uses-token",
      "to": "token.typography.mono-small",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.profile.name-helper",
      "relation": "uses-token",
      "to": "token.color.text-30",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.app-name",
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
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.app-name",
      "relation": "uses-token",
      "to": "token.color.text-85",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.version",
      "relation": "uses-token",
      "to": "token.typography.mono-small",
      "properties": {
        "appliesTo": "font"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "content.about.version",
      "relation": "uses-token",
      "to": "token.color.text-40",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.profile.username",
      "relation": "uses-token",
      "to": "token.color.surface1",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.profile.username",
      "relation": "uses-token",
      "to": "token.color.surface3",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.profile.username",
      "relation": "uses-token",
      "to": "token.color.text-85",
      "properties": {
        "appliesTo": "foreground"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.profile.save",
      "relation": "uses-token",
      "to": "token.color.emerald-500",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.header.search",
      "relation": "uses-token",
      "to": "token.color.surface1",
      "properties": {
        "appliesTo": "background"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.header.search",
      "relation": "uses-token",
      "to": "token.color.surface3",
      "properties": {
        "appliesTo": "borderBrush"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    },
    {
      "from": "control.header.search",
      "relation": "uses-token",
      "to": "token.radius.8",
      "properties": {
        "appliesTo": "cornerRadius"
      },
      "evidence": {
        "kind": "declared",
        "confidence": 1.0,
        "source": {
          "type": "design-system",
          "label": "Orbital Styles/*.xaml"
        },
        "rationale": "Consensus calibration: declared style/brush consumption on the resting screen."
      }
    }
  ],
  "unresolved": [
    {
      "id": "unresolved.header.search-target",
      "question": "What UI does the header search / command palette open?",
      "relatedIds": [
        "control.header.search"
      ],
      "possibleValues": [
        "global command palette",
        "search overlay",
        "unknown"
      ],
      "reason": "PageHeader raises a static SearchRequested event; the handler and resulting UI are outside the supplied source."
    },
    {
      "id": "unresolved.settings.rowfield-consolidation",
      "question": "Are the ABOUT info-rows and the PATHS fields one reusable key/value component or two?",
      "relatedIds": [
        "component.info-row",
        "component.path-field"
      ],
      "possibleValues": [
        "one key-value component",
        "two distinct components"
      ],
      "reason": "Both pair a label with a value, but info-row is a horizontal grid (right-aligned value) while path-field is a vertical stack with a wrapping value. Modeled separately; may be one family."
    },
    {
      "id": "unresolved.settings.external-nav",
      "question": "Should the docs button's external URL launch be modeled as navigates-to?",
      "relatedIds": [
        "control.actions.open-docs"
      ],
      "possibleValues": [
        "navigates-to (external)",
        "triggers (open url)",
        "out of scope for v0.1"
      ],
      "reason": "v0.1 navigates-to targets a known in-app screen; this opens an external browser URL with no screen node."
    }
  ]
}
```

## Source files the gold cites

Line-numbered so you can cite `file:line`.

### `Orbital/Orbital/Presentation/SettingsPage.xaml`

```xml
    1  <Page x:Class="Orbital.Presentation.SettingsPage"
    2        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4        xmlns:utu="using:Uno.Toolkit.UI"
    5        xmlns:controls="using:Orbital.Controls"
    6        Background="{ThemeResource OrbitalSurface0Brush}">
    7  
    8    <Grid>
    9      <Grid.RowDefinitions>
   10        <RowDefinition Height="Auto" />
   11        <RowDefinition Height="*" />
   12      </Grid.RowDefinitions>
   13  
   14      <controls:PageHeader x:Name="Header" Grid.Row="0"
   15                           Title="Settings"
   16                           Subtitle="Profile and preferences" />
   17  
   18      <ScrollViewer Grid.Row="1">
   19        <utu:AutoLayout Spacing="24" Padding="{utu:Responsive Narrow='16', Normal='32', Wide='32'}" PrimaryAxisAlignment="Start">
   20  
   21          <!-- PROFILE SECTION -->
   22          <Border x:Name="ProfileSection" Style="{StaticResource OrbitalCardStyle}" Opacity="0">
   23            <Border.RenderTransform><TranslateTransform Y="8" /></Border.RenderTransform>
   24            <utu:AutoLayout Spacing="16">
   25              <TextBlock Text="PROFILE" Style="{StaticResource OrbitalSectionHeader}"
   26                         Foreground="{ThemeResource OrbitalText38Brush}" />
   27              <StackPanel Spacing="8">
   28                <TextBlock Text="Display Name"
   29                           Style="{StaticResource OrbitalMonoSmall}"
   30                           Foreground="{ThemeResource OrbitalText50Brush}" />
   31                <Grid>
   32                  <Grid.ColumnDefinitions>
   33                    <ColumnDefinition Width="*" />
   34                    <ColumnDefinition Width="8" />
   35                    <ColumnDefinition Width="Auto" />
   36                  </Grid.ColumnDefinitions>
   37                  <TextBox x:Name="UsernameBox"
   38                           FontFamily="{StaticResource OrbitalMonoFont}"
   39                           FontSize="13"
   40                           PlaceholderText="Enter your name"
   41                           Background="{ThemeResource OrbitalSurface1Brush}"
   42                           BorderBrush="{ThemeResource OrbitalSurface3Brush}"
   43                           Foreground="{ThemeResource OrbitalText85Brush}" />
   44                  <Button x:Name="SaveUsernameButton" Grid.Column="2"
   45                          Content="Save"
   46                          Style="{StaticResource OrbitalPrimaryButtonSm}" />
   47                </Grid>
   48                <TextBlock Text="This name appears in the homepage greeting."
   49                           Style="{StaticResource OrbitalMonoSmall}"
   50                           Foreground="{ThemeResource OrbitalText30Brush}" />
   51              </StackPanel>
   52            </utu:AutoLayout>
   53          </Border>
   54  
   55          <!-- TWO-COLUMN LAYOUT -->
   56          <Grid>
   57            <Grid.ColumnDefinitions>
   58              <ColumnDefinition Width="*" />
   59              <ColumnDefinition Width="16" />
   60              <ColumnDefinition Width="*" />
   61            </Grid.ColumnDefinitions>
   62  
   63            <!-- LEFT COLUMN -->
   64            <utu:AutoLayout Grid.Column="0" Spacing="24">
   65  
   66              <!-- ABOUT SECTION -->
   67              <Border x:Name="AboutSection" Style="{StaticResource OrbitalCardStyle}" Opacity="0">
   68                <Border.RenderTransform><TranslateTransform Y="8" /></Border.RenderTransform>
   69                <utu:AutoLayout Spacing="16">
   70                  <TextBlock Text="ABOUT" Style="{StaticResource OrbitalSectionHeader}"
   71                             Foreground="{ThemeResource OrbitalText38Brush}" />
   72  
   73                  <Grid>
   74                    <Grid.ColumnDefinitions>
   75                      <ColumnDefinition Width="Auto" />
   76                      <ColumnDefinition Width="16" />
   77                      <ColumnDefinition Width="*" />
   78                    </Grid.ColumnDefinitions>
   79  
   80                    <!-- Orbital logo -->
   81                    <Border Grid.Column="0" Width="48" Height="48" CornerRadius="12"
   82                            Background="{ThemeResource OrbitalSurface2Brush}">
   83                      <Image Source="ms-appx:///Assets/Icons/Uno-logo.png"
   84                             Width="32" Height="32" Stretch="Uniform"
   85                             HorizontalAlignment="Center" VerticalAlignment="Center" />
   86                    </Border>
   87  
   88                    <StackPanel Grid.Column="2" Spacing="4" VerticalAlignment="Center">
   89                      <TextBlock Text="Orbital" Style="{StaticResource OrbitalBody}"
   90                                 Foreground="{ThemeResource OrbitalText85Brush}"
   91                                 FontWeight="SemiBold" />
   92                      <TextBlock Text="{Binding VersionDisplay, FallbackValue='v0.1.0-alpha'}"
   93                                 Style="{StaticResource OrbitalMonoSmall}"
   94                                 Foreground="{ThemeResource OrbitalText40Brush}" />
   95                    </StackPanel>
   96                  </Grid>
   97  
   98                  <!-- Version details -->
   99                  <StackPanel Spacing="8">
  100                    <Grid>
  101                      <TextBlock Text="Uno Platform SDK"
  102                                 Style="{StaticResource OrbitalMonoSmall}"
  103                                 Foreground="{ThemeResource OrbitalText50Brush}" />
  104                      <TextBlock Text="{Binding EnvStatus.UnoSdkVersion, FallbackValue='...'}"
  105                                 Style="{StaticResource OrbitalMonoSmall}"
  106                                 Foreground="{ThemeResource OrbitalText72Brush}"
  107                                 HorizontalAlignment="Right" />
  108                    </Grid>
  109                    <Grid>
  110                      <TextBlock Text=".NET Runtime"
  111                                 Style="{StaticResource OrbitalMonoSmall}"
  112                                 Foreground="{ThemeResource OrbitalText50Brush}" />
  113                      <TextBlock Text="{Binding DotNetDisplay, FallbackValue='...'}"
  114                                 Style="{StaticResource OrbitalMonoSmall}"
  115                                 Foreground="{ThemeResource OrbitalText72Brush}"
  116                                 HorizontalAlignment="Right" />
  117                    </Grid>
  118                    <Grid>
  119                      <TextBlock Text="Renderer"
  120                                 Style="{StaticResource OrbitalMonoSmall}"
  121                                 Foreground="{ThemeResource OrbitalText50Brush}" />
  122                      <TextBlock Text="{Binding EnvStatus.Renderer, FallbackValue='...'}"
  123                                 Style="{StaticResource OrbitalMonoSmall}"
  124                                 Foreground="{ThemeResource OrbitalText72Brush}"
  125                                 HorizontalAlignment="Right" />
  126                    </Grid>
  127                    <Grid>
  128                      <TextBlock Text="Platform"
  129                                 Style="{StaticResource OrbitalMonoSmall}"
  130                                 Foreground="{ThemeResource OrbitalText50Brush}" />
  131                      <TextBlock Text="{Binding PlatformInfo, FallbackValue='...'}"
  132                                 Style="{StaticResource OrbitalMonoSmall}"
  133                                 Foreground="{ThemeResource OrbitalText72Brush}"
  134                                 HorizontalAlignment="Right" />
  135                    </Grid>
  136                  </StackPanel>
  137                </utu:AutoLayout>
  138              </Border>
  139  
  140            </utu:AutoLayout>
  141  
  142            <!-- RIGHT COLUMN -->
  143            <utu:AutoLayout Grid.Column="2" Spacing="24">
  144  
  145              <!-- PATHS SECTION -->
  146              <Border x:Name="PathsSection" Style="{StaticResource OrbitalCardStyle}" Opacity="0">
  147                <Border.RenderTransform><TranslateTransform Y="8" /></Border.RenderTransform>
  148                <utu:AutoLayout Spacing="16">
  149                  <TextBlock Text="PATHS" Style="{StaticResource OrbitalSectionHeader}"
  150                             Foreground="{ThemeResource OrbitalText38Brush}" />
  151  
  152                  <StackPanel Spacing="8">
  153                    <TextBlock Text="Project Root"
  154                               Style="{StaticResource OrbitalMonoSmall}"
  155                               Foreground="{ThemeResource OrbitalText50Brush}" />
  156                    <TextBlock Text="{Binding ProjectRoot, FallbackValue='...'}"
  157                               Style="{StaticResource OrbitalMonoSmall}"
  158                               Foreground="{ThemeResource OrbitalText72Brush}"
  159                               TextWrapping="Wrap" />
  160                  </StackPanel>
  161                  <StackPanel Spacing="8">
  162                    <TextBlock Text="Recent Projects Database"
  163                               Style="{StaticResource OrbitalMonoSmall}"
  164                               Foreground="{ThemeResource OrbitalText50Brush}" />
  165                    <TextBlock Text="{Binding RecentsPath, FallbackValue='...'}"
  166                               Style="{StaticResource OrbitalMonoSmall}"
  167                               Foreground="{ThemeResource OrbitalText72Brush}"
  168                               TextWrapping="Wrap" />
  169                  </StackPanel>
  170                  <StackPanel Spacing="8">
  171                    <TextBlock Text="Claude Code Skills"
  172                               Style="{StaticResource OrbitalMonoSmall}"
  173                               Foreground="{ThemeResource OrbitalText50Brush}" />
  174                    <TextBlock Text="{Binding SkillsPath, FallbackValue='...'}"
  175                               Style="{StaticResource OrbitalMonoSmall}"
  176                               Foreground="{ThemeResource OrbitalText72Brush}"
  177                               TextWrapping="Wrap" />
  178                  </StackPanel>
  179                </utu:AutoLayout>
  180              </Border>
  181  
  182              <!-- ACTIONS -->
  183              <Border x:Name="ActionsSection" Style="{StaticResource OrbitalCardStyle}" Opacity="0">
  184                <Border.RenderTransform><TranslateTransform Y="8" /></Border.RenderTransform>
  185                <utu:AutoLayout Spacing="12">
  186                  <TextBlock Text="ACTIONS" Style="{StaticResource OrbitalSectionHeader}"
  187                             Foreground="{ThemeResource OrbitalText38Brush}" />
  188  
  189                  <Button x:Name="ClearRecentsButton" Style="{StaticResource OrbitalGhostButtonSm}"
  190                          HorizontalAlignment="Stretch" HorizontalContentAlignment="Left">
  191                    <StackPanel Orientation="Horizontal" Spacing="8">
  192                      <FontIcon Glyph="&#xE74D;" FontSize="14" />
  193                      <TextBlock Text="Clear Recent Projects" />
  194                    </StackPanel>
  195                  </Button>
  196                  <Button x:Name="OpenDataFolderButton" Style="{StaticResource OrbitalGhostButtonSm}"
  197                          HorizontalAlignment="Stretch" HorizontalContentAlignment="Left">
  198                    <StackPanel Orientation="Horizontal" Spacing="8">
  199                      <FontIcon Glyph="&#xE838;" FontSize="14" />
  200                      <TextBlock Text="Open Data Folder" />
  201                    </StackPanel>
  202                  </Button>
  203                  <Button x:Name="OpenDocsButton" Style="{StaticResource OrbitalGhostButtonSm}"
  204                          HorizontalAlignment="Stretch" HorizontalContentAlignment="Left">
  205                    <StackPanel Orientation="Horizontal" Spacing="8">
  206                      <FontIcon Glyph="&#xE8A5;" FontSize="14" />
  207                      <TextBlock Text="Uno Platform Documentation" />
  208                    </StackPanel>
  209                  </Button>
  210                </utu:AutoLayout>
  211              </Border>
  212  
  213            </utu:AutoLayout>
  214          </Grid>
  215  
  216        </utu:AutoLayout>
  217      </ScrollViewer>
  218    </Grid>
  219  </Page>
```

### `Orbital/Orbital/Controls/PageHeader.xaml`

```xml
    1  <UserControl x:Class="Orbital.Controls.PageHeader"
    2               xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    3               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    4               xmlns:utu="using:Uno.Toolkit.UI">
    5  
    6    <Border BorderBrush="{ThemeResource OrbitalSurface2Brush}"
    7            BorderThickness="0,0,0,1"
    8            Padding="32,20">
    9      <Grid>
   10        <Grid.ColumnDefinitions>
   11          <ColumnDefinition Width="*" />
   12          <ColumnDefinition Width="Auto" />
   13        </Grid.ColumnDefinitions>
   14  
   15        <!-- Title + Subtitle -->
   16        <utu:AutoLayout Grid.Column="0"
   17                        Spacing="4"
   18                        PrimaryAxisAlignment="Center">
   19          <TextBlock x:Name="TitleText"
   20                     Style="{StaticResource OrbitalPageTitle}" />
   21          <TextBlock x:Name="SubtitleText"
   22                     Style="{StaticResource OrbitalBody}"
   23                     Foreground="{ThemeResource OrbitalText40Brush}"
   24                     FontWeight="Normal" />
   25        </utu:AutoLayout>
   26  
   27        <!-- Search Bar (clickable border) -->
   28        <Border x:Name="SearchBorder"
   29                Grid.Column="1"
   30                Background="{ThemeResource OrbitalSurface1Brush}"
   31                BorderBrush="{ThemeResource OrbitalSurface3Brush}"
   32                BorderThickness="1"
   33                CornerRadius="8"
   34                Padding="12,8"
   35                VerticalAlignment="Center"
   36                AutomationProperties.Name="Search or run command">
   37          <StackPanel Orientation="Horizontal"
   38                      Spacing="8">
   39            <FontIcon Glyph="&#xE721;"
   40                      FontSize="14"
   41                      Foreground="{ThemeResource OrbitalText35Brush}"
   42                      VerticalAlignment="Center" />
   43            <TextBlock Text="Search or run command..."
   44                       FontFamily="{StaticResource OrbitalMonoFont}"
   45                       FontSize="12"
   46                       Foreground="{ThemeResource OrbitalText30Brush}"
   47                       VerticalAlignment="Center" />
   48            <Border Background="{ThemeResource OrbitalSurface2Brush}"
   49                    CornerRadius="4"
   50                    Padding="6,2"
   51                    VerticalAlignment="Center">
   52              <TextBlock Text="Ctrl+K"
   53                         FontFamily="{StaticResource OrbitalMonoFont}"
   54                         FontSize="10"
   55                         Foreground="{ThemeResource OrbitalText35Brush}" />
   56            </Border>
   57          </StackPanel>
   58        </Border>
   59      </Grid>
   60    </Border>
   61  </UserControl>
```

### `Orbital/Orbital/Presentation/SettingsPage.xaml.cs`

```csharp
    1  using Orbital.Helpers;
    2  
    3  namespace Orbital.Presentation;
    4  
    5  public sealed partial class SettingsPage : Page
    6  {
    7      public SettingsPage()
    8      {
    9          this.InitializeComponent();
   10          this.Loaded += OnLoaded;
   11      }
   12  
   13      private void OnLoaded(object sender, RoutedEventArgs e)
   14      {
   15          AnimationHelper.FadeUp(ProfileSection, 0);
   16          AnimationHelper.FadeUp(AboutSection, 100);
   17          AnimationHelper.FadeUp(PathsSection, 200);
   18          AnimationHelper.FadeUp(ActionsSection, 300);
   19  
   20          LoadUsername();
   21          WireButtons();
   22      }
   23  
   24      private void LoadUsername()
   25      {
   26          var name = SettingsService.GetStoredUsername();
   27          if (!string.IsNullOrEmpty(name))
   28              UsernameBox.Text = name;
   29  
   30          SaveUsernameButton.Click += (_, _) =>
   31          {
   32              var newName = UsernameBox.Text?.Trim() ?? "";
   33              SettingsService.SaveUsername(newName);
   34  
   35              SaveUsernameButton.Content = "Saved!";
   36              DispatcherQueue.TryEnqueue(async () =>
   37              {
   38                  await Task.Delay(1500);
   39                  SaveUsernameButton.Content = "Save";
   40              });
   41          };
   42      }
   43  
   44      private void WireButtons()
   45      {
   46          ClearRecentsButton.Click += async (_, _) =>
   47          {
   48              var host = HostHelper.GetHost();
   49              if (host is null) return;
   50  
   51              var ctx = host.Services.GetRequiredService<IProjectContext>();
   52              var recents = await ctx.GetRecentProjectsAsync(CancellationToken.None);
   53              foreach (var p in recents)
   54                  ctx.RemoveRecentProject(p.SolutionPath);
   55  
   56              var dialog = new ContentDialog
   57              {
   58                  Title = "Cleared",
   59                  Content = "Recent projects list has been cleared.",
   60                  CloseButtonText = "OK",
   61                  XamlRoot = this.XamlRoot,
   62              };
   63              await dialog.ShowAsync();
   64          };
   65  
   66          OpenDataFolderButton.Click += (_, _) =>
   67          {
   68              var dataDir = Path.Combine(
   69                  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Orbital");
   70              try
   71              {
   72                  Directory.CreateDirectory(dataDir);
   73                  System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
   74                  {
   75                      FileName = dataDir,
   76                      UseShellExecute = true,
   77                  });
   78              }
   79              catch { }
   80          };
   81  
   82          OpenDocsButton.Click += async (_, _) =>
   83          {
   84              await Windows.System.Launcher.LaunchUriAsync(
   85                  new Uri("https://platform.uno/docs/articles/intro.html"));
   86          };
   87      }
   88  }
```

### `Orbital/Orbital/Styles/Surfaces.xaml`

```xml
    1  <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    2                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    3  
    4    <!-- Standard card surface: Surface-1 bg, Surface-3 border, 12px radius, 20px padding -->
    5    <Style x:Key="OrbitalCardStyle" TargetType="Border">
    6      <Setter Property="Background" Value="{ThemeResource OrbitalSurface1Brush}" />
    7      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalSurface3Brush}" />
    8      <Setter Property="BorderThickness" Value="1" />
    9      <Setter Property="CornerRadius" Value="12" />
   10      <Setter Property="Padding" Value="20" />
   11    </Style>
   12  
   13    <!-- Console/inset surface: Surface-1.5 bg -->
   14    <Style x:Key="OrbitalConsoleSurfaceStyle" TargetType="Border">
   15      <Setter Property="Background" Value="{ThemeResource OrbitalSurface15Brush}" />
   16      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalSurface2Brush}" />
   17      <Setter Property="BorderThickness" Value="1" />
   18      <Setter Property="CornerRadius" Value="8" />
   19      <Setter Property="Padding" Value="16" />
   20    </Style>
   21  
   22    <!-- Badge styles -->
   23    <Style x:Key="OrbitalBadgeSuccess" TargetType="Border">
   24      <Setter Property="Background" Value="{ThemeResource OrbitalEmerald500_15Brush}" />
   25      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalEmerald500_20Brush}" />
   26      <Setter Property="BorderThickness" Value="1" />
   27      <Setter Property="CornerRadius" Value="6" />
   28      <Setter Property="Padding" Value="8,2" />
   29    </Style>
   30  
   31    <Style x:Key="OrbitalBadgeWarning" TargetType="Border">
   32      <Setter Property="Background" Value="{ThemeResource OrbitalAmber500_15Brush}" />
   33      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalAmber500_20Brush}" />
   34      <Setter Property="BorderThickness" Value="1" />
   35      <Setter Property="CornerRadius" Value="6" />
   36      <Setter Property="Padding" Value="8,2" />
   37    </Style>
   38  
   39    <Style x:Key="OrbitalBadgeError" TargetType="Border">
   40      <Setter Property="Background" Value="{ThemeResource OrbitalRed500_15Brush}" />
   41      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalRed400_20Brush}" />
   42      <Setter Property="BorderThickness" Value="1" />
   43      <Setter Property="CornerRadius" Value="6" />
   44      <Setter Property="Padding" Value="8,2" />
   45    </Style>
   46  
   47    <Style x:Key="OrbitalBadgeMuted" TargetType="Border">
   48      <Setter Property="Background" Value="{ThemeResource OrbitalZinc500_10Brush}" />
   49      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalZinc500_15Brush}" />
   50      <Setter Property="BorderThickness" Value="1" />
   51      <Setter Property="CornerRadius" Value="6" />
   52      <Setter Property="Padding" Value="8,2" />
   53    </Style>
   54  
   55    <!-- Scrollbar styling: 6px wide, Surface-4 color, 3px radius -->
   56    <Style TargetType="ScrollBar">
   57      <Setter Property="MinWidth" Value="6" />
   58      <Setter Property="Width" Value="6" />
   59      <Setter Property="MinHeight" Value="6" />
   60      <Setter Property="Height" Value="Auto" />
   61    </Style>
   62  
   63    <!-- Focus visual: 2px emerald-400 ring for accessibility -->
   64    <SolidColorBrush x:Key="SystemControlFocusVisualPrimaryBrush" Color="{ThemeResource OrbitalEmerald400}" />
   65    <SolidColorBrush x:Key="SystemControlFocusVisualSecondaryBrush" Color="Transparent" />
   66    <Thickness x:Key="FocusVisualMargin">-2</Thickness>
   67  
   68  </ResourceDictionary>
```

### `Orbital/Orbital/Styles/Buttons.xaml`

```xml
    1  <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    2                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    3  
    4    <!-- Primary Button: Emerald bg, dark text. Hover: lift + lighter emerald. Pressed: scale(0.97) -->
    5    <Style x:Key="OrbitalPrimaryButton" TargetType="Button">
    6      <Setter Property="Background" Value="{ThemeResource OrbitalEmerald500Brush}" />
    7      <Setter Property="Foreground" Value="{ThemeResource OrbitalSurface0Brush}" />
    8      <Setter Property="BorderThickness" Value="0" />
    9      <Setter Property="CornerRadius" Value="8" />
   10      <Setter Property="Padding" Value="16,8" />
   11      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
   12      <Setter Property="FontSize" Value="13" />
   13      <Setter Property="FontWeight" Value="Medium" />
   14      <Setter Property="Template">
   15        <Setter.Value>
   16          <ControlTemplate TargetType="Button">
   17            <Border x:Name="RootBorder"
   18                    Background="{TemplateBinding Background}"
   19                    BorderBrush="{TemplateBinding BorderBrush}"
   20                    BorderThickness="{TemplateBinding BorderThickness}"
   21                    CornerRadius="{TemplateBinding CornerRadius}"
   22                    Padding="{TemplateBinding Padding}"
   23                    RenderTransformOrigin="0.5,0.5">
   24              <Border.RenderTransform>
   25                <CompositeTransform x:Name="BtnTransform" ScaleX="1" ScaleY="1" TranslateY="0" />
   26              </Border.RenderTransform>
   27              <VisualStateManager.VisualStateGroups>
   28                <VisualStateGroup x:Name="CommonStates">
   29                  <VisualState x:Name="Normal" />
   30                  <VisualState x:Name="PointerOver">
   31                    <Storyboard>
   32                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
   33                                       To="-2" Duration="0:0:0.15">
   34                        <DoubleAnimation.EasingFunction>
   35                          <CubicEase EasingMode="EaseOut" />
   36                        </DoubleAnimation.EasingFunction>
   37                      </DoubleAnimation>
   38                    </Storyboard>
   39                    <VisualState.Setters>
   40                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalEmerald400Brush}" />
   41                    </VisualState.Setters>
   42                  </VisualState>
   43                  <VisualState x:Name="Pressed">
   44                    <Storyboard>
   45                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleX"
   46                                       To="0.97" Duration="0:0:0.1" />
   47                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleY"
   48                                       To="0.97" Duration="0:0:0.1" />
   49                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
   50                                       To="-1" Duration="0:0:0.1" />
   51                    </Storyboard>
   52                    <VisualState.Setters>
   53                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalEmerald400Brush}" />
   54                    </VisualState.Setters>
   55                  </VisualState>
   56                  <VisualState x:Name="Disabled">
   57                    <VisualState.Setters>
   58                      <Setter Target="RootBorder.Opacity" Value="0.4" />
   59                    </VisualState.Setters>
   60                  </VisualState>
   61                </VisualStateGroup>
   62              </VisualStateManager.VisualStateGroups>
   63              <ContentPresenter Content="{TemplateBinding Content}"
   64                                ContentTemplate="{TemplateBinding ContentTemplate}"
   65                                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
   66                                VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
   67                                Foreground="{TemplateBinding Foreground}" />
   68            </Border>
   69          </ControlTemplate>
   70        </Setter.Value>
   71      </Setter>
   72    </Style>
   73  
   74    <!-- Secondary Button: Surface bg. Hover: lift + Surface-3.5 bg. Pressed: scale(0.97) -->
   75    <Style x:Key="OrbitalSecondaryButton" TargetType="Button">
   76      <Setter Property="Background" Value="{ThemeResource OrbitalSurface2Brush}" />
   77      <Setter Property="Foreground" Value="{ThemeResource OrbitalText80Brush}" />
   78      <Setter Property="BorderBrush" Value="{ThemeResource OrbitalSurface35Brush}" />
   79      <Setter Property="BorderThickness" Value="1" />
   80      <Setter Property="CornerRadius" Value="8" />
   81      <Setter Property="Padding" Value="16,8" />
   82      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
   83      <Setter Property="FontSize" Value="13" />
   84      <Setter Property="FontWeight" Value="Medium" />
   85      <Setter Property="Template">
   86        <Setter.Value>
   87          <ControlTemplate TargetType="Button">
   88            <Border x:Name="RootBorder"
   89                    Background="{TemplateBinding Background}"
   90                    BorderBrush="{TemplateBinding BorderBrush}"
   91                    BorderThickness="{TemplateBinding BorderThickness}"
   92                    CornerRadius="{TemplateBinding CornerRadius}"
   93                    Padding="{TemplateBinding Padding}"
   94                    RenderTransformOrigin="0.5,0.5">
   95              <Border.RenderTransform>
   96                <CompositeTransform x:Name="BtnTransform" ScaleX="1" ScaleY="1" TranslateY="0" />
   97              </Border.RenderTransform>
   98              <VisualStateManager.VisualStateGroups>
   99                <VisualStateGroup x:Name="CommonStates">
  100                  <VisualState x:Name="Normal" />
  101                  <VisualState x:Name="PointerOver">
  102                    <Storyboard>
  103                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
  104                                       To="-2" Duration="0:0:0.15">
  105                        <DoubleAnimation.EasingFunction>
  106                          <CubicEase EasingMode="EaseOut" />
  107                        </DoubleAnimation.EasingFunction>
  108                      </DoubleAnimation>
  109                    </Storyboard>
  110                    <VisualState.Setters>
  111                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalSurface35Brush}" />
  112                    </VisualState.Setters>
  113                  </VisualState>
  114                  <VisualState x:Name="Pressed">
  115                    <Storyboard>
  116                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleX"
  117                                       To="0.97" Duration="0:0:0.1" />
  118                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleY"
  119                                       To="0.97" Duration="0:0:0.1" />
  120                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
  121                                       To="-1" Duration="0:0:0.1" />
  122                    </Storyboard>
  123                    <VisualState.Setters>
  124                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalSurface3Brush}" />
  125                    </VisualState.Setters>
  126                  </VisualState>
  127                  <VisualState x:Name="Disabled">
  128                    <VisualState.Setters>
  129                      <Setter Target="RootBorder.Opacity" Value="0.4" />
  130                    </VisualState.Setters>
  131                  </VisualState>
  132                </VisualStateGroup>
  133              </VisualStateManager.VisualStateGroups>
  134              <ContentPresenter Content="{TemplateBinding Content}"
  135                                ContentTemplate="{TemplateBinding ContentTemplate}"
  136                                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
  137                                VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
  138                                Foreground="{TemplateBinding Foreground}" />
  139            </Border>
  140          </ControlTemplate>
  141        </Setter.Value>
  142      </Setter>
  143    </Style>
  144  
  145    <!-- Ghost Button: Transparent bg. Hover: lift + text brightens, Surface-2 bg. Pressed: scale(0.97) -->
  146    <Style x:Key="OrbitalGhostButton" TargetType="Button">
  147      <Setter Property="Background" Value="Transparent" />
  148      <Setter Property="Foreground" Value="{ThemeResource OrbitalText55Brush}" />
  149      <Setter Property="BorderThickness" Value="0" />
  150      <Setter Property="CornerRadius" Value="8" />
  151      <Setter Property="Padding" Value="16,8" />
  152      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
  153      <Setter Property="FontSize" Value="13" />
  154      <Setter Property="FontWeight" Value="Medium" />
  155      <Setter Property="Template">
  156        <Setter.Value>
  157          <ControlTemplate TargetType="Button">
  158            <Border x:Name="RootBorder"
  159                    Background="{TemplateBinding Background}"
  160                    BorderBrush="{TemplateBinding BorderBrush}"
  161                    BorderThickness="{TemplateBinding BorderThickness}"
  162                    CornerRadius="{TemplateBinding CornerRadius}"
  163                    Padding="{TemplateBinding Padding}"
  164                    RenderTransformOrigin="0.5,0.5">
  165              <Border.RenderTransform>
  166                <CompositeTransform x:Name="BtnTransform" ScaleX="1" ScaleY="1" TranslateY="0" />
  167              </Border.RenderTransform>
  168              <VisualStateManager.VisualStateGroups>
  169                <VisualStateGroup x:Name="CommonStates">
  170                  <VisualState x:Name="Normal" />
  171                  <VisualState x:Name="PointerOver">
  172                    <Storyboard>
  173                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
  174                                       To="-2" Duration="0:0:0.15">
  175                        <DoubleAnimation.EasingFunction>
  176                          <CubicEase EasingMode="EaseOut" />
  177                        </DoubleAnimation.EasingFunction>
  178                      </DoubleAnimation>
  179                    </Storyboard>
  180                    <VisualState.Setters>
  181                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalSurface2Brush}" />
  182                      <Setter Target="ContentPresenter.Foreground" Value="{ThemeResource OrbitalText80Brush}" />
  183                    </VisualState.Setters>
  184                  </VisualState>
  185                  <VisualState x:Name="Pressed">
  186                    <Storyboard>
  187                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleX"
  188                                       To="0.97" Duration="0:0:0.1" />
  189                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="ScaleY"
  190                                       To="0.97" Duration="0:0:0.1" />
  191                      <DoubleAnimation Storyboard.TargetName="BtnTransform" Storyboard.TargetProperty="TranslateY"
  192                                       To="-1" Duration="0:0:0.1" />
  193                    </Storyboard>
  194                    <VisualState.Setters>
  195                      <Setter Target="RootBorder.Background" Value="{ThemeResource OrbitalSurface2Brush}" />
  196                      <Setter Target="ContentPresenter.Foreground" Value="{ThemeResource OrbitalText80Brush}" />
  197                    </VisualState.Setters>
  198                  </VisualState>
  199                  <VisualState x:Name="Disabled">
  200                    <VisualState.Setters>
  201                      <Setter Target="RootBorder.Opacity" Value="0.4" />
  202                    </VisualState.Setters>
  203                  </VisualState>
  204                </VisualStateGroup>
  205              </VisualStateManager.VisualStateGroups>
  206              <ContentPresenter x:Name="ContentPresenter"
  207                                Content="{TemplateBinding Content}"
  208                                ContentTemplate="{TemplateBinding ContentTemplate}"
  209                                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
  210                                VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
  211                                Foreground="{TemplateBinding Foreground}" />
  212            </Border>
  213          </ControlTemplate>
  214        </Setter.Value>
  215      </Setter>
  216    </Style>
  217  
  218    <!-- Small variants -->
  219    <Style x:Key="OrbitalPrimaryButtonSm" TargetType="Button" BasedOn="{StaticResource OrbitalPrimaryButton}">
  220      <Setter Property="Padding" Value="12,6" />
  221    </Style>
  222    <Style x:Key="OrbitalSecondaryButtonSm" TargetType="Button" BasedOn="{StaticResource OrbitalSecondaryButton}">
  223      <Setter Property="Padding" Value="12,6" />
  224    </Style>
  225    <Style x:Key="OrbitalGhostButtonSm" TargetType="Button" BasedOn="{StaticResource OrbitalGhostButton}">
  226      <Setter Property="Padding" Value="12,6" />
  227    </Style>
  228  
  229  </ResourceDictionary>
```

### `Orbital/Orbital/Styles/TextBlock.xaml`

```xml
    1  <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    2                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    3  
    4    <!-- Font families -->
    5    <FontFamily x:Key="OrbitalSansFont">ms-appx:///Assets/Fonts/DMSans-Variable.ttf#DM Sans</FontFamily>
    6    <FontFamily x:Key="OrbitalMonoFont">ms-appx:///Assets/Fonts/JetBrainsMono-Variable.ttf#JetBrains Mono</FontFamily>
    7    <FontFamily x:Key="OrbitalDisplayFont">ms-appx:///Assets/Fonts/SpaceGrotesk-Variable.ttf#Space Grotesk</FontFamily>
    8  
    9    <!-- === DISPLAY / HEADER STYLES (Space Grotesk) === -->
   10  
   11    <!-- 28px Bold - Greeting headline -->
   12    <Style x:Key="OrbitalHeroTitle" TargetType="TextBlock">
   13      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   14      <Setter Property="FontSize" Value="28" />
   15      <Setter Property="FontWeight" Value="Bold" />
   16      <Setter Property="CharacterSpacing" Value="-25" />
   17      <Setter Property="Foreground" Value="{ThemeResource OrbitalText100Brush}" />
   18    </Style>
   19  
   20    <!-- 20px SemiBold - Page titles -->
   21    <Style x:Key="OrbitalPageTitle" TargetType="TextBlock">
   22      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   23      <Setter Property="FontSize" Value="20" />
   24      <Setter Property="FontWeight" Value="SemiBold" />
   25      <Setter Property="CharacterSpacing" Value="-25" />
   26      <Setter Property="Foreground" Value="{ThemeResource OrbitalText90Brush}" />
   27    </Style>
   28  
   29    <!-- 18px SemiBold - Card primary values -->
   30    <Style x:Key="OrbitalCardValue" TargetType="TextBlock">
   31      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   32      <Setter Property="FontSize" Value="18" />
   33      <Setter Property="FontWeight" Value="SemiBold" />
   34      <Setter Property="CharacterSpacing" Value="-25" />
   35      <Setter Property="Foreground" Value="{ThemeResource OrbitalText88Brush}" />
   36    </Style>
   37  
   38    <!-- 16px SemiBold - Session detail title -->
   39    <Style x:Key="OrbitalDetailTitle" TargetType="TextBlock">
   40      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   41      <Setter Property="FontSize" Value="16" />
   42      <Setter Property="FontWeight" Value="SemiBold" />
   43      <Setter Property="Foreground" Value="{ThemeResource OrbitalText88Brush}" />
   44    </Style>
   45  
   46    <!-- 15px SemiBold - Meta values, license title -->
   47    <Style x:Key="OrbitalMetaValue" TargetType="TextBlock">
   48      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   49      <Setter Property="FontSize" Value="15" />
   50      <Setter Property="FontWeight" Value="SemiBold" />
   51      <Setter Property="Foreground" Value="{ThemeResource OrbitalText85Brush}" />
   52    </Style>
   53  
   54    <!-- 14px Medium - Session name, active title -->
   55    <Style x:Key="OrbitalSessionName" TargetType="TextBlock">
   56      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   57      <Setter Property="FontSize" Value="14" />
   58      <Setter Property="FontWeight" Value="Medium" />
   59      <Setter Property="Foreground" Value="{ThemeResource OrbitalText85Brush}" />
   60    </Style>
   61  
   62    <!-- === BODY STYLES (DM Sans) === -->
   63  
   64    <!-- 14px Regular - Subtitle text -->
   65    <Style x:Key="OrbitalSubtitle" TargetType="TextBlock">
   66      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
   67      <Setter Property="FontSize" Value="14" />
   68      <Setter Property="FontWeight" Value="Normal" />
   69      <Setter Property="Foreground" Value="{ThemeResource OrbitalText42Brush}" />
   70    </Style>
   71  
   72    <!-- 13px Medium - Nav labels, button labels (Space Grotesk for sidebar) -->
   73    <Style x:Key="OrbitalBody" TargetType="TextBlock">
   74      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
   75      <Setter Property="FontSize" Value="13" />
   76      <Setter Property="FontWeight" Value="Medium" />
   77      <Setter Property="Foreground" Value="{ThemeResource OrbitalText80Brush}" />
   78    </Style>
   79  
   80    <!-- 12px Regular - Session goal, acceptance check text -->
   81    <Style x:Key="OrbitalCaption" TargetType="TextBlock">
   82      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
   83      <Setter Property="FontSize" Value="12" />
   84      <Setter Property="FontWeight" Value="Normal" />
   85      <Setter Property="Foreground" Value="{ThemeResource OrbitalText65Brush}" />
   86    </Style>
   87  
   88    <!-- 12px Medium - Tab labels, console title -->
   89    <Style x:Key="OrbitalTabLabel" TargetType="TextBlock">
   90      <Setter Property="FontFamily" Value="{StaticResource OrbitalSansFont}" />
   91      <Setter Property="FontSize" Value="12" />
   92      <Setter Property="FontWeight" Value="Medium" />
   93      <Setter Property="Foreground" Value="{ThemeResource OrbitalText40Brush}" />
   94    </Style>
   95  
   96    <!-- === MONO STYLES === -->
   97  
   98    <!-- 32px Light - Hero clock display -->
   99    <Style x:Key="OrbitalClockHero" TargetType="TextBlock">
  100      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  101      <Setter Property="FontSize" Value="32" />
  102      <Setter Property="FontWeight" Value="Light" />
  103      <Setter Property="CharacterSpacing" Value="-25" />
  104      <Setter Property="Foreground" Value="{ThemeResource OrbitalText30Brush}" />
  105    </Style>
  106  
  107    <!-- 13px Medium Mono - Check text, feature names, platform labels -->
  108    <Style x:Key="OrbitalMonoBody" TargetType="TextBlock">
  109      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  110      <Setter Property="FontSize" Value="13" />
  111      <Setter Property="FontWeight" Value="Medium" />
  112      <Setter Property="Foreground" Value="{ThemeResource OrbitalText75Brush}" />
  113    </Style>
  114  
  115    <!-- 12px Regular Mono - Console body, artifact filename, dep name -->
  116    <Style x:Key="OrbitalMonoConsole" TargetType="TextBlock">
  117      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  118      <Setter Property="FontSize" Value="12" />
  119      <Setter Property="FontWeight" Value="Normal" />
  120      <Setter Property="Foreground" Value="{ThemeResource OrbitalText65Brush}" />
  121    </Style>
  122  
  123    <!-- 11px Medium Mono Uppercase - Section headers -->
  124    <Style x:Key="OrbitalSectionHeader" TargetType="TextBlock">
  125      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  126      <Setter Property="FontSize" Value="11" />
  127      <Setter Property="FontWeight" Value="Medium" />
  128      <Setter Property="CharacterSpacing" Value="80" />
  129      <Setter Property="Foreground" Value="{ThemeResource OrbitalText38Brush}" />
  130    </Style>
  131  
  132    <!-- 11px Regular Mono - Badge text, MCP status, metadata, path -->
  133    <Style x:Key="OrbitalMonoSmall" TargetType="TextBlock">
  134      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  135      <Setter Property="FontSize" Value="11" />
  136      <Setter Property="FontWeight" Value="Normal" />
  137      <Setter Property="Foreground" Value="{ThemeResource OrbitalText35Brush}" />
  138    </Style>
  139  
  140    <!-- 10px Medium Uppercase - Section sub-labels (Space Grotesk for sidebar) -->
  141    <Style x:Key="OrbitalSectionSubLabel" TargetType="TextBlock">
  142      <Setter Property="FontFamily" Value="{StaticResource OrbitalDisplayFont}" />
  143      <Setter Property="FontSize" Value="10" />
  144      <Setter Property="FontWeight" Value="Medium" />
  145      <Setter Property="CharacterSpacing" Value="120" />
  146      <Setter Property="Foreground" Value="{ThemeResource OrbitalText32Brush}" />
  147    </Style>
  148  
  149    <!-- 10px Regular Mono - Version pill values, feature desc, timestamps -->
  150    <Style x:Key="OrbitalMonoMeta" TargetType="TextBlock">
  151      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  152      <Setter Property="FontSize" Value="10" />
  153      <Setter Property="FontWeight" Value="Normal" />
  154      <Setter Property="Foreground" Value="{ThemeResource OrbitalText55Brush}" />
  155    </Style>
  156  
  157    <!-- 9px Regular Mono Uppercase - Version pill labels -->
  158    <Style x:Key="OrbitalMonoPillLabel" TargetType="TextBlock">
  159      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  160      <Setter Property="FontSize" Value="9" />
  161      <Setter Property="FontWeight" Value="Normal" />
  162      <Setter Property="CharacterSpacing" Value="80" />
  163      <Setter Property="Foreground" Value="{ThemeResource OrbitalText32Brush}" />
  164    </Style>
  165  
  166    <!-- Console line numbers -->
  167    <Style x:Key="OrbitalMonoLineNumber" TargetType="TextBlock">
  168      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  169      <Setter Property="FontSize" Value="12" />
  170      <Setter Property="FontWeight" Value="Normal" />
  171      <Setter Property="Foreground" Value="{ThemeResource OrbitalText25Brush}" />
  172      <Setter Property="TextAlignment" Value="Right" />
  173      <Setter Property="Width" Value="24" />
  174    </Style>
  175  
  176    <!-- Sidebar clock -->
  177    <Style x:Key="OrbitalSidebarClock" TargetType="TextBlock">
  178      <Setter Property="FontFamily" Value="{StaticResource OrbitalMonoFont}" />
  179      <Setter Property="FontSize" Value="11" />
  180      <Setter Property="FontWeight" Value="Normal" />
  181      <Setter Property="Foreground" Value="{ThemeResource OrbitalText40Brush}" />
  182    </Style>
  183  
  184  </ResourceDictionary>
```

### `Orbital/Orbital/Styles/OrbitalBrushes.xaml`

```xml
    1  <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    2                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    3  
    4    <!-- System brush override -->
    5    <SolidColorBrush x:Key="ApplicationPageBackgroundThemeBrush" Color="#FF0A0A0B" />
    6  
    7    <!-- Orbital surface brushes (monochrome near-black) -->
    8    <SolidColorBrush x:Key="OrbitalSurface0Brush" Color="#FF0A0A0B" />
    9    <SolidColorBrush x:Key="OrbitalSurface05Brush" Color="#FF0E0E10" />
   10    <SolidColorBrush x:Key="OrbitalSurface1Brush" Color="#FF141416" />
   11    <SolidColorBrush x:Key="OrbitalSurface15Brush" Color="#FF0C0C0D" />
   12    <SolidColorBrush x:Key="OrbitalSurface2Brush" Color="#FF1A1A1C" />
   13    <SolidColorBrush x:Key="OrbitalSurface3Brush" Color="#FF212123" />
   14    <SolidColorBrush x:Key="OrbitalSurface35Brush" Color="#FF262628" />
   15    <SolidColorBrush x:Key="OrbitalSurface4Brush" Color="#FF2D2D30" />
   16  
   17    <!-- Orbital accent brushes (unchanged) -->
   18    <SolidColorBrush x:Key="OrbitalEmerald400Brush" Color="#FF34D399" />
   19    <SolidColorBrush x:Key="OrbitalEmerald500Brush" Color="#FF10B981" />
   20    <SolidColorBrush x:Key="OrbitalEmerald600Brush" Color="#FF059669" />
   21    <SolidColorBrush x:Key="OrbitalTeal600Brush" Color="#FF0D9488" />
   22    <SolidColorBrush x:Key="OrbitalViolet400Brush" Color="#FFA78BFA" />
   23    <SolidColorBrush x:Key="OrbitalViolet500Brush" Color="#FF8B5CF6" />
   24    <SolidColorBrush x:Key="OrbitalAmber400Brush" Color="#FFFBBF24" />
   25    <SolidColorBrush x:Key="OrbitalRed400Brush" Color="#FFF87171" />
   26    <SolidColorBrush x:Key="OrbitalRed500Brush" Color="#FFEF4444" />
   27    <SolidColorBrush x:Key="OrbitalBlue400Brush" Color="#FF60A5FA" />
   28    <SolidColorBrush x:Key="OrbitalZinc500Brush" Color="#FF71717A" />
   29  
   30    <!-- Orbital text brushes (unchanged) -->
   31    <SolidColorBrush x:Key="OrbitalText100Brush" Color="#FFE8EAF0" />
   32    <SolidColorBrush x:Key="OrbitalText90Brush" Color="#FFE3E5EC" />
   33    <SolidColorBrush x:Key="OrbitalText88Brush" Color="#FFDEE0E8" />
   34    <SolidColorBrush x:Key="OrbitalText85Brush" Color="#FFD6D9E1" />
   35    <SolidColorBrush x:Key="OrbitalText82Brush" Color="#FFCED1DA" />
   36    <SolidColorBrush x:Key="OrbitalText80Brush" Color="#FFC9CCD6" />
   37    <SolidColorBrush x:Key="OrbitalText75Brush" Color="#FFBCC0CB" />
   38    <SolidColorBrush x:Key="OrbitalText72Brush" Color="#FFB5B9C5" />
   39    <SolidColorBrush x:Key="OrbitalText65Brush" Color="#FFA3A8B6" />
   40    <SolidColorBrush x:Key="OrbitalText55Brush" Color="#FF8A90A0" />
   41    <SolidColorBrush x:Key="OrbitalText50Brush" Color="#FF7B8191" />
   42    <SolidColorBrush x:Key="OrbitalText45Brush" Color="#FF6E7485" />
   43    <SolidColorBrush x:Key="OrbitalText42Brush" Color="#FF676D7E" />
   44    <SolidColorBrush x:Key="OrbitalText40Brush" Color="#FF626878" />
   45    <SolidColorBrush x:Key="OrbitalText38Brush" Color="#FF5D6372" />
   46    <SolidColorBrush x:Key="OrbitalText35Brush" Color="#FF565C6B" />
   47    <SolidColorBrush x:Key="OrbitalText32Brush" Color="#FF505664" />
   48    <SolidColorBrush x:Key="OrbitalText30Brush" Color="#FF4A505D" />
   49    <SolidColorBrush x:Key="OrbitalText25Brush" Color="#FF3F4452" />
   50  
   51    <!-- Opacity-variant accent brushes (unchanged) -->
   52    <SolidColorBrush x:Key="OrbitalEmerald500_15Brush" Color="#FF10B981" Opacity="0.15" />
   53    <SolidColorBrush x:Key="OrbitalEmerald500_20Brush" Color="#FF10B981" Opacity="0.20" />
   54    <SolidColorBrush x:Key="OrbitalEmerald500_25Brush" Color="#FF10B981" Opacity="0.25" />
   55    <SolidColorBrush x:Key="OrbitalEmerald500_30Brush" Color="#FF10B981" Opacity="0.30" />
   56    <SolidColorBrush x:Key="OrbitalEmerald500_10Brush" Color="#FF10B981" Opacity="0.10" />
   57    <SolidColorBrush x:Key="OrbitalEmerald500_5Brush" Color="#FF10B981" Opacity="0.05" />
   58    <SolidColorBrush x:Key="OrbitalEmerald400_20Brush" Color="#FF34D399" Opacity="0.20" />
   59    <SolidColorBrush x:Key="OrbitalViolet500_20Brush" Color="#FF8B5CF6" Opacity="0.20" />
   60    <SolidColorBrush x:Key="OrbitalViolet500_30Brush" Color="#FF8B5CF6" Opacity="0.30" />
   61    <SolidColorBrush x:Key="OrbitalAmber500_15Brush" Color="#FFFBBF24" Opacity="0.15" />
   62    <SolidColorBrush x:Key="OrbitalAmber500_20Brush" Color="#FFFBBF24" Opacity="0.20" />
   63    <SolidColorBrush x:Key="OrbitalRed500_15Brush" Color="#FFEF4444" Opacity="0.15" />
   64    <SolidColorBrush x:Key="OrbitalRed400_20Brush" Color="#FFF87171" Opacity="0.20" />
   65    <SolidColorBrush x:Key="OrbitalZinc500_10Brush" Color="#FF71717A" Opacity="0.10" />
   66    <SolidColorBrush x:Key="OrbitalZinc500_15Brush" Color="#FF71717A" Opacity="0.15" />
   67  
   68    <!-- Orbital Color resources (for gradients, lightweight styling) -->
   69    <Color x:Key="OrbitalEmerald400">#FF34D399</Color>
   70    <Color x:Key="OrbitalEmerald500">#FF10B981</Color>
   71    <Color x:Key="OrbitalEmerald600">#FF059669</Color>
   72    <Color x:Key="OrbitalTeal600">#FF0D9488</Color>
   73    <Color x:Key="OrbitalViolet400">#FFA78BFA</Color>
   74    <Color x:Key="OrbitalViolet500">#FF8B5CF6</Color>
   75    <Color x:Key="OrbitalAmber400">#FFFBBF24</Color>
   76    <Color x:Key="OrbitalRed400">#FFF87171</Color>
   77    <Color x:Key="OrbitalRed500">#FFEF4444</Color>
   78    <Color x:Key="OrbitalBlue400">#FF60A5FA</Color>
   79    <Color x:Key="OrbitalZinc500">#FF71717A</Color>
   80    <Color x:Key="OrbitalSurface0">#FF0A0A0B</Color>
   81    <Color x:Key="OrbitalSurface05">#FF0E0E10</Color>
   82    <Color x:Key="OrbitalSurface1">#FF141416</Color>
   83    <Color x:Key="OrbitalSurface15">#FF0C0C0D</Color>
   84    <Color x:Key="OrbitalSurface2">#FF1A1A1C</Color>
   85    <Color x:Key="OrbitalSurface25">#FF171718</Color>
   86    <Color x:Key="OrbitalSurface3">#FF212123</Color>
   87    <Color x:Key="OrbitalSurface35">#FF262628</Color>
   88    <Color x:Key="OrbitalSurface4">#FF2D2D30</Color>
   89    <Color x:Key="OrbitalText100">#FFE8EAF0</Color>
   90    <Color x:Key="OrbitalText90">#FFE3E5EC</Color>
   91    <Color x:Key="OrbitalText88">#FFDEE0E8</Color>
   92    <Color x:Key="OrbitalText85">#FFD6D9E1</Color>
   93    <Color x:Key="OrbitalText82">#FFCED1DA</Color>
   94    <Color x:Key="OrbitalText80">#FFC9CCD6</Color>
   95    <Color x:Key="OrbitalText75">#FFBCC0CB</Color>
   96    <Color x:Key="OrbitalText72">#FFB5B9C5</Color>
   97    <Color x:Key="OrbitalText65">#FFA3A8B6</Color>
   98    <Color x:Key="OrbitalText55">#FF8A90A0</Color>
   99    <Color x:Key="OrbitalText50">#FF7B8191</Color>
  100    <Color x:Key="OrbitalText45">#FF6E7485</Color>
  101    <Color x:Key="OrbitalText42">#FF676D7E</Color>
  102    <Color x:Key="OrbitalText40">#FF626878</Color>
  103    <Color x:Key="OrbitalText38">#FF5D6372</Color>
  104    <Color x:Key="OrbitalText35">#FF565C6B</Color>
  105    <Color x:Key="OrbitalText32">#FF505664</Color>
  106    <Color x:Key="OrbitalText30">#FF4A505D</Color>
  107    <Color x:Key="OrbitalText25">#FF3F4452</Color>
  108  </ResourceDictionary>
```

### `Orbital/Orbital/Controls/PageHeader.xaml.cs`

```csharp
    1  using Orbital.Helpers;
    2  
    3  namespace Orbital.Controls;
    4  
    5  public sealed partial class PageHeader : UserControl
    6  {
    7      public static event EventHandler? SearchRequested;
    8  
    9      public static readonly DependencyProperty TitleProperty =
   10          DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeader),
   11              new PropertyMetadata("", OnTitleChanged));
   12  
   13      public static readonly DependencyProperty SubtitleProperty =
   14          DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PageHeader),
   15              new PropertyMetadata("", OnSubtitleChanged));
   16  
   17      public string Title
   18      {
   19          get => (string)GetValue(TitleProperty);
   20          set => SetValue(TitleProperty, value);
   21      }
   22  
   23      public string Subtitle
   24      {
   25          get => (string)GetValue(SubtitleProperty);
   26          set => SetValue(SubtitleProperty, value);
   27      }
   28  
   29      public PageHeader()
   30      {
   31          this.InitializeComponent();
   32  
   33          SearchBorder.Tapped += (_, _) => SearchRequested?.Invoke(this, EventArgs.Empty);
   34          SearchBorder.PointerEntered += (s, _) =>
   35          {
   36              var b = (Border)s;
   37              b.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_20Brush"];
   38              b.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"];
   39          };
   40          SearchBorder.PointerExited += (s, _) =>
   41          {
   42              var b = (Border)s;
   43              b.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"];
   44              b.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"];
   45          };
   46  
   47      }
   48  
   49      private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   50      {
   51          if (d is PageHeader header)
   52              header.TitleText.Text = e.NewValue as string ?? "";
   53      }
   54  
   55      private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   56      {
   57          if (d is PageHeader header)
   58              header.SubtitleText.Text = e.NewValue as string ?? "";
   59      }
   60  }
```
