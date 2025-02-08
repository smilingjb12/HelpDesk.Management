using FluentResults;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetAllIncidentsQuery : IRequest<Result<IReadOnlyList<IncidentDto>>>;

public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, Result<IReadOnlyList<IncidentDto>>>
{
  private readonly IDocumentStore _store;

  public GetAllIncidentsQueryHandler(IDocumentStore store)
  {
    _store = store;
  }

  public async Task<Result<IReadOnlyList<IncidentDto>>> Handle(GetAllIncidentsQuery query, CancellationToken ct)
  {
    using var session = _store.QuerySession();
    var incidents = await session.Query<Incident>().ToListAsync(ct);

    var dtos = incidents.Select(incident => new IncidentDto(
        Id: incident.Id,
        Title: incident.Title,
        Description: incident.Description,
        ReportedBy: incident.ReportedBy,
        ReportedAt: incident.ReportedAt,
        Priority: incident.Priority.ToString(),
        Status: incident.Status.ToString(),
        AssignedTo: incident.AssignedTo,
        Comments: incident.Comments.Select(c => new CommentDto(c.Text, c.AddedBy, c.AddedAt)).ToList()
    )).ToList();

    return Result.Ok<IReadOnlyList<IncidentDto>>(dtos);
  }
}