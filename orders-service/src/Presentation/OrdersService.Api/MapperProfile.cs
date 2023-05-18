using AutoMapper;
using OrdersService.Api.ViewModels;
using OrdersService.Application.Commands;
using OrdersService.Application.Entities;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Domain.Models;

namespace OrdersService.Api;

public class MapperProfile : Profile
{
    private readonly IUserAccessor _userAccessor;

    public MapperProfile(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;

        CreateMap<OrderEntity, OrderVM>()
            .ForMember(dest => dest.IsUserSubscribed, options => options.MapFrom(src => src.SubscriberIds.Contains(_userAccessor.Current.Id)));
        CreateMap<Order, OrderVM>()
            .ForMember(dest => dest.IsUserSubscribed, options => options.MapFrom(src => src.SubscriberIds.Contains(_userAccessor.Current.Id)));
        CreateMap<OrderCreationVM, OrderCreationCommand>();
    }
}