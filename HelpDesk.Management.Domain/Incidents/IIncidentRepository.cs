using HelpDesk.Management.Domain.Incidents.Aggregates;

namespace HelpDesk.Management.Domain.Incidents;

public interface IIncidentRepository
{
  Task<IReadOnlyList<Incident>> GetAssignedIncidents(string userId, CancellationToken ct = default);
}