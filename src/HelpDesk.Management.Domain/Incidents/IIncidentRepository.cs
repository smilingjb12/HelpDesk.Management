using FluentResults;
using HelpDesk.Management.Domain.Incidents.Aggregates;

namespace HelpDesk.Management.Domain.Incidents;

public interface IIncidentRepository
{
  Task<Result<Incident>> GetById(Guid id, CancellationToken ct = default);
  Task<IReadOnlyList<Incident>> GetAll(CancellationToken ct = default);
  Task<IReadOnlyList<Incident>> GetAssignedIncidents(string userId, CancellationToken ct = default);
  Task<Result<Guid>> Create(Incident incident, CancellationToken ct = default);
  Task<Result> Update(Incident incident, CancellationToken ct = default);
  Task<IReadOnlyList<IncidentHistoryEntry>> GetIncidentHistory(Guid id, CancellationToken ct = default);
}