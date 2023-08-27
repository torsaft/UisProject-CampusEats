using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Handlers;

public sealed class TipSucceededHandler : INotificationHandler<TipSucceeded>
{
    private readonly CampusEatsContext _db;
    public TipSucceededHandler(CampusEatsContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }
    public async Task Handle(TipSucceeded notification, CancellationToken cancellationToken)
    {
        var session = notification.Session;
        var delivery = await _db.Deliveries
                .FirstOrDefaultAsync(x => x.OrderId.ToString() == session.Metadata["orderId"], cancellationToken);

        if(delivery is null)
            throw new EntityNotFoundException("Can not tip delivery for order that does not exist");

        delivery.TipCourier(Decimal.Parse(session.Metadata["tipAmount"]));
        await _db.SaveChangesAsync(cancellationToken);
    }
}