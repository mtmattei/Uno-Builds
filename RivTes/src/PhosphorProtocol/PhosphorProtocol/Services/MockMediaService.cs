using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockMediaService : IMediaService
{
    public ValueTask<MediaState> GetCurrentState(CancellationToken ct)
    {
        return ValueTask.FromResult(new MediaState(
            Source: "FM",
            TrackTitle: "Midnight City",
            Artist: "M83",
            Duration: TimeSpan.FromMinutes(4.03),
            Position: TimeSpan.FromMinutes(1.42),
            IsPlaying: true,
            Presets:
            [
                new RadioPreset("98.1", "CHFI"),
                new RadioPreset("99.9", "CKFM"),
                new RadioPreset("102.1", "EDGE"),
                new RadioPreset("104.5", "CHUM"),
                new RadioPreset("107.1", "CILQ"),
                new RadioPreset("88.1", "CKLN")
            ]));
    }
}
