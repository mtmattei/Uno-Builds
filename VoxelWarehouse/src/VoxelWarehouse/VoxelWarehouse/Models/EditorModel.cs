using Uno.Extensions.Reactive;

namespace VoxelWarehouse.Models;

public partial record EditorModel
{
    private readonly IVoxelStorageService _storage;

    public EditorModel(IVoxelStorageService storage)
    {
        _storage = storage;
    }

    public IState<VoxelWorldState> World => State<VoxelWorldState>.Value(this, () => Presets.WarehouseA());

    public IState<HandTrackingResult> HandState => State<HandTrackingResult>.Value(this, () => HandTrackingResult.None());

    public IState<(int GX, int GZ)> CursorPosition => State<(int GX, int GZ)>.Value(this, () => (6, 6));

    public IState<bool> ShowMetrics => State<bool>.Value(this, () => true);

    public IState<bool> CameraEnabled => State<bool>.Value(this, () => false);

    public IFeed<UtilizationMetrics> Metrics => World.Select(w => MetricsCalculator.Compute(w));

    public async ValueTask PlaceVoxel(CancellationToken ct)
    {
        var world = await World;
        var cursor = await CursorPosition;
        if (world is null) return;

        var key = (cursor.GX, world.ActiveLayer, cursor.GZ);
        if (world.ActiveTool == ToolMode.Place)
        {
            await World.UpdateAsync(s => s with { Cells = s.Cells.SetItem(key, s.ActiveAsset) }, ct);
        }
        else
        {
            await World.UpdateAsync(s => s with { Cells = s.Cells.Remove(key) }, ct);
        }
    }

    public async ValueTask SetLayer(int layer, CancellationToken ct)
    {
        await World.UpdateAsync(s => s with { ActiveLayer = Math.Clamp(layer, 0, GridConstants.MaxHeight - 1) }, ct);
    }

    public async ValueTask LayerUp(CancellationToken ct)
    {
        var world = await World;
        if (world is null) return;
        await SetLayer(world.ActiveLayer + 1, ct);
    }

    public async ValueTask LayerDown(CancellationToken ct)
    {
        var world = await World;
        if (world is null) return;
        await SetLayer(world.ActiveLayer - 1, ct);
    }

    public async ValueTask SetTool(string tool, CancellationToken ct)
    {
        if (Enum.TryParse<ToolMode>(tool, out var mode))
            await World.UpdateAsync(s => s with { ActiveTool = mode }, ct);
    }

    public async ValueTask SetAsset(string asset, CancellationToken ct)
    {
        if (Enum.TryParse<AssetType>(asset, out var type))
            await World.UpdateAsync(s => s with { ActiveAsset = type }, ct);
    }

    public async ValueTask SetMode(string mode, CancellationToken ct)
    {
        if (Enum.TryParse<EditorMode>(mode, out var m))
            await World.UpdateAsync(s => s with { ActiveMode = m }, ct);
    }

    public async ValueTask LoadPreset(string name, CancellationToken ct)
    {
        var preset = Presets.Get(name);
        await World.UpdateAsync(_ => preset, ct);
    }

    public async ValueTask ToggleMetrics(CancellationToken ct)
    {
        await ShowMetrics.UpdateAsync(v => !v, ct);
    }

    public async ValueTask ToggleCamera(CancellationToken ct)
    {
        await CameraEnabled.UpdateAsync(v => !v, ct);
    }

    public async ValueTask SaveLayout(CancellationToken ct)
    {
        var world = await World;
        if (world is not null)
            await _storage.SaveAsync("current", world, ct);
    }

    public async ValueTask LoadLayout(CancellationToken ct)
    {
        var world = await _storage.LoadAsync("current", ct);
        if (world is not null)
            await World.UpdateAsync(_ => world, ct);
    }
}
