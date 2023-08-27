using CampusEats.Core.Cart.Domain.Dto;
using CampusEats.Core.Common;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Cart.Pipelines;

public sealed class GetCart
{
    public sealed record Request(Guid CartId) : IRequest<GenericResponse<ShoppingCartDto>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<ShoppingCartDto>>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<GenericResponse<ShoppingCartDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var cart = await _db.ShoppingCarts.Include(c => c.CartItems)
                                                .Where(c => c.Id == request.CartId)
                                                .Select(c => c.Adapt<ShoppingCartDto>())
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(cancellationToken);

            return cart != null ? cart : new string[] { "Cart not found" };
        }
    }
}
