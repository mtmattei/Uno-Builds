# Architecture

## Original Architecture (WPF)

### Pattern
MVVM with Store pattern (centralized state management). Clean layered architecture created by SingletonSean for educational purposes.

### Layers
```
Views (XAML UserControls)
  -> ViewModels (INotifyPropertyChanged + INotifyDataErrorInfo)
    -> Commands (ICommand, AsyncCommandBase)
      -> Stores (HotelStore = cache, NavigationStore = nav state)
        -> Services (IReservationProvider, IReservationCreator, IReservationConflictValidator)
          -> DbContext (EF Core 5.0.5 + SQLite)
            -> DTOs (ReservationDTO)
              -> Domain Models (Hotel, Reservation, ReservationBook, RoomID)
```

### Key Dependencies
- .NET 6.0-windows (WPF)
- Entity Framework Core 5.0.5
- Microsoft.EntityFrameworkCore.Sqlite 5.0.5
- Microsoft.Extensions.Hosting 5.0.0
- LoadingSpinner.WPF 1.0.0

### Navigation
Custom ContentControl + DataTemplate type mapping. NavigationStore holds current ViewModel; NavigationService<T> creates new ViewModel instances via factory delegates and sets NavigationStore.CurrentViewModel.

### Validation
- UI: INotifyDataErrorInfo on MakeReservationViewModel
- Domain: ReservationConflictException and InvalidReservationTimeRangeException from ReservationBook

### DI Registration
Host.CreateDefaultBuilder() with custom extension methods. Services registered as singletons, ViewModels as transient with factory delegates.

## Target Architecture (Uno Platform)

### Pattern
MVVM with Store pattern retained. Navigation upgraded to Uno.Extensions region-based navigation. DI via Uno.Extensions Hosting.

### Changes
- NavigationStore and NavigationService<T> removed (replaced by INavigator)
- NavigateCommand<T> removed (replaced by Navigation.Request attached property)
- LoadingSpinner.WPF replaced with ProgressRing
- BooleanToVisibilityConverter removed (implicit in Uno)
- InverseBooleanToVisibilityConverter ported or replaced with x:Bind converter
- EF Core upgraded from 5.0.5 to 10.0.5
- Window -> Page (inside Shell)

### What Stayed the Same
- Hotel, Reservation, ReservationBook, RoomID (domain models)
- ReservationDTO
- ReservationConflictException, InvalidReservationTimeRangeException
- IReservationProvider, IReservationCreator, IReservationConflictValidator
- DatabaseReservationProvider, DatabaseReservationCreator, DatabaseReservationConflictValidator
- ReservoomDbContext, IReservoomDbContextFactory
- HotelStore
- ViewModelBase, ReservationListingViewModel, MakeReservationViewModel, ReservationViewModel
- CommandBase, AsyncCommandBase, LoadReservationsCommand, MakeReservationCommand

## Layer Mapping

| Layer | WPF Original | Uno Migration | Changes |
|-------|-------------|---------------|---------|
| UI | UserControl views, MainWindow | Pages, Shell | Rewritten for WinUI + Material |
| ViewModel | Custom MVVM base | Same base classes | Namespace updates only |
| Commands | Custom ICommand base | Same base classes | Namespace updates only |
| Stores | HotelStore + NavigationStore | HotelStore only | NavigationStore removed |
| Services | DB providers/creators | Same | Zero changes |
| Data Access | EF Core 5 + SQLite | EF Core 10 + SQLite | Package version upgrade |
| Platform | WPF (net6.0-windows) | Uno (net10.0-desktop/wasm) | Cross-platform |

## Dependencies

### Retained (cross-platform)
- Microsoft.EntityFrameworkCore.Sqlite (upgraded 5.0.5 -> 10.0.5)

### Replaced
- LoadingSpinner.WPF -> ProgressRing (built-in)
- Microsoft.Extensions.Hosting -> Uno.Extensions.Hosting (implicit via UnoFeatures)

### Removed
- Microsoft.EntityFrameworkCore.Tools (dev-only, not needed at runtime)

### Added
- Uno.Sdk 6.5.31 (with Material, Toolkit, Navigation, MVUX, etc.)
