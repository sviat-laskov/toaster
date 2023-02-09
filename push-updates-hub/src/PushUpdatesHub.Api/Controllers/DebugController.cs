using Microsoft.AspNetCore.Mvc;
using PushUpdatesHub.Api.Services.Interfaces;
using PushUpdatesHub.IntegrationMessages;

namespace PushUpdatesHub.Api.Controllers
{
    /// <summary>
    ///     Controller for debug purposes. Available only in development environment.
    /// </summary>
    [ApiController]
    [Route("debug/push-updates")]
    public class DebugController : ControllerBase
    {
        private readonly IPushUpdateService _pushUpdateService;

        public DebugController(IPushUpdateService pushUpdateService) { _pushUpdateService = pushUpdateService; }

        /// <summary>
        ///     Sends test push update. Emulates got integration message at message broker.
        /// </summary>
        [HttpPost]
        public Task SendTestPushUpdate([FromBody] PushUpdateIntegrationMessage integrationMessage)
        {
            return _pushUpdateService.Send(
                integrationMessage.Code,
                integrationMessage.InitiatedByUserId,
                integrationMessage.SubscriberIds,
                integrationMessage.Payload,
                integrationMessage.OccurredAt);
        }
    }
}