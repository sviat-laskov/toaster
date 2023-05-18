using OrdersService.Domain.Events.Base;

namespace OrdersService.Domain.Events;

public sealed class OrderConfirmedEvent : OrderEvent
{
    public override string TypeTitle => EventTypeTitles.OrderConfirmed;

    public OrderConfirmedEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId, initiatedBy, occurredAt)
    {
    }
}