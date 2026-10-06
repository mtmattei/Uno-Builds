using System.Globalization;
using FreewriteUno.Models;
using FreewriteUno.Services;

namespace FreewriteUno.Tests;

public sealed class EntryStoreTests : IDisposable
{
    private readonly string _root;
    private readonly EntryStore _store;

    public EntryStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "FreewriteUnoTests", Guid.NewGuid().ToString("N"));
        _store = new EntryStore(_root);
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
        catch { /* best effort */ }
    }

    [Fact]
    public async Task LoadAllAsync_EmptyDirectory_ReturnsEmpty()
    {
        var entries = await _store.LoadAllAsync();
        Assert.Empty(entries);
    }

    [Fact]
    public async Task CreateAsync_WritesFileWithExpectedNameFormat()
    {
        var entry = await _store.CreateAsync();
        var path = Path.Combine(_root, entry.FileName);

        Assert.True(File.Exists(path));
        Assert.Matches(
            @"^\[[0-9a-fA-F-]{36}\]-\[\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\]\.md$",
            entry.FileName);
        Assert.Equal("\n\n", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task LoadAllAsync_ReturnsEntriesNewestFirst()
    {
        var older = await _store.CreateAsync();
        await Task.Delay(1100); // timestamp granularity is seconds
        var newer = await _store.CreateAsync();

        var entries = await _store.LoadAllAsync();

        Assert.Equal(2, entries.Count);
        Assert.Equal(newer.Id, entries[0].Id);
        Assert.Equal(older.Id, entries[1].Id);
    }

    [Fact]
    public async Task LoadAllAsync_IgnoresFilesThatDontMatchPattern()
    {
        var entry = await _store.CreateAsync();
        await File.WriteAllTextAsync(Path.Combine(_root, "notes.md"), "junk");
        await File.WriteAllTextAsync(Path.Combine(_root, "README.md"), "junk");

        var entries = await _store.LoadAllAsync();
        Assert.Single(entries);
        Assert.Equal(entry.Id, entries[0].Id);
    }

    [Fact]
    public async Task LoadAllAsync_HandlesUnreadableFile()
    {
        // Create a filename that matches the regex but write 0 bytes to it.
        var id = Guid.NewGuid();
        var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        var name = $"[{id}]-[{ts}].md";
        await File.WriteAllBytesAsync(Path.Combine(_root, name), Array.Empty<byte>());

        var entries = await _store.LoadAllAsync();
        Assert.Single(entries);
        Assert.Equal(string.Empty, entries[0].Preview);
    }

    [Fact]
    public async Task WriteAsync_OverwritesExistingFile()
    {
        var entry = await _store.CreateAsync();
        await _store.WriteAsync(entry, "\n\nhello");
        await _store.WriteAsync(entry, "\n\nreplaced");

        var actual = await File.ReadAllTextAsync(Path.Combine(_root, entry.FileName));
        Assert.Equal("\n\nreplaced", actual);
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var entry = await _store.CreateAsync();
        Assert.True(File.Exists(Path.Combine(_root, entry.FileName)));

        await _store.DeleteAsync(entry);

        Assert.False(File.Exists(Path.Combine(_root, entry.FileName)));
        var entries = await _store.LoadAllAsync();
        Assert.Empty(entries);
    }

    [Fact]
    public async Task DeleteAsync_NonexistentFile_Succeeds()
    {
        var phantom = new Entry(
            Guid.NewGuid(),
            DateTimeOffset.Now,
            $"[{Guid.NewGuid()}]-[2026-05-20_10-00-00].md",
            string.Empty);

        await _store.DeleteAsync(phantom);
        // no assertion — the test passes if no exception is thrown
    }

    [Fact]
    public async Task PreviewText_Is30CharsOrEllipsis()
    {
        var shortEntry = await _store.CreateAsync();
        await _store.WriteAsync(shortEntry, "\n\nshort body");

        var longEntry = await _store.CreateAsync();
        var longBody = new string('a', 100);
        await _store.WriteAsync(longEntry, "\n\n" + longBody);

        var entries = await _store.LoadAllAsync();
        var loadedShort = entries.Single(e => e.Id == shortEntry.Id);
        var loadedLong = entries.Single(e => e.Id == longEntry.Id);

        Assert.Equal("short body", loadedShort.Preview);
        Assert.Equal(31, loadedLong.Preview.Length); // 30 chars + ellipsis (1 char)
        Assert.EndsWith("…", loadedLong.Preview);
    }
}
