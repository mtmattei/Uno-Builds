using System.Globalization;
using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public sealed partial class NewInspectionViewModel : ObservableObject
{
    public const double MinTemperature = -50;
    public const double MaxTemperature = 250;

    private readonly IFieldCheckRepository _repository;
    private readonly INavigator _navigator;
    private readonly IFilePickerService _filePicker;
    private PickedFile? _attachment;
    private bool _issueTouched;

    public NewInspectionViewModel(IFieldCheckRepository repository, INavigator navigator, IFilePickerService filePicker)
    {
        _repository = repository;
        _navigator = navigator;
        _filePicker = filePicker;
        Validate();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AssetLine))]
    public partial Asset? Asset { get; private set; }

    [ObservableProperty]
    public partial bool IsAssetMissing { get; private set; }

    public string AssetLine => Asset is null ? string.Empty : $"{Asset.Name} · {Asset.Id}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGood), nameof(IsAttention), nameof(IsCritical))]
    public partial InspectionCondition? Condition { get; set; }

    public bool IsGood { get => Condition == InspectionCondition.Good; set { if (value) Condition = InspectionCondition.Good; } }

    public bool IsAttention { get => Condition == InspectionCondition.Attention; set { if (value) Condition = InspectionCondition.Attention; } }

    public bool IsCritical { get => Condition == InspectionCondition.Critical; set { if (value) Condition = InspectionCondition.Critical; } }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OperatingNormallyText))]
    public partial bool OperatingNormally { get; set; } = true;

    public string OperatingNormallyText => OperatingNormally ? "Yes" : "No";

    [ObservableProperty]
    public partial string TemperatureText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool GuardsSecure { get; set; }

    [ObservableProperty]
    public partial bool NoLeaksOrDamage { get; set; }

    [ObservableProperty]
    public partial bool AreaClear { get; set; }

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IssueDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsIssueDescriptionVisible { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTemperatureError))]
    public partial string TemperatureError { get; private set; } = string.Empty;

    public bool HasTemperatureError => TemperatureError.Length > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIssueDescriptionError))]
    public partial string IssueDescriptionError { get; private set; } = string.Empty;

    public bool HasIssueDescriptionError => IssueDescriptionError.Length > 0;

    /// <summary>What still blocks submission, e.g. "To submit: select a condition · confirm 2 checklist items".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMissingSummary))]
    public partial string MissingSummary { get; private set; } = string.Empty;

    public bool HasMissingSummary => MissingSummary.Length > 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial bool IsValid { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand), nameof(PickFileCommand), nameof(RemoveAttachmentCommand), nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(SubmitText))]
    public partial bool IsSubmitting { get; private set; }

    public string SubmitText => IsSubmitting ? "Saving…" : "Submit inspection";

    [ObservableProperty]
    public partial string SubmitError { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAttachment))]
    public partial string AttachmentName { get; private set; } = string.Empty;

    public bool HasAttachment => AttachmentName.Length > 0;

    [ObservableProperty]
    public partial string AttachmentError { get; private set; } = string.Empty;

    public async Task LoadAsync(string assetId)
    {
        try
        {
            var assets = await _repository.GetAssetsAsync();
            Asset = assets.FirstOrDefault(a => a.Id == assetId);
        }
        catch (RepositoryException)
        {
            Asset = null;
        }

        IsAssetMissing = Asset is null;
        Validate();
    }

    partial void OnConditionChanged(InspectionCondition? value) => Validate();

    partial void OnOperatingNormallyChanged(bool value) => Validate();

    partial void OnTemperatureTextChanged(string value) => Validate();

    partial void OnGuardsSecureChanged(bool value) => Validate();

    partial void OnNoLeaksOrDamageChanged(bool value) => Validate();

    partial void OnAreaClearChanged(bool value) => Validate();

    partial void OnIssueDescriptionChanged(string value)
    {
        _issueTouched = true;
        Validate();
    }

    public static bool TryParseTemperature(string text, out double value)
    {
        var normalized = text.Trim().Replace('−', '-').Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)
            && double.IsFinite(value);
    }

    private void Validate()
    {
        var missing = new List<string>();

        if (Condition is null)
        {
            missing.Add("select a condition");
        }

        var tempText = TemperatureText.Trim();
        if (tempText.Length == 0)
        {
            TemperatureError = string.Empty;
            missing.Add("enter a temperature");
        }
        else if (!TryParseTemperature(tempText, out var temperature))
        {
            TemperatureError = "Enter the temperature as a number, for example 27.";
            missing.Add("fix the temperature");
        }
        else if (temperature is < MinTemperature or > MaxTemperature)
        {
            TemperatureError = "Temperature must be between -50 and 250 °C.";
            missing.Add("fix the temperature");
        }
        else
        {
            TemperatureError = string.Empty;
        }

        var openItems = new[] { GuardsSecure, NoLeaksOrDamage, AreaClear }.Count(c => !c);
        if (openItems > 0)
        {
            missing.Add(openItems == 1 ? "confirm 1 checklist item" : $"confirm {openItems} checklist items");
        }

        IsIssueDescriptionVisible = Condition is InspectionCondition.Attention or InspectionCondition.Critical || !OperatingNormally;
        var issueMissing = IsIssueDescriptionVisible && string.IsNullOrWhiteSpace(IssueDescription);
        IssueDescriptionError = issueMissing && _issueTouched ? "Describe the issue before submitting." : string.Empty;
        if (issueMissing)
        {
            missing.Add("describe the issue");
        }

        if (Asset is null)
        {
            missing.Clear();
            missing.Add("the asset could not be loaded");
        }

        MissingSummary = missing.Count == 0 ? string.Empty : "To submit: " + string.Join(" · ", missing) + ".";
        IsValid = missing.Count == 0;
    }

    private bool CanSubmit() => IsValid && !IsSubmitting;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        // AsyncRelayCommand already rejects re-entry; this guards direct calls too.
        if (!CanSubmit() || Asset is null || Condition is null || !TryParseTemperature(TemperatureText, out var temperature))
        {
            return;
        }

        IsSubmitting = true;
        SubmitError = string.Empty;
        try
        {
            var draft = new InspectionDraft(
                Asset.Id,
                Condition.Value,
                OperatingNormally,
                temperature,
                Notes,
                IsIssueDescriptionVisible ? IssueDescription : null);

            var saved = await _repository.AddInspectionAsync(draft, _attachment);
            _navigator.ShowInspectionSuccess(saved.Id);
        }
        catch (RepositoryException)
        {
            SubmitError = "The inspection wasn't saved. Nothing was recorded — check storage and try again.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private bool CanEdit() => !IsSubmitting;

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task PickFileAsync()
    {
        AttachmentError = string.Empty;
        try
        {
            var file = await _filePicker.PickFileAsync();
            if (file is null)
            {
                return; // Cancelled: keep whatever was attached before.
            }

            _attachment = file;
            AttachmentName = file.FileName;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            AttachmentError = "The file picker isn't available right now. You can submit without an attachment.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void RemoveAttachment()
    {
        _attachment = null;
        AttachmentName = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Cancel() => _navigator.GoBack();
}
