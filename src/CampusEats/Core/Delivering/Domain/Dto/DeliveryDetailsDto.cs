using CampusEats.Core.Ordering.Domain;
using Mapster;

namespace CampusEats.Core.Delivering.Domain.Dto;

public sealed class DeliveryDetailsDto
{
    public DeliveryDetailsDto(Delivery delivery, Order order)
    {
        DeliveryId = delivery.Id;
        Status = delivery.Status;
        Address = order.Location.Adapt<AddressDto>();
        Tip = delivery.Tip;
        OrderStatus = order.Status.ToString();
        NextOrderStatus = order.Status switch
        {
            Ordering.Domain.Status.Accepted => Ordering.Domain.Status.Picked.ToString(),
            Ordering.Domain.Status.Picked => Ordering.Domain.Status.Delivered.ToString(),
            _ => null
        };
    }

    public Guid DeliveryId { get; init; }
    public Status Status { get; init; }
    public AddressDto Address { get; init; }
    public decimal Tip { get; init; }
    public decimal DeliveryFee { get; init; }
    public string OrderStatus { get; init; }
    public string? NextOrderStatus { get; init; }
}
