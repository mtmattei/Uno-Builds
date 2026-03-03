using System;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace LiquidMorph.Controls.LiquidMorph;

/// <summary>
/// Hardware-accelerated SKCanvasElement that renders the liquid morph
/// displacement + blur effect each frame during a transition.
/// Both SetSourceBitmap and RenderOverride run on the UI thread,
/// so no lock is needed.
/// </summary>
public sealed class MorphCanvas : SKCanvasElement
{
    private readonly MorphEffectRenderer _renderer = new();
    private SKBitmap? _sourceBitmap;
    public bool IsActive { get; set; }

    public int EdgePadding
    {
        get => _renderer.EdgePadding;
        set => _renderer.EdgePadding = value;
    }

    public float DisplacementAmount
    {
        get => _renderer.DisplacementAmount;
        set => _renderer.DisplacementAmount = value;
    }

    public float BlurAmount
    {
        get => _renderer.BlurAmount;
        set => _renderer.BlurAmount = value;
    }

    public float AnimationProgress
    {
        get => _renderer.AnimationProgress;
        set => _renderer.AnimationProgress = value;
    }

    public float ContentOpacity
    {
        get => _renderer.ContentOpacity;
        set => _renderer.ContentOpacity = value;
    }

    public float ScaleAmount { get; set; } = 1f;

    public void SetSourceBitmap(SKBitmap? bitmap)
    {
        _sourceBitmap = bitmap;
    }

    public void Reseed() => _renderer.Reseed();

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsActive || _sourceBitmap is null) return;

        int w = (int)area.Width;
        int h = (int)area.Height;
        if (w <= 0 || h <= 0) return;

        // Apply scale breathing around center
        if (Math.Abs(ScaleAmount - 1f) > 0.001f)
        {
            canvas.Save();
            canvas.Scale(ScaleAmount, ScaleAmount, w / 2f, h / 2f);
        }

        _renderer.SetSize(w, h);
        _renderer.Render(canvas, _sourceBitmap);

        if (Math.Abs(ScaleAmount - 1f) > 0.001f)
        {
            canvas.Restore();
        }
    }

    public void RequestRedraw()
    {
        Invalidate();
    }

    public void Cleanup()
    {
        IsActive = false;
        _sourceBitmap?.Dispose();
        _sourceBitmap = null;
        _renderer.Dispose();
    }
}
