# Cross-model review — 08-pens-beers

**Reviewer:** codex (default model), read-only against the repo  ·  **Bundle:** `cross-model-review-bundle.md`

Produced to break lineage correlation: this kit's golds and checkers were all authored by one model family, so its blind spots are invisible to its own tooling. Findings below are unedited.

---

## Findings

1. **Severity:** critical  
   **Location:** `unresolved.tab-targets`; `Pens/Pens/Presentation/Shell.xaml.cs:13`  
   **Claim:** Tab destinations are unresolved because no route or selection handler is provided.  
   **Reality:** `_pageFactories` explicitly maps every tag to its page at lines 13–20. `OnTabSelectionChanged` reads the selected item’s tag and calls `NavigateToTab` at lines 28–36; that method installs the resulting page in `NavigationContent` at lines 40–47.  
   **Fix:** Delete `unresolved.tab-targets`. Add five `navigates-to` edges:

   - Schedule tab → Schedule screen
   - Chat tab → Chat screen
   - Beers tab → Beers screen
   - Duties tab → Duties screen
   - Roster tab → Roster screen

   Add minimal target screen nodes if external screens may appear as referenced graph nodes.

2. **Severity:** critical  
   **Location:** `unresolved.loading-error-state`; `Pens/Pens/Presentation/BeersPage.xaml:1`  
   **Claim:** Loading or error feedback might be “surfaced elsewhere.”  
   **Reality:** The complete `BeersPage` visual tree runs through line 221 and contains no binding to `IsLoading`, `HasError`, or `ErrorMessage`. `BeersPage.xaml.cs:3`–`8` adds no programmatic presentation. Within the supplied screen implementation, those properties are not rendered.  
   **Fix:** Delete this unresolved item. Do not emit loading/error state nodes; record, if desired outside the design graph, that the ViewModel conditions have no presentation on this screen.  
   **Uncertainty:** None within the supplied source set.

3. **Severity:** critical  
   **Location:** edge `component.case-tile → triggers → state.case-tile.consumed`; `Pens/Pens/Presentation/BeersPage.xaml:71`  
   **Claim:** The canonical case-tile component is the declared trigger source.  
   **Reality:** The command is attached to the `ItemsRepeater` at lines 71–72, not to the `Border` inside its item template at lines 81–85. The edge’s rationale also says recomputing `ConsumedCases` changes “every tile’s `IsConsumed`,” but the tile presentation is independently bound to each item’s `IsConsumed` at lines 83–84. The cited structure does not establish that all tiles change.  
   **Fix:** Change the trigger source to `control.tracker.case-grid`, or introduce an interactive case-item control only if the command-extension semantics establish item-level invocation. Rewrite the rationale to say the command toggles the selected `CaseBlock`; do not claim every tile’s state changes.  
   **Uncertainty:** The missing `BeersViewModel.cs` listing prevents independently confirming the exact mutation performed by `ToggleCaseCommand`; the current edge is unsupported even without that file.

4. **Severity:** major  
   **Location:** `component.card` and edge `component.stat-card → instance-of → component.card`; `Pens/Pens/Presentation/BeersPage.xaml:49`  
   **Claim:** The season tracker and stat cards instantiate one reusable generic Card component.  
   **Reality:** The tracker uses `LargeCornerRadius`, `SubtleBorderBrush`, 20 padding, and tracker-specific content at lines 49–89. Stat cards use `CardCornerRadius`, `CardBorderBrush`, 16 padding, and a repeated value/caption structure, exemplified at lines 133–150. Their shared facts are principally being dark bordered rectangles—the exact sort of visual coincidence the consolidation rule says is insufficient. No canonical Card is declared.  
   **Fix:** Remove `component.card` and both `instance-of` edges targeting it. Keep `component.card.season-tracker` as a standalone component and `component.stat-card` as the canonical component for the four genuinely repeated stat cards. Move the relevant background/border token edges onto those two canonical components.

5. **Severity:** major  
   **Location:** `content.summary.total-beers`; `Pens/Pens/Presentation/BeersPage.xaml:41`  
   **Claim:** The content node’s text is the fixed string `780 beers (30 per case)`.  
   **Reality:** The number is dynamically bound to `TotalBeers` at line 42. Only `" beers "` and `"(30 per case)"` are literal at lines 43–44. The supplied XAML does not establish `780` as the stable content. The same problem appears in `content.tracker.counter` (`26 / 52 cases`), whose leading value is bound at lines 64–66.  
   **Fix:** Represent these as dynamic text compositions, for example `"{TotalBeers} beers (30 per case)"` and `"{ConsumedCases} / 52 cases"`, with the binding in properties. Do not encode sample runtime values as observed text.

6. **Severity:** major  
   **Location:** token edges for `content.summary.cases-label` and `content.summary.total-beers`; `Pens/Pens/Presentation/BeersPage.xaml:21`  
   **Claim:** The graph captures the design tokens consumed by the modeled summary content.  
   **Reality:** The cases label consumes `BarlowMedium` at line 24, but has no edge to `token.font.body-medium`. The total-beers line consumes neon amber and muted text brushes at lines 42–44, but has neither color edge. These are direct consumed-resource omissions, not speculative token extraction.  
   **Fix:** Add:

   - `content.summary.cases-label → uses-token → token.font.body-medium`
   - `content.summary.total-beers → uses-token → token.color.neon-amber`
   - `content.summary.total-beers → uses-token → token.color.text-muted`

7. **Severity:** major  
   **Location:** `component.legend-swatch`; `Pens/Pens/Presentation/BeersPage.xaml:95`  
   **Claim:** The legend-swatch canonical captures the common internals, while instances carry their differing fills.  
   **Reality:** Both legend entries use `SmallCornerRadius` and muted captions at lines 95–115, but the canonical has no edges to `token.radius.small` or `token.color.text-muted`. The remaining instance also directly consumes `SubtleBorderBrush` at line 99, which is omitted.  
   **Fix:** Add the common radius and muted-text token edges to `component.legend-swatch`; add a `token.color.border-subtle` override edge to `component.legend-swatch.remaining`.

8. **Severity:** minor  
   **Location:** open-question premise versus `region.header`, `region.tab-bar`, `region.beer-summary`, `region.legend`, and `region.stats-grid`; `Pens/Pens/Presentation/Shell.xaml:14`  
   **Claim:** “This gold contains zero” region nodes.  
   **Reality:** The supplied graph contains five region nodes. The source also plainly declares a fixed header band at `Shell.xaml:14`–`50`, a hosted content band at lines 52–56, and fixed bottom navigation at lines 58–158.  
   **Fix:** Correct the review metadata/premise before using this graph for region-count comparisons.

## Region ruling

**Yes, the gold should contain region nodes.** It already correctly reaches for them, but it is missing the structural content region that makes the shell hierarchy coherent.

Add `region.navigation-content` (or `region.main-content`) based on `Pens/Pens/Presentation/Shell.xaml:52`–`56`. Reparent the page-owned areas beneath it:

- `region.beer-summary`
- `component.card.season-tracker`
- `region.legend`
- `region.stats-grid`

Keep `region.header` and `region.tab-bar` as direct shell-level children. Keep the summary, legend, and stats grid as regions: each groups a distinct semantic concept, not merely incidental alignment. Do not add regions for the summary’s horizontal number/label row (`BeersPage.xaml:16`), tracker header grid (`BeersPage.xaml:56`), individual stat-card stack panels (`BeersPage.xaml:140`), or spacer border (`BeersPage.xaml:217`); removing those changes arrangement only.

I would not create a second nested “scroll region” in addition to `region.navigation-content`. Although the `ScrollViewer` at `BeersPage.xaml:9` owns scrolling state, modeling both it and the shell content host would duplicate the same main-content boundary at this graph’s altitude.
