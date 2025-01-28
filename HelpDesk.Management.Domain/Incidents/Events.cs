namespace HelpDesk.Management.Domain.Incidents;

public record IncidentEvent(Guid IncidentId) : IDomainEvent;

public record IncidentLogged(
    string Title,
    string Description,
    string ReportedBy,
    DateTime ReportedAt,
    Priority Priority
) : IDomainEvent;

public record IncidentAssigned(
    Guid IncidentId,
    string AssignedTo,
    DateTime AssignedAt
) : IncidentEvent(IncidentId);

public record CommentAdded(
    Guid IncidentId,
    string Comment,
    string AddedBy,
    DateTime AddedAt
) : IncidentEvent(IncidentId);

public record IncidentStatusChanged(
    Guid IncidentId,
    IncidentStatus NewStatus,
    string ChangedBy,
    DateTime ChangedAt
) : IncidentEvent(IncidentId);