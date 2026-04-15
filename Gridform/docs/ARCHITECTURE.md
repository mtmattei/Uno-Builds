# GRIDFORM — Architecture Brief
## Uno Platform Implementation Specification
### Industrial Tooling Distribution & Warehouse Spatial Planning

---


## 1.1 Solution Architecture

### Target Stack
Uno Platform 6.x, .NET 9, C# 13. Primary targets: Windows (WinAppSDK), WebAssembly (Skia). Secondary: macOS (Skia), Linux (Skia).

### UnoFeatures (csproj)

```xml
<UnoFeatures>
  Material;
  Toolkit;
  Extensions;
  ExtensionsCore;
  Hosting;
  MVUX;
  Navigation;
  ThemeService;
  Skia;
  SkiaRenderer;
  Logging;
  Configuration;
  Serialization;
</UnoFeatures>
```

Additional NuGet packages:
- `SkiaSharp.Views.Uno.WinUI` — for `SKXamlCanvas` custom drawing (isometric warehouse renderer)
- `CommunityToolkit.Mvvm` — for supplementary `[ObservableProperty]` on non-MVUX view models if needed

### Project Structure

```
GridForm.sln
│
├── GridForm/                              # Shared application project
│   ├── App.xaml / App.xaml.cs             # App startup, DI registration, theme init
│   │
│   ├── Themes/                            # Resource dictionaries
│   │   ├── ColorPaletteOverride.xaml      # Material color overrides (warm industrial)
│   │   ├── TextBlockStyles.xaml           # Typography scale overrides
│   │   ├── ButtonStyles.xaml              # Custom button variants (toolbar, ghost, accent)
│   │   ├── BadgeStyles.xaml               # StatusBadge, AIChip, RiskBadge, SLABadge
│   │   └── DataTableStyles.xaml           # Row templates, header styles, hover states
│   │
│   ├── Models/                            # Immutable data records
│   │   ├── PurchaseOrder.cs               # PO entity with line items, chain, history
│   │   ├── LineItem.cs                    # SKU, description, qty, unit, total
│   │   ├── ApprovalStep.cs               # who, role, status, timestamp
│   │   ├── AuditEntry.cs                  # timestamp, message
│   │   ├── VoxelKey.cs                    # (x, y, z) record struct
│   │   ├── AssetType.cs                   # Enum: Pallet, Rack, Container, Equipment, Aisle
│   │   ├── ZoneType.cs                    # Enum: Receiving, Storage, Staging, Shipping
│   │   ├── WarehouseState.cs              # Grid data, zone data, cursor, tool, mode, layer
│   │   ├── WarehouseMetrics.cs            # Floor%, volume%, tonnage, peak, counts
│   │   ├── Notification.cs                # title, body, type, timestamp, read
│   │   └── ActivityEvent.cs               # timestamp, message, type
│   │
│   ├── Services/                          # Abstractions + implementations
│   │   ├── IProcurementService.cs         # GetOrders, GetOrder, Approve, Reject, Escalate
│   │   ├── IWarehouseService.cs           # PlaceVoxel, EraseVoxel, SetZone, LoadPreset, GetMetrics
│   │   ├── INotificationService.cs        # GetNotifications, MarkRead, MarkAllRead
│   │   ├── IActivityService.cs            # GetActivity
│   │   └── Impl/
│   │       ├── InMemoryProcurementService.cs
│   │       ├── InMemoryWarehouseService.cs
│   │       ├── InMemoryNotificationService.cs
│   │       └── InMemoryActivityService.cs
│   │
│   ├── Presentation/                      # Pages + MVUX Models
│   │   ├── Shell.xaml / Shell.xaml.cs      # App shell (topbar, nav, content, statusbar)
│   │   ├── ShellModel.cs                  # Breadcrumbs, notifications, user context
│   │   │
│   │   ├── Dashboard/
│   │   │   ├── DashboardPage.xaml
│   │   │   └── DashboardModel.cs          # KPIs, pipeline, activity feed
│   │   │
│   │   ├── Warehouse/
│   │   │   ├── WarehousePage.xaml
│   │   │   └── WarehouseModel.cs          # Grid state, tool/mode/layer, metrics
│   │   │
│   │   ├── Orders/
│   │   │   ├── OrdersPage.xaml            # List view
│   │   │   ├── OrdersModel.cs             # PO list, selection, bulk actions
│   │   │   ├── OrderDetailPage.xaml       # Detail view (nested route)
│   │   │   └── OrderDetailModel.cs        # Single PO, tab state, approval actions
│   │   │
│   │   └── Placeholder/
│   │       └── ComingSoonPage.xaml
│   │
│   ├── Controls/                          # Reusable custom controls
│   │   ├── KpiCard.xaml                   # Single KPI display
│   │   ├── StatusBadge.xaml               # Status pill with dot + label
│   │   ├── AiChip.xaml                    # AI insight indicator
│   │   ├── RiskBadge.xaml                 # Risk level indicator
│   │   ├── SlaBadge.xaml                  # SLA time remaining with urgency color
│   │   ├── PipelineBar.xaml               # Stacked status distribution bar
│   │   ├── ApprovalChain.xaml             # Vertical stepper for approval workflow
│   │   ├── ActivityFeed.xaml              # Timestamped event list
│   │   ├── BulkActionBar.xaml             # Floating batch operations toolbar
│   │   ├── MiniMetrics.xaml               # Compact metric bars for nav footer
│   │   ├── IsometricCanvas.xaml/.cs       # SkiaSharp-based warehouse renderer
│   │   ├── IsometricRenderer.cs           # Pure drawing logic (SkiaSharp)
│   │   ├── LayerScrubber.xaml             # Vertical z-layer control
│   │   ├── CommandPaletteDialog.xaml       # Global search/action overlay
│   │   └── NotificationFlyout.xaml         # Notification dropdown content
│   │
│   ├── Converters/                        # Value converters
│   │   ├── StatusToColorConverter.cs      # PO status → Brush
│   │   ├── RiskToColorConverter.cs        # Risk level → Brush
│   │   ├── SlaToColorConverter.cs         # SLA urgency → Brush
│   │   ├── AiTypeToColorConverter.cs      # AI alert/warn/info → Brush
│   │   ├── EventTypeToColorConverter.cs   # Activity type → Brush
│   │   ├── BoolToVisibilityConverter.cs   # Standard bool→Visibility
│   │   └── CountToVisibilityConverter.cs  # int > 0 → Visible
│   │
│   └── Helpers/
│       ├── KeyboardAcceleratorHelper.cs   # Global keyboard shortcut registration
│       └── IsometricMath.cs               # Projection functions (shared between model + renderer)
│
├── GridForm.Windows/                      # Windows head
├── GridForm.Wasm/                         # WebAssembly head
└── GridForm.Skia.Gtk/                     # Linux/macOS head (optional)
```

---

## 1.2 Navigation Architecture

### Shell Structure (Region-Based)

The app uses Uno Extensions Navigation with `Region.Attached` for view switching. The shell is a single `Grid` that hosts the topbar, left nav, main content area, and status bar.

```xml
<!-- Shell.xaml (simplified) -->
<Grid uen:Region.Attached="True">
    <Grid.RowDefinitions>
        <RowDefinition Height="48" />   <!-- TopBar -->
        <RowDefinition Height="*" />    <!-- Content -->
        <RowDefinition Height="28" />   <!-- StatusBar -->
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" /> <!-- Nav -->
        <ColumnDefinition Width="*" />   <!-- Main -->
    </Grid.ColumnDefinitions>

    <!-- TopBar spans full width -->
    <controls:TopBar Grid.Row="0" Grid.ColumnSpan="2" />

    <!-- NavigationView (left nav) -->
    <muxc:NavigationView Grid.Row="1" Grid.Column="0"
        x:Name="NavView"
        uen:Region.Attached="True"
        PaneDisplayMode="Left"
        IsPaneToggleButtonVisible="False"
        IsBackButtonVisible="Collapsed"
        IsSettingsVisible="False"
        OpenPaneLength="200">
        <muxc:NavigationView.MenuItems>
            <muxc:NavigationViewItem Content="Dashboard"
                uen:Region.Name="Dashboard"
                x:Uid="Shell.Nav.Dashboard">
                <muxc:NavigationViewItem.Icon>
                    <FontIcon Glyph="&#xE80F;" />
                </muxc:NavigationViewItem.Icon>
            </muxc:NavigationViewItem>
            <muxc:NavigationViewItem Content="Warehouse"
                uen:Region.Name="Warehouse">
                <muxc:NavigationViewItem.Icon>
                    <FontIcon Glyph="&#xE913;" />
                </muxc:NavigationViewItem.Icon>
            </muxc:NavigationViewItem>
            <muxc:NavigationViewItem Content="Orders"
                uen:Region.Name="Orders">
                <muxc:NavigationViewItem.Icon>
                    <FontIcon Glyph="&#xE8A5;" />
                </muxc:NavigationViewItem.Icon>
            </muxc:NavigationViewItem>
        </muxc:NavigationView.MenuItems>
    </muxc:NavigationView>

    <!-- Content region -->
    <Grid Grid.Row="1" Grid.Column="1"
        uen:Region.Attached="True"
        uen:Region.Navigator="Visibility">
        <!-- Pages render here via region navigation -->
    </Grid>

    <!-- StatusBar spans full width -->
    <controls:StatusBar Grid.Row="2" Grid.ColumnSpan="2" />
</Grid>
```

### Route Definitions

```csharp
// App.xaml.cs — Route registration
private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
{
    views.Register(
        new ViewMap<Shell, ShellModel>(),
        new ViewMap<DashboardPage, DashboardModel>(),
        new ViewMap<WarehousePage, WarehouseModel>(),
        new ViewMap<OrdersPage, OrdersModel>(),
        new ViewMap<OrderDetailPage, OrderDetailModel>(),
        new ViewMap<ComingSoonPage>()
    );

    routes.Register(
        new RouteMap("Shell", View: views.FindByViewModel<ShellModel>(),
            Nested: new RouteMap[]
            {
                new("Dashboard", View: views.FindByViewModel<DashboardModel>(), IsDefault: true),
                new("Warehouse", View: views.FindByViewModel<WarehouseModel>()),
                new("Orders", View: views.FindByViewModel<OrdersModel>(),
                    Nested: new RouteMap[]
                    {
                        new("OrderDetail", View: views.FindByViewModel<OrderDetailModel>())
                    }),
                new("Inventory", View: views.FindByView<ComingSoonPage>()),
                new("Vendors", View: views.FindByView<ComingSoonPage>()),
            })
    );
}
```

### Navigation Data Passing (Orders → OrderDetail)

When the user clicks a PO row, the `PurchaseOrder` record is passed as navigation data:

```xml
<!-- OrdersPage.xaml — row click triggers navigation with data -->
<DataTemplate x:Key="OrderRowTemplate">
    <Grid utu:CommandExtensions.Command="{Binding DataContext.NavigateToDetail, ElementName=OrdersRoot}"
          utu:CommandExtensions.CommandParameter="{Binding}">
        <!-- Row content -->
    </Grid>
</DataTemplate>
```

```csharp
// OrdersModel.cs
public partial record OrdersModel(IProcurementService Procurement, INavigator Navigator)
{
    public IListFeed<PurchaseOrder> Orders => ListFeed.Async(Procurement.GetOrders);

    public async ValueTask NavigateToDetail(PurchaseOrder po)
    {
        await Navigator.NavigateRouteAsync(this, "OrderDetail", data: po);
    }
}

// OrderDetailModel.cs — receives data
public partial record OrderDetailModel(PurchaseOrder Order)
{
    public IState<string> ActiveTab => State.Value(this, () => "overview");

    public async ValueTask Approve(/* ... */) { /* ... */ }
    public async ValueTask Reject(/* ... */) { /* ... */ }
    public async ValueTask Escalate(/* ... */) { /* ... */ }
}
```

---

## 1.3 State Management (MVUX)

### Model Architecture

Every page has a corresponding MVUX `partial record` Model that exposes `IFeed<T>`, `IListFeed<T>`, and `IState<T>` properties. MVUX source-generates a bindable ViewModel (`MainViewModel` from `MainModel`).

| Page | Model | Key State |
|---|---|---|
| Shell | `ShellModel` | `IListFeed<Notification> Notifications`, `IState<string> CurrentRoute` |
| Dashboard | `DashboardModel` | `IListFeed<PurchaseOrder> PipelineOrders`, `IListFeed<ActivityEvent> Activity`, `IFeed<WarehouseMetrics> Metrics` |
| Warehouse | `WarehouseModel` | `IState<WarehouseState> State`, `IFeed<WarehouseMetrics> Metrics` |
| Orders | `OrdersModel` | `IListFeed<PurchaseOrder> Orders`, `IState<HashSet<string>> Selected` |
| OrderDetail | `OrderDetailModel` | `PurchaseOrder Order` (injected), `IState<string> ActiveTab` |

### FeedView Pattern (Loading / Error / Empty)

Every data-driven section uses `FeedView` for automatic state handling:

```xml
<mvux:FeedView Source="{Binding PipelineOrders}">
    <mvux:FeedView.ValueTemplate>
        <DataTemplate>
            <ItemsRepeater ItemsSource="{Binding Data}">
                <ItemsRepeater.ItemTemplate>
                    <DataTemplate>
                        <controls:PipelineOrderCard />
                    </DataTemplate>
                </ItemsRepeater.ItemTemplate>
            </ItemsRepeater>
        </DataTemplate>
    </mvux:FeedView.ValueTemplate>
    <mvux:FeedView.ProgressTemplate>
        <DataTemplate>
            <utu:LoadingView IsLoading="True" />
        </DataTemplate>
    </mvux:FeedView.ProgressTemplate>
    <mvux:FeedView.ErrorTemplate>
        <DataTemplate>
            <controls:ErrorState Message="Failed to load orders" />
        </DataTemplate>
    </mvux:FeedView.ErrorTemplate>
    <mvux:FeedView.NoneTemplate>
        <DataTemplate>
            <controls:EmptyState Icon="&#xE8A5;" Message="No purchase orders" />
        </DataTemplate>
    </mvux:FeedView.NoneTemplate>
</mvux:FeedView>
```

### Warehouse State Model

The warehouse uses mutable state via `IState<WarehouseState>`. Because the voxel grid is large (14×14×6 = 1176 possible cells), we avoid full-grid immutable replacement on every click. Instead, `WarehouseState` wraps a `Dictionary<VoxelKey, AssetType>` and `Dictionary<GridCell, ZoneType>` that are mutated in-place, with an explicit `IState.Update()` to trigger re-render.

```csharp
public partial record WarehouseModel(IWarehouseService Warehouse)
{
    public IState<WarehouseState> State => State.Async(this, Warehouse.GetInitialState);
    public IFeed<WarehouseMetrics> Metrics => State.Select(s => s.ComputeMetrics());

    public async ValueTask PlaceAsset(VoxelKey key, AssetType asset)
    {
        await State.Update(s => s with { /* mutate grid */ });
    }
}
```

---

## 1.4 Isometric Renderer — SkiaSharp Integration

### Control: `IsometricCanvas`

The isometric warehouse renderer is implemented as a custom control wrapping `SKXamlCanvas`:

```csharp
public sealed class IsometricCanvas : SKXamlCanvas
{
    // Dependency properties for data binding
    public static readonly DependencyProperty VoxelGridProperty = ...;
    public static readonly DependencyProperty ZoneGridProperty = ...;
    public static readonly DependencyProperty CurrentLayerProperty = ...;
    public static readonly DependencyProperty CurrentAssetProperty = ...;
    public static readonly DependencyProperty ToolModeProperty = ...;
    public static readonly DependencyProperty CursorPositionProperty = ...;

    // Events for interaction
    public event EventHandler<VoxelPlacedEventArgs> VoxelPlaced;
    public event EventHandler<VoxelErasedEventArgs> VoxelErased;
    public event EventHandler<ZonePaintedEventArgs> ZonePainted;
    public event EventHandler<CursorMovedEventArgs> CursorMoved;
    public event EventHandler<LayerChangedEventArgs> LayerChanged;

    public IsometricCanvas()
    {
        PaintSurface += OnPaintSurface;
        PointerMoved += OnPointerMoved;
        PointerPressed += OnPointerPressed;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        IsometricRenderer.Draw(e.Surface.Canvas, e.Info,
            VoxelGrid, ZoneGrid, CurrentLayer, CursorPosition, CurrentAsset, ToolMode);
    }
}
```

### Renderer: `IsometricRenderer`

A static class containing all drawing logic, ported from the prototype's canvas code into SkiaSharp calls:

- `DrawZoneTiles(canvas, zones, ox, oy)` — zone floor tiles with fill + stroke
- `DrawGrid(canvas, layer, ox, oy, alpha)` — isometric grid lines
- `DrawGroundShadows(canvas, voxels, ox, oy)` — shadow tiles for elevated voxels
- `DrawVoxel(canvas, sx, sy, ao, edges, fog, assetType, ghost)` — full voxel with AO, edge highlights, labels
- `DrawGhostCursor(canvas, gx, gz, layer, asset, ox, oy)` — translucent placement preview

Uses `SKPaint` objects with `IsAntialias = true`, recycled across frames via a paint cache to avoid GC pressure.

### Performance Strategy

| Concern | Mitigation |
|---|---|
| 60fps on WASM | Profile early. Use `SKCanvasElement` (Skia targets) over `SKXamlCanvas` for hardware acceleration. Dirty-rect redraw if needed |
| Paint allocation | Pool `SKPaint` objects. Clear and reuse per frame |
| Voxel sort | Pre-sort on mutation, not per-frame. Cache sorted list |
| Pointer events | Throttle `PointerMoved` to every 16ms (match render frame) |

---

## 1.5 Dependency Injection

```csharp
// App.xaml.cs
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var builder = this.CreateBuilder(args)
        .Configure(host => host
            .UseConfiguration()
            .UseLogging()
            .UseSerialization()
            .UseThemeService()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IProcurementService, InMemoryProcurementService>();
                services.AddSingleton<IWarehouseService, InMemoryWarehouseService>();
                services.AddSingleton<INotificationService, InMemoryNotificationService>();
                services.AddSingleton<IActivityService, InMemoryActivityService>();
            })
            .UseNavigation(RegisterRoutes)
        );

    MainWindow = builder.Window;
    Host = await builder.NavigateAsync<Shell>();
}
```

---

## 1.6 Accessibility Architecture

| Requirement | Implementation |
|---|---|
| **Screen reader** | `AutomationProperties.Name` on all interactive elements. `x:Uid` for localization. `AutomationProperties.LiveSetting="Polite"` on toast, notification count, SLA badge |
| **Keyboard** | Global `KeyboardAccelerator` registration in Shell for ⌘K, Escape, 1/2/3 view switching. Warehouse keys (Q/B/Z/X/W/S) registered on WarehousePage |
| **Focus order** | TopBar → Nav → Main → StatusBar. Within main: toolbar → content → action bar. Tab index managed via visual tree order |
| **High contrast** | Leverage Material theme's built-in high-contrast mode. All semantic colors routed through `ColorPaletteOverride.xaml` |
| **Touch targets** | All buttons: `MinHeight="44"`, `MinWidth="44"`. Nav items: full-width hit targets. Table rows: full-width clickable |
| **Canvas a11y** | Keyboard equivalents for all canvas actions (already defined). `AutomationProperties.Name="Warehouse floor plan. Use Q to cycle asset, B for build mode, Z for zone mode, arrow keys for layer"` on `IsometricCanvas` |

---

## 1.7 Technical Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| SkiaSharp canvas jank on WASM at scale | Medium | Medium | Profile with 500+ voxels. Use `SKCanvasElement` on Skia targets. Implement dirty-rect rendering. Consider WebGL fallback |
| Large voxel state causing MVUX rebinding overhead | Low | Low | Mutate in-place + explicit `IState.Update()`. Only rebind metrics summary, not full grid |
| `NavigationView` pane styling clash with warm dark palette | Low | Medium | Fully override `NavigationView` lightweight styling resources. Test early |
| Command palette keyboard focus trap | Low | Medium | Use `ContentDialog` with `FullSizeDesired="False"` and manual focus management |
| Table performance with 100+ PO rows | Low | Low | Use `ItemsRepeater` with `UniformGridLayout` or `StackLayout` for virtualization |
| Cross-platform font rendering (Outfit + JetBrains Mono) | Low | Medium | Bundle fonts as app assets. Register in `Fonts` folder. Fallback chain in XAML |

