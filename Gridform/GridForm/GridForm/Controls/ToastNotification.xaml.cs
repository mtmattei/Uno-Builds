namespace GridForm.Controls;

public sealed partial class ToastNotification : UserControl
{
	private DispatcherTimer? _timer;

	public ToastNotification()
	{
		this.InitializeComponent();
	}

	public void Show(string message, int durationMs = 2400)
	{
		ToastText.Text = message;
		ToastBorder.Visibility = Visibility.Visible;

		_timer?.Stop();
		_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
		_timer.Tick += (s, e) =>
		{
			_timer.Stop();
			ToastBorder.Visibility = Visibility.Collapsed;
		};
		_timer.Start();
	}
}
