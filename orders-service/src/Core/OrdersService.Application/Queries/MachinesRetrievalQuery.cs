using MediatR;
using OrdersService.Application.Entities;

namespace OrdersService.Application.Queries;

public class OrdersRetrievalQuery : IRequest<IEnumerable<OrderEntity>>
{
    public string? FuzzySearchString { get; init; }
}