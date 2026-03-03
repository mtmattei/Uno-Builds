using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;

namespace DepthCardDemo.Controls;

/// <summary>
/// A control that renders a 3D mesh using WriteableBitmap with perspective projection.
/// Integrates with DepthCard's tilt system to rotate the 3D object based on card orientation.
/// </summary>
public partial class Simple3DObject : UserControl
{
    private Image? _image;
    private WriteableBitmap? _bitmap;
    private Mesh3D? _mesh;
    private Mesh3D? _originalMesh;
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private float _rotationZ = 0f;
    private DispatcherTimer? _animationTimer;
    private DispatcherTimer? _renderTimer;
    private bool _isDragging = false;
    private bool _isHovering = false;
    private Point _lastDragPosition;
    private float _targetRotationX = 0f;
    private float _targetRotationY = 0f;
    private DateTime _lastPointerUpdate = DateTime.MinValue;
    private const double POINTER_THROTTLE_MS = 16.67; // ~60fps

    // Parent card tracking
    private DepthCard? _parentDepthCard;

    #region Dependency Properties

    public static readonly DependencyProperty GeometryTypeProperty =
        DependencyProperty.Register(
            nameof(GeometryType),
            typeof(GeometryType),
            typeof(Simple3DObject),
            new PropertyMetadata(GeometryType.Cube, OnGeometryChanged));

    public GeometryType GeometryType
    {
        get => (GeometryType)GetValue(GeometryTypeProperty);
        set => SetValue(GeometryTypeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(Simple3DObject),
            new PropertyMetadata(100.0, OnGeometryChanged));

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty FaceColorProperty =
        DependencyProperty.Register(
            nameof(FaceColor),
            typeof(Windows.UI.Color),
            typeof(Simple3DObject),
            new PropertyMetadata(Windows.UI.Color.FromArgb(255, 100, 150, 255)));

    public Windows.UI.Color FaceColor
    {
        get => (Windows.UI.Color)GetValue(FaceColorProperty);
        set => SetValue(FaceColorProperty, value);
    }

    public static readonly DependencyProperty EnableAutoRotateProperty =
        DependencyProperty.Register(
            nameof(EnableAutoRotate),
            typeof(bool),
            typeof(Simple3DObject),
            new PropertyMetadata(false, OnAutoRotateChanged));

    public bool EnableAutoRotate
    {
        get => (bool)GetValue(EnableAutoRotateProperty);
        set => SetValue(EnableAutoRotateProperty, value);
    }

    public static readonly DependencyProperty AutoRotateSpeedProperty =
        DependencyProperty.Register(
            nameof(AutoRotateSpeed),
            typeof(double),
            typeof(Simple3DObject),
            new PropertyMetadata(1.0));

    public double AutoRotateSpeed
    {
        get => (double)GetValue(AutoRotateSpeedProperty);
        set => SetValue(AutoRotateSpeedProperty, value);
    }

    public static readonly DependencyProperty TiltRotationMultiplierProperty =
        DependencyProperty.Register(
            nameof(TiltRotationMultiplier),
            typeof(double),
            typeof(Simple3DObject),
            new PropertyMetadata(2.0));

    /// <summary>
    /// Multiplier for how much the DepthCard's tilt affects the 3D rotation.
    /// </summary>
    public double TiltRotationMultiplier
    {
        get => (double)GetValue(TiltRotationMultiplierProperty);
        set => SetValue(TiltRotationMultiplierProperty, value);
    }

    public static readonly DependencyProperty TextureTypeProperty =
        DependencyProperty.Register(
            nameof(TextureType),
            typeof(TextureType),
            typeof(Simple3DObject),
            new PropertyMetadata(TextureType.None));

    /// <summary>
    /// Type of procedural texture to apply to the mesh.
    /// </summary>
    public TextureType TextureType
    {
        get => (TextureType)GetValue(TextureTypeProperty);
        set => SetValue(TextureTypeProperty, value);
    }

    public static readonly DependencyProperty MetallicProperty =
        DependencyProperty.Register(
            nameof(Metallic),
            typeof(double),
            typeof(Simple3DObject),
            new PropertyMetadata(0.5));

    /// <summary>
    /// Metallic factor for PBR rendering (0 = dielectric, 1 = metallic).
    /// </summary>
    public double Metallic
    {
        get => (double)GetValue(MetallicProperty);
        set => SetValue(MetallicProperty, value);
    }

    public static readonly DependencyProperty RoughnessProperty =
        DependencyProperty.Register(
            nameof(Roughness),
            typeof(double),
            typeof(Simple3DObject),
            new PropertyMetadata(0.5));

    /// <summary>
    /// Roughness factor for PBR rendering (0 = smooth/glossy, 1 = rough/matte).
    /// </summary>
    public double Roughness
    {
        get => (double)GetValue(RoughnessProperty);
        set => SetValue(RoughnessProperty, value);
    }

    #endregion

    public Simple3DObject()
    {
        InitializeMesh();

        _image = new Image
        {
            Stretch = Stretch.Fill
        };
        Content = _image;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;

        // Enable drag interaction
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCaptureLost += OnPointerCaptureLost;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeBitmap();

        // Subscribe to DepthCard tilt changes
        _parentDepthCard = FindParentDepthCard(this);
        if (_parentDepthCard != null)
        {
            _parentDepthCard.TiltChanged += OnDepthCardTiltChanged;
        }

        // Start render loop
        StartRenderLoop();

        // Start auto-rotation if enabled
        if (EnableAutoRotate)
        {
            StartAutoRotation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe from DepthCard events
        if (_parentDepthCard != null)
        {
            _parentDepthCard.TiltChanged -= OnDepthCardTiltChanged;
        }

        StopRenderLoop();
        StopAutoRotation();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        InitializeBitmap();
        RenderFrame();
    }

    private void InitializeBitmap()
    {
        var width = Math.Max(1, (int)ActualWidth);
        var height = Math.Max(1, (int)ActualHeight);

        if (width > 0 && height > 0)
        {
            _bitmap = new WriteableBitmap(width, height);
            if (_image != null)
            {
                _image.Source = _bitmap;
            }
        }
    }

    private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple3DObject obj)
        {
            obj.InitializeMesh();
            obj.RenderFrame();
        }
    }

    private static void OnAutoRotateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple3DObject obj)
        {
            if ((bool)e.NewValue)
                obj.StartAutoRotation();
            else
                obj.StopAutoRotation();
        }
    }

    private void InitializeMesh()
    {
        var size = (float)Size;
        _originalMesh = GeometryType switch
        {
            GeometryType.Cube => Mesh3D.CreateCube(size),
            GeometryType.Pyramid => Mesh3D.CreatePyramid(size),
            GeometryType.Octahedron => Mesh3D.CreateOctahedron(size),
            GeometryType.Diamond => Mesh3D.CreateDiamond(size),
            GeometryType.Icosahedron => Mesh3D.CreateIcosahedron(size),
            _ => Mesh3D.CreateCube(size)
        };
    }

    private DepthCard? FindParentDepthCard(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is DepthCard card)
                return card;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private void OnDepthCardTiltChanged(object? sender, DepthCardTiltChangedEventArgs e)
    {
        // Skip entirely if tilt rotation is disabled
        if (TiltRotationMultiplier == 0)
            return;

        // Ignore card tilt when user is interacting with the 3D object
        // This prevents jitter when hovering/dragging
        if (_isHovering || _isDragging)
            return;

        // Map tilt to rotation
        _rotationX = (float)(-e.RotateX * TiltRotationMultiplier);
        _rotationY = (float)(e.RotateY * TiltRotationMultiplier);
    }

    private void StartAutoRotation()
    {
        if (_animationTimer != null) return;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60fps
        };
        _animationTimer.Tick += (s, e) =>
        {
            if (!_isDragging)
            {
                _rotationZ += (float)(0.5 * AutoRotateSpeed);
                if (_rotationZ > 360) _rotationZ -= 360;

                // Smooth interpolation toward target
                _rotationX += (_targetRotationX - _rotationX) * 0.05f;
                _rotationY += (_targetRotationY - _rotationY) * 0.05f;
            }
        };
        _animationTimer.Start();
    }

    private void StopAutoRotation()
    {
        _animationTimer?.Stop();
        _animationTimer = null;
    }

    private void StartRenderLoop()
    {
        if (_renderTimer != null) return;

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60fps
        };
        _renderTimer.Tick += (s, e) => RenderFrame();
        _renderTimer.Start();
    }

    private void StopRenderLoop()
    {
        _renderTimer?.Stop();
        _renderTimer = null;
    }

    #region Texture and PBR Helpers

    private Windows.UI.Color GetTextureColor(float u, float v, Vector3 worldPos)
    {
        var baseColor = FaceColor;

        return TextureType switch
        {
            TextureType.Checkerboard => GetCheckerboard(u, v),
            TextureType.Gradient => GetGradient(u, v),
            TextureType.Noise => GetNoise(worldPos),
            TextureType.PBR => baseColor, // Base color for PBR, lighting applied separately
            _ => baseColor
        };
    }

    private Windows.UI.Color GetCheckerboard(float u, float v)
    {
        int size = 4;
        bool checkU = ((int)(u * size) % 2) == 0;
        bool checkV = ((int)(v * size) % 2) == 0;
        bool isWhite = checkU ^ checkV;

        var color1 = FaceColor;
        var color2 = Windows.UI.Color.FromArgb(
            color1.A,
            (byte)(color1.R * 0.3),
            (byte)(color1.G * 0.3),
            (byte)(color1.B * 0.3)
        );

        return isWhite ? color1 : color2;
    }

    private Windows.UI.Color GetGradient(float u, float v)
    {
        var color1 = FaceColor;
        var color2 = Windows.UI.Color.FromArgb(
            color1.A,
            (byte)(color1.R * 0.5),
            (byte)(color1.G * 0.5),
            (byte)(color1.B * 1.2)
        );

        float t = (u + v) * 0.5f;
        return LerpColor(color2, color1, t);
    }

    private Windows.UI.Color GetNoise(Vector3 worldPos)
    {
        float noise = SimplexNoise(worldPos.X * 0.1f, worldPos.Y * 0.1f, worldPos.Z * 0.1f);
        noise = (noise + 1f) * 0.5f; // Remap from [-1,1] to [0,1]

        var color = FaceColor;
        float intensity = 0.7f + noise * 0.3f;

        return Windows.UI.Color.FromArgb(
            color.A,
            (byte)(color.R * intensity),
            (byte)(color.G * intensity),
            (byte)(color.B * intensity)
        );
    }

    private float SimplexNoise(float x, float y, float z)
    {
        // Simple 3D noise approximation using sine waves
        float noise = MathF.Sin(x * 5.2f) * MathF.Cos(y * 5.7f) +
                      MathF.Sin(y * 4.3f) * MathF.Cos(z * 4.8f) +
                      MathF.Sin(z * 5.1f) * MathF.Cos(x * 5.5f);
        return noise * 0.33f;
    }

    private (byte r, byte g, byte b) CalculatePBRLighting(Vector3 normal, Vector3 viewDir, Vector3 lightDir, Windows.UI.Color albedo)
    {
        // PBR lighting using Cook-Torrance BRDF
        float metallic = (float)Metallic;
        float roughness = (float)Roughness;

        // Ambient
        float ambient = 0.2f;

        // Diffuse (Lambert)
        float NdotL = Math.Max(0, Vector3.Dot(normal, lightDir));
        float diffuse = NdotL * (1f - metallic);

        // Specular (Blinn-Phong approximation of Cook-Torrance)
        Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
        float NdotH = Math.Max(0, Vector3.Dot(normal, halfDir));
        float shininess = (1f - roughness) * 128f + 1f;
        float specular = MathF.Pow(NdotH, shininess) * (1f - roughness);

        // Fresnel approximation
        float NdotV = Math.Max(0, Vector3.Dot(normal, viewDir));
        float fresnel = metallic + (1f - metallic) * MathF.Pow(1f - NdotV, 5f);

        // Combine
        float totalLight = ambient + diffuse + specular * fresnel;
        totalLight = Math.Clamp(totalLight, 0f, 2f);

        return (
            (byte)(albedo.R * totalLight),
            (byte)(albedo.G * totalLight),
            (byte)(albedo.B * totalLight)
        );
    }

    private Windows.UI.Color LerpColor(Windows.UI.Color a, Windows.UI.Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Windows.UI.Color.FromArgb(
            (byte)(a.A + (b.A - a.A) * t),
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t)
        );
    }

    #endregion

    private void RenderFrame()
    {
        if (_bitmap == null || _originalMesh == null) return;

        var width = _bitmap.PixelWidth;
        var height = _bitmap.PixelHeight;

        if (width <= 0 || height <= 0) return;

        // Clone and transform mesh
        _mesh = _originalMesh.Clone();

        // Build rotation matrix
        var rotX = Matrix4x4.CreateRotationX(MathF.PI * _rotationX / 180f);
        var rotY = Matrix4x4.CreateRotationY(MathF.PI * _rotationY / 180f);
        var rotZ = Matrix4x4.CreateRotationZ(MathF.PI * _rotationZ / 180f);
        var rotation = rotX * rotY * rotZ;

        _mesh.Transform(rotation);

        // Perspective projection parameters
        var centerX = width / 2f;
        var centerY = height / 2f;
        var fov = 500f; // Field of view distance

        // Project vertices to 2D
        var projectedVertices = new List<Point>();
        foreach (var vertex in _mesh.Vertices)
        {
            var z = vertex.Z + fov;
            if (z <= 0) z = 1; // Avoid division by zero

            var scale = fov / z;
            var x = centerX + vertex.X * scale;
            var y = centerY - vertex.Y * scale; // Flip Y for screen coordinates

            projectedVertices.Add(new Point(x, y));
        }

        // Calculate face depths and normals for z-sorting and back-face culling
        var faceDepths = new List<(Face3D face, float depth, Vector3 normal)>();
        foreach (var face in _mesh.Faces)
        {
            var v0 = _mesh.Vertices[face.V0].ToVector3();
            var v1 = _mesh.Vertices[face.V1].ToVector3();
            var v2 = _mesh.Vertices[face.V2].ToVector3();

            // Calculate average Z for depth sorting
            var avgZ = (v0.Z + v1.Z + v2.Z) / 3f;

            // Calculate face normal for back-face culling and lighting
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            faceDepths.Add((face, avgZ, normal));
        }

        // Sort faces by depth (painter's algorithm - far to near)
        faceDepths.Sort((a, b) => a.depth.CompareTo(b.depth));

        // Get pixel buffer
        using var stream = _bitmap.PixelBuffer.AsStream();
        var pixels = new byte[width * height * 4];

        // Clear to transparent
        Array.Fill<byte>(pixels, 0);

        // Render faces with texture/material support
        var baseColor = FaceColor;
        var lightDirection = Vector3.Normalize(new Vector3(1, 1, 2));
        var viewDirection = new Vector3(0, 0, 1);
        bool hasTexture = TextureType != TextureType.None;

        foreach (var (face, depth, normal) in faceDepths)
        {
            // Back-face culling
            if (Vector3.Dot(normal, viewDirection) <= 0)
                continue; // Skip back-facing triangles

            // Get vertices
            var v0 = _mesh.Vertices[face.V0];
            var v1 = _mesh.Vertices[face.V1];
            var v2 = _mesh.Vertices[face.V2];

            // Get triangle points
            var p0 = projectedVertices[face.V0];
            var p1 = projectedVertices[face.V1];
            var p2 = projectedVertices[face.V2];

            // Calculate triangle center for texture sampling
            var centerWorld = new Vector3(
                (v0.X + v1.X + v2.X) / 3f,
                (v0.Y + v1.Y + v2.Y) / 3f,
                (v0.Z + v1.Z + v2.Z) / 3f
            );

            // Calculate average UV
            float avgU = (v0.U + v1.U + v2.U) / 3f;
            float avgV = (v0.V + v1.V + v2.V) / 3f;

            // Get texture color
            var texColor = hasTexture ? GetTextureColor(avgU, avgV, centerWorld) : baseColor;

            // Apply lighting
            byte faceR, faceG, faceB;
            if (TextureType == TextureType.PBR)
            {
                var pbr = CalculatePBRLighting(normal, viewDirection, lightDirection, texColor);
                faceR = pbr.r;
                faceG = pbr.g;
                faceB = pbr.b;
            }
            else
            {
                // Simple lighting for non-PBR textures
                var lightIntensity = Math.Max(0, Vector3.Dot(normal, lightDirection));
                var ambientLight = 0.3f;
                var totalLight = Math.Clamp(ambientLight + lightIntensity * 0.7f, 0, 1);
                faceR = (byte)(texColor.R * totalLight);
                faceG = (byte)(texColor.G * totalLight);
                faceB = (byte)(texColor.B * totalLight);
            }

            // Fill triangle if textured
            if (hasTexture)
            {
                var faceAlpha = (byte)(texColor.A * 0.9); // Slightly transparent for depth
                FillTriangle(pixels, width, height, p0, p1, p2, faceR, faceG, faceB, faceAlpha);
            }

            // Draw wireframe edges (always)
            var edgeAlpha = (byte)(baseColor.A * (hasTexture ? 0.8 : 1.0)); // Subtle edges when textured
            DrawLine(pixels, width, height, p0, p1, faceR, faceG, faceB, edgeAlpha);
            DrawLine(pixels, width, height, p1, p2, faceR, faceG, faceB, edgeAlpha);
            DrawLine(pixels, width, height, p2, p0, faceR, faceG, faceB, edgeAlpha);
        }

        // Write pixels to bitmap
        stream.Seek(0, System.IO.SeekOrigin.Begin);
        stream.Write(pixels, 0, pixels.Length);
        _bitmap.Invalidate();
    }

    #region Drag Interaction

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        _isHovering = true;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        _isHovering = false;
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
        _lastDragPosition = e.GetCurrentPoint(this).Position;
        CapturePointer(e.Pointer);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            // Throttle pointer events to ~60fps for smooth rendering
            var now = DateTime.UtcNow;
            if ((now - _lastPointerUpdate).TotalMilliseconds < POINTER_THROTTLE_MS)
                return;

            _lastPointerUpdate = now;

            var currentPosition = e.GetCurrentPoint(this).Position;
            var deltaX = currentPosition.X - _lastDragPosition.X;
            var deltaY = currentPosition.Y - _lastDragPosition.Y;

            // Drag sensitivity: 0.01 radians per pixel
            _targetRotationY += (float)(deltaX * 0.01 * 180 / Math.PI);
            _targetRotationX -= (float)(deltaY * 0.01 * 180 / Math.PI);

            // Apply directly during drag
            _rotationY = _targetRotationY;
            _rotationX = _targetRotationX;

            _lastDragPosition = currentPosition;
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        ReleasePointerCapture(e.Pointer);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    #endregion

    private void FillTriangle(byte[] pixels, int width, int height, Point p0, Point p1, Point p2, byte r, byte g, byte b, byte a)
    {
        // Simple scanline triangle rasterization
        var points = new[] { p0, p1, p2 }.OrderBy(p => p.Y).ToArray();
        var y0 = points[0];
        var y1 = points[1];
        var y2 = points[2];

        void DrawScanline(int y, double x1, double x2)
        {
            if (y < 0 || y >= height) return;

            var startX = (int)Math.Ceiling(Math.Min(x1, x2));
            var endX = (int)Math.Floor(Math.Max(x1, x2));

            startX = Math.Max(0, startX);
            endX = Math.Min(width - 1, endX);

            for (int x = startX; x <= endX; x++)
            {
                var index = (y * width + x) * 4;
                pixels[index] = b;     // B
                pixels[index + 1] = g; // G
                pixels[index + 2] = r; // R
                pixels[index + 3] = a; // A
            }
        }

        // Top half
        var dy1 = y1.Y - y0.Y;
        var dy2 = y2.Y - y0.Y;

        if (Math.Abs(dy2) > 0.1)
        {
            for (double y = Math.Ceiling(y0.Y); y <= Math.Floor(y1.Y) && y < height; y++)
            {
                if (y < 0) continue;

                var t1 = (y - y0.Y) / dy2;
                var x1 = y0.X + (y2.X - y0.X) * t1;

                var x2 = y0.X;
                if (Math.Abs(dy1) > 0.1)
                {
                    var t2 = (y - y0.Y) / dy1;
                    x2 = y0.X + (y1.X - y0.X) * t2;
                }

                DrawScanline((int)y, x1, x2);
            }
        }

        // Bottom half
        var dy3 = y2.Y - y1.Y;

        if (Math.Abs(dy2) > 0.1 && Math.Abs(dy3) > 0.1)
        {
            for (double y = Math.Ceiling(y1.Y); y <= Math.Floor(y2.Y) && y < height; y++)
            {
                if (y < 0) continue;

                var t1 = (y - y0.Y) / dy2;
                var x1 = y0.X + (y2.X - y0.X) * t1;

                var t2 = (y - y1.Y) / dy3;
                var x2 = y1.X + (y2.X - y1.X) * t2;

                DrawScanline((int)y, x1, x2);
            }
        }
    }

    private void DrawLine(byte[] pixels, int width, int height, Point p0, Point p1, byte r, byte g, byte b, byte a)
    {
        // Draw thicker line (2px thick) by drawing multiple parallel lines
        int thickness = 2;

        // Calculate perpendicular offset direction
        double dx = p1.X - p0.X;
        double dy = p1.Y - p0.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);

        if (length == 0) return;

        // Normalize and get perpendicular
        double nx = -dy / length;
        double ny = dx / length;

        // Draw multiple parallel lines for thickness
        for (int t = 0; t < thickness; t++)
        {
            double offset = (t - thickness / 2.0 + 0.5);
            Point pp0 = new Point(p0.X + nx * offset, p0.Y + ny * offset);
            Point pp1 = new Point(p1.X + nx * offset, p1.Y + ny * offset);
            DrawSingleLine(pixels, width, height, pp0, pp1, r, g, b, a);
        }
    }

    private void DrawSingleLine(byte[] pixels, int width, int height, Point p0, Point p1, byte r, byte g, byte b, byte a)
    {
        // Bresenham's line algorithm with alpha blending
        int x0 = (int)p0.X;
        int y0 = (int)p0.Y;
        int x1 = (int)p1.X;
        int y1 = (int)p1.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Plot pixel with bounds checking
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
            {
                int index = (y0 * width + x0) * 4;

                // Alpha blending
                float alpha = a / 255f;
                float invAlpha = 1f - alpha;

                pixels[index] = (byte)(b * alpha + pixels[index] * invAlpha);     // B
                pixels[index + 1] = (byte)(g * alpha + pixels[index + 1] * invAlpha); // G
                pixels[index + 2] = (byte)(r * alpha + pixels[index + 2] * invAlpha); // R
                pixels[index + 3] = (byte)Math.Min(255, pixels[index + 3] + a);        // A
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}

/// <summary>
/// Supported 3D geometry types.
/// </summary>
public enum GeometryType
{
    Cube,
    Pyramid,
    Octahedron,
    Diamond,
    Icosahedron
}

/// <summary>
/// Procedural texture types for material rendering.
/// </summary>
public enum TextureType
{
    None,
    Checkerboard,
    Gradient,
    Noise,
    PBR
}
