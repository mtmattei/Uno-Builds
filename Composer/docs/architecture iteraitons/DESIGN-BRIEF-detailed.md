# Composer Context Engine — Design Brief

**Version:** v11 (canonical)
**Audience:** an implementation agent reproducing the visual system in Uno Platform XAML. Self-contained.
**Companion briefs:** `ARCHITECTURE-BRIEF-detailed.md` (structural), `INTERACTION-BRIEF-detailed.md` (behavioral). This brief is the visual truth — every token, every primitive, every pixel.

The design system is **editorial-developer**: hairlines as architecture, marginalia as voice, color discipline, JetBrains Mono only for code-shaped content, Inter for everything else. No card chrome around canvases. No drop shadows. No rounded everything. Tabular grids over cards. Live preview integration over modal dialogs.

If this brief and the reference prototype (`composer-context-engine.jsx`) disagree, the prototype wins.

---

## 1. Foundations

### 1.1 Typography stack

Two families. The discipline is what makes the workspace read as editorial-developer rather than terminal-clone.

| Family            | Where loaded                          | Use cases                                                          |
|-------------------|---------------------------------------|--------------------------------------------------------------------|
| Inter             | Google Fonts CDN (`wght@400;500;600`) | Body, headings, eyebrow labels, italic prose, all UI chrome        |
| JetBrains Mono    | Google Fonts CDN (`wght@400;500;600`) | Code, file paths, hex codes, keyboard shortcuts, type identifiers  |

XAML reference declaration (`Themes/Typography.xaml`):

```xml
<ResourceDictionary>
    <FontFamily x:Key="InterFontFamily">Inter, system-ui, -apple-system, Segoe UI, Helvetica, Arial, sans-serif</FontFamily>
    <FontFamily x:Key="MonoFontFamily">JetBrains Mono, Consolas, Courier New, monospace</FontFamily>
</ResourceDictionary>
```

JetBrains Mono is used **only** for code, file paths, hex values, keyboard shortcuts, type names in entity records, and the `↳` continuation glyph. Never for UI labels, even ones that feel "system-y."

### 1.2 Type scale (exhaustive)

Every visible TextBlock in the app falls into one of these slots. No off-scale sizes.

| Resource key                  | Family    | Size  | Weight | Tracking  | Line-height | Color (default)    | Used for                                              |
|-------------------------------|-----------|-------|--------|-----------|-------------|--------------------|-------------------------------------------------------|
| `DisplayLargeTextStyle`       | Inter     | 26    | 500    | -0.015em  | 1.20        | `Ink` (#1a1a1a)    | Active layer header titles ("What are we building?")  |
| `DisplayMediumTextStyle`      | Inter     | 18    | 600    | -0.010em  | 1.30        | `Ink`              | Markdown H1 in preview pane                           |
| `HeadingLargeTextStyle`       | Mono      | 15    | 500    | 0         | 1.40        | `Ink`              | Type-scale sample line ("Job #4471 · Boiler service") |
| `HeadingMediumTextStyle`      | Inter     | 14    | 600    | 0.020em   | 1.30        | `Ink`              | Markdown H2 in preview (uppercase, hairline underline)|
| `HeadingSmallTextStyle`       | Inter     | 12    | 600    | 0.040em   | 1.30        | `Ink2`             | Markdown H3 in preview (uppercase)                    |
| `BodyLargeTextStyle`          | Inter     | 14    | 400    | 0         | 1.55        | `Ink2`             | Lead questions, prose, locked-card summaries          |
| `BodyMediumTextStyle`         | Inter     | 13    | 400    | 0         | 1.55        | `Ink2`             | Item titles, mid-density descriptions                 |
| `BodySmallTextStyle`          | Inter     | 12    | 400    | 0         | 1.55        | `Ink3`             | Composer placeholders, captions, file row notes       |
| `BodyItalicTextStyle`         | Inter     | 13    | 400 *italic* | 0   | 1.55        | `Ink3`             | Annotation marginalia, recap bridge sentences         |
| `EyebrowLargeTextStyle`       | Inter     | 11    | 500    | 0.04em uppercase | 1.20 | `Ink3`           | Section labels ("Composer", "Files", "Live file")     |
| `EyebrowSmallTextStyle`       | Inter     | 10    | 500    | 0.04em uppercase | 1.20 | `Ink3`           | Layer index labels ("02 · UX"), file row statuses     |
| `EyebrowTinyTextStyle`        | Inter     | 9     | 500    | 0.04em uppercase | 1.20 | `Ink4`           | Source labels ("GENERATED FROM CANVAS")               |
| `MonoLargeTextStyle`          | Mono      | 12    | 400    | -0.010em  | 1.45        | `Ink2`             | Hex codes, type names, inline code                    |
| `MonoMediumTextStyle`         | Mono      | 11    | 500    | -0.010em  | 1.45        | `Ink`              | File path display, command preview                    |
| `MonoSmallTextStyle`          | Mono      | 10    | 500    | 0         | 1.40        | `Ink4`             | Keyboard shortcuts ("⌘↵ to submit"), `↳` glyph        |

Reference XAML for one style (`Themes/TextBlockStyles.xaml`):

```xml
<Style x:Key="DisplayLargeTextStyle" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{StaticResource InterFontFamily}" />
    <Setter Property="FontSize" Value="26" />
    <Setter Property="FontWeight" Value="Medium" />
    <Setter Property="CharacterSpacing" Value="-15" />   <!-- -0.015em × 1000 -->
    <Setter Property="LineHeight" Value="31" />          <!-- 26 × 1.20 -->
    <Setter Property="Foreground" Value="{ThemeResource InkBrush}" />
</Style>
```

WinUI `CharacterSpacing` is in units of 1/1000 em. `LineHeight` is absolute pixels (compute as `FontSize × line-height-ratio`, rounded to nearest 4 to keep the baseline grid).

### 1.3 Color palette (exhaustive)

Every color in the system. Hex values are normative — the prototype's JS color tokens map exactly to these brush keys.

#### 1.3.1 Surface ramp

| Brush key        | Hex       | Theme    | Use                                                       |
|------------------|-----------|----------|-----------------------------------------------------------|
| `PaperBrush`     | `#FFFFFF` | both     | Page background, primary surface                          |
| `Paper2Brush`    | `#FBFBFB` | both     | Tile background (UX flow tiles, panel insets)             |
| `Paper3Brush`    | `#F5F5F5` | both     | Active StackItem background, inline code pill background  |
| `Paper4Brush`    | `#0A0A0A` | both     | Dark CodeBlock background (XAML / scaffold command)       |

The system is light-only in v11. Dark mode is future scope — when added, the surface ramp inverts (`PaperBrush` becomes `#0F0F0F` and so on).

#### 1.3.2 Ink ramp (text)

| Brush key        | Hex       | Use                                                           |
|------------------|-----------|---------------------------------------------------------------|
| `InkBrush`       | `#1A1A1A` | Primary text — headings, active layer titles, key values      |
| `Ink2Brush`      | `#3A3A3A` | Secondary text — body paragraphs, mono inline values          |
| `Ink3Brush`      | `#737373` | Tertiary text — italic prose, eyebrow labels, subtitles       |
| `Ink4Brush`      | `#A3A3A3` | Quaternary text — captions, planned file rows, eyebrow tiny   |
| `Ink5Brush`      | `#D4D4D4` | Faintest — BlockHandle, separator dots between Eyebrows       |

#### 1.3.3 Hairlines

| Brush key         | Hex       | Use                                                         |
|-------------------|-----------|-------------------------------------------------------------|
| `HairlineBrush`   | `#ECECEC` | Primary dividers — section bottoms, locked-card tops        |
| `Hairline2Brush`  | `#F0F0F0` | Nested row dividers (recede behind `HairlineBrush`)         |
| `HairlineDkBrush` | `#1F1F1F` | Borders inside dark CodeBlocks (paper4 surfaces)            |

#### 1.3.4 Semantic colors

| Brush key         | Hex       | Use (scope is load-bearing)                                                 |
|-------------------|-----------|-----------------------------------------------------------------------------|
| `AmberBrush`      | `#C89C3F` | **Active marker only** — active StackItem border, dirty/preview badges, amber CTA tone, file `Drafted` dot |
| `AmberSoftBrush`  | `#FDF8EF` | Proposed-tint background for changed values in preview state               |
| `IndigoBrush`     | `#3D3DFF` | **State (MVUX) module color only**, plus file `Writing` dot               |
| `SuccessBrush`    | `#7A9B6E` | Interactions canvas Success state pill background                          |
| `ErrorBrush`      | `#B04534` | Interactions canvas Error state pill, semantic Warn token reference        |

Color discipline is what holds the system together. Amber appears only on:
- The active StackItem's 2px left border
- The current layer's amber dot in `ProgressIndicator`
- The "dirty/preview" badge in `ActiveLayerHeader`
- The `Generate preview →` button tone
- The file `Drafted` dot + box-shadow glow
- The `Action` swatch's 1.5px self-reference border in DesignTokenGrid
- The acknowledgment line's left rule
- The "Example values" banner background (uses `AmberSoftBrush`)
- The Interactions Offline state pill

If amber is appearing anywhere else, it's a bug.

#### 1.3.5 Implementation phase tints

Six tints scoped exclusively to `ImplementationPhaseGridCanvas`. Never used elsewhere.

| Brush key       | Hex       | Phase label  |
|-----------------|-----------|--------------|
| `Phase1Brush`   | `#B04534` | SCAFFOLD     |
| `Phase2Brush`   | `#3D6F9A` | SHELL        |
| `Phase3Brush`   | `#7D4FA0` | DOMAIN       |
| `Phase4Brush`   | `#B4567D` | SCREENS      |
| `Phase5Brush`   | `#6F8068` | STATES       |
| `Phase6Brush`   | `#C89C3F` | POLISH       |

#### 1.3.6 Interactions state colors

Scoped exclusively to `StateTransitionDiagramCanvas`. Six values, one per `StateKind`.

| Brush key                | Hex       | StateKind   |
|--------------------------|-----------|-------------|
| `StateColorDefault`      | `#1A1A1A` | Default     |
| `StateColorLoading`      | `#3D3DFF` | Loading     |
| `StateColorEmpty`        | `#737373` | Empty       |
| `StateColorError`        | `#B04534` | Error       |
| `StateColorSuccess`      | `#7A9B6E` | Success     |
| `StateColorOffline`      | `#C89C3F` | Offline     |

### 1.4 Spacing scale

Strict 4-point grid. Only these values appear in margins, padding, and gaps. Never `5`, `13`, `27`, etc.

```
4, 8, 12, 16, 24, 32, 48, 64
```

Common applications:
- Inline gaps between Eyebrow + value: `8` or `10`
- Padding inside hairline-divided sections: `12` top, `16` bottom
- Margin between locked context cards: `0` (hairline divider does the work)
- Gutter between center column and rails: `32` (right of CompositionStack, left of FilesRail)
- Page horizontal padding: `32` left, `48` right (asymmetric — more room next to FilesRail)
- Page vertical padding: `32` top, `80` bottom (room for ComposerFooter scroll)
- Focused first-screen padding: `64` top instead of `32`

### 1.5 Corner radii

Strict radius scale. Only these values appear in `CornerRadius`.

| Value  | Use                                                                |
|--------|--------------------------------------------------------------------|
| `0`    | Hairline-bounded zones (most of the workspace)                     |
| `3`    | Inline code pill in markdown preview (`<code>`)                    |
| `4`    | Toggle button group segments (Preview/Edit), inline code blocks    |
| `5`    | Suggestion chips, copy button on CodeBlock, example banner         |
| `6`    | UX flow tile cards, ComposerFooter textarea border                 |
| `22`   | State pills in StateTransitionDiagram (fully rounded, rx=22 on 44h)|

Never use `8` or `12` or other values. The strict ladder is part of the discipline.

---

## 2. Editorial primitives

Six reusable components. Every canvas uses them. Defined once in `Themes/Editorial.xaml` + corresponding code-behind for templated controls, referenced everywhere.

### 2.1 Eyebrow

Caps metadata labels. Inter 10-11px weight 500, 0.04em tracking, uppercase.

```xml
<!-- Templated control: Eyebrow.xaml -->
<UserControl x:Class="ComposerContextEngine.Views.Controls.Eyebrow">
    <TextBlock Text="{x:Bind Text, Mode=OneWay}"
               Style="{StaticResource EyebrowSmallTextStyle}"
               Foreground="{x:Bind ForegroundBrush, Mode=OneWay,
                            Converter={StaticResource BrushOrFallback}, ConverterParameter=Ink3Brush}" />
</UserControl>
```

Used for: layer indices (`02 · UX`), file labels (`README.md`), state badges (`PREVIEW`, `EDITING`, `DRAFTED`), field names, file row statuses, section headers like `Composer` and `Files`.

### 2.2 Mono

Inline code values. JetBrains Mono 11-12px, ink2 default.

```xml
<UserControl x:Class="ComposerContextEngine.Views.Controls.Mono">
    <TextBlock Text="{x:Bind Text, Mode=OneWay}"
               Style="{StaticResource MonoLargeTextStyle}"
               Foreground="{x:Bind ForegroundBrush, Mode=OneWay}" />
</UserControl>
```

Used for: hex codes (`#C89C3F`), file paths, field type names, the `↳` glyph, layer index in stack items, command preview text, version stamps.

### 2.3 Body

Paragraph prose. Inter 13-14px, ink2 default, line-height 1.55.

Used for: descriptions, summaries, item titles, locked-card prose, file row context lines.

### 2.4 SectionHeader

Replaces card-style canvas headers. **Hairline divider only, no background fill, no rounded corners.**

```xml
<!-- Reference layout -->
<Grid Padding="0,0,0,12" Margin="0,0,0,16">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <controls:Mono Grid.Column="0"
                   Text="{x:Bind Filename, Mode=OneWay}"
                   Style="{StaticResource MonoMediumTextStyle}"
                   Foreground="{ThemeResource InkBrush}" />

    <controls:Eyebrow Grid.Column="1"
                      Text="{x:Bind Badge, Mode=OneWay}"
                      Margin="12,0,0,0"
                      ForegroundBrush="{x:Bind BadgeBrush, Mode=OneWay}" />

    <ContentPresenter Grid.Column="3"
                      Content="{x:Bind Action, Mode=OneWay}" />

    <Border Grid.Row="1" Grid.ColumnSpan="4"
            Height="1"
            Background="{ThemeResource HairlineBrush}" />
</Grid>
```

Filenames render **lowercase** in the SectionHeader: `blueprint.svg`, `interaction-states.md`, `ColorPaletteOverride.xaml`. The Mono font already provides typographic distinction; uppercasing both fights the eye.

### 2.5 Annotation

Marginalia. Italic Inter behind a 1px left rule. Replaces the boxed `WhyThis` / `AgentPrompt` callouts from pre-v7 designs.

```xml
<!-- Templated control: Annotation.xaml -->
<UserControl x:Class="ComposerContextEngine.Views.Controls.Annotation">
    <Grid Margin="0,18,0,0">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0"
                Width="1"
                Background="{ThemeResource HairlineBrush}"
                Margin="0,0,16,0" />

        <StackPanel Grid.Column="1" Orientation="Vertical" Spacing="4">
            <controls:Eyebrow Text="{x:Bind Label, Mode=OneWay}"
                              ForegroundBrush="{ThemeResource Ink3Brush}" />
            <TextBlock Text="{x:Bind FormattedContent, Mode=OneWay}"
                       Style="{StaticResource BodyItalicTextStyle}" />
        </StackPanel>
    </Grid>
</UserControl>
```

Two voices controlled by a `Voice` property:
- `Voice="Rationale"` (default): italic Inter, no quote marks. Used for "Why this fits" annotations.
- `Voice="Quote"`: italic Inter wrapped in `"…"`. Used for "Agent prompt" content.

The annotation reads as a developer's margin note, not a called-out warning.

### 2.6 CodeBlock

Generated artifact preview. Light or dark theme, language-tinted.

```xml
<UserControl x:Class="ComposerContextEngine.Views.Controls.CodeBlock">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- header bar -->
            <RowDefinition Height="*" />    <!-- code body -->
        </Grid.RowDefinitions>

        <!-- Header: filename + source label + copy button -->
        <Grid Grid.Row="0" Padding="16,10" Background="{x:Bind HeaderBackground, Mode=OneWay}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <controls:Mono Grid.Column="0" Text="{x:Bind Label, Mode=OneWay}" />
            <controls:Eyebrow Grid.Column="1" Text="{x:Bind Source, Mode=OneWay}" Margin="12,0,0,0" />
            <Button Grid.Column="3" Style="{StaticResource CopyButtonStyle}" Content="Copy" />
        </Grid>

        <!-- Body: pre-formatted code with language-aware coloring -->
        <ScrollViewer Grid.Row="1" HorizontalScrollBarVisibility="Auto">
            <controls:CodeRender
                Code="{x:Bind Code, Mode=OneWay}"
                Language="{x:Bind Language, Mode=OneWay}"
                Theme="{x:Bind Theme, Mode=OneWay}" />
        </ScrollViewer>
    </Grid>
</UserControl>
```

Visual hierarchy:
- **Dark blocks** (`Theme="Dark"`, paper4 background) = "what the agent will write" (XAML, code, scaffold commands)
- **Light blocks** (`Theme="Light"`, paper2 background) = "what the project will look like" (solution trees, file structures)

XAML language tints (Dark theme):
- Elements (`<Page>`, `<Color>`) → `#E66B5C` (red)
- Attributes (`x:Key`, `Foreground`) → `#D4A959` (amber)
- String values (`"#1A1A1A"`) → `#A8C18A` (sage)
- Comments (`<!-- ... -->`) → `#9A9A9A` italic ink3

Tree language tints (Light theme):
- Paths (`├── FieldDispatch/`) → `#1A1A1A` (ink)
- Comments (`# shared`, `# iOS · Android`) → italic ink3

### 2.7 BlockHandle

Notion-style drag affordance. Small `⋮⋮` glyph in the left gutter on hover. Visual only in v11 — drag events are not wired.

```xml
<UserControl x:Class="ComposerContextEngine.Views.Controls.BlockHandle">
    <TextBlock Text="⋮⋮"
               FontFamily="{StaticResource InterFontFamily}"
               FontSize="14"
               Foreground="{ThemeResource Ink5Brush}"
               Margin="-22,0,0,0"
               Visibility="{x:Bind Visible, Mode=OneWay, Converter={StaticResource BoolToVisibility}}" />
</UserControl>
```

Used by `LockedContextCard` and reusable for other repeated rows in future versions.

### 2.8 MarkdownPreview

Renders the synthesized markdown for the active layer in the right rail. Subset implementation tuned for narrow column reading.

Handles:
- `# H1` → `DisplayMediumTextStyle`, ink, margin-bottom 12
- `## H2` → `HeadingMediumTextStyle`, uppercase, hairline underline (1px solid `HairlineBrush`), margin 16/8
- `### H3` → `HeadingSmallTextStyle`, uppercase, ink2, margin 14/6
- Paragraphs → `BodyMediumTextStyle`, ink2, margin-bottom 10
- Bulleted lists → ink2, padding-left 18, item-spacing 3
- Numbered lists → ink2, padding-left 22
- Blockquotes → italic ink3 12px behind a 2px `HairlineBrush` left rule
- Fenced code blocks → paper4 background, `#CFCFCF` foreground, mono 11px, padding 8/10, `HairlineDkBrush` border, radius 4
- Inline `**bold**` → weight 600 ink
- Inline `*italic*` → italic
- Inline `` `code` `` → mono 0.92em in paper3 pill (radius 3, padding 1×5)

Container styling:
- Background `Paper2Brush`
- 1px `HairlineBrush` border, radius 4
- Padding 14/14/6
- MaxHeight 480, vertical scrollbar visible

---

## 3. Layout primitives

### 3.1 Hairlines as architecture

Sections are demarcated by 1px hairline dividers, **not** card frames with rounded corners and shadows. The visual rhythm:

```
{ content section }
PaddingBottom: 12
BorderBottom: 1px solid HairlineBrush
MarginBottom: 16
{ next content section }
```

Cards and panels are explicitly avoided around canvases. The only places card-like containers appear:
- UX flow strip tiles (`Paper2Brush` background, 1px `HairlineBrush`, radius 6)
- Mini control gallery boxes in DesignCanvas (1px `HairlineBrush`, no fill, radius 6)
- Toggle button groups (Preview/Edit) which form a small rounded segmented control
- The CodeBlock primitive (which is a card by definition)
- The MarkdownPreview container
- The example-values banner on IntentCanvas
- The acknowledgment-line strip in ComposerFooter (left rule, no fill)

### 3.2 Tabular grids over cards

Phase plans, color tokens, data contracts, intent fields — all rendered as tabular grids with explicit column definitions. Never as individual cards.

Standard grid templates by canvas:

| Canvas                  | `Grid.ColumnDefinitions`                                  |
|-------------------------|-----------------------------------------------------------|
| IntentCanvas            | `120,*,80`  (label, value, proposed-was)                  |
| DesignTokenGrid colors  | `24,*,100,80`  (swatch, name, hex, proposed-was)          |
| DesignTokenGrid scale   | `60,*,50`  (label, sample, size)                          |
| DataContractGrid fields | `160,*`  (field name, type)                               |
| ImplementationPhaseGrid | `40,140,*,240`  (number, title, body, agent prompt)       |
| LockedContextCard facts | `repeat(2, 1fr)`  with `gap: 4px 32px`                    |

### 3.3 Page padding asymmetry

Pages are padded asymmetrically when both rails are visible:

```
ScrollViewer Padding="32,32,48,80"
                     ↑  ↑  ↑  ↑
                     L  T  R  B
```

Left = 32, Right = 48. The extra 16px on the right gives the FilesRail visual breathing room. Top = 32 (room for progress indicator), Bottom = 80 (room for ComposerFooter so it doesn't bump the bottom of the viewport).

When `RailsVisible == false`, padding switches to `48,64,48,80` and `MaxWidth` tightens to 720.

---

## 4. Workspace shell

### 4.1 Three-column scaffold

```
┌─────────────────────┬───────────────────────────────┬────────────────────────┐
│  CompositionStack   │      ActivePage region        │      FilesRail         │
│       260px         │      flex 1, max 880          │       340px            │
│                     │                                │                        │
│  sticky, top:32     │                                │  sticky, top:32        │
│  borderRight:1px    │                                │  borderLeft:1px        │
│  HairlineBrush      │                                │  HairlineBrush         │
└─────────────────────┴───────────────────────────────┴────────────────────────┘
```

In the focused first-screen state (`activeIndex == 0 && lockedIds.Count == 0`):

```
                  ┌───────────────────────────────┐
                  │       ActivePage region        │
                  │       (centered, max 720)     │
                  │       padding-top: 64          │
                  │                                │
                  └───────────────────────────────┘
```

Both rails collapse to width 0 and opacity 0. Center column tightens to 720 max-width and centers via `HorizontalAlignment="Center"`.

### 4.2 CompositionStack (left rail)

```xml
<Border BorderBrush="{ThemeResource HairlineBrush}" BorderThickness="0,0,1,0">
    <ScrollViewer Padding="0,8,16,16" Width="260">
        <StackPanel>
            <controls:Eyebrow Text="Composition Stack"
                              Style="{StaticResource EyebrowLargeTextStyle}"
                              Margin="24,0,0,4" />
            <TextBlock Text="A conversation that crystallizes into a build system."
                       Style="{StaticResource BodySmallTextStyle}"
                       Margin="24,0,0,16"
                       Foreground="{ThemeResource Ink3Brush}" />

            <ItemsRepeater ItemsSource="{x:Bind StackItems}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:StackItem />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>
        </StackPanel>
    </ScrollViewer>
</Border>
```

### 4.3 StackItem (each of 8 rows)

Per-row specs:

| Element                  | Spec                                                                     |
|--------------------------|--------------------------------------------------------------------------|
| Row outer padding        | 12 top, 16 right, 12 bottom, 24 left                                     |
| Border-left              | 2px solid — `AmberBrush` if active, `Ink2Brush` if locked, transparent if future |
| Background               | `Paper3Brush` if active, `Paper2Brush` on hover (if clickable), else transparent |
| Cursor                   | pointer if active or locked, default if future                           |
| Layer index Mono color   | `AmberBrush` if active, `Ink2Brush` if locked, `Ink4Brush` if future     |
| Layer index Mono weight  | 500                                                                       |
| Layer label color        | `InkBrush` if active or locked, `Ink3Brush` if future                    |
| Layer label weight       | 600 if active, 500 otherwise                                              |
| ✓ glyph                  | shown for locked layers only, top-right of row, `Ink3Brush`              |
| Summary line             | italic `BodySmallTextStyle`, ink2 if locked, ink3 if active or future    |
| Row opacity              | 1.0 if active or locked, 0.5 if future                                   |
| Row transition           | `opacity 240ms ease, background 140ms ease`                              |

### 4.4 FilesRail (right rail)

Flipped order from earlier versions — preview at top, file list at bottom.

```xml
<Border BorderBrush="{ThemeResource HairlineBrush}" BorderThickness="1,0,0,0">
    <ScrollViewer Padding="24,8,16,16" Width="340">
        <StackPanel Spacing="0">

            <!-- 1. Live file panel (or Full bundle view at scaffold) -->
            <controls:LiveFilePanel />

            <!-- 2. Files list and status -->
            <Border Padding="0,18,0,0" Margin="0,32,0,0"
                    BorderBrush="{ThemeResource HairlineBrush}"
                    BorderThickness="0,1,0,0">
                <StackPanel Spacing="0">
                    <controls:Eyebrow Text="Files" />
                    <TextBlock Text="Each layer emits files as it locks."
                               Style="{StaticResource BodySmallTextStyle}"
                               Margin="0,4,0,12" />
                    <ItemsRepeater ItemsSource="{x:Bind FileRows}">
                        <ItemsRepeater.ItemTemplate>
                            <DataTemplate>
                                <controls:FileRow />
                            </DataTemplate>
                        </ItemsRepeater.ItemTemplate>
                    </ItemsRepeater>
                </StackPanel>
            </Border>

            <!-- 3. Locked-count status -->
            <Border Padding="0,12,0,0" Margin="0,14,0,0"
                    BorderBrush="{ThemeResource HairlineBrush}"
                    BorderThickness="0,1,0,0">
                <StackPanel Spacing="4">
                    <controls:Eyebrow Text="{x:Bind LockedCountText, Mode=OneWay}" />
                    <TextBlock Text="{x:Bind StatusContext, Mode=OneWay}"
                               Style="{StaticResource BodyItalicTextStyle}" />
                </StackPanel>
            </Border>

        </StackPanel>
    </ScrollViewer>
</Border>
```

### 4.5 FileRow

Per-row specs (40px tall):

| Property             | Value                                                                |
|----------------------|----------------------------------------------------------------------|
| Layout               | `Grid` with columns `Auto,*,Auto`, padding 0/7                       |
| Status dot           | 7×7 ellipse, gap 10 to filename                                      |
| Dot fill — Drafted   | `AmberBrush`, with amber 4px box-shadow at 13% alpha (`Translation="0,0,4"` + `ThemeShadow` tinted amber) |
| Dot fill — Writing   | `IndigoBrush`, pulse animation opacity 1↔0.5 over 1.6s ease-in-out infinite |
| Dot fill — Planned   | Hollow ring — `Transparent` background, 1px `Ink4Brush` border       |
| Filename             | `MonoLargeTextStyle`, ink if writing or drafted, ink3 if planned    |
| Status badge         | `EyebrowTinyTextStyle`, color matches dot fill                       |
| Row opacity          | 1.0 if writing or drafted, 0.5 if planned                            |

### 4.6 Rail reveal animation

When `RailsVisible` flips false → true:

| Property                       | From → To       | Duration | Easing                       | Delay  |
|--------------------------------|----------------|----------|------------------------------|--------|
| `CompositionStack.Width`       | 0 → 260        | 480ms    | EaseOutQuintic (`PowerEase` Power=5) | 0      |
| `FilesRail.Width`              | 0 → 340        | 480ms    | EaseOutQuintic               | 0      |
| `CompositionStack.Opacity`     | 0 → 1          | 320ms    | EaseInOut                    | 160ms  |
| `FilesRail.Opacity`            | 0 → 1          | 320ms    | EaseInOut                    | 160ms  |
| `ActivePage.MaxWidth`          | 720 → 880      | 480ms    | EaseOutQuintic               | 0      |
| `ActivePage.Padding.Top`       | 64 → 32        | 480ms    | EaseOutQuintic               | 0      |

The 160ms opacity delay is critical — rails *slide* into place first, then their content fades on. Without it, rails feel like they appear; with it, they read as opening up.

XAML reference Storyboard:

```xml
<Storyboard x:Name="RailsRevealStoryboard">
    <DoubleAnimation Storyboard.TargetName="LeftRail"
                     Storyboard.TargetProperty="Width"
                     From="0" To="260" Duration="0:0:0.480">
        <DoubleAnimation.EasingFunction>
            <PowerEase EasingMode="EaseOut" Power="5" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <DoubleAnimation Storyboard.TargetName="LeftRail"
                     Storyboard.TargetProperty="Opacity"
                     From="0" To="1"
                     BeginTime="0:0:0.160" Duration="0:0:0.320">
        <DoubleAnimation.EasingFunction>
            <SineEase EasingMode="EaseInOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
    <!-- mirror for RightRail and ActivePage -->
</Storyboard>
```

### 4.7 ProgressIndicator

Single 1px hairline at top of center column with amber-filled segment showing fraction complete.

```
[━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━]
[████████████████████████ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─]

ARCHITECTURE                                                       03 / 08
```

Specs:
- Outer track: 1px height, `HairlineBrush` fill, full width
- Filled segment: 1px height, `AmberBrush` fill, width = `(activeIndex + 1) / 8 × 100%`
- Width animates over 480ms ease-out-quint when activeIndex changes
- Below the bar, on one row: layer name (`EyebrowSmallTextStyle` ink3) left, counter (`MonoMediumTextStyle` ink4) right, padding 8/0

No dots, no per-layer labels, no chrome.

### 4.8 LockedContextCard

Compressed past-layer summary stacked above the active canvas. Renders inline with the page content (not floating).

Expanded form (default for the most recent 2 locked layers):

```xml
<Grid Padding="0,14,0,14"
      BorderBrush="{ThemeResource HairlineBrush}"
      BorderThickness="0,1,0,0">

    <controls:BlockHandle Visible="{x:Bind IsHovered, Mode=OneWay}" />

    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />  <!-- header row -->
        <RowDefinition Height="Auto" />  <!-- summary prose -->
        <RowDefinition Height="Auto" />  <!-- facts grid -->
    </Grid.RowDefinitions>

    <!-- Header row -->
    <Grid Grid.Row="0" Margin="0,0,0,6">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        <controls:Eyebrow Grid.Column="0" Text="✓ {LayerLabel}" ForegroundBrush="Ink3Brush" Weight="600" />
        <TextBlock Grid.Column="1" Text="·" Margin="8,0" Foreground="{ThemeResource Ink5Brush}" />
        <controls:Eyebrow Grid.Column="2" Text="locked" ForegroundBrush="Ink4Brush" />
        <Button Grid.Column="4" Content="−"
                FontFamily="{StaticResource MonoFontFamily}"
                Foreground="{ThemeResource Ink4Brush}"
                Background="Transparent" BorderThickness="0" />
        <Button Grid.Column="5" Content="Revisit ↗"
                Foreground="{ThemeResource Ink3Brush}"
                Background="Transparent" BorderThickness="0"
                Margin="12,0,0,0" />
    </Grid>

    <!-- Summary -->
    <TextBlock Grid.Row="1"
               Text="{x:Bind Summary, Mode=OneWay}"
               Style="{StaticResource BodyLargeTextStyle}"
               Margin="0,0,0,10" />

    <!-- 2-column facts grid -->
    <ItemsRepeater Grid.Row="2"
                   ItemsSource="{x:Bind Facts}"
                   Padding="0,8,0,0"
                   BorderBrush="{ThemeResource Hairline2Brush}"
                   BorderThickness="0,1,0,0">
        <ItemsRepeater.Layout>
            <UniformGridLayout MinItemWidth="120" ItemsStretch="Fill"
                               MinColumnSpacing="32" MinRowSpacing="4" />
        </ItemsRepeater.Layout>
        <!-- ItemTemplate: Eyebrow label (min-width 90) + Mono value (ink2) -->
    </ItemsRepeater>
</Grid>
```

Collapsed form (default for older locked layers):

```xml
<Grid Padding="0,14,0,14"
      BorderBrush="{ThemeResource HairlineBrush}"
      BorderThickness="0,1,0,0">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />     <!-- inline summary, text-overflow ellipsis -->
        <ColumnDefinition Width="Auto" />  <!-- + button -->
        <ColumnDefinition Width="Auto" />  <!-- Revisit ↗ button -->
    </Grid.ColumnDefinitions>
    <controls:Eyebrow Grid.Column="0" Text="✓ {LayerLabel}" Weight="600" />
    <TextBlock Grid.Column="1" Text="·" />
    <controls:Eyebrow Grid.Column="2" Text="locked" />
    <TextBlock Grid.Column="3" Text="{x:Bind Summary, Mode=OneWay}"
               Style="{StaticResource BodyItalicTextStyle}"
               TextTrimming="CharacterEllipsis"
               Margin="12,0" />
    <!-- chevron and revisit buttons identical to expanded -->
</Grid>
```

The chevron is `−` when expanded, `+` when collapsed. Mono font 11px, ink4. Toggling animates 240ms ease (manually triggered, no Storyboard needed because content size change handles itself).

### 4.9 FuturePreviewCard

Read-only single-line preview of upcoming layers, rendered below the active canvas + composer footer.

```xml
<Border Padding="14,12"
        BorderBrush="{ThemeResource HairlineBrush}"
        BorderThickness="1"
        BorderStyle="Dashed"           <!-- Note: dashed isn't native; emulate via Path -->
        CornerRadius="0"
        Background="Transparent"
        Opacity="{x:Bind Opacity, Mode=OneWay}"
        IsHitTestVisible="False">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <controls:Eyebrow Grid.Column="0" Text="{LayerLabel} · upcoming"
                          ForegroundBrush="Ink4Brush" />
        <TextBlock Grid.Column="1" Text="{x:Bind Hint, Mode=OneWay}"
                   Style="{StaticResource BodyItalicTextStyle}"
                   Margin="16,0,0,0" />
    </Grid>
</Border>
```

Opacity calculation: `Math.Max(0.05, 0.40 - (distance × 0.08))`.

| Distance | Opacity |
|----------|---------|
| 1        | 0.32    |
| 2        | 0.24    |
| 3        | 0.16    |
| 4        | 0.08    |
| 5+       | 0.05    |

Dashed border in WinUI requires drawing as a `Path` with `StrokeDashArray="4,3"` since native `BorderStyle` doesn't support it. The agent should template this as a control with a Path frame.

---

## 5. Layer canvases — pixel-level specs

### 5.1 IntentCanvas

Tabular field grid + example-values banner + annotations.

**Banner (when any field still holds example value):**

```xml
<Border Padding="14,10"
        Background="{ThemeResource AmberSoftBrush}"
        BorderBrush="{ThemeResource HairlineBrush}"
        BorderThickness="1"
        CornerRadius="5"
        Margin="0,0,0,4">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        <controls:Eyebrow Grid.Column="0" Text="Example values" Weight="600"
                          ForegroundBrush="AmberBrush" />
        <TextBlock Grid.Column="1" Margin="8,0"
                   Text="Tap any field to start your own — or clear to begin fresh."
                   Style="{StaticResource BodyItalicTextStyle}" />
        <Button Grid.Column="2" Content="Clear all"
                Foreground="{ThemeResource InkBrush}"
                Background="Transparent" BorderThickness="0"
                FontWeight="Medium" />
    </Grid>
</Border>
```

**Field grid:**

```xml
<ItemsRepeater ItemsSource="{x:Bind Fields}">
    <ItemsRepeater.ItemTemplate>
        <DataTemplate>
            <Grid Padding="0,12"
                  BorderBrush="{ThemeResource Hairline2Brush}"
                  BorderThickness="0,0,0,1">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="120" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="80" />
                </Grid.ColumnDefinitions>
                <controls:Eyebrow Grid.Column="0" Text="{Binding Label}" />
                <TextBox Grid.Column="1" Text="{Binding Value, Mode=TwoWay}"
                         PlaceholderText="{Binding Placeholder}"
                         Style="{StaticResource InvisibleTextBoxStyle}" />
                <controls:Eyebrow Grid.Column="2"
                                  Text="Proposed"
                                  ForegroundBrush="AmberBrush"
                                  Visibility="{Binding IsDifferent, Converter={StaticResource BoolToVis}}" />
            </Grid>
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

`InvisibleTextBoxStyle` removes the default TextBox chrome — no border, no background, only the text cursor visible on focus.

When a field is in the proposed-diff state (`IsDifferent`):
- Background changes to `AmberSoftBrush`
- Padding inflates by 10 on left/right (negative margin to escape gridline)
- Border-radius becomes 4
- Transition `background 220ms ease`

### 5.2 UXFlowStrip

Horizontal flow of 5 screen tiles connected by `→` arrows.

Tile specs:
- Width: 116px (fixed)
- Background: `Paper2Brush`
- Border: 1px `HairlineBrush`
- BorderRadius: 6
- Padding: 10/12
- Inner layout: Vertical `StackPanel` with Spacing 8

Tile content:
1. `EyebrowTinyTextStyle` `SCREEN {N}` in ink4
2. `BodyMediumTextStyle` screen name (weight 500, ink)
3. Mock UI blocks — 3 horizontal `Rectangle` elements:
   - Each 3px tall, `HairlineBrush` fill, radius 1
   - Varying widths: 100%, 70%, 85% (creates an organic "real UI" feel)
   - Spacing 3 between them
   - Margin-top 4
4. `MonoSmallTextStyle` note text in ink3

Between tiles:
- `→` arrow rendered as TextBlock, `InterFontFamily` 14px ink4
- Padding 0/2, vertically centered with `AlignSelf="Center"`

Outer container:
- Horizontal scrolling (`ScrollViewer.HorizontalScrollBarVisibility="Auto"`)
- `Spacing="6"` between elements

### 5.3 ArchitectureBlueprint

Hand-drawn SVG-style diagram. The most visually rich canvas in the system.

**Canvas dimensions:** 800×340 viewBox, scales to fit `MaxWidth="800"`, centered, `display: block, margin: 0 auto`.

In WinUI/Uno, render as a `Canvas` inside a `Viewbox` for proportional scaling. The "rough hand-drawn" effect requires SkiaSharp custom drawing (SVG `feTurbulence` doesn't exist in WinUI/Uno). The implementing agent uses one of:

- **SkiaSharp displacement:** Render rectangles via `SKCanvas.DrawRect` with vertices perturbed by Perlin noise (`SKPath` with manually displaced points)
- **Composition shader:** Apply a `BorderEffect` + `DisplacementMapEffect` via `CompositionEffectBrush` (Win2D bridge)
- **Pre-baked SVG asset:** Render the rough-edged rectangles once as an `Image` source, swap on hover for the bold variant

The visual specs the agent must reproduce:

#### 5.3.1 Backdrop grid

- `Paper2Brush` fill on the whole 800×340 area
- 40×40 pixel grid lines, 0.5px `Hairline2Brush` stroke
- Renders behind everything else

#### 5.3.2 Modules (6 nodes)

Positioned on a 3×2 grid:

| Module ID  | Label         | Position (x, y) | Color (default)              | Description                                |
|------------|---------------|-----------------|------------------------------|--------------------------------------------|
| `pages`    | Pages         | (110, 90)       | `InkBrush`                   | Route surfaces                              |
| `nav`      | Navigation    | (110, 230)      | `InkBrush`                   | Region-based routes                         |
| `mvux`     | State (MVUX)  | (380, 90)       | `IndigoBrush`                | Feeds, States, Selection                    |
| `services` | Services      | (380, 230)      | `Ink2Brush`                  | Job / Habit / etc.                          |
| `http`     | HTTP (Kiota)  | (650, 90)       | `Ink2Brush`                  | Typed clients, generated                    |
| `storage`  | Storage       | (650, 230)      | `Ink2Brush`                  | Local cache, offline-first                  |

Rectangle dimensions:
- Width: 120, Height: 44
- CornerRadius: 6
- BorderThickness: 1px, color = module color
- Background: `PaperBrush`
- Apply turbulence displacement when rendering

Label inside rectangle:
- `BodyMediumTextStyle`, color = module color, weight 500
- Centered both horizontally and vertically inside the rect

File-count badge (visible only when hovered):
- Position: top-right of rect, offset (-12, -8) outside
- Size: ~28×16
- `Paper3Brush` background, 1px module-color border, radius 8
- Text: `MonoSmallTextStyle` `{N}f` (e.g., `4f`, `6f`)

#### 5.3.3 Edges (5 connections)

| From      | To       | Label      |
|-----------|----------|------------|
| pages     | mvux     | binds      |
| pages     | nav      | requests   |
| mvux      | services | consumes   |
| services  | http     | calls      |
| services  | storage  | persists   |

Edge specs:
- Stroke: dashed, 1px `Ink3Brush` default, 1.8px `InkBrush` when connected to hover
- Dash array: `4,3`
- Apply turbulence displacement
- Edge connection point picks the right side of each rect based on relative position

Edge labels:
- `BodyItalicTextStyle` font-size 10, color ink3 default, ink + weight 600 when connected
- Positioned at midpoint of the edge
- Background mask: 4px `PaperBrush` rect behind text to break the line

#### 5.3.4 Turbulence filter parameters

The SVG reference (for agents implementing via SkiaSharp or composition):

```
Default filter:
  feTurbulence type="fractalNoise" baseFrequency="0.022" numOctaves="2" seed="3"
  feDisplacementMap scale="1.5"
  filterRegion: x="-3%" y="-3%" width="106%" height="106%"

Bold (hover) filter:
  feTurbulence type="fractalNoise" baseFrequency="0.018" numOctaves="2" seed="5"
  feDisplacementMap scale="2.0"
```

Key constraints:
- Apply filter only to rect/path elements, never to text (text becomes illegible under displacement)
- Filter region must bleed 3% so edges don't clip
- Distinct seeds per canvas — Architecture uses 3 and 5, Interactions uses 7 and 11

#### 5.3.5 Detail panel (below SVG)

```
┌─────────────────────────────────────────────────────────────────────┐
│ STATE (MVUX)               4 files · 2 connections                  │
│ Feeds, States, Selection — reactive layer between Pages and Services│
└─────────────────────────────────────────────────────────────────────┘
```

Specs:
- Padding: 14 vertical, 0 horizontal
- MinHeight: 64 (prevents layout shift on hover changes)
- Title row: `EyebrowSmallTextStyle` weight 600 ink2 + `EyebrowTinyTextStyle` ink4 with gap 12
- Description: `BodyMediumTextStyle` italic ink2

Resting state (when nothing hovered):
- `BodyMediumTextStyle` italic ink4
- Text: "Hover any module to trace its connections"

#### 5.3.6 Action button (`↻ Regenerate`)

`GhostButton` style (see §6.4), placed in `SectionHeader.Action` slot.

### 5.4 DesignTokenGrid

Three sections stacked vertically in the canvas:

#### 5.4.1 Color tokens table

8 rows in a `Grid.ColumnDefinitions="24,*,100,80"` layout:

```
┌──┬─────────────┬──────────┬──────────────┐
│██│ Surface     │ #0C0D0F  │              │  ← 24×24 swatch, 1.5px AmberBrush border only on Action
├──┼─────────────┼──────────┼──────────────┤
│██│ Action      │ #C89C3F  │  was #...    │  ← amber-bordered swatch
├──┼─────────────┼──────────┼──────────────┤
│██│ Info        │ #7AB3DF  │              │
└──┴─────────────┴──────────┴──────────────┘
```

Row specs:
- Padding: 10 vertical, 0 horizontal
- Border-bottom: 1px `Hairline2Brush` between rows (last row no border)
- Swatch column (24px): Border 1px `Ink5Brush`, radius 4, fill = token color
  - **Exception:** the `Action` swatch has a 1.5px `AmberBrush` border (visual self-reference)
- Name column: `BodyMediumTextStyle` ink
- Hex column: `MonoLargeTextStyle` ink2
- Proposed-was column: `EyebrowTinyTextStyle` amber + `MonoSmallTextStyle` `was {oldHex}` — only when in preview state

Click on swatch opens `ColorPicker` flyout. Changes mark layer dirty + update XAML mirror live.

#### 5.4.2 Type scale section

```
Display    Today's schedule                              26px
Heading    Job #4471 · Boiler service                    15px
Body       Tap a job to assign a technician.             13px
Caption    ARRIVED · 09:14 SYNCED                        10px
```

`Grid.ColumnDefinitions="60,*,50"`. Rows padded 10 vertical with hairline2 separators.

| Slot     | Sample text                                              | Family    | Size | Weight | Color |
|----------|----------------------------------------------------------|-----------|------|--------|-------|
| Display  | "Today's schedule"                                       | Inter     | 26   | 500    | Ink   |
| Heading  | "Job #4471 · Boiler service"                             | Mono      | 15   | 500    | Ink   |
| Body     | "Tap a job to assign a technician. Conflicts surface inline." | Inter | 13 | 400  | Ink2  |
| Caption  | "ARRIVED · 09:14 SYNCED"                                 | Inter     | 10   | 500    | Ink3 (uppercase, 0.04em tracking) |

Sample copy is **real-app-shaped** strings, not lorem ipsum. The body sample even includes a period to demonstrate prose rhythm.

#### 5.4.3 Mini control gallery

Two boxes side by side, `Grid.ColumnDefinitions="*,*"`, gap 16:

**PRIMARY box:**
- 1px `HairlineBrush` border, no fill, radius 6, padding 16
- Eyebrow header "PRIMARY"
- Inner row with two buttons:
  - "Assign tech" — primary CTA: background = `Action` token color, foreground = `Paper4Brush` (for 4.5:1 contrast against amber), radius 5, padding 8/16
  - "View map" — ghost: no fill, 1px ink3 border, ink2 text

**TAB BAR · TOOLKIT box:**
- Identical container styling
- Eyebrow header "TAB BAR · TOOLKIT"
- Three pill segments: `Today / Week / Map`
  - Active segment: `Action` token color background, `Paper4Brush` foreground
  - Inactive segments: transparent, ink3 text
  - Joined visually (no gap, shared borders)

#### 5.4.4 ColorPaletteOverride.xaml mirror

A `CodeBlock` with `Theme="Dark"`, `Language="xaml"`, rendered below the gallery.

Content is the live-synthesized XAML from `DesignModel.ColorPaletteOverrideXaml` (see Architecture brief §5.2 for synthesis logic).

Updates immediately on any token change — no `Generate preview` button required to refresh the mirror. Only the lock cycle uses preview state.

### 5.5 StateTransitionDiagram

The Interactions canvas. Six state pills arranged in 3×2 grid with 8 curved arrow transitions between them.

#### 5.5.1 Canvas dimensions

800×340 viewBox (same as Architecture), backdrop grid identical (`Paper2Brush` + 40×40 lines).

Turbulence filter seeds: 7 (default), 11 (bold).

#### 5.5.2 State pills (6 positions)

| State    | Position (x, y) | Color brush         |
|----------|-----------------|---------------------|
| Default  | (130, 100)      | `StateColorDefault` |
| Loading  | (400, 100)      | `StateColorLoading` |
| Success  | (670, 100)      | `StateColorSuccess` |
| Offline  | (130, 240)      | `StateColorOffline` |
| Empty    | (400, 240)      | `StateColorEmpty`   |
| Error    | (670, 240)      | `StateColorError`   |

Top row (y=100) = primary user-facing states. Bottom row (y=240) = exceptional states.

Pill specs:
- Width: 130, Height: 44
- CornerRadius: 22 (fully rounded on short axis)
- Border: 1px solid state color
- Background: `PaperBrush`
- Apply turbulence displacement
- Label inside: `BodyMediumTextStyle` 13px state color weight 500, centered

#### 5.5.3 Transitions (8 curved arrows)

Each transition has a hand-tuned SVG path. The agent should preserve these paths exactly:

| From    | To        | Label       | Path (SVG `d` attribute reference)                    |
|---------|-----------|-------------|--------------------------------------------------------|
| Default | Loading   | submit      | `M 195 100 C 250 100, 300 100, 335 100`               |
| Default | Offline   | disconnect  | `M 130 122 C 130 160, 130 200, 130 218`               |
| Loading | Success   | resolves    | `M 465 100 C 530 100, 580 100, 605 100`               |
| Loading | Empty     | no data     | `M 400 122 C 400 160, 400 200, 400 218`               |
| Loading | Error     | rejects     | `M 450 122 C 530 160, 600 200, 640 218`               |
| Error   | Loading   | retry       | `M 670 218 C 600 160, 530 122, 465 122`               |
| Offline | Default   | reconnect   | `M 130 218 C 130 200, 130 160, 130 122`               |
| Success | Default   | reset       | `M 670 78 C 500 30, 300 30, 195 78`                   |

Stroke specs:
- Default: 1px `Ink3Brush`, dashed `4,3`
- Connected: 1.8px `InkBrush`, dashed `4,3`
- Apply turbulence displacement

Arrow markers:
- `marker-end="url(#arrow-int)"` reference
- `<marker id="arrow-int" orient="auto-start-reverse">`
- Marker shape: triangle with `M 0 0 L 8 4 L 0 8 z`, fill = same as stroke

Transition labels:
- `BodyItalicTextStyle` 10px, ink3 default, ink + weight 600 when connected
- Positioned at midpoint of curve (hand-tuned)
- Background mask: 4px `PaperBrush` rect

#### 5.5.4 Pulsing dot on active state

```xml
<Ellipse Width="7" Height="7"
         Fill="{ThemeResource AmberBrush}"
         Canvas.Left="{StatePillRight - 6}"
         Canvas.Top="{StatePillTop + 6}">
    <Ellipse.Triggers>
        <EventTrigger RoutedEvent="Ellipse.Loaded">
            <BeginStoryboard>
                <Storyboard RepeatBehavior="Forever">
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="1" To="0.35" Duration="0:0:0.8" AutoReverse="True">
                        <DoubleAnimation.EasingFunction>
                            <SineEase EasingMode="EaseInOut" />
                        </DoubleAnimation.EasingFunction>
                    </DoubleAnimation>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Ellipse.Triggers>
</Ellipse>
```

Pulses opacity 1↔0.35 over 1.6s ease-in-out infinite. Renders only when `isActive && !isHovered` — hover takes visual priority.

#### 5.5.5 Detail panel (below SVG)

Identical layout to Architecture detail panel.

When hovering ≠ active state:
- Left rule (4px tall × full width) above panel: `AmberBrush`
- Hint text: "hover preview · click to select"

When hovering = active or no hover:
- Left rule: `HairlineBrush`
- Hint text: "hover any state to trace its transitions"

#### 5.5.6 Flow tabs (in SectionHeader.Action)

Three pill buttons for flow selection (`Create job`, `Sign in`, `Sync data`):
- Active: `InkBrush` background, `PaperBrush` text
- Inactive: transparent background, ink3 text
- Border: 1px `HairlineBrush`
- CornerRadius: 4, joined visually
- Click sets `ActiveFlowId` + resets `ActiveStateKind` to Default

### 5.6 DataContractGrid

Vertical list of entity sections. Each entity:

```xml
<Border Padding="0,14,0,14"
        BorderBrush="{ThemeResource HairlineBrush}"
        BorderThickness="0,0,0,1">
    <StackPanel>
        <Grid Margin="0,0,0,10">
            <controls:Mono Text="{x:Bind EntityName}" Style="MonoLargeTextStyle" Weight="600" />
            <controls:Eyebrow Text="record" ForegroundBrush="Ink4Brush" Margin="10,0,0,0" />
        </Grid>
        <ItemsRepeater ItemsSource="{x:Bind Fields}">
            <ItemsRepeater.Layout>
                <UniformGridLayout MinColumnSpacing="16" MinRowSpacing="4"
                                   MaximumRowsOrColumns="2" Orientation="Horizontal" />
            </ItemsRepeater.Layout>
            <ItemsRepeater.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="160,*">
                        <controls:Mono Grid.Column="0" Text="{Binding Name}" Foreground="Ink2Brush" />
                        <controls:Mono Grid.Column="1" Text="{Binding TypeText}" Foreground="IndigoBrush" />
                    </Grid>
                </DataTemplate>
            </ItemsRepeater.ItemTemplate>
        </ItemsRepeater>
    </StackPanel>
</Border>
```

The indigo type column is the only place outside Architecture where indigo surfaces. It signals "this is a type, not a value."

Below the entity list: a `CodeBlock` with `Theme="Dark"`, `Language="csharp"`, content = `DataModel.PrimaryRecordCSharp` feed.

### 5.7 ImplementationPhaseGrid

Tabular grid with 6 rows. `Grid.ColumnDefinitions="40,140,*,240"`.

Per row:

| Column | Width | Content                                                        |
|--------|-------|----------------------------------------------------------------|
| 0      | 40    | `EyebrowSmallTextStyle` `P{N}` in phase color, weight 600     |
| 1      | 140   | Title block: phase title (BodyMediumTextStyle, weight 600 ink) + label (EyebrowTinyTextStyle ink3) |
| 2      | *     | Description (BodyMediumTextStyle ink2) + file list (MonoSmallTextStyle ink3, one per line with `+ ` prefix) |
| 3      | 240   | Agent prompt (BodyItalicTextStyle ink3) behind 1px `HairlineBrush` left rule, padding-left 12 |

Row padding: 16 vertical, 0 horizontal. Hairline-row separators between rows.

### 5.8 ScaffoldTerminalCanvas

Dark code block with command + action row below.

**Command block:**
- Background: `Paper4Brush`
- Padding: 20
- BorderRadius: 6
- Border: 1px `HairlineDkBrush`
- Position relative for the Copy button
- Copy button absolute top-right (10/10), `rgba(255,255,255,0.06)` bg, `rgba(255,255,255,0.16)` border, `#FAFAFA` text, padding 4/10, radius 5
- Command text: `MonoLargeTextStyle` `#FAFAFA`, line-height 1.7, white-space pre

**Action row (below):**

```
[ Download bundle ↓ ]  [ Copy prompt-context.md ]    the composition is, for now, complete.
```

- Layout: horizontal `StackPanel`, `Spacing="8"`, `VerticalAlignment="Center"`
- `Download bundle ↓` — `PrimaryButton` (ink tone)
- `Copy prompt-context.md` — `GhostButton`
- Italic caption right-aligned via `Spacer` element + `BodyItalicTextStyle`

---

## 6. Buttons and interactive elements

### 6.1 PrimaryButton

```xml
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{ThemeResource InkBrush}" />
    <Setter Property="Foreground" Value="{ThemeResource PaperBrush}" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="14,8" />
    <Setter Property="CornerRadius" Value="5" />
    <Setter Property="FontFamily" Value="{StaticResource InterFontFamily}" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="FontWeight" Value="Medium" />
    <Setter Property="MinHeight" Value="32" />
</Style>
```

Hover: background = `#000` (slightly darker than ink), transition 140ms ease.

Amber tone variant (`Tone="Amber"`): `Background="{AmberBrush}"`, `Foreground="{Paper4Brush}"`. Used for `Generate preview →` button only.

Disabled state: `Opacity="0.4"`, `IsHitTestVisible="False"`.

### 6.2 GhostButton

```xml
<Style x:Key="GhostButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
    <Setter Property="BorderBrush" Value="{ThemeResource HairlineBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="14,8" />
    <Setter Property="CornerRadius" Value="5" />
    <Setter Property="FontFamily" Value="{StaticResource InterFontFamily}" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="FontWeight" Value="Medium" />
</Style>
```

Hover: `Foreground="{Ink2Brush}"`, `BorderBrush="{Ink3Brush}"`, 140ms ease.

Small variant (`Size="Small"`): `Padding="8,3"`, `FontSize="11"`.

### 6.3 ChipButton (suggestion chips)

```xml
<Style x:Key="ChipButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{ThemeResource Ink3Brush}" />
    <Setter Property="BorderBrush" Value="{ThemeResource HairlineBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="10,4" />
    <Setter Property="CornerRadius" Value="5" />
    <Setter Property="FontFamily" Value="{StaticResource InterFontFamily}" />
    <Setter Property="FontSize" Value="12" />
    <Setter Property="FontWeight" Value="Medium" />
</Style>
```

Hover: `Foreground="{InkBrush}"`, `BorderBrush="{Ink3Brush}"`.

### 6.4 Toggle button group (Preview/Edit)

Two `Button` controls visually joined:

| Property                   | Active button            | Inactive button       |
|----------------------------|--------------------------|------------------------|
| Background                 | `InkBrush`               | Transparent            |
| Foreground                 | `PaperBrush`             | `Ink3Brush`            |
| BorderBrush                | `InkBrush`               | `HairlineBrush`        |
| BorderThickness            | 1                        | 1                      |
| Padding                    | 8/3                      | 8/3                    |
| FontSize                   | 10                       | 10                     |
| FontWeight                 | 500                      | 500                    |
| CornerRadius (left button) | `4,0,0,4`                | `4,0,0,4`              |
| CornerRadius (right button)| `0,4,4,0`                | `0,4,4,0`              |
| Right margin (left button) | -1                       | -1                     |

The negative margin overlaps borders cleanly so the segmented control reads as one shape.

---

## 7. Animation timing

Consistency matters more than absolute values. Three rhythms in the system:

| Rhythm                      | Duration  | Easing                         | Uses                                                  |
|-----------------------------|-----------|--------------------------------|-------------------------------------------------------|
| Workspace opening           | 480ms     | `PowerEase EaseOut Power=5`    | Rail reveal width, FuturePreviewCard opacity, ProgressIndicator fill width |
| Content fade                | 320ms     | `SineEase EaseInOut` (160ms delay) | Rail content opacity                                |
| Responsive interaction      | 200-240ms | `SineEase EaseOut`             | Hover visual changes, hairline color shifts          |
| Button micro-interaction    | 140ms     | `SineEase EaseOut`             | Button hover background/foreground                    |
| Tint flash                  | 220ms     | `SineEase EaseOut`             | Field background changes on preview diff             |
| Pulse                       | 1600ms (infinite) | `SineEase EaseInOut` AutoReverse | Writing-state file row dot, active-state pill dot |

XAML reference Storyboards (all `Themes/Animations.xaml`):

```xml
<Storyboard x:Key="OpeningStoryboardTemplate">
    <DoubleAnimation Duration="0:0:0.480" From="0" To="1" Storyboard.TargetProperty="Width">
        <DoubleAnimation.EasingFunction>
            <PowerEase EasingMode="EaseOut" Power="5" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>

<Storyboard x:Key="ContentFadeStoryboardTemplate">
    <DoubleAnimation Duration="0:0:0.320" From="0" To="1" BeginTime="0:0:0.160"
                     Storyboard.TargetProperty="Opacity">
        <DoubleAnimation.EasingFunction>
            <SineEase EasingMode="EaseInOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>

<Storyboard x:Key="HoverInteractionTemplate">
    <DoubleAnimation Duration="0:0:0.240" Storyboard.TargetProperty="Opacity">
        <DoubleAnimation.EasingFunction>
            <SineEase EasingMode="EaseOut" />
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>
```

---

## 8. Visualization patterns (v8 enrichment)

These five patterns repeat across the Architecture and Interactions canvases. The agent should template them as reusable behaviors.

### 8.1 Hand-drawn wobble

See §5.3.4 — turbulence filter parameters. Implementing without SVG (Uno doesn't ship `feTurbulence`):

**Option A (preferred, Skia renderer):** Custom `SKCanvasView` that:
1. Generates a Perlin noise field at render time
2. Tessellates each rectangle's border into ~20 segments
3. Displaces each segment endpoint by `(noise(x,y) × scale, noise(x+100,y+100) × scale)`
4. Renders via `SKPath.AddPoly`

**Option B (composition):** A `CompositionDistortionEffect` chained with `Win2D.CanvasImageEffect.PerlinNoise`. More performant but requires composition pipeline.

**Option C (pre-baked):** Render the rough rectangles once at app load as PNG/SVG assets per module. Swap on hover. Worst aesthetic (no real-time variation) but easiest implementation.

### 8.2 Connected-set hover computation

When a node is hovered, compute the set of connected nodes + edges:

```csharp
public IFeed<ConnectedSet?> Connected =>
    Feed.Combine(HoveredId, Edges).Select(t =>
    {
        var (hoveredId, edges) = t;
        if (hoveredId == null) return null;
        var nodes = new HashSet<string> { hoveredId };
        var edgeKeys = new HashSet<string>();
        foreach (var edge in edges)
        {
            if (edge.FromId == hoveredId || edge.ToId == hoveredId)
            {
                edgeKeys.Add($"{edge.FromId}-{edge.ToId}");
                nodes.Add(edge.FromId);
                nodes.Add(edge.ToId);
            }
        }
        return new ConnectedSet(nodes.ToImmutableHashSet(), edgeKeys.ToImmutableHashSet());
    });
```

### 8.3 Three-state ternary

Visual properties of nodes and edges read from a 3-way condition:

```csharp
// !hovered ? default : (isConnected ? emphasized : dimmed)
public static double ResolveOpacity(bool hasHover, bool isConnected) =>
    !hasHover ? 0.65 : (isConnected ? 1.0 : 0.12);

public static Brush ResolveStroke(bool hasHover, bool isConnected,
                                    Brush dflt, Brush emphasized, Brush dimmed) =>
    !hasHover ? dflt : (isConnected ? emphasized : dimmed);
```

| Property      | No hover    | Hover + connected | Hover + not connected |
|---------------|-------------|-------------------|------------------------|
| Edge opacity  | 0.65        | 1.0               | 0.12                   |
| Edge stroke   | `Ink3Brush` | `InkBrush`        | `Ink4Brush`            |
| Edge weight   | 1px         | 1.8px             | 1px                    |
| Node opacity  | 1.0         | 1.0               | 0.25                   |
| Label fill    | `Ink3Brush` | `InkBrush`        | `Ink4Brush`            |
| Label weight  | 400         | 600               | 400                    |

### 8.4 Z-order

Layered top to bottom:
1. Backdrop grid (`Paper2Brush` fill + 40×40 grid lines)
2. Edges (paths + labels via paper-masked rects)
3. Nodes (rects + labels + badges)

The click target is always the topmost visible element. Hover events bubble correctly because the node rects sit on top of edges.

### 8.5 Rich detail panel

Specified in §5.3.5 (Architecture) and §5.5.5 (Interactions). Layout identical, content varies. `MinHeight="64"` is critical — prevents layout shift on hover changes.

---

## 9. Resource dictionary structure

### 9.1 Loading order (App.xaml)

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 1. Material baseline (Uno Toolkit Material) -->
            <MaterialTheme xmlns="using:Uno.Material" />

            <!-- 2. Per-token color overrides (live from DesignTokens) -->
            <ResourceDictionary Source="Themes/ColorPaletteOverride.xaml" />

            <!-- 3. Static semantic colors -->
            <ResourceDictionary Source="Themes/ThemeColorOverrides.xaml" />

            <!-- 4. Editorial primitives -->
            <ResourceDictionary Source="Themes/Editorial.xaml" />

            <!-- 5. Brush keys derived from theme colors -->
            <ResourceDictionary Source="Themes/Brushes.xaml" />

            <!-- 6. Typography font families -->
            <ResourceDictionary Source="Themes/Typography.xaml" />

            <!-- 7. TextBlock variant styles -->
            <ResourceDictionary Source="Themes/TextBlockStyles.xaml" />

            <!-- 8. Button styles -->
            <ResourceDictionary Source="Themes/Buttons.xaml" />

            <!-- 9. Animation Storyboards -->
            <ResourceDictionary Source="Themes/Animations.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 9.2 Material slot mapping

The DesignModel writes these Material slots in `ColorPaletteOverride.xaml`. Anything not in this table uses Material defaults.

| DesignTokens field | Material Light slot                | Material Dark slot                 |
|--------------------|------------------------------------|------------------------------------|
| `Action`           | `SecondaryColor`                   | `SecondaryColor` (same across themes)|
| `Warn`             | `ErrorColor`                       | `ErrorColor` (same)                |
| `Surface`          | (Light uses #FAFAFA default)       | `BackgroundColor`                  |
| `Panel`            | (Light uses #FAFAFA default)       | `SurfaceColor`                     |
| `Info`, `Success`, `Tag`, `Locked` | (no canonical Material slot — written to `ThemeColorOverrides.xaml` as additional semantic resources) | |

This is a deliberate constraint: the Design canvas exposes 8 tokens, but only 4 map cleanly to Material slots. The other 4 are additional semantic colors that consuming code references directly by brush key.

---

## 10. Acceptance criteria

A v11-conformant visual implementation:

### Foundation
- [ ] Inter (4xx/5xx/6xx) and JetBrains Mono (4xx/5xx/6xx) loaded as `InterFontFamily` and `MonoFontFamily`
- [ ] All 15 TextBlock styles from §1.2 defined in `Themes/TextBlockStyles.xaml`
- [ ] All 5 surface, 5 ink, 3 hairline, 5 semantic, 6 phase, 6 state-color brushes defined in `Themes/Brushes.xaml`
- [ ] Spacing values used in layouts are all from `4, 8, 12, 16, 24, 32, 48, 64`
- [ ] Corner radii are all from `0, 3, 4, 5, 6, 22`
- [ ] All hex codes live in resource keys — no inline hex anywhere in component XAML

### Editorial primitives
- [ ] `Eyebrow`, `Mono`, `Body`, `SectionHeader`, `Annotation`, `CodeBlock`, `BlockHandle`, `MarkdownPreview` primitives all exist as templated controls or UserControls
- [ ] `SectionHeader` filenames render lowercase
- [ ] `Annotation` supports both `Voice="Rationale"` and `Voice="Quote"` presentations
- [ ] `CodeBlock` supports both `Theme="Light"` and `Theme="Dark"`
- [ ] `MarkdownPreview` handles H1/H2/H3, paragraphs, bulleted+numbered lists, blockquotes, fenced code blocks, inline bold/italic/code

### Shell
- [ ] Three-column scaffold with `CompositionStack` left, `ActivePage` center, `FilesRail` right
- [ ] When `RailsVisible == false`, both rails have width 0 + opacity 0, center column max-width 720 with padding-top 64
- [ ] Rail reveal animation: width 480ms ease-out-quint (no delay), opacity 320ms ease-in-out (160ms delay)
- [ ] `ProgressIndicator` shows 1px hairline track + amber-fill segment + layer name eyebrow + counter mono
- [ ] `LockedContextCard` has expanded (default) and collapsed forms with `−`/`+` chevron toggle
- [ ] `FuturePreviewCard` opacity follows `Math.Max(0.05, 0.40 - (distance × 0.08))` and dashed border

### Layer canvases
- [ ] `IntentCanvas` uses `120,*,80` field grid + amber-tinted example banner with Clear all button
- [ ] `DesignTokenGrid` uses `24,*,100,80` color grid with 1.5px amber border on Action swatch
- [ ] `DesignTokenGrid` type-scale section uses real-app strings, not lorem ipsum
- [ ] `ArchitectureBlueprint` SVG is 800×340 viewBox with hairline grid backdrop
- [ ] Architecture modules are 120×44 with 6 corner radius
- [ ] Architecture turbulence uses seeds 3 (default) and 5 (bold) with scale 1.5 and 2.0
- [ ] `StateTransitionDiagram` uses seeds 7 and 11
- [ ] State pills are 130×44 with `CornerRadius="22"`
- [ ] All 8 transition paths from §5.5.3 are preserved
- [ ] Pulsing dot is 7×7 amber, opacity 1↔0.35 over 1.6s, renders only when `isActive && !isHovered`
- [ ] `UXFlowStrip` tiles are 116px wide on `Paper2Brush` with 6 corner radius
- [ ] `DataContractGrid` uses 160×* field grid with indigo type column
- [ ] `ImplementationPhaseGrid` uses `40,140,*,240` with phase-colored P-eyebrow
- [ ] `ScaffoldTerminalCanvas` command block is `Paper4Brush` with copy button absolute top-right

### Buttons
- [ ] `PrimaryButton` is ink/paper with 14/8 padding, 5 corner radius, 140ms hover transition
- [ ] `PrimaryButton.Tone="Amber"` is amber/paper4 (4.5:1 contrast)
- [ ] `GhostButton` is transparent with 1px hairline border, ink3 text, hover to ink2/ink3
- [ ] `ChipButton` matches `GhostButton` shape at 12px font size
- [ ] Toggle button group (Preview/Edit) uses negative right margin for joined borders

### Animation
- [ ] All transitions use one of the timing rhythms from §7 — no off-table durations
- [ ] Rail reveal Storyboard uses `PowerEase EaseOut Power=5` (approximates JS cubic-bezier)
- [ ] Pulse animation on writing-state file row dot uses `AutoReverse` + `RepeatBehavior="Forever"`

---

## 11. Out of scope (v11)

These visual capabilities are not in v11:

- **Dark mode** — palette and resource keys are light-only
- **Real `feTurbulence` displacement** — current implementation uses SkiaSharp or pre-baked assets, not native SVG filter
- **Right-to-left text** — system assumes LTR
- **Multiple font weights beyond 400/500/600** — no 700 (bold) or 300 (light) variants
- **Animated state transitions** between layer pages — only the rail reveal animates; page swaps are instant
- **Per-canvas theme variants** — no "preview in dark mode" toggle on DesignCanvas
- **Custom focus ring styling** — uses Material defaults
- **Reduced-motion fallback** — animations always play; no `OS animations-reduce` query handling
- **Print stylesheet** — no print support

The companion `INTERACTION-BRIEF-detailed.md` covers every user-facing behavior. The companion `ARCHITECTURE-BRIEF-detailed.md` covers every structural element. This brief covers the visual truth — refer to it for any question about "what does this look like."
