using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using HelpDesk.Management.Domain.Incidents.Validation;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Commands;

public record AssignIncidentCommand(
    Guid IncidentId,
    string AssignedTo
) : IRequest<Result>;

public class AssignIncidentCommandValidator : AbstractValidator<AssignIncidentCommand>
{
    public AssignIncidentCommandValidator()
    {
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.AssignedTo).NotEmpty();
    }
}

public class AssignIncidentCommandHandler : IRequestHandler<AssignIncidentCommand, Result>
{
    private readonly IDocumentStore _store;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IIncidentValidator _validator;

    public AssignIncidentCommandHandler(
        IDocumentStore store,
        ICurrentUserProvider currentUserProvider,
        IIncidentValidator validator)
    {
        _store = store;
        _currentUserProvider = currentUserProvider;
        _validator = validator;
    }

    public async Task<Result> Handle(AssignIncidentCommand command, CancellationToken ct)
    {
        using var session = _store.LightweightSession();
        var incident = await session.Events.AggregateStreamAsync<Incident>(command.IncidentId, token: ct);

        var result = await incident.Assign(command.AssignedTo, _validator);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        session.Events.Append(command.IncidentId, result.Value);
        await session.SaveChangesAsync(ct);
        return Result.Ok();
    }
}