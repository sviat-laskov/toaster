using MediatR;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands.Handlers;

public class OrderSubscriptionRemovalCommandHandler : IRequestHandler<OrderSubscriptionRemovalCommand, Order>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IUserAccessor _userAccessor;

    public OrderSubscriptionRemovalCommandHandler(IOrdersRepository ordersRepository, IUserAccessor userAccessor)
    {
        _ordersRepository = ordersRepository;
        _userAccessor = userAccessor;
    }

    public async Task<Order> Handle(OrderSubscriptionRemovalCommand orderSubscriptionRemovalCommand, CancellationToken cancellationToken)
    {
        Order? order = await _ordersRepository.Get(orderSubscriptionRemovalCommand.OrderId, cancellationToken: cancellationToken);
        if (order is null) throw new IsNotFoundException(orderSubscriptionRemovalCommand.OrderId);

        (Guid userId, _) = _userAccessor.Current;
        order.RemoveSubscription(userId);
        await _ordersRepository.Update(order, cancellationToken);

        return order;
    }
}