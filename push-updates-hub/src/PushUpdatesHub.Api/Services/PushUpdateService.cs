using Microsoft.AspNetCore.SignalR;
using PushUpdatesHub.Api.Services.Interfaces;
using PushUpdatesHub.Common.Constants;

namespace PushUpdatesHub.Api.Services
{
    public class PushUpdateService : IPushUpdateService
    {
        private readonly IHubContext<Hubs.PushUpdatesHub> _hubContext;

        public PushUpdateService(IHubContext<Hubs.PushUpdatesHub> hubContext) { _hubContext = hubContext; }

        public Task Send(
            string pushUpdateCode,
            Guid initiatedBy,
            IEnumerable<Guid> subscriberIds,
            object? payload = null,
            DateTimeOffset? occurredAt = null,
            CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients
                .Users(subscriberIds.Select(id => id.ToString()))
                .SendAsync(
                    PushUpdateConstants.ReceiveMethodName,
                    pushUpdateCode,
                    payload,
                    initiatedBy,
                    occurredAt ?? DateTimeOffset.Now,
                    cancellationToken);
        }
    }
}