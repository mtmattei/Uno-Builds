using Windows.Storage;
using Windows.Storage.Pickers;

#if __ANDROID__ || __IOS__ || __WINDOWS__
using Windows.Media.Capture;
#endif

namespace CameraCaptureUISample.Services;

public class CameraService : ICameraService
{
	public async Task<StorageFile?> CapturePhotoAsync(CancellationToken ct)
	{
		try
		{
#if __ANDROID__ || __IOS__ || __WINDOWS__
			var captureUI = new CameraCaptureUI();
			captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
			return await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
#else
			// Desktop/WASM/Linux fallback: file picker
			var picker = new FileOpenPicker();
			picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");
			picker.FileTypeFilter.Add(".bmp");
			return await picker.PickSingleFileAsync();
#endif
		}
		catch
		{
			return null;
		}
	}
}
