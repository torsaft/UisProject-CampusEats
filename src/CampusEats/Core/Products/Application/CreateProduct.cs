using CampusEats.Core.Common;
using CampusEats.Core.Products.Domain;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;

namespace CampusEats.Core.Products.Application;

public sealed class CreateProduct
{
    public sealed record Request(ProductDto Product) : IRequest<GenericResponse<Guid>>;
    public sealed class Handler : IRequestHandler<Request, GenericResponse<Guid>>
    {
        private readonly CampusEatsContext _db;
        private readonly IEnumerable<IValidator<ProductDto>> _validators;

        public Handler(CampusEatsContext db, IEnumerable<IValidator<ProductDto>> validators)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<GenericResponse<Guid>> Handle(Request request, CancellationToken cancellationToken)
        {
            var errors = _validators.Select(v => v.IsValid(request.Product))
                        .Where(result => !result.IsValid)
                        .Select(result => result.Error)
                        .ToArray();

            if(errors.Length > 0)
            {
                return new(errors);
            }
            var product = new Product(request.Product.Name, request.Product.Price, request.Product.Description);
            _db.Products.Add(product);
            await _db.SaveChangesAsync(cancellationToken);
            return new(product.Id);
        }
    }
}
