using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.Tests;

internal sealed class FileSeedSource(string mockDataDir) : ISeedSource
{
    public string ReadAssetsJson() => File.ReadAllText(Path.Combine(mockDataDir, "assets.json"));

    public string ReadInspectionsJson() => File.ReadAllText(Path.Combine(mockDataDir, "inspections.json"));
}

internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

internal sealed class RecordingNavigator : INavigator
{
    public List<string> Calls { get; } = [];

    public void ShowSection(AppSection section) => Calls.Add($"section:{section}");

    public void ShowAsset(string assetId) => Calls.Add($"asset:{assetId}");

    public void StartInspection(string assetId) => Calls.Add($"start:{assetId}");

    public void ShowInspectionSuccess(string inspectionId) => Calls.Add($"success:{inspectionId}");

    public void ReturnToAsset(string assetId) => Calls.Add($"return:{assetId}");

    public void GoBack() => Calls.Add("back");
}

internal sealed class StubPicker : IFilePickerService
{
    public Func<Task<PickedFile?>> Next { get; set; } = () => Task.FromResult<PickedFile?>(null);

    public Task<PickedFile?> PickFileAsync() => Next();
}

internal static class Fixtures
{
    /// <summary>The frozen benchmark fixtures (FieldCheck/mock-data), located relative to the test assembly.</summary>
    public static string MockDataDir
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "mock-data")))
            {
                dir = dir.Parent;
            }

            return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("mock-data not found"), "mock-data");
        }
    }

    public static readonly DateTimeOffset Now = new(2026, 9, 23, 14, 41, 0, TimeSpan.Zero);

    public static (JsonFieldCheckRepository Repo, string DataDir, FixedClock Clock) NewRepository()
    {
        var dataDir = Path.Combine(Path.GetTempPath(), "fieldcheck-tests", Guid.NewGuid().ToString("N"));
        var clock = new FixedClock(Now);
        return (new JsonFieldCheckRepository(dataDir, new FileSeedSource(MockDataDir), clock), dataDir, clock);
    }

    public static InspectionDraft Draft(string assetId = "CT-007", InspectionCondition condition = InspectionCondition.Attention) =>
        new(assetId, condition, true, 27, "note", condition == InspectionCondition.Good ? null : "Basin-level alarm intermittent");
}
