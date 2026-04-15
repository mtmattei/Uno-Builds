using GridForm.Services.Impl;

namespace GridForm.Presentation;

public sealed partial class MainPage : Page
{
	public MainPage()
	{
		this.InitializeComponent();
		this.Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		var notifService = App.Current.Host?.Services?.GetService<INotificationService>();
		if (notifService is not null)
		{
			var notifications = await notifService.GetNotifications();
			NotifFlyoutContent.Notifications = notifications;
		}
	}

	private void OnSearchTriggerClick(object sender, RoutedEventArgs e)
	{
		CmdPalette.Show();
	}
}
