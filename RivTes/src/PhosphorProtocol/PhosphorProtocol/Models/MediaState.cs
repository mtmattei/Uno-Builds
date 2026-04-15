namespace PhosphorProtocol.Models;

public record MediaState(
    string Source,
    string TrackTitle,
    string Artist,
    TimeSpan Duration,
    TimeSpan Position,
    bool IsPlaying,
    ImmutableList<RadioPreset> Presets);

public record RadioPreset(string Frequency, string StationName);
