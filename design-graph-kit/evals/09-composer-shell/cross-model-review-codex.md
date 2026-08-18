# Cross-model review — 09-composer-shell

**Reviewer:** codex (default model), read-only against the repo  ·  **Bundle:** `cross-model-review-bundle.md`

Produced to break lineage correlation: this kit's golds and checkers were all authored by one model family, so its blind spots are invisible to its own tooling. Findings below are unedited.

---

## Findings

1. **Severity:** critical  
   **Location:** `component.app-title-row`, its children, and `control.title-row.reset -> state.composer-shell.rails-hidden`; `ActiveCanvas.xaml:24-31`  
   **Claim:** AppTitleRow, project name, and Reset are modeled as operative shell content, including a Reset trigger.  
   **Reality:** The entire `AppTitleRow` is unconditionally `Visibility="Collapsed"`. The comment explicitly says the duplicate title is not needed and Reset “will resurface elsewhere.” Its children cannot be seen or invoked on this surface.  
   **Fix:** Remove `component.app-title-row`, `content.title-row.project-name`, `control.title-row.reset`, their containment/token edges, and the Reset trigger from the screen graph. Preserve the parked implementation only as a concise scope note if desired.

2. **Severity:** critical  
   **Location:** `component.layer-row -> triggers -> state.composer-shell.rails-open`; `CompositionStack.xaml.cs:90-96`, `ActiveCanvas.xaml.cs:297-307`, `ShellModel.cs:209-213`  
   **Claim:** Clicking a layer row is represented only as opening the rails.  
   **Reality:** The click invokes `Jump(row.Index)`, which changes `ActiveIndex`. That directly causes `SyncSlot()` to replace `CanvasSlot.Content` with the selected layer canvas. Opening the rails is only one effect, and jumping to index 0 does not necessarily open them.  
   **Fix:** Add `component.layer-row -> triggers -> region.canvas.slot` for the canvas swap. Retain the rails-open edge only with a condition such as `row.Index > 0 || lockedLayers.Count > 0`; otherwise it overstates behavior for the first row.

3. **Severity:** critical  
   **Location:** `control.footer.primary` trigger edges; `ComposerModel.cs:474-490`, `ComposerModel.cs:549-559`, `ShellModel.cs:191-197`, `ShellModel.cs:232-240`  
   **Claim:** Primary action effects are previewing, row locked, and file drafted.  
   **Reality:** Both lock paths also advance `ActiveIndex`; the first lock therefore makes `RailsVisible` true, changes the hosted canvas, changes the active row/file presentation, and populates future previews. The required multi-effect rule is not satisfied.  
   **Fix:** Add triggers from `control.footer.primary` to `state.composer-shell.rails-open` and the canvas-slot target. Represent the advance as a canvas-content swap; add the future-stack populated presentation if that state is introduced.

4. **Severity:** major  
   **Location:** missing trigger edges for `control.locked-card.revisit`; `LockedContextCard.xaml.cs:135-139`, `ShellModel.cs:215-220`  
   **Claim:** The Revisit control is described but has no behavioral edge.  
   **Reality:** Revisit changes `ActiveIndex` and resets the selected locked layer to `Clean`. Thus it swaps the canvas and changes the row/footer/file presentations.  
   **Fix:** Add triggers from `control.locked-card.revisit` to the canvas slot and `state.composer-footer.clean`. Also model the local row/file effects required by the multi-effect rule.

5. **Severity:** major  
   **Location:** `control.canvas.slot`; `ActiveCanvas.xaml:49-58`, `ActiveCanvas.xaml.cs:297-307`  
   **Claim:** The layer canvas slot is a `control`.  
   **Reality:** It is a non-interactive `ContentControl` used solely as a structural host for swappable content. Ontology `control` is for interactive elements; this is a meaningful content region.  
   **Fix:** Retype and rename it as `region.canvas.slot` (or `region.active-canvas.canvas-slot`) with role `contentHost`. Redirect containment and trigger edges accordingly.

6. **Severity:** major  
   **Location:** `state.locked-card.collapsed` and `control.locked-card.expand-toggle -> triggers -> state.locked-card.collapsed`; `LockedContextCard.xaml.cs:39-52`, `LockedContextCard.xaml.cs:67-78`  
   **Claim:** The disclosure has only a collapsed state, and every toggle triggers that state.  
   **Reality:** `IsExpanded` defaults to true, and the button inverts it in both directions. `ApplyExpansion` explicitly defines both visible/expanded and collapsed presentations.  
   **Fix:** Add `state.locked-card.expanded`; connect both states with `has-state`. Either emit conditional trigger effects for both directions or describe the trigger as toggling `IsExpanded`, rather than claiming it always collapses.

7. **Severity:** major  
   **Location:** `state.composer-shell.rails-hidden`, `state.composer-shell.rails-open`, and their `has-state` edges; `Shell.xaml:85-114`, `Shell.xaml.cs:91-110`  
   **Claim:** Rail visibility is a whole-screen state owned by `screen.composer-shell`.  
   **Reality:** The smallest structural owner is `region.composer-shell.workspace`: it owns the two changing column widths and contains the translated/faded rail containers. The rest of the screen is not swapped.  
   **Fix:** Attach both rail states to `region.composer-shell.workspace`, not the screen. Keep the canvas width states separately attached to `component.active-canvas`.

8. **Severity:** major  
   **Location:** token edges attached to internal leaf nodes, including `content.stack.eyebrow -> token.typography.mono-eyebrow`, `asset.progress.track -> token.color.*`, and `content.locked-card.* -> token.*`; `CompositionStack.xaml:6-17`, `ProgressIndicator.xaml:10-38`, `LockedContextCard.xaml:5-79`  
   **Claim:** Tokens are wired individually to internal content and asset parts.  
   **Reality:** These elements are internals of canonical reusable controls. The binding token rule says token use is recorded once on the canonical component, never per internal part. The source shows all cited leaves physically inside their respective `UserControl`s.  
   **Fix:** Move internal token edges to `component.composition-stack`, `component.progress-indicator`, `component.locked-context-card`, and the other canonical controls. Use `appliesTo` to preserve which internal property consumes each token.

9. **Severity:** minor  
   **Location:** `unresolved.stack.items-source`; `CompositionStack.xaml.cs:53-69`  
   **Claim:** Whether the XAML `ItemsSource` is live or vestigial is an unresolved material question.  
   **Reality:** Runtime behavior is decidable: every render assigns the projected immutable row collection directly to `LayerRows.ItemsSource`. Whether the XAML binding was intended for future use does not affect the modeled surface.  
   **Fix:** Remove this unresolved item. Record the binding as overwritten in the repeater’s implementation properties.

10. **Severity:** minor  
    **Location:** `unresolved.canvas-slot-navigation`; `ActiveCanvas.xaml:49-54`, `ActiveCanvas.xaml.cs:292-307`  
    **Claim:** Future region-based navigation is unresolved graph behavior.  
    **Reality:** The current implementation is explicit: region navigation was reverted, and direct content hosting is used. A possible future refactor is not partial knowledge about the present screen.  
    **Fix:** Remove the unresolved item; retain only a non-behavioral implementation note if useful.

11. **Severity:** minor  
    **Location:** `unresolved.hidden-canvas-slots`; `ActiveCanvas.xaml:24-31`, `ActiveLayerHeader.xaml:39-47`  
    **Claim:** The future status of hidden title/header elements is material unresolved knowledge.  
    **Reality:** Their current presentation is fully decided: they are collapsed placeholders retained so code-behind references compile. Future product intent is irrelevant to the current graph.  
    **Fix:** Remove this unresolved item and omit the hidden nodes from visible screen structure.

## Region ruling

**Yes, this gold should contain region nodes—and the pasted graph already contains eight.** The statement that it contains zero is factually inconsistent with the supplied graph.

The justified regions are:

- `region.composer-shell.workspace` — owns the meaningful three-column composition and rail-width state (`Shell.xaml:85-114`).
- `region.composer-shell.left-rail` and `region.composer-shell.right-rail` — own the rail-specific translate/opacity presentation (`Shell.xaml:92-114`).
- `region.canvas.column` — owns the centered, width-constrained scrolling content sequence (`ActiveCanvas.xaml:6-72`).
- `region.canvas.locked-stack` — owns the accumulated locked-context grouping and its dynamic population (`ActiveCanvas.xaml:44-47`; `ActiveCanvas.xaml.cs:254-279`).
- `region.canvas.slot` — should replace the incorrectly typed `control.canvas.slot`; it owns swappable layer content (`ActiveCanvas.xaml:49-58`).
- `region.canvas.future-stack` — owns the semantically distinct upcoming-layer preview sequence (`ActiveCanvas.xaml:66-70`; `ActiveCanvas.xaml.cs:206-251`).
- `region.footer.suggestions` and `region.footer.actions` — meaningful affordance groups, not arbitrary wrappers (`ComposerFooter.xaml:53-83`, `ComposerFooter.xaml:85-132`).

Removing these groups would change meaning—navigation/status rails, current content, locked context, upcoming context, suggestions, and actions—not merely alignment. No additional region should be emitted for the one-label/one-value grids or decorative wrapper panels.
