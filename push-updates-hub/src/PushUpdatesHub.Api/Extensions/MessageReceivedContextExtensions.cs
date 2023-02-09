using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using PushUpdatesHub.Common.Constants;

namespace PushUpdatesHub.Api.Extensions
{
    public static class MessageReceivedContextExtensions
    {
        public static bool IsPushUpdateHubHandShake(this MessageReceivedContext context) { return context.HttpContext.Request.Path.StartsWithSegments(PushUpdateConstants.HubPath); }

        public static void SetAccessTokenFromQuery(this MessageReceivedContext context)
        {
            StringValues accessToken = context.Request.Query[PushUpdateConstants.AccessTokenQueryParameterKey];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
        }
    }
}