using CampusEats.Core.Ordering.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe.Checkout;

namespace CampusEats.Pages;

public sealed class TipSuccessModel : PageModel
{
    private readonly IMediator _mediator;
    public TipSuccessModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
    public async Task<IActionResult> OnGetAsync(string sessionId)
    {
        var sessionService = new SessionService();
        var session = sessionService.Get(sessionId);
        await _mediator.Publish(new TipSucceeded(session));
        return Page();

    }
}