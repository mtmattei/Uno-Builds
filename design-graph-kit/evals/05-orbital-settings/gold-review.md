# Gold review — `05-orbital-settings`

**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** `evals\05-orbital-settings\gold.graph.json` (56 nodes / 88 edges / 3 unresolved)

> Independent review breaking the same-author circularity: every gold in this
> kit was authored and calibrated by one agent lineage. Record findings here;
> **do not edit gold directly** - accepted fixes go through
> `tools/build_graphs.py`, then `scripts/validate_graph.py`.

## Read in this order

1. `evals/05-orbital-settings/fixture.md` — the source list this gold was authored from
2. The actual source files it names (listed below)
3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary
4. `references/ontology.md` — node/edge definitions, state scope and attachment
5. `evals/05-orbital-settings/README.md` — the altitude contract for this eval

## Sources cited by this gold

| Source file | Found | Nodes citing it |
|---|---|---|
| `Orbital/Orbital/Controls/PageHeader.xaml` | yes | 1 |
| `Orbital/Orbital/Presentation/SettingsPage.xaml` | yes | 34 |
| `Orbital/Orbital/Presentation/SettingsPage.xaml.cs` | yes | 4 |

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
| `token.color.emerald-500` | `resourceKey` | `OrbitalEmerald500Brush` | `Orbital Styles/*.xaml` |

## Structural facts

Stated as counts, not judgements — the altitude contract decides which of
these are correct, and that call belongs to the reviewer.

| Node type | In gold |
|---|---|
| `token` | 19 |
| `component` | 17 |
| `content` | 10 |
| `control` | 6 |
| `state` | 2 |
| `screen` | 1 |
| `asset` | 1 |
| `region` | **0** |

| Relation | In gold |
|---|---|
| `uses-token` | 41 |
| `contains` | 29 |
| `instance-of` | 14 |
| `has-state` | 2 |
| `triggers` | 2 |

The cited XAML declares **27** layout containers (`Grid`/`StackPanel`/`AutoLayout`/…) across 3 file(s).
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
| `screen.settings` | screen | Settings | observed | 1.0 | type=Page, class=Orbital.Presentation.SettingsPage | | |
| `component.page-header` | component | Page header | declared | 1.0 | type=UserControl, class=Orbital.Controls.PageHeader | | |
| `control.header.search` | control | Search / command palette | declared | 1.0 | type=Border, xName=SearchBorder | | |
| `content.settings.title` | content | Title | observed | 1.0 | type=TextBlock, styleKey=OrbitalPageTitle | | |
| `content.settings.subtitle` | content | Subtitle | observed | 1.0 | type=TextBlock, styleKey=OrbitalBody | | |
| `component.settings-card` | component | Settings section card | derived | 1.0 | type=Border, styleKey=OrbitalCardStyle | | |
| `component.settings-card.profile` | component | Profile section | declared | 1.0 | xName=ProfileSection | | |
| `component.settings-card.about` | component | About section | declared | 1.0 | xName=AboutSection | | |
| `component.settings-card.paths` | component | Paths section | declared | 1.0 | xName=PathsSection | | |
| `component.settings-card.actions` | component | Actions section | declared | 1.0 | xName=ActionsSection | | |
| `content.profile.section-title` | content | Profile header | observed | 1.0 | type=TextBlock, styleKey=OrbitalSectionHeader | | |
| `content.profile.name-label` | content | Display Name label | observed | 1.0 | type=TextBlock, styleKey=OrbitalMonoSmall | | |
| `control.profile.username` | control | Display Name | declared | 1.0 | type=TextBox, xName=UsernameBox | | |
| `control.profile.save` | control | Save | declared | 1.0 | type=Button, styleKey=OrbitalPrimaryButtonSm, xName=SaveUsernameButton | | |
| `content.profile.name-helper` | content | Name helper | observed | 1.0 | type=TextBlock, styleKey=OrbitalMonoSmall | | |
| `content.about.section-title` | content | About header | observed | 1.0 | type=TextBlock, styleKey=OrbitalSectionHeader | | |
| `asset.about.logo` | asset | Orbital logo | observed | 1.0 | type=Image, source=ms-appx:///Assets/Icons/Uno-logo.png | | |
| `content.about.app-name` | content | App name | observed | 1.0 | type=TextBlock, styleKey=OrbitalBody | | |
| `content.about.version` | content | Version | declared | 1.0 | type=TextBlock, styleKey=OrbitalMonoSmall | | |
| `component.info-row` | component | Info row (label/value) | derived | 1.0 | type=Grid | | |
| `component.info-row.uno-sdk` | component | Uno Platform SDK | declared | 1.0 | — | | |
| `component.info-row.dotnet` | component | .NET Runtime | declared | 1.0 | — | | |
| `component.info-row.renderer` | component | Renderer | declared | 1.0 | — | | |
| `component.info-row.platform` | component | Platform | declared | 1.0 | — | | |
| `content.paths.section-title` | content | Paths header | observed | 1.0 | type=TextBlock, styleKey=OrbitalSectionHeader | | |
| `component.path-field` | component | Path field (label/value) | derived | 1.0 | type=StackPanel | | |
| `component.path-field.project-root` | component | Project Root | declared | 1.0 | — | | |
| `component.path-field.recents-db` | component | Recent Projects Database | declared | 1.0 | — | | |
| `component.path-field.skills` | component | Claude Code Skills | declared | 1.0 | — | | |
| `content.actions.section-title` | content | Actions header | observed | 1.0 | type=TextBlock, styleKey=OrbitalSectionHeader | | |
| `component.action-button` | component | Ghost action button (icon+text) | derived | 1.0 | type=Button, styleKey=OrbitalGhostButtonSm | | |
| `control.actions.clear-recents` | control | Clear Recent Projects | declared | 1.0 | type=Button, xName=ClearRecentsButton, iconGlyph=E74D | | |
| `control.actions.open-data-folder` | control | Open Data Folder | declared | 1.0 | type=Button, xName=OpenDataFolderButton, iconGlyph=E838 | | |
| `control.actions.open-docs` | control | Uno Platform Documentation | declared | 1.0 | type=Button, xName=OpenDocsButton, iconGlyph=E8A5 | | |
| `component.dialog.recents-cleared` | component | Cleared dialog | declared | 1.0 | type=ContentDialog | | |
| `state.settings.entering` | state | Entering | declared | 1.0 | mechanism=code-behind, member=AnimationHelper.FadeUp | | |
| `state.profile.saved` | state | Saved | declared | 1.0 | mechanism=code-behind | | |
| `token.color.surface0` | token | Surface 0 (page bg) | declared | 1.0 | resourceKey=OrbitalSurface0Brush, resourceType=SolidColorBrush | | |
| `token.color.surface1` | token | Surface 1 (card bg) | declared | 1.0 | resourceKey=OrbitalSurface1Brush, resourceType=SolidColorBrush | | |
| `token.color.surface3` | token | Surface 3 (card border) | declared | 1.0 | resourceKey=OrbitalSurface3Brush, resourceType=SolidColorBrush | | |
| `token.radius.12` | token | 12 radius | declared | 1.0 | styleKey=OrbitalCardStyle, property=CornerRadius | | |
| `token.radius.8` | token | 8 radius | declared | 1.0 | property=CornerRadius | | |
| `token.spacing.24` | token | 24 spacing | observed | 1.0 | property=Spacing | | |
| `token.spacing.16` | token | 16 spacing | observed | 1.0 | property=Spacing | | |
| `token.spacing.12` | token | 12 spacing | observed | 1.0 | property=Spacing | | |
| `token.spacing.8` | token | 8 spacing | observed | 1.0 | property=Spacing | | |
| `token.typography.mono-small` | token | Mono small | declared | 1.0 | styleKey=OrbitalMonoSmall, fontResourceKey=OrbitalMonoFont | | |
| `token.color.text-30` | token | Text 30% emphasis | declared | 1.0 | resourceKey=OrbitalText30Brush, resourceType=SolidColorBrush | | |
| `token.color.text-38` | token | Text 38% emphasis | declared | 1.0 | resourceKey=OrbitalText38Brush, resourceType=SolidColorBrush | | |
| `token.color.text-40` | token | Text 40% emphasis | declared | 1.0 | resourceKey=OrbitalText40Brush, resourceType=SolidColorBrush | | |
| `token.color.text-50` | token | Text 50% emphasis | declared | 1.0 | resourceKey=OrbitalText50Brush, resourceType=SolidColorBrush | | |
| `token.color.text-72` | token | Text 72% emphasis | declared | 1.0 | resourceKey=OrbitalText72Brush, resourceType=SolidColorBrush | | |
| `token.color.text-85` | token | Text 85% emphasis | declared | 1.0 | resourceKey=OrbitalText85Brush, resourceType=SolidColorBrush | | |
| `token.color.emerald-500` | token | Emerald 500 (primary accent) | declared | 1.0 | resourceKey=OrbitalEmerald500Brush, resourceType=SolidColorBrush | | |
| `token.typography.page-title` | token | Page title type | declared | 1.0 | styleKey=OrbitalPageTitle | | |
| `token.typography.body` | token | Body type | declared | 1.0 | styleKey=OrbitalBody | | |

## Edges

Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are
the ones a graph must never invent. They are listed first.

| Relation | From | To | Evidence | Verdict | Note |
|---|---|---|---|---|---|
| **triggers** | `control.profile.save` | `state.profile.saved` | declared | | |
| **triggers** | `control.actions.clear-recents` | `component.dialog.recents-cleared` | declared | | |
| **contains** | `screen.settings` | `component.page-header` | observed | | |
| **contains** | `component.page-header` | `content.settings.title` | observed | | |
| **contains** | `component.page-header` | `content.settings.subtitle` | observed | | |
| **contains** | `component.page-header` | `control.header.search` | declared | | |
| **contains** | `screen.settings` | `component.settings-card.profile` | observed | | |
| **instance-of** | `component.settings-card.profile` | `component.settings-card` | derived | | |
| **contains** | `screen.settings` | `component.settings-card.about` | observed | | |
| **instance-of** | `component.settings-card.about` | `component.settings-card` | derived | | |
| **contains** | `screen.settings` | `component.settings-card.paths` | observed | | |
| **instance-of** | `component.settings-card.paths` | `component.settings-card` | derived | | |
| **contains** | `screen.settings` | `component.settings-card.actions` | observed | | |
| **instance-of** | `component.settings-card.actions` | `component.settings-card` | derived | | |
| **contains** | `component.settings-card.profile` | `content.profile.section-title` | observed | | |
| **contains** | `component.settings-card.profile` | `content.profile.name-label` | observed | | |
| **contains** | `component.settings-card.profile` | `control.profile.username` | observed | | |
| **contains** | `component.settings-card.profile` | `control.profile.save` | observed | | |
| **contains** | `component.settings-card.profile` | `content.profile.name-helper` | observed | | |
| **contains** | `component.settings-card.about` | `content.about.section-title` | observed | | |
| **contains** | `component.settings-card.about` | `asset.about.logo` | observed | | |
| **contains** | `component.settings-card.about` | `content.about.app-name` | observed | | |
| **contains** | `component.settings-card.about` | `content.about.version` | observed | | |
| **contains** | `component.settings-card.about` | `component.info-row.uno-sdk` | observed | | |
| **contains** | `component.settings-card.about` | `component.info-row.dotnet` | observed | | |
| **contains** | `component.settings-card.about` | `component.info-row.renderer` | observed | | |
| **contains** | `component.settings-card.about` | `component.info-row.platform` | observed | | |
| **instance-of** | `component.info-row.uno-sdk` | `component.info-row` | derived | | |
| **instance-of** | `component.info-row.dotnet` | `component.info-row` | derived | | |
| **instance-of** | `component.info-row.renderer` | `component.info-row` | derived | | |
| **instance-of** | `component.info-row.platform` | `component.info-row` | derived | | |
| **contains** | `component.settings-card.paths` | `content.paths.section-title` | observed | | |
| **contains** | `component.settings-card.paths` | `component.path-field.project-root` | observed | | |
| **contains** | `component.settings-card.paths` | `component.path-field.recents-db` | observed | | |
| **contains** | `component.settings-card.paths` | `component.path-field.skills` | observed | | |
| **instance-of** | `component.path-field.project-root` | `component.path-field` | derived | | |
| **instance-of** | `component.path-field.recents-db` | `component.path-field` | derived | | |
| **instance-of** | `component.path-field.skills` | `component.path-field` | derived | | |
| **contains** | `component.settings-card.actions` | `content.actions.section-title` | observed | | |
| **contains** | `component.settings-card.actions` | `control.actions.clear-recents` | observed | | |
| **contains** | `component.settings-card.actions` | `control.actions.open-data-folder` | observed | | |
| **contains** | `component.settings-card.actions` | `control.actions.open-docs` | observed | | |
| **instance-of** | `control.actions.clear-recents` | `component.action-button` | derived | | |
| **instance-of** | `control.actions.open-data-folder` | `component.action-button` | derived | | |
| **instance-of** | `control.actions.open-docs` | `component.action-button` | derived | | |
| **has-state** | `screen.settings` | `state.settings.entering` | declared | | |
| **has-state** | `control.profile.save` | `state.profile.saved` | declared | | |
| **uses-token** | `screen.settings` | `token.color.surface0` | declared | | |
| **uses-token** | `screen.settings` | `token.spacing.24` | observed | | |
| **uses-token** | `component.settings-card` | `token.color.surface1` | declared | | |
| **uses-token** | `component.settings-card` | `token.color.surface3` | declared | | |
| **uses-token** | `component.settings-card` | `token.radius.12` | declared | | |
| **uses-token** | `component.settings-card` | `token.spacing.16` | observed | | |
| **uses-token** | `component.settings-card.actions` | `token.spacing.12` | observed | | |
| **uses-token** | `control.profile.save` | `token.radius.8` | declared | | |
| **uses-token** | `component.action-button` | `token.radius.8` | declared | | |
| **uses-token** | `component.info-row` | `token.typography.mono-small` | declared | | |
| **uses-token** | `component.path-field` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.settings.title` | `token.typography.page-title` | declared | | |
| **uses-token** | `content.settings.subtitle` | `token.typography.body` | declared | | |
| **uses-token** | `content.settings.subtitle` | `token.color.text-40` | declared | | |
| **uses-token** | `content.profile.section-title` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.profile.section-title` | `token.color.text-38` | declared | | |
| **uses-token** | `content.about.section-title` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.about.section-title` | `token.color.text-38` | declared | | |
| **uses-token** | `content.paths.section-title` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.paths.section-title` | `token.color.text-38` | declared | | |
| **uses-token** | `content.actions.section-title` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.actions.section-title` | `token.color.text-38` | declared | | |
| **uses-token** | `content.profile.name-label` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.profile.name-label` | `token.color.text-50` | declared | | |
| **uses-token** | `component.info-row` | `token.color.text-50` | declared | | |
| **uses-token** | `component.info-row` | `token.color.text-72` | declared | | |
| **uses-token** | `component.path-field` | `token.color.text-50` | declared | | |
| **uses-token** | `component.path-field` | `token.color.text-72` | declared | | |
| **uses-token** | `content.profile.name-helper` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.profile.name-helper` | `token.color.text-30` | declared | | |
| **uses-token** | `content.about.app-name` | `token.typography.body` | declared | | |
| **uses-token** | `content.about.app-name` | `token.color.text-85` | declared | | |
| **uses-token** | `content.about.version` | `token.typography.mono-small` | declared | | |
| **uses-token** | `content.about.version` | `token.color.text-40` | declared | | |
| **uses-token** | `control.profile.username` | `token.color.surface1` | declared | | |
| **uses-token** | `control.profile.username` | `token.color.surface3` | declared | | |
| **uses-token** | `control.profile.username` | `token.color.text-85` | declared | | |
| **uses-token** | `control.profile.save` | `token.color.emerald-500` | declared | | |
| **uses-token** | `control.header.search` | `token.color.surface1` | declared | | |
| **uses-token** | `control.header.search` | `token.color.surface3` | declared | | |
| **uses-token** | `control.header.search` | `token.radius.8` | declared | | |

## Unresolved items

| Question | Related ids | Genuinely undecidable? | Note |
|---|---|---|---|
| What UI does the header search / command palette open? | `control.header.search` | | |
| Are the ABOUT info-rows and the PATHS fields one reusable key/value component or two? | `component.info-row`, `component.path-field` | | |
| Should the docs button's external URL launch be modeled as navigates-to? | `control.actions.open-docs` | | |

## Verdict

_Overall assessment, and whether the gold is fit to remain the answer key._

Findings accepted: _(list)_
Findings rejected: _(list, with reasons)_

