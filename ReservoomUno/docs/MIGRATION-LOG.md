# Migration Log

**Source:** Reservoom (https://github.com/SingletonSean/reservoom)
**Started:** 2026-04-10
**Status:** Complete

---

## Phase 0: Project Setup

- Scaffolded Uno project with `dotnet new unoapp --preset=recommended --tfm net10.0`
- Uno.Sdk 6.5.31, .NET 10.0.200, EF Core SQLite 10.0.5
- UnoFeatures: Material, Hosting, Toolkit, Navigation, MVUX, Logging, Configuration, Localization, SkiaRenderer
- Build succeeded with 0 errors
- Set up starter kit: CLAUDE.md, .mcp.json, .claude/settings.json, docs/

## Phase 1: Entities, DTOs, Exceptions

- Copied 7 files with namespace changes only (`Reservoom` -> `ReservoomUno`)
- **Gotcha:** `[Key]` attribute ambiguous between `System.ComponentModel.DataAnnotations.KeyAttribute` and `Uno.Extensions.Equality.KeyAttribute`. Fixed with full qualification.
- **Pattern:** ATTRIBUTE_AMBIGUITY -- Uno.Extensions adds KeyAttribute to global scope. Always fully qualify EF Core's [Key].
- Lines changed (excl. namespace): 1 (Key attribute)

## Phase 2: DbContext and EF Core

- Copied 5 files with namespace changes only
- EF Core upgraded from 5.0.5 to 10.0.5 (package version only, no code changes)
- `Database.EnsureCreated()` used instead of `Database.Migrate()` (no migration files carried over)
- Lines changed (excl. namespace): 0

## Phase 3: Services and Stores

- Copied 7 service files with namespace changes only
- HotelStore copied unchanged
- **NavigationStore and NavigationService<T> NOT migrated** -- replaced by Uno.Extensions Navigation
- Lines changed (excl. namespace): 0

## Phase 4: ViewModels and Commands

- ViewModelBase: unchanged
- ReservationViewModel: unchanged
- ReservationListingViewModel: removed NavigationService dependency, MakeReservationCommand property (navigation now in XAML)
- MakeReservationViewModel: changed constructor to accept `INavigator` instead of `NavigationService`, removed CancelCommand (XAML navigation), updated default dates to `DateTime.Today`
- CommandBase, AsyncCommandBase: unchanged
- LoadReservationsCommand: removed unused `using System.Windows`
- MakeReservationCommand: changed NavigationService to `Action` callback pattern
- **NavigateCommand<T> NOT migrated** -- replaced by `uen:Navigation.Request` in XAML
- **MainViewModel NOT migrated** -- replaced by Shell + Uno.Extensions Navigation
- Lines changed (excl. namespace): ~21

## Phase 5: XAML Views

- ReservationListingView (UserControl) -> ReservationListingPage (Page)
  - ListView+GridView -> ListView with card-based DataTemplate
  - LoadingSpinner.WPF -> ProgressRing
  - BooleanToVisibilityConverter -> implicit bool-to-Visibility
  - InverseBooleanToVisibilityConverter -> custom InverseBoolConverter
  - Style Triggers (WPF) -> binding-based visibility
  - Added NavigationBar, Material styling
  - `Command="{Binding MakeReservationCommand}"` -> `uen:Navigation.Request="MakeReservation"`
- MakeReservationView (UserControl) -> MakeReservationPage (Page)
  - DatePicker -> CalendarDatePicker (with DateTimeToDateTimeOffsetConverter)
  - SharedSizeGroup -> Grid column sizing
  - LoadingSpinner.WPF -> ProgressRing
  - DataTrigger -> binding-based IsEnabled
  - Added NavigationBar with back button
  - CancelCommand -> `uen:Navigation.Request="-"`
- MainWindow (Window) -> Shell + Uno.Extensions region-based navigation

## Phase 6: DI and Navigation

- Uno.Extensions Hosting replaces Microsoft.Extensions.Hosting
- All services registered in App.xaml.cs ConfigureServices
- Hotel configured from appsettings.json `HotelName`
- SQLite database path: `ApplicationData.Current.LocalFolder.Path/reservoom.db`
- Route registration: Listing (default), MakeReservation
- ViewMap binds pages to ViewModels for auto-resolution

---

## Recurring Patterns Observed

| Pattern | Description | Frequency | Reusable? |
|---------|-------------|-----------|-----------|
| ATTRIBUTE_AMBIGUITY | Uno.Extensions adds KeyAttribute that conflicts with EF Core's | Once | Yes -- any EF Core migration |
| NAV_STORE_REMOVAL | Custom NavigationStore replaced by Uno.Extensions INavigator | Once | Yes -- any custom MVVM nav |
| LOADING_SPINNER | WPF LoadingSpinner -> ProgressRing | 2 views | Yes |
| DATE_CONTROL_TYPE | WPF DatePicker (DateTime) -> CalendarDatePicker (DateTimeOffset?) | 2 controls | Yes -- needs converter |
| IMPLICIT_BOOL_VIS | BooleanToVisibilityConverter removed -- implicit in Uno | Multiple | Yes |
| TRIGGERS_REMOVAL | WPF Style.Triggers/DataTrigger -> binding-based approach | 1 control | Yes |

---

## Gotchas and Workarounds

| Issue | Workaround | Root Cause | Reported? |
|-------|-----------|------------|-----------|
| [Key] attribute ambiguous | Fully qualify `[System.ComponentModel.DataAnnotations.Key]` | Uno.Extensions.Equality.KeyAttribute in scope | No |
| CalendarDatePicker uses DateTimeOffset? | Created DateTimeToDateTimeOffsetConverter | WinUI date controls use DateTimeOffset, not DateTime | No |
| No GridView in WinUI ListView | Used card-based ItemTemplate instead | GridView is WPF-only ListView detail view | No |
| Nullable warnings from .NET 6 code | Left as warnings (no business logic changes) | .NET 10 has stricter nullable analysis | No |

---

## Lines Changed Tracker (Hero Metric)

| Layer | Files | Source LOC | Lines Changed (excl. namespace) | % Changed |
|-------|-------|------------|--------------------------------|-----------|
| Models/Entities | 4 | 179 | 0 | 0% |
| DTOs | 1 | 20 | 1 | 5% |
| Exceptions | 2 | 49 | 0 | 0% |
| DbContext/Data | 5 | 120 | 0 | 0% |
| Services | 6 | 163 | 0 | 0% |
| Stores | 1 | 63 | 0 | 0% |
| ViewModels | 4 | 380 | 12 | 3.2% |
| Commands | 4 | 200 | 9 | 4.5% |
| **Total Business Logic** | **27** | **1,174** | **22** | **1.9%** |

### Files NOT Migrated (replaced by Uno.Extensions Navigation):
- NavigationStore.cs (31 LOC) -- replaced by INavigator
- NavigationService.cs (27 LOC) -- replaced by INavigator
- NavigateCommand.cs (26 LOC) -- replaced by `uen:Navigation.Request`
- MainViewModel.cs (29 LOC) -- replaced by Shell

### New Infrastructure Files:
- InverseBoolConverter.cs (NEW)
- DateTimeToDateTimeOffsetConverter.cs (NEW)
- ReservationListingPage.xaml/cs (ported from WPF)
- MakeReservationPage.xaml/cs (ported from WPF)

---

## What Was Easy

- Models, Entities, DTOs: 100% unchanged (copy-paste + namespace)
- DbContext + EF Core: 100% unchanged -- SQLite works cross-platform
- Services: 100% unchanged -- clean interfaces, no platform dependencies
- HotelStore: 100% unchanged -- pure business logic
- DI registration: nearly identical to WPF version
- CommandBase / AsyncCommandBase: unchanged -- ICommand is the same

## What Was Difficult

- Navigation system replacement: custom ContentControl+DataTemplate -> Uno.Extensions regions (biggest structural change)
- DatePicker type mismatch: WPF DateTime vs WinUI DateTimeOffset? (needed converter)
- [Key] attribute ambiguity with Uno.Extensions.Equality (surprising gotcha)
- GridView removal: WPF's ListView.View property has no WinUI equivalent

## Where WPF Design Helped

- Clean MVVM with Store pattern made the business logic fully portable
- Interface-based services (IReservationProvider, etc.) decoupled from any UI framework
- DI registration pattern works identically in both frameworks
- No P/Invoke, COM, or platform-specific code

## Where WPF Design Hurt

- Custom NavigationStore/NavigationService pattern had to be replaced entirely
- WPF-specific controls (GridView in ListView, DatePicker, Style.Triggers)
- LoadingSpinner.WPF NuGet dependency (WPF-only)
