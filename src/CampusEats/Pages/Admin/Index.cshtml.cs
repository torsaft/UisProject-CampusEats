using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Ordering.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class IndexModel : PageModel
{
	private readonly IMediator _mediator;
	public IndexModel(IMediator mediator)
	{
		_mediator = mediator;
	}

	[BindProperty]
	public decimal DeliveryFee { get; set; }

	public void OnGet()
	{
		DeliveryFee = Order.CurrentDeliveryFee;
	}
	public async Task<IActionResult> OnPostAsync()
	{
		await _mediator.Send(new SetDeliveryFee.Request(DeliveryFee));
		return RedirectToPage("/Admin/Index");
	}
}
