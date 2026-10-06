using FreewriteUno.Models;

namespace FreewriteUno.Services;

public interface IEntryStore
{
    Task<IReadOnlyList<Entry>> LoadAllAsync(CancellationToken ct = default);
    Task<Entry> CreateAsync(CancellationToken ct = default);
    Task<string> ReadAsync(Entry entry, CancellationToken ct = default);
    Task WriteAsync(Entry entry, string content, CancellationToken ct = default);
    Task DeleteAsync(Entry entry, CancellationToken ct = default);
    string RootPath { get; }
}
