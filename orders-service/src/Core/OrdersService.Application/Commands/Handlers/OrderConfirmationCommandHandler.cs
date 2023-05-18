using MediatR;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands.Handlers;

public class OrderConfirmationCommandHandler : IRequestHandler<OrderConfirmationCommand, Order>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IUserAccessor _userAccessor;

    public OrderConfirmationCommandHandler(IOrdersRepository ordersRepository, IUserAccessor userAccessor)
    {
        _ordersRepository = ordersRepository;
        _userAccessor = userAccessor;
    }

    public async Task<Order> Handle(OrderConfirmationCommand orderConfirmationCommand, CancellationToken cancellationToken)
    {
        Order? order = await _ordersRepository.Get(orderConfirmationCommand.OrderId, cancellationToken: cancellationToken);
        if (order is null) throw new IsNotFoundException(orderConfirmationCommand.OrderId);

        (Guid userId, _) = _userAccessor.Current;
        order.Confirm(userId);
        await _ordersRepository.Update(order, cancellationToken);

        return order;
    }
}