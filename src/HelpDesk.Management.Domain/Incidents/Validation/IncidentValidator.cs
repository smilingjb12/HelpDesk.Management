using FluentResults;

namespace HelpDesk.Management.Domain.Incidents.Validation;

public class IncidentValidator : IIncidentValidator
{
  private readonly IIncidentRepository _repository;

  public IncidentValidator(IIncidentRepository repository)
  {
    _repository = repository;
  }

  public async Task<Result> CheckNotAssignedToIncidentAlready(string userId, CancellationToken ct = default)
  {
    var assignedIncidents = await _repository.GetAssignedIncidents(userId, ct);

    if (assignedIncidents.Any())
    {
      return Result.Fail($"User {userId} already has active incidents assigned");
    }

    return Result.Ok();
  }
}