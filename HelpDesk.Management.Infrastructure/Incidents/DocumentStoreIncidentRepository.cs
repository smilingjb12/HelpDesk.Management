using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;

namespace HelpDesk.Management.Infrastructure.Incidents;

public class DocumentStoreIncidentRepository : IIncidentRepository
{
  private readonly IDocumentStore _store;

  public DocumentStoreIncidentRepository(IDocumentStore store)
  {
    _store = store;
  }

  public async Task<IReadOnlyList<Incident>> GetAssignedIncidents(string userId, CancellationToken ct = default)
  {
    using var session = _store.QuerySession();

    return await session.Query<Incident>()
        .Where(x => x.AssignedTo == userId && x.Status != IncidentStatus.Closed)
        .ToListAsync(ct);
  }
}