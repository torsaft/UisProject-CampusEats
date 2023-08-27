using CampusEats.Core.Delivering.Domain;
using CampusEats.Core.Ordering.Domain.Events;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;

namespace CampusEats.Core.Delivering.Handlers;

public sealed class OrderPlacedHandler : INotificationHandler<OrderPlaced>
{
    private readonly CampusEatsContext _db;

    public OrderPlacedHandler(CampusEatsContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        var delivery = new Delivery(notification.OrderId, notification.DeliveryFee, notification.Location.Adapt<Address>());
        _db.Deliveries.Add(delivery);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

