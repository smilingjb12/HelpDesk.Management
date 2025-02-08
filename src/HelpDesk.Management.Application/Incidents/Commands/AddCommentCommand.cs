using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents;
using MediatR;

namespace HelpDesk.Management.Application.Incidents.Commands;

public record AddCommentCommand(
    Guid IncidentId,
    string Comment
) : IRequest<Result<Guid>>;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<Guid>>
{
    private readonly IIncidentRepository _repository;
    private readonly ICurrentUserProvider _currentUser;

    public AddCommentCommandHandler(IIncidentRepository repository, ICurrentUserProvider currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand command, CancellationToken ct)
    {
        var incidentResult = await _repository.GetById(command.IncidentId, ct);
        if (incidentResult.IsFailed)
        {
            return Result.Fail<Guid>(incidentResult.Errors);
        }

        var result = incidentResult.Value.AddComment(command.Comment, _currentUser.UserId);
        if (result.IsFailed)
        {
            return Result.Fail<Guid>(result.Errors);
        }

        var updateResult = await _repository.Update(incidentResult.Value, ct);
        if (updateResult.IsFailed)
        {
            return Result.Fail<Guid>(updateResult.Errors);
        }

        return Result.Ok(incidentResult.Value.Id);
    }
}