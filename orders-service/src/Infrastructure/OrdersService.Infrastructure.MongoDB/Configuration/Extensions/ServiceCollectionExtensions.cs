using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Infrastructure.EventStoreDB.Repositories;
using OrdersService.Infrastructure.EventStoreDB.Subscriptions;

namespace OrdersService.Infrastructure.EventStoreDB.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureEventStoreDb(this IServiceCollection services) => services
        .AddOptions<EventStoreOptions>()
        .BindConfiguration("EventStore")
        .ValidateDataAnnotations()
        .Services
        .AddSingleton(serviceProvider => new EventStoreClient(EventStoreClientSettings.Create(serviceProvider.GetRequiredService<IOptions<EventStoreOptions>>().Value.ConnectionString)))
        .AddSingleton(serviceProvider => new EventStorePersistentSubscriptionsClient(EventStoreClientSettings.Create(serviceProvider.GetRequiredService<IOptions<EventStoreOptions>>().Value.ConnectionString)))
        .AddSingleton<IOrdersRepository, OrdersRepository>()
        .AddSingleton<OrderSubscription>();
}