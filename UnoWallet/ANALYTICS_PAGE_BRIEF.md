# Analytics Page Implementation Brief

## UI Reference Analysis

Based on `ui-screen-2.png`, the Analytics page contains:

### Page Structure (Top to Bottom)

1. **Header Section**
   - Back arrow button (left)
   - "Analytics" title (center-left)
   - Three-dot menu button (right)

2. **Total Spending Section**
   - Label: "Total Spending"
   - Large dollar amount: "$248,967.83"

3. **Line Chart Section**
   - Interactive line chart with cyan/teal colored line
   - Data points shown as dots on the line
   - Tooltip/callout showing "$4,274.00" with "Nov 25, 2025" date
   - X-axis labels: "Nov 1, 2025" (left) and "Nov 30, 2025" (right)
   - Light gray background grid area

4. **Summary Stats Row (3 columns)**
   - "On Progress": "$61,523.00"
   - "Overdue": "$4,825.43"
   - "Total": "$89,271.92"

5. **Installment Tabs**
   - Two tabs: "4 Installment" (selected) | "6 Installment"
   - Underline indicator on selected tab

6. **Installment List Items**
   Each item contains:
   - Product image (circular)
   - Product name and merchant
   - Price and due date
   - Progress indicator (e.g., "1 of 4 Installment")
   - "Pay Now" action link (cyan/teal)

   Visible items:
   - PS5 / Amazon.com / $836.94 / Due date 18 / 1 of 4 Installment
   - Nikon Camera / Amazon.com / $563.04 / Due date 18 / 1 of 4 Installment
   - Gaming Laptop / $1,746.94 (partially visible)

7. **Bottom Tab Bar**
   - Same as Dashboard: Home, Card, Progress (selected), Messages, Profile
   - "Progress" tab shows pill-style selection with icon + text

---

## Uno Platform Implementation Approach

### Pattern: Minimal Sample App

Following the same minimal approach as the Dashboard page:
- Static mock data only
- No backend/API integration
- Single page focus
- Light theme only

---

## Recommended Uno Platform Components

### 1. Layout Components

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Page container | `Page` with `Grid` | Same pattern as Dashboard |
| Safe area | `utu:SafeArea.Insets` | Handle notch/status bar |
| Scrollable content | `ScrollViewer` | Vertical scroll for content |
| Sections | `StackPanel` | Vertical stacking with spacing |

### 2. Header Section

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Back button | `Button` with `FontIcon` (&#xE72B;) | ChevronLeft glyph |
| Title | `TextBlock` | "Analytics" |
| Menu button | `Button` with `FontIcon` (&#xE712;) | MoreHorizontal glyph |

### 3. Line Chart

**Option A: Custom Path-based Chart (Recommended for sample)**
- Use `Path` with `PathGeometry` to draw the line
- `Ellipse` elements for data points
- `Border` for tooltip callout
- Pros: No external dependencies, full control
- Cons: More XAML code

**Option B: Uno.Extensions.Charts (if available)**
- Check Uno toolkit for chart controls
- May require additional NuGet package

**Recommendation**: Use **Option A** with `Polyline` for simplicity:
```xaml
<Canvas>
    <Polyline Points="..." Stroke="#4DD0E1" StrokeThickness="2"/>
    <Ellipse ... /> <!-- Data points -->
</Canvas>
```

### 4. Summary Stats Row

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Container | `Grid` with 3 columns | Equal width |
| Stat card | `StackPanel` | Centered text |
| Labels | `TextBlock` | Gray color |
| Values | `TextBlock` | Bold, black |

### 5. Installment Tabs

**Recommended**: Custom tab implementation using `Grid` + `Border`

```xaml
<Grid ColumnSpacing="0">
    <Border x:Name="Tab1" BorderThickness="0,0,0,2" BorderBrush="Black">
        <TextBlock Text="4 Installment" />
    </Border>
    <Border x:Name="Tab2" BorderThickness="0,0,0,1" BorderBrush="Gray">
        <TextBlock Text="6 Installment" />
    </Border>
</Grid>
```

Alternative: `utu:TabBar` from Uno.Toolkit.UI
- More semantic but may need customization

### 6. Installment List Items

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| List container | `ItemsRepeater` | Same as transactions in Dashboard |
| Item template | `DataTemplate` | Custom layout |
| Product image | `Border` with `ImageBrush` | Circular (CornerRadius) |
| Product info | `StackPanel` | Name, merchant |
| Price/date | `StackPanel` | Right-aligned |
| Progress text | `TextBlock` | "1 of 4 Installment" |
| Pay Now | `Button` or `HyperlinkButton` | Cyan text |

### 7. Bottom Tab Bar

Reuse the same bottom tab bar from Dashboard with "Progress" selected instead of "Wallet".

---

## File Structure

```
UnoWallet/
├── Models/
│   └── MockData.cs          # Add AnalyticsData, Installment records
├── Presentation/
│   ├── AnalyticsPage.xaml   # New page
│   ├── AnalyticsPage.xaml.cs
│   └── AnalyticsViewModel.cs
└── Assets/Images/
    ├── ps5.svg              # New product images
    ├── nikon.svg
    └── laptop.svg
```

---

## Data Models to Add

```csharp
// Add to MockData.cs

public static class AnalyticsMockData
{
    public static decimal TotalSpending => 248967.83m;
    public static decimal OnProgress => 61523.00m;
    public static decimal Overdue => 4825.43m;
    public static decimal TotalInstallments => 89271.92m;

    public static IReadOnlyList<ChartDataPoint> ChartData { get; } = [...];
    public static IReadOnlyList<Installment> FourInstallments { get; } = [...];
    public static IReadOnlyList<Installment> SixInstallments { get; } = [...];
}

public record ChartDataPoint(DateTime Date, decimal Amount);

public record Installment(
    string ProductName,
    string MerchantName,
    string ImageSource,
    decimal Price,
    int DueDate,
    int CurrentInstallment,
    int TotalInstallments
);
```

---

## Chart Implementation Details

### Simple Polyline Approach

The chart in the UI shows a line graph with approximately 7-8 data points across November 2025.

**Chart container structure:**
```xaml
<Grid Height="180" Margin="0,16">
    <!-- Y-axis area (optional) -->
    <!-- Chart area -->
    <Canvas>
        <!-- Grid lines (optional, light gray) -->
        <!-- Line path -->
        <Polyline Points="0,140 40,130 80,120 140,100 180,60 220,80 280,40"
                  Stroke="#4DD0E1"
                  StrokeThickness="2"
                  StrokeLineJoin="Round"/>

        <!-- Data point dots -->
        <Ellipse Canvas.Left="178" Canvas.Top="58"
                 Width="10" Height="10" Fill="#4DD0E1"/>

        <!-- Tooltip callout -->
        <Border Canvas.Left="150" Canvas.Top="20"
                Background="White" BorderBrush="#E5E5E5"
                BorderThickness="1" CornerRadius="8" Padding="8">
            <StackPanel>
                <TextBlock Text="$4,274.00" FontWeight="SemiBold"/>
                <TextBlock Text="Nov 25, 2025" FontSize="12" Foreground="Gray"/>
            </StackPanel>
        </Border>
    </Canvas>

    <!-- X-axis labels -->
    <Grid VerticalAlignment="Bottom" Margin="0,8,0,0">
        <TextBlock Text="Nov 1, 2025" HorizontalAlignment="Left"/>
        <TextBlock Text="Nov 30, 2025" HorizontalAlignment="Right"/>
    </Grid>
</Grid>
```

**Note**: For a sample app, hardcoded points are acceptable. The Polyline points would be calculated based on the chart area dimensions.

---

## Navigation Integration

Update `App.xaml.cs` to register the Analytics page:

```csharp
views.Register(
    new ViewMap(ViewModel: typeof(ShellViewModel)),
    new ViewMap<DashboardPage, DashboardViewModel>(),
    new ViewMap<AnalyticsPage, AnalyticsViewModel>()  // Add this
);

routes.Register(
    new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
        Nested:
        [
            new("Dashboard", View: views.FindByViewModel<DashboardViewModel>(), IsDefault: true),
            new("Analytics", View: views.FindByViewModel<AnalyticsViewModel>()),  // Add this
        ]
    )
);
```

---

## Implementation Steps

1. **Create mock data models** - Add Installment and ChartDataPoint records
2. **Add product images** - Create placeholder SVGs for PS5, Nikon, Laptop
3. **Create AnalyticsViewModel** - Expose mock data properties
4. **Create AnalyticsPage.xaml** - Build UI top to bottom:
   - Header with back button
   - Total spending display
   - Line chart (Polyline-based)
   - Summary stats row
   - Installment tabs
   - Installment list
   - Bottom tab bar (reuse from Dashboard)
5. **Register navigation** - Update App.xaml.cs
6. **Build and test** - Verify on desktop and Android

---

## Styling Consistency

Reuse existing theme resources from `ColorPaletteOverride.xaml`:
- Primary accent: `#00BCD4` / `#4DD0E1` (cyan/teal)
- Background: `{ThemeResource BackgroundBrush}` - White
- Text primary: `{ThemeResource OnBackgroundBrush}` - Dark
- Text secondary: `{ThemeResource OnSurfaceVariantBrush}` - Gray
- Card background: `{ThemeResource SurfaceVariantBrush}` - Light gray
- Borders: `{ThemeResource OutlineBrush}` - Light gray

---

## Unresolved Questions

1. **Chart interactivity**: Should the tooltip be static (hardcoded) or should it respond to tap/hover on data points? For a sample app, static is simpler.

2. **Tab switching**: Should clicking between "4 Installment" and "6 Installment" actually switch the list content, or is static display sufficient?

3. **Navigation**: How should the user navigate from Dashboard to Analytics? Options:
   - Tap on one of the payment cards
   - Add a dedicated analytics button
   - Tab bar "Progress" button

4. **Back button behavior**: Should the back button navigate to Dashboard, or should we rely on system back navigation?

5. **Product images**: Use placeholder SVGs (like the merchant logos) or actual product images?
