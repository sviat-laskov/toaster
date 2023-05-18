using System.Text;
using System.Text.Json;
using EventStore.Client;
using Newtonsoft.Json;
using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OrdersService.Infrastructure.EventStoreDB.Extensions;

public static class ResolvedEventExtensions
{
    public static OrderEvent ToOrderEvent(this ResolvedEvent resolvedEvent)
    {
        byte[] eventDataBytes = resolvedEvent.Event.Data.ToArray();
        string eventDataJson = Encoding.UTF8.GetString(eventDataBytes);
        string orderEventTypeTitle = JsonSerializer
            .Deserialize<JsonElement>(eventDataJson)
            .GetProperty(nameof(OrderEvent.TypeTitle))
            .GetString()!;

        Type orderEventType = orderEventTypeTitle switch
        {
            EventTypeTitles.OrderCreated => typeof(OrderCreatedEvent),
            EventTypeTitles.OrderConfirmed => typeof(OrderConfirmedEvent),
            EventTypeTitles.OrderSubscriptionAdded => typeof(OrderSubscriptionAddedEvent),
            EventTypeTitles.OrderSubscriptionRemoved => typeof(OrderSubscriptionRemovedEvent),
            _ => throw new ArgumentOutOfRangeException()
        };

        var orderEvent = (OrderEvent)JsonConvert.DeserializeObject(eventDataJson, orderEventType)!;

        return orderEvent;
    }
}