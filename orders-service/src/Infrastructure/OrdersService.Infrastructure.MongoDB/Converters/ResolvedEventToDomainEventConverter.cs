using System.Text;
using System.Text.Json;
using AutoMapper;
using EventStore.Client;
using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;

namespace OrdersService.Infrastructure.EventStoreDB.Converters;

internal class ResolvedEventToDomainEventConverter : ITypeConverter<ResolvedEvent, DomainEvent>
{
    public DomainEvent Convert(ResolvedEvent resolvedEvent, DomainEvent _, ResolutionContext context)
    {
        byte[] eventDataBytes = resolvedEvent.Event.Data.ToArray();
        string eventDataJson = Encoding.UTF8.GetString(eventDataBytes);
        string domainEventEntityTypeTitle = JsonSerializer
            .Deserialize<JsonElement>(eventDataJson)
            .GetProperty("typeTitle")
            .GetString()!;

        Type domainEventEntityType = domainEventEntityTypeTitle switch
        {
            EventTypeTitles.OrderCreated => typeof(OrderCreatedEvent),
            EventTypeTitles.OrderConfirmed => typeof(OrderConfirmedEvent),
            EventTypeTitles.OrderSubscriptionAdded => typeof(OrderSubscriptionAddedEvent),
            EventTypeTitles.OrderSubscriptionRemoved => typeof(OrderSubscriptionRemovedEvent),
            _ => throw new ArgumentOutOfRangeException()
        };

        var orderEvent = (OrderEvent)JsonSerializer.Deserialize(eventDataJson, domainEventEntityType)!;

        return orderEvent;
    }
}