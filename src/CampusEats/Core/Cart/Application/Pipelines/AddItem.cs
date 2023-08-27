using CampusEats.Core.Cart.Domain;
using CampusEats.Core.Common;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Cart.Pipelines;

public sealed class AddItem
{
    public sealed record Request(Guid CartId, Guid ProductId) : IRequest<GenericResponse<Unit>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<Unit>>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<GenericResponse<Unit>> Handle(Request request, CancellationToken cancellationToken)
        {
            var cart = await _db.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if(cart == null)
            {
                cart = new ShoppingCart(request.CartId);
                await _db.ShoppingCarts.AddAsync(cart, cancellationToken);
            }

            var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if(product == null)
                return new[] { "Product not found" };

            cart.AddItem(product.Id, product.Name, product.Price);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
