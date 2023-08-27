using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;


namespace CampusEats.Core.Ordering.Application.Services;

public interface IOrderingService
{
    void UpdateDeliveryFee(decimal newFee);
    Task UpdateOrderDeliveryStatus(Guid orderId);
    Task<Guid> PlaceOrder(Location location, CustomerDto customer, OrderLineDto[] orderLines, string StripePaymentId, decimal deliveryFee);
}