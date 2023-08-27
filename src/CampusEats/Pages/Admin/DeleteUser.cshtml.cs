using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class DeleteModel : PageModel
{
    private readonly IMediator _mediator;

    public DeleteModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [BindProperty]
    public UserDto UserDto { get; private set; } = default!;
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var res = await _mediator.Send(new GetUserDetails.Request(id));
        if(!res.Success)
            return NotFound();

        UserDto = res.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var res = await _mediator.Send(new DeleteUser.Request(id));
        if(res.Success)
            return RedirectToPage("ManageUsers");

        Errors = res.Errors;
        return Page();
    }
}
