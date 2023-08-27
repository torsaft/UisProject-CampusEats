using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain;

public sealed class Customer : BaseEntity
{
    public Customer(string userId, string email)
    {
        UserId = userId;
        Email = email;
    }
    public string UserId { get; set; }
    public string Email { get; set; }
}