using System.Globalization;
using System.Text.Json;
using FieldCheck.Models;

namespace FieldCheck.Services;

/// <summary>
/// Local JSON persistence. Seeds from the embedded fixtures on first run, then reads and
/// writes copies in the app's private data directory. The fixtures are never modified.
/// </summary>
public sealed class JsonFieldCheckRepository : IFieldCheckRepository
{
    public const string InspectorName = "Alex Morgan";

    private const string AssetsFile = "assets.json";
    private const string InspectionsFile = "inspections.json";
    private const string AttachmentsFolder = "attachments";

    private readonly string _dataDirectory;
    private readonly ISeedSource _seed;
    private readonly TimeProvider _clock;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private List<Asset>? _assets;
    private List<Inspection>? _inspections;

    public JsonFieldCheckRepository(string dataDirectory, ISeedSource seed, TimeProvider clock)
    {
        _dataDirectory = dataDirectory;
        _seed = seed;
        _clock = clock;
    }

    public event EventHandler? DataChanged;

    public async Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _assets!.ToList();
    }

    public async Task<IReadOnlyList<Inspection>> GetInspectionsAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _inspections!.ToList();
    }

    public async Task<Inspection> AddInspectionAsync(InspectionDraft draft, PickedFile? attachment, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        await _gate.WaitAsync(ct);
        Inspection inspection;
        try
        {
            var asset = _assets!.FirstOrDefault(a => a.Id == draft.AssetId)
                ?? throw new RepositoryException($"Asset '{draft.AssetId}' was not found.");

            var id = NextInspectionId(_inspections!);
            var now = _clock.GetLocalNow().DateTime;
            var attachmentName = attachment is null ? null : await StoreAttachmentAsync(id, attachment, ct);

            inspection = new Inspection(
                id,
                asset.Id,
                asset.Name,
                new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second),
                draft.Condition,
                draft.OperatingNormally,
                draft.TemperatureC,
                InspectorName,
                string.IsNullOrWhiteSpace(draft.Notes) ? null : draft.Notes.Trim(),
                string.IsNullOrWhiteSpace(draft.IssueDescription) ? null : draft.IssueDescription.Trim(),
                attachmentName);

            var updatedAsset = asset with
            {
                Status = ToAssetStatus(draft.Condition),
                LastInspection = DateOnly.FromDateTime(now),
            };

            var inspections = _inspections!.Prepend(inspection).ToList();
            var assets = _assets!.Select(a => a.Id == asset.Id ? updatedAsset : a).ToList();

            try
            {
                await WriteAsync(InspectionsFile, JsonSerializer.Serialize(inspections, FieldCheckJsonContext.Default.ListInspection), ct);
                await WriteAsync(AssetsFile, JsonSerializer.Serialize(assets, FieldCheckJsonContext.Default.ListAsset), ct);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new RepositoryException("The inspection could not be saved to local storage.", ex);
            }

            // Only commit in-memory state once both files are on disk.
            _inspections = inspections;
            _assets = assets;
        }
        finally
        {
            _gate.Release();
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
        return inspection;
    }

    public static AssetStatus ToAssetStatus(InspectionCondition condition) => condition switch
    {
        InspectionCondition.Good => AssetStatus.Operational,
        InspectionCondition.Attention => AssetStatus.Attention,
        _ => AssetStatus.Critical,
    };

    public static string NextInspectionId(IEnumerable<Inspection> existing)
    {
        var max = existing
            .Select(i => i.Id.StartsWith("INS-", StringComparison.Ordinal)
                && int.TryParse(i.Id.AsSpan(4), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"INS-{max + 1}";
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_assets is not null && _inspections is not null)
        {
            return;
        }

        await _gate.WaitAsync(ct);
        try
        {
            if (_assets is not null && _inspections is not null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(_dataDirectory);
                var assetsJson = await ReadOrSeedAsync(AssetsFile, _seed.ReadAssetsJson, ct);
                var inspectionsJson = await ReadOrSeedAsync(InspectionsFile, _seed.ReadInspectionsJson, ct);

                _assets = JsonSerializer.Deserialize(assetsJson, FieldCheckJsonContext.Default.ListAsset) ?? [];
                _inspections = JsonSerializer.Deserialize(inspectionsJson, FieldCheckJsonContext.Default.ListInspection) ?? [];
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                _assets = null;
                _inspections = null;
                throw new RepositoryException("Local data could not be read.", ex);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<string> ReadOrSeedAsync(string fileName, Func<string> seed, CancellationToken ct)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        if (File.Exists(path))
        {
            return await File.ReadAllTextAsync(path, ct);
        }

        var json = seed();
        await WriteAsync(fileName, json, ct);
        return json;
    }

    private async Task WriteAsync(string fileName, string contents, CancellationToken ct)
    {
        Directory.CreateDirectory(_dataDirectory);
        var path = Path.Combine(_dataDirectory, fileName);
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, contents, ct);
        File.Move(temp, path, overwrite: true);
    }

    private async Task<string> StoreAttachmentAsync(string inspectionId, PickedFile file, CancellationToken ct)
    {
        try
        {
            var folder = Path.Combine(_dataDirectory, AttachmentsFolder);
            Directory.CreateDirectory(folder);
            var target = Path.Combine(folder, inspectionId + Path.GetExtension(file.FileName));
            await using var source = await file.OpenReadAsync();
            await using var destination = File.Create(target);
            await source.CopyToAsync(destination, ct);
            return file.FileName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new RepositoryException("The attachment could not be copied to local storage.", ex);
        }
    }
}
