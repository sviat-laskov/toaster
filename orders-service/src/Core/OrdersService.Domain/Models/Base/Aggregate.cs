using OrdersService.Domain.Events.Base;

namespace OrdersService.Domain.Models.Base;

public abstract class Aggregate<TDomainEvent> where TDomainEvent : DomainEvent
{
    private readonly Queue<TDomainEvent> _uncommittedEvents = new();

    public ulong Version { get; protected set; }

    public Guid Id { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public Guid CreatedBy { get; protected set; }

    public DateTimeOffset LastModifiedAt { get; protected set; }

    public Guid LastModifiedBy { get; protected set; }

    public abstract void When(TDomainEvent @event);

    protected void Enqueue(TDomainEvent @event) => _uncommittedEvents.Enqueue(@event);

    public IReadOnlyCollection<TDomainEvent> DequeueUncommittedEvents()
    {
        TDomainEvent[] uncommittedEvents = _uncommittedEvents.ToArray();
        _uncommittedEvents.Clear();
        return uncommittedEvents;
    }

    protected void Update(TDomainEvent domainEvent, bool isInitial = false)
    {
        LastModifiedAt = domainEvent.OccurredAt;
        LastModifiedBy = domainEvent.InitiatedBy;
        if(!isInitial) Version++;
    }
}