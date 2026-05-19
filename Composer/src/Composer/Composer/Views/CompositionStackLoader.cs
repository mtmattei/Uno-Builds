// CompositionStackLoader.cs
//
// Brief loading animation that plays while the Download Bundle is saving.
// Pure SkiaSharp — renders identically across all Uno targets (Windows, Mac,
// iOS, Android, WASM, Linux) with no XAML animations.
//
// Trimmed to ~3.5s total for the bundle-save use case (the full ~18s reference
// runs the stack → mobile → desktop morph; here we just want the brief reveal
// of the composition stack to acknowledge the user's click).
//
// Host calls Skip() once the actual bundle export completes — animation fades
// to clean and fires AnimationCompleted within ~400ms regardless of progress.

using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace Composer.Views;

public sealed class CompositionStackLoader : UserControl
{
    public event EventHandler? AnimationCompleted;

    public void Skip()
    {
        if (_finished) return;
        _skipping = true;
        _skipStartMs = ElapsedMs;
    }

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

    /// <summary>Restarts the animation from frame zero.</summary>
    public void Restart()
    {
        _finished = false;
        _skipping = false;
        _skipStartMs = 0;
        _clock.Restart();
        _canvas.Invalidate();
        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => { if (!_finished) _canvas.Invalidate(); };
        }
        if (!_timer.IsEnabled) _timer.Start();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Don't auto-start — the caller invokes Restart() when the loader
        // should actually play. Otherwise the animation runs invisibly at
        // app startup and is "finished" before the user ever sees it.
        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => { if (!_finished) _canvas.Invalidate(); };
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        _timer = null;
        _clock.Stop();
    }

    private record Phase(int SelectedIdx, int FromForm, int ToForm, double DurMs);

    // Brief variant — five fast reveals (550ms each) + a short hold = ~3.5s total.
    private static readonly Phase[] Phases =
    [
        new(0, 0, 0, 550),
        new(1, 0, 0, 550),
        new(2, 0, 0, 550),
        new(3, 0, 0, 550),
        new(4, 0, 0, 550),
        new(-1, 0, 0, 700),
    ];

    private static readonly double TotalDurationMs = SumDurations();
    private static double SumDurations()
    {
        double s = 0;
        foreach (var p in Phases) s += p.DurMs;
        return s;
    }

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
        return (Phases[^1], 1.0);
    }

    private static double EaseInOutCubic(double t) =>
        t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private enum LayerId { Plan, Architecture, Design, Wiring, Foundation }

    private record Geo(double X, double Y, double W, double H, double Rx = 0);

    private static readonly Geo[][] Geos =
    [
        // Stack
        [
            new(140, 60,  160, 30),
            new(140, 115, 160, 48),
            new(140, 188, 160, 40),
            new(140, 253, 160, 22),
            new(120, 300, 200, 22, 0),
        ],
    ];

    private static readonly LayerId[] RenderOrder =
        [LayerId.Foundation, LayerId.Wiring, LayerId.Design, LayerId.Architecture, LayerId.Plan];

    private static readonly SKColor Paper    = new(0xFF, 0xFF, 0xFF);
    private static readonly SKColor Paper2   = new(0xFA, 0xFA, 0xFA);
    private static readonly SKColor Ink      = new(0x18, 0x18, 0x1B);
    private static readonly SKColor Ink2     = new(0x3F, 0x3F, 0x46);
    private static readonly SKColor Ink3     = new(0x71, 0x71, 0x7A);
    private static readonly SKColor Hairline = new(0xE4, 0xE4, 0xE7);

    private static SKColor InkAlpha(double a) =>
        new(0x18, 0x18, 0x1B, (byte)Math.Round(a * 255));

    private readonly SKFont _monoFont = new() { Size = 10 };
    private readonly SKFont _serifFont = new() { Size = 10 };

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var canvas = e.Surface.Canvas;

        canvas.Clear(Paper2);

        const float vbX = -40, vbY = 30, vbW = 520, vbH = 480;
        float scale = Math.Min(info.Width / vbW, info.Height / vbH);
        float dx = (info.Width - vbW * scale) / 2f - vbX * scale;
        float dy = (info.Height - vbH * scale) / 2f - vbY * scale;

        canvas.Save();
        canvas.Translate(dx, dy);
        canvas.Scale(scale);

        double tMs = ElapsedMs;

        if (_skipping)
        {
            double fadeMs = ElapsedMs - _skipStartMs;
            const double fadeDur = 400;
            float alpha = (float)Math.Max(0, 1 - fadeMs / fadeDur);
            DrawFrame(canvas, Math.Min(tMs, TotalDurationMs), alpha);
            if (fadeMs >= fadeDur) Finish();
            canvas.Restore();
            return;
        }

        if (tMs >= TotalDurationMs)
        {
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

        Span<double> formOp = stackalloc double[1];
        formOp[0] = 1.0; // always stack form in the brief variant
        double stackOp = formOp[0];

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

        foreach (var id in RenderOrder)
        {
            int selectionIdx = SelectionIndexOf(id);
            bool selected = selectionIdx == phase.SelectedIdx;
            bool anySelected = phase.SelectedIdx != -1;
            DrawLayer(canvas, id, phase, morphT, formOp, selected, anySelected, globalAlpha);
        }

        DrawCaption(canvas, "Building your bundle…", 220, 358,
                    (float)(formOp[0] * globalAlpha));
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

    private void DrawLayer(
        SKCanvas canvas, LayerId id, Phase phase, double morphT,
        ReadOnlySpan<double> formOp, bool selected, bool anySelected,
        float globalAlpha)
    {
        var geo = Geos[0][(int)id];
        float x = (float)geo.X, y = (float)geo.Y, w = (float)geo.W, h = (float)geo.H, rx = (float)geo.Rx;

        double stackOp = formOp[0];
        bool dimmed = anySelected && !selected && stackOp > 0.7;
        float layerAlpha = (dimmed ? 0.4f : 1f) * globalAlpha;
        if (layerAlpha < 0.01f) return;

        bool selectedNow = selected && stackOp > 0.5;
        var fillBase = selectedNow ? InkAlpha(0.10) : InkAlpha(0.04);

        var rect = new SKRect(x, y, x + w, y + h);

        using (var p = new SKPaint { Color = fillBase.WithAlpha((byte)(fillBase.Alpha * layerAlpha)), IsAntialias = true })
        {
            if (rx > 0) canvas.DrawRoundRect(rect, rx, rx, p);
            else canvas.DrawRect(rect, p);
        }

        using (var p = new SKPaint
        {
            Color = Ink.WithAlpha((byte)(255 * layerAlpha)),
            IsStroke = true,
            StrokeWidth = selectedNow ? 1.4f : 0.9f,
            IsAntialias = true,
        })
        {
            if (rx > 0) canvas.DrawRoundRect(rect, rx, rx, p);
            else canvas.DrawRect(rect, p);
        }

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
            using (var p = new SKPaint
            {
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

        if (selected && stackOp > 0.5)
            DrawSelectionBrackets(canvas, x, y, w, h, (float)stackOp * layerAlpha);
    }

    private static void DrawSelectionBrackets(SKCanvas canvas, float x, float y, float w, float h, float alpha)
    {
        const float reach = 6, off = 4;
        using var p = new SKPaint
        {
            Color = Ink.WithAlpha((byte)(255 * alpha)),
            IsStroke = true,
            StrokeWidth = 1.2f,
            IsAntialias = true,
        };
        using var tl = new SKPath();
        tl.MoveTo(x - off, y - off + reach); tl.LineTo(x - off, y - off); tl.LineTo(x - off + reach, y - off);
        canvas.DrawPath(tl, p);

        using var tr = new SKPath();
        tr.MoveTo(x + w + off - reach, y - off); tr.LineTo(x + w + off, y - off); tr.LineTo(x + w + off, y - off + reach);
        canvas.DrawPath(tr, p);

        using var br = new SKPath();
        br.MoveTo(x + w + off, y + h + off - reach); br.LineTo(x + w + off, y + h + off); br.LineTo(x + w + off - reach, y + h + off);
        canvas.DrawPath(br, p);

        using var bl = new SKPath();
        bl.MoveTo(x - off + reach, y + h + off); bl.LineTo(x - off, y + h + off); bl.LineTo(x - off, y + h + off - reach);
        canvas.DrawPath(bl, p);
    }

    private void DrawCaption(SKCanvas canvas, string text, float cx, float cy, float alpha)
    {
        if (alpha < 0.02f) return;
        using var p = new SKPaint { Color = Ink2.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
        _serifFont.Size = 12;
        canvas.DrawText(text, cx, cy, SKTextAlign.Center, _serifFont, p);
    }
}
