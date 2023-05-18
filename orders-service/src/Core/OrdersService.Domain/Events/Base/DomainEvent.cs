using MediatR;

namespace OrdersService.Domain.Events.Base;

public abstract class DomainEvent : INotification
{
    // add version of aggregate here

    public Guid Id { get; } = Guid.NewGuid();

    public Guid AggregateId { get; }

    public abstract string TypeTitle { get; }

    public Guid InitiatedBy { get; }

    public DateTimeOffset OccurredAt { get; }

    protected DomainEvent(
        Guid aggregateId,
        Guid initiatedBy,
        DateTimeOffset? occurredAt = null)
    {
        AggregateId = aggregateId;
        InitiatedBy = initiatedBy;
        OccurredAt = occurredAt ?? DateTimeOffset.Now;
    }
}