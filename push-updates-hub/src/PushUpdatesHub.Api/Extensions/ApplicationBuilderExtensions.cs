using EasyNetQ.AutoSubscribe;
using PushUpdatesHub.Api.IntegrationMessageHandlers;

namespace PushUpdatesHub.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static Task SubscribeToIntegrationMessages(this IApplicationBuilder app)
        {
            return app.ApplicationServices
                .GetRequiredService<AutoSubscriber>()
                .SubscribeAsync(new[] { typeof(PushUpdateIntegrationMessageHandler) });
        }
    }
}