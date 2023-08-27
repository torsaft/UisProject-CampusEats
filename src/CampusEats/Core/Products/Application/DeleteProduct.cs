using CampusEats.Core.Common;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace CampusEats.Core.Products.Application;

public sealed class DeleteProduct
{
    public sealed record Request(Guid Id) : IRequest;

    public sealed class Handler : IRequestHandler<Request>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var product = await _db.Products.SingleOrDefaultAsync(product => product.Id == request.Id, cancellationToken);
            if(product is null) throw new EntityNotFoundException($"Product with ProductId {request.Id} was not found in the database");
            _db.Remove(product);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
