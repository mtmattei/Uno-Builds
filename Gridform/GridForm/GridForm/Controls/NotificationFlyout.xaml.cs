namespace GridForm.Controls;

public sealed partial class NotificationFlyout : UserControl
{
	public static readonly DependencyProperty NotificationsProperty =
		DependencyProperty.Register(nameof(Notifications), typeof(ImmutableList<Notification>), typeof(NotificationFlyout),
			new PropertyMetadata(null));

	public ImmutableList<Notification>? Notifications
	{
		get => (ImmutableList<Notification>?)GetValue(NotificationsProperty);
		set => SetValue(NotificationsProperty, value);
	}

	public NotificationFlyout()
	{
		this.InitializeComponent();
	}
}
