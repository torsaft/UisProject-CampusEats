using CampusEats.Core.Common;

namespace CampusEats.Core.Delivering.Domain.Events;

public sealed record DeliveryAssigned : IDomainEvent
{
    public DeliveryAssigned(Guid orderId, string userId, string email)
    {
        CourierEmail = email;
        OrderId = orderId;
        UserId = userId;
    }

    public Guid OrderId { get; }
    public string UserId { get; init; }
    public string CourierEmail { get; init; }
}
