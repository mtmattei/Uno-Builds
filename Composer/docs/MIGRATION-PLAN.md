# Migration Plan — from current `Composer` to `ComposerContextEngine`

**Date:** 2026-05-11
**Approach:** in-place migration (preserve `src/Composer/`, evolve toward `ARCHITECTURE-BRIEF-from-scratch.md`)
**Status:** proposal — ends with unresolved questions

This plan reconciles the two canonical sources:

- `docs/ARCHITECTURE-BRIEF-from-scratch.md` (3079 lines, this brief, **structural truth**)
- `docs/AUDIT-prototype-vs-current.md` (this codebase's in-flight audit, **prototype-fidelity truth**)

Per the user's reconciliation choice (*"Both apply — case-by-case"*), every brief↔audit conflict is surfaced explicitly below and either gets a phase that resolves it, or becomes an unresolved question at the bottom.

---

## 1 — State of play

### 1.1 What's in the current code

| Concern | Current | Brief target |
|---|---|---|
| Layers | 8 (Intent at 0, Stack archived) | 9 (Stack at 0) |
| Models | one `ComposerModel.cs` with everything | `ShellModel` + 9 layer models + `ComposerModel` (prompts only) + `FilesRailModel` + `CompositionStackModel` |
| Navigation | single `Shell.xaml` with `ActiveCanvas` swapping canvases inline | `Shell.xaml` with `uen:Region.Attached="True"` center, 9 nested routes |
| Pages | none (canvases hosted directly) | `Views/Pages/{Layer}Page.xaml` × 9, canonical 6-slot template |
| Canvases | `Views/Layers/{Name}.xaml` × 8 | `Views/Canvases/{Name}Canvas.xaml` × 9 |
| Header binding | single `LayerHeaderModel` record + `Header` DP (just fixed) | six DPs on `ActiveLayerHeader`: `LayerIndex`, `LayerLabel`, `LayerState`, `Recap`, `Title`, `Subtitle` |
| AI client | Refit `IAnthropicClient` | raw `HttpClient` in `ClaudeLayerPreviewService` |
| Bundle export | `IBundleExporter` | `IBundleBuilder` (+ `IClipboardService`, `IFileDownloadService`) |
| Context derivation | none — no entity-noun extraction | `IContextDeriver` with 13 regex rules → `DerivedContext` |
| Brief generation | `LayerMarkdownTemplates` static methods | `ILayerBriefGenerator` → `LayerBrief` records → `IMarkdownRenderer` |
| Namespace | `Composer.*` | `ComposerContextEngine.*` |
| Theme | local `Tokens.xaml` / `Brushes.xaml` | Material baseline + live-synthesized `ColorPaletteOverride.xaml` |
| Cold-launch surface | Intent canvas | Stack canvas |
| Reset target | Intent | Stack |

### 1.2 Brief↔audit conflicts

| # | Topic | Audit says | Brief says | Reconciliation |
|---|---|---|---|---|
| C1 | Stack layer | Archived; static defaults baked into bundle output | Restored as Layer 0 with full canvas/model | **Brief wins.** Phase M1 reverses F1. |
| C2 | Cold-launch surface | Intent canvas, no recap line | Stack canvas, recap on Intent is `"Stack chosen — now let's name what we're building on it."` | **Brief wins** — entailment of C1. |
| C3 | Reset target | Intent (implied by audit's "Intent first") | `await _navigator.NavigateRouteAsync(this, "Stack", ...)` | **Brief wins** — entailment of C1. |
| C4 | Single page vs multi-page | Audit Phase F4 *plans* navigation decomposition but defers it | Required for v1.0 | **Brief wins.** Phase M3 lands it. |
| C5 | `ActiveLayerHeader` shape | Bind to a single `LayerHeaderModel` record | Six discrete bindable DPs per the canonical Page template | **Brief wins.** Phase M2/M7. |
| C6 | Filename headers inside canvases | F3 standardized `intent.md · EDITING` etc. inside each canvas | Page-level eyebrow comes from `ActiveLayerHeader`; canvases own their own filename header *inside* the canvas body | **Both apply.** F3 work stays as the canvas-internal filename row; brief's `ActiveLayerHeader` rides above the canvas as the page-level eyebrow. No conflict in practice. |

---

## 2 — Phased migration

Each phase ends in a state where:
1. `dotnet build -f net10.0-desktop` succeeds with 0 errors
2. The desktop app launches and the user can manually walk the current flow
3. Earlier phases' verifications still pass

### Phase M1 — Restore Stack layer

**Goal:** Stack is back at index 0, app cold-launches into the Stack canvas, Intent moves to index 1.

Reverses Phase F1 from the audit. The audit's archive decision is superseded by the brief.

Files to add:
- `Models/StackPattern.cs`, `Models/MarkupKind.cs`, `Models/RendererKind.cs`, `Models/HttpClientKind.cs`, `Models/NavigationKind.cs`, `Models/ThemeKind.cs`, `Models/PlatformTarget.cs` (enums for the new `StackPreferences` record shape per brief §9.1)
- `Models/StackPreferences.cs` (updated — keep current record but align fields with brief)
- `Views/Layers/StackPreferencesCanvas.xaml(.cs)` — restored from git history if possible, else rebuilt from prototype

Files to edit:
- `Models/LayerKind.cs` — insert `Stack = 0`; renumber Intent etc.
- `Models/LayerDef.cs` (`Layers.All`) — insert Stack at index 0; update remaining indexes
- `Models/ComposerModel.cs` — restore `Stack` IState, `SetStack`, `ToggleStackPlatform`, `SuggestionChips[LayerKind.Stack]`
- `Views/Controls/ActiveCanvas.xaml.cs` — restore Stack arm of `CreateCanvas` switch
- `Views/Controls/ComposerFooter.xaml.cs` — first-layer label flips to `kind == LayerKind.Stack`
- `Views/Controls/LockedContextCard.xaml.cs` — restore Stack branch (`"MVUX, Material theme"` summary)
- `Views/Controls/CompositionStack.xaml.cs` — restore Stack summary derivation
- `Services/LayerPreviewService.cs` — restore `ParseStack` + dispatch
- `Models/LayerMarkdownTemplates.cs` — `BuildStackPreferences` keeps current signature but reads from `Stack` IState again
- `Services/IBundleExporter.cs` consumers — ensure Stack output file appears first in bundle

**Verify:** app cold-launches into Stack canvas, eyebrow reads `01 · STACK`, `Continue →` button takes the user to Intent at index 1. Reset goes back to Stack.

**Risk:** medium — touches the load-bearing layer ordering. Mitigated by `dotnet build` + manual smoke test.

---

### Phase M2 — Decompose `ComposerModel` into Shell + per-layer models

**Goal:** Match the brief's model graph (§9). One model per layer, `ShellModel` orchestrates state-machine transitions.

Files to add (under `Models/Presentation/` to match brief, or keep flat under `Models/`):
- `ShellModel.cs` — cross-cutting (`ActiveIndex`, `LockedIds`, `LayerStates`, snapshots, `MarkDirty`, `GeneratePreview`, `AcceptPreview`, `DiscardPreview`, `DiscardEdits`, `LockAndContinue`, `Revisit`, `Reset`)
- `StackPreferencesModel.cs`, `IntentModel.cs`, `UXModel.cs`, `ArchitectureModel.cs`, `DesignModel.cs`, `InteractionsModel.cs`, `DataModel.cs`, `ImplementationModel.cs`, `ScaffoldModel.cs`
- `FilesRailModel.cs`, `CompositionStackModel.cs`

Files to edit:
- `Models/ComposerModel.cs` — strip down to **prompt-only** state: `Prompts`, `Overrides`, `PreviewAcks` per brief §9.10. Move everything else to the new owner.
- `App.xaml.cs` — DI registration for the 10 new models (Singleton lifetimes per brief §4.3)
- `Shell.xaml`, `Shell.xaml.cs` — bind to `ShellModel` instead of `ComposerModel`
- Every canvas's `.xaml.cs` — read from its owning layer model (not `ComposerModel`); use `MvuxValueReader` pattern already in place
- `Views/Controls/ActiveCanvas.xaml.cs` — read from `ShellModel.ActiveLayerHeader` (transitional — gets replaced in M3)

**Verify:** app still works, all states flow through the new model graph. Generate-preview still hits the AI service. Lock-and-continue still advances.

**Risk:** high — every binding moves. Mitigated by keeping the old `ComposerModel` as a thin facade during transition, deleting only after the move is complete.

---

### Phase M3 — Region navigation + 9 Pages

**Goal:** Each layer is its own `Page`, hosted in a navigation region. `INavigator` drives advance.

Files to add:
- `Views/Pages/{Layer}Page.xaml(.cs)` × 9 — canonical 6-slot template from brief §12.1
- `Navigation/RouteMap.cs` — 9 routes nested under Shell, `Stack` is `IsDefault: true`

Files to edit:
- `Shell.xaml` — center column becomes `<Grid uen:Region.Attached="True" uen:Region.Name="ActivePage" />`; drop the embedded `ActiveCanvas`
- `Models/Presentation/ShellModel.cs` — inject `INavigator`; `AdvanceToNext` calls `NavigateRouteAsync(this, route, Qualifiers.Nested)` instead of mutating `ActiveIndex` directly
- `App.xaml.cs` — register `RouteMap.Register` via `UseNavigation`
- Drop `Views/Controls/ActiveCanvas.*` (canvases now hosted by their owning Page)

**Verify:** clicking `Lock and continue` navigates from `Stack` → `Intent` → ... → `Scaffold`. Browser back button intercept routes to `Revisit`. Reset navigates to `Stack`.

**Risk:** medium — navigation routing is well-trodden in Uno, but the 9-route setup is intricate. Test each transition.

---

### Phase M4 — New control surface

**Goal:** Add the controls the brief expects in the page template.

Files to add (`Views/Controls/`):
- `ProgressIndicator.xaml(.cs)` — hairline + amber fill + counter (currently inline in `ActiveCanvas`)
- `AppTitleRow.xaml(.cs)` — live `IntentModel.Values.Select(i => i.AppType)`; Reset button visible after first lock
- `CompositionStackRegion.xaml(.cs)` — wraps existing `CompositionStack`
- `FilesRailRegion.xaml(.cs)` — wraps existing `FilesRail`
- `FileRow.xaml(.cs)` — row inside the files rail (status glyph + filename)
- `LiveFilePanel.xaml(.cs)` — code-block view for the right rail active preview
- `MarkdownPreview.xaml(.cs)` — markdown render for preview content
- `Eyebrow.xaml(.cs)`, `MonoText.xaml(.cs)`, `SectionHeader.xaml(.cs)`, `Annotation.xaml(.cs)` (existing — verify shape matches brief), `CodeBlock.xaml(.cs)`, `BlockHandle.xaml(.cs)`

**Verify:** every page renders all 6 slots (or 5 for Scaffold). Progress hairline animates per layer change. AppTitleRow shows the live AppType.

**Risk:** low — additive control work.

---

### Phase M5 — New services

**Goal:** Match the brief's service contracts.

Files to add (`Services/`):
- `IContextDeriver.cs`, `ContextDeriver.cs` — 13 regex rules → `DerivedContext`
- `ILayerBriefGenerator.cs`, `LayerBriefGenerator.cs` — `LayerBrief` records per layer
- `IMarkdownRenderer.cs`, `MarkdownRenderer.cs`
- `IMarkdownGenerator.cs`, `StructuredMarkdownGenerator.cs` — compatibility wrapper
- `IBundleBuilder.cs`, `BundleBuilder.cs` — replaces `IBundleExporter`; adds `BuildPromptContext` + `BuildFullBundleAsync`
- `IClipboardService.cs`, `ClipboardService.cs`
- `IFileDownloadService.cs`, platform implementations
- `Models/DerivedContext.cs`, `Models/LayerBrief.cs`, `Models/SectionSpec.cs`, `Models/CodeBlockSpec.cs`, `Models/AnnotationSpec.cs`, `Models/CrossReference.cs`

Files to edit:
- Layer models (`UXModel`, `ArchitectureModel`, `InteractionsModel`, `DataModel`, `ImplementationModel`, `ScaffoldModel`) — derive `IFeed<T>` from `Intent.Values` through `IContextDeriver` (this is the load-bearing reactivity pattern from brief §11)
- `IAnthropicClient` (current Refit) → `ClaudeLayerPreviewService` (brief's raw HttpClient flavor) **— see Open question Q5**

**Verify:** changing `Intent.AppType` from `"Field-service scheduling"` to `"Habit tracker for runners"` causes UX, Architecture, Interactions, Data, Implementation, Scaffold all to re-derive with `habit` as the entity noun.

**Risk:** medium-high — new reactive feed graph. Each model needs its `IFeed<T>` to combine on the right upstream.

---

### Phase M6 — `ActiveLayerHeader` DP restructuring

**Goal:** Replace the single `Header` DP with six discrete bindable properties per brief §12.1.

Files to edit:
- `Views/Controls/ActiveLayerHeader.xaml(.cs)` — declare `LayerIndex`, `LayerLabel`, `LayerState`, `Recap`, `Title`, `Subtitle` DPs; drop `Header`
- `Models/LayerHeaderModel.cs` — delete (no longer used)
- `Models/ComposerModel.cs` (now `ShellModel`) — drop `ActiveLayerHeader` IFeed; per-layer models expose `LayerIndex`/`LayerLabel`/`Recap`/`Title`/`Subtitle` directly
- Every `{Layer}Page.xaml` — replace `Header="{Binding ActiveLayerHeader}"` with six per-DP bindings

**Verify:** recap line renders on layers > 0; cold-launch Stack page shows no recap.

**Risk:** low — narrow control surface, mechanical.

---

### Phase M7 — Project rename `Composer` → `ComposerContextEngine`

**Goal:** Match the brief's project identity. Last to ship so all the migration work isn't strewn across both namespaces.

Files to edit:
- `Composer.csproj` → `ComposerContextEngine.csproj` (rename + update `RootNamespace`, `ApplicationId`, `ApplicationTitle`)
- Every `.cs` file's namespace and `using` declarations
- `App.xaml`'s `x:Class`
- `.sln` updates

**Verify:** clean build, app launches with new identity.

**Risk:** medium — large refactor surface but mechanical. Best done with a single bulk find-replace pass and a careful build.

---

## 3 — What stays from F3

The recent canvas sweep work survives the migration:

- Lowercase filename headers inside each canvas (`intent.md · EDITING`, `blueprint.svg · LOCKED CONTEXT`, etc.) — they're the **canvas-internal** filename row, distinct from the page-level eyebrow the brief's `ActiveLayerHeader` will provide on top
- The `Annotation` (`WHY THIS … / AGENT PROMPT`) blocks under each canvas
- The `MvuxValueReader.Unwrap<T>` helper — still needed for binding wrapper unwrap
- The `ActiveLayerHeader.Header` DP bug fix (will get superseded by M6's DP split, but kept as a working baseline)
- The shortened IntentCanvas example banner

The `LayerHeaderModel` record gets dropped in M6 — that one's transitional.

---

## 4 — Unresolved questions

These need answers before kicking off Phase M1. I'll keep the audit doc as a reference but won't act on it where it conflicts with the brief.

**Q1 — Stack layer values.** The brief's `StackPreferences` record has 7 fields (`Pattern`, `Markup`, `Renderer`, `Http`, `Nav`, `Theme`, `Platforms`) with new enums. The current `StackPreferences` is a different shape (it pre-dates the audit's archival). Do we adopt the brief's exact enum names and defaults, or migrate the current shape forward?

**Q2 — Project rename timing.** Phase M7 is last. Alternative: rename first (Phase M0) so all subsequent code lands in the right namespace. The trade-off is migration-time pain vs git-history cleanliness. Preference?

**Q3 — Refit vs raw HttpClient.** Current `IAnthropicClient` is Refit-based and works. Brief's `ClaudeLayerPreviewService` uses raw `HttpClient`. Keeping Refit is less invasive; switching aligns with brief verbatim. Which wins?

**Q4 — Material baseline vs current `Tokens.xaml`.** Brief requires Material theme dictionary loaded first (`<mat:MaterialTheme />`) with `ColorPaletteOverride.xaml` live-synthesized from `DesignTokens` on top. Current code has its own `Tokens.xaml` / `Brushes.xaml` flat structure. Replacing it is a separate visual-fidelity exercise from this structural migration. Do M5 in isolation now and defer the theming work, or fold it in?

**Q5 — Companion briefs.** Brief §1 references `DESIGN-BRIEF-from-scratch.md` (visual truth) and `INTERACTION-BRIEF-from-scratch.md` (behavioral truth) as companions. Neither exists in `docs/` yet. Will those be supplied before we hit Phase M4 (which needs the visual specs) and Phase M2/M3 (which need the behavioral state-machine specs)?

**Q6 — `IFileDownloadService` for desktop.** Brief calls for platform-specific download implementations. Current `BundleExporter` uses `FileSavePicker`. Desktop net10.0-desktop has limited picker support; do we keep current and skip WASM/mobile in v1.0, or implement all-platform paths?

**Q7 — Test coverage.** Brief §18 specifies unit tests per model, service unit tests, UI tests per page, smoke tests. Current code has no test project. Add a `Composer.Tests` project as Phase M0, or defer to a later session?

**Q8 — `ComposerModel` deletion.** After Phase M2 splits state, the file is only the prompt-handling residue. The brief renames it to just `ComposerModel.cs` with prompt-only responsibility. Rename in M2 or fold into a later cleanup?
