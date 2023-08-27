namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record LocationDto
{
    public string Building { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

}
