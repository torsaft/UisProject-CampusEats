using CampusEats.Core.Delivering.Application.Pipelines;
using CampusEats.Core.Delivering.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Deliveries
{
    public sealed class CourierIndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public CourierIndexModel(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public int TotalDeliveries { get; set; }
        public decimal TotalMoneyEarned { get; set; }
        public decimal TotalTipEarned { get; set; }
        public DeliveryDto[] DeliveriesAssignedToMe { get; private set; } = Array.Empty<DeliveryDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            (TotalDeliveries, TotalMoneyEarned, TotalTipEarned) = await _mediator.Send(new GetCourierDoneDeliveries.Request());

            DeliveriesAssignedToMe = await _mediator.Send(new GetMyAssignedDeliveries.Request());
            return Page();

        }
    }
}