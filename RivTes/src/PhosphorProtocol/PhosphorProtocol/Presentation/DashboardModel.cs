using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record DashboardModel(IVehicleService VehicleService, IClimateService ClimateService)
{
    public IFeed<VehicleState> Vehicle => Feed.Async(async ct => await VehicleService.GetCurrentState(ct));

    // Climate bar state
    public IState<int> Temperature => State.Value(this, () => 72);
    public IState<int> FanSpeed => State.Value(this, () => 3);
    public IState<int> SeatHeatLeft => State.Value(this, () => 1);
    public IState<int> SeatHeatRight => State.Value(this, () => 0);
    public IState<bool> ACEnabled => State.Value(this, () => true);
    public IState<bool> DefrostEnabled => State.Value(this, () => false);

    public async ValueTask IncrementTemp(CancellationToken ct)
        => await Temperature.Update(t => Math.Min(85, t + 1), ct);

    public async ValueTask DecrementTemp(CancellationToken ct)
        => await Temperature.Update(t => Math.Max(60, t - 1), ct);

    public async ValueTask CycleFan(CancellationToken ct)
        => await FanSpeed.Update(f => f < 5 ? f + 1 : 0, ct);

    public async ValueTask CycleSeatHeatLeft(CancellationToken ct)
        => await SeatHeatLeft.Update(h => (h + 1) % 4, ct);

    public async ValueTask CycleSeatHeatRight(CancellationToken ct)
        => await SeatHeatRight.Update(h => (h + 1) % 4, ct);

    public async ValueTask ToggleAC(CancellationToken ct)
        => await ACEnabled.Update(v => !v, ct);

    public async ValueTask ToggleDefrost(CancellationToken ct)
        => await DefrostEnabled.Update(v => !v, ct);
}
