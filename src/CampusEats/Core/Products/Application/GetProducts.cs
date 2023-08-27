using CampusEats.Core.Common;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Infrastructure;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace CampusEats.Core.Products.Application;

public sealed class GetProducts
{
    public sealed record Request : IRequest<GenericResponse<List<ProductDto>>>;

    public sealed class Handler : IRequestHandler<Request, GenericResponse<List<ProductDto>>>
    {
        private readonly CampusEatsContext _db;

        public Handler(CampusEatsContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<GenericResponse<List<ProductDto>>> Handle(Request request, CancellationToken cancellationToken)
            => await _db.Products
            .OrderBy(p => p.Name)
            .Select(p => p.Adapt<ProductDto>())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
