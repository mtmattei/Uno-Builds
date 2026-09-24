using System.Security.Cryptography;
using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.Tests;

public class RepositoryTests
{
    [Test]
    public async Task Seeds_all_twelve_assets_and_six_inspections()
    {
        var (repo, _, _) = Fixtures.NewRepository();

        var assets = await repo.GetAssetsAsync();
        var inspections = await repo.GetInspectionsAsync();

        assets.Should().HaveCount(12);
        assets.Select(a => a.Id).Should().Contain(["PMP-104", "CT-007", "EF-090"]);
        assets.Single(a => a.Id == "CT-007").Status.Should().Be(AssetStatus.Critical);
        assets.Single(a => a.Id == "CT-007").LastInspection.Should().Be(new DateOnly(2026, 9, 8));
        inspections.Should().HaveCount(6);
    }

    [Test]
    public async Task Added_inspection_is_persisted_and_survives_a_new_repository_instance()
    {
        var (repo, dataDir, clock) = Fixtures.NewRepository();

        var saved = await repo.AddInspectionAsync(Fixtures.Draft(), null);

        saved.Id.Should().Be("INS-24092");
        saved.Inspector.Should().Be("Alex Morgan");

        // Simulates process restart: a fresh repository over the same directory.
        var reloaded = new JsonFieldCheckRepository(dataDir, new FileSeedSource(Fixtures.MockDataDir), clock);
        var inspections = await reloaded.GetInspectionsAsync();
        inspections.Should().ContainSingle(i => i.Id == saved.Id)
            .Which.IssueDescription.Should().Be("Basin-level alarm intermittent");
        inspections.Should().HaveCount(7);

        var asset = (await reloaded.GetAssetsAsync()).Single(a => a.Id == "CT-007");
        asset.Status.Should().Be(AssetStatus.Attention);
        asset.LastInspection.Should().Be(new DateOnly(2026, 9, 23));
    }

    [Test]
    public async Task Generated_ids_are_unique_and_stable()
    {
        var (repo, _, _) = Fixtures.NewRepository();

        var first = await repo.AddInspectionAsync(Fixtures.Draft(), null);
        var second = await repo.AddInspectionAsync(Fixtures.Draft("PMP-104", InspectionCondition.Good), null);

        second.Id.Should().NotBe(first.Id);
        var ids = (await repo.GetInspectionsAsync()).Select(i => i.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().Contain([first.Id, second.Id]);
    }

    [Test]
    public void Next_id_follows_the_highest_existing_number()
    {
        JsonFieldCheckRepository.NextInspectionId([]).Should().Be("INS-1");
        var existing = new[] { "INS-10", "INS-24091", "X-5" }
            .Select(id => new Inspection(id, "A", "A", DateTime.Now, InspectionCondition.Good, true, 1, "x", null));
        JsonFieldCheckRepository.NextInspectionId(existing).Should().Be("INS-24092");
    }

    [Test]
    public async Task Saving_never_modifies_the_benchmark_fixture_files()
    {
        var before = Hash(Path.Combine(Fixtures.MockDataDir, "assets.json")) + Hash(Path.Combine(Fixtures.MockDataDir, "inspections.json"));
        var (repo, _, _) = Fixtures.NewRepository();

        await repo.AddInspectionAsync(Fixtures.Draft(), null);

        var after = Hash(Path.Combine(Fixtures.MockDataDir, "assets.json")) + Hash(Path.Combine(Fixtures.MockDataDir, "inspections.json"));
        after.Should().Be(before);
    }

    [Test]
    public async Task Attachment_is_copied_into_app_storage()
    {
        var (repo, dataDir, _) = Fixtures.NewRepository();
        var photo = Path.Combine(Fixtures.MockDataDir, "inspection-photo.png");

        var saved = await repo.AddInspectionAsync(Fixtures.Draft(), new PickedFile("inspection-photo.png", () => Task.FromResult<Stream>(File.OpenRead(photo))));

        saved.AttachmentFileName.Should().Be("inspection-photo.png");
        File.ReadAllBytes(Path.Combine(dataDir, "attachments", saved.Id + ".png")).Should().Equal(File.ReadAllBytes(photo));
    }

    [Test]
    public async Task Write_failure_throws_and_leaves_state_unchanged()
    {
        var (repo, dataDir, _) = Fixtures.NewRepository();
        await repo.GetInspectionsAsync();

        // Make the inspections file unwritable by replacing its temp path with a directory.
        Directory.CreateDirectory(Path.Combine(dataDir, "inspections.json.tmp"));

        var act = () => repo.AddInspectionAsync(Fixtures.Draft(), null);

        await act.Should().ThrowAsync<RepositoryException>();
        (await repo.GetInspectionsAsync()).Should().HaveCount(6);
    }

    [Test]
    public async Task Corrupt_local_data_surfaces_a_repository_exception()
    {
        var (repo, dataDir, _) = Fixtures.NewRepository();
        Directory.CreateDirectory(dataDir);
        await File.WriteAllTextAsync(Path.Combine(dataDir, "assets.json"), "{ not json");

        var act = () => repo.GetAssetsAsync();

        await act.Should().ThrowAsync<RepositoryException>();
    }

    [Test]
    public async Task Data_modes_are_deterministic()
    {
        var (repo, _, _) = Fixtures.NewRepository();

        (await new DataModeRepository(repo, DataMode.Empty).GetAssetsAsync()).Should().BeEmpty();

        await new DataModeRepository(repo, DataMode.Error).Invoking(r => r.GetAssetsAsync()).Should().ThrowAsync<RepositoryException>();

        var once = new DataModeRepository(repo, DataMode.ErrorOnce);
        await once.Invoking(r => r.GetAssetsAsync()).Should().ThrowAsync<RepositoryException>();
        (await once.GetAssetsAsync()).Should().HaveCount(12);

        await new DataModeRepository(repo, DataMode.SaveError).Invoking(r => r.AddInspectionAsync(Fixtures.Draft(), null)).Should().ThrowAsync<RepositoryException>();
        (await repo.GetInspectionsAsync()).Should().HaveCount(6);
    }

    [TestCase("error", DataMode.Error)]
    [TestCase("ERROR-ONCE", DataMode.ErrorOnce)]
    [TestCase("empty", DataMode.Empty)]
    [TestCase("slow", DataMode.Slow)]
    [TestCase("save-error", DataMode.SaveError)]
    [TestCase(null, DataMode.Normal)]
    [TestCase("bogus", DataMode.Normal)]
    public void Data_mode_parsing(string? value, DataMode expected) => DataModeParser.Parse(value).Should().Be(expected);

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
