# Design Brief: Timeline / Conversation History

> Generated from: HBDBn_AbEAA2fRT.jpg
> Date: 2026-03-04
> Status: Draft — review open questions before implementation

---

## 1. High-Level Intent

### Product Goal
Provide users with a chronological history of their AI conversations, enabling re-engagement with past sessions.

### User Goal
Browse, recall, and resume previous conversations organized by date.

### Screen Purpose
A timeline view that displays a vertically scrolling, date-grouped list of past conversation summaries with a left sidebar for navigation.

### Context
This appears to be part of an AI assistant product (likely a ChatGPT-style interface). The timeline is one of several views accessible from the left icon sidebar. The selected item ("Researched ETF investing strategies") suggests clicking opens the full conversation.

---

## 2. Scope

### Screens Present
| Screen | Description | States Visible |
|--------|-------------|----------------|
| Timeline | Date-grouped conversation history with sidebar | Populated state, hover/selected state on one item |

### States Covered
- [x] Success/populated state
- [x] Hover/selected state (one item highlighted)
- [ ] Empty state
- [ ] Loading state
- [ ] Error state
- [ ] Partial/degraded state

### States Missing or Assumed
- Empty state (no conversations yet)
- Loading/skeleton state
- Error state (failed to load history)
- Search/filter results state
- Multi-select / bulk delete state

### Out of Scope
- Conversation detail view
- Settings/profile screens
- New conversation creation flow

---

## 3. Information Architecture

### Page Hierarchy

```
Page
├── Left Icon Sidebar (persistent nav)
│   ├── App icon / logo (top)
│   ├── Timeline (active) ─── icon with raised/selected state
│   ├── Grid/Dashboard
│   ├── Analytics/Charts
│   ├── Files/Folders
│   ├── Documents
│   ├── AI/Magic wand
│   └── Integrations/Plugins
├── Main Content Area
│   ├── Header Bar
│   │   ├── Page Title ("Timeline")
│   │   └── Time Filter Segmented Control (Day | Week | Month)
│   └── Timeline List (scrollable)
│       ├── Date Group: "Today"
│       │   ├── Item: "Asked for a high-protein meal plan"
│       │   ├── Item: "Worked on the b402 dashboard UX"
│       │   └── Item: "Brainstormed side projects"
│       ├── Date Group: "Yesterday"
│       │   ├── Item: "Researched ETF investing strategies" ← selected/hovered
│       │   ├── Item: "Drafted a polite client email"
│       │   ├── Item: "Asked about improving sleep quality"
│       │   └── Item: "Generated tagline ideas for a landing page"
│       └── Date Group: "Feb 8, 2026"
│           ├── Item: "Explained a TypeScript generic error"
│           └── Item: "Helped rewrite a LinkedIn profile summary"
```

### Grouping Logic
Conversations grouped by date (Today → Yesterday → specific dates). Within each group, items appear in reverse chronological order (most recent first). A vertical timeline connector line joins all items.

### Navigation Model
Left icon sidebar (vertical icon rail) for primary app-level navigation. The segmented control (Day/Week/Month) acts as a filter on the timeline granularity.

---

## 4. User Flows

### Entry Points
- Clicking the timeline icon in the left sidebar
- Possibly the default/home view of the application

### Primary Flow (Happy Path)
1. User sees timeline grouped by date
2. User scrolls to find a past conversation
3. User clicks/hovers on an item (highlight state shown)
4. User is taken to that conversation's detail view

### Alternate Paths
- **Filter by time**: User clicks Week/Month to change grouping granularity
- **Scroll to older**: User scrolls down to load more historical items

### Error/Edge Paths
- No conversations: Empty state needed
- Failed to load: Error state with retry

### Exit Points
- Click a conversation item → conversation detail
- Click any sidebar icon → different section
- Start new conversation (not visible, likely elsewhere)

---

## 5. Component Inventory

### Components
| Component | Variants | Count | Notes |
|-----------|----------|-------|-------|
| Icon Sidebar Rail | Active/Inactive icons | 1 | 8 icon slots |
| Sidebar Icon | Active (raised bg), Inactive (flat) | 8 | ~40px touch target |
| Segmented Control | Day (active), Week, Month | 1 | 3 segments |
| Date Header | Text label | 3 | "Today", "Yesterday", "Feb 8, 2026" |
| Timeline Item | Default, Hovered/Selected | 9 | Conversation summary |
| Timeline Connector | Vertical line + dots | 1 | Connects all items |
| Timeline Dot | Standard (taupe/grey), Active (magenta/pink) | 2 variants | Bullet markers |
| Page Title | — | 1 | "Timeline" |

### Component Details

#### Icon Sidebar Rail
- **Type**: Custom
- **Sizing**: ~56px wide, full height
- **Background**: Matches page background (light warm grey)
- **Icons**: ~24px, spaced ~48-56px vertically
- **Active state**: Rounded rectangle background behind icon (~40x40px), slightly raised/darker

#### Segmented Control
- **Type**: Standard segmented / tab-like control
- **Variants**: 3 segments — Day (active), Week, Month
- **Active state**: White/light background fill, slightly bolder text
- **Inactive state**: No fill, muted text
- **Sizing**: ~120px total width, ~32px height
- **Border-radius**: ~8px overall, ~6px per segment
- **Closest standard**: WinUI SegmentedControl or custom TabBar with pill style

#### Timeline Item
- **Type**: Custom list item
- **States**: Default (text only), Hovered/Selected (grey background fill, pink dot, pink connector segment)
- **Sizing**: Full width of content area, ~48-56px row height
- **Content**: Single line of descriptive text, ~16px font
- **Background on hover**: ~rgba(0,0,0,0.06) or similar warm grey fill with rounded corners

#### Timeline Connector
- **Type**: Custom decorative element
- **Structure**: Thin vertical line (~1px) connecting dots between items, with subtle S-curves between date groups
- **Dot size**: ~8px diameter
- **Active dot**: Magenta/hot pink filled circle
- **Standard dot**: Taupe/warm grey filled circle
- **Connector color**: Light taupe/warm grey, ~40% opacity

---

## 6. Layout + Spacing

### Grid System
- **Columns**: Not a traditional grid — two-panel layout (sidebar + main content)
- **Sidebar width**: ~56px
- **Main content left padding**: ~48px from sidebar edge
- **Content area max width**: ~700px (estimated)

### Spacing Scale
Appears to use a 4-point baseline system:
- 4px, 8px, 12px, 16px, 24px, 32px, 48px

### Baseline Unit Compliance
Generally compliant with 4pt or 8pt system. The timeline item spacing appears consistent.

### Key Measurements
| Element | Property | Value | Baseline Compliant? |
|---------|----------|-------|---------------------|
| Sidebar | width | ~56px | Yes (14×4) |
| Sidebar icon | size | ~24px | Yes (6×4) |
| Sidebar icon spacing | vertical gap | ~48px | Yes (12×4) |
| Page title | top margin | ~24px | Yes |
| Segmented control | height | ~32px | Yes (8×4) |
| Date header | margin-top | ~32px | Yes |
| Timeline item | row height | ~52px | Yes (13×4) |
| Timeline dot | diameter | ~8px | Yes (2×4) |
| Dot to text | horizontal gap | ~16px | Yes (4×4) |
| Content left margin | from sidebar | ~48px | Yes (12×4) |

### ASCII Layout Map

```
┌──────┬──────────────────────────────────────────────────────────────┐
│(app) │  Timeline                          [Day|Week|Month]         │
│      │                                     ^^^^           ← Seg.   │
│ (◫)← │  ● Today                                  ← Date Header    │
│active│  │                                         ~24px bold       │
│      │  │                                                          │
│ (⊞)  │  ├─● Asked for a high-protein meal plan    ← Item ~16px    │
│      │  │                                          muted text      │
│ (📊) │  ├─● Worked on the b402 dashboard UX                       │
│      │  │                                                          │
│ (📁) │  ├─● Brainstormed side projects                             │
│      │  │                                                          │
│ (📄) │  ● Yesterday                               ← Date Header   │
│      │  │                                                          │
│ (✨) │  ┌──────────────────────────────────────────────────┐       │
│      │  │◉ Researched ETF investing strategies     🖱️     │← hover │
│ (🔌) │  └──────────────────────────────────────────────────┘  row  │
│      │  │  ◉ = pink dot, pink connector segment                    │
│      │  │                                                          │
│      │  ├─● Drafted a polite client email                          │
│      │  │                                                          │
│      │  ├─● Asked about improving sleep quality                    │
│      │  │                                                          │
│      │  ├─● Generated tagline ideas for a landing page             │
│      │  │                                                          │
│      │  ● Feb 8, 2026                              ← Date Header   │
│      │  │                                                          │
│      │  ├─● Explained a TypeScript generic error                   │
│      │  │                                                          │
│      │  └─● Helped rewrite a LinkedIn profile summary              │
│      │                                                             │
├──────┤                                                             │
│~56px │  ←───────────── ~600-700px content area ──────────────→     │
└──────┴──────────────────────────────────────────────────────────────┘
         ↕ ~16px between items
         ↕ ~32px before date headers
```

### Responsiveness
- Sidebar could collapse to a hamburger menu on mobile
- Content area would take full width on smaller screens
- Segmented control should remain accessible, possibly moving below the title on narrow screens
- Items are single-column — stacks naturally on mobile
- Touch targets for timeline items need ~48px minimum height (currently ~52px — passes)

---

## 7. Typography System

### Font Families
| Role | Family | Type | Fallback |
|------|--------|------|----------|
| All text | ~SF Pro / Inter / system sans-serif | Sans-serif | -apple-system, sans-serif |

### Type Scale
| Style Name | Size | Weight | Line Height | Letter Spacing | Usage | Modular Scale? |
|------------|------|--------|-------------|----------------|-------|----------------|
| Page Title | ~22px | Semi-bold (600) | ~1.2 | 0 | "Timeline" | Close (~21px at 1.33) |
| Date Header | ~18px | Bold (700) | ~1.25 | 0 | "Today", "Yesterday" | Close |
| Item Text | ~16px | Regular (400) | ~1.5 | 0 | Conversation summaries | Yes (body) |
| Segment Label | ~14px | Medium (500) | ~1.2 | 0 | "Day", "Week", "Month" | Yes |

### Typography Rules Audit
- [x] Sans-serif used for body/labels/small text
- [x] Bold used sparingly (only date headers)
- [x] No italic on buttons/labels
- [x] ALL CAPS not used anywhere (good — sentence case throughout)
- [x] Line height: headers ~1.25, body ~1.5 — compliant
- [x] Font sizes follow a reasonable scale
- [x] Weight increases for small text (segment labels = medium), decreases for large — partially compliant
- [x] Clear hierarchy: size + weight + color effectively differentiated

---

## 8. Color + Theming

### Color Palette
| Token Name | Hex | Weight | Usage |
|------------|-----|--------|-------|
| Background | ~#F0EDE8 | 100 | Page background, warm off-white |
| Surface | ~#FFFFFF | 50 | Segmented control active bg, cards |
| Surface-hover | ~#E8E4DE | 150 | Hovered item background |
| Text-primary | ~#2C2A26 | 800 | Date headers, page title |
| Text-secondary | ~#8A857D | 400 | Item text, muted content |
| Text-tertiary | ~#A8A29E | 300 | Inactive segment labels |
| Icon-default | ~#9B958D | 400 | Sidebar icons (inactive) |
| Icon-active | ~#6B6560 | 600 | Active sidebar icon |
| Accent-pink | ~#FF1493 | 500 | Active timeline dot |
| Connector | ~#C4BEB6 | 200 | Timeline vertical line + standard dots |
| Sidebar-active-bg | ~#FFFFFF | 50 | Active icon background |

### Color Harmony Analysis
**Monochromatic warm neutrals** — the entire palette is built from warm taupes/beiges in a monochromatic scheme. The single accent color (hot pink/magenta) provides a complementary pop. This is a disciplined, harmonious approach.

### Color Psychology Check
- **Warm neutrals**: Approachable, calm, sophisticated — appropriate for a productivity/AI tool
- **Magenta accent**: Creative, energetic — draws attention to the selected/active item effectively
- Overall tone is clean, minimal, and trustworthy. Good match for intent.

### Semantic Mapping
| Semantic Role | Color | Correct Usage? |
|---------------|-------|----------------|
| Active/Selected | Magenta (#FF1493) | Yes — clear attention marker |
| Disabled/Inactive | Muted taupe | Yes |
| Error/Danger | Not shown | Need to define |
| Success | Not shown | Need to define |

### Color Weight Scale
The design uses an implicit warm-neutral weight scale from light (~#F0EDE8) to dark (~#2C2A26). Not formally defined as 100-800 but the progression is clear and consistent.

### Dark/Light Mode
Light mode shown. Dark mode would need:
- Background: dark warm grey (#1C1A18)
- Surface: slightly lighter (#2C2A26)
- Text: inverted to light weights (300-100)
- Accent pink may need slight brightness increase for dark backgrounds

### Contrast Audit
| Element | FG | BG | Ratio (est.) | WCAG AA (4.5:1) | Pass/Fail |
|---------|----|----|-------------|-----------------|-----------|
| Page title | ~#2C2A26 | ~#F0EDE8 | ~10:1 | 4.5:1 | Pass |
| Date header | ~#2C2A26 | ~#F0EDE8 | ~10:1 | 4.5:1 | Pass |
| Item text | ~#8A857D | ~#F0EDE8 | ~3.2:1 | 4.5:1 | **Fail** |
| Inactive segment | ~#A8A29E | ~#F0EDE8 | ~2.5:1 | 4.5:1 | **Fail** |
| Active segment | ~#2C2A26 | ~#FFFFFF | ~14:1 | 4.5:1 | Pass |
| Hovered item text | ~#2C2A26 | ~#E8E4DE | ~9:1 | 4.5:1 | Pass |

---

## 9. Interaction Design

### State Map
| Element | Default | Hover | Pressed | Focused | Disabled |
|---------|---------|-------|---------|---------|----------|
| Sidebar Icon | Muted grey icon | Slightly darker | Darker + scale | Ring/outline | N/A |
| Sidebar Icon (active) | White bg, darker icon | — | — | — | — |
| Segmented Control | Muted text, no bg | Slight bg tint | — | Ring | — |
| Segment (active) | White bg, dark text | — | — | — | — |
| Timeline Item | No bg, muted text | Grey bg fill, cursor pointer | Slightly darker bg | — | — |
| Timeline Item (selected) | Grey bg, pink dot, pink connector | — | — | — | — |
| Timeline Dot | Grey/taupe | — | — | — | — |
| Timeline Dot (active) | Hot pink filled | — | — | — | — |

### Interaction Rules Audit
- [x] Hover state is subtle (background fill change — ~5-10% shade)
- [ ] Input states: N/A (no inputs visible)
- [ ] Submit button: N/A
- [ ] Toggle: N/A
- [x] Active item clearly distinguishable from inactive

### Transitions + Animation
- Timeline items likely fade or slide in on load
- Hover state: smooth background color transition (~150ms ease)
- Segment switching: smooth underline/background slide
- Pink dot could pulse subtly on selection
- Scrolling: smooth scroll with possible date header sticky behavior

### Gestures
- Scroll: vertical scrolling through timeline
- Click/tap: select a conversation item
- Possible: swipe to delete/archive on mobile

### Micro-interactions
- Dot color transition from grey → pink on hover/select
- Connector line segment could animate pink between dots
- Cursor changes to pointer hand (visible in screenshot)

---

## 10. Content + Copy

### All Visible Text
| Location | Text | Type | Capitalization | Notes |
|----------|------|------|----------------|-------|
| Header | "Timeline" | Page Title | Title case | Static |
| Segment | "Day" | Label | Title case | Active |
| Segment | "Week" | Label | Title case | Inactive |
| Segment | "Month" | Label | Title case | Inactive |
| Date group | "Today" | Date Header | Title case | Dynamic |
| Date group | "Yesterday" | Date Header | Title case | Dynamic |
| Date group | "Feb 8, 2026" | Date Header | Abbreviated month | Dynamic |
| Item | "Asked for a high-protein meal plan" | Body | Sentence case | Dynamic |
| Item | "Worked on the b402 dashboard UX" | Body | Sentence case | Dynamic |
| Item | "Brainstormed side projects" | Body | Sentence case | Dynamic |
| Item | "Researched ETF investing strategies" | Body | Sentence case | Dynamic, selected |
| Item | "Drafted a polite client email" | Body | Sentence case | Dynamic |
| Item | "Asked about improving sleep quality" | Body | Sentence case | Dynamic |
| Item | "Generated tagline ideas for a landing page" | Body | Sentence case | Dynamic |
| Item | "Explained a TypeScript generic error" | Body | Sentence case | Dynamic |
| Item | "Helped rewrite a LinkedIn profile summary" | Body | Sentence case | Dynamic |

### Tone
Casual, conversational, first-person perspective. Summaries describe what the AI helped with, using past tense verbs ("Asked", "Worked on", "Brainstormed", "Researched").

### Copy Quality Audit
- [x] Labels are clear and descriptive
- [x] Sentence case used consistently (not ALL CAPS for full lines)
- [x] Summaries are concise and scannable (single line each)
- [ ] No error messages visible — need to define

### Placeholder/Dynamic Content
All conversation summaries are dynamic/data-driven. Date headers are computed from timestamps. The summary text appears to be AI-generated from conversation content.

### Truncation Rules
Single-line summaries suggest truncation with ellipsis if text exceeds one line. Need to define max character count (~60 chars visible).

---

## 11. Data + Logic

### Data Fields
| Field | Type | Source | Editable | Validation |
|-------|------|--------|----------|------------|
| Conversation ID | UUID | Backend | No | — |
| Summary | String | AI-generated | No | Max ~80 chars |
| Timestamp | DateTime | Backend | No | — |
| Date Group | Computed | From timestamp | No | Today/Yesterday/Date |

### Sorting + Filtering
- **Default sort**: Reverse chronological (newest first)
- **Grouping**: By date (Today → Yesterday → specific dates)
- **Filters**: Day/Week/Month segmented control changes grouping granularity
- **Search**: Not visible — may be needed

### Computed/Derived Values
- Date group labels: "Today", "Yesterday", or formatted date from timestamps
- Relative date bucketing based on current date

### Pagination / Infinite Scroll
Not visible — likely infinite scroll as user scrolls down. Consider adding "Load more" or pagination for performance on large histories.

---

## 12. Accessibility

### Keyboard Navigation
Expected tab order:
1. Sidebar icons (top to bottom)
2. Segmented control (Day → Week → Month)
3. Timeline items (top to bottom)
4. Arrow keys for segment switching and item navigation

### Focus Order
Sidebar → Header controls → Timeline items (top-down)

### Screen Reader
- Sidebar icons need `aria-label` (e.g., "Timeline", "Dashboard", "Analytics")
- Segmented control: `role="tablist"` with `role="tab"` for each segment
- Date headers: `role="heading" aria-level="2"`
- Timeline items: `role="listitem"` within `role="list"`, with `aria-selected` for active item
- Announce: "Timeline, Day view. Today: 3 conversations. Yesterday: 4 conversations."

### Touch Targets
| Element | Current Size | Min Required | Pass/Fail |
|---------|-------------|-------------|-----------|
| Sidebar icons | ~40x40px | 40px desktop / 48px mobile | Pass (desktop) / **Fail (mobile)** |
| Segmented control segments | ~40x32px | 40px desktop | Pass |
| Timeline items | ~full-width x 52px | 48px mobile | Pass |
| Timeline dots | ~8px | 48px mobile | **Fail** (but dots likely not independent targets) |

---

## 13. Implementation Notes

### Recommended Patterns
- **Sidebar**: Use a vertical `NavigationRail` or custom icon rail component
- **Segmented Control**: WinUI `SegmentedControl` or Uno Toolkit `SegmentedControl`/`TabBar`
- **Timeline list**: Grouped `ListView` or `ItemsRepeater` with date group headers
- **Timeline connector**: Custom drawing or stacked layout with absolute-positioned line elements
- **Hover state**: Use `PointerEntered`/`PointerExited` visual state or `VisualStateManager`

### Design Tokens to Extract
```
// Colors
--color-bg:              #F0EDE8
--color-surface:         #FFFFFF
--color-surface-hover:   #E8E4DE
--color-text-primary:    #2C2A26
--color-text-secondary:  #8A857D
--color-text-tertiary:   #A8A29E
--color-icon-default:    #9B958D
--color-icon-active:     #6B6560
--color-accent-pink:     #FF1493
--color-connector:       #C4BEB6

// Spacing (4pt system)
--spacing-unit:   4px
--spacing-xs:     4px
--spacing-sm:     8px
--spacing-md:     16px
--spacing-lg:     24px
--spacing-xl:     32px
--spacing-2xl:    48px

// Typography
--font-family:         'SF Pro', 'Inter', system-ui, sans-serif
--font-size-title:     22px
--font-size-date:      18px
--font-size-body:      16px
--font-size-label:     14px
--font-weight-regular: 400
--font-weight-medium:  500
--font-weight-semibold:600
--font-weight-bold:    700

// Shadows
--shadow-sidebar-icon: 0px 1px 3px rgba(0,0,0,0.08)

// Radii
--radius-sm:  4px
--radius-md:  8px
--radius-lg:  12px
--radius-xl:  16px

// Component sizes
--input-height-sm: 32px
--input-height-md: 40px
--input-height-lg: 48px
--button-padding-v: 1em
--button-padding-h: 2em
```

### Edge Cases
- Very long conversation summaries: truncate with ellipsis at ~60 chars
- No conversations: show empty state with illustration + CTA
- Hundreds of conversations: virtualized list with incremental loading
- Same-day multiple conversations: ensure ordering is correct
- Date formatting across locales
- Conversation deleted/archived: remove from timeline with animation

### Dev Gotchas
- The S-curved connector line between date groups is decorative but complex — consider using SVG paths or simplifying to straight lines
- The pink segment of the connector on hover requires tracking which segment connects to the hovered item
- Sidebar icon active state has a subtle shadow/elevation — needs careful layering
- Background color is warm grey, not pure white — ensure all surfaces account for this
- The segmented control border/outline is very subtle — may need careful border handling

---

## 14. Design Quality Audit

### Scorecard
| Category | Rating | Key Findings |
|----------|--------|-------------|
| Color Harmony | **Good** | Monochromatic warm palette with single accent — disciplined and cohesive |
| Color Contrast (WCAG) | **Fair** | Item text and inactive segments fail AA contrast ratio |
| Color Weight System | **Fair** | Implicit scale present but not formally defined as 100-800 |
| Typography Scale | **Good** | Clean 4-level hierarchy, reasonable sizes |
| Typography Hierarchy | **Good** | Clear differentiation via size, weight, and color |
| Spacing Consistency | **Good** | Appears to follow 4pt/8pt baseline system consistently |
| Component Standards | **Good** | Components are minimal and well-executed |
| Visual Hierarchy | **Good** | F-pattern compatible, title → date → items hierarchy is clear |
| Button Hierarchy | **Good** | Only one interactive region type (segmented control), no competing CTAs |
| Icon Consistency | **Good** | All sidebar icons appear to be outline style, consistent sizing |
| Negative Space | **Good** | Generous whitespace, items breathe well, not cluttered |
| Alignment | **Good** | Consistent left-alignment, dots and text align on vertical axis |
| Accessibility | **Fair** | Contrast failures on secondary text, sidebar icons borderline for mobile touch targets |

### Violations Found
| # | Rule Violated | Element | Severity | Recommendation |
|---|---------------|---------|----------|----------------|
| 1 | Contrast minimum (4.5:1) | Item text (~#8A857D on ~#F0EDE8) | **Critical** | Darken item text to at least #706B63 for 4.5:1 ratio |
| 2 | Contrast minimum (4.5:1) | Inactive segment labels (~#A8A29E on ~#F0EDE8) | **Major** | Darken to ~#7A756E or increase font size above 18px for large text exception |
| 3 | Touch targets (48px mobile) | Sidebar icons (~40px) | **Major** | Increase to 48px for mobile targets, 40px acceptable for desktop only |
| 4 | Color weight system | Overall palette | **Minor** | Formalize the warm neutral palette into explicit 100-800 weight tokens |
| 5 | Missing states | Empty, loading, error | **Major** | Design empty state, skeleton loading, and error recovery screens |
| 6 | Icon labels | Sidebar icons have no text labels | **Minor** | Consider tooltips on hover or labels on expanded sidebar per icon rules ("add labels when possible") |

### What's Working Well
- **Warm monochromatic palette** — sophisticated, calm, distinctive from competitor products
- **Single accent color** — magenta dot is an elegant, restrained use of color that draws the eye exactly where needed
- **Typography hierarchy** — 4 clear levels with good differentiation
- **Generous negative space** — items are not crowded, the design breathes
- **Timeline metaphor** — the vertical connector with dots is visually clear and adds character
- **Consistent spacing** — appears to follow baseline grid throughout
- **Minimalist sidebar** — clean icon rail without visual clutter
- **Hover state** — subtle background fill is appropriately restrained (5-10% change)
- **Date grouping** — smart use of relative dates (Today, Yesterday) transitioning to absolute dates

---

## 15. Open Questions

| # | Question | Area | Priority | Default Assumption |
|---|----------|------|----------|--------------------|
| 1 | What does the Day/Week/Month segmented control actually change? Grouping? Filtering? | Interaction | High | Changes date grouping granularity |
| 2 | What happens on item click — navigate to conversation, or expand inline? | Navigation | High | Navigate to full conversation view |
| 3 | Is there a search/filter capability for the timeline? | Feature | High | Not currently, but likely needed |
| 4 | What do each of the 8 sidebar icons represent? | Navigation | High | Assumed from visual similarity to common patterns |
| 5 | Is right-click / long-press context menu supported (rename, delete, pin)? | Interaction | Medium | Yes, likely needed for conversation management |
| 6 | Does the pink accent color persist for the "last viewed" item or is it hover-only? | Visual state | Medium | Hover only (cursor is present) |
| 7 | Is dark mode required? | Theming | Medium | Likely yes given modern product expectations |
| 8 | How are conversation summaries generated — AI-generated or user-named? | Content | Medium | AI-generated with option to rename |
| 9 | What font family is used? | Typography | Low | System sans-serif (SF Pro / Inter) |
| 10 | Is the S-curve in the connector intentional design or artistic flourish? | Visual | Low | Intentional — adds organic feel to timeline |

---

## Appendix: Raw Measurements

- Page background: warm off-white ~#F0EDE8
- Sidebar width: ~56px
- Sidebar icon active background: ~40x40px, ~8px border-radius
- Content left edge to sidebar right: ~48px
- Page title font size: ~22px, semi-bold
- Segmented control: ~120x32px, positioned top-right of header
- Date header font: ~18px bold, ~#2C2A26
- Item text font: ~16px regular, ~#8A857D
- Timeline dot diameter: ~8px
- Active dot color: ~#FF1493 (hot pink / deep pink)
- Standard dot color: ~#C4BEB6
- Connector line: ~1px, ~#C4BEB6
- Item row height: ~52px center-to-center
- Hover background: ~#E8E4DE, ~8px border-radius, extends full width of content area
- Date group top spacing: ~32px
