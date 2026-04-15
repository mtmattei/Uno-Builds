using System;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PhosphorProtocol.SkiaControls;

public sealed class DrivingVisualization : SKXamlCanvas
{
    private readonly DispatcherTimer _timer;
    private int _tick;
    private int _speed = 62;

    // SKPaint objects (reused to avoid GC pressure)
    private readonly SKPaint _roadPaint;
    private readonly SKPaint _laneDashPaint;
    private readonly SKPaint _edgeLinePaint;
    private readonly SKPaint _autopilotGlowPaint;
    private readonly SKPaint _autopilotCorePaint;
    private readonly SKPaint _vehiclePaint;
    private readonly SKPaint _carBodyGlowPaint;
    private readonly SKPaint _carBodyPaint;
    private readonly SKPaint _headlightPaint;
    private readonly SKPaint _headlightBeamPaint;
    private readonly SKPaint _taillightPaint;
    private readonly SKPaint _windshieldPaint;
    private readonly SKPaint _trafficLightFramePaint;
    private readonly SKPaint _trafficLightGreenPaint;
    private readonly SKPaint _vehicleTaillightPaint;
    private readonly SKPaint _wheelPaint;
    private readonly SKPaint _horizonGradientPaint;

    public DrivingVisualization()
    {
        _roadPaint = new SKPaint { Color = SKColor.Parse("#051212"), IsAntialias = true };
        _laneDashPaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            PathEffect = SKPathEffect.CreateDash([12f, 18f], 0)
        };
        _edgeLinePaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _autopilotGlowPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6").WithAlpha(38),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 8,
            IsAntialias = true
        };
        _autopilotCorePaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.5f,
            IsAntialias = true
        };
        _vehiclePaint = new SKPaint
        {
            Color = SKColor.Parse("#143838"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _carBodyGlowPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6").WithAlpha(80),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Miter
        };
        _carBodyPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Miter
        };
        _headlightPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        _headlightBeamPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        _taillightPaint = new SKPaint
        {
            Color = SKColor.Parse("#EE4040"),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
        };
        _windshieldPaint = new SKPaint
        {
            Color = SKColor.Parse("#247070"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f,
            IsAntialias = true
        };
        _trafficLightFramePaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _trafficLightGreenPaint = new SKPaint
        {
            Color = SKColor.Parse("#3AABA6"),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
        };
        _vehicleTaillightPaint = new SKPaint
        {
            Color = SKColor.Parse("#551818"),
            IsAntialias = true
        };
        _wheelPaint = new SKPaint
        {
            Color = SKColor.Parse("#143838"),
            IsAntialias = true
        };
        _horizonGradientPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) =>
        {
            _tick++;
            Invalidate();
        };

        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => _timer.Stop();

        PaintSurface += OnPaintSurface;
    }

    public int Speed
    {
        get => _speed;
        set => _speed = value;
    }

    /// <summary>
    /// Lerps an X position between the bottom-of-road value and the vanishing point
    /// based on a normalized Y (0 = vanishing point, 1 = bottom of road area).
    /// </summary>
    private static float PerspectiveX(float bottomX, float vanishX, float tNorm)
    {
        return vanishX + (bottomX - vanishX) * tNorm;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColor.Parse("#020707"));

        float w = info.Width;
        float h = info.Height;
        float cx = w / 2f;

        // Perspective parameters
        float vanishY = h * 0.15f;          // horizon / vanishing point Y
        float vanishX = cx;                  // vanishing point X (top center)
        float roadBottomWidth = w * 0.7f;    // road width at bottom of canvas
        float roadBottomLeft = cx - roadBottomWidth / 2f;
        float roadBottomRight = cx + roadBottomWidth / 2f;
        float laneWidthBottom = roadBottomWidth / 3f;

        // Scroll offset for lane markings
        float scrollOffset = (_tick * (_speed / 20f)) % 42f;

        // ── 0. Horizon gradient ──────────────────────────────────────────
        // Fade from road color at vanishY down to the CRT background above
        _horizonGradientPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(cx, 0),
            new SKPoint(cx, vanishY),
            [SKColor.Parse("#020707"), SKColor.Parse("#051212")],
            null,
            SKShaderTileMode.Clamp);
        canvas.DrawRect(0, 0, w, vanishY, _horizonGradientPaint);

        // ── 1. Road surface (perspective trapezoid) ──────────────────────
        // Top edge at vanishing point is very narrow; bottom edge is full width.
        float roadTopHalf = roadBottomWidth * 0.04f; // almost converges at top
        using var roadPath = new SKPath();
        roadPath.MoveTo(cx - roadTopHalf, vanishY);
        roadPath.LineTo(roadBottomLeft, h);
        roadPath.LineTo(roadBottomRight, h);
        roadPath.LineTo(cx + roadTopHalf, vanishY);
        roadPath.Close();
        canvas.DrawPath(roadPath, _roadPaint);

        // ── 2. Edge lines (perspective) ──────────────────────────────────
        canvas.DrawLine(cx - roadTopHalf, vanishY, roadBottomLeft, h, _edgeLinePaint);
        canvas.DrawLine(cx + roadTopHalf, vanishY, roadBottomRight, h, _edgeLinePaint);

        // ── 3. Lane dividers (perspective, dashed, scrolling) ────────────
        // Bottom X positions for the two lane dividers
        float lane1BottomX = roadBottomLeft + laneWidthBottom;
        float lane2BottomX = roadBottomLeft + laneWidthBottom * 2;
        // Top X positions converge toward vanishX
        float lane1TopX = PerspectiveX(lane1BottomX, vanishX, 0.08f);
        float lane2TopX = PerspectiveX(lane2BottomX, vanishX, 0.08f);

        _laneDashPaint.PathEffect = SKPathEffect.CreateDash([12f, 18f], scrollOffset);
        canvas.DrawLine(lane1TopX, vanishY, lane1BottomX, h, _laneDashPaint);
        canvas.DrawLine(lane2TopX, vanishY, lane2BottomX, h, _laneDashPaint);

        // ── 4. Autopilot guide rails (perspective, in center lane) ───────
        float railLeftBottomX = lane1BottomX + 4;
        float railRightBottomX = lane2BottomX - 4;
        float railLeftTopX = PerspectiveX(railLeftBottomX, vanishX, 0.08f);
        float railRightTopX = PerspectiveX(railRightBottomX, vanishX, 0.08f);

        float railAlpha = (float)(0.3 + Math.Sin(_tick * 0.04) * 0.12);
        _autopilotGlowPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(railAlpha * 255 * 0.5));
        canvas.DrawLine(railLeftTopX, vanishY, railLeftBottomX, h, _autopilotGlowPaint);
        canvas.DrawLine(railRightTopX, vanishY, railRightBottomX, h, _autopilotGlowPaint);
        _autopilotCorePaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(railAlpha * 255));
        canvas.DrawLine(railLeftTopX, vanishY, railLeftBottomX, h, _autopilotCorePaint);
        canvas.DrawLine(railRightTopX, vanishY, railRightBottomX, h, _autopilotCorePaint);

        // ── 5. Traffic light at top ──────────────────────────────────────
        float tlX = cx;
        float tlY = vanishY - 10;
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(tlX - 8, tlY - 4, tlX + 8, tlY + 20), 4), _trafficLightFramePaint);
        canvas.DrawCircle(tlX, tlY + 13, 4, _trafficLightGreenPaint);

        // ── 6. Detected vehicles (perspective-scaled) ────────────────────
        // Each vehicle's lane center at the bottom
        float[] vehicleLaneBottomX = [roadBottomLeft + laneWidthBottom * 0.5f, roadBottomLeft + laneWidthBottom * 1.5f, roadBottomLeft + laneWidthBottom * 2.5f];
        float[] vehicleBaseYNorm = [0.25f, 0.40f, 0.30f]; // normalized Y within road (0 = vanish, 1 = bottom)

        for (int i = 0; i < 3; i++)
        {
            if (i == 1) continue; // Skip center lane (our lane)

            // tNorm: 0 at vanishY, 1 at bottom (h)
            float tNorm = vehicleBaseYNorm[i] + (float)Math.Sin(_tick * 0.04 + i * 3) * 0.04f;
            tNorm = Math.Clamp(tNorm, 0.05f, 0.95f);
            float vy = vanishY + (h - vanishY) * tNorm;
            float vx = PerspectiveX(vehicleLaneBottomX[i], vanishX, tNorm);

            // Scale vehicle size by perspective (smaller near horizon)
            float perspScale = tNorm;
            float vw = laneWidthBottom * 0.55f * perspScale;
            float vh = vw * 1.8f;

            // Vehicle body (sharp edges)
            var vRect = new SKRect(vx - vw / 2, vy - vh / 2, vx + vw / 2, vy + vh / 2);
            canvas.DrawRect(vRect, _vehiclePaint);

            // Windshield
            canvas.DrawLine(vx - vw / 3, vy - vh / 4, vx + vw / 3, vy - vh / 4, _windshieldPaint);

            // Taillights
            float tlSize = Math.Max(2f, 4f * perspScale);
            canvas.DrawRect(vx - vw / 3, vy + vh / 2 - 3 * perspScale, tlSize, 2 * perspScale, _vehicleTaillightPaint);
            canvas.DrawRect(vx + vw / 3 - tlSize, vy + vh / 2 - 3 * perspScale, tlSize, 2 * perspScale, _vehicleTaillightPaint);
        }

        // ── 7. Your car (bottom center, with idle breathing) ─────────────
        // Idle breathing: subtle ±1.5% scale oscillation on a slow sine
        float breathScale = 1f + (float)Math.Sin(_tick * 0.03) * 0.015f;

        float carX = cx;
        float carY = h * 0.75f;
        float carW = laneWidthBottom * 0.6f * breathScale;
        float carH = carW * 2f;
        var carRect = new SKRect(carX - carW / 2, carY - carH / 2, carX + carW / 2, carY + carH / 2);

        // Sharp-edged car body path (top-down sedan silhouette)
        float hw = carW / 2f;
        float hh = carH / 2f;
        float noseInset = hw * 0.2f;  // tapered nose
        float tailInset = hw * 0.15f; // slight taper at rear
        float cabinTop = carY - hh * 0.35f;
        float cabinBot = carY + hh * 0.25f;

        using var bodyPath = new SKPath();
        // Front (nose) — angular taper
        bodyPath.MoveTo(carX - hw + noseInset, carY - hh);
        bodyPath.LineTo(carX + hw - noseInset, carY - hh);
        // Right side — shoulder flare then straight
        bodyPath.LineTo(carX + hw, carY - hh * 0.6f);
        bodyPath.LineTo(carX + hw, carY + hh * 0.55f);
        // Rear — slight taper
        bodyPath.LineTo(carX + hw - tailInset, carY + hh);
        bodyPath.LineTo(carX - hw + tailInset, carY + hh);
        // Left side
        bodyPath.LineTo(carX - hw, carY + hh * 0.55f);
        bodyPath.LineTo(carX - hw, carY - hh * 0.6f);
        bodyPath.Close();

        canvas.DrawPath(bodyPath, _carBodyGlowPaint);
        canvas.DrawPath(bodyPath, _carBodyPaint);

        // Windshield (angled trapezoid line)
        float wsInset = hw * 0.15f;
        canvas.DrawLine(carX - hw * 0.65f + wsInset, cabinTop, carX + hw * 0.65f - wsInset, cabinTop, _windshieldPaint);
        // Rear window
        canvas.DrawLine(carX - hw * 0.5f, cabinBot, carX + hw * 0.5f, cabinBot, _windshieldPaint);

        // Cabin side pillars (A-pillar lines)
        canvas.DrawLine(carX - hw + noseInset + 2, carY - hh * 0.6f, carX - hw * 0.65f + wsInset, cabinTop, _windshieldPaint);
        canvas.DrawLine(carX + hw - noseInset - 2, carY - hh * 0.6f, carX + hw * 0.65f - wsInset, cabinTop, _windshieldPaint);

        // Headlights (sharp rectangular)
        float headlightPulse = (float)(0.6 + Math.Sin(_tick * 0.06) * 0.4);
        _headlightPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(headlightPulse * 255));
        float hlY = carY - hh + 1;
        canvas.DrawRect(carX - hw + noseInset, hlY, 5, 2, _headlightPaint);
        canvas.DrawRect(carX + hw - noseInset - 5, hlY, 5, 2, _headlightPaint);

        // Headlight beams (gradient fade upward)
        _headlightBeamPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(carX, hlY),
            new SKPoint(carX, hlY - 50),
            [SKColor.Parse("#6FFCF6").WithAlpha((byte)(headlightPulse * 50)), SKColor.Parse("#6FFCF6").WithAlpha(0)],
            null,
            SKShaderTileMode.Clamp);
        canvas.DrawRect(carX - hw * 0.8f, hlY - 50, carW * 0.8f, 50, _headlightBeamPaint);

        // Taillights (sharp rectangular, slightly wider)
        float tlRedY = carY + hh - 2;
        _taillightPaint.Color = SKColor.Parse("#EE4040");
        canvas.DrawRect(carX - hw + tailInset, tlRedY, 6, 2, _taillightPaint);
        canvas.DrawRect(carX + hw - tailInset - 6, tlRedY, 6, 2, _taillightPaint);

        // Wheels (sharp angular blocks)
        canvas.DrawRect(carX - hw - 2, carY - hh * 0.35f, 2, 10, _wheelPaint);
        canvas.DrawRect(carX + hw, carY - hh * 0.35f, 2, 10, _wheelPaint);
        canvas.DrawRect(carX - hw - 2, carY + hh * 0.2f, 2, 10, _wheelPaint);
        canvas.DrawRect(carX + hw, carY + hh * 0.2f, 2, 10, _wheelPaint);

        // Side mirrors (angular)
        canvas.DrawRect(carX - hw - 4, carY - hh * 0.25f, 4, 2, _carBodyPaint);
        canvas.DrawRect(carX + hw, carY - hh * 0.25f, 4, 2, _carBodyPaint);
    }
}
