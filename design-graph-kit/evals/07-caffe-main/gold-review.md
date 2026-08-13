# Gold review — `07-caffe-main`

**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** `evals\07-caffe-main\gold.graph.json` (52 nodes / 73 edges / 1 unresolved)

> Independent review breaking the same-author circularity: every gold in this
> kit was authored and calibrated by one agent lineage. Record findings here;
> **do not edit gold directly** - accepted fixes go through
> `tools/build_graphs.py`, then `scripts/validate_graph.py`.

## Read in this order

1. `evals/07-caffe-main/fixture.md` — the source list this gold was authored from
2. The actual source files it names (listed below)
3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary
4. `references/ontology.md` — node/edge definitions, state scope and attachment
5. `evals/07-caffe-main/README.md` — the altitude contract for this eval

## Sources cited by this gold

| Source file | Found | Nodes citing it |
|---|---|---|
| `Caffe/Caffe/Controls/` | yes | 7 |
| `Caffe/Caffe/MainPage.xaml` | yes | 11 |
| `Caffe/Caffe/MainPage.xaml.cs` | yes | 1 |
| `Caffe/Caffe/Styles/AppResources.xaml` | yes | 28 |
| `Caffe/Caffe/ViewModels/MainViewModel.cs` | yes | 5 |

## Automated pre-pass — identifiers not found in the cited source

Every `properties.uno` value that must be a verbatim quotation from source
(names, style keys, resource keys, classes, members, glyphs) was checked
against the text of every source file this gold cites. The copy-don't-coin
contract says each one should appear literally.

**No fabricated identifiers.** Every quoted uno value exists in the application.

### Token values that disagree with the style they name

A key existing in source does not make the value attributed to it right.
This compares each token's asserted value against the `Setter`s of the
style it names. The check exists because a cross-model reviewer found a
token claiming `OrbitalPageTitle` is 28px when that style declares 20 —
the 28 belonged to a different style, and every existence check passed it.

**7 mismatch(es).**

| Node | Style key | Field | Gold claims | Source declares |
|---|---|---|---|---|
| `token.typography.logo` | `LogoTextStyle` | family | **Cormorant Light** | **ms-appx:///Assets/Fonts/CormorantGaramond-Light.ttf#Cormorant Garamond** |
| `token.typography.tagline` | `TaglineTextStyle` | family | **DM Sans Medium** | **ms-appx:///Assets/Fonts/DMSans-Medium.ttf#DM Sans** |
| `token.typography.card-title` | `CardTitleTextStyle` | family | **Cormorant** | **ms-appx:///Assets/Fonts/CormorantGaramond-Regular.ttf#Cormorant Garamond** |
| `token.typography.volume-badge` | `VolumeBadgeTextStyle` | family | **DM Sans Medium** | **ms-appx:///Assets/Fonts/DMSans-Medium.ttf#DM Sans** |
| `token.typography.parameter-value` | `ParameterValueTextStyle` | family | **Cormorant** | **ms-appx:///Assets/Fonts/CormorantGaramond-Regular.ttf#Cormorant Garamond** |
| `token.typography.parameter-label` | `ParameterLabelTextStyle` | family | **DM Sans Medium** | **ms-appx:///Assets/Fonts/DMSans-Medium.ttf#DM Sans** |
| `token.typography.overview-value` | `OverviewValueTextStyle` | family | **DM Sans** | **ms-appx:///Assets/Fonts/CormorantGaramond-Regular.ttf#Cormorant Garamond** |

## Structural facts

Stated as counts, not judgements — the altitude contract decides which of
these are correct, and that call belongs to the reviewer.

| Node type | In gold |
|---|---|
| `token` | 29 |
| `component` | 13 |
| `state` | 4 |
| `content` | 2 |
| `region` | 2 |
| `screen` | 1 |
| `asset` | 1 |
| `control` | **0** |

| Relation | In gold |
|---|---|
| `uses-token` | 44 |
| `contains` | 17 |
| `has-state` | 4 |
| `triggers` | 4 |
| `instance-of` | 4 |

The cited XAML declares **34** layout containers (`Grid`/`StackPanel`/`AutoLayout`/…) across 5 file(s).
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
| `screen.caffe-main` | screen | Caffe main (brew) | observed | 1.0 | type=Page, class=Caffe.MainPage, viewModel=Caffe.ViewModels.MainViewModel (CommunityToolkit.Mvvm) | | |
| `component.caffe-header` | component | Caffe header | declared | 1.0 | type=UserControl, class=Caffe.Controls.CaffeHeader | | |
| `asset.header.accent-bar` | asset | Accent bar | observed | 1.0 | — | | |
| `content.header.logo` | content | Logo | observed | 1.0 | type=TextBlock, styleKey=LogoTextStyle | | |
| `content.header.tagline` | content | Tagline | observed | 1.0 | type=TextBlock, styleKey=TaglineTextStyle | | |
| `component.caffe-footer` | component | Caffe footer | declared | 1.0 | type=UserControl, class=Caffe.Controls.CaffeFooter | | |
| `region.caffe-main.menu` | region | Espresso menu | observed | 1.0 | — | | |
| `component.espresso-card` | component | Espresso card | declared | 1.0 | type=UserControl, class=Caffe.Controls.EspressoCard | | |
| `component.espresso-card.espresso` | component | Espresso | declared | 1.0 | xName=EspressoCard | | |
| `component.espresso-card.doppio` | component | Doppio | declared | 1.0 | xName=DoppioCard | | |
| `component.espresso-card.ristretto` | component | Ristretto | declared | 1.0 | xName=RistrettoCard | | |
| `component.espresso-card.lungo` | component | Lungo | declared | 1.0 | xName=LungoCard | | |
| `region.caffe-main.parameters` | region | Brew parameters | observed | 1.0 | — | | |
| `component.temperature-gauge` | component | Temperature | declared | 1.0 | type=UserControl, class=Caffe.Controls.TemperatureGauge, xName=TempGauge | | |
| `component.extraction-arc` | component | Extraction time | declared | 1.0 | type=UserControl, class=Caffe.Controls.ExtractionArc, xName=ExtractionArc | | |
| `component.grind-selector` | component | Grind level | declared | 1.0 | type=UserControl, class=Caffe.Controls.GrindSelector, xName=GrindSelector | | |
| `component.selection-overview` | component | Selection overview | declared | 1.0 | type=UserControl, class=Caffe.Controls.SelectionOverview, xName=SelectionOverview | | |
| `component.brew-button` | component | Brew | declared | 1.0 | type=UserControl, class=Caffe.Controls.BrewButton, xName=BrewBtn | | |
| `state.espresso-card.selected` | state | Card selected | declared | 1.0 | mechanism=binding, member=HasSelection / IsSelected | | |
| `state.brew-button.disabled` | state | Brew disabled | declared | 1.0 | mechanism=binding, member=HasSelection (RelayCommand CanExecute) | | |
| `state.selection-overview.hidden` | state | Overview hidden | declared | 1.0 | mechanism=binding, member=HasSelection | | |
| `state.caffe-main.brewing` | state | Brewing | declared | 1.0 | mechanism=binding, member=IsBrewing | | |
| `component.brewing-screen` | component | Brewing overlay | declared | 1.0 | type=UserControl, class=Caffe.Controls.BrewingScreen, xName=BrewingOverlay | | |
| `token.color.coffee-dark` | token | Coffee dark (brew gradient) | declared | 1.0 | resourceKey=CoffeeDarkColor, resourceType=Color | | |
| `token.color.coffee-light` | token | Coffee light (brew gradient) | declared | 1.0 | resourceKey=CoffeeLightColor, resourceType=Color | | |
| `token.color.temperature-high` | token | Temperature high | declared | 1.0 | resourceKey=CaffeTemperatureHighColor, resourceType=Color | | |
| `token.color.temperature-low` | token | Temperature low | declared | 1.0 | resourceKey=CaffeTemperatureLowColor, resourceType=Color | | |
| `token.color.background` | token | Background | declared | 1.0 | resourceKey=CaffeBackgroundBrush, resourceType=SolidColorBrush | | |
| `token.color.surface` | token | Surface | declared | 1.0 | resourceKey=CaffeSurfaceBrush, resourceType=SolidColorBrush | | |
| `token.color.primary` | token | Primary (espresso green) | declared | 1.0 | resourceKey=CaffePrimaryBrush, resourceType=SolidColorBrush | | |
| `token.color.accent-red` | token | Accent red | declared | 1.0 | resourceKey=CaffeAccentRedBrush, resourceType=SolidColorBrush | | |
| `token.color.accent-green` | token | Accent green | declared | 1.0 | resourceKey=CaffeAccentGreenBrush, resourceType=SolidColorBrush | | |
| `token.color.text-primary` | token | Text primary | declared | 1.0 | resourceKey=CaffeTextPrimaryBrush, resourceType=SolidColorBrush | | |
| `token.color.text-secondary` | token | Text secondary | declared | 1.0 | resourceKey=CaffeTextSecondaryBrush, resourceType=SolidColorBrush | | |
| `token.color.text-muted` | token | Text muted | declared | 1.0 | resourceKey=CaffeTextMutedBrush, resourceType=SolidColorBrush | | |
| `token.color.border` | token | Border | declared | 1.0 | resourceKey=CaffeBorderBrush, resourceType=SolidColorBrush | | |
| `token.color.on-primary` | token | On primary | declared | 1.0 | resourceKey=CaffeOnPrimaryBrush, resourceType=SolidColorBrush | | |
| `token.typography.logo` | token | Logo type | declared | 1.0 | styleKey=LogoTextStyle | | |
| `token.typography.tagline` | token | Tagline type | declared | 1.0 | styleKey=TaglineTextStyle | | |
| `token.typography.card-title` | token | Card title type | declared | 1.0 | styleKey=CardTitleTextStyle | | |
| `token.typography.card-description` | token | Card description type | declared | 1.0 | styleKey=CardDescriptionTextStyle | | |
| `token.typography.volume-badge` | token | Volume badge type | declared | 1.0 | styleKey=VolumeBadgeTextStyle | | |
| `token.typography.parameter-value` | token | Parameter value type | declared | 1.0 | styleKey=ParameterValueTextStyle | | |
| `token.typography.parameter-label` | token | Parameter label type | declared | 1.0 | styleKey=ParameterLabelTextStyle | | |
| `token.typography.button` | token | Button type | declared | 1.0 | styleKey=ButtonTextStyle | | |
| `token.typography.grind-hint` | token | Grind hint type | declared | 1.0 | styleKey=GrindHintTextStyle | | |
| `token.typography.overview-label` | token | Overview label type | declared | 1.0 | styleKey=OverviewLabelTextStyle | | |
| `token.typography.overview-value` | token | Overview value type | declared | 1.0 | styleKey=OverviewValueTextStyle | | |
| `token.typography.brewing-title` | token | Brewing title type | declared | 1.0 | styleKey=BrewingTitleTextStyle | | |
| `token.typography.body` | token | Body type | declared | 1.0 | styleKey=BodyTextStyle | | |
| `token.typography.arc-label` | token | Arc label type | declared | 1.0 | styleKey=ArcLabelTextStyle | | |
| `token.radius.14` | token | 14 radius | derived | 1.0 | — | | |

## Edges

Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are
the ones a graph must never invent. They are listed first.

| Relation | From | To | Evidence | Verdict | Note |
|---|---|---|---|---|---|
| **triggers** | `component.brew-button` | `state.caffe-main.brewing` | declared | | |
| **triggers** | `component.espresso-card` | `state.espresso-card.selected` | declared | | |
| **triggers** | `component.espresso-card` | `state.selection-overview.hidden` | declared | | |
| **triggers** | `component.espresso-card` | `state.brew-button.disabled` | declared | | |
| **contains** | `screen.caffe-main` | `component.caffe-header` | observed | | |
| **contains** | `component.caffe-header` | `asset.header.accent-bar` | observed | | |
| **contains** | `component.caffe-header` | `content.header.logo` | observed | | |
| **contains** | `component.caffe-header` | `content.header.tagline` | observed | | |
| **contains** | `screen.caffe-main` | `region.caffe-main.menu` | observed | | |
| **contains** | `screen.caffe-main` | `region.caffe-main.parameters` | observed | | |
| **contains** | `screen.caffe-main` | `component.brew-button` | observed | | |
| **contains** | `screen.caffe-main` | `component.caffe-footer` | observed | | |
| **contains** | `region.caffe-main.parameters` | `component.temperature-gauge` | observed | | |
| **contains** | `region.caffe-main.parameters` | `component.extraction-arc` | observed | | |
| **contains** | `region.caffe-main.parameters` | `component.grind-selector` | observed | | |
| **has-state** | `screen.caffe-main` | `state.caffe-main.brewing` | declared | | |
| **contains** | `screen.caffe-main` | `component.selection-overview` | observed | | |
| **has-state** | `component.espresso-card` | `state.espresso-card.selected` | declared | | |
| **has-state** | `component.brew-button` | `state.brew-button.disabled` | declared | | |
| **has-state** | `component.selection-overview` | `state.selection-overview.hidden` | declared | | |
| **contains** | `state.caffe-main.brewing` | `component.brewing-screen` | declared | | |
| **contains** | `region.caffe-main.menu` | `component.espresso-card.espresso` | observed | | |
| **instance-of** | `component.espresso-card.espresso` | `component.espresso-card` | derived | | |
| **contains** | `region.caffe-main.menu` | `component.espresso-card.doppio` | observed | | |
| **instance-of** | `component.espresso-card.doppio` | `component.espresso-card` | derived | | |
| **contains** | `region.caffe-main.menu` | `component.espresso-card.ristretto` | observed | | |
| **instance-of** | `component.espresso-card.ristretto` | `component.espresso-card` | derived | | |
| **contains** | `region.caffe-main.menu` | `component.espresso-card.lungo` | observed | | |
| **instance-of** | `component.espresso-card.lungo` | `component.espresso-card` | derived | | |
| **uses-token** | `screen.caffe-main` | `token.color.background` | declared | | |
| **uses-token** | `content.header.logo` | `token.typography.logo` | declared | | |
| **uses-token** | `content.header.tagline` | `token.typography.tagline` | declared | | |
| **uses-token** | `asset.header.accent-bar` | `token.color.primary` | declared | | |
| **uses-token** | `asset.header.accent-bar` | `token.color.accent-red` | declared | | |
| **uses-token** | `component.caffe-footer` | `token.color.primary` | declared | | |
| **uses-token** | `component.caffe-footer` | `token.color.accent-red` | declared | | |
| **uses-token** | `component.espresso-card` | `token.color.surface` | declared | | |
| **uses-token** | `component.espresso-card` | `token.color.border` | declared | | |
| **uses-token** | `component.espresso-card` | `token.radius.14` | declared | | |
| **uses-token** | `component.espresso-card` | `token.color.primary` | declared | | |
| **uses-token** | `component.espresso-card` | `token.typography.card-title` | declared | | |
| **uses-token** | `component.espresso-card` | `token.typography.card-description` | declared | | |
| **uses-token** | `component.espresso-card` | `token.typography.volume-badge` | declared | | |
| **uses-token** | `component.espresso-card` | `token.color.on-primary` | declared | | |
| **uses-token** | `component.temperature-gauge` | `token.color.surface` | declared | | |
| **uses-token** | `component.temperature-gauge` | `token.typography.parameter-value` | declared | | |
| **uses-token** | `component.temperature-gauge` | `token.typography.parameter-label` | declared | | |
| **uses-token** | `component.extraction-arc` | `token.color.surface` | declared | | |
| **uses-token** | `component.extraction-arc` | `token.typography.parameter-value` | declared | | |
| **uses-token** | `component.extraction-arc` | `token.typography.parameter-label` | declared | | |
| **uses-token** | `component.extraction-arc` | `token.color.accent-green` | declared | | |
| **uses-token** | `component.grind-selector` | `token.color.surface` | declared | | |
| **uses-token** | `component.grind-selector` | `token.typography.parameter-value` | declared | | |
| **uses-token** | `component.grind-selector` | `token.typography.parameter-label` | declared | | |
| **uses-token** | `component.selection-overview` | `token.color.primary` | declared | | |
| **uses-token** | `component.selection-overview` | `token.radius.14` | declared | | |
| **uses-token** | `component.brew-button` | `token.color.primary` | declared | | |
| **uses-token** | `component.brew-button` | `token.radius.14` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.color.background` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.color.text-primary` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.color.text-secondary` | declared | | |
| **uses-token** | `component.espresso-card` | `token.color.text-muted` | declared | | |
| **uses-token** | `component.brew-button` | `token.typography.button` | declared | | |
| **uses-token** | `component.grind-selector` | `token.typography.grind-hint` | declared | | |
| **uses-token** | `component.selection-overview` | `token.typography.overview-label` | declared | | |
| **uses-token** | `component.selection-overview` | `token.typography.overview-value` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.typography.brewing-title` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.typography.body` | declared | | |
| **uses-token** | `component.extraction-arc` | `token.typography.arc-label` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.color.coffee-dark` | declared | | |
| **uses-token** | `component.brewing-screen` | `token.color.coffee-light` | declared | | |
| **uses-token** | `component.temperature-gauge` | `token.color.temperature-high` | declared | | |
| **uses-token** | `component.temperature-gauge` | `token.color.temperature-low` | declared | | |

## Unresolved items

| Question | Related ids | Genuinely undecidable? | Note |
|---|---|---|---|
| Is the four-espresso menu fixed, or intended to become data-driven? | `region.caffe-main.menu` | | |

## Verdict

_Overall assessment, and whether the gold is fit to remain the answer key._

Findings accepted: _(list)_
Findings rejected: _(list, with reasons)_

