using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain.Dto;

namespace CampusEats.Core.Cart.Domain.Events;

public sealed record CheckoutSucceeded : IDomainEvent
{
    public CheckoutSucceeded(CreateOrderDto orderDto)
    {
        OrderDto = orderDto;
    }

    public CreateOrderDto OrderDto { get; set; }
}
