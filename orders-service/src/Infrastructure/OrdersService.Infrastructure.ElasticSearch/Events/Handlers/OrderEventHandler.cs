using AutoMapper;
using MediatR;
using Nest;
using OrdersService.Application.Entities;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models;

namespace OrdersService.Infrastructure.ElasticSearch.Events.Handlers;

public class OrderEventHandler<TOrderEvent> : INotificationHandler<TOrderEvent> where TOrderEvent : OrderEvent
{
    private readonly IElasticClient _client;
    private readonly IMapper _mapper;
    private readonly IOrdersRepository _ordersRepository;

    public OrderEventHandler(
        IElasticClient client,
        IOrdersRepository ordersRepository,
        IMapper mapper)
    {
        _client = client;
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task Handle(TOrderEvent orderEvent, CancellationToken cancellationToken)
    {
        Order order = (await _ordersRepository.Get(orderEvent.AggregateId, cancellationToken: cancellationToken))!;
        var orderEntity = _mapper.Map<OrderEntity>(order);
        Task clientAction =  orderEvent switch
        {
            OrderCreatedEvent => _client.IndexDocumentAsync(orderEntity, cancellationToken),
            _ => _client.UpdateAsync<OrderEntity>(orderEntity.Id, update => update.Doc(orderEntity), cancellationToken)
        };
        await clientAction;
    }
}