using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;


namespace CampusEats.Core.Cart.Pipelines;

public sealed class CartCheckoutStripe
{
    public sealed record Request(
        Guid CartId,
        LocationDto LocationDto) : IRequest<GenericResponse<string>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<string>>
    {
        private readonly CampusEatsContext _db;
        private readonly IEnumerable<IValidator<Location>> _validators;
        private readonly ICurrentUser _currentUser;

        public Handler(CampusEatsContext db, IEnumerable<IValidator<Location>> validators, ICurrentUser currentUser)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }
        public async Task<GenericResponse<string>> Handle(Request request, CancellationToken cancellationToken)
        {
            var location = new Location(request.LocationDto.Building, request.LocationDto.RoomNumber, request.LocationDto.Notes);
            var errors = _validators.Select(v => v.IsValid(location))
                        .Where(result => !result.IsValid)
                        .Select(result => result.Error)
                        .ToArray();

            if(errors.Length > 0)
            {
                return errors;
            }

            var customerDto = new CustomerDto(_currentUser.UserId, _currentUser.Email);

            var cart = (await _db.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken)) ?? throw new EntityNotFoundException();

            var orderLineDtos = cart.CartItems
                .Select(ol => new OrderLineDto(ol.ProductId, ol.ProductName, ol.Price, ol.Count))
                .ToArray();

            var session = await CheckOut(orderLineDtos, request.CartId, customerDto, request.LocationDto, Order.CurrentDeliveryFee);
            return session.Url;
        }

        public async Task<Session> CheckOut(OrderLineDto[] orderLineDtos, Guid cartId, CustomerDto customer, LocationDto location, decimal deliveryFee)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = "https://localhost:5001/Cart/CheckOut/" + "{CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:5001/Cart",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                Metadata = new Dictionary<string, string>()
                {
                    {"cartId", $"{cartId}"},
                    {"locationBuilding", $"{location.Building}"},
                    {"locationRoomNumber", $"{location.RoomNumber}"},
                    {"locationNotes", $"{location.Notes}"},
                    {"deliveryFee", $"{deliveryFee}"},
                },
                CustomerEmail = customer.Email,

            };
            for(int i = 0; i < orderLineDtos.Length; i++)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = Math.Floor(orderLineDtos[i].Price * 100),
                        Currency = "NOK",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = orderLineDtos[i].ProductName,
                        },
                    },
                    Quantity = orderLineDtos[i].Amount,
                });
            }
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = deliveryFee * 100,
                    Currency = "NOK",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Delivery Fee",
                    },
                },
                Quantity = 1,
            });
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session;
        }
    }
}

