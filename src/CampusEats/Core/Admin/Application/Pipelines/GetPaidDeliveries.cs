using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class GetPaidDeliveries
{
    public sealed record Request() : IRequest<GenericResponse<OrderExpenseDto[]>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<OrderExpenseDto[]>>
    {
        private readonly CampusEatsContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(CampusEatsContext db, ICurrentUser currentUser)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser;
        }
        public async Task<GenericResponse<OrderExpenseDto[]>> Handle(Request request, CancellationToken cancellationToken)
        {
            if(!_currentUser.IsAuthenticated)
            {
                return new[] { "You are not authenticated" };
            }

            var orders = await _db.Orders
                .Where(o => o.Status == Status.Delivered || o.Status == Status.Canceled)
                .Include(x => x.OrderLines)
                .Include(x => x.Customer)
                .OrderBy(x => x.OrderDate)
                .Select(x => new OrderExpenseDto(x))
                .ToArrayAsync();

            return orders ?? Array.Empty<OrderExpenseDto>();
        }
    }
}
