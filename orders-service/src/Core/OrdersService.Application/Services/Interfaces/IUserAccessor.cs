namespace OrdersService.Application.Services.Interfaces;

public interface IUserAccessor
{
    public (Guid Id, string Name) Current { get; }
}