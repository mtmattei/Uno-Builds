namespace VTrack.Presentation;

public partial record HomeModel
{
    private readonly INavigator _navigator;
    private readonly ILogger<HomeModel> _logger;

    public HomeModel(INavigator navigator, ILogger<HomeModel> logger)
    {
        _navigator = navigator;
        _logger = logger;
    }

    public IState<bool> IsUploading => State<bool>.Value(this, () => false);
    public IState<string> UploadFileName => State<string>.Value(this, () => string.Empty);
    public IState<double> UploadProgress => State<double>.Value(this, () => 0);
    public IListState<VideoFile> RecentVideos => ListState<VideoFile>.Empty(this);
    public IState<bool> HasRecentVideos => State<bool>.Value(this, () => false);

    public async ValueTask BrowseFiles(CancellationToken ct)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await ProcessVideoFile(file, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking file");
        }
    }

    public async Task HandleDroppedFile(StorageFile file)
    {
        await ProcessVideoFile(file, CancellationToken.None);
    }

    private async Task ProcessVideoFile(StorageFile file, CancellationToken ct)
    {
        await IsUploading.SetAsync(true, ct);
        await UploadFileName.SetAsync(file.Name, ct);
        await UploadProgress.SetAsync(0, ct);

        try
        {
            for (int i = 0; i <= 100; i += 10)
            {
                await UploadProgress.SetAsync(i, ct);
                await Task.Delay(100, ct);
            }

            var videoFile = new VideoFile(
                Id: Guid.NewGuid().ToString(),
                Name: file.Name,
                Duration: 0,
                ThumbnailUrl: null,
                VideoUrl: file.Path,
                UploadedAt: DateTime.UtcNow);

            await _navigator.NavigateViewModelAsync<VideoAnalysisModel>(this, data: videoFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video file");
        }
        finally
        {
            await IsUploading.SetAsync(false, ct);
        }
    }

    public async ValueTask OpenVideo(VideoFile video, CancellationToken ct)
    {
        await _navigator.NavigateViewModelAsync<VideoAnalysisModel>(this, data: video);
    }
}
