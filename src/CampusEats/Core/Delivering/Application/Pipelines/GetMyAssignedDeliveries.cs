using CampusEats.Core.Delivering.Domain.Dto;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Pipelines;

public sealed class GetMyAssignedDeliveries
{
    public sealed record Request() : IRequest<DeliveryDto[]>;

    public sealed class Handler : IRequestHandler<Request, DeliveryDto[]>
    {
        private readonly CampusEatsContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(CampusEatsContext db, ICurrentUser currentUser)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<DeliveryDto[]> Handle(Request request, CancellationToken cancellationToken)
        {
            if(!_currentUser.IsAuthenticated)
                return Array.Empty<DeliveryDto>();

            var deliveries = await _db.Deliveries
                .Where(d => d.Courier != null && d.Courier.UserId == _currentUser.UserId)
                .OrderBy(d => d.Status)
                .ProjectToType<DeliveryDto>()
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);

            return deliveries ?? Array.Empty<DeliveryDto>();
        }
    }
}
