using System.Security.Claims;

namespace CampusEats.Core.Identity.Application.Services;

public sealed class CurrentUser : ICurrentUser
{
    public CurrentUser(IHttpContextAccessor accessor)
    {
        UserId = accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Email = accessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        IsAuthenticated = accessor.HttpContext!.User.Identity?.IsAuthenticated ?? false;
    }
    public string UserId { get; init; }
    public string Email { get; init; }
    public bool IsAuthenticated { get; init; }
}
