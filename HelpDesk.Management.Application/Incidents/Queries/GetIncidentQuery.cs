using FluentResults;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetIncidentQuery(Guid Id) : IRequest<Result<IncidentDto>>;

public class GetIncidentQueryHandler : IRequestHandler<GetIncidentQuery, Result<IncidentDto>>
{
    private readonly IDocumentStore _store;

    public GetIncidentQueryHandler(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Result<IncidentDto>> Handle(GetIncidentQuery query, CancellationToken ct)
    {
        using var session = _store.QuerySession();
        var incident = await session.Events.AggregateStreamAsync<Incident>(query.Id, token: ct);

        if (incident == null)
        {
            return Result.Fail<IncidentDto>($"Incident {query.Id} not found");
        }

        return Result.Ok(new IncidentDto(
            Id: incident.Id,
            Title: incident.Title,
            Description: incident.Description,
            ReportedBy: incident.ReportedBy,
            ReportedAt: incident.ReportedAt,
            Priority: incident.Priority.ToString(),
            Status: incident.Status.ToString(),
            AssignedTo: incident.AssignedTo,
            Comments: incident.Comments.Select(c => new CommentDto(c.Text, c.AddedBy, c.AddedAt)).ToList()
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