using System.Collections.Immutable;
using Windows.UI;

namespace CameraCaptureUISample.Models;

public static class AnnotationRegistry
{
	public static ImmutableList<AnnotationDefinition> All { get; } =
	[
		new("cameraCaptureUI",
			Label: "CameraCaptureUI",
			IconGlyph: "\uE722",
			BadgeColor: Color.FromArgb(255, 196, 85, 61),
			Summary: "Triggers the native camera capture experience on each platform.",
			CodeSnippet: """
				var captureUI = new CameraCaptureUI();
				captureUI.PhotoSettings.Format =
				    CameraCaptureUIPhotoFormat.Jpeg;
				var file = await captureUI
				    .CaptureFileAsync(CameraCaptureUIMode.Photo);
				""",
			Platforms: new Dictionary<string, PlatformDetail>
			{
				["windows"] = new("CameraCaptureUI",
					"Native WinUI camera dialog — no additional config needed."),
				["android"] = new("CameraCaptureUI → Intent",
					"Wraps native Camera intent. Requires android.permission.CAMERA."),
				["ios"] = new("UIImagePickerController",
					"Native picker. Requires NSCameraUsageDescription in Info.plist."),
				["wasm"] = new("Not supported",
					"CaptureFileAsync returns null — show graceful fallback."),
			}.ToImmutableDictionary()),

		new("bitmapImage",
			Label: "BitmapImage",
			IconGlyph: "\uEB9F",
			BadgeColor: Color.FromArgb(255, 74, 124, 111),
			Summary: "Displays the captured JPEG by loading a stream into a BitmapImage source.",
			CodeSnippet: """
				var bitmapImage = new BitmapImage();
				using var stream = await file.OpenReadAsync();
				await bitmapImage.SetSourceAsync(stream);
				await CapturedImage.UpdateAsync(
				    _ => bitmapImage, ct);
				""",
			Platforms: new Dictionary<string, PlatformDetail>
			{
				["windows"] = new("BitmapImage",
					"WinUI BitmapImage — supports JPEG, PNG, BMP natively."),
				["android"] = new("BitmapImage → Android.Graphics.Bitmap",
					"Uno maps to Android bitmap decoding under the hood."),
				["ios"] = new("BitmapImage → UIImage",
					"Uno maps to UIKit image loading."),
				["wasm"] = new("BitmapImage → HTMLImageElement",
					"Rendered via an HTML img element in the browser."),
			}.ToImmutableDictionary()),

		new("istate",
			Label: "IState<T>",
			IconGlyph: "\uE943",
			BadgeColor: Color.FromArgb(255, 139, 105, 20),
			Summary: "MVUX reactive state that drives visual state transitions without manual PropertyChanged.",
			CodeSnippet: """
				public IState<ImageSource?> CapturedImage =>
				    State<ImageSource?>.Value(this, () => null);

				// Update triggers UI refresh automatically
				await CapturedImage.UpdateAsync(
				    _ => bitmapImage, ct);
				""",
			Platforms: new Dictionary<string, PlatformDetail>
			{
				["windows"] = new("IState<T> → INotifyPropertyChanged",
					"MVUX generates a ViewModel with change notification."),
				["android"] = new("IState<T> → INotifyPropertyChanged",
					"Same generated ViewModel — Uno handles binding."),
				["ios"] = new("IState<T> → INotifyPropertyChanged",
					"Same generated ViewModel — Uno handles binding."),
				["wasm"] = new("IState<T> → INotifyPropertyChanged",
					"Same generated ViewModel — runs in the browser."),
			}.ToImmutableDictionary()),

		new("autoLayout",
			Label: "AutoLayout",
			IconGlyph: "\uE80A",
			BadgeColor: Color.FromArgb(255, 107, 91, 149),
			Summary: "Uno Toolkit's AutoLayout + Responsive markup extension handle adaptive layout.",
			CodeSnippet: """
				<utu:AutoLayout
				    PrimaryAxisAlignment="Center"
				    CounterAxisAlignment="Center"
				    Spacing="0" Padding="24">

				  <Border Width="{utu:Responsive
				      Narrow=340, Wide=540}" />
				</utu:AutoLayout>
				""",
			Platforms: new Dictionary<string, PlatformDetail>
			{
				["windows"] = new("AutoLayout → Panel",
					"Rendered as a custom Panel with Figma-like layout."),
				["android"] = new("AutoLayout → ViewGroup",
					"Maps to Android ViewGroup layout."),
				["ios"] = new("AutoLayout → UIView",
					"Maps to UIKit view hierarchy."),
				["wasm"] = new("AutoLayout → HTML div",
					"Rendered as flexbox-based HTML layout."),
			}.ToImmutableDictionary()),
	];

	public static ImmutableList<string> VisibleKeys(string appState) =>
		appState switch
		{
			"idle" => ["autoLayout", "istate"],
			"captured" => ["bitmapImage", "istate", "cameraCaptureUI"],
			_ => [],
		};
}
