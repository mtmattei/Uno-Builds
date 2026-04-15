using CameraCaptureUISample.Models;
using Windows.UI;

namespace CameraCaptureUISample.Controls;

public sealed partial class AnnotationOverlay : UserControl
{
	private string _appState = "idle";
	private string? _activeKey;
	private string _selectedPlatform = "windows";

	public event EventHandler<string>? AnnotationSelected;
	public event EventHandler<string>? PlatformChanged;

	public AnnotationOverlay()
	{
		this.InitializeComponent();

		BadgeAutoLayout.BadgeColor = Color.FromArgb(255, 107, 91, 149);
		BadgeIState.BadgeColor = Color.FromArgb(255, 139, 105, 20);
		BadgeCameraCaptureUI.BadgeColor = Color.FromArgb(255, 196, 85, 61);
		BadgeBitmapImage.BadgeColor = Color.FromArgb(255, 74, 124, 111);

		BadgeAutoLayout.BadgeTapped += OnBadgeTapped;
		BadgeIState.BadgeTapped += OnBadgeTapped;
		BadgeCameraCaptureUI.BadgeTapped += OnBadgeTapped;
		BadgeBitmapImage.BadgeTapped += OnBadgeTapped;

		DetailPanel.CloseRequested += (_, _) => HidePanel();
		DetailPanel.PlatformSelected += (_, platform) => PlatformChanged?.Invoke(this, platform);

		UpdateBadgeVisibility();
	}

	public void UpdateAppState(string appState)
	{
		_appState = appState;
		_activeKey = null;
		DetailPanel.Hide();
		UpdateBadgeVisibility();
	}

	public void UpdateSelectedPlatform(string platform)
	{
		_selectedPlatform = platform;
		DetailPanel.UpdateSelectedPlatform(platform);
	}

	public void SetActiveAnnotation(string? key)
	{
		if (key == _activeKey)
		{
			HidePanel();
			return;
		}

		_activeKey = key;
		if (key is not null)
		{
			var annotation = AnnotationRegistry.All.FirstOrDefault(a => a.Key == key);
			if (annotation is not null)
			{
				DetailPanel.Show(annotation, _selectedPlatform);
			}
		}
		else
		{
			DetailPanel.Hide();
		}
	}

	private void HidePanel()
	{
		_activeKey = null;
		DetailPanel.Hide();
		AnnotationSelected?.Invoke(this, string.Empty);
	}

	private void OnBadgeTapped(object? sender, string key)
	{
		AnnotationSelected?.Invoke(this, key);
		SetActiveAnnotation(key == _activeKey ? null : key);
	}

	private void UpdateBadgeVisibility()
	{
		var visibleKeys = AnnotationRegistry.VisibleKeys(_appState);

		// All badges in order for stagger assignment
		var allBadges = new (AnnotationBadge badge, string key)[]
		{
			(BadgeAutoLayout, "autoLayout"),
			(BadgeIState, "istate"),
			(BadgeCameraCaptureUI, "cameraCaptureUI"),
			(BadgeBitmapImage, "bitmapImage"),
		};

		// Stagger: 0ms, 100ms, 200ms, 300ms for cascading reveal (§4.5)
		int staggerIndex = 0;
		foreach (var (badge, key) in allBadges)
		{
			if (visibleKeys.Contains(key))
			{
				badge.Visibility = Visibility.Visible;
				badge.PlayEntrance(TimeSpan.FromMilliseconds(staggerIndex * 100));
				staggerIndex++;
			}
			else
			{
				badge.StopAnimations();
				badge.Visibility = Visibility.Collapsed;
			}
		}
	}
}
