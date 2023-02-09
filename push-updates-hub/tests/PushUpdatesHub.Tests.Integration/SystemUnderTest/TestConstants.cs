using PushUpdatesHub.Tests.Integration.SystemUnderTest.Models;

namespace PushUpdatesHub.Tests.Integration.SystemUnderTest
{
    public static class TestConstants
    {
        public static TestUser User1 = new() // Predefined at toaster realm.
        {
            Id = Guid.Parse("5168b6b2-693c-43fe-9a2b-01b7251961c3"),
            Name = "user1",
            Password = "pass"
        };
    }
}