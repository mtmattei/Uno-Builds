---
name: uno-component-states
description: Transform an existing Uno Platform / WinUI component that currently shows populated data into its Loading, Empty, and Error states — preserving the component's exact structure, spacing, typography, shapes, colors, and density. Use whenever the user asks for loading/skeleton/shimmer states, empty states, error or retry states, placeholder states, "what does this look like while loading / with no data / when the fetch fails", or invokes /uno-component-states <file.xaml> [--states loading,empty,error]. This is a state-generation skill, NOT a UI-generation skill — do not use it to design a new component from scratch, and do not answer such requests by designing new UI freehand.
---

# Uno Component State Generator

Generate Loading, Empty, and Error states for an existing Uno Platform
component that is currently shown in its populated/data state.

**The critical rule: do not design three new components. Transform the
existing component into three alternate conditions of itself.**

The populated component is the design specification. Everything you produce
must read as "the same component on a different day" — same footprint, same
design language, same hierarchy — with only the content region changed to
communicate the new condition.

```
INPUT      existing component in its populated/data state
   ↓
ANALYZE    structure · layout · tokens/resources · typography ·
           spacing · shape · existing controls · interaction model
   ↓
GENERATE   Loading · Empty · Error
   ↓
VALIDATE   same footprint · same design language ·
           no unnecessary redesign · valid Uno XAML
```

## Invocation

`/uno-component-states <path/to/Component.xaml>` — generate all three states.

`/uno-component-states <path/to/Component.xaml> --states loading,empty` —
generate only the listed states (`loading`, `empty`, `error`).

If no file is given, resolve the target from conversation context; if it is
still ambiguous, ask which component before touching anything.

## The mental model: invariants vs. state variables

This is what separates state generation from UI generation. Before writing
any XAML, explicitly split the component into two lists:

```
COMPONENT INVARIANTS            STATE VARIABLES
────────────────────            ─────────────────────────────────────────
Root container                  Data content  → skeleton / message / message
Outer dimensions                Primary action → hidden / create CTA / retry
Padding                         Semantic accent → neutral / neutral / error
Corner radius
Background & border
Header / title & its typography
Grid structure & alignment
Button treatment
Color resources
Density
```

Invariants are the component's visual identity — they carry across every
state unchanged unless there is a concrete reason not to. State variables
are the only levers you pull. If you find yourself changing something in
the left column, stop and justify it; "it looks nicer" is not a reason.

Worked example — the card shell never gets reinvented:

```
DATA                                    LOADING
┌─────────────────────────────┐         ┌─────────────────────────────┐
│ Portfolio                   │         │ Portfolio                   │
│                             │         │                             │
│ AAPL    $231.42     +1.2%   │         │ █████   ██████      ████    │
│ NVDA    $182.11     +2.8%   │         │ █████   ██████      ████    │
│ MSFT    $519.30     -0.4%   │         │ █████   ██████      ████    │
└─────────────────────────────┘         └─────────────────────────────┘

EMPTY                                   ERROR
┌─────────────────────────────┐         ┌─────────────────────────────┐
│ Portfolio                   │         │ Portfolio                   │
│                             │         │                             │
│       No stocks yet         │         │    Couldn't load stocks     │
│      + Add a ticker         │         │           Retry             │
│                             │         │                             │
└─────────────────────────────┘         └─────────────────────────────┘
```

---

## Phase 1 — Analyze the existing component

Read the component and enough of its surroundings (ViewModel/model,
resource dictionaries, app-level styles) to answer all of the following
before modifying code. If the Uno App MCP is available (`mcp__uno*` /
Hot Design tools), also inspect the *running* app: visual tree, data
context, effective control properties, and a screenshot of the populated
state — the rendered truth beats your reading of the markup.

**Structure** — root container; rows/columns; repeated elements (the
`ItemsRepeater`/`ListView`/`ItemsControl` and its item template); headers;
actions; content areas; which dimensions are fixed vs. driven by content.

**Visual language** — typography and font hierarchy; foreground hierarchy;
backgrounds; borders; corner radius; shadows; spacing rhythm; alignment;
icon treatment; button styles; density.

**Resources** — find what the app already defines before introducing any
literal value: `StaticResource`/`ThemeResource` brushes, styles, text
styles, spacing and corner-radius tokens, semantic colors (including an
error brush). This repo's apps typically carry Material-style tokens
(`SurfaceBrush`, `OnSurfaceVariantBrush`, `OutlineBrush`, `PrimaryBrush`,
`ErrorBrush`, …) plus app-specific tokens — reuse those, never restate
their values as literals.

**Behavior** — what is being loaded? What would legitimately make it empty?
What could fail, and can the user recover? Can the user act from the empty
state (does a create/add flow actually exist)? Do not invent functionality
the surrounding application does not support.

## Phase 2 — Fix the invariants

Write down (in your working notes, or a brief message to the user) the
invariant list for this specific component: outer dimensions, root
container, padding, corner radius, background, border, header placement,
title typography, major alignment, density. These stay stable across all
states. Change the content's condition, not the component's identity.

---

## Phase 3 — Generate the states

### Loading

Represent the component while its data is on the way.

- Prefer a **skeleton** of the existing content structure: mirror the real
  row count (or a plausible page of rows), column positions, text
  hierarchy, and image/avatar placeholders. The goal is near-zero layout
  shift when data arrives — a skeleton that matches the populated layout
  makes the load feel faster and calmer than any spinner.
- Build skeleton blocks from `Border`/`Rectangle` sized like the real
  content (width approximating typical text length, height matching the
  text style's line height), filled with a low-emphasis surface brush the
  app already has (surface-variant / base-low), with a small corner
  radius. A subtle opacity pulse (`Storyboard` animating `Opacity`
  ~0.4→0.8, auto-reversing, ~1s) is enough; skip shimmer gradients unless
  the app already uses them.
- Do not replace a structured component with a generic centered
  `ProgressRing` unless a spinner is already the app's established loading
  idiom (check other screens first).
- Avoid "Loading…" text unless the component's design language calls
  for it.

### Empty

Represent the **successful absence** of data. Empty is not an error — it
must not look like one.

- Keep the shell and contextual elements (title, header actions).
- Replace the data region with a lightweight treatment: a short message in
  the app's existing secondary text style, optionally an icon from the
  app's icon set, optionally one CTA.
- Add a CTA only when the surrounding app actually supports the action
  ("No projects yet" + "Create project"; "No results" with no button).
  Wire it to an existing command — never a dead button.
- Use neutral foreground/secondary emphasis, existing button styles,
  existing spacing. No decorative illustrations unless the design system
  already uses them.

### Error

Represent failure to retrieve or display the data.

- Keep the shell. Communicate two things: something failed, and what the
  user can do next when recovery is possible ("Couldn't load your
  watchlist." + Retry).
- Use the app's semantic error resource as an *accent* — an icon tint, a
  message foreground — not a flood. The component keeps its identity; the
  error styling is emphasis, not a repaint.
- Wire Retry to the existing load/refresh command if one exists; omit the
  button when there is genuinely nothing to retry.
- Never surface raw exception text unless the component is explicitly
  developer-facing.

---

## Phase 4 — Implementation

Reuse as much of the existing component as possible; state-specific content
replaces the data region, everything else is shared. Follow the state
mechanism the app already uses — do not introduce a new state-management
architecture for this:

- **Uno Toolkit `FeedView`** — if the project already references Uno
  Toolkit (several apps in this repo do), `FeedView` exists precisely for
  this: put the states in its `ProgressTemplate`, `NoneTemplate`, and
  `ErrorTemplate` around the existing `ValueTemplate`.
- **`VisualStateManager`** — a `Loading/Empty/Error/Data` state group on
  the component root, toggling visibility of the shared shell's content
  layers. Good default for a card/section inside a page.
- **State-bound visibility / `x:Load`** — bind each layer to ViewModel
  state (e.g. an enum + converters, or `IsLoading`/`HasError` flags),
  matching however the app already expresses state.
- **MVUX feeds** — if the app uses MVUX, `FeedView` over the feed is the
  native answer; its states map 1:1 to this skill's output.

Preserve all existing bindings — the data state must keep working exactly
as before. Sample/design-time data may be used only for previewing states,
never left wired into production paths.

Before using an unfamiliar Uno/WinUI API or control, consult the Uno
Platform docs (docs MCP if available). Avoid new dependencies.

---

## Phase 5 — Validate

Validate each state against the original, not against your taste.

**Visual consistency** — same footprint? same outer container, spacing
system, typography, corner treatment, button styling, icon style, color
language?

**Loading** — does the skeleton resemble the actual data layout? Is layout
shift minimized? Does it read as *this component* loading, not a different
screen?

**Empty** — is absence of data clearly communicated, and clearly not an
error? Is any CTA actually supported and wired?

**Error** — is failure obvious without overwhelming the component? Is
recovery offered when possible? Does it use the existing semantic error
resource?

**Code** — does the project build (`dotnet build` the smallest target that
compiles the XAML)? Are existing bindings preserved, resources reused,
duplication avoided?

**In-app validation** — if the Uno App MCP is available, drive the running
app into each state (set the ViewModel state, or force the visual state),
screenshot all four conditions, and compare: the shell must be
pixel-stable across them. Fix any state that shifts the footprint or
breaks the design language, then re-check.

---

## Constraints

DO NOT:

- redesign the component or introduce a new visual language
- change dimensions, spacing, corner radii, or colors arbitrarily
  between states
- replace app resources with hardcoded values
- add actions the surrounding app doesn't support
- treat Empty as Error, or make Empty look alarming
- show raw exception messages
- default every Loading state to a spinner
- duplicate the whole component when state-specific content can be swapped
  inside the shared shell
- introduce a new state-management architecture solely for these states

The populated component is always the primary design reference. When
uncertain, preserve rather than invent.
