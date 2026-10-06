using Windows.ApplicationModel.DataTransfer;

namespace FreewriteUno.InlineAi.Services;

/// <summary>
/// Default clipboard implementation backed by <see cref="Clipboard"/>. The set itself is synchronous
/// on most targets; the async signature keeps the call site uniform with gesture-gated WASM writes.
/// </summary>
public sealed class ClipboardService : IClipboardService
{
    public Task SetTextAsync(string text)
    {
        var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
        package.SetText(text ?? string.Empty);
        Clipboard.SetContent(package);
        return Task.CompletedTask;
    }
}
