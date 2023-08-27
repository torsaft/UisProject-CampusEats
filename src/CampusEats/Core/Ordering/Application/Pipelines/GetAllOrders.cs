using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Ordering.Application.Pipelines;

public sealed class GetAllOrders
{
    public sealed record Request() : IRequest<List<Order>>;

    public sealed class Handler : IRequestHandler<Request, List<Order>>
    {
        private readonly CampusEatsContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly UserManager<AppUser> _userManager;

        public Handler(CampusEatsContext db, ICurrentUser currentUser, UserManager<AppUser> userManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public async Task<List<Order>> Handle(Request request, CancellationToken cancellationToken)
        {
            if(!_currentUser.IsAuthenticated)
            {
                return new();
            }

            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if(await _userManager.IsInRoleAsync(user, Roles.Admin.ToString()))
            {
                return await _db.Orders
                        .Include(x => x.OrderLines)
                        .Include(x => x.Customer)
                        .OrderBy(x => x.OrderDate)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
            }

            return await _db.Orders
                .Include(x => x.Customer)
                .Where(x => x.Customer.Email == user.Email)
                .Include(x => x.OrderLines)
                .OrderBy(x => x.OrderDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
