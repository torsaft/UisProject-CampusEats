using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Identity.Domain;

public sealed class AppUser : IdentityUser
{
    public string? FullName { get; set; }

    public RequestStatus? RequestStatus { get; set; }

    public bool FirstLogin { get; set; }
}
