using FluentResults;
using FluentValidation;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
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
    private readonly IIncidentRepository _repository;

    public LogIncidentCommandHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(LogIncidentCommand command, CancellationToken ct)
    {
        var evt = new IncidentLogged(
            Title: command.Title,
            Description: command.Description,
            ReportedBy: command.ReportedBy,
            ReportedAt: DateTime.UtcNow,
            Priority: command.Priority
        );
        var incident = Incident.Create(evt);

        var idResult = await _repository.Create(incident, ct);
        if (idResult.IsFailed)
        {
            return idResult;
        }

        return Result.Ok(idResult.Value);
    }
}