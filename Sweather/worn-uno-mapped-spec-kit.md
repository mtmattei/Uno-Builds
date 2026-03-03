# Worn - Uno Platform Alignment Map

**Source:** worn-spec-kit.md
**Date:** February 2026
**Status:** Draft
**Runtime:** .NET 10 (LTS)
**Uno.Sdk:** 6.5

---

## 1. Architecture Pattern

| Spec Requirement | Uno Platform Recommendation |
|---|---|
| Client-only app, no backend, linear state machine (`INIT` > `REQUESTING_GEO` > `FETCHING_WEATHER` > `PROCESSING` > `READY` / `ERROR`) | **MVUX (Model-View-Update eXtended)** with `IState<T>` and `IFeed<T>`. The spec's `AppState` maps directly to an MVUX `partial record` Model. Each phase (geo, fetch, process) becomes an async pipeline feeding into `IFeed<WornResult>`. MVUX gives loading/error/data states for free via `FeedView`. |
| Pure transformation layer (mapping engine), no side effects | Register `IWeatherMappingEngine` as a singleton service. MVUX Model receives it via constructor injection. The engine stays a pure function: `OpenMeteoResponse` in, `WornResult` out - independently unit-testable. |
| Reactive state - UI reacts to state changes, not imperative manipulation | MVUX `IState<AppPhase>` + `IFeed<WornResult>`. `FeedView` in XAML auto-binds to feed state and swaps between `ProgressTemplate`, `ValueTemplate`, `NoneTemplate`, and `ErrorTemplate` - matching the spec's skeleton > content > error transitions exactly. |
| Dependency Injection + Service registration | **Uno.Extensions Hosting** via `IHostBuilder`. Register: `IWeatherService`, `IGeoLocationService`, `IReverseGeocodingService`, `IWeatherMappingEngine`. Use `.UseConfiguration()` for feature flags. |

**UnoFeatures needed:** `MVUX`, `Hosting`, `Extensions`, `Http`, `Toolkit`, `Material`, `Skia`
**Runtime:** .NET 10 (LTS) with Uno.Sdk 6.5

---

## 2. UI Components - Zone-by-Zone Mapping

### Zone 1: Navigation Bar

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| Logo wordmark ("worn", serif italic) | `TextBlock` with custom `FontFamily` (Playfair Display). Style defined in `App.xaml`. | Use `<Run>` elements if the "o" needs distinct styling. |
| Location Pill (dark pill, pin icon + city) | **`Chip`** (`Uno.Toolkit.UI`) with `Style="{StaticResource AssistChipStyle}"` customized. Or a `Button` styled with `CornerRadius="20"` + `FontIcon` for pin. | Chip gives pill shape, icon support, and hover states out of the box. |
| Loading state ("Locating...") | Bind `Chip.Content` to `IState<string>` for location name. Use `FeedView` or `Visibility` binding on a shimmer placeholder. | MVUX state naturally transitions from null > "Locating..." > "City, State". |
| Error state ("Location unavailable") | Bind to error state in MVUX model. Muted styling via a secondary `TextBlock` style. | |

### Zone 2: Hero Section

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| Two-column / single-column responsive layout | **`Grid`** with `ColumnDefinitions` swapped via `ResponsiveExtension` or `VisualStateManager` + `AdaptiveTrigger` at 1024px. | `{utu:Responsive Narrowest="0" Normal="*"}` on second column width; stack on narrow. |
| Tagline (handwritten-style) | `TextBlock` with `FontFamily="Caveat"` custom style. | Define `CaveatTaglineStyle` in `App.xaml`. |
| Headline ("It's a [X] kind of day") | `TextBlock` with `<Run>` elements. The `[X]` slot uses a `<Run Foreground="{StaticResource Terracotta}">` bound to `{Binding Headline}`. | Multiple `<Run>` elements for concatenation per binding rules (no StringFormat). |
| Description paragraph | `TextBlock` with `TextWrapping="WrapWholeWords"`, body style, bound to `{Binding Description}`. | |
| Fabric Tags (pills with colored dots) | **`ItemsRepeater`** with `ChipGroup` or individual `Chip` controls. Each chip gets a colored `Ellipse` (8px) as its icon + label text. | Bind `ItemsSource` to `IListFeed<FabricTag>`. Use custom `DataTemplate` with `Chip` + colored dot. |
| Outfit Memo Card | **`CardContentControl`** (`Uno.Toolkit.UI`) with `Style="{StaticResource ElevatedCardContentControlStyle}"` and `CornerRadius="24"`. | Provides elevation, theming, and interaction states. |
| "TODAY'S OUTFIT MEMO" label | `TextBlock` with `CharacterSpacing="150"`, uppercase, small caps style. | |
| Outfit Item Row (emoji + name + desc + necessity) | **`ItemsRepeater`** inside the card. Each item template: `AutoLayout Orientation="Horizontal"` with emoji in a colored `Border` (CornerRadius 16), name `TextBlock` (serif), desc `TextBlock` (muted), necessity `TextBlock` (Caveat font, terracotta). | |
| Item row dividers | **`Divider`** (`Uno.Toolkit.UI`) between rows. | Drop-in, Material-styled. |
| Item hover (6px translateX) | `PointerEntered`/`PointerExited` with `Storyboard` > `DoubleAnimation` targeting `(UIElement.RenderTransform).(TranslateTransform.X)`. | Or use `VisualStateManager` with `PointerOver` state on the item template root. |

### Zone 3: Hourly Wardrobe Strip

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| Horizontal scroll container (8 cards, hidden scrollbar) | **`ScrollViewer`** with `HorizontalScrollBarVisibility="Hidden"` + `HorizontalScrollMode="Enabled"` + `VerticalScrollMode="Disabled"` wrapping an `ItemsRepeater` with `StackLayout Orientation="Horizontal"`. | |
| Hour Card (155px fixed width) | Custom `DataTemplate` with `CardContentControl` or styled `Border`. CornerRadius 20-24px, fixed `Width="155"`. | |
| "Now" card (inverted colors) | Use a **`DataTemplateSelector`** that returns a distinct template when `isNow == true` (dark background, light text, gold accent). | Or use a `Binding` converter to swap `Background`/`Foreground` brushes. |
| "Scroll >" hint | `TextBlock` with `Opacity` animation, `AutomationProperties.AccessibilityView="Raw"` (decorative). | |
| Section header ("Your wardrobe, hour by hour") | `TextBlock` with `SectionTitleStyle` (Playfair Display 28px). | |

### Zone 4: Alert Ribbon

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| Alert container (gradient background) | `Border` with `Background` set to a `LinearGradientBrush` (terracotta tones), `CornerRadius="20"`. Content: `AutoLayout Orientation="Horizontal"`. | |
| Conditional visibility (hidden when no alerts) | Bind `Visibility` directly to a `bool` property `HasAlerts` in the MVUX model. MVUX implicit bool-to-Visibility conversion handles this. | Zone is simply absent from the visual tree when false. |
| Alert icon (large emoji) | `TextBlock` with large `FontSize` for the emoji glyph. | Emoji renders cross-platform via Skia renderer. |
| Entrance animation (fade-up) | `Storyboard` with `DoubleAnimation` on `Opacity` (0>1) + `TranslateTransform.Y` (20>0), triggered on `Loaded`. | |

### Zone 5: Weekly Outfit Forecast (Flip Cards)

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| 7-day card grid (responsive columns) | `ItemsRepeater` with `UniformGridLayout` or a `Grid` with columns set via `ResponsiveExtension`: `{utu:Responsive Narrowest=2 Narrow=4 Normal=7}` column count. | Spec: 7 cols >=1024, 4 cols 640-1023, 2 cols <640. |
| Flip card (3D Y-axis rotation) | Custom `UserControl` with two overlaid `Border` elements (Front/Back). Toggle `Visibility` or use `PlaneProjection.RotationY` animated from 0 to 180/180 to 360 via `Storyboard`. | WinUI `PlaneProjection` supports 3D rotation. `Perspective` maps to the projection. |
| Tap to flip interaction | `Tapped` event or bind `Command` via `CommandExtensions.Command` to an MVUX method that toggles `IState<bool> IsFlipped` per card. | Multiple cards flipped simultaneously = each card has independent state. |
| "Today" card (inverted styling) | `DataTemplateSelector` returning a distinct template, or conditional `Style` binding. | Dark background, light text, terracotta back. |
| Color swatch bar | Small `Border` with `Height="4"`, `CornerRadius="2"`, `Background` bound to `SwatchColor`. | |
| Flip hint text | `TextBlock` with delayed fade-in `Storyboard` (BeginTime 1.6s). | |

### Zone 6: Footer

| Spec Component | Uno Platform Control | Notes |
|---|---|---|
| Small logo + tagline | `AutoLayout Orientation="Vertical"` with centered `TextBlock` elements. | Static, no interactions. Minimal. |

---

## 3. State Management

| Spec Concept | MVUX Implementation |
|---|---|
| `AppState.phase` (init, requesting_geo, fetching_weather, processing, ready, error) | `IState<AppPhase>` where `AppPhase` is an enum. MVUX model updates this as the pipeline progresses. |
| `AppState.location` | `IState<GeoCoordinates?>` - set after geolocation succeeds. |
| `AppState.locationName` | `IState<string?>` - set after reverse geocoding. |
| `AppState.weatherData` | `IFeed<OpenMeteoResponse>` - an async feed that fetches from Open-Meteo. Gives loading/error/data states automatically. |
| `AppState.uiData` (WornResult) | `IFeed<WornResult>` - derived by chaining the weather feed through the mapping engine. This is the primary feed that `FeedView` binds to. |
| `AppState.error` | Handled by MVUX's built-in error propagation. `FeedView.ErrorTemplate` renders the spec's error states per zone. |
| Skeleton > Content transitions | `FeedView` with `ProgressTemplate` (skeleton UI) and `ValueTemplate` (loaded content). The crossfade is automatic. |
| 30-minute cache TTL | In-memory cache in `IWeatherService`. Return cached `OpenMeteoResponse` if `lastFetched < 30 min ago`. |
| Card flip states (independent per card) | Each `DayForecast` view model gets an `IState<bool>` for `IsFlipped`. Or handle locally in the `UserControl` code-behind since it's purely visual. |

---

## 4. Navigation

| Spec Requirement | Uno Platform Approach |
|---|---|
| Single-page, vertical scroll, no routing | **No Uno Navigation Extensions needed.** Single `Page` with a `ScrollViewer` wrapping all 6 zones in a vertical `StackPanel` or `AutoLayout`. |
| No multi-page navigation in v1 | The entire app is one `MainPage.xaml`. No `Shell`, no `NavigationView`, no frame navigation. |
| Future: day-detail views, settings | When needed, add Uno.Extensions Navigation with region-based routing. The single-page architecture makes this a clean future addition. |

---

## 5. Theming & Visual System

| Spec Token | Material Palette Mapping | Implementation |
|---|---|---|
| `--cream` (#F5F0E8) | `Surface` / `Background` | Override in `ColorPaletteOverride.xaml` |
| `--warm-black` (#1A1714) | `OnSurface` / `OnBackground` | Override in `ColorPaletteOverride.xaml` |
| `--terracotta` (#C4654A) | `Primary` | Override in `ColorPaletteOverride.xaml` |
| `--sage` (#8B9E7E) | `Secondary` | Override in `ColorPaletteOverride.xaml` |
| `--slate` (#6B7B8D) | `OnSurfaceVariant` | Override in `ColorPaletteOverride.xaml` |
| `--gold` (#C9A96E) | `Tertiary` | Override in `ColorPaletteOverride.xaml` |
| `--linen` (#EDE6D6) | `SurfaceVariant` / Card backgrounds | Override in `ColorPaletteOverride.xaml` |
| `--charcoal` (#3A3632) | `OnSurfaceVariant` (secondary) | Override in `ColorPaletteOverride.xaml` |
| `--blush` (#E8CFC0) | Custom resource `BlushBrush` | Define in `App.xaml` |
| `--denim` (#4A5E78) | Custom resource `DenimBrush` | Define in `App.xaml` |

**Typography:** Define custom styles in a `Themes/TextBlock.xaml` resource dictionary:
- `DisplayStyle` - Playfair Display 700, clamp(52-80px)
- `SectionTitleStyle` - Playfair Display 400, 28px
- `CardTitleStyle` - Playfair Display 400, 15-18px
- `TaglineStyle` - Caveat 400-600, 15-18px
- `BodyStyle` - DM Sans 300-500, 13-17px
- `LabelStyle` - DM Sans 500, 11-12px, uppercase, wide tracking

**Font loading:** Bundle Playfair Display, Caveat, and DM Sans as app assets. Reference via `FontFamily="ms-appx:///Assets/Fonts/PlayfairDisplay-Bold.ttf#Playfair Display"`.

**Elevation/Shadows:** Use `ThemeShadow` with `Translation="0,0,32"` on cards. `ShadowContainer` from Uno Toolkit for finer control over shadow color/offset if needed.

---

## 6. Responsive Design

| Spec Breakpoint | Uno Platform Mechanism |
|---|---|
| >= 1024px (desktop) | `{utu:Responsive Normal=...}` or `AdaptiveTrigger MinWindowWidth="1024"` |
| 640-1023px (tablet) | `{utu:Responsive Narrow=...}` or `AdaptiveTrigger MinWindowWidth="640"` |
| < 640px (phone) | `{utu:Responsive Narrowest=...}` (default/smallest state) |
| Hero: 2-col > 1-col | `VisualStateManager` with `AdaptiveTrigger` swapping `Grid.ColumnDefinitions` |
| Weekly grid: 7 > 4 > 2 cols | `ResponsiveExtension` on `UniformGridLayout.MaximumRowsOrColumns` or swap `ColumnDefinitions` |
| Padding reduction at < 640px | `{utu:Responsive Narrowest=20 Narrow=20 Normal=48}` on `Padding` |
| Hourly strip: always horizontal scroll | `ScrollViewer` with horizontal mode - works at all breakpoints. Fixed 155px card width. |

---

## 7. Platform Features

| Spec Feature | Uno Platform API | Notes |
|---|---|---|
| Browser/device geolocation | `Windows.Devices.Geolocation.Geolocator` | Supported on WASM, Android, iOS, macOS. Permission prompt handled automatically. WASM has 10s default timeout matching spec. |
| Geolocation permission states (granted/denied/error) | `Geolocator.RequestAccessAsync()` returns `GeolocationAccessStatus` (Allowed, Denied, Unspecified) | Map to spec's three states directly. |
| Open-Meteo API (HTTP, no auth) | `HttpClient` via Uno.Extensions HTTP or manual `HttpClient` registration. No Kiota/Refit needed (no OpenAPI spec for Open-Meteo). | Register `HttpClient` with base address in DI. Use typed service `IWeatherService`. |
| Reverse geocoding (Nominatim) | `HttpClient` call to Nominatim API. | Separate `IReverseGeocodingService`. Non-blocking - failure shows "Your Location" fallback. |
| No persistent storage | No `localStorage`, no `ApplicationData`. All in-memory via MVUX state. | Matches spec exactly. |
| Cross-platform targeting | **Uno Platform Single Project** with `Uno.Sdk`. Target `net10.0-desktop`, `net10.0-browserwasm`, and `net10.0-android`. | Answers spec's OQ-10: three platforms from one project. iOS/macOS deferred. |

---

## 8. Animations

| Spec Animation | Uno Platform Implementation |
|---|---|
| Staggered fade-up on load (200ms offset per zone) | `Storyboard` per zone with `DoubleAnimation` on `Opacity` + `TranslateTransform.Y`. Use `BeginTime` offsets: 0ms, 200ms, 400ms, 600ms, 800ms. |
| Skeleton > content crossfade (300ms) | `FeedView` transitions, or manual `Storyboard` triggered when `IFeed` transitions from loading to data. |
| Day card 3D flip (600ms, cubic-bezier) | `DoubleAnimation` on `PlaneProjection.RotationY` (0>180 / 180>360). `EasingFunction` with `CubicBezier(0.4, 0, 0.2, 1)`. |
| Outfit item hover (translateX 300ms) | `PointerOver` `VisualState` with `Storyboard` targeting `TranslateTransform.X`. |
| Alert fade-up | Same pattern as zone entrance: `Opacity` + `TranslateY` on `Loaded`. |
| Flip hint delayed fade (1.6s) | `DoubleAnimation` on `Opacity` with `BeginTime="0:0:1.6"`. |

---

## 9. Accessibility

| Spec Requirement | Uno Platform Implementation |
|---|---|
| Location pill `aria-label` | `AutomationProperties.Name="Current location: {city}"` (data-bound) |
| Outfit card as `<article>` | `AutomationProperties.Name="Today's outfit recommendation"` on `CardContentControl` |
| Necessity labels with `aria-label` | `AutomationProperties.Name="Necessity level: must-have"` on necessity `TextBlock` |
| Hourly strip `role="region"` | `AutomationProperties.Name="Hourly outfit forecast"` on `ScrollViewer` |
| "Now" card `aria-current` | `AutomationProperties.Name` including "Current hour" context |
| Day cards as `role="button"` with `aria-expanded` | `AutomationProperties.Name` + Toggle states via `AutomationProperties.ItemStatus` |
| Keyboard: Enter/Space to flip | Handle `KeyDown` for `VirtualKey.Enter` and `VirtualKey.Space` on card `UserControl` |
| `x:Uid` for localization | Apply `x:Uid` pattern: e.g., `MainPage.HeroSection.Headline`, `MainPage.HourlyStrip.SectionTitle` |

---

## 10. Gaps & Considerations

| Area | Gap / Risk | Mitigation |
|---|---|---|
| **Fabric dot pattern overlay** | Spec calls for a fixed SVG texture over entire page. Uno Platform doesn't have a direct CSS `background-image: fixed` equivalent. | Use an `Image` or `Canvas` with `IsHitTestVisible="False"` at the top of the z-order, set to `Stretch="Fill"` with low opacity. |
| **Google Fonts loading** | Spec uses `display=swap`. In native apps, fonts are bundled, not web-loaded. | Bundle all 3 font families as `.ttf` assets. Eliminates FOUT entirely. |
| **Responsive font clamping** | Spec uses `clamp(52px, 6vw, 80px)`. No CSS `clamp()` in XAML. | Use `ResponsiveExtension`: `FontSize="{utu:Responsive Narrowest=52 Narrow=60 Normal=72 Wide=80}"`. |
| **3D flip card** | `PlaneProjection` works but is less polished than CSS 3D transforms on some platforms. | Test on all targets. Consider a simpler crossfade fallback if projection performance is poor on older Android devices. |
| **Horizontal scroll snap** | CSS `scroll-snap` has no XAML equivalent. | Cards at 155px fixed width still scroll smoothly. Snap-to-item can be approximated with `ScrollViewer.HorizontalSnapPointsType`. |
| **Analytics events** | Spec defines 10 custom events. No built-in analytics in Uno Platform. | Integrate App Center, Firebase Analytics, or a lightweight custom `IAnalyticsService` registered in DI. |

---

## 11. Recommended Project Structure

```
Worn/
├── Worn.csproj                      (Single Project, Uno.Sdk)
├── App.xaml / App.xaml.cs           (Host builder, DI, theming)
├── MainPage.xaml / .cs              (Single page, all 6 zones)
├── Models/
│   ├── WornModel.cs                 (MVUX partial record - main model)
│   ├── AppPhase.cs                  (Enum: Init, RequestingGeo, etc.)
│   ├── WornResult.cs                (Output data shapes)
│   └── WeatherData.cs               (Input data shapes)
├── Services/
│   ├── IWeatherService.cs           (Open-Meteo fetch)
│   ├── IGeoLocationService.cs       (Geolocator wrapper)
│   ├── IReverseGeocodingService.cs  (Nominatim)
│   └── IWeatherMappingEngine.cs     (Pure transform)
├── Controls/
│   ├── FlipCard.xaml / .cs          (Reusable flip card UserControl)
│   ├── HourCard.xaml / .cs          (Hour card template)
│   └── OutfitItemRow.xaml / .cs     (Outfit memo row)
├── Styles/
│   ├── ColorPaletteOverride.xaml    (Material palette overrides)
│   ├── Themes/TextBlock.xaml        (Custom typography styles)
│   └── Themes/Controls.xaml         (Card, Chip, Button overrides)
├── Assets/
│   └── Fonts/                       (PlayfairDisplay, Caveat, DMSans)
└── Strings/
    └── en/Resources.resw            (x:Uid localization)
```

---

## 12. Resolved Questions

| # | Question | Resolution |
|---|---|---|
| 1 | **Platform targeting** | v1 targets **Desktop, WASM, and Android** (`net10.0-desktop`, `net10.0-browserwasm`, `net10.0-android`). iOS and macOS deferred. |
| 2 | **Font licensing** | Playfair Display, Caveat, and DM Sans are all Google Fonts (OFL). Bundling as `.ttf` app assets is permitted. |
| 3 | **Analytics provider** | **Supabase**. Register an `IAnalyticsService` that POSTs event payloads to a Supabase Postgres table via its PostgREST endpoint with the anon key. Lightweight, serverless, no vendor SDK required. |
| 4 | **Shimmer/skeleton animation** | Build a custom shimmer `UserControl` for skeleton placeholders. `LoadingView` from Uno Toolkit handles the loading state wrapper; the shimmer effect is a custom visual layer inside it. |
| 5 | **PlaneProjection performance** | No fallback. Commit to `PlaneProjection` for flip cards on all three targets (Desktop, WASM, Android). |
| 6 | **SVG texture overlay** | The fabric-like dot pattern will be **generated** as an SVG asset during the build phase. Rendered as an `Image` with `IsHitTestVisible="False"` at top z-order. |
