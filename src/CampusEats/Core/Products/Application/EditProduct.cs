using CampusEats.Core.Common;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Products.Application;

public sealed class EditProduct
{
    public sealed record Request(ProductDto Product) : IRequest<Response>;

    public sealed record Response(bool Success, string[] Errors);

    public sealed class Handler : IRequestHandler<Request, Response>
    {
        private readonly CampusEatsContext _db;
        private readonly IEnumerable<IValidator<ProductDto>> _validators;

        public Handler(CampusEatsContext db, IEnumerable<IValidator<ProductDto>> validators)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var existingProduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == request.Product.Id, cancellationToken);
            if(existingProduct is null) throw new EntityNotFoundException($"Product with ProductId {request.Product.Id} was not found in the database.");

            var errors = _validators.Select(v => v.IsValid(request.Product))
                        .Where(result => !result.IsValid)
                        .Select(result => result.Error)
                        .ToArray();
            if(errors.Length > 0)
            {
                return new Response(false, errors);
            }

            existingProduct.Name = request.Product.Name;
            existingProduct.Description = request.Product.Description;
            existingProduct.Price = request.Product.Price;

            await _db.SaveChangesAsync(cancellationToken);
            return new Response(true, Array.Empty<string>());
        }
    }
}
