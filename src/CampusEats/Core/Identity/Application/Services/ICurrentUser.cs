namespace CampusEats.Core.Identity.Application.Services;

public interface ICurrentUser
{
    public string UserId { get; }
    public string Email { get; }
    public bool IsAuthenticated { get; }
}
