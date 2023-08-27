using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Ordering.Application.Pipelines;

public sealed class GetSingleOrder
{
    public sealed record Request(Guid OrderId) : IRequest<GenericResponse<OrderDto>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<OrderDto>>
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
        public async Task<GenericResponse<OrderDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            if(!_currentUser.IsAuthenticated)
            {
                return new string[] { "User is not authenticated" };
            }
            var order = await _db.Orders
                .Include(x => x.OrderLines)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);

            if(order == null)
                return new string[] { "Order not found" };

            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            var isAuthorized = await _userManager.IsInRoleAsync(user, Roles.Admin.ToString());
            isAuthorized = isAuthorized || order.Customer.Email == _currentUser.Email;
            if(!isAuthorized)
            {
                return new string[] { "You are not authorized to view this order" };
            }

            return new OrderDto(order);
        }
    }
}