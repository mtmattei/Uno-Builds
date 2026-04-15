namespace GridForm.Presentation;

public partial record ShellModel(INotificationService Notifications)
{
	public IListFeed<Notification> NotificationList =>
		ListFeed.Async(Notifications.GetNotifications);
}
