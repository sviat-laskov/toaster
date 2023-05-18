using AutoMapper;
using OrdersService.Application.Entities;
using OrdersService.Domain.Models;

namespace OrdersService.Infrastructure.ElasticSearch;

public class MapperProfile : Profile
{
    public MapperProfile() => CreateMap<Order, OrderEntity>()
        .ReverseMap();
}