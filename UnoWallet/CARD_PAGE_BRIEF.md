# Card Page Implementation Brief

## UI Reference Analysis

Based on `screen-3.png`, the Card page displays a credit/debit card with actions and upcoming payments.

### Page Structure (Top to Bottom)

1. **Header Section** (on gray background)
   - Avatar (left) - same as Dashboard
   - Support button (circular, white background)
   - Notification bell with red badge "4"

2. **Card Display Section**
   - Dark card container with rounded corners
   - "Master Card" label with Mastercard logo (red/orange circles)
   - Card number: "828749-2847-03"
   - Stitched wallet/pocket visual effect (darker overlay at top)
   - "Limit Card" label
   - Amount: "$43,093.00" with chart/expand icon

3. **Quick Actions Row** (3 buttons)
   - "Card Details" - card icon
   - "Freeze Card" - snowflake icon
   - "More" - three dots icon
   - Each button has icon above text, white background, rounded corners

4. **Payment Next Section** (white card)
   - Section title: "Payment Next"
   - List of upcoming installment payments:
     - PS5 / Amazon.com / $836.94 / Due date 18 / 1 of 4 Installment / Pay Now
     - Nikon Camera / Amazon.com / $563.04 / Due date 18 / 3 of 4 Installment / Pay Now
     - (Partially visible) / Apple.com / $...46.94 / Due date 18 / Pay Now

5. **Bottom Tab Bar**
   - Home (selected with pill) | Card | Wallet | Progress | Profile
   - "Home" shows as selected with dark pill background

---

## Uno Platform Implementation Approach

### Pattern: Consistent with Existing Pages

Following the same patterns established in Dashboard and Analytics pages:
- Static mock data
- MVVM with CommunityToolkit.Mvvm
- Uno.Toolkit.UI components
- Gray background (#F2F2F7) with white card containers
- Uno.Navigation for tab bar

---

## Recommended Uno Platform Components

### 1. Card Display

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Card container | `Border` | Dark background (#2C2C2E), rounded corners (16px) |
| Mastercard logo | `StackPanel` with `Ellipse` | Two overlapping circles (red + orange) |
| Card number | `TextBlock` | Light gray text |
| Stitched effect | `Border` with darker overlay | Nested border at top with darker background |
| Limit amount | `TextBlock` | Large, bold, white text |

### 2. Quick Actions

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Container | `Grid` with 3 columns | Equal width distribution |
| Action button | `Button` with `StackPanel` | Icon + text vertically stacked |
| Icons | `FontIcon` | Segoe MDL2 Assets glyphs |
| Button style | White background, subtle border | CornerRadius="12" |

### 3. Payment Next List

Reuse the same `Installment` model and item template from Analytics page with minor styling adjustments.

---

## Data Models

Reuse existing models from `MockData.cs`:
- `Installment` record (already exists)
- Add card-specific data to `MockData.cs`

```csharp
public static class CardMockData
{
    public static string CardType => "Master Card";
    public static string CardNumber => "828749-2847-03";
    public static decimal CardLimit => 43093.00m;

    public static IReadOnlyList<Installment> UpcomingPayments { get; } =
    [
        new("PS5", "Amazon.com", "ms-appx:///Assets/Images/ps5.png", 836.94m, 18, 1, 4),
        new("Nikon Camera", "Amazon.com", "ms-appx:///Assets/Images/nikon.png", 563.04m, 18, 3, 4),
        new("AirPods Pro", "Apple.com", "ms-appx:///Assets/Images/airpods.png", 246.94m, 18, 2, 4),
    ];
}
```

---

## File Structure

```
UnoWallet/
├── Models/
│   └── MockData.cs              # Add CardMockData
├── Presentation/
│   ├── CardPage.xaml            # New page
│   ├── CardPage.xaml.cs
│   └── CardViewModel.cs
└── Assets/Images/
    └── airpods.svg              # New product image (if needed)
```

---

## XAML Structure

```xml
<Page Background="#F2F2F7">
    <Grid utu:SafeArea.Insets="Top,Bottom">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <ScrollViewer>
            <StackPanel Padding="16" Spacing="16">

                <!-- Header (same as Dashboard) -->

                <!-- Card Display -->
                <Border Background="#2C2C2E" CornerRadius="16" Padding="20">
                    <!-- Card content -->
                </Border>

                <!-- Quick Actions Row -->
                <Grid ColumnSpacing="12">
                    <!-- 3 action buttons -->
                </Grid>

                <!-- Payment Next Card -->
                <Border Background="White" CornerRadius="16" Padding="20">
                    <!-- Section title + ItemsRepeater -->
                </Border>

            </StackPanel>
        </ScrollViewer>

        <!-- Bottom Tab Bar -->
    </Grid>
</Page>
```

---

## Card Visual Details

### Mastercard Logo Implementation

```xml
<StackPanel Orientation="Horizontal" Spacing="-8">
    <Ellipse Width="24" Height="24" Fill="#EB001B" />
    <Ellipse Width="24" Height="24" Fill="#F79E1B" Margin="-8,0,0,0" />
</StackPanel>
```

### Stitched Wallet Effect

The card has a "wallet pocket" visual at the top - a darker semi-transparent overlay with a curved bottom edge:

```xml
<Border Background="#2C2C2E" CornerRadius="16">
    <Grid>
        <!-- Darker pocket overlay at top -->
        <Border Background="#1C1C1E"
                CornerRadius="16,16,0,0"
                Height="80"
                VerticalAlignment="Top"
                Opacity="0.7">
            <!-- Optional: stitching dots using small ellipses -->
        </Border>

        <!-- Card content -->
        <StackPanel Padding="20" Margin="0,60,0,0">
            <!-- Master Card label, number, limit -->
        </StackPanel>
    </Grid>
</Border>
```

### Stitching Effect (Optional Detail)

Small white dots in an arc pattern to simulate stitching:
```xml
<Canvas>
    <Ellipse Width="3" Height="3" Fill="#FFFFFF40" Canvas.Left="20" Canvas.Top="70"/>
    <Ellipse Width="3" Height="3" Fill="#FFFFFF40" Canvas.Left="30" Canvas.Top="72"/>
    <!-- ... more dots in arc pattern -->
</Canvas>
```

---

## Quick Action Buttons Style

```xml
<Button Background="White"
        BorderBrush="#E5E5EA"
        BorderThickness="1"
        CornerRadius="12"
        Padding="16,12"
        HorizontalAlignment="Stretch">
    <StackPanel Spacing="8" HorizontalAlignment="Center">
        <FontIcon Glyph="&#xE8C7;" FontSize="24" Foreground="#1C1C1E"/>
        <TextBlock Text="Card Details" FontSize="13" Foreground="#1C1C1E"/>
    </StackPanel>
</Button>
```

---

## Navigation Integration

Update `App.xaml.cs`:

```csharp
views.Register(
    new ViewMap(ViewModel: typeof(ShellViewModel)),
    new ViewMap<DashboardPage, DashboardViewModel>(),
    new ViewMap<AnalyticsPage, AnalyticsViewModel>(),
    new ViewMap<CardPage, CardViewModel>()  // Add this
);

routes.Register(
    new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
        Nested:
        [
            new("Dashboard", View: views.FindByViewModel<DashboardViewModel>(), IsDefault: true),
            new("Analytics", View: views.FindByViewModel<AnalyticsViewModel>()),
            new("Card", View: views.FindByViewModel<CardViewModel>()),  // Add this
        ]
    )
);
```

---

## Bottom Tab Bar Updates

All pages need consistent tab bar with navigation:
- **Dashboard**: Home selected, Card navigates to Card page
- **Card**: Card selected (with pill), Home navigates to Dashboard
- **Analytics**: Progress selected

Tab icons mapping:
| Tab | Icon Glyph | Route |
|-----|------------|-------|
| Home | &#xE80F; | Dashboard |
| Card | &#xE8C7; | Card |
| Wallet | &#xE786; | (not implemented) |
| Progress | &#xE9D2; | Analytics |
| Profile | &#xE77B; | (not implemented) |

---

## Implementation Steps

1. **Add CardMockData** to `MockData.cs`
2. **Create CardViewModel** with properties for card info and payments
3. **Create CardPage.xaml** with:
   - Header (copy from Dashboard)
   - Card display with Mastercard logo and wallet effect
   - Quick action buttons row
   - Payment Next list (similar to Analytics installments)
   - Bottom tab bar with Card selected
4. **Register navigation** in `App.xaml.cs`
5. **Update Dashboard tab bar** - Card button navigates to Card page
6. **Build and test**

---

## Color Palette Reference

| Element | Color |
|---------|-------|
| Page background | #F2F2F7 |
| Card container | White |
| Credit card background | #2C2C2E |
| Card pocket overlay | #1C1C1E (70% opacity) |
| Mastercard red | #EB001B |
| Mastercard orange | #F79E1B |
| Primary text | #1C1C1E |
| Secondary text | #8E8E93 |
| Accent (Pay Now) | #00BCD4 |
| Border/divider | #E5E5EA |
| Selected tab | #1C1C1E |

---

## Unresolved Questions

1. **Card pocket stitching**: Should we implement the stitching dots detail or keep it simple without them?

2. **Freeze Card functionality**: Should the "Freeze Card" button toggle state visually (e.g., change icon/color when frozen)?

3. **Card number masking**: Should the card number display be partially masked (e.g., "****-****-2847-03") for a more realistic look?

4. **AirPods image**: Create a new SVG for the third payment item, or reuse an existing placeholder?

5. **Wallet tab**: The reference shows "Home" selected in the tab bar, but this is the Card page. Should the Card tab be selected instead, or is the reference showing navigation from Home?
