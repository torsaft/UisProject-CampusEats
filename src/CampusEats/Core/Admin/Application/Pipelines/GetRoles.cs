using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Admin.Application.Pipelines;


public sealed class GetRoles
{
    public sealed record Request : IRequest<IdentityRole[]>;

    public sealed class Handler : IRequestHandler<Request, IdentityRole[]>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public Handler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<IdentityRole[]> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _roleManager.Roles.ToArrayAsync();
        }
    }
}
