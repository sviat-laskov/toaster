using MediatR;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Commands.Handlers;

public class OrderCreationCommandHandler : IRequestHandler<OrderCreationCommand, Order>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IUserAccessor _userAccessor;

    public OrderCreationCommandHandler(
        IOrdersRepository ordersRepository,
        IUserAccessor userAccessor)
    {
        _ordersRepository = ordersRepository;
        _userAccessor = userAccessor;
    }

    /// <exception cref="AlreadyExistsException{TAggregate,TDomainEvent}" />
    public async Task<Order> Handle(OrderCreationCommand orderCreationCommand, CancellationToken cancellationToken)
    {
        (Guid userId, _) = _userAccessor.Current;
        var order = new Order(
            orderCreationCommand.OrderId,
            orderCreationCommand.UserNote,
            userId);

        await _ordersRepository.Add(order, cancellationToken);

        return order;
    }
}