using EasyNetQ.AutoSubscribe;
using PushUpdatesHub.Api.Services.Interfaces;
using PushUpdatesHub.IntegrationMessages;

namespace PushUpdatesHub.Api.IntegrationMessageHandlers
{
    public sealed class PushUpdateIntegrationMessageHandler : IConsumeAsync<PushUpdateIntegrationMessage>
    {
        private readonly IPushUpdateService _pushUpdateService;

        public PushUpdateIntegrationMessageHandler(IPushUpdateService pushUpdateService) { _pushUpdateService = pushUpdateService; }

        public Task ConsumeAsync(PushUpdateIntegrationMessage message, CancellationToken cancellationToken)
        {
            return _pushUpdateService.Send(
                message.Code,
                message.InitiatedByUserId,
                message.SubscriberIds,
                message.Payload,
                message.OccurredAt,
                cancellationToken);
        }
    }
}