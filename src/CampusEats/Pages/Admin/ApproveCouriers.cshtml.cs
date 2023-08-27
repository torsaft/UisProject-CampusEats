using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Identity.Domain.Dto;
using CampusEats.Core.Identity.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class ApproveCouriers : PageModel
{
    private readonly IMediator _mediator;
    public ApproveCouriers(IMediator mediator) => _mediator = mediator;
    public UserDto[] UserDtos { get; private set; } = Array.Empty<UserDto>();
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync()
    {
        var response = await _mediator.Send(new GetCourierApprovals.Request());
        if(response.Success)
        {
            UserDtos = response.Data;
        }
    }

    public async Task<IActionResult> OnPostAsync(string id, bool approve)
    {
        var response = await _mediator.Send(new ApproveCourier.Request(id, approve));
        if(!response.Success)
        {
            Errors = response.Errors;
            return Page();
        }

        CourierStatusDto user = response.Data;
        await _mediator.Publish(new CourierRequest(user.UserEmail, user.UserCourierStatus));
        return RedirectToPage("/Admin/ApproveCouriers");
    }
}
