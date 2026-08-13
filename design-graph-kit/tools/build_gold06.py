#!/usr/bin/env python3
"""Gold graph for eval 06 — FluxTransit ProfilePage (source-backed, kit v0.4).

Authored from: ProfilePage.xaml, ProfilePage.xaml.cs, ProfileModel.cs (MVUX),
Styles/FluxStyles.xaml. Altitude: v1.4 consensus calibration (typography/color
wired to consumers; canonical internals as properties; tokens screen-scoped).
"""
import json, pathlib

OUT = pathlib.Path(__file__).resolve().parents[1] / "evals" / "06-flux-profile"
XAML = {"type": "xaml", "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml"}
VM = {"type": "csharp", "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs"}
DS = {"type": "design-system", "path": "FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml"}


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


def tok(id, name, cat, value, uno, rationale):
    return node(id, "token", name=name, category=cat, value=value,
                properties={"uno": uno}, evidence=ev("declared", 1.0, DS, rationale))


nodes = [
    node("screen.profile", "screen", name="Profile",
         properties={"uno": {"type": "Page", "class": "FluxTransit.Presentation.ProfilePage"}},
         evidence=ev("observed", 1.0, XAML)),

    node("region.profile.scroll-content", "region", name="Scrolling content", semanticRole="scrollingContent",
         properties={"uno": {"type": "ScrollViewer"}},
         evidence=ev("observed", 1.0, XAML,
         "ScrollViewer owning the safe-area-aware, width-constrained content column for the whole page.")),
    node("region.opus.summary", "region", name="OPUS summary", semanticRole="accountSummary",
         evidence=ev("observed", 1.0, XAML,
         "Horizontal grouping pairing the OPUS card visual with its balance; the pairing identifies which "
         "account the balance belongs to, which is meaning rather than arrangement.")),
    node("region.opus.refresh-action", "region", name="Refresh action", semanticRole="actionSlot",
         evidence=ev("observed", 1.0, XAML,
         "Grid where the update button and the progress row occupy the same cell with inverse visibility; "
         "this is the only part of the OPUS card whose presentation swaps.")),
    node("region.profile.header", "region", name="Header",
         evidence=ev("observed", 1.0, XAML, "Back button + heading grid at top.")),
    node("control.header.back", "control", name="Back", role="button", semanticRole="backNavigation",
         properties={"command": "{Binding GoBack}",
                     "uno": {"type": "Button", "styleKey": "FluxIconButtonStyle", "iconGlyph": "E72B"}},
         evidence=ev("declared", 1.0, XAML, "Icon button bound to the GoBack command.")),
    node("content.header.title", "content", name="Title", text="Profile", semanticRole="pageTitle",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxHeadingLarge"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.header.subtitle", "content", name="Subtitle", text="Manage your transit settings",
         semanticRole="pageSubtitle",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),

    node("component.card", "component", name="Section card (glass)", role="card",
         properties={"uno": {"type": "Border", "styleKey": "FluxGlassPanelStyle"}},
         evidence=ev("derived", 1.0, DS, "Three sections share FluxGlassPanelStyle (glass bg, subtle border, radius 24, padding 24).")),
    node("component.card.opus", "component", name="OPUS card section", semanticRole="opusSection",
         evidence=ev("observed", 1.0, XAML)),
    node("component.card.routes", "component", name="Saved routes section", semanticRole="routesSection",
         evidence=ev("observed", 1.0, XAML)),
    node("component.card.settings", "component", name="Settings section", semanticRole="settingsSection",
         evidence=ev("observed", 1.0, XAML)),

    node("content.opus.section-title", "content", name="Opus header", text="OPUS CARD",
         semanticRole="sectionHeader",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxMicro"}},
         evidence=ev("observed", 1.0, XAML)),
    node("component.opus-card", "component", name="OPUS card visual", role="card",
         properties={"parts": ["brand", "cardNumber"], "brand": "OPUS",
                     "value": "{Binding CardNumber}",
                     "uno": {"type": "Border"}},
         evidence=ev("observed", 1.0, XAML, "120x80 indigo card visual; brand text + bound masked number.")),
    node("content.opus.balance-label", "content", name="Balance label", text="Current Balance",
         semanticRole="fieldLabel",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.opus.balance-value", "content", name="Balance value", semanticRole="metricValue",
         properties={"value": "{Binding OpusBalance}", "prefix": "$",
                     "uno": {"type": "TextBlock", "styleKey": "FluxHeadingLarge"}},
         evidence=ev("declared", 1.0, XAML, "Bound to OpusBalance with a literal $ run.")),
    node("content.opus.refresh-hint", "content", name="Refresh hint", text="Tap to refresh",
         semanticRole="helperText",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("control.opus.update", "control", name="Update Balance", role="button", text="Update Balance",
         semanticRole="primaryAction",
         properties={"command": "{Binding UpdateBalance}",
                     "uno": {"type": "Button", "styleKey": "FluxPrimaryButtonStyle"}},
         evidence=ev("declared", 1.0, XAML, "Bound to UpdateBalance; hidden while IsRefreshing.")),
    node("state.opus.refreshing", "state", name="Refreshing", semanticRole="loading",
         properties={"uno": {"mechanism": "binding", "member": "IsRefreshing"}},
         evidence=ev("declared", 1.0, VM, "IState<bool> IsRefreshing toggles the button/progress swap during UpdateBalance.")),
    node("control.opus.progress", "control", name="Refresh progress", role="progressRing",
         semanticRole="busyIndicator",
         properties={"uno": {"type": "ProgressRing"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.opus.updating-label", "content", name="Updating label", text="Updating...",
         semanticRole="statusText",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),

    node("content.routes.section-title", "content", name="Routes header", text="SAVED ROUTES",
         semanticRole="sectionHeader",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxMicro"}},
         evidence=ev("observed", 1.0, XAML)),
    node("component.route-item", "component", name="Saved route row", role="listItem",
         properties={"parts": ["icon", "name", "stations", "chevron"],
                     "uno": {"type": "Border", "styleKey": "FluxTransitCardStyle"}},
         evidence=ev("derived", 1.0, XAML, "Two identical route rows: transit icon, name over stations, chevron.")),
    node("component.route-item.home-work", "component", name="Home → Work",
         properties={"name": "Home → Work", "stations": "Berri-UQAM → McGill"},
         evidence=ev("observed", 1.0, XAML)),
    node("component.route-item.downtown-loop", "component", name="Downtown Loop",
         properties={"name": "Downtown Loop", "stations": "Place-des-Arts → Mont-Royal"},
         evidence=ev("observed", 1.0, XAML)),
    node("control.routes.add", "control", name="Add New Route", role="button", text="Add New Route",
         semanticRole="addAction",
         properties={"uno": {"type": "Button", "iconGlyph": "E710"}},
         evidence=ev("observed", 1.0, XAML, "Outlined full-width button; no command or click handler bound.")),

    node("content.settings.section-title", "content", name="Settings header", text="SETTINGS",
         semanticRole="sectionHeader",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxMicro"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.settings.api-label", "content", name="API key label",
         text="Gemini API Key (for AI features)", semanticRole="fieldLabel",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("control.settings.api-key", "control", name="Gemini API key", role="textbox",
         semanticRole="apiKeyInput",
         properties={"placeholder": "Enter your Gemini API key",
                     "value": "{Binding GeminiApiKey, Mode=TwoWay}",
                     "uno": {"type": "TextBox"}},
         evidence=ev("declared", 1.0, XAML, "TwoWay-bound to GeminiApiKey.")),
    node("content.settings.api-helper", "content", name="API key helper",
         text="Get your API key from ai.google.dev", semanticRole="helperText",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.settings.language-label", "content", name="Language label", text="Language",
         semanticRole="fieldLabel",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("control.settings.language", "control", name="Language", role="chipGroup",
         semanticRole="languageSelector",
         properties={"options": ["EN", "FR"], "selected": "EN",
                     "uno": {"type": "utu:ChipGroup", "styleKey": "FilterChipGroupStyle"}},
         evidence=ev("observed", 1.0, XAML, "Single-select chip group; EN literal-checked in XAML.")),
    node("content.settings.alerts-label", "content", name="Alerts label", text="Service Alerts",
         semanticRole="fieldLabel",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.settings.alerts-helper", "content", name="Alerts helper",
         text="Receive notifications about delays", semanticRole="helperText",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("control.settings.alerts", "control", name="Service alerts", role="toggle",
         semanticRole="notificationsToggle",
         properties={"value": True,
                     "uno": {"type": "ToggleSwitch"}},
         evidence=ev("observed", 1.0, XAML, "IsOn=True literal; not bound to any state.")),

    node("region.profile.footer", "region", name="App info footer",
         evidence=ev("observed", 1.0, XAML)),
    node("content.footer.version", "content", name="App version", text="Flux Transit v1.0",
         semanticRole="versionLabel",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),
    node("content.footer.credit", "content", name="Credit", text="Built with Uno Platform",
         semanticRole="caption",
         properties={"uno": {"type": "TextBlock", "styleKey": "FluxBody"}},
         evidence=ev("observed", 1.0, XAML)),

    # tokens — screen-consumed, declared in FluxStyles.xaml
    tok("token.color.background", "Background", "color", "#0f172a",
        {"resourceKey": "FluxBackgroundBrush", "resourceType": "SolidColorBrush"}, "Page background."),
    tok("token.color.surface", "Surface", "color", "#1e293b",
        {"resourceKey": "FluxSurfaceBrush", "resourceType": "SolidColorBrush"}, "TextBox background."),
    tok("token.color.glass-panel", "Glass panel", "color", "#1e293b66",
        {"resourceKey": "FluxGlassPanelBrush", "resourceType": "SolidColorBrush"}, "Panel background."),
    tok("token.color.border-subtle", "Border subtle", "color", "#ffffff0d",
        {"resourceKey": "FluxBorderSubtleBrush", "resourceType": "SolidColorBrush"}, "Panel/card border."),
    tok("token.color.border-light", "Border light", "color", "#ffffff1a",
        {"resourceKey": "FluxBorderLightBrush", "resourceType": "SolidColorBrush"}, "Outlined button/TextBox border."),
    tok("token.color.primary", "Primary (indigo 400)", "color", "#818cf8",
        {"resourceKey": "FluxPrimaryBrush", "resourceType": "SolidColorBrush"}, "Primary button bg; route icons."),
    tok("token.color.primary-strong", "Primary strong (indigo 600)", "color", "#4f46e5",
        {"resourceKey": "FluxPrimaryStrongBrush", "resourceType": "SolidColorBrush"}, "OPUS card visual bg."),
    tok("token.color.success", "Success (emerald)", "color", "#34d399",
        {"resourceKey": "FluxSuccessBrush", "resourceType": "SolidColorBrush"}, "Balance value."),
    tok("token.color.text-primary", "Text primary", "color", "#ffffff",
        {"resourceKey": "FluxTextPrimaryBrush", "resourceType": "SolidColorBrush"}, "High-emphasis text."),
    tok("token.color.text-secondary", "Text secondary", "color", "#94a3b8",
        {"resourceKey": "FluxTextSecondaryBrush", "resourceType": "SolidColorBrush"}, "Body text (via FluxBody)."),
    tok("token.color.text-muted", "Text muted", "color", "#64748b",
        {"resourceKey": "FluxTextMutedBrush", "resourceType": "SolidColorBrush"}, "Micro headers; chevrons."),
    tok("token.radius.24", "24 radius", "radius", 24,
        {"resourceKey": "FluxCornerRadiusLarge", "resourceType": "CornerRadius"}, "Glass panels."),
    tok("token.radius.16", "16 radius", "radius", 16,
        {"resourceKey": "FluxCornerRadiusMedium", "resourceType": "CornerRadius"}, "Route cards."),
    tok("token.radius.full", "Full (pill) radius", "radius", 9999,
        {"resourceKey": "FluxCornerRadiusFull", "resourceType": "CornerRadius"}, "Primary button."),
    tok("token.spacing.24", "24 spacing", "spacing", 24,
        {"resourceKey": "FluxSpacingL"}, "Page section gap."),
    tok("token.spacing.16", "16 spacing", "spacing", 16,
        {"resourceKey": "FluxSpacingM"}, "Panel inner gap."),
    tok("token.spacing.8", "8 spacing", "spacing", 8,
        {"resourceKey": "FluxSpacingS"}, "Label/field gaps."),
    tok("token.spacing.4", "4 spacing", "spacing", 4,
        {"resourceKey": "FluxSpacingXS"}, "Route name/stations gap."),
    tok("token.typography.heading-large", "Heading large", "typography",
        {"size": 36, "weight": "Bold", "tracking": -20},
        {"styleKey": "FluxHeadingLarge"}, "Page title; balance value."),
    tok("token.typography.body", "Body", "typography", {"size": 14},
        {"styleKey": "FluxBody"}, "Default body text."),
    # Cross-model review F3/F4: the API and alerts helpers apply FluxBody but
    # override FontSize to 12 locally (ProfilePage.xaml:187,220), so the 14px
    # body token does not describe their rendered typography. Derived, because
    # the source states the size inline rather than through a declared token.
    node("token.typography.helper", "token", name="Helper caption", category="typography",
         value={"size": 12},
         properties={"uno": {"styleKey": "FluxBody", "property": "FontSize=12"}},
         evidence=ev("derived", 1.0, XAML,
                     "Repeated local FontSize=12 override on FluxBody helper text.")),
    tok("token.typography.body-bold", "Body bold", "typography", {"size": 14, "weight": "Bold"},
        {"styleKey": "FluxBodyBold"}, "Route names."),
    tok("token.typography.micro", "Micro header", "typography",
        {"size": 10, "weight": "Bold", "tracking": 80},
        {"styleKey": "FluxMicro"}, "Uppercase section headers."),
]

E_OBS = lambda: ev("observed", 1.0, XAML)
E_DS = lambda r=None: ev("declared", 1.0, DS, r)

edges = [
    # Cross-model review: the whole visible surface is owned by a ScrollViewer
    # holding a safe-area-aware, width-constrained content column. Removing
    # that grouping changes scrolling and viewport safety, not just placement,
    # so it earns a region under the v0.6 rule.
    edge("screen.profile", "contains", "region.profile.scroll-content", E_OBS()),
    edge("region.profile.scroll-content", "contains", "region.profile.header", E_OBS()),
    edge("region.profile.header", "contains", "control.header.back", E_OBS()),
    edge("region.profile.header", "contains", "content.header.title", E_OBS()),
    edge("region.profile.header", "contains", "content.header.subtitle", E_OBS()),
    edge("region.profile.scroll-content", "contains", "component.card.opus", E_OBS()),
    edge("region.profile.scroll-content", "contains", "component.card.routes", E_OBS()),
    edge("region.profile.scroll-content", "contains", "component.card.settings", E_OBS()),
    edge("region.profile.scroll-content", "contains", "region.profile.footer", E_OBS()),
    edge("region.profile.footer", "contains", "content.footer.version", E_OBS()),
    edge("region.profile.footer", "contains", "content.footer.credit", E_OBS()),
]
for s_ in ("opus", "routes", "settings"):
    edges.append(edge(f"component.card.{s_}", "instance-of", "component.card",
                      E_DS("Uses FluxGlassPanelStyle.")))
# The OPUS card splits into a summary grouping (card visual + its balance) and
# the action slot that actually swaps presentation - cross-model findings 11/12.
edges.append(edge("component.card.opus", "contains", "content.opus.section-title", E_OBS()))
edges.append(edge("component.card.opus", "contains", "region.opus.summary", E_OBS()))
edges.append(edge("component.card.opus", "contains", "region.opus.refresh-action", E_OBS()))
for c in ("component.opus-card", "content.opus.balance-label",
          "content.opus.balance-value", "content.opus.refresh-hint"):
    edges.append(edge("region.opus.summary", "contains", c, E_OBS()))
edges.append(edge("region.opus.refresh-action", "contains", "control.opus.update", E_OBS()))
for c in ("content.routes.section-title", "component.route-item.home-work",
          "component.route-item.downtown-loop", "control.routes.add"):
    edges.append(edge("component.card.routes", "contains", c, E_OBS()))
for i in ("home-work", "downtown-loop"):
    edges.append(edge(f"component.route-item.{i}", "instance-of", "component.route-item",
                      ev("derived", 1.0, XAML, "Identical route-row structure.")))
for c in ("content.settings.section-title", "content.settings.api-label", "control.settings.api-key",
          "content.settings.api-helper", "content.settings.language-label", "control.settings.language",
          "content.settings.alerts-label", "content.settings.alerts-helper", "control.settings.alerts"):
    edges.append(edge("component.card.settings", "contains", c, E_OBS()))

edges += [
    # Attached to the action slot, not the whole card: only that grid swaps.
    edge("region.opus.refresh-action", "has-state", "state.opus.refreshing",
         ev("declared", 1.0, VM, "IsRefreshing swaps the update button for the progress row in this grid; "
                                 "the card visual, balance and heading do not change.")),
    edge("control.opus.update", "triggers", "state.opus.refreshing",
         ev("declared", 1.0, VM, "UpdateBalance sets IsRefreshing true for the duration of the refresh.")),
    # Multi-effect rule (v0.6): UpdateBalance has two declared effects - it sets
    # IsRefreshing AND writes a new OpusBalance. Recording only the loading one
    # was the exact omission the rule exists to prevent.
    edge("control.opus.update", "triggers", "content.opus.balance-value",
         ev("declared", 1.0, VM, "UpdateBalance sets OpusBalance to the newly fetched value.")),
    edge("state.opus.refreshing", "contains", "control.opus.progress", E_OBS()),
    edge("state.opus.refreshing", "contains", "content.opus.updating-label", E_OBS()),
]

UT = [
    ("screen.profile", "token.color.background", "background"),
    ("screen.profile", "token.spacing.24", "sectionGap"),
    ("component.card", "token.color.glass-panel", "background"),
    ("component.card", "token.color.border-subtle", "borderBrush"),
    ("component.card", "token.radius.24", "cornerRadius"),
    # Cross-model review: FluxGlassPanelStyle declares Padding=24; the 16 comes
    # from three page-local child StackPanels, not from the cited style.
    ("component.card", "token.spacing.24", "padding"),
    ("component.card", "token.spacing.16", "innerGap"),
    ("content.header.title", "token.typography.heading-large", "font"),
    ("content.header.subtitle", "token.typography.body", "font"),
    ("content.opus.section-title", "token.typography.micro", "font"),
    ("content.routes.section-title", "token.typography.micro", "font"),
    ("content.settings.section-title", "token.typography.micro", "font"),
    ("component.opus-card", "token.color.primary-strong", "background"),
    ("content.opus.balance-label", "token.typography.body", "font"),
    ("content.opus.balance-value", "token.typography.heading-large", "font"),
    ("content.opus.balance-value", "token.color.success", "foreground"),
    ("content.opus.refresh-hint", "token.typography.body", "font"),
    ("control.opus.update", "token.color.primary", "background"),
    ("control.opus.update", "token.radius.full", "cornerRadius"),
    ("component.route-item", "token.radius.16", "cornerRadius"),
    ("component.route-item", "token.color.border-subtle", "borderBrush"),
    ("component.route-item", "token.color.primary", "iconColor"),
    ("component.route-item", "token.color.text-muted", "chevronColor"),
    # Cross-model review F5/F6: the styles these nodes already consume also
    # declare a Foreground - FluxBody supplies TextSecondary, FluxHeadingLarge
    # and FluxBodyBold supply TextPrimary, FluxMicro supplies TextMuted. The
    # gold recorded the typography half of each style and dropped the colour
    # half, leaving text-secondary with no modelled consumer at all.
    ("content.header.title", "token.color.text-primary", "foreground"),
    ("content.header.subtitle", "token.color.text-secondary", "foreground"),
    ("content.opus.balance-label", "token.color.text-secondary", "foreground"),
    ("content.opus.refresh-hint", "token.color.text-secondary", "foreground"),
    ("content.opus.updating-label", "token.color.text-secondary", "foreground"),
    ("content.opus.section-title", "token.color.text-muted", "foreground"),
    ("content.routes.section-title", "token.color.text-muted", "foreground"),
    ("content.settings.section-title", "token.color.text-muted", "foreground"),
    ("component.route-item", "token.color.text-primary", "nameForeground"),
    ("content.settings.api-label", "token.color.text-secondary", "foreground"),
    # The two 12px helpers: derived helper typography, but they still inherit
    # FluxBody's foreground.
    ("content.settings.api-helper", "token.typography.helper", "font"),
    ("content.settings.alerts-helper", "token.typography.helper", "font"),
    ("content.settings.api-helper", "token.color.text-secondary", "foreground"),
    ("content.settings.alerts-helper", "token.color.text-secondary", "foreground"),
    ("component.route-item", "token.typography.body-bold", "nameFont"),
    ("component.route-item", "token.typography.body", "stationsFont"),
    ("component.route-item", "token.spacing.4", "textGap"),
    ("control.routes.add", "token.color.border-light", "borderBrush"),
    ("control.routes.add", "token.color.text-primary", "foreground"),
    ("content.settings.api-label", "token.typography.body", "font"),
    ("control.settings.api-key", "token.color.surface", "background"),
    ("control.settings.api-key", "token.color.border-light", "borderBrush"),
        ("content.settings.language-label", "token.typography.body", "font"),
    ("content.settings.alerts-label", "token.typography.body", "font"),
        ("content.footer.version", "token.typography.body", "font"),
    ("content.footer.credit", "token.typography.body", "font"),
]
for frm, to, applies in UT:
    edges.append(edge(frm, "uses-token", to, E_DS("Declared style/brush consumption."), appliesTo=applies))

unresolved = [
    {
        "id": "unresolved.header.back-target",
        "question": "Where does Back navigate?",
        "relatedIds": ["control.header.back"],
        "possibleValues": ["previous screen on the navigation stack"],
        "reason": "GoBack delegates to INavigator.GoBack; the destination depends on the runtime stack, not a declared screen.",
    },
    {
        "id": "unresolved.routes.add-behavior",
        "question": "What does Add New Route do?",
        "relatedIds": ["control.routes.add"],
        "possibleValues": ["open a route editor", "unknown"],
        "reason": "The button has no Command or Click handler in the source.",
    },
    {
        "id": "unresolved.settings.save-orphan",
        "question": "What invokes the declared SaveSettings command?",
        "relatedIds": ["screen.profile"],
        "possibleValues": ["auto-save on change", "missing UI", "dead code"],
        "reason": "ProfileModel.SaveSettings exists but nothing in the page binds it.",
    },
    {
        "id": "unresolved.settings.unbound-toggles",
        "question": "Are the language chips and alerts toggle meant to bind to model state?",
        "relatedIds": ["control.settings.language", "control.settings.alerts"],
        "possibleValues": ["bind to SelectedLanguage / a notifications state", "static mock"],
        "reason": "Chip IsChecked and ToggleSwitch IsOn are XAML literals; SelectedLanguage exists in the model but is unused by the page.",
    },
]

gold = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.flux-profile",
    "name": "FluxTransit Profile",
    "description": "Gold graph for the FluxTransit ProfilePage (source-backed: XAML + MVUX model + FluxStyles). Kit v0.4 altitude. Gold v1.1: canonical renamed glass-panel -> card per the kit's own Pass-8 vocabulary (defect found by blind runs).",
    "sourceSummary": [XAML,
                      {"type": "csharp", "path": "FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml.cs"},
                      VM, DS],
    "nodes": nodes,
    "edges": edges,
    "unresolved": unresolved,
}

OUT.mkdir(parents=True, exist_ok=True)
(OUT / "gold.graph.json").write_text(json.dumps(gold, indent=2) + "\n")
print("gold06 nodes:", len(nodes), "edges:", len(edges), "unresolved:", len(unresolved))
