using CampusEats.Core.Common;
using CampusEats.Core.Delivering.Domain.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Ordering.Application.Handlers;

public sealed class DeliveryAssignedHandler : INotificationHandler<DeliveryAssigned>
{
    private readonly CampusEatsContext _db;

    public DeliveryAssignedHandler(CampusEatsContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(DeliveryAssigned notification, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);
        if(order == null)
        {
            throw new EntityNotFoundException($"Order with id {notification.OrderId} not found");
        }

        order.MoveToNextPhase();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
