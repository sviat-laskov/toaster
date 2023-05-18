using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace OrdersService.Tests.Integration.SystemUnderTest.Containers;

internal class EventStoreTestContainer : IAsyncDisposable
{
    private const ushort PrivatePort = 2113;

    public static EventStoreTestContainer Instance => new();

    private TestcontainersContainer Container { get; } = new TestcontainersBuilder<TestcontainersContainer>()
        .WithName($"test-ordersservice-eventstore-{Guid.NewGuid()}")
        .WithImage("eventstore/eventstore:22.6.0-bionic")
        .WithExposedPort(PrivatePort)
        .WithPortBinding(PrivatePort, assignRandomHostPort: true)
        .WithEnvironment("EVENTSTORE_HTTP_PORT", "2113")
        .WithEnvironment("EVENTSTORE_INSECURE", "true")
        .WithEnvironment("EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PrivatePort))
        .Build();

    public string ConnectionString => $"esdb://{Container.Hostname}:{Container.GetMappedPublicPort(PrivatePort)}?tls=false";

    public ValueTask DisposeAsync() => Container.DisposeAsync();

    public Task Start() => Container.StartAsync();
}