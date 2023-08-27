using CampusEats.Core.Cart.Domain.Events;
using CampusEats.Core.Ordering.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe.Checkout;

namespace CampusEats.Pages.Cart;

public sealed class CheckOutModel : PageModel
{
    private readonly IMediator _mediator;

    private readonly string _cartKey = "CartId";
    public CheckOutModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<IActionResult> OnGetAsync(string sessionId)
    {
        var sessionService = new SessionService();
        var session = sessionService.Get(sessionId);

        var createOrderDto = new CreateOrderDto(session);

        await _mediator.Publish(new CheckoutSucceeded(createOrderDto));

        HttpContext.Session.Remove(_cartKey);

        return Page();
    }
}
