using System.Security.Claims;
using OrdersService.Application.Services.Interfaces;

namespace OrdersService.Api.Services;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public (Guid Id, string Name) Current
    {
        get
        {
            ClaimsPrincipal claimsPrincipal = _httpContextAccessor.HttpContext!.User;
            return (Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)), claimsPrincipal.Identity!.Name!);
        }
    }
}