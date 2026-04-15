namespace CameraCaptureUISample.Controls;

public sealed partial class LearnModeToggle : UserControl
{
	public static readonly DependencyProperty IsActiveProperty =
		DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LearnModeToggle),
			new PropertyMetadata(true, OnIsActiveChanged));

	public bool IsActive
	{
		get => (bool)GetValue(IsActiveProperty);
		set => SetValue(IsActiveProperty, value);
	}

	public event EventHandler<bool>? LearnModeChanged;

	public LearnModeToggle()
	{
		this.InitializeComponent();
		UpdateVisuals(true);
	}

	private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is LearnModeToggle toggle && e.NewValue is bool isActive)
		{
			toggle.LearnToggle.IsChecked = isActive;
			toggle.UpdateVisuals(isActive);
		}
	}

	private void LearnToggle_Changed(object sender, RoutedEventArgs e)
	{
		var isActive = LearnToggle.IsChecked == true;
		IsActive = isActive;
		UpdateVisuals(isActive);
		LearnModeChanged?.Invoke(this, isActive);
	}

	private void UpdateVisuals(bool isActive)
	{
		if (isActive)
		{
			LearnDot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 196, 85, 61));
			LearnLabel.Text = "Learn";
		}
		else
		{
			LearnDot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 138, 133, 128));
			LearnLabel.Text = "Learn";
		}
	}
}
