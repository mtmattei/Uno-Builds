#!/usr/bin/env python3
"""Build the gold graph for eval 09 (Composer Shell — three-column workspace).

Authored from source, following the kit convention that golds are generated
rather than hand-patched: edit this file, regenerate, revalidate.

Sources (the modeled surface):
  Composer/src/Composer/Composer/Shell.xaml(.cs)                  - 3-column workspace + rail storyboards
  Composer/src/Composer/Composer/Views/Controls/CompositionStack  - left rail
  Composer/src/Composer/Composer/Views/Controls/ActiveCanvas      - center column
  Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader - header inside the canvas
  Composer/src/Composer/Composer/Views/Controls/ProgressIndicator - canvas slot 1
  Composer/src/Composer/Composer/Views/Controls/AppTitleRow       - canvas slot 2 (collapsed)
  Composer/src/Composer/Composer/Views/Controls/ComposerFooter    - prompt + primary action
  Composer/src/Composer/Composer/Views/Controls/LockedContextCard - locked-stack card
  Composer/src/Composer/Composer/Views/Controls/FuturePreviewCard - future-stack card
  Composer/src/Composer/Composer/Views/Controls/FilesRail         - right rail
  Composer/src/Composer/Composer/Themes/Tokens.xaml, Typography.xaml
  Composer/src/Composer/Composer/Models/Presentation/ShellModel.cs, Models/ComposerModel.cs,
  Models/LayerDef.cs, LayerState.cs, FileStatus.cs, ComposerStatus.cs
  Themes/ChipStyles.xaml + ContextEngineStyles.xaml (style keys only)

SCOPE decisions (a reviewer should rule on these):
  1. The eight views under Views/Layers/ are OUT of scope per fixture.md. The
     graph records that ContentControl x:Name="CanvasSlot" hosts a swappable
     per-layer canvas (control.canvas.slot) and stops there.
  2. Every custom control reference is expanded, two levels deep
     (Shell -> ActiveCanvas -> ActiveLayerHeader/ProgressIndicator/...), plus
     the two controls that only exist as code-behind instantiations
     (LockedContextCard, FuturePreviewCard).
  3. Data-bound repeater rows are modeled canonically only. The eight layer
     rows and ten file rows come from ItemsRepeater projections (LayerRow /
     FileRow records), not hand-written XAML elements, so there are no
     instance nodes and no instance-of edges; the collection identities live
     in properties (layers[], files[]). Gold 08 enumerated its five TabBarItems
     because those are five literal XAML elements — the opposite case.
  4. CompositionStack and FilesRail share an identical panel template
     (Paper2 + hairline edge, 20,28 padding, spacing-14 AutoLayout, mono
     eyebrow, italic caption, hairline divider, ItemsRepeater). They are two
     separately declared UserControls, so folding them into one canonical
     would fight the "slugify the declared name" rule. Recorded as
     `unresolved.rail-panel-shared` instead of asserted.
  5. Tokens are screen-scoped: only resources this surface actually consumes,
     directly or through a style it uses. Tokens.xaml + Typography.xaml
     declare well over 150 keys; 21 declared ones reach this surface. Two
     derived tokens (spacing 14, radius 4) are included because the literal
     recurs across semantically related surfaces. Interaction-only shades
     (InkHoverBrush, InkPressedBrush, Paper3Brush in the row-button hover)
     are folded out per the variant-folding rule.

ALTITUDE decisions:
  a. States are screen/component presentation conditions only. The rail
     reveal/hide pair, the canvas focused-first/rails-open dimensions, the
     footer's LayerState machine, the per-row status treatments and the
     locked-card collapse are all declared in source and modeled. Button
     PointerOver/Pressed/Disabled visual states inside InkButtonStyle,
     AmberButtonStyle, LinkButtonStyle and StackRowButtonStyle are style
     internals and are NOT state nodes.
  b. One condition (ShellModel.RailsVisible) drives presentation at three
     nodes, so per the v0.5 condition-vs-presentation rule there is one state
     per affected node (screen, canvas, future stack), each named for its
     local presentation and carrying the driving member in properties.uno.
  c. LayerState.Locked renders the footer identically to LayerState.Clean
     (same switch arm values), so it is folded into state.composer-footer.clean
     via properties.appliesTo rather than given its own node.
  d. Comments cite briefs and planned work (uen:Region.Attached navigation,
     Pages + RouteMap "scaffolding for a future navigation pass"). The graph
     models the implemented direct hosting; the planned navigation is an
     unresolved item, not a node.
  e. Controls that exist but render nothing (AppTitleRow, and the header's
     LayerLabelText / TitleText / SubtitleText) are kept as nodes with
     properties.visibility = "Collapsed" — the DPs are still bound — but no
     content nodes are created for text that never paints.
"""

from __future__ import annotations

import json
from pathlib import Path

ROOT = "Composer/src/Composer/Composer/"

SHELL = ROOT + "Shell.xaml"
SHELL_CS = ROOT + "Shell.xaml.cs"
STACK = ROOT + "Views/Controls/CompositionStack.xaml"
STACK_CS = ROOT + "Views/Controls/CompositionStack.xaml.cs"
CANVAS = ROOT + "Views/Controls/ActiveCanvas.xaml"
CANVAS_CS = ROOT + "Views/Controls/ActiveCanvas.xaml.cs"
HEADER = ROOT + "Views/Controls/ActiveLayerHeader.xaml"
HEADER_CS = ROOT + "Views/Controls/ActiveLayerHeader.xaml.cs"
FILES = ROOT + "Views/Controls/FilesRail.xaml"
FILES_CS = ROOT + "Views/Controls/FilesRail.xaml.cs"
PROGRESS = ROOT + "Views/Controls/ProgressIndicator.xaml"
PROGRESS_CS = ROOT + "Views/Controls/ProgressIndicator.xaml.cs"
TITLEROW = ROOT + "Views/Controls/AppTitleRow.xaml"
TITLEROW_CS = ROOT + "Views/Controls/AppTitleRow.xaml.cs"
FOOTER = ROOT + "Views/Controls/ComposerFooter.xaml"
FOOTER_CS = ROOT + "Views/Controls/ComposerFooter.xaml.cs"
LOCKED = ROOT + "Views/Controls/LockedContextCard.xaml"
LOCKED_CS = ROOT + "Views/Controls/LockedContextCard.xaml.cs"
FUTURE = ROOT + "Views/Controls/FuturePreviewCard.xaml"
FUTURE_CS = ROOT + "Views/Controls/FuturePreviewCard.xaml.cs"
TOKENS = ROOT + "Themes/Tokens.xaml"
TYPO = ROOT + "Themes/Typography.xaml"
CHIPSTYLES = ROOT + "Themes/ChipStyles.xaml"
CTXSTYLES = ROOT + "Themes/ContextEngineStyles.xaml"
SHELLMODEL = ROOT + "Models/Presentation/ShellModel.cs"
MODEL = ROOT + "Models/ComposerModel.cs"
LAYERDEF = ROOT + "Models/LayerDef.cs"

nodes: list[dict] = []
edges: list[dict] = []


def ev(kind: str, path: str, rationale: str | None = None, conf: float = 1.0) -> dict:
    e = {
        "kind": kind,
        "confidence": conf,
        "source": {"type": "xaml" if path.endswith(".xaml") else "csharp", "path": path},
    }
    if rationale:
        e["rationale"] = rationale
    return e


def node(id_, type_, name, evidence, **extra):
    n = {"id": id_, "type": type_, "name": name, "evidence": evidence}
    n.update(extra)
    nodes.append(n)


def edge(frm, rel, to, evidence, **props):
    e = {"from": frm, "relation": rel, "to": to}
    if props:
        e["properties"] = props
    e["evidence"] = evidence
    edges.append(e)


def contains(parent, child, path, rationale=None):
    edge(parent, "contains", child, ev("observed", path, rationale))


def uses(consumer, token, path, applies):
    edge(consumer, "uses-token", token, ev("declared", path), appliesTo=applies)


# ---------------------------------------------------------------- screen
node("screen.composer-shell", "screen", "Composer shell (three-column workspace)",
     ev("observed", SHELL,
        "Page x:Class Composer.Shell. DataContext is assigned in OnLoaded to the MVUX-generated "
        "ComposerViewModel over ComposerModel, which delegates its shell feeds to ShellModel."),
     properties={"uno": {"type": "Page", "class": "Composer.Shell",
                         "property": "RequestedTheme=Light",
                         "viewModel": "ComposerViewModel (MVUX bindable over Composer.Models.ComposerModel)"},
                 "architecture": "MVUX (Uno.Extensions.Reactive) + Uno Toolkit"})

# ---------------------------------------------------------------- shell regions
node("region.composer-shell.workspace", "region", "Workspace grid",
     ev("observed", SHELL,
        "Grid x:Name=WorkspaceRoot with three columns; the two rail columns are declared Width=0 "
        "and snapped to 280px from code-behind."),
     properties={"uno": {"type": "Grid", "xName": "WorkspaceRoot"},
                 "columns": ["LeftRailColumn (0 -> 280)", "* (canvas)", "RightRailColumn (0 -> 280)"],
                 "outerHost": "Grid with utu:SafeArea.Insets=VisibleBounds"})

node("region.composer-shell.left-rail", "region", "Left rail container",
     ev("observed", SHELL,
        "Border x:Name=LeftRailContainer in column 0, Opacity=0 with a TranslateTransform "
        "x:Name=LeftRailTransform X=-40; animated by the rail storyboards."),
     properties={"uno": {"type": "Border", "xName": "LeftRailContainer",
                         "transform": "TranslateTransform x:Name=LeftRailTransform X=-40"},
                 "restingOpacity": 0})

node("region.composer-shell.right-rail", "region", "Right rail container",
     ev("observed", SHELL,
        "Border x:Name=RightRailContainer in column 2, Opacity=0 with a TranslateTransform "
        "x:Name=RightRailTransform X=40."),
     properties={"uno": {"type": "Border", "xName": "RightRailContainer",
                         "transform": "TranslateTransform x:Name=RightRailTransform X=40"},
                 "restingOpacity": 0})

# ---------------------------------------------------------------- left rail
node("component.composition-stack", "component", "Composition stack (left rail)",
     ev("declared", STACK,
        "UserControl Composer.Views.Controls.CompositionStack: Paper2 panel with a right hairline "
        "edge, 20,28 padding, vertical AutoLayout Spacing=14."),
     role="navigationRail",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.CompositionStack"},
                 "parts": ["eyebrow", "caption", "divider", "layer-rows"],
                 "widthWhenOpen": 280})

node("content.stack.eyebrow", "content", "COMPOSITION STACK", ev("observed", STACK),
     text="COMPOSITION STACK", semanticRole="sectionHeader",
     properties={"uno": {"type": "TextBlock", "styleKey": "MonoEyebrow"}})

node("content.stack.caption", "content", "Stack caption", ev("observed", STACK),
     text="A conversation that crystallizes into a build system.",
     properties={"uno": {"type": "TextBlock", "fontResourceKey": "SerifLightItalicFontFamily"}})

node("control.stack.layer-rows", "control", "Layer row list",
     ev("declared", STACK_CS,
        "ItemsRepeater x:Name=LayerRows with a vertical StackLayout (Spacing=2). Render() assigns "
        "ItemsSource to an ImmutableArray<LayerRow> projected from Layers.All on every "
        "ActiveIndex / LayerStates change."),
     role="itemsRepeater",
     properties={"uno": {"type": "ItemsRepeater", "xName": "LayerRows",
                         "property": "ItemsSource=\"{Binding Layers}\" (overwritten in code-behind)",
                         "member": "LayerRow.For(def, isActive, isLocked, dimmed)"}})

node("component.layer-row", "component", "Layer row",
     ev("declared", STACK,
        "ItemTemplate row: a full-width Button (StackRowButtonStyle) over a 3-column Grid — 2px "
        "state border, index + uppercase label + italic hint, trailing glyph. Eight rows, one per "
        "LayerDef in Layers.All."),
     role="navigationRow",
     properties={"uno": {"type": "Button", "styleKey": "StackRowButtonStyle",
                         "property": "Tag={Binding}, Click=OnLayerRowClick",
                         "member": "Composer.Views.Controls.LayerRow"},
                 "parts": ["state-border", "index-label", "label", "hint", "glyph"],
                 "instanceCount": 8,
                 "layers": ["Intent", "UX", "Architecture", "Design System",
                            "Interactions", "Data", "Implementation", "Scaffold"],
                 "layerLabelSource": "LayerDef.Label uppercased; IndexLabel is (Index + 1) formatted D2",
                 "bindings": ["LeftBorderBrush", "Opacity", "IndexLabel", "LabelUpper", "Hint", "Glyph"]})

# ---------------------------------------------------------------- right rail
node("component.files-rail", "component", "Files rail (right rail)",
     ev("declared", FILES,
        "UserControl Composer.Views.Controls.FilesRail: Paper2 panel with a left hairline edge, "
        "20,28 padding, vertical AutoLayout Spacing=14."),
     role="statusRail",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.FilesRail"},
                 "parts": ["eyebrow", "caption", "divider", "file-rows", "divider", "locked-summary"],
                 "widthWhenOpen": 280})

node("content.files.eyebrow", "content", "FILES RAIL", ev("observed", FILES),
     text="FILES RAIL", semanticRole="sectionHeader",
     properties={"uno": {"type": "TextBlock", "styleKey": "MonoEyebrow"}})

node("content.files.caption", "content", "Files caption", ev("observed", FILES),
     text="Each layer emits files as it locks.",
     properties={"uno": {"type": "TextBlock", "fontResourceKey": "SerifLightItalicFontFamily"}})

node("control.files.file-rows", "control", "File row list",
     ev("declared", FILES_CS,
        "ItemsRepeater x:Name=FileRows with a vertical StackLayout (Spacing=2); no ItemsSource in "
        "XAML. Render() builds the rows from Layers.All plus the two synthesized companion files "
        "and assigns them on every FileStatuses change."),
     role="itemsRepeater",
     properties={"uno": {"type": "ItemsRepeater", "xName": "FileRows",
                         "member": "FileRow.For(fileName, status)"}})

node("component.file-row", "component", "File row",
     ev("declared", FILES,
        "ItemTemplate row: 8px status Ellipse, mono file name, mono status badge; Opacity bound "
        "per status."),
     role="statusRow",
     properties={"uno": {"type": "Grid", "member": "Composer.Views.Controls.FileRow"},
                 "parts": ["status-dot", "file-name", "status-badge"],
                 "instanceCount": 10,
                 "files": ["README.md", "ux-flows.md", "architecture.md", "design-system.md",
                           "interaction-spec.md", "data-contracts.md", "implementation-plan.md",
                           "scaffold.command", "README.md", "prompt-context.md"],
                 "bindings": ["DotFill", "DotStroke", "FileName", "StatusBadge", "StatusBadgeBrush", "Opacity"]})

node("content.files.locked-summary", "content", "Locked counter",
     ev("declared", FILES_CS,
        "TextBlock x:Name=LockedSummary; XAML seeds \"0 OF 8 LOCKED\" and Render() rewrites it as "
        "\"{drafted} OF {Layers.All.Length} LOCKED\"."),
     text="0 OF 8 LOCKED",
     properties={"uno": {"type": "TextBlock", "xName": "LockedSummary",
                         "member": "FileStatuses (count of FileStatus.Drafted)"}})

# ---------------------------------------------------------------- center canvas
node("component.active-canvas", "component", "Active canvas (center column)",
     ev("declared", CANVAS,
        "UserControl Composer.Views.Controls.ActiveCanvas, Grid.Column=1, x:Name=CenterCanvas in "
        "Shell.xaml. Hosts the canonical page template: progress, title row, header, locked stack, "
        "canvas slot, footer, future stack."),
     role="contentColumn",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.ActiveCanvas",
                         "xName": "CenterCanvas"}})

node("region.canvas.column", "region", "Canvas column",
     ev("observed", CANVAS,
        "ScrollViewer x:Name=CanvasScrollHost (Padding 32,28,32,40, vertical scroll only) wrapping "
        "utu:AutoLayout x:Name=CanvasColumn, Spacing=20, MinWidth=640 MaxWidth=720, centered."),
     properties={"uno": {"type": "utu:AutoLayout", "xName": "CanvasColumn",
                         "host": "ScrollViewer x:Name=CanvasScrollHost"},
                 "spacing": 20, "minWidth": 640, "maxWidth": 720})

node("component.progress-indicator", "component", "Progress indicator",
     ev("declared", PROGRESS,
        "UserControl Composer.Views.Controls.ProgressIndicator, x:Name=ProgressRegion; three DPs "
        "bound OneWay to ProgressFraction / ProgressLabel / ProgressCounter."),
     role="progress",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.ProgressIndicator",
                         "xName": "ProgressRegion",
                         "member": "Fraction={Binding ProgressFraction}, Label={Binding ProgressLabel}, "
                                   "Counter={Binding ProgressCounter}"},
                 "parts": ["track", "label", "counter"],
                 "motion": "Fill width animated 480ms QuinticEase EaseOut, re-measured on track SizeChanged"})

node("asset.progress.track", "asset", "Progress hairline",
     ev("observed", PROGRESS,
        "Grid x:Name=TrackRoot: 1px hairline Border stretched full width with a 1px amber Border "
        "x:Name=ProgressFill left-aligned at fraction * ActualWidth."),
     role="progressTrack",
     properties={"uno": {"type": "Grid", "xName": "TrackRoot", "fill": "Border x:Name=ProgressFill"},
                 "height": 1})

node("content.progress.label", "content", "Progress label",
     ev("declared", PROGRESS_CS, "TextBlock x:Name=LabelText, set from the Label DP (ShellModel.ProgressLabel: "
                                 "the active layer's uppercase name)."),
     properties={"uno": {"type": "TextBlock", "xName": "LabelText", "member": "ProgressLabel"}})

node("content.progress.counter", "content", "Progress counter",
     ev("declared", PROGRESS_CS, "TextBlock x:Name=CounterText, set from the Counter DP "
                                 "(ShellModel.ProgressCounter: \"01 / 08\" style counter)."),
     properties={"uno": {"type": "TextBlock", "xName": "CounterText", "member": "ProgressCounter"}})

node("component.app-title-row", "component", "App title row",
     ev("declared", CANVAS,
        "UserControl Composer.Views.Controls.AppTitleRow, x:Name=TitleRow, Visibility=Collapsed in "
        "ActiveCanvas.xaml; ProjectName and ShowReset stay bound."),
     role="pageHeader",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.AppTitleRow",
                         "xName": "TitleRow",
                         "member": "ProjectName={Binding ProjectName}, ShowReset={Binding HasLockedLayers}"},
                 "visibility": "Collapsed",
                 "parts": ["project-name", "reset-link"]})

node("content.title-row.project-name", "content", "Project name",
     ev("declared", TITLEROW_CS,
        "TextBlock x:Name=ProjectNameText, 18px Medium sans; set from the ProjectName DP "
        "(ComposerModel.ProjectName = Intent.AppType) with an \"Untitled\" fallback."),
     properties={"uno": {"type": "TextBlock", "xName": "ProjectNameText",
                         "fontResourceKey": "SansFontFamily", "member": "ProjectName"},
                 "fallbackText": "Untitled"})

node("control.title-row.reset", "control", "Reset",
     ev("declared", TITLEROW_CS,
        "Button x:Name=ResetButton, LinkButtonStyle, Content=\"Reset\", "
        "AutomationProperties.Name=\"Reset composition\"; visible only when ShowReset "
        "(HasLockedLayers) is true. OnResetClick invokes the Reset command on the page DataContext."),
     role="button", semanticRole="destructiveAction",
     properties={"uno": {"type": "Button", "xName": "ResetButton", "styleKey": "LinkButtonStyle",
                         "member": "MvuxCommandInvoker.Invoke(dc, \"Reset\")"},
                 "visibility": "Collapsed until HasLockedLayers"})

node("component.active-layer-header", "component", "Active layer header",
     ev("declared", HEADER,
        "UserControl Composer.Views.Controls.ActiveLayerHeader, x:Name=HeaderRegion; six DPs bound "
        "OneWay to ShellModel's ActiveLayer* feeds. Vertical AutoLayout Spacing=6."),
     role="sectionHeader",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.ActiveLayerHeader",
                         "xName": "HeaderRegion",
                         "member": "LayerIndex, LayerLabel, LayerState, Recap, Title, Subtitle"},
                 "parts": ["recap-row", "state-badge"],
                 "collapsedParts": ["LayerLabelText", "TitleText", "SubtitleText"]})

node("content.header.recap", "content", "Layer recap",
     ev("declared", HEADER,
        "utu:AutoLayout x:Name=RecapRow: a mono arrow glyph plus TextBlock x:Name=RecapText "
        "(italic, MaxWidth=560, LineHeight=20) set from the Recap DP."),
     semanticRole="recap",
     properties={"uno": {"type": "TextBlock", "xName": "RecapText", "member": "ActiveLayerRecap"},
                 "prefixGlyph": "↳", "maxWidth": 560, "lineHeight": 20})

node("content.header.state-badge", "content", "Layer state badge",
     ev("declared", HEADER_CS,
        "TextBlock x:Name=StateBadgeText, mono eyebrow size, CharacterSpacing=160, amber; text is "
        "\"EDITED - PREVIEW PENDING\" (Dirty) or \"PREVIEW - REVIEW AND ACCEPT\" (Previewing), "
        "collapsed otherwise."),
     properties={"uno": {"type": "TextBlock", "xName": "StateBadgeText", "member": "ActiveLayerLayerState"}})

node("region.canvas.locked-stack", "region", "Locked context stack",
     ev("observed", CANVAS,
        "utu:AutoLayout x:Name=LockedStack, Spacing=10, above the canvas slot. SyncLockedStack() "
        "fills it with one LockedContextCard per layer whose LayerState is Locked, in Layers.All order."),
     properties={"uno": {"type": "utu:AutoLayout", "xName": "LockedStack",
                         "member": "LayerStates / DefaultExpandedKinds"},
                 "spacing": 10})

node("component.locked-context-card", "component", "Locked context card",
     ev("declared", LOCKED,
        "UserControl Composer.Views.Controls.LockedContextCard: Paper2 Border, 1px hairline, "
        "CornerRadius=4, 20,18 padding. Header row (expand toggle, check + label + LOCKED, Revisit), "
        "italic summary, hairline divider, four-fact grid."),
     role="card",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.LockedContextCard",
                         "member": "LayerKind, Summary, Facts, IsExpanded"},
                 "parts": ["expand-toggle", "header", "revisit", "summary", "divider", "facts-grid"],
                 "factsGrid": "2 columns (120px label / * value) x 4 rows",
                 "instantiatedBy": "ActiveCanvas.SyncLockedStack",
                 "defaultExpanded": "the two most recently locked layers (ShellModel.DefaultExpandedKinds)"})

node("content.locked-card.header", "content", "Locked card header",
     ev("observed", LOCKED,
        "TextBlock of three Runs: a check mark, Run x:Name=HeaderLabelRun (the uppercase layer "
        "label), and a literal \"  ·  LOCKED\"."),
     text="✓ {LAYER}  ·  LOCKED",
     properties={"uno": {"type": "TextBlock", "xName": "HeaderLabelRun"}})

node("content.locked-card.summary", "content", "Locked card summary",
     ev("declared", LOCKED_CS,
        "TextBlock x:Name=SummaryText, italic, set from the Summary DP; ActiveCanvas.LockedSummaryFor "
        "derives the sentence per layer kind (falls back to LayerDef.Hint)."),
     properties={"uno": {"type": "TextBlock", "xName": "SummaryText",
                         "fontResourceKey": "SerifLightItalicFontFamily", "member": "Summary"}})

node("component.info-row", "component", "Fact row",
     ev("declared", LOCKED_CS,
        "Up to four label/value pairs added to Grid x:Name=FactsGrid in Render(): uppercase mono "
        "10px label in column 0, mono 12px value in column 1."),
     role="fact",
     properties={"uno": {"type": "TextBlock", "member": "IList<KeyValuePair<string,string>> Facts"},
                 "parts": ["label", "value"],
                 "instanceCount": 4})

node("control.canvas.slot", "control", "Layer canvas slot",
     ev("declared", CANVAS_CS,
        "ContentControl x:Name=CanvasSlot, MinHeight=480, stretched. SyncSlot() reads the active "
        "LayerKind and assigns the matching per-layer UserControl via CreateCanvas, passing the "
        "shell DataContext down. Region-based navigation was reverted to direct hosting."),
     role="contentHost", semanticRole="swappableContent",
     properties={"uno": {"type": "ContentControl", "xName": "CanvasSlot",
                         "member": "ActiveIndex -> Layers.All[i].Kind -> CreateCanvas(kind)"},
                 "minHeight": 480,
                 "hosts": ["IntentCard", "UXFlowStrip", "ArchitectureBlueprint", "DesignTokenGrid",
                           "InteractionsStateTimeline", "DataContractGrid", "ImplementationPhaseGrid",
                           "ScaffoldTerminal"],
                 "scopeNote": "The hosted per-layer canvases are out of scope for this graph."})

# ---------------------------------------------------------------- composer footer
node("component.composer-footer", "component", "Composer footer",
     ev("declared", FOOTER,
        "UserControl Composer.Views.Controls.ComposerFooter, x:Name=FooterRegion: Paper2 Border, "
        "1px hairline, CornerRadius=6, 20,18 padding, vertical AutoLayout Spacing=14. Two DPs "
        "(State, Prompt) pushed from ActiveCanvas.SyncFooter."),
     role="card", semanticRole="promptComposer",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.ComposerFooter",
                         "xName": "FooterRegion",
                         "member": "State (LayerState), Prompt (string)"},
                 "parts": ["eyebrow", "lead-question", "ack-line", "prompt-input",
                           "suggestions-row", "action-row"]})

node("content.footer.eyebrow", "content", "Composer status eyebrow",
     ev("declared", FOOTER_CS,
        "TextBlock x:Name=EyebrowText, MonoEyebrow style; ApplyState writes "
        "\"COMPOSER · {ComposerStatus.ForLayerState(State)}\" - REFINING / LISTENING / PROPOSING."),
     text="COMPOSER · REFINING",
     properties={"uno": {"type": "TextBlock", "xName": "EyebrowText", "styleKey": "MonoEyebrow",
                         "member": "ComposerStatus.ForLayerState"}})

node("content.footer.lead-question", "content", "Lead question",
     ev("declared", FOOTER_CS,
        "TextBlock x:Name=LeadQuestionText, sans body, LineHeight=22; SyncLeadQuestion reads "
        "ActiveLeadQuestion off the model and collapses the row when empty."),
     properties={"uno": {"type": "TextBlock", "xName": "LeadQuestionText",
                         "member": "ActiveLeadQuestion"}})

node("content.footer.ack", "content", "Preview acknowledgment",
     ev("declared", FOOTER_CS,
        "Border x:Name=AckLine (2px amber left rule, 12,2 padding) wrapping TextBlock x:Name=AckText; "
        "ApplyAck writes \"You asked: ... - here's what changes if I apply that.\" from PreviewAcks[kind]. "
        "Visible only in the Previewing state and only when the ack is non-empty."),
     properties={"uno": {"type": "TextBlock", "xName": "AckText", "member": "PreviewAcks"},
                 "visibility": "Collapsed unless Previewing"})

node("control.footer.prompt-input", "control", "Prompt textarea",
     ev("declared", FOOTER,
        "TextBox x:Name=PromptInput, ChatInputTextBoxStyle, PlaceholderText=\"Refine, or accept "
        "what's drawn…\", AcceptsReturn, MinHeight=68, AutomationProperties.Name=\"Composer prompt\". "
        "TextChanged invokes SetActivePrompt; Ctrl+Enter runs the primary action; Esc discards a preview."),
     role="textbox", semanticRole="promptInput",
     properties={"uno": {"type": "TextBox", "xName": "PromptInput", "styleKey": "ChatInputTextBoxStyle",
                         "member": "SetActivePrompt / DiscardPreview"},
                 "placeholderText": "Refine, or accept what's drawn…",
                 "minHeight": 68,
                 "keyboard": ["Ctrl+Enter -> primary action", "Esc (Previewing) -> DiscardPreview"]})

node("region.footer.suggestions", "region", "Suggestion row",
     ev("observed", FOOTER,
        "Three-column Grid below the textarea: TRY label, utu:AutoLayout x:Name=ChipsRow "
        "(Spacing=8), keyboard hint."),
     properties={"uno": {"type": "utu:AutoLayout", "xName": "ChipsRow"}})

node("content.footer.try-label", "content", "TRY", ev("observed", FOOTER),
     text="TRY", semanticRole="label",
     properties={"uno": {"type": "TextBlock", "fontResourceKey": "MonoFontFamily"}})

node("component.suggestion-chip", "component", "Suggestion chip",
     ev("declared", FOOTER_CS,
        "RenderChips() builds one Button per string in ComposerModel.SuggestionChips[activeKind] - "
        "transparent fill, hairline border, CornerRadius=4, 10,5 padding, mono 11px Medium. Click "
        "sets the textarea text, moves the caret to the end, focuses, and invokes SetActivePrompt."),
     role="button", semanticRole="promptSuggestion",
     properties={"uno": {"type": "Button", "member": "ComposerModel.SuggestionChips"},
                 "instanceCount": 3,
                 "instanceNote": "three chips per layer, swapped when the active layer changes",
                 "exampleValues": ["Mobile-first", "Offline-first", "No backend yet"]})

node("content.footer.kbd-hint", "content", "Submit hint", ev("observed", FOOTER),
     text="⌘  ↵  TO SUBMIT",
     properties={"uno": {"type": "TextBlock", "xName": "KbdHint"}})

node("region.footer.actions", "region", "Action row",
     ev("observed", FOOTER,
        "Four-column Grid: primary button, italic hint, spacer, right-aligned discard links "
        "(AutoLayout Spacing=14)."))

node("control.footer.primary", "control", "Primary action",
     ev("declared", FOOTER_CS,
        "Button x:Name=PrimaryButton, InkButtonStyle by default, AutomationProperties.Name=\"Lock and "
        "continue\". ApplyState rewrites Content and Style per LayerState; TriggerPrimary maps the "
        "resolved state to LockAndContinue / GeneratePreview / AcceptAndLock."),
     role="button", semanticRole="primaryAction",
     properties={"uno": {"type": "Button", "xName": "PrimaryButton", "styleKey": "InkButtonStyle",
                         "member": "LockAndContinue | GeneratePreview | AcceptAndLock"},
                 "labels": {"Clean": "Lock and continue → (Continue → on Intent)",
                            "Dirty": "Generate preview →",
                            "Previewing": "Accept and lock →"}})

node("content.footer.primary-hint", "content", "Primary action hint",
     ev("declared", FOOTER_CS,
        "TextBlock x:Name=PrimaryHintText, italic; ApplyState writes \"accepting the recommendation\" "
        "(Clean) / \"with your edits\" (Dirty) / \"the AI's pass\" (Previewing). Mirrors "
        "ShellModel.ActivePrimaryHint."),
     text="accepting the recommendation",
     properties={"uno": {"type": "TextBlock", "xName": "PrimaryHintText"}})

node("control.footer.discard-edits", "control", "Discard edits",
     ev("declared", FOOTER,
        "Button x:Name=DiscardEditsButton, LinkButtonStyle, Content=\"Discard edits\", collapsed by "
        "default; visible only in the Dirty state. Invokes DiscardEdits and clears the textarea."),
     role="button",
     properties={"uno": {"type": "Button", "xName": "DiscardEditsButton", "styleKey": "LinkButtonStyle",
                         "member": "DiscardEdits"}})

node("control.footer.discard-preview", "control", "Discard preview",
     ev("declared", FOOTER,
        "Button x:Name=DiscardPreviewButton, LinkButtonStyle, Content=\"← Discard preview\", "
        "collapsed by default; visible only in the Previewing state. Invokes DiscardPreview."),
     role="button",
     properties={"uno": {"type": "Button", "xName": "DiscardPreviewButton", "styleKey": "LinkButtonStyle",
                         "member": "DiscardPreview"}})

# ---------------------------------------------------------------- future stack
node("region.canvas.future-stack", "region", "Future preview stack",
     ev("observed", CANVAS,
        "utu:AutoLayout x:Name=FutureStack, Spacing=8, below the footer. SyncFutureStack() adds one "
        "FuturePreviewCard per layer after the active one at opacity max(0.05, 0.40 - distance*0.08)."),
     properties={"uno": {"type": "utu:AutoLayout", "xName": "FutureStack",
                         "member": "ActiveIndex / RailsVisible"},
                 "spacing": 8,
                 "opacityRule": "max(0.05, 0.40 - distance * 0.08)"})

node("component.future-preview-card", "component", "Future preview card",
     ev("declared", FUTURE,
        "UserControl Composer.Views.Controls.FuturePreviewCard, IsHitTestVisible=False: dashed "
        "hairline outline over 20,14 padding with a mono header and italic hint."),
     role="card", semanticRole="upcomingLayer",
     properties={"uno": {"type": "UserControl", "class": "Composer.Views.Controls.FuturePreviewCard",
                         "member": "LayerLabel, Hint"},
                 "parts": ["outline", "header", "hint"],
                 "instantiatedBy": "ActiveCanvas.SyncFutureStack",
                 "hitTestVisible": False})

node("asset.future-card.outline", "asset", "Dashed card outline",
     ev("observed", FUTURE,
        "Rectangle with hairline Stroke, StrokeThickness=1, StrokeDashArray=3,3, RadiusX/Y=4, "
        "Fill=Transparent - laid under the content because Border.BorderBrush has no dash support."),
     role="decoration",
     properties={"uno": {"type": "Rectangle", "property": "StrokeDashArray=3,3"}})

node("content.future-card.header", "content", "Future card header",
     ev("declared", FUTURE_CS, "TextBlock x:Name=HeaderText; Render() writes \"{LayerLabel}  ·  UPCOMING\"."),
     text="{LAYER}  ·  UPCOMING",
     properties={"uno": {"type": "TextBlock", "xName": "HeaderText"}})

node("content.future-card.hint", "content", "Future card hint",
     ev("declared", FUTURE_CS, "TextBlock x:Name=HintText, italic; set from the Hint DP (LayerDef.Hint)."),
     properties={"uno": {"type": "TextBlock", "xName": "HintText", "member": "LayerDef.Hint"}})

# ---------------------------------------------------------------- states
node("state.composer-shell.rails-hidden", "state", "Rails hidden (focused first)",
     ev("declared", SHELL_CS,
        "Initial condition: ShellModel.RailsVisible is false while no layer is locked and "
        "ActiveIndex is 0. Both rail columns are Width=0, both containers Opacity=0, "
        "translated -40 / +40."),
     semanticRole="focusMode",
     properties={"uno": {"mechanism": "code-behind", "member": "ShellModel.RailsVisible",
                         "storyboardKey": "RailsHideStoryboard"},
                 "columnWidth": 0,
                 "motion": "X to -40 / +40 over 320ms QuinticEase EaseOut; Opacity to 0 over 240ms",
                 "reducedMotion": "MotionPreferences.AnimationsEnabled false -> snap without storyboard"})

node("state.composer-shell.rails-open", "state", "Rails open",
     ev("declared", SHELL_CS,
        "SyncRailsAnimation snaps both ColumnDefinition.Width to 280px (Grid columns don't "
        "re-measure smoothly under DoubleAnimation on Skia desktop) and runs RailsRevealStoryboard."),
     properties={"uno": {"mechanism": "code-behind", "member": "ShellModel.RailsVisible",
                         "storyboardKey": "RailsRevealStoryboard"},
                 "columnWidth": 280,
                 "motion": "X to 0 over 480ms QuinticEase EaseOut; Opacity to 1 over 320ms after a 160ms delay",
                 "trigger": "first lock, or any advance past layer 0"})

node("state.active-canvas.focused-first", "state", "Canvas focused-first dimensions",
     ev("declared", CANVAS_CS,
        "SyncFocusedFirst: when RailsVisible is false the canvas column is MinWidth 640 / MaxWidth 720 "
        "and the scroll host's top padding is 64."),
     properties={"uno": {"mechanism": "code-behind", "member": "RailsVisible / IsFocusedFirst"},
                 "minWidth": 640, "maxWidth": 720, "padding": "32,64,32,40"})

node("state.active-canvas.rails-open", "state", "Canvas rails-open dimensions",
     ev("declared", CANVAS_CS,
        "SyncFocusedFirst: when RailsVisible is true the column animates to MinWidth 820 / MaxWidth 880 "
        "over 480ms QuinticEase EaseOut and the top padding snaps to 28 (padding doesn't animate "
        "cleanly on Skia)."),
     properties={"uno": {"mechanism": "code-behind", "member": "RailsVisible / IsFocusedFirst"},
                 "minWidth": 820, "maxWidth": 880, "padding": "32,28,32,40",
                 "motion": "480ms QuinticEase EaseOut on MinWidth and MaxWidth (pooled Storyboard)"})

node("state.future-stack.empty", "state", "Future stack empty",
     ev("declared", CANVAS_CS,
        "SyncFutureStack clears the stack and returns early while RailsVisible is false, so no "
        "future cards render on the first screen."),
     semanticRole="empty",
     properties={"uno": {"mechanism": "code-behind", "member": "RailsVisible"}})

node("state.future-card.entering", "state", "Future card entrance",
     ev("declared", CANVAS_CS,
        "Each card is added at Opacity 0 and animated to its target opacity over 480ms QuinticEase "
        "EaseOut, matching the rail-reveal rhythm; with animations disabled it is added at the "
        "target opacity."),
     semanticRole="entering",
     properties={"uno": {"mechanism": "code-behind", "member": "MotionPreferences.AnimationsEnabled"},
                 "motion": "Opacity 0 -> max(0.05, 0.40 - distance*0.08) over 480ms QuinticEase EaseOut"})

node("state.composer-footer.clean", "state", "Clean (REFINING)",
     ev("declared", FOOTER_CS,
        "LayerState.Clean: eyebrow REFINING, primary \"Lock and continue →\" (\"Continue →\" on the "
        "Intent layer) with InkButtonStyle, both discard links and the ack line collapsed. "
        "LayerState.Locked renders the same arm."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ComposerFooter.State"},
                 "appliesTo": ["LayerState.Clean", "LayerState.Locked"],
                 "eyebrow": "COMPOSER · REFINING",
                 "primaryHint": "accepting the recommendation"})

node("state.composer-footer.dirty", "state", "Dirty (LISTENING)",
     ev("declared", FOOTER_CS,
        "LayerState.Dirty: eyebrow LISTENING, primary \"Generate preview →\" restyled to "
        "AmberButtonStyle, Discard edits visible, ack line collapsed."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ComposerFooter.State",
                         "styleKey": "AmberButtonStyle"},
                 "eyebrow": "COMPOSER · LISTENING",
                 "primaryHint": "with your edits"})

node("state.composer-footer.previewing", "state", "Previewing (PROPOSING)",
     ev("declared", FOOTER_CS,
        "LayerState.Previewing: eyebrow PROPOSING, primary \"Accept and lock →\" with InkButtonStyle, "
        "Discard preview visible, and the amber-ruled acknowledgment line shown when PreviewAcks "
        "holds a quote for the layer."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ComposerFooter.State"},
                 "eyebrow": "COMPOSER · PROPOSING",
                 "primaryHint": "the AI's pass"})

node("state.composer-footer.hidden", "state", "Footer hidden on Scaffold",
     ev("declared", CANVAS_CS,
        "SyncFooter collapses the whole footer when the active layer kind is Scaffold - that canvas "
        "owns its own terminal Download-bundle action, so the standard Continue affordance would "
        "lead nowhere."),
     semanticRole="hidden",
     properties={"uno": {"mechanism": "code-behind", "member": "Layers.All[ActiveIndex].Kind == LayerKind.Scaffold"}})

node("state.layer-header.edited", "state", "Edited badge",
     ev("declared", HEADER_CS, "LayerState.Dirty renders the amber badge \"EDITED - PREVIEW PENDING\"."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ActiveLayerHeader.LayerState"},
                 "text": "EDITED — PREVIEW PENDING"})

node("state.layer-header.preview", "state", "Preview badge",
     ev("declared", HEADER_CS, "LayerState.Previewing renders the amber badge \"PREVIEW - REVIEW AND ACCEPT\"."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ActiveLayerHeader.LayerState"},
                 "text": "PREVIEW — REVIEW AND ACCEPT"})

node("state.layer-header.no-recap", "state", "Recap row collapsed",
     ev("declared", HEADER_CS,
        "The recap row collapses when the Recap DP is null or whitespace - the cold-launch Intent "
        "layer has no prior decision to recap."),
     properties={"uno": {"mechanism": "dependency-property", "member": "ActiveLayerHeader.Recap"}})

node("state.layer-row.active", "state", "Row active",
     ev("declared", STACK_CS,
        "LayerRow.For(isActive): amber 2px left border, full opacity."),
     semanticRole="selected",
     properties={"uno": {"mechanism": "binding", "member": "ActiveIndex"}, "opacity": 1.0})

node("state.layer-row.locked", "state", "Row locked",
     ev("declared", STACK_CS,
        "LayerRow.For(isLocked): Ink2 2px left border and a check-mark glyph in the trailing column."),
     properties={"uno": {"mechanism": "binding", "member": "LayerStates[kind] == LayerState.Locked"},
                 "glyph": "✓"})

node("state.layer-row.upcoming", "state", "Row upcoming",
     ev("declared", STACK_CS,
        "LayerRow.For(dimmed): rows after the active index that are neither active nor locked get a "
        "transparent left border and 0.42 opacity."),
     properties={"uno": {"mechanism": "binding", "member": "index > ActiveIndex"}, "opacity": 0.42})

node("state.file-row.planned", "state", "File planned",
     ev("declared", FILES_CS,
        "FileRow.For(FileStatus.Planned): hollow Ink4-stroked dot, PLANNED badge in Ink4, 0.42 opacity."),
     properties={"uno": {"mechanism": "binding", "member": "FileStatuses[kind]"},
                 "badge": "PLANNED", "opacity": 0.42})

node("state.file-row.writing", "state", "File writing",
     ev("declared", FILES_CS,
        "FileRow.For(FileStatus.Writing): indigo dot and WRITING badge for the currently active layer, "
        "full opacity."),
     properties={"uno": {"mechanism": "binding", "member": "FileStatuses[kind]"},
                 "badge": "WRITING", "opacity": 1.0})

node("state.file-row.drafted", "state", "File drafted",
     ev("declared", FILES_CS,
        "FileRow.For(FileStatus.Drafted): amber dot and DRAFTED badge once the layer is locked, "
        "full opacity."),
     properties={"uno": {"mechanism": "binding", "member": "FileStatuses[kind]"},
                 "badge": "DRAFTED", "opacity": 1.0})

node("state.locked-card.collapsed", "state", "Card collapsed",
     ev("declared", LOCKED_CS,
        "ApplyExpansion hides the summary, divider and facts grid and flips the toggle glyph to +. "
        "Cards default to expanded only for the two most recently locked layers."),
     properties={"uno": {"mechanism": "dependency-property", "member": "LockedContextCard.IsExpanded"},
                 "toggleGlyph": {"expanded": "−", "collapsed": "+"}})

# ---------------------------------------------------------------- tokens
TOKENS_DECLARED = [
    ("token.color.glass-backdrop", "Glass backdrop (page background)", "color",
     "#FFE3ECF7 -> #FFF1F4F9 -> #FFFAF8F1", "GlassBackdropBrush", "LinearGradientBrush"),
    ("token.color.paper", "Paper (on-ink foreground)", "color", "#FFFFFF", "PaperBrush", "SolidColorBrush"),
    ("token.color.paper2", "Paper 2 (panel surface)", "color", "#FAFAFA", "Paper2Brush", "SolidColorBrush"),
    ("token.color.ink", "Ink (primary text)", "color", "#1A1A1A", "InkBrush", "SolidColorBrush"),
    ("token.color.ink2", "Ink 2 (secondary text)", "color", "#3A3A3A", "Ink2Brush", "SolidColorBrush"),
    ("token.color.ink3", "Ink 3 (tertiary text)", "color", "#737373", "Ink3Brush", "SolidColorBrush"),
    ("token.color.ink4", "Ink 4 (lowest-emphasis text)", "color", "#A3A3A3", "Ink4Brush", "SolidColorBrush"),
    ("token.color.hairline", "Hairline (borders and rules)", "color", "#ECECEC", "HairlineBrush", "SolidColorBrush"),
    ("token.color.amber", "Amber (active / locked accent)", "color", "#C89C3F", "AmberBrush", "SolidColorBrush"),
    ("token.color.indigo", "Indigo (in-progress accent)", "color", "#3D3DFF", "IndigoBrush", "SolidColorBrush"),
    ("token.color.transparent", "Transparent", "color", "Transparent", "TransparentBrush", "SolidColorBrush"),
]
for tid, tname, cat, value, key, rtype in TOKENS_DECLARED:
    node(tid, "token", tname,
         ev("declared", TOKENS, "Declared in Themes/Tokens.xaml (Default theme dictionary) and consumed by this surface."),
         category=cat, value=value,
         properties={"uno": {"resourceKey": key, "resourceType": rtype}})

TOKENS_TYPO = [
    ("token.typography.mono", "Mono face (JetBrains Mono)", "typography",
     "ms-appx:///Assets/Fonts/JetBrains_Mono/JetBrainsMono-VariableFont.ttf#JetBrains Mono, Cascadia Mono, Consolas, monospace",
     {"resourceKey": "MonoFontFamily", "resourceType": "FontFamily"}),
    ("token.typography.sans", "Sans face (Inter)", "typography",
     "ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif",
     {"resourceKey": "SansFontFamily", "resourceType": "FontFamily"}),
    ("token.typography.serif", "Prompt input face", "typography",
     "ms-appx:///Assets/Fonts/Inter/InterVariable.ttf#Inter, Bahnschrift, Segoe UI Variable Display, Segoe UI, sans-serif",
     {"resourceKey": "SerifFontFamily", "resourceType": "FontFamily"}),
    ("token.typography.serif-italic", "Italic voice face", "typography",
     "ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif",
     {"resourceKey": "SerifItalicFontFamily", "resourceType": "FontFamily"}),
    ("token.typography.serif-light-italic", "Light italic voice face", "typography",
     "ms-appx:///Assets/Fonts/Inter/InterVariable-Italic.ttf#Inter, Bahnschrift, Segoe UI, sans-serif",
     {"resourceKey": "SerifLightItalicFontFamily", "resourceType": "FontFamily"}),
    ("token.typography.mono-eyebrow", "Mono eyebrow style", "typography",
     {"family": "MonoFontFamily", "size": 11, "weight": "Medium", "tracking": 180,
      "lineHeight": 13, "foreground": "Ink3Brush", "numerals": "Tabular"},
     {"styleKey": "MonoEyebrow", "resourceType": "Style(TextBlock)"}),
    ("token.font-size.eyebrow-micro", "Eyebrow micro size", "typography", 10,
     {"resourceKey": "TypeEyebrowMicroSize", "resourceType": "x:Double"}),
    ("token.font-size.eyebrow", "Eyebrow size", "typography", 11,
     {"resourceKey": "TypeEyebrowSize", "resourceType": "x:Double"}),
    ("token.font-size.label", "Label size", "typography", 12,
     {"resourceKey": "TypeLabelSize", "resourceType": "x:Double"}),
    ("token.font-size.body-small", "Body small size", "typography", 14,
     {"resourceKey": "TypeBodySmallSize", "resourceType": "x:Double"}),
    ("token.font-size.body", "Body size", "typography", 16,
     {"resourceKey": "TypeBodySize", "resourceType": "x:Double"}),
]
for tid, tname, cat, value, uno in TOKENS_TYPO:
    node(tid, "token", tname,
         ev("declared", TYPO, "Declared in Themes/Typography.xaml and consumed by this surface."),
         category=cat, value=value, properties={"uno": uno})

node("token.spacing.14", "token", "Panel rhythm (14)",
     ev("derived", STACK,
        "utu:AutoLayout Spacing=14 recurs on both rails and the composer footer - the panel-level "
        "vertical rhythm. The Spacing14/Space14 resources exist in Tokens.xaml but this surface "
        "writes the literal."),
     category="spacing", value=14, properties={"unit": "px"})

node("token.radius.4", "token", "Small radius (4)",
     ev("derived", LOCKED,
        "CornerRadius=4 on the locked context card, RadiusX/Y=4 on the future preview card outline, "
        "and CornerRadius=4 on the code-built suggestion chips. Written as literals, not via the "
        "declared Corner3/Corner6/CornerPill resources."),
     category="radius", value=4, properties={"unit": "px"})

# ---------------------------------------------------------------- containment
contains("screen.composer-shell", "region.composer-shell.workspace", SHELL)
contains("region.composer-shell.workspace", "region.composer-shell.left-rail", SHELL)
contains("region.composer-shell.workspace", "component.active-canvas", SHELL)
contains("region.composer-shell.workspace", "region.composer-shell.right-rail", SHELL)
contains("region.composer-shell.left-rail", "component.composition-stack", SHELL)
contains("region.composer-shell.right-rail", "component.files-rail", SHELL)

contains("component.composition-stack", "content.stack.eyebrow", STACK)
contains("component.composition-stack", "content.stack.caption", STACK)
contains("component.composition-stack", "control.stack.layer-rows", STACK)
contains("control.stack.layer-rows", "component.layer-row", STACK)

contains("component.files-rail", "content.files.eyebrow", FILES)
contains("component.files-rail", "content.files.caption", FILES)
contains("component.files-rail", "control.files.file-rows", FILES)
contains("control.files.file-rows", "component.file-row", FILES)
contains("component.files-rail", "content.files.locked-summary", FILES)

contains("component.active-canvas", "region.canvas.column", CANVAS)
for child in ("component.progress-indicator", "component.app-title-row", "component.active-layer-header",
              "region.canvas.locked-stack", "control.canvas.slot", "component.composer-footer",
              "region.canvas.future-stack"):
    contains("region.canvas.column", child, CANVAS)

contains("component.progress-indicator", "asset.progress.track", PROGRESS)
contains("component.progress-indicator", "content.progress.label", PROGRESS)
contains("component.progress-indicator", "content.progress.counter", PROGRESS)

contains("component.app-title-row", "content.title-row.project-name", TITLEROW)
contains("component.app-title-row", "control.title-row.reset", TITLEROW)

contains("component.active-layer-header", "content.header.recap", HEADER)
contains("component.active-layer-header", "content.header.state-badge", HEADER)

contains("region.canvas.locked-stack", "component.locked-context-card", CANVAS_CS,
         "SyncLockedStack instantiates one card per locked layer.")
contains("component.locked-context-card", "control.locked-card.expand-toggle", LOCKED)
contains("component.locked-context-card", "content.locked-card.header", LOCKED)
contains("component.locked-context-card", "control.locked-card.revisit", LOCKED)
contains("component.locked-context-card", "content.locked-card.summary", LOCKED)
contains("component.locked-context-card", "component.info-row", LOCKED_CS,
         "Render() adds up to four label/value pairs into FactsGrid.")

contains("component.composer-footer", "content.footer.eyebrow", FOOTER)
contains("component.composer-footer", "content.footer.lead-question", FOOTER)
contains("component.composer-footer", "content.footer.ack", FOOTER)
contains("component.composer-footer", "control.footer.prompt-input", FOOTER)
contains("component.composer-footer", "region.footer.suggestions", FOOTER)
contains("component.composer-footer", "region.footer.actions", FOOTER)
contains("region.footer.suggestions", "content.footer.try-label", FOOTER)
contains("region.footer.suggestions", "component.suggestion-chip", FOOTER_CS,
         "RenderChips fills ChipsRow with one Button per suggestion.")
contains("region.footer.suggestions", "content.footer.kbd-hint", FOOTER)
contains("region.footer.actions", "control.footer.primary", FOOTER)
contains("region.footer.actions", "content.footer.primary-hint", FOOTER)
contains("region.footer.actions", "control.footer.discard-edits", FOOTER)
contains("region.footer.actions", "control.footer.discard-preview", FOOTER)

contains("region.canvas.future-stack", "component.future-preview-card", CANVAS_CS,
         "SyncFutureStack instantiates one card per upcoming layer.")
contains("component.future-preview-card", "asset.future-card.outline", FUTURE)
contains("component.future-preview-card", "content.future-card.header", FUTURE)
contains("component.future-preview-card", "content.future-card.hint", FUTURE)

# ---------------------------------------------------------------- controls that need declared missing nodes
node("control.locked-card.expand-toggle", "control", "Expand toggle",
     ev("declared", LOCKED,
        "Button x:Name=ExpandToggle, LinkButtonStyle, Content is the minus/plus glyph; "
        "OnExpandToggleClick flips IsExpanded locally."),
     role="button", semanticRole="disclosure",
     properties={"uno": {"type": "Button", "xName": "ExpandToggle", "styleKey": "LinkButtonStyle",
                         "member": "LockedContextCard.IsExpanded"}})

node("control.locked-card.revisit", "control", "Revisit",
     ev("declared", LOCKED_CS,
        "Button x:Name=RevisitButton, LinkButtonStyle, Content=\"Revisit\"; OnRevisitClick invokes "
        "Revisit(LayerKind), which jumps ActiveIndex to that layer and resets its state to Clean "
        "(non-destructive)."),
     role="button",
     properties={"uno": {"type": "Button", "xName": "RevisitButton", "styleKey": "LinkButtonStyle",
                         "member": "MvuxCommandInvoker.Invoke(dc, \"Revisit\", LayerKind)"}})

# ---------------------------------------------------------------- states wiring
edge("screen.composer-shell", "has-state", "state.composer-shell.rails-hidden",
     ev("declared", SHELL_CS, "Initial presentation - the rails start at zero width."))
edge("screen.composer-shell", "has-state", "state.composer-shell.rails-open",
     ev("declared", SHELL_CS))
edge("component.active-canvas", "has-state", "state.active-canvas.focused-first",
     ev("declared", CANVAS_CS))
edge("component.active-canvas", "has-state", "state.active-canvas.rails-open",
     ev("declared", CANVAS_CS))
edge("region.canvas.future-stack", "has-state", "state.future-stack.empty",
     ev("declared", CANVAS_CS))
edge("component.future-preview-card", "has-state", "state.future-card.entering",
     ev("declared", CANVAS_CS))
for s in ("clean", "dirty", "previewing", "hidden"):
    edge("component.composer-footer", "has-state", f"state.composer-footer.{s}",
         ev("declared", FOOTER_CS if s != "hidden" else CANVAS_CS))
for s in ("edited", "preview", "no-recap"):
    edge("component.active-layer-header", "has-state", f"state.layer-header.{s}",
         ev("declared", HEADER_CS))
for s in ("active", "locked", "upcoming"):
    edge("component.layer-row", "has-state", f"state.layer-row.{s}", ev("declared", STACK_CS))
for s in ("planned", "writing", "drafted"):
    edge("component.file-row", "has-state", f"state.file-row.{s}", ev("declared", FILES_CS))
edge("component.locked-context-card", "has-state", "state.locked-card.collapsed",
     ev("declared", LOCKED_CS))

# ---------------------------------------------------------------- triggers
edge("component.layer-row", "triggers", "state.composer-shell.rails-open",
     ev("declared", STACK_CS,
        "OnLayerRowClick invokes Jump(row.Index) on the page DataContext; ShellModel.RailsVisible is "
        "lockedLayers.Count > 0 || ActiveIndex > 0, so jumping to any layer past the first opens the rails. "
        "Attached once at the canonical - every row behaves identically."))
edge("control.title-row.reset", "triggers", "state.composer-shell.rails-hidden",
     ev("declared", MODEL,
        "Reset() sets every layer back to Clean and ActiveIndex to 0, which makes RailsVisible false "
        "and runs RailsHideStoryboard."))
edge("control.footer.prompt-input", "triggers", "state.composer-footer.dirty",
     ev("declared", MODEL,
        "TextChanged invokes SetActivePrompt, which marks the active layer Dirty once the text is non-empty."))
edge("component.suggestion-chip", "triggers", "state.composer-footer.dirty",
     ev("declared", FOOTER_CS,
        "Chip click writes the chip text into the textarea and invokes SetActivePrompt -> MarkDirty. "
        "Attached once at the canonical - all chips behave identically."))
edge("control.footer.primary", "triggers", "state.composer-footer.previewing",
     ev("declared", MODEL,
        "In the Dirty state TriggerPrimary invokes GeneratePreview, which stages the proposal and sets "
        "the layer to Previewing."))
edge("control.footer.primary", "triggers", "state.layer-row.locked",
     ev("declared", MODEL,
        "LockAndContinue / AcceptAndLock set the layer to LayerState.Locked and advance ActiveIndex; the "
        "composition stack re-renders that row with the check glyph and the Ink2 rule."))
edge("control.footer.primary", "triggers", "state.file-row.drafted",
     ev("declared", SHELLMODEL,
        "The same lock flips FileStatuses for the layer to Drafted (ShellModel derives it from LockedLayers), "
        "which the files rail renders as an amber dot and DRAFTED badge."))
edge("control.footer.discard-edits", "triggers", "state.composer-footer.clean",
     ev("declared", MODEL, "DiscardEdits restores the pre-edit snapshot, clears the prompt and sets the layer Clean."))
edge("control.footer.discard-preview", "triggers", "state.composer-footer.clean",
     ev("declared", MODEL, "DiscardPreview restores the snapshot, clears the prompt and sets the layer Clean."))
edge("control.locked-card.expand-toggle", "triggers", "state.locked-card.collapsed",
     ev("declared", LOCKED_CS, "OnExpandToggleClick inverts IsExpanded, which drives ApplyExpansion."))

# ---------------------------------------------------------------- token wiring
uses("screen.composer-shell", "token.color.glass-backdrop", SHELL, "background")

uses("component.composition-stack", "token.color.paper2", STACK, "background")
uses("component.composition-stack", "token.color.hairline", STACK, "borderBrush")
uses("component.composition-stack", "token.spacing.14", STACK, "spacing")
uses("component.files-rail", "token.color.paper2", FILES, "background")
uses("component.files-rail", "token.color.hairline", FILES, "borderBrush")
uses("component.files-rail", "token.spacing.14", FILES, "spacing")

uses("content.stack.eyebrow", "token.typography.mono-eyebrow", STACK, "textStyle")
uses("content.files.eyebrow", "token.typography.mono-eyebrow", FILES, "textStyle")
uses("content.stack.caption", "token.typography.serif-light-italic", STACK, "fontFamily")
uses("content.stack.caption", "token.font-size.body-small", STACK, "fontSize")
uses("content.stack.caption", "token.color.ink3", STACK, "foreground")
uses("content.files.caption", "token.typography.serif-light-italic", FILES, "fontFamily")
uses("content.files.caption", "token.font-size.body-small", FILES, "fontSize")
uses("content.files.caption", "token.color.ink3", FILES, "foreground")
uses("content.files.locked-summary", "token.typography.mono", FILES, "fontFamily")
uses("content.files.locked-summary", "token.font-size.eyebrow", FILES, "fontSize")
uses("content.files.locked-summary", "token.color.ink3", FILES, "foreground")

uses("component.layer-row", "token.typography.mono", STACK, "indexAndLabelFont")
uses("component.layer-row", "token.typography.serif-light-italic", STACK, "hintFont")
uses("component.layer-row", "token.font-size.eyebrow", STACK, "indexAndLabelSize")
uses("component.layer-row", "token.font-size.label", STACK, "hintAndGlyphSize")
uses("component.layer-row", "token.color.ink", STACK, "labelForeground")
uses("component.layer-row", "token.color.ink3", STACK, "indexAndHintForeground")
uses("state.layer-row.active", "token.color.amber", STACK_CS, "leftBorderBrush")
uses("state.layer-row.locked", "token.color.ink2", STACK_CS, "leftBorderBrush")

uses("component.file-row", "token.typography.mono", FILES, "fontFamily")
uses("component.file-row", "token.font-size.label", FILES, "fileNameSize")
uses("component.file-row", "token.font-size.eyebrow-micro", FILES, "badgeSize")
uses("component.file-row", "token.color.ink", FILES, "fileNameForeground")
uses("state.file-row.planned", "token.color.ink4", FILES_CS, "dotStrokeAndBadge")
uses("state.file-row.writing", "token.color.indigo", FILES_CS, "dotAndBadge")
uses("state.file-row.drafted", "token.color.amber", FILES_CS, "dotAndBadge")

uses("asset.progress.track", "token.color.hairline", PROGRESS, "trackBackground")
uses("asset.progress.track", "token.color.amber", PROGRESS, "fillBackground")
uses("content.progress.label", "token.typography.mono", PROGRESS, "fontFamily")
uses("content.progress.label", "token.font-size.eyebrow", PROGRESS, "fontSize")
uses("content.progress.label", "token.color.ink3", PROGRESS, "foreground")
uses("content.progress.counter", "token.color.ink4", PROGRESS, "foreground")

uses("content.title-row.project-name", "token.typography.sans", TITLEROW, "fontFamily")
uses("content.title-row.project-name", "token.color.ink", TITLEROW, "foreground")

uses("content.header.recap", "token.typography.serif-light-italic", HEADER, "fontFamily")
uses("content.header.recap", "token.font-size.body-small", HEADER, "fontSize")
uses("content.header.recap", "token.color.ink3", HEADER, "foreground")
uses("content.header.recap", "token.color.ink4", HEADER, "arrowGlyphForeground")
uses("content.header.state-badge", "token.typography.mono", HEADER, "fontFamily")
uses("content.header.state-badge", "token.font-size.eyebrow", HEADER, "fontSize")
uses("content.header.state-badge", "token.color.amber", HEADER, "foreground")

uses("component.composer-footer", "token.color.paper2", FOOTER, "background")
uses("component.composer-footer", "token.color.hairline", FOOTER, "borderBrush")
uses("component.composer-footer", "token.spacing.14", FOOTER, "spacing")
uses("content.footer.eyebrow", "token.typography.mono-eyebrow", FOOTER, "textStyle")
uses("content.footer.lead-question", "token.typography.sans", FOOTER, "fontFamily")
uses("content.footer.lead-question", "token.font-size.body", FOOTER, "fontSize")
uses("content.footer.lead-question", "token.color.ink2", FOOTER, "foreground")
uses("content.footer.ack", "token.typography.serif-italic", FOOTER, "fontFamily")
uses("content.footer.ack", "token.color.ink2", FOOTER, "foreground")
uses("content.footer.ack", "token.color.amber", FOOTER, "leftRuleBrush")
uses("control.footer.prompt-input", "token.typography.serif", CHIPSTYLES, "fontFamily")
uses("control.footer.prompt-input", "token.font-size.body", CHIPSTYLES, "fontSize")
uses("control.footer.prompt-input", "token.color.ink", CHIPSTYLES, "foreground")
uses("control.footer.prompt-input", "token.color.ink4", CHIPSTYLES, "placeholderForeground")
uses("content.footer.try-label", "token.typography.mono", FOOTER, "fontFamily")
uses("content.footer.try-label", "token.font-size.eyebrow", FOOTER, "fontSize")
uses("content.footer.try-label", "token.color.ink3", FOOTER, "foreground")
uses("component.suggestion-chip", "token.typography.mono", FOOTER_CS, "fontFamily")
uses("component.suggestion-chip", "token.color.transparent", FOOTER_CS, "background")
uses("component.suggestion-chip", "token.color.hairline", FOOTER_CS, "borderBrush")
uses("component.suggestion-chip", "token.color.ink2", FOOTER_CS, "foreground")
uses("component.suggestion-chip", "token.radius.4", FOOTER_CS, "cornerRadius")
uses("content.footer.kbd-hint", "token.font-size.eyebrow-micro", FOOTER, "fontSize")
uses("content.footer.kbd-hint", "token.color.ink4", FOOTER, "foreground")
uses("control.footer.primary", "token.color.ink", CHIPSTYLES, "background")
uses("control.footer.primary", "token.color.paper", CHIPSTYLES, "foreground")
uses("control.footer.primary", "token.typography.mono", CHIPSTYLES, "fontFamily")
uses("control.footer.primary", "token.font-size.eyebrow-micro", CHIPSTYLES, "fontSize")
uses("state.composer-footer.dirty", "token.color.amber", CTXSTYLES, "primaryButtonBackground")
uses("content.footer.primary-hint", "token.typography.serif-light-italic", FOOTER, "fontFamily")
uses("content.footer.primary-hint", "token.font-size.body-small", FOOTER, "fontSize")
uses("content.footer.primary-hint", "token.color.ink3", FOOTER, "foreground")

uses("component.locked-context-card", "token.color.paper2", LOCKED, "background")
uses("component.locked-context-card", "token.color.hairline", LOCKED, "borderBrush")
uses("component.locked-context-card", "token.radius.4", LOCKED, "cornerRadius")
uses("content.locked-card.header", "token.typography.mono", LOCKED, "fontFamily")
uses("content.locked-card.header", "token.font-size.eyebrow", LOCKED, "fontSize")
uses("content.locked-card.header", "token.color.ink", LOCKED, "foreground")
uses("content.locked-card.header", "token.color.ink2", LOCKED, "checkGlyphForeground")
uses("content.locked-card.header", "token.color.ink3", LOCKED, "lockedSuffixForeground")
uses("content.locked-card.summary", "token.typography.serif-light-italic", LOCKED, "fontFamily")
uses("content.locked-card.summary", "token.font-size.body-small", LOCKED, "fontSize")
uses("content.locked-card.summary", "token.color.ink", LOCKED, "foreground")
uses("component.info-row", "token.typography.mono", LOCKED_CS, "fontFamily")
uses("component.info-row", "token.color.ink3", LOCKED_CS, "labelForeground")
uses("component.info-row", "token.color.ink", LOCKED_CS, "valueForeground")
uses("control.locked-card.expand-toggle", "token.color.ink4", LOCKED, "foreground")

uses("asset.future-card.outline", "token.color.hairline", FUTURE, "stroke")
uses("asset.future-card.outline", "token.radius.4", FUTURE, "cornerRadius")
uses("content.future-card.header", "token.typography.mono", FUTURE, "fontFamily")
uses("content.future-card.header", "token.font-size.eyebrow", FUTURE, "fontSize")
uses("content.future-card.header", "token.color.ink3", FUTURE, "foreground")
uses("content.future-card.hint", "token.typography.serif-italic", FUTURE, "fontFamily")
uses("content.future-card.hint", "token.font-size.body-small", FUTURE, "fontSize")
uses("content.future-card.hint", "token.color.ink3", FUTURE, "foreground")

# ---------------------------------------------------------------- graph
graph = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.composer-shell.gold",
    "name": "Composer - Shell (three-column context-engine workspace)",
    "description": (
        "Gold graph for the Composer Shell: a three-column MVUX workspace (composition stack, "
        "active layer canvas, files rail) whose custom controls are expanded two levels deep. "
        "Tokens are scoped to the resources this surface consumes; the eight per-layer canvases "
        "hosted by ContentControl x:Name=CanvasSlot are out of scope."
    ),
    "sourceSummary": [
        {"type": "xaml", "path": SHELL},
        {"type": "csharp", "path": SHELL_CS},
        {"type": "xaml", "path": STACK},
        {"type": "csharp", "path": STACK_CS},
        {"type": "xaml", "path": CANVAS},
        {"type": "csharp", "path": CANVAS_CS},
        {"type": "xaml", "path": HEADER},
        {"type": "csharp", "path": HEADER_CS},
        {"type": "xaml", "path": PROGRESS},
        {"type": "csharp", "path": PROGRESS_CS},
        {"type": "xaml", "path": TITLEROW},
        {"type": "csharp", "path": TITLEROW_CS},
        {"type": "xaml", "path": FOOTER},
        {"type": "csharp", "path": FOOTER_CS},
        {"type": "xaml", "path": LOCKED},
        {"type": "csharp", "path": LOCKED_CS},
        {"type": "xaml", "path": FUTURE},
        {"type": "csharp", "path": FUTURE_CS},
        {"type": "xaml", "path": FILES},
        {"type": "csharp", "path": FILES_CS},
        {"type": "design-system", "path": TOKENS},
        {"type": "design-system", "path": TYPO},
        {"type": "design-system", "path": CHIPSTYLES},
        {"type": "design-system", "path": CTXSTYLES},
        {"type": "csharp", "path": SHELLMODEL},
        {"type": "csharp", "path": MODEL},
        {"type": "csharp", "path": LAYERDEF},
    ],
    "nodes": nodes,
    "edges": edges,
    "unresolved": [
        {
            "id": "unresolved.stack.items-source",
            "question": "Is CompositionStack's ItemsSource=\"{Binding Layers}\" live, or vestigial?",
            "relatedIds": ["control.stack.layer-rows", "component.layer-row"],
            "reason": "The XAML binds ItemsSource to a member named Layers, but neither ComposerModel nor "
                      "ShellModel exposes one (Composer.Models.Layers is a static class), and "
                      "CompositionStack.Render() assigns the LayerRow projection to the repeater on every "
                      "state change. The rows shown are the code-behind projection; whether the binding is "
                      "leftover or an intended future shape is not decidable from source. FilesRail declares "
                      "no ItemsSource at all, which suggests the binding is leftover.",
        },
        {
            "id": "unresolved.rail-panel-shared",
            "question": "Are CompositionStack and FilesRail two controls or one panel component with two variants?",
            "relatedIds": ["component.composition-stack", "component.files-rail"],
            "possibleValues": ["two independent UserControls", "one canonical rail panel with mirrored edge"],
            "reason": "Both are Paper2 borders with a single hairline edge (mirrored left/right), 20,28 padding, "
                      "a spacing-14 vertical AutoLayout, a MonoEyebrow title, an italic caption, a 1px divider "
                      "and an ItemsRepeater. The structure is identical, but they are separately declared "
                      "UserControls with different content, so the gold does not fold them into one canonical.",
        },
        {
            "id": "unresolved.canvas-slot-navigation",
            "question": "Will the center column become a navigation region, and does that change the graph's shape?",
            "relatedIds": ["control.canvas.slot", "component.active-canvas", "screen.composer-shell"],
            "reason": "Shell.xaml and ActiveCanvas.xaml.cs both state that the center column is intended to be a "
                      "uen:Region.Attached navigation surface and that the per-layer Pages plus RouteMap remain "
                      "registered as scaffolding; the M3b attempt was reverted because pages did not mount inside "
                      "the nested region. The graph models the implemented direct hosting (SyncSlot assigns a "
                      "UserControl to CanvasSlot.Content). If navigation lands, the eight layer canvases become "
                      "separate screens rather than swapped content.",
        },
        {
            "id": "unresolved.hidden-canvas-slots",
            "question": "Are the collapsed title row and header title/subtitle removed for good?",
            "relatedIds": ["component.app-title-row", "component.active-layer-header",
                           "content.title-row.project-name", "control.title-row.reset"],
            "reason": "AppTitleRow is Visibility=Collapsed in ActiveCanvas.xaml with a comment saying the title "
                      "moved inside the glass container and the Reset affordance 'will resurface elsewhere'; "
                      "ActiveLayerHeader keeps LayerLabelText, TitleText and SubtitleText collapsed 'so existing "
                      "code-behind references compile'. All of them are still bound and still updated at runtime, "
                      "so nothing in the source decides whether they are deprecated or temporarily parked. Their "
                      "text is deliberately not modeled as visible content.",
        },
        {
            "id": "unresolved.files-rail-row-count",
            "question": "Is the duplicated README.md row in the files rail intentional?",
            "relatedIds": ["component.file-row", "control.files.file-rows", "content.files.locked-summary"],
            "reason": "FilesRail.Render() emits Layers.All (whose Intent layer's File is README.md) and then "
                      "appends Layers.ReadmeFileName (README.md again) plus prompt-context.md, producing ten rows "
                      "with README.md twice; the class comment describes nine files. The locked counter meanwhile "
                      "counts only Drafted per-layer files against Layers.All.Length, so it reads '0 OF 8' while "
                      "ten rows render.",
        },
    ],
    "metadata": {
        "goldVersion": "1.0",
        "altitude": "kit v0.4 screen semantics; states are screen/component presentation conditions only",
        "scope": "Shell + its ten referenced controls; Views/Layers/* excluded (hosted by control.canvas.slot)",
        "tokenRule": "declared resources this surface consumes (21) + 2 derived recurring literals; "
                     "interaction-only shades folded out",
    },
}

out = Path(__file__).resolve().parents[1] / "evals" / "09-composer-shell" / "gold.graph.json"
out.write_text(json.dumps(graph, indent=2) + "\n", encoding="utf-8")
print(f"wrote {out}: {len(nodes)} nodes, {len(edges)} edges, {len(graph['unresolved'])} unresolved")
