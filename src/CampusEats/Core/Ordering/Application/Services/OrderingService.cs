using CampusEats.Core.Common;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Core.Ordering.Application.Services;

public sealed class OrderingService : IOrderingService
{
    private readonly CampusEatsContext _db;

    public OrderingService(CampusEatsContext db)
    {
        _db = db;
    }

    public async Task<Guid> PlaceOrder(Location location, CustomerDto customer, OrderLineDto[] orderLinesDto, string stripePaymentId, decimal deliveryFee)
    {
        var newCustomer = new Customer(customer.UserId, customer.Email);
        Order newOrder = new(location, newCustomer, stripePaymentId);
        foreach(var ol in orderLinesDto)
        {
            newOrder.AddOrderLine(ol.ProductId, ol.Price, ol.Amount, ol.ProductName);
        }

        _db.Orders.Add(newOrder);
        await _db.SaveChangesAsync();

        // UPDATE THE STATUS OF THE ORDER TO PLACED AND MAKE SURE THAT THE ORDERPLACED EVENT IS ADDED TO THE ORDER.EVENT LIST
        newOrder.Place();
        await _db.SaveChangesAsync();
        return newOrder.Id;
    }

    public async Task UpdateOrderDeliveryStatus(Guid orderId)
    {
        var order = await _db.Orders.Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == orderId);
        if(order is null)
        {
            throw new EntityNotFoundException($"Order with id {orderId} not found");
        }

        order.MoveToNextPhase();
        await _db.SaveChangesAsync();
    }

    public void UpdateDeliveryFee(decimal newDeliveryFee)
    {
        if(newDeliveryFee <= 0) return;
        Order.CurrentDeliveryFee = newDeliveryFee;
        // Publish DeliveryFeeChanged event (?)
    }
}

