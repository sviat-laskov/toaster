using EventStore.Client;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models;
using OrdersService.Infrastructure.EventStoreDB.Extensions;

namespace OrdersService.Infrastructure.EventStoreDB.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly EventStoreClient _eventStoreClient;

    public OrdersRepository(EventStoreClient eventStoreClient) => _eventStoreClient = eventStoreClient;

    public async Task<Order?> Get(Guid id, long version = long.MaxValue - 1, CancellationToken cancellationToken = default)
    {
        EventStoreClient.ReadStreamResult readResult = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            $"order-{id}",
            StreamPosition.Start,
            version + 1,
            deadline: TimeSpan.FromDays(value: 1),
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState == ReadState.StreamNotFound) return null;

        var order = (Order) Activator.CreateInstance(typeof(Order), nonPublic: true)!;
        await foreach (ResolvedEvent resolvedEvent in readResult)
        {
            var orderEvent = resolvedEvent.ToOrderEvent();
            order.When(orderEvent);
        }

        return order;
    }

    /// <inheritdoc />
    public async Task Add(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _eventStoreClient.AppendToStreamAsync(
                $"order-{order.Id}",
                StreamState.NoStream,
                MapUncommittedDomainEventsToEventsData(order),
                deadline: TimeSpan.FromDays(value: 1),
                cancellationToken: cancellationToken
            );
        }
        catch (WrongExpectedVersionException)
        {
            throw new AlreadyExistsException<Order, OrderEvent>(order);
        }
    }

    /// <inheritdoc />
    public async Task Update(Order order, CancellationToken cancellationToken = default)
    {
        EventData[] eventsData = MapUncommittedDomainEventsToEventsData(order).ToArray();
        ulong expectedVersion = order.Version - (ulong) eventsData.Length;
        try
        {
            await _eventStoreClient.AppendToStreamAsync(
                    $"order-{order.Id}",
                    expectedVersion,
                    eventsData,
                    deadline: TimeSpan.FromDays(value: 1),
                    cancellationToken: cancellationToken
                );
        }
        catch (WrongExpectedVersionException wrongExpectedVersionException)
        {
            throw new IsOutdatedException<Order, OrderEvent>(order, wrongExpectedVersionException.ActualStreamRevision);
        }
    }

    private IEnumerable<EventData> MapUncommittedDomainEventsToEventsData(Order order) => order
        .DequeueUncommittedEvents()
        .Select(orderEvent => orderEvent.ToEventData());
}