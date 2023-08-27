using CampusEats.Core.Cart.Domain.Events;
using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Application.Services;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Ordering.Application.Handlers;

public sealed class CheckoutSucceededHandler : INotificationHandler<CheckoutSucceeded>
{
    private readonly CampusEatsContext _db;
    private readonly IOrderingService _orderingService;
    private readonly ICurrentUser _currentUser;

    public CheckoutSucceededHandler(CampusEatsContext db, IOrderingService orderingService, ICurrentUser currentUser)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _orderingService = orderingService ?? throw new ArgumentNullException(nameof(orderingService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task Handle(CheckoutSucceeded notification, CancellationToken cancellationToken)
    {
        var session = notification.OrderDto.Session ?? throw new BaseException("Cannot have an empty session");
        var cart = await _db.ShoppingCarts
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x => x.Id.ToString() == session.Metadata["cartId"], cancellationToken);

        if(cart == null)
            throw new EntityNotFoundException("Cart not found");

        var ordeLineDtos = cart.CartItems
            .Select(x => new OrderLineDto(x.ProductId, x.ProductName, x.Price, x.Count))
            .ToArray();

        Location location = new
        (
            session.Metadata["locationBuilding"],
            session.Metadata["locationRoomNumber"],
            session.Metadata["locationNotes"]
        );

        string paymentId = session.PaymentIntentId;
        var deliveryFee = Decimal.TryParse(session.Metadata["deliveryFee"] ?? "0", out var fee) ? fee : 0;

        await _orderingService.PlaceOrder(location, _currentUser.Adapt<CustomerDto>(), ordeLineDtos, paymentId, deliveryFee);
    }
}
