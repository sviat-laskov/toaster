using MediatR;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands;

public class OrderConfirmationCommand : IRequest<Order>
{
    public Guid OrderId { get; init; }
}