namespace CampusEats.Core.Delivering.Domain.Dto;

public sealed record DeliveryDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Status Status { get; init; }
    public AddressDto Address { get; init; } = default!;
    public decimal Tip { get; init; }
    public decimal DeliveryFee { get; init; }
}
