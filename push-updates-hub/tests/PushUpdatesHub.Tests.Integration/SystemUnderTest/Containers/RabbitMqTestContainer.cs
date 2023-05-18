using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace PushUpdatesHub.Tests.Integration.SystemUnderTest.Containers
{
    internal class RabbitMqTestContainer : IAsyncDisposable
    {
        private const ushort PrivatePort = 5672;
        private const string User = "user",
                             Password = "pass";

        public static RabbitMqTestContainer Instance => new();

        private TestcontainersContainer Container { get; } = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName($"test-pushupdateshub-rabbitmq-{Guid.NewGuid()}")
            .WithImage("rabbitmq:3.11.0")
            .WithExposedPort(PrivatePort)
            .WithPortBinding(PrivatePort, assignRandomHostPort: true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", User)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PrivatePort))
            .Build();

        public string ConnectionString => $"host={Container.Hostname};port={Container.GetMappedPublicPort(PrivatePort)};publisherConfirms=true;username={User};password={Password}";

        public ValueTask DisposeAsync() => Container.DisposeAsync();

        public Task Start() => Container.StartAsync();
    }
}