using HelpDesk.Management.Domain.Incidents;
using Marten.Events.Projections;

namespace HelpDesk.Management.Application.Incidents.Projections;

public class IncidentHistoryProjection : MultiStreamProjection<IncidentHistoryEntry, Guid>
{
    public IncidentHistoryProjection()
    {
        Identity<IncidentLogged>(e => Guid.NewGuid());
        Identity<IncidentAssigned>(e => Guid.NewGuid());
        Identity<CommentAdded>(e => Guid.NewGuid());
        Identity<IncidentStatusChanged>(e => Guid.NewGuid());
    }

    public void Apply(IncidentLogged evt, IncidentHistoryEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.EventType = "IncidentLogged";
        entry.Description = $"Incident '{evt.Title}' was created with {evt.Priority} priority";
        entry.PerformedBy = evt.ReportedBy;
        entry.Timestamp = evt.ReportedAt;
    }

    public void Apply(IncidentAssigned evt, IncidentHistoryEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.IncidentId = evt.IncidentId;
        entry.EventType = "IncidentAssigned";
        entry.Description = $"Incident was assigned to {evt.AssignedTo}";
        entry.PerformedBy = evt.AssignedTo;
        entry.Timestamp = evt.AssignedAt;
    }

    public void Apply(CommentAdded evt, IncidentHistoryEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.IncidentId = evt.IncidentId;
        entry.EventType = "CommentAdded";
        entry.Description = $"Comment added: {evt.Comment}";
        entry.PerformedBy = evt.AddedBy;
        entry.Timestamp = evt.AddedAt;
    }

    public void Apply(IncidentStatusChanged evt, IncidentHistoryEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.IncidentId = evt.IncidentId;
        entry.EventType = "StatusChanged";
        entry.Description = $"Status changed to {evt.NewStatus}";
        entry.PerformedBy = evt.ChangedBy;
        entry.Timestamp = evt.ChangedAt;
    }
}
