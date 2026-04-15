# Migration Plan: Reservoom → Uno Platform

**Source:** [github.com/SingletonSean/reservoom](https://github.com/SingletonSean/reservoom)
**Author:** SingletonSean (popular .NET YouTube channel)
**Complexity:** MEDIUM
**Estimated effort:** 2-3 days
**Target:** Uno Platform Single Project (Uno.Sdk latest stable)

---

## 1. Project Overview

A hotel reservation system demonstrating WPF MVVM fundamentals. Features include making reservations, listing existing reservations, conflict detection, and data persistence with Entity Framework. Clean layered architecture with ViewModels, Commands, Stores (state management), Services, and DTOs.

**Why this matters for the campaign:** This is the quintessential LOB app pattern — the same architecture that powers thousands of enterprise WPF applications. Created by a popular .NET educator, so the migration story reaches a large audience already familiar with the codebase.

---

## 2. Architecture and Dependency Review

### Current Architecture
- **Pattern:** MVVM with Store pattern (centralized state)
- **Framework:** .NET / WPF (verify specific version)
- **Data Access:** Entity Framework Core
- **State Management:** Store pattern (HotelStore, ReservationStore)
- **DI:** Microsoft.Extensions.DependencyInjection
- **Navigation:** Custom ViewModel-based navigation

### Architecture Layers
```
┌─────────────────────────────────────────┐
│           Views (XAML)                  │
│  ReservationListingView, MakeReservation│
├─────────────────────────────────────────┤
│         ViewModels                      │
│  ReservationListingVM, MakeReservationVM│
│  MainVM, NavigationVM                   │
├─────────────────────────────────────────┤
│         Commands                        │
│  MakeReservationCommand, NavigateCommand│
│  LoadReservationsCommand                │
├─────────────────────────────────────────┤
│         Stores (State)                  │
│  HotelStore, NavigationStore            │
├─────────────────────────────────────────┤
│         Services                        │
│  ReservationProviders, Creators         │
├─────────────────────────────────────────┤
│         DTOs / Entities                 │
│  ReservationDTO, Room, Reservation      │
├─────────────────────────────────────────┤
│     Entity Framework Core (SQLite?)     │
└─────────────────────────────────────────┘
```

### Key Dependencies

| Dependency | Purpose | Uno Equivalent | Migration Impact |
|------------|---------|---------------|-----------------|
| EF Core | ORM / Data persistence | EF Core (keep) | ZERO — same library |
| Microsoft.Extensions.DI | Dependency injection | Same (or Uno.Extensions hosting) | ZERO-LOW |
| SQLite (likely) | Database | SQLite (keep) | ZERO |
| Custom MVVM framework | Base classes | Keep or upgrade to CommunityToolkit.Mvvm | LOW |

**Key advantage:** If this app already uses EF Core + SQLite + DI, the data layer migrates with zero changes. This is the best possible scenario for a LOB migration.

---

## 3. UI and Control Inventory

### Views (estimated)

| View | Controls Used | Migration Effort |
|------|-------------|-----------------|
| ReservationListingView | ListView/ListBox, DataTemplate | LOW — direct equivalent |
| MakeReservationView | TextBox, DatePicker, Button, ComboBox | ZERO — identical controls |
| MainWindow | ContentControl (navigation host) | LOW — replace with Frame/ContentControl |
| Navigation bar | Buttons/Menu | LOW — NavigationView |

### Control Mapping

| WPF Control | Usage | Uno/WinUI Equivalent | Effort |
|-------------|-------|---------------------|--------|
| Window | App shell | Page + Shell | LOW |
| ContentControl | Navigation host | Frame or ContentControl | LOW |
| ListView | Reservation list | ListView | ZERO |
| DataTemplate | List item layout | DataTemplate | ZERO |
| TextBox | Form fields | TextBox | ZERO |
| DatePicker | Date selection | CalendarDatePicker | MINOR — different name |
| Button | Actions | Button | ZERO |
| ComboBox | Room selection | ComboBox | ZERO |
| StackPanel/Grid | Layout | StackPanel/Grid | ZERO |
| TextBlock | Labels | TextBlock | ZERO |

### Binding Analysis
- Uses `{Binding}` throughout → keep or upgrade to `x:Bind`
- DataContext set via DI/ViewModel → continue working
- Commands bound via ICommand → continue working
- Collection binding (ObservableCollection) → continue working

---

## 4. Platform/API Usage Review

| API/Feature | Used? | Uno Support | Action Required |
|-------------|-------|-------------|----------------|
| INotifyPropertyChanged | Yes | Full | None |
| ICommand (custom) | Yes | Full | None (or upgrade to RelayCommand) |
| ObservableCollection | Yes | Full | None |
| EF Core DbContext | Yes | Full | None |
| MS.Extensions.DI | Yes | Full | None |
| ContentControl (navigation) | Yes | Full | Minor wiring changes |
| MessageBox | Possible | ContentDialog | API change |
| DataTemplate | Yes | Full | None |
| DatePicker | Yes | CalendarDatePicker | Name change |
| Window.DataContext | Yes | Page.DataContext | Same pattern |

---

## 5. Likely Blockers

| Blocker | Severity | Mitigation |
|---------|----------|------------|
| Custom navigation system | LOW | Port directly or upgrade to Uno.Extensions.Navigation |
| DatePicker → CalendarDatePicker | LOW | Direct replacement |
| Window → Page | LOW | Structural change, well-documented |
| MessageBox (if used) | LOW | ContentDialog replacement |
| Form validation UI | LOW-MEDIUM | Port existing or use CommunityToolkit validation |

**This is the cleanest migration scenario.** If the app already uses EF Core + SQLite + DI + MVVM, the primary migration work is limited to XAML namespace changes and control replacements.

---

## 6. Migration Strategy

**Approach:** Fresh Uno project + copy layered code bottom-up

The clean layered architecture makes this straightforward:
1. Create Uno project
2. Copy Entities/DTOs → compile (zero changes expected)
3. Copy EF Context → compile (zero changes expected)
4. Copy Services/Stores → compile (zero changes expected)
5. Copy ViewModels → compile (minimal changes)
6. Port XAML views → the only real work

**This sequence proves the "your business logic migrates unchanged" thesis.**

---

## 7. Phased Implementation

### Phase 0: Starter Kit Setup (5 min)
1. Copy migration starter kit template into project root
2. Customize `CLAUDE.md` with Reservoom details (MVVM, EF Core, Store pattern, DI)
3. Copy this migration plan into `docs/MIGRATION-PLAN.md`
4. Initialize `docs/MIGRATION-LOG.md`
5. Fill in `docs/ARCHITECTURE.md` Original Architecture section from source analysis
6. Verify `.mcp.json` and `.claude/settings.json` are in place

### Phase 1: Project Setup (30 min)
1. `dotnet new unoapp -o ReservoomUno --preset=recommended`
2. Add NuGet packages:
   - `Microsoft.EntityFrameworkCore.Sqlite`
   - `CommunityToolkit.Mvvm` (optional upgrade)
3. Set up `global.json`
4. Verify build

### Phase 2: Domain and Data Layer (30 min)
1. Copy entity classes (Reservation, Room) → build (zero changes)
2. Copy DTOs → build (zero changes)
3. Copy DbContext → build (zero changes expected)
4. Copy migration files or regenerate
5. Verify database creation and seeding
6. **Document: "Lines of code changed in business layer: 0"**

### Phase 3: Services and Stores (30 min)
1. Copy service interfaces and implementations
2. Copy Store classes (HotelStore, ReservationStore, NavigationStore)
3. Build — should compile unchanged
4. **Document: "Lines of code changed in service layer: 0"**

### Phase 4: ViewModels (1 hour)
1. Copy ViewModel classes
2. Update namespace references (System.Windows → Microsoft.UI.Xaml where needed)
3. Replace any MessageBox.Show() with async ContentDialog
4. Update DI registration in App.xaml.cs
5. Build and verify

### Phase 5: XAML Views (2-3 hours)
1. Port MainWindow → MainPage + Shell structure
2. Port ReservationListingView:
   - ListView with DataTemplate
   - Collection binding
   - Command bindings
3. Port MakeReservationView:
   - Form layout (TextBox, DatePicker, ComboBox)
   - Validation display
   - Submit command
4. Port navigation system:
   - Option A: Keep custom ContentControl navigation
   - Option B: Upgrade to Uno.Extensions region-based navigation
5. Apply Uno Material theme

### Phase 6: End-to-End Testing (1-2 hours)
1. Test: Create new reservation
2. Test: View reservation list
3. Test: Conflict detection (overlapping dates)
4. Test: Navigation flow
5. Test: Data persists across restart
6. Build and test on Windows, WASM, Desktop

### Phase 7: MVUX Upgrade (Optional, 2-3 hours)
1. Demonstrate converting one ViewModel to MVUX model
2. Replace INotifyPropertyChanged with IFeed/IState
3. Show the "MVVM → MVUX" upgrade path
4. Compare before/after code complexity

---

## 8. Expected Effort/Risk Areas

| Area | Effort | Risk | Notes |
|------|--------|------|-------|
| Project setup | 30 min | ZERO | Standard scaffolding |
| Domain/data layer | 30 min | ZERO | Copy unchanged |
| Services/stores | 30 min | ZERO | Copy unchanged |
| ViewModels | 1 hr | LOW | Mostly namespace changes |
| XAML views | 2-3 hrs | LOW-MEDIUM | Control mapping + navigation |
| Material theming | 30 min | LOW | Built-in |
| E2E testing | 1-2 hrs | LOW | Functional verification |
| MVUX upgrade (opt.) | 2-3 hrs | LOW | Enhancement, not migration |
| **Total** | **6-10 hrs** | **LOW** | |

---

## 9. Key Migration Patterns This Will Demonstrate

1. **"Zero-change business layer"** — entities, services, and stores compile unchanged
2. **EF Core portability** — same DbContext works on all platforms
3. **DI continuity** — Microsoft.Extensions.DI works identically in Uno
4. **Store pattern portability** — centralized state management survives migration
5. **Custom navigation → Region-based navigation** — upgrade path
6. **DatePicker → CalendarDatePicker** — control name mapping
7. **MVVM → MVUX upgrade** — optional modernization during migration
8. **ListView + DataTemplate** — collection UI portability

---

## 10. Campaign Deliverables from This Migration

1. **"Your Business Logic Migrates Unchanged"** — the hero stat (0 lines changed in services)
2. **LOB migration playbook** — step-by-step for any MVVM + EF Core app
3. **MVVM → MVUX comparison** — optional upgrade story
4. **DI migration guide** — proving Microsoft.Extensions.DI continuity
5. **Video tie-in** — SingletonSean's audience already knows this codebase
6. **"Lines Changed" infographic** — visual proof of migration surface area
