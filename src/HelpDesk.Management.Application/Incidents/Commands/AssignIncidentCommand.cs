using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Validation;
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
    private readonly IIncidentRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IIncidentValidator _validator;

    public AssignIncidentCommandHandler(
        IIncidentRepository repository,
        ICurrentUserProvider currentUserProvider,
        IIncidentValidator validator)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
        _validator = validator;
    }

    public async Task<Result> Handle(AssignIncidentCommand command, CancellationToken ct)
    {
        var incidentResult = await _repository.GetById(command.IncidentId, ct);
        if (incidentResult.IsFailed)
        {
            return Result.Fail(incidentResult.Errors);
        }

        var result = await incidentResult.Value.Assign(command.AssignedTo, _validator);
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }

        var updateResult = await _repository.Update(incidentResult.Value, ct);
        if (updateResult.IsFailed)
        {
            return Result.Fail(updateResult.Errors);
        }

        return Result.Ok();
    }
}