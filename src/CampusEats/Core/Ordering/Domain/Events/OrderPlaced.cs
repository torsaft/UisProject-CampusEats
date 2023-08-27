using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Dto;
using Mapster;

namespace CampusEats.Core.Ordering.Domain.Events;

public sealed record OrderPlaced : IDomainEvent
{
    public OrderPlaced(Guid orderId, Location location, decimal deliveryFee)
    {
        OrderId = orderId;
        DeliveryFee = deliveryFee;
        Location = location.Adapt<LocationDto>();
    }
    public Guid OrderId { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public LocationDto Location { get; private set; }
}
