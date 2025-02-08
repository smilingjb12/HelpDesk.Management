using FluentResults;
using HelpDesk.Management.Domain.Incidents.Validation;

namespace HelpDesk.Management.Domain.Incidents.Aggregates;

public partial class Incident
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ReportedBy { get; private set; } = string.Empty;
    public DateTime ReportedAt { get; private set; }
    public Priority Priority { get; private set; }
    public IncidentStatus Status { get; private set; }
    public string AssignedTo { get; private set; } = string.Empty;
    public List<Comment> Comments { get; private set; } = [];

    private Incident() { }

    public static Incident Create(IncidentLogged evt)
    {
        if (string.IsNullOrWhiteSpace(evt.Title))
        {
            throw new ArgumentException("Title cannot be empty or whitespace.");
        }

        return new Incident
        {
            Id = Guid.NewGuid(),
            Title = evt.Title,
            Description = evt.Description,
            ReportedBy = evt.ReportedBy,
            ReportedAt = evt.ReportedAt,
            Priority = evt.Priority,
            Status = IncidentStatus.New
        };
    }

    public async Task<Result<IDomainEvent>> Assign(string assignedTo, IIncidentValidator validator)
    {
        if (Status == IncidentStatus.Closed)
        {
            return Result.Fail("Cannot assign to closed incident");
        }

        if (assignedTo == AssignedTo)
        {
            return Result.Fail("Cannot assign to same user");
        }

        var validationResult = await validator.CheckNotAssignedToIncidentAlready(assignedTo);
        if (validationResult.IsFailed)
        {
            return Result.Fail(validationResult.Errors);
        }

        return Result.Ok(new IncidentAssigned(Id, assignedTo, DateTime.UtcNow) as IDomainEvent);
    }

    public Result<IDomainEvent> AddComment(string comment, string addedBy)
    {
        if (Status == IncidentStatus.Closed)
        {
            return Result.Fail("Cannot add comments to closed incidents");
        }

        return Result.Ok(new CommentAdded(Id, comment, addedBy, DateTime.UtcNow) as IDomainEvent);
    }

    public Result<IDomainEvent> ChangeStatus(IncidentStatus newStatus, string changedBy)
    {
        if (Status == newStatus)
        {
            return Result.Fail("Incident is already in this status");
        }

        return Result.Ok(new IncidentStatusChanged(Id, newStatus, changedBy, DateTime.UtcNow) as IDomainEvent);
    }

    public void Apply(IncidentLogged evt)
    {
        Title = evt.Title;
        Description = evt.Description;
        ReportedBy = evt.ReportedBy;
        ReportedAt = evt.ReportedAt;
        Priority = evt.Priority;
        Status = IncidentStatus.New;
    }

    public void Apply(IncidentAssigned evt) =>
        AssignedTo = evt.AssignedTo;

    public void Apply(CommentAdded evt) =>
        Comments.Add(new Comment(evt.Comment, evt.AddedBy, evt.AddedAt));

    public void Apply(IncidentStatusChanged evt) =>
        Status = evt.NewStatus;
}

public record Comment(string Text, string AddedBy, DateTime AddedAt);