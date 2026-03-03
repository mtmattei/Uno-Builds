# UnoWallet - Sample App Implementation Brief

## Overview

A showcase banking/wallet app demonstrating Uno Platform capabilities. Minimal implementation with mock data - focused on pixel-perfect UI reproduction.

---

## Scope

### In Scope
- Single dashboard screen matching the reference UI
- Static mock data (no backend)
- Bottom tab bar (visual only - no navigation)
- Light theme only

### Out of Scope
- Authentication
- Real API integration
- Multiple pages/navigation
- Dark mode
- Offline storage
- Push notifications

---

## UI Components Breakdown

### From Reference Screenshot

```
┌─────────────────────────────────┐
│ [Avatar]           [🎧] [🔔4]  │  <- Header
├─────────────────────────────────┤
│ Total Balance                   │
│ $248,967.83              [↔]   │  <- Balance Display
├─────────────────────────────────┤
│ ┌─────────┐ ┌─────────────────┐│
│ │Payment  │ │Payment          ││  <- Payment Cards
│ │Next     │ │Completed        ││
│ │$43,093  │ │$274,825.01      ││
│ └─────────┘ └─────────────────┘│
├─────────────────────────────────┤
│ Card Limits                     │
│ ┌─────────────────────────────┐│
│ │ [====○─────────────────────]││  <- Progress Bar
│ │ Today Limits  $614.93/$43K  ││
│ │ ─────────────────────────── ││
│ │ Set card limits          →  ││
│ └─────────────────────────────┘│
├─────────────────────────────────┤
│ Transaction                     │
│ Today ──────────────────────── │
│ [a]  Amazon.com        $89.71  │  <- Transaction Items
│      Nov 18, 2025      9:17 AM │
│ [T]  Temu.com          $30.45  │
│      Sep 13, 2025      8:49 PM │
│ Yesterday ─────────────────────│
│ [...]                          │
├─────────────────────────────────┤
│ [🍎] [💳] [Wallet] [💬] [👤]  │  <- Tab Bar
└─────────────────────────────────┘
```

---

## Uno Platform Components

| UI Element | Uno Component | Notes |
|------------|---------------|-------|
| Root layout | `Grid` + `ScrollViewer` | With `utu:SafeArea.Insets` |
| Header icons | `Button` with `FontIcon` | Standard icon buttons |
| Notification badge | `Grid` overlay or `utu:BadgeButton` | Red circle with count |
| Payment cards | `Border` or `utu:Card` | Rounded corners, light gray bg |
| Progress bar | Custom `Grid` | Solid fill + striped pattern |
| Dividers | `Border` or `utu:Divider` | 1px height |
| Transaction list | `ItemsRepeater` or `ListView` | No virtualization needed for demo |
| Tab bar | `utu:TabBar` | With custom selected style |

---

## File Structure (Minimal)

```
UnoWallet/
├── Assets/
│   ├── Images/
│   │   ├── avatar.png           <- 3D avatar image
│   │   ├── amazon-logo.png      <- Merchant logos
│   │   └── temu-logo.png
│   └── reference/
│       └── mobile-ui-1.png
├── Models/
│   └── MockData.cs              <- Static sample data
├── Presentation/
│   ├── DashboardPage.xaml       <- Main (only) page
│   ├── DashboardPage.xaml.cs
│   └── DashboardViewModel.cs
├── Styles/
│   ├── ColorPaletteOverride.xaml  <- Updated colors
│   └── WalletStyles.xaml          <- Custom styles
├── App.xaml
├── App.xaml.cs
└── GlobalUsings.cs
```

---

## Color Palette (Light Theme)

Based on the reference UI:

```xaml
<!-- Backgrounds -->
<Color x:Key="BackgroundColor">#FFFFFF</Color>
<Color x:Key="SurfaceVariantColor">#F5F7FA</Color>  <!-- Card backgrounds -->

<!-- Text -->
<Color x:Key="OnBackgroundColor">#1A1A1A</Color>    <!-- Primary text -->
<Color x:Key="OnSurfaceVariantColor">#6B7280</Color> <!-- Secondary text -->

<!-- Accent - Progress bar cyan -->
<Color x:Key="TertiaryColor">#4DD0E1</Color>
<Color x:Key="TertiaryContainerColor">#E0F7FA</Color> <!-- Striped portion -->

<!-- Error/Badge -->
<Color x:Key="ErrorColor">#FF3B30</Color>

<!-- Tab bar selected -->
<Color x:Key="SurfaceInverseColor">#1C1C1E</Color>  <!-- Black pill -->
```

---

## DashboardPage.xaml Structure

```xaml
<Page x:Class="UnoWallet.Presentation.DashboardPage"
      xmlns:utu="using:Uno.Toolkit.UI"
      Background="{ThemeResource BackgroundBrush}">

    <Grid utu:SafeArea.Insets="All">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />      <!-- Scrollable content -->
            <RowDefinition Height="Auto" />   <!-- Tab bar -->
        </Grid.RowDefinitions>

        <ScrollViewer Grid.Row="0">
            <StackPanel Padding="20,16" Spacing="24">

                <!-- 1. Header -->
                <Grid Height="48">
                    <!-- Avatar, Support btn, Notification btn -->
                </Grid>

                <!-- 2. Balance -->
                <StackPanel Spacing="4">
                    <TextBlock Text="Total Balance" />
                    <Grid>
                        <TextBlock Text="$248,967.83" FontSize="32" FontWeight="Bold" />
                        <Button HorizontalAlignment="Right"><!-- Expand icon --></Button>
                    </Grid>
                </StackPanel>

                <!-- 3. Payment Cards -->
                <Grid ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    <!-- Two cards -->
                </Grid>

                <!-- 4. Card Limits -->
                <StackPanel Spacing="12">
                    <TextBlock Text="Card Limits" />
                    <Border CornerRadius="12" BorderThickness="1" Padding="16">
                        <!-- Progress bar, limits text, set limits link -->
                    </Border>
                </StackPanel>

                <!-- 5. Transactions -->
                <StackPanel Spacing="8">
                    <TextBlock Text="Transaction" />
                    <!-- Date headers + transaction items -->
                </StackPanel>

            </StackPanel>
        </ScrollViewer>

        <!-- 6. Tab Bar -->
        <Border Grid.Row="1" Background="{ThemeResource SurfaceVariantBrush}">
            <utu:TabBar SelectedIndex="2">
                <!-- 5 tabs, Wallet selected -->
            </utu:TabBar>
        </Border>

    </Grid>
</Page>
```

---

## Mock Data

```csharp
namespace UnoWallet.Models;

public static class MockData
{
    public static decimal TotalBalance => 248967.83m;
    public static decimal NextPayment => 43093.00m;
    public static decimal CompletedPayments => 274825.01m;
    public static decimal TodaySpent => 614.93m;
    public static decimal DailyLimit => 43093.00m;
    public static int NotificationCount => 4;

    public static List<Transaction> Transactions =>
    [
        new("Amazon.com", "amazon-logo.png", 89.71m, new DateTime(2025, 11, 18, 9, 17, 0)),
        new("Temu.com", "temu-logo.png", 30.45m, new DateTime(2025, 9, 13, 20, 49, 0)),
        new("Amazon.com", "amazon-logo.png", 261.92m, new DateTime(2025, 9, 12, 8, 33, 0)),
    ];
}

public record Transaction(
    string MerchantName,
    string LogoAsset,
    decimal Amount,
    DateTime Timestamp
);
```

---

## Key Implementation Details

### 1. Progress Bar (Striped Pattern)

The spending limit progress bar has:
- Solid cyan fill on left (spent portion)
- White circle indicator at current position
- Diagonal striped pattern on right (remaining)

```xaml
<Grid Height="8" CornerRadius="4">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="0.014*" />  <!-- ~1.4% spent -->
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <!-- Spent portion -->
    <Border Grid.Column="0" Background="#4DD0E1" CornerRadius="4,0,0,4">
        <Ellipse Width="12" Height="12" Fill="White"
                 HorizontalAlignment="Right" Margin="0,0,-6,0" />
    </Border>

    <!-- Remaining (striped) - use image brush or gradient -->
    <Border Grid.Column="1" CornerRadius="0,4,4,0"
            Background="{StaticResource StripedBrush}" />
</Grid>
```

### 2. Tab Bar Selected State (Pill Style)

The selected "Wallet" tab has a black pill background with white icon/text:

```xaml
<utu:TabBar SelectedIndex="2">
    <utu:TabBarItem>
        <utu:TabBarItem.Icon>
            <PathIcon Data="..." />  <!-- Apple logo -->
        </utu:TabBarItem.Icon>
    </utu:TabBarItem>
    <utu:TabBarItem>
        <utu:TabBarItem.Icon>
            <FontIcon Glyph="&#xE8C7;" />  <!-- Card -->
        </utu:TabBarItem.Icon>
    </utu:TabBarItem>
    <utu:TabBarItem Content="Wallet">
        <utu:TabBarItem.Icon>
            <FontIcon Glyph="&#xE786;" />  <!-- Wallet -->
        </utu:TabBarItem.Icon>
    </utu:TabBarItem>
    <!-- ... -->
</utu:TabBar>
```

Style the selected state with a black pill (`CornerRadius="16"`, black background, white foreground).

### 3. Transaction Date Grouping

Simple visual grouping with inline headers:

```xaml
<!-- Today header -->
<Grid Margin="0,8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <TextBlock Text="Today" Foreground="#9CA3AF" Margin="0,0,12,0" />
    <Border Grid.Column="1" Height="1" Background="#E5E7EB" VerticalAlignment="Center" />
</Grid>

<!-- Transaction item -->
<Grid Padding="0,12" ColumnSpacing="12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="44" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    <!-- Logo, merchant info, amount -->
</Grid>
```

---

## Implementation Steps

1. **Update ColorPaletteOverride.xaml** - Match reference colors
2. **Add asset images** - Avatar, merchant logos
3. **Create MockData.cs** - Static sample data
4. **Build DashboardPage.xaml** - All UI in single page
5. **Style the TabBar** - Custom pill selection style
6. **Test on Android/Desktop** - Verify pixel-perfect rendering

---

## Assets Needed

| Asset | Size | Notes |
|-------|------|-------|
| avatar.png | 96x96 | 3D cartoon female avatar |
| amazon-logo.png | 88x88 | Amazon "a" logo (circular) |
| temu-logo.png | 88x88 | Temu logo (circular) |
| headset-icon | - | FontIcon or SVG |
| bell-icon | - | FontIcon or SVG |
| expand-icon | - | Four arrows pointing outward |
| arrow-right | - | Chevron for "Set card limits" |

---

## Unresolved Questions

1. **Avatar image** - Do you have the 3D avatar asset, or should we use a placeholder?

2. **Merchant logos** - Use actual brand logos or generic placeholders?

3. **Tab bar icons** - The first icon appears to be an Apple logo - is this intentional or a placeholder?

4. **Striped pattern** - Implement as image asset or programmatic gradient?

5. **Font** - Use SF Pro (iOS native) or stick with Roboto (Material default)?
