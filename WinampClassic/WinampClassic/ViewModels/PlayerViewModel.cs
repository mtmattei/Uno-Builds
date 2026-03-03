using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using WinampClassic.Models;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace WinampClassic.ViewModels;

public partial class PlayerViewModel : ObservableObject, IDisposable
{
    private MediaPlayer? _mediaPlayer;
    private DispatcherTimer? _positionTimer;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _disposed;

    [ObservableProperty]
    private Track? _currentTrack;

    [ObservableProperty]
    private TimeSpan _currentPosition;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private double _volume = 0.75;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isStopped = true;

    [ObservableProperty]
    private bool _showElapsedTime = true;

    [ObservableProperty]
    private double _seekPosition;

    public string TimeDisplay
    {
        get
        {
            var time = ShowElapsedTime ? CurrentPosition : Duration - CurrentPosition;
            var prefix = ShowElapsedTime ? "" : "-";
            return $"{prefix}{(int)time.TotalMinutes}:{time.Seconds:D2}";
        }
    }

    public string PlaybackStatusIcon
    {
        get
        {
            if (IsPlaying) return "\u25B6";
            if (IsPaused) return "\u23F8";
            return "\u23F9";
        }
    }

    public PlayerViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        InitializeMediaPlayer();
        InitializeTimers();
    }

    private void InitializeMediaPlayer()
    {
        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.Volume = Volume;
        _mediaPlayer.MediaEnded += OnMediaEnded;
        _mediaPlayer.MediaOpened += OnMediaOpened;
    }

    private void InitializeTimers()
    {
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _positionTimer.Tick += OnPositionTimerTick;
    }

    private void OnPositionTimerTick(object? sender, object e)
    {
        if (_mediaPlayer?.PlaybackSession != null)
        {
            CurrentPosition = _mediaPlayer.PlaybackSession.Position;
            if (Duration.TotalSeconds > 0)
            {
                SeekPosition = CurrentPosition.TotalSeconds / Duration.TotalSeconds * 100;
            }
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }

    private void OnMediaEnded(MediaPlayer sender, object args)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Stop();
        });
    }

    private void OnMediaOpened(MediaPlayer sender, object args)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (_mediaPlayer?.PlaybackSession != null)
            {
                Duration = _mediaPlayer.PlaybackSession.NaturalDuration;
                OnPropertyChanged(nameof(TimeDisplay));
            }
        });
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".mp3");
        picker.FileTypeFilter.Add(".wav");
        picker.FileTypeFilter.Add(".flac");
        picker.FileTypeFilter.Add(".m4a");
        picker.FileTypeFilter.Add(".ogg");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await LoadFileAsync(file);
        }
    }

    public async Task LoadFileAsync(StorageFile file)
    {
        var track = new Track
        {
            FilePath = file.Path,
            Title = Path.GetFileNameWithoutExtension(file.Name),
            Artist = "",
            Bitrate = 0,
            SampleRate = 44100,
            IsStereo = true
        };

        // Default bitrate when metadata unavailable
        if (track.Bitrate == 0)
            track.Bitrate = 128;

        // Use absolute file URI to avoid double-encoding of special characters
        var uri = new Uri($"file:///{file.Path.Replace('\\', '/')}", UriKind.Absolute);
        var source = MediaSource.CreateFromUri(uri);

        CurrentTrack = track;
        _mediaPlayer!.Source = source;

        Stop();
        OnPropertyChanged(nameof(TimeDisplay));
    }

    [RelayCommand]
    private void Play()
    {
        if (_mediaPlayer?.Source == null) return;

        _mediaPlayer.Play();
        IsPlaying = true;
        IsPaused = false;
        IsStopped = false;
        _positionTimer?.Start();
        OnPropertyChanged(nameof(PlaybackStatusIcon));
    }

    [RelayCommand]
    private void Pause()
    {
        if (!IsPlaying) return;

        _mediaPlayer?.Pause();
        IsPlaying = false;
        IsPaused = true;
        _positionTimer?.Stop();
        OnPropertyChanged(nameof(PlaybackStatusIcon));
    }

    [RelayCommand]
    private void Stop()
    {
        _mediaPlayer?.Pause();
        if (_mediaPlayer?.PlaybackSession != null)
        {
            _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        }
        IsPlaying = false;
        IsPaused = false;
        IsStopped = true;
        CurrentPosition = TimeSpan.Zero;
        SeekPosition = 0;
        _positionTimer?.Stop();
        OnPropertyChanged(nameof(PlaybackStatusIcon));
        OnPropertyChanged(nameof(TimeDisplay));
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }

    [RelayCommand]
    private void Previous()
    {
        if (_mediaPlayer?.PlaybackSession != null)
        {
            _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            CurrentPosition = TimeSpan.Zero;
            SeekPosition = 0;
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }

    [RelayCommand]
    private void Next()
    {
        Stop();
    }

    [RelayCommand]
    private void ToggleTimeDisplay()
    {
        ShowElapsedTime = !ShowElapsedTime;
        OnPropertyChanged(nameof(TimeDisplay));
    }

    [RelayCommand]
    private void SeekBackward()
    {
        if (_mediaPlayer?.PlaybackSession != null)
        {
            var newPosition = CurrentPosition - TimeSpan.FromSeconds(5);
            if (newPosition < TimeSpan.Zero) newPosition = TimeSpan.Zero;
            _mediaPlayer.PlaybackSession.Position = newPosition;
        }
    }

    [RelayCommand]
    private void SeekForward()
    {
        if (_mediaPlayer?.PlaybackSession != null)
        {
            var newPosition = CurrentPosition + TimeSpan.FromSeconds(5);
            if (newPosition > Duration) newPosition = Duration;
            _mediaPlayer.PlaybackSession.Position = newPosition;
        }
    }

    partial void OnVolumeChanged(double value)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Volume = value;
        }
    }

    public void SeekToPosition(double percentage)
    {
        if (_mediaPlayer?.PlaybackSession != null && Duration.TotalSeconds > 0)
        {
            var newPosition = TimeSpan.FromSeconds(Duration.TotalSeconds * percentage / 100);
            _mediaPlayer.PlaybackSession.Position = newPosition;
        }
    }

    public void VolumeUp()
    {
        Volume = Math.Min(1.0, Volume + 0.05);
    }

    public void VolumeDown()
    {
        Volume = Math.Max(0.0, Volume - 0.05);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _positionTimer?.Stop();

        if (_mediaPlayer != null)
        {
            _mediaPlayer.MediaEnded -= OnMediaEnded;
            _mediaPlayer.MediaOpened -= OnMediaOpened;
            _mediaPlayer.Dispose();
            _mediaPlayer = null;
        }
    }
}
