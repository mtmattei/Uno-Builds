# Composer

A conversational composer for bootstrapping Uno Platform projects. Walks a developer through six lightweight decisions (description, wiring, design system, interactions, architecture, plan) and produces a complete project starter pack — README, CLAUDE.md, .mcp.json, DESIGN.md, INTERACTIONS.md, ARCHITECTURE.md, implementation-plan.md, and a scaffold script — that they can hand to Claude Code or paste into a fresh repo.

The interaction model is deliberately *not* a wizard. There are no Next/Back buttons, no progress meter, no required path. The app reads as an editorial transcript on the left and a live build console on the right. Each turn the composer asks a single question with a recommended answer pre-formed, the developer either accepts the recommendation, picks a labeled alternative, or types their own response in free text. As decisions land, artifacts in the right column transition from a hairline outlined dot to a filled black circle with an inscribed checkmark — the panel becomes a quiet status ledger that stays in sync with the chat's current focus.

## Project Instructions

This project is a single-page Uno Platform WebAssembly application. The target framework is `.NET 10`, the renderer is **Skia for WASM** (Uno's high-fidelity renderer that produces identical output across browsers without DOM dependence), the markup language is **XAML**, and state management uses **MVUX** (Uno's reactive Feeds/States/Records pattern). The app uses Uno's Material theme as its base, with a deliberate override layer that strips Material's color and typography down to a monochromatic editorial palette.

The app does no persistence — all state lives in MVUX models for the duration of the session. Bundle export is the only output: the developer fills in their app description, walks the conversation, and clicks "Download bundle" to receive a single Markdown file that contains every artifact concatenated with language-tagged code fences. There is no backend; the contextual reasoning system makes a single client-side call to the Anthropic Messages API on description submission.

## Overview

The composer experience has three structural pieces.

**Foundation panel** (top of right column). Three inputs that the developer fills before the conversation begins: the app's name (text input, required), target platforms (multi-select chip group with a custom morph animation — selected chips shrink from a text label into a 28×28 circular icon), and the .NET runtime (single-select between `.NET 10` and `.NET 9`, defaulted to .NET 10 with a small "REC" eyebrow). Until app name has a value and at least one platform is selected, the conversation cannot begin. The Begin Composing button surfaces only when both conditions are met.

**Chat transcript** (left column). The composer asks questions, the developer answers. There are no chat bubbles. Speakers are distinguished by a small mono uppercase label (`USER` or `COMPOSER`) and a hairline rule between turns. The current composer turn shows a full Suggestion Panel — a tinted card with the recommended answer, an italic single-sentence justification grounded in the developer's app description, an Apply button, and a collapsible Show alternatives section that reveals two or three alternative labeled buttons plus (for the design step only) a panel of design asset inputs (Figma URL, Prototype URL, Screenshot URL or path). Older composer turns collapse into compact summary rows showing the layer name, the developer's chosen label, and a small Edit pill that lets them roll the conversation back to that point.

**Live build artifact panel** (right column, below foundation). Eight artifact cards corresponding to the eight files the bundle will produce. Each card has a status glyph (planned: hairline outlined dot; drafted: filled black circle with an animated checkmark) and a filename. The card auto-expands when its corresponding chat layer becomes active, showing either the drafted content (if the layer has been completed) or a small placeholder "Awaits your decision in chat." (if planned). When the chat advances, the card collapses and the next layer's target card expands. Drafted cards remain clickable for inline review and editing.

## Project Structure

```
src/
  Composer/
    Composer.csproj              — Uno Platform WASM project, .NET 10 target
    App.xaml                     — global resources, theme dictionaries, fonts
    App.xaml.cs                  — host builder, MVUX setup, DI
    MainPage.xaml                — root layout (two-column responsive grid)
    MainPage.xaml.cs             — code-behind (minimal, navigation only)
    Models/
      ComposerModel.cs           — top-level MVUX model (state record + transitions)
      ChatTurn.cs                — record (role, layer, body, callouts, applied label)
      Artifact.cs                — record (id, file, layer, status, content)
      LayerSuggestion.cs         — record (label, reasoning, action, alternatives)
    Views/
      FoundationPanel.xaml       — app name, platforms, runtime
      ChatTranscript.xaml        — message list (ItemsRepeater)
      LivebuildPanel.xaml        — artifact cards (ItemsRepeater)
      SuggestionPanel.xaml       — current turn's recommendation + alternatives
      MessageBlock.xaml          — single chat turn (user or composer)
      CompactComposerTurn.xaml   — collapsed older composer turn
      ArtifactCard.xaml          — single artifact row + expandable body
      PlatformChip.xaml          — animated platform chip (text → icon morph)
      StatusGlyph.xaml           — animated dot → check
    Themes/
      Colors.xaml                — palette overrides (paper/ink/hairline)
      Typography.xaml            — Fraunces + Martian Mono text styles
      ChipStyles.xaml            — platform chip + runtime chip + suggestion chip
      ArtifactCardStyle.xaml     — card frame and animated content host
    Templates/
      ArtifactTemplates.cs       — pure functions: state → markdown content
    Services/
      AnthropicClient.cs         — single HTTP call for contextual reasoning
      BundleExporter.cs          — concatenate artifacts → single .md, save via picker
    Strings/
      en/Resources.resw          — all visible text (x:Uid bindings)
```

## Conventions

The project follows Uno Platform development conventions throughout. All visible text uses `x:Uid` localization (pattern: `MainPage.Label.Description`, `FoundationPanel.Button.Begin`, etc.). Bindings are exclusively used for state — no code-behind state mutation, no manual property updates. Commands are bound via the implicit `IAsyncCommand` MVUX pattern (a public method `OnApplySuggestion` on the model is bound from XAML as `Command="{Binding ApplySuggestion}"`). All interactive controls have meaningful `AutomationProperties.Name` and (where applicable) `AutomationProperties.HelpText`. The chip group uses `aria-pressed` semantics via Uno's automation peers.

Theming starts from Material via `<MaterialColors />` and `<MaterialFonts />`, then overrides selected resources in `Themes/Colors.xaml` and `Themes/Typography.xaml`. No hardcoded hex colors anywhere except in the palette resource file itself. No explicit font sizes outside Typography.xaml.

Animations are XAML Storyboards triggered by `VisualStateManager` state changes. The `VisualStateManagerExtensions` attached property from Uno Toolkit is used to drive states from MVUX state values (so the model declares "Drafted" as a string state and the view's `VisualStateManager` listens for it). Three named easings are defined as `KeySpline` resources: `EaseOutCubic`, `EaseOutExpo`, and `EaseOutQuint`. All durations are multiples of 20ms and clamp at 480ms.

## Key References

- `DESIGN.md` — full visual design system: palette, typography, spacing, every component's visual specification with state variants
- `INTERACTIONS.md` — every animation, transition, keyboard handler, micro-interaction, error and loading state, and accessibility requirement
- `ARCHITECTURE.md` — MVUX model shape, view hierarchy, DI registrations, services, navigation, and the React-component → Uno-control mapping table
- `implementation-plan.md` — six vertical slices with definition-of-done for each

## Pre-Review Cleanup

Before opening a PR, run `dotnet format` against `Composer.csproj`, verify Hot Reload picked up all your XAML edits (no stale visual states), and confirm that the WASM build size hasn't regressed past the 8MB initial-load target. Remove any `<TextBlock Text="..."/>` that hasn't been migrated to `x:Uid`, and audit `App.xaml` to make sure no inline color overrides have leaked outside `Themes/`.

## Verification

Build and run for WebAssembly:

```bash
dotnet run -f net10.0-browserwasm --project src/Composer/Composer.csproj
```

Expected behavior on first load: foundation panel renders with all three fields empty, conversation area shows an empty state with a small mono caption ("Fill the foundation, then begin composing."), Begin button is hidden. Type an app name, the foundation re-drafts README and CLAUDE artifacts in the background (you can verify by manually expanding those cards). Add at least one platform chip (it morphs from text to icon). Begin button appears. Click Begin, the description prompt appears in the chat, and README + CLAUDE cards auto-expand showing their current content. The full conversation should walk wiring → design → interactions → architecture → plan with no errors and no missed transitions. Click Download bundle on the final turn — the FileSavePicker should open with a suggested filename matching the app name.
