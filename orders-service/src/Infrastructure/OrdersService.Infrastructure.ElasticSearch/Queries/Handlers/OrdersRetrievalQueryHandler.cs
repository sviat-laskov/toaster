using MediatR;
using Nest;
using OrdersService.Application.Entities;
using OrdersService.Application.Queries;

namespace OrdersService.Infrastructure.ElasticSearch.Queries.Handlers;

public class OrdersRetrievalQueryHandler : IRequestHandler<OrdersRetrievalQuery, IEnumerable<OrderEntity>>
{
    private readonly IElasticClient _client;

    public OrdersRetrievalQueryHandler(IElasticClient client) => _client = client;

    public async Task<IEnumerable<OrderEntity>> Handle(OrdersRetrievalQuery query, CancellationToken cancellationToken)
    {
        Func<SearchDescriptor<OrderEntity>, ISearchRequest> searchDescriptor = query.FuzzySearchString is null
            ? search => search.MatchAll()
            : search => search.Query(searchQuery => searchQuery.MultiMatch(multiMatchQuery => multiMatchQuery
                .Query(query.FuzzySearchString)
                .Fuzziness(Fuzziness.Auto)
            ));
        ISearchResponse<OrderEntity>? response = await _client.SearchAsync(
            searchDescriptor,
            cancellationToken);

        return response.Documents;
    }
}