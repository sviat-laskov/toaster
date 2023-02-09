namespace PushUpdatesHub.Api.Options
{
    public class KeycloakOptions
    {
        public Uri Uri { get; init; } = null!;

        public string RealmId { get; init; } = null!;

        public string Audience { get; init; } = null!;

        public Uri Authority => new(Uri, $"realms/{RealmId}");
    }
}