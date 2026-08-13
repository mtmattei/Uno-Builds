# Cross-model review — 07-caffe-main

**Reviewer:** codex (default model), read-only against the repo  ·  **Bundle:** `cross-model-review-bundle.md`

Produced to break lineage correlation: this kit's golds and checkers were all authored by one model family, so its blind spots are invisible to its own tooling. Findings below are unedited.

---

1. **Severity:** critical  
   **Location:** missing edges from `component.espresso-card`; `Caffe/Caffe/MainPage.xaml:109-123`, `Caffe/Caffe/ViewModels/MainViewModel.cs:19-23`, `Caffe/Caffe/MainPage.xaml.cs:26-34`  
   **Claim:** Selecting an espresso triggers only `state.espresso-card.selected`.  
   **Reality:** The same selection changes at least four local presentations: the selected card visual, `SelectionOverview` visibility, brew-button enablement, and brew-button text. The graph already models the first three states, but records only one trigger effect. This violates the ontology’s multi-effect rule.  
   **Fix:** Add trigger edges from `component.espresso-card` to the overview and brew-button presentation effects. Prefer positive local states—`state.selection-overview.visible` and `state.brew-button.enabled`—and add `state.brew-button.selection-label` for the `"Brew {SelectedEspresso.Name}"` text change. Replace or complement the negatively named hidden/disabled states accordingly.

2. **Severity:** major  
   **Location:** `state.brew-button.disabled`, `state.selection-overview.hidden`; `Caffe/Caffe/MainPage.xaml:109-123`, `Caffe/Caffe/ViewModels/MainViewModel.cs:46-50`  
   **Claim:** `HasSelection` declares the local “overview hidden” and “brew disabled” presentations.  
   **Reality:** `HasSelection == true` produces the opposite presentations: overview visible, brew enabled, and a selection-specific label. The graph names states for the false/default condition but records the positive `HasSelection` member without its polarity. That makes the semantics machine-ambiguous and loses the label transition entirely.  
   **Fix:** Rename these to `state.selection-overview.visible` and `state.brew-button.enabled`, or add an explicit condition such as `"when": "HasSelection == false"` to the existing states. Add the missing brew-label state.

3. **Severity:** major  
   **Location:** `component.caffe-header -> contains -> asset.header.accent-bar`, `content.header.logo`, and `content.header.tagline`; `Caffe/Caffe/Controls/:299-330`  
   **Claim:** The header’s canonical internals are both listed in `properties.parts` and emitted as child nodes.  
   **Reality:** These are ordinary internals of one declared reusable `UserControl`. The ontology’s component-internals rule says canonical parts are recorded once as properties, not also expanded into children. The footer correctly remains a single component despite having the same kind of internal accent bar.  
   **Fix:** Remove the three header child nodes and their `contains` edges. Retain `["accent-bar", "logo", "tagline"]` in `component.caffe-header.properties.parts`, and move their token usage to the canonical header component with suitable `appliesTo` values.

4. **Severity:** critical  
   **Location:** missing tokens and `uses-token` edges for `component.brewing-screen` and `component.temperature-gauge`; `Caffe/Caffe/Controls/:124-133`, `Caffe/Caffe/Controls/:1160-1173`, `Caffe/Caffe/Styles/AppResources.xaml:30-34`  
   **Claim:** The graph’s token set represents audited resource consumption by the modeled controls.  
   **Reality:** It omits four explicitly declared and directly consumed gradient-color resources: `CoffeeDarkColor`, `CoffeeLightColor`, `CaffeTemperatureHighColor`, and `CaffeTemperatureLowColor`. These are resting/runtime visuals of the modeled surface, not interaction-only style variants.  
   **Fix:** Add four color-token nodes and connect the coffee colors to `component.brewing-screen` and the temperature colors to `component.temperature-gauge`.

5. **Severity:** major  
   **Location:** `token.typography.button`, `token.typography.grind-hint`, `token.typography.overview-label`, `token.typography.overview-value`, `token.typography.body`, `token.typography.brewing-title`, `token.typography.arc-label`; `Caffe/Caffe/Styles/AppResources.xaml:88-133`  
   **Claim:** These nodes describe their declared typography styles.  
   **Reality:** Several discard declared properties, and one is semantically wrong:

   - `overview-value` says `"family": "DM Sans"`, but the style uses `CormorantRegular` at 18 px (`lines 119-124`).
   - `button` omits Medium weight/family and 15 px (`lines 95-100`).
   - `grind-hint` omits 11 px and italic (`lines 102-108`).
   - `overview-label` omits Medium, 10 px, and tracking 100 (`lines 110-117`).
   - `body` omits 14 px (`lines 88-93`).
   - `brewing-title` omits its declared 28 px inherited style composition (`lines 126-129`).
   - `arc-label` omits its declared 8 px inherited style composition (`lines 131-134`).

   **Fix:** Populate the declared typography properties. Correct `overview-value.family` to Cormorant/Cormorant Garamond and add size 18.

6. **Severity:** major  
   **Location:** `component.temperature-gauge`, `component.extraction-arc`, `component.grind-selector`; `Caffe/Caffe/Controls/:1192-1215`, `Caffe/Caffe/Controls/:594-617`, `Caffe/Caffe/Controls/:748-818`  
   **Claim:** These nodes sufficiently represent the reusable input components.  
   **Reality:** Meaningful canonical structure is flattened away. The temperature and extraction components contain sliders with explicit ranges and step sizes; the grind component contains three distinct selector buttons and a state-dependent particle visualization. None of this appears in their `parts` or properties. In particular, calling `ExtractionArc` a “dial” obscures that its actual input is a slider.  
   **Fix:** Add canonical properties such as:

   - Temperature: `parts: ["thermometer", "value", "slider", "range-labels"]`, range 88–96, step 1.
   - Extraction: `role: "sliderGauge"` or `role: "slider"`, `parts: ["arc-visual", "value", "slider", "range-labels"]`, range 20–35, step 1.
   - Grind: `parts: ["particle-display", "value", "hint", "option-buttons", "size-labels"]`.

   Internal controls need not become separate nodes.

7. **Severity:** minor  
   **Location:** `unresolved.menu.data-source`; `Caffe/Caffe/ViewModels/MainViewModel.cs:11-17`, `Caffe/Caffe/MainPage.xaml:52-73`  
   **Claim:** It is materially unresolved whether the four-item menu is fixed or a placeholder for a future data source.  
   **Reality:** The supplied implementation is decidable: it is a hard-coded four-item collection addressed by four fixed indexes and four fixed card instances. Possible future intent is not source ambiguity.  
   **Fix:** Remove this unresolved item. If desired, record `"implementation": "fixed four-item menu"` as a declared/derived property without speculating about future plans.

8. **Severity:** major  
   **Location:** region structure around `region.caffe-main.menu` and `region.caffe-main.parameters`; `Caffe/Caffe/MainPage.xaml:12-21`, `Caffe/Caffe/MainPage.xaml:26-35`, `Caffe/Caffe/MainPage.xaml:76-125`  
   **Claim:** The packet says the gold contains zero regions; the pasted graph actually contains two, modeling only the menu and parameter grid.  
   **Reality:** Two additional source-declared groupings satisfy the proposed region rule:

   - `MainContent` owns the presentation swap against the brewing overlay through `IsBrewing` and contains the scrolling normal experience (`lines 12-15`, `132-140`).
   - The right-side AutoLayout groups parameters, selection summary, and brew action as one responsive configure-and-brew workflow, moving between the second column and second row (`lines 76-125`).

   The outer responsive grid also expresses the menu/workflow split, but I am uncertain it needs its own node once both child regions are represented; adding it would risk transcribing layout rather than meaning.  
   **Fix:** The gold should contain regions. Retain `region.caffe-main.menu` and `region.caffe-main.parameters`; add:

   - `region.caffe-main.normal-content`, owning the scrolling non-brewing surface.
   - `region.caffe-main.brew-workflow`, containing `region.caffe-main.parameters`, `component.selection-overview`, and `component.brew-button`.

   Reparent the existing menu and workflow beneath `normal-content`. Do not emit regions for the card’s 2×2 spacer grid or the parameter component internals.
