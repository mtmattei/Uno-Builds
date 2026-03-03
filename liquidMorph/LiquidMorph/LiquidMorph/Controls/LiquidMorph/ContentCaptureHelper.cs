using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LiquidMorph.Controls.LiquidMorph;

/// <summary>
/// Captures a XAML UIElement subtree into an SKBitmap for use
/// as a texture source in the SkiaSharp effect pipeline.
/// Avoids intermediate byte[] allocation by copying directly
/// from the IBuffer stream into the SKBitmap's native pixel memory.
/// </summary>
public static class ContentCaptureHelper
{
    // Reuse RenderTargetBitmap across captures to reduce WinRT object churn
    [ThreadStatic]
    private static RenderTargetBitmap? _rtb;

    public static async Task<SKBitmap?> CaptureAsync(UIElement element)
    {
        if (element.ActualSize.X <= 0 || element.ActualSize.Y <= 0)
            return null;

        _rtb ??= new RenderTargetBitmap();
        await _rtb.RenderAsync(element);

        if (_rtb.PixelWidth == 0 || _rtb.PixelHeight == 0)
            return null;

        var buffer = await _rtb.GetPixelsAsync();

        var info = new SKImageInfo(
            _rtb.PixelWidth,
            _rtb.PixelHeight,
            SKColorType.Bgra8888,
            SKAlphaType.Premul);

        var bitmap = new SKBitmap(info);

        // Copy directly from IBuffer stream into SKBitmap native memory.
        // Avoids buffer.ToArray() which allocates a large byte[] on the LOH.
        int byteCount = _rtb.PixelWidth * _rtb.PixelHeight * 4;
        using var stream = buffer.AsStream();
        unsafe
        {
            var destSpan = new Span<byte>((void*)bitmap.GetPixels(), byteCount);
            int totalRead = 0;
            while (totalRead < byteCount)
            {
                int read = stream.Read(destSpan.Slice(totalRead));
                if (read == 0) break;
                totalRead += read;
            }
        }

        bitmap.NotifyPixelsChanged();
        return bitmap;
    }
}
