using CampusEats.Core.Common;
using CampusEats.Core.Delivering.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Pipelines;

public sealed class GetDeliveryDetails
{
    public record Request(Guid DeliveryId) : IRequest<GenericResponse<DeliveryDetailsDto>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<DeliveryDetailsDto>>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<GenericResponse<DeliveryDetailsDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var delivery = await _db.Deliveries
                .Include(d => d.Courier)
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);

            if(delivery == null)
                return new[] { $"Delivery with id {request.DeliveryId} not found" };

            var order = await _db.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == delivery.OrderId, cancellationToken);

            if(order == null)
                return new[] { $"Order with id {delivery.OrderId} not found" };

            return new DeliveryDetailsDto(delivery, order);
        }
    }
}
