using CampusEats.Core.Ordering.Application.Services;
using MediatR;

namespace CampusEats.Core.Admin.Application.Pipelines;

public sealed class SetDeliveryFee
{
    public sealed record Request(decimal NewFee) : IRequest<Unit>;

    public sealed class Handler : IRequestHandler<Request, Unit>
    {
        private readonly IOrderingService _orderingService;
        public Handler(IOrderingService orderingService)
        {
            _orderingService = orderingService;
        }

        public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            _orderingService.UpdateDeliveryFee(request.NewFee);
            return Task.FromResult(Unit.Value);
        }
    }
}

