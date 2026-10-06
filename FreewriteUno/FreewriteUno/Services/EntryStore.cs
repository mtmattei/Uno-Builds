using System.Globalization;
using System.Text.RegularExpressions;
using FreewriteUno.Models;

namespace FreewriteUno.Services;

public sealed partial class EntryStore : IEntryStore
{
    private const string FolderName = "Freewrite";
    private const string TimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const string FileNamePattern = @"^\[(?<id>[0-9a-fA-F-]{36})\]-\[(?<ts>\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})\]\.md$";

    private readonly string _root;

    public EntryStore(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    public string RootPath => _root;

    [GeneratedRegex(FileNamePattern)]
    private static partial Regex FileNameRegex();

    public async Task<IReadOnlyList<Entry>> LoadAllAsync(CancellationToken ct = default)
    {
        var entries = new List<Entry>();
        if (!Directory.Exists(_root)) return entries;

        foreach (var path in Directory.EnumerateFiles(_root, "*.md"))
        {
            ct.ThrowIfCancellationRequested();
            var name = Path.GetFileName(path);
            var match = FileNameRegex().Match(name);
            if (!match.Success) continue;
            if (!Guid.TryParse(match.Groups["id"].Value, out var id)) continue;
            if (!DateTimeOffset.TryParseExact(
                    match.Groups["ts"].Value,
                    TimestampFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var createdAt))
                continue;

            string preview;
            try
            {
                var content = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
                preview = BuildPreview(content);
            }
            catch
            {
                preview = string.Empty;
            }

            entries.Add(new Entry(id, createdAt, name, preview));
        }

        entries.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
        return entries;
    }

    public async Task<Entry> CreateAsync(CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.Now;
        var name = $"[{id}]-[{created.ToString(TimestampFormat, CultureInfo.InvariantCulture)}].md";
        var path = Path.Combine(_root, name);
        await File.WriteAllTextAsync(path, "\n\n", ct).ConfigureAwait(false);
        return new Entry(id, created, name, string.Empty);
    }

    public async Task<string> ReadAsync(Entry entry, CancellationToken ct = default)
    {
        var path = Path.Combine(_root, entry.FileName);
        if (!File.Exists(path)) return "\n\n";
        return await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
    }

    public Task WriteAsync(Entry entry, string content, CancellationToken ct = default)
    {
        var path = Path.Combine(_root, entry.FileName);
        return File.WriteAllTextAsync(path, content, ct);
    }

    public Task DeleteAsync(Entry entry, CancellationToken ct = default)
    {
        var path = Path.Combine(_root, entry.FileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    internal const int PreviewMaxChars = 30;

    internal static string BuildPreview(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        // Single-pass: skip leading whitespace, copy up to PreviewMaxChars (newlines → spaces),
        // trim trailing whitespace via lastNonWs, and append ellipsis if more non-ws follows.
        int i = 0;
        while (i < content.Length && char.IsWhiteSpace(content[i])) i++;
        if (i == content.Length) return string.Empty;

        Span<char> buf = stackalloc char[PreviewMaxChars];
        int written = 0;
        int lastNonWs = -1;
        for (; i < content.Length && written < PreviewMaxChars; i++)
        {
            char c = content[i];
            if (c == '\n' || c == '\r') c = ' ';
            buf[written] = c;
            if (!char.IsWhiteSpace(c)) lastNonWs = written;
            written++;
        }

        bool hasMore = false;
        for (; i < content.Length; i++)
        {
            char c = content[i];
            if (c != '\n' && c != '\r' && !char.IsWhiteSpace(c)) { hasMore = true; break; }
        }

        if (lastNonWs < 0) return string.Empty;
        var slice = buf[..(lastNonWs + 1)];
        return hasMore ? new string(slice) + "…" : new string(slice);
    }
}
