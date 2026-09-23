using FieldCheck.Models;

namespace FieldCheck.Services;

/// <summary>
/// Deterministic repository behaviours used for state verification. Selected only at launch
/// (environment variable, command-line argument or Android intent extra); never exposed in the UI.
/// </summary>
public enum DataMode
{
    Normal,
    Slow,
    Empty,
    Error,
    ErrorOnce,
    SaveError,
}

public static class DataModeParser
{
    public const string EnvironmentVariable = "FIELDCHECK_DATA_MODE";
    public const string ArgumentPrefix = "--data-mode=";

    public static DataMode Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "slow" => DataMode.Slow,
        "empty" => DataMode.Empty,
        "error" => DataMode.Error,
        "error-once" => DataMode.ErrorOnce,
        "save-error" => DataMode.SaveError,
        _ => DataMode.Normal,
    };

    public static DataMode FromEnvironment(string? platformValue = null)
    {
        if (!string.IsNullOrWhiteSpace(platformValue))
        {
            return Parse(platformValue);
        }

        var arg = Environment.GetCommandLineArgs()
            .FirstOrDefault(a => a.StartsWith(ArgumentPrefix, StringComparison.OrdinalIgnoreCase));
        return Parse(arg?[ArgumentPrefix.Length..] ?? Environment.GetEnvironmentVariable(EnvironmentVariable));
    }
}

/// <summary>Wraps the real repository and applies a <see cref="DataMode"/>.</summary>
public sealed class DataModeRepository(IFieldCheckRepository inner, DataMode mode) : IFieldCheckRepository
{
    public static readonly TimeSpan SlowDelay = TimeSpan.FromSeconds(4);

    private bool _assetsFailed;
    private bool _inspectionsFailed;

    public event EventHandler? DataChanged
    {
        add => inner.DataChanged += value;
        remove => inner.DataChanged -= value;
    }

    public DataMode Mode => mode;

    public async Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken ct = default)
    {
        await BeforeReadAsync(ref _assetsFailed, ct);
        return mode == DataMode.Empty ? [] : await inner.GetAssetsAsync(ct);
    }

    public async Task<IReadOnlyList<Inspection>> GetInspectionsAsync(CancellationToken ct = default)
    {
        await BeforeReadAsync(ref _inspectionsFailed, ct);
        return mode == DataMode.Empty ? [] : await inner.GetInspectionsAsync(ct);
    }

    public async Task<Inspection> AddInspectionAsync(InspectionDraft draft, PickedFile? attachment, CancellationToken ct = default)
    {
        if (mode == DataMode.Slow)
        {
            await Task.Delay(SlowDelay, ct);
        }

        if (mode == DataMode.SaveError)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400), ct);
            throw new RepositoryException("Simulated local storage write failure.");
        }

        return await inner.AddInspectionAsync(draft, attachment, ct);
    }

    private Task BeforeReadAsync(ref bool alreadyFailed, CancellationToken ct)
    {
        switch (mode)
        {
            case DataMode.Error:
                return FailAsync(ct);
            case DataMode.ErrorOnce when !alreadyFailed:
                alreadyFailed = true;
                return FailAsync(ct);
            case DataMode.Slow:
                return Task.Delay(SlowDelay, ct);
            default:
                return Task.CompletedTask;
        }
    }

    private static async Task FailAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), ct);
        throw new RepositoryException("Simulated local storage read failure.");
    }
}
