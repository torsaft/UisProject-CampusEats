namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record CustomerDto
{
    public CustomerDto(string userId, string email)
    {
        UserId = userId;
        Email = email;
    }
    public string UserId { get; init; }
    public string Email { get; init; }
}
