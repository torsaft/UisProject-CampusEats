using CampusEats.Core.Common;
using CampusEats.Core.Delivering.Domain;
using CampusEats.Core.Delivering.Domain.Dto;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Pipelines;

public sealed class GetUnassignedDeliveries
{
    public sealed record Request() : IRequest<GenericResponse<DeliveryDto[]>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<DeliveryDto[]>>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<GenericResponse<DeliveryDto[]>> Handle(Request request, CancellationToken cancellationToken)
        {
            var deliveries = await _db.Deliveries
                .Where(d => d.Status == Status.Unassigned)
                .ProjectToType<DeliveryDto>()
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);

            if(deliveries == null)
                return new[] { "No deliveries found" };

            return deliveries ?? Array.Empty<DeliveryDto>();
        }
    }
}
