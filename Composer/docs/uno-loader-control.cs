// CompositionStackLoader.cs
//
// Drop-in loading animation for Uno Platform 6.4+ on .NET 10.
// Plays the composition-stack → mobile → desktop sequence once, then raises
// AnimationCompleted. Host can also call Skip() to fast-forward when real
// loading finishes early.
//
// NuGet dependencies on the Uno head project:
//   SkiaSharp.Views.Uno.WinUI  3.119.2 or later
//   (or SkiaSharp.Views.Uno for non-WinUI heads)
//
// SkiaSharp 3.x is required — 2.88.x doesn't ship .NET 10 native assets.
// This file uses the SkiaSharp 3.x text APIs (SKFont + SKTextAlign params
// on canvas.DrawText) which differ from the 2.x APIs.
//
// Usage:
//   <controls:CompositionStackLoader x:Name="Loader"
//                                    AnimationCompleted="OnLoaderFinished"
//                                    Background="#FAFAFA" />
//
//   private void OnLoaderFinished(object sender, EventArgs e)
//   {
//       MainContent.Visibility = Visibility.Visible;
//       Loader.Visibility = Visibility.Collapsed;
//   }
//
//   // If your app finishes loading early:
//   Loader.Skip();
//
// Tested mental model: matches the React reference's phase machine exactly.
// Coordinate space matches the SVG viewBox (-40 30 520 480) so geometry
// constants are direct ports.

using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace YourApp.Controls;

public sealed class CompositionStackLoader : UserControl
{
    // ─── Public surface ────────────────────────────────────────────────────

    public event EventHandler? AnimationCompleted;

    /// <summary>
    /// Fast-forward to the end. Useful when real loading finishes before
    /// the full ~18s animation does — fades to a clean stack hold and
    /// raises AnimationCompleted within ~400ms.
    /// </summary>
    public void Skip()
    {
        if (_finished) return;
        _skipping = true;
        _skipStartMs = ElapsedMs;
    }

    // ─── Private state ─────────────────────────────────────────────────────

    private readonly SKXamlCanvas _canvas;
    private readonly Stopwatch _clock = new();
    private DispatcherTimer? _timer;
    private bool _finished;
    private bool _skipping;
    private double _skipStartMs;

    private double ElapsedMs => _clock.Elapsed.TotalMilliseconds;

    public CompositionStackLoader()
    {
        _canvas = new SKXamlCanvas();
        _canvas.PaintSurface += OnPaintSurface;
        Content = _canvas;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _clock.Restart();
        _timer = new DispatcherTimer
        {
            // ~60fps. Skia's invalidation is cheap; this is fine on WASM too.
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += (_, _) =>
        {
            if (_finished) return;
            _canvas.Invalidate();
        };
        _timer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        _timer = null;
        _clock.Stop();
    }

    // ─── Phase machine — mirrors the React reference ───────────────────────

    private record Phase(int SelectedIdx, int FromForm, int ToForm, double DurMs);

    private static readonly Phase[] Phases =
    [
        new(0,  0, 0, 1500), // PLAN reveal
        new(1,  0, 0, 1500), // ARCHITECTURE reveal
        new(2,  0, 0, 1500), // DESIGN reveal
        new(3,  0, 0, 1500), // WIRING reveal
        new(4,  0, 0, 1500), // FOUNDATION reveal
        new(-1, 0, 0, 1100), // hold full stack
        new(-1, 0, 1, 1700), // morph stack → mobile
        new(-1, 1, 1, 2200), // hold mobile
        new(-1, 1, 2, 1900), // morph mobile → desktop
        new(-1, 2, 2, 2200), // hold desktop
        new(-1, 2, 0, 1300), // return to stack
    ];

    private static readonly double TotalDurationMs = SumDurations();
    private static double SumDurations()
    {
        double s = 0;
        foreach (var p in Phases) s += p.DurMs;
        return s;
    }

    // Walk the timeline. Returns: (phase, morphT 0..1 within current phase).
    private (Phase phase, double morphT) ResolveTimeline(double tMs)
    {
        double cursor = 0;
        for (int i = 0; i < Phases.Length; i++)
        {
            var p = Phases[i];
            if (tMs < cursor + p.DurMs)
            {
                double local = (tMs - cursor) / p.DurMs;
                return (p, EaseInOutCubic(local));
            }
            cursor += p.DurMs;
        }
        // Past the end — return the last hold position
        return (Phases[^1], 1.0);
    }

    private static double EaseInOutCubic(double t) =>
        t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    // ─── Layer geometry — same coordinates as the React reference ─────────

    private enum LayerId { Plan, Architecture, Design, Wiring, Foundation }

    private record Geo(double X, double Y, double W, double H, double Rx = 0);

    // Form 0 = stack, 1 = mobile, 2 = desktop
    private static readonly Geo[][] Geos =
    [
        // Stack
        [
            /* plan         */ new(140, 60,  160, 30),
            /* architecture */ new(140, 115, 160, 48),
            /* design       */ new(140, 188, 160, 40),
            /* wiring       */ new(140, 253, 160, 22),
            /* foundation   */ new(120, 300, 200, 22, 0),
        ],
        // Mobile
        [
            /* plan         */ new(142, 148, 156, 196),
            /* architecture */ new(142, 84,  156, 30),
            /* design       */ new(142, 122, 156, 18),
            /* wiring       */ new(142, 350, 156, 42),
            /* foundation   */ new(130, 60,  180, 400, 18),
        ],
        // Desktop
        [
            /* plan         */ new(146, 120, 306, 244),
            /* architecture */ new(28,  92,  110, 296),
            /* design       */ new(146, 92,  306, 22),
            /* wiring       */ new(146, 368, 306, 20),
            /* foundation   */ new(20,  60,  440, 336, 8),
        ],
    ];

    // Render order — foundation drawn first so it sits behind in assembled forms
    private static readonly LayerId[] RenderOrder =
        [LayerId.Foundation, LayerId.Wiring, LayerId.Design, LayerId.Architecture, LayerId.Plan];

    // ─── Palette ──────────────────────────────────────────────────────────

    private static readonly SKColor Paper    = new(0xFF, 0xFF, 0xFF);
    private static readonly SKColor Paper2   = new(0xFA, 0xFA, 0xFA);
    private static readonly SKColor Ink      = new(0x18, 0x18, 0x1B);
    private static readonly SKColor Ink2     = new(0x3F, 0x3F, 0x46);
    private static readonly SKColor Ink3     = new(0x71, 0x71, 0x7A);
    private static readonly SKColor Hairline = new(0xE4, 0xE4, 0xE7);

    private static SKColor InkAlpha(double a) =>
        new(0x18, 0x18, 0x1B, (byte)Math.Round(a * 255));

    // ─── Paint cache — Skia paints are expensive to allocate per frame ────

    private readonly SKPaint _strokeInk = new() {
        Color = Ink, IsStroke = true, StrokeWidth = 0.9f, IsAntialias = true,
    };
    private readonly SKPaint _strokeInkBold = new() {
        Color = Ink, IsStroke = true, StrokeWidth = 1.4f, IsAntialias = true,
    };
    private readonly SKPaint _strokeHairline = new() {
        Color = Hairline, IsStroke = true, StrokeWidth = 0.5f, IsAntialias = true,
    };
    private readonly SKPaint _fillPaper2 = new() {
        Color = Paper2, IsAntialias = true,
    };
    private readonly SKPaint _fillInk = new() {
        Color = Ink, IsAntialias = true,
    };
    private readonly SKFont _monoFont = new() { Size = 10, Embolden = false };
    private readonly SKFont _serifFont = new() { Size = 10 };

    // ─── Render ────────────────────────────────────────────────────────────

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var canvas = e.Surface.Canvas;

        canvas.Clear(Paper2);

        // Fit the SVG viewBox (-40 30 520 480) into the canvas — preserves
        // the React layout exactly so geometry constants stay 1:1.
        const float vbX = -40, vbY = 30, vbW = 520, vbH = 480;
        float scale = Math.Min(info.Width / vbW, info.Height / vbH);
        float dx = (info.Width - vbW * scale) / 2f - vbX * scale;
        float dy = (info.Height - vbH * scale) / 2f - vbY * scale;

        canvas.Save();
        canvas.Translate(dx, dy);
        canvas.Scale(scale);

        // Compute current timeline state
        double tMs = ElapsedMs;

        // Skip handling — fade out and finish
        if (_skipping)
        {
            double fadeMs = ElapsedMs - _skipStartMs;
            const double fadeDur = 400;
            float alpha = (float)Math.Max(0, 1 - fadeMs / fadeDur);

            // Draw current frame at fading alpha
            DrawFrame(canvas, Math.Min(tMs, TotalDurationMs), alpha);

            if (fadeMs >= fadeDur)
            {
                Finish();
            }
            canvas.Restore();
            return;
        }

        if (tMs >= TotalDurationMs)
        {
            // Hold the final frame for one beat, then finish
            DrawFrame(canvas, TotalDurationMs - 1, 1f);
            canvas.Restore();
            Finish();
            return;
        }

        DrawFrame(canvas, tMs, 1f);
        canvas.Restore();
    }

    private void Finish()
    {
        if (_finished) return;
        _finished = true;
        _timer?.Stop();
        AnimationCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void DrawFrame(SKCanvas canvas, double tMs, float globalAlpha)
    {
        var (phase, morphT) = ResolveTimeline(tMs);

        // Form opacities — sum is 1, drives interior cross-fade
        Span<double> formOp = stackalloc double[3];
        formOp[phase.FromForm] += 1 - morphT;
        formOp[phase.ToForm]   += morphT;
        double stackOp = formOp[0];

        // Dashes (only meaningful in stack form)
        if (stackOp > 0.02 && globalAlpha > 0)
        {
            using var dashPaint = new SKPaint {
                Color = Hairline.WithAlpha((byte)(stackOp * globalAlpha * 255)),
                IsStroke = true,
                StrokeWidth = 0.5f,
                PathEffect = SKPathEffect.CreateDash([3f, 3f], 0),
                IsAntialias = true,
            };
            canvas.DrawLine(100, 50, 100, 332, dashPaint);
            canvas.DrawLine(340, 50, 340, 332, dashPaint);
        }

        // Layers
        foreach (var id in RenderOrder)
        {
            int selectionIdx = SelectionIndexOf(id);
            bool selected = selectionIdx == phase.SelectedIdx;
            bool anySelected = phase.SelectedIdx != -1;
            DrawLayer(canvas, id, phase, morphT, formOp, selected, anySelected, globalAlpha);
        }

        // Captions — three of them, each fading with its form's presence
        DrawCaption(canvas, "Fig. 01 — The composition stack", 220, 358,
                    (float)(formOp[0] * globalAlpha));
        DrawCaption(canvas, "Fig. 02 — The product, as mobile", 220, 485,
                    (float)(formOp[1] * globalAlpha));
        DrawCaption(canvas, "Fig. 03 — The same product, as desktop", 240, 422,
                    (float)(formOp[2] * globalAlpha));
    }

    private static int SelectionIndexOf(LayerId id) => id switch
    {
        LayerId.Plan         => 0,
        LayerId.Architecture => 1,
        LayerId.Design       => 2,
        LayerId.Wiring       => 3,
        LayerId.Foundation   => 4,
        _ => -1,
    };

    // ─── Layer drawing ────────────────────────────────────────────────────

    private void DrawLayer(
        SKCanvas canvas, LayerId id, Phase phase, double morphT,
        ReadOnlySpan<double> formOp, bool selected, bool anySelected,
        float globalAlpha)
    {
        var fromGeo = Geos[phase.FromForm][(int)id];
        var toGeo   = Geos[phase.ToForm][(int)id];

        float x  = (float)Lerp(fromGeo.X,  toGeo.X,  morphT);
        float y  = (float)Lerp(fromGeo.Y,  toGeo.Y,  morphT);
        float w  = (float)Lerp(fromGeo.W,  toGeo.W,  morphT);
        float h  = (float)Lerp(fromGeo.H,  toGeo.H,  morphT);
        float rx = (float)Lerp(fromGeo.Rx, toGeo.Rx, morphT);

        double stackOp = formOp[0];
        bool dimmed = anySelected && !selected && stackOp > 0.7;
        float layerAlpha = (dimmed ? 0.4f : 1f) * globalAlpha;
        if (layerAlpha < 0.01f) return;

        bool selectedNow = selected && stackOp > 0.5;
        var stroke = selectedNow ? _strokeInkBold : _strokeInk;
        var fillBase = selectedNow ? InkAlpha(0.10) : InkAlpha(0.04);

        var rect = new SKRect(x, y, x + w, y + h);

        // Front-face fill
        using (var p = new SKPaint { Color = fillBase.WithAlpha((byte)(fillBase.Alpha * layerAlpha)), IsAntialias = true })
        {
            if (rx > 0) canvas.DrawRoundRect(rect, rx, rx, p);
            else canvas.DrawRect(rect, p);
        }

        // Cross-fade interiors
        canvas.Save();
        if (rx > 0)
        {
            using var clipPath = new SKPath();
            clipPath.AddRoundRect(rect, rx, rx);
            canvas.ClipPath(clipPath, antialias: true);
        }
        else
        {
            canvas.ClipRect(rect);
        }

        for (int formIdx = 0; formIdx < 3; formIdx++)
        {
            float opacity = (float)formOp[formIdx] * layerAlpha;
            if (opacity < 0.02f) continue;
            DrawInterior(canvas, id, formIdx, x, y, w, h, opacity);
        }
        canvas.Restore();

        // Outline
        using (var p = new SKPaint {
            Color = Ink.WithAlpha((byte)(255 * layerAlpha)),
            IsStroke = true,
            StrokeWidth = selectedNow ? 1.4f : 0.9f,
            IsAntialias = true,
        })
        {
            if (rx > 0) canvas.DrawRoundRect(rect, rx, rx, p);
            else canvas.DrawRect(rect, p);
        }

        // Axonometric depth (stack form only)
        if (stackOp > 0.04)
        {
            float depthAlpha = (float)stackOp * layerAlpha;
            const float D = 8f;
            using var topPath = new SKPath();
            topPath.MoveTo(x, y);
            topPath.LineTo(x + w, y);
            topPath.LineTo(x + w + D, y - D * 0.6f);
            topPath.LineTo(x + D, y - D * 0.6f);
            topPath.Close();

            using var rightPath = new SKPath();
            rightPath.MoveTo(x + w, y);
            rightPath.LineTo(x + w + D, y - D * 0.6f);
            rightPath.LineTo(x + w + D, y + h - D * 0.6f);
            rightPath.LineTo(x + w, y + h);
            rightPath.Close();

            var topFillColor = selectedNow ? InkAlpha(0.06) : Paper2;
            using (var p = new SKPaint { Color = topFillColor.WithAlpha((byte)(topFillColor.Alpha * depthAlpha)), IsAntialias = true })
                canvas.DrawPath(topPath, p);
            using (var p = new SKPaint { Color = Paper2.WithAlpha((byte)(255 * depthAlpha)), IsAntialias = true })
                canvas.DrawPath(rightPath, p);
            using (var p = new SKPaint {
                Color = Ink.WithAlpha((byte)(255 * depthAlpha)),
                IsStroke = true,
                StrokeWidth = selectedNow ? 1.4f : 0.9f,
                IsAntialias = true,
            })
            {
                canvas.DrawPath(topPath, p);
                canvas.DrawPath(rightPath, p);
            }
        }

        // Selection brackets
        if (selected && stackOp > 0.5)
        {
            DrawSelectionBrackets(canvas, x, y, w, h, (float)stackOp * layerAlpha);
        }

        // Label flyout
        if (stackOp > 0.05)
        {
            DrawLabel(canvas, id, x, y, w, h, selected, (float)stackOp * layerAlpha);
        }
    }

    private void DrawSelectionBrackets(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const float reach = 6, off = 4;
        using var p = new SKPaint {
            Color = Ink.WithAlpha((byte)(255 * alpha)),
            IsStroke = true,
            StrokeWidth = 1.2f,
            IsAntialias = true,
        };

        // top-left
        using var tl = new SKPath();
        tl.MoveTo(x - off, y - off + reach);
        tl.LineTo(x - off, y - off);
        tl.LineTo(x - off + reach, y - off);
        canvas.DrawPath(tl, p);

        // top-right
        using var tr = new SKPath();
        tr.MoveTo(x + w + off - reach, y - off);
        tr.LineTo(x + w + off, y - off);
        tr.LineTo(x + w + off, y - off + reach);
        canvas.DrawPath(tr, p);

        // bottom-right
        using var br = new SKPath();
        br.MoveTo(x + w + off, y + h + off - reach);
        br.LineTo(x + w + off, y + h + off);
        br.LineTo(x + w + off - reach, y + h + off);
        canvas.DrawPath(br, p);

        // bottom-left
        using var bl = new SKPath();
        bl.MoveTo(x - off + reach, y + h + off);
        bl.LineTo(x - off, y + h + off);
        bl.LineTo(x - off, y + h + off - reach);
        canvas.DrawPath(bl, p);
    }

    private void DrawLabel(SKCanvas canvas, LayerId id, float x, float y, float w, float h, bool selected, float alpha)
    {
        if (!selected) return; // labels only show for the selected layer in this port

        var (label, sub, side) = id switch
        {
            LayerId.Plan         => ("PLAN",          new[] { "implementation-plan.md" },        "left"),
            LayerId.Architecture => ("ARCHITECTURE",  new[] { "ARCHITECTURE.md" },               "right"),
            LayerId.Design       => ("DESIGN SYSTEM", new[] { "DESIGN.md", "INTERACTIONS.md" },  "left"),
            LayerId.Wiring       => ("WIRING",        new[] { ".mcp.json" },                     "right"),
            LayerId.Foundation   => ("FOUNDATION",    new[] { "README.md", "CLAUDE.md" },        "left"),
            _ => ("", Array.Empty<string>(), "left"),
        };

        float cy = y + h / 2;
        float leaderX1 = side == "left" ? 100 : x + w + 8;
        float leaderX2 = side == "left" ? x : 340;
        float labelX = side == "left" ? leaderX1 - 4 : leaderX2 + 4;
        var align = side == "left" ? SKTextAlign.Right : SKTextAlign.Left;

        using var leaderPaint = new SKPaint {
            Color = Ink.WithAlpha((byte)(255 * alpha)),
            IsStroke = true,
            StrokeWidth = 0.5f,
            IsAntialias = true,
        };
        canvas.DrawLine(leaderX1, cy, leaderX2, cy, leaderPaint);

        using var dotPaint = new SKPaint { Color = Ink.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
        canvas.DrawCircle(side == "left" ? leaderX2 : leaderX1, cy, 1.4f, dotPaint);

        using var labelPaint = new SKPaint { Color = Ink.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
        _monoFont.Size = 10;
        canvas.DrawText(label, labelX, cy - 1, align, _monoFont, labelPaint);

        using var subPaint = new SKPaint { Color = Ink3.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
        _monoFont.Size = 6;
        for (int i = 0; i < sub.Length; i++)
        {
            canvas.DrawText(sub[i], labelX, cy + 9 + i * 8, align, _monoFont, subPaint);
        }
        _monoFont.Size = 10; // reset
    }

    private void DrawCaption(SKCanvas canvas, string text, float cx, float cy, float alpha)
    {
        if (alpha < 0.02f) return;
        using var p = new SKPaint { Color = Ink2.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
        _serifFont.Size = 10;
        canvas.DrawText(text, cx, cy, SKTextAlign.Center, _serifFont, p);
    }

    // ─── Per-form interiors ───────────────────────────────────────────────
    //
    // Dispatched by (layerId, formIdx). Each method draws the layer's interior
    // detail at the given geometry and opacity. These mirror the React
    // reference's STACK_RENDER, ASS_RENDER (mobile), and ASS_RENDER (desktop).

    private void DrawInterior(SKCanvas canvas, LayerId id, int formIdx,
                              float x, float y, float w, float h, float alpha)
    {
        switch ((id, formIdx))
        {
            case (LayerId.Plan, 0):         StackPlan(canvas, x, y, w, h, alpha); break;
            case (LayerId.Architecture, 0): StackArchitecture(canvas, x, y, w, h, alpha); break;
            case (LayerId.Design, 0):       StackDesign(canvas, x, y, w, h, alpha); break;
            case (LayerId.Wiring, 0):       StackWiring(canvas, x, y, w, h, alpha); break;
            case (LayerId.Foundation, 0):   StackFoundation(canvas, x, y, w, h, alpha); break;

            case (LayerId.Plan, 1):         MobilePlan(canvas, x, y, w, h, alpha); break;
            case (LayerId.Architecture, 1): MobileArchitecture(canvas, x, y, w, h, alpha); break;
            case (LayerId.Design, 1):       MobileDesign(canvas, x, y, w, h, alpha); break;
            case (LayerId.Wiring, 1):       MobileWiring(canvas, x, y, w, h, alpha); break;
            case (LayerId.Foundation, 1):   MobileFoundation(canvas, x, y, w, h, alpha); break;

            case (LayerId.Plan, 2):         DesktopPlan(canvas, x, y, w, h, alpha); break;
            case (LayerId.Architecture, 2): DesktopArchitecture(canvas, x, y, w, h, alpha); break;
            case (LayerId.Design, 2):       DesktopDesign(canvas, x, y, w, h, alpha); break;
            case (LayerId.Wiring, 2):       DesktopWiring(canvas, x, y, w, h, alpha); break;
            case (LayerId.Foundation, 2):   DesktopFoundation(canvas, x, y, w, h, alpha); break;
        }
    }

    // ─── Helper: alpha-applied paints ──────────────────────────────────────

    private static SKPaint StrokePaint(SKColor color, float strokeWidth, float alpha) =>
        new() {
            Color = color.WithAlpha((byte)(color.Alpha * alpha)),
            IsStroke = true,
            StrokeWidth = strokeWidth,
            IsAntialias = true,
        };

    private static SKPaint FillPaint(SKColor color, float alpha) =>
        new() {
            Color = color.WithAlpha((byte)(color.Alpha * alpha)),
            IsAntialias = true,
        };

    // ─── Stack interiors ───────────────────────────────────────────────────

    private static void StackPlan(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float rowH = Math.Max(2.5f, Math.Min(4f, h * 0.13f));
        var rows = new[] {
            (rowY: y + h * 0.16f, segs: new[] { (0.04f, 0.4f), (0.42f, 0.7f) }),
            (rowY: y + h * 0.46f, segs: new[] { (0.04f, 0.55f), (0.58f, 0.84f) }),
            (rowY: y + h * 0.76f, segs: new[] { (0.04f, 0.45f), (0.48f, 0.78f) }),
        };
        for (int ri = 0; ri < rows.Length; ri++)
        {
            var row = rows[ri];
            for (int i = 0; i < row.segs.Length; i++)
            {
                var (a, b) = row.segs[i];
                float opacity = i == 0 ? 0.85f : 0.45f;
                using var p = FillPaint(Ink, alpha * opacity);
                canvas.DrawRect(new SKRect(x + w * a, row.rowY - rowH / 2,
                                           x + w * b, row.rowY - rowH / 2 + rowH), p);
            }
        }
    }

    private static void StackArchitecture(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float midY = y + h / 2;
        using var p1 = StrokePaint(Ink, 1f, alpha);
        foreach (var f in new[] { 0.25f, 0.5f, 0.75f })
            canvas.DrawLine(x + w * f, y, x + w * f, y + h, p1);
        canvas.DrawLine(x, midY, x + w, midY, p1);
        using var p2 = StrokePaint(Ink, 0.6f, alpha * 0.7f);
        canvas.DrawLine(x, y, x + w * 0.25f, midY, p2);
        canvas.DrawLine(x + w * 0.25f, y, x, midY, p2);
    }

    private static void StackDesign(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float swatchW = (w - 16) / 6;
        float swatchH = h - 12;
        float swatchY = y + 6;
        var fills = new[] {
            Paper, InkAlpha(0.08), InkAlpha(0.30), InkAlpha(0.65), Ink, new SKColor(0,0,0),
        };
        using var stroke = StrokePaint(Ink, 0.5f, alpha);
        for (int i = 0; i < 6; i++)
        {
            using var p = FillPaint(fills[i], alpha);
            var r = new SKRect(x + 8 + i * swatchW, swatchY,
                               x + 8 + i * swatchW + swatchW - 2, swatchY + swatchH);
            canvas.DrawRect(r, p);
            canvas.DrawRect(r, stroke);
        }
    }

    private static void StackWiring(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float midY = y + h / 2;
        using var pinPaint = StrokePaint(Ink, 0.7f, alpha);
        for (int i = 0; i < 5; i++)
        {
            float px = x + (w * (i + 1)) / 6;
            canvas.DrawLine(px, y, px, y - 5, pinPaint);
        }

        using var dashPaint = new SKPaint {
            Color = Ink.WithAlpha((byte)(Ink.Alpha * alpha * 0.7f)),
            IsStroke = true,
            StrokeWidth = 0.5f,
            PathEffect = SKPathEffect.CreateDash([2f, 2f], 0),
            IsAntialias = true,
        };
        canvas.DrawLine(x + 6, midY, x + w - 6, midY, dashPaint);

        using var nodeStroke = StrokePaint(Ink, 0.7f, alpha);
        using var nodeFill = FillPaint(Paper, alpha);
        foreach (var t in new[] { 0.27f, 0.5f, 0.73f })
        {
            float nx = x + w * t;
            canvas.DrawCircle(nx, midY, 2.2f, nodeFill);
            canvas.DrawCircle(nx, midY, 2.2f, nodeStroke);
        }
    }

    private static void StackFoundation(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        // Faint grid via stroke pattern (instead of SVG <pattern>)
        using var gridPaint = new SKPaint {
            Color = Ink.WithAlpha((byte)(Ink.Alpha * alpha * 0.55f * 0.7f)),
            IsStroke = true,
            StrokeWidth = 0.35f,
            IsAntialias = true,
        };
        for (float gx = x; gx <= x + w; gx += 6)
            canvas.DrawLine(gx, y, gx, y + h, gridPaint);
        for (float gy = y; gy <= y + h; gy += 6)
            canvas.DrawLine(x, gy, x + w, gy, gridPaint);

        using var pinPaint = StrokePaint(Ink, 0.7f, alpha);
        foreach (var t in new[] { 0.15f, 0.5f, 0.85f })
            canvas.DrawLine(x + w * t, y + h, x + w * t, y + h + 5, pinPaint);

        using var underline = StrokePaint(Hairline, 0.5f, alpha);
        canvas.DrawLine(x - 6, y + h + 9, x + w + 6, y + h + 9, underline);
    }

    // ─── Mobile interiors ─────────────────────────────────────────────────

    private static void MobilePlan(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const int rows = 3;
        float rowH = h / rows;
        const float padX = 8;
        for (int i = 0; i < rows; i++)
        {
            float ry = y + i * rowH + rowH * 0.14f;
            float thumbSize = Math.Min(rowH * 0.72f, 38);
            float tx = x + padX;
            float lineX = tx + thumbSize + 9;
            float lineMaxW = x + w - padX - lineX;

            using var thumbFill = FillPaint(InkAlpha(0.07), alpha);
            using var thumbStroke = StrokePaint(Ink, 0.6f, alpha);
            var thumbRect = new SKRect(tx, ry, tx + thumbSize, ry + thumbSize);
            canvas.DrawRect(thumbRect, thumbFill);
            canvas.DrawRect(thumbRect, thumbStroke);

            using var diag = StrokePaint(Ink, 0.4f, alpha * 0.4f);
            canvas.DrawLine(tx, ry + thumbSize, tx + thumbSize, ry, diag);

            using (var p = FillPaint(Ink, alpha * 0.85f))
                canvas.DrawRect(new SKRect(lineX, ry + thumbSize * 0.18f,
                                           lineX + lineMaxW * 0.78f, ry + thumbSize * 0.18f + 3.5f), p);
            using (var p = FillPaint(Ink, alpha * 0.45f))
                canvas.DrawRect(new SKRect(lineX, ry + thumbSize * 0.42f,
                                           lineX + lineMaxW * 0.55f, ry + thumbSize * 0.42f + 2.5f), p);
            using (var p = FillPaint(Ink, alpha * 0.30f))
                canvas.DrawRect(new SKRect(lineX, ry + thumbSize * 0.65f,
                                           lineX + lineMaxW * 0.86f, ry + thumbSize * 0.65f + 2f), p);
            using (var p = FillPaint(Ink, alpha * 0.5f))
                canvas.DrawRect(new SKRect(lineX, ry + thumbSize * 0.86f,
                                           lineX + lineMaxW * 0.22f, ry + thumbSize * 0.86f + 2f), p);

            if (i < rows - 1)
            {
                using var divider = StrokePaint(Hairline, 0.5f, alpha);
                canvas.DrawLine(x + padX, y + (i + 1) * rowH,
                                x + w - padX, y + (i + 1) * rowH, divider);
            }
        }
    }

    private static void MobileArchitecture(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float cy = y + h / 2;
        using var hambPaint = StrokePaint(Ink, 1.2f, alpha);
        canvas.DrawLine(x + 10, cy - 5, x + 22, cy - 5, hambPaint);
        canvas.DrawLine(x + 10, cy,     x + 22, cy,     hambPaint);
        canvas.DrawLine(x + 10, cy + 5, x + 22, cy + 5, hambPaint);

        using (var p = FillPaint(Ink, alpha * 0.85f))
            canvas.DrawRect(new SKRect(x + 32, cy - 5,
                                       x + 32 + Math.Min(w * 0.45f, 70), cy - 5 + 4), p);
        using (var p = FillPaint(Ink, alpha * 0.45f))
            canvas.DrawRect(new SKRect(x + 32, cy + 1,
                                       x + 32 + Math.Min(w * 0.30f, 50), cy + 1 + 2.5f), p);

        using var actionStroke = StrokePaint(Ink, 0.8f, alpha);
        canvas.DrawCircle(x + w - 28, cy, 3.2f, actionStroke);

        using var avatarFill = FillPaint(InkAlpha(0.08), alpha);
        using var avatarStroke = StrokePaint(Ink, 0.7f, alpha);
        canvas.DrawCircle(x + w - 14, cy, 6, avatarFill);
        canvas.DrawCircle(x + w - 14, cy, 6, avatarStroke);
    }

    private static void MobileDesign(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float swatchW = (w - 16) / 6;
        float swatchH = Math.Min(h - 4, 14);
        float swatchY = y + (h - swatchH) / 2;
        var fills = new[] {
            Paper, InkAlpha(0.08), InkAlpha(0.30), InkAlpha(0.65), Ink, new SKColor(0,0,0),
        };
        using var stroke = StrokePaint(Ink, 0.5f, alpha);
        for (int i = 0; i < 6; i++)
        {
            using var p = FillPaint(fills[i], alpha);
            var r = new SKRect(x + 8 + i * swatchW, swatchY,
                               x + 8 + i * swatchW + swatchW - 3, swatchY + swatchH);
            canvas.DrawRoundRect(r, swatchH / 2, swatchH / 2, p);
            canvas.DrawRoundRect(r, swatchH / 2, swatchH / 2, stroke);
        }
    }

    private static void MobileWiring(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        var tabs = new[] {
            (px: x + w * 0.20f, type: "home",    sel: false),
            (px: x + w * 0.50f, type: "square",  sel: true),
            (px: x + w * 0.80f, type: "profile", sel: false),
        };
        float iconY = y + h * 0.36f;
        float labelY = y + h * 0.78f;

        using var topDivider = StrokePaint(Ink, 0.5f, alpha * 0.5f);
        canvas.DrawLine(x + 6, y + 1, x + w - 6, y + 1, topDivider);

        foreach (var tab in tabs)
        {
            float cx = tab.px;
            float op = tab.sel ? 1f : 0.5f;
            float effAlpha = alpha * op;

            if (tab.sel)
            {
                using var pip = FillPaint(Ink, alpha);
                canvas.DrawRoundRect(new SKRect(cx - 6, y + 3, cx + 6, y + 5), 1, 1, pip);
            }

            switch (tab.type)
            {
                case "home":
                {
                    using var path = new SKPath();
                    path.MoveTo(cx - 5, iconY + 4);
                    path.LineTo(cx - 5, iconY - 1);
                    path.LineTo(cx,     iconY - 5);
                    path.LineTo(cx + 5, iconY - 1);
                    path.LineTo(cx + 5, iconY + 4);
                    path.Close();
                    if (tab.sel)
                    {
                        using var fill = FillPaint(Ink, effAlpha);
                        canvas.DrawPath(path, fill);
                    }
                    using var s = StrokePaint(Ink, 0.9f, effAlpha);
                    s.StrokeJoin = SKStrokeJoin.Round;
                    canvas.DrawPath(path, s);
                    break;
                }
                case "square":
                {
                    var r = new SKRect(cx - 5, iconY - 4, cx + 5, iconY + 5);
                    if (tab.sel)
                    {
                        using var fill = FillPaint(Ink, effAlpha);
                        canvas.DrawRect(r, fill);
                    }
                    using var s = StrokePaint(Ink, 0.9f, effAlpha);
                    canvas.DrawRect(r, s);
                    break;
                }
                case "profile":
                {
                    if (tab.sel)
                    {
                        using var fill = FillPaint(Ink, effAlpha);
                        canvas.DrawCircle(cx, iconY - 2, 3, fill);
                    }
                    using var s = StrokePaint(Ink, 0.9f, effAlpha);
                    canvas.DrawCircle(cx, iconY - 2, 3, s);
                    using var arc = new SKPath();
                    arc.MoveTo(cx - 5, iconY + 5);
                    arc.QuadTo(cx, iconY + 1, cx + 5, iconY + 5);
                    canvas.DrawPath(arc, s);
                    break;
                }
            }

            using var label = FillPaint(Ink, alpha * (tab.sel ? 0.85f : 0.55f));
            canvas.DrawRect(new SKRect(cx - 9, labelY, cx + 9, labelY + 2), label);
        }
    }

    private static void MobileFoundation(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        // Faint background grid
        using var gridPaint = new SKPaint {
            Color = Ink.WithAlpha((byte)(Ink.Alpha * alpha * 0.18f * 0.7f)),
            IsStroke = true,
            StrokeWidth = 0.35f,
            IsAntialias = true,
        };
        for (float gx = x; gx <= x + w; gx += 6)
            canvas.DrawLine(gx, y, gx, y + h, gridPaint);
        for (float gy = y; gy <= y + h; gy += 6)
            canvas.DrawLine(x, gy, x + w, gy, gridPaint);

        // Status bar elements
        using (var p = FillPaint(Ink, alpha * 0.85f))
            canvas.DrawRect(new SKRect(x + 14, y + 9, x + 28, y + 12), p);

        using (var p = FillPaint(Ink, alpha))
            canvas.DrawRoundRect(new SKRect(x + w / 2 - 18, y + 6.5f,
                                             x + w / 2 + 18, y + 15.5f), 4.5f, 4.5f, p);

        // Signal bars
        using (var p = FillPaint(Ink, alpha * 0.8f))
            canvas.DrawRect(new SKRect(x + w - 36, y + 11, x + w - 34, y + 14), p);
        using (var p = FillPaint(Ink, alpha * 0.9f))
            canvas.DrawRect(new SKRect(x + w - 32, y + 10, x + w - 30, y + 14), p);
        using (var p = FillPaint(Ink, alpha))
            canvas.DrawRect(new SKRect(x + w - 28, y + 9, x + w - 26, y + 14), p);

        // Battery
        using (var p = StrokePaint(Ink, 0.5f, alpha))
            canvas.DrawRoundRect(new SKRect(x + w - 22, y + 9, x + w - 11, y + 14), 0.8f, 0.8f, p);
        using (var p = FillPaint(Ink, alpha))
            canvas.DrawRect(new SKRect(x + w - 20, y + 10.3f, x + w - 14, y + 12.7f), p);
        using (var p = FillPaint(Ink, alpha * 0.8f))
            canvas.DrawRect(new SKRect(x + w - 10, y + 10.3f, x + w - 9, y + 12.7f), p);

        // Home indicator
        using (var p = FillPaint(Ink, alpha * 0.85f))
            canvas.DrawRoundRect(new SKRect(x + w / 2 - 26, y + h - 9,
                                             x + w / 2 + 26, y + h - 6), 1.5f, 1.5f, p);
    }

    // ─── Desktop interiors ────────────────────────────────────────────────

    private static void DesktopPlan(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const float headerH = 22;
        const int rowsCount = 4;
        float rowH = (h - headerH) / rowsCount;
        const float padX = 10;
        var cols = new[] {
            (start: 0.00f, w: 0.40f),
            (start: 0.40f, w: 0.20f),
            (start: 0.60f, w: 0.20f),
            (start: 0.80f, w: 0.20f),
        };

        using (var p = FillPaint(InkAlpha(0.03), alpha))
            canvas.DrawRect(new SKRect(x, y, x + w, y + headerH), p);
        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x, y + headerH, x + w, y + headerH, p);

        using (var p = FillPaint(Ink, alpha * 0.6f))
        {
            foreach (var col in cols)
            {
                float colW = w * col.w;
                canvas.DrawRect(new SKRect(
                    x + col.start * w + padX,
                    y + headerH / 2 - 1.25f,
                    x + col.start * w + padX + Math.Min(colW * 0.4f, 30),
                    y + headerH / 2 - 1.25f + 2.5f), p);
            }
        }

        // Sort caret
        using (var p = StrokePaint(Ink, 0.7f, alpha * 0.6f))
        {
            using var caret = new SKPath();
            caret.MoveTo(x + padX + 32, y + headerH / 2 - 1.5f);
            caret.RLineTo(2, 2);
            caret.RLineTo(2, -2);
            canvas.DrawPath(caret, p);
        }

        var rows = new[] {
            (statusFill: InkAlpha(0.10), ownerFill: InkAlpha(0.10), titleW: 0.80f, subW: 0.50f, statusFilled: false),
            (statusFill: Ink,            ownerFill: InkAlpha(0.18), titleW: 0.65f, subW: 0.40f, statusFilled: true),
            (statusFill: InkAlpha(0.10), ownerFill: InkAlpha(0.10), titleW: 0.72f, subW: 0.55f, statusFilled: false),
            (statusFill: SKColors.Transparent, ownerFill: InkAlpha(0.06), titleW: 0.58f, subW: 0.42f, statusFilled: false),
        };

        for (int ri = 0; ri < rows.Length; ri++)
        {
            var row = rows[ri];
            float ry = y + headerH + ri * rowH;
            float cy = ry + rowH / 2;

            if (ri > 0)
            {
                using var divider = StrokePaint(Hairline, 0.5f, alpha);
                canvas.DrawLine(x, ry, x + w, ry, divider);
            }

            // Avatar + title + subtitle
            using (var p = FillPaint(InkAlpha(0.08), alpha))
                canvas.DrawCircle(x + padX + 6, cy, 5.5f, p);
            using (var p = StrokePaint(Ink, 0.6f, alpha))
                canvas.DrawCircle(x + padX + 6, cy, 5.5f, p);

            float titleW = (w * 0.40f - padX - 18 - 8) * row.titleW;
            float subW = (w * 0.40f - padX - 18 - 8) * row.subW;
            using (var p = FillPaint(Ink, alpha * 0.85f))
                canvas.DrawRect(new SKRect(x + padX + 16, cy - 5,
                                           x + padX + 16 + titleW, cy - 5 + 3), p);
            using (var p = FillPaint(Ink, alpha * 0.45f))
                canvas.DrawRect(new SKRect(x + padX + 16, cy + 0.5f,
                                           x + padX + 16 + subW, cy + 0.5f + 2.2f), p);

            // Status pill
            var pillRect = new SKRect(x + 0.40f * w + padX, cy - 5.5f,
                                       x + 0.40f * w + padX + 42, cy + 5.5f);
            if (row.statusFill != SKColors.Transparent)
            {
                using var p = FillPaint(row.statusFill, alpha);
                canvas.DrawRoundRect(pillRect, 5.5f, 5.5f, p);
            }
            using (var p = StrokePaint(Ink, 0.5f, alpha))
                canvas.DrawRoundRect(pillRect, 5.5f, 5.5f, p);

            var pillTextColor = row.statusFilled ? Paper : Ink;
            float pillTextOpacity = row.statusFilled ? 0.85f : 0.55f;
            using (var p = FillPaint(pillTextColor, alpha * pillTextOpacity))
                canvas.DrawRect(new SKRect(x + 0.40f * w + padX + 6, cy - 1.25f,
                                           x + 0.40f * w + padX + 6 + 28, cy - 1.25f + 2.2f), p);

            // Owner
            using (var p = FillPaint(row.ownerFill, alpha))
                canvas.DrawCircle(x + 0.60f * w + padX + 4, cy, 4, p);
            using (var p = StrokePaint(Ink, 0.5f, alpha))
                canvas.DrawCircle(x + 0.60f * w + padX + 4, cy, 4, p);
            using (var p = FillPaint(Ink, alpha * 0.55f))
                canvas.DrawRect(new SKRect(x + 0.60f * w + padX + 12, cy - 1.25f,
                                           x + 0.60f * w + padX + 12 + Math.Min(w * 0.20f * 0.6f, 35),
                                           cy - 1.25f + 2.5f), p);

            // Date
            using (var p = FillPaint(Ink, alpha * 0.55f))
                canvas.DrawRect(new SKRect(x + 0.80f * w + padX, cy - 1.25f,
                                           x + 0.80f * w + padX + Math.Min(w * 0.20f * 0.55f, 32),
                                           cy - 1.25f + 2.5f), p);
        }
    }

    private static void DesktopArchitecture(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const float brandH = 30;
        float navStartY = y + brandH + 6;
        const int navItems = 4;
        const float userBlockH = 32;
        float navAreaH = h - brandH - userBlockH - 12;
        float itemH = navAreaH / navItems;
        const float padX = 8;

        // Brand
        using (var p = FillPaint(InkAlpha(0.14), alpha))
            canvas.DrawRoundRect(new SKRect(x + padX, y + 8, x + padX + 14, y + 22), 3, 3, p);
        using (var p = StrokePaint(Ink, 0.6f, alpha))
            canvas.DrawRoundRect(new SKRect(x + padX, y + 8, x + padX + 14, y + 22), 3, 3, p);
        using (var p = FillPaint(Ink, alpha * 0.85f))
            canvas.DrawRect(new SKRect(x + padX + 20, y + 11,
                                       x + padX + 20 + Math.Min(w * 0.5f, 50), y + 11 + 3.5f), p);
        using (var p = FillPaint(Ink, alpha * 0.40f))
            canvas.DrawRect(new SKRect(x + padX + 20, y + 17,
                                       x + padX + 20 + Math.Min(w * 0.32f, 32), y + 17 + 2.2f), p);

        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x + padX - 2, y + brandH + 2,
                            x + w - padX + 2, y + brandH + 2, p);

        // Nav items
        for (int i = 0; i < navItems; i++)
        {
            float iy = navStartY + i * itemH;
            float cy = iy + itemH / 2;
            bool isSel = i == 1;

            if (isSel)
            {
                using var p = FillPaint(InkAlpha(0.08), alpha);
                canvas.DrawRoundRect(new SKRect(x + 4, iy + 2, x + w - 4, iy + itemH - 2), 3, 3, p);
            }

            // Icon
            var iconRect = new SKRect(x + padX + 2, cy - 4.5f, x + padX + 11, cy + 4.5f);
            if (isSel)
            {
                using var p = FillPaint(Ink, alpha);
                canvas.DrawRoundRect(iconRect, 1.5f, 1.5f, p);
            }
            using (var p = StrokePaint(Ink, 0.7f, alpha * (isSel ? 1f : 0.7f)))
                canvas.DrawRoundRect(iconRect, 1.5f, 1.5f, p);

            using (var p = FillPaint(Ink, alpha * (isSel ? 0.9f : 0.55f)))
                canvas.DrawRect(new SKRect(x + padX + 16, cy - 1.5f,
                                           x + padX + 16 + Math.Min(w * 0.55f, 60), cy - 1.5f + 3), p);
        }

        // User block
        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x + padX - 2, y + h - userBlockH,
                            x + w - padX + 2, y + h - userBlockH, p);

        using (var p = FillPaint(InkAlpha(0.12), alpha))
            canvas.DrawCircle(x + padX + 6, y + h - userBlockH / 2, 6, p);
        using (var p = StrokePaint(Ink, 0.6f, alpha))
            canvas.DrawCircle(x + padX + 6, y + h - userBlockH / 2, 6, p);

        using (var p = FillPaint(Ink, alpha * 0.65f))
            canvas.DrawRect(new SKRect(x + padX + 16, y + h - userBlockH / 2 - 3.5f,
                                       x + padX + 16 + Math.Min(w * 0.5f, 50),
                                       y + h - userBlockH / 2 - 3.5f + 3), p);
        using (var p = FillPaint(Ink, alpha * 0.35f))
            canvas.DrawRect(new SKRect(x + padX + 16, y + h - userBlockH / 2 + 1,
                                       x + padX + 16 + Math.Min(w * 0.35f, 35),
                                       y + h - userBlockH / 2 + 1 + 2.2f), p);
    }

    private static void DesktopDesign(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        var swatches = new[] {
            Paper, InkAlpha(0.08), InkAlpha(0.30), InkAlpha(0.65), Ink, new SKColor(0,0,0),
        };
        const float padX = 6;
        float swH = h - 6;
        float swY = y + 3;
        float segW = Math.Min(w * 0.42f, 160);
        float swW = (segW - padX * 2) / swatches.Length;
        float searchX = x + segW + 12;
        float searchW = w * 0.30f;
        const float actionW = 36;
        float actionX = x + w - actionW - 8;

        // Segmented container
        using (var p = FillPaint(InkAlpha(0.04), alpha))
            canvas.DrawRoundRect(new SKRect(x + padX, swY, x + segW - padX, swY + swH), swH / 2, swH / 2, p);
        using (var p = StrokePaint(Ink, 0.5f, alpha))
            canvas.DrawRoundRect(new SKRect(x + padX, swY, x + segW - padX, swY + swH), swH / 2, swH / 2, p);

        for (int i = 0; i < swatches.Length; i++)
        {
            var r = new SKRect(x + padX + i * swW + 1.5f, swY + 1.5f,
                                x + padX + i * swW + 1.5f + swW - 3, swY + 1.5f + swH - 3);
            using (var p = FillPaint(swatches[i], alpha))
                canvas.DrawRoundRect(r, (swH - 3) / 2, (swH - 3) / 2, p);
            using (var p = StrokePaint(Ink, 0.4f, alpha))
                canvas.DrawRoundRect(r, (swH - 3) / 2, (swH - 3) / 2, p);
        }

        // Search field
        var searchRect = new SKRect(searchX, swY, searchX + searchW, swY + swH);
        using (var p = FillPaint(InkAlpha(0.03), alpha))
            canvas.DrawRoundRect(searchRect, swH / 2, swH / 2, p);
        using (var p = StrokePaint(Ink, 0.5f, alpha))
            canvas.DrawRoundRect(searchRect, swH / 2, swH / 2, p);

        using (var p = StrokePaint(Ink, 0.6f, alpha))
        {
            canvas.DrawCircle(searchX + 8, swY + swH / 2, 2.5f, p);
            canvas.DrawLine(searchX + 10, swY + swH / 2 + 2,
                            searchX + 12, swY + swH / 2 + 4, p);
        }
        using (var p = FillPaint(Ink, alpha * 0.4f))
            canvas.DrawRect(new SKRect(searchX + 16, swY + swH / 2 - 1,
                                       searchX + 16 + searchW * 0.4f, swY + swH / 2 + 1), p);

        // Secondary icon button (plus icon)
        var secRect = new SKRect(actionX - 24, swY, actionX - 24 + swH, swY + swH);
        using (var p = StrokePaint(Ink, 0.5f, alpha))
            canvas.DrawRoundRect(secRect, 3, 3, p);
        using (var p = StrokePaint(Ink, 0.7f, alpha * 0.7f))
        {
            canvas.DrawLine(actionX - 24 + swH / 2, swY + 4,
                            actionX - 24 + swH / 2, swY + swH - 4, p);
            canvas.DrawLine(actionX - 24 + 4, swY + swH / 2,
                            actionX - 24 + swH - 4, swY + swH / 2, p);
        }

        // Primary action
        var primRect = new SKRect(actionX, swY, actionX + actionW, swY + swH);
        using (var p = FillPaint(Ink, alpha))
            canvas.DrawRoundRect(primRect, 3, 3, p);
        using (var p = FillPaint(Paper, alpha * 0.85f))
            canvas.DrawRect(new SKRect(actionX + 8, swY + swH / 2 - 1.2f,
                                       actionX + actionW - 8, swY + swH / 2 + 1.2f), p);
    }

    private static void DesktopWiring(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        float cy = y + h / 2;

        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x, y, x + w, y, p);

        using (var p = FillPaint(Ink, alpha))
            canvas.DrawCircle(x + 10, cy, 2.5f, p);
        using (var p = FillPaint(Ink, alpha * 0.7f))
            canvas.DrawRect(new SKRect(x + 18, cy - 1.25f, x + 63, cy + 1.25f), p);

        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x + 70, cy - 4, x + 70, cy + 4, p);

        using (var p = FillPaint(Ink, alpha * 0.45f))
            canvas.DrawRect(new SKRect(x + 78, cy - 1.25f, x + 134, cy + 1.25f), p);

        using (var dotFill = FillPaint(Paper, alpha))
        using (var dotStroke = StrokePaint(Ink, 0.7f, alpha))
        {
            canvas.DrawCircle(x + w - 30, cy, 2.2f, dotFill);
            canvas.DrawCircle(x + w - 30, cy, 2.2f, dotStroke);
            canvas.DrawCircle(x + w - 20, cy, 2.2f, dotFill);
            canvas.DrawCircle(x + w - 20, cy, 2.2f, dotStroke);
        }
        using (var p = FillPaint(Ink, alpha))
            canvas.DrawCircle(x + w - 10, cy, 2.2f, p);
        using (var p = StrokePaint(Ink, 0.7f, alpha))
            canvas.DrawCircle(x + w - 10, cy, 2.2f, p);
    }

    private static void DesktopFoundation(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const float tbH = 26;

        // Background grid (very faint)
        using var gridPaint = new SKPaint {
            Color = Ink.WithAlpha((byte)(Ink.Alpha * alpha * 0.12f * 0.7f)),
            IsStroke = true,
            StrokeWidth = 0.35f,
            IsAntialias = true,
        };
        for (float gx = x; gx <= x + w; gx += 6)
            canvas.DrawLine(gx, y, gx, y + h, gridPaint);
        for (float gy = y; gy <= y + h; gy += 6)
            canvas.DrawLine(x, gy, x + w, gy, gridPaint);

        // Title bar background
        using (var p = FillPaint(InkAlpha(0.04), alpha))
        {
            using var tbPath = new SKPath();
            tbPath.MoveTo(x + 8, y);
            tbPath.LineTo(x + w - 8, y);
            tbPath.QuadTo(x + w, y, x + w, y + 8);
            tbPath.LineTo(x + w, y + tbH);
            tbPath.LineTo(x, y + tbH);
            tbPath.LineTo(x, y + 8);
            tbPath.QuadTo(x, y, x + 8, y);
            tbPath.Close();
            canvas.DrawPath(tbPath, p);
        }

        // Traffic lights
        var tlColors = new[] { InkAlpha(0.10), InkAlpha(0.22), InkAlpha(0.40) };
        for (int i = 0; i < 3; i++)
        {
            float cx = x + 14 + i * 12;
            using (var p = FillPaint(tlColors[i], alpha))
                canvas.DrawCircle(cx, y + 13, 3.6f, p);
            using (var p = StrokePaint(Ink, 0.6f, alpha))
                canvas.DrawCircle(cx, y + 13, 3.6f, p);
        }

        // Window title
        using (var p = FillPaint(Ink, alpha * 0.65f))
            canvas.DrawRect(new SKRect(x + w / 2 - 32, y + 11,
                                       x + w / 2 + 32, y + 14.5f), p);

        // Right-side window controls (3 lines)
        using (var p = StrokePaint(Ink, 0.8f, alpha * 0.7f))
        {
            canvas.DrawLine(x + w - 22, y + 9,  x + w - 12, y + 9, p);
            canvas.DrawLine(x + w - 22, y + 13, x + w - 12, y + 13, p);
            canvas.DrawLine(x + w - 22, y + 17, x + w - 12, y + 17, p);
        }

        // Title bar separator
        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x, y + tbH, x + w, y + tbH, p);

        // Sidebar/main divider
        using (var p = StrokePaint(Hairline, 0.5f, alpha))
            canvas.DrawLine(x + 118, y + tbH, x + 118, y + h, p);
    }
}
