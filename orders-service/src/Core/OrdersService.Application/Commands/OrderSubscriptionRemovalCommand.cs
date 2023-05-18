using MediatR;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands;

public class OrderSubscriptionRemovalCommand : IRequest<Order>
{
    public Guid OrderId { get; init; }
}