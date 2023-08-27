using CampusEats.Core.Delivering.Application.Pipelines;
using CampusEats.Core.Delivering.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Deliveries;

public sealed class DetailsModel : PageModel
{
    private readonly IMediator _mediator;

    public DetailsModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public DeliveryDetailsDto Delivery { get; private set; } = null!;
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync(Guid id)
    {
        var res = await _mediator.Send(new GetDeliveryDetails.Request(id));
        if(!res.Success)
        {
            Errors = res.Errors;
            return;
        }
        Delivery = res.Data;
    }

    public async Task<IActionResult> OnPostMarkNextPhaseAsync(Guid id)
    {
        var res = await _mediator.Send(new MarkOrderInNextPhase.Request(id));
        if(res.Success)
            return RedirectToPage();

        Errors = res.Errors;
        return Page();
    }
}
