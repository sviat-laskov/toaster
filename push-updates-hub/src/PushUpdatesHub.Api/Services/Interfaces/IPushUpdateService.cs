namespace PushUpdatesHub.Api.Services.Interfaces
{
    public interface IPushUpdateService
    {
        public Task Send(
            string pushUpdateCode,
            Guid initiatedBy,
            IEnumerable<Guid> subscriberIds,
            object? payload = null,
            DateTimeOffset? occurredAt = null,
            CancellationToken cancellationToken = default);
    }
}