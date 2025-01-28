namespace HelpDesk.Management.Domain.Incidents.AuditLog;

public class IncidentHistoryEntry
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}