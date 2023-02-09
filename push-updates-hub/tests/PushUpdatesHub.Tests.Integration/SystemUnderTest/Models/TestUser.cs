namespace PushUpdatesHub.Tests.Integration.SystemUnderTest.Models
{
    public class TestUser
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string Password { get; init; } = null!;
    }
}