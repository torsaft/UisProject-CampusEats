using CampusEats.Core.Common;

namespace CampusEats.Core.Identity.Domain.Events;

public sealed record UserRegister : IDomainEvent
{
    public UserRegister(string email)
    {
        Email = email;
    }
    public string Email { get; private set; }
}
