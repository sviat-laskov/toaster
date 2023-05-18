using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models.Base;

namespace OrdersService.Application.Exceptions;

public class AlreadyExistsException<TAggregate, TDomainEvent> : ApplicationException 
    where TAggregate : Aggregate<TDomainEvent> 
    where TDomainEvent : DomainEvent
{
    public TAggregate Aggregate { get; }

    public AlreadyExistsException(TAggregate aggregate) => Aggregate = aggregate;
}