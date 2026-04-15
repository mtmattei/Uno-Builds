using System;
using SkiaSharp;
using VoxelWarehouse.Models;

namespace VoxelWarehouse.Controls;

/// <summary>
/// SkiaSharp drawing utilities for the hand tracking overlay.
/// Renders the 21-point hand skeleton, bone connections, gesture highlights,
/// and a targeting reticle at the hand centroid.
/// </summary>
public static class HandSkeletonRenderer
{
    private const float DotRadius = 3f;
    private const float HighlightDotRadius = 6f;
    private const float BoneWidth = 1.5f;
    private const float ReticleRadius = 18f;
    private const float ReticleInnerRadius = 4f;

    private static readonly SKColor DotColor = new(255, 255, 255, 179);       // white 70%
    private static readonly SKColor BoneColor = new(255, 255, 255, 77);        // white 30%
    private static readonly SKColor HighlightColor = new(100, 220, 255, 230);  // cyan highlight
    private static readonly SKColor ReticleColor = new(100, 220, 255, 140);    // cyan reticle
    private static readonly SKColor ReticleFillColor = new(100, 220, 255, 25); // subtle fill
    private static readonly SKColor ConfidenceBarBg = new(255, 255, 255, 20);
    private static readonly SKColor ConfidenceBarFg = new(100, 220, 255, 140);

    /// <summary>
    /// Bone connections as pairs of landmark indices.
    /// 5 finger chains + palm connections.
    /// </summary>
    private static readonly (int From, int To)[] BoneConnections =
    [
        // Thumb
        (0, 1), (1, 2), (2, 3), (3, 4),
        // Index
        (0, 5), (5, 6), (6, 7), (7, 8),
        // Middle
        (0, 9), (9, 10), (10, 11), (11, 12),
        // Ring
        (0, 13), (13, 14), (14, 15), (15, 16),
        // Pinky
        (0, 17), (17, 18), (18, 19), (19, 20),
        // Palm cross-connections
        (5, 9), (9, 13), (13, 17),
    ];

    /// <summary>
    /// Draws the complete hand tracking overlay on the given canvas.
    /// </summary>
    /// <param name="canvas">SkiaSharp canvas to draw on.</param>
    /// <param name="result">Hand tracking result with landmarks.</param>
    /// <param name="canvasWidth">Width of the drawing surface in pixels.</param>
    /// <param name="canvasHeight">Height of the drawing surface in pixels.</param>
    public static void Draw(SKCanvas canvas, HandTrackingResult result, float canvasWidth, float canvasHeight)
    {
        if (!result.HandDetected || result.Landmarks.Length < 21)
            return;

        var landmarks = result.Landmarks;

        // Convert normalized landmarks to screen coordinates
        Span<SKPoint> points = stackalloc SKPoint[21];
        for (int i = 0; i < 21; i++)
        {
            points[i] = new SKPoint(
                landmarks[i].X * canvasWidth,
                landmarks[i].Y * canvasHeight);
        }

        DrawBones(canvas, points);
        DrawLandmarkDots(canvas, points);
        DrawGestureHighlights(canvas, points, result.Gesture);
        DrawReticle(canvas, result.CursorX * canvasWidth, result.CursorY * canvasHeight);
        DrawGestureLabel(canvas, result.Gesture, result.CursorX * canvasWidth, result.CursorY * canvasHeight);
        DrawConfidenceBar(canvas, result.Confidence, canvasWidth, canvasHeight);
    }

    /// <summary>
    /// Draws bone connections between landmark pairs.
    /// </summary>
    private static void DrawBones(SKCanvas canvas, ReadOnlySpan<SKPoint> points)
    {
        using var paint = new SKPaint
        {
            Color = BoneColor,
            StrokeWidth = BoneWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        foreach (var (from, to) in BoneConnections)
        {
            canvas.DrawLine(points[from], points[to], paint);
        }
    }

    /// <summary>
    /// Draws dots at each of the 21 landmark positions.
    /// </summary>
    private static void DrawLandmarkDots(SKCanvas canvas, ReadOnlySpan<SKPoint> points)
    {
        using var paint = new SKPaint
        {
            Color = DotColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        for (int i = 0; i < 21; i++)
        {
            canvas.DrawCircle(points[i], DotRadius, paint);
        }
    }

    /// <summary>
    /// Highlights landmarks relevant to the active gesture.
    /// Pinch: larger dots on thumb tip (4) and index tip (8).
    /// Point: larger dot on index tip (8).
    /// Fist: glow on wrist (0).
    /// </summary>
    private static void DrawGestureHighlights(SKCanvas canvas, ReadOnlySpan<SKPoint> points, GestureType gesture)
    {
        using var paint = new SKPaint
        {
            Color = HighlightColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var glowPaint = new SKPaint
        {
            Color = new SKColor(HighlightColor.Red, HighlightColor.Green, HighlightColor.Blue, 40),
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f)
        };

        switch (gesture)
        {
            case GestureType.Pinch:
                // Highlight thumb and index tips with glow
                canvas.DrawCircle(points[(int)HandLandmarkId.ThumbTip], HighlightDotRadius + 3f, glowPaint);
                canvas.DrawCircle(points[(int)HandLandmarkId.IndexTip], HighlightDotRadius + 3f, glowPaint);
                canvas.DrawCircle(points[(int)HandLandmarkId.ThumbTip], HighlightDotRadius, paint);
                canvas.DrawCircle(points[(int)HandLandmarkId.IndexTip], HighlightDotRadius, paint);

                // Draw connecting line between thumb and index
                using (var linePaint = new SKPaint
                {
                    Color = HighlightColor,
                    StrokeWidth = 2f,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true,
                    PathEffect = SKPathEffect.CreateDash([4f, 3f], 0)
                })
                {
                    canvas.DrawLine(
                        points[(int)HandLandmarkId.ThumbTip],
                        points[(int)HandLandmarkId.IndexTip],
                        linePaint);
                }
                break;

            case GestureType.Point:
                canvas.DrawCircle(points[(int)HandLandmarkId.IndexTip], HighlightDotRadius + 3f, glowPaint);
                canvas.DrawCircle(points[(int)HandLandmarkId.IndexTip], HighlightDotRadius, paint);
                break;

            case GestureType.Fist:
                canvas.DrawCircle(points[(int)HandLandmarkId.Wrist], HighlightDotRadius + 4f, glowPaint);
                canvas.DrawCircle(points[(int)HandLandmarkId.Wrist], HighlightDotRadius, paint);
                break;

            case GestureType.Open:
                // Subtle highlight on all fingertips
                using (var tipPaint = new SKPaint
                {
                    Color = new SKColor(HighlightColor.Red, HighlightColor.Green, HighlightColor.Blue, 100),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawCircle(points[(int)HandLandmarkId.ThumbTip], DotRadius + 2f, tipPaint);
                    canvas.DrawCircle(points[(int)HandLandmarkId.IndexTip], DotRadius + 2f, tipPaint);
                    canvas.DrawCircle(points[(int)HandLandmarkId.MiddleTip], DotRadius + 2f, tipPaint);
                    canvas.DrawCircle(points[(int)HandLandmarkId.RingTip], DotRadius + 2f, tipPaint);
                    canvas.DrawCircle(points[(int)HandLandmarkId.PinkyTip], DotRadius + 2f, tipPaint);
                }
                break;
        }
    }

    /// <summary>
    /// Draws a targeting reticle at the cursor position (hand centroid).
    /// </summary>
    private static void DrawReticle(SKCanvas canvas, float cx, float cy)
    {
        // Outer ring
        using var ringPaint = new SKPaint
        {
            Color = ReticleColor,
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };
        canvas.DrawCircle(cx, cy, ReticleRadius, ringPaint);

        // Subtle fill
        using var fillPaint = new SKPaint
        {
            Color = ReticleFillColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawCircle(cx, cy, ReticleRadius, fillPaint);

        // Inner dot
        using var innerPaint = new SKPaint
        {
            Color = ReticleColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawCircle(cx, cy, ReticleInnerRadius, innerPaint);

        // Crosshair lines (short segments outside the ring)
        using var crossPaint = new SKPaint
        {
            Color = ReticleColor,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        float gap = ReticleRadius + 4f;
        float lineLen = 8f;

        canvas.DrawLine(cx - gap - lineLen, cy, cx - gap, cy, crossPaint);
        canvas.DrawLine(cx + gap, cy, cx + gap + lineLen, cy, crossPaint);
        canvas.DrawLine(cx, cy - gap - lineLen, cx, cy - gap, crossPaint);
        canvas.DrawLine(cx, cy + gap, cx, cy + gap + lineLen, crossPaint);
    }

    /// <summary>
    /// Draws a small label showing the current gesture name below the reticle.
    /// </summary>
    private static void DrawGestureLabel(SKCanvas canvas, GestureType gesture, float cx, float cy)
    {
        if (gesture == GestureType.None)
            return;

        string label = gesture switch
        {
            GestureType.Open => "MOVE",
            GestureType.Pinch => "PLACE",
            GestureType.Fist => "ERASE",
            GestureType.Point => "SELECT",
            _ => ""
        };

        if (string.IsNullOrEmpty(label))
            return;

        using var font = new SKFont(SKTypeface.FromFamilyName("Cascadia Code", SKFontStyle.Bold), 9);
        using var paint = new SKPaint
        {
            Color = ReticleColor,
            IsAntialias = true
        };

        float labelY = cy + ReticleRadius + 20f;
        canvas.DrawText(label, cx, labelY, SKTextAlign.Center, font, paint);
    }

    /// <summary>
    /// Draws a small confidence bar in the bottom-left corner.
    /// </summary>
    private static void DrawConfidenceBar(SKCanvas canvas, float confidence, float canvasWidth, float canvasHeight)
    {
        const float barWidth = 60f;
        const float barHeight = 4f;
        const float margin = 12f;

        float x = margin;
        float y = canvasHeight - margin - barHeight;

        // Background
        using var bgPaint = new SKPaint
        {
            Color = ConfidenceBarBg,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawRoundRect(x, y, barWidth, barHeight, 2f, 2f, bgPaint);

        // Filled portion
        using var fgPaint = new SKPaint
        {
            Color = ConfidenceBarFg,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawRoundRect(x, y, barWidth * Math.Clamp(confidence, 0f, 1f), barHeight, 2f, 2f, fgPaint);

        // Label
        using var font = new SKFont(SKTypeface.FromFamilyName("Cascadia Code"), 7);
        using var textPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 100),
            IsAntialias = true
        };
        canvas.DrawText("HAND", x, y - 3f, SKTextAlign.Left, font, textPaint);
    }
}
