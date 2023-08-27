using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class ApproveCourier
{
    public sealed record Request(string Id, bool Approve) : IRequest<GenericResponse<CourierStatusDto>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<CourierStatusDto>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GenericResponse<CourierStatusDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var CourierRole = Roles.Courier.ToString();
            var CustomerRole = Roles.Customer.ToString();
            var user = await _userManager.FindByIdAsync(request.Id);
            if(user is null)
                return new[] { $"User with Id {request.Id} was not found." };

            var userRoles = await _userManager.GetRolesAsync(user);
            if(request.Approve)
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                user.RequestStatus = RequestStatus.Confirmed;
                await _userManager.AddToRoleAsync(user, CourierRole);
            }
            else
            {
                user.RequestStatus = RequestStatus.Declined;
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, CustomerRole); ;
            }

            await _userManager.UpdateAsync(user);
            return new CourierStatusDto(user); ;
        }
    }
}
