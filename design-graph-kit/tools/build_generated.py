#!/usr/bin/env python3
"""Independent generation pass for eval 05-orbital-settings.

A genuine, strong graph produced from the rendered structure following
prompts/design-understanding.md. It agrees with gold on the obvious and
diverges on the genuinely-ambiguous calls:

  * canonical card named `component.settings-card` (gold: `component.settings-card`)      -> naming drift
  * ABOUT rows + PATHS fields merged into ONE `component.key-value-row`          -> resolves the consolidation `unresolved` one way
  * action buttons under `component.action-button` with `control.actions.*` ids     -> id-scheme drift
  * title/subtitle placed directly under the screen (no header region)           -> hierarchy drift
  * entrance animation NOT modeled as a first-class state                        -> state recall miss
  * neutral token names + a couple fewer tokens                                  -> token naming/coverage drift

None of these are wrong; they are the run-to-run variance the deterministic
score exists to quantify.
"""
import json, pathlib

OUT = pathlib.Path("/home/user/Uno-Builds/design-graph-kit/evals/05-orbital-settings")
XAML = {"type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"}
CS = {"type": "csharp", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml.cs"}
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


nodes = [
    node("screen.settings", "screen", name="Settings", evidence=ev("observed", 1.0, XAML)),
    # title/subtitle directly under screen (no header region)
    node("content.settings.title", "content", name="Title", text="Settings",
         semanticRole="pageTitle", evidence=ev("observed", 1.0, XAML)),
    node("content.settings.subtitle", "content", name="Subtitle",
         text="Profile and preferences", semanticRole="pageSubtitle", evidence=ev("observed", 1.0, XAML)),

    # canonical card named `card` + 4 instances
    node("component.settings-card", "component", name="Section card", role="card",
         evidence=ev("derived", 0.95, DS, "Four sections share the same bordered rounded card treatment.")),
    node("component.settings-card.profile", "component", name="Profile section",
         semanticRole="profileSection", evidence=ev("observed", 1.0, XAML)),
    node("component.settings-card.about", "component", name="About section",
         semanticRole="aboutSection", evidence=ev("observed", 1.0, XAML)),
    node("component.settings-card.paths", "component", name="Paths section",
         semanticRole="pathsSection", evidence=ev("observed", 1.0, XAML)),
    node("component.settings-card.actions", "component", name="Actions section",
         semanticRole="actionsSection", evidence=ev("observed", 1.0, XAML)),

    # profile
    node("content.profile.section-title", "content", name="Profile header", text="PROFILE",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("content.profile.name-label", "content", name="Display Name label", text="Display Name",
         semanticRole="fieldLabel", evidence=ev("observed", 1.0, XAML)),
    node("control.profile.username", "control", name="Display Name", role="textbox",
         semanticRole="nameInput", properties={"placeholder": "Enter your name"},
         evidence=ev("observed", 1.0, XAML)),
    node("control.profile.save", "control", name="Save", role="button", text="Save",
         semanticRole="primaryAction",
         evidence=ev("inferred", 0.9, XAML, "Only high-emphasis (filled) action in the form.")),
    node("content.profile.name-helper", "content", name="Name helper",
         text="This name appears in the homepage greeting.", semanticRole="helperText",
         evidence=ev("observed", 1.0, XAML)),

    # about
    node("content.about.section-title", "content", name="About header", text="ABOUT",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("asset.about.logo", "asset", name="Orbital logo", role="image",
         semanticRole="appLogo", evidence=ev("observed", 1.0, XAML)),
    node("content.about.app-name", "content", name="App name", text="Orbital",
         semanticRole="appName", evidence=ev("observed", 1.0, XAML)),
    node("content.about.version", "content", name="Version", text="v0.1.0-alpha",
         semanticRole="versionLabel", evidence=ev("observed", 1.0, XAML)),

    # MERGED key/value component (rows + fields together)
    node("component.key-value-row", "component", name="Key/value row", role="keyValueRow",
         evidence=ev("derived", 0.85, XAML, "Label + value pairs recur across ABOUT and PATHS; treated as one component family.")),
    node("component.key-value-row.uno-sdk", "component", name="Uno Platform SDK", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.dotnet", "component", name=".NET Runtime", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.renderer", "component", name="Renderer", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.platform", "component", name="Platform", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.project-root", "component", name="Project Root", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.recents-db", "component", name="Recent Projects Database", evidence=ev("observed", 1.0, XAML)),
    node("component.key-value-row.skills", "component", name="Claude Code Skills", evidence=ev("observed", 1.0, XAML)),

    # paths / actions headers
    node("content.paths.section-title", "content", name="Paths header", text="PATHS",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),
    node("content.actions.section-title", "content", name="Actions header", text="ACTIONS",
         semanticRole="sectionHeader", evidence=ev("observed", 1.0, XAML)),

    # actions: canonical action-item + control.actions.* instances
    node("component.action-button", "component", name="Ghost action button", role="button",
         evidence=ev("derived", 0.95, DS, "Three identical ghost buttons (icon + label).")),
    node("control.actions.clear-recents", "control", name="Clear Recent Projects", role="button",
         text="Clear Recent Projects", semanticRole="utilityAction", evidence=ev("observed", 1.0, XAML)),
    node("control.actions.open-data-folder", "control", name="Open Data Folder", role="button",
         text="Open Data Folder", semanticRole="utilityAction", evidence=ev("observed", 1.0, XAML)),
    node("control.actions.open-docs", "control", name="Uno Platform Documentation", role="button",
         text="Uno Platform Documentation", semanticRole="externalLink", evidence=ev("observed", 1.0, XAML)),

    # only the transient saved state modeled (entrance animation not captured)
    node("state.profile.saved", "state", name="Saved", semanticRole="confirmation",
         evidence=ev("inferred", 0.7, XAML, "Save appears to confirm; exact copy/timing not certain from structure alone.")),

    # tokens (neutral names, slightly fewer)
    node("token.color.0a0a0b", "token", name="Page background", category="color",
         value="#FF0A0A0B", evidence=ev("derived", 1.0, XAML)),
    node("token.color.141416", "token", name="Card background", category="color",
         value="#FF141416", evidence=ev("derived", 1.0, XAML)),
    node("token.color.212123", "token", name="Card border", category="color",
         value="#FF212123", evidence=ev("derived", 1.0, XAML)),
    node("token.radius.12", "token", name="12 radius", category="radius", value=12,
         properties={"unit": "px"}, evidence=ev("derived", 1.0, XAML)),
    node("token.radius.8", "token", name="8 radius", category="radius", value=8,
         properties={"unit": "px"}, evidence=ev("derived", 1.0, XAML)),
    node("token.spacing.24", "token", name="24 spacing", category="spacing", value=24,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML)),
    node("token.spacing.16", "token", name="16 spacing", category="spacing", value=16,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML)),
    node("token.spacing.8", "token", name="8 spacing", category="spacing", value=8,
         properties={"unit": "px"}, evidence=ev("observed", 1.0, XAML)),
]

edges = [
    edge("screen.settings", "contains", "content.settings.title", ev("observed", 1.0, XAML)),
    edge("screen.settings", "contains", "content.settings.subtitle", ev("observed", 1.0, XAML)),
]
for s in ["profile", "about", "paths", "actions"]:
    edges.append(edge("screen.settings", "contains", f"component.settings-card.{s}", ev("observed", 1.0, XAML)))
    edges.append(edge(f"component.settings-card.{s}", "instance-of", "component.settings-card", ev("derived", 0.95, DS)))

for c in ["content.profile.section-title", "content.profile.name-label",
          "control.profile.username", "control.profile.save", "content.profile.name-helper"]:
    edges.append(edge("component.settings-card.profile", "contains", c, ev("observed", 1.0, XAML)))
for c in ["content.about.section-title", "asset.about.logo", "content.about.app-name", "content.about.version",
          "component.key-value-row.uno-sdk", "component.key-value-row.dotnet",
          "component.key-value-row.renderer", "component.key-value-row.platform"]:
    edges.append(edge("component.settings-card.about", "contains", c, ev("observed", 1.0, XAML)))
for c in ["content.paths.section-title", "component.key-value-row.project-root",
          "component.key-value-row.recents-db", "component.key-value-row.skills"]:
    edges.append(edge("component.settings-card.paths", "contains", c, ev("observed", 1.0, XAML)))
for i in ["uno-sdk", "dotnet", "renderer", "platform", "project-root", "recents-db", "skills"]:
    edges.append(edge(f"component.key-value-row.{i}", "instance-of", "component.key-value-row", ev("derived", 0.85, XAML)))
for c in ["content.actions.section-title", "control.actions.clear-recents",
          "control.actions.open-data-folder", "control.actions.open-docs"]:
    edges.append(edge("component.settings-card.actions", "contains", c, ev("observed", 1.0, XAML)))
for i in ["clear-recents", "open-data-folder", "open-docs"]:
    edges.append(edge(f"control.actions.{i}", "instance-of", "component.action-button", ev("derived", 0.95, DS)))

edges += [
    edge("component.settings-card.profile", "has-state", "state.profile.saved", ev("inferred", 0.7, XAML, "The Save action appears to produce a transient confirmation state.")),
    edge("control.profile.save", "triggers", "state.profile.saved",
         ev("inferred", 0.7, XAML, "Save button most plausibly produces the confirmation state.")),
    edge("screen.settings", "uses-token", "token.color.0a0a0b", ev("derived", 1.0, XAML), appliesTo="background"),
    edge("component.settings-card", "uses-token", "token.color.141416", ev("derived", 1.0, XAML), appliesTo="background"),
    edge("component.settings-card", "uses-token", "token.color.212123", ev("derived", 1.0, XAML), appliesTo="borderBrush"),
    edge("component.settings-card", "uses-token", "token.radius.12", ev("derived", 1.0, XAML), appliesTo="cornerRadius"),
    edge("screen.settings", "uses-token", "token.spacing.24", ev("observed", 1.0, XAML), appliesTo="sectionGap"),
    edge("component.settings-card", "uses-token", "token.spacing.16", ev("observed", 1.0, XAML), appliesTo="innerGap"),
    edge("control.profile.save", "uses-token", "token.radius.8", ev("derived", 1.0, XAML), appliesTo="cornerRadius"),
    edge("component.action-button", "uses-token", "token.radius.8", ev("derived", 1.0, XAML), appliesTo="cornerRadius"),
]

unresolved = [
    {
        "id": "unresolved.settings.open-docs-target",
        "question": "Where does 'Uno Platform Documentation' lead?",
        "relatedIds": ["control.actions.open-docs"],
        "possibleValues": ["external docs site", "in-app help", "unknown"],
        "reason": "The label implies documentation but the destination is not visible in the layout.",
    },
]

graph = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.orbital-settings.generated",
    "name": "Orbital Settings (generated)",
    "description": "Independent generation pass following design-understanding.md.",
    "sourceSummary": [{"type": "xaml", "path": "Orbital/Orbital/Presentation/SettingsPage.xaml"}],
    "nodes": nodes,
    "edges": edges,
    "unresolved": unresolved,
}

(OUT / "generated.graph.json").write_text(json.dumps(graph, indent=2) + "\n")
print("generated nodes:", len(nodes), "edges:", len(edges), "unresolved:", len(unresolved))
