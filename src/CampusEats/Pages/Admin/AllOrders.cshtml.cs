using CampusEats.Core.Ordering.Application.Pipelines;
using CampusEats.Core.Ordering.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin;

public sealed class AllOrdersModel : PageModel
{
    private readonly IMediator mediator;
    public AllOrdersModel(IMediator mediator)
    {
        this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
    }
    public List<Order> Orders { get; set; } = new();

    [BindProperty]
    public bool FilterOrdersBool { get; set; }

    public async Task<IActionResult> OnGetAsync(bool? filtered)
    {

        Orders = await mediator.Send(new GetAllOrders.Request());
        if(filtered ?? false)
        {
            Orders = Orders.Where(x => (DateTime.UtcNow - x.OrderDate).TotalDays <= 30).ToList();
        }

        FilterOrdersBool = !(filtered ?? false);
        return Page();
    }

    public IActionResult OnPost(bool filterOrdersBool)
    {
        FilterOrdersBool = !filterOrdersBool;
        return RedirectToPage("/Admin/AllOrders");
    }
}
