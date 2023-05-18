namespace OrdersService.Application.Entities;

public class OrderEntity
{
    public Guid Id { get; set; }

    public bool IsConfirmed { get; set; }

    public string? UserNote { get; set; }

    public ISet<Guid> SubscriberIds { get; set; } = null!;

    public ulong Version { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset LastModifiedAt { get; set; }

    public Guid LastModifiedBy { get; set; }
}