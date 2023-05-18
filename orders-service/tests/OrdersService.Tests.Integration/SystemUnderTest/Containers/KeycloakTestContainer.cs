using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using OrdersService.Tests.Integration.SystemUnderTest.Models;

namespace OrdersService.Tests.Integration.SystemUnderTest.Containers
{
    internal class KeycloakTestContainer : IAsyncDisposable
    {
        private const ushort PrivatePort = 8080;
        public const string KeycloakRealmId = "toaster",
                            ClientId = "test",
                            Audience = "account";

        public static KeycloakTestContainer Instance => new();

        private TestcontainersContainer Container { get; } = new TestcontainersBuilder<TestcontainersContainer>()
            .WithName($"test-ordersservice-keycloak-{Guid.NewGuid()}")
            .WithImage("quay.io/keycloak/keycloak:20.0.2")
            .WithCommand("start-dev --import-realm")
            .WithResourceMapping($"./Data/{KeycloakRealmId}-realm.json", $"/opt/keycloak/data/import/{KeycloakRealmId}-realm.json")
            .WithExposedPort(PrivatePort)
            .WithPortBinding(PrivatePort, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PrivatePort))
            .Build();

        public Uri Uri => new UriBuilder(Uri.UriSchemeHttp, Container.Hostname, Container.GetMappedPublicPort(PrivatePort))
        {
            Path = $"/realms/{KeycloakRealmId}"
        }.Uri;

        public ValueTask DisposeAsync() => Container.DisposeAsync();

        public Task Start() => Container.StartAsync();

        public async Task<string> GetAccessTokenViaDirectAccessGrant(TestUser user)
        {
            Uri keycloakUri = Uri;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(keycloakUri, $"{keycloakUri.AbsolutePath}/protocol/openid-connect/token"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "client_id", ClientId },
                    { "username", user.Name },
                    { "password", user.Password }
                })
            };
            using var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadFromJsonAsync<JsonDocument>();
            string accessToken = responseContent!.RootElement.GetProperty("access_token").GetString()!;

            return accessToken;
        }
    }
}