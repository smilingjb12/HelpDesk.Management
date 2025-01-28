using FluentResults;
using HelpDesk.Management.Application.Incidents.Projections;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetAllIncidentHistory : IRequest<Result<IReadOnlyList<IncidentHistoryEntry>>>;

public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentHistory, Result<IReadOnlyList<IncidentHistoryEntry>>>
{
    private readonly IDocumentStore _store;

    public GetAllIncidentsQueryHandler(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Result<IReadOnlyList<IncidentHistoryEntry>>> Handle(GetAllIncidentHistory query, CancellationToken ct)
    {
        using var session = _store.LightweightSession();
        var entries = await session.Query<IncidentHistoryEntry>()
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(ct);

        return Result.Ok(entries);
    }
}