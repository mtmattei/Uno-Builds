using FieldCheck.Models;
using FieldCheck.Services;
using FieldCheck.ViewModels;

namespace FieldCheck.Tests;

public class ViewModelTests
{
    [Test]
    public async Task Dashboard_counts_are_derived_from_data()
    {
        var (repo, _, clock) = Fixtures.NewRepository();
        var vm = new DashboardViewModel(repo, new RecordingNavigator(), clock);

        await vm.LoadAsync();

        vm.State.Should().Be(LoadState.Ready);
        (vm.TotalCount, vm.OperationalCount, vm.AttentionCount, vm.CriticalCount).Should().Be((12, 7, 3, 2));
        vm.NeedsAttention.Select(a => a.Id).Should().Equal("CT-007", "EF-090", "AHU-203", "CNV-018", "BLR-002");

        await repo.AddInspectionAsync(Fixtures.Draft("PMP-104", InspectionCondition.Critical), null);
        await vm.LoadAsync();
        (vm.OperationalCount, vm.CriticalCount).Should().Be((6, 3));
    }

    [Test]
    public async Task Dashboard_reports_empty_and_error_states()
    {
        var (repo, _, clock) = Fixtures.NewRepository();

        var empty = new DashboardViewModel(new DataModeRepository(repo, DataMode.Empty), new RecordingNavigator(), clock);
        await empty.LoadAsync();
        empty.State.Should().Be(LoadState.Empty);

        var flaky = new DashboardViewModel(new DataModeRepository(repo, DataMode.ErrorOnce), new RecordingNavigator(), clock);
        await flaky.LoadAsync();
        flaky.State.Should().Be(LoadState.Error);
        await flaky.LoadCommand.ExecuteAsync(null); // Retry
        flaky.State.Should().Be(LoadState.Ready);
    }

    [TestCase("cooling", AssetFilter.All, new[] { "CT-007" })]
    [TestCase("PMP", AssetFilter.All, new[] { "PMP-104", "PMP-220" })]
    [TestCase("pump", AssetFilter.All, new[] { "PMP-104", "PMP-220" })] // type + name
    [TestCase("ROOF", AssetFilter.All, new[] { "AHU-203", "CT-007", "FAN-305" })] // location, case-insensitive
    [TestCase("", AssetFilter.Critical, new[] { "CT-007", "EF-090" })]
    [TestCase("", AssetFilter.Attention, new[] { "AHU-203", "CNV-018", "BLR-002" })]
    [TestCase("roof", AssetFilter.Critical, new[] { "CT-007" })] // combined
    [TestCase("roof", AssetFilter.Operational, new[] { "FAN-305" })]
    public async Task Assets_search_and_filter(string search, AssetFilter filter, string[] expected)
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var vm = new AssetsViewModel(repo, new RecordingNavigator(), new AssetDetailViewModel(repo, new RecordingNavigator()));
        await vm.LoadAsync();

        vm.SearchText = search;
        vm.Filter = filter;

        vm.Items.Select(r => r.Asset.Id).Should().Equal(expected);
        vm.HasNoResults.Should().BeFalse();
    }

    [Test]
    public async Task Assets_no_results_is_distinct_from_empty()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var vm = new AssetsViewModel(repo, new RecordingNavigator(), new AssetDetailViewModel(repo, new RecordingNavigator()));
        await vm.LoadAsync();

        vm.SearchText = "zzz-nothing";

        vm.Items.Should().BeEmpty();
        vm.HasNoResults.Should().BeTrue();
        vm.State.Should().Be(LoadState.Ready);

        vm.ClearSearchCommand.Execute(null);
        vm.Items.Should().HaveCount(12);
    }

    [Test]
    public async Task Assets_open_pushes_detail_when_narrow_and_selects_when_wide()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var nav = new RecordingNavigator();
        var vm = new AssetsViewModel(repo, nav, new AssetDetailViewModel(repo, nav));
        await vm.LoadAsync();
        var ct = vm.Items.Single(r => r.Asset.Id == "CT-007").Asset;

        vm.OpenAsset(ct);
        nav.Calls.Should().Equal("asset:CT-007");

        vm.IsWide = true;
        vm.OpenAsset(ct);
        vm.SelectedAsset.Should().Be(ct);
        vm.Items.Single(r => r.IsSelected).Asset.Id.Should().Be("CT-007");
        nav.Calls.Should().HaveCount(1);
    }

    [Test]
    public async Task History_is_newest_first_and_filters()
    {
        var (repo, _, clock) = Fixtures.NewRepository();
        var vm = new HistoryViewModel(repo, clock);
        await vm.LoadAsync();

        vm.Items.Select(r => r.Inspection.Id).Should().Equal("INS-24091", "INS-24044", "INS-24086", "INS-24065", "INS-24058", "INS-24072");
        vm.Items[0].IdAndTime.Should().Be("INS-24091 · Sep 18, 9:24 AM");

        vm.Filter = ConditionFilter.Good;
        vm.Items.Select(r => r.Inspection.Id).Should().Equal("INS-24091", "INS-24044");

        vm.Filter = ConditionFilter.All;
        vm.SearchText = "ahu-203";
        vm.Items.Select(r => r.AssetName).Should().Equal("Air Handler 203");

        vm.SearchText = "boiler";
        vm.Items.Select(r => r.Inspection.AssetId).Should().Equal("BLR-002");

        vm.SearchText = "boiler";
        vm.Filter = ConditionFilter.Critical;
        vm.HasNoResults.Should().BeTrue();
    }

    [Test]
    public async Task New_inspection_appears_first_in_history_immediately()
    {
        var (repo, _, clock) = Fixtures.NewRepository();
        var vm = new HistoryViewModel(repo, clock);
        await vm.LoadAsync();

        await repo.AddInspectionAsync(Fixtures.Draft(), null);
        await Task.Delay(50); // DataChanged triggers a reload

        vm.Items.Should().HaveCount(7);
        vm.Items[0].Inspection.Id.Should().Be("INS-24092");
        vm.Items[0].IdAndTime.Should().Be("INS-24092 · Today, 2:41 PM");
        vm.Subtitle.Should().Be("7 completed inspections");
    }

    private static async Task<(NewInspectionViewModel Vm, RecordingNavigator Nav, IFieldCheckRepository Repo, StubPicker Picker)> NewForm(IFieldCheckRepository? repository = null)
    {
        var repo = repository ?? Fixtures.NewRepository().Repo;
        var nav = new RecordingNavigator();
        var picker = new StubPicker();
        var vm = new NewInspectionViewModel(repo, nav, picker);
        await vm.LoadAsync("CT-007");
        return (vm, nav, repo, picker);
    }

    private static void FillValid(NewInspectionViewModel vm)
    {
        vm.IsGood = true;
        vm.OperatingNormally = true;
        vm.TemperatureText = "27";
        vm.GuardsSecure = vm.NoLeaksOrDamage = vm.AreaClear = true;
    }

    [Test]
    public async Task Submit_is_disabled_until_required_fields_are_valid()
    {
        var (vm, _, _, _) = await NewForm();

        vm.AssetLine.Should().Be("Cooling Tower 07 · CT-007");
        vm.SubmitCommand.CanExecute(null).Should().BeFalse();
        vm.MissingSummary.Should().Be("To submit: select a condition · enter a temperature · confirm 3 checklist items.");

        FillValid(vm);
        vm.SubmitCommand.CanExecute(null).Should().BeTrue();
        vm.MissingSummary.Should().BeEmpty();

        vm.AreaClear = false; // each checklist item is individually required
        vm.SubmitCommand.CanExecute(null).Should().BeFalse();
        vm.MissingSummary.Should().Contain("confirm 1 checklist item");
    }

    [TestCase("-50", true)]
    [TestCase("250", true)]
    [TestCase("27.5", true)]
    [TestCase("-51", false)]
    [TestCase("250.1", false)]
    [TestCase("abc", false)]
    [TestCase("", false)]
    public async Task Temperature_range_is_inclusive(string text, bool valid)
    {
        var (vm, _, _, _) = await NewForm();
        FillValid(vm);

        vm.TemperatureText = text;

        vm.IsValid.Should().Be(valid);
        if (!valid && text.Length > 0)
        {
            vm.HasTemperatureError.Should().BeTrue();
            vm.TemperatureError.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task Issue_description_visibility_and_requirement()
    {
        var (vm, _, _, _) = await NewForm();
        FillValid(vm);

        vm.IsIssueDescriptionVisible.Should().BeFalse(); // Good + operating normally
        vm.IsValid.Should().BeTrue();

        vm.IsAttention = true;
        vm.IsIssueDescriptionVisible.Should().BeTrue();
        vm.IsValid.Should().BeFalse();

        vm.IsCritical = true;
        vm.IsIssueDescriptionVisible.Should().BeTrue();

        vm.IsGood = true;
        vm.OperatingNormally = false;
        vm.IsIssueDescriptionVisible.Should().BeTrue();
        vm.IsValid.Should().BeFalse();

        vm.IssueDescription = "Bearing noise";
        vm.IsValid.Should().BeTrue();

        vm.IssueDescription = "   ";
        vm.IsValid.Should().BeFalse();
        vm.HasIssueDescriptionError.Should().BeTrue();
    }

    [Test]
    public async Task Successful_submit_creates_exactly_one_inspection_and_navigates()
    {
        var (vm, nav, repo, _) = await NewForm();
        FillValid(vm);
        vm.IsAttention = true;
        vm.IssueDescription = "Basin-level alarm intermittent; inspect fan vibration.";
        vm.Notes = "line one\nline two";

        await vm.SubmitCommand.ExecuteAsync(null);

        var inspections = await repo.GetInspectionsAsync();
        inspections.Should().HaveCount(7);
        var saved = inspections.Single(i => i.Id == "INS-24092");
        saved.Condition.Should().Be(InspectionCondition.Attention);
        saved.Notes.Should().Be("line one\nline two");
        nav.Calls.Should().Equal("success:INS-24092");
    }

    [Test]
    public async Task Repeated_activation_while_saving_creates_one_inspection()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var slow = new DataModeRepository(repo, DataMode.Slow);
        var (vm, nav, _, _) = await NewForm(slow);
        FillValid(vm);

        var first = vm.SubmitCommand.ExecuteAsync(null);
        vm.IsSubmitting.Should().BeTrue();
        vm.SubmitCommand.CanExecute(null).Should().BeFalse();
        var second = vm.SubmitCommand.ExecuteAsync(null);
        await Task.WhenAll(first, second);

        (await repo.GetInspectionsAsync()).Should().HaveCount(7);
        nav.Calls.Should().Equal("success:INS-24092");
    }

    [Test]
    public async Task Save_failure_reports_error_and_does_not_navigate()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var (vm, nav, _, _) = await NewForm(new DataModeRepository(repo, DataMode.SaveError));
        FillValid(vm);

        await vm.SubmitCommand.ExecuteAsync(null);

        vm.SubmitError.Should().NotBeEmpty();
        nav.Calls.Should().BeEmpty();
        vm.SubmitCommand.CanExecute(null).Should().BeTrue();
        (await repo.GetInspectionsAsync()).Should().HaveCount(6);
    }

    [Test]
    public async Task Cancel_goes_back_without_saving()
    {
        var (vm, nav, repo, _) = await NewForm();
        FillValid(vm);

        vm.CancelCommand.Execute(null);

        nav.Calls.Should().Equal("back");
        (await repo.GetInspectionsAsync()).Should().HaveCount(6);
    }

    [Test]
    public async Task Picker_select_cancel_and_failure_keep_the_form_usable()
    {
        var (vm, _, repo, picker) = await NewForm();
        FillValid(vm);

        picker.Next = () => Task.FromResult<PickedFile?>(null);
        await vm.PickFileCommand.ExecuteAsync(null);
        vm.HasAttachment.Should().BeFalse();
        vm.IsValid.Should().BeTrue();

        var photo = Path.Combine(Fixtures.MockDataDir, "inspection-photo.png");
        picker.Next = () => Task.FromResult<PickedFile?>(new PickedFile("inspection-photo.png", () => Task.FromResult<Stream>(File.OpenRead(photo))));
        await vm.PickFileCommand.ExecuteAsync(null);
        vm.AttachmentName.Should().Be("inspection-photo.png");

        picker.Next = () => Task.FromResult<PickedFile?>(null); // cancel keeps the previous choice
        await vm.PickFileCommand.ExecuteAsync(null);
        vm.AttachmentName.Should().Be("inspection-photo.png");

        picker.Next = () => throw new InvalidOperationException("picker unavailable");
        await vm.PickFileCommand.ExecuteAsync(null);
        vm.AttachmentError.Should().NotBeEmpty();
        vm.AttachmentName.Should().Be("inspection-photo.png");

        await vm.SubmitCommand.ExecuteAsync(null);
        (await repo.GetInspectionsAsync()).Single(i => i.Id == "INS-24092").AttachmentFileName.Should().Be("inspection-photo.png");
    }

    [Test]
    public async Task Asset_detail_reflects_newest_inspection_after_save()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var detail = new AssetDetailViewModel(repo, new RecordingNavigator());
        detail.Load("CT-007");
        await detail.LoadAsync();
        detail.LatestInspection!.Id.Should().Be("INS-24072");
        detail.LastInspectionText.Should().Be("Sep 8, 2026");
        detail.LatestTitle.Should().Be("Critical condition");

        await repo.AddInspectionAsync(Fixtures.Draft(), null);
        await detail.LoadAsync();

        detail.LatestInspection!.Id.Should().Be("INS-24092");
        detail.Asset!.Status.Should().Be(AssetStatus.Attention);
        detail.LastInspectionText.Should().Be("Sep 23, 2026");
        detail.LatestSummary.Should().Be("Basin-level alarm intermittent");
    }

    [Test]
    public async Task Success_actions_navigate_to_asset_and_history()
    {
        var (repo, _, _) = Fixtures.NewRepository();
        var saved = await repo.AddInspectionAsync(Fixtures.Draft(), null);
        var nav = new RecordingNavigator();
        var vm = new InspectionSuccessViewModel(repo, nav);

        await vm.LoadAsync(saved.Id);
        vm.InspectionId.Should().Be("INS-24092");
        vm.AssetName.Should().Be("Cooling Tower 07");
        vm.ConditionText.Should().Be("Attention");
        vm.DetailLine.Should().Be("Sep 23, 2026 · Alex Morgan");

        vm.ViewAssetCommand.Execute(null);
        vm.ViewHistoryCommand.Execute(null);
        nav.Calls.Should().Equal("return:CT-007", "section:History");
    }

    [Test]
    public void Timestamp_formatting()
    {
        var now = new DateTime(2026, 9, 23, 15, 0, 0);
        Formats.Timestamp(new DateTime(2026, 9, 23, 14, 41, 0), now).Should().Be("Today, 2:41 PM");
        Formats.Timestamp(new DateTime(2026, 9, 22, 8, 15, 0), now).Should().Be("Yesterday, 8:15 AM");
        Formats.Timestamp(new DateTime(2026, 9, 18, 9, 24, 0), now).Should().Be("Sep 18, 9:24 AM");
        Formats.Timestamp(new DateTime(2025, 9, 18, 9, 24, 0), now).Should().Be("Sep 18, 2025, 9:24 AM");
        Formats.FacilityLine(now).Should().Be("Facility A · Wednesday, Sep 23");
        Formats.Greeting(now).Should().Be("Good afternoon");
    }
}
