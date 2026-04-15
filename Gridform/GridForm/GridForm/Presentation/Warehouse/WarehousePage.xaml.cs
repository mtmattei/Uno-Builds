namespace GridForm.Presentation.Warehouse;

public sealed partial class WarehousePage : Page
{
	private WarehouseState _state = new();
	private IWarehouseService? _service;

	public WarehousePage()
	{
		this.InitializeComponent();
		this.Loaded += OnPageLoaded;
	}

	private async void OnPageLoaded(object sender, RoutedEventArgs e)
	{
		_service = App.Current.Host?.Services?.GetService<IWarehouseService>();
		if (_service is not null)
		{
			_state = await _service.GetInitialState();
			PushStateToCanvas();
		}
	}

	private void PushStateToCanvas()
	{
		IsoCanvas.VoxelGrid = _state.Voxels;
		IsoCanvas.ZoneGrid = _state.Zones;
		IsoCanvas.CurrentLayer = _state.CurrentLayer;
		IsoCanvas.CurrentAsset = _state.CurrentAsset;
		IsoCanvas.ToolMode = _state.CurrentTool;
		IsoCanvas.BuildMode = _state.CurrentMode;
		Scrubber.CurrentLayer = _state.CurrentLayer;
		IsoCanvas.Invalidate();
	}

	private void OnVoxelClicked(object? sender, VoxelClickEventArgs e)
	{
		if (e.Mode == WarehouseMode.Build)
		{
			var key = new VoxelKey(e.GX, e.Layer, e.GZ);
			if (e.Tool == WarehouseTool.Erase)
				_state.Voxels.Remove(key);
			else
				_state.Voxels[key] = e.Asset;
		}
		else
		{
			var zoneKey = (e.GX, e.GZ);
			if (e.Tool == WarehouseTool.Erase)
				_state.Zones.Remove(zoneKey);
			else
				_state.Zones[zoneKey] = ZoneType.Storage;
		}
		PushStateToCanvas();
	}

	private void OnBuildMode(object sender, RoutedEventArgs e)
	{
		_state = _state with { CurrentMode = WarehouseMode.Build };
		PushStateToCanvas();
	}

	private void OnZoneMode(object sender, RoutedEventArgs e)
	{
		_state = _state with { CurrentMode = WarehouseMode.Zone };
		PushStateToCanvas();
	}

	private void OnToggleErase(object sender, RoutedEventArgs e)
	{
		_state = _state with
		{
			CurrentTool = _state.CurrentTool == WarehouseTool.Erase
				? WarehouseTool.Place
				: WarehouseTool.Erase
		};
		PushStateToCanvas();
	}

	private void OnAssetSelected(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.Tag is string tag && Enum.TryParse<AssetType>(tag, out var asset))
		{
			_state = _state with { CurrentAsset = asset };
			PushStateToCanvas();
		}
	}

	private async void OnLoadPresetA(object sender, RoutedEventArgs e)
	{
		if (_service is not null)
		{
			_state = await _service.LoadPreset("A");
			PushStateToCanvas();
		}
	}

	private async void OnLoadPresetB(object sender, RoutedEventArgs e)
	{
		if (_service is not null)
		{
			_state = await _service.LoadPreset("B");
			PushStateToCanvas();
		}
	}

	private void OnClearGrid(object sender, RoutedEventArgs e)
	{
		_state = new WarehouseState();
		PushStateToCanvas();
	}

	private void OnLayerChanged(object? sender, int layer)
	{
		_state = _state with { CurrentLayer = Math.Clamp(layer, 0, WarehouseState.MaxLayers - 1) };
		PushStateToCanvas();
	}
}
