using System;
using FibonacciSphere.ViewModels;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace FibonacciSphere.Controls;

public sealed class SphereCanvasElement : SKCanvasElement
{
    private SphereViewModel? _viewModel;

    public SphereViewModel? ViewModel
    {
        get => _viewModel;
        set => _viewModel = value;
    }

    public SphereCanvasElement()
    {
        if (!IsSupportedOnCurrentPlatform())
        {
            throw new PlatformNotSupportedException("SKCanvasElement is only supported on Skia-based platforms.");
        }
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (_viewModel?.Renderer is null)
        {
            return;
        }

        _viewModel.Renderer.Render(canvas, (int)area.Width, (int)area.Height);
    }

    public void RequestInvalidate()
    {
        Invalidate();
    }
}
