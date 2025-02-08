using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Aggregates;
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
    private readonly IIncidentRepository _repository;
    private readonly ICurrentUserProvider _currentUser;

    public ChangeStatusCommandCommandHandler(IIncidentRepository repository, ICurrentUserProvider currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(ChangeStatusCommandCommand command, CancellationToken ct)
    {
        var incidentResult = await _repository.GetById(command.IncidentId, ct);
        if (incidentResult.IsFailed)
        {
            return Result.Fail<Guid>(incidentResult.Errors);
        }

        var result = incidentResult.Value.ChangeStatus(command.NewStatus, _currentUser.UserId);
        if (result.IsFailed)
        {
            return Result.Fail<Guid>(result.Errors);
        }

        var updateResult = await _repository.Update(incidentResult.Value, ct);
        if (updateResult.IsFailed)
        {
            return Result.Fail<Guid>(updateResult.Errors);
        }

        return Result.Ok(command.IncidentId);
    }
}