# Project Instructions -- Reservoom WPF Migration

## Overview

This is a WPF-to-Uno-Platform migration of **Reservoom**, originally a hotel reservation system built with .NET 6 / WPF by SingletonSean.

**Source repo:** https://github.com/SingletonSean/reservoom
**Migration plan:** See `docs/MIGRATION-PLAN.md`
**Migration log:** See `docs/MIGRATION-LOG.md`

## Architecture

- Pattern: MVVM with Store pattern (retained from WPF) + Uno.Extensions Navigation
- Navigation: Uno Navigation (region-based) -- replaces WPF ContentControl + DataTemplate navigation
- DI: Microsoft.Extensions.DependencyInjection via Uno.Extensions.Hosting
- Data: Entity Framework Core 10.0.5 + SQLite
- State: HotelStore (reservation cache), NavigationStore (removed -- replaced by Uno Navigation)

## Original WPF Architecture

- Pattern: MVVM with Store pattern (centralized state)
- Key libraries: LoadingSpinner.WPF (replaced with ProgressRing)
- Data access: EF Core 5.0.5
- Database: SQLite (file-based, `reservoom.db`)
- Navigation: Custom ContentControl + DataTemplate type mapping via NavigationStore + NavigationService<T>
- DI: Microsoft.Extensions.Hosting + DependencyInjection
- Platform dependencies: None (no P/Invoke, COM, or Registry)
- Validation: INotifyDataErrorInfo on ViewModels, domain exceptions in ReservationBook

## Project Structure

- `ReservoomUno/` -- Uno Platform Single Project
- `ReservoomUno/Models/` -- Domain models (Hotel, Reservation, ReservationBook, RoomID) + DTOs
- `ReservoomUno/Presentation/` -- Pages and ViewModels
- `ReservoomUno/Services/` -- Service interfaces and implementations
- `ReservoomUno/Stores/` -- HotelStore (state management)
- `ReservoomUno/Commands/` -- ICommand implementations
- `ReservoomUno/DbContexts/` -- EF Core DbContext and factories
- `ReservoomUno/Exceptions/` -- Domain exceptions
- `ReservoomUno/Strings/en/` -- Localization resources
- `docs/` -- Migration documentation

## Migration Conventions

- Migrate business logic (Models, Services, Stores, Commands, ViewModels) first -- expect zero or minimal changes.
- Migrate XAML views last -- this is where most changes occur.
- Replace `{Binding}` with `x:Bind` where feasible. Keep `{Binding}` if the migration is mechanical.
- Replace WPF-only controls with WinUI equivalents per the control mapping table below.
- Replace LoadingSpinner.WPF with ProgressRing.
- Use `INavigator` for navigation via Uno.Extensions, never frame-based.
- Prefer Uno Toolkit controls (`NavigationBar`, `TabBar`, `SafeArea`) over raw WinUI equivalents.
- Keep XAML lean -- use Lightweight Styling and theme resources over inline values.
- Search Uno Platform docs via MCP before assuming API usage or patterns.
- Log every migration decision, gotcha, and workaround in `docs/MIGRATION-LOG.md`.

## Control Mapping Reference

| WPF Control | Uno/WinUI Equivalent | Notes |
|-------------|---------------------|-------|
| Window | Page (inside Shell) | Structural change |
| ContentControl (nav host) | Frame via Uno.Extensions Navigation | Region-based |
| ListView + GridView | ListView + DataTemplate | Direct equivalent |
| TextBox | TextBox | Identical |
| DatePicker | CalendarDatePicker | Name change |
| Button | Button | Identical |
| ScrollViewer | ScrollViewer | Identical |
| StackPanel/Grid | StackPanel/Grid | Identical |
| TextBlock | TextBlock | Identical |
| LoadingSpinner.WPF | ProgressRing | Built-in WinUI |
| BooleanToVisibilityConverter | Direct bool-to-Visibility binding | Implicit in Uno |
| MessageBox | ContentDialog | Async API (currently commented out in source) |

## Key References

- `docs/MIGRATION-PLAN.md` -- phased migration strategy
- `docs/MIGRATION-LOG.md` -- running log of changes, gotchas, patterns
- `docs/ARCHITECTURE.md` -- source and target architecture
- `docs/DESIGN-BRIEF.md` -- design language

## Hero Metric

**Campaign stat: "Zero lines changed in the business logic layer."**
Track every line changed in Models, Services, Stores, Commands, and ViewModels.

## Verification

```bash
dotnet build
dotnet run -f net10.0-desktop
```
