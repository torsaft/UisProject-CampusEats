using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Cart.Pipelines;

public sealed class GetItemCount
{
	public sealed record Request(Guid CartId) : IRequest<int>;

	public sealed class Handler : IRequestHandler<Request, int>
	{
		private readonly CampusEatsContext _db;

		public Handler(CampusEatsContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

		public async Task<int> Handle(Request request, CancellationToken cancellationToken)
			=> await _db.ShoppingCarts.Include(c => c.CartItems)
			.Where(c => c.Id == request.CartId)
			.SelectMany(c => c.CartItems)
			.SumAsync(i => i.Count, cancellationToken);

	}
}
