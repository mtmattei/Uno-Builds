using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InfiniteImage.Models;
using InfiniteImage.Services;
using System.ComponentModel;
using System.Numerics;

namespace InfiniteImage.ViewModels;

/// <summary>
/// ViewModel for the Infinite 3D Canvas.
/// </summary>
[Bindable(true)]
public partial class CanvasViewModel : ObservableObject
{
    private readonly ChunkService _chunkService;
    private readonly ProjectionService _projectionService;
    private readonly PerformanceTelemetry _telemetry;
    private readonly ImageCacheService _imageCacheService;
    private readonly PhotoLibraryService _photoLibraryService;

    [ObservableProperty]
    private Camera _camera = new();

    [ObservableProperty]
    private List<ProjectedPlane> _visiblePlanes = [];

    [ObservableProperty]
    private int _fps;

    [ObservableProperty]
    private int _chunkCount;

    [ObservableProperty]
    private int _planeCount;

    [ObservableProperty]
    private double _viewportWidth = 800;

    [ObservableProperty]
    private double _viewportHeight = 600;

    [ObservableProperty]
    private string _coordX = "0";

    [ObservableProperty]
    private string _coordY = "0";

    [ObservableProperty]
    private string _coordZ = "0";

    [ObservableProperty]
    private string _fpsText = "60 FPS";

    [ObservableProperty]
    private string _chunkCountText = "0";

    [ObservableProperty]
    private string _planeCountText = "0";

    [ObservableProperty]
    private string _depthText = "0";

    [ObservableProperty]
    private double _speedBarWidth;

    [ObservableProperty]
    private double _motionGlowOpacity;

    [ObservableProperty]
    private string _currentDateText = "";

    [ObservableProperty]
    private string _backgroundYear = "";

    [ObservableProperty]
    private double _backgroundYearOpacity = 0.2;

    [ObservableProperty]
    private double _yearScale = 1.0;

    [ObservableProperty]
    private double _yearRotation = 0.0;

    [ObservableProperty]
    private double _yearOffsetX = 0.0;

    [ObservableProperty]
    private double _yearOffsetY = 0.0;

    [ObservableProperty]
    private bool _isLibraryMode;

    [ObservableProperty]
    private bool _isRandomMode = true;

    [ObservableProperty]
    private DateTimeOffset _earliestDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _latestDate = DateTimeOffset.Now;

    [ObservableProperty]
    private float _timelineMaxZ = 1000f;

    [ObservableProperty]
    private float _currentTimelineZ;

    // Input accumulator for smooth scrolling
    private float _scrollAccumulator;
    private readonly HashSet<string> _pressedKeys = [];

    // FPS tracking
    private int _frameCount;
    private DateTime _lastFpsUpdate = DateTime.Now;

    // Memory pressure monitoring
    private int _framesSinceMemoryCheck;
    private const int MemoryCheckFrameInterval = 300; // Every 5 seconds at 60fps

    // HUD update throttling
    private int _framesSinceHudUpdate;
    private const int HudUpdateFrameInterval = 3; // Update HUD every 3 frames (~20fps)

    public CanvasViewModel(
        ChunkService chunkService,
        ProjectionService projectionService,
        PerformanceTelemetry telemetry,
        ImageCacheService imageCacheService,
        PhotoLibraryService photoLibraryService)
    {
        _chunkService = chunkService;
        _projectionService = projectionService;
        _telemetry = telemetry;
        _imageCacheService = imageCacheService;
        _photoLibraryService = photoLibraryService;

        // Load library on startup
        _ = LoadLibraryOnStartupAsync();
    }

    private async Task LoadLibraryOnStartupAsync()
    {
        var library = await _photoLibraryService.LoadLibraryAsync();
        if (library != null)
        {
            IsLibraryMode = true;
            IsRandomMode = false;

            // Update timeline properties
            EarliestDate = library.EarliestDate;
            LatestDate = library.LatestDate;
            TimelineMaxZ = TimelineConfig.CalculateZForDate(library.LatestDate, library.EarliestDate);

            // Jump to earliest date
            Camera.SetPosition(0, 0, 0);
        }
    }

    public async Task LoadPhotoLibraryAsync(Window? window = null)
    {
        var library = await _photoLibraryService.SelectAndScanFolderAsync(window);
        if (library != null && library.TotalPhotos > 0)
        {
            IsLibraryMode = true;
            IsRandomMode = false;

            // Update timeline properties
            EarliestDate = library.EarliestDate;
            LatestDate = library.LatestDate;
            TimelineMaxZ = TimelineConfig.CalculateZForDate(library.LatestDate, library.EarliestDate);

            // Clear chunk cache to regenerate with new mode
            _chunkService.ClearCache();

            // Jump to earliest date
            Camera.SetPosition(0, 0, 0);
        }
    }

    /// <summary>
    /// Set camera Z position from timeline scrubber
    /// </summary>
    public void SetTimelineZ(float z)
    {
        Camera.SetPosition(Camera.Position.X, Camera.Position.Y, z);
    }

    /// <summary>
    /// Updates the canvas state with frame rate limiting.
    /// </summary>
    public void Update()
    {
        _telemetry.BeginFrame();

        ProcessKeyboardInput();
        ProcessScrollInput();

        Camera.Update();

        UpdateVisiblePlanes();

        // Throttle HUD updates to reduce string allocations
        _framesSinceHudUpdate++;
        if (_framesSinceHudUpdate >= HudUpdateFrameInterval)
        {
            _framesSinceHudUpdate = 0;
            UpdateHudProperties();
        }

        UpdateFps();

        CheckMemoryPressure();

        _telemetry.EndFrame();
    }

    private void ProcessKeyboardInput()
    {
        float dx = 0, dy = 0, dz = 0;

        // WinUI uses VirtualKey enum names
        if (_pressedKeys.Contains("W") || _pressedKeys.Contains("Up"))
            dy -= CanvasConfig.KeyboardSpeedXY;
        if (_pressedKeys.Contains("S") || _pressedKeys.Contains("Down"))
            dy += CanvasConfig.KeyboardSpeedXY;
        if (_pressedKeys.Contains("A") || _pressedKeys.Contains("Left"))
            dx -= CanvasConfig.KeyboardSpeedXY;
        if (_pressedKeys.Contains("D") || _pressedKeys.Contains("Right"))
            dx += CanvasConfig.KeyboardSpeedXY;

        if (_pressedKeys.Contains("E") || _pressedKeys.Contains("Space"))
            dz += CanvasConfig.KeyboardSpeedZ;
        if (_pressedKeys.Contains("Q") || _pressedKeys.Contains("LeftShift") || _pressedKeys.Contains("RightShift"))
            dz -= CanvasConfig.KeyboardSpeedZ;

        if (dx != 0 || dy != 0 || dz != 0)
        {
            Camera.AddInput(dx, dy, dz);
        }
    }

    private void ProcessScrollInput()
    {
        if (Math.Abs(_scrollAccumulator) > 0.01f)
        {
            Camera.AddInput(0, 0, _scrollAccumulator);
            _scrollAccumulator *= 0.85f; // Decay
        }
    }

    private void UpdateVisiblePlanes()
    {
        var (cx, cy, cz) = Camera.ChunkCoords;
        var chunks = _chunkService.GetActiveChunks(cx, cy, cz);

        VisiblePlanes = _projectionService.ProjectPlanes(
            chunks, Camera, ViewportWidth, ViewportHeight, out var usedCache);

        ChunkCount = _chunkService.CachedChunkCount;
        PlaneCount = VisiblePlanes.Count;

        // Update telemetry
        _telemetry.VisiblePlanes = PlaneCount;
        _telemetry.UsedCachedProjection = usedCache;
    }

    private void UpdateHudProperties()
    {
        // Round before converting to reduce string allocations
        var roundedX = (int)Camera.Position.X;
        var roundedY = (int)Camera.Position.Y;
        var roundedZ = (int)Camera.Position.Z;

        // Only allocate strings if values changed
        if (CoordX != roundedX.ToString())
            CoordX = roundedX.ToString();

        if (CoordY != roundedY.ToString())
            CoordY = roundedY.ToString();

        if (CoordZ != roundedZ.ToString())
            CoordZ = roundedZ.ToString();

        // Update stat text
        if (ChunkCountText != ChunkCount.ToString())
            ChunkCountText = ChunkCount.ToString();

        if (PlaneCountText != PlaneCount.ToString())
            PlaneCountText = PlaneCount.ToString();

        if (DepthText != roundedZ.ToString())
            DepthText = roundedZ.ToString();

        // Speed bar (240px max width)
        var speed = Math.Min(Camera.Velocity.Length(), 100);
        SpeedBarWidth = speed * 2.4;

        // Motion glow based on Z velocity
        var zSpeed = Math.Abs(Camera.Velocity.Z);
        MotionGlowOpacity = Math.Min(zSpeed / 50, 0.5);

        // Update background year display - always use library dates if available
        if (_photoLibraryService.CurrentLibrary != null && _photoLibraryService.CurrentLibrary.TotalPhotos > 0)
        {
            // In library mode, show date in top panel
            if (IsLibraryMode)
            {
                CurrentTimelineZ = Camera.Position.Z;
                var currentDate = TimelineConfig.CalculateDateForZ(
                    Camera.Position.Z,
                    _photoLibraryService.CurrentLibrary.EarliestDate);
                CurrentDateText = currentDate.ToString("yyyy");

                // Update background year from actual photo dates
                var currentYear = currentDate.Year.ToString();
                if (BackgroundYear != currentYear)
                {
                    BackgroundYear = currentYear;
                }
            }
            else
            {
                // Even in random mode with a library loaded, use actual dates
                var currentDate = TimelineConfig.CalculateDateForZ(
                    Camera.Position.Z,
                    _photoLibraryService.CurrentLibrary.EarliestDate);
                var currentYear = currentDate.Year.ToString();
                if (BackgroundYear != currentYear)
                {
                    BackgroundYear = currentYear;
                }
            }

            // Dynamic transformations based on camera movement
            var yearZSpeed = Math.Abs(Camera.Velocity.Z);
            var totalSpeed = Camera.Velocity.Length();

            // Opacity: more visible when moving through time
            var targetOpacity = yearZSpeed > 5 ? 0.35 : 0.18;
            BackgroundYearOpacity = BackgroundYearOpacity * 0.88 + targetOpacity * 0.12;

            // Scale: subtle pulse based on Z velocity
            var targetScale = 1.0 + (yearZSpeed * 0.008);
            targetScale = Math.Min(targetScale, 1.15);
            YearScale = YearScale * 0.85 + targetScale * 0.15;

            // Rotation: subtle tilt based on XY velocity
            var targetRotation = Camera.Velocity.X * -0.5;
            targetRotation = Math.Clamp(targetRotation, -3.0, 3.0);
            YearRotation = YearRotation * 0.92 + targetRotation * 0.08;

            // Offset: subtle parallax effect based on camera position and velocity
            var targetOffsetX = Camera.Velocity.X * -2.0;
            var targetOffsetY = Camera.Velocity.Y * -2.0;
            targetOffsetX = Math.Clamp(targetOffsetX, -40.0, 40.0);
            targetOffsetY = Math.Clamp(targetOffsetY, -40.0, 40.0);
            YearOffsetX = YearOffsetX * 0.9 + targetOffsetX * 0.1;
            YearOffsetY = YearOffsetY * 0.9 + targetOffsetY * 0.1;
        }
        else
        {
            // No library loaded - hide the year
            BackgroundYearOpacity = BackgroundYearOpacity * 0.85;
            YearScale = YearScale * 0.9 + 1.0 * 0.1;
            YearRotation = YearRotation * 0.9;
            YearOffsetX = YearOffsetX * 0.9;
            YearOffsetY = YearOffsetY * 0.9;
        }
    }

    partial void OnCurrentTimelineZChanged(float value)
    {
        // When timeline scrubber changes, update camera position
        if (IsLibraryMode && Math.Abs(Camera.Position.Z - value) > 0.1f)
        {
            Camera.SetPosition(Camera.Position.X, Camera.Position.Y, value);
        }
    }

    private void UpdateFps()
    {
        _frameCount++;
        var now = DateTime.Now;
        if ((now - _lastFpsUpdate).TotalMilliseconds >= 1000)
        {
            Fps = _frameCount;
            FpsText = $"{Fps} FPS";

            _frameCount = 0;
            _lastFpsUpdate = now;

            // Update telemetry from image cache
            _telemetry.CachedImages = _imageCacheService.CachedImageCount;
            _telemetry.ImageCacheMemoryBytes = _imageCacheService.TotalMemoryBytes;
        }
    }

    private void CheckMemoryPressure()
    {
        _framesSinceMemoryCheck++;

        if (_framesSinceMemoryCheck >= MemoryCheckFrameInterval)
        {
            _framesSinceMemoryCheck = 0;

            // Check if we're over 80% of budget
            if (_imageCacheService.TotalMemoryBytes > CanvasConfig.MaxImageCacheMemoryBytes * 0.8)
            {
                // Run memory cleanup on background thread to avoid blocking UI
                _ = Task.Run(() => _imageCacheService.OnMemoryPressure());
            }
        }
    }

    /// <summary>
    /// Handle key press.
    /// </summary>
    public void OnKeyDown(string key)
    {
        if (key == "R")
        {
            ResetCamera();
            return;
        }
        _pressedKeys.Add(key);
    }

    /// <summary>
    /// Handle key release.
    /// </summary>
    public void OnKeyUp(string key)
    {
        _pressedKeys.Remove(key);
    }

    /// <summary>
    /// Handle mouse/touch drag.
    /// </summary>
    public void OnPan(double deltaX, double deltaY)
    {
        Camera.AddInput(
            -(float)deltaX * CanvasConfig.PanSensitivity,
            -(float)deltaY * CanvasConfig.PanSensitivity,
            0);
    }

    /// <summary>
    /// Handle scroll wheel.
    /// </summary>
    public void OnScroll(double deltaY)
    {
        _scrollAccumulator += (float)deltaY * CanvasConfig.ZoomSensitivity;
    }

    /// <summary>
    /// Handle pinch gesture (touch).
    /// </summary>
    public void OnPinch(double scaleDelta)
    {
        // Pinch in = fly forward, pinch out = fly backward
        var zDelta = (1 - scaleDelta) * 50;
        Camera.AddInput(0, 0, (float)zDelta);
    }

    [RelayCommand]
    private void ResetCamera()
    {
        Camera.Reset();
    }

    /// <summary>
    /// Update viewport dimensions.
    /// </summary>
    public void SetViewport(double width, double height)
    {
        ViewportWidth = width;
        ViewportHeight = height;
        _projectionService.MarkDirty();
    }

    /// <summary>
    /// Get image cache service for use by controls.
    /// </summary>
    public ImageCacheService ImageCache => _imageCacheService;

    /// <summary>
    /// Get performance telemetry.
    /// </summary>
    public PerformanceTelemetry Telemetry => _telemetry;
}
