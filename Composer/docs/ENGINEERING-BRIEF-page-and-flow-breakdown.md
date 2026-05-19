# Engineering Brief — Page and UX-Flow Breakdown

**Status:** ready to read
**Purpose:** unstick an implementation agent whose generated Uno code doesn't match the prototype's behavior
**Companion to:** ARCHITECTURE-BRIEF-detailed.md, DESIGN-BRIEF-detailed.md, INTERACTION-BRIEF-detailed.md, ENGINEERING-BRIEF-01-stack-preferences.md, ENGINEERING-BRIEF-02-structured-layer-brief-generators.md

The detailed briefs cover *what* the system is. This one covers *what's on each page, what flows between pages, and what's in the Shell versus the Page versus the Canvas* — which is the boundary most agents get wrong when translating a single-component React prototype into a multi-page Uno app.

If your agent has generated Uno code where the rails are inside pages, or where pages call `Navigator.NavigateAsync` directly from code-behind, or where locked context cards are duplicated per page, this brief is what unsticks it.

---

## Part 1 — Prototype-to-Uno concept mapping

The prototype is a single 2815-line React component that switches its center-column content on `activeIndex`. The Uno port splits this into a **Shell + 9 Pages**. The mapping is:

| Prototype concept (in `composer-context-engine.jsx`)               | Uno equivalent                                              |
|---------------------------------------------------------------------|-------------------------------------------------------------|
| `CompositionEngine` default export                                  | `Shell.xaml` (the three-column UserControl)                 |
| `CompositionStack` left-rail component                              | `CompositionStackRegion.xaml` (UserControl in Shell)        |
| `FilesRail` right-rail component                                    | `FilesRailRegion.xaml` (UserControl in Shell)               |
| `ProgressIndicator` component                                       | `ProgressIndicator.xaml` UserControl, rendered on every Page |
| `LockedContextCard` × N (rendered in `lockedCards` useMemo)         | `LockedContextCard.xaml` rendered via `ItemsRepeater` on every Page, source bound to `ShellModel.LockedCards` |
| `ActiveLayerHeader` component                                       | `ActiveLayerHeader.xaml` UserControl, rendered on every Page with per-page bindings |
| The `renderCanvas()` switch returning per-layer JSX                  | **9 separate Pages** — `StackPage`, `IntentPage`, ..., `ScaffoldPage` — each hosting ONE canvas UserControl |
| Per-layer canvas component (`IntentCanvas`, `ArchitectureCanvas`, …) | Canvas UserControl per layer (`IntentCanvas.xaml`, etc.), embedded in the matching Page |
| `ComposerFooter` component                                          | `ComposerFooter.xaml` UserControl, rendered on every Page (except Scaffold) with per-page bindings |
| `FuturePreviewCard` × N                                             | `FuturePreviewCard.xaml` rendered via `ItemsRepeater` on every Page, source bound to `ShellModel.FutureCards` |
| `activeIndex` state                                                 | `ShellModel.ActiveIndex` `IState<int>`                      |
| `setActiveIndex(i)` call                                            | `ShellModel.NavigateToLayer(LayerKind)` → calls `INavigator.NavigateRouteAsync` |
| `lockedIds` Set state                                               | `ShellModel.LockedIds` `IState<ImmutableHashSet<LayerKind>>` |
| `prompts[layerId]` per-layer textarea content                       | `ComposerModel.Prompts` `IState<ImmutableDictionary<LayerKind, string>>` |
| `overrideMarkdown[layerId]`                                         | `ComposerModel.Overrides` `IState<ImmutableDictionary<LayerKind, string>>` |
| The single big component                                            | NOT one giant Page — 9 Pages each hosting their own ViewModel resolved from DI |

**The single most important concept the agent gets wrong:**

Pages do NOT contain the rails. The Shell contains the rails. Each Page contains only the content for the *center column* — the progress indicator, the locked context stack, the active layer header, the canvas, the composer footer, and the future preview cards. Everything visible to the left or right of the center column is rendered by the Shell and stays in place as the user navigates between pages.

---

## Part 2 — Shell anatomy

### 2.1 Visual layout

```
┌─ Shell.xaml ────────────────────────────────────────────────────────────────┐
│                                                                              │
│   ┌─ LeftRail region ─┬─ ActivePage region ───┬─ RightRail region ─┐         │
│   │                    │                       │                    │         │
│   │   CompositionStack │   {one of 9 Pages}   │     FilesRail      │         │
│   │   (sticky, 260px)  │   (flex, max 880px)  │   (sticky, 340px)  │         │
│   │                    │                       │                    │         │
│   │   - StackItem      │   ┌─ Page content ──┐│   - Live file      │         │
│   │   - StackItem      │   │ ProgressInd.    ││     panel          │         │
│   │   - StackItem      │   │ LockedCards × N ││   - File list      │         │
│   │   - StackItem      │   │ ActiveHeader    ││   - Locked count   │         │
│   │   - StackItem      │   │ Canvas          ││                    │         │
│   │   - StackItem      │   │ ComposerFooter  ││                    │         │
│   │   - StackItem      │   │ FutureCards × M ││                    │         │
│   │   - StackItem      │   └─────────────────┘│                    │         │
│   │   - StackItem      │                       │                    │         │
│   │                    │                       │                    │         │
│   └────────────────────┴───────────────────────┴────────────────────┘         │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Three named regions

The Shell defines three Uno Navigation regions. Only the center one (`ActivePage`) ever navigates — the other two stay put.

```xml
<UserControl x:Class="ComposerContextEngine.Views.Shell"
             xmlns:uen="using:Uno.Extensions.Navigation.UI"
             xmlns:controls="using:ComposerContextEngine.Views.Controls"
             Background="{ThemeResource BackgroundBrush}">

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="{Binding LeftRailWidth, Mode=OneWay}" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="{Binding RightRailWidth, Mode=OneWay}" />
        </Grid.ColumnDefinitions>

        <!-- LEFT: fixed content, no navigation -->
        <controls:CompositionStackRegion
            Grid.Column="0"
            Opacity="{Binding LeftRailOpacity, Mode=OneWay}"
            Visibility="{Binding LeftRailVisibility, Mode=OneWay}" />

        <!-- CENTER: the only region that navigates -->
        <Grid Grid.Column="1"
              uen:Region.Attached="True"
              uen:Region.Name="ActivePage"
              MaxWidth="{Binding CenterMaxWidth, Mode=OneWay}"
              HorizontalAlignment="Center" />

        <!-- RIGHT: fixed content, no navigation -->
        <controls:FilesRailRegion
            Grid.Column="2"
            Opacity="{Binding RightRailOpacity, Mode=OneWay}"
            Visibility="{Binding RightRailVisibility, Mode=OneWay}" />
    </Grid>
</UserControl>
```

`CompositionStackRegion` and `FilesRailRegion` are not navigation regions — they are UserControls with their own DataContext (resolved via the Shell's model graph). They render once, persist for the session, and rebind reactively as ShellModel state changes.

The `ActivePage` region is where the 9 Pages take turns. Navigation between them is driven by `ShellModel.NavigateToLayer(LayerKind)` which calls `INavigator.NavigateRouteAsync` with the route name. The current Page is unloaded; the next Page is loaded; the rails do not change.

### 2.3 What CompositionStackRegion renders

```
COMPOSITION STACK
A conversation that crystallizes into a build system.
─────────────────────────────────────────────────
01  STACK             ✓        ← Locked, ink2 border, ink text
    "MVUX, Material theme, 4 platforms"

02  INTENT            ✓        ← Locked
    "Field-service scheduling, for technicians..."

03  UX                          ← ACTIVE, amber border, paper3 bg
    five-screen dispatch flow

04  ARCHITECTURE                ← Future, 0.5 opacity
    how it is shaped

05  DESIGN SYSTEM
    how it feels

06  INTERACTIONS
    every state of every flow

07  DATA
    shapes and contracts

08  IMPLEMENTATION
    phased build plan

09  SCAFFOLD
    runnable starting point
```

Each `StackItem` is bound to `LayerDef` data + a derived `StackItemState` (Active / Locked / Future). Clicking a Locked item triggers `ShellModel.Revisit(kind)`. Clicking the Active item is a no-op. Clicking a Future item is a no-op (it's not hit-testable).

### 2.4 What FilesRailRegion renders

```
Live file                                  View all → 
Updates as the canvas changes. Edit to override.

ux-flows.md                  [Copy] [Preview][Edit]
GENERATED FROM CANVAS

┌────────────────────────────────────────────┐
│  # UX FLOWS                                 │
│  ─────────                                  │
│  ## Dispatch flow                           │
│  1. Dashboard — Today's jobs                │
│  2. Job detail — Status, location           │
│  ...                                        │
└────────────────────────────────────────────┘

─────────────────────────────────────────────
Files
Each layer emits files as it locks.

●  stack-preferences.md           DRAFTED
●  README.md                       DRAFTED
●  ux-flows.md                     WRITING
○  architecture.md                 PLANNED
○  design-system.md                PLANNED
○  interaction-spec.md             PLANNED
○  data-contracts.md               PLANNED
○  implementation-plan.md          PLANNED
○  scaffold.command                PLANNED
○  prompt-context.md               PLANNED

─────────────────────────────────────────────
2 of 9 locked
UX will write ux-flows.md when locked.
```

`FilesRailRegion` reads from `FilesRailModel` (lifetime: scoped to Shell session). The "Live file" content tracks the currently active page automatically via `Shell.ActiveIndex`.

---

## Part 3 — The page template (the 6-slot pattern)

**Every Page renders the same vertical sequence of UserControls.** The only thing that varies per Page is which canvas is in slot 5, and what bindings feed slot 4 and slot 6.

```
Page content (center column, scrollable)
┌──────────────────────────────────────────────────────────────┐
│                                                                │
│   [Slot 1]  ProgressIndicator                                  │
│             ─── [Shell-bound — same data on every page]        │
│             Hairline track + amber-filled segment + counter    │
│                                                                │
│   [Slot 2]  AppTitleRow                                        │
│             ─── [Shell-bound — same on every page]             │
│             Project name + Reset button                        │
│                                                                │
│   [Slot 3]  LockedContextCards (ItemsRepeater)                 │
│             ─── [Shell-bound — varies by activeIndex]          │
│             Renders LockedContextCard for every locked layer    │
│             whose index < activeIndex                          │
│                                                                │
│   [Slot 4]  ActiveLayerHeader                                  │
│             ─── [Page-specific bindings]                       │
│             Recap line (italic) + index + title + subtitle    │
│                                                                │
│   [Slot 5]  Canvas (THE UNIQUE PART)                           │
│             ─── [Page-specific Canvas UserControl]             │
│             Each Page hosts a DIFFERENT canvas here            │
│                                                                │
│   [Slot 6]  ComposerFooter                                     │
│             ─── [Page-specific bindings, except Scaffold]      │
│             Status word + lead question + suggestions +        │
│             textarea + action buttons                          │
│                                                                │
│   [Slot 7]  FuturePreviewCards (ItemsRepeater)                 │
│             ─── [Shell-bound — varies by activeIndex]          │
│             Renders FuturePreviewCard for every layer          │
│             whose index > activeIndex (dimmed, read-only)      │
│                                                                │
└──────────────────────────────────────────────────────────────┘
```

### 3.1 Canonical Page XAML

Every Page looks like this. Substitute the canvas UserControl in the canvas slot:

```xml
<Page x:Class="ComposerContextEngine.Views.Pages.IntentPage"
      xmlns:utu="using:Uno.Toolkit.UI"
      xmlns:controls="using:ComposerContextEngine.Views.Controls"
      xmlns:canvases="using:ComposerContextEngine.Views.Canvases"
      DataContext="{Binding Shell, Source={x:Static x:Application.Current}}">

    <ScrollViewer Padding="32,32,48,80">
        <utu:AutoLayout Orientation="Vertical" Spacing="0">

            <!-- Slot 1: Shell-bound progress -->
            <controls:ProgressIndicator
                ActiveIndex="{Binding ActiveIndex, Mode=OneWay}"
                Total="{Binding LayerCount, Mode=OneWay}"
                ActiveLayerLabel="{Binding ActiveLayerLabel, Mode=OneWay}" />

            <!-- Slot 2: Shell-bound app title row -->
            <controls:AppTitleRow
                ProjectName="{Binding ProjectName, Mode=OneWay}"
                ResetCommand="{Binding Reset}" />

            <!-- Slot 3: Shell-bound locked cards -->
            <ItemsRepeater ItemsSource="{Binding LockedCards, Mode=OneWay}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:LockedContextCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>

            <!-- Slot 4: Page-specific header — recap, title, subtitle come from this Page's model -->
            <controls:ActiveLayerHeader
                LayerIndex="{Binding Intent.LayerIndex, Mode=OneWay}"
                LayerLabel="{Binding Intent.LayerLabel, Mode=OneWay}"
                LayerState="{Binding Intent.LayerState, Mode=OneWay}"
                Recap="{Binding Intent.Recap, Mode=OneWay}"
                Title="{Binding Intent.Title, Mode=OneWay}"
                Subtitle="{Binding Intent.Subtitle, Mode=OneWay}" />

            <!-- Slot 5: THE UNIQUE PART — this Page's specific canvas -->
            <canvases:IntentCanvas DataContext="{Binding Intent}" />

            <!-- Slot 6: Page-specific composer — lead question, suggestions come from this Page's model -->
            <controls:ComposerFooter
                LayerState="{Binding Intent.LayerState, Mode=OneWay}"
                LeadQuestion="{Binding Intent.LeadQuestion, Mode=OneWay}"
                Suggestions="{Binding Intent.Suggestions, Mode=OneWay}"
                PromptValue="{Binding Intent.PromptValue, Mode=TwoWay}"
                IsFirstLayer="{Binding IsFirstLayer, Mode=OneWay}"
                AiConfigured="{Binding AiConfigured, Mode=OneWay}"
                PreviewAck="{Binding Intent.PreviewAck, Mode=OneWay}"
                LockAndContinueCommand="{Binding Intent.LockAndContinue}"
                GeneratePreviewCommand="{Binding Intent.GeneratePreview}"
                AcceptPreviewCommand="{Binding Intent.AcceptPreview}"
                DiscardPreviewCommand="{Binding Intent.DiscardPreview}" />

            <!-- Slot 7: Shell-bound future cards -->
            <ItemsRepeater ItemsSource="{Binding FutureCards, Mode=OneWay}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:FuturePreviewCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>

        </utu:AutoLayout>
    </ScrollViewer>
</Page>
```

Every Page is this exact template with two changes:
- Line for slot 5 references a different canvas UserControl
- Bindings in slots 4 and 6 reference a different layer's MVUX model

There is no Page-specific layout code. There is no Page code-behind logic. Pages are dumb shells over their bound canvas.

---

## Part 4 — Per-page breakdown

Nine pages. Each has one unique canvas, one set of header bindings, one set of composer bindings. Everything else is shared.

### 4.1 StackPage (index 0)

| Slot | What it renders                                                                                       |
|------|-------------------------------------------------------------------------------------------------------|
| 1    | ProgressIndicator: `01 / 09`, segment width 11.1%                                                     |
| 2    | AppTitleRow                                                                                            |
| 3    | LockedContextCards: **none** (Stack is the first layer; nothing locked before it)                    |
| 4    | ActiveLayerHeader: recap = **null** (first layer has no bridge sentence), title = "What are we building on?", subtitle = "Pattern, markup, renderer, navigation, theme, platforms. Every downstream brief references this." |
| 5    | **StackPreferencesCanvas** — 6 single-select radio rows + 1 multi-select platforms row + annotations  |
| 6    | ComposerFooter: status = "Refining" or "Listening" or "Proposing", lead question = "These defaults match canonical Uno conventions. Anything you want to change before locking?", suggestions = `["MVVM instead", "Add macOS + Linux", "Custom theme"]`, primary button label = **"Continue →"** (softened first-layer label) |
| 7    | FuturePreviewCards: 8 dimmed cards (Intent through Scaffold)                                          |

**This is the very first thing the user sees on launch.** Rails are collapsed (RailsVisible = false because activeIndex = 0 and lockedIds is empty). Center column tightened to 720px max, padding-top 64.

### 4.2 IntentPage (index 1)

| Slot | What it renders                                                                                                                                |
|------|------------------------------------------------------------------------------------------------------------------------------------------------|
| 1    | ProgressIndicator: `02 / 09`, segment 22.2%                                                                                                    |
| 2    | AppTitleRow                                                                                                                                     |
| 3    | LockedContextCards: 1 card (Stack)                                                                                                              |
| 4    | ActiveLayerHeader: recap = "Stack chosen — now let's name what we're building on it.", title = "What are we building?", subtitle = "Fill what you know. I'll infer the rest as we go." |
| 5    | **IntentCanvas** — example-values banner (if showing defaults) + 4-row field grid + annotations                                                |
| 6    | ComposerFooter: lead question = "If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?", suggestions = `["Mobile-first", "Offline-first", "No backend yet"]` |
| 7    | FuturePreviewCards: 7 dimmed cards                                                                                                              |

This is where rails first appear (animated reveal triggered by the Stack lock).

### 4.3 UXPage (index 2)

| Slot | What it renders                                                                                                          |
|------|--------------------------------------------------------------------------------------------------------------------------|
| 3    | LockedContextCards: 2 cards (Stack, Intent)                                                                              |
| 4    | recap = "We've named what we're building — now let's trace how someone uses it.", title = "How do users move through it?", subtitle = `"Five screens for the primary {entityNoun} flow. Architecture in §03 will pick the navigation primitive."` |
| 5    | **UXFlowStripCanvas** — horizontal strip of 5 screen tiles, derived from ctx.entityNoun                                  |
| 6    | lead question = "Drag-to-reorder schedule, or list-with-time-pickers? Stay on screen after dispatch, or return to dashboard?", suggestions = `["Drag-to-reorder", "Return to dashboard", "Modal confirmation"]` |

### 4.4 ArchitecturePage (index 3)

| Slot | What it renders                                                                                                                                |
|------|------------------------------------------------------------------------------------------------------------------------------------------------|
| 3    | LockedContextCards: 3 cards (Stack, Intent, UX) — but **only 2 most recent are expanded by default**; Stack collapsed to header row           |
| 4    | recap = "With the user's path mapped — let's figure out the shape underneath.", title = "How is this app shaped?", subtitle = "A blueprint of how modules connect, and the solution tree they imply." |
| 5    | **ArchitectureBlueprintCanvas** — SVG/Skia diagram with 6 modules (or 5 if offline-first, HTTP dropped), hover-to-explore, detail panel, solution tree below |
| 6    | lead question with stack-derived `{entityNoun}` and `{userNoun}` substitutions, suggestions reference `{userNoun}`                            |

The Architecture canvas exposes a `↻ Regenerate diagram` action button in its SectionHeader.Action slot. Clicking it bypasses the dirty-state check and calls `GeneratePreview` directly.

### 4.5 DesignPage (index 4)

| Slot | What it renders                                                                                                                                |
|------|------------------------------------------------------------------------------------------------------------------------------------------------|
| 3    | LockedContextCards: 4 cards — only 2 most recent (UX, Architecture) expanded; Stack and Intent collapsed                                       |
| 4    | recap = "Modules in place — let's give the surface a feel.", title = "How should it feel?", subtitle = "Tokens, type scale on real sample copy, and the ColorPaletteOverride.xaml the agent will write." |
| 5    | **DesignTokenGridCanvas** — color swatch grid + type-scale samples + mini control gallery + live ColorPaletteOverride.xaml CodeBlock           |
| 6    | lead question, suggestions = `["Stay with amber", "Use a brand color", "Show alternatives"]`                                                   |

Critical: the ColorPaletteOverride.xaml in the canvas updates **live on every token edit** — not gated by Generate preview. Only the layer-lock cycle uses preview state.

### 4.6 InteractionsPage (index 5)

| Slot | What it renders                                                                                                                                |
|------|------------------------------------------------------------------------------------------------------------------------------------------------|
| 5    | **StateTransitionDiagramCanvas** — 6 state pills in 3×2 grid, 8 hand-tuned curved arrow transitions, flow tabs in SectionHeader.Action slot, dual hover/click model, pulsing dot on active state |
| 6    | lead question, suggestions = `["Queue silently", "Always show banner", "Banner only when queue has items"]`                                    |

### 4.7 DataPage (index 6)

Standard 6-slot pattern. **DataContractGridCanvas** in slot 5 — vertical list of entity sections with field grids + a `Models/{Entity}.cs` CodeBlock below.

### 4.8 ImplementationPage (index 7)

Standard pattern. **ImplementationPhaseGridCanvas** — tabular 6-phase grid with `40,140,*,240` columns.

### 4.9 ScaffoldPage (index 8)

**This is the only Page that omits slot 6 (ComposerFooter).**

| Slot | What it renders                                                                                                                                |
|------|------------------------------------------------------------------------------------------------------------------------------------------------|
| 5    | **ScaffoldTerminalCanvas** — dark code block with `dotnet new unoapp` command + Copy button + action row with `Download bundle ↓` + `Copy prompt-context.md` + italic caption "the composition is, for now, complete." |
| 6    | **OMITTED** — no composer prompt, no suggestion chips, no preview state on Scaffold                                                            |
| 7    | FuturePreviewCards: none (Scaffold is the last layer)                                                                                          |

On ScaffoldPage, the FilesRail shows the `View all →` toggle in its header (visible only on Scaffold with ≥7 locked layers).

---

## Part 5 — The eight UX flows

Every user journey through the app reduces to one of these eight flows. Each is specified as numbered steps with explicit triggers and system responses.

### Flow 1 — First-touch (initial launch)

```
T+0     Cold launch. App.OnLaunched runs.
        ShellModel resolved from DI. Initial state:
          - ActiveIndex = 0
          - LockedIds = empty
          - LayerStates = all Clean
          - StackPrefs = STACK_DEFAULTS

T+0.1   Navigator.NavigateRouteAsync("Stack") — initial route is Stack.

T+0.2   StackPage loads in ActivePage region.
        RailsVisible evaluates false (ActiveIndex == 0 && LockedIds empty).
        CenterMaxWidth = 720, LeftRail.Width = 0, RightRail.Width = 0.
        Padding-top = 64.

T+0.3   User sees:
          - Centered StackPreferencesCanvas
          - "What are we building on?" title
          - 6 stack defaults pre-selected
          - "Continue →" primary button (softened first-layer label)
          - No rails visible

T+...   User reviews defaults (optional: toggles platforms / pattern).

T+N     User clicks "Continue →".
        ShellModel.LockAndContinue() called.
          - LayerStates[Stack] → Locked
          - LockedIds.Add(Stack) → LockedIds now {Stack}
          - LockedIds was empty before, so RevisitHintShown = true
          - ActiveIndex 0 → 1
          - Navigate to "Intent" route

T+N+0.1 RailsVisible re-evaluates: LockedIds.Count > 0 → TRUE.

T+N+0.1 RailsRevealStoryboard starts:
          - LeftRail.Width 0 → 260 over 480ms ease-out-quintic
          - RightRail.Width 0 → 340 over 480ms ease-out-quintic
          - CenterMaxWidth 720 → 880 over 480ms ease-out-quintic
          - Padding-top 64 → 32 over 480ms ease-out-quintic
          - Rail opacities 0 → 1 over 320ms ease-in-out with 160ms delay

T+N+0.5 IntentPage now loaded in ActivePage region.
        Rails fully visible.
        CompositionStack shows 9 rows (Stack locked, Intent active, 7 future).
        FilesRail shows Live file = README.md generated from intent.
```

### Flow 2 — Standard advancement (layer N → layer N+1)

```
T+0     User on LayerN. State = Clean.
        ActiveLayerHeader shows recap from RECAPS[layerN_id].
        ComposerFooter shows "Continue →" (if first layer) or "Lock and continue →".

T+...   User reads the page, optionally edits canvas or types in composer.

T+M     User clicks "Lock and continue →" (or hits Cmd/Ctrl+Enter with empty prompt).
        ShellModel.LockAndContinue() called.
          - LayerStates[LayerN] → Locked
          - LockedIds.Add(LayerN)
          - ActiveIndex N → N+1
          - Navigate to LayerN+1's route

T+M+0.1 LayerN+1 page loads.
        Slot 3 (LockedContextCards) now includes LayerN's card.
        Slot 4 shows LayerN+1's recap, title, subtitle.
        Slot 5 hosts LayerN+1's canvas.
        Slot 7 has one fewer FuturePreviewCard.

T+M+0.1 If activeIndex > 2 (i.e. at least 3 locked layers exist now):
        Auto-collapse heuristic engages.
        DefaultExpandedKinds = the 2 most recent locked layers.
        Older locked cards render in collapsed form (single header row with `+` chevron).

T+M+0.1 If activeIndex == LAYERS.length - 1 (8, the Scaffold layer):
        ComposerFooter is OMITTED from this Page.
        FilesRail shows the View all → toggle in its header.
```

### Flow 3 — Refine cycle (clean → dirty → preview → accept)

```
State: Clean

User action: Edits a canvas value (e.g., changes a Design token color)
            OR types non-empty text in the ComposerFooter textarea.
System:     ShellModel.MarkDirty(LayerKind) called.
              - Snapshot captured: { Intent, Design, StackPrefs }
              - LayerStates[layerN] → Dirty
            ActiveLayerHeader status badge appears: "Edited — preview pending" (amber).
            ComposerFooter:
              - status word: "Refining" → "Listening"
              - primary button changes from ink "Lock and continue →" to amber "Generate preview →"
              - secondary button appears: "Discard edits"
              - italic note: "I'll redraw with your edits" (or "Add an API key" if !aiConfigured)

State: Dirty

User action: Clicks "Generate preview →" or presses Cmd/Ctrl+Enter.
System:     ShellModel.GeneratePreview() called.
              - Captures the user's prompt text
              - Calls ILayerPreviewService.GeneratePreviewAsync(...)
              - On success: PreviewValues[layerN] = result.ProposedValues
                            PreviewAcks[layerN] = templated from prompt
                            LayerStates[layerN] → Previewing
              - On failure: falls through to IdentityLayerPreviewService
                            (returns current values as proposed values silently)

State: Previewing

UI changes:
  - Canvas re-renders with PROPOSED values
  - Changed fields/swatches/items show amberSoft (#FDF8EF) background tint
  - "Proposed" eyebrow appears in diff zones
  - ActiveLayerHeader status badge changes to "Preview — review and accept"
  - ComposerFooter:
    - status word: "Listening" → "Proposing"
    - lead text replaced by: "Here's how I'd redraw this layer with your edits applied..."
    - acknowledgment line appears below lead: italic Inter behind amber left rule,
      e.g. `You asked: "Use offline-first persistence" — here's what changes if I apply that.`
    - primary button: "Accept and lock →" (ink)
    - secondary button: "← Discard preview"
    - textarea + suggestion chips hidden in this state

User action: Clicks "Accept and lock →".
System:     ShellModel.AcceptPreview() called.
              - Applies PreviewValues[layerN] to the layer's canonical state
              - Clears PreviewValues[layerN], PreviewAcks[layerN]
              - LayerStates[layerN] → Locked
              - LockedIds.Add(layerN)
              - Prompts[layerN] cleared
              - ActiveIndex advances, navigate to next layer's route
```

### Flow 4 — Discard cycle (clean → dirty → discard)

```
State: Dirty (after user edits)

User action: Clicks "Discard edits".
System:     ShellModel.DiscardEdits() called.
              - Restore from snapshot: Intent, Design, StackPrefs values revert
              - Prompts[layerN] cleared
              - LayerStates[layerN] → Clean
            ActiveLayerHeader status badge disappears.
            ComposerFooter reverts to clean state (primary button, lead question).
            Canvas re-renders with restored values.

OR

State: Previewing (after user generated preview)

User action: Clicks "← Discard preview" or presses Esc.
System:     ShellModel.DiscardPreview() called.
              - Restore from snapshot (same restoration as Discard edits — preview was an
                augmentation of dirty state, both go back to last clean state)
              - PreviewValues[layerN] cleared
              - PreviewAcks[layerN] cleared
              - Prompts[layerN] cleared
              - LayerStates[layerN] → Clean
            All UI reverts.
```

### Flow 5 — Revisit a locked layer

```
T+0     User on LayerN (activeIndex = N). Some prior layers locked.

User action: Clicks "Revisit ↗" on a LockedContextCard for layerM (where M < N)
             OR clicks the layerM row in CompositionStack.

System:     ShellModel.Revisit(LayerKindM) called.
              - LayerStates[layerM] → Clean (was Locked; values preserved)
              - ActiveIndex N → M
              - Navigate to layerM's route

T+0.1   LayerM page loads.
        Slot 3 (LockedContextCards) renders only locked layers with index < M.
        Slot 4-6 bound to layerM's model.
        Slot 7 (FuturePreviewCards) renders layers with index > M.

IMPORTANT: Downstream locked layers (those between M+1 and N) STAY LOCKED.
           Their state is unchanged. The user is editing layerM in isolation.

T+...   User edits layerM. Marks dirty. Maybe generates preview. Eventually locks again.

T+K     User clicks "Lock and continue →" on layerM (now re-locked).
        ShellModel.LockAndContinue() — same as Flow 2.
          - LayerStates[layerM] → Locked
          - ActiveIndex M → M+1

T+K+0.1 User lands on layerM+1's page.
        IMPORTANT: layerM+1 may have been locked before (in the original advancement).
        If so, LayerStates[layerM+1] is still Locked. ActiveLayerHeader shows the
        Locked status. User can re-lock and advance, or revisit, or discard.

        Downstream layers (M+2, M+3, ..., N) remain locked until the user re-walks
        through them. This is by design — the user controls how far the change cascades.
```

### Flow 6 — Reset

```
User action: Clicks "Reset" button in AppTitleRow.

System:     Shows ContentDialog:
            Title: "Reset?"
            Body: "This will discard all locked layers and start over. Continue?"
            Buttons: "Reset" (primary, amber) + "Cancel" (ghost)

User action: Clicks "Reset".

System:     ShellModel.Reset() called.
              - ActiveIndex → 0
              - LockedIds → empty
              - LayerStates → all Clean
              - StackPrefs → STACK_DEFAULTS
              - Intent values → INTENT_EXAMPLE
              - Design tokens → DesignModel.Defaults
              - All snapshots cleared
              - All PreviewValues cleared
              - All PreviewAcks cleared
              - All Prompts cleared
              - All Overrides cleared
              - Navigate to "Stack" route

T+0.1   StackPage loads.
        RailsVisible re-evaluates false → reverse RailsRevealStoryboard fires.
          - LeftRail.Width 260 → 0 over 480ms
          - RightRail.Width 340 → 0 over 480ms
          - CenterMaxWidth 880 → 720, padding-top 32 → 64
          - Rail opacities 1 → 0

T+0.5   User is back at the focused first screen.
```

### Flow 7 — Files Rail interactions

```
Live file panel toggle behaviors:

User action: Clicks "Preview" button (active by default).
System:     EditingMode = false. MarkdownPreview renders the synthesized markdown
            of the active layer's file.

User action: Clicks "Edit" button.
System:     EditingMode = true. Textarea replaces preview. Content = current
            rendered markdown.

User action: Types in the textarea.
System:     Calls ComposerModel.SetOverride(layerN, newText).
            Override active indicator + "Reset" link appear.
            LayerStates[layerN] → Dirty (the layer's state machine sees the edit).

User action: Clicks "Reset" link in Override active row.
System:     ComposerModel.SetOverride(layerN, null) — clears the override.
            Content falls back to MARKDOWN_GEN[layerN](state) output.

User action: Clicks "Copy" button.
System:     Calls IClipboardService.SetTextAsync with current displayed content.
            Button shows "✓ Copied" for 1400ms.

On Scaffold layer, additional toggle:

User action: Clicks "View all →" (only visible on Scaffold with ≥7 locked layers).
System:     ViewAllMode = true. Live file panel switches to:
              - Eyebrow: "Full bundle"
              - Filename: "prompt-context.md"
              - Source: "ALL 9 LAYERS · CONCATENATED"
              - Content: all 9 layer markdowns concatenated with `<!-- filename -->` separators
              - Preview/Edit toggle hidden (read-only in this mode)

User action: Clicks "← Per-layer".
System:     ViewAllMode = false. Returns to per-layer view of currently active page.
```

### Flow 8 — Composer Footer keyboard and chip interactions

```
Textarea behaviors:

User action: Clicks in the textarea.
System:     Focus enters. Border transitions HairlineBrush → Ink2Brush over 140ms.

User action: Types first non-empty character.
System:     ComposerModel.UpdatePrompt(layerN, text) called.
            ShellModel.MarkDirty(layerN) (only on first character).
            Composer transitions Clean → Dirty.

User action: Presses Cmd+Enter (macOS) or Ctrl+Enter (Win/Linux).
System:     If layer is Dirty AND aiConfigured:
              GeneratePreview() fires.
            If layer is Clean:
              LockAndContinue() fires.
            If layer is Previewing:
              No-op.

User action: Presses plain Enter.
System:     Inserts newline (standard textarea).

User action: Presses Esc.
System:     If layer is Previewing: DiscardPreview() fires. Textarea blurs.
            Otherwise: textarea blurs (no state change).

User action: Presses Tab.
System:     Focus advances to next focusable element per WinUI tab order.

Suggestion chip behaviors:

User action: Clicks a suggestion chip.
System:     Sets textarea value to chip's text.
            Focuses textarea (caret at end).
            ShellModel.MarkDirty(layerN) (if not already Dirty/Previewing).
            Chip hover styles reset.

Generate preview button disabled state:

If aiConfigured == false AND layerState == Dirty:
  - Button visible but disabled (40% opacity, no hit-test)
  - Italic note next to button reads: "Add an API key in settings to enable previews"
  - Hover on button shows tooltip: "Generate preview requires an AI provider"
```

---

## Part 6 — Navigation triggers matrix

Every navigation in the app reduces to one of these triggers. The agent should resist any urge to add ad-hoc navigation paths beyond this matrix.

| Trigger                                                | Method called                       | ActiveIndex change | Side effects                                                                |
|--------------------------------------------------------|-------------------------------------|--------------------|----------------------------------------------------------------------------|
| Click "Continue →" (first layer)                       | `ShellModel.LockAndContinue()`      | 0 → 1              | LayerStates[Stack] → Locked, LockedIds.Add(Stack), RailsVisible flips      |
| Click "Lock and continue →" (subsequent)               | `ShellModel.LockAndContinue()`      | N → N+1            | LayerStates[N] → Locked, LockedIds.Add(N)                                  |
| Click "Accept and lock →" (after preview)              | `ShellModel.AcceptPreview()`        | N → N+1            | PreviewValues adopted, then same as LockAndContinue                        |
| Press Cmd/Ctrl+Enter with empty prompt (clean state)   | `ShellModel.LockAndContinue()`      | N → N+1            | Same as Lock and continue button                                            |
| Press Cmd/Ctrl+Enter with non-empty prompt (dirty)     | `ShellModel.GeneratePreview()`      | (no change)        | LayerStates[N] → Previewing, PreviewValues populated, PreviewAcks captured |
| Click "Revisit ↗" on a LockedContextCard               | `ShellModel.Revisit(kindM)`         | N → M (M < N)      | LayerStates[M] → Clean, values preserved, downstream layers untouched     |
| Click a Locked row in CompositionStack                 | `ShellModel.Revisit(kindM)`         | N → M              | Same as Revisit ↗                                                          |
| Click the Active row in CompositionStack               | (no-op)                             | —                  | —                                                                          |
| Click a Future row in CompositionStack                 | (no-op, not hit-testable)           | —                  | —                                                                          |
| Click "Reset" button in AppTitleRow → confirm          | `ShellModel.Reset()`                | N → 0              | All state wiped, navigate to Stack route, RailsVisible flips false         |
| Click "Reset" → cancel                                 | (no-op)                             | —                  | —                                                                          |
| Click "Download bundle ↓" on Scaffold                  | `ScaffoldModel.DownloadBundle()`    | 8 → (no change)    | Blob saved as `{AppName}-bundle.md`, LayerStates[Scaffold] → Locked        |
| Click "Copy prompt-context.md" on Scaffold             | `ScaffoldModel.CopyPromptContext()` | (no change)        | Clipboard set, no nav change                                               |

**There is no "Back" button.** There is no `Frame.GoBack()`. Navigation backwards happens only via Revisit (which is a targeted jump, not a history back). Removing the back-button assumption from the agent's mental model is often the unsticking move.

---

## Part 7 — Common agent confusions

These are the failure modes most agents fall into when translating the prototype.

### 7.1 "Rails are inside pages"

**Wrong:** Each Page's XAML has its own three-column Grid with the rails inline.

**Right:** The rails are in `Shell.xaml`. Pages render only the center column. The center column lives inside the Shell's `ActivePage` region.

**Fix:** Move all rail UserControls out of pages and into `Shell.xaml`. Pages should not have `CompositionStackRegion` or `FilesRailRegion` references in their markup.

### 7.2 "Pages call INavigator from code-behind"

**Wrong:**
```csharp
// IntentPage.xaml.cs
private async void OnContinueClick(object sender, RoutedEventArgs e)
{
    await Navigator.NavigateRouteAsync(this, "UX");
}
```

**Right:** Pages have no navigation code-behind. The composer's primary button binds to `ShellModel.LockAndContinue` (or the layer model's command that calls into the Shell). Navigation is the consequence of state changes, not a UI event handler.

```xml
<!-- ComposerFooter.xaml -->
<Button Content="{Binding PrimaryButtonLabel, Mode=OneWay}"
        Command="{Binding LockAndContinueCommand, Mode=OneWay}" />
```

```csharp
// ShellModel.cs
public async ValueTask LockAndContinue()
{
    var idx = await ActiveIndex;
    // ... mutate state ...
    if (idx < Layers.All.Length - 1)
    {
        await ActiveIndex.SetAsync(idx + 1);
        await _navigator.NavigateRouteAsync(this,
            Layers.All[idx + 1].Route,
            qualifier: Qualifiers.Nested);
    }
}
```

### 7.3 "Each Page has its own copy of locked context cards"

**Wrong:** IntentPage hardcodes a card for the Stack layer; UXPage hardcodes cards for Stack and Intent; ArchitecturePage hardcodes cards for Stack, Intent, UX; …

**Right:** Every Page has the **same** ItemsRepeater bound to `ShellModel.LockedCards`. The Shell computes the list of cards based on activeIndex and lockedIds. Pages don't know which cards are in the list — they just render whatever the binding provides.

### 7.4 "ActiveLayerHeader is hardcoded per page"

**Wrong:**
```xml
<!-- ArchitecturePage.xaml -->
<TextBlock Text="How is this app shaped?" Style="{StaticResource DisplayLargeTextStyle}" />
```

**Right:** The header binds to the layer's MVUX model, which exposes Title and Subtitle as computed feeds:

```xml
<controls:ActiveLayerHeader
    Title="{Binding Architecture.Title, Mode=OneWay}"
    Subtitle="{Binding Architecture.Subtitle, Mode=OneWay}" />
```

```csharp
// ArchitectureModel.cs
public string Title => "How is this app shaped?";
public string Subtitle => "A blueprint of how modules connect, and the solution tree they imply.";
public string? Recap => "With the user's path mapped — let's figure out the shape underneath.";
```

This keeps the static copy out of XAML where it can drift, and makes it possible to localize via `x:Uid` once that work lands.

### 7.5 "ComposerFooter is rebuilt per page"

**Wrong:** Each Page has its own ComposerFooter implementation with the lead question and chips hardcoded.

**Right:** There is **one** `ComposerFooter.xaml` UserControl. It takes 8 DependencyProperties (LayerState, LeadQuestion, Suggestions, PromptValue, IsFirstLayer, AiConfigured, PreviewAck, and a CommandPack with the four buttons' commands). Each Page wires these to its own model. The footer renders one of three layouts (Clean / Dirty / Previewing) based on LayerState.

### 7.6 "Canvas state lives in the Page's code-behind"

**Wrong:**
```csharp
// IntentPage.xaml.cs
private IntentValues _values = new("...", "...", "...", "...");
```

**Right:** Canvas state lives in the **layer's MVUX model**. The model exposes it as `IState<T>`. The Canvas UserControl binds two-way to the model's state.

```csharp
// IntentModel.cs
public IState<IntentValues> Values => State.Value(this, () => IntentModel.Example);
```

```xml
<!-- IntentCanvas.xaml -->
<TextBox Text="{Binding Values.AppType, Mode=TwoWay}" />
```

### 7.7 "Navigation routes are flat siblings"

**Wrong:**
```csharp
routes.Register(new RouteMap("Intent", ...));
routes.Register(new RouteMap("UX", ...));
// ...
```

**Right:** The 9 page routes are **nested under** the Shell route. Only the Shell route is top-level. Navigation requests use the Nested qualifier:

```csharp
routes.Register(new RouteMap("", View: views.FindByViewModel<ShellModel>(),
    Nested: new[]
    {
        new RouteMap("Stack",          View: ..., IsDefault: true),
        new RouteMap("Intent",         View: ...),
        new RouteMap("UX",             View: ...),
        // ...
    }));
```

```csharp
await _navigator.NavigateRouteAsync(this, route: "Intent",
    qualifier: Qualifiers.Nested);
```

If the agent's routes are flat siblings of Shell, the ActivePage region won't host them correctly.

### 7.8 "FilesRail reads from the active page"

**Wrong:** FilesRail has a reference to whatever Page is currently active and reaches into its model to read canvas state.

**Right:** FilesRail reads from **ShellModel** + each layer model directly via DI. The FilesRailModel is injected with IntentModel, DesignModel, ComposerModel, etc. and binds to their states. It doesn't know about pages.

### 7.9 "Locked layers' canvases are read-only views"

**Wrong:** When a user revisits a locked layer, their edits are blocked until they "unlock" it via a button.

**Right:** Clicking Revisit ↗ **transitions the state from Locked to Clean automatically**. Values are preserved. The user can immediately edit. No unlock button.

### 7.10 "Scaffold has a composer footer"

**Wrong:** ScaffoldPage renders the same 6-slot template as every other Page.

**Right:** ScaffoldPage **omits slot 6** (ComposerFooter). The scaffold layer has no prompt, no chips, no preview state. Locking happens implicitly when the user clicks "Download bundle ↓". The ScaffoldTerminalCanvas in slot 5 contains all the page's interaction.

---

## Part 8 — A minimal verification checklist

When the agent generates the Uno port, run this checklist on the result. If any item fails, the implementation has drifted from the prototype.

### Shell
- [ ] `Shell.xaml` has exactly 3 columns
- [ ] `Shell.xaml` has exactly one `uen:Region.Name="ActivePage"` element (the center column)
- [ ] `CompositionStackRegion` and `FilesRailRegion` are in `Shell.xaml`, NOT inside any Page
- [ ] Shell binds column widths to `ShellModel.LeftRailWidth` / `RightRailWidth` (computed feeds)
- [ ] Shell column-width animation uses `PowerEase EaseOut Power=5` over 480ms

### Pages
- [ ] Exactly 9 Pages exist: StackPage, IntentPage, UXPage, ArchitecturePage, DesignPage, InteractionsPage, DataPage, ImplementationPage, ScaffoldPage
- [ ] Every Page follows the 6-slot pattern (or 5-slot for ScaffoldPage)
- [ ] No Page has navigation code in its `.xaml.cs`
- [ ] Every Page binds slot 4 (header) and slot 6 (composer) to its own layer model — not to the Shell directly
- [ ] Every Page binds slot 3 (locked cards) and slot 7 (future cards) to `ShellModel.LockedCards` / `FutureCards`
- [ ] Every Page binds slot 1 (progress) to `ShellModel.ActiveIndex` and `ShellModel.LayerCount`

### Navigation
- [ ] 9 routes are nested under the Shell route (NOT flat siblings of Shell)
- [ ] Stack route has `IsDefault: true`
- [ ] Initial navigation in `Shell.Loaded` calls `NavigateRouteAsync(this, "Stack", Qualifiers.Nested)` — or relies on IsDefault
- [ ] Every nav trigger goes through a ShellModel method, never `INavigator` from code-behind

### Models
- [ ] `ShellModel` owns ActiveIndex, LockedIds, LayerStates, snapshots
- [ ] `ShellModel` has methods: LockAndContinue, GeneratePreview, AcceptPreview, DiscardPreview, DiscardEdits, Revisit, Reset
- [ ] `StackPreferencesModel`, `IntentModel`, `DesignModel` each own their own `IState<T>` for their canvas data
- [ ] `ComposerModel` owns per-layer Prompts, Overrides, PreviewAcks dictionaries
- [ ] `FilesRailModel` has access to all the layer models via DI (not via the active Page)

### Behavioral
- [ ] RailsVisible flips false → true on first lock; animation plays
- [ ] On RailsVisible flipping false (reset), reverse animation plays
- [ ] Clicking Revisit ↗ on a locked card transitions ONLY that layer to Clean; downstream layers remain Locked
- [ ] Cmd/Ctrl+Enter in composer textarea fires GeneratePreview (dirty) or LockAndContinue (clean)
- [ ] ScaffoldPage has no ComposerFooter
- [ ] FilesRail "View all →" toggle is visible only on Scaffold with ≥7 locked layers
- [ ] LockedContextCard auto-collapses all but the most recent 2 by default
- [ ] First-layer primary button reads "Continue →" instead of "Lock and continue →"

If all checkboxes pass, the structural port is faithful. Visual fidelity is then a matter of executing the `DESIGN-BRIEF-detailed.md` styling rules — but the navigation and state machine, which is what most agents fumble, will be correct.

---

## End of brief

Read this alongside the three detailed briefs. This one fills the gap between "the prototype is a single component" and "the Uno port is 9 pages with named regions" — the conceptual move that's easy to get wrong on the first attempt.
