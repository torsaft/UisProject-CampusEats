using CampusEats.Core.Ordering.Application.Pipelines;
using CampusEats.Core.Ordering.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Orders;

public sealed class SingleOrderModel : PageModel
{
	private readonly IMediator _mediator;
	public SingleOrderModel(IMediator mediator) => _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));

	public OrderDto? OrderDto { get; private set; }

	[BindProperty]
	public decimal TipAmount { get; set; }
	public string[] Errors { get; private set; } = Array.Empty<string>();

	public async Task<IActionResult> OnGetAsync(Guid Id)
	{
		var res = await _mediator.Send(new GetSingleOrder.Request(Id));
		if(!res.Success)
			return RedirectToPage("/Index");

		OrderDto = res.Data;
		return Page();
	}

	public async Task<IActionResult> OnPostCancelAsync(Guid Id)
	{
		var res = await _mediator.Send(new CancelOrder.Request(Id));
		if(res.Success)
			return RedirectToPage("/Orders/Index");

		Errors = res.Errors;
		return Page();
	}
	public async Task<IActionResult> OnPostTipAsync(Guid Id)
	{
		var res = await _mediator.Send(new TipCourier.Request(Id, TipAmount));
		return Redirect(res.Session.Url);
	}
}