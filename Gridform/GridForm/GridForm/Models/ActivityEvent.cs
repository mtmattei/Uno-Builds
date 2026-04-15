namespace GridForm.Models;

public record ActivityEvent(
	DateTimeOffset Timestamp,
	string Message,
	ActivityEventType Type);

public enum ActivityEventType
{
	Approval,
	Submission,
	Escalation,
	System,
	Delivery
}
