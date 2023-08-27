namespace CampusEats.Core.Delivering.Domain.Dto;

public sealed record AddressDto
{
    public string Building { get; init; } = null!;
    public string RoomNumber { get; init; } = null!;
    public string Notes { get; init; } = null!;
}