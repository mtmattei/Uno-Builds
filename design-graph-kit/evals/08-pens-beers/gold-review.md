# Gold review — `08-pens-beers`

**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** `evals\08-pens-beers\gold.graph.json` (56 nodes / 78 edges / 2 unresolved)

> Independent review breaking the same-author circularity: every gold in this
> kit was authored and calibrated by one agent lineage. Record findings here;
> **do not edit gold directly** - accepted fixes go through
> `tools/build_graphs.py`, then `scripts/validate_graph.py`.

## Read in this order

1. `evals/08-pens-beers/fixture.md` — the source list this gold was authored from
2. The actual source files it names (listed below)
3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary
4. `references/ontology.md` — node/edge definitions, state scope and attachment
5. `evals/08-pens-beers/README.md` — the altitude contract for this eval

## Sources cited by this gold

| Source file | Found | Nodes citing it |
|---|---|---|
| `Pens/Pens/App.xaml` | yes | 18 |
| `Pens/Pens/Converters/Converters.cs` | yes | 1 |
| `Pens/Pens/Presentation/BeersPage.xaml` | yes | 22 |
| `Pens/Pens/Presentation/Shell.xaml` | yes | 11 |
| `Pens/Pens/Presentation/Shell.xaml.cs` | yes | 4 |

## Automated pre-pass — identifiers not found in the cited source

Every `properties.uno` value that must be a verbatim quotation from source
(names, style keys, resource keys, classes, members, glyphs) was checked
against the text of every source file this gold cites. The copy-don't-coin
contract says each one should appear literally.

**No fabricated identifiers.** Every quoted uno value exists in the application.

### Real, but not provable from the node's own citation (4)

The value exists in the app, but the node's `evidence.source` does not
point at a file containing it — commonly because the source is given as
a glob label rather than a path. The mapping is right; the provenance
is unverifiable, which is what makes this worth a reviewer's attention.

| Node | uno key | Value | Cited as |
|---|---|---|---|
| `screen.schedule` | `class` | `Pens.Presentation.SchedulePage` | `Pens/Pens/Presentation/Shell.xaml.cs` |
| `screen.chat` | `class` | `Pens.Presentation.ChatPage` | `Pens/Pens/Presentation/Shell.xaml.cs` |
| `screen.duties` | `class` | `Pens.Presentation.DutiesPage` | `Pens/Pens/Presentation/Shell.xaml.cs` |
| `screen.roster` | `class` | `Pens.Presentation.RosterPage` | `Pens/Pens/Presentation/Shell.xaml.cs` |

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
| `token` | 18 |
| `component` | 17 |
| `content` | 8 |
| `screen` | 5 |
| `region` | 5 |
| `asset` | 1 |
| `control` | 1 |
| `state` | 1 |

| Relation | In gold |
|---|---|
| `uses-token` | 30 |
| `contains` | 28 |
| `instance-of` | 13 |
| `navigates-to` | 5 |
| `has-state` | 1 |
| `triggers` | 1 |

The cited XAML declares **24** layout containers (`Grid`/`StackPanel`/`AutoLayout`/…) across 5 file(s).
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
| `screen.beers` | screen | Beers | observed | 1.0 | type=Page, class=Pens.Presentation.BeersPage, xName=PageRoot, styleKey=ArenaDarkBrush | | |
| `region.header` | region | Team header | observed | 1.0 | type=Border, property=AutomationProperties.Name | | |
| `asset.team-logo` | asset | Penguins team logo | observed | 1.0 | type=Image, source=ms-appx:///Assets/images/pens-logo.png | | |
| `content.header.team-name` | content | PENGUINS | observed | 1.0 | type=TextBlock, fontResourceKey=BebasNeueFont | | |
| `content.header.league-name` | content | DORVAL YOUNGTIMERS | observed | 1.0 | type=TextBlock, fontResourceKey=BarlowMedium | | |
| `region.tab-bar` | region | Bottom navigation | observed | 1.0 | type=TabBar, xName=TabBar | | |
| `component.tab-item` | component | Tab item | observed | 1.0 | type=TabBarItem, property=Tag | | |
| `component.tab-item.schedule` | component | Schedule tab | observed | 1.0 | type=TabBarItem, property=Tag=Schedule, iconGlyph=Calendar | | |
| `component.tab-item.chat` | component | Chat tab | observed | 1.0 | type=TabBarItem, property=Tag=Chat, iconGlyph=Message | | |
| `component.tab-item.beers` | component | Beers tab | observed | 1.0 | type=TabBarItem, property=Tag=Beers, iconGlyph=&#xE799; | | |
| `component.tab-item.duties` | component | Duties tab | observed | 1.0 | type=TabBarItem, property=Tag=Duties, iconGlyph=Bullets | | |
| `component.tab-item.roster` | component | Roster tab | observed | 1.0 | type=TabBarItem, property=Tag=Roster, iconGlyph=People | | |
| `screen.schedule` | screen | Schedule | declared | 1.0 | type=Page, class=Pens.Presentation.SchedulePage | | |
| `screen.chat` | screen | Chat | declared | 1.0 | type=Page, class=Pens.Presentation.ChatPage | | |
| `screen.duties` | screen | Duties | declared | 1.0 | type=Page, class=Pens.Presentation.DutiesPage | | |
| `screen.roster` | screen | Roster | declared | 1.0 | type=Page, class=Pens.Presentation.RosterPage | | |
| `region.beer-summary` | region | Season consumption summary | observed | 1.0 | — | | |
| `content.summary.cases-value` | content | Consumed cases value | declared | 1.0 | type=TextBlock, member=ConsumedCases, fontResourceKey=BebasNeueFont | | |
| `content.summary.cases-label` | content | cases | observed | 1.0 | type=TextBlock, property=x:Uid=BeersPage_Cases | | |
| `content.summary.season-caption` | content | CONSUMED THIS SEASON | observed | 1.0 | type=TextBlock, property=x:Uid=BeersPage_ConsumedThisSeason | | |
| `content.summary.total-beers` | content | Total beers line | declared | 1.0 | type=TextBlock, member=TotalBeers | | |
| `component.card` | component | Card | observed | 1.0 | type=Border, styleKey=BoardsDarkBrush | | |
| `component.card.season-tracker` | component | Season tracker card | observed | 1.0 | type=Border, resourceKey=LargeCornerRadius | | |
| `content.tracker.title` | content | SEASON TRACKER | observed | 1.0 | type=TextBlock, property=x:Uid=BeersPage_SeasonTracker | | |
| `content.tracker.counter` | content | Case counter | declared | 1.0 | type=TextBlock, member=ConsumedCases | | |
| `control.tracker.case-grid` | control | Case grid | declared | 1.0 | type=ItemsRepeater, member=CaseBlocks, property=utu:CommandExtensions.Command=ToggleCaseCommand | | |
| `component.case-tile` | component | Case tile | declared | 1.0 | type=Border, resourceKey=SmallCornerRadius, member=IsConsumed | | |
| `state.case-tile.consumed` | state | Consumed | declared | 1.0 | member=IsConsumed, mechanism=IValueConverter on the item template | | |
| `region.legend` | region | Tracker legend | observed | 1.0 | — | | |
| `component.legend-swatch` | component | Legend swatch | observed | 1.0 | type=Border | | |
| `component.legend-swatch.remaining` | component | Remaining swatch | observed | 1.0 | type=Border, property=x:Uid=BeersPage_Remaining | | |
| `component.legend-swatch.consumed` | component | Consumed swatch | observed | 1.0 | type=Border, property=x:Uid=BeersPage_Consumed | | |
| `region.stats-grid` | region | Stats grid | observed | 1.0 | — | | |
| `component.stat-card` | component | Stat card | observed | 1.0 | type=Border, resourceKey=CardCornerRadius, styleKey=CardBorderBrush | | |
| `component.stat-card.avg-per-game` | component | Avg / Game | observed | 1.0 | type=Border, property=x:Uid=BeersPage_AvgPerGame | | |
| `component.stat-card.games-played` | component | Games Played | observed | 1.0 | type=Border, property=x:Uid=BeersPage_GamesPlayed | | |
| `component.stat-card.top-consumer` | component | Top Consumer | observed | 1.0 | type=Border, property=x:Uid=BeersPage_TopConsumer | | |
| `component.stat-card.most-in-game` | component | Most In A Game | observed | 1.0 | type=Border, property=x:Uid=BeersPage_MostInGame | | |
| `token.color.neon-amber` | token | Neon amber (accent) | declared | 1.0 | resourceKey=NeonAmberColor, resourceType=Color | | |
| `token.color.powder-blue` | token | Powder blue | declared | 1.0 | resourceKey=PowderBlueColor, resourceType=Color | | |
| `token.color.arena-dark` | token | Arena dark (page background) | declared | 1.0 | resourceKey=ArenaDarkColor, resourceType=Color | | |
| `token.color.boards-dark` | token | Boards dark (card surface) | declared | 1.0 | resourceKey=BoardsDarkColor, resourceType=Color | | |
| `token.color.boards-mid` | token | Boards mid (resting tile) | declared | 1.0 | resourceKey=BoardsMidColor, resourceType=Color | | |
| `token.color.text-primary` | token | Text primary | declared | 1.0 | resourceKey=TextPrimaryColor, resourceType=Color | | |
| `token.color.text-muted` | token | Text muted | declared | 1.0 | resourceKey=TextMutedColor, resourceType=Color | | |
| `token.color.border-subtle` | token | Subtle border | declared | 1.0 | resourceKey=SubtleBorderBrush, resourceType=SolidColorBrush | | |
| `token.color.border-card` | token | Card border | declared | 1.0 | resourceKey=CardBorderBrush, resourceType=SolidColorBrush | | |
| `token.color.border-powder-blue` | token | Powder blue border | declared | 1.0 | resourceKey=PowderBlueBorderBrush, resourceType=SolidColorBrush | | |
| `token.color.border-amber-semi` | token | Amber semi border (consumed tile) | declared | 1.0 | resourceKey=NeonAmberSemiBorderBrush, resourceType=SolidColorBrush | | |
| `token.color.border-white-subtle` | token | Subtle white border (resting tile) | declared | 1.0 | resourceKey=SubtleWhiteBorderBrush, resourceType=SolidColorBrush | | |
| `token.gradient.neon-amber` | token | Neon amber gradient (consumed legend swatch) | declared | 1.0 | resourceKey=NeonAmberGradientBrush, resourceType=LinearGradientBrush | | |
| `token.radius.small` | token | Small radius | declared | 1.0 | resourceKey=SmallCornerRadius, resourceType=CornerRadius | | |
| `token.radius.card` | token | Card radius | declared | 1.0 | resourceKey=CardCornerRadius, resourceType=CornerRadius | | |
| `token.radius.large` | token | Large radius | declared | 1.0 | resourceKey=LargeCornerRadius, resourceType=CornerRadius | | |
| `token.font.display` | token | Display face (Bebas Neue) | declared | 1.0 | resourceKey=BebasNeueFont, resourceType=FontFamily | | |
| `token.font.body-medium` | token | Body medium (Barlow Medium) | declared | 1.0 | resourceKey=BarlowMedium, resourceType=FontFamily | | |

## Edges

Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are
the ones a graph must never invent. They are listed first.

| Relation | From | To | Evidence | Verdict | Note |
|---|---|---|---|---|---|
| **navigates-to** | `component.tab-item.schedule` | `screen.schedule` | declared | | |
| **navigates-to** | `component.tab-item.chat` | `screen.chat` | declared | | |
| **navigates-to** | `component.tab-item.duties` | `screen.duties` | declared | | |
| **navigates-to** | `component.tab-item.roster` | `screen.roster` | declared | | |
| **navigates-to** | `component.tab-item.beers` | `screen.beers` | declared | | |
| **triggers** | `control.tracker.case-grid` | `state.case-tile.consumed` | declared | | |
| **instance-of** | `component.tab-item.schedule` | `component.tab-item` | observed | | |
| **contains** | `region.tab-bar` | `component.tab-item.schedule` | observed | | |
| **instance-of** | `component.tab-item.chat` | `component.tab-item` | observed | | |
| **contains** | `region.tab-bar` | `component.tab-item.chat` | observed | | |
| **instance-of** | `component.tab-item.beers` | `component.tab-item` | observed | | |
| **contains** | `region.tab-bar` | `component.tab-item.beers` | observed | | |
| **instance-of** | `component.tab-item.duties` | `component.tab-item` | observed | | |
| **contains** | `region.tab-bar` | `component.tab-item.duties` | observed | | |
| **instance-of** | `component.tab-item.roster` | `component.tab-item` | observed | | |
| **contains** | `region.tab-bar` | `component.tab-item.roster` | observed | | |
| **instance-of** | `component.card.season-tracker` | `component.card` | observed | | |
| **instance-of** | `component.legend-swatch.remaining` | `component.legend-swatch` | observed | | |
| **contains** | `region.legend` | `component.legend-swatch.remaining` | observed | | |
| **instance-of** | `component.legend-swatch.consumed` | `component.legend-swatch` | observed | | |
| **contains** | `region.legend` | `component.legend-swatch.consumed` | observed | | |
| **instance-of** | `component.stat-card` | `component.card` | derived | | |
| **instance-of** | `component.stat-card.avg-per-game` | `component.stat-card` | observed | | |
| **contains** | `region.stats-grid` | `component.stat-card.avg-per-game` | observed | | |
| **instance-of** | `component.stat-card.games-played` | `component.stat-card` | observed | | |
| **contains** | `region.stats-grid` | `component.stat-card.games-played` | observed | | |
| **instance-of** | `component.stat-card.top-consumer` | `component.stat-card` | observed | | |
| **contains** | `region.stats-grid` | `component.stat-card.top-consumer` | observed | | |
| **instance-of** | `component.stat-card.most-in-game` | `component.stat-card` | observed | | |
| **contains** | `region.stats-grid` | `component.stat-card.most-in-game` | observed | | |
| **contains** | `screen.beers` | `region.header` | observed | | |
| **contains** | `screen.beers` | `region.beer-summary` | observed | | |
| **contains** | `screen.beers` | `component.card.season-tracker` | observed | | |
| **contains** | `screen.beers` | `region.legend` | observed | | |
| **contains** | `screen.beers` | `region.stats-grid` | observed | | |
| **contains** | `screen.beers` | `region.tab-bar` | observed | | |
| **contains** | `region.header` | `asset.team-logo` | observed | | |
| **contains** | `region.header` | `content.header.team-name` | observed | | |
| **contains** | `region.header` | `content.header.league-name` | observed | | |
| **contains** | `region.beer-summary` | `content.summary.cases-value` | observed | | |
| **contains** | `region.beer-summary` | `content.summary.cases-label` | observed | | |
| **contains** | `region.beer-summary` | `content.summary.season-caption` | observed | | |
| **contains** | `region.beer-summary` | `content.summary.total-beers` | observed | | |
| **contains** | `component.card.season-tracker` | `content.tracker.title` | observed | | |
| **contains** | `component.card.season-tracker` | `content.tracker.counter` | observed | | |
| **contains** | `component.card.season-tracker` | `control.tracker.case-grid` | observed | | |
| **contains** | `control.tracker.case-grid` | `component.case-tile` | observed | | |
| **has-state** | `component.case-tile` | `state.case-tile.consumed` | declared | | |
| **uses-token** | `screen.beers` | `token.color.arena-dark` | declared | | |
| **uses-token** | `component.card` | `token.color.boards-dark` | declared | | |
| **uses-token** | `component.card` | `token.color.border-card` | declared | | |
| **uses-token** | `component.card.season-tracker` | `token.radius.large` | declared | | |
| **uses-token** | `component.card.season-tracker` | `token.color.border-subtle` | declared | | |
| **uses-token** | `component.stat-card` | `token.radius.card` | declared | | |
| **uses-token** | `component.case-tile` | `token.radius.small` | declared | | |
| **uses-token** | `component.case-tile` | `token.color.boards-mid` | declared | | |
| **uses-token** | `component.case-tile` | `token.color.border-white-subtle` | declared | | |
| **uses-token** | `state.case-tile.consumed` | `token.color.neon-amber` | declared | | |
| **uses-token** | `state.case-tile.consumed` | `token.color.border-amber-semi` | declared | | |
| **uses-token** | `content.summary.cases-value` | `token.color.neon-amber` | declared | | |
| **uses-token** | `content.summary.cases-value` | `token.font.display` | declared | | |
| **uses-token** | `content.summary.cases-label` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.summary.season-caption` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.tracker.title` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.tracker.counter` | `token.color.powder-blue` | declared | | |
| **uses-token** | `component.stat-card` | `token.font.display` | declared | | |
| **uses-token** | `component.stat-card` | `token.color.neon-amber` | declared | | |
| **uses-token** | `component.stat-card` | `token.color.text-muted` | declared | | |
| **uses-token** | `component.legend-swatch.consumed` | `token.gradient.neon-amber` | declared | | |
| **uses-token** | `component.legend-swatch.remaining` | `token.color.boards-mid` | declared | | |
| **uses-token** | `content.header.team-name` | `token.font.display` | declared | | |
| **uses-token** | `content.header.team-name` | `token.color.text-primary` | declared | | |
| **uses-token** | `content.header.league-name` | `token.color.text-muted` | declared | | |
| **uses-token** | `asset.team-logo` | `token.color.border-powder-blue` | declared | | |
| **uses-token** | `component.tab-item` | `token.font.body-medium` | declared | | |
| **uses-token** | `component.tab-item` | `token.color.text-muted` | declared | | |
| **uses-token** | `region.header` | `token.color.border-subtle` | declared | | |
| **uses-token** | `region.tab-bar` | `token.color.border-subtle` | declared | | |

## Unresolved items

| Question | Related ids | Genuinely undecidable? | Note |
|---|---|---|---|
| Are the four stat values placeholders? | `component.stat-card`, `component.stat-card.avg-per-game`, `component.stat-card.games-played`, `component.stat-card.top-consumer`, `component.stat-card.most-in-game` | | |
| Where are IsLoading and HasError rendered? | `screen.beers` | | |

## Verdict

_Overall assessment, and whether the gold is fit to remain the answer key._

Findings accepted: _(list)_
Findings rejected: _(list, with reasons)_

