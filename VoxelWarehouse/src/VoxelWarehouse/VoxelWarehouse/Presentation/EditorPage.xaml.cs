using Microsoft.UI.Xaml.Media;
using VoxelWarehouse.ML;

namespace VoxelWarehouse.Presentation;

public sealed partial class EditorPage : Page
{
    /// <summary>Fires when the canvas is ready for the Shell to connect to.</summary>
    public static event Action<EditorPage, IsometricCanvasControl>? CanvasRegistered;

    private readonly Border[] _layerBars;
    private static readonly SolidColorBrush LayerActive = new(Windows.UI.Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush LayerHasData = new(Windows.UI.Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush LayerEmpty = new(Windows.UI.Color.FromArgb(0x05, 0xFF, 0xFF, 0xFF));

    private ICameraService? _camera;
    private OnnxHandTracker? _tracker;
    private HandTrackingLoop? _trackingLoop;
    private bool _cameraEnabled;

    public EditorPage()
    {
        this.InitializeComponent();
        _layerBars = [Layer0, Layer1, Layer2, Layer3, Layer4, Layer5];
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        VoxelCanvas.WorldChanged += OnCanvasWorldChanged;
        UpdateLayerBars(VoxelCanvas.World);
        UpdateCursorDisplay();

        // Notify Shell that the canvas is ready
        CanvasRegistered?.Invoke(this, VoxelCanvas);

        // Camera service setup
        if (App.Current is App app && app.Host?.Services is { } sp)
            _camera = sp.GetService<ICameraService>();
        _camera ??= new CameraService();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => StopTracking();

    private void OnCanvasWorldChanged(VoxelWorldState world)
    {
        UpdateLayerBars(world);
        UpdateCursorDisplay();
    }

    private void UpdateCursorDisplay()
    {
        var pos = VoxelCanvas.CursorGridPosition;
        CursorInfo.Text = $"POS {pos.GX},{pos.GZ} \u00B7 H{VoxelCanvas.World.ActiveLayer}";
        AssetInfo.Text = $"{VoxelCanvas.World.ActiveAsset.ToString().ToUpper()} \u00B7 {VoxelCanvas.World.ActiveTool.ToString().ToUpper()}";
    }

    #region Layer Control

    private void UpdateLayerBars(VoxelWorldState world)
    {
        for (int i = 0; i < _layerBars.Length; i++)
            _layerBars[i].Background = i == world.ActiveLayer ? LayerActive
                : world.Cells.Keys.Any(k => k.Y == i) ? LayerHasData : LayerEmpty;
    }

    private void OnLayerUp(object sender, RoutedEventArgs e)
    {
        VoxelCanvas.World = VoxelCanvas.World with
        {
            ActiveLayer = Math.Clamp(VoxelCanvas.World.ActiveLayer + 1, 0, GridConstants.MaxHeight - 1)
        };
        UpdateLayerBars(VoxelCanvas.World);
    }

    private void OnLayerDown(object sender, RoutedEventArgs e)
    {
        VoxelCanvas.World = VoxelCanvas.World with
        {
            ActiveLayer = Math.Clamp(VoxelCanvas.World.ActiveLayer - 1, 0, GridConstants.MaxHeight - 1)
        };
        UpdateLayerBars(VoxelCanvas.World);
    }

    #endregion

    #region Hand Tracking

    public void ToggleCamera()
    {
        _cameraEnabled = !_cameraEnabled;
        if (_cameraEnabled)
            StartTracking();
        else
            StopTracking();
    }

    private void StartTracking()
    {
        if (_camera is null) return;

        try
        {
            var modelsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Models");
            var palmPath = Path.Combine(modelsDir, "palm_detection_mediapipe_2023feb.onnx");
            var landmarkPath = Path.Combine(modelsDir, "handpose_estimation_mediapipe_2023feb.onnx");

            if (!File.Exists(palmPath) || !File.Exists(landmarkPath)) return;

            _tracker = new OnnxHandTracker(modelsDir);
            _trackingLoop = new HandTrackingLoop(_tracker, _camera);
            _trackingLoop.ResultReady += OnHandTrackingResult;
            _trackingLoop.Start();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[EditorPage] TRACKING ERROR: {ex}");
            _cameraEnabled = false;
            StopTracking(); // Clean up partial state
        }
    }

    private void StopTracking()
    {
        try { _trackingLoop?.Stop(); } catch { /* best-effort */ }
        _trackingLoop?.Dispose();
        _trackingLoop = null;
        _tracker?.Dispose();
        _tracker = null;
        _camera?.StopCapture();
        _cameraEnabled = false;
    }

    private void OnHandTrackingResult(HandTrackingResult result)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            VoxelCanvas.UpdateHandState(result);
            if (result.HandDetected)
                CursorInfo.Text = $"HAND {result.Gesture} ({result.Confidence:P0}) | {_trackingLoop?.LastInferenceMs:F0}ms";
        });
    }

    #endregion
}
