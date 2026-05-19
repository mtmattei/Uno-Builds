# Audit — Prototype vs Current Implementation

**Status:** decisions locked, fix in progress
**Date:** 2026-05-11
**Canonical source of truth:** `C:\Users\Platform006\Downloads\composer-context-engine.jsx` (the React prototype, 2815 lines)

This audit reconciles the Uno port with the prototype after multiple sessions
of layered briefs caused drift. The most recent decision: the **prototype is
canonical**. Briefs that contradict the prototype get archived; briefs that
elaborate on it stay.

---

## 1 — Canonical decisions

### 1.1 Layer order (8 layers, Intent first)

From `LAYERS` in `composer-context-engine.jsx` lines 62–71:

| Index | Id         | Label           | File                      | Hint                            |
|-------|------------|-----------------|---------------------------|---------------------------------|
| 0     | `intent`   | Intent          | `README.md`               | what the app is for             |
| 1     | `ux`       | UX              | `ux-flows.md`             | how users move through it       |
| 2     | `arch`     | Architecture    | `architecture.md`         | how it is shaped                |
| 3     | `design`   | Design System   | `design-system.md`        | how it feels                    |
| 4     | `interact` | Interactions    | `interaction-spec.md`     | every state of every flow       |
| 5     | `data`     | Data            | `data-contracts.md`       | shapes and contracts            |
| 6     | `impl`     | Implementation  | `implementation-plan.md`  | phased build plan               |
| 7     | `scaffold` | Scaffold        | `scaffold.command`        | runnable starting point         |

There is **no Stack layer in the prototype**. The Stack-preferences concept
introduced by `ENGINEERING-BRIEF-01-stack-preferences.md` is archived — its
goals (MVUX, Material, Skia, Region, Kiota, four-platform default) become
**static bundle defaults** baked into `LayerMarkdownTemplates.BuildStackPreferences`
output, not a user-facing canvas. `ENGINEERING-BRIEF-page-and-flow-breakdown.md`
references Stack at index 0 — that part of the brief is superseded by this audit.

### 1.2 Cold-launch surface (per prototype lines 2763–2793)

What the user sees on first paint:

1. **ProgressIndicator** — hairline track + amber fill at `1/8 ≈ 12.5%`; eyebrow
   "Intent" left, mono `01 / 08` right.
2. **App title row** — `<Body size={18} weight={500}>{intent.appType || 'Untitled'}</Body>`.
   With default intent, this reads **"Field-service scheduling"**. No Reset
   button until at least one layer is locked.
3. **No locked cards** (none locked yet).
4. **ActiveLayerHeader** — eyebrow `01 · INTENT`, title "What are we building?",
   subtitle "Fill what you know. I'll infer the rest as we go." **No recap line**
   (Intent is the first layer; `RECAPS.intent === null`).
5. **IntentCanvas** — filename header `intent.md · EDITING`, example-values
   banner ("Example values · Clear all"), 4 field rows (App type / Primary user
   / Workflow / Platforms) pre-filled with `INTENT_EXAMPLE`, two annotations
   ("Why this intent" + "Agent prompt").
6. **ComposerFooter** — eyebrow `COMPOSER · REFINING`, lead question ("If I
   summarize the intent right now…"), suggestion chips (Mobile-first /
   Offline-first / No backend yet), textarea, **"Continue →"** primary button
   (the first-layer softened label).
7. **No future cards** while rails are hidden.

Layout: center column max-width **680** (prototype line 2761), padding
**`64px 48px 80px`**, `justifyContent: center`. Rails width zero. Background `#ffffff`.

### 1.3 Rails reveal condition (prototype line 2562)

```js
const railsVisible = lockedIds.size > 0 || activeIndex > 0;
```

Flips on first lock OR direct nav forward. When true:
- CompositionStack appears at left, width **260**, sticky top 32.
- FilesRail appears at right, width **340** (per file rail spec; see prototype).
- Center column expands to max-width 880, padding `32px 48px 80px`,
  `justifyContent: flex-start`.

### 1.4 Cold-launch app title

The prototype always shows the project name above the canvas. On cold-launch
this reads **"Field-service scheduling"** because `intent.appType` defaults to
that. As the user edits Intent, the title updates live. This is **derived
from Intent.AppType, not a separate input**. Today's implementation has
`AppName` as a separate state field (from the deleted hero) — it should be
removed and replaced with a feed off `Intent.AppType`.

---

## 2 — Gaps between current implementation and prototype

### 2.1 Layer enum and ordering (load-bearing)

| File                              | Current                               | Should be                              |
|-----------------------------------|---------------------------------------|----------------------------------------|
| `Models/LayerKind.cs`             | `Stack, Intent, UX, …` (9 values)     | `Intent, UX, Architecture, DesignSystem, Interactions, Data, Implementation, Scaffold` (8) |
| `Models/LayerDef.cs`              | `Layers.All` starts with Stack at 0   | `Layers.All` starts with Intent at 0   |
| `Models/StackPreferences.cs`      | Public `IState<StackPreferences>` plus `SetStack` / `ToggleStackPlatform` commands | Internal default value used only by `LayerMarkdownTemplates.BuildStackPreferences` |
| `Views/Layers/StackPreferencesCanvas.xaml(.cs)` | Hosted by ActiveCanvas at index 0 | Deleted; no canvas mounts for layer 0 |
| `Views/Controls/ActiveCanvas.xaml.cs` | `CreateCanvas` switch maps Stack → StackPreferencesCanvas | Stack arm removed; switch starts at Intent |
| `Views/Controls/ComposerFooter.xaml.cs` | First-layer label check tests Stack OR Intent | Only Intent triggers `"Continue →"` |
| `Models/ComposerModel.cs`         | `SuggestionChips[LayerKind.Stack]`     | Entry removed                          |
| `Services/LayerPreviewService.cs` | `ParseStack` + dispatch arm           | Removed                                |
| `Views/Controls/LockedContextCard.xaml.cs` | Stack-specific render branch | Removed                       |
| `Models/LayerMarkdownTemplates.cs` | `BuildStackPreferences(StackPreferences)` reads from user state | Same function, called with `StackPreferences.Default` |

### 2.2 Removed hero leftovers that should fold into Intent

The hero deletion (Phase A this session) dropped the UI but kept orphan state
on `ComposerModel`:

- `AppName` IState — replace with `Intent.Select(i => i.AppType)` feed
- `AppDescription` IState — fold into `Intent.Notes` or drop
- `Runtime` IState — drop (stack defaults to .NET 10)
- `IsXxxSelected` IStates per platform — drop (Stack defaults cover platforms)
- `ReferenceScreenshotCount` / `ReferenceScreenshotPaths` — keep, surface as
  a reference-images section on either Intent or Design canvas later

### 2.3 Visual fidelity gaps (less critical, defer)

These exist but are smaller than the structural gaps above. Audit per-canvas
in a follow-up session once the structural changes have settled:

- IntentCanvas: the prototype uses a tighter 14px input with no border (just
  the underlying hairline row separator). Our IntentCard.xaml is close but
  the field grid template column is `120px 1fr 80px` exact, and our spacing
  needs a side-by-side check against the prototype's `padding: '12px 0'`.
- ProgressIndicator: prototype is **1px** height hairline + amber fill +
  10px gap to the eyebrow/counter row below. Confirm ours matches.
- AppTitleRow: prototype renders the live `intent.appType`, not the deleted
  hero `AppName`. Reset button only appears once locked > 0.
- LockedContextCard: prototype auto-collapses all but the most-recent-2 via
  the `lockedCards` useMemo (lines 2653–2680). Our `DefaultExpandedKinds`
  feed should be reused but the per-layer summary/contents derivations need
  to match the prototype's table.
- FuturePreviewCards: only render when `railsVisible`. Prototype
  conditional at line 2790.

---

## 3 — Phased fix plan

**Phase F1 — Strip Stack from the layer system (this session)**

Files to edit:
- `Models/LayerKind.cs` — remove `Stack` enum value
- `Models/LayerDef.cs` — drop `Stack` entry from `Layers.All`; Intent moves to index 0
- `Models/ComposerModel.cs` — drop `SetStack`, `ToggleStackPlatform`, `SuggestionChips[Stack]`; keep `Stack` IState only as an immutable default (or convert to a hardcoded constant inside `BuildBundleFilesPlaceholder`)
- `Models/LayerSnapshot.cs` — keep the `Stack` slot for now (it's a value-type holder); no canvas writes to it
- `Views/Controls/ActiveCanvas.xaml.cs` — remove the `Stack` arm of `CreateCanvas`
- `Views/Controls/ComposerFooter.xaml.cs` — first-layer-label check: `kind == LayerKind.Intent`
- `Views/Controls/LockedContextCard.xaml.cs` — remove `Stack` branch
- `Views/Controls/CompositionStack.xaml.cs` — remove `Stack` summary derivation
- `Services/LayerPreviewService.cs` — remove `ParseStack` + dispatch
- `Models/LayerMarkdownTemplates.cs` — `BuildStackPreferences` keeps existing signature; called from `BuildBundleFilesPlaceholder` with `StackPreferences.Default` once `Stack` IState is removed (or with `await Stack` if we keep the IState)

Files to delete:
- `Views/Layers/StackPreferencesCanvas.xaml`
- `Views/Layers/StackPreferencesCanvas.xaml.cs`

Build verification: `dotnet build` clean, app boots into Intent canvas
with eyebrow `01 · INTENT`, "Continue →" button on the composer.

**Phase F2 — Wire AppTitleRow off Intent.AppType (next session)**

Files to edit:
- `Models/ComposerModel.cs` — drop `AppName` IState; add `ProjectName` feed
  derived from `Intent.AppType`
- `Shell.xaml` — add AppTitleRow above WorkspaceRoot (or inside the center
  column above the canvas) bound to `ProjectName`
- Drop `Runtime`, `IsXxxSelected`, `AppDescription` states once their
  dependents are rewired

**Phase F3 — Per-canvas visual audit (next sessions, one canvas at a time)**

For each layer canvas: side-by-side compare to the prototype's component,
fix typography weights, spacing, hairline rules, hover affordances, eyebrow
copy. Use this audit doc as the dispatcher.

**Phase F4 — Shell + 9-page navigation decomposition (after F3)**

Eventually apply `ENGINEERING-BRIEF-page-and-flow-breakdown.md` — but with
8 routes nested under Shell, not 9. Add the `uen:Region.Attached`
navigation region. Decompose ComposerModel into ShellModel + 8 per-layer
models. This is a multi-session effort.

---

## 4 — What stays from prior briefs

- `ARCHITECTURE-BRIEF-detailed.md` — modules table, hover panel, regenerate
  affordance: keep all of this for the Architecture canvas
- `DESIGN-BRIEF-detailed.md` — color tokens, type scale, spacing rhythm,
  ColorPaletteOverride.xaml output: keep
- `INTERACTION-BRIEF-detailed.md` — 3 × 6 state matrix, pulsing dot,
  hover/active decoupling: keep
- `ENGINEERING-BRIEF-02-structured-layer-brief-generators.md` — per-layer
  markdown templates: keep
- `ENGINEERING-BRIEF-page-and-flow-breakdown.md` — Shell + nested pages
  topology: keep, except adjust to **8** routes not 9
- `ENGINEERING-BRIEF-01-stack-preferences.md` — **archived**; replaced by
  static defaults in `LayerMarkdownTemplates.BuildStackPreferences`
