using OrdersService.Domain.Events.Base;

namespace OrdersService.Domain.Events;

public sealed class OrderCreatedEvent : OrderEvent
{
    public string? UserNote { get; }

    public override string TypeTitle => EventTypeTitles.OrderCreated;

    public OrderCreatedEvent(
        Guid? aggregateId,
        string? userNote,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null) : base(aggregateId ?? Guid.NewGuid(), initiatedBy, occurredAt) => UserNote = userNote;
}