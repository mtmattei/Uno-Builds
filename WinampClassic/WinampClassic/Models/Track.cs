namespace WinampClassic.Models;

public class Track
{
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int Bitrate { get; set; }
    public int SampleRate { get; set; }
    public bool IsStereo { get; set; } = true;

    public string DisplayName => string.IsNullOrEmpty(Artist)
        ? Title
        : $"{Artist} - {Title}";

    public string BitrateDisplay => $"{Bitrate}";
    public string SampleRateDisplay => $"{SampleRate / 1000}";
    public string ChannelModeDisplay => IsStereo ? "STEREO" : "MONO";
}
