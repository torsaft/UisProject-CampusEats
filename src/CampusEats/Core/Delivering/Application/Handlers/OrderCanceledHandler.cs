using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Handlers;

public sealed class OrderCanceledHandler : INotificationHandler<OrderCanceled>
{
    private readonly CampusEatsContext _db;

    public OrderCanceledHandler(CampusEatsContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }
    public async Task Handle(OrderCanceled notification, CancellationToken cancellationToken)
    {
        var delivery = await _db.Deliveries
            .FirstOrDefaultAsync(x => x.OrderId == notification.OrderId, cancellationToken);

        if(delivery == default)
            throw new EntityNotFoundException("Can not cancel delivery for order that does not exist");

        delivery.Cancel();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
