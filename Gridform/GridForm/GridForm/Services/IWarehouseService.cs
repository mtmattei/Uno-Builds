namespace GridForm.Services;

public interface IWarehouseService
{
	ValueTask<WarehouseState> GetInitialState(CancellationToken ct = default);
	ValueTask<WarehouseState> LoadPreset(string presetName, CancellationToken ct = default);
}
