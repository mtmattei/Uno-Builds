# Gold review — `06-flux-profile`

**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** `evals\06-flux-profile\gold.graph.json` (61 nodes / 91 edges / 5 unresolved)

> Independent review breaking the same-author circularity: every gold in this
> kit was authored and calibrated by one agent lineage. Record findings here;
> **do not edit gold directly** - accepted fixes go through
> `tools/build_graphs.py`, then `scripts/validate_graph.py`.

## Read in this order

1. `evals/06-flux-profile/fixture.md` — the source list this gold was authored from
2. The actual source files it names (listed below)
3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary
4. `references/ontology.md` — node/edge definitions, state scope and attachment
5. `evals/06-flux-profile/README.md` — the altitude contract for this eval

## Sources cited by this gold

| Source file | Found | Nodes citing it |
|---|---|---|
| `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfileModel.cs` | yes | 1 |
| `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml` | yes | 37 |
| `FluxTransit/FluxTransit/FluxTransit/Styles/FluxStyles.xaml` | yes | 23 |

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

**No mismatches.** Every token value matches the style it cites.

## Structural facts

Stated as counts, not judgements — the altitude contract decides which of
these are correct, and that call belongs to the reviewer.

| Node type | In gold |
|---|---|
| `token` | 23 |
| `content` | 16 |
| `component` | 8 |
| `control` | 7 |
| `region` | 5 |
| `screen` | 1 |
| `state` | 1 |
| `asset` | **0** |

| Relation | In gold |
|---|---|
| `uses-token` | 49 |
| `contains` | 34 |
| `instance-of` | 5 |
| `triggers` | 2 |
| `has-state` | 1 |

The cited XAML declares **21** layout containers (`Grid`/`StackPanel`/`AutoLayout`/…) across 3 file(s).
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
| `screen.profile` | screen | Profile | observed | 1.0 | type=Page, class=FluxTransit.Presentation.ProfilePage | | |
| `region.profile.scroll-content` | region | Scrolling content | observed | 1.0 | type=ScrollViewer | | |
| `region.opus.summary` | region | OPUS summary | observed | 1.0 | — | | |
| `region.opus.refresh-action` | region | Refresh action | observed | 1.0 | — | | |
| `region.profile.header` | region | Header | observed | 1.0 | — | | |
| `control.header.back` | control | Back | declared | 1.0 | type=Button, styleKey=FluxIconButtonStyle, iconGlyph=E72B | | |
| `content.header.title` | content | Title | observed | 1.0 | type=TextBlock, styleKey=FluxHeadingLarge | | |
| `content.header.subtitle` | content | Subtitle | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `component.card` | component | Section card (glass) | derived | 1.0 | type=Border, styleKey=FluxGlassPanelStyle | | |
| `component.card.opus` | component | OPUS card section | observed | 1.0 | — | | |
| `component.card.routes` | component | Saved routes section | observed | 1.0 | — | | |
| `component.card.settings` | component | Settings section | observed | 1.0 | — | | |
| `content.opus.section-title` | content | Opus header | observed | 1.0 | type=TextBlock, styleKey=FluxMicro | | |
| `component.opus-card` | component | OPUS card visual | observed | 1.0 | type=Border | | |
| `content.opus.balance-label` | content | Balance label | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `content.opus.balance-value` | content | Balance value | declared | 1.0 | type=TextBlock, styleKey=FluxHeadingLarge | | |
| `content.opus.refresh-hint` | content | Refresh hint | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `control.opus.update` | control | Update Balance | declared | 1.0 | type=Button, styleKey=FluxPrimaryButtonStyle | | |
| `state.opus.refreshing` | state | Refreshing | declared | 1.0 | mechanism=binding, member=IsRefreshing | | |
| `control.opus.progress` | control | Refresh progress | observed | 1.0 | type=ProgressRing | | |
| `content.opus.updating-label` | content | Updating label | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `content.routes.section-title` | content | Routes header | observed | 1.0 | type=TextBlock, styleKey=FluxMicro | | |
| `component.route-item` | component | Saved route row | derived | 1.0 | type=Border, styleKey=FluxTransitCardStyle | | |
| `component.route-item.home-work` | component | Home → Work | observed | 1.0 | — | | |
| `component.route-item.downtown-loop` | component | Downtown Loop | observed | 1.0 | — | | |
| `control.routes.add` | control | Add New Route | observed | 1.0 | type=Button, iconGlyph=E710 | | |
| `content.settings.section-title` | content | Settings header | observed | 1.0 | type=TextBlock, styleKey=FluxMicro | | |
| `content.settings.api-label` | content | API key label | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `control.settings.api-key` | control | Gemini API key | declared | 1.0 | type=TextBox | | |
| `content.settings.api-helper` | content | API key helper | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `content.settings.language-label` | content | Language label | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `control.settings.language` | control | Language | observed | 1.0 | type=utu:ChipGroup, styleKey=FilterChipGroupStyle | | |
| `content.settings.alerts-label` | content | Alerts label | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `content.settings.alerts-helper` | content | Alerts helper | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `control.settings.alerts` | control | Service alerts | observed | 1.0 | type=ToggleSwitch | | |
| `region.profile.footer` | region | App info footer | observed | 1.0 | — | | |
| `content.footer.version` | content | App version | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `content.footer.credit` | content | Credit | observed | 1.0 | type=TextBlock, styleKey=FluxBody | | |
| `token.color.background` | token | Background | declared | 1.0 | resourceKey=FluxBackgroundBrush, resourceType=SolidColorBrush | | |
| `token.color.surface` | token | Surface | declared | 1.0 | resourceKey=FluxSurfaceBrush, resourceType=SolidColorBrush | | |
| `token.color.glass-panel` | token | Glass panel | declared | 1.0 | resourceKey=FluxGlassPanelBrush, resourceType=SolidColorBrush | | |
| `token.color.border-subtle` | token | Border subtle | declared | 1.0 | resourceKey=FluxBorderSubtleBrush, resourceType=SolidColorBrush | | |
| `token.color.border-light` | token | Border light | declared | 1.0 | resourceKey=FluxBorderLightBrush, resourceType=SolidColorBrush | | |
| `token.color.primary` | token | Primary (indigo 400) | declared | 1.0 | resourceKey=FluxPrimaryBrush, resourceType=SolidColorBrush | | |
| `token.color.primary-strong` | token | Primary strong (indigo 600) | declared | 1.0 | resourceKey=FluxPrimaryStrongBrush, resourceType=SolidColorBrush | | |
| `token.color.success` | token | Success (emerald) | declared | 1.0 | resourceKey=FluxSuccessBrush, resourceType=SolidColorBrush | | |
| `token.color.text-primary` | token | Text primary | declared | 1.0 | resourceKey=FluxTextPrimaryBrush, resourceType=SolidColorBrush | | |
| `token.color.text-secondary` | token | Text secondary | declared | 1.0 | resourceKey=FluxTextSecondaryBrush, resourceType=SolidColorBrush | | |
| `token.color.text-muted` | token | Text muted | declared | 1.0 | resourceKey=FluxTextMutedBrush, resourceType=SolidColorBrush | | |
| `token.radius.24` | token | 24 radius | declared | 1.0 | resourceKey=FluxCornerRadiusLarge, resourceType=CornerRadius | | |
| `token.radius.16` | token | 16 radius | declared | 1.0 | resourceKey=FluxCornerRadiusMedium, resourceType=CornerRadius | | |
| `token.radius.full` | token | Full (pill) radius | declared | 1.0 | resourceKey=FluxCornerRadiusFull, resourceType=CornerRadius | | |
| `token.spacing.24` | token | 24 spacing | declared | 1.0 | resourceKey=FluxSpacingL | | |
| `token.spacing.16` | token | 16 spacing | declared | 1.0 | resourceKey=FluxSpacingM | | |
| `token.spacing.8` | token | 8 spacing | declared | 1.0 | resourceKey=FluxSpacingS | | |
| `token.spacing.4` | token | 4 spacing | declared | 1.0 | resourceKey=FluxSpacingXS | | |
| `token.typography.heading-large` | token | Heading large | declared | 1.0 | styleKey=FluxHeadingLarge | | |
| `token.typography.body` | token | Body | declared | 1.0 | styleKey=FluxBody | | |
| `token.typography.helper` | token | Helper caption | derived | 1.0 | styleKey=FluxBody, property=FontSize=12 | | |
| `token.typography.body-bold` | token | Body bold | declared | 1.0 | styleKey=FluxBodyBold | | |
| `token.typography.micro` | token | Micro header | declared | 1.0 | styleKey=FluxMicro | | |

## Edges

Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are
the ones a graph must never invent. They are listed first.

| Relation | From | To | Evidence | Verdict | Note |
|---|---|---|---|---|---|
| **triggers** | `control.opus.update` | `state.opus.refreshing` | declared | | |
| **triggers** | `control.opus.update` | `content.opus.balance-value` | declared | | |
| **contains** | `screen.profile` | `region.profile.scroll-content` | observed | | |
| **contains** | `region.profile.scroll-content` | `region.profile.header` | observed | | |
| **contains** | `region.profile.header` | `control.header.back` | observed | | |
| **contains** | `region.profile.header` | `content.header.title` | observed | | |
| **contains** | `region.profile.header` | `content.header.subtitle` | observed | | |
| **contains** | `region.profile.scroll-content` | `component.card.opus` | observed | | |
| **contains** | `region.profile.scroll-content` | `component.card.routes` | observed | | |
| **contains** | `region.profile.scroll-content` | `component.card.settings` | observed | | |
| **contains** | `region.profile.scroll-content` | `region.profile.footer` | observed | | |
| **contains** | `region.profile.footer` | `content.footer.version` | observed | | |
| **contains** | `region.profile.footer` | `content.footer.credit` | observed | | |
| **instance-of** | `component.card.opus` | `component.card` | declared | | |
| **instance-of** | `component.card.routes` | `component.card` | declared | | |
| **instance-of** | `component.card.settings` | `component.card` | declared | | |
| **contains** | `component.card.opus` | `content.opus.section-title` | observed | | |
| **contains** | `component.card.opus` | `region.opus.summary` | observed | | |
| **contains** | `component.card.opus` | `region.opus.refresh-action` | observed | | |
| **contains** | `region.opus.summary` | `component.opus-card` | observed | | |
| **contains** | `region.opus.summary` | `content.opus.balance-label` | observed | | |
| **contains** | `region.opus.summary` | `content.opus.balance-value` | observed | | |
| **contains** | `region.opus.summary` | `content.opus.refresh-hint` | observed | | |
| **contains** | `region.opus.refresh-action` | `control.opus.update` | observed | | |
| **contains** | `component.card.routes` | `content.routes.section-title` | observed | | |
| **contains** | `component.card.routes` | `component.route-item.home-work` | observed | | |
| **contains** | `component.card.routes` | `component.route-item.downtown-loop` | observed | | |
| **contains** | `component.card.routes` | `control.routes.add` | observed | | |
| **instance-of** | `component.route-item.home-work` | `component.route-item` | derived | | |
| **instance-of** | `component.route-item.downtown-loop` | `component.route-item` | derived | | |
| **contains** | `component.card.settings` | `content.settings.section-title` | observed | | |
| **contains** | `component.card.settings` | `content.settings.api-label` | observed | | |
| **contains** | `component.card.settings` | `control.settings.api-key` | observed | | |
| **contains** | `component.card.settings` | `content.settings.api-helper` | observed | | |
| **contains** | `component.card.settings` | `content.settings.language-label` | observed | | |
| **contains** | `component.card.settings` | `control.settings.language` | observed | | |
| **contains** | `component.card.settings` | `content.settings.alerts-label` | observed | | |
| **contains** | `component.card.settings` | `content.settings.alerts-helper` | observed | | |
| **contains** | `component.card.settings` | `control.settings.alerts` | observed | | |
| **has-state** | `region.opus.refresh-action` | `state.opus.refreshing` | declared | | |
| **contains** | `state.opus.refreshing` | `control.opus.progress` | observed | | |
| **contains** | `state.opus.refreshing` | `content.opus.updating-label` | observed | | |
| **uses-token** | `screen.profile` | `token.color.background` | declared | | |
| **uses-token** | `screen.profile` | `token.spacing.24` | derived | | |
| **uses-token** | `component.card` | `token.color.glass-panel` | declared | | |
| **uses-token** | `component.card` | `token.color.border-subtle` | declared | | |
| **uses-token** | `component.card` | `token.radius.24` | declared | | |
| **uses-token** | `component.card` | `token.spacing.24` | declared | | |
| **uses-token** | `component.card` | `token.spacing.16` | declared | | |
| **uses-token** | `content.header.title` | `token.typography.heading-large` | declared | | |
| **uses-token** | `content.header.subtitle` | `token.typography.body` | declared | | |
| **uses-token** | `content.opus.section-title` | `token.typography.micro` | declared | | |
| **uses-token** | `content.routes.section-title` | `token.typography.micro` | declared | | |
| **uses-token** | `content.settings.section-title` | `token.typography.micro` | declared | | |
| **uses-token** | `component.opus-card` | `token.color.primary-strong` | declared | | |
| **uses-token** | `content.opus.balance-label` | `token.typography.body` | declared | | |
| **uses-token** | `content.opus.balance-value` | `token.typography.heading-large` | declared | | |
| **uses-token** | `content.opus.balance-value` | `token.color.success` | declared | | |
| **uses-token** | `content.opus.refresh-hint` | `token.typography.body` | declared | | |
| **uses-token** | `control.opus.update` | `token.color.primary` | declared | | |
| **uses-token** | `control.opus.update` | `token.radius.full` | declared | | |
| **uses-token** | `component.route-item` | `token.radius.16` | declared | | |
| **uses-token** | `component.route-item` | `token.color.border-subtle` | declared | | |
| **uses-token** | `component.route-item` | `token.color.primary` | declared | | |
| **uses-token** | `component.route-item` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.header.title` | `token.color.text-primary` | declared | | |
| **uses-token** | `content.header.subtitle` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.opus.balance-label` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.opus.refresh-hint` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.opus.updating-label` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.opus.section-title` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.routes.section-title` | `token.color.text-muted` | declared | | |
| **uses-token** | `content.settings.section-title` | `token.color.text-muted` | declared | | |
| **uses-token** | `component.route-item` | `token.color.text-primary` | declared | | |
| **uses-token** | `content.settings.api-label` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.settings.api-helper` | `token.typography.helper` | declared | | |
| **uses-token** | `content.settings.alerts-helper` | `token.typography.helper` | declared | | |
| **uses-token** | `content.settings.api-helper` | `token.color.text-secondary` | declared | | |
| **uses-token** | `content.settings.alerts-helper` | `token.color.text-secondary` | declared | | |
| **uses-token** | `component.route-item` | `token.typography.body-bold` | declared | | |
| **uses-token** | `component.route-item` | `token.typography.body` | declared | | |
| **uses-token** | `component.route-item` | `token.spacing.4` | derived | | |
| **uses-token** | `control.routes.add` | `token.color.border-light` | declared | | |
| **uses-token** | `control.routes.add` | `token.color.text-primary` | declared | | |
| **uses-token** | `content.settings.api-label` | `token.typography.body` | declared | | |
| **uses-token** | `control.settings.api-key` | `token.color.surface` | declared | | |
| **uses-token** | `control.settings.api-key` | `token.color.border-light` | declared | | |
| **uses-token** | `content.settings.language-label` | `token.typography.body` | declared | | |
| **uses-token** | `content.settings.alerts-label` | `token.typography.body` | declared | | |
| **uses-token** | `content.footer.version` | `token.typography.body` | declared | | |
| **uses-token** | `content.footer.credit` | `token.typography.body` | declared | | |

## Unresolved items

| Question | Related ids | Genuinely undecidable? | Note |
|---|---|---|---|
| What happens when a saved route row is tapped? | `component.route-item`, `component.route-item.home-work`, `component.route-item.downtown-loop` | | |
| Where does Back navigate? | `control.header.back` | | |
| What does Add New Route do? | `control.routes.add` | | |
| What invokes the declared SaveSettings command? | `screen.profile` | | |
| Are the language chips and alerts toggle meant to bind to model state? | `control.settings.language`, `control.settings.alerts` | | |

## Verdict

_Overall assessment, and whether the gold is fit to remain the answer key._

Findings accepted: _(list)_
Findings rejected: _(list, with reasons)_

