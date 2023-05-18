using AutoMapper;
using OrdersService.Application.PushUpdatePayloads;
using OrdersService.Domain.Events;

namespace OrdersService.Application;

public class MapperProfile : Profile
{
    public MapperProfile() => CreateMap<OrderConfirmedEvent, OrderConfirmedPushUpdatePayload>()
        .ForMember(payload => payload.OrderId, config => config.MapFrom(src => src.AggregateId));
}