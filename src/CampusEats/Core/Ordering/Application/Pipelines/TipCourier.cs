using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace CampusEats.Core.Ordering.Application.Pipelines;

public sealed class TipCourier
{
    public sealed record Response(Session Session);
    public sealed record Request(Guid OrderId, decimal TipAmount) : IRequest<Response>;

    public sealed class Handler : IRequestHandler<Request, Response>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db)
        {
            _db = db;
        }
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var order = await _db.Orders.Include(o => o.Customer).FirstOrDefaultAsync(c => c.Id == request.OrderId, cancellationToken);
            if(order == null)
                throw new EntityNotFoundException("Order not found");

            var response = await CheckOut(order, request.TipAmount);
            return response;
        }

        public async Task<Response> CheckOut(Order order, decimal tipAmount)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = order.Customer.Email,
                SuccessUrl = $"https://localhost:5001/Orders/TipSuccess/" + "{CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:5001/Order/TipCancel",
                Metadata = new Dictionary<string, string>
                {
                    {"orderId", order.Id.ToString()},
                    {"tipAmount", tipAmount.ToString()}
                }
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = tipAmount * 100,
                    Currency = "NOK",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tip",
                    },
                },
                Quantity = 1,
            });
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new Response(Session: session);
        }
    }
}