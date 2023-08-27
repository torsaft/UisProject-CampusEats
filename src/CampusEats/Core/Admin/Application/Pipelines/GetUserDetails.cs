using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class GetUserDetails
{
    public sealed record Request(string Id) : IRequest<GenericResponse<UserDto>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<UserDto>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GenericResponse<UserDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if(user == null)
                return new[] { "User not found" };

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Roles = _userManager.GetRolesAsync(user).Result,
                CanBecomeAdmin = user.PasswordHash != null
            };
            return userDto;
        }
    }
}

