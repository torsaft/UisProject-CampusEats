using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Core.Identity.Application.Pipeline;

public sealed class EditUserInfo
{
    public sealed record Request(string UserId, string FullName, string PhoneNumber) : IRequest<IdentityResult>;

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

            user.FullName = request.FullName ?? "";
            user.PhoneNumber = request.PhoneNumber ?? "";

            return await _userManager.UpdateAsync(user);
        }
    }
}
