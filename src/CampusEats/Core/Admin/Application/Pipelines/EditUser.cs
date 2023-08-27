using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class EditUser
{
    public sealed record Request(
        string Id,
        string Role,
        string FullName,
        string PhoneNumber
        ) : IRequest<GenericResponse<Unit>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<Unit>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager, ICurrentUser currentUser)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GenericResponse<Unit>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if(user is null)
                return new[] { $"User with Id {request.Id} was not found." };

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;

            var userRoles = await _userManager.GetRolesAsync(user);
            if(!userRoles.Contains(request.Role))
            {
                if(request.Role == Roles.Customer.ToString())
                {
                    user.RequestStatus = RequestStatus.Declined;
                }

                if(request.Role == Roles.Courier.ToString())
                {
                    user.RequestStatus = RequestStatus.Confirmed;
                }

                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, request.Role);
            }
            await _userManager.UpdateAsync(user);
            return Unit.Value;
        }
    }
}
