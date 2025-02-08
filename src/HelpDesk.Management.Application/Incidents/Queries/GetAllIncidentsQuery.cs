using FluentResults;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetAllIncidentsQuery() : IRequest<List<IncidentSummaryDto>>;

public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, List<IncidentSummaryDto>>
{
  private readonly IIncidentRepository _repository;

  public GetAllIncidentsQueryHandler(IIncidentRepository repository)
  {
    _repository = repository;
  }

  public async Task<List<IncidentSummaryDto>> Handle(GetAllIncidentsQuery query, CancellationToken ct)
  {
    var incidents = await _repository.GetAll(ct);

    return incidents.Select(incident => new IncidentSummaryDto(
        Id: incident.Id,
        Title: incident.Title,
        Status: incident.Status.ToString(),
        Priority: incident.Priority.ToString(),
        ReportedBy: incident.ReportedBy,
        ReportedAt: incident.ReportedAt,
        AssignedTo: incident.AssignedTo
    )).ToList();
  }
}

public record IncidentSummaryDto(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    string ReportedBy,
    DateTime ReportedAt,
    string AssignedTo
);