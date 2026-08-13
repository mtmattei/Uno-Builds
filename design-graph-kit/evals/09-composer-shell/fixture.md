# Fixture: Composer Shell (source-backed)

The fourth source-backed eval, and the densest design system in the pool:
Composer carries 8 theme dictionaries (`ChipStyles.xaml` alone is 51 KB), a
separate `Typography.xaml` and `Tokens.xaml`, and 11 reusable controls. It is
chosen to stress the two rules that the earlier evals only lightly exercised —
**screen-scoped token extraction** (a whole-dictionary dump here would be
enormous and wrong) and **component reference expansion** (the screen is almost
nothing *but* references).

Architecture: **MVUX** (`ShellModel` feeds), Uno Toolkit, `net10.0-desktop` +
`net10.0-browserwasm`.

## Source (the modeled surface)

- `Composer/src/Composer/Composer/Shell.xaml` — the three-column workspace and
  the two rail storyboards
- `Composer/src/Composer/Composer/Shell.xaml.cs` — rail column toggling
- `Composer/src/Composer/Composer/Views/Controls/CompositionStack.xaml` — left rail
- `Composer/src/Composer/Composer/Views/Controls/ActiveCanvas.xaml` — center canvas
- `Composer/src/Composer/Composer/Views/Controls/ActiveLayerHeader.xaml` — header inside the canvas
- `Composer/src/Composer/Composer/Views/Controls/FilesRail.xaml` — right rail
- `Composer/src/Composer/Composer/Themes/Tokens.xaml` — color/spacing/radius tokens
- `Composer/src/Composer/Composer/Themes/Typography.xaml` — type scale

## Scope boundary

The eight views under `Views/Layers/` (IntentCard, DesignTokenGrid,
ScaffoldTerminal, …) are **out of scope**. They are content the center canvas
hosts, not part of the shell surface, and pulling them in would turn one eval
into eight. A graph that models them is over-reaching; a graph that notes the
canvas hosts swappable layer content is correct.

`ChipStyles.xaml`, `PlatformChip.xaml`, `RuntimeChip.xaml`, `Icons.xaml` and
`ContextEngineStyles.xaml` are in the app but are consumed by those layer
views. Tokens should be extracted for what **this** surface actually consumes —
that is the discipline being measured.

## What makes this eval hard

1. **The screen is a composition of references.** `Shell.xaml` contains three
   custom controls and almost no leaf content of its own. A graph that stops at
   the three references has missed the screen entirely — the PageHeader lesson,
   but three times over and one level deeper (`ActiveCanvas` itself contains
   `ActiveLayerHeader`).
2. **Token dictionaries are large enough to punish over-extraction.** Emitting
   every `x:Key` in `Themes/` would produce hundreds of token nodes for a
   surface that consumes a fraction of them.
3. **Real declared state.** Two storyboards (`RailsRevealStoryboard`,
   `RailsHideStoryboard`) with exact timings, driven from code-behind by
   toggling `ColumnDefinition.Width` between 0 and 280. The rails are 0px on
   the first screen and snap open on first lock, which is a genuine screen
   state, not a style-level hover.
4. **Comments state design intent that the markup does not.** `Shell.xaml`
   cites briefs and explains *why* the columns snap rather than animate
   ("Grid columns don't smoothly re-measure under DoubleAnimation on Skia
   desktop"). Rationale is evidence about the design, and worth carrying —
   but it is not a licence to invent behavior the code does not implement.
