# Design

## Aesthetic philosophy

**Editorial transcript meets agent build console.** The chat reads like a printed conversation — a thin hairline between turns, mono labels above each speaker, italic prose for the user's voice and upright serif for the composer's. The right column is a build console: monospace filenames, status indicators, a glyph that fills with a checkmark as work completes.

The reference is Naoto Fukasawa's "Super Normal" / "without thought" design philosophy. The interface gets out of the way. Decisions surface as a single recommendation with reasoning, not a wall of options. The work is the artifacts being drafted in real time — the UI itself recedes.

**No bubbles, no avatars, no emoji, no shadows other than the one outer page elevation.** Color is reserved for one thing: the only saturated element on the page is the dark "Apply" button when active. Everything else is an ink-on-paper grayscale.

**Three nouns, in order of visual weight:**
1. The conversation (left column) — the user is reading and responding here.
2. The decision card (suggested → reasoning → Apply) — the focal point of every composer turn.
3. The artifact panel (right column) — quietly tracks completion in the background.

## Color palette

All colors are defined as `Color` resources in `Themes/ColorPaletteOverride.xaml` and consumed via `{ThemeResource …}`. **No hardcoded hex values anywhere outside this file.**

| Token | Hex | Usage |
|---|---|---|
| `PaperBrush`         | `#FFFFFF` | Page surface (the inset white sheet). |
| `Paper2Brush`        | `#FAFAFA` | Suggestion card tint. Foundation panel rests on this. |
| `Paper3Brush`        | `#F4F4F5` | Page background (outside the inset sheet). Artifact body pre. Disabled chip background. |
| `InkBrush`           | `#18181B` | Primary text. Solid Apply button background. Selected chip background. Status glyph filled state. |
| `Ink2Brush`          | `#3F3F46` | Secondary text (USER prose, composer body when not the current turn, ghost button text). |
| `Ink3Brush`          | `#71717A` | Mono labels (eyebrows, status text), placeholder body text. |
| `Ink4Brush`          | `#A1A1AA` | Tertiary — empty-state text, disabled foreground, planned-status circle stroke. |
| `HairlineBrush`      | `#E4E4E7` | Every divider, every default border, every chip outline when inactive. |

Note: Material theme resources are loaded but their semantic colors (`PrimaryBrush`, `OnPrimaryBrush`, `SurfaceBrush`, etc.) are remapped to the ink/paper tokens in `ColorPaletteOverride.xaml`. The brand "Apply" button uses `InkBrush` as its background — the only UI element that registers as a saturated color block.

```xml
<!-- Themes/ColorPaletteOverride.xaml — excerpt -->
<ResourceDictionary xmlns="...">
    <Color x:Key="PaperColor">#FFFFFF</Color>
    <Color x:Key="Paper2Color">#FAFAFA</Color>
    <Color x:Key="Paper3Color">#F4F4F5</Color>
    <Color x:Key="InkColor">#18181B</Color>
    <Color x:Key="Ink2Color">#3F3F46</Color>
    <Color x:Key="Ink3Color">#71717A</Color>
    <Color x:Key="Ink4Color">#A1A1AA</Color>
    <Color x:Key="HairlineColor">#E4E4E7</Color>

    <SolidColorBrush x:Key="PaperBrush"    Color="{StaticResource PaperColor}" />
    <SolidColorBrush x:Key="Paper2Brush"   Color="{StaticResource Paper2Color}" />
    <SolidColorBrush x:Key="Paper3Brush"   Color="{StaticResource Paper3Color}" />
    <SolidColorBrush x:Key="InkBrush"      Color="{StaticResource InkColor}" />
    <SolidColorBrush x:Key="Ink2Brush"     Color="{StaticResource Ink2Color}" />
    <SolidColorBrush x:Key="Ink3Brush"     Color="{StaticResource Ink3Color}" />
    <SolidColorBrush x:Key="Ink4Brush"     Color="{StaticResource Ink4Color}" />
    <SolidColorBrush x:Key="HairlineBrush" Color="{StaticResource HairlineColor}" />
</ResourceDictionary>
```

## Typography

Two type families, both variable-weight web fonts embedded in `Assets/Fonts/`:

- **Fraunces** (variable, with italic axis) — body prose, headlines. Optical size `9pt` to `144pt`. Weights used: 300 (placeholder text), 400 (body, headlines). Italic for user-quoted prose, page subtitle, freeText hints, and the editorial headline.
- **Martian Mono** (variable) — labels, code, status. Weights used: 400 (status row), 500 (mono labels, button text), 600 (callout layer label).

Reference both in `App.xaml`:

```xml
<FontFamily x:Key="SerifFontFamily">ms-appx:///Assets/Fonts/Fraunces-VariableFont.ttf#Fraunces</FontFamily>
<FontFamily x:Key="MonoFontFamily">ms-appx:///Assets/Fonts/MartianMono-VariableFont.ttf#Martian Mono</FontFamily>
```

### Text styles

Every visible run of text uses a named style from `Themes/Typography.xaml`. **Do not set `FontSize`, `FontFamily`, or `FontWeight` inline.**

| Style | Family | Size | Weight | Style | Letter spacing | Color | Used by |
|---|---|---|---|---|---|---|---|
| `EditorialHeadlineTextStyle`  | Serif | 32 | 400 | Italic | `-0.01em` | `InkBrush`  | Page title "Compose by talking it through." |
| `EmptyStateHeadlineTextStyle` | Serif | 22 | 400 | Italic | normal     | `InkBrush`  | "Name your app to start." / "{App} is ready to compose." |
| `MessageBodyTextStyle`        | Serif | 16 | 400 | Regular | normal    | `InkBrush`  | COMPOSER message body and post line. |
| `UserBodyTextStyle`           | Serif | 16 | 400 | Italic  | normal    | `Ink2Brush` | USER message body. |
| `SubtitleTextStyle`           | Serif | 14 | 400 | Italic  | normal    | `Ink3Brush` | Header subtitle, reasoning prose, free-text hint, compact composer turn label. |
| `SuggestionLabelTextStyle`    | Serif | 18 | 400 | Regular | normal    | `InkBrush`  | The recommendation label inside the suggestion card. |
| `AlternativeLabelTextStyle`   | Serif | 14 | 400 | Regular | normal    | `InkBrush`  | Each alternative button label. |
| `MessagePostTextStyle`        | Serif | 16 | 400 | Regular | normal    | `InkBrush`  | Composer follow-up question. |
| `EmptyStateBodyTextStyle`     | Serif | 14 | 400 | Regular | normal    | `Ink2Brush` | Empty state instruction paragraph. |
| `MonoLabelTextStyle`          | Mono  | 10 | 500 | Regular | `0.18em`  | `Ink3Brush` | All-caps role labels, eyebrows, foundation field labels. |
| `MonoLabelInkTextStyle`       | Mono  | 10 | 500 | Regular | `0.18em`  | `InkBrush`  | The same, but darker — used for active state (USER / COMPOSER speaker labels). |
| `MonoTimestampTextStyle`      | Mono  | 9  | 400 | Regular | `0.16em`  | `Ink4Brush` | Per-turn timestamps. |
| `MonoStatusTextStyle`         | Mono  | 9  | 400 | Regular | `0.18em`  | `Ink2Brush` | Drafted / Planned status text on artifact card. (`Ink4Brush` when planned.) |
| `MonoFilenameTextStyle`       | Mono  | 11 | 500 | Regular | `0.04em`  | `InkBrush`  | Artifact card filename. (`Ink3Brush` when planned.) |
| `CalloutLayerTextStyle`       | Mono  | 10 | 600 | Regular | `0.20em`  | `InkBrush`  | Callout layer name. |
| `CalloutStatusTextStyle`      | Mono  | 9  | 400 | Regular | `0.18em`  | `Ink2Brush` | Callout status. |
| `CalloutNotesTextStyle`       | Mono  | 10 | 400 | Regular | `0.04em`  | `Ink3Brush` | Callout notes line. |
| `MonoButtonTextStyle`         | Mono  | 10 | 500 | Regular | `0.16em`  | (varies)    | Apply button, Show alternatives, etc. |
| `MonoChipTextStyle`           | Mono  | 10 | 500 | Regular | `0.14em`  | (varies)    | Platform chip text label, runtime chip. |
| `MonoEyebrowTextStyle`        | Mono  | 9  | 500 | Regular | `0.20em`  | `Ink3Brush` | "SUGGESTED", "ALTERNATIVES", "OR PROVIDE YOUR OWN ASSETS". |
| `MonoFieldLabelTextStyle`     | Mono  | 8.5| 500 | Regular | `0.18em`  | `Ink3Brush` | AssetField labels (Figma URL, Prototype URL, Screenshots). |
| `MonoHelpTextStyle`           | Mono  | 9  | 400 | Regular | `0.14em`  | `Ink4Brush` | "Enter to send · Shift+Enter for newline" |

All `letter-spacing` values map to XAML `CharacterSpacing` in units of `1/1000em`: `0.18em` → `CharacterSpacing="180"`. `0.14em` → `140`. `0.20em` → `200`. `0.16em` → `160`. `0.04em` → `40`. `-0.01em` → `-10`.

All-caps labels: set `Typography.Capitals="AllPetiteCaps"` is *wrong* — these are uppercase-by-content. The text in resources is already uppercase or wrapped with `ToUpper()` in a converter. Do not rely on a CSS `text-transform: uppercase`.

## Spacing scale

Tokens in `Themes/Spacing.xaml` (or inline as Doubles in `App.xaml`):

```
SpaceTight2  = 2
SpaceTight4  = 4
Space6       = 6
Space8       = 8
Space10      = 10
Space12      = 12
Space14      = 14
Space16      = 16
Space20      = 20
Space22      = 22
Space24      = 24
Space28      = 28
Space32      = 32
Space36      = 36
Space40      = 40
Space48      = 48
Space64      = 64
Space80      = 80
```

Use multiples of 4. Avoid arbitrary values. The two outliers are `22` (suggestion card horizontal padding) and `36` (transcript and panel side padding) — keep them as named tokens, don't substitute.

## Layout

### Page

- Body background: `Paper3Brush`. Centers a single inset sheet.
- Sheet: `MaxWidth="1100"`, `Padding="40,32"` from the page edges. Sheet background `PaperBrush`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="4"`. Subtle shadow: `Translation="0,0,12"` with a `ThemeShadow`.
- Sheet `MinHeight` is `viewport - 80px`. On smaller viewports the sheet still fills.
- The sheet animates in on first mount with `pageIn` (see INTERACTIONS.md): translateY 12 → 0, opacity 0 → 1, 600ms cubic-bezier `(0.16, 1, 0.3, 1)`.

### Header

- `Padding="36,28,36,20"`, `BorderThickness="0,0,0,1"`, `BorderBrush="HairlineBrush"`.
- Two columns via `Grid`: left flex content, right `Auto` content. `VerticalAlignment="Bottom"` for both.
- Left content (top to bottom): mono eyebrow `"PLATE III · THE CONVERSATIONAL COMPOSER"`, then `EditorialHeadlineTextStyle` headline, then italic subtitle 14px.
- Right content (one row, gap `Space16`): drafted-count mono label `"00 / 08 DRAFTED"` with `FontFeatureSettings="tnum"` for tabular numerals, then a `Reset` text button — underlined (offset 3px, color `HairlineBrush`), hover transitions both color and decoration to `InkBrush`.

### Two-column body

- `Grid` with `ColumnDefinitions="*, 380"`. The right column is fixed `380px`; the left column flexes.
- `BorderThickness="1,0,0,0"` on the right column from the perspective of the left column's right edge — implement as `BorderBrush="HairlineBrush"` on the right child's left edge.
- `MinHeight="0"` on both children (so they can overflow with their own scroll).

### Left column (transcript)

- `Padding="36,0"` horizontal. No top/bottom padding — the first message provides its own top spacing.
- Vertical layout:
  - Scrollable region (transcript) with `flex: 1` (use a `Grid.Row="0"` that fills).
  - Footer `InputBox` pinned to the bottom, `Padding="36,16,36,28"`, with a subtle `BorderThickness="0,1,0,0"` divider.
- Custom scrollbar: `6px` wide thumb, color `HairlineBrush`, hover `Ink4Brush`, transparent track. Implement with `ScrollViewer` style override.

### Right column (foundation + live build)

- `Padding="32,32,32,28"`. Vertical layout, gap `Space24` between sections.
- **Foundation panel** (top section, `Paper2Brush` background, `BorderThickness="1"`, `HairlineBrush`, `CornerRadius="4"`, `Padding="20,18"`):
  - Mono label "APP NAME *" (asterisk in `InkBrush`).
  - TextBox bound to `AppName`. Inside the TextBox: serif 16, italic when empty, normal when filled.
  - Mono label "PLATFORMS *", `MarginTop="14"`.
  - WrapPanel (or AutoLayout horizontal flow) of `PlatformChip` for Web / Windows / Android / iOS / Desktop. Default selection: Web, Android, iOS.
  - Mono label ".NET RUNTIME *", `MarginTop="14"`.
  - Horizontal AutoLayout of two `RuntimeChip`: `.NET 10` (REC) and `.NET 9`. Default `.NET 10`.
  - **Begin button** appears below the runtime chips when the foundation is valid AND the chat is empty.
- **Live build panel** (artifact list):
  - Mono eyebrow "LIVE BUILD" with right-aligned drafted count.
  - Vertical stack of `ArtifactCard` for all 8 artifacts in registry order: README.md, CLAUDE.md, .mcp.json, DESIGN.md, INTERACTIONS.md, ARCHITECTURE.md, implementation-plan.md, scaffold.sh.
  - Below the cards, the **Download bundle** primary action — appears when `DraftedCount == 8`.

## Component visual specs

### PlatformChip

Implements a shrink-to-icon morph between unselected (text label) and selected (filled circular icon).

**Unselected state:**
- Pill: `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `Background="Transparent"`, `CornerRadius="999"`, `Padding="0,12"` (height-driven via padding only).
- Children layered: text run (visible) + icon span (collapsed to 0 width, opacity 0).
- Text: `MonoChipTextStyle`, color `Ink2Brush`.
- Hover: border to `InkBrush`, text to `InkBrush`. 180ms ease-out on both.

**Selected state:**
- Same pill becomes a `28×28` circle: `Width=28`, `Height=28`, `Background="InkBrush"`, `BorderBrush="InkBrush"`, `Padding="0"`.
- Text run collapses to 0 width with opacity fade.
- Icon span expands to 14×14, opacity 1, color `PaperBrush` (white on ink).
- Transition timing: `MaxWidth` 360ms `cubic-bezier(0.16, 1, 0.3, 1)`, opacity 240ms ease-out delayed 100ms, scale (0.6 → 1) 320ms `cubic-bezier(0.16, 1, 0.3, 1)` delayed 100ms.

**Implementation notes for Uno:**
- Use a `ContentControl` with two children in a single `Grid` (text + icon overlapping with `Visibility` and animated via Storyboards, or both visible with animated `Width` and `Opacity`).
- Use `VisualStateManager` with `Unselected` and `Selected` states. Wire to `IsSelected` dependency property.
- Storyboards live in `PlatformChip.xaml`'s `Style` resource.
- Icon paths: see "Platform icons" below.

**Accessibility:** `AutomationProperties.Name="{x:Bind PlatformName}"`, `IsToggle="True"` semantically. The chip is a button with toggle behavior.

### Platform icons

Each is a 14×14 SVG, single-color (`currentColor`). In XAML, use `Path` elements with `Stroke="{TemplateBinding Foreground}"` so they invert with state. Path data lifted directly from prototype:

| Platform | Path |
|---|---|
| Web      | Circle `cx=7 cy=7 r=5.5` stroke `1.2` + horizontal line `(1.5,7)→(12.5,7)` stroke `1` + two arc paths forming the meridian `M 7 1.5 C 4 4 4 10 7 12.5` and `M 7 1.5 C 10 4 10 10 7 12.5` stroke `1`. |
| Windows  | Four 4.5×4.5 squares: `(2,2)`, `(7.5,2)`, `(2,7.5)`, `(7.5,7.5)`. Filled `currentColor`. |
| Android  | Two antenna lines `(4,3.4)→(3.2,2.2)` and `(10,3.4)→(10.8,2.2)` stroke `1.1` round caps + half-dome path `M 2.5 6.2 C 2.5 4.3 4.5 3 7 3 C 9.5 3 11.5 4.3 11.5 6.2 L 11.5 10 L 2.5 10 Z` filled. |
| iOS      | Rounded rect `(4,1.5,6,11)` rx `1.2` stroke `1.2` + speaker line `(6,11)→(8,11)` stroke `1.1` round caps. |
| Desktop  | Rounded rect `(1.5,2.5,11,7)` rx `0.6` stroke `1.2` + base line `(5,12)→(9,12)` + stem `(7,9.5)→(7,12)`. |

Render as `Microsoft.UI.Xaml.Shapes.Path` with explicit `Data` strings or as `PathIcon` controls in `Themes/PlatformIcons.xaml`.

### RuntimeChip

Single-select pill, two options:
- `.NET 10` (recommended — shows a tiny `REC` micro-label inline)
- `.NET 9`

**Unselected state:**
- `Padding="6,12"`, `CornerRadius="999"`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `Background="Transparent"`.
- Text: `MonoChipTextStyle`, color `Ink2Brush`.
- REC micro-label: 8px mono, `0.18em` letter-spacing, opacity 0.55, weight 400, no transform (already uppercase).

**Selected state:**
- `Background="InkBrush"`, `BorderBrush="InkBrush"`, text color `PaperBrush`.
- REC micro-label opacity 0.7.

200ms ease-out on all properties.

### ArtifactCard

A row that opens to reveal content. The row itself never animates the chevron when undrafted (chevron is hidden when planned and the row is non-interactive when planned).

**Header row:**
- Container: `BorderThickness="0,1,0,0"` `HairlineBrush`, `Padding="0,14"`. The card has no left/right padding — it inherits the panel padding.
- Left: `StatusGlyph` (14×14), gap `Space8`, then filename in `MonoFilenameTextStyle`.
- Spacer (`*` column).
- Right: `Drafted` / `Planned` text in `MonoStatusTextStyle`. When drafted, also a `Chevron` (10×10) rotated by `expanded` state.
- The whole row is a `Button` (no chrome, transparent background). When planned, `IsEnabled="False"` and `Cursor="Default"`. When drafted, click toggles expanded.

**Body (when expanded):**
- Indent: `Margin="22,0,0,0"` from the card's left edge (matches the glyph + gap offset so content aligns under the filename).
- Two states:
  - **Drafted** → a `<pre>`-equivalent: a TextBox with `IsReadOnly="True"`, `AcceptsReturn="True"`, `TextWrapping="Wrap"`. Mono 10.5px, color `Ink2Brush`, line-height 1.55, `Background="Paper3Brush"`, `BorderBrush="HairlineBrush"`, `BorderThickness="1"`, `CornerRadius="3"`, `Padding="12,10"`. Hovering changes border to `Ink3Brush` and background to `Paper2Brush` over 180ms.
  - **Drafted + edit mode** → swap to an editable TextBox. `Background="PaperBrush"`, `BorderThickness="1"`, `BorderBrush="InkBrush"`, `MinHeight="120"`, `AcceptsReturn="True"`, `TextWrapping="Wrap"`, autoresize vertical. On blur, commit. `Esc` cancels, `Cmd/Ctrl+Enter` commits.
  - **Planned + expanded** → italic placeholder: serif 12, `Ink4Brush`, `Background="Paper3Brush"`, `BorderThickness="1"` *dashed*, `BorderBrush="HairlineBrush"`, `CornerRadius="3"`, `Padding="14"`. Body text: "Awaits your decision in the chat."
- Edit hint: a small mono "CLICK TO EDIT" label, 8px, `Ink4Brush`, top-right of the pre, `Margin="0,6,8,0"`. Opacity 0 by default; opacity 1 on hover of the pre. 180ms transition.

**Body expansion animation:**
- The body is wrapped in a `Grid` whose `RowDefinition` height animates between `0` and `Auto` via `gridTemplateRows: 0fr ↔ 1fr`. In Uno, the closest equivalent is animating the `Grid.RowDefinition.MaxHeight` between `0` and a measured natural height, OR using a `Border` with `MaxHeight` Storyboard. Duration 320ms ease-out. The inner content is wrapped in a `Border` with `ClipToBounds="True"` (or `Clip` geometry) so children don't bleed during the animation.

### StatusGlyph

A 14×14 vector glyph that animates from an outlined empty circle (planned) to a filled black circle with an inscribed white check (drafted).

- Outer circle: `cx=7 cy=7 r=6`, `StrokeThickness=1`. Properties:
  - Planned: `Fill="Transparent"`, `Stroke="Ink4Brush"`.
  - Drafted: `Fill="InkBrush"`, `Stroke="InkBrush"`.
  - 320ms ease-out on both `Fill` and `Stroke`.
- Inner check path: `M 4 7.4 L 6 9.2 L 10 5.2`, `StrokeThickness=1.5`, `StrokeStartLineCap="Round"`, `StrokeEndLineCap="Round"`, `StrokeLineJoin="Round"`, `Stroke="PaperBrush"`. Animated via `StrokeDashArray="12"` and `StrokeDashOffset` from `12` → `0`. Duration 380ms `cubic-bezier(0.16, 1, 0.3, 1)`, **delayed 140ms** so the circle fills first, then the check draws.

The 140ms delay is essential — without it the glyph reads as a binary swap instead of a complete-the-task animation.

In Uno, use `Path` with `StrokeDashArray` and animate `StrokeDashOffset` via Storyboard. The dash trick works on Skia renderer.

### Chevron

10×10 vector. Path `M 2.5 4 L 5 6.5 L 7.5 4`, `StrokeThickness=1.2`, round caps. Color `Ink3Brush` by default. Rotates 0° → 180° on `expanded`. 240ms ease-out.

### Callout

Inline build-state callout inside a composer message body. Visual:
- `BorderThickness="2,0,0,0"`, `BorderBrush="InkBrush"`, `Padding="14,0,0,0"` (left padding only, after the rule).
- Vertical layout, gap `Space2`.
- First row (horizontal, baseline-aligned, gap `Space8`):
  - Layer name in `CalloutLayerTextStyle`.
  - Tiny dot: 6×6 circle, `Fill="InkBrush"`, `CornerRadius="999"`.
  - Status in `CalloutStatusTextStyle`.
- Second row: notes in `CalloutNotesTextStyle`.
- Vertical margin: `10,0` (10px top and bottom).

### CompactComposerTurn

Locked-in composer turn rendered as a single horizontal row.

- `BorderThickness="0,1,0,0"` `HairlineBrush`, `Padding="0,14"`.
- Horizontal layout, vertical center, gap `Space12`:
  - 14×14 filled-circle-with-check icon (same as drafted `StatusGlyph` but always drafted).
  - Center column (`*` width, `MinWidth=0`):
    - Layer label in `MonoLabelTextStyle` (e.g. "DESCRIPTION", "WIRING", "DESIGN SYSTEM", "INTERACTIONS", "ARCHITECTURE", "PLAN").
    - Applied label in serif 14, `InkBrush`, single line `TextTrimming="CharacterEllipsis"`. If `appliedLabel` is missing, fallback to "Locked in".
  - **Edit pill** button: ghost button, mono 9, `0.18em` letter-spacing, `Padding="10,5"`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="999"`. Inline pencil glyph 9×9 (path `M 6 2 L 1.5 6.5 L 1 8 L 2.5 7.5 L 7 3 Z M 6 2 L 7 1 L 8 2 L 7 3`, stroke `1`, line-join round). Hover: border + text → `InkBrush`. 180ms.

### MessageBlock (full)

A full speaker turn (USER or COMPOSER, when latest).

- `BorderThickness="0,1,0,0"` `HairlineBrush`, `Padding="0,24"`.
- Header row: speaker label (`MonoLabelInkTextStyle` — "USER" or "COMPOSER"), gap `Space10`, timestamp in `MonoTimestampTextStyle`. `Padding="0,0,0,8"`.
- Body paragraph: serif 16, color `InkBrush` for COMPOSER or `Ink2Brush` italic for USER. `LineHeight="1.55"` (use `LineHeight` property). Preserve newlines (`TextWrapping="Wrap"` and explicit linebreaks).
- Optional callouts list (vertical stack, `MarginTop="12"`).
- Optional post line (serif 16, `InkBrush`, `MarginTop="12"`).
- Optional `SuggestionPanel` below (see next section).

Mount animation: opacity 0 → 1 + translateY 8 → 0, 480ms `cubic-bezier(0.16, 1, 0.3, 1)`. Triggered when this is the just-added message (the `JustAddedId` MVUX state matches).

### SuggestionPanel

The decision card. Visually distinct from the chat — tinted background, contained padding, the focal element of every composer turn.

- Outer container: `Background="Paper2Brush"`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="6"`, `Padding="22,20,22,18"`.
- Vertical content stack:
  1. **Eyebrow row**: mono "SUGGESTED" eyebrow (`MonoEyebrowTextStyle`) + a horizontal hairline rule extending to the right (`Background="HairlineBrush"`, `Height="1"`). Gap `Space8` between. `MarginBottom="8"`.
  2. **Suggestion label** in `SuggestionLabelTextStyle`. `MarginBottom="12"`.
  3. **Reasoning paragraph** (when present): italic serif 14, `Ink2Brush`, line-height 1.55, with a vertical hairline on the left (`BorderThickness="2,0,0,0"`, `BorderBrush="Ink4Brush"`, `Padding="12,0,0,0"`). `Margin="0,0,0,16"`.
  4. **Action row** (horizontal, gap `Space8`, wrap): the **Apply** primary button + (when alternatives or asset inputs exist) a **Show alternatives** ghost button.
  5. **Collapsible panel** for alternatives + assets (see below).
- Outside the card, an italic hint in serif 13, `Ink4Brush`, `MarginTop="12"`, `Padding="2,0"` — the `freeTextHint` ("Or list the servers you want.", etc.).

**Apply button** (primary):
- `Background="InkBrush"`, `Foreground="PaperBrush"`, `BorderThickness="1"`, `BorderBrush="InkBrush"`, `CornerRadius="999"`, `Padding="16,9"`. `MonoButtonTextStyle`. Disabled state: `Opacity="0.4"`, no pointer events.
- Subtle press transform: `Scale(0.98)` for 140ms ease-out on `PointerPressed`.

**Show alternatives button** (ghost):
- `Background="Transparent"`, `Foreground="Ink2Brush"`, `BorderThickness="1"`, `BorderBrush="Ink3Brush"`, `CornerRadius="999"`, `Padding="14,9"`. `MonoButtonTextStyle`.
- Inline chevron 9×9 rotates 0° → 180° on open, 220ms ease-out.
- Label text varies:
  - When closed and only alternatives exist: "Show alternatives".
  - When closed and asset inputs also exist (design layer): "Show alternatives & assets".
  - When open: "Hide options".

**Collapsible alternatives panel**:
- Wrapped in a `MaxHeight`-animated `Border` (320ms ease-out, see ArtifactCard body for the same pattern).
- Inside: `MarginTop="16"`, `BorderThickness="0,1,0,0"`, `HairlineBrush`, `PaddingTop="16"`.
- Sections:
  - **Alternatives**: eyebrow "ALTERNATIVES", then a vertical stack (gap `Space6`, `MarginBottom="18"` if assets follow else `0`) of full-width alternative buttons.
  - **Asset inputs** (design layer only): eyebrow "OR PROVIDE YOUR OWN ASSETS", then three `AssetField`s (Figma URL, Prototype URL, Screenshots — the last is multiline 2 rows), then an "Apply with these assets" button.

**Alternative button**:
- Full-width, left-aligned. `Background="PaperBrush"`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="4"`, `Padding="14,10"`. Label in `AlternativeLabelTextStyle`.
- Hover: border → `Ink2Brush`, background → `Paper3Brush`. 180ms ease-out.

**Apply with these assets button**:
- Same shape as Apply but smaller `Padding="14,8"`. Disabled (gray) when no asset field is non-empty: `Background="Paper3Brush"`, `Foreground="Ink4Brush"`, `BorderBrush="HairlineBrush"`. When at least one asset is provided: same as primary Apply (`InkBrush` solid). 220ms ease-out on transition.

### AssetField

Labeled input. Vertical layout, gap `Space2`:
- Label: `MonoFieldLabelTextStyle` (8.5px mono, `0.18em`, `Ink3Brush`).
- TextBox:
  - Single-line variant: `MonoChipTextStyle`-like font (mono 12), `Background="PaperBrush"`, `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="3"`, `Padding="10,7"`. On focus: border → `InkBrush`. 180ms ease-out.
  - Multi-line variant: serif 13, `AcceptsReturn="True"`, `MinHeight=2 rows`, `MaxHeight=` reasonable cap, vertical resize allowed (or auto-grow up to a cap).

### InputBox

Persistent footer composer input.

- Container: `Padding="36,16,36,28"`, `Background="PaperBrush"`. `BorderThickness="0,1,0,0"`, `BorderBrush="HairlineBrush"` separates from the transcript.
- When `value` is empty AND there are suggestion chips (only on the very first turn — the description prompt exposes example prompts), show a wrap-row of chips with mono labels. Gap `Space6`. Hover changes border + text to `InkBrush`. (See "PROMPT_SUGGESTIONS" in INTERACTIONS.md.)
- Input row: `Border` `BorderThickness="1"`, `BorderBrush="HairlineBrush"`, `CornerRadius="4"`, `Padding="12"`. Inside, horizontal flex with gap `Space12`:
  - Multi-line `TextBox`, `AcceptsReturn="True"`, `TextWrapping="Wrap"`, `MinHeight="36"`, 2 rows starting. Border-less, transparent background. Serif 16, italic when empty. Placeholder "Continue the composition…" in `Ink4Brush`.
  - **Send button**: ghost when empty/disabled (`Background="Transparent"`, `Foreground="Ink4Brush"`, `BorderBrush="HairlineBrush"`), primary when ready (`Background="InkBrush"`, `Foreground="PaperBrush"`, `BorderBrush="InkBrush"`). `CornerRadius="999"`, `Padding="14,7"`. Mono label "SEND" + arrow `→` 1px translateY -1.
- On focus of the input row: outer border → `InkBrush`. 180ms ease-out.
- Below the row: tiny mono help "ENTER TO SEND · SHIFT+ENTER FOR NEWLINE" in `MonoHelpTextStyle`, `MarginTop="8"`.

### ThinkingIndicator

Composer is "composing…" placeholder.

- `BorderThickness="0,1,0,0"` `HairlineBrush`, `Padding="0,24"`.
- Header row: "COMPOSER" speaker label + "composing…" mono 9 in `Ink4Brush`. Gap `Space10`. Bottom margin 8.
- Three pulsing dots: 5×5 circles, `CornerRadius="999"`, `Background="Ink3Brush"`. Animation `dotPulse` (see INTERACTIONS.md): opacity 0.3 → 1 → 0.3, scale 1 → 1.25 → 1, 1100ms infinite. Delays: 0ms, 160ms, 320ms.

### Empty state (no messages yet)

When `Messages` is empty, the transcript shows:
- Mono eyebrow: `"BEGIN"` or `"FOUNDATION SET"` based on whether foundation is valid.
- Headline 22px italic serif: `"Name your app to start."` or `"{appName} is ready to compose."`
- Body paragraph 14px serif, line-height 1.55, color `Ink2Brush`. Two variants — pre-foundation vs. foundation-set. Copy:
  - Pre: "Set the app name, pick your platforms, and write a one-line description in the live build panel. I'll draft README, CLAUDE, and scaffold.sh as you type — then we'll handle the rest in conversation."
  - Foundation-set: "Click \"Begin composing\" in the live build panel to wire the agent. From there I'll guide you through MCP servers, design system, architecture, and implementation plan — each step drafts the relevant artifact instantly."
- Top padding `80`, max width `480`.

### Begin button

- Below the runtime chips in the foundation panel.
- Mono 10, `0.18em` letter-spacing, weight 500.
- `Padding="14,10"`, `Background="InkBrush"`, `Foreground="PaperBrush"`, `BorderBrush="InkBrush"`, `CornerRadius="999"`. `MarginTop="14"`.
- Label: "BEGIN COMPOSING".
- Only visible when `FoundationReady && Messages.Count == 0`. Hidden once a conversation has started.

### Download bundle button (terminal action)

- Below the artifact panel, full-width primary button.
- Same primary button style as Apply but `Padding="14,12"`.
- Label: "DOWNLOAD COMPOSITION (.MD)".
- Visible when `DraftedCount == 8` (all artifacts drafted).
- Click triggers `BundleExporter.ExportAsync()` which serializes all 8 artifacts into a single fenced markdown file `golden-path-composition.md`. See ARCHITECTURE.md for the format.

## Layout grid (responsive)

The interface assumes desktop browser usage at 1280+ width. Below that:

- 905-1280: same layout, just narrower transcript column. Right column stays at 380px.
- 600-904: collapse to single column. Foundation panel + live build move to a top section (collapsible header), transcript fills the rest. Use `ResponsiveView` from Uno Toolkit:
  ```xml
  <utu:ResponsiveView>
    <utu:ResponsiveView.NarrowTemplate>...</utu:ResponsiveView.NarrowTemplate>
    <utu:ResponsiveView.NormalTemplate>...</utu:ResponsiveView.NormalTemplate>
  </utu:ResponsiveView>
  ```
- Below 600: not officially supported. The composer is a desktop tool first.

The prototype is desktop-only. This breakpoint behavior is forward-looking — implement only after parity with desktop is achieved.

## Iconography

All icons in the app are inline `Path` shapes with explicit `Data` strings. **No icon font, no PNGs, no SVG files.** This keeps the visual identity consistent and avoids font-loading delays in WASM.

- Status glyph: see above.
- Platform icons: see above.
- Pencil (Edit pill): see CompactComposerTurn.
- Chevrons (ArtifactCard, Show alternatives): see Chevron.
- Send arrow: literal `→` character, no path.

## Things that are deliberately not in the design

- **No avatars.** Speakers are distinguished by mono labels above their turn.
- **No emoji.** Tone is editorial, not casual.
- **No theme toggle.** This is a single-mode interface; light theme only.
- **No drop shadows on internal elements.** Only the outer page sheet has a subtle shadow.
- **No primary color.** The only saturated element is the dark Apply button.
- **Pointer-focus has no outline** — focus arrived via mouse uses the same hairline → ink transition as hover. Keyboard focus is the exception (see "Focus visuals" below).
- **No tab strips, no breadcrumbs, no progress bars.** Progress is communicated only via the `00 / 08 DRAFTED` count and the artifact panel's status glyphs.

---

## Visual states (the behavioral details are in INTERACTIONS.md — this section covers what each state *looks like*)

### Disabled state visuals

Three treatments, applied per the rule in INTERACTIONS.md:

**Hidden** — `Visibility=Collapsed`. No layout space reserved. Examples: `Begin Composing` when foundation invalid, `Download bundle` when not all 8 drafted.

**Ghost-disabled** — control is visible but inert:
- `Background=Transparent` (or `Paper3Brush` for filled controls).
- `Foreground=Ink4Brush`.
- `BorderBrush=HairlineBrush`.
- `Cursor=NotAllowed`.
- No hover transitions fire.
- `IsHitTestVisible=false` so events don't propagate.
- Applied to: `Send` button when input empty, `Apply with these assets` when no asset typed, `ArtifactCard` header when planned.

**Faded-disabled** — visible at reduced opacity, used for transient blocks:
- `Opacity=0.4`.
- `IsHitTestVisible=false`.
- Border, background, foreground colors unchanged (so when re-enabled, no perceived state shift — just opacity fading back up).
- 220ms ease-out transition between enabled and disabled.
- Applied to: `Apply`, `Show alternatives`, alternative buttons, asset fields, `InputBox`, `Apply with these assets` — **only when `IsThinking` is true.**

A given control may use ghost-disabled in one context and faded-disabled in another. `Apply with these assets` is ghost-disabled when no asset is typed (always-applicable rule) and faded-disabled when the API is in flight (transient block). It transitions between both as conditions change.

### Focus visuals (keyboard)

Keyboard focus is communicated by a 2px solid `InkBrush` outline at 2px offset from the control's natural bounds. Pointer (mouse) focus does **not** show this outline — it uses the existing hover treatment.

Applied via a shared focus visual style in `Themes/FocusVisuals.xaml`:

```xml
<Style TargetType="Control" x:Key="EditorialFocusStyle">
    <Setter Property="UseSystemFocusVisuals" Value="False" />
    <Setter Property="FocusVisualPrimaryBrush" Value="{ThemeResource InkBrush}" />
    <Setter Property="FocusVisualPrimaryThickness" Value="2" />
    <Setter Property="FocusVisualSecondaryBrush" Value="Transparent" />
    <Setter Property="FocusVisualMargin" Value="-2" />
</Style>
```

Apply to every interactive control's default style by basing on or merging this. The outline is only drawn when `FocusState=Keyboard` (Uno honors this distinction).

**Where focus visuals are NOT applied:**
- TextBoxes (App name, Asset fields, InputBox, edit-mode artifact body) — the input's own border swap to `InkBrush` already communicates focus. Stacking an outline on top creates visual noise.
- The read-only artifact body pre — not focusable.
- Static text and labels.

### Pressed (active) state visuals

Pressed feedback is purely visual; the action fires on `PointerReleased`.

| Control category | Pressed visual | Duration |
|---|---|---|
| Primary buttons (Apply, Begin, Download bundle, Send-active) | `ScaleTransform 0.98` | 100-140ms ease-out |
| Ghost buttons (Show alternatives, Edit pill, Reset) | `Opacity 0.7` | 80ms ease-out |
| Cards & rows (ArtifactCard header, alternative button) | `Opacity 0.85` | 80ms ease-out |
| Chips (Platform, Runtime) | `ScaleTransform 0.97` | 100ms ease-out |

Implementation: `VisualStateManager` `PointerPressed` state on each control's template, with the appropriate Storyboard.

### Error banner visual

Surfaces above the `InputBox` when an API call or file save fails. **Only one banner at a time** — new errors replace previous.

- Container: full-width within the transcript column padding. `Background=PaperBrush`, `BorderThickness="1"`, `BorderBrush="InkBrush"`, `CornerRadius="4"`, `Padding="14,12"`, `MarginBottom="12"`.
- The ink border is the only place a 1px ink border appears as an emphasis device (everything else uses hairline) — errors deserve attention.
- Layout: horizontal AutoLayout, `Spacing="12"`, vertical center.
  - **Alert glyph** (14×14, inline `Path`): a triangle with a centered exclamation. Path data:
    ```
    M 7 1.5 L 13 12.5 L 1 12.5 Z      // outer triangle
    M 7 5.5 L 7 9                      // exclamation stem
    M 7 10.5 L 7 11                    // exclamation dot (short stroke)
    ```
    `Stroke="InkBrush"`, `StrokeThickness="1.4"`, `StrokeLineCap="Round"`, `StrokeLineJoin="Round"`, `Fill="Transparent"`.
  - **Body**: serif 14, `InkBrush`, `LineHeight="1.4"`. The error message text. Wraps if long.
  - **Spacer** (`*` width).
  - **Dismiss button** (16×16): `×` glyph, ghost button. `Foreground=Ink3Brush`, hover `→ InkBrush`. 180ms.
- Mount: reuses `msgIn` (480ms slide + fade) for visual consistency with messages.
- Auto-dismiss: 8s timer from appearance (cleared if user dismisses manually).
- Manual dismiss: opacity 0 + translateY -4, 240ms ease-out, then unmount.
- `aria-live="polite"` for screen readers (Uno: `AutomationProperties.LiveSetting="Polite"`).

### Visual state summary table (per component)

For quick reference. Each component has a row; each column is a visual state. `—` means the state doesn't apply.

| Component | Default | Hover | Focus (kb) | Pressed | Selected/Active | Disabled |
|---|---|---|---|---|---|---|
| App name TextBox | hairline border, italic placeholder | hairline | ink border | — | (typing) ink border | — |
| PlatformChip | hairline outline, text | ink border + text | 2px outline | scale 0.97 | filled circle, white icon | — |
| RuntimeChip | hairline outline | ink border + text | 2px outline | scale 0.97 | ink fill, paper text | — |
| Begin button | ink solid | shadow lift | 2px outline | scale 0.98 | — | hidden |
| Apply button | ink solid | shadow lift | 2px outline | scale 0.98 | — | faded 0.4 |
| Show alternatives | ghost (Ink2 text) | ink border + text | 2px outline | opacity 0.7 | (open) chevron 180° | faded 0.4 |
| Alternative button | paper bg, hairline | Ink2 border, paper3 bg | 2px outline | opacity 0.85 | — | faded 0.4 |
| Apply with assets | ghost (when none typed) / ink solid (when typed) | shadow lift (when active) | 2px outline | scale 0.98 | — | ghost (no assets) / faded (thinking) |
| AssetField TextBox | hairline | hairline | ink border | — | (typing) ink border | faded 0.4 |
| InputBox container | hairline border | hairline | (focus on textarea) ink border | — | — | faded 0.6 |
| Send button | ghost (empty) / ink solid (ready) | shadow lift (when ready) | 2px outline | scale 0.98 | — | ghost (always when empty) |
| Suggestion chip | hairline outline, ink3 text | ink border + text | 2px outline | — | — | — |
| ArtifactCard header (planned) | empty circle, ink3 text | — | — | — | — | inert (cursor default, ink4) |
| ArtifactCard header (drafted, collapsed) | filled circle, ink text, chevron 0° | — (cursor pointer is the affordance) | 2px outline | opacity 0.85 | — | — |
| ArtifactCard header (drafted, expanded) | same + chevron 180° | — | 2px outline | opacity 0.85 | — | — |
| Pre body (read-only) | paper3 bg, hairline | Ink3 border, paper2 bg, "CLICK TO EDIT" hint | (not focusable) | — | (edit mode) ink border, paper bg, white | — |
| Edit pill | ghost, hairline | ink border + text | 2px outline | opacity 0.85 | — | — |
| Reset (header) | ink3, hairline underline | ink, ink underline | 2px outline | opacity 0.7 | — | — |
| Download bundle | ink solid | shadow lift | 2px outline | scale 0.98 | — | hidden (when <8 drafted) |
| Error banner | paper bg, ink border | — | — | — | — | — |
| Error dismiss × | ink3 | ink | 2px outline | opacity 0.7 | — | — |
