# FREEWRITE.UNO — Design Brief

Version 0.2 · 2026-05-20 · Author: M. Mattei
Spec: 2 of 3 (Architecture · Design · Interaction)
Design anchor: Naoto Fukasawa — *Super Normal*, *Without Thought*

---

## Context

Freewrite is a writing surface ported from macOS SwiftUI to Uno Platform. The product's single commitment is to disappear from the writer's attention. This document specifies the visual system that operationalises that commitment. Architecture lives in 01-architecture-brief.md. Interaction lives in 03-interaction-brief.md.

---

## 1. Visual Direction

### 1.1 The aesthetic position, in one paragraph

Refined minimalism executed with precision. The product's emotional register is the desk lamp, not the firework. There are no accent colours, no animated illustrations, no "delight" moments. Feedback is signalled by opacity shifts, not hue shifts. Backgrounds use warm neutrals rather than pure white or pure black. Typography carries the entire personality of the editor surface; everything else is chrome that the writer should forget exists.

### 1.2 Why Fukasawa

Most "minimal" apps treat minimalism as a style — small typography, thin lines, lots of negative space. Fukasawa's contribution is the inversion: the Super Normal object is not striking. It is *beautiful in the way it is hard to notice*. It supports an action so naturally that the user forgets the object exists. This is the correct framing for a writing app whose explicit goal is to remove itself from the writer's attention.

Five principles drawn from Fukasawa's published canon, applied directly:

1. **Without Thought.** No decision-making before writing begins. Launch creates an entry, focuses the editor, and starts saving.
2. **At the outline of consciousness.** Chrome sits at the edge of the screen and fades out one second after the timer starts. The writer should sense the chrome more than see it.
3. **Calm before expression.** No accent colour, no movement that pulls attention, no celebration of milestones.
4. **Honest materials.** Files are plain Markdown on disk with a known location and a human-readable name. No proprietary container.
5. **Behaviour drives form.** Every feature is reviewed against the act of writing. Features that survive must directly support that act.

### 1.3 Anti-patterns (written down so we can refuse them)

- Streaks, daily goals, word counts, character counts.
- Social features, collaborative editing, cloud sync.
- Themes beyond light / dark.
- AI writing assistance embedded in the editor surface.
- Animations longer than 250 ms.
- Iconography that requires a legend.
- Onboarding tours, tooltips on launch, "welcome to" dialogs.

---

## 2. Layout Structure

### 2.1 Single-page composition

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                                                                 │
│                   ┌─────────────────────────┐                   │
│                   │                         │                   │
│                   │   Editor — max 720 px   │                   │
│                   │   centred, generous     │                   │
│                   │   padding, no chrome    │                   │
│                   │                         │                   │
│                   │   ▌ cursor              │                   │
│                   │                         │                   │
│                   └─────────────────────────┘                   │
│                                                                 │
│                                                                 │
│             ┌───┬──────┬───────┬───┬───┬───┬───┬───┬───┐        │
│             │18 │ Lato │ 14:32 │ ⌫ │ 💬│ 📄│ + │ ☾ │ 🕐│        │
│             └───┴──────┴───────┴───┴───┴───┴───┴───┴───┘        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

The editor occupies the full viewport. The chrome bar is a pill-shaped object centred at the bottom, sitting on the surface as a single unit rather than a row of buttons. The history sidebar is a transient overlay from the right edge, dismissed on selection or on click outside.

### 2.2 The chrome pill

The chrome is enclosed in a 1 px `OutlineColor` border on the surface, not floating buttons. The pill exists so the eye can dismiss the entire control bar in one pass instead of cataloguing each button. This is a meaningful Fukasawa decision: the chrome is *one object* the writer can ignore as a unit.

### 2.3 Editor margins

| Viewport | Editor padding | Content max-width | Effective line length |
|---|---|---|---|
| ≥ 1280 px (desktop, large) | 96 px horizontal, 48 px top, 112 px bottom | 720 px | ~66 chars at 18 sp |
| 600–1280 px (desktop, medium) | 48 px horizontal, 48 px top, 112 px bottom | 720 px | ~66 chars at 18 sp |
| < 600 px (mobile / narrow) | 24 px horizontal, 32 px top, 96 px bottom | none (fills width) | varies, typically 38–50 chars |

The 112 px bottom on desktop reserves space for the chrome pill so editor text never slides under it. On mobile, the chrome pill compresses (see §6.3 in this brief) and bottom padding drops to 96 px.

### 2.4 Sidebar

| Property | Value |
|---|---|
| Width | 280 px |
| Placement | Right edge |
| `DisplayMode` | Overlay |
| Background | `SurfaceBrush` |
| Left border | 1 px `OutlineBrush` |
| Padding | 32 px top, 16 px sides, 24 px bottom |

On viewports < 600 px, sidebar width becomes 100 vw (full overlay) and the close affordance is a tap-outside-or-anywhere-on-content gesture.

---

## 3. Typography

Per Uno guidance: every text element references a named `TextBlock` style. No `FontSize` or `FontWeight` set inline. The styles below register into the Material type scale.

### 3.1 The type roles

| Role | Style key | Family | Size | Line height | Weight | Used for |
|---|---|---|---|---|---|---|
| Editor body | `EditorBodyTextStyle` | Lato Regular *(cycles)* | 18 sp *(cycles 16–26)* | 27 sp (1.5×) | 400 | The text the user writes |
| Sidebar preview | `SidebarPreviewTextStyle` | Lato Regular | 14 sp | 20 sp | 400 | First-30-char entry summary |
| Sidebar date | `SidebarDateTextStyle` | JetBrains Mono Regular | 11 sp | 16 sp | 400 | "Today" / "Yesterday" / "May 18" |
| Chrome label | `ChromeLabelTextStyle` | Lato Regular | 13 sp | 16 sp | 400 | Font name, size, flyout items |
| Timer | `TimerTextStyle` | JetBrains Mono Regular | 13 sp | 16 sp | 400 | MM:SS countdown |
| Sidebar section header | `SidebarHeaderTextStyle` | JetBrains Mono Regular | 10 sp | 14 sp | 400, uppercase, 0.12 em letter-spacing | "HISTORY" label |

### 3.2 The font system

Three fonts ship with the app, bundled as `.ttf` assets:

| Font | Role |
|---|---|
| Lato | Default editor body; all chrome labels; all sidebar previews |
| Newsreader | Editor body alternative — gives the surface a "page" feel |
| JetBrains Mono | Timer, dates, section headers — anything that reads as metadata or measurement |

The chrome stays in Lato even when the editor cycles to Newsreader or JetBrains Mono. The chrome should be perceptually invisible regardless of the editor's typographic personality.

The original macOS app's "Random font from system fonts" feature is dropped. Bundled fonts are the only fonts.

### 3.3 The 1.5× line height decision

The Uno guidance default is 140% of font size. This brief deliberately specifies 150%. The extra 10% is what makes the editor feel like a page rather than a chat transcript. At 18 sp, line height is 27 sp — a multiple of 4 (rounded from 27 exactly, since the baseline grid is forgiving above the editor's max width).

This is the one place the brief overrides Material defaults. The override is registered in `Themes/TextBlock.xaml`.

### 3.4 Line length

Editor max-width is 720 px. At 18 sp Lato, this yields approximately 66 characters per line — the centre of the legibility range (45–72 chars) called out in the Uno design guidance. The Uno rules permit up to 120 chars on desktop with increased line-height; this brief prefers 66 because the writer's focus is sustained, not scanning.

---

## 4. Colour

### 4.1 The palette tokens

Every colour referenced in the app comes from `Themes/ColorPaletteOverride.xaml`. No hex codes appear anywhere else in the codebase. The palette has six tokens per theme, plus a `selection` value that is opacity-derived from `OnSurface`.

#### Light palette

| Token | Hex | Role |
|---|---|---|
| `SurfaceColor` | `#FAF9F6` | Editor background, page background |
| `OnSurfaceColor` | `#2B2B2B` | Body text, active chrome icon |
| `OnSurfaceMutedColor` | `#8A857E` | Inactive chrome icon, sidebar metadata |
| `OnSurfaceFaintColor` | `#C9C4BE` | Disabled state — used sparingly |
| `OutlineColor` | `#EAE7E1` | Chrome pill border, sidebar separator, hairlines |
| `BackgroundAccentColor` | `#F0EDE6` | Selected sidebar row, hover state, backspace-lock flash |

#### Dark palette

| Token | Hex | Role |
|---|---|---|
| `SurfaceColor` | `#1A1916` | Editor background, page background |
| `OnSurfaceColor` | `#E6E3DE` | Body text, active chrome icon |
| `OnSurfaceMutedColor` | `#7A766F` | Inactive chrome icon, sidebar metadata |
| `OnSurfaceFaintColor` | `#3A3833` | Disabled state — used sparingly |
| `OutlineColor` | `#2A2823` | Chrome pill border, sidebar separator, hairlines |
| `BackgroundAccentColor` | `#252320` | Selected sidebar row, hover state, backspace-lock flash |

### 4.2 The rules

| Rule | Rationale |
|---|---|
| No accent colour | Active state is signalled by opacity, never hue |
| Surface and OnSurface deliberately avoid pure white / pure black | Pure values read as "device"; warm neutrals read as "object" |
| All backgrounds carry a slight warm cast (~10° from neutral, yellow-leaning) | Synthetic neutrals feel sterile; warm neutrals feel calm |
| Brushes are aliased to semantic names in a second-tier dictionary | Views reference `EditorTextBrush`, not `OnSurfaceBrush` |

Light mode contrast: `OnSurface` on `Surface` = 12.6:1 (WCAG AAA). Dark mode: 13.1:1. Both well exceed the 4.5:1 floor.

### 4.3 Brush aliases

A second dictionary in `Themes/ColorPaletteOverride.xaml` maps colour tokens to semantic brush names. This is the level the rest of the app talks to:

| Brush | Resolves to |
|---|---|
| `EditorBackgroundBrush` | `SurfaceColor` |
| `EditorTextBrush` | `OnSurfaceColor` |
| `EditorCaretBrush` | `OnSurfaceColor` |
| `EditorSelectionBrush` | `OnSurfaceColor` at 14% (light) / 16% (dark) |
| `ChromeIconBrushActive` | `OnSurfaceColor` |
| `ChromeIconBrushInactive` | `OnSurfaceMutedColor` |
| `ChromeOutlineBrush` | `OutlineColor` |
| `SidebarRowBrushSelected` | `BackgroundAccentColor` |
| `SidebarMetaBrush` | `OnSurfaceMutedColor` |

When the palette is tuned in M2, the only file that changes is `ColorPaletteOverride.xaml`. The rest of the app continues to bind the aliases.

---

## 5. Space & Rhythm

### 5.1 The spacing scale

Per Uno guidance, all spacing is from the 4 / 8 base scale. This brief extends the high end:

| Token | Value | Use |
|---|---|---|
| `Space4` | 4 | Hairline separation between elements in a tight group |
| `Space8` | 8 | Label + control pairs; chrome icon padding |
| `Space12` | 12 | Sidebar row padding |
| `Space16` | 16 | Default content padding |
| `Space24` | 24 | Section gap inside sidebar; mobile editor horizontal |
| `Space32` | 32 | Mobile editor top padding |
| `Space48` | 48 | Desktop editor vertical padding |
| `Space64` | 64 | (reserved, currently unused) |
| `Space96` | 96 | Desktop editor horizontal padding |
| `Space112` | 112 | Desktop editor bottom padding (reserves space for chrome) |

### 5.2 The grid

The editor is intentionally not on a strict 4 dp baseline grid because the editor's content is variable-height user text. The chrome and sidebar are. All chrome control padding is 6 px or 8 px; all icon sizes are 14 px; all gaps between chrome controls are 2 px (tight) plus a 1 px separator hairline.

---

## 6. Component Hierarchy

### 6.1 The component inventory

| Surface | Control | Style applied | Notes |
|---|---|---|---|
| Page root | `SplitView` | (default) | `DisplayMode=Overlay`, `PanePlacement=Right`, `OpenPaneLength=280` |
| Editor | `TextBox` | `EditorTextBoxStyle` | All chrome stripped; binds `Foreground`, `CaretBrush`, `SelectionHighlightColor` |
| Chrome bar | `Border` containing `StackPanel` (or `AutoLayout` if Toolkit present) | (inline) | Pill shape via `CornerRadius=10`, 1 px `ChromeOutlineBrush` border |
| Chrome icon button | `Button` | `ChromeIconButtonStyle` | Transparent background, no border, icon-only |
| Chrome text button | `Button` | `ChromeTextButtonStyle` | Same, with text content (font name, font size) |
| Chrome separator | `Border` (1 px wide, 14 px tall) | (inline) | `ChromeOutlineBrush` |
| Sidebar item | `ListViewItem` | `SidebarRowStyle` | 12 px padding, `BackgroundAccentColor` when selected |
| Chat flyout | `Flyout` on the chat button | (default) | Two `MenuFlyoutItem` for Claude / ChatGPT |
| Delete confirm | `ContentDialog` | (default) | Standard confirm before destructive action |
| Toast | `Border` with text | (inline) | "Copied to clipboard"; opacity 0.92; auto-dismiss 1.8 s |

No `NavigationView`. No `CommandBar`. No `Pivot`. No `TabView`. None of them are justified by the product's shape.

### 6.2 The editor `TextBox` style

`EditorTextBoxStyle` in `Themes/TextBox.xaml` strips every default chrome element from the WinUI `TextBox`:

- `BorderThickness=0`
- `Background=Transparent`
- `Padding=0` (margins live on the wrapping container)
- `ScrollViewer.VerticalScrollBarVisibility=Hidden`
- `BackgroundFocusVisualPrimaryThickness=0`
- `BackgroundFocusVisualSecondaryThickness=0`
- Removes the bottom border that WinUI adds on focus
- Removes the inset that WinUI applies to the placeholder text region
- Binds `Foreground` to `EditorTextBrush`
- Binds `CaretBrush` to `EditorCaretBrush`
- Binds `SelectionHighlightColor` to `EditorSelectionBrush`

The visible result on screen: only the user's text and the caret. No box, no underline, no focus ring.

### 6.3 Responsive chrome

At viewports < 600 px, the chrome pill compresses by hiding low-priority controls into an overflow menu:

| Priority | Always visible | Behind overflow on mobile |
|---|---|---|
| 1 | Timer | — |
| 1 | Theme toggle | — |
| 1 | Sidebar toggle | — |
| 1 | New entry | — |
| 2 | Backspace lock | — |
| 2 | Font size | ✓ |
| 2 | Font name | ✓ |
| 3 | Chat | ✓ |
| 3 | PDF export | ✓ |

This uses Uno Toolkit's `ResponsiveExtension` to swap between two `StackPanel` definitions on the same `<sw600` breakpoint. The brief does not specify the overflow menu icon (ellipsis is the default option).

---

## 7. Theme Usage

### 7.1 Theme switching

A single button in the chrome toggles between `ElementTheme.Light` and `ElementTheme.Dark`. The choice is persisted to `ApplicationData.Current.LocalSettings` via `ISettingsStore`. Theme is applied at the `Page` level (not `App`), so switching is instant without an app restart.

The 200 ms cross-fade on `Surface` and `OnSurface` is achieved by Material's built-in theme-dictionary transition. No custom Storyboard required.

### 7.2 Why we have a theme switcher at all

The Uno design guidance deprioritises theme switching. We include it because freewrite is a writing app used at both 10 AM and 11 PM, and forcing a single mode would be a worse user outcome than the small implementation cost. The toggle does not follow OS theme automatically — the choice is explicit and persistent. Writers know whether they want a dark surface at any given moment.

---

## 8. Responsive & Adaptive Behaviour

### 8.1 Breakpoints

| Range | Behaviour |
|---|---|
| ≥ 1280 px | Desktop default: editor padding 96 / 48, chrome pill full, sidebar 280 px overlay |
| 600–1280 px | Desktop compressed: editor padding 48 / 48, chrome pill full |
| < 600 px | Mobile: editor padding 24 / 32, chrome pill compressed (see §6.3), sidebar 100 vw |

Implemented via `ResponsiveExtension` markup in XAML. No code-behind viewport checks.

### 8.2 Orientation

Portrait and landscape both supported on Android. The editor's max-width (720 px) means landscape phone renders nearly identically to portrait phone in terms of effective line length; the difference is vertical breathing room. No orientation-specific logic.

### 8.3 Density

DPI scaling is handled by the framework. The 4 / 8 spacing scale is in effective pixels and scales correctly across all tested DPI settings. No special handling required.

---

## Unresolved Questions

- **`TextBox.LineHeight` fidelity.** WinUI `TextBox` partially honours `LineHeight`; native Android `EditText` does not. The 27 sp target at 18 sp font may differ visually between TFMs. Verified in M1. If variance is visible, the fallback is per-platform compensation in `EditorBodyTextStyle` or a switch to `RichEditBox`.
- **Material vs Cupertino on Skia.Desktop macOS.** Skia.Desktop covers macOS but the typographic conventions differ from the WinUI Material baseline. Decision deferred; v1 ships Material everywhere.
- **The mono-font-for-metadata convention.** Sidebar dates and the timer are in JetBrains Mono. The macOS original uses Lato for everything. The mono distinction is a small departure from the source product. Flag for user feedback after a session of real use.
- **Overflow menu visual.** §6.3 specifies an overflow on mobile but does not draw it. Ellipsis icon is the default option; alternatives (a flyout, a bottom-sheet, a long-press on the chrome itself) are worth considering. Defer to M3.
- **Print typography.** PDF export currently specifies Newsreader at 18 sp / 1.5×. The print medium may favour 12 pt / 1.6× more reliably; QuestPDF prototyping in M3 will settle this.
- **Selection highlight on dark mode.** 16% opacity of `OnSurface` on the dark background produces a slightly lighter cool tone than expected. Tune in M2 against actual reading conditions.
