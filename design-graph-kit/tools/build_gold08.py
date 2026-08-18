#!/usr/bin/env python3
"""Build the gold graph for eval 08 (Pens / Beers screen).

Authored from source, following the kit convention that golds are generated
rather than hand-patched: edit this file, regenerate, revalidate.

Sources:
  Pens/Pens/Presentation/BeersPage.xaml     - page content
  Pens/Pens/Presentation/BeersViewModel.cs  - MVVM (ObservableObject/RelayCommand)
  Pens/Pens/Presentation/CaseBlock.cs       - the tile record
  Pens/Pens/Presentation/Shell.xaml         - team header + bottom TabBar
  Pens/Pens/App.xaml                        - design system (colors, fonts, radii)
  Pens/Pens/Converters/Converters.cs        - IsConsumed -> brush mapping

Scope note: the eval's input image shows the *shell chrome* (team header, bottom
tab bar) around the page content, so the gold models that whole surface. A gold
covering BeersPage.xaml alone would score blind runs down for correctly
reporting what they can see.

Altitude note: this gold uses `region` nodes for the source's real structural
groupings. Gold 05 does not, and whether it should is open question 6 in the
review packets. Recorded here deliberately so the reviewer rules on one
consistent convention across both evals rather than on a silent divergence.
"""

from __future__ import annotations

import json
from pathlib import Path

PAGE = "Pens/Pens/Presentation/BeersPage.xaml"
VM = "Pens/Pens/Presentation/BeersViewModel.cs"
SHELL = "Pens/Pens/Presentation/Shell.xaml"
APP = "Pens/Pens/App.xaml"
CONV = "Pens/Pens/Converters/Converters.cs"
BLOCK = "Pens/Pens/Presentation/CaseBlock.cs"

nodes: list[dict] = []
edges: list[dict] = []


def ev(kind: str, path: str, rationale: str | None = None, conf: float = 1.0) -> dict:
    e = {"kind": kind, "confidence": conf, "source": {"type": "xaml" if path.endswith(".xaml") else "csharp", "path": path}}
    if rationale:
        e["rationale"] = rationale
    return e


def node(id_, type_, name, evidence, **extra):
    n = {"id": id_, "type": type_, "name": name, "evidence": evidence}
    n.update(extra)
    nodes.append(n)


def edge(frm, rel, to, evidence):
    edges.append({"from": frm, "relation": rel, "to": to, "evidence": evidence})


def contains(parent, child, path=PAGE):
    edge(parent, "contains", child, ev("observed", path))


def uses(consumer, token, path=PAGE):
    edge(consumer, "uses-token", token, ev("declared", path))


# ---------------------------------------------------------------- screen
node("screen.beers", "screen", "Beers",
     ev("observed", PAGE, "Page x:Class Pens.Presentation.BeersPage, hosted in Shell's NavigationContent."),
     properties={"uno": {"type": "Page", "class": "Pens.Presentation.BeersPage",
                         "xName": "PageRoot", "styleKey": "ArenaDarkBrush"}})

# ---------------------------------------------------------------- shell chrome
node("region.header", "region", "Team header",
     ev("observed", SHELL, "Shell.xaml row 0: bordered header bar with logo and team identity."),
     properties={"uno": {"type": "Border", "property": "AutomationProperties.Name"}})
node("asset.team-logo", "asset", "Penguins team logo",
     ev("observed", SHELL, "Image Source ms-appx:///Assets/images/pens-logo.png in a 56x56 rounded border."),
     properties={"uno": {"type": "Image", "source": "ms-appx:///Assets/images/pens-logo.png"}})
node("content.header.team-name", "content", "PENGUINS", ev("observed", SHELL),
     text="PENGUINS", properties={"uno": {"type": "TextBlock", "fontResourceKey": "BebasNeueFont"}})
node("content.header.league-name", "content", "DORVAL YOUNGTIMERS", ev("observed", SHELL),
     text="DORVAL YOUNGTIMERS", properties={"uno": {"type": "TextBlock", "fontResourceKey": "BarlowMedium"}})

node("region.tab-bar", "region", "Bottom navigation",
     ev("observed", SHELL, "Shell.xaml row 2: utu:TabBar with five TabBarItems in a 5-column ItemsPanel."),
     properties={"uno": {"type": "TabBar", "xName": "TabBar"}})
node("component.tab-item", "component", "Tab item", ev("observed", SHELL,
     "Five utu:TabBarItem instances, each an icon over a caption; identical internals, Tag distinguishes them."),
     role="navigationTab",
     properties={"uno": {"type": "TabBarItem", "property": "Tag"},
                 "internals": ["icon", "caption"]})
for slug, label, glyph in [
    ("schedule", "Schedule", "Calendar"),
    ("chat", "Chat", "Message"),
    ("beers", "Beers", "&#xE799;"),
    ("duties", "Duties", "Bullets"),
    ("roster", "Roster", "People"),
]:
    node(f"component.tab-item.{slug}", "component", f"{label} tab", ev("observed", SHELL),
         text=label, properties={"uno": {"type": "TabBarItem", "property": f"Tag={label}", "iconGlyph": glyph}})
    edge(f"component.tab-item.{slug}", "instance-of", "component.tab-item", ev("observed", SHELL))
    contains("region.tab-bar", f"component.tab-item.{slug}", SHELL)

# Cross-model review, critical finding: the first version of this gold recorded
# the tab destinations as `unresolved`, because it was authored from Shell.xaml
# without opening Shell.xaml.cs - where `_pageFactories` maps every Tag to its
# page (lines 13-20), OnTabSelectionChanged reads that tag, and NavigateToTab
# installs the page. The destinations are fully declared. The sibling screens
# are modeled as bare navigation targets; their internals are out of scope.
SHELLCS = "Pens/Pens/Presentation/Shell.xaml.cs"
for slug, label, page in [
    ("schedule", "Schedule", "SchedulePage"),
    ("chat", "Chat", "ChatPage"),
    ("duties", "Duties", "DutiesPage"),
    ("roster", "Roster", "RosterPage"),
]:
    node(f"screen.{slug}", "screen", label,
         ev("declared", SHELLCS, f'_pageFactories["{label}"] constructs {page} with its view model.'),
         properties={"uno": {"type": "Page", "class": f"Pens.Presentation.{page}"},
                     "scopeNote": "navigation target only; not modeled in this eval"})
    edge(f"component.tab-item.{slug}", "navigates-to", f"screen.{slug}",
         ev("declared", SHELLCS, "OnTabSelectionChanged -> NavigateToTab(tag) -> NavigationContent."))
edge("component.tab-item.beers", "navigates-to", "screen.beers",
     ev("declared", SHELLCS, 'OnTabSelectionChanged -> NavigateToTab("Beers") -> this screen.'))

# ---------------------------------------------------------------- hero summary
node("region.beer-summary", "region", "Season consumption summary",
     ev("observed", PAGE, "Centered StackPanel; AutomationProperties.Name 'Season beer consumption statistics'."))
node("content.summary.cases-value", "content", "Consumed cases value",
     ev("declared", PAGE, "TextBlock bound to ConsumedCases, 80pt display face."),
     properties={"uno": {"type": "TextBlock", "member": "ConsumedCases", "fontResourceKey": "BebasNeueFont"}})
node("content.summary.cases-label", "content", "cases", ev("observed", PAGE),
     text="cases", properties={"uno": {"type": "TextBlock", "property": "x:Uid=BeersPage_Cases"}})
node("content.summary.season-caption", "content", "CONSUMED THIS SEASON", ev("observed", PAGE),
     text="CONSUMED THIS SEASON",
     properties={"uno": {"type": "TextBlock", "property": "x:Uid=BeersPage_ConsumedThisSeason"}})
node("content.summary.total-beers", "content", "Total beers line",
     ev("declared", PAGE, "Runs: bound TotalBeers, ' beers ', then literal '(30 per case)'."),
     text="780 beers (30 per case)",
     properties={"uno": {"type": "TextBlock", "member": "TotalBeers"}})

# ---------------------------------------------------------------- canonical card
node("component.card", "component", "Card",
     ev("observed", PAGE, "Bordered rounded container: BoardsDarkBrush fill, 1px border, used for the tracker and the four stat boxes."),
     role="container",
     properties={"uno": {"type": "Border", "styleKey": "BoardsDarkBrush"},
                 "internals": ["background", "cornerRadius", "border"]})

# ---------------------------------------------------------------- season tracker
node("component.card.season-tracker", "component", "Season tracker card",
     ev("observed", PAGE, "Border with LargeCornerRadius and SubtleBorderBrush containing the tracker grid."),
     properties={"uno": {"type": "Border", "resourceKey": "LargeCornerRadius"}})
edge("component.card.season-tracker", "instance-of", "component.card", ev("observed", PAGE))

node("content.tracker.title", "content", "SEASON TRACKER", ev("observed", PAGE),
     text="SEASON TRACKER", properties={"uno": {"type": "TextBlock", "property": "x:Uid=BeersPage_SeasonTracker"}})
node("content.tracker.counter", "content", "Case counter",
     ev("declared", PAGE, "Runs: bound ConsumedCases then literal ' / 52 cases'."),
     text="26 / 52 cases", properties={"uno": {"type": "TextBlock", "member": "ConsumedCases"}})

node("control.tracker.case-grid", "control", "Case grid",
     ev("declared", PAGE, "ItemsRepeater over CaseBlocks with UniformGridLayout (28x28 min, 8 spacing); "
                          "utu:CommandExtensions.Command binds ToggleCaseCommand."),
     role="itemsRepeater",
     properties={"uno": {"type": "ItemsRepeater", "member": "CaseBlocks",
                         "property": "utu:CommandExtensions.Command=ToggleCaseCommand"}})
node("component.case-tile", "component", "Case tile",
     ev("declared", PAGE, "DataTemplate x:DataType local:CaseBlock: a Border whose Background and BorderBrush "
                          "come from IsConsumed through two converters. 52 instances (TotalCases)."),
     role="statusTile",
     properties={"uno": {"type": "Border", "resourceKey": "SmallCornerRadius", "member": "IsConsumed"},
                 "instanceCount": 52})
node("state.case-tile.consumed", "state", "Consumed",
     ev("declared", CONV, "BoolToConsumedBackgroundConverter -> NeonAmberBrush and "
                          "BoolToConsumedBorderConverter -> NeonAmberSemiBorderBrush when IsConsumed is true; "
                          "the resting treatment is BoardsMidBrush / SubtleWhiteBorderBrush."),
     properties={"uno": {"member": "IsConsumed", "mechanism": "IValueConverter on the item template"}})

# ---------------------------------------------------------------- legend
node("region.legend", "region", "Tracker legend",
     ev("observed", PAGE, "Centered horizontal AutoLayout; AutomationProperties.Name 'Legend for beer tracker'."))
node("component.legend-swatch", "component", "Legend swatch",
     ev("observed", PAGE, "18x18 rounded Border plus caption; two instances distinguished only by fill."),
     role="legendKey", properties={"uno": {"type": "Border"}, "internals": ["swatch", "caption"]})
for slug, label in [("remaining", "Remaining"), ("consumed", "Consumed")]:
    node(f"component.legend-swatch.{slug}", "component", f"{label} swatch", ev("observed", PAGE),
         text=label, properties={"uno": {"type": "Border", "property": f"x:Uid=BeersPage_{label}"}})
    edge(f"component.legend-swatch.{slug}", "instance-of", "component.legend-swatch", ev("observed", PAGE))
    contains("region.legend", f"component.legend-swatch.{slug}")

# ---------------------------------------------------------------- stats
node("region.stats-grid", "region", "Stats grid",
     ev("observed", PAGE, "2x2 Grid with 12px gutter columns/rows holding four stat cards."))
node("component.stat-card", "component", "Stat card",
     ev("observed", PAGE, "Border with CardCornerRadius and CardBorderBrush, 16 padding, containing a "
                          "32pt display value over an 11pt letterspaced caption. Four identical instances."),
     role="statistic",
     properties={"uno": {"type": "Border", "resourceKey": "CardCornerRadius", "styleKey": "CardBorderBrush"},
                 "internals": ["value", "caption"]})
edge("component.stat-card", "instance-of", "component.card", ev("derived", PAGE))
for slug, value, caption, uid in [
    ("avg-per-game", "10", "AVG / GAME", "BeersPage_AvgPerGame"),
    ("games-played", "12", "GAMES PLAYED", "BeersPage_GamesPlayed"),
    ("top-consumer", "B-Rob", "TOP CONSUMER", "BeersPage_TopConsumer"),
    ("most-in-game", "18", "MOST IN A GAME", "BeersPage_MostInGame"),
]:
    node(f"component.stat-card.{slug}", "component", caption.title(), ev("observed", PAGE),
         text=f"{value} {caption}",
         properties={"uno": {"type": "Border", "property": f"x:Uid={uid}"}})
    edge(f"component.stat-card.{slug}", "instance-of", "component.stat-card", ev("observed", PAGE))
    contains("region.stats-grid", f"component.stat-card.{slug}")

# ---------------------------------------------------------------- tokens
TOKENS = [
    ("token.color.neon-amber", "Neon amber (accent)", "color", "#FFAA00", "NeonAmberColor", "Color"),
    ("token.color.powder-blue", "Powder blue", "color", "#B4D7E8", "PowderBlueColor", "Color"),
    ("token.color.arena-dark", "Arena dark (page background)", "color", "#0A0C10", "ArenaDarkColor", "Color"),
    ("token.color.boards-dark", "Boards dark (card surface)", "color", "#12161D", "BoardsDarkColor", "Color"),
    ("token.color.boards-mid", "Boards mid (resting tile)", "color", "#1A1F2A", "BoardsMidColor", "Color"),
    ("token.color.text-primary", "Text primary", "color", "#F0F4F8", "TextPrimaryColor", "Color"),
    ("token.color.text-muted", "Text muted", "color", "#6B7A8F", "TextMutedColor", "Color"),
    ("token.color.border-subtle", "Subtle border", "color", "#0DFFFFFF", "SubtleBorderBrush", "SolidColorBrush"),
    ("token.color.border-card", "Card border", "color", "#08FFFFFF", "CardBorderBrush", "SolidColorBrush"),
    ("token.color.border-powder-blue", "Powder blue border", "color", "#4DB4D7E8", "PowderBlueBorderBrush", "SolidColorBrush"),
    ("token.color.border-amber-semi", "Amber semi border (consumed tile)", "color", "#80FFAA00", "NeonAmberSemiBorderBrush", "SolidColorBrush"),
    ("token.color.border-white-subtle", "Subtle white border (resting tile)", "color", "#1AFFFFFF", "SubtleWhiteBorderBrush", "SolidColorBrush"),
    ("token.gradient.neon-amber", "Neon amber gradient (consumed legend swatch)", "color", "#FFAA00 -> #CC8800", "NeonAmberGradientBrush", "LinearGradientBrush"),
    ("token.radius.small", "Small radius", "radius", "4", "SmallCornerRadius", "CornerRadius"),
    ("token.radius.card", "Card radius", "radius", "12", "CardCornerRadius", "CornerRadius"),
    ("token.radius.large", "Large radius", "radius", "16", "LargeCornerRadius", "CornerRadius"),
    ("token.font.display", "Display face (Bebas Neue)", "typography", "ms-appx:///Assets/Fonts/BebasNeue-Regular.ttf#Bebas Neue", "BebasNeueFont", "FontFamily"),
    ("token.font.body-medium", "Body medium (Barlow Medium)", "typography", "ms-appx:///Assets/Fonts/Barlow-Medium.ttf#Barlow", "BarlowMedium", "FontFamily"),
]
for tid, tname, cat, value, key, rtype in TOKENS:
    node(tid, "token", tname, ev("declared", APP, "Declared in App.xaml application resources."),
         category=cat, value=value,
         properties={"uno": {"resourceKey": key, "resourceType": rtype}})

# ---------------------------------------------------------------- containment
contains("screen.beers", "region.header", SHELL)
contains("screen.beers", "region.beer-summary")
contains("screen.beers", "component.card.season-tracker")
contains("screen.beers", "region.legend")
contains("screen.beers", "region.stats-grid")
contains("screen.beers", "region.tab-bar", SHELL)

contains("region.header", "asset.team-logo", SHELL)
contains("region.header", "content.header.team-name", SHELL)
contains("region.header", "content.header.league-name", SHELL)

for c in ("content.summary.cases-value", "content.summary.cases-label",
          "content.summary.season-caption", "content.summary.total-beers"):
    contains("region.beer-summary", c)

contains("component.card.season-tracker", "content.tracker.title")
contains("component.card.season-tracker", "content.tracker.counter")
contains("component.card.season-tracker", "control.tracker.case-grid")
contains("control.tracker.case-grid", "component.case-tile")

# ---------------------------------------------------------------- behavior
edge("component.case-tile", "has-state", "state.case-tile.consumed",
     ev("declared", CONV, "Per-tile visual state driven by the CaseBlock.IsConsumed flag."))
# Cross-model review: the command is attached to the ItemsRepeater
# (BeersPage.xaml:71-72), not to the Border in its item template, and toggling
# recomputes the count for the tapped block rather than flipping every tile.
edge("control.tracker.case-grid", "triggers", "state.case-tile.consumed",
     ev("declared", VM, "utu:CommandExtensions.Command on the repeater invokes "
                        "ToggleCaseAsync(CaseBlock), which sets ConsumedCases from the "
                        "tapped block's index."))

# ---------------------------------------------------------------- token wiring
uses("screen.beers", "token.color.arena-dark")
uses("component.card", "token.color.boards-dark")
uses("component.card", "token.color.border-card")
uses("component.card.season-tracker", "token.radius.large")
uses("component.card.season-tracker", "token.color.border-subtle")
uses("component.stat-card", "token.radius.card")
uses("component.case-tile", "token.radius.small")
uses("component.case-tile", "token.color.boards-mid")
uses("component.case-tile", "token.color.border-white-subtle")
uses("state.case-tile.consumed", "token.color.neon-amber", CONV)
uses("state.case-tile.consumed", "token.color.border-amber-semi", CONV)
uses("content.summary.cases-value", "token.color.neon-amber")
uses("content.summary.cases-value", "token.font.display")
uses("content.summary.cases-label", "token.color.text-muted")
uses("content.summary.season-caption", "token.color.text-muted")
uses("content.tracker.title", "token.color.text-muted")
uses("content.tracker.counter", "token.color.powder-blue")
uses("component.stat-card", "token.font.display")
uses("component.stat-card", "token.color.neon-amber")
uses("component.stat-card", "token.color.text-muted")
uses("component.legend-swatch.consumed", "token.gradient.neon-amber")
uses("component.legend-swatch.remaining", "token.color.boards-mid")
uses("content.header.team-name", "token.font.display", SHELL)
uses("content.header.team-name", "token.color.text-primary", SHELL)
uses("content.header.league-name", "token.color.text-muted", SHELL)
uses("asset.team-logo", "token.color.border-powder-blue", SHELL)
uses("component.tab-item", "token.font.body-medium", SHELL)
uses("component.tab-item", "token.color.text-muted", SHELL)
uses("region.header", "token.color.border-subtle", SHELL)
uses("region.tab-bar", "token.color.border-subtle", SHELL)

graph = {
    "schemaVersion": "0.1.0",
    "graphId": "eval.pens-beers.gold",
    "name": "Pens - Beers (season beer tracker)",
    "sourceSummary": [
        {"type": "xaml", "label": PAGE},
        {"type": "csharp", "label": VM},
        {"type": "csharp", "label": BLOCK},
        {"type": "xaml", "label": SHELL},
        {"type": "xaml", "label": APP},
        {"type": "csharp", "label": CONV},
    ],
    "nodes": nodes,
    "edges": edges,
    "unresolved": [
        # `unresolved.tab-targets` was removed after the cross-model review:
        # the destinations are declared in Shell.xaml.cs, which the first pass
        # never opened. They are now five `navigates-to` edges. An unresolved
        # item that the source answers is worse than a missing one - it teaches
        # a consumer that the fact is unknowable.
        {
            "id": "unresolved.stat-values",
            "question": "Are the four stat values placeholders?",
            "relatedIds": ["component.stat-card", "component.stat-card.avg-per-game",
                           "component.stat-card.games-played", "component.stat-card.top-consumer",
                           "component.stat-card.most-in-game"],
            "reason": "The hero count and tracker counter are bound to the ViewModel, but all four stat values "
                      "('10', '12', 'B-Rob', '18') are literal Text in XAML with no binding and no ViewModel "
                      "member behind them. Whether that is deliberate or unfinished is not decidable from source.",
        },
        {
            "id": "unresolved.loading-error-state",
            "question": "Where are IsLoading and HasError rendered?",
            "relatedIds": ["screen.beers"],
            "reason": "BeersViewModel declares IsLoading, ErrorMessage and HasError and sets them in "
                      "LoadBeerCountAsync and the toggle rollback path, but BeersPage.xaml binds none of them. "
                      "Nothing in the page binds them, so this screen renders no load or failure feedback at all. Recorded because the ViewModel declares presentation state the screen never shows, which is a defect worth surfacing rather than an ambiguity.",
        },
    ],
}

out = Path(__file__).resolve().parents[1] / "evals" / "08-pens-beers" / "gold.graph.json"
out.write_text(json.dumps(graph, indent=2) + "\n", encoding="utf-8")
print(f"wrote {out}: {len(nodes)} nodes, {len(edges)} edges, {len(graph['unresolved'])} unresolved")
