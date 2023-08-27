using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Application.Services;
using CampusEats.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Pipelines;

public sealed class MarkOrderInNextPhase
{
    public sealed record Request(Guid DeliveryId) : IRequest<GenericResponse<Unit>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<Unit>>
    {
        private readonly IOrderingService _orderingService;
        private readonly CampusEatsContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(IOrderingService orderingService, CampusEatsContext db, ICurrentUser currentUser)
        {
            _orderingService = orderingService ?? throw new ArgumentNullException(nameof(orderingService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser;
        }
        public async Task<GenericResponse<Unit>> Handle(Request request, CancellationToken cancellationToken)
        {
            var delivery = await _db.Deliveries
                .Include(x => x.Courier)
                .Where(d => d.Id == request.DeliveryId && d.Courier != null)
                .Select(d => new { d.OrderId, d.Courier!.UserId })
                .FirstOrDefaultAsync(cancellationToken);

            if(delivery == null)
                return new[] { "Delivery not found" };

            if(delivery.UserId != _currentUser.UserId)
                return new[] { "You are not authorized to perform this action" };

            if(delivery.OrderId == Guid.Empty)
                return new[] { $"Order for given delivery does not exist" };

            await _orderingService.UpdateOrderDeliveryStatus(delivery.OrderId);
            return Unit.Value;
        }
    }
}
