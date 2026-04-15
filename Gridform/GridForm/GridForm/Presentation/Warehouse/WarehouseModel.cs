namespace GridForm.Presentation.Warehouse;

public partial record WarehouseModel(IWarehouseService Warehouse)
{
	public IState<WarehouseState> State =>
		State<WarehouseState>.Async(this, Warehouse.GetInitialState);

	public IFeed<WarehouseMetrics> Metrics =>
		Feed.Async(async ct =>
		{
			var s = await State.Value(ct);
			return s?.ComputeMetrics() ?? new WarehouseMetrics(0, 0, 0, 0, ImmutableDictionary<AssetType, int>.Empty, 0);
		});

	public async ValueTask PlaceAsset(int gx, int gz, int layer, AssetType asset)
	{
		await State.UpdateAsync(s =>
		{
			if (s is null) return s;
			var key = new VoxelKey(gx, layer, gz);
			s.Voxels[key] = asset;
			return s;
		});
	}

	public async ValueTask EraseAsset(int gx, int gz, int layer)
	{
		await State.UpdateAsync(s =>
		{
			if (s is null) return s;
			var key = new VoxelKey(gx, layer, gz);
			s.Voxels.Remove(key);
			return s;
		});
	}

	public async ValueTask PaintZone(int gx, int gz, ZoneType zone)
	{
		await State.UpdateAsync(s =>
		{
			if (s is null) return s;
			s.Zones[(gx, gz)] = zone;
			return s;
		});
	}

	public async ValueTask EraseZone(int gx, int gz)
	{
		await State.UpdateAsync(s =>
		{
			if (s is null) return s;
			s.Zones.Remove((gx, gz));
			return s;
		});
	}

	public async ValueTask SetMode(WarehouseMode mode)
	{
		await State.UpdateAsync(s => s is null ? s : s with { CurrentMode = mode });
	}

	public async ValueTask SetTool(WarehouseTool tool)
	{
		await State.UpdateAsync(s => s is null ? s : s with { CurrentTool = tool });
	}

	public async ValueTask SetAsset(AssetType asset)
	{
		await State.UpdateAsync(s => s is null ? s : s with { CurrentAsset = asset });
	}

	public async ValueTask CycleAsset()
	{
		await State.UpdateAsync(s =>
		{
			if (s is null) return s;
			var next = (AssetType)(((int)s.CurrentAsset + 1) % Enum.GetValues<AssetType>().Length);
			return s with { CurrentAsset = next };
		});
	}

	public async ValueTask SetLayer(int layer)
	{
		await State.UpdateAsync(s => s is null ? s : s with { CurrentLayer = Math.Clamp(layer, 0, WarehouseState.MaxLayers - 1) });
	}

	public async ValueTask LoadPreset(string name)
	{
		var preset = await Warehouse.LoadPreset(name);
		await State.UpdateAsync(_ => preset);
	}

	public async ValueTask ClearGrid()
	{
		await State.UpdateAsync(_ => new WarehouseState());
	}
}
