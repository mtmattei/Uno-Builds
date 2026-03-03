using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace VTrack.Presentation;

public sealed partial class VideoAnalysisPage : Page
{
    private Windows.Media.Playback.MediaPlayer? _mediaPlayer;
    private VideoAnalysisModel? _model;

    public VideoAnalysisPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _model = DataContext as VideoAnalysisModel;
        if (_model != null)
        {
            _model.InvalidateOverlay = () => BoundingBoxCanvas.Invalidate();
            _model.PlayRequested = () => MediaPlayer.MediaPlayer?.Play();
            _model.PauseRequested = () => MediaPlayer.MediaPlayer?.Pause();
            _model.SeekRequested = (position) =>
            {
                var mp = MediaPlayer.MediaPlayer;
                if (mp != null)
                    mp.PlaybackSession.Position = TimeSpan.FromSeconds(position);
            };
            InitializeMediaPlayer();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CleanupMediaPlayer();
    }

    private void InitializeMediaPlayer()
    {
        var videoUrl = _model?.VideoUrl;
        System.Diagnostics.Debug.WriteLine($"InitializeMediaPlayer called with URL: {videoUrl}");

        if (string.IsNullOrEmpty(videoUrl))
        {
            System.Diagnostics.Debug.WriteLine("Video URL is null or empty");
            return;
        }

        try
        {
            Uri mediaUri;

            // Check if it's already a URI or a local file path
            if (Uri.TryCreate(videoUrl, UriKind.Absolute, out var parsedUri) &&
                (parsedUri.Scheme == "http" || parsedUri.Scheme == "https" || parsedUri.Scheme == "file"))
            {
                mediaUri = parsedUri;
                System.Diagnostics.Debug.WriteLine($"Using existing URI: {mediaUri}");
            }
            else if (File.Exists(videoUrl))
            {
                // Convert local file path to file:// URI
                // VLC on Windows expects file:/// with forward slashes
                var normalizedPath = videoUrl.Replace("\\", "/");
                mediaUri = new Uri($"file:///{normalizedPath}");
                System.Diagnostics.Debug.WriteLine($"Converted local path to URI: {mediaUri}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"File does NOT exist: {videoUrl}");
                _model?.SetMediaError($"File not found: {videoUrl}");
                return;
            }

            // Create MediaSource from URI - the only supported method in Uno Platform
            var mediaSource = MediaSource.CreateFromUri(mediaUri);
            MediaPlayer.Source = mediaSource;
            MediaPlayer.AutoPlay = true;

            System.Diagnostics.Debug.WriteLine("MediaPlayerElement source set via CreateFromUri");

            // Delay event attachment to allow MediaPlayer to be created
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(500);
                _mediaPlayer = MediaPlayer.MediaPlayer;
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.PlaybackSession.PositionChanged += OnPositionChanged;
                    _mediaPlayer.MediaOpened += OnMediaOpened;
                    _mediaPlayer.MediaFailed += OnMediaFailed;
                    System.Diagnostics.Debug.WriteLine("MediaPlayer events attached after delay");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing media player: {ex.Message}\n{ex.StackTrace}");
            _model?.SetMediaError($"Error: {ex.Message}");
        }
    }

    private void OnMediaFailed(Windows.Media.Playback.MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Media failed: {args.Error} - {args.ErrorMessage}");
            _model?.SetMediaError($"Failed to load video: {args.ErrorMessage}");
        });
    }

    private void CleanupMediaPlayer()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.PlaybackSession.PositionChanged -= OnPositionChanged;
            _mediaPlayer.MediaOpened -= OnMediaOpened;
            _mediaPlayer.MediaFailed -= OnMediaFailed;
            _mediaPlayer = null;
        }
        MediaPlayer.Source = null;
    }

    private void OnMediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
    {
        System.Diagnostics.Debug.WriteLine($"Media opened! Duration: {sender.PlaybackSession.NaturalDuration.TotalSeconds}s");
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_model != null && sender.PlaybackSession.NaturalDuration.TotalSeconds > 0)
            {
                _model.SetVideoDuration(sender.PlaybackSession.NaturalDuration.TotalSeconds);
                System.Diagnostics.Debug.WriteLine("Video duration set in model");
            }
        });
    }

    private void OnPositionChanged(MediaPlaybackSession sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_model != null)
            {
                _model.UpdatePosition(sender.Position.TotalSeconds);
                BoundingBoxCanvas.Invalidate();
            }
        });
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (_model?.CurrentBoxes is not { } boxes || boxes.Count == 0)
            return;

        var subjects = _model.TrackedSubjectsData;
        var canvasWidth = e.Info.Width;
        var canvasHeight = e.Info.Height;

        foreach (var box in boxes)
        {
            var subject = subjects.FirstOrDefault(s => s.Id == box.SubjectId);
            if (subject == null) continue;

            // Parse color from hex
            var color = SKColor.Parse(subject.Color);

            // Calculate actual pixel coordinates
            var x = (float)(box.X * canvasWidth);
            var y = (float)(box.Y * canvasHeight);
            var width = (float)(box.Width * canvasWidth);
            var height = (float)(box.Height * canvasHeight);

            // Draw bounding box
            using var strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = 3,
                IsAntialias = true
            };
            canvas.DrawRect(x, y, width, height, strokePaint);

            // Draw label background
            var labelText = $"{subject.Label} ({box.Confidence:P0})";
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 14,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
            };

            var textBounds = new SKRect();
            textPaint.MeasureText(labelText, ref textBounds);

            using var bgPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color.WithAlpha(200)
            };

            var labelRect = new SKRect(x, y - textBounds.Height - 8, x + textBounds.Width + 12, y);
            canvas.DrawRoundRect(labelRect, 4, 4, bgPaint);

            // Draw label text
            canvas.DrawText(labelText, x + 6, y - 6, textPaint);
        }
    }

    private void OnSubjectSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_model != null && sender is ListView listView)
        {
            var selectedIds = listView.SelectedItems
                .OfType<TrackedSubject>()
                .Select(s => s.Id)
                .ToList();
            _model.UpdateSelectedSubjects(selectedIds);
            BoundingBoxCanvas.Invalidate();
        }
    }

    public void PlayVideo() => MediaPlayer.MediaPlayer?.Play();
    public void PauseVideo() => MediaPlayer.MediaPlayer?.Pause();
    public void SeekTo(TimeSpan position)
    {
        var mp = MediaPlayer.MediaPlayer;
        if (mp != null)
            mp.PlaybackSession.Position = position;
    }
}
