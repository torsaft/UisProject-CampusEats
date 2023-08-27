using CampusEats.Core.Delivering.Domain;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Delivering.Application.Pipelines;

public sealed class GetCourierDoneDeliveries
{
    public sealed record Request() : IRequest<(int, decimal, decimal)>;

    public sealed class Handler : IRequestHandler<Request, (int, decimal, decimal)>
    {
        private readonly CampusEatsContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(CampusEatsContext db, ICurrentUser currentUser)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<(int, decimal, decimal)> Handle(Request request, CancellationToken cancellationToken)
        {
            var deliveries = await _db.Deliveries
                .Include(d => d.Courier)
                .Where(d => d.Courier != null && d.Courier.Email == _currentUser.Email)
                .Where(d => d.Status == Status.Delivered || d.Status == Status.Canceled) // Filter based on courier
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            decimal earnings = 0;
            decimal tip = 0;

            foreach (var delivery in deliveries)
            {
                earnings += delivery.Fee * 0.8m;
                tip += delivery.Tip;
            }

            return (deliveries.Count, earnings, tip);
        }
    }
}