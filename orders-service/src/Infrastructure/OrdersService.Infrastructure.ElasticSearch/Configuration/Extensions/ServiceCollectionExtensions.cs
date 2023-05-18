using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;
using OrdersService.Domain.Models;
using OrdersService.Infrastructure.ElasticSearch.Queries.Handlers;

namespace OrdersService.Infrastructure.ElasticSearch.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureElasticSearch(this IServiceCollection services)
    {
        services
            .AddOptions<ElasticSearchOptions>()
            .BindConfiguration("ElasticSearch")
            .ValidateDataAnnotations();

        services
            .AddSingleton(serviceProvider =>
            {
                ElasticSearchOptions options = serviceProvider.GetRequiredService<IOptions<ElasticSearchOptions>>().Value;
                return new ConnectionSettings(new Uri(options.ConnectionString))
                    .PrettyJson()
                    .DefaultIndex(options.IndexName)
                    .DefaultMappingFor<Order>(selector => selector
                        .IdProperty(order => order.Id)
                        .IndexName(options.IndexName));
            })
            .AddSingleton<IElasticClient, ElasticClient>(serviceProvider => new ElasticClient(serviceProvider.GetRequiredService<ConnectionSettings>()));

        services
            .AddAutoMapper(typeof(MapperProfile))
            .AddMediatR(typeof(OrdersRetrievalQueryHandler));

        return services;
    }
}