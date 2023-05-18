using OrdersService.Domain.Events.Base;

namespace OrdersService.Domain.Events;

public sealed class OrderSubscriptionAddedEvent : OrderSubscriptionEvent
{
    public override string TypeTitle => EventTypeTitles.OrderSubscriptionAdded;

    public OrderSubscriptionAddedEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId, initiatedBy, occurredAt)
    {
    }
}