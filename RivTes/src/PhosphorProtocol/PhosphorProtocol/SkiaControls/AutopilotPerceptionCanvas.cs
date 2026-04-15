using System;
using System.Collections.Immutable;
using Microsoft.UI.Xaml;
using PhosphorProtocol.Models;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PhosphorProtocol.SkiaControls;

/// <summary>
/// Full-bleed AI perception visualization: radial sweep scanner, detected object bounding boxes
/// with confidence halos, predicted trajectory curve, lane boundaries, and neural-net grid overlay.
/// </summary>
public sealed class AutopilotPerceptionCanvas : SKXamlCanvas
{
    private readonly DispatcherTimer _timer;
    private int _tick;
    private float _transitionProgress = 1f; // 0→1 for entry animation

    // State (updated from outside)
    private ImmutableList<DetectedObject> _objects = [];
    private ImmutableList<PathPoint> _predictedPath = [];
    private double _confidence = 0.95;
    private double _steeringAngle;

    // Paints
    private readonly SKPaint _gridPaint;
    private readonly SKPaint _sweepPaint;
    private readonly SKPaint _pathGlowPaint;
    private readonly SKPaint _pathCorePaint;
    private readonly SKPaint _pathConfidencePaint;
    private readonly SKPaint _boxPaint;
    private readonly SKPaint _boxFillPaint;
    private readonly SKPaint _boxLabelPaint;
    private readonly SKPaint _pedestrianBoxPaint;
    private readonly SKPaint _cyclistBoxPaint;
    private readonly SKPaint _haloPaint;
    private readonly SKPaint _lanePaint;
    private readonly SKPaint _laneGlowPaint;
    private readonly SKPaint _horizonPaint;
    private readonly SKPaint _neuralNodePaint;
    private readonly SKPaint _neuralLinePaint;
    private readonly SKPaint _scanBurstPaint;
    private readonly SKPaint _confidenceArcPaint;
    private readonly SKPaint _confidenceArcBgPaint;

    public AutopilotPerceptionCanvas()
    {
        _gridPaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.5f,
            IsAntialias = true
        };
        _sweepPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        _pathGlowPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6").WithAlpha(30),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 12,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        _pathCorePaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.5f,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        _pathConfidencePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        _boxPaint = new SKPaint
        {
            Color = SKColor.Parse("#3AABA6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _boxFillPaint = new SKPaint
        {
            Color = SKColor.Parse("#3AABA6").WithAlpha(15),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        _boxLabelPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            IsAntialias = true,
            TextSize = 9,
            Typeface = SKTypeface.FromFamilyName("Share Tech Mono", SKFontStyle.Normal)
        };
        _pedestrianBoxPaint = new SKPaint
        {
            Color = SKColor.Parse("#D4A832"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _cyclistBoxPaint = new SKPaint
        {
            Color = SKColor.Parse("#D4A832").WithAlpha(200),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        _haloPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4)
        };
        _lanePaint = new SKPaint
        {
            Color = SKColor.Parse("#247070"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([8f, 12f], 0)
        };
        _laneGlowPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6").WithAlpha(25),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6,
            IsAntialias = true
        };
        _horizonPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        _neuralNodePaint = new SKPaint
        {
            Color = SKColor.Parse("#143838"),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        _neuralLinePaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222").WithAlpha(60),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.5f,
            IsAntialias = true
        };
        _scanBurstPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        };
        _confidenceArcPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        _confidenceArcBgPaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) =>
        {
            _tick++;
            if (_transitionProgress < 1f)
                _transitionProgress = Math.Min(1f, _transitionProgress + 0.025f);
            Invalidate();
        };

        Loaded += (_, _) =>
        {
            _transitionProgress = 0f;
            _timer.Start();
        };
        Unloaded += (_, _) => _timer.Stop();

        PaintSurface += OnPaintSurface;
    }

    public void UpdateState(AutopilotState state)
    {
        _objects = state.Objects;
        _predictedPath = state.PredictedPath;
        _confidence = state.Confidence;
        _steeringAngle = state.SteeringAngle;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColor.Parse("#020707"));

        float w = info.Width;
        float h = info.Height;
        float cx = w / 2f;
        float cy = h / 2f;

        // Apply entry transition (radial bloom from center)
        float t = EaseOutExpo(_transitionProgress);

        // ── 0. Perspective grid (neural net substrate) ─────────────────
        DrawNeuralGrid(canvas, w, h, t);

        // ── 1. Horizon gradient ───────────────────────────────────────
        float vanishY = h * 0.22f;
        _horizonPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(cx, 0),
            new SKPoint(cx, vanishY),
            [SKColor.Parse("#020707"), SKColor.Parse("#051212").WithAlpha((byte)(40 * t))],
            null, SKShaderTileMode.Clamp);
        canvas.DrawRect(0, 0, w, vanishY, _horizonPaint);

        // ── 2. Lane boundaries (perspective) ──────────────────────────
        DrawLaneBoundaries(canvas, w, h, vanishY, t);

        // ── 3. Predicted path (confidence-gradient curve) ─────────────
        DrawPredictedPath(canvas, w, h, t);

        // ── 4. Radial sweep scanner ───────────────────────────────────
        DrawRadialSweep(canvas, w, h, t);

        // ── 5. Detected objects with bounding boxes + halos ───────────
        DrawDetectedObjects(canvas, w, h, t);

        // ── 6. Scan burst rings (entry effect, fades after transition)
        if (_transitionProgress < 0.8f)
            DrawScanBurst(canvas, cx, cy, w, h);

        // ── 7. Corner confidence arc ──────────────────────────────────
        DrawConfidenceArc(canvas, w, h, t);

        // ── 8. Neural processing nodes (subtle background animation) ──
        DrawNeuralNodes(canvas, w, h, t);
    }

    private void DrawNeuralGrid(SKCanvas canvas, float w, float h, float t)
    {
        float gridSpacing = 40f;
        float vanishY = h * 0.22f;
        float cx = w / 2f;
        byte alpha = (byte)(Math.Clamp(t * 0.6, 0, 1) * 30);
        _gridPaint.Color = SKColor.Parse("#0A2222").WithAlpha(alpha);

        // Perspective grid lines converging to vanishing point
        for (float x = 0; x < w; x += gridSpacing)
        {
            float tNorm = (x - cx) / (w / 2f);
            float topX = cx + tNorm * w * 0.05f;
            canvas.DrawLine(topX, vanishY, x, h, _gridPaint);
        }

        // Horizontal lines with perspective spacing
        for (int i = 1; i <= 12; i++)
        {
            float yNorm = i / 12f;
            float y = vanishY + (h - vanishY) * yNorm * yNorm; // quadratic for perspective
            canvas.DrawLine(0, y, w, y, _gridPaint);
        }
    }

    private void DrawLaneBoundaries(SKCanvas canvas, float w, float h, float vanishY, float t)
    {
        float cx = w / 2f;
        float laneWidth = w * 0.25f;
        float steerOffset = (float)(_steeringAngle * 1.5);

        // Left and right lane edges
        float[] offsets = [-laneWidth / 2f, laneWidth / 2f, -laneWidth, laneWidth];

        _lanePaint.PathEffect = SKPathEffect.CreateDash([8f, 12f], _tick * 2f);
        byte glowAlpha = (byte)(t * 25);
        _laneGlowPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha(glowAlpha);

        foreach (float off in offsets)
        {
            float bottomX = cx + off + steerOffset;
            float topX = cx + off * 0.06f;
            canvas.DrawLine(topX, vanishY, bottomX, h, _laneGlowPaint);
            canvas.DrawLine(topX, vanishY, bottomX, h, _lanePaint);
        }
    }

    private void DrawPredictedPath(SKCanvas canvas, float w, float h, float t)
    {
        if (_predictedPath.Count < 2) return;

        using var pathGlow = new SKPath();
        using var pathCore = new SKPath();

        bool first = true;
        for (int i = 0; i < _predictedPath.Count; i++)
        {
            var pt = _predictedPath[i];
            float x = (float)(pt.X * w);
            float y = (float)(pt.Y * h);

            if (first)
            {
                pathGlow.MoveTo(x, y);
                pathCore.MoveTo(x, y);
                first = false;
            }
            else
            {
                pathGlow.LineTo(x, y);
                pathCore.LineTo(x, y);
            }
        }

        // Confidence-gradient fill between path edges
        var firstPt = _predictedPath[0];
        var lastPt = _predictedPath[^1];
        _pathConfidencePaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint((float)(firstPt.X * w), (float)(firstPt.Y * h)),
            new SKPoint((float)(lastPt.X * w), (float)(lastPt.Y * h)),
            [SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * 20)), SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * 4))],
            null, SKShaderTileMode.Clamp);

        // Draw path spread area
        using var areaPath = new SKPath();
        float spread = 30f;
        areaPath.MoveTo((float)(_predictedPath[0].X * w) - spread, (float)(_predictedPath[0].Y * h));
        for (int i = 0; i < _predictedPath.Count; i++)
        {
            float pathSpread = spread * (float)_predictedPath[i].Confidence;
            areaPath.LineTo((float)(_predictedPath[i].X * w) - pathSpread, (float)(_predictedPath[i].Y * h));
        }
        for (int i = _predictedPath.Count - 1; i >= 0; i--)
        {
            float pathSpread = spread * (float)_predictedPath[i].Confidence;
            areaPath.LineTo((float)(_predictedPath[i].X * w) + pathSpread, (float)(_predictedPath[i].Y * h));
        }
        areaPath.Close();
        canvas.DrawPath(areaPath, _pathConfidencePaint);

        // Glow + core path
        _pathGlowPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * 30));
        canvas.DrawPath(pathGlow, _pathGlowPaint);

        float pathPulse = (float)(0.7 + Math.Sin(_tick * 0.06) * 0.3);
        _pathCorePaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * pathPulse * 220));
        canvas.DrawPath(pathCore, _pathCorePaint);
    }

    private void DrawRadialSweep(SKCanvas canvas, float w, float h, float t)
    {
        float cx = w / 2f;
        float sweepCenterY = h * 0.85f; // Sweep origin near bottom (car position)
        float maxRadius = Math.Max(w, h) * 0.9f;
        float sweepAngle = (_tick * 1.8f) % 360f;

        // Sweep arc (fading cone)
        _sweepPaint.Shader = SKShader.CreateSweepGradient(
            new SKPoint(cx, sweepCenterY),
            [
                SKColor.Parse("#6FFCF6").WithAlpha(0),
                SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * 18)),
                SKColor.Parse("#6FFCF6").WithAlpha((byte)(t * 6)),
                SKColor.Parse("#6FFCF6").WithAlpha(0)
            ],
            [0f, 0.02f, 0.06f, 0.10f]);

        canvas.Save();
        canvas.RotateDegrees(sweepAngle - 90, cx, sweepCenterY);
        canvas.DrawCircle(cx, sweepCenterY, maxRadius * t, _sweepPaint);
        canvas.Restore();

        // Concentric range rings
        for (int ring = 1; ring <= 4; ring++)
        {
            float radius = maxRadius * ring / 4f * t;
            byte ringAlpha = (byte)(12 + Math.Sin(_tick * 0.03 + ring) * 5);
            _gridPaint.Color = SKColor.Parse("#0A2222").WithAlpha(ringAlpha);
            canvas.DrawCircle(cx, sweepCenterY, radius, _gridPaint);
        }
    }

    private void DrawDetectedObjects(SKCanvas canvas, float w, float h, float t)
    {
        foreach (var obj in _objects)
        {
            float x = (float)(obj.RelativeX * w);
            float y = (float)(obj.RelativeY * h);
            float bw = (float)(obj.Width * w);
            float bh = (float)(obj.Height * h);
            var rect = new SKRect(x - bw / 2, y - bh / 2, x + bw / 2, y + bh / 2);

            // Select paint by type
            var strokePaint = obj.Type switch
            {
                "pedestrian" => _pedestrianBoxPaint,
                "cyclist" => _cyclistBoxPaint,
                _ => _boxPaint
            };

            byte objAlpha = (byte)(t * 200);

            // ── Draw the actual object silhouette inside the box ──
            switch (obj.Type)
            {
                case "vehicle":
                    DrawVehicleSilhouette(canvas, x, y, bw, bh, objAlpha);
                    break;
                case "pedestrian":
                    DrawPedestrianSilhouette(canvas, x, y, bw, bh, objAlpha);
                    break;
                case "cyclist":
                    DrawCyclistSilhouette(canvas, x, y, bw, bh, objAlpha);
                    break;
            }

            // Confidence halo (pulsing)
            float haloPulse = (float)(0.5 + Math.Sin(_tick * 0.08 + obj.RelativeX * 10) * 0.3);
            byte haloAlpha = (byte)(obj.Confidence * haloPulse * t * 100);
            _haloPaint.Color = strokePaint.Color.WithAlpha(haloAlpha);
            var haloRect = new SKRect(rect.Left - 6, rect.Top - 6, rect.Right + 6, rect.Bottom + 6);
            canvas.DrawRoundRect(new SKRoundRect(haloRect, 4), _haloPaint);

            // Bounding box stroke
            strokePaint.Color = strokePaint.Color.WithAlpha(objAlpha);
            canvas.DrawRoundRect(new SKRoundRect(rect, 2), strokePaint);

            // Corner brackets (targeting reticle style)
            float bracketLen = Math.Min(bw, bh) * 0.3f;
            DrawCornerBrackets(canvas, rect, bracketLen, strokePaint);

            // Label above box
            string label = $"{obj.Type.ToUpperInvariant()} {obj.Confidence:P0}";
            _boxLabelPaint.Color = strokePaint.Color.WithAlpha((byte)(t * 230));
            canvas.DrawText(label, rect.Left, rect.Top - 5, _boxLabelPaint);

            // Velocity arrow above object
            if (obj.Velocity > 5)
            {
                float velLen = Math.Min((float)(obj.Velocity * 0.25), bh * 0.6f);
                _boxPaint.Color = SKColor.Parse("#3AABA6").WithAlpha((byte)(t * 120));
                canvas.DrawLine(x, rect.Top - 12, x, rect.Top - 12 - velLen, _boxPaint);
                canvas.DrawLine(x, rect.Top - 12 - velLen, x - 3, rect.Top - 12 - velLen + 4, _boxPaint);
                canvas.DrawLine(x, rect.Top - 12 - velLen, x + 3, rect.Top - 12 - velLen + 4, _boxPaint);
            }
        }
    }

    /// <summary>Top-down vehicle silhouette (like DrivingVisualization) — body, windshield, taillights.</summary>
    private void DrawVehicleSilhouette(SKCanvas canvas, float cx, float cy, float bw, float bh, byte alpha)
    {
        float carW = bw * 0.75f;
        float carH = bh * 0.85f;
        var carRect = new SKRect(cx - carW / 2, cy - carH / 2, cx + carW / 2, cy + carH / 2);

        // Body outline
        _boxPaint.Color = SKColor.Parse("#247070").WithAlpha(alpha);
        _boxPaint.StrokeWidth = 1.2f;
        canvas.DrawRoundRect(new SKRoundRect(carRect, 3), _boxPaint);

        // Windshield line (front, top portion)
        float wsY = cy - carH * 0.25f;
        _boxPaint.Color = SKColor.Parse("#143838").WithAlpha(alpha);
        canvas.DrawLine(cx - carW * 0.3f, wsY, cx + carW * 0.3f, wsY, _boxPaint);

        // Rear window
        float rwY = cy + carH * 0.2f;
        canvas.DrawLine(cx - carW * 0.22f, rwY, cx + carW * 0.22f, rwY, _boxPaint);

        // Taillights (red dots at back)
        float tlY = cy + carH / 2 - 2;
        _haloPaint.Color = SKColor.Parse("#EE4040").WithAlpha((byte)(alpha * 0.7));
        _haloPaint.StrokeWidth = 0;
        canvas.DrawCircle(cx - carW * 0.3f, tlY, Math.Max(1.5f, carW * 0.06f), _haloPaint);
        canvas.DrawCircle(cx + carW * 0.3f, tlY, Math.Max(1.5f, carW * 0.06f), _haloPaint);
        _haloPaint.StrokeWidth = 2; // restore

        // Wheels (small marks on sides)
        _boxPaint.Color = SKColor.Parse("#0A2222").WithAlpha(alpha);
        float wheelH = carH * 0.12f;
        canvas.DrawRect(cx - carW / 2 - 1.5f, cy - carH * 0.18f, 1.5f, wheelH, _boxPaint);
        canvas.DrawRect(cx + carW / 2, cy - carH * 0.18f, 1.5f, wheelH, _boxPaint);
        canvas.DrawRect(cx - carW / 2 - 1.5f, cy + carH * 0.1f, 1.5f, wheelH, _boxPaint);
        canvas.DrawRect(cx + carW / 2, cy + carH * 0.1f, 1.5f, wheelH, _boxPaint);
    }

    /// <summary>Simple pedestrian figure — head circle + body line + legs.</summary>
    private void DrawPedestrianSilhouette(SKCanvas canvas, float cx, float cy, float bw, float bh, byte alpha)
    {
        _pedestrianBoxPaint.Color = SKColor.Parse("#D4A832").WithAlpha(alpha);
        float scale = Math.Min(bw, bh);

        // Head
        float headR = scale * 0.2f;
        float headY = cy - bh * 0.3f;
        canvas.DrawCircle(cx, headY, Math.Max(headR, 2f), _pedestrianBoxPaint);

        // Body
        float bodyTop = headY + headR;
        float bodyBottom = cy + bh * 0.1f;
        canvas.DrawLine(cx, bodyTop, cx, bodyBottom, _pedestrianBoxPaint);

        // Arms
        float armY = bodyTop + (bodyBottom - bodyTop) * 0.25f;
        float armSpread = scale * 0.3f;
        canvas.DrawLine(cx - armSpread, armY + armSpread * 0.3f, cx + armSpread, armY - armSpread * 0.1f, _pedestrianBoxPaint);

        // Legs
        float legSpread = scale * 0.25f;
        float legBottom = cy + bh * 0.38f;
        canvas.DrawLine(cx, bodyBottom, cx - legSpread, legBottom, _pedestrianBoxPaint);
        canvas.DrawLine(cx, bodyBottom, cx + legSpread, legBottom, _pedestrianBoxPaint);
    }

    /// <summary>Cyclist figure — circle (wheel) + body leaning forward.</summary>
    private void DrawCyclistSilhouette(SKCanvas canvas, float cx, float cy, float bw, float bh, byte alpha)
    {
        _cyclistBoxPaint.Color = SKColor.Parse("#D4A832").WithAlpha((byte)(alpha * 0.85));
        float scale = Math.Min(bw, bh);

        // Rear wheel
        float wheelR = scale * 0.22f;
        float wheelY = cy + bh * 0.2f;
        canvas.DrawCircle(cx - bw * 0.18f, wheelY, Math.Max(wheelR, 2f), _cyclistBoxPaint);

        // Front wheel
        canvas.DrawCircle(cx + bw * 0.18f, wheelY, Math.Max(wheelR, 2f), _cyclistBoxPaint);

        // Frame (connecting wheels)
        canvas.DrawLine(cx - bw * 0.18f, wheelY, cx + bw * 0.18f, wheelY, _cyclistBoxPaint);

        // Body (leaning forward)
        float seatY = cy - bh * 0.05f;
        canvas.DrawLine(cx - bw * 0.05f, seatY, cx + bw * 0.15f, wheelY, _cyclistBoxPaint);

        // Head
        float headY = cy - bh * 0.25f;
        canvas.DrawCircle(cx + bw * 0.05f, headY, Math.Max(scale * 0.15f, 1.5f), _cyclistBoxPaint);

        // Torso
        canvas.DrawLine(cx - bw * 0.05f, seatY, cx + bw * 0.05f, headY + scale * 0.15f, _cyclistBoxPaint);
    }

    private static void DrawCornerBrackets(SKCanvas canvas, SKRect rect, float len, SKPaint paint)
    {
        // Top-left
        canvas.DrawLine(rect.Left, rect.Top, rect.Left + len, rect.Top, paint);
        canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Top + len, paint);
        // Top-right
        canvas.DrawLine(rect.Right, rect.Top, rect.Right - len, rect.Top, paint);
        canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Top + len, paint);
        // Bottom-left
        canvas.DrawLine(rect.Left, rect.Bottom, rect.Left + len, rect.Bottom, paint);
        canvas.DrawLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom - len, paint);
        // Bottom-right
        canvas.DrawLine(rect.Right, rect.Bottom, rect.Right - len, rect.Bottom, paint);
        canvas.DrawLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom - len, paint);
    }

    private void DrawScanBurst(SKCanvas canvas, float cx, float cy, float w, float h)
    {
        // Entry effect: expanding rings from center
        float burstProgress = _transitionProgress * 3f; // Runs faster than overall transition
        for (int i = 0; i < 3; i++)
        {
            float ringT = Math.Clamp(burstProgress - i * 0.3f, 0f, 1f);
            float radius = ringT * Math.Max(w, h) * 0.6f;
            byte alpha = (byte)((1f - ringT) * 80);
            _scanBurstPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha(alpha);
            canvas.DrawCircle(cx, cy, radius, _scanBurstPaint);
        }
    }

    private void DrawConfidenceArc(SKCanvas canvas, float w, float h, float t)
    {
        // Bottom-left confidence gauge arc
        float arcR = 35f;
        float arcX = 40f;
        float arcY = h - 40f;
        var arcRect = new SKRect(arcX - arcR, arcY - arcR, arcX + arcR, arcY + arcR);

        // Background arc
        canvas.DrawArc(arcRect, -210, 240, false, _confidenceArcBgPaint);

        // Confidence fill
        float confAngle = (float)(_confidence * 240 * t);
        float confPulse = (float)(0.85 + Math.Sin(_tick * 0.05) * 0.15);
        byte confAlpha = (byte)(confPulse * 255);

        var confColor = _confidence > 0.8 ? SKColor.Parse("#6FFCF6") :
                        _confidence > 0.5 ? SKColor.Parse("#D4A832") :
                        SKColor.Parse("#EE4040");
        _confidenceArcPaint.Color = confColor.WithAlpha(confAlpha);
        canvas.DrawArc(arcRect, -210, confAngle, false, _confidenceArcPaint);

        // Center percentage
        _boxLabelPaint.Color = confColor.WithAlpha((byte)(t * 255));
        _boxLabelPaint.TextSize = 11;
        string confText = $"{_confidence:P0}";
        float textWidth = _boxLabelPaint.MeasureText(confText);
        canvas.DrawText(confText, arcX - textWidth / 2, arcY + 4, _boxLabelPaint);

        // Label
        _boxLabelPaint.TextSize = 8;
        _boxLabelPaint.Color = SKColor.Parse("#247070").WithAlpha((byte)(t * 200));
        float labelWidth = _boxLabelPaint.MeasureText("CONFIDENCE");
        canvas.DrawText("CONFIDENCE", arcX - labelWidth / 2, arcY + 16, _boxLabelPaint);
        _boxLabelPaint.TextSize = 9; // restore
    }

    private void DrawNeuralNodes(SKCanvas canvas, float w, float h, float t)
    {
        // Sparse network of animated nodes in the background
        int nodeCount = 8;
        var random = new Random(42); // Fixed seed for stable positions
        var nodes = new SKPoint[nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            float nx = (float)(random.NextDouble() * w);
            float ny = (float)(random.NextDouble() * h);
            // Gentle drift
            nx += (float)Math.Sin(_tick * 0.01 + i * 2) * 8;
            ny += (float)Math.Cos(_tick * 0.012 + i * 3) * 6;
            nodes[i] = new SKPoint(nx, ny);

            float pulse = (float)(0.3 + Math.Sin(_tick * 0.04 + i) * 0.2);
            _neuralNodePaint.Color = SKColor.Parse("#143838").WithAlpha((byte)(pulse * t * 150));
            canvas.DrawCircle(nx, ny, 2.5f, _neuralNodePaint);
        }

        // Connect nearby nodes
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = i + 1; j < nodeCount; j++)
            {
                float dist = SKPoint.Distance(nodes[i], nodes[j]);
                if (dist < 200)
                {
                    byte lineAlpha = (byte)((1f - dist / 200f) * 30 * t);
                    _neuralLinePaint.Color = SKColor.Parse("#0A2222").WithAlpha(lineAlpha);
                    canvas.DrawLine(nodes[i], nodes[j], _neuralLinePaint);
                }
            }
        }
    }

    private static float EaseOutExpo(float x)
        => x >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * x);
}
