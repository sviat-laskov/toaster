using OrdersService.Application.Exceptions;
using OrdersService.Domain.Models;

namespace OrdersService.Application.Repositories.Interfaces;

public interface IOrdersRepository
{
    public Task<Order?> Get(Guid id, long version = long.MaxValue - 1, CancellationToken cancellationToken = default);

    /// <exception cref="AlreadyExistsException{TAggregate, TDomainEvent}"></exception>
    public Task Add(Order order, CancellationToken cancellationToken = default);

    /// <exception cref="IsOutdatedException{TAggregate, TDomainEvent}"></exception>
    public Task Update(Order order, CancellationToken cancellationToken = default);
}