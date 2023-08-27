using CampusEats.Core.Common;

namespace CampusEats.Core.Delivering.Domain;

public sealed class Courier : BaseEntity
{
    public Courier(string userId, string email)
    {
        UserId = userId;
        Email = email;
    }

    public string UserId { get; private set; }
    public string Email { get; private set; }
}
