namespace FieldCheck.Services;

public enum AppSection
{
    Dashboard,
    Assets,
    History,
}

/// <summary>App navigation as seen by view models. Implemented by the shell over a Frame.</summary>
public interface INavigator
{
    void ShowSection(AppSection section);

    /// <summary>Opens an asset: master/detail selection on wide layouts, a pushed page otherwise.</summary>
    void ShowAsset(string assetId);

    void StartInspection(string assetId);

    /// <summary>Replaces the inspection form with the success page.</summary>
    void ShowInspectionSuccess(string inspectionId);

    /// <summary>Leaves the success page and shows the asset that was inspected.</summary>
    void ReturnToAsset(string assetId);

    void GoBack();
}

public interface IFilePickerService
{
    /// <summary>Shows the platform picker. Returns null when the user cancels.</summary>
    Task<Models.PickedFile?> PickFileAsync();
}
