using System.ComponentModel.DataAnnotations;

namespace OrdersService.Infrastructure.ElasticSearch.Configuration;

public class ElasticSearchOptions
{
    [Required]
    public string ConnectionString { get; init; } = null!;

    [Required]
    public string IndexName { get; init; } = null!;
}