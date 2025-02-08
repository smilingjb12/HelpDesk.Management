namespace HelpDesk.Management.Domain.Incidents;

public class AggregateBase<TId>
    where TId : notnull
{
    protected List<IDomainEvent> _events = [];

    public TId Id { get; set; } = default!;

    public IList<IDomainEvent> PopPendingEvents()
    {
        var events = _events;
        _events = [];
        return events;
    }
}
