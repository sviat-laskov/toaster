using MediatR;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands;

public class OrderCreationCommand : IRequest<Order>
{
    public Guid OrderId { get; init; }

    /// <example>Please, don't call me.</example>
    public string? UserNote { get; init; } = null!;
}