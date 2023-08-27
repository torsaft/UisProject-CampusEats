using CampusEats.Core.Ordering.Application.Pipelines;
using CampusEats.Core.Ordering.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Orders;

public sealed class AllOrdersModel : PageModel
{
    private readonly IMediator mediator;
    public AllOrdersModel(IMediator mediator)
    {
        this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
    }
    public List<Order> Orders { get; set; } = new();

    public async Task OnGetAsync()
    {
        Orders = await mediator.Send(new GetAllOrders.Request());
    }
}
