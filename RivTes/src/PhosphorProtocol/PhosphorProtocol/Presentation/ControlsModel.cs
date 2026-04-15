namespace PhosphorProtocol.Presentation;

public partial record ControlsModel
{
    public IState<bool> DoorsLocked => State.Value(this, () => true);
    public IState<bool> MirrorsFolded => State.Value(this, () => false);
    public IState<string> HeadlightMode => State.Value(this, () => "AUTO");
    public IState<bool> TrunkOpen => State.Value(this, () => false);
    public IState<bool> FrunkOpen => State.Value(this, () => false);

    public async ValueTask ToggleDoorLock(CancellationToken ct)
        => await DoorsLocked.Update(v => !v, ct);

    public async ValueTask ToggleMirrors(CancellationToken ct)
        => await MirrorsFolded.Update(v => !v, ct);

    public async ValueTask SetHeadlights(string mode, CancellationToken ct)
        => await HeadlightMode.Set(mode, ct);

    public async ValueTask ToggleTrunk(CancellationToken ct)
        => await TrunkOpen.Update(v => !v, ct);

    public async ValueTask ToggleFrunk(CancellationToken ct)
        => await FrunkOpen.Update(v => !v, ct);
}
