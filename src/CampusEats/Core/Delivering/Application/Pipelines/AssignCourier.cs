using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Pipelines;

public sealed class AssignCourier
{
    public sealed record Request(Guid OrderId) : IRequest<GenericResponse<Unit>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<Unit>>
    {
        private readonly CampusEatsContext _db;

        private readonly ICurrentUser _currentUser;

        public Handler(CampusEatsContext db, ICurrentUser currentUser)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<GenericResponse<Unit>> Handle(Request request, CancellationToken cancellationToken)
        {
            var delivery = await _db.Deliveries
                .Include(d => d.Courier)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if(delivery == null)
                return new[] { $"No delivery with {request.OrderId} exists" };

            delivery.AssignCourier(_currentUser.UserId, _currentUser.Email);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
