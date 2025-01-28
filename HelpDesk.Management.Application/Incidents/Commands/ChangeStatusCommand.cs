using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Commands;

public record ChangeStatusCommandCommand(
    Guid IncidentId,
    IncidentStatus NewStatus
) : IRequest<Result<Guid>>;

public class ChangeStatusCommandCommandValidator : AbstractValidator<ChangeStatusCommandCommand>
{
    public ChangeStatusCommandCommandValidator()
    {
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Status must be a valid incident status value");
    }
}

public class ChangeStatusCommandCommandHandler : IRequestHandler<ChangeStatusCommandCommand, Result<Guid>>
{
    private readonly IDocumentStore _store;
    private readonly ICurrentUserProvider _currentUser;

    public ChangeStatusCommandCommandHandler(IDocumentStore store, ICurrentUserProvider currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(ChangeStatusCommandCommand command, CancellationToken ct)
    {
        using var session = _store.LightweightSession();
        var incident = await session.Events.AggregateStreamAsync<Incident>(command.IncidentId, token: ct);

        var result = incident.ChangeStatus(command.NewStatus, _currentUser.UserId);
        if (result.IsFailed)
        {
            return Result.Fail<Guid>(result.Errors);
        }

        session.Events.Append(command.IncidentId, result.Value);
        await session.SaveChangesAsync(ct);
        return Result.Ok(command.IncidentId);
    }
}