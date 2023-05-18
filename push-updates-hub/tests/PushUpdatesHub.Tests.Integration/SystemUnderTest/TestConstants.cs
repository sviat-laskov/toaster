using PushUpdatesHub.Tests.Integration.SystemUnderTest.Models;

namespace PushUpdatesHub.Tests.Integration.SystemUnderTest
{
    public static class TestConstants
    {
        public static TestUser User1 = new() // Predefined at toaster realm.
        {
            Id = Guid.Parse("764cc427-7b40-460a-bad0-4648185b77d2"),
            Name = "user1",
            Password = "pass"
        };
    }
}