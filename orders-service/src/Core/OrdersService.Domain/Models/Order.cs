using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models.Base;

namespace OrdersService.Domain.Models;

public sealed class Order : Aggregate<OrderEvent>
{
    public bool IsConfirmed { get; private set; }

    public string? UserNote { get; private set; }

    public ISet<Guid> SubscriberIds { get; } = new HashSet<Guid>();

    private Order() { }

    /// <exception cref="ArgumentException" />
    public Order(Guid id, string? userNote, Guid createdBy)
    {
        if (id == Guid.Empty) throw new ArgumentException("Order id cannot be empty.", nameof(id));

        var orderCreatedEvent = new OrderCreatedEvent(id, userNote, createdBy);
        Apply(orderCreatedEvent);
        Enqueue(orderCreatedEvent);
        AddSubscription(createdBy);
    }

    public void Confirm(Guid initiatedBy)
    {
        if (IsConfirmed) return;

        var orderConfirmedEvent = new OrderConfirmedEvent(Id, initiatedBy);
        Apply(orderConfirmedEvent);
        Enqueue(orderConfirmedEvent);
    }

    public void AddSubscription(Guid initiatedBy)
    {
        if (SubscriberIds.Contains(initiatedBy)) return;

        var orderSubscriptionAddedEvent = new OrderSubscriptionAddedEvent(Id, initiatedBy);
        Apply(orderSubscriptionAddedEvent);
        Enqueue(orderSubscriptionAddedEvent);
    }

    public void RemoveSubscription(Guid initiatedBy)
    {
        if (!SubscriberIds.Contains(initiatedBy)) return;

        var orderSubscriptionRemovedEvent = new OrderSubscriptionRemovedEvent(Id, initiatedBy);
        Apply(orderSubscriptionRemovedEvent);
        Enqueue(orderSubscriptionRemovedEvent);
    }

    public override void When(OrderEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent orderCreatedEvent:
                Apply(orderCreatedEvent);
                return;
            case OrderConfirmedEvent orderConfirmedEvent:
                Apply(orderConfirmedEvent);
                return;
            case OrderSubscriptionEvent orderSubscriptionEvent:
                Apply(orderSubscriptionEvent);
                return;
        }
    }

    private void Apply(OrderCreatedEvent orderCreatedEvent)
    {
        Id = orderCreatedEvent.AggregateId;
        CreatedAt = orderCreatedEvent.OccurredAt;
        CreatedBy = orderCreatedEvent.InitiatedBy;
        UserNote = orderCreatedEvent.UserNote;

        Update(orderCreatedEvent, true);
    }

    private void Apply(OrderConfirmedEvent orderConfirmedEvent)
    {
        IsConfirmed = true;

        Update(orderConfirmedEvent);
    }

    private void Apply(OrderSubscriptionEvent orderSubscriptionEvent)
    {
        _ = orderSubscriptionEvent switch
        {
            OrderSubscriptionAddedEvent => SubscriberIds.Add(orderSubscriptionEvent.SubscriberId),
            OrderSubscriptionRemovedEvent => SubscriberIds.Remove(orderSubscriptionEvent.SubscriberId),
            _ => throw new ArgumentOutOfRangeException(nameof(orderSubscriptionEvent), orderSubscriptionEvent, null)
        };

        Update(orderSubscriptionEvent);
    }
}