using HelpDesk.Management.Domain.Incidents;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Queries;

public record GetAllIncidentHistoryQuery(Guid IncidentId) : IRequest<List<IncidentHistoryDto>>;

public class GetAllIncidentHistoryQueryHandler : IRequestHandler<GetAllIncidentHistoryQuery, List<IncidentHistoryDto>>
{
    private readonly IIncidentRepository _repository;

    public GetAllIncidentHistoryQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IncidentHistoryDto>> Handle(GetAllIncidentHistoryQuery query, CancellationToken ct)
    {
        var history = await _repository.GetIncidentHistory(query.IncidentId, ct);

        if (!history.Any())
        {
            return [];
        }

        return history.Select(entry => new IncidentHistoryDto(
            Id: entry.Id,
            IncidentId: entry.IncidentId,
            EventType: entry.EventType,
            Description: entry.Description,
            PerformedBy: entry.PerformedBy,
            Timestamp: entry.Timestamp
        )).ToList();
    }
}

public record IncidentHistoryDto(
    Guid Id,
    Guid IncidentId,
    string EventType,
    string Description,
    string PerformedBy,
    DateTime Timestamp
);