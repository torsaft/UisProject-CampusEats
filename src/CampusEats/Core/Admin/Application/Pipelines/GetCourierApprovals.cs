using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class GetCourierApprovals
{
    public sealed record Request : IRequest<GenericResponse<UserDto[]>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<UserDto[]>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<GenericResponse<UserDto[]>> Handle(Request request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if(!roles.Contains(Roles.Admin.ToString()))
                {
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = _userManager.GetRolesAsync(user).Result,
                        RequestStatus = user.RequestStatus
                    });
                }
            }
            return userDtos.OrderBy(x => x.RequestStatus).ToArray();
        }
    }
}