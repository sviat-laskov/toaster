using MediatR;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands;

public class OrderSubscriptionAdditionCommand : IRequest<Order>
{
    public Guid OrderId { get; init; }
}