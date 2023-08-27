using CampusEats.Core.Cart.Pipelines;
using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;
    public List<ProductDto> Products { get; private set; } = new();
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync()
    {
        var result = await _mediator.Send(new GetProducts.Request());
        if(!result.Success)
        {
            Errors = result.Errors;
            return;
        }
        Products = result.Data;
    }

    public async Task<IActionResult> OnPostAddToCartAsync(Guid productId)
    {
        var cartId = HttpContext.Session.GetGuid("CartId");
        if(cartId == null)
        {
            cartId = Guid.NewGuid();
            HttpContext.Session.SetString("CartId", cartId.ToString()!);
        }

        var res = await _mediator.Send(new AddItem.Request(cartId.Value, productId));
        if(res.Success)
            return RedirectToPage();

        Errors = res.Errors;
        return Page();
    }
}
