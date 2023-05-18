using MediatR;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands.Handlers;

public class OrderSubscriptionAdditionCommandHandler : IRequestHandler<OrderSubscriptionAdditionCommand, Order>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IUserAccessor _userAccessor;

    public OrderSubscriptionAdditionCommandHandler(IOrdersRepository ordersRepository, IUserAccessor userAccessor)
    {
        _ordersRepository = ordersRepository;
        _userAccessor = userAccessor;
    }

    public async Task<Order> Handle(OrderSubscriptionAdditionCommand orderSubscriptionAdditionCommand, CancellationToken cancellationToken)
    {
        Order? order = await _ordersRepository.Get(orderSubscriptionAdditionCommand.OrderId, cancellationToken: cancellationToken);
        if (order is null) throw new IsNotFoundException(orderSubscriptionAdditionCommand.OrderId);

        (Guid userId, _) = _userAccessor.Current;
        order.AddSubscription(userId);
        await _ordersRepository.Update(order, cancellationToken);

        return order;
    }
}