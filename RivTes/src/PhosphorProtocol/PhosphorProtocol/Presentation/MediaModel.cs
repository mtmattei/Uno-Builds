using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record MediaModel(IMediaService MediaService)
{
    public IFeed<MediaState> Media => Feed.Async(async ct => await MediaService.GetCurrentState(ct));

    public IState<string> ActiveSource => State.Value(this, () => "FM");

    public async ValueTask SelectSource(string source, CancellationToken ct)
        => await ActiveSource.Set(source, ct);
}
