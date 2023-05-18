using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace OrdersService.Tests.Integration.SystemUnderTest.Containers
{
    internal class ElasticSearchTestContainer : IAsyncDisposable
    {
        private const ushort PrivatePort = 9200;

        public static ElasticSearchTestContainer Instance => new();

        private TestcontainersContainer Container { get; } = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName($"test-ordersservice-elasticsearch-{Guid.NewGuid()}")
            .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.4.1")
            .WithExposedPort(PrivatePort)
            .WithPortBinding(PrivatePort, assignRandomHostPort: true)
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms1g -Xmx1g")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PrivatePort))
            .Build();

        public string ConnectionString => $"http://{Container.Hostname}:{Container.GetMappedPublicPort(PrivatePort)}/";

        public ValueTask DisposeAsync() => Container.DisposeAsync();

        public Task Start() => Container.StartAsync();
    }
}