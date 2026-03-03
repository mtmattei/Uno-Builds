using SkiaSharp;
using UnoVox.Models;

namespace UnoVox.Services;

public interface IHandTracker
{
    Task<bool> InitializeAsync();
    Task<IReadOnlyList<HandDetection>> DetectAsync(SKBitmap frame, CancellationToken ct = default);
}
