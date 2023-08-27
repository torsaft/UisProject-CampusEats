using CampusEats.Core.Products.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace CampusEats.Core.Cart.Handlers;

public sealed class ProductPriceChangedHandler : INotificationHandler<ProductPriceChanged>
{
    private readonly CampusEatsContext _db;

    public ProductPriceChangedHandler(CampusEatsContext db)
        => _db = db ?? throw new ArgumentNullException(nameof(db));

    public async Task Handle(ProductPriceChanged notification, CancellationToken cancellationToken)
    {
        var carts = await _db.ShoppingCarts.Include(c => c.CartItems)
                        .Where(c => c.CartItems.Any())
                        .ToListAsync(cancellationToken);

        foreach(var cart in carts)
        {
            foreach(var item in cart.CartItems.Where(i => i.ProductId == notification.ProductId))
            {
                item.Price = notification.NewPrice;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
