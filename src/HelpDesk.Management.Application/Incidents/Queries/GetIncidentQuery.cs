using FluentResults;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetIncidentQuery(Guid Id) : IRequest<Result<IncidentDto>>;

public class GetIncidentQueryHandler : IRequestHandler<GetIncidentQuery, Result<IncidentDto>>
{
    private readonly IIncidentRepository _repository;

    public GetIncidentQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IncidentDto>> Handle(GetIncidentQuery query, CancellationToken ct)
    {
        var incidentResult = await _repository.GetById(query.Id, ct);
        if (incidentResult.IsFailed)
        {
            return Result.Fail<IncidentDto>(incidentResult.Errors);
        }

        return Result.Ok(new IncidentDto(
            Id: incidentResult.Value.Id,
            Title: incidentResult.Value.Title,
            Description: incidentResult.Value.Description,
            ReportedBy: incidentResult.Value.ReportedBy,
            ReportedAt: incidentResult.Value.ReportedAt,
            Priority: incidentResult.Value.Priority.ToString(),
            Status: incidentResult.Value.Status.ToString(),
            AssignedTo: incidentResult.Value.AssignedTo,
            Comments: incidentResult.Value.Comments.Select(c => new CommentDto(c.Text, c.AddedBy, c.AddedAt)).ToList()
        ));
    }
}

public record IncidentDto(
    Guid Id,
    string Title,
    string Description,
    string ReportedBy,
    DateTime ReportedAt,
    string Priority,
    string Status,
    string AssignedTo,
    List<CommentDto> Comments
);

public record CommentDto(
    string Text,
    string AddedBy,
    DateTime AddedAt
);