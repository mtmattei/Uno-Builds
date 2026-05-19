# Architecture Brief — YouTube × Microsoft (Uno Platform port)

**Status:** Draft v0.1 · Owner: DevRel · Target: WinAppSDK + Skia (Win/macOS/Linux/Wasm) · Mobile via Uno (iOS/Android)

This brief covers structure, dependencies, data flow, and the performance plan. It does not cover what the app looks like or how it feels — see the design and interactions briefs for those.

---

## 1. Solution layout

A single-head Uno Platform solution generated from `dotnet new unoapp`:

```
YouTubeMs/
├── YouTubeMs.csproj                  # single multi-target head (net10.0-windows, -ios, -android, -maccatalyst, -browserwasm, -desktop)
├── App.xaml(.cs)                     # Host wiring, DI, theme load, navigation registration
├── MainPage.xaml(.cs)                # Shell — sidebar + content frame (regions)
├── Pages/
│   ├── HomePage.xaml(.cs)            # The screen shown in the reference
│   ├── ExplorePage, SubscriptionsPage, LibraryPage, HistoryPage, WatchLaterPage, LikedPage
│   └── WatchPage.xaml(.cs)           # Video detail (out of scope for v1, route reserved)
├── Controls/
│   ├── VideoCard.xaml                # Reused in For You / Trending / search results
│   ├── RailItem.xaml                 # Compact recommendation row item
│   └── HeroFeatured.xaml             # The "Microsoft Featured" banner
├── Models/                           # MVUX models (one per screen)
│   ├── HomeModel.cs
│   └── ShellModel.cs
├── Services/
│   ├── ICatalogService.cs / CatalogService.cs   # Videos, channels, recommendations (Kiota client)
│   ├── INotificationService.cs
│   └── ISearchService.cs
├── Themes/
│   ├── ColorPaletteOverride.xaml
│   ├── App.xaml resource merges
│   └── Controls/                     # Lightweight style overrides
└── Strings/
    ├── en/Resources.resw
    └── fr/Resources.resw             # Localized via x:Uid
```

Single project head, multi-targeted. No platform-specific UI projects — the Skia renderer is the assumption ([rules: "all UI Controls and related features work on iOS, Android, macOS, Linux, Windows and WebAssembly"](#)).

## 2. UnoFeatures (csproj)

```xml
<UnoFeatures>
  Toolkit;
  Material;
  MVUX;
  Navigation;
  Hosting;
  Configuration;
  HttpKiota;
  Serialization;
  Localization;
  Logging;
  ThemeService;
  SkiaRenderer;
</UnoFeatures>
```

**Material is chosen over Fluent** per the Uno usage rules: when both are present, prefer Material. The Material v2 type scale and color tokens map cleanly onto the visual direction (see design brief). Cupertino is intentionally omitted — we want one design language across all platforms, not native chrome.

`HttpKiota` is preferred over Refit per the agent rules. `MVUX` over `Mvvm` because the screen is feed-shaped (lists with refresh, pagination, async load) — exactly what Feeds/ListFeeds were built for.

## 3. Composition root

`App.xaml.cs` uses the `IHostBuilder` extension pipeline:

- `UseConfiguration` + `appsettings.json` for endpoint URLs and feature flags
- `UseHttpKiota<ICatalogClient>()` for the typed API client
- `UseLogging` (Serilog sink in dev, console in release)
- `UseLocalization`
- `UseToolkitNavigation` + `UseNavigation(RegisterRoutes)` for region-based routing
- `UseSerialization`
- `UseThemeService` for runtime theme switching (system-driven only — we don't expose a manual toggle, per rules)

Services are registered by interface so the design surface uses constructor injection only — no service locator, no static state.

## 4. Navigation topology

Region-based, declared in XAML on `MainPage` — never invoked from code-behind ([rules: "ALWAYS prefer the XAML-based navigation attached properties (Navigation.Request or Region.Attached and Region.Name)"](#)).

```xml
<Grid uen:Region.Attached="True">
  <!-- Sidebar TabBar drives a named region "Shell" -->
  <utu:TabBar uen:Region.Attached="True" ... >
      <utu:TabBarItem uen:Region.Name="Home"          Content="Home"/>
      <utu:TabBarItem uen:Region.Name="Explore"       Content="Explore"/>
      ...
  </utu:TabBar>

  <Grid uen:Region.Name="Shell" uen:Region.Navigator="Visibility">
      <local:HomePage          uen:Region.Name="Home"           Visibility="Collapsed"/>
      <local:ExplorePage       uen:Region.Name="Explore"        Visibility="Collapsed"/>
      ...
  </Grid>
</Grid>
```

`Visibility` navigator keeps all panes alive after first visit so back/forward feels instant — fine here because each screen is bounded. For deeper hierarchies (Watch → Comments → Channel) we'd switch to `Frame` navigation under a nested route.

`RouteMap` registration uses `Nested` to mirror this hierarchy so deep links resolve cleanly: `/Home`, `/Home/Watch?id=...`, `/Subscriptions/Channel?slug=...`.

## 5. MVUX models

One model per screen. Models expose `IFeed<T>` / `IListFeed<T>` / `IState<T>` properties. The XAML binds to method names directly via implicit `IAsyncCommand` — no `[RelayCommand]` plumbing, no code-behind invocation ([rules: "ALWAYS using Bindings to bind the Command properties in XAML to the implicit IAsyncCommand that matches the public method name"](#)).

```csharp
public partial record HomeModel(ICatalogService Catalog)
{
    // One-shot fetch, refreshable
    public IFeed<FeaturedVideo>     Featured        => Feed.Async(Catalog.GetFeaturedAsync);

    // Paginated, supports incremental loading
    public IListFeed<Video>         ForYou          => ListFeed.AsyncPaginated(Catalog.GetForYouPageAsync);

    public IListFeed<Video>         Trending        => ListFeed.Async(Catalog.GetTrendingAsync);
    public IListFeed<RailVideo>     Recommendations => ListFeed.Async(Catalog.GetRailAsync);

    // Mutable
    public IState<string>           Query           => State<string>.Value(this, () => string.Empty);
    public IState<bool>             NotifPanelOpen  => State<bool>.Value(this, () => false);

    // Implicit command — bound from XAML as Command="{Binding Play}"
    public async ValueTask Play(Video v)            => await Navigator.NavigateAsync<WatchPage>(v.Id);
    public async ValueTask Search(string q)         => await Catalog.SearchAsync(q);
    public async ValueTask Refresh()                => await Featured.RefreshAsync();
}
```

`ShellModel` carries cross-screen state (current user, unread notification count) and is registered as a singleton.

## 6. Data layer

A single Kiota-generated `ICatalogClient` against a backend that exposes:

- `GET /featured` → one item, used for the hero
- `GET /for-you?cursor=...` → keyset-paginated (per the docs' guidance on paginated lists)
- `GET /trending` / `GET /recommendations`
- `GET /search?q=...`

`CatalogService` wraps the client with caching (in-memory, 60 s TTL on hot endpoints), maps DTOs to domain records, and exposes the cancellation token plumbing so model refreshes can be aborted.

For v1 prototyping, the same interface has a `MockCatalogService` that returns fixtures from embedded JSON — switched at composition time via `appsettings.Development.json`.

## 7. Performance plan

The reference UI has three lists rendered simultaneously (For You ×6+, Trending ×4+, Recommendations rail ×4+). Below the fold, more sections will be added. Decisions:

| Concern              | Decision                                                                                                                         |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| For You row          | `ItemsRepeater` inside a horizontal `ScrollViewer`; virtualized via `StackLayout`. Toolkit `ItemsRepeaterExtensions.IncrementalLoading` wired to the `IListFeed`. |
| Trending grid        | `ItemsRepeater` with `UniformGridLayout` (4 cols at Wide, 2 at Normal, 1 at Narrow).                                              |
| Recommendations rail | Plain `ItemsRepeater` — count is bounded (~6 items), no virtualization needed.                                                   |
| Subscriptions list   | `ListView` — could grow to 100s; native virtualization wins.                                                                      |
| Page scroll          | Single outer `ScrollViewer` wrapping the content `Grid`. Hero stays sticky-on-scroll via `Canvas.ZIndex` + scroll listener for v2. |
| Image loading        | `Image.Source` bound to URL; rely on Uno's native image cache. Thumbnails sized server-side (240×135 / 600×340). No raw 4K decoding on the client. |
| Cold startup         | AOT for iOS (mandatory) and Wasm (`<RunAOTCompilation>true</RunAOTCompilation>`). XAML resources flattened into the merged dictionary at build time. |
| Memory               | `ItemsRepeater` + `Visibility` navigator means ~4 screens stay in memory at once. Acceptable; if it bites we move to `Frame`-based for seldom-used routes. |
| Threading            | All `IListFeed` work runs on the threadpool by construction; UI dispatches happen inside MVUX. We never `Task.Wait` or `.Result` anywhere. |

## 8. Diagnostics

`ILogger<T>` injected everywhere. Categories: `YouTubeMs.Catalog`, `YouTubeMs.Nav`, `YouTubeMs.Theme`. In dev, Serilog writes to Console + file with structured properties (`{VideoId}`, `{Latency}`). On Wasm, the browser console is the sink.

## 9. Testing

- **Unit:** `xUnit` against `HomeModel` with a fake `ICatalogService` — verifies feed states, refresh, command behavior.
- **UI:** Uno UI test runner for two smoke flows — render Home, click Play on a card.
- **Hot Reload** is the primary inner loop ([rules: hot reload enabled via `DOTNET_MODIFIABLE_ASSEMBLIES=debug`](#)). XAML edits land without rebuild; C# edits land for non-structural changes.

## 10. Open architectural questions

1. **Watch page transport** — embedded `MediaPlayerElement` (MediaPlayerElement UnoFeature) vs platform-native player handoff. Native handoff wins on battery and codec support; we lose the design system.
2. **Notification panel** — currently a flyout pinned to the bell. As volume grows we'll want push (signalR or WebPush). Out of scope for v1.
3. **Auth** — `AuthenticationOidc` is the planned path but unscoped here. v1 ships unauthenticated/mocked.

---
*Cross-references: design tokens are defined in the design brief; motion durations in the interactions brief. Source paths cited above resolve into the official Uno docs at `platform.uno/docs`.*
