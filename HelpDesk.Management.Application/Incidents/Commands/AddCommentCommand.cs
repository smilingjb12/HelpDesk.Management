using FluentResults;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Domain.Incidents.Aggregates;
using Marten;
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
    private readonly IDocumentStore _store;
    private readonly ICurrentUserProvider _currentUser;

    public AddCommentCommandHandler(IDocumentStore store, ICurrentUserProvider currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand command, CancellationToken ct)
    {
        using var session = _store.LightweightSession();
        var incident = await session.Events.AggregateStreamAsync<Incident>(command.IncidentId, token: ct);
        var result = incident.AddComment(command.Comment, _currentUser.UserId);
        if (result.IsFailed)
        {
            return Result.Fail<Guid>(result.Errors);
        }

        session.Events.Append(command.IncidentId, result.Value);
        await session.SaveChangesAsync(ct);
        return Result.Ok();
    }
}