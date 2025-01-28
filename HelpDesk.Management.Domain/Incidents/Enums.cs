namespace HelpDesk.Management.Domain.Incidents;

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

public enum IncidentStatus
{
    New,
    InProgress,
    OnHold,
    Resolved,
    Closed
}
