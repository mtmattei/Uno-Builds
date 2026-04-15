using System;
using System.IO;
using System.Threading;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace PrecisionDial.Controls;

/// <summary>
/// Plays a short mechanical click on each dial detent crossing. The click
/// waveform is synthesized in C# (see <see cref="ClickWavSynth"/>), written
/// once to the app's local folder (cross-platform via Uno Storage), and
/// played through a pool of <see cref="MediaPlayer"/> instances so rapid
/// clicks don't cut each other off.
///
/// Failures are silent — if audio init fails the service no-ops so the dial
/// keeps working. Diagnostic errors are stashed on <see cref="LastError"/>
/// for debug access.
/// </summary>
internal sealed class ClickAudioService
{
    private const int PlayerPoolSize = 4;
    private const string WavFileName = "dial-click.wav";

    private readonly MediaPlayer?[] _players = new MediaPlayer?[PlayerPoolSize];
    private int _nextPlayerIndex;
    private bool _ready;

    public string? LastError { get; private set; }

    public async void Prepare()
    {
        if (_ready) return;

        try
        {
            // Write the WAV to the app's LocalFolder once.
            var localFolder = ApplicationData.Current.LocalFolder;
            var wavFile = await localFolder.CreateFileAsync(
                WavFileName,
                CreationCollisionOption.ReplaceExisting);

            var bytes = ClickWavSynth.GenerateMechanicalClick();
            await FileIO.WriteBytesAsync(wavFile, bytes);

            // Uno doesn't implement MediaSource.CreateFromStorageFile on most
            // targets, but CreateFromUri with the ms-appdata:///local/ scheme
            // resolves through the same LocalFolder on every platform.
            var uri = new Uri("ms-appdata:///local/" + WavFileName);

            for (int i = 0; i < PlayerPoolSize; i++)
            {
                var mp = new MediaPlayer
                {
                    Source = MediaSource.CreateFromUri(uri),
                    AutoPlay = false,
                    Volume = 0.85,
                };
                _players[i] = mp;
            }

            _ready = true;
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + ": " + ex.Message;
            _ready = false;
        }
    }

    public void PlayClick()
    {
        if (!_ready) return;

        try
        {
            var idx = Interlocked.Increment(ref _nextPlayerIndex) & (PlayerPoolSize - 1);
            var mp = _players[idx];
            if (mp is null) return;

            mp.PlaybackSession.Position = TimeSpan.Zero;
            mp.Play();
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + ": " + ex.Message;
        }
    }
}
