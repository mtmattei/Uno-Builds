namespace GridForm.Models;

public partial record Notification(
	string Id,
	string Title,
	string Body,
	NotificationType Type,
	DateTimeOffset Timestamp,
	bool IsRead = false);

public enum NotificationType
{
	Info,
	Warning,
	Success,
	Error
}
