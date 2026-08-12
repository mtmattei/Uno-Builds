# Fixture: FluxTransit Profile (source-backed)

Second real eval — a different app, design system, and architecture than
eval 05, to test that the kit's results are not Orbital-specific.

## Source

- `FluxTransit/FluxTransit/FluxTransit/Presentation/ProfilePage.xaml` — layout
- `.../ProfilePage.xaml.cs` — trivial (InitializeComponent only)
- `.../Presentation/ProfileModel.cs` — **MVUX** model: `IState<>`s
  (OpusBalance, GeminiApiKey, SelectedLanguage, IsRefreshing) and commands
  (GoBack, UpdateBalance, SaveSettings)
- `.../Styles/FluxStyles.xaml` — glass-morphism design system (colors,
  spacing scale, corner radii incl. pill, type ramp)

## What makes this different from eval 05

- **MVUX instead of code-behind**: behavior arrives as declared bindings and
  model commands, not Click handlers — evidence discipline must follow
  bindings.
- **A binding-driven state**: `IsRefreshing` swaps the Update button for a
  ProgressRing + "Updating..." row (declared loading state + trigger).
- **Toolkit controls**: `utu:ChipGroup`/`Chip` (language), `ToggleSwitch`,
  `ProgressRing`.
- **Honesty traps**: Back navigates to a *stack-dependent* target;
  `Add New Route` has **no** command bound; `SaveSettings` is declared in the
  model but **nothing invokes it**; the chips/toggle are XAML **literals**,
  not bound to the model's `SelectedLanguage`. All four belong in
  `unresolved`, not as invented behavior.
- **Different token flavor**: alpha-channel glass colors (`#1e293b66`),
  pill radius (9999), a declared spacing scale (XS/S/M/L), tracked type ramp.

## What this eval tests

- generalization of the v0.4 rules (id grammar, altitude, token scoping,
  uno mapping) to a second design language;
- consolidation: 3 glass panels → one canonical; 2 route rows → canonical +
  instances;
- binding-declared state/trigger modeling;
- unresolved discipline on four distinct traps.
