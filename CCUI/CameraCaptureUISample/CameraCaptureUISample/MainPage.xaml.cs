using CameraCaptureUISample.Models;
using CameraCaptureUISample.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CameraCaptureUISample;

public sealed partial class MainPage : Page
{
	private readonly ICameraService _cameraService;
	private string _appState = "idle";

	public MainPage()
	{
		this.InitializeComponent();

		_cameraService = ((App)Application.Current).Host!
			.Services.GetRequiredService<ICameraService>();

		LearnToggle.LearnModeChanged += OnLearnModeChanged;
		AnnotationOverlayControl.UpdateAppState("idle");

		UpdatePlatformHint();
	}

	private void UpdatePlatformHint()
	{
#if !__ANDROID__ && !__IOS__ && !__WINDOWS__
		HintText.Text = "tap to select a photo";
#endif
	}

	private async void OpenButton_Click(object sender, RoutedEventArgs e)
	{
		await CapturePhotoAsync();
	}

	private async void ShutterButton_Click(object sender, RoutedEventArgs e)
	{
		await CapturePhotoAsync();
	}

	private void ResetButton_Click(object sender, RoutedEventArgs e)
	{
		TransitionToState("idle");
	}

	private async Task CapturePhotoAsync()
	{
		try
		{
			var file = await _cameraService.CapturePhotoAsync(CancellationToken.None);
			if (file is null) return;

			// Flash animation
			FlashAnimation.Begin();

			var bitmapImage = new BitmapImage();
			using (var stream = await file.OpenReadAsync())
			{
				await bitmapImage.SetSourceAsync(stream);
			}

			CapturedPhoto.Source = bitmapImage;
			TimestampText.Text = DateTime.Now.ToString("yyyy.MM.dd  HH:mm");

			TransitionToState("captured");
			PhotoRevealAnimation.Begin();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Capture error: {ex.Message}");
		}
	}

	private void TransitionToState(string state)
	{
		_appState = state;

		switch (state)
		{
			case "idle":
				IdleContent.Visibility = Visibility.Visible;
				CapturedContent.Visibility = Visibility.Collapsed;
				OpenButton.Visibility = Visibility.Visible;
				ShutterButton.Visibility = Visibility.Collapsed;
				ResetButton.Visibility = Visibility.Collapsed;
				CapturedPhoto.Source = null;
				break;

			case "captured":
				IdleContent.Visibility = Visibility.Collapsed;
				CapturedContent.Visibility = Visibility.Visible;
				OpenButton.Visibility = Visibility.Collapsed;
				ShutterButton.Visibility = Visibility.Visible;
				ResetButton.Visibility = Visibility.Visible;
				break;
		}

		AnnotationOverlayControl.UpdateAppState(state);
	}

	private void OnLearnModeChanged(object? sender, bool isActive)
	{
		AnnotationOverlayControl.Visibility = isActive
			? Visibility.Visible
			: Visibility.Collapsed;
	}
}
