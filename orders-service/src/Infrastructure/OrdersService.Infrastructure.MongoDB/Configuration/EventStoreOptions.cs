using System.ComponentModel.DataAnnotations;

namespace OrdersService.Infrastructure.EventStoreDB.Configuration;

public class EventStoreOptions
{
    [Required]
    public string ConnectionString { get; init; } = null!;
}