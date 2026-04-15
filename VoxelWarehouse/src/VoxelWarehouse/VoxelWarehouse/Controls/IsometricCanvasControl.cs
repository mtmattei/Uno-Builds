using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Windows.System;

namespace VoxelWarehouse.Controls;

public sealed class IsometricCanvasControl : SKCanvasElement
{
    private int _cursorGX = 6;
    private int _cursorGZ = 6;
    private float _originX;
    private float _originY;
    private int _rotation;
    private float _zoom = 1.0f;
    private HandTrackingResult? _handState;

    private VoxelWorldState _world = Presets.WarehouseA();

    // Render cache: sorted voxel list only recomputed when cells or rotation change
    private KeyValuePair<(int X, int Y, int Z), AssetType>[]? _sortedCellsCache;
    private int _sortedCellsHash;
    private int _sortedRotation = -1;

    private const float MinZoom = 0.4f;
    private const float MaxZoom = 3.0f;
    private const float ZoomStep = 0.15f;

    public IsometricCanvasControl()
    {
        PointerMoved += OnPointerMoved;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
        Loaded += (_, _) => Invalidate();
    }

    #region Public API

    public VoxelWorldState World
    {
        get => _world;
        set { _world = value; Invalidate(); }
    }

    public int Rotation
    {
        get => _rotation;
        set { _rotation = ((value % 4) + 4) % 4; Invalidate(); }
    }

    public float Zoom
    {
        get => _zoom;
        set { _zoom = Math.Clamp(value, MinZoom, MaxZoom); Invalidate(); }
    }

    public (int GX, int GZ) CursorGridPosition => (_cursorGX, _cursorGZ);

    public event Action<VoxelWorldState>? WorldChanged;
    public event Action<int, int>? CursorMoved;

    public bool HandTrackingActive => _handState is { HandDetected: true };

    public void RotateCW() => Rotation = _rotation + 1;
    public void RotateCCW() => Rotation = _rotation - 1;
    public void ZoomIn() => Zoom += ZoomStep;
    public void ZoomOut() => Zoom -= ZoomStep;
    public void ZoomReset() => Zoom = 1.0f;

    public void UpdateHandState(HandTrackingResult result)
    {
        _handState = result;

        if (result.HandDetected)
        {
            float handX = Math.Clamp((result.CursorX - 0.15f) / 0.7f, 0f, 1f);
            float handY = Math.Clamp((result.CursorY - 0.15f) / 0.7f, 0f, 1f);

            int gx = (int)(handX * (GridConstants.GridSize - 1) + 0.5f);
            int gz = (int)(handY * (GridConstants.GridSize - 1) + 0.5f);
            _cursorGX = Math.Clamp(gx, 0, GridConstants.GridSize - 1);
            _cursorGZ = Math.Clamp(gz, 0, GridConstants.GridSize - 1);

            var key = (_cursorGX, _world.ActiveLayer, _cursorGZ);
            switch (result.Gesture)
            {
                case GestureType.Pinch:
                    _world = _world with { Cells = _world.Cells.SetItem(key, _world.ActiveAsset) };
                    WorldChanged?.Invoke(_world);
                    break;
                case GestureType.Fist:
                    _world = _world with { Cells = _world.Cells.Remove(key) };
                    WorldChanged?.Invoke(_world);
                    break;
            }
        }

        Invalidate();
    }

    #endregion

    #region Rendering

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        float width = (float)area.Width;
        float height = (float)area.Height;
        _originX = width / 2f;
        _originY = height * 0.35f;

        // Background layers (unscaled)
        canvas.Clear(new SKColor(19, 21, 25));
        DrawScanlines(canvas, width, height);
        DrawVignette(canvas, width, height);

        // Apply zoom transform around grid origin
        canvas.Save();
        canvas.Translate(_originX, _originY);
        canvas.Scale(_zoom, _zoom);
        canvas.Translate(-_originX, -_originY);

        // All grid rendering uses the unscaled origin — the canvas transform handles zoom
        VoxelRenderer.DrawZones(canvas, _originX, _originY, _world.Zones, _rotation);
        VoxelRenderer.DrawGrid(canvas, _originX, _originY, GridConstants.GridSize, _rotation);
        VoxelRenderer.DrawGroundShadows(canvas, _originX, _originY, _world.Cells, _rotation);

        if (_world.ActiveLayer > 0)
            DrawLayerGrid(canvas, _world.ActiveLayer, 0.45f);

        // Cache sorted cells — only re-sort when cells or rotation change
        int cellHash = _world.Cells.Count; // cheap proxy for change detection
        if (_sortedCellsCache is null || cellHash != _sortedCellsHash || _rotation != _sortedRotation)
        {
            _sortedCellsCache = _world.Cells
                .OrderBy(kv =>
                {
                    var (rx, rz) = IsoMath.RotateGrid(kv.Key.X, kv.Key.Z, _rotation);
                    return rx + rz;
                })
                .ThenBy(kv => kv.Key.Y)
                .ToArray();
            _sortedCellsHash = cellHash;
            _sortedRotation = _rotation;
        }

        foreach (var (key, asset) in _sortedCellsCache)
        {
            var (sx, sy) = IsoMath.GridToScreen(key.X, key.Z, key.Y, _originX, _originY, _rotation);
            float fog = IsoMath.FogFactor(key.X, key.Z, _rotation);
            float aoTop = VoxelRenderer.ComputeAO_Top(_world.Cells, key.X, key.Y, key.Z);
            float aoLeft = VoxelRenderer.ComputeAO_Left(_world.Cells, key.X, key.Y, key.Z);
            float aoRight = VoxelRenderer.ComputeAO_Right(_world.Cells, key.X, key.Y, key.Z);

            VoxelRenderer.DrawVoxel(canvas, sx, sy, asset, aoTop, aoLeft, aoRight, fog);

            if (!_world.Cells.ContainsKey((key.X, key.Y + 1, key.Z)))
                VoxelRenderer.DrawAssetLabel(canvas, sx, sy, asset, fog);
        }

        VoxelRenderer.DrawStackingPreview(canvas, _originX, _originY, _cursorGX, _cursorGZ, _world.ActiveLayer, _rotation);

        var (gsx, gsy) = IsoMath.GridToScreen(_cursorGX, _cursorGZ, _world.ActiveLayer, _originX, _originY, _rotation);
        float gfog = IsoMath.FogFactor(_cursorGX, _cursorGZ, _rotation);
        VoxelRenderer.DrawGhostCursor(canvas, gsx, gsy, _world.ActiveAsset, gfog);
        VoxelRenderer.DrawCrosshair(canvas, gsx, gsy, width / _zoom, height / _zoom);

        canvas.Restore(); // End zoom transform

        // HUD overlays (unscaled, always on top)
        if (_handState is { HandDetected: true })
        {
            float pipW = 160, pipH = 120;
            float pipX = width - pipW - 16;
            float pipY = height - pipH - 60;

            using var pipBg = new SKPaint { Color = new SKColor(0, 0, 0, 140), Style = SKPaintStyle.Fill };
            using var pipBorder = new SKPaint { Color = new SKColor(255, 255, 255, 30), Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            canvas.DrawRoundRect(pipX, pipY, pipW, pipH, 4, 4, pipBg);
            canvas.DrawRoundRect(pipX, pipY, pipW, pipH, 4, 4, pipBorder);

            canvas.Save();
            canvas.ClipRect(new SKRect(pipX, pipY, pipX + pipW, pipY + pipH));
            canvas.Translate(pipX, pipY);
            HandSkeletonRenderer.Draw(canvas, _handState, pipW, pipH);
            canvas.Restore();
        }

        DrawFrameBorder(canvas, width, height);
        DrawCornerMarks(canvas, width, height);
    }

    #endregion

    #region Overlays

    private static void DrawScanlines(SKCanvas canvas, float width, float height)
    {
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, 64), StrokeWidth = 1.0f, IsAntialias = false };
        for (float y = 0; y < height; y += 3f)
            canvas.DrawLine(0, y, width, y, paint);
    }

    private static void DrawVignette(SKCanvas canvas, float width, float height)
    {
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(width / 2f, 0),
                new SKPoint(width / 2f, height),
                [new SKColor(12, 12, 18, 60), new SKColor(3, 3, 5, 210)],
                [0.0f, 1.0f], SKShaderTileMode.Clamp),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    private void DrawLayerGrid(SKCanvas canvas, int layer, float alpha)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(10 * alpha)),
            StrokeWidth = 0.5f, IsAntialias = true, Style = SKPaintStyle.Stroke,
            PathEffect = SKPathEffect.CreateDash([4f, 6f], 0)
        };
        int gs = GridConstants.GridSize;
        for (int i = 0; i <= gs; i++)
        {
            var (x1, y1) = IsoMath.GridToScreen(i, 0, layer, _originX, _originY, _rotation);
            var (x2, y2) = IsoMath.GridToScreen(i, gs, layer, _originX, _originY, _rotation);
            canvas.DrawLine(x1, y1 + GridConstants.HalfHeight, x2, y2 + GridConstants.HalfHeight, paint);
            var (x3, y3) = IsoMath.GridToScreen(0, i, layer, _originX, _originY, _rotation);
            var (x4, y4) = IsoMath.GridToScreen(gs, i, layer, _originX, _originY, _rotation);
            canvas.DrawLine(x3, y3 + GridConstants.HalfHeight, x4, y4 + GridConstants.HalfHeight, paint);
        }
    }

    private static void DrawFrameBorder(SKCanvas canvas, float w, float h)
    {
        using var p = new SKPaint { Color = new SKColor(255, 255, 255, 5), Style = SKPaintStyle.Stroke, StrokeWidth = 1f, IsAntialias = true };
        canvas.DrawRect(8, 44, w - 16, h - 52, p);
    }

    private static void DrawCornerMarks(SKCanvas canvas, float w, float h)
    {
        using var p = new SKPaint { Color = new SKColor(255, 255, 255, 20), Style = SKPaintStyle.Stroke, StrokeWidth = 1f, IsAntialias = true };
        const float s = 12, o = 4, t = 48;
        canvas.DrawLine(o, t, o + s, t, p); canvas.DrawLine(o, t, o, t + s, p);
        canvas.DrawLine(w - o, t, w - o - s, t, p); canvas.DrawLine(w - o, t, w - o, t + s, p);
        canvas.DrawLine(o, h - o, o + s, h - o, p); canvas.DrawLine(o, h - o, o, h - o - s, p);
        canvas.DrawLine(w - o, h - o, w - o - s, h - o, p); canvas.DrawLine(w - o, h - o, w - o, h - o - s, p);
    }

    #endregion

    #region Input

    private bool _isDragging;
    private bool _isErasing;
    private int _lastDragGX = -1, _lastDragGZ = -1;

    private (int GX, int GZ) ScreenToGridZoomed(float screenX, float screenY)
    {
        float x = (screenX - _originX) / _zoom + _originX;
        float y = (screenY - _originY) / _zoom + _originY;
        return IsoMath.ScreenToGrid(x, y, _world.ActiveLayer, _originX, _originY, _rotation);
    }

    private void PlaceOrEraseAt(int gx, int gz, bool erase)
    {
        if (_world.ActiveMode == EditorMode.Zone)
        {
            var zoneKey = (gx, gz);
            if (erase)
                _world = _world with { Zones = _world.Zones.Remove(zoneKey) };
            else
            {
                var current = _world.Zones.GetValueOrDefault(zoneKey, ZoneType.None);
                var next = (ZoneType)(((int)current + 1) % 5);
                _world = _world with { Zones = _world.Zones.SetItem(zoneKey, next) };
            }
        }
        else
        {
            var key = (gx, _world.ActiveLayer, gz);
            if (erase)
                _world = _world with { Cells = _world.Cells.Remove(key) };
            else
                _world = _world with { Cells = _world.Cells.SetItem(key, _world.ActiveAsset) };
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);
        var (gx, gz) = ScreenToGridZoomed((float)pt.Position.X, (float)pt.Position.Y);

        bool moved = gx != _cursorGX || gz != _cursorGZ;
        _cursorGX = gx;
        _cursorGZ = gz;

        // Drag-to-place: continuously place/erase while mouse is held and moving
        if (_isDragging && moved && (gx != _lastDragGX || gz != _lastDragGZ))
        {
            PlaceOrEraseAt(gx, gz, _isErasing);
            _lastDragGX = gx;
            _lastDragGZ = gz;
            WorldChanged?.Invoke(_world);
        }

        if (moved)
        {
            CursorMoved?.Invoke(_cursorGX, _cursorGZ);
            Invalidate();
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);
        var (gx, gz) = ScreenToGridZoomed((float)pt.Position.X, (float)pt.Position.Y);

        _isErasing = pt.Properties.IsRightButtonPressed || _world.ActiveTool == ToolMode.Erase;
        _isDragging = true;
        _lastDragGX = gx;
        _lastDragGZ = gz;

        // Shift+Click = auto-stack: place on next empty layer at this column
        bool shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (shift && !_isErasing && _world.ActiveMode == EditorMode.Build)
        {
            // Find the next empty Y at this (gx, gz)
            for (int y = 0; y < GridConstants.MaxHeight; y++)
            {
                if (!_world.Cells.ContainsKey((gx, y, gz)))
                {
                    _world = _world with { Cells = _world.Cells.SetItem((gx, y, gz), _world.ActiveAsset) };
                    break;
                }
            }
        }
        else
        {
            PlaceOrEraseAt(gx, gz, _isErasing);
        }

        CapturePointer(e.Pointer);
        Invalidate();
        WorldChanged?.Invoke(_world);
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        _lastDragGX = -1;
        _lastDragGZ = -1;
        ReleasePointerCapture(e.Pointer);
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);
        int delta = pt.Properties.MouseWheelDelta > 0 ? 1 : -1;

        bool ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (ctrl)
        {
            Zoom += delta * ZoomStep;
        }
        else
        {
            int newLayer = Math.Clamp(_world.ActiveLayer + delta, 0, GridConstants.MaxHeight - 1);
            if (newLayer != _world.ActiveLayer)
            {
                _world = _world with { ActiveLayer = newLayer };
                Invalidate();
                WorldChanged?.Invoke(_world);
            }
        }
    }

    #endregion
}
