# 04 — API + Auth Integration Spec

Session: `api-auth-integration`
Date: 2026-05-08
Status: Draft (planning, not implemented)

## Goals

- **v1 (this spec, build now):** Replace mock data on the Home page with live YouTube Data API v3 reads, using an API key only. No user OAuth.
- **v1.5 (build next):** Add `Search` against `search.list`.
- **v2 (forward design only — do not build yet):** Add Google OAuth so Subscriptions / Library / WatchLater / Liked can hit user-scoped endpoints.

## Non-goals

- No user OAuth in v1.
- No subscriptions / library / history / watchlater / liked behind real data in v1 — those pages stay on mock.
- No mobile, no Windows native (WinAppSDK) targets. Build set stays `net10.0-desktop` + `net10.0-browserwasm`.
- No video playback changes — `YouTubeId` flows through unchanged.

## Current baseline

- `ICatalogService` (Services/Catalog) — 6 methods, all `ValueTask`-returning.
- `MockCatalogService` reads embedded `Assets/Mocks/catalog.json` via `Lazy<CatalogSnapshot>`.
- Hosting (`App.xaml.cs`): `UseHttp`, `UseConfiguration.Section<AppConfig>()`, DI registers `MockCatalogService` as `ICatalogService` singleton.
- `UnoFeatures`: `Material; Dsp; Hosting; Toolkit; Logging; MVUX; Configuration; HttpKiota; Serialization; Localization; Navigation; ThemeService;`.
- `AppConfig` currently has only `Environment`. No API config section.
- `DebugHttpHandler` already registered as `DelegatingHandler` in DEBUG.

## v1 scope — Home via API key

### Endpoint mapping

| App method                | YouTube Data API v3 endpoint                                  | Quota |
|---------------------------|---------------------------------------------------------------|-------|
| `GetFeaturedAsync`        | `videos.list?chart=mostPopular&maxResults=1` (region-scoped)  | 1     |
| `GetTrendingAsync`        | `videos.list?chart=mostPopular&maxResults=20` (region-scoped) | 1     |
| `GetForYouAsync`          | `videos.list?chart=mostPopular&videoCategoryId={cat}&maxResults=20` | 1 |
| `GetRailAsync`            | `playlistItems.list?playlistId={configured}` + `videos.list` to enrich durations/views | 2 |
| `GetSubscriptionsAsync`   | **stays on mock** in v1                                       | n/a   |
| `SearchAsync`             | `search.list?q={query}&type=video` — **deferred to v1.5**     | 100   |

**Cold home load:** ~5 quota units (Trending + ForYou + Featured-from-Trending-cache + Rail enrichment). Featured can reuse the Trending response's first item — no extra call.

### Quota & caching strategy

- 10,000 units/day default — cold home load fits easily; search is the cliff.
- In-memory snapshot cache keyed by endpoint + region + category, TTL **10 minutes** for `mostPopular`, **1 hour** for the rail playlist.
- On 403 `quotaExceeded`: surface via `IFeed` error state (already wired through MVUX) and fall back to last cached snapshot if any. The `ErrorState` control already exists.
- No retries on 4xx. Single retry with exponential backoff on 5xx / network errors.

### DTO mapping (YouTube → app models)

YouTube `videos.list` returns `items[].{id, snippet, contentDetails, statistics}`. Mapping to `Video`:

- `Video.Id` ← `items[].id`
- `Video.YouTubeId` ← `items[].id` (same — keeps watch URL helpers working)
- `Video.Title` ← `items[].snippet.title`
- `Video.ThumbnailUrl` ← `items[].snippet.thumbnails.medium.url` (320×180); for hero use `maxres` (1280×720) with fallback chain `maxres → standard → high`
- `Video.Duration` ← parse ISO-8601 `items[].contentDetails.duration` via `System.Xml.XmlConvert.ToTimeSpan` (zero deps)
- `Video.ViewCount` ← `long.Parse(items[].statistics.viewCount)`
- `Video.PublishedAt` ← `items[].snippet.publishedAt`
- `Video.Channel` ← derived (see below)

`FeaturedVideo`:
- `BackgroundImageUrl` ← `maxres` thumbnail (1280×720). Hero is currently 1600×640 — accept the size mismatch, `UniformToFill` covers it. We'll revisit later.
- `BadgeText` ← localized "Trending" string for v1 (no real "featured" concept from API).
- `Description` ← `items[].snippet.description` (truncate to ~240 chars at render time).

### Channel derivations (no API field exists)

YouTube's API gives channel name, ID, thumbnail, but **not brand gradient or initials**. These are app-only concepts:

- `Channel.Name` ← `items[].snippet.channelTitle`
- `Channel.Slug` ← `items[].snippet.channelId`
- `Channel.AvatarUrl` ← from a separate `channels.list?part=snippet&id={ids}` call (1 unit per ≤50 channels). Cache aggressively — channels rarely change.
- `Channel.IsVerified` ← `false` initially (API doesn't expose verified flag publicly; revisit if needed).
- `Channel.Initials` ← derived: first letter of first two words of `Name`, uppercased.
- `Channel.AccentTop / AccentBottom` ← derived deterministically from `channelId` hash → HSL hue, lightness range 35–60 for top, 15–35 for bottom. Implementation in a `ChannelAccentPalette` static helper.

### Service layer changes

New file structure under `Services/Catalog/`:

```
Catalog/
├── ICatalogService.cs                 (unchanged contract)
├── MockCatalogService.cs              (unchanged, kept for fallback + tests)
├── YouTube/
│   ├── YouTubeCatalogService.cs       (ICatalogService impl)
│   ├── YouTubeDataApiClient.cs        (thin Refit-style or HttpClient wrapper)
│   ├── Dto/
│   │   ├── VideoListResponse.cs
│   │   ├── PlaylistItemsResponse.cs
│   │   ├── ChannelListResponse.cs
│   │   └── ...
│   ├── Mapping/
│   │   ├── VideoMapper.cs
│   │   └── ChannelAccentPalette.cs
│   └── Caching/
│       └── CatalogCache.cs            (MemoryCache wrapper, TTL'd)
```

`YouTubeCatalogService` keeps subscriptions on a delegated mock instance for v1:

```csharp
public sealed class YouTubeCatalogService(
    YouTubeDataApiClient api,
    CatalogCache cache,
    MockCatalogService mockFallback) : ICatalogService { ... }
```

`GetSubscriptionsAsync` delegates to `mockFallback` until v2.

### DI / config selector

`appsettings.json`:

```json
{
  "AppConfig": { "Environment": "Production" },
  "YouTube": {
    "ApiKey": "",
    "BaseUrl": "https://www.googleapis.com/youtube/v3/",
    "Region": "US",
    "ForYouCategoryId": "10",
    "RailPlaylistId": "PL..."
  }
}
```

`appsettings.development.json` — non-committed override for local API key (add to `.gitignore` rule audit).

`App.xaml.cs` `ConfigureServices` becomes:

```csharp
.ConfigureServices((context, services) =>
{
    services.Configure<YouTubeOptions>(context.Configuration.GetSection("YouTube"));
    services.AddSingleton<MockCatalogService>();          // available for fallback
    services.AddSingleton<CatalogCache>();
    services.AddSingleton<YouTubeDataApiClient>();

    var key = context.Configuration["YouTube:ApiKey"];
    if (string.IsNullOrWhiteSpace(key))
        services.AddSingleton<ICatalogService>(sp => sp.GetRequiredService<MockCatalogService>());
    else
        services.AddSingleton<ICatalogService, YouTubeCatalogService>();
})
```

This means: **no key configured → mock; key present → real**. Same binary, no `#if` flags. CI builds without a key just keep working on mock.

### Secrets posture

- **Desktop:** key is in `appsettings.development.json` for dev; for shipped builds, accept that desktop binaries leak the key (any user can extract). Mitigation: restrict the key in Google Cloud Console by API (YouTube Data v3 only) and quota.
- **WASM:** key ships in the JS bundle and is fully public. Mitigation: HTTP referrer restriction in Google Cloud Console pinned to the deployment domain. Acceptable for v1 dev/demo. **Do not deploy publicly** without either (a) a tiny backend proxy that holds the key, or (b) accepting the abuse risk on a referrer-restricted key.
- Add `appsettings.development.json` and any `appsettings.local.json` to `.gitignore` if not already.

## v1.5 — Search

Single `search.list` call per query. Cost is 100 units/call. Add:
- 500 ms debounce in the search input (prevent burst).
- Cache by query string for 10 minutes.
- Cap to 50 searches/day per app instance via a soft counter; surface a clean error past the cap.

This is small enough to fold into v1 if scope allows; spec it separately so v1 doesn't grow.

## v2 — OAuth (forward design, not implemented)

Trigger: when we want real Subscriptions / Library / WatchLater / Liked.

**Scopes:** `https://www.googleapis.com/auth/youtube.readonly` is enough for read-only user data. Liked-videos add/remove would need `https://www.googleapis.com/auth/youtube`.

**Flows by target:**

| Target          | Flow                                        | Notes |
|-----------------|---------------------------------------------|-------|
| Skia Desktop    | OAuth 2.0 loopback (PKCE, public client)    | Spawn system browser → `http://127.0.0.1:{ephemeral}/`. No client secret. |
| WASM            | OAuth 2.0 redirect (PKCE, public client)    | Google supports PKCE without secret for browser apps. CORS is allowed on `oauth2.googleapis.com/token`. Redirect URI must be registered. |

**Library:** `Uno.Extensions.Authentication.Oidc` already integrates with the host builder. It handles token cache per platform (DPAPI on Windows, IndexedDB on WASM via Uno's secure storage abstractions). Token refresh is automatic.

**Hosting integration sketch:**

```csharp
.UseAuthentication(auth => auth
    .AddOidc(oidc => oidc
        .Authority("https://accounts.google.com")
        .ClientId("{from Google Cloud Console}")
        .Scopes("openid", "profile", "email", "https://www.googleapis.com/auth/youtube.readonly")
        .RedirectUri(...)            // per-target
    ))
```

**Service shape:** `IAuthenticatedCatalogService : ICatalogService` (or just expand `ICatalogService` once auth ships) with subs/library/etc. now hitting real endpoints. Auth-required calls wrapped in a delegating handler that injects `Authorization: Bearer {access_token}` and triggers re-auth on 401.

**Open in v2 design:** whether to keep `ICatalogService` as one service (auth state internal) or split into `IPublicCatalogService` + `IUserCatalogService`. Lean toward one service with a `bool IsAuthenticated` exposed as `IFeed<bool>` consumed by pages — keeps `HomeModel` untouched. Decide when v2 starts.

## Migration sequence (suggested commits)

1. `feat(config): add YouTube options and dev appsettings override` — adds `YouTubeOptions`, gitignore entry, empty placeholder section.
2. `feat(catalog): add YouTube DTOs and Refit-style API client (no DI yet)` — `YouTubeDataApiClient` + DTOs, unit-testable in isolation.
3. `feat(catalog): map videos.list and channels.list to app models` — `VideoMapper`, `ChannelAccentPalette`, ISO-8601 duration parsing.
4. `feat(catalog): YouTubeCatalogService for Home endpoints with caching` — `CatalogCache` + service impl. Subscriptions still delegate to mock.
5. `feat(catalog): wire DI selector — mock if no key, real if key present` — `App.xaml.cs` change.
6. `chore: smoke test home with real API` — manual verification on Desktop and WASM.

Each commit must `dotnet build` clean. Keep mock service alive throughout.

## Verification

For each commit:

```bash
dotnet build YouTubeMs/YouTubeMs.csproj -f net10.0-desktop
dotnet build YouTubeMs/YouTubeMs.csproj -f net10.0-browserwasm
```

Smoke (after step 5) — both with `YouTube:ApiKey` set:
- Home featured renders a real trending video with thumbnail, channel, duration, view count.
- Trending rail shows ≥10 items.
- Network tab on WASM shows `https://www.googleapis.com/youtube/v3/videos?...` with the configured key.
- Stop the dev key and reload — app falls back to mock without exception (after restart).

## Unresolved questions

1. **Region selection.** Hardcode `US`, take from `CultureInfo.CurrentCulture`, or expose a setting? Defaulting to `US` is the safest for content density; deriving from culture risks empty `mostPopular` results in some regions.
2. **"For You" semantics without auth.** `mostPopular` filtered by category is the placeholder. Which category? `10` (Music) is on-brand for "MSYouTube" but narrows content. Alternatives: rotate categories, or expose a category picker. Decide before step 4.
3. **Rail content source.** Is there a specific curated playlist you want pinned, or should the rail be "popular gaming" / "popular music" / etc.? Need a `RailPlaylistId` value to put in config.
4. **Channel verified flag.** YouTube Data API v3 doesn't expose verified status. Drop the visual indicator, or scrape from somewhere else (out of scope for v1)?
5. **`BadgeText` for Featured.** Currently mock data sets things like "PREMIERING NOW". With real data, all we know is "this is #1 most popular today" — is `"Trending #1"` acceptable as the badge?
6. **Hero image aspect ratio.** Featured background is currently 1600×640 (2.5:1). YouTube `maxres` is 1280×720 (16:9). Accept letterboxing/cropping via `UniformToFill`, or commission a different layout for the API era?
7. **WASM secrets policy for the demo deployment.** If this gets deployed publicly (e.g. GitHub Pages), do we accept a referrer-restricted public key, or does it justify a tiny backend proxy now? The proxy also unblocks server-side caching (further quota wins).
8. **Search in v1 or v1.5.** v1.5 is cleaner, but if you want search live on day one, fold it into the v1 sequence between steps 5 and 6.
9. **Subs/Library mock retention.** Once auth ships in v2, do we keep `MockCatalogService` around for offline dev / CI, or rip it out?
