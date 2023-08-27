using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain.Events;

public class OrderPicked : IDomainEvent
{
    public OrderPicked(Guid orderId, Customer customer)
    {
        OrderId = orderId;
        CustomerEmail = customer.Email;
    }
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; }
}
