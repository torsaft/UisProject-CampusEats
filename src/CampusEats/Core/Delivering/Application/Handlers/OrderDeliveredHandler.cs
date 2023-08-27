using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Handlers;

public sealed class OrderDeliveredHandler : INotificationHandler<OrderDelivered>
{
    private readonly CampusEatsContext _db;

    public OrderDeliveredHandler(CampusEatsContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(OrderDelivered notification, CancellationToken cancellationToken)
    {
        var delivery = await _db.Deliveries
            .FirstOrDefaultAsync(x => x.OrderId == notification.OrderId, cancellationToken);

        if(delivery is null)
            throw new EntityNotFoundException($"Delivery with order id {notification.OrderId} not found");

        delivery.Complete();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
