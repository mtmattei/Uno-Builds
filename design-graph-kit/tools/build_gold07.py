#!/usr/bin/env python3
"""Gold graph for eval 07 — Caffe MainPage (source-backed, MVVM, kit v0.4.2).

Sources: MainPage.xaml + MainPage.xaml.cs, ViewModels/MainViewModel.cs
(CommunityToolkit.Mvvm: ObservableObject, [ObservableProperty],
[RelayCommand(CanExecute)]), Models/EspressoItem.cs, Controls/*.xaml
(resource consumption audited), Styles/AppResources.xaml.
Altitude: v0.4.2 (custom controls = components/controls with uno.class;
internals as properties; consensus token wiring; screen-scoped tokens).
"""
import json, pathlib

OUT = pathlib.Path(__file__).resolve().parents[1] / "evals" / "07-caffe-main"
XAML = {"type": "xaml", "path": "Caffe/Caffe/MainPage.xaml"}
CB = {"type": "csharp", "path": "Caffe/Caffe/MainPage.xaml.cs"}
VM = {"type": "csharp", "path": "Caffe/Caffe/ViewModels/MainViewModel.cs"}
DS = {"type": "design-system", "path": "Caffe/Caffe/Styles/AppResources.xaml"}
CTRL = {"type": "xaml", "path": "Caffe/Caffe/Controls/"}


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


def tok(id, name, cat, value, uno, rationale, kind="declared", src=DS):
    return node(id, "token", name=name, category=cat, value=value,
                properties={"uno": uno} if uno else None and {},
                evidence=ev(kind, 1.0, src, rationale)) if uno else node(
                id, "token", name=name, category=cat, value=value,
                evidence=ev(kind, 1.0, src, rationale))


ESPRESSOS = [
    ("espresso", "Espresso", 30, "Pure, concentrated, bold", "EspressoCard"),
    ("doppio", "Doppio", 60, "Double the intensity", "DoppioCard"),
    ("ristretto", "Ristretto", 20, "Short, sweet, powerful", "RistrettoCard"),
    ("lungo", "Lungo", 50, "Long pull, smooth finish", "LungoCard"),
]

nodes = [
    node("screen.caffe-main", "screen", name="Caffe main (brew)",
         properties={"uno": {"type": "Page", "class": "Caffe.MainPage",
                             "viewModel": "Caffe.ViewModels.MainViewModel (CommunityToolkit.Mvvm)"}},
         evidence=ev("observed", 1.0, XAML)),

    # header / footer (declared reusable controls)
    node("component.caffe-header", "component", name="Caffe header", role="pageHeader",
         properties={"parts": ["accent-bar", "logo", "tagline"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.CaffeHeader"}},
         evidence=ev("declared", 1.0, CTRL)),
    node("component.caffe-footer", "component", name="Caffe footer", role="pageFooter",
         properties={"parts": ["accent-bar"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.CaffeFooter"}},
         evidence=ev("declared", 1.0, CTRL)),

    # menu region: canonical espresso card + 4 instances
    node("region.caffe-main.menu", "region", name="Espresso menu",
         evidence=ev("observed", 1.0, XAML, "2x2 grid of espresso cards.")),
    node("component.espresso-card", "component", name="Espresso card", role="card",
         properties={"parts": ["name", "volume-badge", "description"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.EspressoCard"}},
         evidence=ev("declared", 1.0, CTRL,
                     "Reusable card bound to an EspressoItem (Name, VolumeML, Description); IsSelected visual.")),
]

for slug, name_, vol, desc, xname in ESPRESSOS:
    nodes.append(node(f"component.espresso-card.{slug}", "component", name=name_,
                      properties={"name": name_, "volumeML": vol, "description": desc,
                                  "uno": {"xName": xname}},
                      evidence=ev("declared", 1.0, XAML,
                                  "Bound to ViewModel.EspressoItems; Tapped selects this espresso.")))

nodes += [
    # parameters region: three interactive parameter controls
    node("region.caffe-main.parameters", "region", name="Brew parameters",
         evidence=ev("observed", 1.0, XAML, "Three-column parameter panel.")),
    node("component.temperature-gauge", "component", name="Temperature", role="gauge",
         semanticRole="temperatureInput",
         properties={"value": "{x:Bind ViewModel.Temperature, Mode=TwoWay}",
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.TemperatureGauge", "xName": "TempGauge"}},
         evidence=ev("declared", 1.0, XAML)),
    node("component.extraction-arc", "component", name="Extraction time", role="dial",
         semanticRole="extractionTimeInput",
         properties={"value": "{x:Bind ViewModel.ExtractionTime, Mode=TwoWay}",
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.ExtractionArc", "xName": "ExtractionArc"}},
         evidence=ev("declared", 1.0, XAML)),
    node("component.grind-selector", "component", name="Grind level", role="selector",
         semanticRole="grindLevelInput",
         properties={"value": "{x:Bind ViewModel.GrindLevel, Mode=TwoWay}",
                     "options": ["Fine", "Medium", "Coarse"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.GrindSelector", "xName": "GrindSelector"}},
         evidence=ev("declared", 1.0, VM, "GrindLevel enum clamped 0-2; labels via ToLabel.")),

    # selection overview + brew
    node("component.selection-overview", "component", name="Selection overview", role="summary",
         semanticRole="selectionSummary",
         properties={"parts": ["espresso-name", "temperature", "grind", "extraction-time"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.SelectionOverview", "xName": "SelectionOverview"}},
         evidence=ev("declared", 1.0, XAML, "Visible only while HasSelection.")),
    node("component.brew-button", "component", name="Brew", role="button",
         semanticRole="primaryAction",
         properties={"text": "{x:Bind ViewModel.BrewButtonText}",
                     "command": "BrewCommand (RelayCommand, CanExecute=HasSelection)",
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.BrewButton", "xName": "BrewBtn"}},
         evidence=ev("declared", 1.0, CB, "BrewRequested -> BrewCommand.ExecuteAsync when CanExecute.")),

    # states (declared via ViewModel + bindings)
    node("state.espresso-card.selected", "state", name="Card selected", semanticRole="selected",
         properties={"uno": {"mechanism": "binding", "member": "HasSelection / IsSelected"}},
         evidence=ev("declared", 1.0, VM, "Selected card gets IsSelected visual via code-behind sync.")),
    # Cross-model review: these were named for the FALSE branch of HasSelection
    # while recording the member without its polarity, which left the semantics
    # machine-ambiguous - and made the selection trigger edges point at the
    # states a selection *ends*. Named for the condition they actually describe,
    # with the polarity explicit.
    node("state.brew-button.enabled", "state", name="Brew enabled", semanticRole="enabled",
         properties={"when": "HasSelection == true",
                     "uno": {"mechanism": "binding", "member": "HasSelection", "property": "RelayCommand CanExecute"}},
         evidence=ev("declared", 1.0, VM, "BrewCommand CanExecute=HasSelection; the button becomes invokable once a card is selected.")),
    node("state.brew-button.selection-label", "state", name="Brew label names the selection",
         semanticRole="labelled",
         properties={"when": "HasSelection == true", "text": "Brew {SelectedEspresso.Name}",
                     "uno": {"mechanism": "binding", "member": "BrewButtonText"}},
         evidence=ev("declared", 1.0, VM, "BrewButtonText becomes 'Brew {SelectedEspresso.Name}' once a selection exists.")),
    node("state.selection-overview.visible", "state", name="Overview visible", semanticRole="visible",
         properties={"when": "HasSelection == true",
                     "uno": {"mechanism": "binding", "member": "HasSelection"}},
         evidence=ev("declared", 1.0, VM, "Overview Visibility bound to HasSelection; shown once a card is selected.")),
    node("state.caffe-main.brewing", "state", name="Brewing", semanticRole="busy",
         properties={"uno": {"mechanism": "binding", "member": "IsBrewing"}},
         evidence=ev("declared", 1.0, VM,
                     "BrewCommand sets IsBrewing; MainContent hides, BrewingScreen overlay x:Loads; ~2.5s simulated progress.")),
    node("component.brewing-screen", "component", name="Brewing overlay", role="overlay",
         semanticRole="progressOverlay",
         properties={"parts": ["espresso-name", "parameters-text", "progress"],
                     "bindings": ["EspressoName", "ParametersText", "BrewProgress"],
                     "uno": {"type": "UserControl", "class": "Caffe.Controls.BrewingScreen", "xName": "BrewingOverlay"}},
         evidence=ev("declared", 1.0, XAML)),
]

# tokens — declared in AppResources.xaml, consumed by the page or its components
TOKENS = [
    # Cross-model review: these four are declared Colors consumed directly as
    # GradientStops by the modeled controls (BrewingScreen, TemperatureGauge).
    # They are resting runtime visuals, not interaction-only variants, so the
    # variant-folding rule does not exclude them - they were simply missed.
    ("token.color.coffee-dark", "Coffee dark (brew gradient)", "color", "#3D2314", {"resourceKey": "CoffeeDarkColor", "resourceType": "Color"}),
    ("token.color.coffee-light", "Coffee light (brew gradient)", "color", "#5D3A1A", {"resourceKey": "CoffeeLightColor", "resourceType": "Color"}),
    ("token.color.temperature-high", "Temperature high", "color", "#C1121F", {"resourceKey": "CaffeTemperatureHighColor", "resourceType": "Color"}),
    ("token.color.temperature-low", "Temperature low", "color", "#E07070", {"resourceKey": "CaffeTemperatureLowColor", "resourceType": "Color"}),
    ("token.color.background", "Background", "color", "#FAFAFA", {"resourceKey": "CaffeBackgroundBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.surface", "Surface", "color", "#FFFFFF", {"resourceKey": "CaffeSurfaceBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.primary", "Primary (espresso green)", "color", "#1B4332", {"resourceKey": "CaffePrimaryBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.accent-red", "Accent red", "color", "#C1121F", {"resourceKey": "CaffeAccentRedBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.accent-green", "Accent green", "color", "#2D6A4F", {"resourceKey": "CaffeAccentGreenBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-primary", "Text primary", "color", "#1A1A1A", {"resourceKey": "CaffeTextPrimaryBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-secondary", "Text secondary", "color", "#888888", {"resourceKey": "CaffeTextSecondaryBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.text-muted", "Text muted", "color", "#999999", {"resourceKey": "CaffeTextMutedBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.border", "Border", "color", "#E0E0E0", {"resourceKey": "CaffeBorderBrush", "resourceType": "SolidColorBrush"}),
    ("token.color.on-primary", "On primary", "color", "#FFFFFF", {"resourceKey": "CaffeOnPrimaryBrush", "resourceType": "SolidColorBrush"}),
    ("token.typography.logo", "Logo type", "typography", {"family": "Cormorant Light", "size": 48}, {"styleKey": "LogoTextStyle"}),
    ("token.typography.tagline", "Tagline type", "typography", {"family": "DM Sans Medium", "size": 11, "tracking": 150}, {"styleKey": "TaglineTextStyle"}),
    ("token.typography.card-title", "Card title type", "typography", {"family": "Cormorant", "size": 22}, {"styleKey": "CardTitleTextStyle"}),
    ("token.typography.card-description", "Card description type", "typography", {"family": "DM Sans", "size": 12}, {"styleKey": "CardDescriptionTextStyle"}),
    ("token.typography.volume-badge", "Volume badge type", "typography", {"family": "DM Sans Medium", "size": 11}, {"styleKey": "VolumeBadgeTextStyle"}),
    ("token.typography.parameter-value", "Parameter value type", "typography", {"family": "Cormorant", "size": 26}, {"styleKey": "ParameterValueTextStyle"}),
    ("token.typography.parameter-label", "Parameter label type", "typography", {"family": "DM Sans Medium", "size": 10, "tracking": 100}, {"styleKey": "ParameterLabelTextStyle"}),
    ("token.typography.button", "Button type", "typography", {"family": "DM Sans"}, {"styleKey": "ButtonTextStyle"}),
    ("token.typography.grind-hint", "Grind hint type", "typography", {"family": "DM Sans"}, {"styleKey": "GrindHintTextStyle"}),
    ("token.typography.overview-label", "Overview label type", "typography", {"family": "DM Sans"}, {"styleKey": "OverviewLabelTextStyle"}),
    ("token.typography.overview-value", "Overview value type", "typography", {"family": "DM Sans"}, {"styleKey": "OverviewValueTextStyle"}),
    ("token.typography.brewing-title", "Brewing title type", "typography", {"family": "Cormorant"}, {"styleKey": "BrewingTitleTextStyle"}),
    ("token.typography.body", "Body type", "typography", {"family": "DM Sans"}, {"styleKey": "BodyTextStyle"}),
    ("token.typography.arc-label", "Arc label type", "typography", {"family": "DM Sans"}, {"styleKey": "ArcLabelTextStyle"}),
]
for id_, name_, cat, val, uno in TOKENS:
    nodes.append(node(id_, "token", name=name_, category=cat, value=val,
                      properties={"uno": uno},
                      evidence=ev("declared", 1.0, DS)))
nodes.append(node("token.radius.14", "token", name="14 radius", category="radius", value=14,
                  properties={"unit": "px"},
                  evidence=ev("derived", 1.0, CTRL,
                              "Recurs across EspressoCard, BrewButton, SelectionOverview.")))

E_OBS = lambda src=XAML: ev("observed", 1.0, src)

edges = [
    edge("screen.caffe-main", "contains", "component.caffe-header", E_OBS()),
    edge("screen.caffe-main", "contains", "region.caffe-main.menu", E_OBS()),
    edge("screen.caffe-main", "contains", "region.caffe-main.parameters", E_OBS()),
    edge("screen.caffe-main", "contains", "component.brew-button", E_OBS()),
    edge("screen.caffe-main", "contains", "component.caffe-footer", E_OBS()),
    edge("region.caffe-main.parameters", "contains", "component.temperature-gauge", E_OBS()),
    edge("region.caffe-main.parameters", "contains", "component.extraction-arc", E_OBS()),
    edge("region.caffe-main.parameters", "contains", "component.grind-selector", E_OBS()),
    edge("screen.caffe-main", "has-state", "state.caffe-main.brewing", ev("declared", 1.0, VM)),
    edge("screen.caffe-main", "contains", "component.selection-overview", E_OBS()),
    edge("component.espresso-card", "has-state", "state.espresso-card.selected", ev("declared", 1.0, VM)),
    edge("component.brew-button", "has-state", "state.brew-button.enabled", ev("declared", 1.0, VM)),
    edge("component.brew-button", "has-state", "state.brew-button.selection-label", ev("declared", 1.0, VM)),
    edge("component.selection-overview", "has-state", "state.selection-overview.visible", ev("declared", 1.0, VM)),
    edge("state.caffe-main.brewing", "contains", "component.brewing-screen", ev("declared", 1.0, XAML, "x:Load bound to IsBrewing.")),
    edge("component.brew-button", "triggers", "state.caffe-main.brewing",
         ev("declared", 1.0, CB, "BrewRequested -> BrewCommand -> IsBrewing=true for the simulated brew.")),
    edge("component.espresso-card", "triggers", "state.espresso-card.selected",
         ev("declared", 1.0, CB, "Every card's Tapped handler selects its espresso (canonical attachment; instances identical).")),
    # Multi-effect rule (v0.6), from the cross-model review: one selection
    # changes four local presentations, not one. Recording only the card visual
    # understated what the source declares - and the three affected states were
    # already modeled, so only the trigger edges were missing.
    edge("component.espresso-card", "triggers", "state.selection-overview.visible",
         ev("declared", 1.0, VM, "Selection sets HasSelection, which reveals the otherwise-hidden "
                                 "selection overview.")),
    edge("component.espresso-card", "triggers", "state.brew-button.selection-label",
         ev("declared", 1.0, VM, "Selection rewrites BrewButtonText to 'Brew {SelectedEspresso.Name}'.")),
    edge("component.espresso-card", "triggers", "state.brew-button.enabled",
         ev("declared", 1.0, VM, "HasSelection also lifts the brew button out of its disabled state and "
                                 "changes its label to 'Brew {SelectedEspresso.Name}'.")),
]
for slug, *_ , xname in [(s, n, v, d, x) for s, n, v, d, x in ESPRESSOS]:
    edges.append(edge("region.caffe-main.menu", "contains", f"component.espresso-card.{slug}", E_OBS()))
    edges.append(edge(f"component.espresso-card.{slug}", "instance-of", "component.espresso-card",
                      ev("derived", 1.0, XAML)))


UT = [
    ("screen.caffe-main", "token.color.background", "background"),
    ("component.caffe-header", "token.typography.logo", "logoFont"),
    ("component.caffe-header", "token.typography.tagline", "taglineFont"),
    ("component.caffe-header", "token.color.primary", "accentBarLeftColor"),
    ("component.caffe-header", "token.color.accent-red", "accentBarRightColor"),
    ("component.caffe-footer", "token.color.primary", "accentColor"),
    ("component.caffe-footer", "token.color.accent-red", "accentColor2"),
    ("component.espresso-card", "token.color.surface", "background"),
    ("component.espresso-card", "token.color.border", "borderBrush"),
    ("component.espresso-card", "token.radius.14", "cornerRadius"),
    ("component.espresso-card", "token.color.primary", "selectionAccent"),
    ("component.espresso-card", "token.typography.card-title", "nameFont"),
    ("component.espresso-card", "token.typography.card-description", "descriptionFont"),
    ("component.espresso-card", "token.typography.volume-badge", "badgeFont"),
    ("component.espresso-card", "token.color.on-primary", "badgeForeground"),
    ("component.temperature-gauge", "token.color.surface", "background"),
    ("component.temperature-gauge", "token.typography.parameter-value", "valueFont"),
    ("component.temperature-gauge", "token.typography.parameter-label", "labelFont"),
    ("component.extraction-arc", "token.color.surface", "background"),
    ("component.extraction-arc", "token.typography.parameter-value", "valueFont"),
    ("component.extraction-arc", "token.typography.parameter-label", "labelFont"),
    ("component.extraction-arc", "token.color.accent-green", "arcColor"),
    ("component.grind-selector", "token.color.surface", "background"),
    ("component.grind-selector", "token.typography.parameter-value", "valueFont"),
    ("component.grind-selector", "token.typography.parameter-label", "labelFont"),
    ("component.selection-overview", "token.color.primary", "background"),
    ("component.selection-overview", "token.radius.14", "cornerRadius"),
    ("component.brew-button", "token.color.primary", "background"),
    ("component.brew-button", "token.radius.14", "cornerRadius"),
    ("component.brewing-screen", "token.color.background", "background"),
    ("component.brewing-screen", "token.color.text-primary", "titleColor"),
    ("component.brewing-screen", "token.color.text-secondary", "parametersColor"),
    ("component.espresso-card", "token.color.text-muted", "descriptionColor"),
    ("component.brew-button", "token.typography.button", "font"),
    ("component.grind-selector", "token.typography.grind-hint", "hintFont"),
    ("component.selection-overview", "token.typography.overview-label", "labelFont"),
    ("component.selection-overview", "token.typography.overview-value", "valueFont"),
    ("component.brewing-screen", "token.typography.brewing-title", "titleFont"),
    ("component.brewing-screen", "token.typography.body", "bodyFont"),
    ("component.extraction-arc", "token.typography.arc-label", "labelFont"),
    # Gradient stops consumed directly by the two controls that declare them.
    ("component.brewing-screen", "token.color.coffee-dark", "gradientStop"),
    ("component.brewing-screen", "token.color.coffee-light", "gradientStop"),
    ("component.temperature-gauge", "token.color.temperature-high", "gradientStop"),
    ("component.temperature-gauge", "token.color.temperature-low", "gradientStop"),
]
for frm, to, applies in UT:
    edges.append(edge(frm, "uses-token", to,
                      ev("declared", 1.0, DS, "Audited StaticResource consumption in the page/controls."),
                      appliesTo=applies))

unresolved = [
    {
        "id": "unresolved.menu.data-source",
        "question": "Is the four-espresso menu fixed, or intended to become data-driven?",
        "relatedIds": ["region.caffe-main.menu"],
        "possibleValues": ["fixed product menu", "placeholder for a data source"],
        "reason": "EspressoItems is a hard-coded ObservableCollection in the ViewModel; no service or persistence is referenced.",
    },
]

gold = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.caffe-main",
    "name": "Caffe Main",
    "description": "Gold graph for the Caffe MainPage (source-backed MVVM: CommunityToolkit ObservableObject/RelayCommand, x:Bind). Kit v0.5 altitude. Gold v1.1: calibrated to the eval-07 5/5 blind consensus (screen slug, UserControl=component, per-site states, canonical trigger attachment, token breadth).",
    "sourceSummary": [XAML, CB, VM,
                      {"type": "csharp", "path": "Caffe/Caffe/Models/EspressoItem.cs"},
                      CTRL, DS],
    "nodes": nodes,
    "edges": edges,
    "unresolved": unresolved,
}

OUT.mkdir(parents=True, exist_ok=True)
(OUT / "gold.graph.json").write_text(json.dumps(gold, indent=2) + "\n")
print("gold07 nodes:", len(nodes), "edges:", len(edges), "unresolved:", len(unresolved))
