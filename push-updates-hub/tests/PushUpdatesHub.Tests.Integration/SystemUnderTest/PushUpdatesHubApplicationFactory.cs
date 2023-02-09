using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PushUpdatesHub.Api;
using PushUpdatesHub.Common.Constants;
using PushUpdatesHub.Tests.Integration.SystemUnderTest.Containers;
using PushUpdatesHub.Tests.Integration.SystemUnderTest.Models;

namespace PushUpdatesHub.Tests.Integration.SystemUnderTest
{
    internal class PushUpdatesHubApplicationFactory : WebApplicationFactory<Program>
    {
        public RabbitMqTestContainer RabbitMqTestContainer { get; init; } = RabbitMqTestContainer.Instance;

        public KeycloakTestContainer KeycloakTestContainer { get; init; } = KeycloakTestContainer.Instance;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            Task
                .WhenAll(RabbitMqTestContainer.Start(), KeycloakTestContainer.Start())
                .GetAwaiter()
                .GetResult();
            return base.CreateHost(builder
                .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "RabbitMQ:ConnectionString", RabbitMqTestContainer.ConnectionString },
                    { "Keycloak:Uri", KeycloakTestContainer.Uri.ToString() },
                    { "Keycloak:RealmId", KeycloakTestContainer.KeycloakRealmId },
                    { "Keycloak:Audience", KeycloakTestContainer.Audience }
                }))
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddNUnit()));
        }

        public HubConnection CreatePushUpdatesHubConnection(TestUser user)
        {
            return new HubConnectionBuilder()
                .WithUrl(new Uri(Server.BaseAddress, PushUpdateConstants.HubPath), options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.AccessTokenProvider = () => KeycloakTestContainer.GetAccessTokenViaDirectAccessGrant(user)!;
                    options.SkipNegotiation = true;
                    options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                    options.WebSocketFactory = async (context, cancellationToken) =>
                    {
                        WebSocketClient webSocketClient = Server.CreateWebSocketClient();
                        Uri uri = new UriBuilder(context.Uri) { Query = QueryString.Create(PushUpdateConstants.AccessTokenQueryParameterKey, (await options.AccessTokenProvider!())!).Value }.Uri;
                        WebSocket webSocket = await webSocketClient.ConnectAsync(uri, cancellationToken);

                        return webSocket;
                    };
                })
                .Build();
        }

        public override async ValueTask DisposeAsync()
        {
            await RabbitMqTestContainer.DisposeAsync();
            await KeycloakTestContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}