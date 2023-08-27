using CampusEats.Core.Common;
using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages.Products;

public sealed class DeleteModel : PageModel
{
	private readonly IMediator _mediator;

	public DeleteModel(IMediator mediator)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
	}

	public ProductDto Product { get; private set; } = default!;

	public async Task<IActionResult> OnGetAsync(Guid id)
	{
		try
		{
			Product = await _mediator.Send(new GetProductById.Request(id));
			return Page();
		}
		catch(EntityNotFoundException)
		{
			return NotFound();
		}
	}

	public async Task<IActionResult> OnPostAsync(Guid id)
	{
		await _mediator.Send(new DeleteProduct.Request(id));
		return RedirectToPage("Index");
	}
}
