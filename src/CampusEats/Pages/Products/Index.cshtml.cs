using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages.Products;

public class IndexModel : PageModel
{
	private readonly IMediator _mediator;

	public IndexModel(IMediator mediator) => _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));

	public List<ProductDto> Products { get; private set; } = new();

	public async Task OnGetAsync()
	{
		var res = await _mediator.Send(new GetProducts.Request());
		if(res.Success)
		{
			Products = res.Data;
		}
	}

}
