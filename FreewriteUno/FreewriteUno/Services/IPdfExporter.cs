namespace FreewriteUno.Services;

public interface IPdfExporter
{
    Task<byte[]> RenderAsync(string content, string title, CancellationToken ct = default);
}
