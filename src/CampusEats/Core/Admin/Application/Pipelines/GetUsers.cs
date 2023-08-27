using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class GetUsers
{
    public sealed record Request : IRequest<GenericResponse<UserDto[]>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<UserDto[]>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUser _currentUser;

        public Handler(UserManager<AppUser> userManager, ICurrentUser currentUser)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _currentUser = currentUser;
        }

        public async Task<GenericResponse<UserDto[]>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId);
            if(user == null || !await _userManager.IsInRoleAsync(user, Roles.Admin.ToString()))
            {
                return new[] { "Unauthorized" };
            }

            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach(var u in users)
            {
                userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }
            return userDtos.ToArray();
        }
    }
}