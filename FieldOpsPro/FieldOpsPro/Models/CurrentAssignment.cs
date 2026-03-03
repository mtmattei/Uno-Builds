using FieldOpsPro.Models.Enums;

namespace FieldOpsPro.Models;

public record CurrentAssignment(
    AgentStatus Status,
    string? Destination = null,
    string? Eta = null,
    Location? DestinationLocation = null
);
