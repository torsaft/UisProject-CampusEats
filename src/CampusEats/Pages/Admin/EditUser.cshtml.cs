using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class EditUserModel : PageModel
{
    private readonly IMediator _mediator;
    public EditUserModel(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public UserDto UserDto { get; set; } = new();

    [BindProperty]
    public IList<IdentityRole> AllRoles { get; set; } = default!;

    [BindProperty]
    public string CurRole { get; set; } = default!;

    [BindProperty]
    public bool CanBecomeAdmin { get; set; } = false;

    [BindProperty]
    public string FullName { get; set; } = default!;

    [BindProperty]
    public string PhoneNumber { get; set; } = default!;
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var res = await _mediator.Send(new GetUserDetails.Request(id));
        if(!res.Success)
        {
            return NotFound();
        }

        UserDto = res.Data;
        AllRoles = await _mediator.Send(new GetRoles.Request());

        if(UserDto.Roles != null && UserDto.Roles.Any())
        {
            CurRole = UserDto.Roles[0];
        }
        else
        {
            CurRole = Roles.Customer.ToString();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {

        var response = await _mediator.Send(new EditUser.Request(UserDto.Id, CurRole, FullName, PhoneNumber));
        if(response.Success)
            return RedirectToPage();

        Errors = response.Errors;
        return Page();
    }
}