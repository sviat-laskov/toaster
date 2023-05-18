namespace OrdersService.Application.Exceptions;

public class IsNotFoundException : ApplicationException
{
    public Guid AggregateId { get; }

    public IsNotFoundException(Guid aggregateId) => AggregateId = aggregateId;
}