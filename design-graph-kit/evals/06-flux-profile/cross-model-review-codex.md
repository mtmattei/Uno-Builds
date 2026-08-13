# Cross-model review — 06-flux-profile

**Reviewer:** codex (default model), read-only against the repo  ·  **Bundle:** `cross-model-review-bundle.md`

Produced to break lineage correlation: this kit's golds and checkers were all authored by one model family, so its blind spots are invisible to its own tooling. Findings below are unedited.

---

1. **Severity:** critical  
   **Location:** missing behavioral edge from `control.opus.update`; `ProfileModel.cs:39-47`  
   **Claim:** Update Balance only triggers `state.opus.refreshing`.  
   **Reality:** `UpdateBalance` has two declared effects: it changes `IsRefreshing` and writes a new value to `OpusBalance` at line 47. The graph records only the loading effect, contrary to the multi-effect rule.  
   **Fix:** Add `control.opus.update -> triggers -> content.opus.balance-value`, with rationale that `UpdateBalance` updates the bound `OpusBalance` value.

2. **Severity:** major  
   **Location:** `component.card -> uses-token -> token.spacing.16`; `ProfilePage.xaml:40,98,172`; `FluxStyles.xaml:116-122`  
   **Claim:** The canonical glass card consumes spacing 16 as an `innerGap`, supported by the design-system style.  
   **Reality:** `FluxGlassPanelStyle` does not declare an inner gap. It declares `Padding="24"` at line 121. Spacing 16 comes from three page-local child `StackPanel` declarations, not from the cited style.  
   **Fix:** Change this edge’s evidence to derived XAML evidence citing the three instance declarations. Also add `component.card -> uses-token -> token.spacing.24` with `appliesTo: padding`, citing `FluxStyles.xaml:121`.

3. **Severity:** major  
   **Location:** `content.settings.api-helper -> uses-token -> token.typography.body`; `ProfilePage.xaml:184-187`  
   **Claim:** The API helper consumes the 14px `FluxBody` typography token.  
   **Reality:** The element applies `FluxBody` but locally overrides `FontSize` to 12. The graph’s typography token has `size: 14`, so that token does not accurately describe the rendered typography.  
   **Fix:** Remove the `token.typography.body` font edge from this node. Introduce a derived 12px helper/caption typography token if the repeated 12px treatment is being modeled.

4. **Severity:** major  
   **Location:** `content.settings.alerts-helper -> uses-token -> token.typography.body`; `ProfilePage.xaml:217-220`  
   **Claim:** The alerts helper consumes the 14px body typography token.  
   **Reality:** Its local `FontSize="12"` overrides the 14px size supplied by `FluxBody`.  
   **Fix:** Remove the body-typography edge and connect it to the same derived 12px helper/caption token proposed for the API helper.

5. **Severity:** major  
   **Location:** missing token-consumption edges involving `token.color.text-secondary`; `FluxStyles.xaml:95-98`; representative consumers at `ProfilePage.xaml:33-34,65-66,71-73,232-237`  
   **Claim:** The graph includes `token.color.text-secondary` and says it represents body text, but models no consumer of it.  
   **Reality:** Every non-overridden `FluxBody` use inherits `FluxTextSecondaryBrush` through the style. The graph records typography consumption for these nodes while omitting the equally declared foreground consumption.  
   **Fix:** Add `uses-token` edges with `appliesTo: foreground` from the relevant `FluxBody` content concepts to `token.color.text-secondary`. For the 12px helpers, retain this color edge even after replacing their typography edge.

6. **Severity:** major  
   **Location:** missing token-consumption edges involving `token.color.text-primary` and `token.color.text-muted`; `FluxStyles.xaml:73-77,101-105,108-113`; consumers at `ProfilePage.xaml:31-32,99-100,117-120,145-148,173-174`  
   **Claim:** The graph models the typography styles but largely omits their declared foreground tokens.  
   **Reality:** `FluxHeadingLarge` consumes text-primary, `FluxBodyBold` consumes text-primary, and `FluxMicro` consumes text-muted. Only unrelated or partial color edges are present.  
   **Fix:** Add foreground `uses-token` edges for the title, canonical route-item name part, and canonical section-header concepts. If the three section-title nodes remain separate, connect each to text-muted.

7. **Severity:** major  
   **Location:** missing unresolved item for `component.route-item.home-work` and `component.route-item.downtown-loop`; `ProfilePage.xaml:103-128,131-156`  
   **Claim:** The route rows are modeled as list items with a chevron, without documenting any behavioral uncertainty.  
   **Reality:** Both rows visually advertise disclosure/navigation through `E76C` chevrons, but they are plain `Border` elements with no command, click handler, gesture recognizer, or interactive wrapper. Their intended behavior is as unsupported as the Add New Route behavior.  
   **Fix:** Add an unresolved item such as `unresolved.routes.item-behavior`, related to the canonical route item or both instances, stating that the chevron suggests disclosure but no interaction is declared.  
   **Uncertainty:** I am certain the source declares no interaction. Whether visual intent must always become `unresolved` is an ontology-policy choice, but omitting it here is inconsistent with the gold’s treatment of the adjacent Add button.

8. **Severity:** minor  
   **Location:** `screen.profile -> uses-token -> token.spacing.24`; `ProfilePage.xaml:11-15`; `FluxStyles.xaml:56-60`  
   **Claim:** The edge is a declared design-system consumption.  
   **Reality:** The page uses the literal `Spacing="24"`; it does not reference `FluxSpacingL`. Equality to the declared token is derived, not declared.  
   **Fix:** Change the edge evidence kind to `derived`, cite both `ProfilePage.xaml:13` and `FluxStyles.xaml:60`, and state that the literal equals the declared spacing token.

9. **Severity:** minor  
   **Location:** `component.route-item -> uses-token -> token.spacing.4`; `ProfilePage.xaml:116,144`; `FluxStyles.xaml:57`  
   **Claim:** The route component’s spacing-token consumption is declared by the design system.  
   **Reality:** Both rows use the literal `Spacing="4"`. Neither references `FluxSpacingXS`; the token equivalence is derived.  
   **Fix:** Change the edge evidence to `derived` and cite the two XAML uses plus the resource declaration.

10. **Severity:** major  
    **Location:** region hierarchy around `screen.profile`; `ProfilePage.xaml:10-15,240-241`  
    **Claim:** Header, cards, and footer are direct screen children; the graph allegedly has “zero” regions.  
    **Reality:** The JSON already contains two regions—`region.profile.header` and `region.profile.footer`—so the stated zero-region premise is false. More importantly, the entire visible surface is owned by a `ScrollViewer` containing a safe-area-aware, width-constrained content column. Removing that grouping changes scrolling, viewport safety, and the page’s content-surface behavior, not merely placement.  
    **Fix:** Add `region.profile.scroll-content`. Change the hierarchy to `screen.profile -> contains -> region.profile.scroll-content`, then place the header, three card instances, and footer beneath it.

11. **Severity:** major  
    **Location:** state ownership at `component.card.opus -> has-state -> state.opus.refreshing`; `ProfilePage.xaml:77-92`  
    **Claim:** The entire OPUS card owns the refreshing state.  
    **Reality:** Only the action/status grid swaps presentation: the update button and progress row occupy the same grid and have inverse visibility conditions. The card visual, balance summary, and section heading do not swap. The ontology requires attachment to the smallest presentation-owning node.  
    **Fix:** Add `region.opus.refresh-action` for the grid at lines 77-92, contain the update button there, and move `has-state` from `component.card.opus` to this region. Keep the loading-state children under `state.opus.refreshing`.

12. **Severity:** major  
    **Location:** missing `region.opus.summary`; `ProfilePage.xaml:44-75`  
    **Claim:** The OPUS visual, balance label, balance value, and refresh hint are flattened as independent children of the whole card.  
    **Reality:** The source explicitly groups the physical OPUS-card representation and its associated balance information into one horizontal summary. That pairing carries meaning: it identifies which account/card the displayed balance belongs to. Removing it loses semantic association, not merely arrangement.  
    **Fix:** Add `region.opus.summary`, contained by `component.card.opus`; place `component.opus-card`, the balance label/value, and refresh hint beneath it.

**Region ruling:** Yes. The gold should contain region nodes. Retain the existing header and footer, and add `region.profile.scroll-content`, `region.opus.summary`, and `region.opus.refresh-action`. Do not promote the language, alerts, API-field, or route-row layout panels to regions; those are field/item internals whose structure is already adequately represented by their parent concepts.
