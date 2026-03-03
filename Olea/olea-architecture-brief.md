# OLEA — Olive Oil Tasting Journal

## Architecture Brief

Logic, Navigation, Data & Behavior Specification · Version 1.0 — February 2026

---

## Table of Contents

1. Application Overview
2. Data Models
3. Application State
4. Navigation & Routing
5. Tab 1: New Tasting — Behavior & Logic
6. Flavor Wheel — Rendering Algorithm
7. Flavor Wheel — Interaction Logic
8. Intensity Slider — Logic
9. Save Tasting — Workflow
10. Tab 2: Journal — Behavior & Logic
11. Tab 3: Explore — Behavior & Logic
12. Toast Notification System
13. Lifecycle & Initialization
14. Persistence Strategy
15. Testing Checklist

---

## 1. Application Overview

### PURPOSE

A personal olive oil tasting journal that lets users record tasting sessions, tag flavor notes via an interactive flavor wheel, rate oils, and explore olive oil regions. Single-user, local-first.

### SCREEN COUNT

One shell with three swappable content tabs: (1) New Tasting, (2) My Journal, (3) Explore Regions. No sub-pages, modals, or drill-down views in v1.

### ARCHITECTURE PATTERN

MVVM recommended for XAML-based implementations. The app has a thin ViewModel layer (form state, journal collection, selected tab) and a static data layer (flavor wheel data, region data). For web implementations, a simple component-based architecture with lifted state is sufficient.

---

## 2. Data Models

### 2.1 FlavorNote

| Field | Type | Description |
|---|---|---|
| Name | string | Display name, e.g. "Green Apple", "Black Pepper" |
| Color | string (hex) | Background color for chip display, e.g. "#7BA85A" |

### 2.2 IntensityProfile

| Field | Type | Range | Default |
|---|---|---|---|
| Fruity | int | 0–10 | 5 |
| Bitter | int | 0–10 | 3 |
| Pungent | int | 0–10 | 4 |

### 2.3 TastingEntry

| Field | Type | Required | Notes |
|---|---|---|---|
| Id | string/GUID | Auto | Generated on save. Unique identifier. |
| Name | string | Yes | Oil brand/name. Must be non-empty to save. |
| Origin | string | No | Defaults to "Unknown" if empty. |
| Cultivar | string | No | Olive variety. |
| HarvestDate | string (YYYY-MM) | No | Month picker format. |
| TastingDate | string (YYYY-MM-DD) | No | Defaults to today if empty. |
| Rating | int | No | 0–5. 0 = unrated. |
| Flavors | List\<FlavorNote\> | No | Selected from wheel. Can be empty. |
| Intensities | IntensityProfile | No | Captured from sliders. |
| Notes | string | No | Free-text tasting notes. |

### 2.4 FlavorCategory (static data)

| Field | Type | Description |
|---|---|---|
| Name | string | Category name, e.g. "Fruity" |
| Color | string (hex) | Segment fill color |
| Children | List\<FlavorNote\> | Subcategory flavors with their own colors |

### 2.5 Region (static data)

| Field | Type | Description |
|---|---|---|
| Flag | string | Emoji flag character (e.g. 🇮🇹) |
| Name | string | Region name |
| Area | string | Geographic context |
| Description | string | 1–2 sentence flavor profile description |
| Cultivars | List\<string\> | Olive cultivar names |

---

## 3. Application State

### GLOBAL STATE

| State Variable | Type | Initial Value | Scope |
|---|---|---|---|
| ActiveTab | enum {New, Journal, Explore} | New | Shell-level |
| Journal | ObservableList\<TastingEntry\> | Seeded with 4 demo entries | App-level |

### NEW TASTING FORM STATE (SCOPED TO TAB 1)

| State Variable | Type | Initial Value | Reset On Save |
|---|---|---|---|
| OilName | string | "" | Yes → "" |
| OilOrigin | string | "" | Yes → "" |
| OilCultivar | string | "" | Yes → "" |
| HarvestDate | string | "" | No (platform clears) |
| TastingDate | string | "" | No (platform clears) |
| CurrentRating | int | 0 | Yes → 0 |
| SelectedFlavors | List\<FlavorNote\> | [] | Yes → [] |
| Intensities | IntensityProfile | {5, 3, 4} | No (keeps last values) |
| OilNotes | string | "" | Yes → "" |

---

## 4. Navigation & Routing

### PATTERN

Single-page application with tab-based content switching. No URL routing required. Only one content section is visible at a time; the other two are hidden (not destroyed — form state persists when switching tabs).

### TAB SWITCH PROCEDURE

1. Set ActiveTab to the new tab value (New | Journal | Explore)
2. Update nav link visual states: active link gets olive-dark color + gold underline; others revert to text-muted
3. Update tab pill visual states: active tab gets olive-dark filled style; others revert to outline
4. Update page header title and subtitle (see table below)
5. Show the matching section with fadeUp entrance animation (0.6s); hide others
6. If switching to Journal: re-render the journal card grid (reflects any new saves)

### PAGE HEADER CONTENT PER TAB

| Tab | Title | Subtitle |
|---|---|---|
| New | Record a Tasting | Capture every nuance of your olive oil experience |
| Journal | Your Journal | {count} tasting(s) recorded (dynamic) |
| Explore | Explore Regions | Discover the world's great olive oil terroirs |

### DUAL SYNC

Both the nav links (in the top bar) and the tab pills (in the content area) trigger the same tab switch. When either is clicked, both update visually to stay in sync.

---

## 5. Tab 1: New Tasting — Behavior & Logic

### FORM VALIDATION

Only the Name field is required. If the user clicks Save and Name is empty, focus the Name input and abort. No error message is shown (focus is sufficient feedback).

### STAR RATING INTERACTION

1. User clicks star at position N (1–5)
2. Set CurrentRating = N
3. For each star S in [1..5]: if S <= N, fill = gold; else fill = border color

Stars are not toggle-able (clicking star 3 when rating is 3 does NOT clear to 0 — it stays at 3). To implement clear-on-reclick, that would be a v2 enhancement.

---

## 6. Flavor Wheel — Rendering Algorithm

### INPUT CONSTANTS

- **centerX, centerY =** 200, 200
- **innerRadius =** 60 (center circle edge + 2px gap = 62 for ring start)
- **midRadius =** 125 (end of category ring)
- **outerRadius =** 175 (end of subcategory ring)
- **totalCategories =** 8
- **gapDegrees =** 1.2 (between categories)
- **subGapDegrees =** 0.5 (between subcategories)

### ALGORITHM (PSEUDOCODE)

```
sliceAngle = (360 - gapDegrees * totalCategories) / totalCategories
currentAngle = -90  // Start at 12 o'clock

FOR EACH category IN wheelData:
  startAngle = currentAngle
  endAngle = startAngle + sliceAngle

  // Draw category segment (inner ring)
  path = arcPath(cx, cy, innerR+2, midR, startAngle, endAngle)
  Draw filled path with category.color at 88% opacity
  Place category label at midpoint angle, radius = (innerR + midR) / 2 + 4

  // Draw subcategory segments (outer ring)
  subAngle = sliceAngle / category.children.length
  FOR EACH child IN category.children (index i):
    subStart = startAngle + i * subAngle
    subEnd = subStart + subAngle - subGapDegrees
    subPath = arcPath(cx, cy, midR+2, outerR, subStart, subEnd)
    Draw filled path with child.color at 82% opacity
    Place child label at midpoint angle, radius = (midR + outerR) / 2 + 2

  currentAngle = endAngle + gapDegrees
```

### arcPath FUNCTION

Given (cx, cy, innerRadius, outerRadius, startAngleDeg, endAngleDeg):

Convert angles to radians. Calculate 4 corner points (2 on inner arc, 2 on outer arc). Return a closed path: MoveTo(innerStart) → LineTo(outerStart) → Arc(outerEnd) → LineTo(innerEnd) → Arc(innerStart) → Close. Use large-arc-flag = 1 if angle span > 180°.

---

## 7. Flavor Wheel — Interaction Logic

### toggleFlavor(name, color)

1. Search SelectedFlavors for an entry with matching name
2. If found: remove it (deselect)
3. If not found: add { name, color } to SelectedFlavors
4. Re-render the selected flavors tag area
5. Update wheel segment visual states (selected segments get brightness/saturation filter)

### SELECTING BOTH CATEGORY AND CHILD

Categories and subcategories are independent selections. The user can select "Fruity" (the category) AND "Green Apple" (a child) simultaneously — they are separate flavor tags. There is no parent-child coupling in the selection model.

### RENDERING SELECTED FLAVORS

If SelectedFlavors is empty: show italic placeholder text "No flavors selected yet". Otherwise: render a wrapping horizontal row of FlavorTag chips. Each chip shows the flavor name, uses the flavor's color as background, and has an × remove button that calls toggleFlavor to deselect.

---

## 8. Intensity Slider — Logic

### ON CLICK/TAP

1. Get the click/tap X coordinate relative to the track element's left edge
2. Calculate percentage: clamp((clickX - trackLeft) / trackWidth, 0, 1)
3. Convert to integer: round(percentage × 10)
4. Update the matching intensity in state (e.g. Intensities.Fruity = value)
5. Update fill bar width to (value × 10)%
6. Update the numeric display to the new value

### DRAG SUPPORT (V2 ENHANCEMENT)

The prototype only handles click/tap. A production version should support drag along the track for continuous adjustment, using pointer-down + pointer-move + pointer-up events.

---

## 9. Save Tasting — Workflow

### TRIGGER

User clicks the "Save to Journal" button.

### PROCEDURE

1. **Validate:** Trim OilName. If empty, focus the Name input field and STOP.
2. **Build entry:** Create a new TastingEntry object from current form state. Generate unique Id. Default Origin to "Unknown" if empty. Default TastingDate to today's date if empty. Deep-copy SelectedFlavors and Intensities.
3. **Prepend to Journal:** Insert the new entry at position 0 (newest first).
4. **Reset form:** Clear OilName, OilOrigin, OilCultivar, OilNotes to empty strings. Set CurrentRating to 0. Clear all star active states. Clear SelectedFlavors to empty list. Update wheel visual states (deselect all). Do NOT reset intensities (they persist as useful defaults).
5. **Show toast:** Display "Tasting saved to your journal" toast notification.

### POST-SAVE NOTE

The user stays on the New Tasting tab after saving. They can switch to Journal tab to see their new entry at the top of the grid.

---

## 10. Tab 2: Journal — Behavior & Logic

### RENDERING

On every switch to the Journal tab, re-render the grid from the current Journal list. This ensures newly saved entries appear immediately.

### EMPTY STATE LOGIC

IF Journal.length == 0: render the empty state (olive emoji, title, subtitle). ELSE: render the card grid.

### DATE FORMATTING

TastingDate string (YYYY-MM-DD) is formatted for display as "MMM D, YYYY" (e.g. "Jan 15, 2025"). Use locale-aware formatting where available.

### ORIGIN DISPLAY

Show "{origin}" if no cultivar. Show "{origin} · {cultivar}" if cultivar exists. The middle-dot is a · character with spaces.

### INTENSITY BAR WIDTH

Each intensity bar fill width = intensity value × 10 (as percentage). So a value of 7 = 70% width. Value of 0 = 0% (empty track visible).

### CARD CLICK (V2)

In the prototype, cards are clickable (cursor: pointer) but have no drill-down behavior. A v2 could open a detail/edit view.

---

## 11. Tab 3: Explore — Behavior & Logic

### RENDERING

On each switch to the Explore tab, render the region cards from the static region data. In practice this data never changes, so rendering once on first visit and caching is acceptable.

### DATA SOURCE

6 hardcoded regions (see Design Brief Section 12 and 15 for full data). This is static reference content — no user interaction beyond viewing and card hover effects.

### FUTURE ENHANCEMENT

A v2 could make region cards tappable to show a detail view with a map, recommended oils from that region, and links to journal entries tagged with that origin.

---

## 12. Toast Notification System

### API

`showToast(message: string)` — a single global function/method.

### LIFECYCLE

1. Set toast text content to the message
2. Add "show" state: animate from translateY(+80px), opacity 0 → translateY(0), opacity 1 with spring easing over 0.4s
3. Start a 2500ms timer
4. On timer expiry: remove "show" state, reversing the animation

### CONCURRENT TOASTS

Only one toast at a time. If showToast is called while a toast is visible, replace the text and restart the timer. No stacking.

---

## 13. Lifecycle & Initialization

### APP STARTUP SEQUENCE

1. **Seed demo data:** Populate Journal with 4 hardcoded TastingEntry objects (see Design Brief Section 15)
2. **Build flavor wheel:** Programmatically generate the wheel SVG/Canvas from the FlavorCategory data
3. **Set initial tab:** ActiveTab = New, render the New Tasting view
4. **Trigger entrance animations:** Page header fadeUp (0s), tab bar fadeUp (0.1s delay)

### TAB LIFECYCLE

Tabs are NOT destroyed on switch. Form state persists across tab changes. The wheel is built once at startup and never rebuilt. Journal and Explore are re-rendered on each switch (lightweight since they're just data → template).

---

## 14. Persistence Strategy

### V1: IN-MEMORY ONLY

All data lives in memory for the prototype. Journal entries are lost on app restart. Demo data is re-seeded on each launch.

### V2: LOCAL PERSISTENCE

Serialize the Journal list to JSON and persist to platform-appropriate local storage:

- **Web:** localStorage or IndexedDB
- **Desktop/Mobile (.NET):** ApplicationData local folder, JSON file, or SQLite via Entity Framework Core
- **Cross-platform (.NET MAUI / Uno):** IPreferences for simple key-value, or SQLite for structured data

### V3: CLOUD SYNC

Optional future enhancement: sync journal entries across devices via a backend API or cloud service (e.g. Azure Cosmos DB, Firebase, Supabase).

---

## 15. Testing Checklist

An implementing agent should verify all of the following:

### NAVIGATION

- [ ] Clicking tab pills switches content sections correctly
- [ ] Clicking nav links switches content sections and syncs tab pills
- [ ] Page title and subtitle update on each tab switch
- [ ] Section entrance animation plays on each switch

### NEW TASTING FORM

- [ ] All text inputs accept input and display placeholder text
- [ ] Date pickers open native date/month controls
- [ ] Star rating: clicking star N fills stars 1..N, clears N+1..5
- [ ] Star hover shows scale-up effect
- [ ] Save with empty name focuses the name input and does not save
- [ ] Save with valid name adds entry to journal and resets form
- [ ] Toast appears on successful save

### FLAVOR WHEEL

- [ ] All 8 category segments and 25 subcategory segments render correctly
- [ ] Labels are visible and positioned at midpoints of arcs
- [ ] Clicking a segment adds/removes it from selected flavors
- [ ] Selected segment visual state changes (brightness/saturation)
- [ ] Flavor tags appear below wheel with correct colors
- [ ] Remove button on flavor tags works
- [ ] Placeholder text appears when no flavors selected

### INTENSITY SLIDERS

- [ ] Clicking on track sets value proportional to click position
- [ ] Fill bar width updates with animation
- [ ] Numeric value display updates
- [ ] Each slider is independent (Fruity, Bitter, Pungent)

### JOURNAL

- [ ] Seed data renders 4 cards on first visit
- [ ] New saves appear at the top of the grid
- [ ] Empty state shows when all entries are removed (if delete is implemented)
- [ ] Card hover lifts with shadow transition
- [ ] Dates format correctly
- [ ] Origin · Cultivar displays correctly (with and without cultivar)
- [ ] Intensity bars reflect saved values

### EXPLORE

- [ ] All 6 region cards render with correct data
- [ ] Card hover animation works
- [ ] Cultivar tags display correctly

### RESPONSIVE

- [ ] At <= 900px: grids collapse to single column, wheel shrinks, nav padding reduces
- [ ] Journal grid auto-reflows based on available width

---

*— End of Architecture Brief —*
