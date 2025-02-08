using FluentResults;

namespace HelpDesk.Management.Domain.Incidents.Validation;

public interface IIncidentValidator
{
  Task<Result> CheckNotAssignedToIncidentAlready(string userId, CancellationToken ct = default);
}