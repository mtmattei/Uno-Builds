# UnoWallet Development Chat History

## Project Overview
Building a banking/wallet sample app using Uno Platform to showcase the platform's capabilities. The app has three main screens:
1. **Dashboard (Home)** - Shows balance, card limits, and recent transactions
2. **Analytics (Progress)** - Shows spending chart with LiveCharts2, stats, and installment tabs
3. **Card** - Shows credit card visual, quick actions, and upcoming payments

## Key Requirements
- Minimal implementation - Static mock data, no backend, light theme only
- Pixel-perfect UI matching reference screenshots
- Gray background (#F2F2F7) with white card containers
- Uno.Navigation for page navigation via bottom tab bar
- LiveCharts2 for the analytics chart with gradient fill
- Working navigation between all 3 pages

---

## Session Summary

### 1. Analytics Page Implementation
- Created mock data models (`AnalyticsMockData`, `ChartDataPoint`, `Installment`)
- Created product SVGs (ps5.svg, nikon.svg, laptop.svg)
- Created `AnalyticsViewModel.cs` with tab switching logic
- Created `AnalyticsPage.xaml` with full UI
- Created converters for tab indicator styling
- Updated `App.xaml.cs` with navigation routes

### 2. Gray Background with White Cards
Updated both `DashboardPage.xaml` and `AnalyticsPage.xaml` to use:
- Page background: `#F2F2F7`
- White card containers with `CornerRadius="16"` and `Padding="20"`

### 3. LiveCharts2 Integration
- Added `LiveChartsCore.SkiaSharpView.Uno.WinUI` package
- Used Central Package Management (Directory.Packages.props)
- Fixed XAML namespace from `using:LiveChartsCore.SkiaSharpView.Uno.WinUI` to `using:LiveChartsCore.SkiaSharpView.WinUI`
- Updated `AnalyticsViewModel.cs` with `ISeries[]`, `Axis[]` for X and Y axes
- Added gradient fill under the line chart
- Added "Powered by LiveCharts2" label

### 4. Card Page Implementation
- Added `CardMockData` to `MockData.cs`
- Created `CardViewModel.cs` with amount visibility toggle and freeze card toggle
- Created `CardPage.xaml` with:
  - Credit card visual (dark background, Mastercard logo, stitching dots)
  - Quick action buttons (Card Details, Freeze Card, More)
  - Payment Next list

### 5. Navigation Setup
Updated all page tab bars to navigate between pages using `uen:Navigation.Request`:
- Dashboard: Home (selected), Card -> Card, Progress -> Analytics
- Card: Home -> Dashboard, Card (selected), Progress -> Analytics
- Analytics: Home -> Dashboard, Card -> Card, Progress (selected)

### 6. Analytics Page Layout Fix
Per reference design:
- Total Spending + Chart sits directly on gray background (no white container)
- On Progress, Overdue, Total - each in its own separate white rounded container
- Installments section in white container

### 7. Installment Card Design
Updated installment items to have:
- White top section with rounded top corners - product info
- Gray bottom section (`#F2F2F7`) with rounded bottom corners - installment progress and "Pay Now" button
- This creates a "pulled out" effect matching the reference design

### 8. Bottom Navigation Styling
- Selected tab: Dark gray pill (`#3C3C3C`) with white icon and text
- Unselected tabs: Gray icons (`#8E8E93`) on transparent background
- Tab bar background: Same gray as page background (`#F2F2F7`)

### 9. Button Hover Effect
Created global button style in App.xaml:
- Default CornerRadius: 12 (always rounded)
- Hover: Subtle gray outline (`#60606060` - 37% opacity)
- Pressed: Slightly darker outline with very subtle background tint
- Border thickness: 1px

---

## Technical Stack
- **Uno Platform** with .NET 9 (net9.0-desktop, net9.0-android)
- **MVVM pattern** with CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`)
- **Uno.Toolkit.UI** components (`SafeArea`, `TabBar`)
- **Uno.Extensions.Navigation** for page routing
- **LiveChartsCore.SkiaSharpView.Uno.WinUI** for interactive charts
- **Central Package Management** (Directory.Packages.props)
- **Uno.Resizetizer** for SVG to PNG conversion

---

## Key Files Modified/Created

### Models
- `Models/MockData.cs` - Contains all static mock data for three pages

### ViewModels
- `Presentation/DashboardViewModel.cs`
- `Presentation/AnalyticsViewModel.cs` - LiveCharts2 config, tab switching
- `Presentation/CardViewModel.cs` - Visibility toggle, freeze card toggle

### Pages
- `Presentation/DashboardPage.xaml` / `.xaml.cs`
- `Presentation/AnalyticsPage.xaml` / `.xaml.cs`
- `Presentation/CardPage.xaml` / `.xaml.cs`

### Converters
- `Converters/BoolToTabIndicatorConverter.cs` - Multiple converters for tab styling

### Configuration
- `App.xaml` - Global button style with hover effect
- `App.xaml.cs` - Navigation route registration
- `Directory.Packages.props` - LiveCharts2 version
- `UnoWallet.csproj` - LiveCharts2 reference

### Assets
- `Assets/Images/ps5.svg`
- `Assets/Images/nikon.svg`
- `Assets/Images/laptop.svg`

---

## Errors Encountered and Fixed

1. **OpacityMask not supported**: Removed OpacityMask usage, used simpler layered Border approach

2. **Central Package Management error**:
   - Error: `NU1008: The following PackageReference items cannot define a value for Version`
   - Fix: Added `<PackageVersion>` to `Directory.Packages.props`, removed version from csproj

3. **LiveCharts2 XAML namespace error**:
   - Error: `UXAML0001: The type {using:LiveChartsCore.SkiaSharpView.Uno.WinUI}CartesianChart could not be found`
   - Fix: Changed to `xmlns:lvc="using:LiveChartsCore.SkiaSharpView.WinUI"`

4. **Process lock during build**:
   - Error: `MSB3027: Could not copy apphost.exe - file is locked`
   - Fix: Killed running process with `taskkill /f /im UnoWallet.exe`

---

## Color Palette Used
- Page background: `#F2F2F7`
- White cards: `White` with `CornerRadius="16"`
- Primary text: `#1C1C1E`
- Secondary text: `#8E8E93`
- Accent/Chart color: `#4DD0E1` (cyan)
- Selected tab: `#3C3C3C`
- Dividers: `#E5E5EA`
- Error/Badge: `#FF3B30`
- Pay Now button: `#00BCD4`

---

## Navigation Routes (App.xaml.cs)
```csharp
routes.Register(
    new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
        Nested:
        [
            new("Dashboard", View: views.FindByViewModel<DashboardViewModel>(), IsDefault: true),
            new("Analytics", View: views.FindByViewModel<AnalyticsViewModel>()),
            new("Card", View: views.FindByViewModel<CardViewModel>()),
        ]
    )
);
```

---

## Build Commands
```bash
# Build for desktop
dotnet build UnoWallet.csproj -f net9.0-desktop --configuration Debug

# Run the app
start "" "bin\Debug\net9.0-desktop\UnoWallet.exe"

# Kill running app
taskkill /f /im UnoWallet.exe
```
