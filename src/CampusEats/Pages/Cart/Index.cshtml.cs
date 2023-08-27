using CampusEats.Core.Cart.Domain.Dto;
using CampusEats.Core.Cart.Pipelines;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Cart;

public sealed class CartModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly string _cartKey = "CartId";
    public CartModel(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    [BindProperty]
    public LocationDto Location { get; set; } = new();
    public ShoppingCartDto Cart { get; private set; } = default!;
    public string[] Errors { get; private set; } = Array.Empty<string>();
    public bool IsAuthenticated => _currentUser.IsAuthenticated;

    public async Task OnGetAsync()
    {
        var cartId = HttpContext.Session.GetGuid(_cartKey);
        if(cartId is null) return;

        var res = await _mediator.Send(new GetCart.Request(cartId.Value));
        if(res.Success)
        {
            Cart = res.Data;
            return;
        }
        Errors = res.Errors;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cartId = HttpContext.Session.GetGuid(_cartKey);
        if(cartId is null) return Page();

        var res = await _mediator.Send(new CartCheckoutStripe.Request((Guid)cartId, Location));
        if(res.Success)
        {
            string url = res.Data;
            return Redirect(url);
        }
        Errors = res.Errors;
        return Page();
    }
}
