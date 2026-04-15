namespace GridForm.Models;

public record ApprovalStep(
	string Name,
	string Role,
	ApprovalStatus Status,
	DateTimeOffset? CompletedAt = null);

public enum ApprovalStatus
{
	Waiting,
	Current,
	Done
}
