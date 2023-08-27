
using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Ordering.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Admin
{
    public sealed class EarningsModel : PageModel
    {
        private readonly IMediator _mediator;

        public EarningsModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public OrderExpenseDto[] Deliveries { get; private set; } = Array.Empty<OrderExpenseDto>();
        public int TotalDeliveries { get; private set; }

        public decimal MoneyForCourier { get; private set; }

        public decimal TotalMoneyEarned { get; private set; }
        public async Task OnGetAsync()
        {
            var response = await _mediator.Send(new GetPaidDeliveries.Request());
            if(!response.Success)
                return;

            Deliveries = response.Data;
            TotalDeliveries = Deliveries.Length;
            foreach(var delivery in Deliveries)
            {
                TotalMoneyEarned += delivery.TotalCost;
                TotalMoneyEarned += delivery.DeliveryFee * 0.2m;
                MoneyForCourier += delivery.DeliveryFee * 0.8m;
            }
            RedirectToPage();
        }
    }
}