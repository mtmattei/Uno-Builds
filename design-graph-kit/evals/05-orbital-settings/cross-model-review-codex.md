# Cross-model review — 05-orbital-settings

**Reviewer:** codex (default model), read-only against the repo  ·  **Bundle:** `cross-model-review-bundle.md`

Produced to break lineage correlation: this kit's golds and checkers were all authored by one model family, so its blind spots are invisible to its own tooling. Findings below are unedited.

---

## Findings

1. **Severity:** critical  
   **Location:** `token.typography.page-title`; `Orbital/Orbital/Styles/TextBlock.xaml:21-24`  
   **Claim:** `OrbitalPageTitle` is 28px SemiBold.  
   **Reality:** The declared style sets `FontSize="20"` and `FontWeight="SemiBold"`. The 28px style is `OrbitalHeroTitle`, not `OrbitalPageTitle`.  
   **Fix:** Change `token.typography.page-title.value.size` from `28` to `20`.

2. **Severity:** critical  
   **Location:** `content.about.version`; `Orbital/Orbital/Presentation/SettingsPage.xaml:92-94`  
   **Claim:** The version content is the literal text `v0.1.0-alpha`.  
   **Reality:** The visible value is bound to `VersionDisplay`; `v0.1.0-alpha` is only its fallback. Runtime content may differ.  
   **Fix:** Remove the fixed `text` assertion. Record `binding: "VersionDisplay"` and `fallbackValue: "v0.1.0-alpha"` in properties.

3. **Severity:** critical  
   **Location:** edge `screen.settings -> has-state -> state.settings.entering`; `Orbital/Orbital/Presentation/SettingsPage.xaml.cs:13-18`  
   **Claim:** The whole settings screen owns one entering state.  
   **Reality:** Only `ProfileSection`, `AboutSection`, `PathsSection`, and `ActionsSection` are animated. The header does not change, and each card receives a distinct stagger delay. Under the attachment and multi-node condition rules, the screen is not the smallest affected node.  
   **Fix:** Replace the screen-level state with four locally attached states:

   - `component.settings-card.profile -> has-state -> state.profile.entering`
   - `component.settings-card.about -> has-state -> state.about.entering`
   - `component.settings-card.paths -> has-state -> state.paths.entering`
   - `component.settings-card.actions -> has-state -> state.actions.entering`

   Record delays `0`, `100`, `200`, and `300` milliseconds respectively.

4. **Severity:** critical  
   **Location:** `token.typography.mono-small` and its edges from all four section-title nodes; `Orbital/Orbital/Styles/TextBlock.xaml:123-137`  
   **Claim:** `OrbitalSectionHeader` and `OrbitalMonoSmall` constitute one typography token.  
   **Reality:** They are separate declared styles. `OrbitalSectionHeader` is Medium with `CharacterSpacing="80"`; `OrbitalMonoSmall` is Normal and has no such character spacing. Sharing family and size does not make their typography equivalent.  
   **Fix:** Add `token.typography.section-header` with family JetBrains Mono, size 11, weight Medium, and character spacing 80. Attach section titles to it; retain `token.typography.mono-small` for regular label, helper, version, row, and path text.

5. **Severity:** major  
   **Location:** `component.settings-card` token wiring; `Orbital/Orbital/Styles/Surfaces.xaml:5-10`  
   **Claim:** The canonical card’s consumed style is adequately represented by background, border color, radius, and inner-gap tokens.  
   **Reality:** `OrbitalCardStyle` also explicitly declares `BorderThickness="1"` and `Padding="20"`. These are reusable, screen-consumed design values but are absent from the graph. Conversely, the card style does not declare the graph’s `innerGap`; that comes from child layouts.  
   **Fix:** Add `token.border.1` and `token.spacing.20`, with `uses-token` edges from `component.settings-card` applying to `borderThickness` and `padding`. Keep inner gaps modeled separately from the surface style.

6. **Severity:** major  
   **Location:** page-level spacing/token wiring; `Orbital/Orbital/Presentation/SettingsPage.xaml:18-19`  
   **Claim:** The page-level layout is represented by a 24px section gap.  
   **Reality:** The scrolling content also declares responsive horizontal/outer padding: 16 for Narrow and 32 for Normal/Wide. The 32px consumed spacing token is entirely absent, and the existing 16px token is not connected to this page-level use.  
   **Fix:** Add `token.spacing.32`; connect the scrolling-content region to 16 and 32 with properties describing the responsive breakpoints. Do not treat these as card-inner spacing.

7. **Severity:** major  
   **Location:** `component.settings-card` token wiring; `Orbital/Orbital/Styles/Surfaces.xaml:10`  
   **Claim:** The canonical card token wiring captures the card’s reusable spacing.  
   **Reality:** The declared card padding is 20px, while `token.spacing.16` is attached as if it were canonical card styling. The 16px value belongs to child `AutoLayout` containers in three instances (`SettingsPage.xaml:24`, `69`, `148`), and the Actions instance uses 12px (`SettingsPage.xaml:185`).  
   **Fix:** Attach 20px padding to the canonical card. Model 16px as the content-layout gap for Profile/About/Paths and 12px as the Actions override, rather than describing 16px as a universal card token.

8. **Severity:** major  
   **Location:** `component.action-button`; `Orbital/Orbital/Styles/Buttons.xaml:146-154`  
   **Claim:** The canonical ghost action button’s relevant resting tokens are represented only by radius 8.  
   **Reality:** The shared style also declares transparent background, `OrbitalText55Brush`, 13px `OrbitalSansFont`, Medium weight, and base padding; the small variant overrides padding to `12,6` at lines 225-226. These are shared across all three instances and should be attached once to the canonical component.  
   **Fix:** Add canonical token usage for text-55, button typography, and small-button padding. Preserve hover/pressed values as design-system internals.

9. **Severity:** major  
   **Location:** `control.header.search`; `Orbital/Orbital/Controls/PageHeader.xaml:37-55`  
   **Claim:** The search affordance is sufficiently described by placeholder text, shortcut, surface1, surface3, and radius8.  
   **Reality:** Its reusable internal composition includes a search icon plus a distinct Ctrl+K badge. The badge consumes Surface2 and radius4; icon and badge text consume Text35; the placeholder consumes Text30. These resting, visible parts and consumed tokens are flattened away.  
   **Fix:** Keep them as canonical component properties rather than child nodes, per the internals rule: add parts such as `["searchIcon", "placeholder", "shortcutBadge"]`, plus icon glyph `E721`. Add consumed Surface2, Text35, Text30, and radius4 tokens.

10. **Severity:** minor  
    **Location:** `token.spacing.8`; `Orbital/Orbital/Presentation/SettingsPage.xaml:27`, `99`, `152`, `161`, `170`, `191`  
    **Claim:** An 8px spacing token exists in the screen graph.  
    **Reality:** It has no `uses-token` edge despite being consumed repeatedly by the profile field group, ABOUT row list, all path fields, and action-button icon/label compositions. It is an orphan token.  
    **Fix:** Attach it once to each relevant canonical concept or structural owner: profile field group, `component.info-row`, `component.path-field`, and `component.action-button`.

## Unresolved items

No defect found in the three unresolved items. The search target is outside the supplied source (`PageHeader.xaml.cs:33`), the row/field consolidation is genuinely semantic rather than factual (`SettingsPage.xaml:99-136`, `152-178`), and the ontology does not define an external-screen target for the known URI launch (`SettingsPage.xaml.cs:82-85`).

## Region ruling

**Yes. This gold should contain region nodes.** Zero regions is the wrong structural model.

Add these five regions:

- `region.header-band` — the fixed, non-scrolling top row containing `component.page-header`. The root grid separates an Auto-height header row from the remaining content at `SettingsPage.xaml:8-18`.
- `region.settings-content` — the scrolling settings-content area. `ScrollViewer` explicitly owns scrolling and contains the page’s cards at `SettingsPage.xaml:18-19`.
- `region.settings-columns` — the declared two-column composition beginning at `SettingsPage.xaml:55-61`.
- `region.about-column` — the left column, which groups ABOUT independently at `SettingsPage.xaml:63-140`.
- `region.configuration-column` — the right column, which semantically groups PATHS and ACTIONS at `SettingsPage.xaml:142-213`.

Rewire containment so the screen contains `region.header-band` and `region.settings-content`; the header band contains the page-header component; the content region contains Profile and `region.settings-columns`; the column region contains the two column regions; and those columns contain their respective cards.

These are not arbitrary panel transcripts. Removing the header/content boundary would erase fixed-header versus scrolling-content behavior. Removing the column hierarchy would erase the intentional pairing of ABOUT on the left against PATHS plus ACTIONS on the right. The smaller label/value grids remain arrangement only and should not become regions.
