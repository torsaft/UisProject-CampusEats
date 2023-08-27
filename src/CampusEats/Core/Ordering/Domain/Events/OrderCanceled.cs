using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain.Events;

public class OrderCanceled : IDomainEvent
{
    public OrderCanceled(Guid orderId, decimal sum, Customer customer, Status status)
    {
        OrderId = orderId;
        Sum = sum;
        CustomerEmail = customer.Email;
        Status = status;
    }
    public Guid OrderId { get; init; }
    public decimal Sum { get; init; }
    public string CustomerEmail { get; init; }
    public Status Status { get; init; }
}
