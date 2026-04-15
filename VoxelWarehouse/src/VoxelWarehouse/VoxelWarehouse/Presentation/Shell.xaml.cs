using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Text;

namespace VoxelWarehouse.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    // Syntax palette brushes
    private static readonly SolidColorBrush AccentBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xD4, 0xA9, 0x59));
    private static readonly SolidColorBrush AccentDimBrush = new(Windows.UI.Color.FromArgb(0x14, 0xD4, 0xA9, 0x59));
    private static readonly SolidColorBrush KeywordBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xD4, 0x92, 0x7A));
    private static readonly SolidColorBrush AmberBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xD4, 0xA9, 0x59));
    private static readonly SolidColorBrush RedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xD4, 0x92, 0x7A));
    private static readonly SolidColorBrush BlueBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x7C, 0xAF, 0xC2));
    private static readonly SolidColorBrush StringBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xA0, 0xB8, 0x7A));
    private static readonly SolidColorBrush BorderBrush_ = new(Windows.UI.Color.FromArgb(0xFF, 0x23, 0x26, 0x30));
    private static readonly SolidColorBrush RaisedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x1E, 0x20, 0x28));
    private static readonly SolidColorBrush T1Brush = new(Windows.UI.Color.FromArgb(0xFF, 0xDE, 0xDB, 0xD4));
    private static readonly SolidColorBrush T2Brush = new(Windows.UI.Color.FromArgb(0xFF, 0xB8, 0xB4, 0xAD));
    private static readonly SolidColorBrush T3Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x5C, 0x5F, 0x69));
    private static readonly SolidColorBrush T4Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x2E, 0x30, 0x39));
    private static readonly SolidColorBrush TransparentBrush = new(Microsoft.UI.Colors.Transparent);
    private static readonly SolidColorBrush WhiteBrush = new(Microsoft.UI.Colors.White);
    private static readonly FontFamily MonoFont = new("Martian Mono, Cascadia Code, Cascadia Mono, Consolas, monospace");

    // Asset voxel tint brushes (for the picker icons)
    private static readonly SolidColorBrush PalletBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xB4, 0xAF, 0xA0));
    private static readonly SolidColorBrush RackBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x8C, 0x9B, 0xAA));
    private static readonly SolidColorBrush ContainerBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xAA, 0xA0, 0x96));
    private static readonly SolidColorBrush EquipBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xA5, 0x96, 0x8C));
    private static readonly SolidColorBrush AisleBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x55, 0x5A, 0x5F));

    // Zone tint brushes
    private static readonly SolidColorBrush ReceivingBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xB4, 0xC8, 0xDC));
    private static readonly SolidColorBrush StorageBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xDC, 0xD2, 0xB4));
    private static readonly SolidColorBrush StagingBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xC8, 0xB4, 0xDC));
    private static readonly SolidColorBrush ShippingBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xB4, 0xDC, 0xC8));

    private string _activeTab = "Warehouse";
    private AssetType _activeAsset = AssetType.Pallet;
    private string _activeMode = "Build"; // Build, Zone, Erase
    private Button[] _assetButtons = [];
    private IsometricCanvasControl? _canvas;

    // Undo/Redo stacks — VoxelWorldState is immutable, so we just store snapshots
    private const int MaxUndoDepth = 50;
    private readonly Stack<VoxelWorldState> _undoStack = new();
    private readonly Stack<VoxelWorldState> _redoStack = new();
    private VoxelWorldState? _lastSnapshot;

    public Shell()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        KeyDown += OnKeyDown;
    }

    public ContentControl ContentControl => Splash;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildAssetPicker();
        BuildAssetCounts(null);
        BuildZoneCounts(null);

        // Watch for EditorPage availability
        EditorPage.CanvasRegistered += OnCanvasRegistered;

        // Watch for order selection in procurement view
        OrdersPage.OrderSelected += OnOrderSelected;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe static events to prevent memory leaks
        EditorPage.CanvasRegistered -= OnCanvasRegistered;
        OrdersPage.OrderSelected -= OnOrderSelected;

        if (_canvas is not null)
        {
            _canvas.WorldChanged -= OnWorldChanged;
            _canvas.CursorMoved -= OnCursorMoved;
        }
    }

    #region Canvas Connection

    private void OnCanvasRegistered(EditorPage page, IsometricCanvasControl canvas)
    {
        _editorPage = page;
        _canvas = canvas;
        _canvas.WorldChanged += OnWorldChanged;
        _canvas.CursorMoved += OnCursorMoved;
        _lastSnapshot = _canvas.World;
        UpdateAllMetrics(_canvas.World);
        UpdateUndoRedoButtons();
    }

    private void OnWorldChanged(VoxelWorldState world) =>
        DispatcherQueue?.TryEnqueue(() =>
        {
            // Push previous state to undo stack (bounded)
            if (_lastSnapshot is not null && _lastSnapshot != world)
            {
                _undoStack.Push(_lastSnapshot);
                // Trim if over capacity — drop oldest by rebuilding once
                if (_undoStack.Count > MaxUndoDepth)
                {
                    var keep = _undoStack.Take(MaxUndoDepth).Reverse().ToArray();
                    _undoStack.Clear();
                    foreach (var s in keep) _undoStack.Push(s);
                }
                _redoStack.Clear();
                UpdateUndoRedoButtons();
            }
            _lastSnapshot = world;

            UpdateAllMetrics(world);
            UpdateContextInspector(world);
        });

    private void OnCursorMoved(int gx, int gz) =>
        DispatcherQueue?.TryEnqueue(() =>
        {
            if (_canvas is not null)
                UpdateContextInspector(_canvas.World);
        });

    private void UpdateAllMetrics(VoxelWorldState world)
    {
        var m = MetricsCalculator.Compute(world);

        // Top bar KPIs
        KpiFloor.Text = $"{m.FloorUtilPercent}%";
        KpiVol.Text = $"{m.VolumeUtilPercent}%";
        KpiTons.Text = $"{m.TotalWeightTons}";
        KpiPOs.Text = $"{SeedDataService.GetPurchaseOrders().Count}";

        // Left rail utilization
        UtilFloor.Text = $"{m.FloorUtilPercent}%";
        UtilVolume.Text = $"{m.VolumeUtilPercent}%";
        UtilWeight.Text = $"{m.TotalWeightTons}t";
        UtilPeak.Text = $"{m.PeakHeight}";
        UtilUnits.Text = $"{m.TotalUnits}";

        const double barMax = 170;
        FloorBar.Width = Math.Max(0, Math.Min(barMax, barMax * m.FloorUtilPercent / 100.0));
        VolumeBar.Width = Math.Max(0, Math.Min(barMax, barMax * m.VolumeUtilPercent / 100.0));
        WeightBar.Width = Math.Max(0, Math.Min(barMax, barMax * m.TotalWeightTons / 200.0));

        BuildAssetCounts(m);
        BuildZoneCounts(m);
    }

    #endregion

    #region Camera Toggle

    private bool _cameraActive;
    private EditorPage? _editorPage;

    private void OnToggleCamera(object sender, RoutedEventArgs e)
    {
        if (_editorPage is null) return;

        _editorPage.ToggleCamera();
        _cameraActive = !_cameraActive;
        CameraDot.Fill = _cameraActive ? StringBrush : T4Brush;
    }

    #endregion

    #region Undo / Redo

    private void OnUndo(object sender, RoutedEventArgs e) => Undo();
    private void OnRedo(object sender, RoutedEventArgs e) => Redo();

    private void Undo()
    {
        if (_undoStack.Count == 0 || _canvas is null) return;
        _redoStack.Push(_canvas.World);
        var prev = _undoStack.Pop();
        _lastSnapshot = prev; // prevent re-pushing to undo
        _canvas.World = prev;
        UpdateAllMetrics(prev);
        UpdateContextInspector(prev);
        UpdateUndoRedoButtons();
    }

    private void Redo()
    {
        if (_redoStack.Count == 0 || _canvas is null) return;
        _undoStack.Push(_canvas.World);
        var next = _redoStack.Pop();
        _lastSnapshot = next;
        _canvas.World = next;
        UpdateAllMetrics(next);
        UpdateContextInspector(next);
        UpdateUndoRedoButtons();
    }

    private void UpdateUndoRedoButtons()
    {
        BtnUndo.Opacity = _undoStack.Count > 0 ? 1.0 : 0.3;
        BtnRedo.Opacity = _redoStack.Count > 0 ? 1.0 : 0.3;
    }

    private async void OnClearLayout(object sender, RoutedEventArgs e)
    {
        if (_canvas is null) return;

        var dialog = new ContentDialog
        {
            Title = "Clear Layout",
            Content = "This will erase all placed voxels and zones. Continue?",
            PrimaryButtonText = "Clear",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            _canvas.World = VoxelWorldState.Empty();
            UpdateAllMetrics(_canvas.World);
        }
    }

    #endregion

    #region Tab Navigation

    private async void OnTabWarehouse(object sender, RoutedEventArgs e) => await SwitchTab("Warehouse", "Editor");
    private async void OnTabProcurement(object sender, RoutedEventArgs e) => await SwitchTab("Procurement", "Orders");

    private async Task SwitchTab(string tab, string route)
    {
        if (_activeTab == tab) return;
        _activeTab = tab;
        UpdateTabState();

        if (tab == "Warehouse")
            RestoreWarehouseRightPanel();

        try
        {
            var navigator = this.Navigator();
            if (navigator is not null)
                await navigator.NavigateRouteAsync(this, route);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Shell] Nav error: {ex.Message}");
        }
    }

    private void UpdateTabState()
    {
        bool isWarehouse = _activeTab == "Warehouse";

        TabWarehouse.Background = isWarehouse ? AccentBrush : TransparentBrush;
        TabWarehouse.BorderBrush = isWarehouse ? AccentBrush : BorderBrush_;
        TabWarehouseLabel.Foreground = isWarehouse ? WhiteBrush : T3Brush;

        TabProcurement.Background = isWarehouse ? TransparentBrush : AccentBrush;
        TabProcurement.BorderBrush = isWarehouse ? BorderBrush_ : AccentBrush;
        TabProcurementLabel.Foreground = isWarehouse ? T3Brush : WhiteBrush;

        // Show/hide warehouse left rail content
        LeftRailContent.Visibility = isWarehouse ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region Mode & Tool Controls

    private void OnSetMode(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tag) return;
        _activeMode = tag;
        UpdateModeButtons();

        if (_canvas is null) return;

        switch (tag)
        {
            case "Build":
                _canvas.World = _canvas.World with { ActiveMode = EditorMode.Build, ActiveTool = ToolMode.Place };
                break;
            case "Zone":
                _canvas.World = _canvas.World with { ActiveMode = EditorMode.Zone };
                break;
            case "Erase":
                _canvas.World = _canvas.World with { ActiveMode = EditorMode.Build, ActiveTool = ToolMode.Erase };
                break;
        }
    }

    private void UpdateModeButtons()
    {
        SetModeActive(BtnBuild, _activeMode == "Build");
        SetModeActive(BtnZone, _activeMode == "Zone");
        SetModeActive(BtnErase, _activeMode == "Erase");
    }

    private static void SetModeActive(Button btn, bool active)
    {
        btn.Background = active ? AccentDimBrush : TransparentBrush;
        btn.BorderBrush = active ? AccentBrush : BorderBrush_;
        if (btn.Content is TextBlock tb)
        {
            tb.Foreground = active ? AccentBrush : T3Brush;
            tb.FontWeight = active ? FontWeights.SemiBold : FontWeights.Light;
        }
    }

    #endregion

    #region Asset Picker

    private void BuildAssetPicker()
    {
        AssetPicker.Children.Clear();
        var assets = new (AssetType Type, string Name, string Weight, SolidColorBrush Color)[]
        {
            (AssetType.Pallet, "PALLET", "1.2t", PalletBrush),
            (AssetType.Rack, "RACK", "0.4t", RackBrush),
            (AssetType.Container, "CONTAINER", "2.5t", ContainerBrush),
            (AssetType.Equipment, "EQUIP", "3t", EquipBrush),
            (AssetType.Aisle, "AISLE", "0t", AisleBrush),
        };

        var buttons = new List<Button>();
        foreach (var (type, name, weight, color) in assets)
        {
            bool active = type == _activeAsset;
            var btn = new Button
            {
                Tag = type.ToString(),
                Style = (Style)Application.Current.Resources["GfAssetRowStyle"],
                Background = active ? RaisedBrush : TransparentBrush,
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new Border
            {
                Width = 14, Height = 14, CornerRadius = new CornerRadius(2),
                Background = color, Opacity = active ? 1.0 : 0.5,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(icon, 0);

            var label = new TextBlock
            {
                Text = name, FontFamily = MonoFont, FontSize = 9,
                FontWeight = active ? FontWeights.SemiBold : FontWeights.Light,
                CharacterSpacing = 100,
                Foreground = active ? T1Brush : T3Brush,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(label, 1);

            var weightText = new TextBlock
            {
                Text = weight, FontFamily = MonoFont, FontSize = 8,
                FontWeight = FontWeights.Light, CharacterSpacing = 80,
                Foreground = T4Brush, VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(weightText, 2);

            grid.Children.Add(icon);
            grid.Children.Add(label);
            grid.Children.Add(weightText);
            btn.Content = grid;

            btn.Click += OnSetAsset;
            buttons.Add(btn);
            AssetPicker.Children.Add(btn);
        }
        _assetButtons = [.. buttons];
    }

    private void OnSetAsset(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tag) return;
        if (!Enum.TryParse<AssetType>(tag, out var type)) return;

        _activeAsset = type;

        foreach (var b in _assetButtons)
        {
            bool active = b.Tag?.ToString() == tag;
            b.Background = active ? RaisedBrush : TransparentBrush;

            if (b.Content is Grid g)
            {
                foreach (var child in g.Children)
                {
                    if (child is Border icon && icon.Width == 14)
                        icon.Opacity = active ? 1.0 : 0.5;
                    else if (child is TextBlock tb && tb.FontSize == 9) // name label
                    {
                        tb.Foreground = active ? T1Brush : T3Brush;
                        tb.FontWeight = active ? FontWeights.SemiBold : FontWeights.Light;
                    }
                }
            }
        }

        if (_canvas is not null)
            _canvas.World = _canvas.World with { ActiveAsset = type };
    }

    #endregion

    #region Asset & Zone Counts

    private void BuildAssetCounts(UtilizationMetrics? m)
    {
        AssetCounts.Children.Clear();
        var items = new (string Name, AssetType Type, SolidColorBrush Color)[]
        {
            ("PALLET", AssetType.Pallet, PalletBrush),
            ("RACK", AssetType.Rack, RackBrush),
            ("CONTAINER", AssetType.Container, ContainerBrush),
            ("EQUIP", AssetType.Equipment, EquipBrush),
        };

        foreach (var (name, type, color) in items)
        {
            int count = m?.AssetCounts.GetValueOrDefault(type) ?? 0;
            AssetCounts.Children.Add(MakeCountRow(name, count.ToString(), color));
        }
    }

    private void BuildZoneCounts(UtilizationMetrics? m)
    {
        ZoneCounts.Children.Clear();
        var items = new (string Name, ZoneType Type, SolidColorBrush Color)[]
        {
            ("RECEIVING", ZoneType.Receiving, ReceivingBrush),
            ("STORAGE", ZoneType.Storage, StorageBrush),
            ("STAGING", ZoneType.Staging, StagingBrush),
            ("SHIPPING", ZoneType.Shipping, ShippingBrush),
        };

        foreach (var (name, type, color) in items)
        {
            int count = m?.ZoneCellCounts.GetValueOrDefault(type) ?? 0;
            ZoneCounts.Children.Add(MakeCountRow(name, count.ToString(), color));
        }
    }

    private static Grid MakeCountRow(string label, string value, SolidColorBrush dotColor)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var dot = new Ellipse { Width = 5, Height = 5, Fill = dotColor, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(dot, 0);

        var lbl = new TextBlock
        {
            Text = label, FontFamily = MonoFont, FontSize = 9,
            FontWeight = FontWeights.Light, CharacterSpacing = 80,
            Foreground = T2Brush, VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        Grid.SetColumn(lbl, 1);

        var val = new TextBlock
        {
            Text = value, FontFamily = MonoFont, FontSize = 10,
            FontWeight = FontWeights.Normal, CharacterSpacing = 60,
            Foreground = T1Brush, VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(val, 2);

        grid.Children.Add(dot);
        grid.Children.Add(lbl);
        grid.Children.Add(val);
        return grid;
    }

    #endregion

    #region Context Inspector

    private static readonly Dictionary<AssetType, string> AssetWeights = new()
    {
        [AssetType.Pallet] = "1.2t",
        [AssetType.Rack] = "0.4t",
        [AssetType.Container] = "2.5t",
        [AssetType.Equipment] = "3t",
        [AssetType.Aisle] = "0t",
    };

    private void UpdateContextInspector(VoxelWorldState world)
    {
        if (_canvas is null) return;

        var (gx, gz) = _canvas.CursorGridPosition;
        int layer = world.ActiveLayer;

        // Coordinate
        InspCoord.Text = $"({gx}, {layer}, {gz})";
        InspX.Text = $"{gx}";
        InspZ.Text = $"{gz}";

        // Height = topmost occupied layer at this column
        int height = 0;
        for (int y = GridConstants.MaxHeight - 1; y >= 0; y--)
        {
            if (world.Cells.ContainsKey((gx, y, gz))) { height = y + 1; break; }
        }
        InspHeight.Text = $"{height}";
        InspLayer.Text = $"{layer}/{GridConstants.MaxHeight - 1}";

        // Asset at cursor
        var cellKey = (gx, layer, gz);
        if (world.Cells.TryGetValue(cellKey, out var asset))
        {
            InspAssetName.Text = asset.ToString().ToUpper();
            InspAssetWeight.Text = AssetWeights.GetValueOrDefault(asset, "");
        }
        else
        {
            InspAssetName.Text = "--";
            InspAssetWeight.Text = "";
        }

        // Zone at cursor
        var zoneKey = (gx, gz);
        if (world.Zones.TryGetValue(zoneKey, out var zone) && zone != ZoneType.None)
            InspZone.Text = zone.ToString().ToUpper();
        else
            InspZone.Text = "--";

        // Capacity: column fill percentage
        int filled = 0;
        for (int y = 0; y < GridConstants.MaxHeight; y++)
        {
            if (world.Cells.ContainsKey((gx, y, gz))) filled++;
        }
        int capacityPct = (int)Math.Round(filled * 100.0 / GridConstants.MaxHeight);
        CapacityText.Text = $"{capacityPct}/100";
        CapacityBar.Width = Math.Max(0, Math.Min(180, 180 * capacityPct / 100.0));

        // SKU and PO — use seed data keyed by position hash for demo consistency
        var orders = SeedDataService.GetPurchaseOrders();
        int orderIdx = Math.Abs((gx * 7 + gz * 13 + layer * 3) % orders.Count);
        var po = orders[orderIdx];

        if (world.Cells.ContainsKey(cellKey))
        {
            InspSkuId.Text = $"EM-{orderIdx:D3}";
            InspSkuDesc.Text = $"{po.Detail.Split('\n').FirstOrDefault()?.Trim() ?? po.VendorName}";
            InspPoCount.Text = "1";
            InspPoId.Text = po.Id;
            InspPoPriority.Text = po.Risk.ToString();

            // AI analysis text
            var zoneName = (world.Zones.TryGetValue(zoneKey, out var z2) && z2 != ZoneType.None)
                ? z2.ToString().ToLower() : "unzoned";
            var conflict = capacityPct > 80 ? "Nearing capacity limit." : "No spatial conflicts detected.";
            InspAiText.Text = $"Location ({gx},{layer},{gz}) in {zoneName} zone. " +
                $"Capacity utilization {capacityPct}% \u2014 {(capacityPct > 80 ? "approaching threshold" : "within operational range")}. {conflict}";
        }
        else
        {
            InspSkuId.Text = "--";
            InspSkuDesc.Text = "--";
            InspPoCount.Text = "0";
            InspPoId.Text = "--";
            InspPoPriority.Text = "";
            InspAiText.Text = "Select a cell to view spatial analysis.";
        }
    }

    private void OnClearInspector(object sender, RoutedEventArgs e)
    {
        InspCoord.Text = "(0, 0, 0)";
        InspX.Text = "0"; InspZ.Text = "0";
        InspHeight.Text = "0"; InspLayer.Text = "0/5";
        InspAssetName.Text = "--"; InspAssetWeight.Text = "";
        InspZone.Text = "--";
        CapacityText.Text = "0/100"; CapacityBar.Width = 0;
        InspSkuId.Text = "--"; InspSkuDesc.Text = "--";
        InspPoCount.Text = "0"; InspPoId.Text = "--"; InspPoPriority.Text = "";
        InspAiText.Text = "Select a cell to view spatial analysis.";
    }

    #endregion

    #region Order Selection

    private void OnOrderSelected(PurchaseOrder? po)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            if (po is null) return;
            ShowOrderInInspector(po);
        });
    }

    private void ShowOrderInInspector(PurchaseOrder po)
    {
        InspCoord.Text = po.Id;
        InspAssetName.Text = po.VendorName;
        InspAssetWeight.Text = $"${po.Amount:N0}";
        InspZone.Text = po.Status.ToString().ToUpper();
        InspSkuId.Text = po.Id;
        InspSkuDesc.Text = po.Detail.Split('\n').FirstOrDefault()?.Trim() ?? "";
        InspPoPriority.Text = po.Risk.ToString();
        InspAiText.Text = po.AiBrief ?? $"Order {po.Id} from {po.VendorName}, {po.VendorRegion}.";
    }

    private void RestoreWarehouseRightPanel()
    {
        OnClearInspector(this, new RoutedEventArgs());

        if (_canvas is not null)
            UpdateAllMetrics(_canvas.World);
    }

    #endregion

    #region Layout Presets

    private void OnLoadPreset(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string name && _canvas is not null)
        {
            _canvas.World = Presets.Get(name);
            UpdateAllMetrics(_canvas.World);
        }
    }

    #endregion

    #region Keyboard Shortcuts

    private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        bool ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        switch (e.Key)
        {
            // Undo/Redo: Ctrl+Z / Ctrl+Y
            case Windows.System.VirtualKey.Z when ctrl:
                Undo();
                e.Handled = true;
                return;
            case Windows.System.VirtualKey.Y when ctrl:
                Redo();
                e.Handled = true;
                return;

            // View switching: 1=Warehouse, 2=Procurement
            case Windows.System.VirtualKey.Number1:
                OnTabWarehouse(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Number2:
                OnTabProcurement(this, new RoutedEventArgs());
                e.Handled = true;
                break;

            // Mode shortcuts
            case Windows.System.VirtualKey.B:
                _activeMode = "Build";
                UpdateModeButtons();
                if (_canvas is not null)
                    _canvas.World = _canvas.World with { ActiveMode = EditorMode.Build, ActiveTool = ToolMode.Place };
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Z:
                _activeMode = "Zone";
                UpdateModeButtons();
                if (_canvas is not null)
                    _canvas.World = _canvas.World with { ActiveMode = EditorMode.Zone };
                e.Handled = true;
                break;

            // Asset cycle
            case Windows.System.VirtualKey.Q:
                CycleAsset();
                e.Handled = true;
                break;

            // Camera toggle
            case Windows.System.VirtualKey.C:
                OnToggleCamera(this, new RoutedEventArgs());
                e.Handled = true;
                break;

            // Erase mode
            case Windows.System.VirtualKey.E:
                _activeMode = "Erase";
                UpdateModeButtons();
                if (_canvas is not null)
                    _canvas.World = _canvas.World with { ActiveMode = EditorMode.Build, ActiveTool = ToolMode.Erase };
                e.Handled = true;
                break;
        }
    }

    private void CycleAsset()
    {
        var assets = Enum.GetValues<AssetType>();
        int idx = Array.IndexOf(assets, _activeAsset);
        _activeAsset = assets[(idx + 1) % assets.Length];

        // Rebuild picker to update visual state
        BuildAssetPicker();

        if (_canvas is not null)
            _canvas.World = _canvas.World with { ActiveAsset = _activeAsset };
    }

    #endregion
}
