using FluentResults;
using FluentValidation;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Commands;

public record LogIncidentCommand(
    string Title,
    string Description,
    string ReportedBy,
    Priority Priority
) : IRequest<Result<Guid>>;

public class LogIncidentCommandValidator : AbstractValidator<LogIncidentCommand>
{
    public LogIncidentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ReportedBy).NotEmpty();
    }
}

public class LogIncidentCommandHandler : IRequestHandler<LogIncidentCommand, Result<Guid>>
{
    private readonly IDocumentStore _store;

    public LogIncidentCommandHandler(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<Result<Guid>> Handle(LogIncidentCommand command, CancellationToken ct)
    {
        using var session = _store.LightweightSession();

        var evt = new IncidentLogged(
            Title: command.Title,
            Description: command.Description,
            ReportedBy: command.ReportedBy,
            ReportedAt: DateTime.UtcNow,
            Priority: command.Priority
        );
        var incident = Incident.Create(evt);

        session.Events.StartStream<Incident>(incident.Id, evt);
        await session.SaveChangesAsync(ct);

        return Result.Ok(incident.Id);
    }
}