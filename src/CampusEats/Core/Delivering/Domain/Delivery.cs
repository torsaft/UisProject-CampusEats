using CampusEats.Core.Common;
using CampusEats.Core.Delivering.Domain.Events;

namespace CampusEats.Core.Delivering.Domain;

public sealed class Delivery : BaseEntity
{
    private Delivery()
    {
    }

    public Delivery(Guid orderId, decimal fee, Address address)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Fee = fee;
        Address = address;
        Status = Status.Unassigned;
        Tip = 0;
    }

    public Guid OrderId { get; private set; }
    public Status Status { get; private set; }
    public Courier? Courier { get; private set; } = null;
    public Address Address { get; private set; } = default!;
    public decimal Tip { get; private set; }
    public decimal Fee { get; private set; }

    public void AssignCourier(string userId, string email)
    {
        if(Status != Status.Unassigned) return;

        Courier = new Courier(userId, email);
        Status = Status.Assigned;
        AddEvent(new DeliveryAssigned(OrderId, userId, email));
    }

    public void Cancel()
    {
        if(Status == Status.Canceled || Status == Status.Delivered) return;
        Status = Status.Canceled;
    }

    public void Complete()
    {
        if(Status != Status.Assigned) return;
        Status = Status.Delivered;
    }

    public void TipCourier(decimal tip)
    {
        if(Tip > 0 || tip <= 0) return;
        Tip = tip;
    }
}
