namespace OrdersService.Domain.Events.Base;

public abstract class OrderEvent : DomainEvent
{
    protected OrderEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId, initiatedBy, occurredAt)
    {
    }
}