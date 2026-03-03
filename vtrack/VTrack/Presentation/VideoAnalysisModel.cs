namespace VTrack.Presentation;

public partial record VideoAnalysisModel
{
    private readonly INavigator _navigator;
    private readonly ILogger<VideoAnalysisModel> _logger;
    private readonly VideoFile _videoFile;
    private List<string> _selectedSubjectIds = new();

    public VideoAnalysisModel(
        INavigator navigator,
        ILogger<VideoAnalysisModel> logger,
        VideoFile videoFile)
    {
        _navigator = navigator;
        _logger = logger;
        _videoFile = videoFile;
    }

    public Action? InvalidateOverlay { get; set; }
    public Action? PlayRequested { get; set; }
    public Action? PauseRequested { get; set; }
    public Action<double>? SeekRequested { get; set; }

    public string VideoName => _videoFile.Name;
    public string VideoUrl => _videoFile.VideoUrl;

    // Playback State
    public IState<bool> IsPlaying => State<bool>.Value(this, () => false);
    public IState<double> CurrentPosition => State<double>.Value(this, () => 0);
    public IState<double> VideoDuration => State<double>.Value(this, () => 100);
    public IState<bool> IsMuted => State<bool>.Value(this, () => false);

    // Tracking State
    public IState<string> QueryText => State<string>.Value(this, () => string.Empty);
    public IState<bool> IsProcessing => State<bool>.Value(this, () => false);
    public IState<double> ProcessingProgress => State<double>.Value(this, () => 0);
    public IListState<TrackedSubject> TrackedSubjects => ListState<TrackedSubject>.Empty(this);
    public IListState<BoundingBox> AllBoundingBoxes => ListState<BoundingBox>.Empty(this);

    // Simple computed states
    public IState<bool> HasSubjects => State<bool>.Value(this, () => false);
    public IState<bool> ShowEmptyState => State<bool>.Value(this, () => true);
    public IState<bool> CanStartTracking => State<bool>.Value(this, () => false);
    public IState<string> PlayPauseIcon => State<string>.Value(this, () => "\uE768");
    public IState<string> MuteIcon => State<string>.Value(this, () => "\uE767");
    public IState<int> CurrentFrame => State<int>.Value(this, () => 0);
    public IState<int> TotalFrames => State<int>.Value(this, () => 0);
    public IState<string> CurrentTimeText => State<string>.Value(this, () => "00:00");
    public IState<string> DurationText => State<string>.Value(this, () => "00:00");
    public IState<string> MediaError => State<string>.Value(this, () => string.Empty);

    // Data access for the view
    public IReadOnlyList<TrackedSubject> TrackedSubjectsData { get; private set; } = Array.Empty<TrackedSubject>();
    public IReadOnlyList<BoundingBox> CurrentBoxes { get; private set; } = Array.Empty<BoundingBox>();

    public void SetVideoDuration(double duration)
    {
        _ = VideoDuration.SetAsync(duration, CancellationToken.None);
        _ = TotalFrames.SetAsync((int)(duration * 30), CancellationToken.None);
        _ = DurationText.SetAsync(TimeSpan.FromSeconds(duration).ToString(@"mm\:ss"), CancellationToken.None);
    }

    public void SetMediaError(string errorMessage)
    {
        _logger.LogError("Media playback error: {Error}", errorMessage);
        _ = MediaError.SetAsync(errorMessage, CancellationToken.None);
    }

    public void UpdatePosition(double position)
    {
        _ = CurrentPosition.SetAsync(position, CancellationToken.None);
        _ = CurrentFrame.SetAsync((int)(position * 30), CancellationToken.None);
        _ = CurrentTimeText.SetAsync(TimeSpan.FromSeconds(position).ToString(@"mm\:ss"), CancellationToken.None);
        UpdateCurrentBoxes(position);
    }

    public void UpdateSelectedSubjects(List<string> selectedIds)
    {
        _selectedSubjectIds = selectedIds;
        UpdateCurrentBoxes(0);
    }

    private void UpdateCurrentBoxes(double position)
    {
        var frameRate = 30.0;
        var currentFrame = (int)(position * frameRate);

        CurrentBoxes = TrackedSubjectsData.Count > 0
            ? _allBoxesCache
                .Where(b => b.Frame == currentFrame)
                .Where(b => _selectedSubjectIds.Count == 0 || _selectedSubjectIds.Contains(b.SubjectId))
                .ToList()
            : Array.Empty<BoundingBox>();

        InvalidateOverlay?.Invoke();
    }

    private List<BoundingBox> _allBoxesCache = new();

    public async ValueTask GoBack(CancellationToken ct)
    {
        await _navigator.GoBack(this);
    }

    public async ValueTask PlayPause(CancellationToken ct)
    {
        var isPlaying = await IsPlaying.Value(ct);
        var newIsPlaying = !isPlaying;
        await IsPlaying.SetAsync(newIsPlaying, ct);
        await PlayPauseIcon.SetAsync(newIsPlaying ? "\uE769" : "\uE768", ct);

        if (newIsPlaying)
        {
            PlayRequested?.Invoke();
        }
        else
        {
            PauseRequested?.Invoke();
        }
    }

    public async ValueTask ToggleMute(CancellationToken ct)
    {
        var isMuted = await IsMuted.Value(ct);
        await IsMuted.SetAsync(!isMuted, ct);
        await MuteIcon.SetAsync(isMuted ? "\uE767" : "\uE74F", ct);
    }

    public async ValueTask UpdateQueryText(string text, CancellationToken ct)
    {
        await QueryText.SetAsync(text, ct);
        await CanStartTracking.SetAsync(!string.IsNullOrWhiteSpace(text), ct);
    }

    public async ValueTask StartTracking(CancellationToken ct)
    {
        var query = await QueryText.Value(ct);
        if (string.IsNullOrWhiteSpace(query)) return;

        await IsProcessing.SetAsync(true, ct);
        await ProcessingProgress.SetAsync(0, ct);
        await ShowEmptyState.SetAsync(false, ct);

        try
        {
            _logger.LogInformation("Starting tracking with query: {Query}", query);

            for (int i = 0; i <= 100; i += 5)
            {
                await ProcessingProgress.SetAsync(i, ct);
                await Task.Delay(100, ct);
            }

            var subjects = GenerateMockSubjects(query);
            TrackedSubjectsData = subjects;

            // Update list state using Update method
            await TrackedSubjects.UpdateAsync(_ => subjects.ToImmutableList(), ct);

            var totalFrames = (int)(await VideoDuration.Value(ct) * 30);
            var boxes = GenerateMockBoundingBoxes(subjects, totalFrames);
            _allBoxesCache = boxes;

            await AllBoundingBoxes.UpdateAsync(_ => boxes.ToImmutableList(), ct);

            _selectedSubjectIds = subjects.Select(s => s.Id).ToList();
            await HasSubjects.SetAsync(true, ct);
            UpdateCurrentBoxes(await CurrentPosition.Value(ct));

            _logger.LogInformation("Tracking completed: {SubjectCount} subjects found", subjects.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during tracking");
            await ShowEmptyState.SetAsync(true, ct);
        }
        finally
        {
            await IsProcessing.SetAsync(false, ct);
        }
    }

    private List<TrackedSubject> GenerateMockSubjects(string query)
    {
        var colors = new[] { "#FF5722", "#4CAF50", "#2196F3", "#9C27B0", "#FFC107" };
        var random = new Random();

        return Enumerable.Range(1, random.Next(2, 5))
            .Select(i => new TrackedSubject(
                Id: $"subject-{i}",
                Label: $"{query} #{i}",
                Color: colors[(i - 1) % colors.Length],
                Confidence: 0.7 + random.NextDouble() * 0.25,
                FirstFrame: 0,
                LastFrame: 300))
            .ToList();
    }

    private List<BoundingBox> GenerateMockBoundingBoxes(List<TrackedSubject> subjects, int totalFrames)
    {
        var boxes = new List<BoundingBox>();
        var random = new Random();

        foreach (var subject in subjects)
        {
            var startX = random.NextDouble() * 0.5 + 0.1;
            var startY = random.NextDouble() * 0.5 + 0.1;
            var velocityX = (random.NextDouble() - 0.5) * 0.002;
            var velocityY = (random.NextDouble() - 0.5) * 0.001;

            for (int frame = 0; frame < Math.Min(totalFrames, 300); frame++)
            {
                var x = Math.Clamp(startX + velocityX * frame, 0.05, 0.75);
                var y = Math.Clamp(startY + velocityY * frame, 0.05, 0.75);

                boxes.Add(new BoundingBox(
                    SubjectId: subject.Id,
                    Frame: frame,
                    X: x,
                    Y: y,
                    Width: 0.15 + random.NextDouble() * 0.05,
                    Height: 0.2 + random.NextDouble() * 0.05,
                    Confidence: subject.Confidence - random.NextDouble() * 0.1));
            }
        }

        return boxes;
    }
}
