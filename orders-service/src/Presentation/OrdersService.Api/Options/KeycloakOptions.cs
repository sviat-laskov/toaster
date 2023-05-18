namespace OrdersService.Api.Options
{
    public class KeycloakOptions
    {
        public Uri Uri { get; init; } = null!;

        public Uri Authority { get; init; } = null!;

        public string Audience { get; init; } = null!;
    }
}