using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public interface IMediaService
{
    ValueTask<MediaState> GetCurrentState(CancellationToken ct);
}
