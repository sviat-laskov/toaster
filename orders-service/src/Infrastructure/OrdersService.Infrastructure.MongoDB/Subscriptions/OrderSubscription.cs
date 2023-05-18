using EventStore.Client;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrdersService.Infrastructure.EventStoreDB.Extensions;

namespace OrdersService.Infrastructure.EventStoreDB.Subscriptions;

public class OrderSubscription
{
    private const string OrderGroupName = "order-group",
                         OrderStreamsPrefix = "order";
    private readonly EventStorePersistentSubscriptionsClient _eventStorePersistentSubscriptionsClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public OrderSubscription(
        EventStorePersistentSubscriptionsClient eventStorePersistentSubscriptionsClient,
        IServiceScopeFactory serviceScopeFactory)
    {
        _eventStorePersistentSubscriptionsClient = eventStorePersistentSubscriptionsClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private async Task EnsureSubscriptionExists()
    {
        try
        {
            await _eventStorePersistentSubscriptionsClient.CreateToAllAsync(
                OrderGroupName,
                StreamFilter.Prefix(OrderStreamsPrefix),
                new PersistentSubscriptionSettings(),
                TimeSpan.MaxValue);
        }
        catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.AlreadyExists)
        {
        }
    }

    public async Task SubscribeToOrderUpdates()
    {
        await EnsureSubscriptionExists();
        await _eventStorePersistentSubscriptionsClient.SubscribeToAllAsync(
            OrderGroupName,
            async (subscription, resolvedEvent, _, cancellationToken) =>
            {
                try
                {
                    await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
                    var publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();

                    var orderEvent = resolvedEvent.ToOrderEvent();
                    await publisher.Publish(orderEvent, cancellationToken);

                    await subscription.Ack(resolvedEvent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await subscription.Nack(PersistentSubscriptionNakEventAction.Retry, ex.Message, resolvedEvent);
                }
            },
            (_, dropReason, exception) => Console.WriteLine($"Subscription was dropped due to {dropReason}. {exception}"));
    }
}