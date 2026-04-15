namespace GridForm.Models;

public record AuditEntry(
	DateTimeOffset Timestamp,
	string Message);
