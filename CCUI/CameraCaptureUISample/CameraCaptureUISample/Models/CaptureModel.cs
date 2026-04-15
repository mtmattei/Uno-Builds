using CameraCaptureUISample.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CameraCaptureUISample.Models;

public partial record CaptureModel(ICameraService Camera)
{
	public IState<ImageSource?> CapturedImage =>
		State<ImageSource?>.Value(this, () => null);

	public IState<bool> IsLearnModeActive =>
		State<bool>.Value(this, () => true);

	public IState<string?> ActiveAnnotationKey =>
		State<string?>.Value(this, () => null);

	public IState<string> SelectedPlatform =>
		State<string>.Value(this, () => GetCurrentPlatform());

	public async ValueTask CapturePhoto(CancellationToken ct)
	{
		var file = await Camera.CapturePhotoAsync(ct);
		if (file is not null)
		{
			var bitmapImage = new BitmapImage();
			using var stream = await file.OpenReadAsync();
			await bitmapImage.SetSourceAsync(stream);
			await CapturedImage.UpdateAsync(_ => bitmapImage, ct);
		}
		await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
	}

	public async ValueTask Reset(CancellationToken ct)
	{
		await CapturedImage.UpdateAsync(_ => null, ct);
		await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
	}

	public async ValueTask ToggleLearnMode(CancellationToken ct)
	{
		await IsLearnModeActive.UpdateAsync(v => !v, ct);
		await ActiveAnnotationKey.UpdateAsync(_ => null, ct);
	}

	public async ValueTask SelectAnnotation(string key, CancellationToken ct)
	{
		var current = await ActiveAnnotationKey;
		await ActiveAnnotationKey.UpdateAsync(
			_ => current == key ? null : key, ct);
	}

	public async ValueTask SelectPlatform(string platform, CancellationToken ct)
	{
		await SelectedPlatform.UpdateAsync(_ => platform, ct);
	}

	private static string GetCurrentPlatform()
	{
#if __ANDROID__
		return "android";
#elif __IOS__
		return "ios";
#elif __WASM__
		return "wasm";
#else
		return "windows";
#endif
	}
}
