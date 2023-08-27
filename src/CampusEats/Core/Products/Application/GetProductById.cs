using CampusEats.Core.Common;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Products.Application;

public sealed class GetProductById
{
    public sealed record Request(Guid Id) : IRequest<ProductDto>;

    public sealed class Handler : IRequestHandler<Request, ProductDto>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<ProductDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if(product is null) throw new EntityNotFoundException($"Product with ProductId {request.Id} was not found in the database");
            return product.Adapt<ProductDto>();
        }
    }
}
