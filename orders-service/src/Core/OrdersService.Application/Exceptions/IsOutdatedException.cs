using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models.Base;

namespace OrdersService.Application.Exceptions;

public class IsOutdatedException<TAggregate, TDomainEvent> : ApplicationException
    where TAggregate : Aggregate<TDomainEvent>
    where TDomainEvent : DomainEvent
{
    public TAggregate Aggregate { get; }

    public ulong CurrentVersion { get; }

    public IsOutdatedException(TAggregate aggregate, ulong currentVersion)
    {
        Aggregate = aggregate;
        CurrentVersion = currentVersion;
    }
}