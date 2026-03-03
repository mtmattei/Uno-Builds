using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using UnoVox.Models;
using UnoVox.Services;

namespace UnoVox.Presentation;

public partial class VoxelEditorViewModel : ObservableObject, IDisposable
{
    private readonly VoxelGrid _voxelGrid;
    private readonly CameraController _camera;
    private readonly VoxelRenderer _renderer;
    private readonly UndoStack _undoStack;
    private readonly RayCaster _rayCaster;
    private readonly IProjectFileService _fileService;
    private readonly IWebcamService _webcamService;
    private readonly IHandTracker _handTracker;
    private readonly IRoboflowGestureClassifier _roboflowClassifier;
    private readonly GestureDetector _gestureDetector;
    private readonly GestureStateMachine _gestureStateMachine;
    private readonly HandToVoxelMapper _handMapper;
    private readonly LandmarkSmoother _landmarkSmoother;
    private readonly ThreadSafeSKBitmapPool _bitmapPool;
    private readonly ILogger<VoxelEditorViewModel>? _logger;

    // Mouse tracking
    private bool _isLeftMouseDown;
    private bool _isRightMouseDown;
    private bool _isMiddleMouseDown;
    private Windows.Foundation.Point _lastMousePosition;
    
    // Canvas dimensions
    private float _canvasWidth = 800;
    private float _canvasHeight = 600;

    // File tracking
    private Windows.Storage.StorageFile? _currentFile;
    private ProjectMetadata? _projectMetadata;
    
    [ObservableProperty]
    private bool _isDirty;
    
    [ObservableProperty]
    private string _projectName = "Untitled";
    
    // Current state
    [ObservableProperty]
    private string _currentColor = "#FF0000";

    public Windows.UI.Color SelectedColor => ParseColor(CurrentColor);

    private static Windows.UI.Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
            return Windows.UI.Color.FromArgb(255,
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16));
        return Windows.UI.Color.FromArgb(255, 255, 0, 0);
    }

    partial void OnCurrentColorChanged(string value) => OnPropertyChanged(nameof(SelectedColor));
    
    [ObservableProperty]
    private bool _canUndo;
    
    [ObservableProperty]
    private bool _canRedo;
    
    [ObservableProperty]
    private string _cursorPosition = "0, 0, 0";
    
    [ObservableProperty]
    private string _currentTool = "Place Mode";
    
    [ObservableProperty]
    private int _fps = 0;
    
    [ObservableProperty]
    private List<Windows.UI.Color> _colorPalette;

    [ObservableProperty]
    private (int x, int y, int z)? _cursorVoxel;
    
    [ObservableProperty]
    private (int x, int y, int z)? _selectedVoxel;

    // Webcam
    [ObservableProperty]
    private bool _isWebcamActive;
    
    [ObservableProperty]
    private string _webcamStatus = "Not started";
    
    [ObservableProperty]
    private string _webcamButtonText = "Start Camera";

    [ObservableProperty]
    private List<string> _availableCameras = new();

    [ObservableProperty]
    private int _selectedCameraIndex = 0;

    private SKXamlCanvas? _canvas;
    private SKBitmap? _currentWebcamFrame;
    private Microsoft.UI.Xaml.DispatcherTimer? _renderTimer;
    private int _frameCount;
    private int _webcamFrameCount;
    private DateTime _lastFpsUpdate = DateTime.Now;
    private Microsoft.UI.Dispatching.DispatcherQueue? _dispatcher;
    
    // Hand tracking
    [ObservableProperty]
    private bool _isHandTrackingEnabled;

    [ObservableProperty]
    private string _handTrackingStatus = "Idle";

    [ObservableProperty]
    private int _handCount = 0;

    [ObservableProperty]
    private string _currentGesture = "None";

    // Roboflow cloud gesture classification
    [ObservableProperty]
    private bool _isRoboflowEnabled;

    [ObservableProperty]
    private string _roboflowStatus = "Not configured";

    [ObservableProperty]
    private string _roboflowGesture = "None";

    [ObservableProperty]
    private long _roboflowLatencyMs;

    private int _voxelSize = 8;
    public int VoxelSize
    {
        get => _voxelSize;
        set
        {
            if (SetProperty(ref _voxelSize, value))
            {
                _renderer.VoxelSize = value;
                InvalidateCanvas();
            }
        }
    }

    private IReadOnlyList<HandDetection>? _latestHandDetections;
    private DateTime _lastHandProcessTime = DateTime.MinValue;
    private int _isProcessingHands; // 0 = false, 1 = true (Interlocked)
    private CancellationTokenSource? _handCts;
    
    // Two-hand navigation state
    private (float x, float y)? _lastTwoHandCenter;
    private float? _lastTwoHandDistance;
    
    // Resize gesture state
    private float? _lastResizeSpread;
    private const int MinVoxelSize = 4;
    private const int MaxVoxelSize = 24;
    
    // AR placement mode - draw on flat plane in front of camera
    private const float ArPlaneDepth = 16f; // Fixed depth in front of camera
    
    // Gesture action tracking
    private GestureType _lastGestureType = GestureType.None;
    private int _currentColorIndex = 0;

    // Extrude tracking (for continuous voxel placement while dragging)
    private (int x, int y, int z)? _lastPlacedVoxel;

    public VoxelEditorViewModel(
        IProjectFileService fileService,
        IWebcamService webcamService,
        IHandTracker handTracker,
        IRoboflowGestureClassifier roboflowClassifier,
        GestureDetector gestureDetector,
        GestureStateMachine gestureStateMachine,
        HandToVoxelMapper handMapper,
        LandmarkSmoother landmarkSmoother,
        ILogger<VoxelEditorViewModel>? logger = null)
    {
        _voxelGrid = new VoxelGrid(32);
        _camera = new CameraController();
        _renderer = new VoxelRenderer();
        _undoStack = new UndoStack();
        _rayCaster = new RayCaster();
        _fileService = fileService;
        _webcamService = webcamService;
        _handTracker = handTracker;
        _roboflowClassifier = roboflowClassifier;
        _gestureDetector = gestureDetector;
        _gestureStateMachine = gestureStateMachine;
        _handMapper = handMapper;
        _landmarkSmoother = landmarkSmoother;
        _logger = logger;
        _bitmapPool = new ThreadSafeSKBitmapPool(maxPoolSize: 8);

        // Store dispatcher for UI thread marshaling
        // Try GetForCurrentThread first (works if on UI thread), fallback to MainWindow
        _dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                      ?? App.MainWindow?.DispatcherQueue;

        // Enable hand tracking by default so users see tracking without toggling
        IsHandTrackingEnabled = true;

        // Initialize hand tracker (async, non-blocking with error logging)
        SafeFireAndForget(InitializeHandTrackerAsync);

        // Initialize Roboflow classifier (async, non-blocking with error logging)
        SafeFireAndForget(InitializeRoboflowAsync);

        // Enumerate available cameras (async, non-blocking with error logging)
        SafeFireAndForget(EnumerateCamerasAsync);
        
        _webcamService.FrameCaptured += OnWebcamFrameCaptured;
        
        // Initialize default color palette
        _colorPalette = new List<Windows.UI.Color>
        {
            Windows.UI.Color.FromArgb(255, 255, 0, 0),     // Red
            Windows.UI.Color.FromArgb(255, 0, 255, 0),     // Green
            Windows.UI.Color.FromArgb(255, 0, 0, 255),     // Blue
            Windows.UI.Color.FromArgb(255, 255, 255, 0),   // Yellow
            Windows.UI.Color.FromArgb(255, 255, 0, 255),   // Magenta
            Windows.UI.Color.FromArgb(255, 0, 255, 255),   // Cyan
            Windows.UI.Color.FromArgb(255, 255, 128, 0),   // Orange
            Windows.UI.Color.FromArgb(255, 128, 0, 255),   // Purple
            Windows.UI.Color.FromArgb(255, 255, 255, 255), // White
            Windows.UI.Color.FromArgb(255, 128, 128, 128), // Gray
            Windows.UI.Color.FromArgb(255, 64, 64, 64),    // Dark Gray
            Windows.UI.Color.FromArgb(255, 0, 128, 0),     // Dark Green
        };

        // Start render loop at 60 FPS (will reduce to 30 FPS when hand tracking is active)
        _renderTimer = new Microsoft.UI.Xaml.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
        };
        _renderTimer.Tick += (s, e) => InvalidateCanvas();
        _renderTimer.Start();

        // Don't place test voxels - start with empty grid
        // PlaceTestVoxels();
    }

    private void PlaceTestVoxels()
    {
        // Place a few voxels to demonstrate the system
        _voxelGrid.PlaceVoxel(16, 16, 16, "#FF0000"); // Red center
        _voxelGrid.PlaceVoxel(17, 16, 16, "#00FF00"); // Green
        _voxelGrid.PlaceVoxel(16, 17, 16, "#0000FF"); // Blue
        _voxelGrid.PlaceVoxel(16, 16, 17, "#FFFF00"); // Yellow
        _voxelGrid.PlaceVoxel(18, 16, 16, "#FF00FF"); // Magenta
    }

    public void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_canvas == null && sender is SKXamlCanvas canvas)
        {
            _canvas = canvas;
        }

        var surface = e.Surface;
        var canvas2 = surface.Canvas;
        var info = e.Info;
        
        // Clear with black background (or camera feed if available)
        canvas2.Clear(SKColors.Black);
        
        // Draw camera feed first (if active and available)
        if (IsWebcamActive)
        {
            SKBitmap? frameToDraw = null;
            lock (_frameLock)
            {
                frameToDraw = _currentWebcamFrame;
            }

            if (frameToDraw != null && frameToDraw.Width > 0 && frameToDraw.Height > 0)
            {
                // Calculate scaling to fit the frame while maintaining aspect ratio
                var scaleX = (float)info.Width / frameToDraw.Width;
                var scaleY = (float)info.Height / frameToDraw.Height;
                var scale = Math.Min(scaleX, scaleY);

                var scaledWidth = frameToDraw.Width * scale;
                var scaledHeight = frameToDraw.Height * scale;

                var destRect = new SKRect(
                    (info.Width - scaledWidth) / 2,
                    (info.Height - scaledHeight) / 2,
                    (info.Width + scaledWidth) / 2,
                    (info.Height + scaledHeight) / 2
                );

                canvas2.DrawBitmap(frameToDraw, destRect);
            }
        }
        
        // Store canvas dimensions
        _canvasWidth = info.Width;
        _canvasHeight = info.Height;
        
        // Draw voxels on top with blending
        _renderer.Render(canvas2, _voxelGrid, _camera, info.Width, info.Height, 
            CursorVoxel, SelectedVoxel);
        
        // Draw hand tracking visualization overlay
        if (IsHandTrackingEnabled && _latestHandDetections != null && _latestHandDetections.Count > 0)
        {
            DrawHandTrackingOverlay(canvas2, info.Width, info.Height);
        }
        
        // Update FPS
        _frameCount++;
        var now = DateTime.Now;
        if ((now - _lastFpsUpdate).TotalSeconds >= 1.0)
        {
            Fps = _frameCount;
            _frameCount = 0;
            _lastFpsUpdate = now;
        }
    }

    public void OnPointerPressed(object? sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
        _lastMousePosition = point.Position;

        // Cast ray for voxel interaction
        var ray = _rayCaster.ScreenToWorldRay(
            (float)point.Position.X, (float)point.Position.Y,
            _camera, _canvasWidth, _canvasHeight);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isLeftMouseDown = true;
            
            // Place voxel at cursor position
            if (CursorVoxel.HasValue)
            {
                var pos = CursorVoxel.Value;
                if (_voxelGrid.IsValidPosition(pos.x, pos.y, pos.z) && 
                    !_voxelGrid.HasVoxel(pos.x, pos.y, pos.z))
                {
                    var cmd = new PlaceVoxelCommand(pos.x, pos.y, pos.z, CurrentColor);
                    _undoStack.ExecuteCommand(cmd, _voxelGrid);
                    UpdateUndoRedoState();
                    SelectedVoxel = pos;
                    IsDirty = true; // Mark as dirty
                }
            }
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _isRightMouseDown = true;
            
            // Check if we're clicking on a voxel to remove it (not camera control)
            var hitVoxel = _rayCaster.RayGridIntersection(ray.origin, ray.direction, _voxelGrid, findEmpty: false);
            if (hitVoxel.HasValue && !e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.None))
            {
                // This is intentional - allow camera control by default
            }
        }
        else if (point.Properties.IsMiddleButtonPressed)
        {
            _isMiddleMouseDown = true;
        }
    }

    public void OnPointerMoved(object? sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
        var currentPosition = point.Position;
        
        var deltaX = (float)(currentPosition.X - _lastMousePosition.X);
        var deltaY = (float)(currentPosition.Y - _lastMousePosition.Y);

        // Camera controls
        if (_isMiddleMouseDown || (point.Properties.IsRightButtonPressed && e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift)))
        {
            // Pan camera
            _camera.Pan(deltaX, deltaY);
        }
        else if (_isRightMouseDown)
        {
            // Orbit camera
            _camera.Orbit(deltaX, deltaY);
        }

        _lastMousePosition = currentPosition;

        // Update cursor position via ray casting
        var ray = _rayCaster.ScreenToWorldRay(
            (float)currentPosition.X, (float)currentPosition.Y,
            _camera, _canvasWidth, _canvasHeight);

        // Find placement position (empty voxel adjacent to existing voxels)
        var placementPos = _rayCaster.FindPlacementPosition(ray.origin, ray.direction, _voxelGrid);
        
        if (placementPos.HasValue)
        {
            CursorVoxel = placementPos;
            CursorPosition = $"{placementPos.Value.x}, {placementPos.Value.y}, {placementPos.Value.z}";
        }
        else
        {
            CursorVoxel = null;
            CursorPosition = "Out of bounds";
        }
    }

    public void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
    {
        _isLeftMouseDown = false;
        _isRightMouseDown = false;
        _isMiddleMouseDown = false;
    }

    public void OnPointerWheelChanged(object? sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
        var delta = point.Properties.MouseWheelDelta;
        
        _camera.ZoomCamera(delta);
    }

    public void OnColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var color = args.NewColor;
        CurrentColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public void OnPaletteColorSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Windows.UI.Color color)
        {
            CurrentColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    [RelayCommand]
    public async Task NewProject()
    {
        // Check for unsaved changes
        if (IsDirty)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved changes. Do you want to save before creating a new project?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't Save",
                CloseButtonText = "Cancel",
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                await SaveProject();
            }
            else if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.None)
            {
                return; // Cancel
            }
        }

        // Clear everything
        _voxelGrid.Clear();
        _undoStack.Clear();
        _camera.Reset();
        _currentFile = null;
        _projectMetadata = new ProjectMetadata
        {
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Author = Environment.UserName
        };
        ProjectName = "Untitled";
        IsDirty = false;
        UpdateUndoRedoState();
    }

    [RelayCommand]
    public async Task OpenProject()
    {
        // Check for unsaved changes
        if (IsDirty)
        {
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved changes. Do you want to save before opening another project?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't Save",
                CloseButtonText = "Cancel",
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                await SaveProject();
            }
            else if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.None)
            {
                return; // Cancel
            }
        }

        try
        {
            var (project, file) = await _fileService.LoadProjectAsync();
            
            if (project != null && file != null)
            {
                _fileService.LoadProjectIntoGrid(project, _voxelGrid, _camera);
                _currentFile = file;
                _projectMetadata = project.Metadata;
                ProjectName = file.DisplayName;
                IsDirty = false;
                _undoStack.Clear();
                UpdateUndoRedoState();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog("Failed to open project", ex.Message);
        }
    }

    [RelayCommand]
    public async Task SaveProject()
    {
        try
        {
            var palette = ColorPalette.Select(c => $"#{c.R:X2}{c.G:X2}{c.B:X2}").ToList();
            var project = _fileService.CreateProjectFromGrid(_voxelGrid, _camera, palette, _projectMetadata);
            
            var file = await _fileService.SaveProjectAsync(project, _currentFile);
            
            if (file != null)
            {
                _currentFile = file;
                ProjectName = file.DisplayName;
                IsDirty = false;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog("Failed to save project", ex.Message);
        }
    }

    [RelayCommand]
    public async Task SaveProjectAs()
    {
        try
        {
            var palette = ColorPalette.Select(c => $"#{c.R:X2}{c.G:X2}{c.B:X2}").ToList();
            var project = _fileService.CreateProjectFromGrid(_voxelGrid, _camera, palette, _projectMetadata);
            
            // Force new file selection by passing null
            var file = await _fileService.SaveProjectAsync(project, null);
            
            if (file != null)
            {
                _currentFile = file;
                ProjectName = file.DisplayName;
                IsDirty = false;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog("Failed to save project", ex.Message);
        }
    }

    [RelayCommand]
    public void Undo()
    {
        _undoStack.Undo(_voxelGrid);
        UpdateUndoRedoState();
        IsDirty = true;
    }

    [RelayCommand]
    public void Redo()
    {
        _undoStack.Redo(_voxelGrid);
        UpdateUndoRedoState();
        IsDirty = true;
    }

    [RelayCommand]
    public void ResetCamera()
    {
        _camera.Reset();
    }

    [RelayCommand]
    public void DeleteSelected()
    {
        if (SelectedVoxel.HasValue)
        {
            var pos = SelectedVoxel.Value;
            if (_voxelGrid.HasVoxel(pos.x, pos.y, pos.z))
            {
                var cmd = new RemoveVoxelCommand(pos.x, pos.y, pos.z);
                _undoStack.ExecuteCommand(cmd, _voxelGrid);
                UpdateUndoRedoState();
                SelectedVoxel = null;
                IsDirty = true;
            }
        }
    }

    [RelayCommand]
    public void ClearGrid()
    {
        // Clear all voxels from the grid
        _voxelGrid.Clear();
        _undoStack.Clear();
        UpdateUndoRedoState();
        SelectedVoxel = null;
        CursorVoxel = null;
        IsDirty = true;
        InvalidateCanvas();
    }

    private void UpdateUndoRedoState()
    {
        CanUndo = _undoStack.CanUndo;
        CanRedo = _undoStack.CanRedo;
    }

    private DateTime _lastInvalidate = DateTime.MinValue;
    private const int MinInvalidateMs = 33; // Max 30 FPS to prevent excessive redraws
    
    /// <summary>
    /// Sets the canvas reference from the View (called during OnNavigatedTo)
    /// </summary>
    public void SetCanvas(SKXamlCanvas canvas)
    {
        _canvas = canvas;
        // Trigger initial paint
        _canvas?.Invalidate();
    }

    private void InvalidateCanvas()
    {
        // Debounce canvas invalidation to prevent rendering more than 30 FPS
        // This reduces CPU/GPU load when multiple systems trigger redraws
        var now = DateTime.UtcNow;
        if ((now - _lastInvalidate).TotalMilliseconds >= MinInvalidateMs)
        {
            _lastInvalidate = now;
            _canvas?.Invalidate();
        }
    }

    // Webcam methods
    private async Task EnumerateCamerasAsync()
    {
        // Set default while enumerating
        AvailableCameras = new List<string> { "Detecting cameras..." };

        try
        {
            var cameras = await WebcamService.GetAvailableCamerasAsync();

            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    if (cameras.Count > 0)
                    {
                        AvailableCameras = cameras.Select(c => c.name).ToList();
                    }
                    else
                    {
                        // Provide default options even if enumeration fails
                        // User can still try to start camera on index 0
                        AvailableCameras = new List<string>
                        {
                            "Camera 0 (default)",
                            "Camera 1",
                            "Camera 2"
                        };
                        WebcamStatus = "Auto-detect failed, try manually";
                    }

                    System.Diagnostics.Debug.WriteLine($"[ViewModel] Found {cameras.Count} cameras, showing {AvailableCameras.Count} options");
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Error enumerating cameras: {ex.Message}");
            if (_dispatcher != null)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    // Provide defaults even on error
                    AvailableCameras = new List<string>
                    {
                        "Camera 0 (default)",
                        "Camera 1",
                        "Camera 2"
                    };
                    WebcamStatus = "Enum error, try manually";
                });
            }
        }
    }

    [RelayCommand]
    private async Task ToggleWebcam()
    {
        if (IsWebcamActive)
        {
            await StopWebcam();
        }
        else
        {
            await StartWebcam();
        }
    }

    [RelayCommand]
    private void CalibrateBackground()
    {
        if (!IsWebcamActive || _currentWebcamFrame == null)
        {
            WebcamStatus = "Start camera first";
            return;
        }

        try
        {
            // Capture current frame as background
            if (_handTracker is OnnxHandTracker onnxTracker)
            {
                onnxTracker.CaptureBackground(_currentWebcamFrame);
                WebcamStatus = "✓ Background calibrated";
                System.Diagnostics.Debug.WriteLine("[ViewModel] Background calibration complete");
            }
        }
        catch (Exception ex)
        {
            WebcamStatus = $"Calibration failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Calibration error: {ex.Message}");
        }
    }

    private async Task StartWebcam()
    {
        try
        {
            WebcamStatus = "Initializing...";
            WebcamButtonText = "Starting...";

            // Ensure valid camera index (ComboBox may return -1 if nothing selected)
            var cameraIndex = SelectedCameraIndex >= 0 ? SelectedCameraIndex : 0;
            _webcamService.SetCameraIndex(cameraIndex);
            
            var initialized = await _webcamService.InitializeAsync();
            if (!initialized)
            {
                WebcamStatus = "Failed to initialize camera";
                WebcamButtonText = "Start Camera";
                return;
            }

            var started = await _webcamService.StartCaptureAsync();
            if (started)
            {
                IsWebcamActive = true;
                WebcamStatus = $"Camera {cameraIndex} active";
                WebcamButtonText = "Stop Camera";
            }
            else
            {
                WebcamStatus = "Failed to start camera";
                WebcamButtonText = "Start Camera";
            }
        }
        catch (Exception ex)
        {
            WebcamStatus = $"Error: {ex.Message}";
            WebcamButtonText = "Start Camera";
        }
    }

    private async Task StopWebcam()
    {
        try
        {
            await _webcamService.StopCaptureAsync();
            IsWebcamActive = false;
            WebcamStatus = "Camera stopped";
            WebcamButtonText = "Start Camera";
            _currentWebcamFrame?.Dispose();
            _currentWebcamFrame = null;
            _handCts?.Cancel();
            _handCts = null;
            InvalidateCanvas();
        }
        catch (Exception ex)
        {
            WebcamStatus = $"Error stopping: {ex.Message}";
        }
    }

    private readonly object _frameLock = new();

    private void OnWebcamFrameCaptured(object? sender, SKBitmap frame)
    {
        // Ensure we have a valid dispatcher - get from main window if not already set
        // NOTE: GetForCurrentThread() won't work here since we're on a background thread
        _dispatcher ??= App.MainWindow?.DispatcherQueue;

        // This is called from background thread, marshal to UI thread
        var dispatcherAvailable = _dispatcher?.TryEnqueue(() =>
        {
            _webcamFrameCount++;
            WebcamStatus = $"Frames: {_webcamFrameCount}";

            // Thread-safe frame update
            lock (_frameLock)
            {
                _currentWebcamFrame?.Dispose();
                _currentWebcamFrame = frame;
            }

            // Hand tracking processing (throttled)
            if (IsHandTrackingEnabled && Interlocked.CompareExchange(ref _isProcessingHands, 1, 0) == 0)
            {
                var now = DateTime.UtcNow;
                if ((now - _lastHandProcessTime).TotalMilliseconds >= 16) // ~60 FPS
                {
                    SKBitmap? frameForProcessing = null;
                    lock (_frameLock)
                    {
                        if (_currentWebcamFrame != null)
                        {
                            // Rent from pool instead of copying
                            frameForProcessing = _bitmapPool.Rent(_currentWebcamFrame.Width, _currentWebcamFrame.Height);
                            _currentWebcamFrame.CopyTo(frameForProcessing);
                        }
                    }

                    if (frameForProcessing != null)
                    {
                        _lastHandProcessTime = now;
                        _handCts ??= new CancellationTokenSource();
                        var ct = _handCts.Token;

                        Task.Run(async () =>
                        {
                            try
                            {
                                var detections = await _handTracker.DetectAsync(frameForProcessing, ct);

                                if (ct.IsCancellationRequested)
                                {
                                    return;
                                }

                                // Back to UI to update state and invalidate
                                _dispatcher?.TryEnqueue(() =>
                                {
                                    _latestHandDetections = detections;
                                    HandCount = detections?.Count ?? 0;
                                    
                                    // TWO-HAND NAVIGATION: If 2 hands detected, use for camera control
                                    if (detections != null && detections.Count >= 2)
                                    {
                                        // Smooth both hands to reduce jitter before navigation calculations
                                        var sh1 = _landmarkSmoother.Smooth(detections[0]);
                                        var sh2 = _landmarkSmoother.Smooth(detections[1]);
                                        ProcessTwoHandNavigation(sh1, sh2);
                                        HandTrackingStatus = "✋✋ Two-hand navigation (pan/zoom)";
                                        CurrentGesture = "Navigation";
                                    }
                                    // SINGLE-HAND INTERACTION: Drawing/erasing with gestures
                                    else if (detections != null && detections.Count > 0)
                                    {
                                        // Reset two-hand state when going back to one hand
                                        _lastTwoHandCenter = null;
                                        _lastTwoHandDistance = null;

                                        // Smooth hand landmarks before gesture detection and voxel mapping
                                        var firstHand = _landmarkSmoother.Smooth(detections[0]);
                                        var gesture = _gestureDetector.DetectGesture(firstHand);

                                        // Roboflow cloud classification (runs in parallel, non-blocking)
                                        // This provides an additional gesture classification for comparison/override
                                        if (IsRoboflowEnabled && _currentWebcamFrame != null)
                                        {
                                            // Calculate hand bounding box for ROI (improves Roboflow accuracy)
                                            var landmarks = firstHand.Landmarks;
                                            if (landmarks.Count >= 21)
                                            {
                                                var minX = landmarks.Min(l => l.X);
                                                var maxX = landmarks.Max(l => l.X);
                                                var minY = landmarks.Min(l => l.Y);
                                                var maxY = landmarks.Max(l => l.Y);

                                                // Convert normalized coords to pixel coords with padding
                                                var padding = 0.1f;
                                                var frameW = _currentWebcamFrame.Width;
                                                var frameH = _currentWebcamFrame.Height;
                                                var roi = new SKRectI(
                                                    (int)((minX - padding) * frameW),
                                                    (int)((minY - padding) * frameH),
                                                    (int)((maxX + padding) * frameW),
                                                    (int)((maxY + padding) * frameH)
                                                );

                                                // Fire and forget - don't block hand tracking
                                                _ = ClassifyWithRoboflowAsync(_currentWebcamFrame, roi);
                                            }
                                        }
                                        
                                        // Update state machine with new gesture detection
                                        var stateChanged = _gestureStateMachine.Update(gesture);
                                        var confirmedGesture = _gestureStateMachine.CurrentGesture;
                                        var isConfirmed = _gestureStateMachine.IsGestureConfirmed;
                                        
                                        // Show detailed status with confirmation state
                                        CurrentGesture = _gestureStateMachine.GetStateDescription();

                                        // Map hand position to voxel grid cursor WITH AR PLANE
                                        // This places voxels on a flat plane in front of camera (like drawing on invisible canvas)
                                        var voxelPos = _handMapper.MapToArPlane(firstHand, ArPlaneDepth);
                                        if (voxelPos.HasValue)
                                        {
                                            CursorVoxel = voxelPos;
                                            CursorPosition = $"{voxelPos.Value.x}, {voxelPos.Value.y}, {voxelPos.Value.z}";
                                            
                                            // ONLY process actions when gesture is CONFIRMED (multi-frame validation)
                                            // This eliminates false positives and requires intentional, sustained gestures
                                            System.Diagnostics.Debug.WriteLine($"[HandTracking] Gesture={confirmedGesture}, isConfirmed={isConfirmed}, pos={voxelPos.Value}");
                                            if (isConfirmed && confirmedGesture != GestureType.None)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"[HandTracking] Calling ProcessGestureAction");
                                                ProcessGestureAction(confirmedGesture, voxelPos.Value);
                                            }
                                            
                                            var status = isConfirmed ? "✓" : "⋯";
                                            HandTrackingStatus = $"{status} {confirmedGesture} @ ({voxelPos.Value.x},{voxelPos.Value.y},{voxelPos.Value.z})";
                                        }
                                        else
                                        {
                                            HandTrackingStatus = $"Hands: {HandCount} | {CurrentGesture}";
                                        }
                                    }
                                    else
                                    {
                                        CurrentGesture = "None";
                                        HandTrackingStatus = "No hands";
                                        _handMapper.Reset(); // Reset smoothing when hand disappears
                                        _landmarkSmoother.Reset();
                                        _gestureStateMachine.Reset(); // Reset state machine
                                        _lastGestureType = GestureType.None; // Reset gesture tracking
                                        _lastTwoHandCenter = null;
                                        _lastTwoHandDistance = null;
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                _dispatcher?.TryEnqueue(() =>
                                {
                                    HandTrackingStatus = $"Tracking error: {ex.Message}";
                                });
                            }
                            finally
                            {
                                _bitmapPool.Return(frameForProcessing);
                                Interlocked.Exchange(ref _isProcessingHands, 0);
                            }
                        });
                    }
                    else
                    {
                        // No frame to process, reset flag
                        Interlocked.Exchange(ref _isProcessingHands, 0);
                    }
                }
                else
                {
                    // Too soon since last process, reset flag
                    Interlocked.Exchange(ref _isProcessingHands, 0);
                }
            }
        });

        // If dispatcher enqueue failed, dispose the frame to avoid memory leak
        if (dispatcherAvailable != true)
        {
            Console.WriteLine("[ViewModel] Dispatcher not available, frame dropped");
            frame.Dispose();
        }
    }

    public void Dispose()
    {
        _renderTimer?.Stop();

        // Unsubscribe from webcam events to prevent memory leaks
        if (_webcamService != null)
        {
            _webcamService.FrameCaptured -= OnWebcamFrameCaptured;
        }

        _webcamService?.Dispose();
        _currentWebcamFrame?.Dispose();
        _handCts?.Cancel();
        _handCts?.Dispose();

        // Dispose services
        (_handTracker as IDisposable)?.Dispose();

        // Dispose bitmap pool
        _bitmapPool?.Dispose();
    }

    private async void SafeFireAndForget(Func<Task> action, [CallerMemberName] string? caller = null)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Fire-and-forget failed in {Caller}", caller);
            System.Diagnostics.Debug.WriteLine($"[VoxelEditorViewModel] Fire-and-forget failed in {caller}: {ex.Message}");
        }
    }

    private async Task ShowErrorDialog(string title, string message)
    {
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow?.Content?.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task InitializeHandTrackerAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ViewModel] Initializing hand tracker...");
            var initialized = await _handTracker.InitializeAsync();
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Hand tracker initialized: {initialized}");
            if (!initialized)
            {
                HandTrackingStatus = "Failed to initialize";
            }
            else if (_handTracker is OnnxHandTracker ot)
            {
                HandTrackingStatus = ot.ModelsAvailable
                    ? "ONNX models loaded"
                    : "ONNX models missing — using fallback (reduced accuracy)";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Hand tracker init error: {ex.Message}");
            HandTrackingStatus = $"Init error: {ex.Message}";
        }
    }

    private async Task InitializeRoboflowAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ViewModel] Initializing Roboflow classifier...");

            if (!_roboflowClassifier.IsConfigured)
            {
                RoboflowStatus = "Not configured (set API key)";
                System.Diagnostics.Debug.WriteLine("[ViewModel] Roboflow API key not configured");
                return;
            }

            RoboflowStatus = "Initializing...";
            var initialized = await _roboflowClassifier.InitializeAsync();

            if (initialized)
            {
                RoboflowStatus = "Ready (cloud)";
                IsRoboflowEnabled = true;
                System.Diagnostics.Debug.WriteLine("[ViewModel] Roboflow classifier initialized successfully");
            }
            else
            {
                RoboflowStatus = "Init failed";
                System.Diagnostics.Debug.WriteLine("[ViewModel] Roboflow classifier initialization failed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Roboflow init error: {ex.Message}");
            RoboflowStatus = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Classifies gesture using Roboflow cloud API (if enabled and available).
    /// This runs in parallel with local gesture detection for comparison.
    /// </summary>
    private async Task ClassifyWithRoboflowAsync(SKBitmap frame, SKRectI? handRegion)
    {
        if (!IsRoboflowEnabled || !_roboflowClassifier.IsAvailable)
            return;

        try
        {
            var result = await _roboflowClassifier.ClassifyAsync(frame, handRegion);

            // Update UI on dispatcher
            _dispatcher?.TryEnqueue(() =>
            {
                if (result.Success)
                {
                    RoboflowGesture = result.Gesture != GestureType.None
                        ? $"{result.Gesture} ({result.Confidence:P0})"
                        : "None";
                    RoboflowLatencyMs = result.LatencyMs;
                    RoboflowStatus = $"OK ({result.LatencyMs}ms)";

                    // If Roboflow detected a gesture with high confidence, use it
                    // This allows Roboflow to override local detection when confident
                    if (result.Gesture != GestureType.None && result.Confidence > 0.7f)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Roboflow] High-confidence gesture: {result.Gesture} ({result.Confidence:P0})");
                    }
                }
                else
                {
                    RoboflowStatus = result.ErrorMessage ?? "Error";
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ViewModel] Roboflow classify error: {ex.Message}");
            _dispatcher?.TryEnqueue(() =>
            {
                RoboflowStatus = "Error";
            });
        }
    }

    [RelayCommand]
    private void ToggleRoboflow()
    {
        if (!_roboflowClassifier.IsConfigured)
        {
            RoboflowStatus = "Set API key first";
            return;
        }

        IsRoboflowEnabled = !IsRoboflowEnabled;
        RoboflowStatus = IsRoboflowEnabled ? "Enabled" : "Disabled";
    }

    /// <summary>
    /// Processes gesture actions for voxel control.
    /// Pinch and ClosedFist support continuous drawing/erasing with EXTRUDE (path-filling) while held.
    /// OpenPalm and None are ignored to prevent accidental undo when releasing pinch.
    /// </summary>
    private void ProcessGestureAction(GestureType gestureType, (int x, int y, int z) voxelPos)
    {
        System.Diagnostics.Debug.WriteLine($"[ProcessGestureAction] Called with gesture={gestureType}, pos=({voxelPos.x},{voxelPos.y},{voxelPos.z})");

        // Reset extrude tracking when gesture changes or ends
        if (gestureType != _lastGestureType || gestureType == GestureType.None || gestureType == GestureType.OpenPalm)
        {
            _lastPlacedVoxel = null;
        }

        // Ignore OpenPalm and None gestures - these cause accidental actions when releasing other gestures
        if (gestureType == GestureType.OpenPalm || gestureType == GestureType.None)
        {
            _lastGestureType = gestureType;
            return;
        }

        var pos = voxelPos;

        switch (gestureType)
        {
            case GestureType.Pinch:
                // Pinch = Place voxel with EXTRUDE support (fills path while dragging)
                Console.WriteLine($"[HandleGestureAction] Pinch detected at voxel pos ({pos.x},{pos.y},{pos.z})");
                ProcessPinchPlacement(pos);
                break;

            case GestureType.ClosedFist:
                // Closed Fist = Remove voxel with EXTRUDE support (erases path while dragging)
                ProcessFistErasure(pos);
                break;

            case GestureType.Point:
                // Point = Cycle through color palette (only on transition)
                if (gestureType != _lastGestureType)
                {
                    _currentColorIndex = (_currentColorIndex + 1) % ColorPalette.Count;
                    var newColor = ColorPalette[_currentColorIndex];
                    CurrentColor = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                    System.Diagnostics.Debug.WriteLine($"[Gesture] Changed color to {CurrentColor}");
                }
                break;
        }

        _lastGestureType = gestureType;
    }

    /// <summary>
    /// Handles pinch gesture for voxel placement with extrude (line drawing) support
    /// </summary>
    private void ProcessPinchPlacement((int x, int y, int z) pos)
    {
        Console.WriteLine($"[ProcessPinchPlacement] Called with pos=({pos.x},{pos.y},{pos.z})");

        if (!_voxelGrid.IsValidPosition(pos.x, pos.y, pos.z))
        {
            Console.WriteLine($"[ProcessPinchPlacement] Invalid position!");
            return;
        }

        // If this is a continued pinch and position changed, fill path (EXTRUDE)
        if (_lastPlacedVoxel.HasValue && _lastPlacedVoxel.Value != pos)
        {
            // EXTRUDE MODE: Fill voxels along 3D path from last position to current
            Console.WriteLine($"[ProcessPinchPlacement] EXTRUDE from {_lastPlacedVoxel.Value} to {pos}");
            var path = GetVoxelPath(_lastPlacedVoxel.Value, pos);

            foreach (var voxel in path)
            {
                if (_voxelGrid.IsValidPosition(voxel.x, voxel.y, voxel.z) &&
                    !_voxelGrid.HasVoxel(voxel.x, voxel.y, voxel.z))
                {
                    Console.WriteLine($"[ProcessPinchPlacement] Placing voxel at ({voxel.x},{voxel.y},{voxel.z})");
                    var cmd = new PlaceVoxelCommand(voxel.x, voxel.y, voxel.z, CurrentColor);
                    _undoStack.ExecuteCommand(cmd, _voxelGrid);
                    UpdateUndoRedoState();
                    IsDirty = true;
                }
            }
        }
        else if (!_voxelGrid.HasVoxel(pos.x, pos.y, pos.z))
        {
            // First voxel or same position - just place single voxel
            Console.WriteLine($"[ProcessPinchPlacement] Placing single voxel at ({pos.x},{pos.y},{pos.z})");
            var cmd = new PlaceVoxelCommand(pos.x, pos.y, pos.z, CurrentColor);
            _undoStack.ExecuteCommand(cmd, _voxelGrid);
            UpdateUndoRedoState();
            IsDirty = true;
        }

        _lastPlacedVoxel = pos;
        SelectedVoxel = pos;
    }

    /// <summary>
    /// Handles closed fist gesture for voxel removal with extrude (line erasing) support
    /// </summary>
    private void ProcessFistErasure((int x, int y, int z) pos)
    {
        if (!_voxelGrid.IsValidPosition(pos.x, pos.y, pos.z))
            return;

        // If position changed, erase along path (EXTRUDE)
        if (_lastPlacedVoxel.HasValue && _lastPlacedVoxel.Value != pos)
        {
            // EXTRUDE MODE: Erase voxels along 3D path from last position to current
            var path = GetVoxelPath(_lastPlacedVoxel.Value, pos);

            foreach (var voxel in path)
            {
                if (_voxelGrid.IsValidPosition(voxel.x, voxel.y, voxel.z) &&
                    _voxelGrid.HasVoxel(voxel.x, voxel.y, voxel.z))
                {
                    var cmd = new RemoveVoxelCommand(voxel.x, voxel.y, voxel.z);
                    _undoStack.ExecuteCommand(cmd, _voxelGrid);
                    UpdateUndoRedoState();
                    IsDirty = true;
                }
            }
        }
        else if (_voxelGrid.HasVoxel(pos.x, pos.y, pos.z))
        {
            // First voxel or same position - just remove single voxel
            var cmd = new RemoveVoxelCommand(pos.x, pos.y, pos.z);
            _undoStack.ExecuteCommand(cmd, _voxelGrid);
            UpdateUndoRedoState();
            IsDirty = true;
        }

        _lastPlacedVoxel = pos;
    }

    /// <summary>
    /// Gets all voxels along a 3D line from start to end using 3D Bresenham algorithm
    /// </summary>
    private List<(int x, int y, int z)> GetVoxelPath((int x, int y, int z) start, (int x, int y, int z) end)
    {
        var path = new List<(int x, int y, int z)>();

        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);
        int dz = Math.Abs(end.z - start.z);

        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;
        int sz = start.z < end.z ? 1 : -1;

        int dm = Math.Max(dx, Math.Max(dy, dz));
        int i = dm;

        int x = start.x, y = start.y, z = start.z;

        // 3D Bresenham line algorithm
        while (i >= 0)
        {
            path.Add((x, y, z));

            if (x == end.x && y == end.y && z == end.z)
                break;

            int err1 = 2 * dx - dm;
            int err2 = 2 * dy - dm;
            int err3 = 2 * dz - dm;

            if (err1 > 0) { x += sx; dx -= dm; }
            if (err2 > 0) { y += sy; dy -= dm; }
            if (err3 > 0) { z += sz; dz -= dm; }

            dx += Math.Abs(end.x - start.x);
            dy += Math.Abs(end.y - start.y);
            dz += Math.Abs(end.z - start.z);

            i--;
        }

        return path;
    }

    /// <summary>
    /// Process two-hand navigation for camera pan and zoom
    /// Both hands visible = pan (by moving center) and zoom (by pinching/spreading)
    /// </summary>
    private void ProcessResizeGesture(HandDetection hand)
    {
        var (isActive, spread) = GestureDetector.DetectResizeGesture(hand, 1.0);
        
        if (isActive)
        {
            if (_lastResizeSpread.HasValue)
            {
                var spreadDelta = spread - _lastResizeSpread.Value;
                
                // Only resize if delta is significant
                if (MathF.Abs(spreadDelta) > 0.05f)
                {
                    // Map spread (0-1) to voxel size change
                    var sizeChange = (int)(spreadDelta * 20f); // Spread 0.1 = 2 voxel units
                    var newSize = Math.Clamp(VoxelSize + sizeChange, MinVoxelSize, MaxVoxelSize);
                    
                    if (newSize != VoxelSize)
                    {
                        VoxelSize = newSize;
                        System.Diagnostics.Debug.WriteLine($"[Resize] Voxel size: {VoxelSize} (spread: {spread:F2})");
                    }
                }
            }
            
            _lastResizeSpread = spread;
        }
        else
        {
            _lastResizeSpread = null;
        }
    }

    private void ProcessTwoHandNavigation(HandDetection hand1, HandDetection hand2)
    {
        if (hand1.Landmarks.Count < 9 || hand2.Landmarks.Count < 9)
            return;

        // Use palm centers (landmark 9) for both hands
        var palm1 = hand1.Landmarks[9];
        var palm2 = hand2.Landmarks[9];
        
        // Calculate center point between hands
        var centerX = (palm1.X + palm2.X) / 2f;
        var centerY = (palm1.Y + palm2.Y) / 2f;
        
        // Calculate distance between hands (for zoom)
        var dx = palm2.X - palm1.X;
        var dy = palm2.Y - palm1.Y;
        var distance = MathF.Sqrt(dx * dx + dy * dy);
        
        // PANNING: Track center movement (like dragging with right mouse)
        if (_lastTwoHandCenter.HasValue)
        {
            var deltaX = (centerX - _lastTwoHandCenter.Value.x) * 800f; // Reduced sensitivity (was 2000)
            var deltaY = (centerY - _lastTwoHandCenter.Value.y) * 800f;
            
            // Pan camera (similar to right-click drag)
            if (MathF.Abs(deltaX) > 0.5f || MathF.Abs(deltaY) > 0.5f)
            {
                _camera.Pan(-deltaX, deltaY); // Note: X inverted for natural feel
            }
        }

        // ZOOMING: Track distance change (pinch to zoom in, spread to zoom out)
        if (_lastTwoHandDistance.HasValue)
        {
            var distanceDelta = distance - _lastTwoHandDistance.Value;

            // Zoom based on hand distance change (more sensitive)
            if (MathF.Abs(distanceDelta) > 0.005f) // Lower threshold
            {
                var zoomFactor = distanceDelta * 200f; // Increased sensitivity (was 50)
                _camera.ZoomCamera(zoomFactor);
            }
        }
        
        // Store current values for next frame
        _lastTwoHandCenter = (centerX, centerY);
        _lastTwoHandDistance = distance;
    }

    /// <summary>
    /// Draws visual overlay showing detected hand landmarks, connections, and gesture info
    /// </summary>
    private void DrawHandTrackingOverlay(SKCanvas canvas, float width, float height)
    {
        if (_latestHandDetections == null || _latestHandDetections.Count == 0)
            return;

        var hand = _latestHandDetections[0];
        var landmarks = hand.Landmarks;

        if (landmarks.Count < 21)
            return;

        // Paint for landmarks (fingertips and joints)
        using var landmarkPaint = new SKPaint
        {
            Color = SKColors.Lime,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Paint for connections between landmarks
        using var connectionPaint = new SKPaint
        {
            Color = SKColors.Cyan.WithAlpha(180),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        // Paint for important landmarks (fingertips)
        using var fingertipPaint = new SKPaint
        {
            Color = SKColors.Yellow,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Draw hand skeleton connections
        var connections = new (int, int)[]
        {
            // Thumb
            (0, 1), (1, 2), (2, 3), (3, 4),
            // Index finger
            (0, 5), (5, 6), (6, 7), (7, 8),
            // Middle finger
            (0, 9), (9, 10), (10, 11), (11, 12),
            // Ring finger
            (0, 13), (13, 14), (14, 15), (15, 16),
            // Pinky
            (0, 17), (17, 18), (18, 19), (19, 20)
        };

        foreach (var (start, end) in connections)
        {
            if (start < landmarks.Count && end < landmarks.Count)
            {
                var startPoint = new SKPoint(landmarks[start].X * width, landmarks[start].Y * height);
                var endPoint = new SKPoint(landmarks[end].X * width, landmarks[end].Y * height);
                canvas.DrawLine(startPoint, endPoint, connectionPaint);
            }
        }

        // Draw landmarks
        for (int i = 0; i < landmarks.Count; i++)
        {
            var landmark = landmarks[i];
            var x = landmark.X * width;
            var y = landmark.Y * height;

            // Fingertips are larger and different color
            bool isFingertip = (i == 4 || i == 8 || i == 12 || i == 16 || i == 20);
            var paint = isFingertip ? fingertipPaint : landmarkPaint;
            var radius = isFingertip ? 8f : 5f;

            canvas.DrawCircle(x, y, radius, paint);
        }

        // Draw gesture info overlay
        var gesture = _gestureDetector.DetectGesture(hand);
        
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 32);
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var bgPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(180),
            Style = SKPaintStyle.Fill
        };

        var gestureText = $"Gesture: {gesture.Type} ({gesture.Confidence:P0})";
        var textBounds = new SKRect();
        font.MeasureText(gestureText, out textBounds);
        
        var textX = 20;
        var textY = height - 60;
        
        // Draw background
        canvas.DrawRect(textX - 10, textY - textBounds.Height - 10, 
            textBounds.Width + 20, textBounds.Height + 20, bgPaint);
        
        // Draw text
        canvas.DrawText(gestureText, textX, textY, font, textPaint);

        // Draw hand detection confidence
        var statusText = $"Hands: {_latestHandDetections.Count} | Landmarks: {landmarks.Count}";
        using var smallFont = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 24);
        smallFont.MeasureText(statusText, out textBounds);
        
        canvas.DrawRect(textX - 10, textY - textBounds.Height - 50, 
            textBounds.Width + 20, textBounds.Height + 20, bgPaint);
        canvas.DrawText(statusText, textX, textY - 30, smallFont, textPaint);
    }

    // MVVM Toolkit generated hook for IsHandTrackingEnabled changes
    partial void OnIsHandTrackingEnabledChanged(bool value)
    {
        if (value)
        {
            _handCts?.Cancel();
            _handCts?.Dispose();
            _handCts = new CancellationTokenSource();
            HandTrackingStatus = "Enabled";
            
            // Reduce render rate to 30 FPS when hand tracking is active for better performance
            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0); // 30 FPS
                _renderTimer.Start();
            }
        }
        else
        {
            _handCts?.Cancel();
            _handCts?.Dispose();
            _handCts = null;
            _latestHandDetections = null;
            HandCount = 0;
            HandTrackingStatus = "Disabled";
            
            // Restore 60 FPS when hand tracking is disabled
            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0); // 60 FPS
                _renderTimer.Start();
            }
            
            InvalidateCanvas();
        }
    }

    // Auto-switch camera device when the user changes the selection
    partial void OnSelectedCameraIndexChanged(int value)
    {
        // Update the service selection immediately
        _webcamService.SetCameraIndex(value);

        // If webcam is currently active, restart capture on the new device
        if (IsWebcamActive)
        {
            _ = Task.Run(async () =>
            {
                _dispatcher?.TryEnqueue(() =>
                {
                    WebcamStatus = $"Switching to camera {value}...";
                });
                try
                {
                    await StopWebcam();
                    await StartWebcam();
                }
                catch (Exception ex)
                {
                    _dispatcher?.TryEnqueue(() =>
                    {
                        WebcamStatus = $"Camera switch failed: {ex.Message}";
                    });
                }
            });
        }
    }
}
