using FieldCheck.Models;
using Windows.Storage.Pickers;

namespace FieldCheck.Services;

/// <summary>Opens the platform file picker (Storage Access Framework on Android, Win32 dialog on Windows).</summary>
public sealed class FilePickerService(Func<Window> window) : IFilePickerService
{
    public async Task<PickedFile?> PickFileAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail,
        };
        picker.FileTypeFilter.Add("*");

#if !HAS_UNO
        // WinAppSDK pickers must be parented to the app window.
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window()));
#else
        _ = window;
#endif

        var file = await picker.PickSingleFileAsync();
        return file is null
            ? null
            : new PickedFile(file.Name, () => file.OpenStreamForReadAsync());
    }
}
