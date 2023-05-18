using OrdersService.Tests.Integration.SystemUnderTest.Models;

namespace OrdersService.Tests.Integration.SystemUnderTest;

public static class TestConstants // Predefined at toaster realm.
{
    public static TestUser User1 = new()
    {
        Id = Guid.Parse("764cc427-7b40-460a-bad0-4648185b77d2"),
        Name = "user1",
        Password = "pass"
    };

    public static TestUser User2 = new()
    {
        Id = Guid.Parse("ec74e839-09ae-48a1-ae5c-b74505aae38e"),
        Name = "user1",
        Password = "pass"
    };

    public static TestUser Admin1 = new()
    {
        Id = Guid.Parse("174a7e51-2d54-4b07-90eb-67168a8dd519"),
        Name = "admin1",
        Password = "pass"
    };
}