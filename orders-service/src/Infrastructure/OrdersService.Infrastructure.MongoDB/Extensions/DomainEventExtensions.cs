using System.Text;
using EventStore.Client;
using Newtonsoft.Json;
using OrdersService.Domain.Events.Base;

namespace OrdersService.Infrastructure.EventStoreDB.Extensions;

public static class DomainEventExtensions
{
    public static EventData ToEventData(this DomainEvent domainEvent)
    {
        string domainEventJson = JsonConvert.SerializeObject(domainEvent);
        byte[] domainEventBytes = Encoding.UTF8.GetBytes(domainEventJson);
        var eventData = new EventData(Uuid.NewUuid(), domainEvent.TypeTitle, domainEventBytes);

        return eventData;
    }
}