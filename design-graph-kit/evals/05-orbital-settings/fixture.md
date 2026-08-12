# Fixture: Orbital Settings (source-backed)

This is the first **real** eval in the kit. Unlike evals 01–04 (synthetic
fixture descriptions), the input here is an existing Uno Platform screen with
full source: XAML, code-behind, and style dictionaries. That makes most facts
`observed`/`declared`/`derived` rather than `inferred`, and it lets the graph
legitimately use `triggers` and a source-backed transient state — things a
screenshot alone could never support.

## Source

- `Orbital/Orbital/Presentation/SettingsPage.xaml` — layout & controls
- `Orbital/Orbital/Presentation/SettingsPage.xaml.cs` — behavior (button wiring, entrance animation, transient "Saved!" state)
- `Orbital/Orbital/Styles/Surfaces.xaml` — `OrbitalCardStyle` (radius 12, padding 20, border 1, Surface1 bg, Surface3 border)
- `Orbital/Orbital/Styles/Buttons.xaml` — `OrbitalPrimaryButtonSm` / `OrbitalGhostButtonSm` (radius 8, font 13, padding 12,6)
- `Orbital/Orbital/Styles/TextBlock.xaml` — mono/display type styles (`OrbitalSectionHeader`, `OrbitalMonoSmall`, `OrbitalBody`)
- `Orbital/Orbital/Styles/OrbitalBrushes.xaml` — surface/text color tokens

## Visible / declared structure

- **Page header** — Title `Settings`, Subtitle `Profile and preferences`.
- **PROFILE** card — `Display Name` label, a name textbox (placeholder `Enter your name`), a prominent `Save` button, helper text `This name appears in the homepage greeting.`
- **ABOUT** card — Uno logo asset, app name `Orbital`, a version line, then four repeated label/value rows: `Uno Platform SDK`, `.NET Runtime`, `Renderer`, `Platform`.
- **PATHS** card — three repeated label + wrapping-value fields: `Project Root`, `Recent Projects Database`, `Claude Code Skills`.
- **ACTIONS** card — three repeated ghost buttons (icon + text): `Clear Recent Projects`, `Open Data Folder`, `Uno Platform Documentation`.

All four section containers share `OrbitalCardStyle` (a reusable card).

## Source-backed behavior (from code-behind)

- Every section fades/translates in on load (`AnimationHelper.FadeUp`) → an **entrance** presentation state.
- `Save` persists the name via `SettingsService.SaveUsername(...)` and shows a transient **`Saved!`** confirmation for 1.5s → a `triggers` edge into a source-backed transient state.
- `Open Data Folder`, `Clear Recent Projects`, `Uno Platform Documentation` run utility side-effects (open folder, clear list + dialog, launch external URL).

## What this eval tests

- hierarchy on a real, non-trivial screen (header + 4 cards);
- **consolidation**: four cards → one `settings-card` component; repeated rows/fields/buttons → canonical components + `instance-of`;
- **token extraction from declared styles** (color / spacing / radius / typography) rather than guessed pixels;
- **state modeling** for an entrance state and a transient "saved" state;
- **source-backed behavior** via `triggers` (contrast with eval 04, where behavior must stay `unresolved`);
- **uncertainty discipline**: genuine modeling ambiguities recorded in `unresolved` (are info-rows and path-fields the same key/value component? is an external-URL launch a `navigates-to`?).
