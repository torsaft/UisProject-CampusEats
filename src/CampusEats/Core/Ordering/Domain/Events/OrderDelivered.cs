using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain.Events;

public sealed record OrderDelivered : IDomainEvent
{
    public OrderDelivered(Guid orderId, Customer customer)
    {
        CustomerEmail = customer.Email;
        OrderId = orderId;
    }

    public Guid OrderId { get; init; }
    public string CustomerEmail { get; init; }
}
