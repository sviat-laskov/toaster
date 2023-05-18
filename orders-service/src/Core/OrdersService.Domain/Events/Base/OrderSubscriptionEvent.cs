namespace OrdersService.Domain.Events.Base;

public abstract class OrderSubscriptionEvent : OrderEvent
{
    public Guid SubscriberId { get; }

    protected OrderSubscriptionEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId, initiatedBy, occurredAt) => SubscriberId = initiatedBy;
}