namespace OrdersService.Domain.Events;

public static class EventTypeTitles
{
    public const string OrderCreated = "order-created",
                        OrderConfirmed = "order-confirmed",
                        OrderSubscriptionAdded = "order-subscription-added",
                        OrderSubscriptionRemoved = "order-subscription-removed";
}