# StillThere — Design Brief

Translates the web prototype's visual system into Uno Platform resources for `net9.0-desktop` (Skia) and `net9.0-android` (Skia). Restraint is the design: one off-white ground, hairline rules, a two-font system, and age expressed only through the timer's color. No cards, no badges, no fills, no chrome.

Grounded in Uno styling/theming and responsive guidance from the Uno docs MCP.

---

## Visual direction

`Opinion / design intent.` Editorial restraint. The page reads like a quiet ledger. The single moment of expression is task completion (the hand-drawn scratch + a calm sage check). Everything else is type, space, and hairlines.

Principles carried from the prototype:
- The list is the interface. No dashboard chrome competes with it.
- Sorted by neglect, not priority — oldest task sits at the top.
- Age is signaled by **timer color only**. Titles do not change weight or color with age (except Fresh, which dims slightly).
- The list closing up after completion is the reward, so motion is reserved for that.

---

## Color — tokens, never inline

`Framework best practice.` No hardcoded hex in markup. All colors live in `Themes/Colors.xaml`; brushes in `Themes/Brushes.xaml`; everything references brushes by key. Light theme only (no dark-mode toggle — explicitly out of scope per Uno usage rules and product intent).

| Token | Value | Role |
|---|---|---|
| `Bg` | `#FAFAF7` | page ground (warm off-white) |
| `Ink` | `#18181A` | primary text, active timer |
| `InkSoft` | `#5A5852` | secondary text, default timer |
| `InkMute` | `#97948B` | meta, placeholders, fresh timer |
| `InkFaint` | `#C5C2B8` | strike-through of completed, counts |
| `Rule` | `#EAE7DE` | hairline separators, control underline |
| `RuleSoft` | `#F1EFE7` | expanded-row background |
| `Aging` | `#B57729` | amber — Aging timer, snooze accent |
| `Stale` | `#B23A2A` | terracotta — Stale timer, active filter |
| `Rotten` | `#7E2418` | oxblood — Rotten timer |
| `Done` | `#5C7355` | muted sage — completion scratch + check ONLY |

`Project convention.` Color is the information layer. The age palette runs cool-neutral → warm → hot so a glance at the timer column reads neglect without reading numbers. `Done` is the one cool note and appears nowhere except the completion moment — it is the visual signature of relief.

Contrast: `Ink`/`InkSoft` on `Bg` clear 4.5:1 body. The warm timer colors are used at ≥14 px and as the row's loudest element, clearing large-text 3:1.

---

## Typography

`Project convention (design specified to the letter).` Two families, bundled as assets:
- **Geist** — UI / titles / body
- **Geist Mono** — timers, meta, tags, counts, control labels (tabular numerals)

`Note on Uno usage rules.` The rules say to reuse existing `TextBlock` styles and avoid explicit sizes. This design specifies an exact ramp, which the rules permit when the user provides a specific design. We encode the ramp as named styles in `Themes/TextBlock.xaml` so usage stays declarative (`Style="{StaticResource TaskTitle}"`) rather than inline sizes scattered through markup.

Type ramp (baseline on a 4 px grid; line-height a multiple of 4):

| Style key | Family | Size / line-height / weight | Used on |
|---|---|---|---|
| `AppTitle` | Geist | 17 / 24 / 500 | "StillThere" header |
| `TaskTitle` | Geist | 16 / 24 / 400 | task titles, add input, edit input |
| `TaskNotes` | Geist | 14 / 20 / 400 | notes under title |
| `Timer` | Geist Mono | 14 / 20 / 400 (tabular) | age readout (color by state) |
| `Meta` | Geist Mono | 13 / 16 / 400 | header summary, created date |
| `Tag` | Geist Mono | 12 / 16 / 400 | tags, tag-filter chips, counts |
| `MicroAction` | Geist Mono | 11 / 16 / 400 | row actions (done/break/snooze) |
| `SheetTitle` | Geist | 18 / 24 / 500 | snooze sheet question |

`Common convention.` Body never below 14 px; only de-emphasized meta uses 11–13 px. Titles wrap rather than truncate. The mono ramp owns everything numeric/temporal so the "measured" content is visually distinct from the "written" content.

---

## Spacing & layout

`Framework best practice.` Spacing on a 4/8 scale only: 4, 8, 12, 16, 20, 24, 32, 40. Margins never let text touch the edge (≥16 px). Use Toolkit `AutoLayout` for flows with `Spacing`/`Padding` on the container; never set margins on children inside an AutoLayout.

### Page structure (ListPage)

```
SafeArea (Android insets)
└ ScrollViewer
  └ Root (vertical AutoLayout, max content width 680, centered)
     ├ Header
     │   AppTitle  "StillThere"
     │   Summary (Meta): "{open} open · oldest {age} · [filter toggle] · [clear filters]"
     ├ Controls (horizontal AutoLayout, bottom hairline)
     │   Search input (borderless, flex)   ·   State toggle (Meta, right)
     ├ TagRow (horizontal wrap AutoLayout) — only when tags exist
     │   Tag chips: "#work 4" (active → Stale color)
     ├ TaskList (ItemsRepeater in the scroll, top hairline)
     │   TaskRow template (see below)
     ├ AddRow (horizontal AutoLayout, bottom hairline)
     │   "+"  ·  add input  ·  hint "#tags after title" (desktop only)
     └ CompletedSection
         Toggle (Meta): "› {n} completed"
         CompletedList (revealed) + "clear all completed"
```

`Project convention.` Content column caps at **680 px** and centers; on Desktop the page is a calm column in a wide window, on Android it fills width within SafeArea. Line length stays in the legible 45–72 char band at this width.

### TaskRow — component hierarchy

```
TaskRow (Grid: * | Auto)   row padding 20 vertical
  Column 0 (body, min-width 0):
    TaskTitle  (wraps)              ← ScratchView overlays this on completion
    TaskNotes  (only if present)
    TagRow     (only if tags; chips are Tag style, tappable)
  Column 1 (right):
    Timer  (Timer style; Foreground bound to AgeState→brush)
  Overlay (right, revealed by hover on Desktop / expand on Android):
    Actions: done · break · snooze   (MicroAction style)
  Bottom: 1 px Rule separator
```

Expanded (edit) variant: row background → `RuleSoft`, 4 px corner radius, negative horizontal margin to bleed the tint; body swaps to three borderless inputs (title / notes / tags) stacked; action row gains `delete`.

CompletedRow: `InkMute` title with `InkFaint` strike-through; right side Meta "`{relative} ago · took {lifespan}`"; `restore` revealed on hover.

---

## Age-state → brush mapping

`Project convention.` One place owns the mapping (a value converter or, preferably, the projected `TaskItemVm` exposes a `TimerBrush` key). Titles are unaffected except Fresh.

| AgeState | Timer brush | Timer weight | Title treatment |
|---|---|---|---|
| Fresh | `InkMute` | 400 | dim to `InkSoft` |
| Active | `Ink` | 400 | `Ink` |
| Aging | `Aging` | 400 | `Ink` |
| Stale | `Stale` | 500 | `Ink` |
| Rotten | `Rotten` | 500 | `Ink` |

`Framework best practice.` Drive these through `VisualStateManager` states on the row (state name = AgeState) rather than per-property converters where possible, so the whole row's age presentation is declared in one `VisualStateGroup`.

---

## Resource organization

`Framework best practice (Uno usage rules).` New styles, colors, and templates are declared in dedicated dictionaries merged in `App.xaml`:

```
Themes/Colors.xaml      Color resources (the token table)
Themes/Brushes.xaml     SolidColorBrush per token; AgeState brushes
Themes/TextBlock.xaml   the type ramp styles
Themes/Styles.xaml      Search/AddInput/MicroAction button, ScrollViewer, ItemsRepeater layout, SnoozeSheet
```

Lightweight styling: control styles reference brush keys, never literals. Where a control style already encodes a color, we do not override inline.

---

## Responsive / adaptive behavior

`Framework best practice.` Use the Toolkit `Responsive` markup extension and `ResponsiveView` for width-breakpoint differences; `SafeArea` for Android insets. Same View, different values — no separate mobile/desktop layouts.

Breakpoints (Uno responsive grid guidance):

| Width | Layout intent |
|---|---|
| < 600 (Android phone) | full-width column inside SafeArea; row actions revealed by **tap-to-expand**; add bar pinned bottom; snooze as bottom sheet; add hint hidden |
| 600–904 (large phone / small window) | same column, comfortable padding; actions still tap-reveal |
| ≥ 905 (Desktop window) | 680 px centered column; row actions revealed on **hover**; snooze as centered dialog; add hint shown; add row inline (not pinned) |

`Project convention.` The only structural divergence is the add bar (pinned-bottom dock on phone, inline on desktop) and the snooze surface (bottom sheet vs. centered dialog). Both are expressed with `Responsive` values / `ResponsiveView`, not platform `#if`.

Touch targets: all interactive elements ≥ 44×44 on Android (row action buttons get larger hit padding via the `Responsive` extension); hover targets can be tighter on Desktop.

---

## Iconography & assets

`Common convention.` Minimal. The completion check is a glyph (`✓`) rendered in `Done`, the add affordance is a typographic `+`, the completed-toggle chevron is `›` rotated. No icon font dependency required; if any icon is needed use `FontIcon`. Fonts (Geist, Geist Mono) ship as embedded assets and are registered in `App.xaml`.

---

## Elevation & depth

`Opinion.` None. The design is flat by intent — hairlines and tint zones create hierarchy, not shadows. The one exception worth considering is the snooze surface; if it needs to separate from the page, use `ThemeShadow` via `Translation Z` (8–16) on the sheet only, per Uno elevation guidance. Default is no shadow.

---

## Unresolved Questions

- Font licensing for bundling: confirm Geist + Geist Mono can ship as embedded app assets for Android/Desktop distribution, or substitute (e.g., a metrically similar pairing) if licensing blocks embedding.
- The 680 px content cap is borrowed from the web prototype. On a large desktop window that leaves wide empty margins — keep the calm column, or allow the column to grow toward ~760 px on very wide windows?
- Active-tag and active-filter both use the `Stale` color. If a user filters by tag *and* flips the state toggle, two unrelated controls glow the same terracotta. Acceptable, or give the tag filter its own accent to disambiguate?
- Completed-row "took {lifespan}" uses the same mono Meta style as everything else. Worth a quieter treatment so the completed list reads as clearly past-tense, or is the strike-through enough?
