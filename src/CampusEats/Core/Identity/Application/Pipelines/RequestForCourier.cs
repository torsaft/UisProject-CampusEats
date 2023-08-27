using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Identity.Application.Pipeline;

public sealed class RequestForCourier
{
    public sealed record Request(string UserId) : IRequest<IdentityResult>;

    public sealed class Handler : IRequestHandler<Request, IdentityResult>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IdentityResult> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            // request.ID er her Guid null, men man kan hente ut de andre verdiene...
            if(user is null) throw new EntityNotFoundException($"User with UserID {request.UserId} was not found in the database.");


            if(await _userManager.IsInRoleAsync(user, Roles.Courier.ToString()))
            {
                return new IdentityResult();
            }

            user.RequestStatus = RequestStatus.Pending;

            return await _userManager.UpdateAsync(user);
        }
    }
}

