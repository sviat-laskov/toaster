using AutoMapper;
using EasyNetQ;
using MediatR;
using OrdersService.Application.PushUpdatePayloads;
using OrdersService.Application.Repositories.Interfaces;
using OrdersService.Domain.Events;
using OrdersService.Domain.Models;
using PushUpdatesHub.IntegrationMessages;

namespace OrdersService.Application.Events.Handlers;

public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedEvent>
{
    private readonly IBus _bus;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;

    public OrderConfirmedEventHandler(
        IBus bus,
        IOrdersRepository ordersRepository,
        IMapper mapper)
    {
        _bus = bus;
        _ordersRepository = ordersRepository;
        _mapper = mapper;
    }

    public async Task Handle(OrderConfirmedEvent orderConfirmedEvent, CancellationToken cancellationToken)
    {
        Order? order = await _ordersRepository.Get(orderConfirmedEvent.AggregateId, cancellationToken: cancellationToken);

        var pushUpdateIntegrationMessage = new PushUpdateIntegrationMessage
        {
            Code = orderConfirmedEvent.TypeTitle,
            InitiatedByUserId = orderConfirmedEvent.InitiatedBy,
            SubscriberIds = order!.SubscriberIds,
            Payload = _mapper.Map<OrderConfirmedPushUpdatePayload>(orderConfirmedEvent),
            OccurredAt = DateTimeOffset.UtcNow
        };

        await _bus.PubSub.PublishAsync(pushUpdateIntegrationMessage, cancellationToken: cancellationToken);
    }
}