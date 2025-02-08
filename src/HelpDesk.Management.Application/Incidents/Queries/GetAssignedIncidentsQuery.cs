using HelpDesk.Management.Domain.Incidents;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetAssignedIncidentsQuery(string UserId) : IRequest<List<IncidentSummaryDto>>;

public class GetAssignedIncidentsQueryHandler : IRequestHandler<GetAssignedIncidentsQuery, List<IncidentSummaryDto>>
{
  private readonly IIncidentRepository _repository;

  public GetAssignedIncidentsQueryHandler(IIncidentRepository repository)
  {
    _repository = repository;
  }

  public async Task<List<IncidentSummaryDto>> Handle(GetAssignedIncidentsQuery query, CancellationToken ct)
  {
    var incidents = await _repository.GetAssignedIncidents(query.UserId, ct);

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