using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Events;

namespace CampusEats.Core.Ordering.Domain;

public sealed class Order : BaseEntity
{
    public static decimal CurrentDeliveryFee { get; set; } = 100;
    private Order()
    {
    }
    public Order(Location location, Customer customer, string stripePaymentId)
    {
        Id = Guid.NewGuid();
        OrderDate = DateTime.UtcNow;
        Location = location;
        Customer = customer;
        StripePaymentId = stripePaymentId;
        DeliveryFee = CurrentDeliveryFee;
    }

    private readonly List<OrderLine> _orderLines = new();
    public IEnumerable<OrderLine> OrderLines => _orderLines.AsReadOnly();

    public Location Location { get; private set; } = default!;
    public Customer Customer { get; private set; } = default!;
    public Status Status { get; private set; } = Status.New;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string StripePaymentId { get; private set; } = null!;
    public decimal DeliveryFee { get; private set; }
    public bool IsActive => Status == Status.New || Status == Status.Accepted || Status == Status.Placed;

    public void AddOrderLine(Guid productId, decimal price, int amount, string name)
    {
        var orderLine = new OrderLine(productId, price, amount, name);
        _orderLines.Add(orderLine);
    }
    public void Place()
    {
        if(Status != Status.New) return;
        Status = Status.Placed;
        AddEvent(new OrderPlaced(Id, Location, DeliveryFee));
    }

    public decimal Cancel()
    {
        if(!IsActive) return 0;

        var refundAmount = OrderLines.Sum(x => x.Sum);
        if(Status != Status.Accepted)
        {
            refundAmount += DeliveryFee;
        }
        Status = Status.Canceled;
        AddEvent(new OrderCanceled(Id, refundAmount, Customer, Status));
        return refundAmount;
    }

    public void MoveToNextPhase()
    {
        var newStatus = Status switch
        {
            Status.Placed => Status.Accepted,
            Status.Accepted => Status.Picked,
            Status.Picked => Status.Delivered,
            _ => Status
        };

        if(newStatus == Status) return;
        Status = newStatus;

        if(Status == Status.Picked)
            AddEvent(new OrderPicked(Id, Customer));

        if(Status == Status.Delivered)
            AddEvent(new OrderDelivered(Id, Customer));
    }
}
