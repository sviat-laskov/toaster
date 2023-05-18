using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrdersService.Api;
using OrdersService.Tests.Integration.SystemUnderTest.Containers;

namespace OrdersService.Tests.Integration.SystemUnderTest
{
    internal class OrdersServiceApplicationFactory : WebApplicationFactory<Program>
    {
        public EventStoreTestContainer EventStoreTestContainer { get; init; } = EventStoreTestContainer.Instance;

        public RabbitMqTestContainer RabbitMqTestContainer { get; init; } = RabbitMqTestContainer.Instance;

        public ElasticSearchTestContainer ElasticSearchTestContainer { get; init; } = ElasticSearchTestContainer.Instance;

        public KeycloakTestContainer KeycloakTestContainer { get; init; } = KeycloakTestContainer.Instance;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            Task
                .WhenAll(
                    EventStoreTestContainer.Start(),
                    RabbitMqTestContainer.Start(),
                    ElasticSearchTestContainer.Start(),
                    KeycloakTestContainer.Start())
                .GetAwaiter()
                .GetResult();
            return base.CreateHost(builder
                .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "EventStore:ConnectionString", EventStoreTestContainer.ConnectionString },
                    { "RabbitMQ:ConnectionString", RabbitMqTestContainer.ConnectionString },
                    { "ElasticSearch:ConnectionString", ElasticSearchTestContainer.ConnectionString },
                    { "ElasticSearch:IndexName", "orders" },
                    { "Keycloak:Uri", KeycloakTestContainer.Uri.ToString() },
                    { "Keycloak:Authority", KeycloakTestContainer.Uri.ToString() },
                    { "Keycloak:Audience", KeycloakTestContainer.Audience }
                }))
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddNUnit()));
        }

        public override async ValueTask DisposeAsync()
        {
            await EventStoreTestContainer.DisposeAsync();
            await RabbitMqTestContainer.DisposeAsync();
            await ElasticSearchTestContainer.DisposeAsync();
            await KeycloakTestContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}