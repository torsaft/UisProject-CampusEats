using Stripe.Checkout;

namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record CreateOrderDto(Session? Session);
