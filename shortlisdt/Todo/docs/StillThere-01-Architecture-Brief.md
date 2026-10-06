# StillThere — Architecture Brief

Target: **Uno Platform**, single codebase, two heads — `net9.0-desktop` (Skia) and `net9.0-android` (Skia renderer). Source of truth for behavior is the web prototype `stillthere.html`.

Grounded in Uno Platform official guidance (MVUX, Skia renderer, Storage, Toolkit) retrieved via the Uno docs MCP.

---

## Scope

**v1 (this spec):** one page — the time-aware task list — with add, inline edit (title / notes / tags), complete (with the scratch-out animation), break-down, snooze-with-reason, delete, search, tag filter, state filter, and a collapsible completed section. Local persistence. Live 1 Hz aging.

**Roadmap (architected for, not built):** Focus Mode, Insights, Dashboard. The shell is structured so these slot in as navigation regions later without reworking the data layer.

---

## Platform & rendering baseline

`Framework best practice.` Skia is the default rendering engine as of Uno.Sdk 6.0+ on every target except WinAppSDK, including Desktop (Windows/macOS/Linux) and Android. The whole visual tree is drawn on a Skia canvas with no native views, which gives one pixel-identical UI across Desktop and Android.

Why this matters for StillThere specifically: the signature interaction is a procedurally generated hand-drawn scratch-out. With the Skia renderer, the same `SKCanvasElement` draw code produces the same result on both heads — no per-platform animation fork.

```
Decision: Skia renderer on both heads (net9.0-desktop, net9.0-android).
Reason: Single visual tree → the custom scratch + collapse animations render identically; no native-control divergence to test around.
Tradeoff: Android no longer uses native controls, so platform-native scroll/overscroll feel is emulated by Uno rather than inherited from Android. Acceptable for a design-led app where visual consistency is the priority.
```

Required `UnoFeatures` (csproj): `MVUX`, `Toolkit`, `Storage`, `Serialization`, `Hosting`, `Extensions`, `Skia`, `SkiaRenderer`. `Svg` only if we choose the SVG fallback for the scratch (see Interaction Brief — primary path is SkiaSharp, so `Svg` is optional).

`Opinion.` Skip `Material`/`Cupertino` theme packs. The design is an ultra-restrained custom system; pulling in Material brings ripple, elevation, and a type scale we would spend more time overriding than authoring. We define our own resource dictionaries instead. (Uno's usage rules prefer Material *when present* — the lever here is to not include it, which is a supported configuration.)

---

## State management — MVUX

`Framework best practice.` Use MVUX. The app has durable, editable, locally-owned state that mutates from many actions and must re-derive a filtered/sorted view continuously. That is the `IListState<T>` use case (editable collection), as opposed to `IListFeed<T>` (read-only, service-pulled).

### Model

One MVUX model backs the page: `ListModel`.

```
TaskItem (record)          immutable; mutations produce a new record
  Id            Guid
  Title         string
  Notes         string
  Tags          ImmutableArray<string>
  CreatedAt     DateTimeOffset
  SnoozeReason  string?          // logged only; clock keeps running

CompletedItem (record)
  (TaskItem fields) + CompletedAt DateTimeOffset

AgeState (enum)   Fresh | Active | Aging | Stale | Rotten
FilterMode (enum) All | StalePlus
```

`AgeState` is a pure function of `Now - CreatedAt`; it is never stored. Thresholds: Fresh < 2 h, Active < 24 h, Aging < 3 d, Stale < 7 d, Rotten ≥ 7 d.

### Feeds & states on `ListModel`

```
IListState<TaskItem>        Tasks            // the editable source of truth
IListState<CompletedItem>   Completed
IState<string>              SearchQuery
IState<FilterMode>          Filter
IState<string?>             ActiveTag
IState<DateTimeOffset>      Now              // advanced by the 1 Hz tick
IState<bool>                ShowCompleted

IFeed<IImmutableList<TaskItemVm>> VisibleTasks   // derived: sort + filter + search + age
IFeed<IImmutableList<TagCount>>   TagCounts        // derived from Tasks
IFeed<HeaderSummary>              Header           // open count, oldest age, filter active
```

`Framework best practice.` `VisibleTasks` is a derived feed combining `Tasks`, `Now`, `Filter`, `ActiveTag`, `SearchQuery`. Sorting (oldest first) and filtering live in the model, never in XAML. Each emitted `TaskItemVm` carries the precomputed `AgeState` and a formatted age string so the view binds directly.

`Now` is what makes ages live. The 1 Hz tick updates `Now`; every derived feed that depends on it re-emits, refreshing age strings and state transitions without per-item timers.

### Commands

`Framework best practice.` MVUX auto-generates `IAsyncCommand`s from public methods on the model. XAML binds to them by name; code-behind never invokes them.

```
Add(string rawTitle)          // parses inline #tags, appends with CreatedAt = now
Update(Guid id, string title, string notes, IEnumerable<string> tags)
Complete(Guid id)             // moves Task → Completed (CompletedAt = now)
BreakDown(Guid id, IEnumerable<string> steps)  // split; children inherit tags, fresh clocks
Snooze(Guid id, string reason)
Delete(Guid id)               // hard delete; not archived
Restore(Guid id)              // Completed → Tasks; CreatedAt preserved
ClearCompleted()
ToggleFilter()                // All ↔ StalePlus
SetTag(string? tag)           // null clears
SetSearch(string query)
ClearFilters()
ToggleCompleted()
```

```
Decision: IListState<TaskItem> as the single mutable source; VisibleTasks is a derived feed.
Reason: All actions mutate one collection; the on-screen list is a pure projection (sort+filter+search+age). MVUX recomputes the projection automatically on any input change, including the Now tick.
Tradeoff: Every tick re-runs the projection. At realistic task counts (tens, low hundreds) this is trivial; if a user ever held thousands of open tasks we would throttle age-string recompute. Documented, not pre-optimized.
```

---

## The 1 Hz tick

`Project convention.` A single `DispatcherTimer` (1 s interval) owned by `ListModel`, started on activation, stopped on deactivation. Its tick updates the `Now` state on the UI thread. No per-row timers.

`Framework best practice.` Age-state transitions (e.g., Stale → Rotten) fall out of the `Now`-dependent projection; the view reacts through binding. The timer touches exactly one piece of state.

```
Decision: One model-level DispatcherTimer driving a Now state, vs. a timer per visible row.
Reason: O(1) timers regardless of list length; transitions are derived, not pushed per item.
Tradeoff: Sub-second precision is not guaranteed (1 s cadence). The domain is hours-to-days of aging, so second-level jitter is invisible.
```

---

## Persistence

`Framework best practice.` Persist with `Windows.Storage.ApplicationData.Current.LocalFolder` (cross-platform on Skia Desktop and Android via the `Storage` feature). One JSON document holds `{ tasks, completed }`.

`Common convention.` Serialize with `System.Text.Json` using a **source-generated** `JsonSerializerContext`. Android ships trimmed/AOT-leaning builds; reflection-based serialization is fragile under trimming, source-gen is safe.

Layered behind an interface so the model never touches the filesystem directly:

```
ITaskStore
  Task<StoreSnapshot> LoadAsync()
  Task SaveAsync(StoreSnapshot snapshot)

JsonTaskStore : ITaskStore     // ApplicationData LocalFolder, source-gen JSON
```

`Project convention.` Saves are **debounced** (~400 ms trailing) and coalesced — a burst of edits writes once. Load runs once on activation and hydrates `Tasks`/`Completed`; if the file is missing, seed nothing (real empty state) or the demo set behind a build flag.

```
Decision: Single JSON document in LocalFolder behind ITaskStore, debounced writes.
Reason: The data is small and always read/written together; one document avoids partial-write races and keeps the repository trivial. The interface keeps the model testable with an in-memory fake.
Tradeoff: No incremental/row-level writes. Fine at this scale; revisit only if the document grows large enough that full rewrites stutter.
```

---

## Dependency injection & host

`Framework best practice.` Use the Uno.Extensions `IHostBuilder`. Register `ITaskStore`, the serializer context, and the model. The model receives `ITaskStore` by constructor injection.

```
App host:
  services.AddSingleton<ITaskStore, JsonTaskStore>();
  services.AddTransient<ListModel>();
```

---

## Navigation & shell

`v1:` one page (`ListPage`) hosted directly in the window. No `Frame` required.

`Roadmap.` `Framework best practice.` When Focus Mode / Insights / Dashboard arrive, introduce Uno.Extensions Navigation with **region navigation** and a `TabBar` — bottom bar on Android, vertical rail on Desktop (selected via the Toolkit `Responsive` markup extension). Navigation is declared in XAML (`Region.Attached`, `Navigation.Request`); code-behind never calls navigation APIs.

`Decision (deferred):` Structure `ListPage` as a region-ready root now (a single named content region), so adding the TabBar later is additive. Do not build the TabBar or extra pages in v1 (Do-Not-Overbuild).

---

## Project structure

```
StillThere/                      (shared class library, the app)
  App.xaml(.cs)                  host, resources merge
  Presentation/
    ListPage.xaml(.cs)           the one page
    ListModel.cs                 MVUX model (states, feeds, commands, tick)
    TaskItemVm.cs                projected view record (AgeState + formatted strings)
  Domain/
    TaskItem.cs  CompletedItem.cs  AgeState.cs  FilterMode.cs
    AgeRules.cs                  thresholds + AgeState/formatting pure functions
    TagParser.cs                 inline #tag extraction + normalization
  Services/
    ITaskStore.cs  JsonTaskStore.cs  StoreSnapshot.cs  StoreJsonContext.cs
  Controls/
    ScratchView.cs               SKCanvasElement subclass (completion scratch)
  Themes/
    Colors.xaml  Brushes.xaml  TextBlock.xaml  Styles.xaml
StillThere.Desktop/              net9.0-desktop head
StillThere.Mobile/               net9.0-android head  (iOS slot available, not targeted now)
```

---

## Testing & validation

`Framework best practice.`
- **Unit (pure domain):** `AgeRules` thresholds and boundary times; `TagParser` (`#a #a` dedupes, casing, punctuation); break-down step parsing; oldest-first ordering; combined filter+search+tag logic. No UI dependency.
- **Model:** drive `ListModel` with an in-memory `ITaskStore`; assert `VisibleTasks` projections for each filter combination, restore preserving `CreatedAt`, complete moving to `Completed`, debounced save firing once per burst.
- **Runtime:** see the Interaction Brief's runtime verification steps; validate both heads with Hot Reload (Desktop first for speed, then Android).
- **Trimming:** Android Release build must round-trip the JSON store — guards the source-gen serializer.

---

## Architecture Brief — summary table

| Concern | Choice | Primitive |
|---|---|---|
| Rendering | Skia, both heads | Uno.Sdk 6.0+ default |
| State | MVUX | `IListState`, `IState`, derived `IFeed` |
| Live aging | one timer → `Now` state | `DispatcherTimer` |
| View projection | sort+filter+search+age in model | derived `IFeed` |
| Persistence | one JSON doc, debounced | `ApplicationData.LocalFolder` + source-gen `System.Text.Json` |
| DI | host builder | `IHostBuilder` |
| Navigation | single page (v1) | region-ready for roadmap |
| Signature animation | SkiaSharp custom draw | `SKCanvasElement` |
| Adaptive | width breakpoints | Toolkit `Responsive` markup |

---

## Unresolved Questions

- TFM pin: target `net9.0` heads on current Uno.Sdk 6.x, or pin to a specific Uno.Sdk patch you are already standardized on across the other projects?
- Demo seed: ship the 7-task demo set behind a `DEBUG`/first-run flag, or start every install on a true empty state?
- iOS: leave the mobile head Android-only, or keep an iOS TFM in the csproj now (cost is near-zero given Skia) for a later switch-on?
- Snooze semantics: the prototype logs a reason and keeps the clock running. Confirm that is the intended product behavior (vs. snooze actually pausing/deferring age) — it changes whether `SnoozeReason` is purely informational or affects the age projection.
