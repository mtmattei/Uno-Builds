namespace FreewriteUno.InlineAi.Services;

/// <summary>
/// Testable seam over the platform clipboard for the Copy action. WASM clipboard writes are async
/// and gesture-gated, so callers await this rather than assuming a synchronous set.
/// </summary>
public interface IClipboardService
{
    Task SetTextAsync(string text);
}
