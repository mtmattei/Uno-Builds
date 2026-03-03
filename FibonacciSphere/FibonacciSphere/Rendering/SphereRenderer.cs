using System;
using System.Collections.Generic;
using System.Numerics;
using FibonacciSphere.Helpers;
using FibonacciSphere.Math;
using FibonacciSphere.Models;
using SkiaSharp;

namespace FibonacciSphere.Rendering;

/// <summary>
/// Main renderer for the Fibonacci sphere visualization.
/// </summary>
public class SphereRenderer : IDisposable
{
    private readonly Camera3D _camera;
    private readonly TrailRenderer _trailRenderer;
    private readonly SKPaint _pointPaint;
    private readonly SKPaint _glowPaint;
    private readonly SKPaint _selectedPaint;

    private List<SpherePoint> _points;
    private SphereSettings _settings;
    private float _rotationAngleY;
    private float _rotationAngleX;
    private float _time;
    private Vector2 _screenSize;

    public List<SpherePoint> Points => _points;
    public Camera3D Camera => _camera;

    public SphereRenderer()
    {
        _camera = new Camera3D();
        _trailRenderer = new TrailRenderer();
        _points = new List<SpherePoint>();
        _settings = new SphereSettings();

        _pointPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _glowPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8f)
        };

        _selectedPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true,
            Color = SKColors.White
        };

        GeneratePoints(_settings);
    }

    /// <summary>
    /// Regenerates all points based on current settings.
    /// </summary>
    public void GeneratePoints(SphereSettings settings)
    {
        _settings = settings;
        _camera.SetDistance(settings.CameraDistance);

        // Generate positions based on selected shape
        var positions = settings.Shape switch
        {
            ShapeType.UnoLogo => UnoLogoDistribution.GenerateUnoLogoParametric(settings.PointCount),
            _ => FibonacciDistribution.GenerateFibonacciSphere(settings.PointCount)
        };

        var phases = FibonacciDistribution.GenerateWobblePhases(settings.PointCount);
        var random = new Random(42);

        _points = new List<SpherePoint>(settings.PointCount);

        for (int i = 0; i < settings.PointCount; i++)
        {
            // Generate color gradient based on position
            float t = (float)i / (settings.PointCount - 1);
            var color = InterpolateColor(
                new SKColor(settings.PrimaryColor),
                new SKColor(settings.SecondaryColor),
                settings.UseGradientColors ? t : 0f);

            // Random wobble direction for random mode
            var randomDir = Vector3.Normalize(new Vector3(
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1)));

            _points.Add(new SpherePoint
            {
                Index = i,
                BasePosition = positions[i],
                CurrentPosition = positions[i],
                WobblePhase = phases[i],
                RandomWobbleDirection = randomDir,
                Size = settings.BasePointSize,
                Color = color,
                MaxTrailLength = settings.TrailLength
            });
        }
    }

    /// <summary>
    /// Updates settings without regenerating points (for live adjustments).
    /// </summary>
    public void UpdateSettings(SphereSettings settings)
    {
        // Regenerate if shape, point count, or colors changed
        if (settings.Shape != _settings.Shape ||
            settings.PointCount != _settings.PointCount ||
            settings.PrimaryColor != _settings.PrimaryColor ||
            settings.SecondaryColor != _settings.SecondaryColor ||
            settings.UseGradientColors != _settings.UseGradientColors)
        {
            GeneratePoints(settings);
            return;
        }

        _settings = settings;
        _camera.SetDistance(settings.CameraDistance);

        // Update trail length for existing points
        foreach (var point in _points)
        {
            point.MaxTrailLength = settings.TrailLength;
            if (settings.TrailLength == 0)
            {
                point.ClearTrail();
            }
        }
    }

    /// <summary>
    /// Updates animation state.
    /// </summary>
    public void Update(float deltaTime)
    {
        _time += deltaTime;

        // Auto-rotation
        if (_settings.IsRotating)
        {
            float direction = _settings.RotateClockwise ? 1f : -1f;
            _rotationAngleY += _settings.RotationSpeed * deltaTime * direction;
        }

        // Pre-calculate values used in loop
        float baseSize = _settings.BasePointSize;
        float sizeVariation = _settings.SizeVariation;
        bool hasSizeVariation = sizeVariation > 0;
        bool hasPulse = _settings.PulseSpeed > 0 && _settings.PulseAmount > 0;
        float pulseTimeComponent = _time * _settings.PulseSpeed;
        float pulseAmount = _settings.PulseAmount;
        bool hasWobble = _settings.WobbleAmplitude > 0;
        float wobbleTimeComponent = _time * _settings.WobbleFrequency;
        float wobbleAmplitude = _settings.WobbleAmplitude;
        var wobbleAxis = _settings.WobbleAxis;

        // Update each point
        foreach (var point in _points)
        {
            // Apply combined Y then X rotation (more efficient than two separate calls)
            var rotated = point.BasePosition.RotateYX(_rotationAngleY, _rotationAngleX);

            // Apply wobble effect (inlined for performance)
            if (hasWobble)
            {
                float wobble = MathF.Sin(wobbleTimeComponent + point.WobblePhase);
                float offset = wobble * wobbleAmplitude;

                rotated = wobbleAxis switch
                {
                    WobbleAxis.Radial => rotated * (1f + offset),
                    WobbleAxis.Tangential => ApplyTangentialWobble(rotated, offset),
                    WobbleAxis.Random => rotated + point.RandomWobbleDirection * offset,
                    _ => rotated
                };
            }

            // Calculate size with pre-computed values
            float finalSize = baseSize;
            if (hasSizeVariation)
            {
                finalSize += MathF.Sin(point.Index * 0.5f) * sizeVariation;
            }
            if (hasPulse)
            {
                float pulse = MathF.Sin(pulseTimeComponent + point.WobblePhase);
                finalSize *= 1f + pulse * pulseAmount;
            }

            point.CurrentPosition = rotated;
            point.Size = finalSize;
        }
    }

    private Vector3 ApplyTangentialWobble(Vector3 position, float offset)
    {
        // Calculate tangent vector (perpendicular to radius and up)
        var radial = Vector3.Normalize(position);
        var tangent = Vector3.Cross(radial, Vector3.UnitY);
        if (tangent.LengthSquared() < 0.001f)
        {
            tangent = Vector3.Cross(radial, Vector3.UnitX);
        }
        tangent = Vector3.Normalize(tangent);

        return position + tangent * offset;
    }

    /// <summary>
    /// Renders the sphere to the canvas.
    /// </summary>
    public void Render(SKCanvas canvas, int width, int height)
    {
        _screenSize = new Vector2(width, height);
        _camera.SetScreenSize(width, height);

        // Clear with subtle grey background
        canvas.Clear(new SKColor(0xFF101010)); // Subtle grey from palette

        // Project all points and sort by depth (back to front)
        var projectedPoints = new List<(SpherePoint point, Vector2 screen, float depth)>();

        foreach (var point in _points)
        {
            var (screenPos, depth) = _camera.ProjectToScreen(point.CurrentPosition, _screenSize);
            point.Depth = depth;

            // Add to trail history
            if (_settings.TrailLength > 0)
            {
                point.AddToTrail(screenPos);
            }

            projectedPoints.Add((point, screenPos, depth));
        }

        // Sort by depth (render far points first) - in-place sort for efficiency
        projectedPoints.Sort((a, b) => b.depth.CompareTo(a.depth));

        // Render trails first (behind points)
        foreach (var (point, _, _) in projectedPoints)
        {
            _trailRenderer.RenderTrail(canvas, point, _settings);
        }

        // Render points
        foreach (var (point, screenPos, depth) in projectedPoints)
        {
            RenderPoint(canvas, point, screenPos, depth);
        }
    }

    private void RenderPoint(SKCanvas canvas, SpherePoint point, Vector2 screenPos, float depth)
    {
        // Calculate size based on depth
        float size = point.Size;
        if (_settings.DepthScaling)
        {
            // Depth is typically in range [-1, 1], map to size multiplier
            float depthFactor = 1f - (depth + 1f) * 0.25f; // 0.5 to 1.0 range
            size *= depthFactor;
        }

        // Calculate alpha based on depth
        float alpha = _settings.DepthScaling
            ? 0.5f + (1f - depth) * 0.25f // 0.5 to 1.0 range
            : 1f;

        var color = point.Color.WithAlpha((byte)(alpha * 255));

        // Render glow for hovered points
        if (point.IsHovered || point.IsSelected)
        {
            _glowPaint.Color = color.WithAlpha(100);
            canvas.DrawCircle(screenPos.X, screenPos.Y, size * 2f, _glowPaint);
        }

        // Render main point
        _pointPaint.Color = color;
        canvas.DrawCircle(screenPos.X, screenPos.Y, size, _pointPaint);

        // Render selection ring
        if (point.IsSelected)
        {
            canvas.DrawCircle(screenPos.X, screenPos.Y, size + 4f, _selectedPaint);
        }
    }

    /// <summary>
    /// Manually rotates the sphere by the given delta angles.
    /// </summary>
    public void ManualRotate(float deltaYaw, float deltaPitch)
    {
        _rotationAngleY += deltaYaw;
        _rotationAngleX += deltaPitch;

        // Clamp pitch to prevent gimbal lock issues
        _rotationAngleX = System.Math.Clamp(_rotationAngleX, -MathF.PI / 2f + 0.1f, MathF.PI / 2f - 0.1f);
    }

    /// <summary>
    /// Performs hit testing to find the point at the given screen position.
    /// </summary>
    public SpherePoint? HitTest(Vector2 screenPosition, float tolerance = 20f)
    {
        return HitTesting.FindNearestPoint(_points, screenPosition, _screenSize, _camera, tolerance);
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void ClearSelection()
    {
        foreach (var point in _points)
        {
            point.IsSelected = false;
        }
    }

    /// <summary>
    /// Clears all hover states.
    /// </summary>
    public void ClearHover()
    {
        foreach (var point in _points)
        {
            point.IsHovered = false;
        }
    }

    /// <summary>
    /// Resets the sphere to initial state.
    /// </summary>
    public void Reset()
    {
        _rotationAngleX = 0;
        _rotationAngleY = 0;
        _time = 0;
        ClearSelection();
        ClearHover();
        foreach (var point in _points)
        {
            point.ClearTrail();
        }
    }

    private static SKColor InterpolateColor(SKColor a, SKColor b, float t)
    {
        return new SKColor(
            (byte)(a.Red + (b.Red - a.Red) * t),
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Blue + (b.Blue - a.Blue) * t),
            (byte)(a.Alpha + (b.Alpha - a.Alpha) * t)
        );
    }

    public void Dispose()
    {
        _pointPaint.Dispose();
        _glowPaint.Dispose();
        _selectedPaint.Dispose();
        _trailRenderer.Dispose();
    }
}
