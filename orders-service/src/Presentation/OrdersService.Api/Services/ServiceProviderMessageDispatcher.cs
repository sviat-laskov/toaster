using EasyNetQ.AutoSubscribe;

namespace OrdersService.Api.Services;

public class ServiceProviderMessageDispatcher : IAutoSubscriberMessageDispatcher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ServiceProviderMessageDispatcher(IServiceScopeFactory serviceScopeFactory) { _serviceScopeFactory = serviceScopeFactory; }

    public void Dispatch<TMessage, TConsumer>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class
        where TConsumer : class, IConsume<TMessage>
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
        consumer.Consume(message, cancellationToken);
    }

    public async Task DispatchAsync<TMessage, TConsumer>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class
        where TConsumer : class, IConsumeAsync<TMessage>
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
        await consumer.ConsumeAsync(message, cancellationToken);
    }
}