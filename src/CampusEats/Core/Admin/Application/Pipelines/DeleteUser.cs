using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class DeleteUser
{
    public sealed record Request(Guid Id) : IRequest<GenericResponse<Unit>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<Unit>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        => _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        public async Task<GenericResponse<Unit>> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if(user is null)
                return new[] { $"User with UserId {request.Id} was not found in the database" };
            await _userManager.DeleteAsync(user);
            return Unit.Value;
        }
    }
}
