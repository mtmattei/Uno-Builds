using CameraCaptureUISample.Models;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI;

namespace CameraCaptureUISample.Controls;

public sealed partial class AnnotationPanel : UserControl
{
	private AnnotationDefinition? _annotation;
	private string _selectedPlatform = "windows";

	public event EventHandler? CloseRequested;
	public event EventHandler<string>? PlatformSelected;

	public AnnotationPanel()
	{
		this.InitializeComponent();
	}

	public void Show(AnnotationDefinition annotation, string selectedPlatform)
	{
		_annotation = annotation;
		_selectedPlatform = selectedPlatform;

		HeaderLabel.Text = annotation.Label;
		HeaderDot.Fill = new SolidColorBrush(annotation.BadgeColor);
		SummaryText.Text = annotation.Summary;
		CodeSnippetText.Text = annotation.CodeSnippet;

		UpdatePlatformSelection();
		UpdatePlatformDetail();

		Visibility = Visibility.Visible;
		PanelEntranceAnimation.Begin();
	}

	public void Hide()
	{
		Visibility = Visibility.Collapsed;
	}

	public void UpdateSelectedPlatform(string platform)
	{
		_selectedPlatform = platform;
		UpdatePlatformSelection();
		UpdatePlatformDetail();
	}

	private void UpdatePlatformSelection()
	{
		BtnWindows.IsChecked = _selectedPlatform == "windows";
		BtnAndroid.IsChecked = _selectedPlatform == "android";
		BtnIos.IsChecked = _selectedPlatform == "ios";
		BtnWasm.IsChecked = _selectedPlatform == "wasm";
	}

	private void UpdatePlatformDetail()
	{
		if (_annotation is null) return;

		if (_annotation.Platforms.TryGetValue(_selectedPlatform, out var detail))
		{
			PlatformApiName.Text = detail.ApiName;
			PlatformNote.Text = detail.Note;
		}
		else
		{
			PlatformApiName.Text = "Not available";
			PlatformNote.Text = "No platform-specific information.";
		}
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		CloseRequested?.Invoke(this, EventArgs.Empty);
	}

	private void PlatformButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is ToggleButton btn && btn.Tag is string platform)
		{
			_selectedPlatform = platform;
			UpdatePlatformSelection();
			UpdatePlatformDetail();
			PlatformSelected?.Invoke(this, platform);
		}
	}
}
