# Gold review — `09-composer-shell`

**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** `evals\09-composer-shell\gold.graph.json` (99 nodes / 192 edges / 5 unresolved)

> Independent review breaking the same-author circularity: every gold in this
> kit was authored and calibrated by one agent lineage. Record findings here;
> **do not edit gold directly** - accepted fixes go through
> `tools/build_graphs.py`, then `scripts/validate_graph.py`.

## Read in this order

1. `evals/09-composer-shell/fixture.md` — the source list this gold was authored from
2. The actual source files it names (listed below)
3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary
4. `references/ontology.md` — node/edge definitions, state scope and attachment
5. `evals/09-composer-shell/README.md` — the altitude contract for this eval

## Sources cited by this gold

| Source file | Found | Nodes citing it |
|---|---|---|
| `Composer/src/Composer/Composer/Shell.xaml` | yes | 4 |
| `Composer/src/Composer/Composer/Shell.xaml.cs` | yes | 2 |
| `Composer/src/Composer/Composer/Themes/Tokens.xaml` | yes | 11 |
| `Composer/src/Composer/Composer/Themes/Typography.xaml` | yes | 11 |
| `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml` | yes | 5 |
| `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml.cs` | yes | 6 |
| `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml` | yes | 2 |
| `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml.cs` | yes | 4 |
| `Composer/src/Composer/Composer/Views/Controls/AppTitleRow.xaml.cs` | yes | 2 |
| `Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml` | yes | 8 |
| `Composer/src/Composer/Composer/Views/Controls/ComposerFooter.xaml.cs` | yes | 9 |
| `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml` | yes | 5 |
| `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml.cs` | yes | 4 |
| `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml` | yes | 4 |
| `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml.cs` | yes | 5 |
| `Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml` | yes | 2 |
| `Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard.xaml.cs` | yes | 2 |
| `Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml` | yes | 4 |
| `Composer/src/Composer/Composer/Views/Controls/LockedContextCard.xaml.cs` | yes | 5 |
| `Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml` | yes | 2 |
| `Composer/src/Composer/Composer/Views/Controls/ProgressIndicator.xaml.cs` | yes | 2 |

## Automated pre-pass — identifiers not found in the cited source

Every `properties.uno` value that must be a verbatim quotation from source
(names, style keys, resource keys, classes, members, glyphs) was checked
against the text of every source file this gold cites. The copy-don't-coin
contract says each one should appear literally.

**No fabricated identifiers.** Every quoted uno value exists in the application.

### Real, but not provable from the node's own citation (1)

The value exists in the app, but the node's `evidence.source` does not
point at a file containing it — commonly because the source is given as
a glob label rather than a path. The mapping is right; the provenance
is unverifiable, which is what makes this worth a reviewer's attention.

| Node | uno key | Value | Cited as |
|---|---|---|---|
| `component.app-title-row` | `class` | `Composer.Views.Controls.AppTitleRow` | `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml` |

### Token values that disagree with the style they name

A key existing in source does not make the value attributed to it right.
This compares each token's asserted value against the `Setter`s of the
style it names. The check exists because a cross-model reviewer found a
token claiming `OrbitalPageTitle` is 28px when that style declares 20 —
the 28 belonged to a different style, and every existence check passed it.

**No mismatches.** Every token value matches the style it cites.

## Structural facts

Stated as counts, not judgements — the altitude contract decides which of
these are correct, and that call belongs to the reviewer.

| Node type | In gold |
|---|---|
| `token` | 24 |
| `state` | 21 |
| `content` | 20 |
| `component` | 13 |
| `region` | 9 |
| `control` | 9 |
| `asset` | 2 |
| `screen` | 1 |

| Relation | In gold |
|---|---|
| `uses-token` | 103 |
| `contains` | 53 |
| `has-state` | 21 |
| `triggers` | 15 |

The cited XAML declares **26** layout containers (`Grid`/`StackPanel`/`AutoLayout`/…) across 21 file(s).
Compare against the `region` count above when judging check 6.

## Checks only a human can make

For each, mark **ok** / **finding** and add a line of evidence.

| # | Check | Verdict | Note |
|---|---|---|---|
| 1 | Every node and edge is evidence-backed by the named source — spot-check rationales against the XAML/C# | | |
| 2 | Every source-backed component reference is expanded (the PageHeader lesson) — no declared `UserControl` left as an opaque region | | |
| 3 | Altitude respected: no style-level hover/pressed/disabled states, tokens screen-scoped, canonical internals as properties not per-instance nodes | | |
| 4 | `properties.uno` values copied exactly from source (keys, x:Names, types) | | |
| 5 | `unresolved` items are genuinely undecidable from the source, not laziness | | |
| 6 | Structure the graph omits: does any real arrangement in the source go unrepresented? | | |

## Nodes

| Node id | Type | Name | Evidence | Conf | Uno mapping | Verdict | Note |
|---|---|---|---|---|---|---|---|
| `screen.composer-shell` | screen | Composer shell (three-column workspace) | observed | 1.0 | type=Page, class=Composer.Shell, property=RequestedTheme=Light, viewModel=ComposerViewModel (MVUX bindable over Composer.Models.ComposerModel) | | |
| `region.composer-shell.workspace` | region | Workspace grid | observed | 1.0 | type=Grid, xName=WorkspaceRoot | | |
| `region.composer-shell.left-rail` | region | Left rail container | observed | 1.0 | type=Border, xName=LeftRailContainer, transform=TranslateTransform x:Name=LeftRailTransform X=-40 | | |
| `region.composer-shell.right-rail` | region | Right rail container | observed | 1.0 | type=Border, xName=RightRailContainer, transform=TranslateTransform x:Name=RightRailTransform X=40 | | |
| `component.composition-stack` | component | Composition stack (left rail) | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.CompositionStack | | |
| `content.stack.eyebrow` | content | COMPOSITION STACK | observed | 1.0 | type=TextBlock, styleKey=MonoEyebrow | | |
| `content.stack.caption` | content | Stack caption | observed | 1.0 | type=TextBlock, fontResourceKey=SerifLightItalicFontFamily | | |
| `control.stack.layer-rows` | control | Layer row list | declared | 1.0 | type=ItemsRepeater, xName=LayerRows, property=ItemsSource="{Binding Layers}" (overwritten in code-behind), member=LayerRow.For(def, isActive, isLocked, dimmed) | | |
| `component.layer-row` | component | Layer row | declared | 1.0 | type=Button, styleKey=StackRowButtonStyle, property=Tag={Binding}, Click=OnLayerRowClick, member=Composer.Views.Controls.LayerRow | | |
| `component.files-rail` | component | Files rail (right rail) | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.FilesRail | | |
| `content.files.eyebrow` | content | FILES RAIL | observed | 1.0 | type=TextBlock, styleKey=MonoEyebrow | | |
| `content.files.caption` | content | Files caption | observed | 1.0 | type=TextBlock, fontResourceKey=SerifLightItalicFontFamily | | |
| `control.files.file-rows` | control | File row list | declared | 1.0 | type=ItemsRepeater, xName=FileRows, member=FileRow.For(fileName, status) | | |
| `component.file-row` | component | File row | declared | 1.0 | type=Grid, member=Composer.Views.Controls.FileRow | | |
| `content.files.locked-summary` | content | Locked counter | declared | 1.0 | type=TextBlock, xName=LockedSummary, member=FileStatuses (count of FileStatus.Drafted) | | |
| `component.active-canvas` | component | Active canvas (center column) | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.ActiveCanvas, xName=CenterCanvas | | |
| `region.canvas.column` | region | Canvas column | observed | 1.0 | type=utu:AutoLayout, xName=CanvasColumn, host=ScrollViewer x:Name=CanvasScrollHost | | |
| `component.progress-indicator` | component | Progress indicator | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.ProgressIndicator, xName=ProgressRegion, member=Fraction={Binding ProgressFraction}, Label={Binding ProgressLabel}, Counter={Binding ProgressCounter} | | |
| `asset.progress.track` | asset | Progress hairline | observed | 1.0 | type=Grid, xName=TrackRoot, fill=Border x:Name=ProgressFill | | |
| `content.progress.label` | content | Progress label | declared | 1.0 | type=TextBlock, xName=LabelText, member=ProgressLabel | | |
| `content.progress.counter` | content | Progress counter | declared | 1.0 | type=TextBlock, xName=CounterText, member=ProgressCounter | | |
| `component.app-title-row` | component | App title row | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.AppTitleRow, xName=TitleRow, member=ProjectName={Binding ProjectName}, ShowReset={Binding HasLockedLayers} | | |
| `content.title-row.project-name` | content | Project name | declared | 1.0 | type=TextBlock, xName=ProjectNameText, fontResourceKey=SansFontFamily, member=ProjectName | | |
| `control.title-row.reset` | control | Reset | declared | 1.0 | type=Button, xName=ResetButton, styleKey=LinkButtonStyle, member=MvuxCommandInvoker.Invoke(dc, "Reset") | | |
| `component.active-layer-header` | component | Active layer header | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.ActiveLayerHeader, xName=HeaderRegion, member=LayerIndex, LayerLabel, LayerState, Recap, Title, Subtitle | | |
| `content.header.recap` | content | Layer recap | declared | 1.0 | type=TextBlock, xName=RecapText, member=ActiveLayerRecap | | |
| `content.header.state-badge` | content | Layer state badge | declared | 1.0 | type=TextBlock, xName=StateBadgeText, member=ActiveLayerLayerState | | |
| `region.canvas.locked-stack` | region | Locked context stack | observed | 1.0 | type=utu:AutoLayout, xName=LockedStack, member=LayerStates / DefaultExpandedKinds | | |
| `component.locked-context-card` | component | Locked context card | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.LockedContextCard, member=LayerKind, Summary, Facts, IsExpanded | | |
| `content.locked-card.header` | content | Locked card header | observed | 1.0 | type=TextBlock, xName=HeaderLabelRun | | |
| `content.locked-card.summary` | content | Locked card summary | declared | 1.0 | type=TextBlock, xName=SummaryText, fontResourceKey=SerifLightItalicFontFamily, member=Summary | | |
| `component.info-row` | component | Fact row | declared | 1.0 | type=TextBlock, member=IList<KeyValuePair<string,string>> Facts | | |
| `region.canvas.slot` | region | Layer canvas slot | declared | 1.0 | type=ContentControl, xName=CanvasSlot, member=ActiveIndex -> Layers.All[i].Kind -> CreateCanvas(kind) | | |
| `component.composer-footer` | component | Composer footer | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.ComposerFooter, xName=FooterRegion, member=State (LayerState), Prompt (string) | | |
| `content.footer.eyebrow` | content | Composer status eyebrow | declared | 1.0 | type=TextBlock, xName=EyebrowText, styleKey=MonoEyebrow, member=ComposerStatus.ForLayerState | | |
| `content.footer.lead-question` | content | Lead question | declared | 1.0 | type=TextBlock, xName=LeadQuestionText, member=ActiveLeadQuestion | | |
| `content.footer.ack` | content | Preview acknowledgment | declared | 1.0 | type=TextBlock, xName=AckText, member=PreviewAcks | | |
| `control.footer.prompt-input` | control | Prompt textarea | declared | 1.0 | type=TextBox, xName=PromptInput, styleKey=ChatInputTextBoxStyle, member=SetActivePrompt / DiscardPreview | | |
| `region.footer.suggestions` | region | Suggestion row | observed | 1.0 | type=utu:AutoLayout, xName=ChipsRow | | |
| `content.footer.try-label` | content | TRY | observed | 1.0 | type=TextBlock, fontResourceKey=MonoFontFamily | | |
| `component.suggestion-chip` | component | Suggestion chip | declared | 1.0 | type=Button, member=ComposerModel.SuggestionChips | | |
| `content.footer.kbd-hint` | content | Submit hint | observed | 1.0 | type=TextBlock, xName=KbdHint | | |
| `region.footer.actions` | region | Action row | observed | 1.0 | — | | |
| `control.footer.primary` | control | Primary action | declared | 1.0 | type=Button, xName=PrimaryButton, styleKey=InkButtonStyle, member=LockAndContinue \| GeneratePreview \| AcceptAndLock | | |
| `content.footer.primary-hint` | content | Primary action hint | declared | 1.0 | type=TextBlock, xName=PrimaryHintText | | |
| `control.footer.discard-edits` | control | Discard edits | declared | 1.0 | type=Button, xName=DiscardEditsButton, styleKey=LinkButtonStyle, member=DiscardEdits | | |
| `control.footer.discard-preview` | control | Discard preview | declared | 1.0 | type=Button, xName=DiscardPreviewButton, styleKey=LinkButtonStyle, member=DiscardPreview | | |
| `region.canvas.future-stack` | region | Future preview stack | observed | 1.0 | type=utu:AutoLayout, xName=FutureStack, member=ActiveIndex / RailsVisible | | |
| `component.future-preview-card` | component | Future preview card | declared | 1.0 | type=UserControl, class=Composer.Views.Controls.FuturePreviewCard, member=LayerLabel, Hint | | |
| `asset.future-card.outline` | asset | Dashed card outline | observed | 1.0 | type=Rectangle, property=StrokeDashArray=3,3 | | |
| `content.future-card.header` | content | Future card header | declared | 1.0 | type=TextBlock, xName=HeaderText | | |
| `content.future-card.hint` | content | Future card hint | declared | 1.0 | type=TextBlock, xName=HintText, member=LayerDef.Hint | | |
| `state.composer-shell.rails-hidden` | state | Rails hidden (focused first) | declared | 1.0 | mechanism=code-behind, member=ShellModel.RailsVisible, storyboardKey=RailsHideStoryboard | | |
| `state.composer-shell.rails-open` | state | Rails open | declared | 1.0 | mechanism=code-behind, member=ShellModel.RailsVisible, storyboardKey=RailsRevealStoryboard | | |
| `state.active-canvas.focused-first` | state | Canvas focused-first dimensions | declared | 1.0 | mechanism=code-behind, member=RailsVisible / IsFocusedFirst | | |
| `state.active-canvas.rails-open` | state | Canvas rails-open dimensions | declared | 1.0 | mechanism=code-behind, member=RailsVisible / IsFocusedFirst | | |
| `state.future-stack.empty` | state | Future stack empty | declared | 1.0 | mechanism=code-behind, member=RailsVisible | | |
| `state.future-card.entering` | state | Future card entrance | declared | 1.0 | mechanism=code-behind, member=MotionPreferences.AnimationsEnabled | | |
| `state.composer-footer.clean` | state | Clean (REFINING) | declared | 1.0 | mechanism=dependency-property, member=ComposerFooter.State | | |
| `state.composer-footer.dirty` | state | Dirty (LISTENING) | declared | 1.0 | mechanism=dependency-property, member=ComposerFooter.State, styleKey=AmberButtonStyle | | |
| `state.composer-footer.previewing` | state | Previewing (PROPOSING) | declared | 1.0 | mechanism=dependency-property, member=ComposerFooter.State | | |
| `state.composer-footer.hidden` | state | Footer hidden on Scaffold | declared | 1.0 | mechanism=code-behind, member=Layers.All[ActiveIndex].Kind == LayerKind.Scaffold | | |
| `state.layer-header.edited` | state | Edited badge | declared | 1.0 | mechanism=dependency-property, member=ActiveLayerHeader.LayerState | | |
| `state.layer-header.preview` | state | Preview badge | declared | 1.0 | mechanism=dependency-property, member=ActiveLayerHeader.LayerState | | |
| `state.layer-header.no-recap` | state | Recap row collapsed | declared | 1.0 | mechanism=dependency-property, member=ActiveLayerHeader.Recap | | |
| `state.layer-row.active` | state | Row active | declared | 1.0 | mechanism=binding, member=ActiveIndex | | |
| `state.layer-row.locked` | state | Row locked | declared | 1.0 | mechanism=binding, member=LayerStates[kind] == LayerState.Locked | | |
| `state.layer-row.upcoming` | state | Row upcoming | declared | 1.0 | mechanism=binding, member=index > ActiveIndex | | |
| `state.file-row.planned` | state | File planned | declared | 1.0 | mechanism=binding, member=FileStatuses[kind] | | |
| `state.file-row.writing` | state | File writing | declared | 1.0 | mechanism=binding, member=FileStatuses[kind] | | |
| `state.file-row.drafted` | state | File drafted | declared | 1.0 | mechanism=binding, member=FileStatuses[kind] | | |
| `state.locked-card.expanded` | state | Expanded | declared | 1.0 | mechanism=code-behind, member=IsExpanded | | |
| `state.locked-card.collapsed` | state | Card collapsed | declared | 1.0 | mechanism=dependency-property, member=LockedContextCard.IsExpanded | | |
| `token.color.glass-backdrop` | token | Glass backdrop (page background) | declared | 1.0 | resourceKey=GlassBackdropBrush, resourceType=LinearGradientBrush | | |
| `token.color.paper` | token | Paper (on-ink foreground) | declared | 1.0 | resourceKey=PaperBrush, resourceType=SolidColorBrush | | |
| `token.color.paper2` | token | Paper 2 (panel surface) | declared | 1.0 | resourceKey=Paper2Brush, resourceType=SolidColorBrush | | |
| `token.color.ink` | token | Ink (primary text) | declared | 1.0 | resourceKey=InkBrush, resourceType=SolidColorBrush | | |
| `token.color.ink2` | token | Ink 2 (secondary text) | declared | 1.0 | resourceKey=Ink2Brush, resourceType=SolidColorBrush | | |
| `token.color.ink3` | token | Ink 3 (tertiary text) | declared | 1.0 | resourceKey=Ink3Brush, resourceType=SolidColorBrush | | |
| `token.color.ink4` | token | Ink 4 (lowest-emphasis text) | declared | 1.0 | resourceKey=Ink4Brush, resourceType=SolidColorBrush | | |
| `token.color.hairline` | token | Hairline (borders and rules) | declared | 1.0 | resourceKey=HairlineBrush, resourceType=SolidColorBrush | | |
| `token.color.amber` | token | Amber (active / locked accent) | declared | 1.0 | resourceKey=AmberBrush, resourceType=SolidColorBrush | | |
| `token.color.indigo` | token | Indigo (in-progress accent) | declared | 1.0 | resourceKey=IndigoBrush, resourceType=SolidColorBrush | | |
| `token.color.transparent` | token | Transparent | declared | 1.0 | resourceKey=TransparentBrush, resourceType=SolidColorBrush | | |
| `token.typography.mono` | token | Mono face (JetBrains Mono) | declared | 1.0 | resourceKey=MonoFontFamily, resourceType=FontFamily | | |
| `token.typography.sans` | token | Sans face (Inter) | declared | 1.0 | resourceKey=SansFontFamily, resourceType=FontFamily | | |
| `token.typography.serif` | token | Prompt input face | declared | 1.0 | resourceKey=SerifFontFamily, resourceType=FontFamily | | |
| `token.typography.serif-italic` | token | Italic voice face | declared | 1.0 | resourceKey=SerifItalicFontFamily, resourceType=FontFamily | | |
| `token.typography.serif-light-italic` | token | Light italic voice face | declared | 1.0 | resourceKey=SerifLightItalicFontFamily, resourceType=FontFamily | | |
| `token.typography.mono-eyebrow` | token | Mono eyebrow style | declared | 1.0 | styleKey=MonoEyebrow, resourceType=Style(TextBlock) | | |
| `token.font-size.eyebrow-micro` | token | Eyebrow micro size | declared | 1.0 | resourceKey=TypeEyebrowMicroSize, resourceType=x:Double | | |
| `token.font-size.eyebrow` | token | Eyebrow size | declared | 1.0 | resourceKey=TypeEyebrowSize, resourceType=x:Double | | |
| `token.font-size.label` | token | Label size | declared | 1.0 | resourceKey=TypeLabelSize, resourceType=x:Double | | |
| `token.font-size.body-small` | token | Body small size | declared | 1.0 | resourceKey=TypeBodySmallSize, resourceType=x:Double | | |
| `token.font-size.body` | token | Body size | declared | 1.0 | resourceKey=TypeBodySize, resourceType=x:Double | | |
| `token.spacing.14` | token | Panel rhythm (14) | derived | 1.0 | — | | |
| `token.radius.4` | token | Small radius (4) | derived | 1.0 | — | | |
| `control.locked-card.expand-toggle` | control | Expand toggle | declared | 1.0 | type=Button, xName=ExpandToggle, styleKey=LinkButtonStyle, member=LockedContextCard.IsExpanded | | |
| `control.locked-card.revisit` | control | Revisit | declared | 1.0 | type=Button, xName=RevisitButton, styleKey=LinkButtonStyle, member=MvuxCommandInvoker.Invoke(dc, "Revisit", LayerKind) | | |

## Edges

Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are
the ones a graph must never invent. They are listed first.

| Relation | From | To | Evidence | Verdict | Note |
|---|---|---|---|---|---|
| **triggers** | `component.layer-row` | `region.canvas.slot` | declared | | |
| **triggers** | `component.layer-row` | `state.composer-shell.rails-open` | declared | | |
| **triggers** | `control.footer.prompt-input` | `state.composer-footer.dirty` | declared | | |
| **triggers** | `component.suggestion-chip` | `state.composer-footer.dirty` | declared | | |
| **triggers** | `control.footer.primary` | `state.composer-footer.previewing` | declared | | |
| **triggers** | `control.footer.primary` | `region.canvas.slot` | declared | | |
| **triggers** | `control.footer.primary` | `state.composer-shell.rails-open` | declared | | |
| **triggers** | `control.footer.primary` | `state.layer-row.locked` | declared | | |
| **triggers** | `control.footer.primary` | `state.file-row.drafted` | declared | | |
| **triggers** | `control.footer.discard-edits` | `state.composer-footer.clean` | declared | | |
| **triggers** | `control.footer.discard-preview` | `state.composer-footer.clean` | declared | | |
| **triggers** | `control.locked-card.revisit` | `region.canvas.slot` | declared | | |
| **triggers** | `control.locked-card.revisit` | `state.composer-footer.clean` | declared | | |
| **triggers** | `control.locked-card.expand-toggle` | `state.locked-card.expanded` | declared | | |
| **triggers** | `control.locked-card.expand-toggle` | `state.locked-card.collapsed` | declared | | |
| **contains** | `screen.composer-shell` | `region.composer-shell.workspace` | observed | | |
| **contains** | `region.composer-shell.workspace` | `region.composer-shell.left-rail` | observed | | |
| **contains** | `region.composer-shell.workspace` | `component.active-canvas` | observed | | |
| **contains** | `region.composer-shell.workspace` | `region.composer-shell.right-rail` | observed | | |
| **contains** | `region.composer-shell.left-rail` | `component.composition-stack` | observed | | |
| **contains** | `region.composer-shell.right-rail` | `component.files-rail` | observed | | |
| **contains** | `component.composition-stack` | `content.stack.eyebrow` | observed | | |
| **contains** | `component.composition-stack` | `content.stack.caption` | observed | | |
| **contains** | `component.composition-stack` | `control.stack.layer-rows` | observed | | |
| **contains** | `control.stack.layer-rows` | `component.layer-row` | observed | | |
| **contains** | `component.files-rail` | `content.files.eyebrow` | observed | | |
| **contains** | `component.files-rail` | `content.files.caption` | observed | | |
| **contains** | `component.files-rail` | `control.files.file-rows` | observed | | |
| **contains** | `control.files.file-rows` | `component.file-row` | observed | | |
| **contains** | `component.files-rail` | `content.files.locked-summary` | observed | | |
| **contains** | `component.active-canvas` | `region.canvas.column` | observed | | |
| **contains** | `region.canvas.column` | `component.progress-indicator` | observed | | |
| **contains** | `region.canvas.column` | `component.app-title-row` | observed | | |
| **contains** | `region.canvas.column` | `component.active-layer-header` | observed | | |
| **contains** | `region.canvas.column` | `region.canvas.locked-stack` | observed | | |
| **contains** | `region.canvas.column` | `region.canvas.slot` | observed | | |
| **contains** | `region.canvas.column` | `component.composer-footer` | observed | | |
| **contains** | `region.canvas.column` | `region.canvas.future-stack` | observed | | |
| **contains** | `component.progress-indicator` | `asset.progress.track` | observed | | |
| **contains** | `component.progress-indicator` | `content.progress.label` | observed | | |
| **contains** | `component.progress-indicator` | `content.progress.counter` | observed | | |
| **contains** | `component.app-title-row` | `content.title-row.project-name` | observed | | |
| **contains** | `component.app-title-row` | `control.title-row.reset` | observed | | |
| **contains** | `component.active-layer-header` | `content.header.recap` | observed | | |
| **contains** | `component.active-layer-header` | `content.header.state-badge` | observed | | |
| **contains** | `region.canvas.locked-stack` | `component.locked-context-card` | observed | | |
| **contains** | `component.locked-context-card` | `control.locked-card.expand-toggle` | observed | | |
| **contains** | `component.locked-context-card` | `content.locked-card.header` | observed | | |
| **contains** | `component.locked-context-card` | `control.locked-card.revisit` | observed | | |
| **contains** | `component.locked-context-card` | `content.locked-card.summary` | observed | | |
| **contains** | `component.locked-context-card` | `component.info-row` | observed | | |
| **contains** | `component.composer-footer` | `content.footer.eyebrow` | observed | | |
| **contains** | `component.composer-footer` | `content.footer.lead-question` | observed | | |
| **contains** | `component.composer-footer` | `content.footer.ack` | observed | | |
| **contains** | `component.composer-footer` | `control.footer.prompt-input` | observed | | |
| **contains** | `component.composer-footer` | `region.footer.suggestions` | observed | | |
| **contains** | `component.composer-footer` | `region.footer.actions` | observed | | |
| **contains** | `region.footer.suggestions` | `content.footer.try-label` | observed | | |
| **contains** | `region.footer.suggestions` | `component.suggestion-chip` | observed | | |
| **contains** | `region.footer.suggestions` | `content.footer.kbd-hint` | observed | | |
| **contains** | `region.footer.actions` | `control.footer.primary` | observed | | |
| **contains** | `region.footer.actions` | `content.footer.primary-hint` | observed | | |
| **contains** | `region.footer.actions` | `control.footer.discard-edits` | observed | | |
| **contains** | `region.footer.actions` | `control.footer.discard-preview` | observed | | |
| **contains** | `region.canvas.future-stack` | `component.future-preview-card` | observed | | |
| **contains** | `component.future-preview-card` | `asset.future-card.outline` | observed | | |
| **contains** | `component.future-preview-card` | `content.future-card.header` | observed | | |
| **contains** | `component.future-preview-card` | `content.future-card.hint` | observed | | |
| **has-state** | `region.composer-shell.workspace` | `state.composer-shell.rails-hidden` | declared | | |
| **has-state** | `region.composer-shell.workspace` | `state.composer-shell.rails-open` | declared | | |
| **has-state** | `component.active-canvas` | `state.active-canvas.focused-first` | declared | | |
| **has-state** | `component.active-canvas` | `state.active-canvas.rails-open` | declared | | |
| **has-state** | `region.canvas.future-stack` | `state.future-stack.empty` | declared | | |
| **has-state** | `component.future-preview-card` | `state.future-card.entering` | declared | | |
| **has-state** | `component.composer-footer` | `state.composer-footer.clean` | declared | | |
| **has-state** | `component.composer-footer` | `state.composer-footer.dirty` | declared | | |
| **has-state** | `component.composer-footer` | `state.composer-footer.previewing` | declared | | |
| **has-state** | `component.composer-footer` | `state.composer-footer.hidden` | declared | | |
| **has-state** | `component.active-layer-header` | `state.layer-header.edited` | declared | | |
| **has-state** | `component.active-layer-header` | `state.layer-header.preview` | declared | | |
| **has-state** | `component.active-layer-header` | `state.layer-header.no-recap` | declared | | |
| **has-state** | `component.layer-row` | `state.layer-row.active` | declared | | |
| **has-state** | `component.layer-row` | `state.layer-row.locked` | declared | | |
| **has-state** | `component.layer-row` | `state.layer-row.upcoming` | declared | | |
| **has-state** | `component.file-row` | `state.file-row.planned` | declared | | |
| **has-state** | `component.file-row` | `state.file-row.writing` | declared | | |
| **has-state** | `component.file-row` | `state.file-row.drafted` | declared | | |
| **has-state** | `component.locked-context-card` | `state.locked-card.collapsed` | declared | | |
| **has-state** | `component.locked-context-card` | `state.locked-card.expanded` | declared | | |
| **uses-token** | `screen.composer-shell` | `token.color.glass-backdrop` | declared | | |
| **uses-token** | `component.composition-stack` | `token.color.paper2` | declared | | |
| **uses-token** | `component.composition-stack` | `token.color.hairline` | declared | | |
| **uses-token** | `component.composition-stack` | `token.spacing.14` | declared | | |
| **uses-token** | `component.files-rail` | `token.color.paper2` | declared | | |
| **uses-token** | `component.files-rail` | `token.color.hairline` | declared | | |
| **uses-token** | `component.files-rail` | `token.spacing.14` | declared | | |
| **uses-token** | `content.stack.eyebrow` | `token.typography.mono-eyebrow` | declared | | |
| **uses-token** | `content.files.eyebrow` | `token.typography.mono-eyebrow` | declared | | |
| **uses-token** | `content.stack.caption` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `content.stack.caption` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.stack.caption` | `token.color.ink3` | declared | | |
| **uses-token** | `content.files.caption` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `content.files.caption` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.files.caption` | `token.color.ink3` | declared | | |
| **uses-token** | `content.files.locked-summary` | `token.typography.mono` | declared | | |
| **uses-token** | `content.files.locked-summary` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.files.locked-summary` | `token.color.ink3` | declared | | |
| **uses-token** | `component.layer-row` | `token.typography.mono` | declared | | |
| **uses-token** | `component.layer-row` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `component.layer-row` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `component.layer-row` | `token.font-size.label` | declared | | |
| **uses-token** | `component.layer-row` | `token.color.ink` | declared | | |
| **uses-token** | `component.layer-row` | `token.color.ink3` | declared | | |
| **uses-token** | `state.layer-row.active` | `token.color.amber` | declared | | |
| **uses-token** | `state.layer-row.locked` | `token.color.ink2` | declared | | |
| **uses-token** | `component.file-row` | `token.typography.mono` | declared | | |
| **uses-token** | `component.file-row` | `token.font-size.label` | declared | | |
| **uses-token** | `component.file-row` | `token.font-size.eyebrow-micro` | declared | | |
| **uses-token** | `component.file-row` | `token.color.ink` | declared | | |
| **uses-token** | `state.file-row.planned` | `token.color.ink4` | declared | | |
| **uses-token** | `state.file-row.writing` | `token.color.indigo` | declared | | |
| **uses-token** | `state.file-row.drafted` | `token.color.amber` | declared | | |
| **uses-token** | `asset.progress.track` | `token.color.hairline` | declared | | |
| **uses-token** | `asset.progress.track` | `token.color.amber` | declared | | |
| **uses-token** | `content.progress.label` | `token.typography.mono` | declared | | |
| **uses-token** | `content.progress.label` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.progress.label` | `token.color.ink3` | declared | | |
| **uses-token** | `content.progress.counter` | `token.color.ink4` | declared | | |
| **uses-token** | `content.title-row.project-name` | `token.typography.sans` | declared | | |
| **uses-token** | `content.title-row.project-name` | `token.color.ink` | declared | | |
| **uses-token** | `content.header.recap` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `content.header.recap` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.header.recap` | `token.color.ink3` | declared | | |
| **uses-token** | `content.header.recap` | `token.color.ink4` | declared | | |
| **uses-token** | `content.header.state-badge` | `token.typography.mono` | declared | | |
| **uses-token** | `content.header.state-badge` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.header.state-badge` | `token.color.amber` | declared | | |
| **uses-token** | `component.composer-footer` | `token.color.paper2` | declared | | |
| **uses-token** | `component.composer-footer` | `token.color.hairline` | declared | | |
| **uses-token** | `component.composer-footer` | `token.spacing.14` | declared | | |
| **uses-token** | `content.footer.eyebrow` | `token.typography.mono-eyebrow` | declared | | |
| **uses-token** | `content.footer.lead-question` | `token.typography.sans` | declared | | |
| **uses-token** | `content.footer.lead-question` | `token.font-size.body` | declared | | |
| **uses-token** | `content.footer.lead-question` | `token.color.ink2` | declared | | |
| **uses-token** | `content.footer.ack` | `token.typography.serif-italic` | declared | | |
| **uses-token** | `content.footer.ack` | `token.color.ink2` | declared | | |
| **uses-token** | `content.footer.ack` | `token.color.amber` | declared | | |
| **uses-token** | `control.footer.prompt-input` | `token.typography.serif` | declared | | |
| **uses-token** | `control.footer.prompt-input` | `token.font-size.body` | declared | | |
| **uses-token** | `control.footer.prompt-input` | `token.color.ink` | declared | | |
| **uses-token** | `control.footer.prompt-input` | `token.color.ink4` | declared | | |
| **uses-token** | `content.footer.try-label` | `token.typography.mono` | declared | | |
| **uses-token** | `content.footer.try-label` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.footer.try-label` | `token.color.ink3` | declared | | |
| **uses-token** | `component.suggestion-chip` | `token.typography.mono` | declared | | |
| **uses-token** | `component.suggestion-chip` | `token.color.transparent` | declared | | |
| **uses-token** | `component.suggestion-chip` | `token.color.hairline` | declared | | |
| **uses-token** | `component.suggestion-chip` | `token.color.ink2` | declared | | |
| **uses-token** | `component.suggestion-chip` | `token.radius.4` | declared | | |
| **uses-token** | `content.footer.kbd-hint` | `token.font-size.eyebrow-micro` | declared | | |
| **uses-token** | `content.footer.kbd-hint` | `token.color.ink4` | declared | | |
| **uses-token** | `control.footer.primary` | `token.color.ink` | declared | | |
| **uses-token** | `control.footer.primary` | `token.color.paper` | declared | | |
| **uses-token** | `control.footer.primary` | `token.typography.mono` | declared | | |
| **uses-token** | `control.footer.primary` | `token.font-size.eyebrow-micro` | declared | | |
| **uses-token** | `state.composer-footer.dirty` | `token.color.amber` | declared | | |
| **uses-token** | `content.footer.primary-hint` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `content.footer.primary-hint` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.footer.primary-hint` | `token.color.ink3` | declared | | |
| **uses-token** | `component.locked-context-card` | `token.color.paper2` | declared | | |
| **uses-token** | `component.locked-context-card` | `token.color.hairline` | declared | | |
| **uses-token** | `component.locked-context-card` | `token.radius.4` | declared | | |
| **uses-token** | `content.locked-card.header` | `token.typography.mono` | declared | | |
| **uses-token** | `content.locked-card.header` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.locked-card.header` | `token.color.ink` | declared | | |
| **uses-token** | `content.locked-card.header` | `token.color.ink2` | declared | | |
| **uses-token** | `content.locked-card.header` | `token.color.ink3` | declared | | |
| **uses-token** | `content.locked-card.summary` | `token.typography.serif-light-italic` | declared | | |
| **uses-token** | `content.locked-card.summary` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.locked-card.summary` | `token.color.ink` | declared | | |
| **uses-token** | `component.info-row` | `token.typography.mono` | declared | | |
| **uses-token** | `component.info-row` | `token.color.ink3` | declared | | |
| **uses-token** | `component.info-row` | `token.color.ink` | declared | | |
| **uses-token** | `control.locked-card.expand-toggle` | `token.color.ink4` | declared | | |
| **uses-token** | `asset.future-card.outline` | `token.color.hairline` | declared | | |
| **uses-token** | `asset.future-card.outline` | `token.radius.4` | declared | | |
| **uses-token** | `content.future-card.header` | `token.typography.mono` | declared | | |
| **uses-token** | `content.future-card.header` | `token.font-size.eyebrow` | declared | | |
| **uses-token** | `content.future-card.header` | `token.color.ink3` | declared | | |
| **uses-token** | `content.future-card.hint` | `token.typography.serif-italic` | declared | | |
| **uses-token** | `content.future-card.hint` | `token.font-size.body-small` | declared | | |
| **uses-token** | `content.future-card.hint` | `token.color.ink3` | declared | | |

## Unresolved items

| Question | Related ids | Genuinely undecidable? | Note |
|---|---|---|---|
| Is CompositionStack's ItemsSource="{Binding Layers}" live, or vestigial? | `control.stack.layer-rows`, `component.layer-row` | | |
| Are CompositionStack and FilesRail two controls or one panel component with two variants? | `component.composition-stack`, `component.files-rail` | | |
| Will the center column become a navigation region, and does that change the graph's shape? | `region.canvas.slot`, `component.active-canvas`, `screen.composer-shell` | | |
| Are the collapsed title row and header title/subtitle removed for good? | `component.app-title-row`, `component.active-layer-header`, `content.title-row.project-name`, `control.title-row.reset` | | |
| Is the duplicated README.md row in the files rail intentional? | `component.file-row`, `control.files.file-rows`, `content.files.locked-summary` | | |

## Verdict

_Overall assessment, and whether the gold is fit to remain the answer key._

Findings accepted: _(list)_
Findings rejected: _(list, with reasons)_

