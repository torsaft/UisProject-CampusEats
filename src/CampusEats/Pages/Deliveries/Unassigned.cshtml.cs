using CampusEats.Core.Delivering.Application.Pipelines;
using CampusEats.Core.Delivering.Domain.Dto;
using CampusEats.Core.Delivering.Pipelines;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Deliveries;

public sealed class UnassignedModel : PageModel
{
    private readonly IMediator _mediator;
    public UnassignedModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public DeliveryDto[] Deliveries { get; private set; } = Array.Empty<DeliveryDto>();
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync()
    {
        var res = await _mediator.Send(new GetUnassignedDeliveries.Request());
        if(res.Success)
        {
            Deliveries = res.Data;
        }
    }

    public async Task<IActionResult> OnPostAssignSelfAsync(Guid orderId)
    {
        var res = await _mediator.Send(new AssignCourier.Request(orderId));
        if(res.Success)
            return RedirectToPage();

        Errors = res.Errors;
        return Page();
    }
}
