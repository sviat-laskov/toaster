namespace OrdersService.Api.ViewModels;

public class OrderVM
{
    public Guid Id { get; init; }

    public ulong Version { get; init; }

    public string? UserNote { get; init; }

    public bool IsUserSubscribed { get; init; }
}