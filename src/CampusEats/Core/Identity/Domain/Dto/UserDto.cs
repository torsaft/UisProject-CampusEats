namespace CampusEats.Core.Identity.Domain.Dto;

public sealed record UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string>? Roles { get; set; }
    public bool CanBecomeAdmin { get; set; } = false;
    public RequestStatus? RequestStatus { get; set; }
}
