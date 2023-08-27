using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class ManageUsersModel : PageModel
{
    private readonly IMediator _mediator;
    public ManageUsersModel(IMediator mediator) => _mediator = mediator;

    public UserDto[] UserDtos { get; private set; } = Array.Empty<UserDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var res = await _mediator.Send(new GetUsers.Request());
        if(!res.Success)
            return Unauthorized();

        UserDtos = res.Data;
        return Page();
    }
}
