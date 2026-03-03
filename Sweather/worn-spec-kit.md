# Worn — Spec Kit

**Product:** Worn — Weather by Wardrobe
**Version:** 1.0
**Date:** February 2026
**Status:** Draft

---

# Document 1: Design Brief

## 1.1 Product Summary

Worn is a weather application that communicates forecasts exclusively through clothing recommendations. It never displays temperatures, percentages, or traditional meteorological terms to the end user. Instead, weather data is translated into outfit tiers, garment suggestions, fabric recommendations, and personality-driven copy.

**Target Users:**
- People who check the weather each morning to decide what to wear (the majority use case for consumer weather apps)
- Users fatigued by data-heavy weather interfaces who want a quick, actionable answer
- Fashion-conscious individuals who appreciate personality and editorial tone in utility apps

**Top 3 Jobs to Be Done:**
1. "Tell me what to wear right now so I can walk out the door" (immediate outfit recommendation)
2. "Help me plan what to pack/prep for the rest of today" (hourly garment timeline)
3. "Give me a heads-up on what the week looks like so I'm not caught off guard" (7-day outfit forecast with alerts)

## 1.2 Screen Breakdown

The application is a single-screen, vertically-scrolled layout with five distinct content zones. There is no multi-page navigation in v1.

### Screen: Main (Single Page)

#### Zone 1 — Navigation Bar

**Purpose:** Brand identity + location context.

**Components:**
| Component | Description |
|-----------|------------|
| Logo | "worn" wordmark, serif typeface, italic accent on the "o" |
| Location Pill | Dark pill showing pin icon + reverse-geocoded city/state. Tappable. |

**Behaviors:**
- Location pill displays the user's auto-detected location on load.
- Tapping the location pill is reserved for future manual location override (v1: display only, no interaction beyond visual hover state).
- Sticky/fixed positioning is NOT used; nav scrolls with content.

**States:**
| State | Behavior |
|-------|----------|
| Loading | Location pill shows pulsing skeleton placeholder, text "Locating..." |
| Resolved | Displays "City, State" (or "City, Country" outside US) |
| Geo Denied | Displays "Location unavailable" with muted styling. See Error States. |
| Geo Error | Same as Geo Denied |

**Accessibility:**
- Location pill must have `aria-label="Current location: {city, state}"`
- Logo is decorative, does not need alt text if implemented as styled text

---

#### Zone 2 — Hero Section

**Purpose:** The single most important answer — "what should I wear right now?"

**Layout:** Two-column on wide viewports (≥1024px), single-column stacked on narrow.

##### Left Column: Headline + Description

**Components:**
| Component | Description |
|-----------|------------|
| Tagline | Time-of-day + day-of-week contextual label (e.g., "Monday morning mood"). Handwritten-style typeface. |
| Headline | Large serif headline in the format "It's a [outfit tier] kind of day." The outfit tier phrase is the highlighted/accented word. 4–6 variants per tier, randomly selected per load. |
| Description | 2–3 sentence prose paragraph explaining the outfit rationale in conversational tone. Derived from current conditions + modifier context. |
| Fabric Tags | Horizontal row of pill-shaped tags with colored dots. Each tag represents a condition-derived property (e.g., "cotton-friendly", "hair-safe", "closed-toe"). |

**Behaviors:**
- Headline text is dynamically assembled: the structure "It's a [X] kind of day." is fixed; the [X] slot is populated by the mapping engine's headline output.
- Fabric tags are conditionally rendered — only tags whose conditions evaluate true are shown. The number of visible tags varies (typically 2–5).
- All content in this zone animates in on load with a staggered fade-up (200ms delay offset between tagline → headline → description → tags).

**States:**
| State | Behavior |
|-------|----------|
| Loading | Skeleton placeholders for headline (2 lines), description (3 lines), tags (3 pills). Subtle shimmer animation. |
| Loaded | Populated with engine output. Fade-up entrance. |
| Error | Fallback static content: headline "Check back soon", description "We're having trouble getting weather data right now." No fabric tags shown. |

##### Right Column: Outfit Memo Card

**Components:**
| Component | Description |
|-----------|------------|
| Card Container | Rounded rectangle (24px radius) with subtle shadow. Section label "TODAY'S OUTFIT MEMO" in small caps. |
| Outfit Item Row | Repeating row: emoji icon (in colored rounded square) + garment name (serif) + description (sans-serif, muted) + necessity label (handwritten-style, accent color). Items separated by subtle 1px dividers. |

**Outfit Item Data Shape:**
```
{
  emoji: string       // e.g., "🧥"
  name: string        // e.g., "Lightweight Jacket"
  desc: string        // e.g., "Denim, canvas, or a soft bomber"
  necessity: string   // e.g., "must-have", "smart pick", "maybe"
}
```

**Necessity Levels (in descending urgency):**
`survival` → `non-negotiable` → `must-have` → `smart pick` → `go-to` → `safe bet` → `easy pick` → `nice touch` → `maybe`

**Behaviors:**
- Item count varies by tier + modifiers (typically 3–6 items).
- Items have a subtle horizontal slide on hover/focus (6px translateX).
- No tap/click action on items in v1.

**States:**
| State | Behavior |
|-------|----------|
| Loading | Card with 4 skeleton rows |
| Loaded | Populated, staggered entrance (items appear sequentially, ~100ms offset each) |
| Empty | Should never occur — engine always returns at least 3 items per tier |
| Error | Card shows "Couldn't load outfit details" in muted text |

**Responsiveness:**
- ≥1024px: Two-column grid, 1fr 1fr, 60px gap
- <1024px: Single column, left content stacks above right content
- <640px: Reduced padding (48px → 20px)

**Accessibility:**
- Outfit card should be an `<article>` or `<section>` with `aria-label="Today's outfit recommendation"`
- Each outfit item row should be semantically grouped (name + description as label, necessity as badge)
- Necessity labels should have `aria-label` clarifying urgency (e.g., aria-label="Necessity level: must-have")

---

#### Zone 3 — Hourly Wardrobe Strip

**Purpose:** Show how the outfit recommendation evolves throughout the day. Helps users plan for temperature transitions.

**Components:**
| Component | Description |
|-----------|------------|
| Section Header | Serif title "Your wardrobe, hour by hour" + right-aligned "Scroll →" hint |
| Hour Card | Vertical card: time label (uppercase, small) + emoji (large) + garment headline (serif) + vibe descriptor (italic, muted) |
| "Now" Card | Visually distinct variant with inverted colors (dark background, light text) and gold-colored time label |

**Hour Card Data Shape:**
```
{
  time: string        // ISO datetime, displayed as "7 AM", "1 PM", etc.
  emoji: string       // Single emoji
  garment: string     // e.g., "Jacket On", "Re-Layer", "Shed a Layer"
  vibe: string        // e.g., "brisk commute", "warming up", "evening chill"
  tierId: string      // Used for potential theming
  isRaining: boolean
  isSnowing: boolean
}
```

**Behaviors:**
- Horizontal scroll container showing 8 hour cards. Scrollbar hidden.
- "Now" card is the card matching the current hour (or nearest past hour in the data).
- Garment headlines are transition-aware: when the outfit tier changes between adjacent hours, the card uses a transition label ("Re-Layer", "Shed a Layer", "Umbrella Up") instead of the static tier garment.
- "Scroll →" hint text is static (no auto-scroll behavior in v1).

**States:**
| State | Behavior |
|-------|----------|
| Loading | 8 skeleton cards in horizontal scroll |
| Loaded | Cards populated, "Now" card visually highlighted |
| Fewer than 8 hours remaining in day | Show available hours only; don't pad with next-day data in v1 |
| Error | Section hidden entirely |

**Responsiveness:**
- Cards have fixed width (155px) at all breakpoints.
- Container is always horizontally scrollable regardless of viewport width.

**Accessibility:**
- Scroll container needs `role="region"` with `aria-label="Hourly outfit forecast"`
- "Now" card should have `aria-current="time"`
- Consider left/right arrow key navigation within the strip for keyboard users
- Scroll hint should be `aria-hidden="true"` (decorative)

---

#### Zone 4 — Alert Ribbon

**Purpose:** Surface upcoming weather events that require wardrobe preparation, translated into clothing language.

**Components:**
| Component | Description |
|-----------|------------|
| Alert Container | Rounded rectangle with gradient background (terracotta tones). Horizontal layout: icon + title + description. |
| Alert Icon | Large emoji (e.g., ☂️, 🌨️, 🔥, 🥶, 💨, ☀️) |
| Alert Title | Serif, e.g., "Umbrella Advisory for Wednesday" |
| Alert Description | Prose sentence with clothing-specific advice |

**Alert Data Shape:**
```
{
  icon: string     // Emoji
  title: string    // e.g., "Umbrella Advisory for Wednesday"
  desc: string     // e.g., "Expect full rain gear territory..."
}
```

**Alert Types:**
| Type | Trigger | Icon |
|------|---------|------|
| Rain | precip_probability_max > 70% AND precip_sum > 5mm | ☂️ |
| Snow | weather_code 71–77 | 🌨️ |
| Extreme Cold | "survival" tier | 🥶 |
| Extreme Heat | "scorcher" tier | 🔥 |
| High UV | uv_index_max ≥ 8 | ☀️ |
| Wind | wind_speed_max > 35mph | 💨 |

**Behaviors:**
- Multiple alerts can be generated. In v1, display only the first (highest priority). Priority order: snow > extreme cold > rain > extreme heat > wind > UV.
- Alerts only reference future days (not today).
- If no alerts exist, the entire zone is hidden — no empty state.
- Entrance animation: fade-up from below.

**States:**
| State | Behavior |
|-------|----------|
| Has alerts | First alert displayed |
| No alerts | Zone hidden entirely (no DOM element or collapsed with display:none) |
| Error | Zone hidden |

**Accessibility:**
- Container should have `role="alert"` or `role="status"`
- Alert title + description should be a single readable unit for screen readers

---

#### Zone 5 — Weekly Outfit Forecast

**Purpose:** 7-day planning view. Each day is a flip card with a front (outfit summary) and back (styling tip).

**Components:**
| Component | Description |
|-----------|------------|
| Section Header | Serif title "The week in outfits" + right-aligned "7-day wardrobe forecast" |
| Day Card (Front) | Day abbreviation (uppercase, small) + emoji + outfit headline (serif) + fabric types (muted) + color swatch bar |
| Day Card (Back) | Label (uppercase, small, gold) + styling tip (italic serif) + fabric detail (muted) + "tap to flip back" hint |
| "Today" Card | Visually distinct with inverted colors (dark background, light text). Back side uses terracotta background. |
| Flip Hint | Centered text below the grid: "tap any day to reveal styling notes" |

**Day Card Front Data Shape:**
```
{
  date: string            // ISO date
  emoji: string
  headline: string        // e.g., "Sweater Weather"
  fabrics: string[]       // e.g., ["wool", "knit"]
  swatchColor: string     // Hex color for the swatch bar
}
```

**Day Card Back Data Shape:**
```
{
  backLabel: string          // e.g., "Styling Note", "Survival Tip", "Rain Tip"
  tip: string                // e.g., "Chunky knit, warm scarf, that cozy main-character energy"
  backFabricDetail: string   // e.g., "exposed ankles at your own risk"
}
```

**Behaviors:**
- Tap/click toggles the card between front and back states.
- Flip animation: 3D Y-axis rotation (180°) with `perspective: 800px`. Duration: 600ms, ease-out cubic bezier.
- Both faces use `backface-visibility: hidden`.
- Only one card can be flipped at a time: NO — multiple cards can be flipped simultaneously in v1.
- The "today" card is always the first card (leftmost).
- The flip hint text fades in on load with a delayed animation (1.6s delay).

**States:**
| State | Behavior |
|-------|----------|
| Loading | 7 skeleton cards in grid |
| Loaded | Cards populated with front face visible |
| Flipped | Card rotated 180° showing back face |
| Error | Section shows "Couldn't load weekly forecast" message |

**Responsiveness:**
| Breakpoint | Grid |
|-----------|------|
| ≥1024px | 7 columns (1fr each) |
| 640–1023px | 4 columns |
| <640px | 2 columns |

**Accessibility:**
- Each card should be `role="button"` with `aria-expanded="false"` (front) / `aria-expanded="true"` (back)
- Back content should be `aria-hidden="true"` when front is showing, and vice versa
- Keyboard: Enter/Space to toggle flip
- The flip hint should be associated with the card group via `aria-describedby`

---

#### Zone 6 — Footer

**Purpose:** Brand reinforcement + tagline.

**Components:**
- Small logo wordmark
- Tagline: "dressing you for the world outside — no degrees, no percentages, just vibes."

**Behaviors:** Static. No interactions.

---

## 1.3 Interaction Rules

### Navigation
- Single-page scroll. No routing, no page transitions in v1.
- All content zones are vertically stacked and scroll together.

### Gestures
| Gesture | Context | Action |
|---------|---------|--------|
| Tap | Day card | Toggle flip animation |
| Horizontal swipe/scroll | Hourly strip | Scroll through hour cards |
| Hover | Outfit item row | 6px horizontal slide |
| Hover | Location pill | Darken + slight lift |
| Hover | Day card (front) | Elevation increase (shadow) |

### Transitions
| Trigger | Animation | Duration | Easing |
|---------|-----------|----------|--------|
| Page load | Staggered fade-up for all zones | 800ms–1400ms | ease-out, 200ms stagger |
| Data loaded | Content replaces skeleton | 300ms crossfade | ease-in-out |
| Day card flip | 3D Y-rotation | 600ms | cubic-bezier(0.4, 0, 0.2, 1) |
| Outfit item hover | translateX | 300ms | ease |

### Validation
- No user input in v1 (no forms, no search). Validation is limited to geolocation permission handling.

---

## 1.4 Visual System

### Typography

| Role | Family | Weight | Size | Tracking | Case |
|------|--------|--------|------|----------|------|
| Display / Headlines | Playfair Display (serif) | 700 | clamp(52px, 6vw, 80px) | -1.5px | Sentence |
| Section Titles | Playfair Display (serif) | 400 | 28px | 0 | Sentence |
| Card Titles | Playfair Display (serif) | 400 | 15–18px | 0 | Sentence |
| Tagline / Handwritten | Caveat (cursive) | 400–600 | 15–18px | 0.5px | Sentence |
| Body / UI | DM Sans (sans-serif) | 300–500 | 13–17px | 0–1.5px | Mixed |
| Labels / Metadata | DM Sans (sans-serif) | 500 | 11–12px | 1.5–2.5px | Uppercase |

**Font loading strategy:** Google Fonts with `display=swap`. Fallback stack: system serif for Playfair, system sans-serif for DM Sans, cursive for Caveat.

### Color Tokens

| Token | Hex | Usage |
|-------|-----|-------|
| `--cream` | #F5F0E8 | Page background |
| `--warm-black` | #1A1714 | Primary text, "today" card background |
| `--terracotta` | #C4654A | Accent color, highlights, necessity labels |
| `--sage` | #8B9E7E | Fabric tag dots, swatches |
| `--slate` | #6B7B8D | Muted text, secondary labels |
| `--blush` | #E8CFC0 | Swatches, soft accent |
| `--denim` | #4A5E78 | Swatches, tag dots |
| `--gold` | #C9A96E | "Now" and "Today" accent text, swatches |
| `--linen` | #EDE6D6 | Card backgrounds |
| `--charcoal` | #3A3632 | Secondary text, hover states |

### Spacing System

Base unit: 4px. Common increments:
- 8px (tag gaps, small padding)
- 12px (card inner gaps)
- 16px (card gap in grids)
- 20px (item row padding, inter-component)
- 24px (section heading margin-bottom)
- 28px (card internal padding vertical)
- 36px (description margin-bottom)
- 48px (section horizontal padding, card internal padding)
- 60px (inter-section vertical spacing, hero column gap)

### Elevation / Shadows

| Level | Shadow | Usage |
|-------|--------|-------|
| Resting card | `0 20px 60px rgba(26,23,20,0.08)` | Outfit memo card |
| Card hover | `0 16px 40px rgba(26,23,20,0.1)` | Day cards on hover |
| Hour card hover | `0 12px 32px rgba(26,23,20,0.08)` | Hourly strip cards |

### Border Radius

| Context | Radius |
|---------|--------|
| Cards (outfit memo, day cards, hour cards) | 20–24px |
| Location pill, fabric tags | 20–24px (full-round) |
| Icon containers | 16px |
| Swatch bars | 2px |

### Texture

A subtle fabric-like dot pattern overlays the entire page via a fixed SVG background image. This is decorative and should be `pointer-events: none` and at a high z-index to sit over content without blocking interaction.

### Component Inventory

| Component | Variants | Occurrences |
|-----------|----------|-------------|
| Logo Wordmark | Full (28px), Small (18px) | Nav, Footer |
| Location Pill | Default, Hover, Loading, Error | Nav |
| Fabric Tag | Dynamic color dot per tag | Hero |
| Outfit Item Row | Default, Hover | Outfit Card (3–6 per card) |
| Hour Card | Default, Now (inverted) | Hourly Strip (8) |
| Day Card | Default, Today (inverted); Front, Back (flip) | Weekly Grid (7) |
| Alert Ribbon | 6 types by icon/color | Alert Zone (0–1) |
| Section Header | Title + Subtitle | 2 sections |
| Skeleton Placeholder | Line, Card, Pill variants | All zones during loading |

---

## 1.5 Analytics Events

| Event Name | Trigger | Properties |
|------------|---------|------------|
| `page_load` | App initialized | `location`, `tier_id`, `headline` |
| `weather_loaded` | API data processed | `tier_id`, `modifier_count`, `alert_count`, `load_time_ms` |
| `geo_permission_granted` | User allows geolocation | `accuracy` |
| `geo_permission_denied` | User denies geolocation | — |
| `geo_error` | Geolocation fails technically | `error_code`, `error_message` |
| `day_card_flipped` | User taps a day card | `day_index`, `day_name`, `tier_id`, `was_flipped` (toggling direction) |
| `hourly_strip_scrolled` | User scrolls the hourly strip | `scroll_direction`, `visible_hours` |
| `alert_viewed` | Alert ribbon enters viewport | `alert_type`, `alert_day` |
| `api_error` | Weather API call fails | `status_code`, `error_message` |
| `session_duration` | Page unload / visibility change | `duration_seconds`, `interactions_count` |

---
---

# Document 2: Architecture Brief

## 2.1 System Context

```
┌─────────────────────────────────────────────────────────┐
│                     CLIENT APP                          │
│                                                         │
│  ┌──────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  UI Layer │◄─│ Mapping Engine│◄─│  Data/API Layer  │  │
│  │ (Render)  │  │ (Transform)  │  │  (Fetch + Cache) │  │
│  └──────────┘  └──────────────┘  └────────┬─────────┘  │
│                                           │             │
└───────────────────────────────────────────┼─────────────┘
                                            │
                    ┌───────────────────────┬┴──────────────────────┐
                    │                       │                       │
            ┌───────▼──────┐    ┌──────────▼──────┐    ┌──────────▼──────┐
            │  Browser /   │    │   Open-Meteo    │    │  Nominatim /   │
            │  Platform    │    │   Forecast API  │    │  Geocoding     │
            │  Geolocation │    │  (no auth)      │    │  Service       │
            └──────────────┘    └─────────────────┘    └────────────────┘
```

**Client App:** All logic runs client-side. No backend server. No user accounts.

**External Dependencies:**

| Dependency | Purpose | Auth | Rate Limit | Failure Impact |
|-----------|---------|------|------------|----------------|
| Browser Geolocation API | User coordinates | Permission prompt | N/A | Cannot load weather; show error state |
| Open-Meteo Forecast API | Weather data | None (no key) | 10,000/day (fair use) | App non-functional; show error state |
| Nominatim (OpenStreetMap) | Reverse geocoding | None | 1 req/sec | Location pill shows "Your Location" fallback; non-critical |

## 2.2 Data Model

### Entities

**WeatherInput** (raw from API, normalized):
```
CurrentWeather {
  apparentTemp: number (°F)
  temp: number (°F)
  humidity: number (%)
  precipitation: number (mm)
  rain: number (mm)
  snowfall: number (cm)
  weatherCode: number (WMO)
  windSpeed: number (mph)
  windGusts: number (mph)
  cloudCover: number (%)
  uvIndex: number (0–11+)
  isDay: boolean
  visibility: number (m)
  isRaining: boolean (derived)
  isSnowing: boolean (derived)
  precipProbability: number (%, from hourly)
}

HourlyWeather {
  time: ISO string
  apparentTemp: number
  precipProbability: number
  precipitation: number
  weatherCode: number
  windSpeed: number
  uvIndex: number
  cloudCover: number
  isRaining: boolean
  isSnowing: boolean
  humidity: number
  visibility: number
}

DailyWeather {
  date: ISO string
  weatherCode: number
  apparentTempMax: number
  apparentTempMin: number
  apparentTempMid: number (derived: avg of max/min)
  precipSum: number
  precipProbabilityMax: number
  windSpeedMax: number
  uvIndexMax: number
  sunrise: ISO string
  sunset: ISO string
  isRainy: boolean (derived)
  isSnowy: boolean (derived)
}
```

**OutfitOutput** (UI-ready, from mapping engine):
```
WornResult {
  tagline: string
  headline: string
  description: string
  tier: TierId
  outfit: OutfitItem[]
  fabricTags: FabricTag[]
  hourly: HourlyMoment[]
  daily: DayForecast[]
  alerts: Alert[]
  tierInfo: TierInfo
}

OutfitItem {
  emoji: string
  name: string
  desc: string
  necessity: NecessityLevel
}

FabricTag {
  label: string
  color: string (hex)
}

HourlyMoment {
  time: ISO string
  emoji: string
  garment: string
  vibe: string
  tierId: TierId
  isRaining: boolean
  isSnowing: boolean
}

DayForecast {
  date: ISO string
  headline: string
  emoji: string
  tierId: TierId
  fabrics: string[]
  swatchColor: string (hex)
  tip: string
  backLabel: string
  backFabricDetail: string
}

Alert {
  icon: string (emoji)
  title: string
  desc: string
}
```

**Enumerations:**

```
TierId: "scorcher" | "hot" | "warm" | "pleasant" | "light-jacket"
        | "sweater-weather" | "coat-up" | "bundle" | "survival"

NecessityLevel: "survival" | "non-negotiable" | "must-have" | "smart pick"
                | "go-to" | "safe bet" | "easy pick" | "nice touch" | "maybe"
```

### Relationships

```
Location (lat, lng) ──fetches──► Open-Meteo API ──returns──► Raw API JSON
Raw API JSON ──normalizes──► CurrentWeather + HourlyWeather[] + DailyWeather[]
Normalized Weather ──maps──► WornResult (via Mapping Engine)
WornResult ──renders──► UI Zones (1:1 mapping per zone)
```

## 2.3 API Surface

### External API Contract (Open-Meteo)

**Request:**
```
GET https://api.open-meteo.com/v1/forecast
  ?latitude={lat}
  &longitude={lng}
  &temperature_unit=fahrenheit
  &wind_speed_unit=mph
  &forecast_days=7
  &timezone=auto
  &current=apparent_temperature,temperature_2m,precipitation,rain,
           snowfall,weather_code,wind_speed_10m,wind_gusts_10m,
           relative_humidity_2m,cloud_cover,uv_index,is_day,visibility
  &hourly=apparent_temperature,precipitation_probability,precipitation,
          rain,snowfall,weather_code,wind_speed_10m,uv_index,
          cloud_cover,relative_humidity_2m,visibility
  &daily=weather_code,apparent_temperature_max,apparent_temperature_min,
         precipitation_sum,precipitation_probability_max,
         wind_speed_10m_max,uv_index_max,sunrise,sunset
```

**Response:** JSON with `current`, `hourly`, and `daily` objects containing arrays keyed by variable name. Hourly arrays contain 168 values (7 days × 24 hours). Daily arrays contain 7 values.

### Internal API Contract (Mapping Engine)

```
processWeatherData(apiData: OpenMeteoResponse) → WornResult
fetchWeather(lat: number, lng: number) → Promise<OpenMeteoResponse>
getGeolocation() → Promise<{lat: number, lng: number}>
reverseGeocode(lat: number, lng: number) → Promise<string>
```

The mapping engine is a pure transformation layer with no side effects (except `fetchWeather` and `getGeolocation` which are I/O). This makes it independently testable with mock API data.

## 2.4 Caching Strategy

| Data | Cache Location | TTL | Invalidation |
|------|---------------|-----|--------------|
| Weather API response | In-memory (client) | 30 minutes | Timer-based refresh or page reload |
| Reverse geocode result | In-memory (client) | Session lifetime | Only refetched on location change |
| Geolocation coordinates | In-memory (client) | Session lifetime | Not re-requested unless user triggers |
| Mapping engine output | In-memory (client) | Derived from weather cache — regenerated when weather refreshes | Tied to weather cache TTL |

**No persistent storage in v1.** No localStorage, no IndexedDB, no cookies. Every page load is a fresh fetch. This simplifies the privacy story and avoids stale data concerns.

**Future consideration:** If an offline mode is added, cache the last successful API response to persistent storage with a "last updated" timestamp, and display stale data with a visible "as of X" indicator.

## 2.5 State Management Strategy

The app has a simple, linear state machine:

```
[INIT] → [REQUESTING_GEO] → [FETCHING_WEATHER] → [PROCESSING] → [READY]
                │                    │                               │
                ▼                    ▼                               ▼
          [GEO_DENIED]        [FETCH_ERROR]                   [REFRESHING]
          [GEO_ERROR]                                               │
                                                                    ▼
                                                              [READY] (updated)
```

**State shape (conceptual):**
```
AppState {
  phase: "init" | "requesting_geo" | "fetching_weather" | "processing" | "ready" | "error"
  location: { lat: number, lng: number } | null
  locationName: string | null
  weatherData: OpenMeteoResponse | null
  uiData: WornResult | null
  error: { type: "geo_denied" | "geo_error" | "api_error", message: string } | null
  lastUpdated: Date | null
}
```

**Implementation options (choose based on platform):**
- Simple state object with reactive binding (suitable for most frameworks)
- Observable/stream-based pattern for platforms that support it
- Redux-style store if the app grows in complexity (unlikely for v1)

The key constraint is that the UI must react to state changes — skeleton → content transitions must be driven by state, not imperative DOM manipulation.

## 2.6 Navigation Model

Single-screen app. No routing in v1.

**Future consideration:** If day-detail views or settings are added, use URL hash routing or platform-native navigation stack depending on implementation platform.

## 2.7 Offline / Latency Behavior

| Scenario | Behavior |
|----------|----------|
| No network on load | Show error state: "Can't reach weather data. Check your connection and try again." |
| Slow network (>3s) | Skeleton stays visible; no timeout until 10s |
| Network timeout (>10s) | Transition to error state |
| Network restored | Manual retry only (refresh page or retry button) in v1 |
| Offline after successful load | Data persists in memory; UI remains functional until page unload |
| API returns error (4xx/5xx) | Show error state with retry option |
| API returns unexpected schema | Treat as error; log malformed response for diagnostics |

## 2.8 Security & Privacy

### Authentication
None. No user accounts, no API keys.

### Authorization
Geolocation access requires browser/platform permission prompt. The app must handle all three states: granted, denied, prompt-not-yet-shown.

### Storage
No data persisted to disk/storage in v1. All data is ephemeral (in-memory, session lifetime).

### PII Considerations
| Data Point | Classification | Handling |
|-----------|---------------|----------|
| GPS coordinates | Sensitive location data | Never logged, never sent to analytics, never persisted. Sent only to Open-Meteo and Nominatim via HTTPS. |
| City/State name | Derived, less sensitive | May appear in analytics events. Derived from reverse geocoding, not stored. |

### Threat Considerations
| Threat | Mitigation |
|--------|------------|
| API response tampering (MITM) | HTTPS only for all external calls |
| Malicious API response | Input validation on all numeric fields; reject NaN/null/undefined before passing to mapping engine |
| Location tracking | No persistent storage; coordinates discarded on page unload |
| Nominatim abuse (user enumeration via coords) | Zoom level 10 limits precision to city level; no street-level data sent |

## 2.9 Non-Functional Requirements

### Performance Budgets

| Metric | Target | Rationale |
|--------|--------|-----------|
| First Contentful Paint | < 1.5s | Skeleton UI visible before data loads |
| Time to Interactive | < 3s | All zones populated and interactive |
| Weather API latency | < 1s (p95) | Open-Meteo typically responds in 100–300ms |
| Mapping engine processing | < 50ms | Pure in-memory computation, no I/O |
| Page weight (initial) | < 200KB (gzipped, excluding fonts) | Minimal dependencies |
| Font loading | < 500ms (with swap) | FOUT acceptable; FOIT not acceptable |

### Reliability

| Metric | Target |
|--------|--------|
| Uptime (client) | N/A (client-only, no server) |
| API availability dependency | Open-Meteo: ~99.5% historical |
| Graceful degradation | All error states have user-friendly messaging; no blank screens |

### Observability

| Signal | What to Capture | Implementation Approach |
|--------|----------------|----------------------|
| Logs | API errors, geo errors, mapping engine exceptions | Client-side error logging to analytics/crash reporting service |
| Metrics | Load time, API latency, geo permission rates, interaction counts | Analytics events (see Design Brief §1.5) |
| Traces | Not needed for v1 | N/A |

## 2.10 Build / Release Considerations

### Environments

| Environment | Purpose | Data Source |
|------------|---------|-------------|
| Development | Local iteration | Live Open-Meteo API (no key required) or mock JSON fixtures |
| Staging | Pre-release verification | Live Open-Meteo API |
| Production | End users | Live Open-Meteo API |

**Mock data fixtures:** The mapping engine should be testable with static JSON fixtures representing each tier (scorcher through survival) plus modifier combinations (rain + cold, snow + wind, etc.). At least 12 fixture files recommended.

### Feature Flags

| Flag | Purpose | Default |
|------|---------|---------|
| `enable_manual_location` | Allow tapping location pill to search/override location | Off |
| `enable_auto_refresh` | Periodically refresh weather data without page reload | Off |
| `enable_celsius` | Show °C-based tier thresholds (for i18n) | Off |

### Testing Strategy

| Layer | Approach |
|-------|----------|
| Mapping engine | Unit tests per tier, per modifier, per combination. Input: mock weather data → Output: expected WornResult. High coverage target (>95%). |
| UI rendering | Snapshot / visual regression tests per state (loading, loaded, error) per zone. |
| Integration | End-to-end test: mock geolocation → mock API response → verify rendered UI content. |
| Accessibility | Automated audit (axe-core or equivalent) on all states. |

---
---

# Document 3: PRD

## 3.1 Problem Statement

People check weather apps primarily to answer one question: "What should I wear?" Yet every weather app forces users to mentally translate raw data — degrees, percentages, wind speeds — into clothing decisions. This translation is repetitive, error-prone (people consistently underdress for wind chill, overdress for dry cold, and forget UV protection), and adds friction to a daily routine.

Worn eliminates that translation entirely by presenting weather information exclusively through outfit recommendations, removing all traditional meteorological data from the user-facing experience.

## 3.2 Goals

1. **Deliver an immediately actionable outfit recommendation** within 3 seconds of opening the app, with zero cognitive translation required.
2. **Surface hourly outfit transitions** so users can plan for temperature changes throughout the day (e.g., "you'll want to re-layer by 5 PM").
3. **Proactively warn about upcoming wardrobe-impacting weather** using clothing-language alerts instead of meteorological jargon.

## 3.3 Non-Goals (v1)

- Manual location search or override
- User accounts, preferences, or saved locations
- Personalized comfort thresholds (e.g., "I run hot")
- Integration with calendar or travel apps
- Notifications or push alerts
- Monetization or ads
- Social sharing features
- Celsius/metric support (future flag exists but not active in v1)
- Offline mode with persistent cache

## 3.4 Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Daily active users (DAU) | Baseline establishment (no target for v1) | Analytics |
| Return rate (7-day) | >30% of first-week users return | Analytics |
| Time to first meaningful interaction | <3s median | Performance monitoring |
| Geo permission grant rate | >70% | Analytics event |
| Day card flip rate | >20% of sessions include at least 1 flip | Analytics event |
| Error rate | <5% of sessions encounter an error state | Analytics event |

## 3.5 Functional Requirements

### FR-1: Geolocation Detection

The app must request the user's location via the platform's geolocation API on first load.

**Acceptance Criteria:**

**Given** the user opens the app for the first time
**When** the geolocation permission prompt appears
**Then** the app displays a skeleton loading state while awaiting the response

**Given** the user grants geolocation permission
**When** coordinates are received
**Then** the app fetches weather data for those coordinates AND reverse-geocodes the coordinates to display "City, State" in the location pill

**Given** the user denies geolocation permission
**When** the denial is received
**Then** the location pill displays "Location unavailable" AND the hero section displays a friendly error message: "We need your location to dress you for the weather. Enable location access and refresh to try again." AND no weather data is loaded AND no API calls are made

**Given** the geolocation request times out (>10s)
**When** the timeout occurs
**Then** the app transitions to the same error state as denial

**Maps to:** Zone 1 (Nav), all zones (loading/error states)

---

### FR-2: Weather Data Fetching

The app must fetch current, hourly (168h), and daily (7-day) weather data from a free, keyless weather API.

**Acceptance Criteria:**

**Given** valid coordinates are available
**When** the weather API is called
**Then** the response includes: apparent temperature, raw temperature, humidity, precipitation, rain, snowfall, weather code, wind speed, wind gusts, cloud cover, UV index, day/night flag, and visibility for current conditions AND apparent temperature, precipitation probability, precipitation, weather code, wind speed, UV index, cloud cover, humidity, and visibility for hourly data AND weather code, apparent temp max/min, precipitation sum, precipitation probability max, wind speed max, UV index max, sunrise, and sunset for daily data

**Given** the weather API returns an error
**When** the error is received
**Then** all content zones display their respective error states AND the error is logged for diagnostics

**Given** the weather API returns data with missing or null fields
**When** the data is processed
**Then** missing numeric fields default to safe values (0 for precipitation, 50 for humidity, 10000 for visibility) AND the mapping engine still produces a valid result

**Maps to:** All zones (data dependency)

---

### FR-3: Outfit Tier Mapping

The mapping engine must classify the current apparent temperature into one of 9 outfit tiers and produce a corresponding outfit recommendation.

**Tiers (°F apparent temperature):**

| Tier ID | Range | Headline Example |
|---------|-------|-----------------|
| scorcher | 95°F+ | "Barely dressed weather" |
| hot | 85–94°F | "Less is more" |
| warm | 75–84°F | "T-shirt confident" |
| pleasant | 65–74°F | "The sweet spot" |
| light-jacket | 55–64°F | "Light jacket kind of day" |
| sweater-weather | 45–54°F | "Sweater weather" |
| coat-up | 32–44°F | "Coat up" |
| bundle | 15–31°F | "Full bundle mode" |
| survival | Below 15°F | "Question your choices" |

**Acceptance Criteria:**

**Given** an apparent temperature of 58°F with no active modifiers
**When** the mapping engine processes the data
**Then** the tier is "light-jacket" AND the headline is one of the 5 defined variants for that tier AND the outfit contains at least 3 items AND each item has an emoji, name, description, and necessity level

**Given** an apparent temperature on a tier boundary (e.g., exactly 55°F)
**When** the mapping engine processes the data
**Then** the temperature maps to the tier whose range includes that value (55°F → "light-jacket", not "sweater-weather")

**Maps to:** Zone 2 (Hero headline, description, outfit card)

---

### FR-4: Modifier Layers

Weather modifiers (rain, snow, wind, humidity, UV, cloud cover, visibility) must layer additional items and adjustments on top of the base tier outfit.

**Modifier Thresholds:**

| Modifier | Condition | Effect |
|----------|-----------|--------|
| Rain (light) | precip_probability 40–60% | Add compact umbrella |
| Rain (medium) | precip_probability 60–80% OR rain 1–5mm | Add umbrella + waterproof jacket |
| Rain (heavy) | precip_probability >80% OR rain >5mm | Add sturdy umbrella + waterproof jacket + rain boots |
| Snow | snowfall > 0 | Add waterproof boots + thermal socks |
| Wind | wind >20mph OR gusts >30mph | Add windbreaker + scarf |
| Humidity | humidity > 75% | Swap fabric tags: cotton → moisture-wicking |
| UV High | uv_index 6–7 | Add sunglasses + hat |
| UV Very High | uv_index 8–9 | Add sunglasses + wide brim hat + sunscreen |
| UV Extreme | uv_index ≥10 | Add sunscreen + wide brim hat |
| Clear sky | cloud_cover < 30% AND uv < 6 | Add sunglasses |

**Acceptance Criteria:**

**Given** an apparent temperature of 60°F with precipitation_probability of 85%
**When** the mapping engine processes the data
**Then** the base tier is "light-jacket" AND the outfit includes rain modifier items (sturdy umbrella, waterproof jacket, rain boots) in addition to the base items AND the headline is overridden to a rain headline (e.g., "Waterproof or regret") AND duplicate items are removed (e.g., if base tier already has boots, rain boots replace rather than duplicate)

**Given** multiple modifiers are active simultaneously (e.g., rain + wind + low UV)
**When** the mapping engine processes the data
**Then** all applicable modifier items are added AND items are deduplicated by name

**Maps to:** Zone 2 (Outfit card), Zone 2 (Fabric tags)

---

### FR-5: Fabric Tags

The app must display condition-derived fabric/clothing property tags based on boolean evaluations of current weather data.

**Tag Definitions:**

| Tag | Condition |
|-----|-----------|
| cotton-friendly | No rain AND humidity < 70% AND temp 55–85°F |
| moisture-wicking | Humidity > 75% OR temp > 85°F |
| waterproof | Precip probability > 50% OR active precipitation/snow |
| hair-safe | Wind < 12mph AND no rain |
| closed-toe | Temp < 65°F OR rain OR snow |
| sunscreen-worthy | UV ≥ 6 |
| windproof | Wind > 20mph |
| breathable | Temp > 75°F |
| open-toe OK | Temp ≥ 70°F AND no rain AND no snow |

**Acceptance Criteria:**

**Given** current conditions of 62°F, 55% humidity, no rain, 8mph wind, UV 3
**When** fabric tags are evaluated
**Then** the visible tags are: "cotton-friendly", "hair-safe", "closed-toe"

**Given** conditions change to include rain
**When** fabric tags are re-evaluated
**Then** "hair-safe" is removed, "cotton-friendly" is removed, and "waterproof" is added

**Maps to:** Zone 2 (Fabric tags row)

---

### FR-6: Hourly Outfit Timeline

The app must display the next 8 hours as individual garment moments with transition awareness.

**Acceptance Criteria:**

**Given** hourly data is available
**When** the hourly strip is built
**Then** 8 cards are displayed starting from the current hour AND the card matching the current hour is visually highlighted as "Now" AND each card shows time, emoji, garment name, and vibe descriptor

**Given** the outfit tier changes between hour N and hour N+1
**When** the hour N+1 card is generated
**Then** it uses a transition label (e.g., "Re-Layer", "Shed a Layer", "Umbrella Up") instead of the default tier garment

**Given** rain starts between hour N and hour N+1
**When** the hour N+1 card is generated
**Then** it shows "Umbrella Up" with the rain-start emoji

**Maps to:** Zone 3 (Hourly strip)

---

### FR-7: 7-Day Outfit Forecast with Flip Cards

The app must display a 7-day forecast as interactive flip cards showing outfit headlines (front) and styling tips (back).

**Acceptance Criteria:**

**Given** daily forecast data is available
**When** the weekly grid is rendered
**Then** 7 cards are displayed, one per day AND the current day's card is visually distinguished AND each card front shows: day abbreviation, emoji, outfit headline, fabric types, and color swatch

**Given** a user taps a day card
**When** the tap is registered
**Then** the card performs a 3D Y-axis flip animation (600ms) AND the back face is revealed showing: label, styling tip, fabric detail, and "tap to flip back" hint

**Given** a user taps a flipped card
**When** the tap is registered
**Then** the card flips back to the front face with the same animation

**Given** multiple cards
**When** the user flips several cards
**Then** each card maintains its own flip state independently (multiple can be flipped simultaneously)

**Maps to:** Zone 5 (Weekly forecast)

---

### FR-8: Clothing-Language Alerts

The app must generate proactive alerts for upcoming weather events that require wardrobe preparation, using clothing terminology instead of meteorological terms.

**Acceptance Criteria:**

**Given** Wednesday's forecast shows precipitation_probability_max > 70% and precipitation_sum > 5mm
**When** alerts are generated
**Then** an alert is created with title "Umbrella Advisory for Wednesday" and clothing-specific description

**Given** no upcoming days trigger alert conditions
**When** alerts are generated
**Then** the alert zone is not rendered (no empty state, no placeholder)

**Given** multiple alert conditions are met across different days
**When** alerts are generated
**Then** only the highest-priority alert is displayed (priority: snow > extreme cold > rain > extreme heat > wind > UV)

**Maps to:** Zone 4 (Alert ribbon)

---

### FR-9: Headline Variety

Each outfit tier and modifier override must have 4–6 headline variants. The selected variant is random per page load.

**Acceptance Criteria:**

**Given** the tier is "light-jacket"
**When** the headline is generated
**Then** it is one of: "Light jacket kind of day", "Grab a layer by the door", "That jacket on the hook? Today's the day", "Sleeves mandatory, coat optional", "One good layer does the trick"

**Given** a user refreshes the page with the same weather conditions
**When** the headline is regenerated
**Then** it MAY be a different variant (random selection, not guaranteed different)

**Maps to:** Zone 2 (Hero headline), Zone 5 (Day card front)

---

### FR-10: Reverse Geocoding

The app must convert GPS coordinates into a human-readable location name for display.

**Acceptance Criteria:**

**Given** coordinates resolve to a known city
**When** reverse geocoding completes
**Then** the location pill shows "City, State" (US) or "City, Country" (international)

**Given** the reverse geocoding service is unavailable
**When** the request fails or times out
**Then** the location pill shows "Your Location" AND the weather data still loads normally (geocoding failure is non-blocking)

**Maps to:** Zone 1 (Location pill)

---

## 3.6 Edge Cases

| Edge Case | Expected Behavior |
|-----------|------------------|
| User at North/South Pole | Open-Meteo supports global coordinates; app functions normally. Tier likely "survival". |
| User on a ship (oceanic coordinates) | Weather data available; reverse geocoding may return country or "Your Location" fallback. |
| Apparent temp exactly on tier boundary (e.g., 55.0°F) | Boundary belongs to the tier whose `min` matches (55°F → light-jacket). |
| All modifiers active simultaneously (rain + snow + wind + high UV) | All modifier items added, deduplicated by name. Snow overrides rain for headline. |
| Zero hourly data remaining in current day | Show fewer than 8 cards; do not pad with next-day data. |
| User loads at 11:59 PM | "Now" card is 11 PM; hourly strip may show only 1 card. |
| API returns 0 for all values | Defaults produce a "pleasant" tier (65°F midpoint assumption is NOT used; 0°F → "survival" tier). |
| Leap second / DST transition | Use timezone-aware ISO parsing; Open-Meteo handles via `timezone=auto`. |
| User denies geo, then re-enables in browser settings | App does not auto-retry; user must refresh the page. |
| Very slow network (15+ seconds) | 10s timeout triggers error state. User can retry manually. |

## 3.7 User Stories

### P0 — Must Have (Launch Blockers)

| ID | Story | Acceptance Criteria Reference |
|----|-------|------|
| US-1 | As a user, I want to see what to wear right now based on my location so I can walk out the door prepared. | FR-1, FR-2, FR-3, FR-4 |
| US-2 | As a user, I want to see how my outfit needs will change throughout the day so I can plan layers or bring extras. | FR-6 |
| US-3 | As a user, I want to see the week's outfit forecast so I can plan ahead. | FR-7 |
| US-4 | As a user, I want to be warned about upcoming weather that affects my wardrobe so I'm not caught off guard. | FR-8 |
| US-5 | As a user, I want the app to work without creating an account or entering any information so I can get value immediately. | FR-1, FR-2 (no auth) |

### P1 — Should Have

| ID | Story | Acceptance Criteria Reference |
|----|-------|------|
| US-6 | As a user, I want to see fabric/material recommendations so I know what types of clothing work best. | FR-5 |
| US-7 | As a user, I want to tap day cards for styling tips so I get more detailed advice. | FR-7 (flip interaction) |
| US-8 | As a user, I want headline variety so the app feels fresh each day. | FR-9 |

### P2 — Nice to Have (Post-Launch)

| ID | Story | Acceptance Criteria Reference |
|----|-------|------|
| US-9 | As a user, I want to manually set my location so I can check weather for other places. | Non-goal for v1 (flag: `enable_manual_location`) |
| US-10 | As a user, I want the app to auto-refresh while I have it open so I always see current data. | Non-goal for v1 (flag: `enable_auto_refresh`) |
| US-11 | As a user who uses metric, I want the app to adapt tier thresholds to Celsius. | Non-goal for v1 (flag: `enable_celsius`) |

## 3.8 Release Milestones

| Milestone | Scope | Exit Criteria |
|-----------|-------|---------------|
| **M1: Engine** | Mapping engine complete with all tiers, modifiers, headline variants, hourly transitions. | Unit tests pass for all 9 tiers × 6 modifier combinations. All outputs match expected data shapes. |
| **M2: UI Shell** | All 6 zones rendered with skeleton states. Responsive across 3 breakpoints. Flip card interaction working. | Visual review approved. Accessibility audit passes (0 critical, 0 serious violations). |
| **M3: Integration** | Engine wired to UI. Geolocation → API → Engine → Render pipeline complete. Error states implemented. | End-to-end test passes on 3 environments (dev, staging, production). All loading/error/loaded states verified. |
| **M4: Polish** | Animations tuned. Analytics events firing. Performance budgets met. Cross-browser/platform testing. | FCP < 1.5s, TTI < 3s. Analytics events verified in dashboard. Tested on target platforms. |

## 3.9 Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Open-Meteo API outage | Low | High (app non-functional) | Error state with retry. Monitor API status. Evaluate fallback API if reliability becomes an issue. |
| Nominatim rate limiting | Medium | Low (cosmetic — "Your Location" fallback works) | Cache result per session. Respect 1 req/s limit. |
| Geolocation permission denial rates higher than expected | Medium | High (app non-functional without location) | Clear, friendly error messaging. Future: add manual location search. |
| Outfit tier boundaries feel wrong to users ("58°F doesn't feel like jacket weather to me") | Medium | Medium (perceived inaccuracy) | Using apparent temperature (not raw) helps. Future: user comfort preference slider. |
| Headline/copy tone doesn't resonate | Low | Medium (brand perception) | A/B test headline sets. Track day_card_flipped as engagement signal. |
| API schema changes without notice | Low | High (app breaks) | Validate response shape before processing. Default safe values for missing fields. |

## 3.10 Open Questions

| # | Question | Impact | Suggested Resolution |
|---|----------|--------|---------------------|
| OQ-1 | Should the app support Celsius in v1 or defer entirely? The tier thresholds are defined in °F. | Architecture — requires parallel threshold tables or conversion layer | Defer to v2 behind `enable_celsius` flag. Tier logic should accept a unit parameter for future-proofing. |
| OQ-2 | What is the refresh behavior when the app is left open for hours? Auto-refresh or manual only? | UX — stale data risk vs. complexity | v1: manual only (page refresh). Flag `enable_auto_refresh` for v2. |
| OQ-3 | Should the hourly strip show next-day hours when fewer than 8 remain in the current day? | UX — late-night users see very few cards | v1: show only current-day hours. Revisit if analytics show high late-night usage. |
| OQ-4 | How should the app handle locations where apparent temperature data is unavailable from the API? | Data — some remote locations may have gaps | Fall back to raw `temperature_2m` if `apparent_temperature` is null/undefined. Document this fallback. |
| OQ-5 | Should multiple alerts be displayed (stacked) or strictly one? | UX — multiple weather events in one week | v1: single highest-priority alert. Revisit if users report missing important alerts. |
| OQ-6 | Is Nominatim the best reverse geocoding option, given its 1 req/s rate limit and usage policy? | Architecture — scalability if user base grows | Acceptable for v1. Evaluate Open-Meteo's own geocoding API or a dedicated service if rate limits become an issue. |
| OQ-7 | Should the "today" card in the weekly grid always be position 0, or should the grid always show Mon–Sun? | UX — consistency vs. relevance | Design shows today as first card. Implement as: today is always position 0, subsequent days follow chronologically, wrapping through the week. |
| OQ-8 | What is the desired behavior if the user's timezone differs from their physical location (e.g., VPN)? | Data — `timezone=auto` uses coordinate-based timezone, not device timezone | Coordinate-based timezone (from Open-Meteo's `timezone=auto`) is correct for weather purposes. No change needed. |
| OQ-9 | Should the mapping engine be deterministic for the same input (for testing) or always random (for variety)? | Testing — headline selection uses Math.random() | Keep random for production. Provide a seeded random option for unit tests via dependency injection of the random function. |
| OQ-10 | What platforms are targeted for v1? Web-only, or native mobile as well? | Architecture — affects build tooling, font loading, geolocation API surface | Left intentionally agnostic. Decision criteria: if web-only, deploy as PWA. If native, evaluate cross-platform frameworks. If both, consider shared engine with platform-specific UI. |

---

*End of Spec Kit*
