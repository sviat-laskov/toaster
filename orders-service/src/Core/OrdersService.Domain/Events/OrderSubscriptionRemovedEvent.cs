using OrdersService.Domain.Events.Base;

namespace OrdersService.Domain.Events;

public sealed class OrderSubscriptionRemovedEvent : OrderSubscriptionEvent
{
    public override string TypeTitle => EventTypeTitles.OrderSubscriptionRemoved;

    public OrderSubscriptionRemovedEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId, initiatedBy, occurredAt)
    {
    }
}