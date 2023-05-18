namespace OrdersService.Api.ViewModels;

public record OrderCreationVM
{
    public Guid? OrderId { get; init; }

    /// <example>Please, don't call me.</example>
    public string? UserNote { get; init; }
}