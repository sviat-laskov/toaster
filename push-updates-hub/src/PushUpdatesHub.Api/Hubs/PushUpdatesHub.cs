using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PushUpdatesHub.Api.Hubs
{
    [Authorize]
    public class PushUpdatesHub : Hub
    {
    }
}