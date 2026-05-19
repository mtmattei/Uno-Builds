# Design Brief — YouTube × Microsoft (Uno Platform port)

**Status:** Draft v0.1 · Owner: DevRel · Source of truth for visuals

This brief covers theme, color, typography, spacing, layout grids, component anatomy, and the state matrix. Code structure lives in the architecture brief; motion lives in the interactions brief. Where Uno Toolkit provides a control, we use it — never a hand-rolled `Border + CornerRadius + ThemeShadow` for what `CardContentControl` already solves.

---

## 1. Theme foundation

**Material v2** is the base theme system (Material wins when both Material and Fluent are present per Uno rules). We do not ship a manual light/dark toggle; the app follows system theme via `ThemeService`. The reference is a dark-first aesthetic, but every token below is defined for both modes so light theme is a free fallout.

Tokens live in `Themes/ColorPaletteOverride.xaml` merged into `App.xaml`. Nothing in the visual layer hardcodes a hex value — every color comes through `{ThemeResource}`.

### 1.1 Color palette (semantic mapping over the Material v2 token set)

| Token (Material v2 key)       | Light                  | Dark                   | Usage                                              |
| ----------------------------- | ---------------------- | ---------------------- | -------------------------------------------------- |
| `PrimaryBrush`                | `#0AA5B6` (teal-600)   | `#2EE0E0` (teal-glow)  | Active nav, play button, focus ring, verified tick |
| `OnPrimaryBrush`              | `#FFFFFF`              | `#03252A`              | Text/iconography on Primary surfaces               |
| `PrimaryContainerBrush`       | `#CFF6F7`              | `#0E3438`              | Active tab pill, chip selected fill                |
| `SecondaryBrush`              | `#5A8B92`              | `#9DC7CB`              | Secondary actions, channel chips                   |
| `SurfaceBrush`                | `#F4F8FA`              | `#0A1D2A`              | Page background base                               |
| `SurfaceVariantBrush`         | `#E2EBEF`              | `#142E3A`              | Sidebar, search field, rail items                  |
| `OnSurfaceBrush`              | `#0B1A22`              | `#F3F8FB`              | Primary text                                       |
| `OnSurfaceVariantBrush`       | `#3F525C`              | `#8FA1AB`              | Secondary text, metadata, placeholders             |
| `OutlineBrush`                | `#D5DDE2`              | `#1F3B49`              | Hairlines, dividers, default card stroke           |
| `OutlineVariantBrush`         | `#E8EEF1`              | `#16303C`              | Subtler dividers (sidebar internal)                |
| `ErrorBrush`                  | `#BA1A1A`              | `#FFB4AB`              | Failed-load badges, errors                         |
| `BackgroundBrush`             | gradient (light)       | gradient (dark)        | The teal→navy mesh — see §1.2                      |

The signature glow color (`#2EE0E0` in dark) is reserved: it appears only on the avatar ring, the active-nav indicator, the play CTA, the focus halo, and verified ticks. It loses meaning if it appears elsewhere.

### 1.2 Page background

Implemented as a `Grid` background with two stacked elements rather than a flat color: a `LinearGradientBrush` (teal-deep → navy-deep, 160°) plus a low-opacity radial overlay element in the top-left corner driving the "spotlight" feel. On Skia this composites cheaply; on legacy WinUI we fall back to a static asset. No `BackdropFilter` or runtime blur — it's expensive on mobile and the design doesn't need it once the gradient is right.

## 2. Typography

We use the **Material v2 type scale exclusively** ([rules: "Use existing TextBlock styles only; do not set explicit font sizes or weights"](#)). Mapping for this app:

| Role                          | Style key                | Sample                          |
| ----------------------------- | ------------------------ | ------------------------------- |
| Page title ("Home")           | `HeadlineMediumTextBlockStyle` | 30 sp, 36 lh                |
| Hero headline                 | `DisplaySmallTextBlockStyle`   | 36 sp, 44 lh, weight 600    |
| Section header ("For You")    | `TitleLargeTextBlockStyle`     | 22 sp, 28 lh                |
| Card video title              | `BodyLargeTextBlockStyle`      | 16 sp, 24 lh, weight 500    |
| Channel name                  | `BodyMediumTextBlockStyle`     | 14 sp, 20 lh                |
| Stats / timestamps / metadata | `BodySmallTextBlockStyle`      | 12 sp, 16 lh                |
| Buttons & chips               | `LabelLargeTextBlockStyle`     | 14 sp, weight 500           |
| Sidebar nav labels            | `LabelLargeTextBlockStyle`     |                              |
| Sidebar section header        | `LabelMediumTextBlockStyle`    | 12 sp, color OnSurfaceVariant |

Font family is the Material default (Roboto-derived). We do **not** swap in Segoe UI even though the brand says Microsoft — Material v2 was tuned with this metric. Forcing Segoe breaks line-height alignment with the 4 dp baseline grid. The "Microsoft" brand reading comes from the four-square logo and the "Microsoft Featured" pill, not the body type.

## 3. Spacing & grid

Spacing scale: **4, 8, 12, 16, 24, 32, 48, 64**. No values outside this scale anywhere in the app.

Layout grid columns × outer margin per breakpoint (per the layout rules):

| Breakpoint name (Toolkit) | Width range (px)   | Columns | Gutter | Outer margin |
| ------------------------- | ------------------ | ------- | ------ | ------------ |
| Narrowest                 | 0–149              | 1       | n/a    | 16           |
| Narrow                    | 150–599            | 4       | 16     | 16           |
| Normal                    | 600–904            | 8       | 16     | 16           |
| Wide                      | 905–1280           | 12      | 20     | 20           |
| Widest                    | 1281+              | 12      | 24     | 24           |

The reference image is at **Widest** (≈1248 px content). Ports to other breakpoints are spec'd in §6.

All flows use **Uno Toolkit `AutoLayout`** for stacking with `Spacing` and `Padding` on the parent — children never set `Margin` ([rules: "NEVER set padding or margin on children; use Spacing/Padding on AutoLayout"](#)). Star sizing in `Grid` is acceptable for the shell-level split (sidebar fixed, content `*`).

## 4. Elevation

`ThemeShadow` only, with explicit Translation Z values:

| Surface                       | Z      |
| ----------------------------- | ------ |
| Sidebar (Mica panel)          | 0      |
| Search field                  | 0      |
| Hero card                     | 16     |
| Video card (default)          | 4      |
| Video card (hover/pressed)    | 16     |
| Notification flyout           | 24     |
| Toast                         | 32     |

We do not stack a container shadow over a child shadow — `CardContentControl`'s style already encodes the right elevation; we choose the style (Elevated / Filled / Outlined) instead of compositing.

## 5. Component inventory

Every component below maps to either a Toolkit control or a templated `UserControl` built from one. **No bespoke `Border` cards.**

### 5.1 Shell (MainPage)

```
Grid (rows: *, cols: 248px / *)
├── utu:AutoLayout  (Sidebar, vertical, Spacing=16, Padding=16)
│    ├── Brand row                     (Image + 2 TextBlocks)
│    ├── utu:TabBar                    (vertical orientation, drives Region "Shell")
│    ├── utu:Divider
│    ├── Subscriptions header          (LabelMedium)
│    └── ItemsRepeater                 (sub-channels)
└── Grid  (Content area)
     ├── utu:NavigationBar             (top bar — search + actions)
     └── ScrollViewer  uen:Region.Name="Shell"
          └── HomePage / ExplorePage / ...
```

Sidebar uses `SurfaceVariantBrush` with a 1-px `OutlineBrush` stroke and 16-px corner radius. On Wide and below it collapses into a `DrawerControl`.

### 5.2 Top bar

`utu:NavigationBar` with custom `Content` is the pattern — it gives us the platform-correct safe-area handling (especially on iOS) for free. Slots:

- **Left:** empty (sidebar carries the brand at Widest)
- **Center:** `AutoBox` containing a `TextBox` styled as a pill (search) with leading icon and trailing clear button
- **Right:** notification `Button` (icon-only via `ControlExtensions.Icon`) and a templated user `Button` showing the avatar

### 5.3 Hero card

`utu:CardContentControl` with `ElevatedCardContentControlStyle`, height 320 dp, content template:

- Background `Image` with `Stretch="UniformToFill"` — server-supplied 1600×640 jpeg
- Foreground `Grid` with a left-anchored gradient overlay (a vector `Rectangle` with `LinearGradientBrush`, not a bitmap — keeps it crisp)
- `AutoLayout` (vertical, Spacing 16) for the badge / title / description / CTA stack

CTA: a `Button` styled `FilledButtonStyle` overridden with the Primary brush gradient and a leading 22-dp circular play glyph. Duration is a `TextBlock` next to it using `BodyMedium`.

The "Microsoft Featured" badge is a `utu:Chip` with `IsCheckable="False"` and a custom 14×14 four-square `Image` in the leading slot.

### 5.4 Video card (used in For You and Trending)

`utu:CardContentControl` with `OutlinedCardContentControlStyle`, content template:

```
AutoLayout (vertical, Spacing=8)
├── Grid                                    -- thumbnail container, 16:10 ratio
│    ├── Image  Stretch="UniformToFill"
│    ├── Border (gradient overlay, opacity bound to IsPointerOver)
│    ├── Border (play disc, scaled via VSM)
│    └── TextBlock (duration pill, BodySmall on a Surface chip)
└── AutoLayout (vertical, Spacing=4, Padding=0,4,0,0)
     ├── TextBlock  Style=BodyLarge  TextWrapping=Wrap  MaxLines=2
     ├── AutoLayout (horizontal, Spacing=4)   -- channel + verified
     │    ├── TextBlock  Style=BodyMedium
     │    └── ImageIcon  (verified glyph, 14 dp, Primary brush)
     └── TextBlock  Style=BodySmall  Opacity=.7  -- "2.1M views · 1 day ago"
```

The trending tile is the same control with a different `DataTemplate` selector — image fills the full 120-dp card, title overlaid bottom-left, rank chip pinned top-right.

### 5.5 Recommendation rail item

Plain `Button` (so it gets focus + keyboard activation for free) styled flat, content laid out by `AutoLayout` horizontal, Spacing 12:

- 80×50 dp `Image` (rounded 6 dp)
- vertical `AutoLayout` (title 2-line BodyMedium, channel BodySmall)

Hover pulls a 2-dp Primary accent strip from the left edge — implemented in the template's VSM, not a runtime element add.

## 6. Layout — three breakpoints

| Region          | Wide / Widest (reference) | Normal (~768)             | Narrow (<600)                    |
| --------------- | ------------------------- | ------------------------- | -------------------------------- |
| Sidebar         | Pinned 248 dp             | Collapsed → DrawerControl | Collapsed → DrawerControl        |
| Top search      | Centered, 540 dp pill     | Stretch fills row         | Icon-only, expands on tap        |
| Hero row        | Hero 1fr · Rail 320 dp    | Hero full-width above rail (rail becomes horizontal scroller) | Hero full-width, rail hidden behind a "Recommended" link |
| For You         | 4 columns                 | 3 columns                 | 1.2 columns (peek-next-card)      |
| Trending        | 4 columns                 | 2 columns                 | 1 column                          |

This is implemented with `utu:ResponsiveView` at the structural splits and `utu:ResponsiveExtension` at the property level (column count, padding, font hint sizes). Default Toolkit breakpoints are accepted; we don't override them.

## 7. State matrix

Every interactive control has a defined visual for each state. States are driven by `utu:VisualStateManagerExtensions.States` where the binding is non-trivial; the standard pointer/focus states ride on the existing Material templates.

| Component         | Default                      | Hover / PointerOver           | Pressed                         | Focused (kbd)                              | Selected / Active             | Disabled              | Loading                    | Empty / Error              |
| ----------------- | ---------------------------- | ----------------------------- | ------------------------------- | ------------------------------------------ | ----------------------------- | --------------------- | -------------------------- | -------------------------- |
| Nav item          | OnSurfaceVariant text        | SurfaceVariant fill, OnSurface text | Translate Y +1 dp, fill darker | 2 dp Primary outer ring, 2 dp offset       | Primary indicator pill behind | 38 % opacity          | n/a                        | n/a                        |
| Search field      | OutlineVariant border        | Outline border                | Outline border, fill -8 % L     | 1 dp Primary border + 4 dp Primary glow halo | n/a                          | 38 % opacity          | inline ring at trailing icon | error border + helper text |
| Hero CTA          | Primary fill, gradient       | +2 % brightness, +2 dp lift   | -2 dp, ripple                   | 2 dp focus ring, offset 2 dp                | n/a                           | 38 % opacity, no ring | swap glyph for spinner     | n/a                        |
| Video card        | Outlined, surface tint       | +1 dp lift, thumbnail zoom 1.04, play disc fades in | -1 dp                           | 2 dp ring on the card, not on the image    | n/a                           | 38 % opacity          | shimmer skeleton (see int.) | error placeholder image    |
| Trending tile     | Outlined                     | Background pan + brightness +6 % | -1 dp                           | 2 dp ring                                  | n/a                           | 38 % opacity          | shimmer                    | error placeholder           |
| Avatar            | Conic ring static            | Ring brightness +20 %         | n/a                             | Solid Primary 2 dp ring                    | n/a                           | grayscale             | spinner overlay on ring    | n/a                        |
| Bell              | Icon at 70 % opacity         | 100 % opacity                 | scale 0.94                      | 2 dp ring                                  | panel open: stays full opacity | 38 % opacity          | small dot pulses Primary   | n/a                        |
| Notification dot  | Primary fill, breathing      | n/a                           | n/a                             | n/a                                        | n/a                           | hidden                | n/a                        | n/a                        |
| Drawer (mobile)   | off-canvas                   | n/a                           | n/a                             | first item focused on open                 | n/a                           | n/a                   | n/a                        | n/a                        |

Loading and empty/error visuals are **always** mediated by `utu:LoadingView` bound to the relevant `IFeed.IsExecuting` (via the AsyncCommand `ILoadable` contract). Skeleton shimmer is the loading template, a typed empty-state UserControl is the empty template.

## 8. Iconography

- 18 dp glyphs in the sidebar nav (consistent set: filled when active, outlined otherwise)
- 20 dp in the top bar (bell, search)
- 22 dp inside the hero CTA
- 14 dp for verified ticks and inline metadata

Source: Material Symbols (Outlined + Filled variants) shipped as `.ttf` so we can use a single `FontIcon` everywhere — no per-platform raster fallback. For the four-square Microsoft mark we use a vector `Image` at 26 dp (sidebar) and 14 dp (hero pill).

## 9. Imagery

Two thumbnail sizes from the backend: 240×135 (rail, sidebar previews) and 600×340 (cards, trending). The hero pulls 1600×640. Anything bigger is downsampled on the server. We never decode 4 K imagery on-device.

`Stretch="UniformToFill"` for photographs (cards, hero, rail), `Stretch="Uniform"` for icons. Failed loads show a generated gradient (teal → navy with the channel initials centered) — not a broken-image glyph.

## 10. Localization & accessibility

- Every visible TextBlock and interactive control gets `x:Uid` (e.g. `HomePage.HeroTitle`, `HomePage.PlayButton`).
- French and English ship at v1. RTL languages are not blocked — `AutoLayout` flips correctly under `FlowDirection`.
- Color contrast is 4.5:1 minimum for body, 3:1 for large text — verified on both light and dark token sets.
- Touch targets ≥ 44×44 dp (`MinHeight`/`MinWidth` set on every interactive control).
- `AutomationProperties.Name` set on all icon-only buttons; `AutomationProperties.HelpText` on the avatar (it's not labeled visually).
- Focus order matches reading order. Tab cycles: top bar → sidebar → main content sections in document order.

---
*Cross-references: see the architecture brief for which model exposes each piece of data, and the interactions brief for how each state in §7 transitions to the next.*
