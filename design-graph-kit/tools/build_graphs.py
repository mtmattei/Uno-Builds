#!/usr/bin/env python3
"""Build the gold and generated Design Graphs for eval 05-orbital-settings.

Gold = hand-authored answer key, written from the Orbital SettingsPage source
(XAML + code-behind + style dictionaries).

Generated = an independent generation pass following design-understanding.md,
working primarily from the rendered structure. It is a *good* pass but makes
different-but-defensible judgment calls (merge rows/fields into one key/value
component, model sections as regions, neutral token names, fewer tokens) so the
deterministic score reflects realistic run-to-run drift rather than sabotage.
"""
import json, pathlib

OUT = pathlib.Path(__file__).resolve().parents[1] / "evals" / "05-orbital-settings"

XAML = {"type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"}
CS = {"type": "csharp", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"}
PH = {"type": "xaml", "path": "Orbital/Orbital/Controls/PageHeader.xaml"}
PHCS = {"type": "csharp", "path": "Orbital/Orbital/Controls/PageHeader.xaml.cs"}
DS = {"type": "design-system", "label": "Orbital Styles/*.xaml"}


def ev(kind, conf, src, rationale=None):
    e = {"kind": kind, "confidence": conf, "source": src}
    if rationale:
        e["rationale"] = rationale
    return e


def node(id, type, **kw):
    ev_ = kw.pop("evidence")
    n = {"id": id, "type": type}
    n.update(kw)
    n["evidence"] = ev_
    return n


def edge(frm, rel, to, ev_, **props):
    e = {"from": frm, "relation": rel, "to": to}
    if props:
        e["properties"] = props
    e["evidence"] = ev_
    return e


# ---------------------------------------------------------------------------
# GOLD
# ---------------------------------------------------------------------------
gold_nodes = [
    node("screen.settings", "screen", name="Settings",
         evidence=ev("observed", 1.0, XAML)),
    node("component.page-header", "component", name="Page header", role="pageHeader",
         evidence=ev("declared", 1.0, XAML,
         "controls:PageHeader is a reusable UserControl (Orbital.Controls) instantiated with Title/Subtitle.")),
    node("control.header.search", "control", name="Search / command palette", role="button",
         semanticRole="searchTrigger",
         properties={"placeholder": "Search or run command...", "shortcutHint": "Ctrl+K"},
         evidence=ev("declared", 1.0, PH, "SearchBorder: tappable search affordance with Ctrl+K badge.")),
    node("content.settings.title", "content", name="Title", text="Settings",
         semanticRole="pageTitle", evidence=ev("observed", 1.0, XAML)),
    node("content.settings.subtitle", "content", name="Subtitle",
         text="Profile and preferences", semanticRole="pageSubtitle",
         evidence=ev("observed", 1.0, XAML)),

    # canonical card + 4 section instances
    node("component.settings-card", "component", name="Settings section card",
         role="card", evidence=ev("derived", 1.0, DS,
         "All four section containers use OrbitalCardStyle (radius 12, padding 20, Surface1 bg, Surface3 border).")),
    node("component.settings-card.profile", "component", name="Profile section",
         semanticRole="profileSection", evidence=ev("declared", 1.0, XAML, "x:Name=ProfileSection.")),
    node("component.settings-card.about", "component", name="About section",
         semanticRole="aboutSection", evidence=ev("declared", 1.0, XAML, "x:Name=AboutSection.")),
    node("component.settings-card.paths", "component", name="Paths section",
         semanticRole="pathsSection", evidence=ev("declared", 1.0, XAML, "x:Name=PathsSection.")),
    node("component.settings-card.actions", "component", name="Actions section",
         semanticRole="actionsSection", evidence=ev("declared", 1.0, XAML, "x:Name=ActionsSection.")),

    # profile children
    node("content.profile.section-title", "content", name="Profile header",
         text="PROFILE", semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("content.profile.name-label", "content", name="Display Name label",
         text="Display Name", semanticRole="fieldLabel", evidence=ev("observed", 1.0, XAML)),
    node("control.profile.username", "control", name="Display Name", role="textbox",
         semanticRole="nameInput", properties={"placeholder": "Enter your name"},
         evidence=ev("declared", 1.0, XAML, "x:Name=UsernameBox.")),
    node("control.profile.save", "control", name="Save", role="button", text="Save",
         semanticRole="primaryAction",
         evidence=ev("declared", 1.0, XAML, "x:Name=SaveUsernameButton, OrbitalPrimaryButtonSm (only high-emphasis action).")),
    node("content.profile.name-helper", "content", name="Name helper",
         text="This name appears in the homepage greeting.", semanticRole="helperText",
         evidence=ev("observed", 1.0, XAML)),

    # about children
    node("content.about.section-title", "content", name="About header", text="ABOUT",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("asset.about.logo", "asset", name="Orbital logo", role="image",
         properties={"source": "ms-appx:///Assets/Icons/Uno-logo.png"},
         semanticRole="appLogo", evidence=ev("observed", 1.0, XAML)),
    node("content.about.app-name", "content", name="App name", text="Orbital",
         semanticRole="appName", evidence=ev("observed", 1.0, XAML)),
    # Cross-model review finding 2: the visible value is bound; "v0.1.0-alpha"
    # is only the FallbackValue, so asserting it as `text` states a placeholder
    # as if it were the content.
    node("content.about.version", "content", name="Version",
         semanticRole="versionLabel",
         properties={"uno": {"type": "TextBlock", "member": "VersionDisplay"},
                     "fallbackValue": "v0.1.0-alpha"},
         evidence=ev("declared", 1.0, XAML, "Bound VersionDisplay, FallbackValue v0.1.0-alpha.")),
    node("component.info-row", "component", name="Info row (label/value)",
         role="keyValueRow", evidence=ev("derived", 1.0, XAML,
         "Four label/value grids with identical structure inside ABOUT.")),
    node("component.info-row.uno-sdk", "component", name="Uno Platform SDK",
         properties={"value": "{Binding EnvStatus.UnoSdkVersion}"},
         evidence=ev("declared", 1.0, XAML)),
    node("component.info-row.dotnet", "component", name=".NET Runtime",
         properties={"value": "{Binding DotNetDisplay}"}, evidence=ev("declared", 1.0, XAML)),
    node("component.info-row.renderer", "component", name="Renderer",
         properties={"value": "{Binding EnvStatus.Renderer}"}, evidence=ev("declared", 1.0, XAML)),
    node("component.info-row.platform", "component", name="Platform",
         properties={"value": "{Binding PlatformInfo}"}, evidence=ev("declared", 1.0, XAML)),

    # paths children
    node("content.paths.section-title", "content", name="Paths header", text="PATHS",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("component.path-field", "component", name="Path field (label/value)",
         role="keyValueField", evidence=ev("derived", 1.0, XAML,
         "Three label + wrapping-value stacks with identical structure inside PATHS.")),
    node("component.path-field.project-root", "component", name="Project Root",
         properties={"value": "{Binding ProjectRoot}"}, evidence=ev("declared", 1.0, XAML)),
    node("component.path-field.recents-db", "component", name="Recent Projects Database",
         properties={"value": "{Binding RecentsPath}"}, evidence=ev("declared", 1.0, XAML)),
    node("component.path-field.skills", "component", name="Claude Code Skills",
         properties={"value": "{Binding SkillsPath}"}, evidence=ev("declared", 1.0, XAML)),

    # actions children
    node("content.actions.section-title", "content", name="Actions header", text="ACTIONS",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("component.action-button", "component", name="Ghost action button (icon+text)",
         role="button", evidence=ev("derived", 1.0, DS,
         "Three OrbitalGhostButtonSm buttons, each an icon glyph + label, stretched.")),
    node("control.actions.clear-recents", "control", name="Clear Recent Projects",
         role="button", text="Clear Recent Projects", semanticRole="utilityAction",
         evidence=ev("declared", 1.0, XAML, "x:Name=ClearRecentsButton.")),
    node("control.actions.open-data-folder", "control", name="Open Data Folder",
         role="button", text="Open Data Folder", semanticRole="utilityAction",
         evidence=ev("declared", 1.0, XAML, "x:Name=OpenDataFolderButton.")),
    node("control.actions.open-docs", "control", name="Uno Platform Documentation",
         role="button", text="Uno Platform Documentation", semanticRole="externalLink",
         properties={"uri": "https://platform.uno/docs/articles/intro.html"},
         evidence=ev("declared", 1.0, CS, "OpenDocsButton launches the docs URI (LaunchUriAsync).")),

    node("component.dialog.recents-cleared", "component", name="Cleared dialog", role="dialog",
         semanticRole="confirmationDialog",
         properties={"title": "Cleared", "body": "Recent projects list has been cleared.", "closeText": "OK"},
         evidence=ev("declared", 1.0, CS, "ContentDialog shown after clearing recent projects.")),

    # states (source-backed)
    #
    # Cross-model review finding 3: this was one screen-level state, which
    # broke the kit's own Pass 4 attachment rule ("smallest node whose
    # presentation actually changes") and the v0.5 condition-vs-presentation
    # rule. Only the four cards animate - the header does not - and each gets a
    # distinct stagger delay, so the condition drives four local presentations.
    node("state.profile.entering", "state", name="Entering", semanticRole="entering",
         properties={"delayMs": 0, "uno": {"mechanism": "AnimationHelper.FadeUp", "member": "OnLoaded"}},
         evidence=ev("declared", 1.0, CS, "AnimationHelper.FadeUp(ProfileSection, 0) on Loaded.")),
    node("state.about.entering", "state", name="Entering", semanticRole="entering",
         properties={"delayMs": 100, "uno": {"mechanism": "AnimationHelper.FadeUp", "member": "OnLoaded"}},
         evidence=ev("declared", 1.0, CS, "AnimationHelper.FadeUp(AboutSection, 100) on Loaded.")),
    node("state.paths.entering", "state", name="Entering", semanticRole="entering",
         properties={"delayMs": 200, "uno": {"mechanism": "AnimationHelper.FadeUp", "member": "OnLoaded"}},
         evidence=ev("declared", 1.0, CS, "AnimationHelper.FadeUp(PathsSection, 200) on Loaded.")),
    node("state.actions.entering", "state", name="Entering", semanticRole="entering",
         properties={"delayMs": 300, "uno": {"mechanism": "AnimationHelper.FadeUp", "member": "OnLoaded"}},
         evidence=ev("declared", 1.0, CS, "AnimationHelper.FadeUp(ActionsSection, 300) on Loaded.")),
    node("state.profile.saved", "state", name="Saved", semanticRole="confirmation",
         evidence=ev("declared", 1.0, CS, "Save sets button content to 'Saved!' for 1.5s after SettingsService.SaveUsername.")),

    # tokens (declared from styles)
    node("token.color.surface0", "token", name="Surface 0 (page bg)", category="color",
         value="#FF0A0A0B", evidence=ev("declared", 1.0, DS, "OrbitalSurface0Brush; page Background.")),
    node("token.color.surface1", "token", name="Surface 1 (card bg)", category="color",
         value="#FF141416", evidence=ev("declared", 1.0, DS, "OrbitalSurface1Brush; OrbitalCardStyle Background.")),
    node("token.color.surface3", "token", name="Surface 3 (card border)", category="color",
         value="#FF212123", evidence=ev("declared", 1.0, DS, "OrbitalSurface3Brush; OrbitalCardStyle BorderBrush.")),
    node("token.radius.12", "token", name="12 radius", category="radius", value=12,
         properties={"unit": "px"}, evidence=ev("declared", 1.0, DS, "OrbitalCardStyle CornerRadius.")),
    node("token.radius.8", "token", name="8 radius", category="radius", value=8,
         properties={"unit": "px"}, evidence=ev("declared", 1.0, DS, "Button/TextBox CornerRadius.")),
    node("token.spacing.24", "token", name="24 spacing", category="spacing", value=24,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML, "Root AutoLayout Spacing=24 (section gap).")),
    node("token.spacing.16", "token", name="16 spacing", category="spacing", value=16,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML, "Card inner AutoLayout Spacing=16.")),
    node("token.spacing.12", "token", name="12 spacing", category="spacing", value=12,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML, "Actions AutoLayout Spacing=12.")),
    node("token.spacing.8", "token", name="8 spacing", category="spacing", value=8,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML, "Label/field StackPanel Spacing=8.")),
    node("token.typography.mono-small", "token", name="Mono small", category="typography",
         value={"family": "JetBrains Mono", "size": 11},
         evidence=ev("declared", 1.0, DS, "OrbitalMonoSmall / OrbitalSectionHeader (mono, 11).")),
]

gold_edges = [
    edge("screen.settings", "contains", "component.page-header", ev("observed", 1.0, XAML)),
    edge("component.page-header", "contains", "content.settings.title", ev("observed", 1.0, XAML)),
    edge("component.page-header", "contains", "content.settings.subtitle", ev("observed", 1.0, XAML)),
    edge("component.page-header", "contains", "control.header.search", ev("declared", 1.0, PH)),
]
for s in ["profile", "about", "paths", "actions"]:
    gold_edges.append(edge("screen.settings", "contains", f"component.settings-card.{s}", ev("observed", 1.0, XAML)))
    gold_edges.append(edge(f"component.settings-card.{s}", "instance-of", "component.settings-card",
                           ev("derived", 1.0, DS, "Shares OrbitalCardStyle.")))

# profile
for c in ["content.profile.section-title", "content.profile.name-label",
          "control.profile.username", "control.profile.save", "content.profile.name-helper"]:
    gold_edges.append(edge("component.settings-card.profile", "contains", c, ev("observed", 1.0, XAML)))
# about
for c in ["content.about.section-title", "asset.about.logo", "content.about.app-name",
          "content.about.version", "component.info-row.uno-sdk", "component.info-row.dotnet",
          "component.info-row.renderer", "component.info-row.platform"]:
    gold_edges.append(edge("component.settings-card.about", "contains", c, ev("observed", 1.0, XAML)))
for i in ["uno-sdk", "dotnet", "renderer", "platform"]:
    gold_edges.append(edge(f"component.info-row.{i}", "instance-of", "component.info-row", ev("derived", 1.0, XAML)))
# paths
for c in ["content.paths.section-title", "component.path-field.project-root",
          "component.path-field.recents-db", "component.path-field.skills"]:
    gold_edges.append(edge("component.settings-card.paths", "contains", c, ev("observed", 1.0, XAML)))
for i in ["project-root", "recents-db", "skills"]:
    gold_edges.append(edge(f"component.path-field.{i}", "instance-of", "component.path-field", ev("derived", 1.0, XAML)))
# actions
for c in ["content.actions.section-title", "control.actions.clear-recents",
          "control.actions.open-data-folder", "control.actions.open-docs"]:
    gold_edges.append(edge("component.settings-card.actions", "contains", c, ev("observed", 1.0, XAML)))
for i in ["clear-recents", "open-data-folder", "open-docs"]:
    gold_edges.append(edge(f"control.actions.{i}", "instance-of", "component.action-button", ev("derived", 1.0, DS)))

# states + triggers
gold_edges += [
    # One condition (OnLoaded), four locally-attached presentations.
    edge("component.settings-card.profile", "has-state", "state.profile.entering", ev("declared", 1.0, CS)),
    edge("component.settings-card.about", "has-state", "state.about.entering", ev("declared", 1.0, CS)),
    edge("component.settings-card.paths", "has-state", "state.paths.entering", ev("declared", 1.0, CS)),
    edge("component.settings-card.actions", "has-state", "state.actions.entering", ev("declared", 1.0, CS)),
    edge("control.profile.save", "has-state", "state.profile.saved",
         ev("declared", 1.0, CS, "v0.2 attachment rule: the Save button is the smallest node whose presentation changes.")),
    edge("control.profile.save", "triggers", "state.profile.saved",
         ev("declared", 1.0, CS, "Save handler swaps content to 'Saved!' after persisting the name.")),
    edge("control.actions.clear-recents", "triggers", "component.dialog.recents-cleared",
         ev("declared", 1.0, CS, "ClearRecentsButton handler clears recents then shows the Cleared dialog.")),
]

# uses-token
gold_edges += [
    edge("screen.settings", "uses-token", "token.color.surface0", ev("declared", 1.0, DS), appliesTo="background"),
    edge("screen.settings", "uses-token", "token.spacing.24", ev("observed", 1.0, XAML), appliesTo="sectionGap"),
    edge("component.settings-card", "uses-token", "token.color.surface1", ev("declared", 1.0, DS), appliesTo="background"),
    edge("component.settings-card", "uses-token", "token.color.surface3", ev("declared", 1.0, DS), appliesTo="borderBrush"),
    edge("component.settings-card", "uses-token", "token.radius.12", ev("declared", 1.0, DS), appliesTo="cornerRadius"),
    edge("component.settings-card", "uses-token", "token.spacing.16", ev("observed", 1.0, XAML), appliesTo="innerGap"),
    edge("component.settings-card.actions", "uses-token", "token.spacing.12", ev("observed", 1.0, XAML), appliesTo="innerGap"),
    edge("control.profile.save", "uses-token", "token.radius.8", ev("declared", 1.0, DS), appliesTo="cornerRadius"),
    edge("component.action-button", "uses-token", "token.radius.8", ev("declared", 1.0, DS), appliesTo="cornerRadius"),
    edge("component.info-row", "uses-token", "token.typography.mono-small", ev("declared", 1.0, DS), appliesTo="font"),
    edge("component.path-field", "uses-token", "token.typography.mono-small", ev("declared", 1.0, DS), appliesTo="font"),
]

gold_unresolved = [
    {
        "id": "unresolved.header.search-target",
        "question": "What UI does the header search / command palette open?",
        "relatedIds": ["control.header.search"],
        "possibleValues": ["global command palette", "search overlay", "unknown"],
        "reason": "PageHeader raises a static SearchRequested event; the handler and resulting UI are outside the supplied source.",
    },
    {
        "id": "unresolved.settings.rowfield-consolidation",
        "question": "Are the ABOUT info-rows and the PATHS fields one reusable key/value component or two?",
        "relatedIds": ["component.info-row", "component.path-field"],
        "possibleValues": ["one key-value component", "two distinct components"],
        "reason": "Both pair a label with a value, but info-row is a horizontal grid (right-aligned value) while path-field is a vertical stack with a wrapping value. Modeled separately; may be one family.",
    },
    {
        "id": "unresolved.settings.external-nav",
        "question": "Should the docs button's external URL launch be modeled as navigates-to?",
        "relatedIds": ["control.actions.open-docs"],
        "possibleValues": ["navigates-to (external)", "triggers (open url)", "out of scope for v0.1"],
        "reason": "v0.1 navigates-to targets a known in-app screen; this opens an external browser URL with no screen node.",
    },
]



# ---------------------------------------------------------------------------
# GOLD v1.4 — consensus calibration (blind-v4): adopt the five-run consensus
# token-wiring altitude. Resting-surface text-emphasis + accent + typography
# tokens, wired to their consumers. All values declared in Orbital source.
# ---------------------------------------------------------------------------
_CAL_TOKENS = [
    ("token.color.text-30", "Text 30% emphasis", "color", "#FF4A505D", {"resourceKey": "OrbitalText30Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-38", "Text 38% emphasis", "color", "#FF5D6372", {"resourceKey": "OrbitalText38Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-40", "Text 40% emphasis", "color", "#FF626878", {"resourceKey": "OrbitalText40Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-50", "Text 50% emphasis", "color", "#FF7B8191", {"resourceKey": "OrbitalText50Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-72", "Text 72% emphasis", "color", "#FFB5B9C5", {"resourceKey": "OrbitalText72Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-85", "Text 85% emphasis", "color", "#FFD6D9E1", {"resourceKey": "OrbitalText85Brush", "resourceType": "SolidColorBrush"}),
    ("token.color.emerald-500", "Emerald 500 (primary accent)", "color", "#FF10B981", {"resourceKey": "OrbitalEmerald500Brush", "resourceType": "SolidColorBrush"}),
]
for _id, _name, _cat, _val, _uno in _CAL_TOKENS:
    gold_nodes.append(node(_id, "token", name=_name, category=_cat, value=_val,
                           properties={"uno": _uno},
                           evidence=ev("declared", 1.0, DS, "Declared brush resource consumed by the resting screen.")))
# Cross-model review finding 1: this said size 28, which is OrbitalHeroTitle's
# value. OrbitalPageTitle declares FontSize 20 (Styles/TextBlock.xaml:21-24).
# The key existed, so every existence check passed while the value was another
# style's - which is why tools/build_review_packet.py now compares values too.
gold_nodes.append(node("token.typography.page-title", "token", name="Page title type", category="typography",
                       value={"family": "Space Grotesk", "size": 20, "weight": "SemiBold"},
                       properties={"uno": {"styleKey": "OrbitalPageTitle"}},
                       evidence=ev("declared", 1.0, DS, "OrbitalPageTitle style (display face, 20, SemiBold).")))
# Cross-model review finding 4: OrbitalSectionHeader is a distinct declared
# style - Medium weight with CharacterSpacing 80 - not the same token as
# OrbitalMonoSmall (Normal, no tracking). Same family and size is not the same
# typography.
gold_nodes.append(node("token.typography.section-header", "token", name="Section header type", category="typography",
                       value={"family": "JetBrains Mono", "size": 11, "weight": "Medium", "letterSpacing": 80},
                       properties={"uno": {"styleKey": "OrbitalSectionHeader"}},
                       evidence=ev("declared", 1.0, DS, "OrbitalSectionHeader style (mono 11, Medium, tracking 80).")))
gold_nodes.append(node("token.typography.body", "token", name="Body type", category="typography",
                       value={"family": "Space Grotesk", "size": 13},
                       properties={"uno": {"styleKey": "OrbitalBody"}},
                       evidence=ev("declared", 1.0, DS, "OrbitalBody style (display face, 13).")))

_CAL_EDGES = [
    ("content.settings.title", "token.typography.page-title", "font"),
    ("content.settings.subtitle", "token.typography.body", "font"),
    ("content.settings.subtitle", "token.color.text-40", "foreground"),
    ("content.profile.section-title", "token.typography.section-header", "font"),
    ("content.profile.section-title", "token.color.text-38", "foreground"),
    ("content.about.section-title", "token.typography.section-header", "font"),
    ("content.about.section-title", "token.color.text-38", "foreground"),
    ("content.paths.section-title", "token.typography.section-header", "font"),
    ("content.paths.section-title", "token.color.text-38", "foreground"),
    ("content.actions.section-title", "token.typography.section-header", "font"),
    ("content.actions.section-title", "token.color.text-38", "foreground"),
    ("content.profile.name-label", "token.typography.mono-small", "font"),
    ("content.profile.name-label", "token.color.text-50", "foreground"),
    ("component.info-row", "token.color.text-50", "labelColor"),
    ("component.info-row", "token.color.text-72", "valueColor"),
    ("component.path-field", "token.color.text-50", "labelColor"),
    ("component.path-field", "token.color.text-72", "valueColor"),
    ("content.profile.name-helper", "token.typography.mono-small", "font"),
    ("content.profile.name-helper", "token.color.text-30", "foreground"),
    ("content.about.app-name", "token.typography.body", "font"),
    ("content.about.app-name", "token.color.text-85", "foreground"),
    ("content.about.version", "token.typography.mono-small", "font"),
    ("content.about.version", "token.color.text-40", "foreground"),
    ("control.profile.username", "token.color.surface1", "background"),
    ("control.profile.username", "token.color.surface3", "borderBrush"),
    ("control.profile.username", "token.color.text-85", "foreground"),
    ("control.profile.save", "token.color.emerald-500", "background"),
    ("control.header.search", "token.color.surface1", "background"),
    ("control.header.search", "token.color.surface3", "borderBrush"),
    ("control.header.search", "token.radius.8", "cornerRadius"),
]
for _frm, _to, _applies in _CAL_EDGES:
    gold_edges.append(edge(_frm, "uses-token", _to,
                           ev("declared", 1.0, DS, "Consensus calibration: declared style/brush consumption on the resting screen."),
                           appliesTo=_applies))

# ---------------------------------------------------------------------------
# v0.4 Uno mapping layer (references/uno-mapping.md) — all values verified in
# the Orbital source; omission over invention.
# ---------------------------------------------------------------------------
UNO = {
    "screen.settings": {"type": "Page", "class": "Orbital.Presentation.SettingsPage"},
    "component.page-header": {"type": "UserControl", "class": "Orbital.Controls.PageHeader"},
    "control.header.search": {"type": "Border", "xName": "SearchBorder"},
    "content.settings.title": {"type": "TextBlock", "styleKey": "OrbitalPageTitle"},
    "content.settings.subtitle": {"type": "TextBlock", "styleKey": "OrbitalBody"},
    "component.settings-card": {"type": "Border", "styleKey": "OrbitalCardStyle"},
    "component.settings-card.profile": {"xName": "ProfileSection"},
    "component.settings-card.about": {"xName": "AboutSection"},
    "component.settings-card.paths": {"xName": "PathsSection"},
    "component.settings-card.actions": {"xName": "ActionsSection"},
    "content.profile.section-title": {"type": "TextBlock", "styleKey": "OrbitalSectionHeader"},
    "content.profile.name-label": {"type": "TextBlock", "styleKey": "OrbitalMonoSmall"},
    "control.profile.username": {"type": "TextBox", "xName": "UsernameBox"},
    "control.profile.save": {"type": "Button", "styleKey": "OrbitalPrimaryButtonSm", "xName": "SaveUsernameButton"},
    "content.profile.name-helper": {"type": "TextBlock", "styleKey": "OrbitalMonoSmall"},
    "content.about.section-title": {"type": "TextBlock", "styleKey": "OrbitalSectionHeader"},
    "asset.about.logo": {"type": "Image", "source": "ms-appx:///Assets/Icons/Uno-logo.png"},
    "content.about.app-name": {"type": "TextBlock", "styleKey": "OrbitalBody"},
    "content.about.version": {"type": "TextBlock", "styleKey": "OrbitalMonoSmall"},
    "component.info-row": {"type": "Grid"},
    "content.paths.section-title": {"type": "TextBlock", "styleKey": "OrbitalSectionHeader"},
    "component.path-field": {"type": "StackPanel"},
    "content.actions.section-title": {"type": "TextBlock", "styleKey": "OrbitalSectionHeader"},
    "component.action-button": {"type": "Button", "styleKey": "OrbitalGhostButtonSm"},
    "control.actions.clear-recents": {"type": "Button", "xName": "ClearRecentsButton", "iconGlyph": "E74D"},
    "control.actions.open-data-folder": {"type": "Button", "xName": "OpenDataFolderButton", "iconGlyph": "E838"},
    "control.actions.open-docs": {"type": "Button", "xName": "OpenDocsButton", "iconGlyph": "E8A5"},
    "component.dialog.recents-cleared": {"type": "ContentDialog"},
    "state.profile.saved": {"mechanism": "code-behind"},
    "token.color.surface0": {"resourceKey": "OrbitalSurface0Brush", "resourceType": "SolidColorBrush"},
    "token.color.surface1": {"resourceKey": "OrbitalSurface1Brush", "resourceType": "SolidColorBrush"},
    "token.color.surface3": {"resourceKey": "OrbitalSurface3Brush", "resourceType": "SolidColorBrush"},
    "token.radius.12": {"styleKey": "OrbitalCardStyle", "property": "CornerRadius"},
    "token.radius.8": {"property": "CornerRadius"},
    "token.spacing.24": {"property": "Spacing"},
    "token.spacing.16": {"property": "Spacing"},
    "token.spacing.12": {"property": "Spacing"},
    "token.spacing.8": {"property": "Spacing"},
    "token.typography.mono-small": {"styleKey": "OrbitalMonoSmall", "fontResourceKey": "OrbitalMonoFont"},
}
for _n in gold_nodes:
    if _n["id"] in UNO:
        _n.setdefault("properties", {})["uno"] = UNO[_n["id"]]

gold = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.orbital-settings",
    "name": "Orbital Settings",
    "description": "Gold graph for the Orbital SettingsPage (source-backed: XAML + code-behind + styles). v1.4: consensus-calibrated token wiring (blind-v4) + properties.uno mapping layer.",
    "sourceSummary": [
        {"type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"},
        {"type": "csharp", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"},
        {"type": "xaml", "path": "Orbital/Orbital/Controls/PageHeader.xaml"},
        {"type": "csharp", "path": "Orbital/Orbital/Controls/PageHeader.xaml.cs"},
        {"type": "design-system", "label": "Orbital Styles/*.xaml"},
    ],
    "nodes": gold_nodes,
    "edges": gold_edges,
    "unresolved": gold_unresolved,
}

(OUT / "gold.graph.json").write_text(json.dumps(gold, indent=2) + "\n")
print("gold nodes:", len(gold_nodes), "edges:", len(gold_edges), "unresolved:", len(gold_unresolved))
