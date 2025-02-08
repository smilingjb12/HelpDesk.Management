using FluentResults;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using HelpDesk.Management.Application.Incidents.Projections;
using Marten;

namespace HelpDesk.Management.Infrastructure.Incidents;

public class DocumentStoreIncidentRepository : IIncidentRepository
{
  private readonly IDocumentStore _store;

  public DocumentStoreIncidentRepository(IDocumentStore store)
  {
    _store = store;
  }

  public async Task<Result<Incident>> GetById(Guid id, CancellationToken ct = default)
  {
    using var session = _store.QuerySession();
    var incident = await session.LoadAsync<Incident>(id, ct);

    if (incident == null)
    {
      return Result.Fail($"Incident with id {id} not found");
    }

    return Result.Ok(incident);
  }

  public async Task<IReadOnlyList<Incident>> GetAll(CancellationToken ct = default)
  {
    using var session = _store.QuerySession();
    return await session.Query<Incident>().ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Incident>> GetAssignedIncidents(string userId, CancellationToken ct = default)
  {
    using var session = _store.QuerySession();
    return await session.Query<Incident>()
        .Where(x => x.AssignedTo == userId && x.Status != IncidentStatus.Closed)
        .ToListAsync(ct);
  }

  public async Task<Result<Guid>> Create(Incident incident, CancellationToken ct = default)
  {
    using var session = _store.LightweightSession();
    session.Store(incident);
    await session.SaveChangesAsync(ct);
    return Result.Ok(incident.Id);
  }

  public async Task<Result> Update(Incident incident, CancellationToken ct = default)
  {
    using var session = _store.LightweightSession();
    session.Update(incident);
    await session.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<IReadOnlyList<IncidentHistoryEntry>> GetIncidentHistory(Guid id, CancellationToken ct = default)
  {
    using var session = _store.QuerySession();
    return await session.Query<IncidentHistoryEntry>()
        .Where(x => x.IncidentId == id)
        .OrderByDescending(x => x.Timestamp)
        .ToListAsync(ct);
  }
}