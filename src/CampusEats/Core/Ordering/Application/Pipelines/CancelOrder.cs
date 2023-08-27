using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace CampusEats.Core.Ordering.Application.Pipelines;

public sealed class CancelOrder
{
	public sealed record Request(Guid OrderId) : IRequest<GenericResponse<OrderDto>>;

	public sealed class Handler : IRequestHandler<Request, GenericResponse<OrderDto>>
	{
		private readonly CampusEatsContext _db;
		private readonly ICurrentUser _currentUser;

		public Handler(CampusEatsContext db, ICurrentUser currentUser)
		{
			_db = db;
			_currentUser = currentUser;
		}

		public async Task<GenericResponse<OrderDto>> Handle(Request request, CancellationToken cancellationToken)
		{
			var order = await _db.Orders
				.Include(x => x.OrderLines)
				.Include(x => x.Customer)
				.FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);

			if(order is null)
				return new[] { "Order not found" };

			if(order.Customer.UserId != _currentUser.UserId)
				return new[] { "You are not authorized to cancel this order" };

			var refundAmount = order.Cancel();
			if(refundAmount <= 0)
				return new[] { "This order cannot be cancelled" };

			var options = new RefundCreateOptions
			{
				PaymentIntent = $"{order.StripePaymentId}",
				Amount = (long)refundAmount * 100
			};
			var service = new RefundService();
			await service.CreateAsync(options);

			await _db.SaveChangesAsync(cancellationToken);
			return new OrderDto(order);
		}
	}
}
